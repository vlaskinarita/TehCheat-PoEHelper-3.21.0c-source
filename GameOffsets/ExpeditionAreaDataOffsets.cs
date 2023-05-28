using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ExpeditionAreaDataOffsets
{
	[FieldOffset(32)]
	public NativePtrArray ModsData;
}
