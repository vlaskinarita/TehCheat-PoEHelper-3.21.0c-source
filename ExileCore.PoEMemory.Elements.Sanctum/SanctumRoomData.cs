using System.Collections.Generic;
using System.Linq;
using ExileCore.PoEMemory.FilesInMemory.Sanctum;

namespace ExileCore.PoEMemory.Elements.Sanctum;

public class SanctumRoomData : RemoteMemoryObject
{
	public SanctumRoom FightRoom => base.TheGame.Files.SanctumRooms.GetByAddress(base.M.Read<long>(base.Address + 8));

	public SanctumRoom RewardRoom => base.TheGame.Files.SanctumRooms.GetByAddress(base.M.Read<long>(base.Address + 24));

	public SanctumPersistentEffect RoomEffect => base.TheGame.Files.SanctumPersistentEffects.GetByAddress(base.M.Read<long>(base.Address + 40));

	public SanctumDeferredRewardDisplayCategory Reward1 => base.TheGame.Files.SanctumDeferredRewardDisplayCategories.GetByAddress(base.M.Read<long>(base.Address + 64));

	public SanctumDeferredRewardDisplayCategory Reward2 => base.TheGame.Files.SanctumDeferredRewardDisplayCategories.GetByAddress(base.M.Read<long>(base.Address + 80));

	public SanctumDeferredRewardDisplayCategory Reward3 => base.TheGame.Files.SanctumDeferredRewardDisplayCategories.GetByAddress(base.M.Read<long>(base.Address + 96));

	public List<SanctumDeferredRewardDisplayCategory> Rewards => new SanctumDeferredRewardDisplayCategory[3] { Reward1, Reward2, Reward3 }.Where((SanctumDeferredRewardDisplayCategory x) => x != null && x.Address != 0).ToList();
}
