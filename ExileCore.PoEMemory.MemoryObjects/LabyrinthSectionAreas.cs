using System.Collections.Generic;
using System.Linq;
using ExileCore.PoEMemory.FilesInMemory;

namespace ExileCore.PoEMemory.MemoryObjects;

public class LabyrinthSectionAreas
{
	private List<WorldArea> cruelAreas;

	private List<WorldArea> endgameAreas;

	private List<WorldArea> mercilesAreas;

	private List<WorldArea> normalAreas;

	public WorldAreas FilesWorldAreas { get; }

	public string Name { get; set; }

	public IList<long> NormalAreasPtrs { get; set; }

	public IList<long> CruelAreasPtrs { get; set; }

	public IList<long> MercilesAreasPtrs { get; set; }

	public IList<long> EndgameAreasPtrs { get; set; }

	public IList<WorldArea> NormalAreas
	{
		get
		{
			if (normalAreas == null)
			{
				normalAreas = NormalAreasPtrs.Select((long x) => FilesWorldAreas.GetByAddress(x)).ToList();
			}
			return normalAreas;
		}
	}

	public IList<WorldArea> CruelAreas
	{
		get
		{
			if (cruelAreas == null)
			{
				cruelAreas = CruelAreasPtrs.Select((long x) => FilesWorldAreas.GetByAddress(x)).ToList();
			}
			return cruelAreas;
		}
	}

	public IList<WorldArea> MercilesAreas
	{
		get
		{
			if (mercilesAreas == null)
			{
				mercilesAreas = MercilesAreasPtrs.Select((long x) => FilesWorldAreas.GetByAddress(x)).ToList();
			}
			return mercilesAreas;
		}
	}

	public IList<WorldArea> EndgameAreas
	{
		get
		{
			if (endgameAreas == null)
			{
				endgameAreas = EndgameAreasPtrs.Select((long x) => FilesWorldAreas.GetByAddress(x)).ToList();
			}
			return endgameAreas;
		}
	}

	public LabyrinthSectionAreas(WorldAreas filesWorldAreas)
	{
		FilesWorldAreas = filesWorldAreas;
		NormalAreasPtrs = new List<long>();
		CruelAreasPtrs = new List<long>();
		MercilesAreasPtrs = new List<long>();
		EndgameAreasPtrs = new List<long>();
	}
}
