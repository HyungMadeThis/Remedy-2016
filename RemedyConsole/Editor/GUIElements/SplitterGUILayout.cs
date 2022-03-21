using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Remedy.GUIElements
{
	class SplitterGUILayout
	{
		private static MethodInfo m_beginVerticalSplitMethod;
		private static MethodInfo m_beginHorizontalSplitMethod;
		private static MethodInfo m_endVerticalSplitMethod;
		private static MethodInfo m_endHorizontalSplitMethod;

		public static Type SplitterGUILayoutType { get; private set; }


		static SplitterGUILayout()
		{
			var assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
			SplitterGUILayoutType = assembly.GetType("UnityEditor.SplitterGUILayout");
		}

		public static void BeginVerticalSplit(SplitterState state, params GUILayoutOption[] options)
		{
			if (m_beginVerticalSplitMethod == null)
			{
				m_beginVerticalSplitMethod = SplitterGUILayoutType.GetMethod("BeginVerticalSplit",
					new[] { SplitterState.SplitterStateType, typeof(GUILayoutOption[]) });
			}
			m_beginVerticalSplitMethod.Invoke(null, new[] { state.InternalObject, options });
		}

		public static void BeginHorizontalSplit(SplitterState state, params GUILayoutOption[] options)
		{
			if (m_beginHorizontalSplitMethod == null)
			{
				m_beginHorizontalSplitMethod = SplitterGUILayoutType.GetMethod("BeginHorizontalSplit",
					new[] { SplitterState.SplitterStateType, typeof(GUILayoutOption[]) });
			}
			m_beginHorizontalSplitMethod.Invoke(null, new[] { state.InternalObject, options });
		}

		public static void EndVerticalSplit()
		{
			if (m_endVerticalSplitMethod == null)
			{
				m_endVerticalSplitMethod = SplitterGUILayoutType.GetMethod("EndVerticalSplit");
			}
			m_endVerticalSplitMethod.Invoke(null, null);
		}

		public static void EndHorizontalSplit()
		{
			if (m_endHorizontalSplitMethod == null)
			{
				m_endHorizontalSplitMethod = SplitterGUILayoutType.GetMethod("EndHorizontalSplit");
			}
			m_endHorizontalSplitMethod.Invoke(null, null);
		}
	}
}
