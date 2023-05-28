using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar;

public static class KMeans
{
	public static int[] Cluster(Vector2d[] rawData, int numClusters)
	{
		if (numClusters >= rawData.Length)
		{
			return Enumerable.Range(0, rawData.Length).ToArray();
		}
		bool clusteringUpdated = true;
		bool meansUpdated = true;
		int[] clustering = InitClustering(rawData, numClusters);
		Vector2d[] means = new Vector2d[numClusters];
		int num = rawData.Length * 10;
		int index = 0;
		while (clusteringUpdated && meansUpdated && index < num)
		{
			meansUpdated = UpdateMeans(rawData, clustering, means);
			clusteringUpdated = UpdateClustering(rawData, clustering, means);
			index++;
		}
		return clustering;
	}

	private static int[] InitClustering(Vector2d[] data, int numClusters)
	{
		HashSet<int> selectedClusters = new HashSet<int> { 0 };
		while (selectedClusters.Count < numClusters)
		{
			(Vector2d, int) newPointIndex = (from x in data.Select((Vector2d tuple, int index) => (tuple, index))
				where !selectedClusters.Contains(x.index)
				select x).MaxBy<(Vector2d, int), double>(((Vector2d tuple, int index) c) => selectedClusters.Min((int x) => Distance(c.tuple, data[x])));
			selectedClusters.Add(newPointIndex.Item2);
		}
		Dictionary<int, int> clusterNumbers = selectedClusters.Select((int x, int i) => (x, i)).ToDictionary<(int, int), int, int>(((int x, int i) x) => x.x, ((int x, int i) x) => x.i);
		return data.Select((Vector2d x) => clusterNumbers[selectedClusters.MinBy((int y) => Distance(x, data[y]))]).ToArray();
	}

	private static bool UpdateMeans(Vector2d[] data, int[] clustering, Vector2d[] means)
	{
		int length = means.Length;
		int[] numArray = new int[length];
		for (int index1 = 0; index1 < data.Length; index1++)
		{
			int index2 = clustering[index1];
			numArray[index2]++;
		}
		for (int l = 0; l < length; l++)
		{
			if (numArray[l] == 0)
			{
				return false;
			}
		}
		for (int k = 0; k < means.Length; k++)
		{
			means[k] = default(Vector2d);
		}
		for (int j = 0; j < data.Length; j++)
		{
			means[clustering[j]] += data[j];
		}
		for (int i = 0; i < means.Length; i++)
		{
			means[i] /= (double)numArray[i];
		}
		return true;
	}

	private static bool UpdateClustering(Vector2d[] data, int[] clustering, Vector2d[] means)
	{
		int length = means.Length;
		bool didUpdate = false;
		int[] clusteringCopy = new int[clustering.Length];
		Array.Copy(clustering, clusteringCopy, clustering.Length);
		Dictionary<int, int> clusterSizes = (from x in clusteringCopy
			group x by x).ToDictionary((IGrouping<int, int> x) => x.Key, (IGrouping<int, int> x) => x.Count());
		double[] distances = new double[length];
		for (int index3 = 0; index3 < data.Length; index3++)
		{
			for (int index4 = 0; index4 < length; index4++)
			{
				distances[index4] = Distance(data[index3], means[index4]);
			}
			int newClusterIndex = distances.Select((double distance, int index) => (distance, index)).MinBy<(double, int), double>(((double distance, int index) x) => x.distance).Item2;
			ref int clusterIndex = ref clusteringCopy[index3];
			if (newClusterIndex != clusterIndex)
			{
				didUpdate = true;
				if (clusterSizes[clusterIndex] > 1)
				{
					clusterSizes[clusterIndex]--;
					clusterSizes[newClusterIndex]++;
					clusterIndex = newClusterIndex;
				}
			}
		}
		if (!didUpdate)
		{
			return false;
		}
		int[] numArray2 = new int[length];
		for (int index5 = 0; index5 < data.Length; index5++)
		{
			int index6 = clusteringCopy[index5];
			numArray2[index6]++;
		}
		for (int index2 = 0; index2 < length; index2++)
		{
			if (numArray2[index2] == 0)
			{
				return false;
			}
		}
		Array.Copy(clusteringCopy, clustering, clusteringCopy.Length);
		return true;
	}

	private static double Distance(Vector2d v1, Vector2d v2)
	{
		return (v1 - v2).Length;
	}
}
