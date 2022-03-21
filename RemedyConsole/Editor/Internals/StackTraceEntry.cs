namespace Remedy.Internals
{
	public class StackTraceEntry : FileEntry
	{
		public string MethodName;
		public string ClassName;
		public string MethodParams;

		public StackTraceEntry(string name, int line) : base(name, line)
		{
		}
	}

}