namespace ExileCore.PoEMemory.MemoryObjects;

public class MapDeviceWindow : Element
{
	public Element OpenMapDialog => GetChildAtIndex(3);

	public Element CloseMapDialog => GetChildAtIndex(4);

	public Element ChooseZanaMod => GetChildAtIndex(5);

	public Element BottomMapSettings => GetChildAtIndex(6);

	public Element ActivateButton => BottomMapSettings?.GetChildAtIndex(1);

	public Element ChooseMastersMods => BottomMapSettings?.GetChildAtIndex(3);
}
