using System;
using ExileCore.Shared.Enums;

namespace ExileCore.Shared.SomeMagic;

[Obsolete("Report if you're using this, otherwise it's going to be deleted soon")]
public class MemoryProtection : IDisposable
{
	public static SafeMemoryHandle ProcessHandle { get; private set; }

	public static IntPtr Address { get; private set; }

	public static int Size { get; private set; }

	public static MemoryProtectionType OldProtection { get; private set; }

	public static MemoryProtectionType NewProtection { get; private set; }

	public MemoryProtection(SafeMemoryHandle processHandle, IntPtr address, int size, MemoryProtectionType protection = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
	{
		ProcessHandle = processHandle;
		Address = address;
		Size = size;
		NewProtection = protection;
		OldProtection = NativeMethods.ChangeMemoryProtection(ProcessHandle, Address, Size, NewProtection);
	}

	public void Dispose()
	{
		NativeMethods.ChangeMemoryProtection(ProcessHandle, Address, Size, OldProtection);
		GC.SuppressFinalize(this);
	}

	~MemoryProtection()
	{
		Dispose();
	}
}
