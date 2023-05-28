using System.Runtime.InteropServices;

namespace GameOffsets.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ProphecyChainDatStructure
{
	[FieldOffset(0)]
	public long IdString;

	[FieldOffset(8)]
	public int Unknown1;

	[FieldOffset(12)]
	public long TotalUnknown2Keys;

	[FieldOffset(20)]
	public long Unknown2Ptr;

	[FieldOffset(28)]
	public long TotalUnknown3Keys;

	[FieldOffset(36)]
	public long Unknown3Ptr;

	[FieldOffset(44)]
	public int Unknown4;

	[FieldOffset(48)]
	public int Unknown5;
}
