using System;
using System.Collections.Generic;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using GameOffsets;

namespace ExileCore.PoEMemory.FilesInMemory;

public class ModsDat : FileInMemory
{
	public class ModRecord
	{
		private class LevelComparer : IComparer<ModRecord>
		{
			public int Compare(ModRecord x, ModRecord y)
			{
				return -x.MinLevel + y.MinLevel;
			}
		}

		public const int NumberOfStats = 4;

		public static IComparer<ModRecord> ByLevelComparer = new LevelComparer();

		public long Address { get; }

		public string Key { get; }

		public ModType AffixType { get; }

		public ModDomain Domain { get; }

		public string Group { get; }

		public int MinLevel { get; }

		public StatsDat.StatRecord[] StatNames { get; }

		public IntRange[] StatRange { get; }

		public IDictionary<string, int> TagChances { get; }

		public TagsDat.TagRecord[] Tags { get; }

		public long Unknown8 { get; }

		public string UserFriendlyName { get; }

		public bool IsEssence { get; }

		public string Tier { get; }

		public string TypeName { get; }

		public ModRecord(IMemory m, StatsDat sDat, TagsDat tagsDat, long addr)
		{
			Address = addr;
			ModsRecordOffsets ModsRecord = m.Read<ModsRecordOffsets>(addr);
			Key = RemoteMemoryObject.Cache.StringCache.Read($"{"ModsDat"}{ModsRecord.Key.buf}", () => ModsRecord.Key.ToString(m));
			Unknown8 = ModsRecord.Unknown8;
			MinLevel = ModsRecord.MinLevel;
			long read = m.Read<long>(ModsRecord.TypeName);
			TypeName = RemoteMemoryObject.Cache.StringCache.Read($"{"ModsDat"}{read}", () => m.ReadStringU(read, 255));
			long s1 = ((ModsRecord.StatNames1 == 0L) ? 0 : m.Read<long>(ModsRecord.StatNames1));
			long s2 = ((ModsRecord.StatNames2 == 0L) ? 0 : m.Read<long>(ModsRecord.StatNames2));
			long s3 = ((ModsRecord.StatNames3 == 0L) ? 0 : m.Read<long>(ModsRecord.StatNames3));
			long s4 = ((ModsRecord.StatName4 == 0L) ? 0 : m.Read<long>(ModsRecord.StatName4));
			StatNames = new StatsDat.StatRecord[4]
			{
				(ModsRecord.StatNames1 == 0L) ? null : sDat.records[RemoteMemoryObject.Cache.StringCache.Read($"{"ModsDat"}{s1}", () => m.ReadStringU(s1))],
				(ModsRecord.StatNames2 == 0L) ? null : sDat.records[RemoteMemoryObject.Cache.StringCache.Read($"{"ModsDat"}{s2}", () => m.ReadStringU(s2))],
				(ModsRecord.StatNames3 == 0L) ? null : sDat.records[RemoteMemoryObject.Cache.StringCache.Read($"{"ModsDat"}{s3}", () => m.ReadStringU(s3))],
				(ModsRecord.StatName4 == 0L) ? null : sDat.records[RemoteMemoryObject.Cache.StringCache.Read($"{"ModsDat"}{s4}", () => m.ReadStringU(s4))]
			};
			Domain = (ModDomain)ModsRecord.Domain;
			UserFriendlyName = RemoteMemoryObject.Cache.StringCache.Read($"{"ModsDat"}{ModsRecord.UserFriendlyName}", () => m.ReadStringU(ModsRecord.UserFriendlyName));
			AffixType = (ModType)ModsRecord.AffixType;
			Group = RemoteMemoryObject.Cache.StringCache.Read($"{"ModsDat"}{ModsRecord.Group}", () => m.ReadStringU(ModsRecord.Group));
			StatRange = new IntRange[4]
			{
				new IntRange(ModsRecord.StatRange1, ModsRecord.StatRange2),
				new IntRange(ModsRecord.StatRange3, ModsRecord.StatRange4),
				new IntRange(ModsRecord.StatRange5, ModsRecord.StatRange6),
				new IntRange(ModsRecord.StatRange7, ModsRecord.StatRange8)
			};
			Tags = new TagsDat.TagRecord[ModsRecord.Tags];
			long ta = ModsRecord.ta;
			for (int j = 0; j < Tags.Length; j++)
			{
				long addr2 = ta + 16 * j;
				long i = m.Read<long>(addr2, new int[1]);
				string key = RemoteMemoryObject.Cache.StringCache.Read($"{"ModsDat"}{i}", () => m.ReadStringU(i, 255));
				Tags[j] = tagsDat.Records[key];
			}
			TagChances = new Dictionary<string, int>();
			long tc = ModsRecord.tc;
			for (int k = 0; k < Tags.Length; k++)
			{
				TagChances[Tags[k].Key] = m.Read<int>(tc + 4 * k);
			}
			IsEssence = ModsRecord.IsEssence == 1;
			Tier = RemoteMemoryObject.Cache.StringCache.Read($"{"ModsDat"}{ModsRecord.Tier}", () => m.ReadStringU(ModsRecord.Tier));
		}

		public override string ToString()
		{
			return $"Name: {UserFriendlyName}, Key: {Key}, MinLevel: {MinLevel}";
		}
	}

	public IDictionary<string, ModRecord> records { get; } = new Dictionary<string, ModRecord>(StringComparer.OrdinalIgnoreCase);


	public IDictionary<long, ModRecord> DictionaryRecords { get; } = new Dictionary<long, ModRecord>();


	public IDictionary<Tuple<string, ModType>, List<ModRecord>> recordsByTier { get; } = new Dictionary<Tuple<string, ModType>, List<ModRecord>>();


	public ModsDat(IMemory m, Func<long> address, StatsDat sDat, TagsDat tagsDat)
		: base(m, address)
	{
		loadItems(sDat, tagsDat);
	}

	public ModRecord GetModByAddress(long address)
	{
		DictionaryRecords.TryGetValue(address, out var value);
		return value;
	}

	private void loadItems(StatsDat sDat, TagsDat tagsDat)
	{
		foreach (long item in RecordAddresses())
		{
			ModRecord modRecord;
			try
			{
				modRecord = new ModRecord(base.M, sDat, tagsDat, item);
			}
			catch (Exception exception)
			{
				Logger.Log.Warning(exception, "Error load ModRecord");
				continue;
			}
			if (records.ContainsKey(modRecord.Key))
			{
				continue;
			}
			DictionaryRecords.Add(item, modRecord);
			records.Add(modRecord.Key, modRecord);
			if (modRecord.Domain != ModDomain.Monster)
			{
				Tuple<string, ModType> key = Tuple.Create(modRecord.Group, modRecord.AffixType);
				if (!recordsByTier.TryGetValue(key, out var value))
				{
					value = new List<ModRecord>();
					recordsByTier[key] = value;
				}
				value.Add(modRecord);
			}
		}
		foreach (List<ModRecord> value2 in recordsByTier.Values)
		{
			value2.Sort(ModRecord.ByLevelComparer);
		}
	}
}
