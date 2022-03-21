namespace Remedy.Internals.Log
{
	public interface IDebugLogFilter
	{
		bool IsEntryVisible(LogEntry entry);

		bool Enabled { get; set; }
	}
}