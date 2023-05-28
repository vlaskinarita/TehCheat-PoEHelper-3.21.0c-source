using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Helpers;
using GameOffsets;
using MoreLinq;
using SharpDX;

namespace ExileCore.PoEMemory;

public class Element : RemoteMemoryObject
{
	public const int OffsetBuffers = 0;

	private static readonly int ChildStartOffset = Extensions.GetOffset((ElementOffsets x) => x.ChildStart);

	private static readonly int EntityOffset = Extensions.GetOffset((NormalInventoryItemOffsets x) => x.Item);

	private readonly CachedValue<ElementOffsets> _cacheElement;

	private readonly List<Element> _childrens = new List<Element>();

	private CachedValue<SharpDX.RectangleF> _getClientRect;

	private Element _parent;

	private long childHashCache;

	public ElementOffsets Elem => _cacheElement.Value;

	public bool IsValid => Elem.SelfPointer == base.Address;

	public long ChildCount => (Elem.ChildEnd - Elem.ChildStart) / 8;

	public bool IsVisibleLocal => ((Elem.IsVisibleLocal >> 3) & 1) > 0;

	public bool IsSelected => Elem.IsSelected == 0;

	public Element Root => base.TheGame.IngameState.UIRoot;

	public Element Parent
	{
		get
		{
			object obj;
			if (Elem.Parent != 0L)
			{
				obj = _parent;
				if (obj == null)
				{
					return _parent = GetObject<Element>(Elem.Parent);
				}
			}
			else
			{
				obj = null;
			}
			return (Element)obj;
		}
	}

	[Obsolete]
	public SharpDX.Vector2 Position => Elem.Position.ToSharpDx();

	public System.Numerics.Vector2 PositionNum => Elem.Position;

	public float X => PositionNum.X;

	public float Y => PositionNum.Y;

	public Element Tooltip
	{
		get
		{
			if (base.Address != 0L)
			{
				return ReadObject<Element>(Elem.Tooltip);
			}
			return null;
		}
	}

	public float Scale => Elem.Scale;

	public float Width => Elem.Size.X;

	public float Height => Elem.Size.Y;

	public bool isHighlighted => Elem.isHighlighted;

	public Entity Entity => ReadObject<Entity>(base.Address + EntityOffset);

	public System.Drawing.Point Center
	{
		get
		{
			SharpDX.Vector2 center = GetClientRect().Center;
			return new System.Drawing.Point(Convert.ToInt32(center.X), Convert.ToInt32(center.Y));
		}
	}

	public virtual string Text => GetText(256);

	public bool IsVisible
	{
		get
		{
			if (base.Address >= 1770350607106052L || base.Address <= 0)
			{
				return false;
			}
			if (IsVisibleLocal)
			{
				return GetParentChain().All((Element current) => current.IsVisibleLocal);
			}
			return false;
		}
	}

	public IList<Element> Children => GetChildren<Element>();

	public long ChildHash => Elem.Childs.GetHashCode();

	public SharpDX.RectangleF GetClientRectCache => _getClientRect?.Value ?? (_getClientRect = new TimeCache<SharpDX.RectangleF>(GetClientRect, 200L)).Value;

	public Element this[int index] => GetChildAtIndex(index);

	public int? IndexInParent => Parent?.Children.IndexOf(this);

