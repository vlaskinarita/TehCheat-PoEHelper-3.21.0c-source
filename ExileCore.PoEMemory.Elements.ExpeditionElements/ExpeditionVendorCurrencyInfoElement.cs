namespace ExileCore.PoEMemory.Elements.ExpeditionElements;

public class ExpeditionVendorCurrencyInfoElement : Element
{
	public int GwennenRerolls
	{
		get
		{
			if (!int.TryParse(GetChildFromIndices(0, 1)?.Text ?? "", out var result))
			{
				return 0;
			}
			return result;
		}
	}

	public int TujenRerolls
	{
		get
		{
			if (!int.TryParse(GetChildFromIndices(1, 1)?.Text ?? "", out var result))
			{
				return 0;
			}
			return result;
		}
	}

	public int RogRerolls
	{
		get
		{
			if (!int.TryParse(GetChildFromIndices(2, 1)?.Text ?? "", out var result))
			{
				return 0;
			}
			return result;
		}
	}

	public int DannigRerolls
	{
		get
		{
			if (!int.TryParse(GetChildFromIndices(3, 1)?.Text ?? "", out var result))
			{
				return 0;
			}
			return result;
		}
	}
}
