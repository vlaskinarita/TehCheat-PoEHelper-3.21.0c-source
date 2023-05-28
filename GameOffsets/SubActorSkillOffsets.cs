using System.Runtime.InteropServices;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct SubActorSkillOffsets
{
	public const int SecondCooldownOffset = 32;

	[FieldOffset(0)]
	public ushort Id;

	[FieldOffset(8)]
	public long EffectsPerLevelPtr;

	[FieldOffset(112)]
	public byte CanBeUsedWithWeapon;

	[FieldOffset(113)]
	public byte CannotBeUsed;

	[FieldOffset(116)]
	public int TotalUses;

	[FieldOffset(128)]
	public long CooldownPtr;

	[FieldOffset(220)]
	public int SoulsPerUse;

	[FieldOffset(224)]
	public int TotalVaalUses;

	[FieldOffset(240)]
	public long StatsPtr;
}
