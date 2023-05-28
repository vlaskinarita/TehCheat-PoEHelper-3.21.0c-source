using System.Runtime.InteropServices;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct Example
{
	[FieldOffset(0)]
	public int SomeField;
}
