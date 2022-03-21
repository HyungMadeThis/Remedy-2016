using System;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Remedy.GUIElements;
using Remedy.Internals;
using Remedy.Internals.Detail;

namespace Remedy
{
	public class RemedyConsole : EditorWindow, IHasCustomMenu {  
		private static RemedyConsole _instance;
		private ConsoleFlags m_consoleFlags;

		private ListViewGUI m_listView;
		private SplitterState m_splitterState;
		private SplitterState m_detailsSplitterState;
		private LogEntry m_activeEntry;
		private GUIContent m_textGUIContent;
		private LogDetailView m_logDetailView;
		private FileDetailView m_fileDetailView;
		private LogHandler m_logger;
		private bool m_smallList;
		private bool m_isEditor = false;
		private string m_terminalCmd = "";
		private bool m_editorIsCompiling = false;

		//Settings_PATH can't be in RemedyConstants because it causes the constructor to be called too early 
		// and we get an error when trying to initialize "ToolbarPopup" b/c it has a GUI call.
		public static string Settings_PATH = "Assets/RemedyConsole/Editor/settings.asset";


		private Type m_logEntriesType;	//reflection
		private MethodInfo m_setConsoleFlagMethod; 

		#region Init
		//========================Init=========================//
		/// <summary>
		/// Singleton Instance
		/// </summary>
		public static RemedyConsole Instance
		{
			get { return _instance ?? (_instance = GetWindow<RemedyConsole>()); }
		}

