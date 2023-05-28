using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ExileCore.Shared.SomeMagic;

public static class MarshalType<T> where T : struct
{
	internal unsafe delegate void* GetPointerDelegate(ref T generic);

	internal static readonly GetPointerDelegate GetPointer;

	public static Type Type { get; }

	public static TypeCode TypeCode { get; }

	public static int Size { get; }

	public static bool IsIntPtr { get; }

	public static bool HasUnmanagedTypes { get; }

	unsafe static MarshalType()
	{
		TypeCode = Type.GetTypeCode(typeof(T));
		if (typeof(T) == typeof(bool))
		{
			Size = 1;
			Type = typeof(T);
		}
		else if (typeof(T).IsEnum)
		{
			Type enumUnderlyingType = typeof(T).GetEnumUnderlyingType();
			Size = Marshal.SizeOf(enumUnderlyingType);
			Type = enumUnderlyingType;
			TypeCode = Type.GetTypeCode(enumUnderlyingType);
		}
		else
		{
			Size = Marshal.SizeOf(typeof(T));
			Type = typeof(T);
		}
		IsIntPtr = Type == typeof(IntPtr);
		HasUnmanagedTypes = Type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any((FieldInfo m) => m.GetCustomAttributes(typeof(MarshalAsAttribute), inherit: true).Any());
		DynamicMethod dynamicMethod = new DynamicMethod("GetPinnedPointer<" + Type.FullName.Replace(".", "<>") + ">", typeof(void*), new Type[1] { Type.MakeByRefType() }, typeof(MarshalType<>).Module);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Conv_U);
		iLGenerator.Emit(OpCodes.Ret);
		GetPointer = (GetPointerDelegate)dynamicMethod.CreateDelegate(typeof(GetPointerDelegate));
	}
}
