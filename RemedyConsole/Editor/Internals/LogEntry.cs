using System;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Remedy.Internals
{
	public class LogEntry
	{
		private string m_firstLine;
		private string m_firstTwoLines;
		private int ValidIntegrity;

		public string Condition{ get; set; }
		public int ErrorNum{ get; private set; }
		public string File { get; private set; }
		public int Line{ get; private set; }
		public LogEntryMode Mode{ get; set; }
		public int InstanceID{ get; private set; }
		public int Identifier{ get; private set; }
		public int IsWorldPlaying{ get; private set; }
		public float Time{ get; set; }
		public int Count{ get; set; }
		private List<StackTraceEntry> m_stackTrace;
		private List<FileEntry> m_files;
		private Texture m_icon;
		private string m_cs;

		private const LogEntryMode ModeError =
			LogEntryMode.Error | LogEntryMode.Assert | LogEntryMode.Fatal | LogEntryMode.AssetImportError | LogEntryMode.ScriptingError | LogEntryMode.ScriptCompileError |
			LogEntryMode.GraphCompileError | LogEntryMode.ScriptingAssertion;
		private const LogEntryMode ModeWarn = LogEntryMode.AssetImportWarning | LogEntryMode.ScriptingWarning | LogEntryMode.ScriptCompileWarning;
		private const LogEntryMode ModeLog = LogEntryMode.Log | LogEntryMode.ScriptingLog;
		private const LogEntryMode ModeTerminal = LogEntryMode.TerminalEntry;

		[Flags]
		public enum LogEntryMode
		{
			Error = 1,
			Assert = 2,
			Log = 4,
			Fatal = 16,
			DontPreprocessCondition = 32,
			AssetImportError = 64,
			AssetImportWarning = 128,
			ScriptingError = 256,
			ScriptingWarning = 512,
			ScriptingLog = 1024,
			ScriptCompileError = 2048,
			ScriptCompileWarning = 4096,
			StickyError = 8192,
			MayIgnoreLineNumber = 16384,
			ReportBug = 32768,
			DisplayPreviousErrorInStatusBar = 65536,
			ScriptingException = 131072,
			DontExtractStacktrace = 262144,
			ShouldClearOnPlay = 524288,
			GraphCompileError = 1048576,
			ScriptingAssertion = 2097152,
			TerminalEntry = 4194304,
		}

		public enum LogModeEnum
		{
			Info,
			Log,
			Warning,
			Error,
			Terminal
		}

		private LogEntry()
		{
			Time = UnityEngine.Time.time;
		}

		public LogEntry(string condition, int errorNum, string file, int line, int mode, int InstanceId, int identifier, int isWorldPlaying) : this()
		{
			Condition = condition;
			ErrorNum = errorNum;
			File = file;
			Line = line;
			Mode = (LogEntryMode) mode; 
			InstanceID = InstanceId;
			Identifier = identifier;
			IsWorldPlaying = isWorldPlaying;

			RemedyTerminal.CheckIfTerminalEntry(this);
		}

		public LogModeEnum LogMode
		{
			get
			{
				if ((Mode & ModeError) != 0)
				{
					return LogModeEnum.Error;
				}
				else if ((Mode & ModeWarn) != 0)
				{
					return LogModeEnum.Warning;
				}
				else if ((Mode & ModeLog) != 0)
				{
					return LogModeEnum.Log;
				}
				else if ((Mode & ModeTerminal) != 0)
				{
					return LogModeEnum.Terminal;
				}

				return LogModeEnum.Info;
			}
		}

		public string GetFirstLine()
		{
			if (m_firstLine != null)
			{
				return m_firstLine;
			}

			var index = Condition.IndexOf("\n", StringComparison.Ordinal);
			m_firstLine = index == -1 ? Condition : Condition.Substring(0, index);
			return m_firstLine;
		}

		public string GetFirstTwoLines()
		{
			if (m_firstTwoLines != null)
			{
				return m_firstTwoLines;
			}

			m_firstTwoLines = Condition;
			var index = Condition.IndexOf("\n", StringComparison.Ordinal);
			if (index != -1)
			{
				index = Condition.IndexOf("\n", index + 1, StringComparison.Ordinal);
				if (index != -1)
				{
					m_firstTwoLines = Condition.Substring(0, index);
				}
			}

			return m_firstTwoLines;
		}

		public FileEntry[] Files
		{
			get
			{
				if (m_files == null)
				{
					ParseCondition();
				}
				//List<FileEntry> reverseFiles = new List<FileEntry>(m_files); 
				//reverseFiles.Reverse();
				//return reverseFiles.ToArray();
				return m_files.ToArray();
			}
		}

		public string Basename 
		{
			get
			{
				if (Files.Length > 0)
					return Files[0].Basename;
				else
					return "Unknown";
			}
		}

		public string CS
		{
			get
			{
				if (m_files == null)
				{
					ParseCondition();
				}
				return m_cs;
			}
		}

		public StackTraceEntry[] StackTrace
		{
			get
			{
				if(m_stackTrace == null)
				{
					ParseCondition();
				}
				
				return m_stackTrace.ToArray();
			}
		}

		private void ParseCondition()
		{
			m_stackTrace = new List<StackTraceEntry>();
			m_files = new List<FileEntry>();

			if (IsWarning)
			{
				// try to get files out of condition
				ParseWarning();

				// try to read code style warning out of condition
				ParseCS();
			}

			// parse stack trace
			ParseStackTrace();

			// try to get file with InstanceID
			if (InstanceID != 0 && m_files.Count == 0)
			{
				m_files.Add(new FileEntry(
					AssetDatabase.GetAssetPath(InstanceID),
					-1));
			}
		}

		private void ParseCS()
		{
			var regex = new Regex(":\\s*warning\\s*(CS[^\\s:]+)\\s*:");
			var m = regex.Match(Condition);
			if (m.Success)
			{
				m_cs = m.Groups[1].Value;
			}
		}

		private void ParseWarning()
		{ 
			var regex = new Regex("^(Assets/[^\\(]+)\\(([0-9]+),([0-9]+)\\):");
			var m = regex.Match(Condition);

			if (m.Success)
			{
				m_files.Add(new FileEntry(m.Groups[1].Value, int.Parse(m.Groups[2].Value)));
			}
		}

		private void ParseStackTrace()
		{
			var lines = Condition.Split('\n');
			// ^([^\s\(]*)\.([^\s\(]*)\s*\(([^\)]*)\)(?:\s*\(at\s*([^\)]+):([0-9]+)\))?$
			// m1: Class
			// m2: Method
			// m3: Method Params
			// m4: FileName
			// m5: Line
			var regex = new Regex("^([^\\s\\(]*)[\\.:]([^\\s\\(]*)\\s*\\(([^\\)]*)\\)(?:\\s*\\(at\\s*([^\\)]+):([0-9]+)\\))?$");
			foreach (var line in lines)
			{
				var m = regex.Match(line);
				if (m.Success)
				{
					var lineNumber = 0;
					var fileName = string.Empty;

					// match with file and linenumber
					if (int.TryParse(m.Groups[5].Value, out lineNumber))
					{
						fileName = m.Groups[4].Value;
					}

					var file = new StackTraceEntry(fileName, lineNumber)
					{
						ClassName = m.Groups[1].Value,
						MethodName = m.Groups[2].Value,
						MethodParams = m.Groups[3].Value
					};

					// Don't insert log methods / classes
					if (file.ClassName == "PS.Editor.Console.LogHandler" ||
						file.ClassName == "PS.Editor.Console.DebugLog" ||
						file.ClassName.StartsWith("UnityEngine.Debug"))
					{
						continue;
					}

					m_stackTrace.Add(file);
					m_files.Add(file);
				}
			}
		}

		public Texture Icon
		{
			get { return m_icon ?? (m_icon = AssetDatabase.GetCachedIcon(File)); }
		}

		public bool IsWarning
		{
			get { return LogMode == LogModeEnum.Warning; }
		}

		public bool isError
		{
			get { return LogMode == LogModeEnum.Error; }
		}

		protected bool Equals(LogEntry other)
		{
			return string.Equals(Condition, other.Condition) && ErrorNum == other.ErrorNum && string.Equals(File, other.File) && Line == other.Line && Mode == other.Mode && InstanceID == other.InstanceID && Identifier == other.Identifier && IsWorldPlaying == other.IsWorldPlaying;
		}

		public void RemoveFileFromFiles(string fileName)
		{
			if(m_files == null)
				return;
			List<FileEntry> allFilesWithName = m_files.FindAll(x => x.Basename == fileName);
			foreach(FileEntry file in allFilesWithName)
			{
				m_files.Remove(file);
			}
		}

		public static int CalculateHashCode(string condition, int errorNum, string file, int line, int InstanceID, int identifier)
		{
			unchecked
			{
				var hashCode = (condition != null ? condition.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ errorNum;
				hashCode = (hashCode * 397) ^ (file != null ? file.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ line;
				hashCode = (hashCode * 397) ^ InstanceID;
				hashCode = (hashCode * 397) ^ identifier;
				return hashCode;
			}
		}

		public int CalculateHashCode()
		{
			return CalculateHashCode(Condition, ErrorNum, File, Line, InstanceID, Identifier);
		}

		public override int GetHashCode()
		{
			return CalculateHashCode();
		}
	}
}