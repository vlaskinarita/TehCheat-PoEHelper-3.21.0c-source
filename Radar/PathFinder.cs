using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GameOffsets.Native;

namespace Radar;

public class PathFinder
{
	private readonly bool[][] _grid;

	private readonly ConcurrentDictionary<Vector2i, Dictionary<Vector2i, float>> ExactDistanceField = new ConcurrentDictionary<Vector2i, Dictionary<Vector2i, float>>();

	private readonly int _dimension2;

	private readonly int _dimension1;

	private static readonly IReadOnlyList<Vector2i> NeighborOffsets = new List<Vector2i>
	{
		new Vector2i(0, 1),
		new Vector2i(1, 1),
		new Vector2i(1, 0),
		new Vector2i(1, -1),
		new Vector2i(0, -1),
		new Vector2i(-1, -1),
		new Vector2i(-1, 0),
		new Vector2i(-1, 1)
	};

	public PathFinder(int[][] grid, int[] pathableValues)
	{
		HashSet<int> pv = pathableValues.ToHashSet();
		_grid = grid.Select((int[] x) => x.Select((int y) => pv.Contains(y)).ToArray()).ToArray();
		_dimension1 = _grid.Length;
		_dimension2 = _grid[0].Length;
	}

	private bool IsTilePathable(Vector2i tile)
	{
		if (tile.X < 0 || tile.X >= _dimension2)
		{
			return false;
		}
		if (tile.Y < 0 || tile.Y >= _dimension1)
		{
			return false;
		}
		return _grid[tile.Y][tile.X];
	}

	private static IEnumerable<Vector2i> GetNeighbors(Vector2i tile)
	{
		return NeighborOffsets.Select((Vector2i offset) => tile + offset);
	}

	private static float GetExactDistance(Vector2i tile, Dictionary<Vector2i, float> dict)
	{
		return dict.GetValueOrDefault(tile, float.PositiveInfinity);
	}

	public IEnumerable<List<Vector2i>> RunFirstScan(Vector2i start, Vector2i target)
	{
		ExactDistanceField.TryAdd(target, new Dictionary<Vector2i, float>());
		Dictionary<Vector2i, float> exactDistanceField = ExactDistanceField[target];
		exactDistanceField[target] = 0f;
		Dictionary<Vector2i, Vector2i> localBacktrackDictionary = new Dictionary<Vector2i, Vector2i>();
		BinaryHeap<float, Vector2i> queue = new BinaryHeap<float, Vector2i>();
		queue.Add(0f, target);
		Stopwatch sw = Stopwatch.StartNew();
		localBacktrackDictionary.Add(target, target);
		List<Vector2i> reversePath = new List<Vector2i>();
		KeyValuePair<float, Vector2i> top;
		while (queue.TryRemoveTop(out top))
		{
			Vector2i current = top.Value;
			float currentDistance = top.Key;
			if (reversePath.Count == 0 && current.Equals(start))
			{
				reversePath.Add(current);
				Vector2i it = current;
				Vector2i previous2;
				while (it != target && localBacktrackDictionary.TryGetValue(it, out previous2))
				{
					reversePath.Add(previous2);
					it = previous2;
				}
				yield return reversePath;
			}
			foreach (Vector2i neighbor in GetNeighbors(current))
			{
				TryEnqueueTile(neighbor, current, currentDistance);
			}
			if (sw.ElapsedMilliseconds > 100)
			{
				yield return reversePath;
				sw.Restart();
			}
		}
		void TryEnqueueTile(Vector2i coord, Vector2i previous, float previousScore)
		{
			if (IsTilePathable(coord) && !localBacktrackDictionary.ContainsKey(coord))
			{
				localBacktrackDictionary.Add(coord, previous);
				float exactDistance = previousScore + coord.DistanceF(previous);
				exactDistanceField.TryAdd(coord, exactDistance);
				queue.Add(exactDistance, coord);
			}
		}
	}

	public List<Vector2i> FindPath(Vector2i start, Vector2i target)
	{
		Dictionary<Vector2i, float> exactDistanceField = ExactDistanceField[target];
		if (float.IsPositiveInfinity(GetExactDistance(start, exactDistanceField)))
		{
			return null;
		}
		List<Vector2i> path = new List<Vector2i>();
		Vector2i current = start;
		while (current != target)
		{
			Vector2i next = GetNeighbors(current).MinBy((Vector2i x) => GetExactDistance(x, exactDistanceField));
			path.Add(next);
			current = next;
		}
		return path;
	}
}
