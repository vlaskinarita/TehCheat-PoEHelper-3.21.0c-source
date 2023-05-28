namespace ExileCore.PoEMemory.MemoryObjects;

public class LabyrinthSectionOverrides
{
	public string Name { get; internal set; }

	public string OverrideName { get; internal set; }

	public override string ToString()
	{
		return OverrideName;
	}
}
