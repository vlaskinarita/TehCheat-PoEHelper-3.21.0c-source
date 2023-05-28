using ExileCore.Shared.Attributes;
using ExileCore.Shared.Nodes;

namespace ExileCore;

[Submenu]
public class CorePluginSettings
{
	[Menu("Load plugins in multithread", "When you use a lot plugins that option can help hud faster start. Currently not recommend use it because can be unstable start.")]
	public ToggleNode MultiThreadLoadPlugins { get; set; } = new ToggleNode(value: false);


	[Menu("Avoid locking plugin dlls", "Requires restart to apply. Only enable this if you need to do live dll replacement without restarting the HUD.")]
	public ToggleNode AvoidLockingDllFiles { get; set; } = new ToggleNode(value: false);


	[Menu(null, "Load plugins from source even if there is a compiled plugin with the same name")]
	public ToggleNode PreferSourcePlugins { get; set; } = new ToggleNode(value: false);

}
