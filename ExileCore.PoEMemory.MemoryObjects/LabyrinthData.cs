using System.Collections.Generic;
using SharpDX;

namespace ExileCore.PoEMemory.MemoryObjects;

public class LabyrinthData : RemoteMemoryObject
{
	internal static Dictionary<long, LabyrinthRoom> CachedRoomsDictionary;

	public IList<LabyrinthRoom> Rooms
	{
		get
		{
			long num = base.M.Read<long>(base.Address);
			long num2 = base.M.Read<long>(base.Address + 8);
			List<LabyrinthRoom> list = new List<LabyrinthRoom>();
			CachedRoomsDictionary = new Dictionary<long, LabyrinthRoom>();
			int value = 0;
			for (long num3 = num; num3 < num2; num3 += 96)
			{
				DebugWindow.LogMsg($"Room {value} Addr: {num3.ToString("x")}", 0f, Color.White);
				if (num3 != 0L)
				{
					LabyrinthRoom labyrinthRoom = new LabyrinthRoom(base.M, num3, base.TheGame.Files.WorldAreas)
					{
						Id = value++
					};
					list.Add(labyrinthRoom);
					CachedRoomsDictionary.Add(num3, labyrinthRoom);
				}
			}
			return list;
		}
	}

	internal static LabyrinthRoom GetRoomById(long roomId)
	{
		CachedRoomsDictionary.TryGetValue(roomId, out var value);
		return value;
	}
}
