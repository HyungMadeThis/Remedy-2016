using System.Text.RegularExpressions;

namespace Remedy.Internals.Log
{

	public enum FilterMode
	{
		File,
		RegEx
	}

	public class StringFilter : IDebugLogFilter
	{
		private FilterMode _filterMode;
		private string _filter;
		private Regex _regex;

		public StringFilter()
		{
			Enabled = true;
		}

		public bool IsEntryVisible(LogEntry entry)
		{
			if (!Enabled)
			{
				return true;
			}

			if (_filterMode == FilterMode.RegEx && _regex != null)
			{
				return _regex.IsMatch(entry.Condition) || _regex.IsMatch(entry.File);
			}

			if (_filterMode == FilterMode.File)
			{
				return string.IsNullOrEmpty(_filter) || entry.File.Contains(_filter);
			}

			return false;
		}

		public bool Enabled { get; set; }

		public string Filter
		{
			get { return _filter; }
			set
			{
				if (_filter != value)
				{
					_filter = value;
					UpdateFilter();
				}
			}
		}

		public FilterMode FilterMode
		{
			get { return _filterMode; }
			set
			{
				if (_filterMode != value)
				{
					_filterMode = value;
					UpdateFilter();
				}
			}
		}

		private void UpdateFilter()
		{
			if (_filterMode == FilterMode.RegEx)
			{
				try
				{
					_regex = new Regex(_filter);
				}
				catch
				{
					_regex = null;
				}
			}
			else
			{
				_regex = null;
			}
		}
	}
}