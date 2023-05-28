using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using ExileCore.Shared.Enums;

namespace ExileCore.Shared.SomeMagic;

[Obsolete("Report if you're using this, otherwise it's going to be deleted soon")]
public static class NativeMethods
{
	public static bool LogError;

	public static SafeMemoryHandle OpenProcess(int pId, ProcessAccessRights accessRights = ProcessAccessRights.PROCESS_ALL_ACCESS)
	{
		SafeMemoryHandle safeMemoryHandle = Imports.OpenProcess(accessRights, bInheritHandle: false, pId);
		if (safeMemoryHandle == null || safeMemoryHandle.IsInvalid || safeMemoryHandle.IsClosed)
		{
			throw new Win32Exception($"[Error Code: {Marshal.GetLastWin32Error()}] Unable to open process {pId} with access {accessRights:X}");
		}
		return safeMemoryHandle;
	}

	public static int GetProcessId(SafeMemoryHandle processHandle)
	{
		int processId = Imports.GetProcessId(processHandle);
		if (processId == 0)
		{
			throw new Win32Exception($"[Error Code: {Marshal.GetLastWin32Error()}] Unable to get Id from process handle 0x{processHandle.DangerousGetHandle().ToString("X")}");
		}
		return processId;
	}

	public static bool Is64BitProcess(SafeMemoryHandle processHandle)
	{
		if (!Imports.IsWow64Process(processHandle, out var wow64Process))
		{
			throw new Win32Exception($"[Error Code: {Marshal.GetLastWin32Error()}] Unable to determine if process handle 0x{processHandle.DangerousGetHandle().ToString("X")} is 64 bit");
		}
		return !wow64Process;
	}

	public static string GetClassName(IntPtr windowHandle)
	{
		StringBuilder stringBuilder = new StringBuilder(65535);
		if (Imports.GetClassName(windowHandle, stringBuilder, stringBuilder.Capacity) == 0)
		{
			throw new Win32Exception($"[Error Code: {Marshal.GetLastWin32Error()}] Unable to get class name from window handle 0x{windowHandle.ToString("X")}");
		}
		return stringBuilder.ToString();
	}

	public static bool CloseHandle(IntPtr handle)
	{
		if (!Imports.CloseHandle(handle))
		{
			throw new Win32Exception($"[Error Code: {Marshal.GetLastWin32Error()}] Unable to close handle 0x{handle.ToString("X")}");
		}
		return true;
	}

