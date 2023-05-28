using System.Numerics;
using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ServerDataOffsets
{
	public const int Skip = 32768;

	public const int ATLAS_REGION_UPGRADES = 34930;

	public const int WaypointsUnlockStateOffset = 42209;

	public const int BetrayalDataOffset2 = 1968;

	[FieldOffset(0)]
	public long MasterAreas;

	[FieldOffset(5920)]
	public long PlayerRelatedData;

	[FieldOffset(5944)]
	public byte NetworkState;

	[FieldOffset(5984)]
	public NativeStringU League;

	[FieldOffset(6104)]
	public int TimeInGame;

	[FieldOffset(6112)]
	public int TimeInGame2;

	[FieldOffset(6136)]
	public int Latency;

	[FieldOffset(6144)]
	public NativePtrArray PlayerStashTabs;

	[FieldOffset(6168)]
	public NativePtrArray GuildStashTabs;

	[FieldOffset(5552)]
	public NativeStringU PartyLeaderName;

	[FieldOffset(5568)]
	public byte PartyStatusType;

	[FieldOffset(5592)]
	public NativePtrArray CurrentParty;

	[FieldOffset(5616)]
	public byte PartyAllocationType;

	[FieldOffset(5617)]
	public bool PartyDownscaleDisabled;

	[FieldOffset(6776)]
	public long GuildName;

	[FieldOffset(6792)]
	public SkillBarIdsStruct SkillBarIds;

	[FieldOffset(6820)]
	public Vector2 WorldMousePosition;

	[FieldOffset(6888)]
	public NativePtrArray NearestPlayers;

	[FieldOffset(7624)]
	public NativePtrArray PlayerInventories;

	[FieldOffset(8312)]
	public NativePtrArray NPCInventories;

	[FieldOffset(9000)]
	public NativePtrArray GuildInventories;

	[FieldOffset(9376)]
	public ushort TradeChatChannel;

	[FieldOffset(9384)]
	public ushort GlobalChatChannel;

	[FieldOffset(9544)]
	public ushort LastActionId;

	[FieldOffset(8632)]
	public int CompletedMapsCount;

	[FieldOffset(9712)]
	public long CompletedMaps;

	[FieldOffset(9776)]
	public long BonusCompletedAreas;

	[FieldOffset(10400)]
	public int DialogDepth;

	[FieldOffset(10404)]
	public byte MonsterLevel;

	[FieldOffset(10405)]
	public byte MonstersRemaining;

	[FieldOffset(10578)]
	public int CurrentAzuriteAmount;

	[FieldOffset(10594)]
	public ushort CurrentSulphiteAmount;

	[FieldOffset(11168)]
	public ServerDataArtifacts Artifacts;
}
