using ExileCore.PoEMemory.Elements;

namespace ExileCore.PoEMemory.MemoryObjects;

public class GameUi : Element
{
	public Element UnusedPassivePointsButton => GetChildAtIndex(3);

	public int UnusedPassivePointsAmount => GetUnusedPassivePointsAmount();

	public SentinelPanel SentinelPanel => GetChildFromIndices(7, 12, 4)?.AsObject<SentinelPanel>();

	private int GetUnusedPassivePointsAmount()
	{
		Element childFromIndices = GetChildFromIndices(3, 1);
		if (childFromIndices == null || !childFromIndices.IsVisible)
		{
			return 0;
		}
		if (!int.TryParse(childFromIndices.Text, out var result))
		{
			return 0;
		}
		return result;
	}
}
