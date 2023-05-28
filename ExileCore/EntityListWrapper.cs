using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;

namespace ExileCore;

public class EntityListWrapper
{
	private readonly CoreSettings _settings;

	private readonly int coroutineTimeWait = 100;

	private readonly ConcurrentDictionary<uint, Entity> entityCache;

	private readonly GameController gameController;

	private readonly Queue<uint> keysForDelete = new Queue<uint>(24);

	private readonly Coroutine parallelUpdateDictionary;

	private readonly Stack<Entity> Simple = new Stack<Entity>(512);

	private readonly Coroutine updateEntity;

	private readonly EntityCollectSettingsContainer entityCollectSettingsContainer;

	private static EntityListWrapper _instance;

	private static readonly DebugInformation CollectEntitiesDebug = new DebugInformation("Collect Entities");

	public ICollection<Entity> Entities => entityCache.Values;

	public uint EntitiesVersion { get; }

	public Entity Player { get; private set; }

	public List<Entity> OnlyValidEntities { get; } = new List<Entity>(500);


	public List<Entity> NotOnlyValidEntities { get; } = new List<Entity>(500);


	public Dictionary<uint, Entity> NotValidDict { get; } = new Dictionary<uint, Entity>(500);


	public Dictionary<EntityType, List<Entity>> ValidEntitiesByType { get; }

	public event Action<Entity> EntityAdded;

	public event Action<Entity> EntityAddedAny;

	public event Action<Entity> EntityIgnored;

	public event Action<Entity> EntityRemoved;

	public event EventHandler<Entity> PlayerUpdate;

	public EntityListWrapper(GameController gameController, CoreSettings settings, MultiThreadManager multiThreadManager)
	{
		EntityListWrapper entityListWrapper = this;
		_instance = this;
		this.gameController = gameController;
		_settings = settings;
		entityCache = new ConcurrentDictionary<uint, Entity>();
		gameController.Area.OnAreaChange += AreaChanged;
		EntitiesVersion = 0u;
		updateEntity = new Coroutine(RefreshState, new WaitTime(coroutineTimeWait), null, "Update Entity")
		{
			Priority = CoroutinePriority.High,
			SyncModWork = true
		};
		entityCollectSettingsContainer = new EntityCollectSettingsContainer();
		entityCollectSettingsContainer.Simple = Simple;
		entityCollectSettingsContainer.KeyForDelete = keysForDelete;
		entityCollectSettingsContainer.EntityCache = entityCache;
		entityCollectSettingsContainer.MultiThreadManager = multiThreadManager;
		entityCollectSettingsContainer.ParseEntitiesInMultiThread = () => settings.PerformanceSettings.ParseEntitiesInMultiThread;
		entityCollectSettingsContainer.EntitiesCount = () => gameController.IngameState.Data.EntitiesCount;
		entityCollectSettingsContainer.EntitiesVersion = EntitiesVersion;
		entityCollectSettingsContainer.CollectEntitiesInParallelWhenMoreThanX = settings.PerformanceSettings.CollectEntitiesInParallelWhenMoreThanX;
		entityCollectSettingsContainer.DebugInformation = CollectEntitiesDebug;
		parallelUpdateDictionary = new Coroutine(Test(), null, "Collect entites")
		{
			SyncModWork = true
		};
		UpdateCondition(1000 / (int)settings.PerformanceSettings.EntitiesFps);
		settings.PerformanceSettings.EntitiesFps.OnValueChanged += delegate(object? sender, int i)
		{
			entityListWrapper.UpdateCondition(1000 / i);
		};
		Array enumValues = typeof(EntityType).GetEnumValues();
		ValidEntitiesByType = new Dictionary<EntityType, List<Entity>>(enumValues.Length);
		foreach (EntityType item in enumValues)
		{
			ValidEntitiesByType[item] = new List<Entity>(8);
		}
		PlayerUpdate += delegate(object? sender, Entity entity)
		{
			Entity.Player = entity;
		};
		IEnumerator Test()
		{
			while (true)
			{
				yield return gameController.IngameState.Data.EntityList.CollectEntities(entityListWrapper.entityCollectSettingsContainer);
				yield return new WaitTime(1000 / (int)settings.PerformanceSettings.EntitiesFps);
				entityListWrapper.parallelUpdateDictionary.UpdateTicks((uint)(entityListWrapper.parallelUpdateDictionary.Ticks + 1));
			}
		}
	}

