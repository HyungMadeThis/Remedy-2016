using System;
using UnityEditor;
using UnityEngine;

namespace Remedy.Internals
{
	public class FileEntry
	{
		public string Name;
		public int Line;
		private Texture m_icon;

		public FileEntry(string name, int line)
		{
			Name = name;
			Line = line;
		}

		public Texture Icon
		{
			get
			{
				if (!IsInAssetDirectory)
				{
					return null;
				}

				return m_icon ?? (m_icon = AssetDatabase.GetCachedIcon(Name)); 
			}
		}

		public string Basename 
		{
			get
			{
				var index = Name.LastIndexOf("/", StringComparison.Ordinal);
				return index == -1 ? Name : Name.Substring(index + 1);
			}
		}

		public bool IsInAssetDirectory
		{
			get { return Name.StartsWith("Assets/"); }
		}
	}
}