using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using JM.LinqFaster;
using MoreLinq.Extensions;
using SharpDX;

namespace ExileCore.Shared;

public class PluginManager
{
	private record LoadedAssembly(Assembly Assembly, string PathOnDisk);

	private const string PluginsDirectory = "Plugins";

	private const string CompiledPluginsDirectory = "Compiled";

	private const string SourcePluginsDirectory = "Source";

	private const string TempPluginsDirectory = "Temp";

	private readonly GameController _gameController;

	private readonly Graphics _graphics;

	private readonly MultiThreadManager _multiThreadManager;

	private readonly bool _parallelLoading;

	private readonly Dictionary<string, string> _directories = new Dictionary<string, string>();

	private readonly ConcurrentDictionary<string, DateTime> _lastAssemblyLoadTime = new ConcurrentDictionary<string, DateTime>();

	private readonly object _locker = new object();

	public bool AllPluginsLoaded { get; }

	public string RootDirectory { get; }

	public List<PluginWrapper> Plugins { get; } = new List<PluginWrapper>();


	public ConcurrentDictionary<string, string> FailedSourcePlugins { get; } = new ConcurrentDictionary<string, string>();


	public PluginManager(GameController gameController, Graphics graphics, MultiThreadManager multiThreadManager)
	{
		PluginManager pluginManager = this;
		_gameController = gameController;
		_graphics = graphics;
		_multiThreadManager = multiThreadManager;
		RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
		_directories["Temp"] = Path.Combine(RootDirectory, "Plugins", "Temp");
		_directories["Plugins"] = Path.Combine(RootDirectory, "Plugins");
		_directories["Compiled"] = Path.Combine(_directories["Plugins"], "Compiled");
		_directories["Source"] = Path.Combine(_directories["Plugins"], "Source");
		_gameController.EntityListWrapper.EntityAdded += EntityListWrapperOnEntityAdded;
		_gameController.EntityListWrapper.EntityRemoved += EntityListWrapperOnEntityRemoved;
		_gameController.EntityListWrapper.EntityAddedAny += EntityListWrapperOnEntityAddedAny;
		_gameController.EntityListWrapper.EntityIgnored += EntityListWrapperOnEntityIgnored;
		_gameController.Area.OnAreaChange += OnAreaChange;
		_parallelLoading = _gameController.Settings.CoreSettings.PluginSettings.MultiThreadLoadPlugins;
		foreach (KeyValuePair<string, string> directory in _directories)
		{
			Directory.CreateDirectory(directory.Value);
		}
		(DirectoryInfo[], DirectoryInfo[]) tuple = SearchPlugins();
		DirectoryInfo[] item = tuple.Item1;
		DirectoryInfo[] sourcePlugins = tuple.Item2;
		Task task = null;
		if (sourcePlugins.Length != 0)
		{
			task = Task.Run(delegate
			{
				pluginManager.LoadPluginsFromSource(sourcePlugins);
			});
		}
		LoadCompiledPlugins(item, _parallelLoading);
		task?.Wait();
		Plugins = (from x in Plugins
			orderby x.Order, x.CanBeMultiThreading descending, x.Name
			select x).ToList();
		PluginWrapper pluginWrapper = Plugins.FirstOrDefault((PluginWrapper x) => x.Name.Equals("DevTree"));
		if (pluginWrapper != null)
		{
			try
			{
				pluginWrapper.Plugin.GetType().GetField("Plugins").SetValue(pluginWrapper.Plugin, new Func<List<PluginWrapper>>(devTreePlugins));
			}
			catch (Exception ex)
			{
				LogError(ex.ToString());
			}
		}
		if (_parallelLoading)
		{
			_ = gameController.IngameState.IngameUi;
			_ = gameController.IngameState.Data;
			_ = gameController.IngameState.ServerData;
			Parallel.ForEach(Plugins, delegate(PluginWrapper wrapper)
			{
				wrapper.Initialise(gameController);
			});
		}
		else
		{
			Plugins.ForEach(delegate(PluginWrapper wrapper)
			{
				wrapper.Initialise(gameController);
			});
		}
		OnAreaChange(gameController.Area.CurrentArea);
		Enumerable.DistinctBy(Plugins, (PluginWrapper x) => x.PathOnDisk).ForEach(delegate(PluginWrapper x)
		{
			x.SubscrideOnFile(pluginManager.ReloadChangedDll);
		});
		AllPluginsLoaded = true;
		List<PluginWrapper> devTreePlugins()
		{
			return pluginManager.Plugins;
		}
	}