		[MenuItem("Window/Remedy")]
		public static void Init()
		{
			Instance.Show(true);
			Instance.Focus();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public RemedyConsole()
		{
			InitConsole();
		}

		/// <summary>
		/// Initialized Settings, Properties and do the reflection stuff
		/// </summary>
		private void InitConsole()
		{
			if (m_listView != null)
			{
				return; 
			}
			m_listView = new ListViewGUI(new ListViewState(0, 32));
			m_consoleFlags = ConsoleFlags.Collapse;

			m_splitterState = new SplitterState(new[] { 70f, 30f }, new[] { 32, 50 }, null);
			m_detailsSplitterState = new SplitterState(new[] { 70f, 30f }, new[] { 18, 18 }, null);

			m_logDetailView = new LogDetailView(this);
			m_fileDetailView = new FileDetailView(this);

			m_textGUIContent = new GUIContent();

			// Reflection
			var assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
			m_logEntriesType = assembly.GetType("UnityEditorInternal.LogEntries");
			m_setConsoleFlagMethod = m_logEntriesType.GetMethod("SetConsoleFlag");
		}
		
		/// <summary>
		/// Sets title, flags and settings. (should this be in OnGUI?)(nah)
		/// </summary>
		protected void OnEnable()
		{ 
			_instance = this;
			InitConsole();
			InitFlags();

			var loadIconMethod = typeof(EditorGUIUtility).GetMethod("LoadIcon", BindingFlags.NonPublic | BindingFlags.Static);
			titleContent.image = (Texture2D) loadIconMethod.Invoke(null, new object[] { "UnityEditor.ConsoleWindow" });
			titleContent.text = "Remedy";

			if (DebugLog == null)
			{
				DebugLog = new DebugLog();
				DebugLog.OnEntryAdded += OnDebugLogAdded;
				DebugLog.OnCountChanged += OnDebugLogCountChanged;

				m_logger = new LogHandler(Debug.logger.logHandler, DebugLog);
				Debug.logger.logHandler = m_logger;
			}
			LoadSettings();
		}

		protected void OnDisable()
		{
			AssetDatabase.SaveAssets();
			if (DebugLog != null)
			{
				DebugLog.OnCountChanged -= OnDebugLogCountChanged;
				DebugLog.OnEntryAdded -= OnDebugLogAdded;
			}
		}
		//=====================================================//
		#endregion

		#region GetSet
		//================Getters and Setters==================//
		/// <summary>
		/// Settings scriptable object
		/// </summary>
		public Settings Settings { get; private set; }

		///<summary>
		/// DebugLog reference
		///</summary>
		public DebugLog DebugLog { get; private set; }

		/// <summary>
		/// SmallList option
		/// </summary>
		public bool SmallList
		{
			get { return m_smallList; }
			private set
			{
				if (m_smallList != value)
				{
					m_smallList = value;

					UpdateConsoleStyles();
				}
			}
		}

		//=====================================================//
		#endregion

		#region OnGUI
		//========================ONGUI========================//
		protected void OnGUI()
		{
			var currentEvent = Event.current;
			LoadIcons();

			DrawToolbar();

			SplitterGUILayout.BeginVerticalSplit(m_splitterState);

			DrawTopPanel();
			DrawBottomPanel();

			SplitterGUILayout.EndVerticalSplit();

			DrawTerminal();
			HandleCopyEvent(currentEvent);
		}

		private void DrawToolbar() 
		{
			GUILayout.BeginHorizontal(RemedyConstants.Toolbar);
			if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
        	{
				DebugLog.Clear();
     		}
			EditorGUILayout.Space();
			Collapse = GUILayout.Toggle(Collapse, "Collapse", EditorStyles.toolbarButton);
			ClearOnPlay = GUILayout.Toggle(ClearOnPlay, "Clear on Play", EditorStyles.toolbarButton);
			ErrorPause = GUILayout.Toggle(ErrorPause, "Error Pause", EditorStyles.toolbarButton);

			GUILayout.FlexibleSpace();

			int errorCount = 0, warningCount = 0, logCount = 0;
			DebugLog.GetCountsByType(ref errorCount, ref warningCount, ref logCount);
			EditorGUILayout.Space();

			// Log Levels
			LogLevelLog = GUILayout.Toggle(
				LogLevelLog,
				new GUIContent(
					logCount > 999 ? "999+" : logCount.ToString(),
					logCount == 0 ? RemedyConstants.InfoIconMono : RemedyConstants.InfoIconSmall),
				RemedyConstants.MiniButton);
			LogLevelWarning = GUILayout.Toggle(
				LogLevelWarning,
				new GUIContent(
					warningCount > 999 ? "999+" : warningCount.ToString(),
					warningCount == 0 ? RemedyConstants.WarnIconMono : RemedyConstants.WarnIconSmall),
				RemedyConstants.MiniButton);
			LogLevelError = GUILayout.Toggle(
				LogLevelError,
				new GUIContent(
					errorCount > 999 ? "999+" : errorCount.ToString(),
					errorCount == 0 ? RemedyConstants.ErrorIconMono : RemedyConstants.ErrorIconSmall),
				RemedyConstants.MiniButton);
			GUILayout.EndHorizontal();
		}

		private void DrawTopPanel()
		{
			var currentEvent = Event.current;
			var controlId = GUIUtility.GetControlID(FocusType.Passive);

			DebugLog.Update(); 
			var rows = DebugLog.Length;

			m_listView.State.TotalRows = rows;

			m_listView.ListView(RemedyConstants.Box, listViewElement => { 
				LogEntry entry = DebugLog[listViewElement.Row];
				GUIStyle bg = listViewElement.Row % 2 == 0 ? (RemedyConstants.EvenBackground) : RemedyConstants.OddBackground;
				bool active = m_listView.State.Row == listViewElement.Row;
				Rect contentRect = listViewElement.Position;

				if (currentEvent.type == EventType.ContextClick)
				{
					if (contentRect.Contains(currentEvent.mousePosition))
					{
						ShowContextMenu(entry);
						currentEvent.Use();
					}
				}

				if (currentEvent.type == EventType.Repaint)
				{
					m_textGUIContent.text = FormatContent(entry,
						m_smallList ? entry.GetFirstLine() : entry.GetFirstTwoLines(), 
						active);
						
						//If mode is Log, we will display the filename//
						if(Settings.ShowLogCallFile && entry.LogMode == LogEntry.LogModeEnum.Log)
						{
							m_textGUIContent.text = "[" + entry.Basename + "]\t" + m_textGUIContent.text;
						}


					bg.Draw(contentRect, false, false, active, false);
					var style = RemedyConstants.GetStyleForLogEntry(entry);
					style.Draw(contentRect, m_textGUIContent, controlId, active);

					if (Collapse)
					{
						var badgePosition = listViewElement.Position;
						m_textGUIContent.text = entry.Count.ToString();
						var badgeSize = RemedyConstants.CountBadge.CalcSize(m_textGUIContent);
						badgePosition.xMin = badgePosition.xMax - badgeSize.x;
						badgePosition.yMin += (badgePosition.yMax - badgePosition.yMin - badgeSize.y)*0.5f;
                        badgePosition.x -= 5;
                        GUI.Label(badgePosition, m_textGUIContent, RemedyConstants.CountBadge);
                    }
                }
				//Double click to open file that called the line.
                else if (currentEvent.type == EventType.MouseDown && currentEvent.clickCount == 2)
                {
					//Find the first file that is accessable and access it.
                    FileEntry[] fileEntries = entry.Files;
                    FileEntry file = null;
                    for (int i = 0; i < fileEntries.Length; i++)
                    {
                        if (fileEntries[i].IsInAssetDirectory)
                        {
                            file = fileEntries[i];
                            break;
                        }
                    }
                    if (file != null)
                        Helper.OpenFile(file.Name, file.Line);
                }

            });

			if (m_listView.State.TotalRows == 0 || m_listView.State.Row >= m_listView.State.TotalRows || m_listView.State.Row < 0)
			{
				SetActiveEntry(null);
			}
			else
			{
				SetActiveEntry(DebugLog[m_listView.State.Row]);
			}
		}

		private void DrawBottomPanel()
		{
			if (Settings.ShowFilesInDetails)
            {
                GUILayout.BeginVertical();

                SplitterGUILayout.BeginHorizontalSplit(m_detailsSplitterState);

                GUILayout.BeginVertical();
                var detailView = m_logDetailView;
                detailView.OnGUI(m_activeEntry);
                GUILayout.EndVertical();

				//Split here//
				
				GUILayout.BeginVertical();
				GUILayout.Space(1);
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("Files");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Space(1);
                var filesView = m_fileDetailView;
                filesView.OnGUI(m_activeEntry);
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();

                SplitterGUILayout.EndHorizontalSplit();

                GUILayout.EndVertical();
            }
            else
            {
				GUILayout.BeginVertical();
                var detailView = m_logDetailView;
                detailView.OnGUI(m_activeEntry);
                GUILayout.EndVertical();
            }
		}

 		private void DrawTerminal()
 		{
			GUI.color = Color.gray;
			GUILayout.BeginVertical(GUILayout.Height(30));
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.Label(">", GUILayout.Width(10));
			Color color = GUI.color;
			GUI.color = RemedyConstants.TerminalColor;
 			m_terminalCmd =  GUILayout.TextField(m_terminalCmd, GUILayout.ExpandWidth(true));
			 color = GUI.color;
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();

			if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
			{
				if(m_terminalCmd != "")
				{
					RemedyTerminal.NewCommand(m_terminalCmd);
					m_terminalCmd = ""; 
				}
    		}
		}

		/// <summary>
		/// TopRight options menu items
		/// </summary>
		public void AddItemsToMenu(GenericMenu menu)
		{
			menu.AddItem(new GUIContent("Select Last Changed"), Settings.SelectLastChanged, () =>
			{
				Settings.SelectLastChanged = !Settings.SelectLastChanged;
			});

			menu.AddItem(new GUIContent("Show Associated Files"), Settings.ShowFilesInDetails, () =>
			{
				Settings.ShowFilesInDetails = !Settings.ShowFilesInDetails;
			});

			menu.AddItem(new GUIContent("Show File Name In Logs"), Settings.ShowLogCallFile, () =>
			{
				Settings.ShowLogCallFile = !Settings.ShowLogCallFile;
			});

			menu.AddItem(new GUIContent("Small List Entries"), SmallList, () =>
			{
				SmallList = !SmallList;
				Settings.SmallList = SmallList;
			});

			menu.AddItem(new GUIContent("Reverse Order"), DebugLog.Reverse, () =>
			{
				DebugLog.Reverse = !DebugLog.Reverse;
				Settings.ReverseOrder = DebugLog.Reverse;
			});

			menu.AddItem(new GUIContent("Open Editor Log"), false, InternalEditorUtility.OpenEditorConsole);
		}

		/// <summary>
		/// RightClick options
		/// </summary>
		private void ShowContextMenu(LogEntry entry)
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Copy"), false, OnContextMenuCopyClick, entry);

			menu.AddSeparator(string.Empty);
			menu.AddItem(new GUIContent("Ignore this file"), false, OnContextMenuIgnoreFileClick, entry);
			if (!string.IsNullOrEmpty(entry.CS))
			{
				menu.AddItem(new GUIContent("Ignore " + entry.CS), false, OnContextmenuIgnoreCSClick, entry);
			}

			menu.AddSeparator(string.Empty);

			if (entry.StackTrace.Length > 0)
			{
				foreach (var stackEntry in entry.StackTrace)
				{
					var menuItemTitle = stackEntry.ClassName + "." + stackEntry.MethodName;
					menu.AddItem(new GUIContent("Stack Trace/" + menuItemTitle), false, OnContextMenuStackEntryClick, stackEntry);
				}
			}
			else
			{
				foreach (var file in entry.Files)
				{
					var menuItemTitle = file.Basename;
					menu.AddItem(new GUIContent("Files/" + menuItemTitle), false, OnContextMenuFileClick, file);
				}
			}
			menu.ShowAsContext();
		}
		//=====================================================//
		#endregion

