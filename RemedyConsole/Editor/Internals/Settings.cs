using System;
using UnityEngine;

namespace Remedy.Internals
{
	///<summary>
	/// Settings ScriptableObject
	///</summary>
	[Serializable]
	public class Settings : ScriptableObject
	{
		[SerializeField]
		public bool ReverseOrder;

		[SerializeField]
		public bool SmallList;

		[SerializeField]
		public bool SelectLastChanged = true;

		[SerializeField]
		public bool ShowFilesInDetails;

		[SerializeField]
		public bool ShowLogCallFile;
	}

}
