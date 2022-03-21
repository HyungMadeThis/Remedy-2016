using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Remedy.Internals
{
	public static class RemedyConstants
	{
		public static readonly GUIStyle Toolbar;
		public static readonly GUIStyle MiniButton;
		public static readonly GUIStyle PlusButton;
		public static readonly GUIStyle Box;
		public static readonly GUIStyle Button;
		public static readonly GUIStyle LogStyle;
		public static readonly GUIStyle WarningStyle;
		public static readonly GUIStyle ErrorStyle;
		public static readonly GUIStyle TerminalStyle;
		public static readonly GUIStyle EvenBackground;
		public static readonly GUIStyle OddBackground;
		public static readonly GUIStyle MessageStyle;
		public static readonly GUIStyle CountBadge;

		public static readonly GUIStyle ListStyle;
		public static readonly GUIStyle ListStyleInactive;
		public static GUIStyle ToolbarPopup;

		public static Texture2D InfoIcon;
		public static Texture2D ConsoleInfoIcon;
		public static Texture2D InfoIconSmall;
		public static Texture2D ConsoleInfoIconSmall;
		public static Texture2D InfoIconMono;
		public static Texture2D WarnIcon;
		public static Texture2D ConsoleWarnIcon;
		public static Texture2D WarnIconSmall;
		public static Texture2D ConsoleWarnIconSmall;
		public static Texture2D WarnIconMono;
		public static Texture2D ErrorIcon;
		public static Texture2D ConsoleErrorIcon;
		public static Texture2D ErrorIconSmall;
		public static Texture2D ConsoleErrorIconSmall;
		public static Texture2D ErrorIconMono;

		public static Color TerminalColor = Color.magenta;


		static RemedyConstants()
		{
			Toolbar = new GUIStyle("Toolbar");
			Box = new GUIStyle("CN Box");
			Button = new GUIStyle("Button");
			MiniButton = new GUIStyle("ToolbarButton");
			PlusButton = new GUIStyle("OL Plus");

			LogStyle = new GUIStyle("CN EntryInfo");
			LogStyle.fontStyle = FontStyle.Bold;
			WarningStyle = new GUIStyle("CN EntryWarn");
			//WarningStyle.fontStyle = FontStyle.Bold;
			ErrorStyle = new GUIStyle("CN EntryError");
			//ErrorStyle.fontStyle = FontStyle.Bold;
			TerminalStyle = new GUIStyle("CN EntryInfo");
			//TerminalStyle.fontStyle = FontStyle.Bold;

			EvenBackground = new GUIStyle("CN EntryBackEven");
			OddBackground = new GUIStyle("CN EntryBackodd");
			MessageStyle = new GUIStyle("CN Message");
			CountBadge = new GUIStyle("CN CountBadge");

			ToolbarPopup = new GUIStyle(GUI.skin.FindStyle("ToolbarPopup"));
			
			ListStyle = new GUIStyle("Pr Label") {fixedHeight = 16};
			ListStyleInactive = new GUIStyle(ListStyle)
			{
				normal = {textColor = Color.gray},
				onNormal = {textColor = new Color(255,255,255, 0.5f)}, 
				focused = {textColor = Color.gray},
				onFocused = {textColor = new Color(255, 255, 255, 0.5f)},
			};
			LoadTextures();
		}

		public static bool LoadTextures()
		{
			var returning = false;
			if (ConsoleErrorIcon == null)
			{
				ConsoleErrorIcon = LoadTextureFromFile("Assets/RemedyConsole/Editor/Resources/console.erroricon.png");
				ConsoleErrorIconSmall = LoadTextureFromFile("Assets/RemedyConsole/Editor/Resources/console.erroricon.sml.png");
				ConsoleInfoIcon = LoadTextureFromFile("Assets/RemedyConsole/Editor/Resources/console.infoicon.png");
				ConsoleInfoIconSmall = LoadTextureFromFile("Assets/RemedyConsole/Editor/Resources/console.infoicon.sml.png");
				ConsoleWarnIcon = LoadTextureFromFile("Assets/RemedyConsole/Editor/Resources/console.warnicon.png");
				ConsoleWarnIconSmall = LoadTextureFromFile("Assets/RemedyConsole/Editor/Resources/console.warnicon.sml.png");
				returning = true;
			}


			if (InfoIcon == null)
			{
				InfoIcon = LoadIcon("console.infoicon");
				InfoIconSmall = LoadIcon("console.infoicon.sml");
				InfoIconMono = InfoIconSmall;
				WarnIcon = LoadIcon("console.warnicon");
				WarnIconSmall = LoadIcon("console.warnicon.sml");
				WarnIconMono = LoadIcon("console.warnicon.inactive.sml");
				ErrorIcon = LoadIcon("console.erroricon");
				ErrorIconSmall = LoadIcon("console.erroricon.sml");
				ErrorIconMono = LoadIcon("console.erroricon.inactive.sml");
				returning = true;
			}

			return returning;
		}


		private static Texture2D LoadIcon(string name)
		{
			var type = typeof(EditorGUIUtility);
			var loadIcon = type.GetMethod("LoadIcon", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
			return loadIcon.Invoke(null, new object[] { name }) as Texture2D;
		}

		private static Texture2D LoadTextureFromFile(string file)
		{
			var texture = new Texture2D(1, 1);
			texture.LoadImage(System.IO.File.ReadAllBytes(file));
			texture.Apply();
			return texture;
		}

		/// <summary>
		/// Returns the GUIStyle for a log entry.
		/// </summary>
		public static GUIStyle GetStyleForLogEntry(LogEntry logEntry)
		{
			if (logEntry.LogMode == LogEntry.LogModeEnum.Error)
			{
				return ErrorStyle;
			}
			else if(logEntry.LogMode == LogEntry.LogModeEnum.Warning)
			{
				return WarningStyle;
			}
			else if(logEntry.LogMode == LogEntry.LogModeEnum.Terminal)
			{
				return TerminalStyle;
			}
			else
			{
				return LogStyle;
			}
		}

		/// <summary>
		/// Log Entry colors when displayed on the console.
		/// </summary>
		public static string GetLogEntryColor(LogEntry logEntry)
		{
            if (logEntry.LogMode == LogEntry.LogModeEnum.Error)
            {
                return "<color=red>";
            }
            else if (logEntry.LogMode == LogEntry.LogModeEnum.Warning)
            {
                return "<color=orange>";
            }
            else if (logEntry.LogMode == LogEntry.LogModeEnum.Log)
            {
                return "<color=grey>";
            }
            else if (logEntry.LogMode == LogEntry.LogModeEnum.Terminal)
            {
                return "<color=magenta>";
            }
            else
            {
                return "<color=black>";
            }
		}

		/// <summary>
		/// Detail line color. Potentially could have different Detail line colors for every LogEntry type.
		/// </summary>
		public static string GetSecondLineColor(LogEntry logEntry)
		{
			return "<color=grey>";
		}
	}
}