		#region LoadSettings
		//===================Load Settings=====================//
		/// <summary>
		/// Loads settings from ScriptableObject
		/// </summary>
		protected void LoadSettings()
		{
			Settings = AssetDatabase.LoadAssetAtPath<Settings>(Settings_PATH);
			if (Settings == null)
			{
				Settings = CreateInstance<Settings>();

				AssetDatabase.CreateAsset(Settings, Settings_PATH);
				AssetDatabase.SaveAssets();
			}

			SmallList = Settings.SmallList = false;
			DebugLog.Reverse = Settings.ReverseOrder;

		}

		/// <summary>
		/// Loads icons from RemedyConstants
		/// </summary>
		private void LoadIcons()
		{
			if(!m_isEditor && !Application.isPlaying)
			{
				if (RemedyConstants.LoadTextures())
				{
					UpdateConsoleStyles();
				}
				m_isEditor = true;
			}
			else if(m_isEditor && Application.isPlaying)
			{
				m_isEditor = false;
			}
		}

		/// <summary>
		/// Updates the GUIStyle's used to draw an console item. Handles the switch between Small and Large
		/// view.
		/// </summary>
		private void UpdateConsoleStyles()
		{
			m_listView.State.RowHeight = m_smallList ? 20 : 32;

			GUIStyle style = RemedyConstants.LogStyle;
			style.normal.background = m_smallList ? RemedyConstants.ConsoleInfoIconSmall : RemedyConstants.ConsoleInfoIcon;
			style.onNormal.background = style.normal.background;
			style.padding.left = m_smallList ? 20 : 32;
			style.border = m_smallList ? new RectOffset(20, 2, 2, 2) : new RectOffset(30, 3, 3, 3);
			style.fixedHeight = m_smallList ? 20 : 32;

			style = RemedyConstants.WarningStyle;
			style.normal.background = m_smallList ? RemedyConstants.ConsoleWarnIconSmall : RemedyConstants.ConsoleWarnIcon;
			style.onNormal.background = style.normal.background;
			style.padding.left = m_smallList ? 20 : 32;
			style.border = m_smallList ? new RectOffset(20, 2, 2, 2) : new RectOffset(30, 3, 3, 3);
			style.fixedHeight = m_smallList ? 20 : 32;

			style = RemedyConstants.ErrorStyle;
			style.normal.background = m_smallList ? RemedyConstants.ConsoleErrorIconSmall : RemedyConstants.ConsoleErrorIcon;
			style.onNormal.background = style.normal.background;
			style.padding.left = m_smallList ? 20 : 32;
			style.border = m_smallList ? new RectOffset(20, 2, 2, 2) : new RectOffset(30, 3, 3, 3);
			style.fixedHeight = m_smallList ? 20 : 32;

			style = RemedyConstants.TerminalStyle;
			style.normal.background = m_smallList ? RemedyConstants.ConsoleInfoIconSmall : RemedyConstants.ConsoleInfoIcon;
			style.onNormal.background = style.normal.background;
			style.padding.left = m_smallList ? 20 : 32;
			style.border = m_smallList ? new RectOffset(20, 2, 2, 2) : new RectOffset(30, 3, 3, 3);
			style.fixedHeight = m_smallList ? 20 : 32;

			Repaint();
		}
		//=====================================================//
		#endregion