	public static int ReadProcessMemory(SafeMemoryHandle processHandle, IntPtr address, [Out] byte[] buffer, int size)
	{
		if (!Imports.ReadProcessMemory(processHandle, address, buffer, size, out var lpBytesRead) && LogError)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(52, 3, stringBuilder2);
			handler.AppendLiteral("[Error Code: ");
			handler.AppendFormatted(Marshal.GetLastWin32Error());
			handler.AppendLiteral("] Unable to read memory from 0x");
			handler.AppendFormatted(address.ToString($"X{IntPtr.Size}"));
			handler.AppendLiteral("[Size: ");
			handler.AppendFormatted(size);
			handler.AppendLiteral("]");
			stringBuilder2.AppendLine(ref handler);
			StackFrame[] frames = new StackTrace(fNeedFileInfo: true).GetFrames();
			if (frames != null)
			{
				for (int i = 1; i < Math.Min(frames.Length, 10); i++)
				{
					StackFrame value = frames[i];
					stringBuilder.Append(value);
				}
			}
			Core.Logger?.Error(stringBuilder.ToString());
		}
		return lpBytesRead;
	}

	public static int WriteProcessMemory(SafeMemoryHandle processHandle, IntPtr address, [Out] byte[] buffer, int size)
	{
		int iBytesWritten = 0;
		if (!Imports.WriteProcessMemory(processHandle, address, buffer, size, out iBytesWritten) && LogError)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(51, 3, stringBuilder2);
			handler.AppendLiteral("[Error Code: ");
			handler.AppendFormatted(Marshal.GetLastWin32Error());
			handler.AppendLiteral("] Unable to write memory at 0x");
			handler.AppendFormatted(address.ToString($"X{IntPtr.Size}"));
			handler.AppendLiteral("[Size: ");
			handler.AppendFormatted(size);
			handler.AppendLiteral("]");
			stringBuilder3.AppendLine(ref handler);
			StackFrame[] frames = new StackTrace(fNeedFileInfo: true).GetFrames();
			for (int i = 1; i < Math.Min(frames.Length, 10); i++)
			{
				StackFrame stackFrame = frames[i];
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(12, 3, stringBuilder2);
				handler.AppendFormatted(stackFrame.GetFileName());
				handler.AppendLiteral(" -> ");
				handler.AppendFormatted(stackFrame.GetMethod().Name);
				handler.AppendLiteral(", line: ");
				handler.AppendFormatted(stackFrame.GetFileLineNumber());
				stringBuilder4.AppendLine(ref handler);
			}
			Core.Logger?.Error(stringBuilder.ToString());
		}
		return iBytesWritten;
	}

	public static IntPtr Allocate([Optional] IntPtr address, int size, MemoryProtectionType protect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
	{
		IntPtr result = Imports.VirtualAlloc(address, size, MemoryAllocationState.MEM_COMMIT, protect);
		if (result.Equals(0))
		{
			throw new Win32Exception($"[Error Code: {Marshal.GetLastWin32Error()}] Unable to allocate memory at 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
		}
		return result;
	}

	public static IntPtr Allocate(SafeMemoryHandle processHandle, [Optional] IntPtr address, int size, MemoryProtectionType protect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
	{
		IntPtr result = Imports.VirtualAllocEx(processHandle, address, size, MemoryAllocationState.MEM_COMMIT, protect);
		if (result.Equals(0))
		{
			throw new Win32Exception(string.Format("[Error Code: {0}] Unable to allocate memory to process handle 0x{1} at 0x{2}[Size: {3}]", Marshal.GetLastWin32Error(), processHandle.DangerousGetHandle().ToString("X"), address.ToString($"X{IntPtr.Size}"), size));
		}
		return result;
	}

	public static bool Free(IntPtr address, int size = 0, MemoryFreeType free = MemoryFreeType.MEM_RELEASE)
	{
		if (!Imports.VirtualFree(address, size, free))
		{
			throw new Win32Exception($"[Error Code: {Marshal.GetLastWin32Error()}] Unable to free memory at 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
		}
		return true;
	}

	public static bool Free(SafeMemoryHandle processHandle, IntPtr address, int size = 0, MemoryFreeType free = MemoryFreeType.MEM_RELEASE)
	{
		if (!Imports.VirtualFreeEx(processHandle, address, size, free))
		{
			throw new Win32Exception(string.Format("[Error Code: {0}] Unable to free memory from process handle 0x{1} at 0x{2}[Size: {3}]", Marshal.GetLastWin32Error(), processHandle.DangerousGetHandle().ToString("X"), address.ToString($"X{IntPtr.Size}"), size));
		}
		return true;
	}

	public unsafe static void Copy(void* destination, void* source, int size)
	{
		try
		{
			Imports.MoveMemory(destination, source, size);
		}
		catch
		{
			throw new Win32Exception(string.Format("[Error Code: {0}] Unable to copy memory to {0} from {1}[Size: {2}]", Marshal.GetLastWin32Error(), ((ulong*)destination)->ToString($"X{IntPtr.Size}"), ((ulong*)source)->ToString($"X{IntPtr.Size} ({size})")));
		}
	}

	public static MemoryProtectionType ChangeMemoryProtection(IntPtr address, int size, MemoryProtectionType newProtect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
	{
		if (!Imports.VirtualProtect(address, size, newProtect, out var lpflOldProtect))
		{
			throw new Win32Exception($"[Error Code: {Marshal.GetLastWin32Error()}] Unable to change memory protection at 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}] to {newProtect.ToString("X")}");
		}
		return lpflOldProtect;
	}

	public static MemoryProtectionType ChangeMemoryProtection(SafeMemoryHandle processHandle, IntPtr address, int size, MemoryProtectionType newProtect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
	{
		if (!Imports.VirtualProtectEx(processHandle, address, size, newProtect, out var lpflOldProtect))
		{
			throw new Win32Exception($"[Error Code: {Marshal.GetLastWin32Error()}] Unable to change memory protection of process handle 0x{processHandle.DangerousGetHandle().ToString("X")} at 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}] to {newProtect.ToString("X")}");
		}
		return lpflOldProtect;
	}

	public static MemoryBasicInformation Query(IntPtr address, int size)
	{
		if (Imports.VirtualQuery(address, out var lpBuffer, size) == 0)
		{
			throw new Win32Exception($"[Error Code: {Marshal.GetLastWin32Error()}] Unable to retrieve memory information from 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
		}
		return lpBuffer;
	}

	public static MemoryBasicInformation Query(SafeMemoryHandle processHandle, IntPtr address, int size)
	{
		if (Imports.VirtualQueryEx(processHandle, address, out var lpBuffer, size) == 0)
		{
			throw new Win32Exception($"[Error Code: {Marshal.GetLastWin32Error()}] Unable to retrieve memory information of process handle 0x{processHandle.DangerousGetHandle().ToString("X")} from 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
		}
		return lpBuffer;
	}
}
