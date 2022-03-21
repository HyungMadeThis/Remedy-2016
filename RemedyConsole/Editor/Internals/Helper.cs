using System;
using UnityEditor;
using UnityEngine;

namespace Remedy.Internals
{
	public static class Helper
	{

		private static GUIContent _guiContent = new GUIContent();

		public static string GetBasename(string file)
		{
			var index = file.LastIndexOf("/", StringComparison.Ordinal);
			return index >= 0 ? file.Substring(index + 1) : file;
		}

		public static GUIContent IconContent(Texture icon, string tooltip)
		{
			return new GUIContent(string.Empty, tooltip)
			{
				image = icon
			};
		}

		public static GUIContent Content(string text, string tooltip = "", Texture2D icon = null)
		{
			_guiContent.text = text;
			_guiContent.tooltip = tooltip;
			_guiContent.image = icon;
			return _guiContent;
		}

		public static bool OpenFile(string file, int line = 0)
		{
			try
			{
				var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(file);
				if (asset == null)
				{
					return false;
				}

				if (line > 0)
				{
					AssetDatabase.OpenAsset(asset, line);
				}
				else
				{
					AssetDatabase.OpenAsset(asset);
				}
			}
			catch
			{
				return false;
			}

			return true;
		}
	}
}