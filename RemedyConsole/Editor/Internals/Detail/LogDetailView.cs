using UnityEditor;
using UnityEngine;

namespace Remedy.Internals.Detail
{
	public class LogDetailView : DetailView
	{
		private Vector2 m_logScrollPosition;

		public override void OnGUI(LogEntry entry)
		{
			if (entry == null)
			{
				return;
			}

			//string content = Parent.FormatContent(entry.LogMode == LogEntry.LogModeEnum.Log,entry.Condition, false);
			string content = Parent.FormatContent(entry,entry.Condition, false);

			m_logScrollPosition = GUILayout.BeginScrollView(m_logScrollPosition, RemedyConstants.Box);

			TextGUIContent.text = content;
			float minMessageHeight = RemedyConstants.MessageStyle.CalcHeight(TextGUIContent, Parent.position.width);
			EditorGUILayout.SelectableLabel(content, RemedyConstants.MessageStyle,
				GUILayout.MinHeight(minMessageHeight), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			
			
			if (Event.current.type == EventType.ExecuteCommand) Debug.LogWarning(EditorGUIUtility.systemCopyBuffer);
			GUILayout.EndScrollView();
		}

		public LogDetailView(RemedyConsole parent) : base(parent)
		{
			
		}
	}
}