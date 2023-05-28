using System;

namespace ExileCore.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class SubmenuAttribute : Attribute
{
	public bool CollapsedByDefault { get; set; }
}
