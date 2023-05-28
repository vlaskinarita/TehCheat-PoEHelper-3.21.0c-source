using System.Numerics;
using System.Runtime.InteropServices;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct CameraOffsets
{
	[FieldOffset(8)]
	public int Width;

	[FieldOffset(12)]
	public int Height;

	[FieldOffset(128)]
	public Matrix4x4 MatrixBytes;

	[FieldOffset(244)]
	public Vector3 Position;

	[FieldOffset(452)]
	public float ZFar;
}
