using System;

namespace ExileCore.Shared.PInvoke;

internal struct ClientID
{
	public IntPtr UniqueProcess;

	public IntPtr UniqueThread;
}
