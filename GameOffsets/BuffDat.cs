using System.Runtime.InteropServices;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct BuffDat
{
	[FieldOffset(0)]
	public long Name;

	[FieldOffset(8)]
	public long Description;

	[FieldOffset(16)]
	public byte IsInvisible;

	[FieldOffset(17)]
	public byte IsRemovable;

	[FieldOffset(18)]
	public long DisplayName;
}
