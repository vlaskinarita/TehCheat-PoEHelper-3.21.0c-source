namespace ExileCore.Shared;

public class IntRange
{
	public int Min { get; private set; }

	public int Max { get; private set; }

	public IntRange(int min, int max)
	{
		Min = min;
		Max = max;
	}

	public IntRange()
	{
		Min = int.MaxValue;
		Max = int.MinValue;
	}

	public void Include(int value)
	{
		if (value < Min)
		{
			Min = value;
		}
		if (value > Max)
		{
			Max = value;
		}
	}

	public override string ToString()
	{
		return $"{Min} - {Max}";
	}

	public float GetPercentage(int val)
	{
		if (Min == Max)
		{
			return 1f;
		}
		return (float)(val - Min) / (float)(Max - Min);
	}

	public bool HasSpread()
	{
		return Max != Min;
	}
}
