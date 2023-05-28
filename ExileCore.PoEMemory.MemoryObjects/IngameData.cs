using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using GameOffsets;
using GameOffsets.Native;

namespace ExileCore.PoEMemory.MemoryObjects;

public class IngameData : RemoteMemoryObject
{
	public class PortalObject : RemoteMemoryObject
	{
		public const int StructSize = 56;

		public long Id => base.M.Read<long>(base.Address);

		public string PlayerOwner => NativeStringReader.ReadString(base.Address + 8, base.M);

		public WorldArea Area => base.TheGame.Files.WorldAreas.GetAreaByWorldId(base.M.Read<int>(base.Address + 44));

		public override string ToString()
		{
			return PlayerOwner + " => " + Area.Name;
		}
	}

	private const double TileHeightFinalMultiplier = 7.8125;

	private static readonly int EntitiesCountOffset = Extensions.GetOffset((IngameDataOffsets x) => x.EntitiesCount);

	private readonly CachedValue<IngameDataOffsets> _cacheStruct;

	private readonly CachedValue<AreaTemplate> _CurrentArea;

	private readonly CachedValue<WorldArea> _CurrentWorldArea;

	private readonly CachedValue<long> _EntitiesCount;

	private EntityList _EntityList;

	private readonly CachedValue<ServerData> _serverData;

	private readonly CachedValue<Entity> _localPlayer;

	private NativePtrArray cacheStats;

	private Dictionary<GameStat, int> mapStats = new Dictionary<GameStat, int>();

	private readonly CachedValue<float[][]> _terrainHeight;

	private readonly CachedValue<int[][]> _terrainPathfindingData;

	private readonly CachedValue<Vector2i> _areaDimensions;

	public IngameDataOffsets DataStruct => _cacheStruct.Value;

	public long EntitiesCount => _EntitiesCount.Value;

	public AreaTemplate CurrentArea => _CurrentArea.Value;

	public WorldArea CurrentWorldArea => _CurrentWorldArea.Value;

	public int CurrentAreaLevel => _cacheStruct.Value.CurrentAreaLevel;

	public uint CurrentAreaHash => _cacheStruct.Value.CurrentAreaHash;

	public Entity LocalPlayer => _localPlayer.Value;

	public ServerData ServerData => _serverData.Value;

	public long EntiteisTest => DataStruct.EntityList;

	public EntityList EntityList => _EntityList ?? (_EntityList = GetObject<EntityList>(DataStruct.EntityList));

	private long LabDataPtr => _cacheStruct.Value.LabDataPtr;

	public LabyrinthData LabyrinthData
	{
		get
		{
			if (LabDataPtr != 0L)
			{
				return GetObject<LabyrinthData>(LabDataPtr);
			}
			return null;
		}
	}

	public TerrainData Terrain => _cacheStruct.Value.Terrain;

	public Vector2i AreaDimensions => _areaDimensions.Value;

	public int[][] RawPathfindingData => _terrainPathfindingData.Value;

	public float[][] RawTerrainHeightData => _terrainHeight.Value;

	public Dictionary<GameStat, int> MapStats
	{
		get
		{
			if (cacheStats.Equals(_cacheStruct.Value.MapStats))
			{
				return mapStats;
			}
			long first = _cacheStruct.Value.MapStats.First;
			if ((int)(_cacheStruct.Value.MapStats.Last - first) / 8 > 200)
			{
				return null;
			}
			(GameStat, int)[] source = base.M.ReadStdVector<(GameStat, int)>(_cacheStruct.Value.MapStats);
			cacheStats = _cacheStruct.Value.MapStats;
			mapStats = source.ToDictionary(((GameStat stat, int value) x) => x.stat, ((GameStat stat, int value) x) => x.value);
			return mapStats;
		}
	}

	public IList<PortalObject> TownPortals
	{
		get
		{
			long startAddress = base.M.Read<long>(base.Address + 2360);
			long endAddress = base.M.Read<long>(base.Address + 2368);
			return base.M.ReadStructsArray<PortalObject>(startAddress, endAddress, 56, base.TheGame);
		}
	}

	public IngameData()
	{
		_cacheStruct = new AreaCache<IngameDataOffsets>(() => base.M.Read<IngameDataOffsets>(base.Address));
		_serverData = new AreaCache<ServerData>(() => GetObject<ServerData>(_cacheStruct.Value.ServerData));
		_localPlayer = new AreaCache<Entity>(() => GetObject<Entity>(_cacheStruct.Value.LocalPlayer));
		_CurrentArea = new AreaCache<AreaTemplate>(() => GetObject<AreaTemplate>(_cacheStruct.Value.CurrentArea));
		_CurrentWorldArea = new AreaCache<WorldArea>(() => base.TheGame.Files.WorldAreas.GetByAddress(CurrentArea.Address));
		_EntitiesCount = new FrameCache<long>(() => base.M.Read<long>(base.Address + EntitiesCountOffset));
		_terrainHeight = new AreaCache<float[][]>(GetTerrainHeight);
		_terrainPathfindingData = new AreaCache<int[][]>(GetTerrainPathfindingData);
		_areaDimensions = new AreaCache<Vector2i>(() => new Vector2i(Terrain.BytesPerRow * 2, (int)(Terrain.LayerMelee.Size / Terrain.BytesPerRow)));
	}

