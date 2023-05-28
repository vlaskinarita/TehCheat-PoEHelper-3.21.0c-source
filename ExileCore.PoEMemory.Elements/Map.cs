namespace ExileCore.PoEMemory.Elements;

public class Map : Element
{
	private Element _largeMap;

	private Element _smallMap;

	public Element LargeMap => _largeMap ?? (_largeMap = ReadObjectAt<Element>(632));

	public float LargeMapShiftX => base.M.Read<float>(LargeMap.Address + 616);

	public float LargeMapShiftY => base.M.Read<float>(LargeMap.Address + 620);

	public float LargeMapZoom => base.M.Read<float>(LargeMap.Address + 684);

	public Element SmallMiniMap => _smallMap ?? (_smallMap = ReadObjectAt<Element>(640));

	public float SmallMinMapX => base.M.Read<float>(SmallMiniMap.Address + 616);

	public float SmallMinMapY => base.M.Read<float>(SmallMiniMap.Address + 620);

	public float SmallMinMapZoom => base.M.Read<float>(SmallMiniMap.Address + 684);

	public Element OrangeWords => ReadObjectAt<Element>(680);

	public Element BlueWords => ReadObjectAt<Element>(824);
}
