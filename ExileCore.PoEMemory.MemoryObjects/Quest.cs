namespace ExileCore.PoEMemory.MemoryObjects;

public class Quest : RemoteMemoryObject
{
	private string id;

	private string name;

	public string Id
	{
		get
		{
			if (id == null)
			{
				return id = base.M.ReadStringU(base.M.Read<long>(base.Address), 255);
			}
			return id;
		}
	}

	public int Act => base.M.Read<int>(base.Address + 8);

	public string Name
	{
		get
		{
			if (name == null)
			{
				return name = base.M.ReadStringU(base.M.Read<long>(base.Address + 12));
			}
			return name;
		}
	}

	public string Icon => base.M.ReadStringU(base.M.Read<long>(base.Address + 24));

	public override string ToString()
	{
		return "Id: " + Id + ", Name: " + Name;
	}
}
