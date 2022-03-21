using System.Collections.Generic;

namespace Remedy.Internals.Log
{
	public class FileFilter : IDebugLogFilter
	{
		private readonly HashSet<string> _files;

		public FileFilter(List<string> files = null)
		{
			_files = files == null ? 
				new HashSet<string>() : 
				new HashSet<string>(files);
			Enabled = true;
		}

		public bool IsEntryVisible(LogEntry entry)
		{
			if (!Enabled)
			{
				return true;
			}

			return !Contains(entry.File);
		}

		public bool Enabled { get; set; }

		public void AddFile(string file)
		{
			_files.Add(file);
		}

		public bool Contains(string file)
		{
			return _files.Contains(file);
		}

		public bool RemoveFile(string file)
		{
			return _files.Remove(file);
		}
	}
}