using System.Numerics;
using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct IngameStateOffsets
{
	[FieldOffset(24)]
	public long Data;

	[FieldOffset(120)]
	public long WorldData;

	[FieldOffset(152)]
	public long EntityLabelMap;

	[FieldOffset(416)]
	public long UIRoot;

	[FieldOffset(472)]
	public long UIHoverElement;

	[FieldOffset(480)]
	public Vector2 CurentUIElementPos;

	[FieldOffset(488)]
	public long UIHover;

	[FieldOffset(528)]
	public Vector2i MouseGlobal;

	[FieldOffset(540)]
	public Vector2 UIHoverPos;

	[FieldOffset(548)]
	public Vector2 MouseInGame;

	[FieldOffset(1040)]
	public float TimeInGameF;

	[FieldOffset(1104)]
	public long IngameUi;
}
