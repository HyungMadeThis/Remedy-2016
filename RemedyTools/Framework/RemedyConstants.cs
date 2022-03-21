using UnityEngine;

namespace RemedyDebug
{
	public static class RemedyConstants
	{
		static RemedyConstants()
		{
			
		}


		public static string ColorTagStart(string hexColor)
		{
			return "<color=#" + hexColor + "FF>";
		}
		public static string ColorTagEnd = "</color>";
		public static string BoldTagStart = "<b>";
		public static string BoldTagEnd = "</b>";
	}
}
