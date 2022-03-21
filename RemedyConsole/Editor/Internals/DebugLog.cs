using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Remedy.Internals.Log;

namespace Remedy.Internals
{
	///<summary>
	/// Reflectioned the shit out of UnityEditorInternal.LogEntries
	///</summary>
	public class DebugLog
	{
		private int _oldFlags;

		public event EventHandler OnCountChanged;
		public event EventHandler OnEntryAdded;

		private int _countOfLogEntries;
		public bool ShowIgnored { get; set; }

		private readonly Dictionary<int, LogEntry> _hashMap;
		private readonly List<LogEntry> _internalEntries;
		private readonly Stack<LogEntry> _addedEntries;
		private readonly Stack<LogEntry> _changedEntries;
		private List<LogEntry> _entries;

		// reflection
		private readonly Type _logEntryType;
		private readonly MethodInfo _getEntryCountMethod;
		private readonly MethodInfo _startGettingEntriesMethod;
		private readonly MethodInfo _getEntryInternalMethod;
		private readonly MethodInfo _getCountsByTypeMethod;
		private readonly MethodInfo _endGettingEntriesMethod;
		private readonly MethodInfo _clearMethod;
		private readonly PropertyInfo _consoleFlagsProperty;
		// TODO: LOOK INTO THIS FUNCTION >>> public static extern void RowGotDoubleClicked(int index);
		
		public bool NeedListRebuild { get; private set; }
		private bool _reverse;

		public DebugLog()
		{
			_internalEntries = new List<LogEntry>();
			NeedListRebuild = true;
			_hashMap = new Dictionary<int, LogEntry>();
			_addedEntries = new Stack<LogEntry>();
			_changedEntries = new Stack<LogEntry>();
			_filters = new Dictionary<string, IDebugLogFilter>();

			System.Reflection.Assembly assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
			Type logEntriesType = assembly.GetType("UnityEditorInternal.LogEntries");
			_logEntryType = logEntriesType.Assembly.GetType("UnityEditorInternal.LogEntry");

			// reflection methods
			_startGettingEntriesMethod = logEntriesType.GetMethod("StartGettingEntries");
			_endGettingEntriesMethod = logEntriesType.GetMethod("EndGettingEntries");
			_getEntryCountMethod = logEntriesType.GetMethod("GetEntryCount");
			_getEntryInternalMethod = logEntriesType.GetMethod("GetEntryInternal");
			_getCountsByTypeMethod = logEntriesType.GetMethod("GetCountsByType");
			_clearMethod = logEntriesType.GetMethod("Clear");

			_consoleFlagsProperty = logEntriesType.GetProperty("consoleFlags");


			Update();
		}

		public bool Reverse
		{
			get { return _reverse; }
			set
			{
				if (_reverse != value)
				{
					_reverse = value;
					NeedListRebuild = true;
				}
			}
		}

		public int Length
		{
			get
			{
				UpdateFilteredList();
				return _entries.Count;
			}
		}

		public LogEntry this[int index]
		{
			get
			{
				UpdateFilteredList();
				if (index >= _entries.Count)
				{
					return new LogEntry("", 0, "", 0, 0, 0, 0, 0);
				}
				return _entries[index];
			}
		}

		protected void UpdateFilteredList()
		{
			if (!NeedListRebuild && _entries != null)
			{
				return;
			}

			var list = _internalEntries.Where(IsEntryVisible);

			if (_reverse)
			{
				list = list.Reverse();
			}

			_entries = list.ToList();
			NeedListRebuild = false;
		}

		private bool IsEntryVisible(LogEntry entry)
		{
			return _filters.Values.All(debugLogFilter => debugLogFilter.IsEntryVisible(entry));
		}

		public void Update(Mode mode = Mode.None)
		{
			// Change on console params needs a rebuild (collapse, filtering..)
			var flags = (int) _consoleFlagsProperty.GetValue(null, null);
			if (flags != _oldFlags)
			{
				mode = Mode.Rebuild;
				_oldFlags = flags;
			}

			try
			{
				int rows = StartGettingEntries();


				// if there are less entries, than before => rebuild list
				if (rows < _countOfLogEntries && mode != Mode.Rebuild)
				{
					mode = Mode.Rebuild;
				}
				// if there are new entries, than before => add them
				else if (rows > _countOfLogEntries && mode == Mode.None)
				{
					mode = Mode.Add;
				}

				_countOfLogEntries = rows;

				if (mode != Mode.None)
				{
					NeedListRebuild = Rebuild(mode);
				}
			}
			finally
			{
				EndGettingEntries();
			}

			while (_changedEntries.Count > 0)
			{
				var entry = _changedEntries.Pop();
				if (OnCountChanged != null)
				{
					OnCountChanged(entry, EventArgs.Empty);
				}
			}

			while (_addedEntries.Count > 0)
			{
				var entry = _addedEntries.Pop();
				if (OnEntryAdded != null)
				{
					OnEntryAdded(entry, EventArgs.Empty);
				}
			}
		}