		#region EverythingElse
		//=====================================================//
		private void OnDebugLogCountChanged(object sender, EventArgs e)
		{
			var entry = sender as LogEntry;
			if (entry == null) return;

			if (Settings.SelectLastChanged)
			{
				SelectEntry(entry);
			}
			Repaint();
		}

		private void OnDebugLogAdded(object sender, EventArgs e)
		{
			var entry = sender as LogEntry;
			if (entry == null) return;

			if (Settings.SelectLastChanged)
			{
				SelectEntry(entry);
			}
			Repaint();
		}

		private void SetActiveEntry(LogEntry entry)
		{
			if (entry != null)
			{
				var currentEntry = m_activeEntry;
				m_activeEntry = entry;

				if (currentEntry != null && currentEntry.InstanceID == entry.InstanceID)
				{
					return;
				}

				if (entry.InstanceID != 0)
				{
					EditorGUIUtility.PingObject(entry.InstanceID);
				}
			}
			else
			{
				m_activeEntry = null;
			}
		}

		/// <summary>
		/// Selects an entry in the console
		/// </summary>
		private void SelectEntry(LogEntry entry)
		{
			// if(Event.current.type != EventType.Repaint)
			// {
			// 	return;
			// }
			var index = DebugLog.GetLastIndexOf(entry);
			m_listView.State.Row = index;
			m_listView.State.ScrollPos = new Vector2(0, index*m_listView.State.RowHeight);
		}

