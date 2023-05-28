using System.Collections.Generic;
using System.Linq;
using ExileCore.Shared.Cache;
using GameOffsets;
using JM.LinqFaster;

namespace ExileCore.PoEMemory.Components;

public sealed class Buffs : Component
{
	private readonly CachedValue<List<Buff>> _cachedValueBuffs;

	public List<Buff> BuffsList => _cachedValueBuffs.Value;

	public Buffs()
	{
		_cachedValueBuffs = new FrameCache<List<Buff>>(ParseBuffs);
	}

	public List<Buff> ParseBuffs()
	{
		BuffsOffsets buffsOffsets = base.M.Read<BuffsOffsets>(base.Address);
		return base.M.ReadPointersArray(buffsOffsets.Buffs.First, buffsOffsets.Buffs.Last).Select(base.GetObject<Buff>).ToList();
	}

	public bool HasBuff(string buff)
	{
		return BuffsList?.AnyF((Buff x) => x.Name == buff) ?? false;
	}

	public bool TryGetBuff(string name, out Buff buff)
	{
		buff = BuffsList.FirstOrDefault((Buff x) => x.Name == name);
		return buff != null;
	}
}
