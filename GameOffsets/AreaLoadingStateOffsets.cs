using System.Runtime.InteropServices;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct AreaLoadingStateOffsets
{
	[FieldOffset(200)]
	public long IsLoading;

	[FieldOffset(872)]
	public uint TotalLoadingScreenTimeMs;

	[FieldOffset(904)]
	public long AreaName;
}
