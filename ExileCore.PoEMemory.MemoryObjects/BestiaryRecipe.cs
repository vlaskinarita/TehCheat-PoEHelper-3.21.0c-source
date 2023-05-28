using System.Collections.Generic;
using System.Linq;

namespace ExileCore.PoEMemory.MemoryObjects;

public class BestiaryRecipe : RemoteMemoryObject
{
	private List<BestiaryRecipeComponent> components;

	private string description;

	private string hint;

	private string notes;

	private string recipeId;

	private BestiaryRecipeComponent specialMonster;

	public int Id { get; internal set; }

	public string RecipeId
	{
		get
		{
			if (recipeId == null)
			{
				return recipeId = base.M.ReadStringU(base.M.Read<long>(base.Address));
			}
			return recipeId;
		}
	}

	public string Description
	{
		get
		{
			if (description == null)
			{
				return description = base.M.ReadStringU(base.M.Read<long>(base.Address + 8));
			}
			return description;
		}
	}

	public string Notes
	{
		get
		{
			if (notes == null)
			{
				return notes = base.M.ReadStringU(base.M.Read<long>(base.Address + 32));
			}
			return notes;
		}
	}

	public string HintText
	{
		get
		{
			if (hint == null)
			{
				return hint = base.M.ReadStringU(base.M.Read<long>(base.Address + 40));
			}
			return hint;
		}
	}

	public bool RequireSpecialMonster => Components.Count == 4;

	public BestiaryRecipeComponent SpecialMonster
	{
		get
		{
			if (!RequireSpecialMonster)
			{
				return null;
			}
			if (specialMonster == null)
			{
				specialMonster = Components.FirstOrDefault();
			}
			return specialMonster;
		}
	}

	public IList<BestiaryRecipeComponent> Components
	{
		get
		{
			if (components == null)
			{
				int count = base.M.Read<int>(base.Address + 16);
				IList<long> source = base.M.ReadSecondPointerArray_Count(base.M.Read<long>(base.Address + 24), count);
				components = source.Select((long x) => base.TheGame.Files.BestiaryRecipeComponents.GetByAddress(x)).ToList();
			}
			return components;
		}
	}

	public override string ToString()
	{
		return HintText + ": " + Description;
	}
}