	public float GetTerrainHeightAt(Vector2 gridPosition)
	{
		return _terrainHeight.Value[(int)gridPosition.Y][(int)gridPosition.X];
	}

	public int GetPathfindingValueAt(Vector2 gridPosition)
	{
		return _terrainPathfindingData.Value[(int)gridPosition.Y][(int)gridPosition.X];
	}

	private int[][] GetTerrainPathfindingData()
	{
		byte[] mapTextureData = base.M.ReadStdVector<byte>(Terrain.LayerMelee);
		int bytesPerRow = Terrain.BytesPerRow;
		int num = mapTextureData.Length / bytesPerRow;
		int[][] processedTerrainData = new int[num][];
		int xSize = bytesPerRow * 2;
		for (int i = 0; i < num; i++)
		{
			processedTerrainData[i] = new int[xSize];
		}
		Parallel.For(0, num, delegate(int y)
		{
			for (int j = 0; j < xSize; j += 2)
			{
				byte b = mapTextureData[y * bytesPerRow + j / 2];
				for (int k = 0; k < 2; k++)
				{
					int num2 = (b >> 4 * k) & 0xF;
					processedTerrainData[y][j + k] = num2;
				}
			}
		});
		return processedTerrainData;
	}

	private float[][] GetTerrainHeight()
	{
		byte[] rotationSelector = base.TheGame.TerrainRotationSelector;
		byte[] rotationHelper = base.TheGame.TerrainRotationHelper;
		TerrainData terrainMetadata = Terrain;
		TileStructure[] tileData = base.M.ReadStdVector<TileStructure>(terrainMetadata.TgtArray);
		Dictionary<long, sbyte[]> tileHeightCache = (from addr in tileData.Select((TileStructure x) => x.SubTileDetailsPtr).Distinct().AsParallel()
			select new
			{
				addr = addr,
				data = base.M.ReadStdVector<sbyte>(base.M.Read<SubTileStructure>(addr).SubTileHeight)
			}).ToDictionary(x => x.addr, x => x.data);
		int gridSizeX = terrainMetadata.NumCols * 23;
		int num = terrainMetadata.NumRows * 23;
		float[][] result = new float[num][];
		Parallel.For(0, num, delegate(int y)
		{
			result[y] = new float[gridSizeX];
			for (int i = 0; i < gridSizeX; i++)
			{
				int num2 = y / 23 * terrainMetadata.NumCols + i / 23;
				if (num2 < 0 || num2 >= tileData.Length)
				{
					DebugWindow.LogError($"Tile data array length is {tileData.Length}, index was {num2}");
					result[y][i] = 0f;
				}
				else
				{
					TileStructure tileStructure = tileData[num2];
					sbyte[] array = tileHeightCache[tileStructure.SubTileDetailsPtr];
					int num3 = 0;
					if (array.Length == 1)
					{
						num3 = array[0];
					}
					else if (array.Length != 0)
					{
						int num4 = i % 23;
						int num5 = y % 23;
						int num6 = 22;
						int[] obj = new int[4]
						{
							num6 - num4,
							num4,
							num6 - num5,
							num5
						};
						int num7 = rotationSelector[tileStructure.RotationSelector] * 3;
						int num8 = rotationHelper[num7];
						int num9 = rotationHelper[num7 + 1];
						int num10 = rotationHelper[num7 + 2];
						int num11 = obj[num8 * 2 + num9];
						int index = obj[num10 + (1 - num8) * 2] * 23 + num11;
						num3 = GetTileHeightFromPackedArray(array, index);
					}
					result[y][i] = 0f - (float)((double)(tileStructure.TileHeight * terrainMetadata.TileHeightMultiplier + num3) * 7.8125);
				}
			}
		});
		return result;
	}

	private unsafe static int GetTileHeightFromPackedArray(sbyte[] tileHeightArray, int index)
	{
		object obj = tileHeightArray.Length switch
		{
			69 => (3, 2, 7, 1, 1, true), 
			137 => (2, 4, 3, 2, 3, true), 
			281 => (1, 16, 1, 4, 15, true), 
			_ => default((int, int, int, int, int, bool)), 
		};
		var (num, num2, num3, num4, num5, _) = ((int, int, int, int, int, bool))obj;
		if (!((ValueTuple<int, int, int, int, int, bool>*)(&obj))->Item6)
		{
			if (index >= 0 && index < tileHeightArray.Length)
			{
				return tileHeightArray[index];
			}
			DebugWindow.LogError($"Tile height array length is {tileHeightArray.Length}, index (0) was {index}");
		}
		int num6 = (index >> num) + num2;
		if (num6 < 0 || num6 >= tileHeightArray.Length)
		{
			DebugWindow.LogError($"Tile height array length is {tileHeightArray.Length}, index (1) was {num6}");
		}
		else
		{
			int num7 = ((byte)tileHeightArray[num6] >> (index & num3) * num4) & num5;
			if (num7 >= 0 && num7 < tileHeightArray.Length)
			{
				return tileHeightArray[num7];
			}
			DebugWindow.LogError($"Tile height array length is {tileHeightArray.Length}, index (2) was {num6}, {num7}");
		}
		return 0;
	}
}
