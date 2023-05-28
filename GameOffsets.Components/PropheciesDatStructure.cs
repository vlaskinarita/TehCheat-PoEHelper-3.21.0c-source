using System.Runtime.InteropServices;

namespace GameOffsets.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct PropheciesDatStructure
{
	[FieldOffset(0)]
	public long IdString;

	[FieldOffset(8)]
	public long PredictionText;

	[FieldOffset(16)]
	public int IdNumber;

	[FieldOffset(20)]
	public long Name;

	[FieldOffset(28)]
	public long FlavourText;

	[FieldOffset(36)]
	public long TotalClientStringKeys;

	[FieldOffset(44)]
	public long ClientStringKeysPtr;

	[FieldOffset(52)]
	public long AudioFile;

	[FieldOffset(68)]
	public long ProphecyChainPtr;

	[FieldOffset(76)]
	public int ProphecyChainPosition;

	[FieldOffset(80)]
	public byte IsEnabled;

	[FieldOffset(81)]
	public int SealCost;
}
