using Remedy.GUIElements;
using UnityEngine;

namespace Remedy.Internals.Detail
{
	public abstract class DetailView
	{
		protected readonly GUIContent TextGUIContent;
		protected readonly ListViewGUI ListView;

		protected DetailView(RemedyConsole parent)
		{ 
			Parent = parent;
			TextGUIContent = new GUIContent();
			ListView = new ListViewGUI(new ListViewState(0, 20));
		}

		public RemedyConsole Parent { get; private set; }

		public abstract void OnGUI(LogEntry entry);
		
	}
}