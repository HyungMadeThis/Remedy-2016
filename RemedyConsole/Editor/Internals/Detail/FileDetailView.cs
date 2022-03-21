using UnityEngine;

namespace Remedy.Internals.Detail
{
	public class FileDetailView : DetailView
	{
		private LogEntry m_current;

		public override void OnGUI(LogEntry entry)
		{
			if (entry == null)
			{
				return;
			}

			var e = Event.current;
			var controlId = GUIUtility.GetControlID(ListView.GetHashCode(), FocusType.Passive);
			GUILayout.BeginVertical();
			
			entry.RemoveFileFromFiles("LogHandler.cs");	//Don't want LogHandler displayed as a called class.
			entry.RemoveFileFromFiles("RemedyTerminal.cs");
			entry.RemoveFileFromFiles("RemedyConsole.cs");
			entry.RemoveFileFromFiles("ListViewGUI.cs");

			ListView.State.TotalRows = entry.Files.Length;
			if (m_current != entry || ListView.State.Row >= ListView.State.TotalRows)
			{
				ListView.State.Row = -1;
			}

			ListView.ListView(RemedyConstants.Box, listViewElement =>
			{
				FileEntry file = entry.Files[listViewElement.Row];

				if (e.type == EventType.Repaint)
				{
					var style = file.IsInAssetDirectory ? RemedyConstants.ListStyle : RemedyConstants.ListStyleInactive;
					var bg = listViewElement.Row % 2 == 0 ? RemedyConstants.EvenBackground : RemedyConstants.OddBackground;
					var active = ListView.State.Row == listViewElement.Row;
					var contentRect = listViewElement.Position;

					TextGUIContent.text = file.Basename;
					TextGUIContent.image = file.Icon;

					// missing file name? => internal function like casts, etc..
					if (string.IsNullOrEmpty(TextGUIContent.text))
					{
						TextGUIContent.text = "Internal function";
						style = RemedyConstants.ListStyleInactive;
					}

					bg.Draw(contentRect, false, false, active, false);
					style.Draw(contentRect, TextGUIContent, false, active, false, false);
				}
				// open the file on double click
				else if (e.type == EventType.MouseDown && e.clickCount == 2)
				{
					if(file.IsInAssetDirectory)
						Helper.OpenFile(file.Name, file.Line);
				}
			}, controlId);
			
			GUILayout.EndVertical();
			m_current = entry;
		}

		public FileDetailView(RemedyConsole parent) : base(parent)
		{
		}
	}
}