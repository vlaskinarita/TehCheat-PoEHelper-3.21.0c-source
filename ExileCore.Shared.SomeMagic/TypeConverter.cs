using System;
using System.Runtime.InteropServices;

namespace ExileCore.Shared.SomeMagic;

[Obsolete("Report if you're using this, otherwise it's going to be deleted soon")]
public static class TypeConverter
{
	public unsafe static T PointerToGenericType<T>(IntPtr pointer) where T : struct
	{
		switch (MarshalType<T>.TypeCode)
		{
		case TypeCode.Object:
			if (MarshalType<T>.IsIntPtr)
			{
				return (T)(object)(*(IntPtr*)(void*)pointer);
			}
			break;
		case TypeCode.Boolean:
			return (T)(object)(*(bool*)(void*)pointer);
		case TypeCode.SByte:
			return (T)(object)(*(sbyte*)(void*)pointer);
		case TypeCode.Byte:
			return (T)(object)(*(byte*)(void*)pointer);
		case TypeCode.Int16:
			return (T)(object)(*(short*)(void*)pointer);
		case TypeCode.UInt16:
			return (T)(object)(*(ushort*)(void*)pointer);
		case TypeCode.Int32:
			return (T)(object)(*(int*)(void*)pointer);
		case TypeCode.UInt32:
			return (T)(object)(*(uint*)(void*)pointer);
		case TypeCode.Int64:
			return (T)(object)(*(long*)(void*)pointer);
		case TypeCode.UInt64:
			return (T)(object)(*(ulong*)(void*)pointer);
		case TypeCode.Single:
			return (T)(object)(*(float*)(void*)pointer);
		case TypeCode.Double:
			return (T)(object)(*(double*)(void*)pointer);
		}
		if (!MarshalType<T>.HasUnmanagedTypes)
		{
			T generic = default(T);
			NativeMethods.Copy(MarshalType<T>.GetPointer(ref generic), pointer.ToPointer(), MarshalType<T>.Size);
			return generic;
		}
		return (T)Marshal.PtrToStructure(pointer, typeof(T));
	}

	public unsafe static byte[] GenericTypeToBytes<T>(T generic) where T : struct
	{
		int size = MarshalType<T>.Size;
		switch (MarshalType<T>.TypeCode)
		{
		case TypeCode.Object:
			if (MarshalType<T>.IsIntPtr)
			{
				switch (size)
				{
				case 4:
					return BitConverter.GetBytes(((IntPtr)(object)generic).ToInt32());
				case 8:
					return BitConverter.GetBytes(((IntPtr)(object)generic).ToInt64());
				}
			}
			break;
		case TypeCode.Boolean:
			return BitConverter.GetBytes((bool)(object)generic);
		case TypeCode.SByte:
			return BitConverter.GetBytes((sbyte)(object)generic);
		case TypeCode.Byte:
			return BitConverter.GetBytes((byte)(object)generic);
		case TypeCode.Int16:
			return BitConverter.GetBytes((short)(object)generic);
		case TypeCode.UInt16:
			return BitConverter.GetBytes((ushort)(object)generic);
		case TypeCode.Int32:
			return BitConverter.GetBytes((int)(object)generic);
		case TypeCode.UInt32:
			return BitConverter.GetBytes((uint)(object)generic);
		case TypeCode.Int64:
			return BitConverter.GetBytes((long)(object)generic);
		case TypeCode.UInt64:
			return BitConverter.GetBytes((ulong)(object)generic);
		case TypeCode.Single:
			return BitConverter.GetBytes((float)(object)generic);
		case TypeCode.Double:
			return BitConverter.GetBytes((double)(object)generic);
		}
		byte[] array = new byte[size];
		if (!MarshalType<T>.HasUnmanagedTypes)
		{
			void* source = MarshalType<T>.GetPointer(ref generic);
			fixed (byte* destination = array)
			{
				NativeMethods.Copy(destination, source, size);
				return array;
			}
		}
		using LocalAllocation localAllocation = new LocalAllocation(size);
		localAllocation.Write(generic);
		return localAllocation.Read();
	}

	public unsafe static T BytesToGenericType<T>(byte[] bytes) where T : struct
	{
		int size = MarshalType<T>.Size;
		switch (MarshalType<T>.TypeCode)
		{
		case TypeCode.Object:
			if (MarshalType<T>.IsIntPtr)
			{
				switch (bytes.Length)
				{
				case 1:
					return (T)(object)new IntPtr(BitConverter.ToInt32(new byte[4]
					{
						bytes[0],
						0,
						0,
						0
					}, 0));
				case 2:
					return (T)(object)new IntPtr(BitConverter.ToInt32(new byte[4]
					{
						bytes[0],
						bytes[1],
						0,
						0
					}, 0));
				case 4:
					return (T)(object)new IntPtr(BitConverter.ToInt32(bytes, 0));
				case 8:
					return (T)(object)new IntPtr(BitConverter.ToInt64(bytes, 0));
				}
			}
			break;
		case TypeCode.Boolean:
			return (T)(object)BitConverter.ToBoolean(bytes, 0);
		case TypeCode.SByte:
		case TypeCode.Byte:
			return (T)(object)bytes[0];
		case TypeCode.Int16:
			return (T)(object)BitConverter.ToInt16(bytes, 0);
		case TypeCode.UInt16:
			return (T)(object)BitConverter.ToUInt16(bytes, 0);
		case TypeCode.Int32:
			return (T)(object)BitConverter.ToInt32(bytes, 0);
		case TypeCode.UInt32:
			return (T)(object)BitConverter.ToUInt32(bytes, 0);
		case TypeCode.Int64:
			return (T)(object)BitConverter.ToInt64(bytes, 0);
		case TypeCode.UInt64:
			return (T)(object)BitConverter.ToUInt64(bytes, 0);
		case TypeCode.Single:
			return (T)(object)BitConverter.ToSingle(bytes, 0);
		case TypeCode.Double:
			return (T)(object)BitConverter.ToDouble(bytes, 0);
		}
		T generic = default(T);
		if (!MarshalType<T>.HasUnmanagedTypes)
		{
			void* destination = MarshalType<T>.GetPointer(ref generic);
			fixed (byte* source = bytes)
			{
				NativeMethods.Copy(destination, source, size);
				return generic;
			}
		}
		using LocalAllocation localAllocation = new LocalAllocation(size);
		localAllocation.Write(bytes);
		return localAllocation.Read<T>();
	}
}
