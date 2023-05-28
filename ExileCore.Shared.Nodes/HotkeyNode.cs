using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ImGuiNET;
using Newtonsoft.Json;
using SharpDX;

namespace ExileCore.Shared.Nodes;

public class HotkeyNode
{
	private static readonly IEnumerable<Keys> SelectableKeys = Enum.GetValues<Keys>().Except(new Keys[1]).ToList();

	private bool _pressed;

	private bool _unPressed;

	[JsonIgnore]
	public Action OnValueChanged = delegate
	{
	};

	private Keys value;

	public Keys Value
	{
		get
		{
			return value;
		}
		set
		{
			if (this.value != value)
			{
				this.value = value;
				try
				{
					OnValueChanged();
				}
				catch
				{
					DebugWindow.LogMsg("Error in function that subscribed for: HotkeyNode.OnValueChanged", 10f, Color.Red);
				}
			}
		}
	}

	public HotkeyNode()
	{
		value = Keys.Space;
	}

	public HotkeyNode(Keys value)
	{
		Value = value;
	}

	public static implicit operator Keys(HotkeyNode node)
	{
		return node.Value;
	}

	public static implicit operator HotkeyNode(Keys value)
	{
		return new HotkeyNode(value);
	}

	public bool PressedOnce()
	{
		if (Input.IsKeyDown(value))
		{
			if (_pressed)
			{
				return false;
			}
			_pressed = true;
			return true;
		}
		_pressed = false;
		return false;
	}

	public bool UnpressedOnce()
	{
		if (Input.GetKeyState(value))
		{
			_unPressed = true;
		}
		else if (_unPressed)
		{
			_unPressed = false;
			return true;
		}
		return false;
	}

	public bool DrawPickerButton(string id)
	{
		if (ImGui.Button(id))
		{
			ImGui.OpenPopup(id);
		}
		bool result = false;
		bool p_open = true;
		if (ImGui.BeginPopupModal(id, ref p_open, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
		{
			if (Input.GetKeyState(Keys.Escape))
			{
				ImGui.CloseCurrentPopup();
				ImGui.EndPopup();
				return false;
			}
			foreach (Keys selectableKey in SelectableKeys)
			{
				if (Input.GetKeyState(selectableKey))
				{
					Value = selectableKey;
					result = true;
					ImGui.CloseCurrentPopup();
					break;
				}
			}
			ImGui.Text($" Press new key to change '{Value}' or Esc for exit.");
			ImGui.EndPopup();
		}
		return result;
	}
}
