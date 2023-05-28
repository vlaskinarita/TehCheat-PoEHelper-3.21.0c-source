using System.Collections.Generic;
using System.Linq;
using GameOffsets.Native;

namespace ExileCore.PoEMemory.Elements.Sanctum;

public class SanctumFloorData : RemoteMemoryObject
{
	public NativePtrArray RoomDataArray => base.M.Read<NativePtrArray>(base.Address + 24);

	public List<SanctumRoomData> RoomData => base.M.ReadStructsArray<SanctumRoomData>(RoomDataArray.First, RoomDataArray.Last, 112, null);

	public byte[][][] RoomLayout => (from x in base.M.ReadStdVectorStride<NativePtrArray>(base.M.Read<NativePtrArray>(base.Address), 32)
		select (from y in base.M.ReadStdVectorStride<NativePtrArray>(x, 56)
			select base.M.ReadStdVector<byte>(y)).ToArray()).ToArray();
}