	public void StartWork()
	{
		Core.MainRunner.Run(updateEntity);
		Core.ParallelRunner.Run(parallelUpdateDictionary);
	}

	private void UpdateCondition(int coroutineTimeWait = 50)
	{
		parallelUpdateDictionary.UpdateCondtion(new WaitTime(coroutineTimeWait));
		updateEntity.UpdateCondtion(new WaitTime(coroutineTimeWait));
	}

	private void AreaChanged(AreaInstance area)
	{
		try
		{
			entityCollectSettingsContainer.Break = true;
			Entity localPlayer = gameController.Game.IngameState.Data.LocalPlayer;
			if (Player == null)
			{
				if (localPlayer != null && localPlayer.Path != null && localPlayer.Path.StartsWith("Meta"))
				{
					Player = localPlayer;
					Player.IsValid = true;
					this.PlayerUpdate?.Invoke(this, Player);
				}
			}
			else if (Player.Address != localPlayer.Address && localPlayer.Path.StartsWith("Meta"))
			{
				Player = localPlayer;
				Player.IsValid = true;
				this.PlayerUpdate?.Invoke(this, Player);
			}
			entityCache.Clear();
			OnlyValidEntities.Clear();
			NotOnlyValidEntities.Clear();
			foreach (KeyValuePair<EntityType, List<Entity>> item in ValidEntitiesByType)
			{
				item.Value.Clear();
			}
		}
		catch (Exception value)
		{
			DebugWindow.LogError($"{"EntityListWrapper"} -> {value}");
		}
	}

	private void UpdateEntityCollections()
	{
		OnlyValidEntities.Clear();
		NotOnlyValidEntities.Clear();
		NotValidDict.Clear();
		foreach (KeyValuePair<EntityType, List<Entity>> item in ValidEntitiesByType)
		{
			item.Value.Clear();
		}
		while (keysForDelete.Count > 0)
		{
			uint key = keysForDelete.Dequeue();
			if (entityCache.TryGetValue(key, out var value))
			{
				this.EntityRemoved?.Invoke(value);
				entityCache.TryRemove(key, out var _);
			}
		}
		foreach (KeyValuePair<uint, Entity> item2 in entityCache)
		{
			Entity value3 = item2.Value;
			if (value3.IsValid)
			{
				OnlyValidEntities.Add(value3);
				ValidEntitiesByType[value3.Type].Add(value3);
			}
			else
			{
				NotOnlyValidEntities.Add(value3);
				NotValidDict[value3.Id] = value3;
			}
		}
	}

	public void RefreshState()
	{
		if (gameController.Area.CurrentArea == null || entityCollectSettingsContainer.NeedUpdate || Player == null || !Player.IsValid)
		{
			return;
		}
		while (Simple.Count > 0)
		{
			Entity entity = Simple.Pop();
			if (entity == null)
			{
				DebugWindow.LogError("EntityListWrapper.RefreshState entity is null. (Very strange).");
				continue;
			}
			uint id = entity.Id;
			if (!entityCache.TryGetValue(id, out var _) && (id < int.MaxValue || (bool)_settings.PerformanceSettings.ParseServerEntities) && entity.Type != 0 && (entity.League != LeagueType.Legion || entity.Stats != null))
			{
				this.EntityAddedAny?.Invoke(entity);
				if (entity.Type >= EntityType.Monster)
				{
					this.EntityAdded?.Invoke(entity);
				}
				entityCache[id] = entity;
			}
		}
		UpdateEntityCollections();
		entityCollectSettingsContainer.NeedUpdate = true;
	}

	public static Entity GetEntityById(uint id)
	{
		if (!_instance.entityCache.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public string GetLabelForEntity(Entity entity)
	{
		HashSet<long> hashSet = new HashSet<long>();
		long num = gameController.Game.IngameState.EntityLabelMap;
		while (true)
		{
			hashSet.Add(num);
			if (gameController.Memory.Read<long>(num + 16) == entity.Address)
			{
				break;
			}
			num = gameController.Memory.Read<long>(num);
			if (hashSet.Contains(num) || num == 0L || num == -1)
			{
				return null;
			}
		}
		long num2 = gameController.Memory.Read<long>(num + 24, new int[1] { 448 });
		return gameController.Game.ReadObject<EntityLabel>(num2 + 744).Text;
	}
}
