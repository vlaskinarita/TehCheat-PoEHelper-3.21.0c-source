using System.Collections.Generic;
using System.Linq;
using ExileCore.PoEMemory.FilesInMemory;

namespace ExileCore.PoEMemory.MemoryObjects;

public class MonsterVariety : RemoteMemoryObject
{
	private string _varietyId;

	public int Id { get; internal set; }

	public string VarietyId => _varietyId ?? (_varietyId = base.M.ReadStringU(base.M.Read<long>(base.Address)));

	public long MonsterTypePtr => base.M.Read<long>(base.Address + 16);

	public int ObjectSize => base.M.Read<int>(base.Address + 28);

	public int MinimumAttackDistance => base.M.Read<int>(base.Address + 32);

	public int MaximumAttackDistance => base.M.Read<int>(base.Address + 36);

	public string ACTFile => base.M.ReadStringU(base.M.Read<long>(base.Address + 40));

	public string AOFile => base.M.ReadStringU(base.M.Read<long>(base.Address + 48));

	public string BaseMonsterTypeIndex => base.M.ReadStringU(base.M.Read<long>(base.Address + 56));

	public IEnumerable<ModsDat.ModRecord> Mods
	{
		get
		{
			int count = base.M.Read<int>(base.Address + 64);
			return (from x in base.M.ReadSecondPointerArray_Count(base.M.Read<long>(base.Address + 72), count)
				select base.TheGame.Files.Mods.GetModByAddress(x)).ToList();
		}
	}

	public int ModelSizeMultiplier => base.M.Read<int>(base.Address + 100);

	public int ExperienceMultiplier => base.M.Read<int>(base.Address + 140);

	public int CriticalStrikeChance => base.M.Read<int>(base.Address + 172);

	public string AISFile => base.M.ReadStringU(base.M.Read<long>(base.Address + 196));

	public string MonsterName => base.M.ReadStringU(base.M.Read<long>(base.Address + 260));

	public int DamageMultiplier => base.M.Read<int>(base.Address + 252);

	public int LifeMultiplier => base.M.Read<int>(base.Address + 256);

	public int AttackSpeed => base.M.Read<int>(base.Address + 260);

	public override string ToString()
	{
		return $"Name: {MonsterName}, VarietyId: {VarietyId}, BaseMonsterTypeIndex: {BaseMonsterTypeIndex}";
	}
}
