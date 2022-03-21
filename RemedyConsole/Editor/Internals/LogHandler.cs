using System;
using UnityEngine;

namespace Remedy.Internals
{
	///<summary>
	/// Custom DebugLog handler
	///</summary>
	public class LogHandler : ILogHandler
	{
		private readonly ILogHandler m_logger;
		private readonly DebugLog m_log;

		//This shit aint useful if I can't see what ILogHandler is actually doing!
		public LogHandler(ILogHandler parent, DebugLog log)
		{
			m_logger = parent;
			m_log = log;
		}

		public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
		{
			m_logger.LogFormat(logType, context, format, args);
			m_log.Update(DebugLog.Mode.Add);
		}

		public void LogException(Exception exception, UnityEngine.Object context)
		{
			m_logger.LogException(exception, context);
			m_log.Update(DebugLog.Mode.Add);
		}
	}
}