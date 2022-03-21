using System;
using System.Reflection;
using UnityEditor;

namespace RemedyDebug
{
	public class SplitterState
	{
		public object InternalObject { get; private set; }
		public static Type SplitterStateType { get; private set; }

		static SplitterState()
		{
			var assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
			SplitterStateType = assembly.GetType("UnityEditor.SplitterState");
		}

		public SplitterState(float[] relativeSizes, int[] minSizes, int[] maxSizes)
		{
			InternalObject = Activator.CreateInstance(SplitterStateType, relativeSizes, minSizes, maxSizes);
		}
	}
}

