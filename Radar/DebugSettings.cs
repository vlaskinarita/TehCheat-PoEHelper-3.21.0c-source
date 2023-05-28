using ExileCore.Shared.Attributes;
using ExileCore.Shared.Nodes;

namespace Radar;

[Submenu]
public class DebugSettings
{
	public ToggleNode DrawHeightMap { get; set; } = new ToggleNode(value: false);


	public ToggleNode DisableHeightAdjust { get; set; } = new ToggleNode(value: false);


	public ToggleNode SkipNeighborFill { get; set; } = new ToggleNode(value: false);


	public ToggleNode SkipEdgeDetector { get; set; } = new ToggleNode(value: false);


	public ToggleNode SkipRecoloring { get; set; } = new ToggleNode(value: false);


	public ToggleNode DisableDrawRegionLimiting { get; set; } = new ToggleNode(value: false);


	public ToggleNode IgnoreFullscreenPanels { get; set; } = new ToggleNode(value: false);


	public RangeNode<int> MapCenterOffsetX { get; set; } = new RangeNode<int>(0, -1000, 1000);


	public RangeNode<int> MapCenterOffsetY { get; set; } = new RangeNode<int>(0, -1000, 1000);

}
