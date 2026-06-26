using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Salar.Bois.Generator;

[Generator]
public sealed class BoisSourceGenerator : ISourceGenerator
{
    private const string ReaderAttributeName = "Salar.Bois.BoisReaderAttribute";
    private const string WriterAttributeName = "Salar.Bois.BoisWriterAttribute";

    private static readonly DiagnosticDescriptor InvalidMethodSignature = new(
        "BOISGEN001",
        "Invalid BOIS generator method signature",
        "{0}",
        "Salar.Bois.Generator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsupportedType = new(
        "BOISGEN002",
        "Unsupported BOIS generator type",
        "{0}",
        "Salar.Bois.Generator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(static () => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver || receiver.Candidates.Count == 0)
            return;

        var readerAttribute = context.Compilation.GetTypeByMetadataName(ReaderAttributeName);
        var writerAttribute = context.Compilation.GetTypeByMetadataName(WriterAttributeName);
        if (readerAttribute is null || writerAttribute is null)
            return;

        var groups = new Dictionary<(INamedTypeSymbol ContainingType, string FileName), List<GenerationMethod>>(new GroupComparer());

        foreach (var candidate in receiver.Candidates)
        {
            var semanticModel = context.Compilation.GetSemanticModel(candidate.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(candidate, context.CancellationToken) is not IMethodSymbol method)
                continue;

            var operation = GetOperation(method, readerAttribute, writerAttribute);
            if (operation is null)
                continue;

            if (!TryCreateGenerationMethod(method, operation.Value, context, out var generationMethod))
                continue;

            var key = (generationMethod.ContainingType, GetFileName(generationMethod.RootType));
            if (!groups.TryGetValue(key, out var methods))
            {
                methods = [];
                groups.Add(key, methods);
            }
            methods.Add(generationMethod);
        }

        foreach (var group in groups)
        {
            var source = EmitContainingType(context.Compilation, group.Key.ContainingType, group.Value.ToImmutableArray(), context.ReportDiagnostic);
            if (!string.IsNullOrWhiteSpace(source))
                context.AddSource(group.Key.FileName + "-generated.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static OperationKind? GetOperation(IMethodSymbol method, INamedTypeSymbol readerAttribute, INamedTypeSymbol writerAttribute)
    {
        foreach (var attribute in method.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, readerAttribute))
                return OperationKind.Reader;
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, writerAttribute))
                return OperationKind.Writer;
        }
        return null;
    }

    private static bool TryCreateGenerationMethod(IMethodSymbol method, OperationKind operation, GeneratorExecutionContext context, out GenerationMethod generationMethod)
    {
        generationMethod = default!;

        if (method.ContainingType is not INamedTypeSymbol containingType || !containingType.IsStatic || !IsPartial(containingType))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidMethodSignature, method.Locations.FirstOrDefault(), $"Method '{method.Name}' must be declared inside a static partial class."));
            return false;
        }

        if (!method.IsStatic || !method.DeclaringSyntaxReferences.Any(static x => x.GetSyntax() is MethodDeclarationSyntax m && m.Modifiers.Any(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidMethodSignature, method.Locations.FirstOrDefault(), $"Method '{method.Name}' must be declared as a static partial method."));
            return false;
        }

        if (operation == OperationKind.Reader)
        {
            if (!TryCreateReaderGenerationMethod(method, containingType, out generationMethod))
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidMethodSignature, method.Locations.FirstOrDefault(), $"Reader method '{method.Name}' must have one of the supported signatures: {GetSupportedReaderSignatures()}."));
                return false;
            }

            return true;
        }

        if (!TryCreateWriterGenerationMethod(method, containingType, out generationMethod))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidMethodSignature, method.Locations.FirstOrDefault(), $"Writer method '{method.Name}' must have one of the supported signatures: {GetSupportedWriterSignatures()}."));
            return false;
        }

        return true;
    }

    private static string EmitContainingType(Compilation compilation, INamedTypeSymbol containingType, ImmutableArray<GenerationMethod> methods, Action<Diagnostic> report)
    {
        var emitter = new ContainingTypeEmitter(compilation, containingType, report);
        return emitter.Emit(methods);
    }

    private static bool IsPartial(INamedTypeSymbol type)
        => type.DeclaringSyntaxReferences.Any(static x => x.GetSyntax() is TypeDeclarationSyntax t && t.Modifiers.Any(SyntaxKind.PartialKeyword));

    private static bool IsStream(ITypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.IO.Stream";

    private static bool IsBufferReader(ITypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Salar.BinaryBuffers.BufferReaderBase";

    private static bool IsBufferWriter(ITypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Salar.BinaryBuffers.BufferWriterBase";

    private static bool IsEncoding(ITypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Text.Encoding";

    private static bool IsByteArray(ITypeSymbol type)
        => type is IArrayTypeSymbol arrayType && arrayType.ElementType.SpecialType == SpecialType.System_Byte;

    private static bool IsInt32(ITypeSymbol type)
        => type.SpecialType == SpecialType.System_Int32;

    private static bool TryCreateReaderGenerationMethod(IMethodSymbol method, INamedTypeSymbol containingType, out GenerationMethod generationMethod)
    {
        generationMethod = default!;

        if (method.ReturnsVoid)
            return false;

        var parameters = method.Parameters;
        var encodingParameterIndex = TryGetTrailingEncodingParameterIndex(parameters);
        var parameterCount = encodingParameterIndex is null ? parameters.Length : parameters.Length - 1;

        ReaderSignature? signature = null;

        if (parameterCount == 1)
        {
            if (IsStream(parameters[0].Type))
            {
                signature = new ReaderSignature(ReaderInputKind.Stream, 0, -1, -1, encodingParameterIndex);
            }
            else if (IsBufferReader(parameters[0].Type))
            {
                signature = new ReaderSignature(ReaderInputKind.BufferReader, 0, -1, -1, encodingParameterIndex);
            }
        }
        else if (parameterCount == 3 &&
                 IsByteArray(parameters[0].Type) &&
                 IsInt32(parameters[1].Type) &&
                 IsInt32(parameters[2].Type))
        {
            signature = new ReaderSignature(ReaderInputKind.ByteArray, 0, 1, 2, encodingParameterIndex);
        }

        if (signature is null)
            return false;

        generationMethod = new GenerationMethod(method, containingType, OperationKind.Reader, method.ReturnType, signature);
        return true;
    }

    private static bool TryCreateWriterGenerationMethod(IMethodSymbol method, INamedTypeSymbol containingType, out GenerationMethod generationMethod)
    {
        generationMethod = default!;

        if (!method.ReturnsVoid)
            return false;

        var parameters = method.Parameters;
        var encodingParameterIndex = TryGetTrailingEncodingParameterIndex(parameters);
        var parameterCount = encodingParameterIndex is null ? parameters.Length : parameters.Length - 1;

        WriterSignature? signature = null;

        if (parameterCount == 2)
        {
            if (IsStream(parameters[0].Type) || IsBufferWriter(parameters[0].Type))
            {
                signature = new WriterSignature(
                    IsStream(parameters[0].Type) ? WriterOutputKind.Stream : WriterOutputKind.BufferWriter,
                    1,
                    0,
                    -1,
                    -1,
                    encodingParameterIndex);
            }
            else if (IsStream(parameters[1].Type) || IsBufferWriter(parameters[1].Type))
            {
                signature = new WriterSignature(
                    IsStream(parameters[1].Type) ? WriterOutputKind.Stream : WriterOutputKind.BufferWriter,
                    0,
                    1,
                    -1,
                    -1,
                    encodingParameterIndex);
            }
        }
        else if (parameterCount == 4)
        {
            if (IsByteArray(parameters[0].Type) &&
                IsInt32(parameters[1].Type) &&
                IsInt32(parameters[2].Type))
            {
                signature = new WriterSignature(WriterOutputKind.ByteArray, 3, 0, 1, 2, encodingParameterIndex);
            }
            else if (IsByteArray(parameters[1].Type) &&
                     IsInt32(parameters[2].Type) &&
                     IsInt32(parameters[3].Type))
            {
                signature = new WriterSignature(WriterOutputKind.ByteArray, 0, 1, 2, 3, encodingParameterIndex);
            }
        }

        if (signature is null)
            return false;

        generationMethod = new GenerationMethod(method, containingType, OperationKind.Writer, parameters[signature.ModelParameterIndex].Type, signature);
        return true;
    }

    private static int? TryGetTrailingEncodingParameterIndex(ImmutableArray<IParameterSymbol> parameters)
    {
        if (parameters.Length == 0)
            return null;

        var lastIndex = parameters.Length - 1;
        return IsEncoding(parameters[lastIndex].Type) ? lastIndex : null;
    }

    private static string GetSupportedReaderSignatures()
        => string.Join(
            ", ",
            [
                "'static partial T Method(System.IO.Stream source)'",
                "'static partial T Method(System.IO.Stream source, System.Text.Encoding encoding)'",
                "'static partial T Method(Salar.BinaryBuffers.BufferReaderBase reader)'",
                "'static partial T Method(Salar.BinaryBuffers.BufferReaderBase reader, System.Text.Encoding encoding)'",
                "'static partial T Method(byte[] buffer, int position, int length)'",
                "or 'static partial T Method(byte[] buffer, int position, int length, System.Text.Encoding encoding)'"
            ]);

    private static string GetSupportedWriterSignatures()
        => string.Join(
            ", ",
            [
                "'static partial void Method(T model, System.IO.Stream output)'",
                "'static partial void Method(System.IO.Stream output, T model)'",
                "'static partial void Method(T model, Salar.BinaryBuffers.BufferWriterBase writer)'",
                "'static partial void Method(Salar.BinaryBuffers.BufferWriterBase writer, T model)'",
                "'static partial void Method(T model, byte[] output, int position, int length)'",
                "'static partial void Method(byte[] output, int position, int length, T model)'",
                "and those same signatures with an optional trailing System.Text.Encoding parameter"
            ]);

    private static string GetFileName(ITypeSymbol type)
    {
        return type switch
        {
            IArrayTypeSymbol arrayType => arrayType.ElementType.Name + "Array",
            INamedTypeSymbol namedType => namedType.Name,
            _ => "BoisGenerated"
        };
    }

    private sealed class SyntaxReceiver : ISyntaxReceiver
    {
        public List<MethodDeclarationSyntax> Candidates { get; } = [];

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax method &&
                method.Modifiers.Any(SyntaxKind.PartialKeyword) &&
                HasBoisAttribute(method))
            {
                Candidates.Add(method);
            }
        }

        private static bool HasBoisAttribute(MethodDeclarationSyntax method)
            => method.AttributeLists
                .SelectMany(static list => list.Attributes)
                .Any(static attribute => IsBoisAttributeName(GetAttributeName(attribute.Name)));

        private static bool IsBoisAttributeName(string attributeName)
            => attributeName is "BoisReader" or "BoisReaderAttribute" or "BoisWriter" or "BoisWriterAttribute";

        private static string GetAttributeName(NameSyntax name)
            => name switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                QualifiedNameSyntax qualified => GetAttributeName(qualified.Right),
                AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name.Identifier.ValueText,
                _ => name.ToString()
            };
    }

    private sealed class ContainingTypeEmitter
    {
        private static readonly SymbolDisplayFormat QualifiedTypeFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _containingType;
        private readonly Action<Diagnostic> _report;
        private readonly INamedTypeSymbol? _nullableType;
        private readonly INamedTypeSymbol? _collectionType;
        private readonly INamedTypeSymbol? _dictionaryType;
        private readonly INamedTypeSymbol? _nameValueCollectionType;
        private readonly ITypeSymbol? _guidType;
        private readonly ITypeSymbol? _colorType;
        private readonly ITypeSymbol? _uriType;
        private readonly ITypeSymbol? _versionType;
        private readonly ITypeSymbol? _dateTimeOffsetType;
        private readonly ITypeSymbol? _timeSpanType;
        private readonly ITypeSymbol? _dateOnlyType;
        private readonly ITypeSymbol? _timeOnlyType;
        private readonly ITypeSymbol? _dbNullType;
        private readonly ITypeSymbol? _dataTableType;
        private readonly ITypeSymbol? _dataSetType;

        public ContainingTypeEmitter(Compilation compilation, INamedTypeSymbol containingType, Action<Diagnostic> report)
        {
            _compilation = compilation;
            _containingType = containingType;
            _report = report;
            _nullableType = compilation.GetTypeByMetadataName("System.Nullable`1");
            _collectionType = compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1");
            _dictionaryType = compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2");
            _nameValueCollectionType = compilation.GetTypeByMetadataName("System.Collections.Specialized.NameValueCollection");
            _guidType = compilation.GetTypeByMetadataName("System.Guid");
            _colorType = compilation.GetTypeByMetadataName("System.Drawing.Color");
            _uriType = compilation.GetTypeByMetadataName("System.Uri");
            _versionType = compilation.GetTypeByMetadataName("System.Version");
            _dateTimeOffsetType = compilation.GetTypeByMetadataName("System.DateTimeOffset");
            _timeSpanType = compilation.GetTypeByMetadataName("System.TimeSpan");
            _dateOnlyType = compilation.GetTypeByMetadataName("System.DateOnly");
            _timeOnlyType = compilation.GetTypeByMetadataName("System.TimeOnly");
            _dbNullType = compilation.GetTypeByMetadataName("System.DBNull");
            _dataTableType = compilation.GetTypeByMetadataName("System.Data.DataTable");
            _dataSetType = compilation.GetTypeByMetadataName("System.Data.DataSet");
        }

        public string Emit(ImmutableArray<GenerationMethod> methods)
        {
            var builder = new CodeBuilder();
            builder.Line("// <auto-generated />");
            builder.Line("#nullable enable");
            builder.Line("using global::System;");
            builder.Line("using global::Salar.BinaryBuffers;");
            builder.Line("using global::Salar.Bois.CodeGen;");
            builder.Line("using global::Salar.BinaryBuffers.Compatibility;");
            builder.Line();

            if (!_containingType.ContainingNamespace.IsGlobalNamespace)
            {
                builder.Line($"namespace {_containingType.ContainingNamespace.ToDisplayString()};");
                builder.Line();
            }

            builder.Line($"{GetContainingTypeDeclaration()} ");
            builder.Line("{");
            builder.Indent();

            foreach (var method in methods.OrderBy(static x => x.Method.Name, StringComparer.Ordinal))
            {
                EmitMethod(builder, method);
                builder.Line();
            }

            builder.Unindent();
            builder.Line("}");
            return builder.ToString();
        }

        private void EmitMethod(CodeBuilder builder, GenerationMethod method)
        {
            var emitter = new MethodEmitter(this, method);
            emitter.Emit(builder);
        }

        private string GetContainingTypeDeclaration()
        {
            var access = _containingType.DeclaredAccessibility == Accessibility.Public ? "public " : "internal ";
            return $"{access}static partial class {_containingType.Name}";
        }

        private sealed class MethodEmitter
        {
            private readonly ContainingTypeEmitter _owner;
            private readonly GenerationMethod _method;
            private readonly Dictionary<string, LocalFunctionModel> _readFunctions = new(StringComparer.Ordinal);
            private readonly Dictionary<string, LocalFunctionModel> _writeFunctions = new(StringComparer.Ordinal);
            private readonly Dictionary<string, int> _nestedFunctionUseCounts = new(StringComparer.Ordinal);
            private readonly Queue<LocalFunctionModel> _pending = new();
            private int _id;

            public MethodEmitter(ContainingTypeEmitter owner, GenerationMethod method)
            {
                _owner = owner;
                _method = method;
                CountNestedFunctionUses(method.RootType);
            }

            public void Emit(CodeBuilder builder)
            {
                if (!_owner.ValidateRootType(_method.RootType, out var error))
                {
                    _owner.Report(_method.Method.Locations.FirstOrDefault(), error);
                    EmitStub(builder, error);
                    return;
                }

                var signature = BuildSignature(_method.Method);
                builder.Line(signature);
                builder.Line("{");
                builder.Indent();

                if (_method.Operation == OperationKind.Reader)
                {
                    EmitReaderSourceSetup(builder);
                    if (!TryEmitRead(_method.RootType, builder, out error, setupEncoding: true))
                    {
                        _owner.Report(_method.Method.Locations.FirstOrDefault(), error);
                        builder.Line($"throw new global::NotSupportedException({Literal(error)});");
                    }
                }
                else
                {
                    EmitWriterValueSetup(builder);
                    EmitRootWriterNullGuard(builder);
                    EmitWriterSetup(builder);
                    if (!TryEmitWrite(_method.RootType, builder, out error, suppressNullCheck: true))
                    {
                        _owner.Report(_method.Method.Locations.FirstOrDefault(), error);
                        builder.Line($"throw new global::NotSupportedException({Literal(error)});");
                    }
                }

                while (_pending.Count > 0)
                    EmitLocalFunction(builder, _pending.Dequeue());

                builder.Unindent();
                builder.Line("}");
            }

            private void EmitStub(CodeBuilder builder, string error)
            {
                builder.Line(BuildSignature(_method.Method));
                builder.Line("{");
                builder.Indent();
                builder.Line($"throw new global::NotSupportedException({Literal(error)});");
                builder.Unindent();
                builder.Line("}");
            }

            private string EnsureReadFunction(ITypeSymbol type)
            {
                type = _owner.LocalFunctionType(type);
                var key = _owner.TypeKey(type);
                if (_readFunctions.TryGetValue(key, out var existing))
                    return existing.Name;

                var function = new LocalFunctionModel(OperationKind.Reader, type, $"ReadType{_id++}");
                _readFunctions.Add(key, function);
                _pending.Enqueue(function);
                return function.Name;
            }

            private string EnsureWriteFunction(ITypeSymbol type)
            {
                type = _owner.LocalFunctionType(type);
                var key = _owner.TypeKey(type);
                if (_writeFunctions.TryGetValue(key, out var existing))
                    return existing.Name;

                var function = new LocalFunctionModel(OperationKind.Writer, type, $"WriteType{_id++}");
                _writeFunctions.Add(key, function);
                _pending.Enqueue(function);
                return function.Name;
            }

            private bool ShouldUseNestedFunction(ITypeSymbol type)
            {
                var key = _owner.TypeKey(_owner.LocalFunctionType(type));
                return _nestedFunctionUseCounts.TryGetValue(key, out var count) && count > 1;
            }

            private void CountNestedFunctionUses(ITypeSymbol type)
            {
                if (!_owner.TryGetMembers(type, out var members, out _))
                    return;

                foreach (var member in members)
                    CountNestedFunctionUse(member.Type);
            }

            private void CountNestedFunctionUse(ITypeSymbol type)
            {
                if (_owner.TryGetBasicType(type, out _) || _owner.IsEnum(type))
                    return;

                if (type is IArrayTypeSymbol arrayType)
                {
                    CountNestedElementFunctionUse(arrayType.ElementType);
                    return;
                }
                if (_owner.TryGetDictionaryInfo(type, out var dictionaryInfo))
                {
                    CountNestedElementFunctionUse(dictionaryInfo.KeyType);
                    CountNestedElementFunctionUse(dictionaryInfo.ValueType);
                    return;
                }
                if (_owner.TryGetCollectionInfo(type, out var collectionInfo))
                {
                    CountNestedElementFunctionUse(collectionInfo.ElementType);
                    return;
                }
                if (_owner.IsNameValueCollection(type))
                    return;

                AddNestedFunctionUse(type);
            }

            private void CountNestedElementFunctionUse(ITypeSymbol type)
            {
                if (_owner.TryGetBasicType(type, out _) || _owner.IsEnum(type))
                    return;

                AddNestedFunctionUse(type);
            }

            private void AddNestedFunctionUse(ITypeSymbol type)
            {
                var key = _owner.TypeKey(_owner.LocalFunctionType(type));
                _nestedFunctionUseCounts[key] = _nestedFunctionUseCounts.TryGetValue(key, out var count) ? count + 1 : 1;
            }

            private void EmitLocalFunction(CodeBuilder builder, LocalFunctionModel function)
            {
                if (function.Operation == OperationKind.Reader)
                    EmitReadLocalFunction(builder, function);
                else
                    EmitWriteLocalFunction(builder, function);
            }

            private void EmitReadLocalFunction(CodeBuilder builder, LocalFunctionModel function)
            {
                builder.Line();
                builder.Line($"static {TypeName(function.Type)} {function.Name}(global::Salar.BinaryBuffers.BufferReaderBase reader, global::System.Text.Encoding encoding)");
                builder.Line("{");
                builder.Indent();

                if (!TryEmitRead(function.Type, builder, out var error))
                {
                    _owner.Report(_method.Method.Locations.FirstOrDefault(), error);
                    builder.Line($"throw new global::NotSupportedException({Literal(error)});");
                }

                builder.Unindent();
                builder.Line("}");
            }

            private void EmitWriteLocalFunction(CodeBuilder builder, LocalFunctionModel function)
            {
                builder.Line();
                builder.Line($"static void {function.Name}(global::Salar.BinaryBuffers.BufferWriterBase writer, {TypeName(function.Type)} value, global::System.Text.Encoding encoding)");
                builder.Line("{");
                builder.Indent();

                if (!TryEmitWrite(function.Type, builder, out var error))
                {
                    _owner.Report(_method.Method.Locations.FirstOrDefault(), error);
                    builder.Line($"throw new global::NotSupportedException({Literal(error)});");
                }

                builder.Unindent();
                builder.Line("}");
            }

            private void EmitReaderSourceSetup(CodeBuilder builder)
            {
                var signature = (ReaderSignature)_method.Signature;
                var sourceName = Escape(_method.Method.Parameters[signature.SourceParameterIndex].Name);
                switch (signature.InputKind)
                {
                    case ReaderInputKind.Stream:
                        builder.Line($"var reader = new StreamBufferReader({sourceName});");
                        break;
                    case ReaderInputKind.BufferReader:
                        if (_method.Method.Parameters[signature.SourceParameterIndex].Name != "reader")
                            builder.Line($"var reader = {sourceName};");
                        break;
                    case ReaderInputKind.ByteArray:
                        var positionName = Escape(_method.Method.Parameters[signature.PositionParameterIndex].Name);
                        var lengthName = Escape(_method.Method.Parameters[signature.LengthParameterIndex].Name);
                        builder.Line($"var reader = new BinaryBufferReader({sourceName}, {positionName}, {lengthName});");
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            private void EmitWriterValueSetup(CodeBuilder builder)
            {
                var signature = (WriterSignature)_method.Signature;
                var valueName = Escape(_method.Method.Parameters[signature.ModelParameterIndex].Name);
                if (_method.Method.Parameters[signature.ModelParameterIndex].Name != "value")
                    builder.Line($"var value = {valueName};");
            }

            private void EmitRootWriterNullGuard(CodeBuilder builder)
            {
                if (!_owner.IsNullable(_method.RootType))
                    return;

                var signature = (WriterSignature)_method.Signature;
                var modelParameterName = Escape(_method.Method.Parameters[signature.ModelParameterIndex].Name);
                builder.Line("if (value is null)");
                builder.Line("{");
                builder.Indent();
                builder.Line($"throw new global::System.ArgumentNullException(nameof({modelParameterName}), \"Object cannot be null.\");");
                builder.Unindent();
                builder.Line("}");
            }

            private void EmitWriterSetup(CodeBuilder builder)
            {
                var signature = (WriterSignature)_method.Signature;
                EmitEncodingSetup(builder, signature.EncodingParameterIndex);

                var outputName = Escape(_method.Method.Parameters[signature.OutputParameterIndex].Name);
                switch (signature.OutputKind)
                {
                    case WriterOutputKind.Stream:
                        builder.Line($"var writer = new StreamBufferWriter({outputName});");
                        break;
                    case WriterOutputKind.BufferWriter:
                        if (_method.Method.Parameters[signature.OutputParameterIndex].Name != "writer")
                            builder.Line($"var writer = {outputName};");
                        break;
                    case WriterOutputKind.ByteArray:
                        var positionName = Escape(_method.Method.Parameters[signature.PositionParameterIndex].Name);
                        var lengthName = Escape(_method.Method.Parameters[signature.LengthParameterIndex].Name);
                        builder.Line($"var writer = new BinaryBufferWriter({outputName}, {positionName}, {lengthName});");
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            private void EmitEncodingSetup(CodeBuilder builder, int? encodingParameterIndex)
            {
                if (encodingParameterIndex is null)
                {
                    builder.Line("var encoding = global::Salar.Bois.BoisSerializer.DefaultEncoding;");
                    return;
                }

                if (_method.Method.Parameters[encodingParameterIndex.Value].Name == "encoding")
                {
                    builder.Line("encoding ??= global::Salar.Bois.BoisSerializer.DefaultEncoding;");
                    return;
                }

                var encodingName = Escape(_method.Method.Parameters[encodingParameterIndex.Value].Name);
                builder.Line($"var encoding = {encodingName} ?? global::Salar.Bois.BoisSerializer.DefaultEncoding;");
            }

            private bool TryEmitWrite(ITypeSymbol type, CodeBuilder builder, out string error, bool suppressNullCheck = false)
            {
                if (_owner.TryGetBasicType(type, out var basicType))
                {
                    builder.Line(_owner.GetWriteStatement(type, basicType, "value"));
                    error = string.Empty;
                    return true;
                }

                if (_owner.IsEnum(type))
                {
                    builder.Line(_owner.GetEnumWriteStatement(type, "value"));
                    error = string.Empty;
                    return true;
                }

                if (type is IArrayTypeSymbol arrayType)
                    return EmitWriteArray(arrayType, builder, out error, suppressNullCheck);

                if (_owner.TryGetDictionaryInfo(type, out var dict))
                    return EmitWriteDictionary(dict, builder, out error, suppressNullCheck);

                if (_owner.TryGetCollectionInfo(type, out var coll))
                    return EmitWriteCollection(coll, builder, out error, suppressNullCheck);

                if (_owner.IsNameValueCollection(type))
                    return EmitWriteNameValueCollection(builder, out error, suppressNullCheck);

                return EmitWriteObject(type, builder, out error, suppressNullCheck);
            }

            private bool TryEmitRead(ITypeSymbol type, CodeBuilder builder, out string error, bool setupEncoding = false)
            {
                if (_owner.TryGetBasicType(type, out var basicType))
                {
                    if (setupEncoding)
                        EmitReaderEncodingSetup(builder);
                    builder.Line($"return {_owner.GetReadExpression(type, basicType)};");
                    error = string.Empty;
                    return true;
                }

                if (_owner.IsEnum(type))
                {
                    builder.Line($"return BoisPrimitiveReaders.ReadEnumGeneric<{TypeName(type)}>(reader);");
                    error = string.Empty;
                    return true;
                }

                if (type is IArrayTypeSymbol arrayType)
                    return EmitReadArray(arrayType, builder, out error, setupEncoding);

                if (_owner.TryGetDictionaryInfo(type, out var dict))
                    return EmitReadDictionary(type, dict, builder, out error, setupEncoding);

                if (_owner.TryGetCollectionInfo(type, out var coll))
                    return EmitReadCollection(type, coll, builder, out error, setupEncoding);

                if (_owner.IsNameValueCollection(type))
                    return EmitReadNameValueCollection(type, builder, out error, setupEncoding);

                return EmitReadObject(type, builder, out error, setupEncoding);
            }

            private void EmitReaderEncodingSetup(CodeBuilder builder)
            {
                var signature = (ReaderSignature)_method.Signature;
                EmitEncodingSetup(builder, signature.EncodingParameterIndex);
            }

            private bool EmitWriteObject(ITypeSymbol type, CodeBuilder builder, out string error, bool suppressNullCheck = false, string expression = "value")
            {
                if (!_owner.TryGetMembers(type, out var members, out error))
                    return false;

                if (members.Length == 0)
                {
                    error = string.Empty;
                    return true;
                }

                if (!_owner.IsExplicitStruct(type))
                {
                    if (!suppressNullCheck)
                    {
                        builder.Line($"if ({expression} is null)");
                        builder.Line("{");
                        builder.Indent();
                        builder.Line("BoisPrimitiveWriters.WriteNullValue(writer);");
                        builder.Line("return;");
                        builder.Unindent();
                        builder.Line("}");
                    }
                    builder.Line($"BoisNumericSerializers.WriteUIntNullableMemberCount(writer, {members.Length}u);");
                }

                foreach (var member in members)
                {
                    if (!EmitWriteMember(member, builder, out error, expression))
                        return false;
                }

                error = string.Empty;
                return true;
            }

            private bool EmitReadObject(ITypeSymbol type, CodeBuilder builder, out string error, bool setupEncoding = false)
            {
                if (!_owner.TryGetMembers(type, out var members, out error))
                    return false;

                if (members.Length == 0)
                {
                    if (!_owner.TryGetCreationExpression(type, out var emptyCreationExpression, out error))
                        return false;

                    builder.Line($"return {emptyCreationExpression};");
                    error = string.Empty;
                    return true;
                }

                if (!_owner.TryGetCreationExpression(type, out var creationExpression, out error))
                    return false;

                if (!_owner.IsExplicitStruct(type))
                {
                    builder.Line("var memberCount = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                    builder.Line("if (memberCount is null)");
                    builder.Indent();
                    builder.Line("return null!;");
                    builder.Unindent();
                }
                if (setupEncoding)
                    EmitReaderEncodingSetup(builder);
                builder.Line($"var instance = {creationExpression};");
                foreach (var member in members)
                {
                    if (!EmitReadMember(member, builder, out error))
                        return false;
                }
                builder.Line("return instance;");
                return true;
            }

            private bool EmitWriteMember(MemberModel member, CodeBuilder builder, out string error, string ownerExpression = "value")
            {
                var access = $"{ownerExpression}.{Escape(member.Symbol.Name)}";
                if (_owner.TryGetBasicType(member.Type, out var basicType))
                {
                    builder.Line(_owner.GetWriteStatement(member.Type, basicType, access));
                    error = string.Empty;
                    return true;
                }
                if (_owner.IsEnum(member.Type))
                {
                    builder.Line(_owner.GetEnumWriteStatement(member.Type, access));
                    error = string.Empty;
                    return true;
                }
                if (member.Type is IArrayTypeSymbol arrayType)
                    return EmitWriteNestedArray(arrayType, access, builder, out error);
                if (_owner.TryGetDictionaryInfo(member.Type, out var dict))
                    return EmitWriteNestedDictionary(dict, access, builder, out error);
                if (_owner.TryGetCollectionInfo(member.Type, out var coll))
                    return EmitWriteNestedCollection(coll, access, builder, out error);
                if (_owner.IsNameValueCollection(member.Type))
                    return EmitWriteNestedNameValue(access, builder, out error);

                return EmitWriteNestedComplex(member.Type, access, builder, out error);
            }

            private bool EmitWriteNestedComplex(ITypeSymbol type, string expression, CodeBuilder builder, out string error)
            {
                if (ShouldUseNestedFunction(type))
                {
                    EmitWriteNestedComplexFunctionCall(type, expression, builder);
                    error = string.Empty;
                    return true;
                }

                if (_owner.IsExplicitStruct(type))
                {
                    return EmitWriteObject(type, builder, out error, suppressNullCheck: true, expression);
                }

                builder.Line($"if ({expression} is null)");
                builder.Line("{");
                builder.Indent();
                builder.Line("BoisPrimitiveWriters.WriteNullValue(writer);");
                builder.Unindent();
                builder.Line("}");
                builder.Line("else");
                builder.Line("{");
                builder.Indent();
                builder.Line("writer.Write((byte)0);");
                if (!EmitWriteObject(type, builder, out error, suppressNullCheck: true, expression))
                    return false;
                builder.Unindent();
                builder.Line("}");
                error = string.Empty;
                return true;
            }

            private void EmitWriteNestedComplexFunctionCall(ITypeSymbol type, string expression, CodeBuilder builder)
            {
                if (_owner.IsExplicitStruct(type))
                {
                    builder.Line($"{EnsureWriteFunction(type)}(writer, {expression}, encoding);");
                    return;
                }

                builder.Line($"if ({expression} is null)");
                builder.Line("{");
                builder.Indent();
                builder.Line("BoisPrimitiveWriters.WriteNullValue(writer);");
                builder.Unindent();
                builder.Line("}");
                builder.Line("else");
                builder.Line("{");
                builder.Indent();
                builder.Line("writer.Write((byte)0);");
                builder.Line($"{EnsureWriteFunction(type)}(writer, {expression}, encoding);");
                builder.Unindent();
                builder.Line("}");
            }

            private bool EmitReadMember(MemberModel member, CodeBuilder builder, out string error)
                => EmitReadMemberInto(member, "instance", builder, out error);

            private bool EmitReadMemberInto(MemberModel member, string instanceExpression, CodeBuilder builder, out string error)
            {
                var target = $"{instanceExpression}.{Escape(member.Symbol.Name)}";
                if (_owner.TryGetBasicType(member.Type, out var basicType))
                {
                    builder.Line($"{target} = {_owner.GetReadExpression(member.Type, basicType)};");
                    error = string.Empty;
                    return true;
                }
                if (_owner.IsEnum(member.Type))
                {
                    builder.Line($"{target} = BoisPrimitiveReaders.ReadEnumGeneric<{TypeName(member.Type)}>(reader);");
                    error = string.Empty;
                    return true;
                }

                if (member.IsGetterOnlyMutableCollection)
                {
                    var mutableTargetName = Escape(member.Symbol.Name) + "Target";
                    builder.Line($"var {mutableTargetName} = {target} ?? throw new global::System.InvalidOperationException({Literal($"Property '{member.Symbol.Name}' returned null during deserialization.")});");
                    builder.Line($"{mutableTargetName}.Clear();");
                    if (_owner.TryGetDictionaryInfo(member.Type, out var dict))
                    {
                        EmitReadIntoDictionary(dict, mutableTargetName, builder);
                        error = string.Empty;
                        return true;
                    }
                    if (_owner.TryGetCollectionInfo(member.Type, out var coll))
                    {
                        EmitReadIntoCollection(coll, mutableTargetName, builder);
                        error = string.Empty;
                        return true;
                    }
                    if (_owner.IsNameValueCollection(member.Type))
                    {
                        EmitReadIntoNameValue(mutableTargetName, builder);
                        error = string.Empty;
                        return true;
                    }
                }

                var localName = Escape(member.Symbol.Name) + "Items";
                if (member.Type is IArrayTypeSymbol memberArrayType)
                {
                    EmitReadNestedArray(memberArrayType, target, localName, builder);
                    error = string.Empty;
                    return true;
                }
                if (_owner.TryGetDictionaryInfo(member.Type, out var memberDictionary))
                {
                    EmitReadNestedDictionary(memberDictionary, target, localName, builder);
                    error = string.Empty;
                    return true;
                }
                if (_owner.TryGetCollectionInfo(member.Type, out var memberCollection))
                {
                    EmitReadNestedCollection(memberCollection, target, localName, builder);
                    error = string.Empty;
                    return true;
                }
                if (_owner.IsNameValueCollection(member.Type))
                {
                    EmitReadNestedNameValue(target, localName, builder);
                    error = string.Empty;
                    return true;
                }

                var nestedValueName = Escape(member.Symbol.Name) + "Value";
                if (!EmitReadNestedComplex(member.Type, target, nestedValueName, builder, out error))
                    return false;
                error = string.Empty;
                return true;
            }

            private bool EmitReadNestedComplex(ITypeSymbol type, string target, string localName, CodeBuilder builder, out string error)
            {
                if (ShouldUseNestedFunction(type))
                {
                    EmitReadNestedComplexFunctionCall(type, target, builder);
                    error = string.Empty;
                    return true;
                }

                if (_owner.IsExplicitStruct(type))
                {
                    return EmitReadObjectInto(type, target, localName, builder, out error, hasNullMarker: false);
                }

                builder.Line("if (reader.ReadByte() == 0b01000000)");
                builder.Indent();
                builder.Line($"{target} = null!;");
                builder.Unindent();
                builder.Line("else");
                builder.Line("{");
                builder.Indent();
                if (!EmitReadObjectInto(type, target, localName, builder, out error, hasNullMarker: true))
                    return false;
                builder.Unindent();
                builder.Line("}");
                error = string.Empty;
                return true;
            }

            private void EmitReadNestedComplexFunctionCall(ITypeSymbol type, string target, CodeBuilder builder)
            {
                if (_owner.IsExplicitStruct(type))
                {
                    builder.Line($"{target} = {EnsureReadFunction(type)}(reader, encoding);");
                    return;
                }

                builder.Line("if (reader.ReadByte() == 0b01000000)");
                builder.Indent();
                builder.Line($"{target} = null!;");
                builder.Unindent();
                builder.Line("else");
                builder.Indent();
                builder.Line($"{target} = {EnsureReadFunction(type)}(reader, encoding);");
                builder.Unindent();
            }

            private bool EmitReadObjectInto(ITypeSymbol type, string target, string localName, CodeBuilder builder, out string error, bool hasNullMarker)
            {
                if (!_owner.TryGetMembers(type, out var members, out error))
                    return false;

                if (members.Length == 0)
                {
                    if (!_owner.TryGetCreationExpression(type, out var emptyCreationExpression, out error))
                        return false;

                    builder.Line($"{target} = {emptyCreationExpression};");
                    error = string.Empty;
                    return true;
                }

                if (!_owner.TryGetCreationExpression(type, out var creationExpression, out error))
                    return false;

                if (!_owner.IsExplicitStruct(type))
                {
                    builder.Line("var memberCount = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                    builder.Line("if (memberCount is null)");
                    builder.Indent();
                    builder.Line($"{target} = null!;");
                    builder.Unindent();
                    builder.Line("else");
                    builder.Line("{");
                    builder.Indent();
                }

                builder.Line($"var {localName} = {creationExpression};");
                foreach (var member in members)
                {
                    if (!EmitReadMemberInto(member, localName, builder, out error))
                        return false;
                }
                builder.Line($"{target} = {localName};");

                if (!_owner.IsExplicitStruct(type))
                {
                    builder.Unindent();
                    builder.Line("}");
                }

                error = string.Empty;
                return true;
            }

            private void EmitReadNestedArray(IArrayTypeSymbol arrayType, string target, string localName, CodeBuilder builder)
            {
                var countName = localName + "Count";
                builder.Line($"var {countName} = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line($"if ({countName} is null)");
                builder.Indent();
                builder.Line($"{target} = null!;");
                builder.Unindent();
                builder.Line("else");
                builder.Line("{");
                builder.Indent();
                builder.Line($"var {localName} = new {TypeName(arrayType.ElementType)}[(int){countName}.Value];");
                builder.Line($"for (var i = 0; i < {localName}.Length; i++)");
                builder.Line("{");
                builder.Indent();
                builder.Line($"{localName}[i] = {GetReadValueExpression(arrayType.ElementType)};");
                builder.Unindent();
                builder.Line("}");
                builder.Line($"{target} = {localName};");
                builder.Unindent();
                builder.Line("}");
            }

            private void EmitReadNestedCollection(CollectionInfo collectionInfo, string target, string localName, CodeBuilder builder)
            {
                var countName = localName + "Count";
                builder.Line($"var {countName} = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line($"if ({countName} is null)");
                builder.Indent();
                builder.Line($"{target} = null!;");
                builder.Unindent();
                builder.Line("else");
                builder.Line("{");
                builder.Indent();
                builder.Line($"var {localName} = {GetCreationExpressionOrThrow(collectionInfo.Type)};");
                EmitReadIntoCollectionBody(collectionInfo, localName, countName + ".Value", builder);
                builder.Line($"{target} = {localName};");
                builder.Unindent();
                builder.Line("}");
            }

            private void EmitReadNestedDictionary(DictionaryInfo dictionaryInfo, string target, string localName, CodeBuilder builder)
            {
                var countName = localName + "Count";
                builder.Line($"var {countName} = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line($"if ({countName} is null)");
                builder.Indent();
                builder.Line($"{target} = null!;");
                builder.Unindent();
                builder.Line("else");
                builder.Line("{");
                builder.Indent();
                builder.Line($"var {localName} = {GetCreationExpressionOrThrow(dictionaryInfo.Type)};");
                EmitReadIntoDictionaryBody(dictionaryInfo, localName, countName + ".Value", builder);
                builder.Line($"{target} = {localName};");
                builder.Unindent();
                builder.Line("}");
            }

            private void EmitReadNestedNameValue(string target, string localName, CodeBuilder builder)
            {
                var countName = localName + "Count";
                builder.Line($"var {countName} = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line($"if ({countName} is null)");
                builder.Indent();
                builder.Line($"{target} = null!;");
                builder.Unindent();
                builder.Line("else");
                builder.Line("{");
                builder.Indent();
                builder.Line($"var {localName} = new global::System.Collections.Specialized.NameValueCollection();");
                EmitReadIntoNameValueBody(localName, countName + ".Value", builder);
                builder.Line($"{target} = {localName};");
                builder.Unindent();
                builder.Line("}");
            }

            private string GetCreationExpressionOrThrow(ITypeSymbol type)
            {
                if (_owner.TryGetCreationExpression(type, out var expression, out var error))
                    return expression;

                throw new global::System.InvalidOperationException(error);
            }

            private bool EmitWriteArray(IArrayTypeSymbol arrayType, CodeBuilder builder, out string error, bool suppressNullCheck = false)
                => EmitWriteNestedArray(arrayType, "value", builder, out error, suppressNullCheck);

            private bool EmitWriteNestedArray(IArrayTypeSymbol arrayType, string expression, CodeBuilder builder, out string error, bool suppressNullCheck = false)
            {
                if (!suppressNullCheck)
                {
                    builder.Line($"if ({expression} is null)");
                    builder.Line("{");
                    builder.Indent();
                    builder.Line("BoisPrimitiveWriters.WriteNullValue(writer);");
                    builder.Unindent();
                    builder.Line("}");
                    builder.Line("else");
                    builder.Line("{");
                    builder.Indent();
                }
                builder.Line($"BoisNumericSerializers.WriteUIntNullableMemberCount(writer, (uint){expression}.Length);");
                builder.Line($"foreach (var item in {expression})");
                builder.Line("{");
                builder.Indent();
                builder.Line(GetWriteElementStatement(arrayType.ElementType, "item"));
                builder.Unindent();
                builder.Line("}");
                if (!suppressNullCheck)
                {
                    builder.Unindent();
                    builder.Line("}");
                }
                error = string.Empty;
                return true;
            }

            private bool EmitReadArray(IArrayTypeSymbol arrayType, CodeBuilder builder, out string error, bool setupEncoding = false)
            {
                builder.Line("var itemCount = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line("if (itemCount is null)");
                builder.Indent();
                builder.Line("return null!;");
                builder.Unindent();
                if (setupEncoding)
                    EmitReaderEncodingSetup(builder);
                builder.Line($"var items = new {TypeName(arrayType.ElementType)}[(int)itemCount.Value];");
                builder.Line("for (var i = 0; i < items.Length; i++)");
                builder.Line("{");
                builder.Indent();
                builder.Line($"items[i] = {GetReadValueExpression(arrayType.ElementType)};");
                builder.Unindent();
                builder.Line("}");
                builder.Line("return items;");
                error = string.Empty;
                return true;
            }

            private bool EmitWriteCollection(CollectionInfo collectionInfo, CodeBuilder builder, out string error, bool suppressNullCheck = false)
                => EmitWriteNestedCollection(collectionInfo, "value", builder, out error, suppressNullCheck);

            private bool EmitWriteNestedCollection(CollectionInfo collectionInfo, string expression, CodeBuilder builder, out string error, bool suppressNullCheck = false)
            {
                if (!suppressNullCheck)
                {
                    builder.Line($"if ({expression} is null)");
                    builder.Line("{");
                    builder.Indent();
                    builder.Line("BoisPrimitiveWriters.WriteNullValue(writer);");
                    builder.Unindent();
                    builder.Line("}");
                    builder.Line("else");
                    builder.Line("{");
                    builder.Indent();
                }
                builder.Line($"BoisNumericSerializers.WriteUIntNullableMemberCount(writer, (uint){expression}.Count);");
                builder.Line($"foreach (var item in {expression})");
                builder.Line("{");
                builder.Indent();
                builder.Line(GetWriteElementStatement(collectionInfo.ElementType, "item"));
                builder.Unindent();
                builder.Line("}");
                if (!suppressNullCheck)
                {
                    builder.Unindent();
                    builder.Line("}");
                }
                error = string.Empty;
                return true;
            }

            private bool EmitReadCollection(ITypeSymbol type, CollectionInfo collectionInfo, CodeBuilder builder, out string error, bool setupEncoding = false)
            {
                if (!_owner.TryGetCreationExpression(type, out var create, out error))
                    return false;
                builder.Line("var itemCount = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line("if (itemCount is null)");
                builder.Indent();
                builder.Line("return null!;");
                builder.Unindent();
                if (setupEncoding)
                    EmitReaderEncodingSetup(builder);
                builder.Line($"var items = {create};");
                EmitReadIntoCollectionBody(collectionInfo, "items", "itemCount.Value", builder);
                builder.Line("return items;");
                error = string.Empty;
                return true;
            }

            private void EmitReadIntoCollection(CollectionInfo collectionInfo, string target, CodeBuilder builder)
            {
                builder.Line("var itemCount = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line("if (itemCount is not null)");
                builder.Line("{");
                builder.Indent();
                EmitReadIntoCollectionBody(collectionInfo, target, "itemCount.Value", builder);
                builder.Unindent();
                builder.Line("}");
            }

            private void EmitReadIntoCollectionBody(CollectionInfo collectionInfo, string target, string countExpression, CodeBuilder builder)
            {
                builder.Line($"for (var i = 0; i < {countExpression}; i++)");
                builder.Line("{");
                builder.Indent();
                builder.Line($"{target}.Add({GetReadValueExpression(collectionInfo.ElementType)});");
                builder.Unindent();
                builder.Line("}");
            }

            private bool EmitWriteDictionary(DictionaryInfo dictionaryInfo, CodeBuilder builder, out string error, bool suppressNullCheck = false)
                => EmitWriteNestedDictionary(dictionaryInfo, "value", builder, out error, suppressNullCheck);

            private bool EmitWriteNestedDictionary(DictionaryInfo dictionaryInfo, string expression, CodeBuilder builder, out string error, bool suppressNullCheck = false)
            {
                if (!suppressNullCheck)
                {
                    builder.Line($"if ({expression} is null)");
                    builder.Line("{");
                    builder.Indent();
                    builder.Line("BoisPrimitiveWriters.WriteNullValue(writer);");
                    builder.Unindent();
                    builder.Line("}");
                    builder.Line("else");
                    builder.Line("{");
                    builder.Indent();
                }
                builder.Line($"BoisNumericSerializers.WriteUIntNullableMemberCount(writer, (uint){expression}.Count);");
                builder.Line($"foreach (var item in {expression})");
                builder.Line("{");
                builder.Indent();
                builder.Line(GetWriteElementStatement(dictionaryInfo.KeyType, "item.Key"));
                builder.Line(GetWriteElementStatement(dictionaryInfo.ValueType, "item.Value"));
                builder.Unindent();
                builder.Line("}");
                if (!suppressNullCheck)
                {
                    builder.Unindent();
                    builder.Line("}");
                }
                error = string.Empty;
                return true;
            }

            private bool EmitReadDictionary(ITypeSymbol type, DictionaryInfo dictionaryInfo, CodeBuilder builder, out string error, bool setupEncoding = false)
            {
                if (!_owner.TryGetCreationExpression(type, out var create, out error))
                    return false;
                builder.Line("var itemCount = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line("if (itemCount is null)");
                builder.Indent();
                builder.Line("return null!;");
                builder.Unindent();
                if (setupEncoding)
                    EmitReaderEncodingSetup(builder);
                builder.Line($"var items = {create};");
                EmitReadIntoDictionaryBody(dictionaryInfo, "items", "itemCount.Value", builder);
                builder.Line("return items;");
                error = string.Empty;
                return true;
            }

            private void EmitReadIntoDictionary(DictionaryInfo dictionaryInfo, string target, CodeBuilder builder)
            {
                builder.Line("var itemCount = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line("if (itemCount is not null)");
                builder.Line("{");
                builder.Indent();
                EmitReadIntoDictionaryBody(dictionaryInfo, target, "itemCount.Value", builder);
                builder.Unindent();
                builder.Line("}");
            }

            private void EmitReadIntoDictionaryBody(DictionaryInfo dictionaryInfo, string target, string countExpression, CodeBuilder builder)
            {
                builder.Line($"for (var i = 0; i < {countExpression}; i++)");
                builder.Line("{");
                builder.Indent();
                builder.Line($"var key = {GetReadValueExpression(dictionaryInfo.KeyType)};");
                builder.Line($"var value = {GetReadValueExpression(dictionaryInfo.ValueType)};");
                builder.Line($"{target}.Add(key, value);");
                builder.Unindent();
                builder.Line("}");
            }

            private bool EmitWriteNameValueCollection(CodeBuilder builder, out string error, bool suppressNullCheck = false)
                => EmitWriteNestedNameValue("value", builder, out error, suppressNullCheck);

            private bool EmitWriteNestedNameValue(string expression, CodeBuilder builder, out string error, bool suppressNullCheck = false)
            {
                if (!suppressNullCheck)
                {
                    builder.MultiLine($$"""
                    if ({{expression}} is null)
                    {
                        BoisPrimitiveWriters.WriteNullValue(writer);
                    }
                    else
                    {
                    """);
                    builder.Indent();
                }
                builder.Line($"BoisNumericSerializers.WriteUIntNullableMemberCount(writer, (uint){expression}.Count);");
                builder.Line($"foreach (var key in {expression}.AllKeys)");
                builder.Line("{");
                builder.Indent();
                builder.Line("BoisPrimitiveWriters.WriteValue(writer, key, encoding);");
                builder.Line($"BoisPrimitiveWriters.WriteValue(writer, {expression}[key], encoding);");
                builder.Unindent();
                builder.Line("}");
                if (!suppressNullCheck)
                {
                    builder.Unindent();
                    builder.Line("}");
                }
                error = string.Empty;
                return true;
            }

            private bool EmitReadNameValueCollection(ITypeSymbol type, CodeBuilder builder, out string error, bool setupEncoding = false)
            {
                if (!_owner.TryGetCreationExpression(type, out var create, out error))
                    return false;
                builder.Line("var itemCount = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line("if (itemCount is null)");
                builder.Indent();
                builder.Line("return null!;");
                builder.Unindent();
                if (setupEncoding)
                    EmitReaderEncodingSetup(builder);
                builder.Line($"var items = {create};");
                EmitReadIntoNameValueBody("items", "itemCount.Value", builder);
                builder.Line("return items;");
                error = string.Empty;
                return true;
            }

            private void EmitReadIntoNameValue(string target, CodeBuilder builder)
            {
                builder.Line("var itemCount = BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line("if (itemCount is not null)");
                builder.Line("{");
                builder.Indent();
                EmitReadIntoNameValueBody(target, "itemCount.Value", builder);
                builder.Unindent();
                builder.Line("}");
            }

            private void EmitReadIntoNameValueBody(string target, string countExpression, CodeBuilder builder)
            {
                builder.Line($"for (var i = 0; i < {countExpression}; i++)");
                builder.Line("{");
                builder.Indent();
                builder.Line("var key = BoisPrimitiveReaders.ReadString(reader, encoding);");
                builder.Line("var value = BoisPrimitiveReaders.ReadString(reader, encoding);");
                builder.Line($"{target}.Add(key, value);");
                builder.Unindent();
                builder.Line("}");
            }

            private string GetWriteElementStatement(ITypeSymbol type, string expression)
            {
                if (_owner.TryGetBasicType(type, out var basicType))
                    return _owner.GetWriteStatement(type, basicType, expression);
                if (_owner.IsEnum(type))
                    return _owner.GetEnumWriteStatement(type, expression);
                return $"{EnsureWriteFunction(type)}(writer, {expression}, encoding);";
            }

            private string GetReadValueExpression(ITypeSymbol type)
            {
                if (_owner.TryGetBasicType(type, out var basicType))
                    return _owner.GetReadExpression(type, basicType);
                if (_owner.IsEnum(type))
                    return $"BoisPrimitiveReaders.ReadEnumGeneric<{TypeName(type)}>(reader)";
                return $"{EnsureReadFunction(type)}(reader, encoding)";
            }

            private string BuildSignature(IMethodSymbol method)
            {
                var access = method.DeclaredAccessibility switch
                {
                    Accessibility.Public => "public ",
                    Accessibility.Internal => "internal ",
                    _ => string.Empty
                };
                var returnType = method.ReturnsVoid ? "void" : TypeName(method.ReturnType);
                var parameters = string.Join(", ", method.Parameters.Select(static p => $"{p.Type.ToDisplayString(QualifiedTypeFormat)} {Escape(p.Name)}"));
                return $"{access}static partial {returnType} {method.Name}({parameters})";
            }

            private static string Escape(string value) => SyntaxFacts.GetKeywordKind(value) != SyntaxKind.None ? "@" + value : value;

            private string TypeName(ITypeSymbol type) => type.ToDisplayString(QualifiedTypeFormat);
        }

        public void Report(Location? location, string message)
            => _report(Diagnostic.Create(UnsupportedType, location, message));

        public string TypeKey(ITypeSymbol type) => type.ToDisplayString(QualifiedTypeFormat);

        public ITypeSymbol LocalFunctionType(ITypeSymbol type) => Bare(type).WithNullableAnnotation(NullableAnnotation.NotAnnotated);

        public bool ValidateRootType(ITypeSymbol type, out string error)
        {
            if (IsNullableComplex(type))
            {
                error = $"Nullable complex type '{type.ToDisplayString()}' is not supported by the BOIS source generator.";
                return false;
            }
            error = string.Empty;
            return true;
        }

        public bool TryGetMembers(ITypeSymbol type, out ImmutableArray<MemberModel> members, out string error)
        {
            error = string.Empty;
            var ordered = new List<MemberModel>();
            var readFields = true;
            var readProps = true;
            foreach (var attribute in type.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() == "Salar.Bois.BoisContractAttribute" && attribute.ConstructorArguments.Length == 2)
                {
                    readFields = attribute.ConstructorArguments[0].Value as bool? ?? true;
                    readProps = attribute.ConstructorArguments[1].Value as bool? ?? true;
                    break;
                }
            }

            foreach (var symbol in EnumerateMembers(type, readFields, readProps))
            {
                var (included, index) = ReadMemberOptions(symbol);
                if (!included)
                    continue;

                if (symbol is IFieldSymbol field)
                {
                    if (field.IsStatic || field.DeclaredAccessibility != Accessibility.Public || field.IsReadOnly)
                        continue;
                    Insert(ordered, new MemberModel(field, field.Type, index, false));
                    continue;
                }

                if (symbol is IPropertySymbol property)
                {
                    if (property.IsStatic ||
                        property.DeclaredAccessibility != Accessibility.Public ||
                        property.IsIndexer ||
                        property.GetMethod is null ||
                        property.GetMethod.DeclaredAccessibility != Accessibility.Public)
                        continue;

                    if (property.SetMethod is not null && property.SetMethod.DeclaredAccessibility == Accessibility.Public)
                    {
                        Insert(ordered, new MemberModel(property, property.Type, index, false));
                    }
                }
            }

            members = ordered.ToImmutableArray();
            return true;
        }

        private static IEnumerable<ISymbol> EnumerateMembers(ITypeSymbol type, bool includeFields, bool includeProperties)
        {
            var stack = new Stack<INamedTypeSymbol>();
            for (var current = type as INamedTypeSymbol; current is not null; current = current.BaseType)
                stack.Push(current);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (includeFields)
                {
                    foreach (var field in current.GetMembers().OfType<IFieldSymbol>())
                        yield return field;
                }
                if (includeProperties)
                {
                    foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
                        yield return property;
                }
            }
        }

        private static void Insert(List<MemberModel> members, MemberModel member)
        {
            if (member.Index >= 0)
                members.Insert(Math.Min(member.Index, members.Count), member);
            else
                members.Add(member);
        }

        private static (bool Included, int Index) ReadMemberOptions(ISymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() != "Salar.Bois.BoisMemberAttribute")
                    continue;

                var included = true;
                var index = -1;
                if (attribute.ConstructorArguments.Length == 1)
                {
                    var argument = attribute.ConstructorArguments[0];
                    if (argument.Type?.SpecialType == SpecialType.System_Int32)
                        index = argument.Value as int? ?? -1;
                    else if (argument.Type?.SpecialType == SpecialType.System_Boolean)
                        included = argument.Value as bool? ?? true;
                }
                else if (attribute.ConstructorArguments.Length == 2)
                {
                    index = attribute.ConstructorArguments[0].Value as int? ?? -1;
                    included = attribute.ConstructorArguments[1].Value as bool? ?? true;
                }
                return (included, index);
            }
            return (true, -1);
        }

        public bool TryGetCreationExpression(ITypeSymbol type, out string expression, out string error)
        {
            var bare = Bare(type);
            var constructionType = bare.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            if (IsExplicitStruct(type))
            {
                expression = "new " + constructionType.ToDisplayString(QualifiedTypeFormat) + "()";
                error = string.Empty;
                return true;
            }

            if (bare.TypeKind == TypeKind.Interface || bare.TypeKind == TypeKind.TypeParameter)
            {
                expression = string.Empty;
                error = $"Type '{type.ToDisplayString()}' must be concrete or provide an existing initialized instance for generated BOIS code.";
                return false;
            }

            if (bare is not INamedTypeSymbol named || !named.InstanceConstructors.Any(static c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public))
            {
                expression = string.Empty;
                error = $"Type '{type.ToDisplayString()}' must expose a public parameterless constructor for generated BOIS code.";
                return false;
            }

            expression = "new " + constructionType.ToDisplayString(QualifiedTypeFormat) + "()";
            error = string.Empty;
            return true;
        }

        public bool TryGetBasicType(ITypeSymbol type, out BasicType basicType)
        {
            var bare = Bare(type);
            var nullableValue = IsNullableValueType(type);

            if (bare.SpecialType == SpecialType.System_String)
            {
                basicType = BasicType.String;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_Boolean)
            {
                basicType = nullableValue ? BasicType.BoolNullable : BasicType.Bool;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_Char)
            {
                basicType = nullableValue ? BasicType.CharNullable : BasicType.Char;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_Int16)
            {
                basicType = nullableValue ? BasicType.Int16Nullable : BasicType.Int16;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_Int32)
            {
                basicType = nullableValue ? BasicType.Int32Nullable : BasicType.Int32;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_Int64)
            {
                basicType = nullableValue ? BasicType.Int64Nullable : BasicType.Int64;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_UInt16)
            {
                basicType = nullableValue ? BasicType.UInt16Nullable : BasicType.UInt16;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_UInt32)
            {
                basicType = nullableValue ? BasicType.UInt32Nullable : BasicType.UInt32;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_UInt64)
            {
                basicType = nullableValue ? BasicType.UInt64Nullable : BasicType.UInt64;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_Single)
            {
                basicType = nullableValue ? BasicType.SingleNullable : BasicType.Single;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_Double)
            {
                basicType = nullableValue ? BasicType.DoubleNullable : BasicType.Double;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_Decimal)
            {
                basicType = nullableValue ? BasicType.DecimalNullable : BasicType.Decimal;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_Byte)
            {
                basicType = nullableValue ? BasicType.ByteNullable : BasicType.Byte;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_SByte)
            {
                basicType = nullableValue ? BasicType.SByteNullable : BasicType.SByte;
                return true;
            }
            if (bare.SpecialType == SpecialType.System_DateTime)
            {
                basicType = nullableValue ? BasicType.DateTimeNullable : BasicType.DateTime;
                return true;
            }
            if (Equal(bare, _dateTimeOffsetType))
            {
                basicType = nullableValue ? BasicType.DateTimeOffsetNullable : BasicType.DateTimeOffset;
                return true;
            }
            if (Equal(bare, _dateOnlyType))
            {
                basicType = nullableValue ? BasicType.DateOnlyNullable : BasicType.DateOnly;
                return true;
            }
            if (Equal(bare, _timeOnlyType))
            {
                basicType = nullableValue ? BasicType.TimeOnlyNullable : BasicType.TimeOnly;
                return true;
            }
            if (bare is IArrayTypeSymbol arrayType && arrayType.ElementType.SpecialType == SpecialType.System_Byte)
            {
                basicType = BasicType.ByteArray;
                return true;
            }
            if (Equal(bare, _timeSpanType))
            {
                basicType = nullableValue ? BasicType.TimeSpanNullable : BasicType.TimeSpan;
                return true;
            }
            if (Equal(bare, _guidType))
            {
                basicType = nullableValue ? BasicType.GuidNullable : BasicType.Guid;
                return true;
            }
            if (Equal(bare, _colorType))
            {
                basicType = nullableValue ? BasicType.ColorNullable : BasicType.Color;
                return true;
            }
            if (Equal(bare, _dbNullType))
            {
                basicType = BasicType.DbNull;
                return true;
            }
            if (Equal(bare, _uriType))
            {
                basicType = BasicType.Uri;
                return true;
            }
            if (Equal(bare, _versionType))
            {
                basicType = BasicType.Version;
                return true;
            }
            if (EqualOrDerivedFrom(bare, _dataTableType))
            {
                basicType = BasicType.DataTable;
                return true;
            }
            if (EqualOrDerivedFrom(bare, _dataSetType))
            {
                basicType = BasicType.DataSet;
                return true;
            }

            basicType = default;
            return false;
        }

        public string GetWriteStatement(ITypeSymbol type, BasicType basicType, string expression)
        {
            switch (basicType)
            {
                case BasicType.String:
                    return $"BoisPrimitiveWriters.WriteValue(writer, {expression}, encoding);";

                case BasicType.Bool:
                case BasicType.BoolNullable:
                case BasicType.Char:
                case BasicType.CharNullable:
                case BasicType.DateTime:
                case BasicType.DateTimeNullable:
                case BasicType.DateTimeOffset:
                case BasicType.DateTimeOffsetNullable:
                case BasicType.TimeSpan:
                case BasicType.TimeSpanNullable:
                case BasicType.Guid:
                case BasicType.GuidNullable:
                case BasicType.Color:
                case BasicType.ColorNullable:
                case BasicType.DbNull:
                case BasicType.Uri:
                case BasicType.Version:
                case BasicType.DateOnly:
                case BasicType.DateOnlyNullable:
                case BasicType.TimeOnly:
                case BasicType.TimeOnlyNullable:
                case BasicType.ByteArray:
                    return $"BoisPrimitiveWriters.WriteValue(writer, {expression});";

                case BasicType.Int16:
                case BasicType.Int16Nullable:
                case BasicType.Int32:
                case BasicType.Int32Nullable:
                case BasicType.Int64:
                case BasicType.Int64Nullable:
                case BasicType.UInt16:
                case BasicType.UInt16Nullable:
                case BasicType.UInt32:
                case BasicType.UInt32Nullable:
                case BasicType.UInt64:
                case BasicType.UInt64Nullable:
                case BasicType.ByteNullable:
                case BasicType.SByteNullable:
                    return $"BoisNumericSerializers.WriteVarInt(writer, {expression});";

                case BasicType.Single:
                case BasicType.SingleNullable:
                case BasicType.Double:
                case BasicType.DoubleNullable:
                case BasicType.Decimal:
                case BasicType.DecimalNullable:
                    return $"BoisNumericSerializers.WriteVarDecimal(writer, {expression});";

                case BasicType.Byte:
                    return $"BoisNumericSerializers.WriteByte(writer, {expression});";

                case BasicType.SByte:
                    return $"BoisNumericSerializers.WriteSByte(writer, {expression});";

                case BasicType.DataTable:
                case BasicType.DataSet:
                    return $"BoisPrimitiveWriters.WriteValue(writer, {expression}, encoding);";

                default:
                    throw new InvalidOperationException();
            }
        }

        public string GetEnumWriteStatement(ITypeSymbol type, string expression)
        {
            var enumExpression = IsNullableValueType(type)
                ? $"({expression}.HasValue ? (global::System.Enum)(object){expression}.Value : null)"
                : $"((global::System.Enum)(object){expression})";
            var isNullable = IsNullable(type) ? "true" : "false";
            return $"BoisPrimitiveWriters.WriteValue(writer, {enumExpression}, typeof({Bare(type).ToDisplayString(QualifiedTypeFormat)}), {isNullable});";
        }

        public string GetReadExpression(ITypeSymbol type, BasicType basicType) => basicType switch
        {
            BasicType.String => "BoisPrimitiveReaders.ReadString(reader, encoding)",
            BasicType.Bool => "BoisPrimitiveReaders.ReadBoolean(reader)",
            BasicType.BoolNullable => "BoisPrimitiveReaders.ReadBooleanNullable(reader)",
            BasicType.Char => "BoisPrimitiveReaders.ReadChar(reader)",
            BasicType.CharNullable => "BoisPrimitiveReaders.ReadCharNullable(reader)",
            BasicType.Int16 => "BoisNumericSerializers.ReadVarInt16(reader)",
            BasicType.Int16Nullable => "BoisNumericSerializers.ReadVarInt16Nullable(reader)",
            BasicType.Int32 => "BoisNumericSerializers.ReadVarInt32(reader)",
            BasicType.Int32Nullable => "BoisNumericSerializers.ReadVarInt32Nullable(reader)",
            BasicType.Int64 => "BoisNumericSerializers.ReadVarInt64(reader)",
            BasicType.Int64Nullable => "BoisNumericSerializers.ReadVarInt64Nullable(reader)",
            BasicType.UInt16 => "BoisNumericSerializers.ReadVarUInt16(reader)",
            BasicType.UInt16Nullable => "BoisNumericSerializers.ReadVarUInt16Nullable(reader)",
            BasicType.UInt32 => "BoisNumericSerializers.ReadVarUInt32(reader)",
            BasicType.UInt32Nullable => "BoisNumericSerializers.ReadVarUInt32Nullable(reader)",
            BasicType.UInt64 => "BoisNumericSerializers.ReadVarUInt64(reader)",
            BasicType.UInt64Nullable => "BoisNumericSerializers.ReadVarUInt64Nullable(reader)",
            BasicType.Single => "BoisNumericSerializers.ReadVarSingle(reader)",
            BasicType.SingleNullable => "BoisNumericSerializers.ReadVarSingleNullable(reader)",
            BasicType.Double => "BoisNumericSerializers.ReadVarDouble(reader)",
            BasicType.DoubleNullable => "BoisNumericSerializers.ReadVarDoubleNullable(reader)",
            BasicType.Decimal => "BoisNumericSerializers.ReadVarDecimal(reader)",
            BasicType.DecimalNullable => "BoisNumericSerializers.ReadVarDecimalNullable(reader)",
            BasicType.Byte => "BoisNumericSerializers.ReadByte(reader)",
            BasicType.ByteNullable => "BoisNumericSerializers.ReadVarByteNullable(reader)",
            BasicType.SByte => "BoisNumericSerializers.ReadSByte(reader)",
            BasicType.SByteNullable => "BoisNumericSerializers.ReadVarSByteNullable(reader)",
            BasicType.DateTime => "BoisPrimitiveReaders.ReadDateTime(reader)",
            BasicType.DateTimeNullable => "BoisPrimitiveReaders.ReadDateTimeNullable(reader)",
            BasicType.DateTimeOffset => "BoisPrimitiveReaders.ReadDateTimeOffset(reader)",
            BasicType.DateTimeOffsetNullable => "BoisPrimitiveReaders.ReadDateTimeOffsetNullable(reader)",
            BasicType.DateOnly => "BoisPrimitiveReaders.ReadDateOnly(reader)",
            BasicType.DateOnlyNullable => "BoisPrimitiveReaders.ReadDateOnlyNullable(reader)",
            BasicType.TimeOnly => "BoisPrimitiveReaders.ReadTimeOnly(reader)",
            BasicType.TimeOnlyNullable => "BoisPrimitiveReaders.ReadTimeOnlyNullable(reader)",
            BasicType.ByteArray => "BoisPrimitiveReaders.ReadByteArray(reader)",
            BasicType.TimeSpan => "BoisPrimitiveReaders.ReadTimeSpan(reader)",
            BasicType.TimeSpanNullable => "BoisPrimitiveReaders.ReadTimeSpanNullable(reader)",
            BasicType.Guid => "BoisPrimitiveReaders.ReadGuid(reader)",
            BasicType.GuidNullable => "BoisPrimitiveReaders.ReadGuidNullable(reader)",
            BasicType.Color => "BoisPrimitiveReaders.ReadColor(reader)",
            BasicType.ColorNullable => "BoisPrimitiveReaders.ReadColorNullable(reader)",
            BasicType.DbNull => "BoisPrimitiveReaders.ReadDbNull(reader)",
            BasicType.Uri => "BoisPrimitiveReaders.ReadUri(reader)",
            BasicType.Version => "BoisPrimitiveReaders.ReadVersion(reader)",
            BasicType.DataTable => "BoisPrimitiveReaders.ReadDataTable(reader, encoding)",
            BasicType.DataSet => "BoisPrimitiveReaders.ReadDataSet(reader, encoding)",
            _ => throw new InvalidOperationException()
        };

        public bool TryGetCollectionInfo(ITypeSymbol type, out CollectionInfo info)
        {
            var named = Bare(type) as INamedTypeSymbol;
            var iface = named?.AllInterfaces.FirstOrDefault(i => Equal(i.OriginalDefinition, _collectionType))
                       ?? (named is not null && Equal(named.OriginalDefinition, _collectionType) ? named : null);
            if (iface is null || iface.TypeArguments.Length != 1)
            {
                info = default;
                return false;
            }
            info = new CollectionInfo(type, iface.TypeArguments[0]);
            return true;
        }

        public bool TryGetDictionaryInfo(ITypeSymbol type, out DictionaryInfo info)
        {
            var named = Bare(type) as INamedTypeSymbol;
            var iface = named?.AllInterfaces.FirstOrDefault(i => Equal(i.OriginalDefinition, _dictionaryType))
                       ?? (named is not null && Equal(named.OriginalDefinition, _dictionaryType) ? named : null);
            if (iface is null || iface.TypeArguments.Length != 2)
            {
                info = default;
                return false;
            }
            info = new DictionaryInfo(type, iface.TypeArguments[0], iface.TypeArguments[1]);
            return true;
        }

        public bool IsNameValueCollection(ITypeSymbol type) => EqualOrDerivedFrom(Bare(type), _nameValueCollectionType);
        public bool IsEnum(ITypeSymbol type) => Bare(type).TypeKind == TypeKind.Enum;
        public bool IsExplicitStruct(ITypeSymbol type) => Bare(type).IsValueType && Bare(type).TypeKind != TypeKind.Enum && Bare(type).SpecialType == SpecialType.None;
        public bool IsNullable(ITypeSymbol type) => !Bare(type).IsValueType || IsNullableValueType(type);
        private bool IsNullableValueType(ITypeSymbol type) => type is INamedTypeSymbol named && Equal(named.OriginalDefinition, _nullableType);
        private bool IsNullableComplex(ITypeSymbol type) => IsNullableValueType(type) && !TryGetBasicType(type, out _) && !IsEnum(type);
        private ITypeSymbol Bare(ITypeSymbol type) => type is INamedTypeSymbol named && named.IsGenericType && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T ? named.TypeArguments[0] : type;
        private bool EqualOrDerivedFrom(ITypeSymbol type, ITypeSymbol? baseType)
        {
            for (var current = type as INamedTypeSymbol; current is not null; current = current.BaseType)
            {
                if (Equal(current, baseType))
                    return true;
            }
            return false;
        }
        private bool Equal(ITypeSymbol? left, ITypeSymbol? right) => left is not null && right is not null && SymbolEqualityComparer.Default.Equals(left, right);
        private static string Literal(string value) => SymbolDisplay.FormatLiteral(value, true);
    }

    private sealed class GroupComparer : IEqualityComparer<(INamedTypeSymbol ContainingType, string FileName)>
    {
        public bool Equals((INamedTypeSymbol ContainingType, string FileName) x, (INamedTypeSymbol ContainingType, string FileName) y)
            => SymbolEqualityComparer.Default.Equals(x.ContainingType, y.ContainingType) && x.FileName == y.FileName;

        public int GetHashCode((INamedTypeSymbol ContainingType, string FileName) obj)
        {
            unchecked
            {
                var hash = SymbolEqualityComparer.Default.GetHashCode(obj.ContainingType);
                hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(obj.FileName);
                return hash;
            }
        }
    }

    private sealed record GenerationMethod(IMethodSymbol Method, INamedTypeSymbol ContainingType, OperationKind Operation, ITypeSymbol RootType, MethodSignature Signature);
    private sealed record LocalFunctionModel(OperationKind Operation, ITypeSymbol Type, string Name);
    private sealed record MemberModel(ISymbol Symbol, ITypeSymbol Type, int Index, bool IsGetterOnlyMutableCollection);
    private sealed record CollectionInfo(ITypeSymbol Type, ITypeSymbol ElementType);
    private sealed record DictionaryInfo(ITypeSymbol Type, ITypeSymbol KeyType, ITypeSymbol ValueType);
    private abstract record MethodSignature(int? EncodingParameterIndex);
    private sealed record ReaderSignature(ReaderInputKind InputKind, int SourceParameterIndex, int PositionParameterIndex, int LengthParameterIndex, int? EncodingParameterIndex) : MethodSignature(EncodingParameterIndex);
    private sealed record WriterSignature(WriterOutputKind OutputKind, int ModelParameterIndex, int OutputParameterIndex, int PositionParameterIndex, int LengthParameterIndex, int? EncodingParameterIndex) : MethodSignature(EncodingParameterIndex);

    private enum OperationKind { Reader, Writer }
    private enum ReaderInputKind { Stream, BufferReader, ByteArray }
    private enum WriterOutputKind { Stream, BufferWriter, ByteArray }

    private enum BasicType
    {
        String,
        Bool,
        BoolNullable,
        Char,
        CharNullable,
        Int16,
        Int16Nullable,
        Int32,
        Int32Nullable,
        Int64,
        Int64Nullable,
        UInt16,
        UInt16Nullable,
        UInt32,
        UInt32Nullable,
        UInt64,
        UInt64Nullable,
        Single,
        SingleNullable,
        Double,
        DoubleNullable,
        Decimal,
        DecimalNullable,
        Byte,
        ByteNullable,
        SByte,
        SByteNullable,
        DateTime,
        DateTimeNullable,
        DateTimeOffset,
        DateTimeOffsetNullable,
        DateOnly,
        DateOnlyNullable,
        TimeOnly,
        TimeOnlyNullable,
        ByteArray,
        TimeSpan,
        TimeSpanNullable,
        Guid,
        GuidNullable,
        Color,
        ColorNullable,
        DbNull,
        Uri,
        Version,
        DataTable,
        DataSet,
    }

    private sealed class CodeBuilder
    {
        private readonly StringBuilder _builder = new();
        private int _indent;

        public void Line(string text = "")
        {
            if (text.Length > 0)
                _builder.Append(new string(' ', _indent * 4));
            _builder.AppendLine(text);
        }

        public void MultiLine(string text)
        {
            if (text.Length == 0)
                return;

            var indentText = new string(' ', _indent * 4);
            var normalizedText = text.Replace("\r\n", "\n").Replace('\r', '\n');
            var lines = normalizedText.Split('\n');

            foreach (var line in lines)
            {
                _builder.Append(indentText);
                _builder.AppendLine(line);
            }
        }

        public void Indent() => _indent++;

        public void Unindent() => _indent--;

        public override string ToString() => _builder.ToString();
    }
}
