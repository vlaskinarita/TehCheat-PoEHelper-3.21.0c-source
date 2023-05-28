using System.Collections.Generic;
using System.Linq;
using ExileCore.PoEMemory.FilesInMemory;
using ExileCore.Shared.Interfaces;

namespace ExileCore.PoEMemory.MemoryObjects;

public class LabyrinthRoom
{
	public class LabyrinthSecret
	{
		public string SecretName { get; internal set; }

		public string Name { get; internal set; }

		public override string ToString()
		{
			return SecretName;
		}
	}

	public class LabyrinthSection
	{
		public WorldAreas FilesWorldAreas { get; }

		public string SectionType { get; internal set; }

		public IList<LabyrinthSectionOverrides> Overrides { get; internal set; } = new List<LabyrinthSectionOverrides>();


		public LabyrinthSectionAreas SectionAreas { get; internal set; }

		internal LabyrinthSection(IMemory M, long addr, WorldAreas filesWorldAreas)
		{
			FilesWorldAreas = filesWorldAreas;
			SectionType = M.ReadStringU(M.Read<long>(addr + 8, new int[1]));
			int num = M.Read<int>(addr + 92);
			long startAddress = M.Read<long>(addr + 100);
			IList<long> list = M.ReadSecondPointerArray_Count(startAddress, num);
			for (int i = 0; i < num; i++)
			{
				LabyrinthSectionOverrides labyrinthSectionOverrides = new LabyrinthSectionOverrides();
				long num2 = list[i];
				labyrinthSectionOverrides.OverrideName = M.ReadStringU(M.Read<long>(num2));
				labyrinthSectionOverrides.Name = M.ReadStringU(M.Read<long>(num2 + 8));
				Overrides.Add(labyrinthSectionOverrides);
			}
			SectionAreas = new LabyrinthSectionAreas(FilesWorldAreas);
			long num3 = M.Read<long>(addr + 76);
			SectionAreas.Name = M.ReadStringU(M.Read<long>(num3));
			int count = M.Read<int>(num3 + 8);
			long startAddress2 = M.Read<long>(num3 + 16);
			SectionAreas.NormalAreasPtrs = M.ReadSecondPointerArray_Count(startAddress2, count);
			int count2 = M.Read<int>(num3 + 24);
			long startAddress3 = M.Read<long>(num3 + 32);
			SectionAreas.CruelAreasPtrs = M.ReadSecondPointerArray_Count(startAddress3, count2);
			int count3 = M.Read<int>(num3 + 40);
			long startAddress4 = M.Read<long>(num3 + 48);
			SectionAreas.MercilesAreasPtrs = M.ReadSecondPointerArray_Count(startAddress4, count3);
			int count4 = M.Read<int>(num3 + 56);
			long startAddress5 = M.Read<long>(num3 + 64);
			SectionAreas.EndgameAreasPtrs = M.ReadSecondPointerArray_Count(startAddress5, count4);
		}

		public override string ToString()
		{
			string text = "";
			if (Overrides.Count > 0)
			{
				text = "Overrides: " + string.Join(", ", Overrides.Select((LabyrinthSectionOverrides x) => x.ToString()).ToArray());
			}
			return "SectionType: " + SectionType + ", " + text;
		}
	}

	private readonly long Address;

	private readonly IMemory M;

	public WorldAreas FilesWorldAreas { get; }

	public int Id { get; internal set; }

	public LabyrinthSecret Secret1 { get; internal set; }

	public LabyrinthSecret Secret2 { get; internal set; }

	public LabyrinthRoom[] Connections { get; internal set; }

	public LabyrinthSection Section { get; internal set; }

	internal LabyrinthRoom(IMemory m, long address, WorldAreas filesWorldAreas)
	{
		FilesWorldAreas = filesWorldAreas;
		M = m;
		Address = address;
		Secret1 = ReadSecret(M.Read<long>(Address + 64));
		Secret2 = ReadSecret(M.Read<long>(Address + 80));
		Section = ReadSection(M.Read<long>(Address + 48));
		IList<long> source = M.ReadPointersArray(Address, Address + 32);
		Connections = source.Select((long x) => (x != 0L) ? LabyrinthData.GetRoomById(x) : null).ToArray();
	}

	internal LabyrinthSection ReadSection(long addr)
	{
		if (addr == 0L)
		{
			return null;
		}
		return new LabyrinthSection(M, addr, FilesWorldAreas);
	}

	private LabyrinthSecret ReadSecret(long addr)
	{
		M.Read<long>(addr);
		if (addr == 0L)
		{
			return null;
		}
		return new LabyrinthSecret
		{
			SecretName = M.ReadStringU(M.Read<long>(addr)),
			Name = M.ReadStringU(M.Read<long>(addr + 8))
		};
	}

	public override string ToString()
	{
		string value = "";
		List<LabyrinthRoom> list = Connections.Where((LabyrinthRoom r) => r != null).ToList();
		if (list.Count > 0)
		{
			value = "LinkedWith: " + string.Join(", ", list.Select((LabyrinthRoom x) => x.Id.ToString()).ToArray());
		}
		return $"Id: {Id}, Secret1: {((Secret1 == null) ? "None" : Secret1.SecretName)}, Secret2: {((Secret2 == null) ? "None" : Secret2.SecretName)}, {value}, Section: {Section}";
	}
}
