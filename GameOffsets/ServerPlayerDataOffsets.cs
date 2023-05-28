using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ServerPlayerDataOffsets
{
	[FieldOffset(400)]
	public NativePtrArray PassiveSkillIds;

	[FieldOffset(584)]
	public byte PlayerClass;

	[FieldOffset(588)]
	public int CharacterLevel;

	[FieldOffset(592)]
	public int PassiveRefundPointsLeft;

	[FieldOffset(596)]
	public int QuestPassiveSkillPoints;

	[FieldOffset(600)]
	public int FreePassiveSkillPointsLeft;

	[FieldOffset(596)]
	public int TotalAscendencyPoints;

	[FieldOffset(608)]
	public int SpentAscendencyPoints;
}
