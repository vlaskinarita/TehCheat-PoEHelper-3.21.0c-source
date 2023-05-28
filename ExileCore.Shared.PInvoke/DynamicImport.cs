using System;
using System.Runtime.InteropServices;

namespace ExileCore.Shared.PInvoke;

[Obsolete("Report if you're using this, otherwise it's going to be deleted soon")]
internal static class DynamicImport
{
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetModuleHandleW", SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string modulename);

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string procname);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryW", SetLastError = true)]
	private static extern IntPtr LoadLibrary(string lpFileName);

	public static T Import<T>(IntPtr moduleHandle, string methodName)
	{
		return Marshal.GetDelegateForFunctionPointer<T>(ImportMethod(moduleHandle, methodName));
	}

	public static T Import<T>(string libraryName, string methodName)
	{
		return Marshal.GetDelegateForFunctionPointer<T>(ImportMethod(libraryName, methodName));
	}

	public static IntPtr ImportLibrary(string libraryName)
	{
		if (libraryName == string.Empty)
		{
			throw new ArgumentOutOfRangeException("libraryName");
		}
		IntPtr intPtr = GetModuleHandle(libraryName);
		if (intPtr == IntPtr.Zero)
		{
			intPtr = LoadLibrary(libraryName);
		}
		if (intPtr == IntPtr.Zero)
		{
			throw new DynamicImportException("DynamicImport failed to import library \"" + libraryName + "\"!");
		}
		return intPtr;
	}

	public static IntPtr ImportMethod(IntPtr moduleHandle, string methodName)
	{
		if (moduleHandle == IntPtr.Zero)
		{
			throw new ArgumentOutOfRangeException("moduleHandle");
		}
		if (string.IsNullOrEmpty(methodName))
		{
			throw new ArgumentOutOfRangeException("methodName");
		}
		IntPtr procAddress = GetProcAddress(moduleHandle, methodName);
		if (procAddress == IntPtr.Zero)
		{
			throw new DynamicImportException("DynamicImport failed to find method \"" + methodName + "\" in module \"0x" + moduleHandle.ToString("X") + "\"!");
		}
		return procAddress;
	}

	public static IntPtr ImportMethod(string libraryName, string methodName)
	{
		return ImportMethod(ImportLibrary(libraryName), methodName);
	}
}
