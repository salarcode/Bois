using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Salar.Bois.Generator;

[Generator]
public sealed class BoisSourceGenerator : ISourceGenerator
{
    private const string ReaderAttributeName = "Salar.Bois.Generator.Attributes.BoisReaderAttribute";
    private const string WriterAttributeName = "Salar.Bois.Generator.Attributes.BoisWriterAttribute";

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
            if (method.ReturnsVoid || method.Parameters.Length != 1 || !IsStream(method.Parameters[0].Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidMethodSignature, method.Locations.FirstOrDefault(), $"Reader method '{method.Name}' must have the signature 'static partial T Method(System.IO.Stream source)'."));
                return false;
            }

            generationMethod = new GenerationMethod(method, containingType, operation, method.ReturnType);
            return true;
        }

        if (!method.ReturnsVoid || method.Parameters.Length != 2 || !IsStream(method.Parameters[0].Type))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidMethodSignature, method.Locations.FirstOrDefault(), $"Writer method '{method.Name}' must have the signature 'static partial void Method(System.IO.Stream output, T model)'."));
            return false;
        }

        generationMethod = new GenerationMethod(method, containingType, operation, method.Parameters[1].Type);
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
            if (syntaxNode is MethodDeclarationSyntax method && method.AttributeLists.Count > 0 && method.Modifiers.Any(SyntaxKind.PartialKeyword))
                Candidates.Add(method);
        }
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
            private readonly Queue<LocalFunctionModel> _pending = new();
            private int _id;

            public MethodEmitter(ContainingTypeEmitter owner, GenerationMethod method)
            {
                _owner = owner;
                _method = method;
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
                    var sourceName = Escape(_method.Method.Parameters[0].Name);
                    var rootFunction = EnsureReadFunction(_method.RootType);
                    builder.Line($"var reader = new global::Salar.BinaryBuffers.Compatibility.StreamBufferReader({sourceName});");
                    builder.Line($"return {rootFunction}(reader);");
                }
                else
                {
                    var outputName = Escape(_method.Method.Parameters[0].Name);
                    var valueName = Escape(_method.Method.Parameters[1].Name);
                    var rootFunction = EnsureWriteFunction(_method.RootType);
                    builder.Line($"var writer = new global::Salar.BinaryBuffers.Compatibility.StreamBufferWriter({outputName});");
                    builder.Line($"{rootFunction}(writer, {valueName});");
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
                builder.Line($"throw new global::System.NotSupportedException({Literal(error)});");
                builder.Unindent();
                builder.Line("}");
            }

            private string EnsureReadFunction(ITypeSymbol type)
            {
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
                var key = _owner.TypeKey(type);
                if (_writeFunctions.TryGetValue(key, out var existing))
                    return existing.Name;

                var function = new LocalFunctionModel(OperationKind.Writer, type, $"WriteType{_id++}");
                _writeFunctions.Add(key, function);
                _pending.Enqueue(function);
                return function.Name;
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
                builder.Line($"static {TypeName(function.Type)} {function.Name}(global::Salar.BinaryBuffers.BufferReaderBase reader)");
                builder.Line("{");
                builder.Indent();

                if (!TryEmitRead(function.Type, builder, out var error))
                {
                    _owner.Report(_method.Method.Locations.FirstOrDefault(), error);
                    builder.Line($"throw new global::System.NotSupportedException({Literal(error)});");
                }

                builder.Unindent();
                builder.Line("}");
            }

            private void EmitWriteLocalFunction(CodeBuilder builder, LocalFunctionModel function)
            {
                builder.Line();
                builder.Line($"static void {function.Name}(global::Salar.BinaryBuffers.BufferWriterBase writer, {TypeName(function.Type)} value)");
                builder.Line("{");
                builder.Indent();

                if (!TryEmitWrite(function.Type, builder, out var error))
                {
                    _owner.Report(_method.Method.Locations.FirstOrDefault(), error);
                    builder.Line($"throw new global::System.NotSupportedException({Literal(error)});");
                }

                builder.Unindent();
                builder.Line("}");
            }

            private bool TryEmitWrite(ITypeSymbol type, CodeBuilder builder, out string error)
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
                    return EmitWriteArray(arrayType, builder, out error);

                if (_owner.TryGetDictionaryInfo(type, out var dict))
                    return EmitWriteDictionary(dict, builder, out error);

                if (_owner.TryGetCollectionInfo(type, out var coll))
                    return EmitWriteCollection(coll, builder, out error);

                if (_owner.IsNameValueCollection(type))
                    return EmitWriteNameValueCollection(builder, out error);

                return EmitWriteObject(type, builder, out error);
            }

            private bool TryEmitRead(ITypeSymbol type, CodeBuilder builder, out string error)
            {
                if (_owner.TryGetBasicType(type, out var basicType))
                {
                    builder.Line($"return {_owner.GetReadExpression(type, basicType)};");
                    error = string.Empty;
                    return true;
                }

                if (_owner.IsEnum(type))
                {
                    builder.Line($"return global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadEnumGeneric<{TypeName(type)}>(reader);");
                    error = string.Empty;
                    return true;
                }

                if (type is IArrayTypeSymbol arrayType)
                    return EmitReadArray(arrayType, builder, out error);

                if (_owner.TryGetDictionaryInfo(type, out var dict))
                    return EmitReadDictionary(type, dict, builder, out error);

                if (_owner.TryGetCollectionInfo(type, out var coll))
                    return EmitReadCollection(type, coll, builder, out error);

                if (_owner.IsNameValueCollection(type))
                    return EmitReadNameValueCollection(type, builder, out error);

                return EmitReadObject(type, builder, out error);
            }

            private bool EmitWriteObject(ITypeSymbol type, CodeBuilder builder, out string error)
            {
                if (!_owner.TryGetMembers(type, out var members, out error))
                    return false;

                if (!_owner.IsExplicitStruct(type))
                {
                    builder.Line("if (value is null)");
                    builder.Line("{");
                    builder.Indent();
                    builder.Line("global::Salar.Bois.Generator.Serializers.BoisPrimitiveWriters.WriteNullValue(writer);");
                    builder.Line("return;");
                    builder.Unindent();
                    builder.Line("}");
                    builder.Line($"global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.WriteUIntNullableMemberCount(writer, {members.Length}u);");
                }

                foreach (var member in members)
                {
                    if (!EmitWriteMember(member, builder, out error))
                        return false;
                }

                error = string.Empty;
                return true;
            }

            private bool EmitReadObject(ITypeSymbol type, CodeBuilder builder, out string error)
            {
                if (!_owner.TryGetMembers(type, out var members, out error))
                    return false;

                if (!_owner.IsExplicitStruct(type))
                {
                    builder.Line("var memberCount = global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                    builder.Line("if (memberCount is null)");
                    builder.Indent();
                    builder.Line("return null!;");
                    builder.Unindent();
                }

                if (!_owner.TryGetCreationExpression(type, out var creationExpression, out error))
                    return false;

                builder.Line($"var instance = {creationExpression};");
                foreach (var member in members)
                {
                    if (!EmitReadMember(member, builder, out error))
                        return false;
                }
                builder.Line("return instance;");
                return true;
            }

            private bool EmitWriteMember(MemberModel member, CodeBuilder builder, out string error)
            {
                var access = $"value.{Escape(member.Symbol.Name)}";
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

                builder.Line($"{EnsureWriteFunction(member.Type)}(writer, {access});");
                error = string.Empty;
                return true;
            }

            private bool EmitReadMember(MemberModel member, CodeBuilder builder, out string error)
            {
                var target = $"instance.{Escape(member.Symbol.Name)}";
                if (_owner.TryGetBasicType(member.Type, out var basicType))
                {
                    builder.Line($"{target} = {_owner.GetReadExpression(member.Type, basicType)};");
                    error = string.Empty;
                    return true;
                }
                if (_owner.IsEnum(member.Type))
                {
                    builder.Line($"{target} = global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadEnumGeneric<{TypeName(member.Type)}>(reader);");
                    error = string.Empty;
                    return true;
                }

                if (member.IsGetterOnlyMutableCollection)
                {
                    var localName = Escape(member.Symbol.Name) + "Target";
                    builder.Line($"var {localName} = {target} ?? throw new global::System.InvalidOperationException({Literal($"Property '{member.Symbol.Name}' returned null during deserialization.")});");
                    builder.Line($"{localName}.Clear();");
                    if (_owner.TryGetDictionaryInfo(member.Type, out var dict))
                    {
                        EmitReadIntoDictionary(dict, localName, builder);
                        error = string.Empty;
                        return true;
                    }
                    if (_owner.TryGetCollectionInfo(member.Type, out var coll))
                    {
                        EmitReadIntoCollection(coll, localName, builder);
                        error = string.Empty;
                        return true;
                    }
                    if (_owner.IsNameValueCollection(member.Type))
                    {
                        EmitReadIntoNameValue(localName, builder);
                        error = string.Empty;
                        return true;
                    }
                }

                builder.Line($"{target} = {EnsureReadFunction(member.Type)}(reader);");
                error = string.Empty;
                return true;
            }

            private bool EmitWriteArray(IArrayTypeSymbol arrayType, CodeBuilder builder, out string error)
                => EmitWriteNestedArray(arrayType, "value", builder, out error, root:true);

            private bool EmitWriteNestedArray(IArrayTypeSymbol arrayType, string expression, CodeBuilder builder, out string error, bool root=false)
            {
                builder.Line($"if ({expression} is null)");
                builder.Line("{");
                builder.Indent();
                builder.Line("global::Salar.Bois.Generator.Serializers.BoisPrimitiveWriters.WriteNullValue(writer);");
                builder.Unindent();
                builder.Line("}");
                builder.Line("else");
                builder.Line("{");
                builder.Indent();
                builder.Line($"global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.WriteUIntNullableMemberCount(writer, (uint){expression}.Length);");
                builder.Line($"foreach (var item in {expression})");
                builder.Line("{");
                builder.Indent();
                builder.Line(GetWriteElementStatement(arrayType.ElementType, "item"));
                builder.Unindent();
                builder.Line("}");
                builder.Unindent();
                builder.Line("}");
                error = string.Empty;
                return true;
            }

            private bool EmitReadArray(IArrayTypeSymbol arrayType, CodeBuilder builder, out string error)
            {
                builder.Line("var itemCount = global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line("if (itemCount is null)");
                builder.Indent();
                builder.Line("return null!;");
                builder.Unindent();
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

            private bool EmitWriteCollection(CollectionInfo collectionInfo, CodeBuilder builder, out string error)
                => EmitWriteNestedCollection(collectionInfo, "value", builder, out error, root:true);

            private bool EmitWriteNestedCollection(CollectionInfo collectionInfo, string expression, CodeBuilder builder, out string error, bool root=false)
            {
                builder.Line($"if ({expression} is null)");
                builder.Line("{");
                builder.Indent();
                builder.Line("global::Salar.Bois.Generator.Serializers.BoisPrimitiveWriters.WriteNullValue(writer);");
                builder.Unindent();
                builder.Line("}");
                builder.Line("else");
                builder.Line("{");
                builder.Indent();
                builder.Line($"global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.WriteUIntNullableMemberCount(writer, (uint){expression}.Count);");
                builder.Line($"foreach (var item in {expression})");
                builder.Line("{");
                builder.Indent();
                builder.Line(GetWriteElementStatement(collectionInfo.ElementType, "item"));
                builder.Unindent();
                builder.Line("}");
                builder.Unindent();
                builder.Line("}");
                error = string.Empty;
                return true;
            }

            private bool EmitReadCollection(ITypeSymbol type, CollectionInfo collectionInfo, CodeBuilder builder, out string error)
            {
                if (!_owner.TryGetCreationExpression(type, out var create, out error))
                    return false;
                builder.Line("var itemCount = global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line("if (itemCount is null)");
                builder.Indent();
                builder.Line("return null!;");
                builder.Unindent();
                builder.Line($"var items = {create};");
                EmitReadIntoCollectionBody(collectionInfo, "items", "itemCount.Value", builder);
                builder.Line("return items;");
                error = string.Empty;
                return true;
            }

            private void EmitReadIntoCollection(CollectionInfo collectionInfo, string target, CodeBuilder builder)
            {
                builder.Line("var itemCount = global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
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

            private bool EmitWriteDictionary(DictionaryInfo dictionaryInfo, CodeBuilder builder, out string error)
                => EmitWriteNestedDictionary(dictionaryInfo, "value", builder, out error, root:true);

            private bool EmitWriteNestedDictionary(DictionaryInfo dictionaryInfo, string expression, CodeBuilder builder, out string error, bool root=false)
            {
                builder.Line($"if ({expression} is null)");
                builder.Line("{");
                builder.Indent();
                builder.Line("global::Salar.Bois.Generator.Serializers.BoisPrimitiveWriters.WriteNullValue(writer);");
                builder.Unindent();
                builder.Line("}");
                builder.Line("else");
                builder.Line("{");
                builder.Indent();
                builder.Line($"global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.WriteUIntNullableMemberCount(writer, (uint){expression}.Count);");
                builder.Line($"foreach (var item in {expression})");
                builder.Line("{");
                builder.Indent();
                builder.Line(GetWriteElementStatement(dictionaryInfo.KeyType, "item.Key"));
                builder.Line(GetWriteElementStatement(dictionaryInfo.ValueType, "item.Value"));
                builder.Unindent();
                builder.Line("}");
                builder.Unindent();
                builder.Line("}");
                error = string.Empty;
                return true;
            }

            private bool EmitReadDictionary(ITypeSymbol type, DictionaryInfo dictionaryInfo, CodeBuilder builder, out string error)
            {
                if (!_owner.TryGetCreationExpression(type, out var create, out error))
                    return false;
                builder.Line("var itemCount = global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line("if (itemCount is null)");
                builder.Indent();
                builder.Line("return null!;");
                builder.Unindent();
                builder.Line($"var items = {create};");
                EmitReadIntoDictionaryBody(dictionaryInfo, "items", "itemCount.Value", builder);
                builder.Line("return items;");
                error = string.Empty;
                return true;
            }

            private void EmitReadIntoDictionary(DictionaryInfo dictionaryInfo, string target, CodeBuilder builder)
            {
                builder.Line("var itemCount = global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
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

            private bool EmitWriteNameValueCollection(CodeBuilder builder, out string error)
                => EmitWriteNestedNameValue("value", builder, out error, root:true);

            private bool EmitWriteNestedNameValue(string expression, CodeBuilder builder, out string error, bool root=false)
            {
                builder.Line($"if ({expression} is null)");
                builder.Line("{");
                builder.Indent();
                builder.Line("global::Salar.Bois.Generator.Serializers.BoisPrimitiveWriters.WriteNullValue(writer);");
                builder.Unindent();
                builder.Line("}");
                builder.Line("else");
                builder.Line("{");
                builder.Indent();
                builder.Line($"global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.WriteUIntNullableMemberCount(writer, (uint){expression}.Count);");
                builder.Line($"foreach (var key in {expression}.AllKeys)");
                builder.Line("{");
                builder.Indent();
                builder.Line("global::Salar.Bois.Generator.Serializers.BoisPrimitiveWriters.WriteValue(writer, key, global::System.Text.Encoding.UTF8);");
                builder.Line($"global::Salar.Bois.Generator.Serializers.BoisPrimitiveWriters.WriteValue(writer, {expression}[key], global::System.Text.Encoding.UTF8);");
                builder.Unindent();
                builder.Line("}");
                builder.Unindent();
                builder.Line("}");
                error = string.Empty;
                return true;
            }

            private bool EmitReadNameValueCollection(ITypeSymbol type, CodeBuilder builder, out string error)
            {
                if (!_owner.TryGetCreationExpression(type, out var create, out error))
                    return false;
                builder.Line("var itemCount = global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
                builder.Line("if (itemCount is null)");
                builder.Indent();
                builder.Line("return null!;");
                builder.Unindent();
                builder.Line($"var items = {create};");
                EmitReadIntoNameValueBody("items", "itemCount.Value", builder);
                builder.Line("return items;");
                error = string.Empty;
                return true;
            }

            private void EmitReadIntoNameValue(string target, CodeBuilder builder)
            {
                builder.Line("var itemCount = global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt32Nullable(reader);");
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
                builder.Line("var key = global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadString(reader, global::System.Text.Encoding.UTF8);");
                builder.Line("var value = global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadString(reader, global::System.Text.Encoding.UTF8);");
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
                return $"{EnsureWriteFunction(type)}(writer, {expression});";
            }

            private string GetReadValueExpression(ITypeSymbol type)
            {
                if (_owner.TryGetBasicType(type, out var basicType))
                    return _owner.GetReadExpression(type, basicType);
                if (_owner.IsEnum(type))
                    return $"global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadEnumGeneric<{TypeName(type)}>(reader)";
                return $"{EnsureReadFunction(type)}(reader)";
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
                    if (property.IsStatic || property.DeclaredAccessibility != Accessibility.Public || property.IsIndexer || property.GetMethod is null || property.GetMethod.DeclaredAccessibility != Accessibility.Public)
                        continue;

                    var getterOnlyMutable = property.SetMethod is null && (TryGetCollectionInfo(property.Type, out _) || TryGetDictionaryInfo(property.Type, out _) || IsNameValueCollection(property.Type));
                    if (property.SetMethod is not null && property.SetMethod.DeclaredAccessibility == Accessibility.Public || getterOnlyMutable)
                    {
                        Insert(ordered, new MemberModel(property, property.Type, index, getterOnlyMutable));
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

            if (bare.SpecialType == SpecialType.System_String) { basicType = BasicType.String; return true; }
            if (bare.SpecialType == SpecialType.System_Boolean) { basicType = nullableValue ? BasicType.BoolNullable : BasicType.Bool; return true; }
            if (bare.SpecialType == SpecialType.System_Char) { basicType = nullableValue ? BasicType.CharNullable : BasicType.Char; return true; }
            if (bare.SpecialType == SpecialType.System_Int16) { basicType = nullableValue ? BasicType.Int16Nullable : BasicType.Int16; return true; }
            if (bare.SpecialType == SpecialType.System_Int32) { basicType = nullableValue ? BasicType.Int32Nullable : BasicType.Int32; return true; }
            if (bare.SpecialType == SpecialType.System_Int64) { basicType = nullableValue ? BasicType.Int64Nullable : BasicType.Int64; return true; }
            if (bare.SpecialType == SpecialType.System_UInt16) { basicType = nullableValue ? BasicType.UInt16Nullable : BasicType.UInt16; return true; }
            if (bare.SpecialType == SpecialType.System_UInt32) { basicType = nullableValue ? BasicType.UInt32Nullable : BasicType.UInt32; return true; }
            if (bare.SpecialType == SpecialType.System_UInt64) { basicType = nullableValue ? BasicType.UInt64Nullable : BasicType.UInt64; return true; }
            if (bare.SpecialType == SpecialType.System_Single) { basicType = nullableValue ? BasicType.SingleNullable : BasicType.Single; return true; }
            if (bare.SpecialType == SpecialType.System_Double) { basicType = nullableValue ? BasicType.DoubleNullable : BasicType.Double; return true; }
            if (bare.SpecialType == SpecialType.System_Decimal) { basicType = nullableValue ? BasicType.DecimalNullable : BasicType.Decimal; return true; }
            if (bare.SpecialType == SpecialType.System_Byte) { basicType = nullableValue ? BasicType.ByteNullable : BasicType.Byte; return true; }
            if (bare.SpecialType == SpecialType.System_SByte) { basicType = nullableValue ? BasicType.SByteNullable : BasicType.SByte; return true; }
            if (bare.SpecialType == SpecialType.System_DateTime) { basicType = nullableValue ? BasicType.DateTimeNullable : BasicType.DateTime; return true; }
            if (Equal(bare, _dateTimeOffsetType)) { basicType = nullableValue ? BasicType.DateTimeOffsetNullable : BasicType.DateTimeOffset; return true; }
            if (Equal(bare, _dateOnlyType)) { basicType = nullableValue ? BasicType.DateOnlyNullable : BasicType.DateOnly; return true; }
            if (Equal(bare, _timeOnlyType)) { basicType = nullableValue ? BasicType.TimeOnlyNullable : BasicType.TimeOnly; return true; }
            if (bare is IArrayTypeSymbol arrayType && arrayType.ElementType.SpecialType == SpecialType.System_Byte) { basicType = BasicType.ByteArray; return true; }
            if (Equal(bare, _timeSpanType)) { basicType = nullableValue ? BasicType.TimeSpanNullable : BasicType.TimeSpan; return true; }
            if (Equal(bare, _guidType)) { basicType = nullableValue ? BasicType.GuidNullable : BasicType.Guid; return true; }
            if (Equal(bare, _colorType)) { basicType = nullableValue ? BasicType.ColorNullable : BasicType.Color; return true; }
            if (Equal(bare, _dbNullType)) { basicType = BasicType.DbNull; return true; }
            if (Equal(bare, _uriType)) { basicType = BasicType.Uri; return true; }
            if (Equal(bare, _versionType)) { basicType = BasicType.Version; return true; }
            if (Equal(bare, _dataTableType)) { basicType = BasicType.DataTable; return true; }
            if (Equal(bare, _dataSetType)) { basicType = BasicType.DataSet; return true; }

            basicType = default;
            return false;
        }

        public string GetWriteStatement(ITypeSymbol type, BasicType basicType, string expression) => basicType switch
        {
            BasicType.String => $"global::Salar.Bois.Generator.Serializers.BoisPrimitiveWriters.WriteValue(writer, {expression}, global::System.Text.Encoding.UTF8);",
            BasicType.Bool or BasicType.BoolNullable or BasicType.Char or BasicType.CharNullable or BasicType.DateTime or BasicType.DateTimeNullable or BasicType.DateTimeOffset or BasicType.DateTimeOffsetNullable or BasicType.TimeSpan or BasicType.TimeSpanNullable or BasicType.Guid or BasicType.GuidNullable or BasicType.Color or BasicType.ColorNullable or BasicType.DbNull or BasicType.Uri or BasicType.Version or BasicType.DateOnly or BasicType.DateOnlyNullable or BasicType.TimeOnly or BasicType.TimeOnlyNullable or BasicType.ByteArray => $"global::Salar.Bois.Generator.Serializers.BoisPrimitiveWriters.WriteValue(writer, {expression});",
            BasicType.Int16 or BasicType.Int16Nullable or BasicType.Int32 or BasicType.Int32Nullable or BasicType.Int64 or BasicType.Int64Nullable or BasicType.UInt16 or BasicType.UInt16Nullable or BasicType.UInt32 or BasicType.UInt32Nullable or BasicType.UInt64 or BasicType.UInt64Nullable or BasicType.ByteNullable or BasicType.SByteNullable => $"global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.WriteVarInt(writer, {expression});",
            BasicType.Single or BasicType.SingleNullable or BasicType.Double or BasicType.DoubleNullable or BasicType.Decimal or BasicType.DecimalNullable => $"global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.WriteVarDecimal(writer, {expression});",
            BasicType.Byte => $"global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.WriteByte(writer, {expression});",
            BasicType.SByte => $"global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.WriteSByte(writer, {expression});",
            BasicType.DataTable or BasicType.DataSet => $"global::Salar.Bois.Generator.Serializers.BoisPrimitiveWriters.WriteValue(writer, {expression}, global::System.Text.Encoding.UTF8);",
            _ => throw new InvalidOperationException()
        };

        public string GetEnumWriteStatement(ITypeSymbol type, string expression)
        {
            var enumExpression = IsNullableValueType(type)
                ? $"({expression}.HasValue ? (global::System.Enum)(object){expression}.Value : null)"
                : $"({expression} is null ? null : (global::System.Enum)(object){expression})";
            var isNullable = IsNullable(type) ? "true" : "false";
            return $"global::Salar.Bois.Generator.Serializers.BoisPrimitiveWriters.WriteValue(writer, {enumExpression}, typeof({Bare(type).ToDisplayString(QualifiedTypeFormat)}), {isNullable});";
        }

        public string GetReadExpression(ITypeSymbol type, BasicType basicType) => basicType switch
        {
            BasicType.String => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadString(reader, global::System.Text.Encoding.UTF8)",
            BasicType.Bool => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadBoolean(reader)",
            BasicType.BoolNullable => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadBooleanNullable(reader)",
            BasicType.Char => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadChar(reader)",
            BasicType.CharNullable => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadCharNullable(reader)",
            BasicType.Int16 => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarInt16(reader)",
            BasicType.Int16Nullable => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarInt16Nullable(reader)",
            BasicType.Int32 => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarInt32(reader)",
            BasicType.Int32Nullable => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarInt32Nullable(reader)",
            BasicType.Int64 => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarInt64(reader)",
            BasicType.Int64Nullable => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarInt64Nullable(reader)",
            BasicType.UInt16 => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt16(reader)",
            BasicType.UInt16Nullable => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt16Nullable(reader)",
            BasicType.UInt32 => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt32(reader)",
            BasicType.UInt32Nullable => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt32Nullable(reader)",
            BasicType.UInt64 => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt64(reader)",
            BasicType.UInt64Nullable => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarUInt64Nullable(reader)",
            BasicType.Single => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarSingle(reader)",
            BasicType.SingleNullable => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarSingleNullable(reader)",
            BasicType.Double => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarDouble(reader)",
            BasicType.DoubleNullable => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarDoubleNullable(reader)",
            BasicType.Decimal => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarDecimal(reader)",
            BasicType.DecimalNullable => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarDecimalNullable(reader)",
            BasicType.Byte => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadByte(reader)",
            BasicType.ByteNullable => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarByteNullable(reader)",
            BasicType.SByte => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadSByte(reader)",
            BasicType.SByteNullable => "global::Salar.Bois.Generator.Serializers.BoisNumericSerializers.ReadVarSByteNullable(reader)",
            BasicType.DateTime => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadDateTime(reader)",
            BasicType.DateTimeNullable => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadDateTimeNullable(reader)",
            BasicType.DateTimeOffset => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadDateTimeOffset(reader)",
            BasicType.DateTimeOffsetNullable => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadDateTimeOffsetNullable(reader)",
            BasicType.DateOnly => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadDateOnly(reader)",
            BasicType.DateOnlyNullable => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadDateOnlyNullable(reader)",
            BasicType.TimeOnly => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadTimeOnly(reader)",
            BasicType.TimeOnlyNullable => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadTimeOnlyNullable(reader)",
            BasicType.ByteArray => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadByteArray(reader)",
            BasicType.TimeSpan => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadTimeSpan(reader)",
            BasicType.TimeSpanNullable => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadTimeSpanNullable(reader)",
            BasicType.Guid => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadGuid(reader)",
            BasicType.GuidNullable => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadGuidNullable(reader)",
            BasicType.Color => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadColor(reader)",
            BasicType.ColorNullable => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadColorNullable(reader)",
            BasicType.DbNull => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadDbNull(reader)",
            BasicType.Uri => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadUri(reader)",
            BasicType.Version => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadVersion(reader)",
            BasicType.DataTable => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadDataTable(reader, global::System.Text.Encoding.UTF8)",
            BasicType.DataSet => "global::Salar.Bois.Generator.Serializers.BoisPrimitiveReaders.ReadDataSet(reader, global::System.Text.Encoding.UTF8)",
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

        public bool IsNameValueCollection(ITypeSymbol type) => Equal(Bare(type), _nameValueCollectionType);
        public bool IsEnum(ITypeSymbol type) => Bare(type).TypeKind == TypeKind.Enum;
        public bool IsExplicitStruct(ITypeSymbol type) => Bare(type).IsValueType && Bare(type).TypeKind != TypeKind.Enum && Bare(type).SpecialType == SpecialType.None;
        public bool IsNullable(ITypeSymbol type) => !Bare(type).IsValueType || IsNullableValueType(type);
        private bool IsNullableValueType(ITypeSymbol type) => type is INamedTypeSymbol named && Equal(named.OriginalDefinition, _nullableType);
        private bool IsNullableComplex(ITypeSymbol type) => IsNullableValueType(type) && !TryGetBasicType(type, out _) && !IsEnum(type);
        private ITypeSymbol Bare(ITypeSymbol type) => type is INamedTypeSymbol named && named.IsGenericType && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T ? named.TypeArguments[0] : type;
        private bool Equal(ITypeSymbol? left, ITypeSymbol? right) => left is not null && right is not null && SymbolEqualityComparer.Default.Equals(left, right);
        private static string Literal(string value) => SymbolDisplay.FormatLiteral(value, true);
    }

    private sealed class GroupComparer : IEqualityComparer<(INamedTypeSymbol ContainingType, string FileName)>
    {
        public bool Equals((INamedTypeSymbol ContainingType, string FileName) x, (INamedTypeSymbol ContainingType, string FileName) y)
            => SymbolEqualityComparer.Default.Equals(x.ContainingType, y.ContainingType) && x.FileName == y.FileName;

        public int GetHashCode((INamedTypeSymbol ContainingType, string FileName) obj)
            => HashCode.Combine(SymbolEqualityComparer.Default.GetHashCode(obj.ContainingType), obj.FileName);
    }

    private sealed record GenerationMethod(IMethodSymbol Method, INamedTypeSymbol ContainingType, OperationKind Operation, ITypeSymbol RootType);
    private sealed record LocalFunctionModel(OperationKind Operation, ITypeSymbol Type, string Name);
    private sealed record MemberModel(ISymbol Symbol, ITypeSymbol Type, int Index, bool IsGetterOnlyMutableCollection);
    private sealed record CollectionInfo(ITypeSymbol Type, ITypeSymbol ElementType);
    private sealed record DictionaryInfo(ITypeSymbol Type, ITypeSymbol KeyType, ITypeSymbol ValueType);

    private enum OperationKind { Reader, Writer }
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

        public void Indent() => _indent++;
        public void Unindent() => _indent--;
        public override string ToString() => _builder.ToString();
    }
}
