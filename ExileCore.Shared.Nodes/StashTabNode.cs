using ExileCore.Shared.Enums;
using Newtonsoft.Json;

namespace ExileCore.Shared.Nodes;

public class StashTabNode
{
	public const string EMPTYNAME = "-NoName-";

	public string Name { get; set; } = "-NoName-";


	public int VisibleIndex { get; set; } = -1;


	[JsonIgnore]
	public bool Exist { get; set; }

	[JsonIgnore]
	internal int Id { get; set; } = -1;


	[JsonIgnore]
	public bool IsRemoveOnly { get; set; }

	public StashTabNode()
	{
	}

	public StashTabNode(string name, int visibleIndex, int id, InventoryTabFlags flag)
	{
		Name = name;
		VisibleIndex = visibleIndex;
		Id = id;
		IsRemoveOnly = (flag & InventoryTabFlags.RemoveOnly) == InventoryTabFlags.RemoveOnly;
	}
}
