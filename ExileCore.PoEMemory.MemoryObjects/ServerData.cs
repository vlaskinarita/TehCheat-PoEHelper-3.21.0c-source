using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.FilesInMemory.Atlas;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using GameOffsets;
using GameOffsets.Native;
using SharpDX;

namespace ExileCore.PoEMemory.MemoryObjects;

public class ServerData : RemoteMemoryObject
{
	private static readonly int PlayerStashTabsOffset = Extensions.GetOffset((ServerDataOffsets x) => x.PlayerStashTabs) + 32768;

	private static readonly int GuildStashTabsOffset = Extensions.GetOffset((ServerDataOffsets x) => x.GuildStashTabs) + 32768;

	private readonly CachedValue<ServerDataOffsets> _cachedValue;

	private readonly CachedValue<ServerPlayerData> _PlayerData;

	private readonly List<Player> _nearestPlayers = new List<Player>();

	public ServerPlayerData PlayerInformation => _PlayerData.Value;

	public ServerDataOffsets ServerDataStruct => _cachedValue.Value;

	public ushort TradeChatChannel => ServerDataStruct.TradeChatChannel;

	public ushort GlobalChatChannel => ServerDataStruct.GlobalChatChannel;

	public byte MonsterLevel => ServerDataStruct.MonsterLevel;

	public int DialogDepth => ServerDataStruct.DialogDepth;

	public byte MonstersRemaining => ServerDataStruct.MonstersRemaining;

	public ushort CurrentSulphiteAmount => _cachedValue.Value.CurrentSulphiteAmount;

	public int CurrentAzuriteAmount => _cachedValue.Value.CurrentAzuriteAmount;

	[Obsolete]
	public SharpDX.Vector2 WorldMousePosition => _cachedValue.Value.WorldMousePosition.ToSharpDx();

	public System.Numerics.Vector2 WorldMousePositionNum => _cachedValue.Value.WorldMousePosition;

	public IList<Player> NearestPlayers
	{
		get
		{
			if (base.Address == 0L)
			{
				return new List<Player>();
			}
			long first = ServerDataStruct.NearestPlayers.First;
			long last = ServerDataStruct.NearestPlayers.Last;
			first += 16;
			if (first < base.Address || (last - first) / 16 > 50)
			{
				return _nearestPlayers;
			}
			_nearestPlayers.Clear();
			for (long num = first; num < last; num += 16)
			{
				_nearestPlayers.Add(ReadObject<Player>(num));
			}
			return _nearestPlayers;
		}
	}

	public ushort LastActionId => ServerDataStruct.LastActionId;

	public int CharacterLevel => PlayerInformation.Level;

	public int PassiveRefundPointsLeft => PlayerInformation.PassiveRefundPointsLeft;

	public int FreePassiveSkillPointsLeft => PlayerInformation.FreePassiveSkillPointsLeft;

	public int QuestPassiveSkillPoints => PlayerInformation.QuestPassiveSkillPoints;

	public int TotalAscendencyPoints => PlayerInformation.TotalAscendencyPoints;

	public int SpentAscendencyPoints => PlayerInformation.SpentAscendencyPoints;

	public PartyAllocation PartyAllocationType => (PartyAllocation)ServerDataStruct.PartyAllocationType;

	public string League => ServerDataStruct.League.ToString(base.M);

	public PartyStatus PartyStatusType => (PartyStatus)ServerDataStruct.PartyStatusType;

	public bool IsInGame => NetworkState == NetworkStateE.Connected;

	public NetworkStateE NetworkState => (NetworkStateE)ServerDataStruct.NetworkState;

	public int Latency => ServerDataStruct.Latency;

	public string Guild => NativeStringReader.ReadString(ServerDataStruct.GuildName, base.M);

	public BetrayalData BetrayalData => GetObject<BetrayalData>(base.M.Read<long>(base.Address + 208, new int[1] { 1968 }));

	public string GuildTag => NativeStringReader.ReadString(ServerDataStruct.GuildName + 32, base.M);

	public BitArray WaypointsUnlockState => new BitArray(base.M.ReadBytes(base.Address + 42209, 24));

