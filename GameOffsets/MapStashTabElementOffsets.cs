using System.Runtime.InteropServices;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct MapStashTabElementOffsets
{
	[FieldOffset(2720)]
	public int Base;
}
