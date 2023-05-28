using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct PathfindingComponentOffsets
{
	public static int PathNodeStart = 44;

	[FieldOffset(44)]
	public Vector2i ClickToNextPosition;

	[FieldOffset(52)]
	public Vector2i WasInThisPosition;

	[FieldOffset(1304)]
	public byte IsMoving;

	[FieldOffset(1304)]
	public int DestinationNodes;

	[FieldOffset(1360)]
	public Vector2i WantMoveToPosition;

	[FieldOffset(1372)]
	public float StayTime;
}
