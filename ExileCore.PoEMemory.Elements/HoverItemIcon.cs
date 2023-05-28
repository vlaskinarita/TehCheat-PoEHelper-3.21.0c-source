using System;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using GameOffsets;

namespace ExileCore.PoEMemory.Elements;

public class HoverItemIcon : Element
{
	private static readonly int InventPosXOffset = Extensions.GetOffset((NormalInventoryItemOffsets x) => x.InventPosX);

	private static readonly int InventPosYOffset = Extensions.GetOffset((NormalInventoryItemOffsets x) => x.InventPosY);

	private static readonly int ItemsOnGroundLabelElementOffset = Extensions.GetOffset((IngameUIElementsOffsets x) => x.itemsOnGroundLabelRoot);

	private ToolTipType? _tooltipType;

	[Obsolete("Use Element.Tooltip")]
	public Element InventoryItemTooltip => base.Tooltip;

	[Obsolete("Use Element.Tooltip")]
	public Element ItemInChatTooltip => base.Tooltip;

	public ItemOnGroundTooltip ToolTipOnGround => base.TheGame.IngameState.IngameUi.ItemOnGroundTooltip;

	public int InventPosX => base.M.Read<int>(base.Address + InventPosXOffset);

	public int InventPosY => base.M.Read<int>(base.Address + InventPosYOffset);

	public ToolTipType ToolTipType
	{
		get
		{
			try
			{
				ToolTipType valueOrDefault = _tooltipType.GetValueOrDefault();
				ToolTipType result;
				if (!_tooltipType.HasValue)
				{
					valueOrDefault = GetToolTipType();
					_tooltipType = valueOrDefault;
					result = valueOrDefault;
				}
				else
				{
					result = valueOrDefault;
				}
				return result;
			}
			catch (Exception ex)
			{
				Core.Logger?.Error(ex.Message + " " + ex.StackTrace);
				return ToolTipType.None;
			}
		}
	}

	public new Element Tooltip => ToolTipType switch
	{
		ToolTipType.ItemOnGround => ToolTipOnGround.Tooltip, 
		ToolTipType.InventoryItem => base.Tooltip, 
		ToolTipType.ItemInChat => base.Tooltip.Children[1], 
		_ => null, 
	};

	public Element ItemFrame => ToolTipType switch
	{
		ToolTipType.ItemOnGround => ToolTipOnGround.ItemFrame, 
		ToolTipType.ItemInChat => base.Tooltip.Children[0], 
		_ => null, 
	};

	public Entity Item => ToolTipType switch
	{
		ToolTipType.ItemOnGround => base.TheGame.IngameState.IngameUi.ReadObjectAt<ItemsOnGroundLabelElement>(ItemsOnGroundLabelElementOffset)?.ItemOnHover?.GetComponent<WorldItem>()?.ItemEntity, 
		ToolTipType.InventoryItem => base.Entity, 
		ToolTipType.ItemInChat => null, 
		_ => null, 
	};

	private ToolTipType GetToolTipType()
	{
		try
		{
			Element tooltip = base.Tooltip;
			if (tooltip != null && tooltip.IsVisible)
			{
				return ToolTipType.InventoryItem;
			}
			ItemOnGroundTooltip toolTipOnGround = ToolTipOnGround;
			if (toolTipOnGround != null && toolTipOnGround.Tooltip != null)
			{
				tooltip = toolTipOnGround.TooltipUI;
				if (tooltip != null && tooltip.IsVisible)
				{
					return ToolTipType.ItemOnGround;
				}
			}
			tooltip = base.Tooltip;
			if (tooltip != null && tooltip.IsVisible && tooltip.ChildCount > 1 && base.Tooltip.GetChildAtIndex(0).IsVisible && base.Tooltip.GetChildAtIndex(1).IsVisible)
			{
				return ToolTipType.ItemInChat;
			}
		}
		catch (Exception value)
		{
			Core.Logger?.Error($"HoverItemIcon.cs -> {value}");
		}
		return ToolTipType.None;
	}
}
