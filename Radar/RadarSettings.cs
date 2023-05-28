using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using Newtonsoft.Json;
using SharpDX;

namespace Radar;

public class RadarSettings : ISettings
{
	[JsonIgnore]
	public ButtonNode Reload { get; set; } = new ButtonNode();


	public ToggleNode Enable { get; set; } = new ToggleNode(value: true);


	public RangeNode<float> CustomScale { get; set; } = new RangeNode<float>(1f, 0.1f, 10f);


	public ToggleNode DrawWalkableMap { get; set; } = new ToggleNode(value: true);


	public ColorNode TerrainColor { get; set; } = new ColorNode(new Color(new Vector3(150f) / 255f));


	public RangeNode<int> MaximumMapTextureDimension { get; set; } = new RangeNode<int>(4096, 100, 4096);


	public RangeNode<int> MaximumPathCount { get; set; } = new RangeNode<int>(1000, 0, 1000);


	public PathfindingSettings PathfindingSettings { get; set; } = new PathfindingSettings();


	public DebugSettings Debug { get; set; } = new DebugSettings();

}
