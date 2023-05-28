using System.Runtime.InteropServices;

namespace GameOffsets.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct SampleComponentOffset
{
	[FieldOffset(0)]
	public ComponentHeader Header;
}
