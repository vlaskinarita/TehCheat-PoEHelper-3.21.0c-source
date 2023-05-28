using System;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace ExileCore.Shared.SomeMagic;

[SuppressUnmanagedCodeSecurity]
[Obsolete("Report if you're using this, otherwise it's going to be deleted soon")]
public sealed class SafeMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	public SafeMemoryHandle()
		: base(ownsHandle: true)
	{
	}

	public SafeMemoryHandle(IntPtr handle)
		: base(ownsHandle: true)
	{
		SetHandle(handle);
	}

	protected override bool ReleaseHandle()
	{
		if (handle != IntPtr.Zero)
		{
			return Imports.CloseHandle(handle);
		}
		return false;
	}
}