		private bool Rebuild(Mode mode = Mode.Add)
		{
			var needRebuild = true;
			if (mode == Mode.Add)
			{
				if (_internalEntries.Count == _countOfLogEntries)
				{
					for (var i = 0; i < _internalEntries.Count; i++)
					{
						var entry = _internalEntries[i];
						var count = GetEntryCount(i);
						if (count != entry.Count)
						{
							entry.Count = count;
							if (OnCountChanged != null)
							{
								_changedEntries.Push(entry);
							}
						}
					}
				}
				else
				{
					for (var i = _internalEntries.Count; i < _countOfLogEntries; i++)
					{
						var entry = GetEntry(i);
						if (entry != null)
						{
							entry.Time = Time.time;
							_internalEntries.Add(entry);

							if (_entries != null && IsEntryVisible(entry))
							{
								_entries.Add(entry);
								if (OnEntryAdded != null)
								{
									_addedEntries.Push(entry);
								}
							}
						}
					}
				}

				needRebuild = false;
			}
			else if (mode == Mode.Rebuild)
			{
				_internalEntries.Clear();
				for (var i = 0; i < _countOfLogEntries; i++)
				{
					var entry = GetEntry(i);
					if (entry != null)
					{
						_internalEntries.Add(entry);
					}
				}
			}

			_countOfLogEntries = _internalEntries.Count;
			return needRebuild;
		}

		public enum Mode
		{
			None,
			Add,
			Rebuild
		}

		#region Reflection

		public void Clear()
		{
			_clearMethod.Invoke(null, null);
			ClearLists();
		}

		protected void ClearLists()
		{
			_internalEntries.Clear();
			_hashMap.Clear();
			_entries.Clear();
			_countOfLogEntries = 0;
			NeedListRebuild = true;
		}

		protected int StartGettingEntries()
		{
			return (int) _startGettingEntriesMethod.Invoke(null, null);
		}

		protected void EndGettingEntries()
		{
			_endGettingEntriesMethod.Invoke(null, null);
		}

		protected LogEntry GetEntry(int num)
		{
			LogEntry entry;
			var output = Activator.CreateInstance(_logEntryType, true);
			var parameters = new[] {num, output};
			_getEntryInternalMethod.Invoke(null, parameters);

			var condition = GetPropertyValue<string>(output, "condition");
			var errorNum = GetPropertyValue<int>(output, "errorNum");
			var file = GetPropertyValue<string>(output, "file");
			var line = GetPropertyValue<int>(output, "line");
			var instanceId = GetPropertyValue<int>(output, "instanceID");
			var identifier = GetPropertyValue<int>(output, "identifier");

			var hash = LogEntry.CalculateHashCode(condition, errorNum, file, line, instanceId, identifier);
			if (_hashMap.ContainsKey(hash))
			{
				entry = _hashMap[hash];
			}
			else
			{
				entry = new LogEntry(condition, errorNum, file, line, GetPropertyValue<int>(output, "mode"), instanceId, identifier, GetPropertyValue<int>(output, "isWorldPlaying"));
				_hashMap.Add(hash, entry);
				_addedEntries.Push(entry);
			}

			entry.Count = GetEntryCount(num);

			return entry;
		}

		protected int GetEntryCount(int row)
		{
			return (int) _getEntryCountMethod.Invoke(null, new object[] {row});
		}

		protected static TReturnType GetPropertyValue<TReturnType>(object src, string name)
		{
			var prop = src.GetType().GetField(name, (BindingFlags) (-1));
			if (prop != null) return (TReturnType) prop.GetValue(src);
			return default(TReturnType);
		}

		public void GetCountsByType(ref int errorCount, ref int warningCount, ref int logCount)
		{
			var args = new object[] {errorCount, warningCount, logCount};
			_getCountsByTypeMethod.Invoke(null, args);
			errorCount = (int) args[0];
			warningCount = (int) args[1];
			logCount = (int) args[2];
		}

		#endregion

		#region Filters
		private readonly Dictionary<string, IDebugLogFilter> _filters;

#endregion

		public int GetLastIndexOf(LogEntry entry)
		{
			UpdateFilteredList();
			return _entries.LastIndexOf(entry);
		}
	}
}