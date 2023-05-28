using System.Runtime.InteropServices;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct NormalInventoryItemOffsets
{
	[FieldOffset(1080)]
	public long Item;

	[FieldOffset(1088)]
	public int InventPosX;

	[FieldOffset(1092)]
	public int InventPosY;

	[FieldOffset(1096)]
	public int Width;

	[FieldOffset(1100)]
	public int Height;
}
