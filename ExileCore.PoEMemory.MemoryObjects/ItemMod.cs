using System;
using System.Collections.Generic;
using System.Linq;
using GameOffsets.Native;

namespace ExileCore.PoEMemory.MemoryObjects;

public class ItemMod : RemoteMemoryObject
{
	public static int STRUCT_SIZE = 56;

	private string displayName;

	private string group;

	private int level;

	private string name;

	private string rawName;

	[Obsolete("Use Values instead")]
	public int Value1 => base.M.Read<int>(base.Address, new int[1]);

	[Obsolete("Use Values instead")]
	public int Value2 => base.M.Read<int>(base.Address, new int[1] { 4 });

	[Obsolete("Use Values instead")]
	public int Value3 => base.M.Read<int>(base.Address, new int[1] { 8 });

	[Obsolete("Use Values instead")]
	public int Value4 => base.M.Read<int>(base.Address, new int[1] { 12 });

	public List<int> Values
	{
		get
		{
			long num = base.M.Read<long>(base.Address);
			long num2 = base.M.Read<long>(base.Address + 8);
			long num3 = (num2 - num) / 8;
			if (num3 < 0 || num3 > 10)
			{
				return new List<int>();
			}
			return base.M.ReadStructsArray<int>(num, num2, 4);
		}
	}

	public List<Vector2i> ValuesMinMax
	{
		get
		{
			long num = base.M.Read<long>(base.Address + 40) + 120;
			long num2 = num + Values.Count * 4 * 2;
			List<Vector2i> list = new List<Vector2i>();
			for (long num3 = num; num3 < num2; num3 += 8)
			{
				list.Add(new Vector2i(base.M.Read<int>(num3), base.M.Read<int>(num3 + 4)));
			}
			return list;
		}
	}

	public string RawName
	{
		get
		{
			if (rawName == null)
			{
				ParseName();
			}
			return rawName;
		}
	}

	public string Group
	{
		get
		{
			if (group == null)
			{
				ParseName();
			}
			return group;
		}
	}

	public string Name
	{
		get
		{
			if (rawName == null)
			{
				ParseName();
			}
			return name;
		}
	}

	public string DisplayName
	{
		get
		{
			if (rawName == null)
			{
				ParseName();
			}
			return displayName;
		}
	}

	public int Level
	{
		get
		{
			if (rawName == null)
			{
				ParseName();
			}
			return level;
		}
	}

	private void ParseName()
	{
		long addr = base.M.Read<long>(base.Address + 40, new int[1]);
		rawName = RemoteMemoryObject.Cache.StringCache.Read($"{"ItemMod"}{addr}", () => base.M.ReadStringU(addr));
		displayName = RemoteMemoryObject.Cache.StringCache.Read($"{"ItemMod"}{addr + 100}", () => base.M.ReadStringU(base.M.Read<long>(base.Address + 40, new int[1] { 100 })));
		name = rawName.Replace("_", "");
		group = RemoteMemoryObject.Cache.StringCache.Read($"{"ItemMod"}{addr + 2552}", () => base.M.ReadStringU(base.M.Read<long>(base.Address + 40, new int[2] { 2552, 0 })));
		int num = name.IndexOfAny("0123456789".ToCharArray());
		if (num < 0 || !int.TryParse(name.Substring(num), out level))
		{
			level = 1;
		}
		else
		{
			name = name.Substring(0, num);
		}
	}

	public override string ToString()
	{
		List<Vector2i> minMax = ValuesMinMax;
		IEnumerable<string> values = Values.Select(delegate(int x, int i)
		{
			Vector2i vector2i = minMax[i];
			return (vector2i.X != vector2i.Y) ? $"{x} [{vector2i.X}-{vector2i.Y}]" : x.ToString();
		});
		return Name + " (" + string.Join(", ", values) + ")";
	}
}
