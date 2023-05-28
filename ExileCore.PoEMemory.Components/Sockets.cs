using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExileCore.PoEMemory.MemoryObjects;

namespace ExileCore.PoEMemory.Components;

public class Sockets : Component
{
	public class SocketedGem
	{
		public Entity GemEntity;

		public int SocketIndex;
	}

	public int LargestLinkSize
	{
		get
		{
			if (base.Address == 0L)
			{
				return 0;
			}
			long num = base.M.Read<long>(base.Address + 96);
			long num2 = base.M.Read<long>(base.Address + 104) - num;
			if (num2 <= 0 || num2 > 6)
			{
				return 0;
			}
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				int num4 = base.M.Read<byte>(num + i);
				if (num4 > num3)
				{
					num3 = num4;
				}
			}
			return num3;
		}
	}

	public List<int[]> Links
	{
		get
		{
			List<int[]> list = new List<int[]>();
			if (base.Address == 0L)
			{
				return list;
			}
			long num = base.M.Read<long>(base.Address + 96);
			long num2 = base.M.Read<long>(base.Address + 104) - num;
			if (num2 <= 0 || num2 > 6)
			{
				return list;
			}
			int num3 = 0;
			List<int> socketList = SocketList;
			for (int i = 0; i < num2; i++)
			{
				int num4 = base.M.Read<byte>(num + i);
				int num5 = num3 + num4;
				if (num5 > socketList.Count)
				{
					return list;
				}
				int[] item = socketList.Take(num3..num5).ToArray();
				list.Add(item);
				num3 = num5;
			}
			return list;
		}
	}

	public List<int> SocketList
	{
		get
		{
			List<int> list = new List<int>();
			if (base.Address == 0L)
			{
				return list;
			}
			long num = base.Address + 24;
			for (int i = 0; i < 6; i++)
			{
				int num2 = base.M.Read<int>(num);
				if (num2 >= 1 && num2 <= 6)
				{
					list.Add(base.M.Read<int>(num));
				}
				num += 4;
			}
			return list;
		}
	}

	public int NumberOfSockets => SocketList.Count;

	public bool IsRGB
	{
		get
		{
			if (base.Address != 0L)
			{
				return Links.Any((int[] current) => current.Length >= 3 && current.Contains(1) && current.Contains(2) && current.Contains(3));
			}
			return false;
		}
	}

	public List<string> SocketGroup
	{
		get
		{
			List<string> list = new List<string>();
			foreach (int[] link in Links)
			{
				StringBuilder stringBuilder = new StringBuilder();
				int[] array = link;
				for (int i = 0; i < array.Length; i++)
				{
					switch (array[i])
					{
					case 1:
						stringBuilder.Append("R");
						break;
					case 2:
						stringBuilder.Append("G");
						break;
					case 3:
						stringBuilder.Append("B");
						break;
					case 4:
						stringBuilder.Append("W");
						break;
					case 5:
						stringBuilder.Append('A');
						break;
					case 6:
						stringBuilder.Append("O");
						break;
					}
				}
				list.Add(stringBuilder.ToString());
			}
			return list;
		}
	}

	public List<SocketedGem> SocketedGems
	{
		get
		{
			List<SocketedGem> list = new List<SocketedGem>();
			long num = base.Address + 48;
			for (int i = 0; i < 6; i++)
			{
				if (base.M.Read<long>(num) != 0L)
				{
					list.Add(new SocketedGem
					{
						SocketIndex = i,
						GemEntity = ReadObject<Entity>(num)
					});
				}
				num += 8;
			}
			return list;
		}
	}
}
