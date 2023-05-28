using ExileCore.PoEMemory.FilesInMemory;

namespace ExileCore.PoEMemory.Elements;

public class TabletTileElement : Element
{
	private LakeRoom _room;

	public int TileY => base.M.Read<int>(base.Address + 820);

	public int TileX => base.M.Read<int>(base.Address + 816);

	public int Difficulty => base.M.Read<int>(base.Address + 840);

	public LakeRoom Room => _room ?? (_room = base.TheGame.Files.LakeRooms.GetByAddress(base.M.Read<long>(base.Address + 824)));
}
