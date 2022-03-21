using UnityEngine;
using Remedy.Internals;

namespace Remedy
{
	public static class RemedyTerminal
	{
		private static string HELP_TAG = "[T0]";
		private static string UNRECOGNIZED_TAG = "[T-1]";

		public static void NewCommand(string cmd)
		{
			if(cmd == "/help")
				Debug.Log(HELP_TAG + cmd);
			else
				Debug.Log(UNRECOGNIZED_TAG + cmd);
		}


		public static void CheckIfTerminalEntry(LogEntry entry)
		{
			string copy = entry.Condition;
			if(copy.Substring(0,2) == "[T" && entry.Condition.IndexOf("]") != -1)
			{
				ParseTerminalEntry(entry);
			}
		}
		private static void ParseTerminalEntry(LogEntry entry)
		{
			int indexOfClose = entry.Condition.IndexOf("]")+1;
			string terminalTag = entry.Condition.Substring(0,indexOfClose);
			entry.Condition = entry.Condition.Substring(indexOfClose);
			entry.Condition = entry.GetFirstLine() + "\n";
			entry.Mode = LogEntry.LogEntryMode.TerminalEntry;

			if(terminalTag == HELP_TAG) 
			{
				entry.Condition +=
				"\\/ Please see details \\/" + "\n\n" +
				"I don't have much to say here yet." + "\n" +
				"But hopefully I can make this more useful somehow." + "\n" +
				"I hope that all made sense";
			}
			else if(terminalTag == UNRECOGNIZED_TAG)
			{
				entry.Condition +=
				"Please type \"/help\" for commands.";
			}
		}
	}
}