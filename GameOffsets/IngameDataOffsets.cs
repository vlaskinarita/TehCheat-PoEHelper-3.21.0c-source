using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct IngameDataOffsets
{
	[FieldOffset(136)]
	public long CurrentArea;

	[FieldOffset(168)]
	public byte CurrentAreaLevel;

	[FieldOffset(236)]
	public uint CurrentAreaHash;

	[FieldOffset(256)]
	public NativePtrArray MapStats;

	[FieldOffset(272)]
	public long LabDataPtr;

	[FieldOffset(648)]
	public long IngameStatePtr;

	[FieldOffset(840)]
	public long IngameStatePtr2;

	[FieldOffset(1880)]
	public long ServerData;

	[FieldOffset(1888)]
	public long LocalPlayer;

	[FieldOffset(2064)]
	public long EntityList;

	[FieldOffset(2072)]
	public long EntitiesCount;

	[FieldOffset(2464)]
	public TerrainData Terrain;

	[FieldOffset(2496)]
	public NativePtrArray TgtArray;
}