	public IList<ushort> SkillBarIds
	{
		get
		{
			if (base.Address == 0L)
			{
				return new List<ushort>();
			}
			SkillBarIdsStruct skillBarIds = _cachedValue.Value.SkillBarIds;
			return new List<ushort>
			{
				skillBarIds.SkillBar1, skillBarIds.SkillBar2, skillBarIds.SkillBar3, skillBarIds.SkillBar4, skillBarIds.SkillBar5, skillBarIds.SkillBar6, skillBarIds.SkillBar7, skillBarIds.SkillBar8, skillBarIds.SkillBar9, skillBarIds.SkillBar10,
				skillBarIds.SkillBar11, skillBarIds.SkillBar12, skillBarIds.SkillBar13
			};
		}
	}

	public IList<ushort> PassiveSkillIds
	{
		get
		{
			if (base.Address == 0L)
			{
				return null;
			}
			long first = PlayerInformation.AllocatedPassivesIds.First;
			int num = (int)(PlayerInformation.AllocatedPassivesIds.Last - first);
			byte[] array = base.M.ReadMem(first, num);
			List<ushort> list = new List<ushort>();
			if (num < 0 || num > 500)
			{
				return new List<ushort>();
			}
			for (int i = 0; i < array.Length; i += 2)
			{
				ushort item = BitConverter.ToUInt16(array, i);
				list.Add(item);
			}
			return list;
		}
	}

	public IList<ServerStashTab> PlayerStashTabs => GetStashTabs(PlayerStashTabsOffset, PlayerStashTabsOffset + 8);

	public IList<ServerStashTab> GuildStashTabs => GetStashTabs(GuildStashTabsOffset, GuildStashTabsOffset + 8);

	public IList<InventoryHolder> PlayerInventories => ReadInventoryHolders(ServerDataStruct.PlayerInventories);

	public IList<InventoryHolder> NPCInventories => ReadInventoryHolders(ServerDataStruct.NPCInventories);

	public IList<InventoryHolder> GuildInventories => ReadInventoryHolders(ServerDataStruct.GuildInventories);

	public IList<WorldArea> CompletedAreas => GetAreas(ServerDataStruct.CompletedMaps);

	public IList<WorldArea> ShapedMaps => new List<WorldArea>();

	public IList<WorldArea> BonusCompletedAreas => GetAreas(ServerDataStruct.BonusCompletedAreas);

	public IList<WorldArea> ElderGuardiansAreas => new List<WorldArea>();

	public IList<WorldArea> MasterAreas => new List<WorldArea>();

	public IList<WorldArea> ShaperElderAreas => new List<WorldArea>();

	public List<byte> RegionIds_Debug
	{
		get
		{
			List<byte> list = new List<byte>();
			for (int i = 0; i < 8; i++)
			{
				list.Add(GetAtlasRegionUpgradesByRegion(i));
			}
			return list;
		}
	}

	public ushort LesserBrokenCircleArtifacts => ServerDataStruct.Artifacts.LesserBrokenCircleArtifacts;

	public ushort GreaterBrokenCircleArtifacts => ServerDataStruct.Artifacts.GreaterBrokenCircleArtifacts;

	public ushort GrandBrokenCircleArtifacts => ServerDataStruct.Artifacts.GrandBrokenCircleArtifacts;

	public ushort ExceptionalBrokenCircleArtifacts => ServerDataStruct.Artifacts.ExceptionalBrokenCircleArtifacts;

	public ushort LesserBlackScytheArtifacts => ServerDataStruct.Artifacts.LesserBlackScytheArtifacts;

	public ushort GreaterBlackScytheArtifacts => ServerDataStruct.Artifacts.GreaterBlackScytheArtifacts;

	public ushort GrandBlackScytheArtifacts => ServerDataStruct.Artifacts.GrandBlackScytheArtifacts;

	public ushort ExceptionalBlackScytheArtifacts => ServerDataStruct.Artifacts.ExceptionalBlackScytheArtifacts;

	public ushort LesserOrderArtifacts => ServerDataStruct.Artifacts.LesserOrderArtifacts;

	public ushort GreaterOrderArtifacts => ServerDataStruct.Artifacts.GreaterOrderArtifacts;

	public ushort GrandOrderArtifacts => ServerDataStruct.Artifacts.GrandOrderArtifacts;

	public ushort ExceptionalOrderArtifacts => ServerDataStruct.Artifacts.ExceptionalOrderArtifacts;

