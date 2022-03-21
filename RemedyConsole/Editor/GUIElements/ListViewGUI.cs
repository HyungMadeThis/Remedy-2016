using UnityEngine;
using System.Reflection;
using UnityEditor;
using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Remedy.GUIElements
{
	public class ListViewGUI
	{
		public static Type m_ListViewGUI_TYPE { get; private set; }
		private static Type m_ListViewElementsEnum_TYPE;
		private static Type m_internalListViewState;
		private readonly ListViewState m_state;
		private static MethodInfo m_listViewMethod;
		private static readonly PropertyInfo m_visibleRectProperty;

		private static readonly FieldInfo m_beganHorizontalField;
		private static readonly FieldInfo m_rectField;
		private static readonly FieldInfo m_stateField;
		private static readonly FieldInfo m_rectHeightField;
		private static readonly FieldInfo m_endRowField;
		private static readonly FieldInfo m_invisibleRowsField;

		private object m_ilvState;
		private int m_listViewHash;
		private readonly int[] m_dummyWidths = new int[1];

		static ListViewGUI()
		{
			var assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
			var listViewShared_TYPE = assembly.GetType("UnityEditor.ListViewShared");

			m_ListViewGUI_TYPE = assembly.GetType("UnityEditor.ListViewGUI");

			m_ListViewElementsEnum_TYPE = listViewShared_TYPE.GetNestedType("ListViewElementsEnumerator", BindingFlags.NonPublic);
			m_internalListViewState = listViewShared_TYPE.GetNestedType("InternalListViewState", BindingFlags.NonPublic);

			var guiClipType = Assembly.GetAssembly(typeof(UnityEngine.Object)).GetType("UnityEngine.GUIClip");
			m_visibleRectProperty = guiClipType.GetProperty("visibleRect");

			// Fields
			m_beganHorizontalField = m_internalListViewState.GetField("beganHorizontal");
			m_rectField = m_internalListViewState.GetField("rect");
			m_stateField = m_internalListViewState.GetField("state");
			m_rectHeightField = m_internalListViewState.GetField("rectHeight");
			m_endRowField = m_internalListViewState.GetField("endRow");
			m_invisibleRowsField = m_internalListViewState.GetField("invisibleRows");
		}

		public ListViewGUI(ListViewState state)
		{
			m_state = state;
			m_ilvState = Activator.CreateInstance(m_internalListViewState);
		}
		public ListViewState State
		{
			get { return m_state; }
		}
		public void ListView(GUIStyle style, Action<ListViewElement> @do, int controlId = 0)
		{
			GUILayout.BeginHorizontal(style, new GUILayoutOption[0]);
			m_state.ScrollPos = EditorGUILayout.BeginScrollView(m_state.ScrollPos);
			m_beganHorizontalField.SetValue(m_ilvState, true);
			m_state.DraggedFrom = -1;
			m_state.DraggedTo = -1;
			m_state.FileNames = (string[])null;
			var iterator = DoListView(GUILayoutUtility.GetRect(1f, m_state.TotalRows * m_state.RowHeight + 3), m_state, null,
				string.Empty, controlId);

			while (iterator != null && iterator.MoveNext())
			{
				var element = iterator.Current;
				@do.Invoke(CopyStruct<ListViewElement>(ref element));
			}
		}
		private IEnumerator DoListView(Rect pos, ListViewState state, int[] colWidths, string dragTitle, int controlId = 0)
		{
			if (m_listViewHash != 0)
			{
				m_listViewHash = GetHashCode();
			}
			if (controlId == 0)
			{
				controlId = GUIUtility.GetControlID(m_listViewHash, FocusType.Passive);
			}
			state.ID = controlId;
			state.SelectionChanged = false;
			var visibleRect = (Rect)m_visibleRectProperty.GetValue(null, null);
			var rect = visibleRect.x < 0.0 || visibleRect.y < 0.0
				? pos
				: pos.y >= 0.0
					? new Rect(0.0f, state.ScrollPos.y, visibleRect.width, visibleRect.height)
					: new Rect(0.0f, 0.0f, visibleRect.width, visibleRect.height);
			if (rect.width <= 0.0)
				rect.width = 1f;
			if (rect.height <= 0.0)
				rect.height = 1f;
			m_rectField.SetValue(m_ilvState, rect);
			var yFrom = (int)((-(double)pos.y + rect.yMin) / state.RowHeight);
			var yTo = yFrom + (int)Math.Ceiling((((double)rect.yMin - pos.y) % state.RowHeight + rect.height) / state.RowHeight) - 1;
			if (colWidths == null)
			{
				m_dummyWidths[0] = (int)rect.width;
				colWidths = m_dummyWidths;
			}
			m_invisibleRowsField.SetValue(m_ilvState, yFrom);
			m_endRowField.SetValue(m_ilvState, yTo);
			m_rectHeightField.SetValue(m_ilvState, (int)rect.height);
			m_stateField.SetValue(m_ilvState, state.InternalObject);
			if (yFrom < 0)
				yFrom = 0;
			if (yTo >= state.TotalRows)
				yTo = state.TotalRows - 1;

			var args = new[] {
				m_ilvState,
				colWidths,
				yFrom,
				yTo,
				dragTitle,
				new Rect(0.0f, yFrom*state.RowHeight, pos.width, state.RowHeight)
			};
			return Activator.CreateInstance(m_ListViewElementsEnum_TYPE, (BindingFlags)(-1), (Binder)null, (object[])args, CultureInfo.InvariantCulture) as IEnumerator;
		}

		private static T CopyStruct<T>(ref object s1)
		{
			var handle = GCHandle.Alloc(s1, GCHandleType.Pinned);
			var typedStruct = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();
			return typedStruct;
		}
	}
	
}