	public string PathFromRoot
	{
		get
		{
			List<Element> parentChain = GetParentChain();
			if (parentChain.Count != 0)
			{
				parentChain.RemoveAt(parentChain.Count - 1);
				parentChain.Reverse();
			}
			parentChain.Add(this);
			ILookup<long?, string> properties = (from p in base.TheGame.IngameState.IngameUi.GetType().GetProperties()
				where typeof(Element).IsAssignableFrom(p.PropertyType)
				where p.GetIndexParameters().Length == 0
				select (p, (p.GetValue(base.TheGame.IngameState.IngameUi) as Element)?.Address)).Where<(PropertyInfo, long?)>(delegate((PropertyInfo property, long? Address) t)
			{
				long? item = t.Address;
				return !item.HasValue || item.GetValueOrDefault() != 0;
			}).ToLookup<(PropertyInfo, long?), long?, string>(((PropertyInfo property, long? Address) x) => x.Address, ((PropertyInfo property, long? Address) x) => x.property.Name);
			return string.Join("->", parentChain.Select(delegate(Element x)
			{
				List<string> list = properties[x.Address].ToList();
				if (list != null)
				{
					switch (list.Count)
					{
					case 0:
						return x.IndexInParent.ToString();
					case 1:
						return $"({list.First()}){x.IndexInParent}";
					}
				}
				List<string> values = list;
				return $"({string.Join('/', values)}){x.IndexInParent}";
			}));
		}
	}

	public Element()
	{
		_cacheElement = new FrameCache<ElementOffsets>(() => (base.Address != 0L) ? base.M.Read<ElementOffsets>(base.Address) : default(ElementOffsets));
	}

