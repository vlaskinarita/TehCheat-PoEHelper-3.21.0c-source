using ExileCore.PoEMemory.MemoryObjects;

namespace ExileCore.PoEMemory.Elements;

public class SentinelSubPanel : Element
{
	public SentinelData SentinelData => base.Tooltip.ReadObjectAt<SentinelData>(776);

	public Entity SentinelItem => GetChildFromIndices(default(int), default(int))?.ReadObjectAt<Entity>(928);
}