	public ushort LesserSunArtifacts => ServerDataStruct.Artifacts.LesserSunArtifacts;

	public ushort GreaterSunArtifacts => ServerDataStruct.Artifacts.GreaterSunArtifacts;

	public ushort GrandSunArtifacts => ServerDataStruct.Artifacts.GrandSunArtifacts;

	public ushort ExceptionalSunArtifacts => ServerDataStruct.Artifacts.ExceptionalSunArtifacts;

	public ServerData()
	{
		_cachedValue = new FrameCache<ServerDataOffsets>(() => base.M.Read<ServerDataOffsets>(base.Address + 32768));
		_PlayerData = new FrameCache<ServerPlayerData>(() => GetObject<ServerPlayerData>(_cachedValue.Value.PlayerRelatedData));
	}

	public int GetBeastCapturedAmount(BestiaryCapturableMonster monster)
	{
		return base.M.Read<int>(base.Address + 21056 + monster.Id * 4);
	}

	private IList<ServerStashTab> GetStashTabs(int offsetBegin, int offsetEnd)
	{
		long num = base.M.Read<long>(base.Address + offsetBegin);
		long num2 = base.M.Read<long>(base.Address + offsetEnd);
		if (num <= 0 || num2 <= 0)
		{
			return null;
		}
		long num3 = num2 - num;
		if (num3 <= 0 || num3 > 65535 || num <= 0 || num2 <= 0)
		{
			return new List<ServerStashTab>();
		}
		return new List<ServerStashTab>(base.M.ReadStructsArray<ServerStashTab>(num, num2, 104, base.TheGame));
	}

	private IList<InventoryHolder> ReadInventoryHolders(NativePtrArray array)
	{
		long first = array.First;
		long last = array.Last;
		if (first == 0L || last <= first || (last - first) / 32 > 1024)
		{
			return new List<InventoryHolder>();
		}
		return base.M.ReadStructsArray<InventoryHolder>(first, last, 32, this).ToList();
	}

	public ServerInventory GetPlayerInventoryBySlot(InventorySlotE slot)
	{
		foreach (InventoryHolder playerInventory in PlayerInventories)
		{
			if (playerInventory.Inventory.InventSlot == slot)
			{
				return playerInventory.Inventory;
			}
		}
		return null;
	}

	public ServerInventory GetPlayerInventoryByType(InventoryTypeE type)
	{
		foreach (InventoryHolder playerInventory in PlayerInventories)
		{
			if (playerInventory.Inventory.InventType == type)
			{
				return playerInventory.Inventory;
			}
		}
		return null;
	}

	public ServerInventory GetPlayerInventoryBySlotAndType(InventoryTypeE type, InventorySlotE slot)
	{
		foreach (InventoryHolder playerInventory in PlayerInventories)
		{
			if (playerInventory.Inventory.InventType == type && playerInventory.Inventory.InventSlot == slot)
			{
				return playerInventory.Inventory;
			}
		}
		return null;
	}

	public IList<WorldArea> GetAreas(long address)
	{
		if (base.Address == 0L || address == 0L)
		{
			return new List<WorldArea>();
		}
		List<WorldArea> list = new List<WorldArea>();
		long num = base.M.Read<long>(address);
		int num2 = 0;
		if (num == 0L)
		{
			return list;
		}
		long num3 = num;
		do
		{
			if (num3 == 0L)
			{
				return list;
			}
			long num4 = base.M.Read<long>(num3 + 16);
			if (num4 == 0L)
			{
				break;
			}
			WorldArea byAddress = base.TheGame.Files.WorldAreas.GetByAddress(num4);
			if (byAddress != null)
			{
				list.Add(byAddress);
			}
			num2++;
			if (num2 > 1024)
			{
				list = new List<WorldArea>();
				break;
			}
			num3 = base.M.Read<long>(num3);
		}
		while (num3 != num);
		return list;
	}

	public byte GetAtlasRegionUpgradesByRegion(int regionId)
	{
		return base.M.Read<byte>(base.Address + 34930 + regionId);
	}

	public byte GetAtlasRegionUpgradesByRegion(AtlasRegion region)
	{
		return base.M.Read<byte>(base.Address + 34930 + region.Index);
	}
}
