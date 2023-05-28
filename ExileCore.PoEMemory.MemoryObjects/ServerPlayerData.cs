using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using GameOffsets;
using GameOffsets.Native;

namespace ExileCore.PoEMemory.MemoryObjects;

public class ServerPlayerData : RemoteMemoryObject
{
	private readonly CachedValue<ServerPlayerDataOffsets> _CachedValue;

	public ServerPlayerDataOffsets ServerPlayerDataStruct => _CachedValue.Value;

	public CharacterClass Class => (CharacterClass)(ServerPlayerDataStruct.PlayerClass & 0xF);

	public int Level => ServerPlayerDataStruct.CharacterLevel;

	public int PassiveRefundPointsLeft => ServerPlayerDataStruct.PassiveRefundPointsLeft;

	public int QuestPassiveSkillPoints => ServerPlayerDataStruct.QuestPassiveSkillPoints;

	public int FreePassiveSkillPointsLeft => ServerPlayerDataStruct.FreePassiveSkillPointsLeft;

	public int TotalAscendencyPoints => ServerPlayerDataStruct.TotalAscendencyPoints;

	public int SpentAscendencyPoints => ServerPlayerDataStruct.SpentAscendencyPoints;

	public NativePtrArray AllocatedPassivesIds => ServerPlayerDataStruct.PassiveSkillIds;

	public ServerPlayerData()
	{
		_CachedValue = new FrameCache<ServerPlayerDataOffsets>(() => base.M.Read<ServerPlayerDataOffsets>(base.Address));
	}
}