	public string GetText(int maxLength)
	{
		string text = Elem.Text.ToString(base.M, maxLength);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text.Replace("\u00a0\u00a0\u00a0\u00a0", "{{icon}}");
		}
		return null;
	}

	protected List<Element> GetChildren<T>() where T : Element
	{
		ElementOffsets elem = Elem;
		if (base.Address == 0L || elem.ChildStart == 0L || elem.ChildEnd == 0L || ChildCount <= 0)
		{
			return _childrens;
		}
		if (ChildHash == childHashCache)
		{
			return _childrens;
		}
		IList<long> list = base.M.ReadPointersArray(elem.ChildStart, elem.ChildEnd);
		if (list.Count > 1000 || list.Count != ChildCount)
		{
			return _childrens;
		}
		_childrens.Clear();
		foreach (long item in list)
		{
			_childrens.Add(GetObject<Element>(item));
		}
		childHashCache = ChildHash;
		return _childrens;
	}

	public List<T> GetChildrenAs<T>() where T : Element, new()
	{
		ElementOffsets elem = Elem;
		if (base.Address == 0L || elem.ChildStart == 0L || elem.ChildEnd == 0L || ChildCount <= 0)
		{
			return new List<T>();
		}
		IList<long> list = base.M.ReadPointersArray(elem.ChildStart, elem.ChildEnd);
		if (list.Count != ChildCount)
		{
			return new List<T>();
		}
		List<T> list2 = new List<T>();
		foreach (long item in list)
		{
			list2.Add(GetObject<T>(item));
		}
		return list2;
	}

	public List<Element> GetParentChain()
	{
		List<Element> list = new List<Element>();
		if (base.Address == 0L)
		{
			return list;
		}
		HashSet<Element> hashSet = new HashSet<Element>();
		Element root = Root;
		Element parent = Parent;
		if (root == null || parent == null)
		{
			return list;
		}
		while (!hashSet.Contains(parent) && root.Address != parent.Address && parent.Address != 0L && hashSet.Count < 100)
		{
			list.Add(parent);
			hashSet.Add(parent);
			parent = parent.Parent;
			if (parent == null)
			{
				break;
			}
		}
		return list;
	}

	public SharpDX.Vector2 GetParentPos()
	{
		float num = 0f;
		float num2 = 0f;
		float scale = base.TheGame.IngameState.UIRoot.Scale;
		foreach (Element item in GetParentChain())
		{
			num += item.X * item.Scale / scale;
			num2 += item.Y * item.Scale / scale;
		}
		return new SharpDX.Vector2(num, num2);
	}

	public virtual SharpDX.RectangleF GetClientRect()
	{
		if (base.Address == 0L)
		{
			return SharpDX.RectangleF.Empty;
		}
		SharpDX.Vector2 parentPos = GetParentPos();
		float num = base.TheGame.IngameState.Camera.Width;
		float num2 = base.TheGame.IngameState.Camera.Height;
		float num3 = num / num2 / 1.6f;
		float num4 = num / 2560f / num3;
		float num5 = num2 / 1600f;
		int blackBarSize = base.TheGame.BlackBarSize;
		float scale = base.TheGame.IngameState.UIRoot.Scale;
		float num6 = (parentPos.X + X * Scale / scale) * num4;
		float y = (parentPos.Y + Y * Scale / scale) * num5;
		return new SharpDX.RectangleF(num6 + (float)blackBarSize, y, num4 * Width * Scale / scale, num5 * Height * Scale / scale);
	}

	public virtual SharpDX.RectangleF GetClientRectWithTrans(SharpDX.Vector2 posTrans)
	{
		if (base.Address == 0L)
		{
			return SharpDX.RectangleF.Empty;
		}
		SharpDX.Vector2 vector = GetParentPos() + posTrans;
		float num = base.TheGame.IngameState.Camera.Width;
		float num2 = base.TheGame.IngameState.Camera.Height;
		float num3 = num / num2 / 1.6f;
		float num4 = num / 2560f / num3;
		float num5 = num2 / 1600f;
		float scale = base.TheGame.IngameState.UIRoot.Scale;
		float x = (vector.X + X * Scale / scale) * num4;
		float y = (vector.Y + Y * Scale / scale) * num5;
		return new SharpDX.RectangleF(x, y, num4 * Width * Scale / scale, num5 * Height * Scale / scale);
	}

	public virtual SharpDX.RectangleF GetRect(SharpDX.Vector2 rootPos)
	{
		if (base.Address == 0L)
		{
			return SharpDX.RectangleF.Empty;
		}
		SharpDX.Vector2 vector = rootPos;
		float num = base.TheGame.IngameState.Camera.Width;
		float num2 = base.TheGame.IngameState.Camera.Height;
		float num3 = num / num2 / 1.6f;
		float num4 = num / 2560f / num3;
		float num5 = num2 / 1600f;
		float scale = base.TheGame.IngameState.UIRoot.Scale;
		float x = (vector.X + X * Scale / scale) * num4;
		float y = (vector.Y + Y * Scale / scale) * num5;
		return new SharpDX.RectangleF(x, y, num4 * Width * Scale / scale, num5 * Height * Scale / scale);
	}

	public Element FindChildRecursive(Func<Element, bool> condition)
	{
		if (condition(this))
		{
			return this;
		}
		foreach (Element child in Children)
		{
			Element element = child.FindChildRecursive(condition);
			if (element != null)
			{
				return element;
			}
		}
		return null;
	}

	public Element FindChildRecursive(string text, bool contains = false)
	{
		return FindChildRecursive((Element elem) => elem.Text == text || (contains && (elem.Text?.Contains(text) ?? false)));
	}

	public Element GetChildFromIndices(params int[] indices)
	{
		Element element = this;
		foreach (int num in indices)
		{
			element = element.GetChildAtIndex(num);
			if (element == null)
			{
				string str2 = "";
				indices.ForEach(delegate(int i)
				{
					str2 += $"[{i}] ";
				});
				DebugWindow.LogMsg($"{"Element"} with index: {num} not found. Indices: {str2}");
				return null;
			}
			if (element.Address == 0L)
			{
				string str = "";
				indices.ForEach(delegate(int i)
				{
					str += $"[{i}] ";
				});
				DebugWindow.LogMsg($"{"Element"} with index: {num} 0 address. Indices: {str}");
				return GetObject<Element>(0L);
			}
		}
		return element;
	}

	public Element GetChildAtIndex(int index)
	{
		if (index < ChildCount)
		{
			return GetObject<Element>(base.M.Read<long>(base.Address + ChildStartOffset, new int[1] { index * 8 }));
		}
		return null;
	}
}
