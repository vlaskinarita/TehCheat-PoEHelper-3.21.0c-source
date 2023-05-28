using System;
using System.Text;

namespace ExileCore.Shared.SomeMagic;

[Obsolete("Report if you're using this, otherwise it's going to be deleted soon")]
public class MemoryLiterate
{
	private readonly SafeMemoryHandle _safeMemoryHandle;

	public MemoryLiterate(SafeMemoryHandle safeMemoryHandle)
	{
		_safeMemoryHandle = safeMemoryHandle;
	}

	public byte[] Read(IntPtr address, int size)
	{
		byte[] array = new byte[size];
		NativeMethods.ReadProcessMemory(_safeMemoryHandle, address, array, size);
		return array;
	}

	public byte[] Read(Pointer pointer, int size)
	{
		byte[] array;
		if (pointer.Offsets.Count == 0)
		{
			array = new byte[size];
			NativeMethods.ReadProcessMemory(_safeMemoryHandle, pointer.BaseAddress, array, size);
			return array;
		}
		int size2 = MarshalType<IntPtr>.Size;
		array = new byte[size2];
		NativeMethods.ReadProcessMemory(_safeMemoryHandle, pointer.BaseAddress, array, size2);
		IntPtr intPtr = TypeConverter.BytesToGenericType<IntPtr>(array);
		int num = pointer.Offsets.Count - 1;
		for (int i = 0; i < num; i++)
		{
			NativeMethods.ReadProcessMemory(_safeMemoryHandle, intPtr + pointer.Offsets[i], array, size2);
			intPtr = TypeConverter.BytesToGenericType<IntPtr>(array);
		}
		array = new byte[size];
		NativeMethods.ReadProcessMemory(_safeMemoryHandle, intPtr + pointer.Offsets[num], array, size);
		return array;
	}

	public T Read<T>(IntPtr address) where T : struct
	{
		return TypeConverter.BytesToGenericType<T>(Read(address, MarshalType<T>.Size));
	}

	public T Read<T>(Pointer pointer) where T : struct
	{
		return TypeConverter.BytesToGenericType<T>(Read(pointer, MarshalType<T>.Size));
	}

	public T[] Read<T>(IntPtr address, int count) where T : struct
	{
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = Read<T>(address + i * MarshalType<T>.Size);
		}
		return array;
	}

	public string Read(IntPtr address, int size, Encoding encoding)
	{
		byte[] bytes = Read(address, size);
		string text = encoding.GetString(bytes);
		int num = text.IndexOf('\0');
		if (num != -1)
		{
			text = text.Remove(num);
		}
		return text;
	}

	public string Read(Pointer pointer, int size, Encoding encoding)
	{
		byte[] bytes = Read(pointer, size);
		string text = encoding.GetString(bytes);
		int num = text.IndexOf('\0');
		if (num != -1)
		{
			text = text.Remove(num);
		}
		return text;
	}

	public bool Write(IntPtr address, byte[] bytes)
	{
		using (new MemoryProtection(_safeMemoryHandle, address, bytes.Length))
		{
			return NativeMethods.WriteProcessMemory(_safeMemoryHandle, address, bytes, bytes.Length) == bytes.Length;
		}
	}

	public bool Write(Pointer pointer, byte[] bytes)
	{
		if (pointer.Offsets.Count == 0)
		{
			using (new MemoryProtection(_safeMemoryHandle, pointer.BaseAddress, bytes.Length))
			{
				return NativeMethods.WriteProcessMemory(_safeMemoryHandle, pointer.BaseAddress, bytes, bytes.Length) == bytes.Length;
			}
		}
		int size = MarshalType<IntPtr>.Size;
		IntPtr intPtr = TypeConverter.BytesToGenericType<IntPtr>(Read(pointer.BaseAddress, size));
		int num = pointer.Offsets.Count - 1;
		for (int i = 0; i < num; i++)
		{
			intPtr = TypeConverter.BytesToGenericType<IntPtr>(Read(intPtr + pointer.Offsets[i], size));
		}
		intPtr += pointer.Offsets[num];
		using (new MemoryProtection(_safeMemoryHandle, intPtr, bytes.Length))
		{
			return NativeMethods.WriteProcessMemory(_safeMemoryHandle, intPtr, bytes, bytes.Length) == bytes.Length;
		}
	}

	public bool Write<T>(IntPtr address, T value) where T : struct
	{
		return Write(address, TypeConverter.GenericTypeToBytes(value));
	}

	public bool Write<T>(Pointer pointer, T value) where T : struct
	{
		return Write(pointer, TypeConverter.GenericTypeToBytes(value));
	}

	public bool Write(IntPtr address, string value, Encoding encoding)
	{
		if (value[value.Length - 1] != 0)
		{
			value += "\0";
		}
		byte[] bytes = encoding.GetBytes(value);
		return Write(address, bytes);
	}

	public bool Write(Pointer pointer, string value, Encoding encoding)
	{
		if (value[value.Length - 1] != 0)
		{
			value += "\0";
		}
		byte[] bytes = encoding.GetBytes(value);
		return Write(pointer, bytes);
	}
}
