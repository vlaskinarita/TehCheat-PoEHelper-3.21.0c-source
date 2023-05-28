using ExileCore.Shared.Attributes;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace Radar;

[Submenu]
public class PathfindingSettings
{
	public ToggleNode ShowPathsToTargetsOnMap { get; set; } = new ToggleNode(value: true);


	public ColorNode DefaultMapPathColor { get; set; } = new ColorNode(Color.Green);


	public ToggleNode UseRainbowColorsForMapPaths { get; set; } = new ToggleNode(value: true);


	public ToggleNode ShowAllTargets { get; set; } = new ToggleNode(value: false);


	public ToggleNode ShowSelectedTargets { get; set; } = new ToggleNode(value: true);


	public ToggleNode EnableTargetNameBackground { get; set; } = new ToggleNode(value: true);


	public ColorNode TargetNameColor { get; set; } = new ColorNode(Color.Violet);


	public WorldPathSettings WorldPathSettings { get; set; } = new WorldPathSettings();

}