		/// <summary>
		/// Formats the content
		/// </summary>
		public string FormatContent(LogEntry entry, string content, bool active)
		{
			// First line: default color -> next lines grey/lightblue (depends on active)
			var index = content.IndexOf("\n", StringComparison.Ordinal);
			if (index > 0)
            {
                StringBuilder builder = new StringBuilder(content.Length + 20);
				builder.Append(RemedyConstants.GetLogEntryColor(entry));
				builder.Append(content.Substring(0, index));
				builder.Append("</color>");
				builder.Append(RemedyConstants.GetSecondLineColor(entry));
				builder.Append(content.Substring(index));
				builder.Append("</color>");
				content = builder.ToString();
			}
			else
			{
				StringBuilder builder = new StringBuilder(content.Length + 20);
				builder.Append(RemedyConstants.GetLogEntryColor(entry));
				builder.Append(content);
				builder.Append("</color>");
				content = builder.ToString();
			}
			return content;
		}

		private void OnContextmenuIgnoreCSClick(object userdata)
		{
			if (!(userdata is LogEntry))
			{
				return;
			}
			DebugLog.Update(DebugLog.Mode.Rebuild);
			Repaint();
		}

		private void OnContextMenuIgnoreFileClick(object userdata)
		{
			if (!(userdata is LogEntry))
			{
				return;
			}
			DebugLog.Update(DebugLog.Mode.Rebuild);
			Repaint();
		}

		private static void OnContextMenuFileClick(object item)
		{
			if (!(item is FileEntry))
			{
				return;
			}

			var entry = (FileEntry)item;
			var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(entry.Name);
			AssetDatabase.OpenAsset(asset, entry.Line);
		}

		private static void OnContextMenuCopyClick(object item)
		{
			var entry = item as LogEntry;
			if (entry != null)
			{
				EditorGUIUtility.systemCopyBuffer = entry.Condition;
			}
		}

		private static void OnContextMenuStackEntryClick(object item)
		{
			if (!(item is StackTraceEntry))
			{
				return;
			}

			var entry = (StackTraceEntry)item;
			var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(entry.Name);
			AssetDatabase.OpenAsset(asset, entry.Line);
		}

		private void HandleCopyEvent(Event currentEvent)
		{
			if (m_activeEntry == null)
			{
				return;
			}

			var text = m_activeEntry.Condition;
			if (currentEvent.type != EventType.ValidateCommand && currentEvent.type != EventType.ExecuteCommand ||
				currentEvent.commandName != "Copy" || text == string.Empty)
			{
				return;
			}

			if (currentEvent.type == EventType.ExecuteCommand)
			{
				EditorGUIUtility.systemCopyBuffer = text;
			}

			currentEvent.Use();
		}
		//=====================================================//
		#endregion

