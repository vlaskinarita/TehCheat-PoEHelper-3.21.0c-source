using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace ExileCore.Shared.PInvoke;

internal class DynamicImportException : Win32Exception
{
	protected DynamicImportException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public DynamicImportException()
	{
	}

	public DynamicImportException(int error)
		: base(error)
	{
	}

	public DynamicImportException(string message)
		: base(message + Environment.NewLine + "ErrorCode: " + Marshal.GetLastWin32Error())
	{
	}

	public DynamicImportException(int error, string message)
		: base(error, message)
	{
	}

	public DynamicImportException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
