namespace ExileCore.PoEMemory.FilesInMemory;

public class ChestRecord : RemoteMemoryObject
{
	public string Id => base.M.ReadStringU(base.M.Read<long>(base.Address));

	public string DisplayName => base.M.ReadStringU(base.M.Read<long>(base.Address + 13));

	public override string ToString()
	{
		return $"{Id} ({base.Address:X})";
	}
}
