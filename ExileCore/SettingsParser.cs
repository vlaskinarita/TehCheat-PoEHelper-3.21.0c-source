using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using GameOffsets.Native;
using ImGuiNET;
using MoreLinq;

namespace ExileCore;

public static class SettingsParser
{
	public static void Parse(ISettings settings, List<ISettingsHolder> draws, int id = -1)
	{
		int nextKey = -2;
		Parse(settings, draws, id, ref nextKey);
	}

	private static void Parse(object settings, List<ISettingsHolder> draws, int id, ref int nextKey)
	{
		if (settings == null)
		{
			DebugWindow.LogError("Cant parse null settings.");
			return;
		}
		PropertyInfo[] properties = settings.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.GetCustomAttribute<IgnoreMenuAttribute>() != null)
			{
				continue;
			}
			MenuAttribute menuAttribute = propertyInfo.GetCustomAttribute<MenuAttribute>();
			if (propertyInfo.Name == "Enable" && menuAttribute == null)
			{
				continue;
			}
			string text = Regex.Replace(Regex.Replace(propertyInfo.Name, "(((?<![A-Z])\\B[A-Z])|(\\B[A-Z](?![A-Z])))", " $1"), "(?<!^)\\b(of|the|a|and|or|to|in|as|at)\\b", (Match m) => m.Value.ToLowerInvariant(), RegexOptions.IgnoreCase);
			if (menuAttribute == null)
			{
				menuAttribute = new MenuAttribute(text);
			}
			int parentIndex = ((menuAttribute.parentIndex == -1) ? id : menuAttribute.parentIndex);
			SettingsHolder holder = new SettingsHolder
			{
				Name = (menuAttribute.MenuName ?? text),
				Tooltip = menuAttribute.Tooltip,
				ID = ((menuAttribute.index == -1) ? nextKey-- : menuAttribute.index)
			};
			object value = propertyInfo.GetValue(settings);
			ConditionalDisplayAttribute conditionalDisplayAttribute = propertyInfo.GetCustomAttribute<ConditionalDisplayAttribute>() ?? value?.GetType().GetCustomAttribute<ConditionalDisplayAttribute>();
			if (conditionalDisplayAttribute != null)
			{
				string conditionMethodName = conditionalDisplayAttribute.ConditionMethodName;
				bool comparisonValue = conditionalDisplayAttribute.ComparisonValue;
				holder.DisplayCondition = GetConditionMethodOrProperty(settings, conditionMethodName, comparisonValue);
			}
			SubmenuAttribute submenuAttribute = propertyInfo.GetCustomAttribute<SubmenuAttribute>() ?? value?.GetType().GetCustomAttribute<SubmenuAttribute>();
			holder.CollapsedByDefault = submenuAttribute?.CollapsedByDefault ?? menuAttribute.CollapsedByDefault;
			if (value is ISettings || submenuAttribute != null)
			{
				draws.Add(holder);
				Parse(value, draws, holder.ID, ref nextKey);
				bool flag = false;
				if (parentIndex != -1)
				{
					ISettingsHolder settingsHolder = GetAllDrawers(draws).Find((ISettingsHolder x) => x.ID == parentIndex);
					if (settingsHolder != null)
					{
						flag = true;
						settingsHolder.Children.Add(holder);
					}
				}
				if (flag)
				{
					draws.Remove(holder);
				}
				continue;
			}
			if (parentIndex != -1)
			{
				GetAllDrawers(draws).Find((ISettingsHolder x) => x.ID == parentIndex)?.Children.Add(holder);
			}
			else
			{
				draws.Add(holder);
			}
			ButtonNode buttonNode = value as ButtonNode;
			if (buttonNode == null)
			{
				if (value == null || value is EmptyNode)
				{
					continue;
				}
				CustomNode customNode = value as CustomNode;
				if (customNode == null)
				{
					HotkeyNode hotkeyNode = value as HotkeyNode;
					if (hotkeyNode == null)
					{
						ToggleNode toggleNode = value as ToggleNode;
						if (toggleNode == null)
						{
							ColorNode colorNode = value as ColorNode;
							if (colorNode == null)
							{
								TextNode textNode = value as TextNode;
								if (textNode == null)
								{
									ListNode listNode = value as ListNode;
									if (listNode == null)
									{
										FileNode fileNode = value as FileNode;
										if (fileNode == null)
										{
											RangeNode<int> rangeNode = value as RangeNode<int>;
											if (rangeNode == null)
											{
												RangeNode<float> rangeNode2 = value as RangeNode<float>;
												if (rangeNode2 == null)
												{
													RangeNode<long> rangeNode3 = value as RangeNode<long>;
													if (rangeNode3 == null)
													{
														RangeNode<Vector2> rangeNode4 = value as RangeNode<Vector2>;
														if (rangeNode4 == null)
														{
															RangeNode<Vector2i> rangeNode5 = value as RangeNode<Vector2i>;
															if (rangeNode5 != null)
															{
																holder.DrawDelegate = delegate
																{
																	Vector2i value3 = rangeNode5.Value;
																	ImGui.SliderInt2(holder.Unique, ref value3.X, rangeNode5.Min.X, rangeNode5.Max.X);
																	rangeNode5.Value = value3;
																};
															}
															else
															{
																Core.Logger.Warning($"{value} not supported for menu now. Ask developers to add this type.");
															}
														}
														else
														{
															holder.DrawDelegate = delegate
															{
																Vector2 v5 = rangeNode4.Value;
																ImGui.SliderFloat2(holder.Unique, ref v5, rangeNode4.Min.X, rangeNode4.Max.X);
																rangeNode4.Value = v5;
															};
														}
													}
													else
													{
														holder.DrawDelegate = delegate
														{
															int v4 = (int)rangeNode3.Value;
															ImGui.SliderInt(holder.Unique, ref v4, (int)rangeNode3.Min, (int)rangeNode3.Max);
															rangeNode3.Value = v4;
														};
													}
												}
												else
												{
													holder.DrawDelegate = delegate
													{
														float v3 = rangeNode2.Value;
														ImGui.SliderFloat(holder.Unique, ref v3, rangeNode2.Min, rangeNode2.Max);
														rangeNode2.Value = v3;
													};
												}
											}
											else
											{
												holder.DrawDelegate = delegate
												{
													int v2 = rangeNode.Value;
													ImGui.SliderInt(holder.Unique, ref v2, rangeNode.Min, rangeNode.Max);
													rangeNode.Value = v2;
												};
											}
											continue;
										}
										holder.DrawDelegate = delegate
										{
											if (ImGui.TreeNode(holder.Unique))
											{
												string value2 = fileNode.Value;
												if (ImGui.BeginChildFrame(1u, new Vector2(0f, 300f)))
												{
													DirectoryInfo directoryInfo = new DirectoryInfo("config");
													if (directoryInfo.Exists)
													{
														FileInfo[] files = directoryInfo.GetFiles();
														foreach (FileInfo fileInfo in files)
														{
															if (ImGui.Selectable(fileInfo.Name, value2 == fileInfo.FullName))
															{
																fileNode.Value = fileInfo.FullName;
															}
														}
													}
													ImGui.EndChildFrame();
												}
												ImGui.TreePop();
											}
										};
										continue;
									}
									holder.DrawDelegate = delegate
									{
										if (ImGui.BeginCombo(holder.Unique, listNode.Value))
										{
											foreach (string value4 in listNode.Values)
											{
												if (ImGui.Selectable(value4))
												{
													listNode.Value = value4;
													ImGui.EndCombo();
													return;
												}
											}
											ImGui.EndCombo();
										}
									};
								}
								else
								{
									holder.DrawDelegate = delegate
									{
										string input = textNode.Value ?? "";
										ImGui.InputText(holder.Unique, ref input, 200u);
										textNode.Value = input;
									};
								}
								continue;
							}
							holder.DrawDelegate = delegate
							{
								Vector4 col = colorNode.Value.ToVector4().ToVector4Num();
								if (ImGui.ColorEdit4(holder.Unique, ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.AlphaPreviewHalf))
								{
									colorNode.Value = col.ToSharpColor();
								}
							};
						}
						else
						{
							holder.DrawDelegate = delegate
							{
								bool v = toggleNode.Value;
								ImGui.Checkbox(holder.Unique, ref v);
								toggleNode.Value = v;
							};
						}
					}
					else
					{
						holder.DrawDelegate = delegate
						{
							string id2 = $"{holder.Name} {hotkeyNode.Value}";
							hotkeyNode.DrawPickerButton(id2);
						};
					}
				}
				else
				{
					holder.DrawDelegate = delegate
					{
						customNode.DrawDelegate?.Invoke();
					};
				}
				continue;
			}
			holder.DrawDelegate = delegate
			{
				if (ImGui.Button(holder.Unique))
				{
					buttonNode.OnPressed();
				}
			};
		}
	}

	private static Func<bool> GetConditionMethodOrProperty(object settings, string methodName, bool comparisonValue)
	{
		Type type = settings.GetType();
		MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes);
		if (method != null)
		{
			if (method.ReturnType == typeof(bool))
			{
				return () => comparisonValue == (bool)method.Invoke(settings, new object[0]);
			}
			MethodInfo conversionMethod2 = GetBoolConversionMethod(method.ReturnType);
			if (conversionMethod2 != null)
			{
				return () => comparisonValue == (bool)conversionMethod2.Invoke(null, new object[1] { method.Invoke(settings, new object[0]) });
			}
		}
		else
		{
			PropertyInfo property = type.GetProperty(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.PropertyType == typeof(bool))
			{
				return () => comparisonValue == (bool)property.GetValue(settings);
			}
			MethodInfo conversionMethod = GetBoolConversionMethod(property.PropertyType);
			if (conversionMethod != null)
			{
				return () => comparisonValue == (bool)conversionMethod.Invoke(null, new object[1] { property.GetValue(settings) });
			}
		}
		DebugWindow.LogError($"Wanted to use method or property {methodName} on type {type} as display condition but can't find it or it has a wrong type.");
		return null;
	}

	private static MethodInfo GetBoolConversionMethod(Type type)
	{
		return type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(delegate(MethodInfo x)
		{
			if (x.IsSpecialName)
			{
				string name = x.Name;
				if ((name == "op_Implicit" || name == "op_Explicit") && x.ReturnType == typeof(bool))
				{
					ParameterInfo[] parameters = x.GetParameters();
					if (parameters != null && parameters.Length == 1)
					{
						return parameters[0].ParameterType == type;
					}
				}
			}
			return false;
		});
	}

	private static List<ISettingsHolder> GetAllDrawers(List<ISettingsHolder> SettingPropertyDrawers)
	{
		List<ISettingsHolder> result = new List<ISettingsHolder>();
		GetDrawersRecurs(SettingPropertyDrawers, result);
		return result;
	}

	private static void GetDrawersRecurs(IList<ISettingsHolder> drawers, IList<ISettingsHolder> result)
	{
		foreach (ISettingsHolder drawer in drawers)
		{
			if (!result.Contains(drawer))
			{
				result.Add(drawer);
				continue;
			}
			Core.Logger.Error($" Possible stashoverflow or duplicating drawers detected while generating menu. Drawer SettingName: {drawer.Name}, Id: {drawer.ID}", 5);
		}
		drawers.ForEach(delegate(ISettingsHolder x)
		{
			GetDrawersRecurs(x.Children, result);
		});
	}
}
