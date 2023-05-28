using ExileCore.Shared.Attributes;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace Radar;

[Submenu]
public class WorldPathSettings
{
	public ToggleNode ShowPathsToTargets { get; set; } = new ToggleNode(value: true);


	public ToggleNode ShowPathsToTargetsOnlyWithClosedMap { get; set; } = new ToggleNode(value: true);


	public ToggleNode UseRainbowColorsForPaths { get; set; } = new ToggleNode(value: true);


	public ColorNode DefaultPathColor { get; set; } = new ColorNode(Color.Red);


	public ToggleNode OffsetPaths { get; set; } = new ToggleNode(value: true);


	public RangeNode<float> PathThickness { get; set; } = new RangeNode<float>(1f, 1f, 20f);


	public RangeNode<int> DrawEveryNthSegment { get; set; } = new RangeNode<int>(1, 1, 10);

}
