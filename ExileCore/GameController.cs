using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace ExileCore;

public class GameController : IDisposable
{
	private readonly CoreSettings _settings;

	private static readonly DebugInformation DebClearCache = new DebugInformation("Clear cache", main: false);

	private readonly DebugInformation debDeltaTime;

	private readonly TimeCache<Vector2> LeftCornerMap;

	private readonly TimeCache<Vector2> UnderCornerMap;

	private bool IsForeGroundLast;

	private bool WasInGame;

	public PluginBridge PluginBridge;

	private Stopwatch sw { get; } = Stopwatch.StartNew();


	public long ElapsedMs => sw.ElapsedMilliseconds;

	public TheGame Game { get; }

	public AreaController Area { get; }

	public GameWindow Window { get; }

	public IngameState IngameState => Game.IngameState;

	public FilesContainer Files => Game.Files;

	public Entity Player => EntityListWrapper.Player;

	public bool IsForeGroundCache { get; set; }

	public bool InGame { get; private set; }

	public bool IsLoading { get; private set; }

	public PluginPanel LeftPanel { get; }

	public PluginPanel UnderPanel { get; }

	public IMemory Memory { get; }

	public SoundController SoundController { get; }

	public SettingsContainer Settings { get; }

	public MultiThreadManager MultiThreadManager { get; }

	public EntityListWrapper EntityListWrapper { get; }

	public Cache Cache { get; set; }

	public double DeltaTime => debDeltaTime.Tick;

	public bool Initialized { get; }

	public ICollection<Entity> Entities => EntityListWrapper.Entities;

	public Dictionary<string, object> Debug { get; } = new Dictionary<string, object>();


	public static event Action<bool> eIsForegroundChanged;

	public GameController(Memory memory, SoundController soundController, SettingsContainer settings, MultiThreadManager multiThreadManager)
	{
		_settings = settings.CoreSettings;
		Memory = memory;
		SoundController = soundController;
		Settings = settings;
		MultiThreadManager = multiThreadManager;
		try
		{
			Cache = new Cache();
			Game = new TheGame(memory, Cache, settings.CoreSettings);
			Area = new AreaController(Game);
			Window = new GameWindow(memory.Process);
			WasInGame = Game.InGame;
			EntityListWrapper = new EntityListWrapper(this, _settings, multiThreadManager);
		}
		catch (Exception ex)
		{
			DebugWindow.LogError(ex.ToString());
		}
		PluginBridge = new PluginBridge();
		IsForeGroundCache = WinApi.IsForegroundWindow(Window.Process.MainWindowHandle);
		LeftPanel = new PluginPanel(GetLeftCornerMap());
		UnderPanel = new PluginPanel(GetUnderCornerMap());
		debDeltaTime = Core.DebugInformations.FirstOrDefault((DebugInformation x) => x.Name == "Delta Time");
		LeftCornerMap = new TimeCache<Vector2>(GetLeftCornerMap, 500L);
		UnderCornerMap = new TimeCache<Vector2>(GetUnderCornerMap, 500L);
		eIsForegroundChanged += delegate(bool b)
		{
			if (b)
			{
				Core.MainRunner.ResumeCoroutines(Core.MainRunner.Coroutines);
				Core.ParallelRunner.ResumeCoroutines(Core.ParallelRunner.Coroutines);
			}
			else
			{
				Core.MainRunner.PauseCoroutines(Core.MainRunner.Coroutines);
				Core.ParallelRunner.PauseCoroutines(Core.ParallelRunner.Coroutines);
			}
		};
		ButtonNode refreshArea = _settings.RefreshArea;
		refreshArea.OnPressed = (Action)Delegate.Combine(refreshArea.OnPressed, (Action)delegate
		{
			Area.ForceRefreshArea(areaChangeMultiThread: false);
		});
		ButtonNode reloadFiles = _settings.ReloadFiles;
		reloadFiles.OnPressed = (Action)Delegate.Combine(reloadFiles.OnPressed, new Action(Game.ReloadFiles));
		Area.RefreshState();
		EntityListWrapper.StartWork();
		Initialized = true;
	}

	public void Dispose()
	{
		Memory?.Dispose();
	}

	public void Tick()
	{
		try
		{
			if (IsForeGroundLast != IsForeGroundCache)
			{
				IsForeGroundLast = IsForeGroundCache;
				GameController.eIsForegroundChanged(IsForeGroundCache);
			}
			AreaInstance.CurrentHash = Game.CurrentAreaHash;
			if (LeftPanel.Used)
			{
				LeftPanel.StartDrawPoint = LeftCornerMap.Value;
			}
			if (UnderPanel.Used)
			{
				UnderPanel.StartDrawPoint = UnderCornerMap.Value;
			}
			if (Core.FramesCount % 3u == 0 && Area.RefreshState())
			{
				DebClearCache.TickAction(delegate
				{
					RemoteMemoryObject.Cache.TryClearCache();
				});
			}
			InGame = Game.InGame;
			IsLoading = Game.IsLoading;
			if (InGame)
			{
				if (!WasInGame)
				{
					Game.ReloadFiles();
					Game.IngameState.UpdateData();
					WasInGame = true;
				}
				CachedValue.Latency = Game.IngameState.ServerData.Latency;
			}
		}
		catch (Exception ex)
		{
			DebugWindow.LogError(ex.ToString());
		}
	}

	public Vector2 GetLeftCornerMap()
	{
		if (!InGame)
		{
			return Vector2.Zero;
		}
		IngameState ingameState = Game.IngameState;
		RectangleF getClientRectCache = ingameState.IngameUi.Map.SmallMiniMap.GetClientRectCache;
		Element mapSideUI = ingameState.IngameUi.MapSideUI;
		switch (Game.DiagnosticInfoType)
		{
		case DiagnosticInfoType.Off:
			if (mapSideUI != null && mapSideUI.IsVisibleLocal)
			{
				getClientRectCache.X -= mapSideUI.GetClientRectCache.Width;
			}
			break;
		case DiagnosticInfoType.Short:
			getClientRectCache.X -= 265f;
			break;
		case DiagnosticInfoType.Full:
			if (mapSideUI != null && mapSideUI.IsVisibleLocal)
			{
				getClientRectCache.X -= mapSideUI.GetClientRectCache.Width;
			}
			getClientRectCache.Y += 175f;
			break;
		}
		return new Vector2(getClientRectCache.X, getClientRectCache.Y);
	}

	private Vector2 GetUnderCornerMap()
	{
		if (!InGame)
		{
			return Vector2.Zero;
		}
		RectangleF getClientRectCache = Game.IngameState.IngameUi.GemLvlUpPanel.Parent.GetClientRectCache;
		return new Vector2(getClientRectCache.X + getClientRectCache.Width, getClientRectCache.Y + getClientRectCache.Height);
	}

	static GameController()
	{
		GameController.eIsForegroundChanged = delegate
		{
		};
	}
}
