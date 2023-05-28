using System.Runtime.InteropServices;

namespace GameOffsets.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct Prophecy
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(32)]
	public long PropheciesDatPtr;
}
