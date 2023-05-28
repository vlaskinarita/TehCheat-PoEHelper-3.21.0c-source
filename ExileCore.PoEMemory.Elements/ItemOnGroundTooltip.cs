namespace ExileCore.PoEMemory.Elements;

public class ItemOnGroundTooltip : Element
{
	public Element ItemFrame => GetChildAtIndex(0)?.GetChildAtIndex(0);

	public Element TooltipUI => GetChildAtIndex(0)?.GetChildAtIndex(0);

	public Element Item2DIcon => TooltipUI?.GetChildAtIndex(0);

	public new Element Tooltip => TooltipUI?.GetChildAtIndex(1);
}
