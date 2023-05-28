using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Interfaces;

namespace ExileCore.PoEMemory.FilesInMemory;

public class MonsterVarieties : UniversalFileWrapper<MonsterVariety>
{
	private readonly Dictionary<string, MonsterVariety> MonsterVarietyMetadataDictionary = new Dictionary<string, MonsterVariety>();

	public new IList<MonsterVariety> EntriesList => base.EntriesList.ToList();

	public MonsterVarieties(IMemory m, Func<long> address)
		: base(m, address)
	{
	}

	public MonsterVariety TranslateFromMetadata(string path)
	{
		CheckCache();
		MonsterVarietyMetadataDictionary.TryGetValue(path, out var value);
		return value;
	}

	protected override void EntryAdded(long addr, MonsterVariety entry)
	{
		MonsterVarietyMetadataDictionary.Add(entry.VarietyId, entry);
	}
}
