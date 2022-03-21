using System.Collections.Generic;

namespace Remedy.Internals.Log
{
	public class CodeStyleFilter : IDebugLogFilter
	{
		private readonly HashSet<string> _codeStyles;

		public CodeStyleFilter(List<string> codeStyles = null)
		{
			_codeStyles = codeStyles == null ? 
				new HashSet<string>() : 
				new HashSet<string>(codeStyles);
			Enabled = true;
		}

		public bool IsEntryVisible(LogEntry entry)
		{
			if (!Enabled)
			{
				return true;
			}

			if (string.IsNullOrEmpty(entry.CS))
			{
				return true;
			}
			
			return !Contains(entry.CS);
		}

		public bool Enabled { get; set; }

		public void AddCodeStyle(string codeStyle)
		{
			_codeStyles.Add(codeStyle);
		}

		public bool Contains(string codeStyle)
		{
			return _codeStyles.Contains(codeStyle);
		}

		public bool RemoveCodeStyle(string codeStyle)
		{
			return _codeStyles.Remove(codeStyle);
		}
	}
}