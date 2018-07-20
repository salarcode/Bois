using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Salar.Bois.Serializers;
// ReSharper disable AssignNullToNotNullAttribute

namespace Salar.Bois.Types
{
	class BoisTypeCompiler
	{
		public void Compile(Type type)
		{

		}

		delegate double DividerDelegate(int a, int b);
		public static void CompileType(Type type, bool writer, bool reader)
		{

			//تعریف امضای متد
			var myMethod = new DynamicMethod(
				name: "Compile_" + type.Name,
				returnType: null,
				parameterTypes: new[] { type, typeof(BinaryWriter) },
				m: typeof(BoisTypeCompiler).Module);



			//تعریف بدنه متد
			var il = myMethod.GetILGenerator();
			il.Emit(opcode: OpCodes.Ldarg_0); //بارگذاری پارامتر اول بر روی پشته ارزیابی
			il.Emit(opcode: OpCodes.Ldarg_1); //بارگذاری پارامتر دوم بر روی پشته ارزیابی
			il.Emit(opcode: OpCodes.Div); // دو پارامتر از پشته ارزیابی دریافت و تقسیم خواهند شد
			il.Emit(opcode: OpCodes.Ret); // دریافت نتیجه نهایی از پشته ارزیابی و بازگشت آن




			//فراخوانی متد پویا
			//روش اول
			var result = myMethod.Invoke(obj: null, parameters: new object[] { 10, 2 });
			Console.WriteLine(result);

			//روش دوم
			var method = (DividerDelegate)myMethod.CreateDelegate(delegateType: typeof(DividerDelegate));
			Console.WriteLine(method(10, 2));
		}

		public static void GenerateWriter(ILGenerator writer)
		{

		}

		public static void WriteInt32(PropertyInfo memberInfo, short memberIndex, ILGenerator writer)
		{
			var getter = memberInfo.GetGetMethod(true);

			writer.Emit(OpCodes.Ldarg_0); // BinaryWriter
			writer.Emit(OpCodes.Ldloc, arg: memberIndex);
			writer.Emit(OpCodes.Callvirt, meth: getter);
			writer.Emit(OpCodes.Call, meth: typeof(PrimitivesSerializer).GetMethod(nameof(PrimitivesSerializer.WriteVarInt)));
			writer.Emit(OpCodes.Nop);
		}

		public static void WriteInt32(FieldInfo memberInfo, short memberIndex, ILGenerator writer)
		{
			writer.Emit(OpCodes.Ldarg_0); // BinaryWriter
			writer.Emit(OpCodes.Ldloc, arg: memberIndex);
			writer.Emit(OpCodes.Ldfld, field: memberInfo);
			writer.Emit(OpCodes.Call, meth: typeof(PrimitivesSerializer).GetMethod(nameof(PrimitivesSerializer.WriteVarInt)));
			writer.Emit(OpCodes.Nop);
		}
	}
}
