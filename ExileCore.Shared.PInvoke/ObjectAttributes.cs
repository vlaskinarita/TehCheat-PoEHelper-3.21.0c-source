using System;

namespace ExileCore.Shared.PInvoke;

internal struct ObjectAttributes
{
	public int Length;

	public IntPtr RootDirectory;

	public IntPtr ObjectName;

	public int Attributes;

	public IntPtr SecurityDescriptor;

	public IntPtr SecurityQualityOfService;
}