	private void LoadCompiledPlugins(DirectoryInfo[] compiledPlugins, bool parallel)
	{
		if (parallel)
		{
			Parallel.ForEach(compiledPlugins, Load);
		}
		else
		{
			compiledPlugins.ForEach(Load);
		}
		void Load(DirectoryInfo info)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			string name = info.Name;
			List<string> list = new List<string>();
			if (name.EndsWith("-master"))
			{
				string text = name;
				int length = "-master".Length;
				list.Add(text.Substring(0, text.Length - length));
			}
			LoadedAssembly loadedAssembly = LoadAssembly(info, list);
			if (loadedAssembly != null)
			{
				List<PluginWrapper> collection = TryLoadPlugins(loadedAssembly);
				DebugWindow.LogMsg($"Plugins from directory {info.Name} loaded in {stopwatch.ElapsedMilliseconds} ms.", 1f, Color.Orange);
				lock (_locker)
				{
					Plugins.AddRange(collection);
				}
			}
		}
	}

	private LoadedAssembly LoadAssembly(DirectoryInfo dir, IEnumerable<string> suggestedDllNames)
	{
		try
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(dir.FullName);
			if (!directoryInfo.Exists)
			{
				LogError($"Directory - {dir} not found.");
				return null;
			}
			FileInfo fileInfo = directoryInfo.GetFiles(directoryInfo.Name + "*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault();
			List<string> list = suggestedDllNames.Prepend(directoryInfo.Name).ToList();
			if (fileInfo == null)
			{
				FileInfo[] files = directoryInfo.GetFiles("*.dll", SearchOption.TopDirectoryOnly);
				if (files.Length == 1)
				{
					fileInfo = files.First();
				}
				else
				{
					List<(FileInfo file, string fileName, string noSpaceFileName)> allDllsWithNames = files.Select((FileInfo x) => (x, Path.GetFileNameWithoutExtension(x.Name), Path.GetFileNameWithoutExtension(x.Name).Replace(" ", null))).ToList();
					List<(FileInfo, string, string)> list2 = list.Where((string x) => !string.IsNullOrWhiteSpace(x)).SelectMany((string targetName) => allDllsWithNames.Where(((FileInfo file, string fileName, string noSpaceFileName) x) => x.fileName.Equals(targetName, StringComparison.InvariantCultureIgnoreCase) || x.noSpaceFileName.Equals(targetName.Replace(" ", null), StringComparison.InvariantCultureIgnoreCase))).Distinct()
						.ToList();
					if (list2.Count == 1)
					{
						fileInfo = list2.First().Item1;
					}
				}
			}
			if (fileInfo == null)
			{
				LogError("Unable to find plugin dll in " + dir.FullName + ". Looked for names similar to " + string.Join(", ", list));
				return null;
			}
			return LoadAssembly(fileInfo);
		}
		catch (Exception value)
		{
			LogError($"{"LoadAssembly"} -> {value}");
			return null;
		}
	}

	private LoadedAssembly LoadAssembly(FileInfo dll)
	{
		try
		{
			if (dll == null || !dll.Exists)
			{
				return null;
			}
			PluginAssemblyLoadContext pluginAssemblyLoadContext = new PluginAssemblyLoadContext(dll.FullName, _gameController.Settings.CoreSettings.PluginSettings.AvoidLockingDllFiles);
			if ((bool)_gameController.Settings.CoreSettings.PluginSettings.AvoidLockingDllFiles)
			{
				using (FileStream assembly = File.OpenRead(dll.FullName))
				{
					string fullName = dll.FullName;
					int length = ".exe".Length;
					string path = fullName.Substring(0, fullName.Length - length) + ".pdb";
					using FileStream assemblySymbols = (File.Exists(path) ? File.OpenRead(path) : null);
					return new LoadedAssembly(pluginAssemblyLoadContext.LoadFromStream(assembly, assemblySymbols), dll.FullName);
				}
			}
			return new LoadedAssembly(pluginAssemblyLoadContext.LoadFromAssemblyPath(dll.FullName), dll.FullName);
		}
		catch (Exception value)
		{
			LogError($"{"LoadAssembly"} -> {value}");
			return null;
		}
	}

	private LoadedAssembly CompileAndLoadPluginAssembly(DirectoryInfo info, PluginCompiler compiler)
	{
		FileInfo fileInfo = info.GetFiles("*.csproj", SearchOption.AllDirectories).FirstOrDefault((FileInfo f) => !f.Name.Contains("test", StringComparison.OrdinalIgnoreCase) && !f.Name.Contains("_tmp", StringComparison.OrdinalIgnoreCase));
		if (fileInfo == null)
		{
			DebugWindow.LogError("Plugin " + info.Name + " will not be compiled because there are no csproj files in the top-level directory");
			return null;
		}
		string text = Path.Join(_directories["Temp"], info.Name);
		try
		{
			compiler.CompilePlugin(fileInfo, text);
		}
		catch (Exception ex)
		{
			File.WriteAllText(Path.Join(info.FullName, "Errors.txt"), ex.Message);
			Logger.Log.Error(ex, "Compilation of " + info.Name + " failed");
			FailedSourcePlugins[info.FullName] = ex.Message;
			return null;
		}
		return LoadAssembly(new DirectoryInfo(text), new string[1] { Path.GetFileNameWithoutExtension(fileInfo.Name) });
	}

	private void LoadPluginsFromSource(IEnumerable<DirectoryInfo> sourcePlugins)
	{
		PluginCompiler compiler;
		using (new PerformanceTimer("Compile and load source plugins"))
		{
			compiler = PluginCompiler.Create();
			try
			{
				if (compiler == null)
				{
					LogError("Plugin compilation is disabled");
				}
				else if (_parallelLoading)
				{
					Parallel.ForEach(sourcePlugins, CompileAndLoadPlugin);
				}
				else
				{
					sourcePlugins.ForEach(CompileAndLoadPlugin);
				}
			}
			finally
			{
				if (compiler != null)
				{
					((IDisposable)compiler).Dispose();
				}
			}
		}
		void CompileAndLoadPlugin(DirectoryInfo directoryInfo)
		{
			using (new PerformanceTimer("Compile and load source plugin: " + directoryInfo.Name))
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				LoadedAssembly loadedAssembly = CompileAndLoadPluginAssembly(directoryInfo, compiler);
				if (loadedAssembly != null)
				{
					List<PluginWrapper> collection = TryLoadPlugins(loadedAssembly);
					DebugWindow.LogMsg($"Plugins from directory {directoryInfo.Name} compiled and loaded in {stopwatch.ElapsedMilliseconds} ms.", 1f, Color.Orange);
					lock (_locker)
					{
						Plugins.AddRange(collection);
						return;
					}
				}
			}
		}
	}

	public void LoadFailedSourcePlugin(string path)
	{
		FailedSourcePlugins.Remove(path, out var _);
		LoadPluginsFromSource(new DirectoryInfo[1]
		{
			new DirectoryInfo(path)
		});
	}

	private List<PluginWrapper> TryLoadPlugins(LoadedAssembly asm)
	{
		List<PluginWrapper> list = new List<PluginWrapper>();
		try
		{
			DirectoryInfo directory = new FileInfo(asm.PathOnDisk).Directory;
			string fullName = directory.FullName;
			Type[] types = asm.Assembly.GetTypes();
			if (types.Length == 0)
			{
				LogError("Not found any types in plugin " + asm.PathOnDisk);
				return list;
			}
			Type[] array = types.WhereF((Type type) => typeof(IPlugin).IsAssignableFrom(type) && !type.IsAbstract);
			if (types.FirstOrDefaultF((Type type) => typeof(ISettings).IsAssignableFrom(type)) == null)
			{
				LogError("Not found setting class");
				return list;
			}
			Type[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				if (Activator.CreateInstance(array2[i]) is IPlugin plugin)
				{
					plugin.DirectoryName = directory.Name;
					plugin.DirectoryFullName = fullName;
					PluginWrapper pluginWrapper = new PluginWrapper(plugin, asm.PathOnDisk);
					pluginWrapper.SetApi(_gameController, _graphics, this);
					pluginWrapper.LoadSettings();
					pluginWrapper.Onload();
					list.Add(pluginWrapper);
				}
			}
			return list;
		}
		catch (Exception value)
		{
			LogError($"Error when load plugin ({asm.Assembly.ManifestModule.ScopeName}): {value})");
			return list;
		}
	}

	private void ReloadChangedDll(PluginWrapper wrapper, FileSystemEventArgs args)
	{
		try
		{
			if ((args.ChangeType & WatcherChangeTypes.Deleted) != 0)
			{
				return;
			}
			string fullPath = args.FullPath;
			if (!(fullPath != wrapper.PathOnDisk))
			{
				DateTime valueOrDefault = _lastAssemblyLoadTime.GetValueOrDefault(fullPath, DateTime.MinValue);
				if (!(DateTime.UtcNow - valueOrDefault < TimeSpan.FromSeconds(2.0)))
				{
					_lastAssemblyLoadTime[fullPath] = DateTime.UtcNow;
					ReloadPluginDll(fullPath);
				}
			}
		}
		catch (Exception value)
		{
			DebugWindow.LogError($"HotReload error: {value}");
		}
	}

	private void ReloadPluginDll(string fullPath)
	{
		string pluginName = Path.GetFileNameWithoutExtension(fullPath);
		Core.MainRunner.Run(new Coroutine(delegate
		{
			FileInfo dll = new FileInfo(fullPath);
			LoadedAssembly loadedAssembly = LoadAssembly(dll);
			if (loadedAssembly == null)
			{
				LogError(pluginName + " cant load assembly for reloading.");
			}
			else
			{
				List<PluginWrapper> list = TryLoadPlugins(loadedAssembly);
				if (list.Any())
				{
					foreach (PluginWrapper item in Plugins.Where((PluginWrapper x) => x.PathOnDisk == fullPath))
					{
						item.Close();
					}
					lock (_locker)
					{
						Plugins.RemoveAll((PluginWrapper x) => x.PathOnDisk == fullPath);
					}
					foreach (PluginWrapper item2 in list)
					{
						item2.Initialise(_gameController);
						if (item2.IsEnable)
						{
							item2.AreaChange(_gameController.Area.CurrentArea);
						}
						foreach (Entity entity in _gameController.Entities)
						{
							item2.EntityAdded(entity);
						}
					}
					list.First().SubscrideOnFile(ReloadChangedDll);
					lock (_locker)
					{
						Plugins.AddRange(list);
					}
				}
			}
		}, new WaitTime(1000), null, "Reload: " + pluginName, infinity: false)
		{
			SyncModWork = true
		});
	}

	private (DirectoryInfo[] CompiledDirectories, DirectoryInfo[] SourceDirectories) SearchPlugins()
	{
		DirectoryInfo[] array = (from x in new DirectoryInfo(_directories["Compiled"]).GetDirectories()
			where x.EnumerateFiles("*.dll", SearchOption.AllDirectories).Any()
			select x).ToArray();
		DirectoryInfo[] array2 = (from x in new DirectoryInfo(_directories["Source"]).GetDirectories()
			where (x.Attributes & FileAttributes.Hidden) == 0
			select x).ToArray();
		if ((bool)_gameController.Settings.CoreSettings.PluginSettings.PreferSourcePlugins)
		{
			array = array.ExceptBy(array2.Select((DirectoryInfo x) => x.Name), (DirectoryInfo x) => x.Name).ToArray();
		}
		else
		{
			array2 = array2.ExceptBy(array.Select((DirectoryInfo x) => x.Name), (DirectoryInfo x) => x.Name).ToArray();
		}
		return (array, array2);
	}

	public void CloseAllPlugins()
	{
		foreach (PluginWrapper plugin in Plugins)
		{
			plugin.Close();
		}
	}

	private void OnAreaChange(AreaInstance area)
	{
		foreach (PluginWrapper plugin in Plugins)
		{
			if (plugin.IsEnable)
			{
				plugin.AreaChange(area);
			}
		}
	}

	private void EntityListWrapperOnEntityIgnored(Entity entity)
	{
		foreach (PluginWrapper plugin in Plugins)
		{
			if (plugin.IsEnable)
			{
				plugin.EntityIgnored(entity);
			}
		}
	}

	private void EntityListWrapperOnEntityAddedAny(Entity entity)
	{
		foreach (PluginWrapper plugin in Plugins)
		{
			if (plugin.IsEnable)
			{
				plugin.EntityAddedAny(entity);
			}
		}
	}

	private void EntityListWrapperOnEntityAdded(Entity entity)
	{
		if ((bool)_gameController.Settings.CoreSettings.PerformanceSettings.AddedMultiThread && _multiThreadManager.ThreadsCount > 0)
		{
			List<Job> listJob = new List<Job>();
			Plugins.WhereF((PluginWrapper x) => x.IsEnable).Batch(_multiThreadManager.ThreadsCount).ForEach(delegate(IEnumerable<PluginWrapper> wrappers)
			{
				listJob.Add(_multiThreadManager.AddJob(delegate
				{
					wrappers.ForEach(delegate(PluginWrapper x)
					{
						x.EntityAdded(entity);
					});
				}, "Entity added"));
			});
			_multiThreadManager.Process(this);
			SpinWait.SpinUntil(() => listJob.AllF((Job x) => x.IsCompleted), 500);
			return;
		}
		foreach (PluginWrapper plugin in Plugins)
		{
			if (plugin.IsEnable)
			{
				plugin.EntityAdded(entity);
			}
		}
	}

	private void EntityListWrapperOnEntityRemoved(Entity entity)
	{
		foreach (PluginWrapper plugin in Plugins)
		{
			if (plugin.IsEnable)
			{
				plugin.EntityRemoved(entity);
			}
		}
	}

	private void LogError(string msg)
	{
		DebugWindow.LogError(msg, 5f);
	}

	public void ReceivePluginEvent(string eventId, object args, IPlugin owner)
	{
		foreach (PluginWrapper plugin in Plugins)
		{
			if (plugin.IsEnable && plugin.Plugin != owner)
			{
				plugin.ReceiveEvent(eventId, args);
			}
		}
	}
}
