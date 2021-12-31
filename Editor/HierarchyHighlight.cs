using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace PythagoRascal.UnityUtils.Editor
{
	[InitializeOnLoad]
	public static class HierarchyHighlight
	{
	#region Variables

		private const string DISABLED_KEY = "com.pythagorascal.unity-utils_HierarchyHighlight_Disabled";
		private static bool _Disabled;

		private static readonly Dictionary<int, Color> _Lookup;

		private const string HEX_COLOUR_PATTERN
			= @"^\{(#[0-9A-Fa-f]{6}|#[0-9A-Fa-f]{3})\}(.+)$";
		private static readonly Regex _HexColourHighlightRegex = new Regex(HEX_COLOUR_PATTERN);

		private const string NAMED_COLOUR_PATTERN
			= @"^\{(red|cyan|blue|darkblue|lightblue|purple|yellow|lime|fuchsia|white|silver|grey|black|orange|brown|maroon|green|olive|navy|teal|aqua|magenta)\}(.+)$";
		private static readonly Regex _NamedColourHighlightRegex = new Regex(NAMED_COLOUR_PATTERN);

		private const string SHORTCUT_COLOUR_PATTERN = @"^\{([mgbpdyorw])?\}(.+)$";
		private static readonly Regex _InitialsColourHighlightRegex = new Regex(SHORTCUT_COLOUR_PATTERN);
		private static readonly string[] _Initials
			= new string[] { "", "w", "m", "g", "b", "p", "d", "y", "o", "r" };
		private static readonly Dictionary<string, Color> _InitialsColours = new Dictionary<string, Color>()
		{
			{ "", new Color(127 / 255f, 140 / 255f, 141 / 255f) },
			{ "w", new Color(236 / 255f, 240 / 255f, 241 / 255f) },
			{ "m", new Color(22 / 255f, 160 / 255f, 133 / 255f) },
			{ "g", new Color(46 / 255f, 204 / 255f, 113 / 255f) },
			{ "b", new Color(41 / 255f, 128 / 255f, 185 / 255f) },
			{ "p", new Color(142 / 255f, 68 / 255f, 173 / 255f) },
			{ "d", new Color(44 / 255f, 62 / 255f, 80 / 255f) },
			{ "y", new Color(241 / 255f, 196 / 255f, 15 / 255f) },
			{ "o", new Color(230 / 255f, 126 / 255f, 34 / 255f) },
			{ "r", new Color(192 / 255f, 57 / 255f, 43 / 255f) },
		};

	#endregion

	#region Constructor

		static HierarchyHighlight()
		{
			_Lookup = new Dictionary<int, Color>();
			if (EditorPrefs.HasKey(DISABLED_KEY))
				_Disabled = EditorPrefs.GetBool(DISABLED_KEY);

			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
		}

	#endregion

	#region Menu Items

		[MenuItem("Tools/HierarchyHighlight/Toggle Enabled")]
		private static void ToggleEnabled()
		{
			if (EditorPrefs.HasKey(DISABLED_KEY))
			{
				bool disabled = EditorPrefs.GetBool(DISABLED_KEY);
				_Disabled = !disabled;
				EditorPrefs.SetBool(DISABLED_KEY, _Disabled);
			}
			else
			{
				EditorPrefs.SetBool(DISABLED_KEY, true);
				_Disabled = true;
			}
		}

		[MenuItem("Tools/HierarchyHighlight/Cycle Highlight Colour &#c")]
		private static void CycleHighlightColours()
		{
			int instanceId = Selection.activeInstanceID;
			var selected = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
			if (selected == null)
				return;

			Match match = _InitialsColourHighlightRegex.Match(selected.name);
			if (match.Success)
			{
				int initialsIdx = (Array.IndexOf(_Initials, match.Groups[1].Value) + 1) % _Initials.Length;
				string initial = _Initials[initialsIdx];
				selected.name = $"{{{initial}}}{match.Groups[2].Value}";
				UpdateLookup(instanceId, _InitialsColours[initial]);
			}
			else
			{
				selected.name = $"{{}} {selected.name}";
				UpdateLookup(instanceId, _InitialsColours[""]);
			}
		}

	#endregion

	#region GUI

		private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
		{
			if (_Disabled)
				return;

			if (Selection.Contains(instanceId))
				return;

			var gameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
			if (gameObject == null)
				return;

			string name = gameObject.name;
			string text = default;
			Color colour = default;

			if (GetColourFromHex(name, ref text, ref colour))
			{
				UpdateLookup(instanceId, colour);
			}
			else if (GetColourFromName(name, ref text, ref colour))
			{
				UpdateLookup(instanceId, colour);
			}
			else if (GetColourFromInitial(gameObject, ref text, ref colour))
			{
				UpdateLookup(instanceId, colour);
			}
			else
			{
				RemoveLookup(instanceId);
				return;
			}

			EditorGUI.DrawRect(selectionRect, colour);
			selectionRect.y -= 2.5f;
			EditorGUI.DropShadowLabel(selectionRect, text);
		}

	#endregion

	#region Colouring

		private static bool GetColourFromHex(string name, ref string text, ref Color colour)
		{
			Match match = _HexColourHighlightRegex.Match(name);
			if (!match.Success)
				return false;

			Color parsed;
			if (!ColorUtility.TryParseHtmlString(match.Groups[1].Value, out parsed))
				return false;

			text = match.Groups[2].Value;
			colour = parsed;

			return true;
		}

		private static bool GetColourFromName(string name, ref string text, ref Color colour)
		{
			Match match = _NamedColourHighlightRegex.Match(name);
			if (!match.Success)
				return false;

			Color parsed;
			if (!ColorUtility.TryParseHtmlString(match.Groups[1].Value, out parsed))
				return false;

			text = match.Groups[2].Value;
			colour = parsed;

			return true;
		}

		private static bool GetColourFromInitial(GameObject gameObject, ref string text, ref Color colour)
		{
			Match match = _InitialsColourHighlightRegex.Match(gameObject.name);
			if (!match.Success)
				return false;

			string initial = match.Groups[1].Value;
			text = match.Groups[2].Value;

			if (string.IsNullOrEmpty(initial) || !_InitialsColours.ContainsKey(initial))
			{
				bool found = false;
				int counter = 1;
				Transform parent = gameObject.transform.parent;
				while (parent != null)
				{
					int parentId = parent.gameObject.GetInstanceID();
					if (_Lookup.ContainsKey(parentId))
					{
						Color parentColour = _Lookup[parentId];
						float h, s, v;
						Color.RGBToHSV(parentColour, out h, out s, out v);
						s *= Mathf.Pow(1.1f, counter);
						v *= Mathf.Pow(0.9f, counter);
						colour = Color.HSVToRGB(h, s, v);
						colour.a = parentColour.a;

						found = true;
						break;
					}

					counter++;
					parent = parent.parent;
				}
				if (!found)
					colour = _InitialsColours[""];
			}
			else
			{
				colour = _InitialsColours[initial];
			}

			return true;
		}

	#endregion

	#region Caching

		private static void UpdateLookup(int instanceId, Color colour)
		{
			if (!_Lookup.ContainsKey(instanceId))
				_Lookup.Add(instanceId, colour);
			else
				_Lookup[instanceId] = colour;
		}

		private static void RemoveLookup(int instanceId)
		{
			if (_Lookup.ContainsKey(instanceId))
				_Lookup.Remove(instanceId);
		}

	#endregion
	}
}
