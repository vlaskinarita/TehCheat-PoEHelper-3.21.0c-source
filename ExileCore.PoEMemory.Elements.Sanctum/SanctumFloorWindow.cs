using System.Collections.Generic;
using System.Linq;

namespace ExileCore.PoEMemory.Elements.Sanctum;

public class SanctumFloorWindow : Element
{
	public List<List<SanctumRoomElement>> RoomsByLayer => GetChildFromIndices(0, 0, 0, 1)?.Children.Select((Element x) => x.GetChildrenAs<SanctumRoomElement>()).ToList() ?? new List<List<SanctumRoomElement>>();

	public List<SanctumRoomElement> Rooms => RoomsByLayer.SelectMany((List<SanctumRoomElement> x) => x).ToList();

	public SanctumFloorData FloorData => GetObject<SanctumFloorData>(base.M.Read<long>(base.Address + 704) + 344);
}