		#region Flags
		//=======================Flags=========================//
		[Flags]
		public enum ConsoleFlags
		{
			Collapse = 1,
			ClearOnPlay = 2,
			ErrorPause = 4,
			Verbose = 8,
			StopForAssert = 16,
			StopForError = 32,
			Autoscroll = 64,
			LogLevelLog = 128,
			LogLevelWarning = 256,
			LogLevelError = 512,
		}
		public bool Collapse
		{
			get { return (m_consoleFlags & ConsoleFlags.Collapse) == ConsoleFlags.Collapse; }
			set
			{
				SetConsoleFlag(ConsoleFlags.Collapse, value);
			}
		}
		public bool ClearOnPlay
		{
			get { return (m_consoleFlags & ConsoleFlags.ClearOnPlay) == ConsoleFlags.ClearOnPlay; }
			set
			{
				SetConsoleFlag(ConsoleFlags.ClearOnPlay, value);
			}
		}
		public bool ErrorPause
		{
			get { return (m_consoleFlags & ConsoleFlags.ErrorPause) == ConsoleFlags.ErrorPause; }
			set
			{
				SetConsoleFlag(ConsoleFlags.ErrorPause, value);
			}
		}
		public bool Autoscroll
		{
			get { return (m_consoleFlags & ConsoleFlags.Autoscroll) == ConsoleFlags.Autoscroll; }
			set
			{
				SetConsoleFlag(ConsoleFlags.Autoscroll, value);
			}
		}
		public bool LogLevelError
		{
			get { return (m_consoleFlags & ConsoleFlags.LogLevelError) == ConsoleFlags.LogLevelError; }
			set
			{
				SetConsoleFlag(ConsoleFlags.LogLevelError, value);
			}
		}
		public bool LogLevelLog
		{
			get { return (m_consoleFlags & ConsoleFlags.LogLevelLog) == ConsoleFlags.LogLevelLog; }
			set
			{
				SetConsoleFlag(ConsoleFlags.LogLevelLog, value);
			}
		}
		public bool LogLevelWarning
		{
			get { return (m_consoleFlags & ConsoleFlags.LogLevelWarning) == ConsoleFlags.LogLevelWarning; }
			set
			{
				SetConsoleFlag(ConsoleFlags.LogLevelWarning, value);
			}
		}
		public bool StopForAssert
		{
			get { return (m_consoleFlags & ConsoleFlags.StopForAssert) == ConsoleFlags.StopForAssert; }
			set
			{
				SetConsoleFlag(ConsoleFlags.StopForAssert, value);
			}
		}
		public bool StopForError
		{
			get { return (m_consoleFlags & ConsoleFlags.StopForError) == ConsoleFlags.StopForError; }
			set
			{
				SetConsoleFlag(ConsoleFlags.StopForError, value);
			}
		}
		public bool Verbose
		{
			get { return (m_consoleFlags & ConsoleFlags.Verbose) == ConsoleFlags.Verbose; }
			set
			{
				SetConsoleFlag(ConsoleFlags.Verbose, value);
			}
		}
		private void InitFlags()
		{
			var flagsProperty = m_logEntriesType.GetProperty("consoleFlags");
			var flags = flagsProperty.GetValue(null, null);
			m_consoleFlags = (ConsoleFlags)flags;
		}
		private void SetConsoleFlag(ConsoleFlags flag, bool value)
		{
			if (value)
				m_consoleFlags |= flag;
			else 
				m_consoleFlags &= ~flag;
			m_setConsoleFlagMethod.Invoke(null, new object[] {(int) flag, value});

			Repaint();
		} 		
		//=====================================================//
		#endregion
		public void Update()
		{
			//Updating the console after every compile. This is ugly AF but I can't think of any other way to do it.
			if(!m_editorIsCompiling && EditorApplication.isCompiling)
			{
				m_editorIsCompiling = !m_editorIsCompiling;
			}
			else if(m_editorIsCompiling && !EditorApplication.isCompiling)
			{
				//If compiling just finished, fake a SetConsoleFlag(ErrorPause) so the console will update properly. fml.
				m_editorIsCompiling = !m_editorIsCompiling;
				SetConsoleFlag(ConsoleFlags.ErrorPause, ErrorPause);
			}
		}
	}
}
