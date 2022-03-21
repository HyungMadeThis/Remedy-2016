using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace Remedy.GUIElements
{
	/// <summary>
	/// Reflection'd UnityEditor.ListViewGUI
	/// </summary>
	public class ListViewState
	{
		private readonly FieldInfo m_totalRowsField;
		private readonly FieldInfo m_rowField;
		private readonly FieldInfo m_scrollPosField;
		private readonly FieldInfo m_rowHeight;
		private readonly FieldInfo m_idField;
		private readonly FieldInfo m_columnField;
		private readonly FieldInfo m_selectionChangedField;
		private readonly FieldInfo m_draggedFromField;
		private readonly FieldInfo m_draggedToField;
		private readonly FieldInfo m_fileNamesField;

		public static Type ListViewState_TYPE;
		public object InternalObject;

		static ListViewState()
		{
			var assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
			ListViewState_TYPE = assembly.GetType("UnityEditor.ListViewState");
		}
		public ListViewState(int totalRows = 0, int rowHeight = 16)
		{
			InternalObject = Activator.CreateInstance(ListViewState_TYPE, totalRows, rowHeight);

			m_scrollPosField = ListViewState_TYPE.GetField("scrollPos");
			m_rowField = ListViewState_TYPE.GetField("row");
			m_totalRowsField = ListViewState_TYPE.GetField("totalRows");
			m_rowHeight = ListViewState_TYPE.GetField("rowHeight");
			m_idField = ListViewState_TYPE.GetField("ID");
			m_columnField = ListViewState_TYPE.GetField("column");
			m_selectionChangedField = ListViewState_TYPE.GetField("selectionChanged");
			m_draggedFromField = ListViewState_TYPE.GetField("draggedFrom");
			m_draggedToField = ListViewState_TYPE.GetField("draggedTo");
			m_fileNamesField = ListViewState_TYPE.GetField("fileNames");
		}

		public int TotalRows
		{
			get { return (int)m_totalRowsField.GetValue(InternalObject); }
			set { m_totalRowsField.SetValue(InternalObject, value); }
		}
		public Vector2 ScrollPos
		{
			get { return (Vector2)m_scrollPosField.GetValue(InternalObject); }
			set { m_scrollPosField.SetValue(InternalObject, value); }
		}

		public int Row
		{
			get { return (int)m_rowField.GetValue(InternalObject); }
			set { m_rowField.SetValue(InternalObject, value); }
		}

		public int RowHeight
		{
			get { return (int)m_rowHeight.GetValue(InternalObject); }
			set { m_rowHeight.SetValue(InternalObject, value); }
		}

		public int ID
		{
			get { return (int)m_idField.GetValue(InternalObject); }
			set { m_idField.SetValue(InternalObject, value); }
		}

		public int Column
		{
			get { return (int)m_columnField.GetValue(InternalObject); }
			set { m_columnField.SetValue(InternalObject, value); }
		}

		public bool SelectionChanged
		{
			get { return (bool)m_selectionChangedField.GetValue(InternalObject); }
			set { m_selectionChangedField.SetValue(InternalObject, value); }
		}

		public int DraggedFrom
		{
			get { return (int)m_draggedFromField.GetValue(InternalObject); }
			set { m_draggedFromField.SetValue(InternalObject, value); }
		}

		public int DraggedTo
		{
			get { return (int)m_draggedToField.GetValue(InternalObject); }
			set { m_draggedToField.SetValue(InternalObject, value); }
		}

		public string[] FileNames
		{
			get { return (string[])m_fileNamesField.GetValue(InternalObject); }
			set { m_fileNamesField.SetValue(InternalObject, value); }
		}


	}
}