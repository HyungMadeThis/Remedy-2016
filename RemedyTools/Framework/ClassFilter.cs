using System;
using System.Collections.Generic;
using UnityEngine;

namespace RemedyDebug
{
    [Serializable]
    public class ClassFilter
    {
        public ClassFilter(string name)
        {
            m_className = name;
            m_subClassFilters = new List<SubClassFilter>();

            //Create a subfilter for each value in the enum SubFilterType.
            int numSubFilters = Enum.GetValues(typeof(RemedyFilterType)).Length;
            for(int i = 0; i < numSubFilters; i ++)
            {
                RemedyFilterType type = (RemedyFilterType)i;
                m_subClassFilters.Add(new SubClassFilter(type));
            }
        }

        [SerializeField]
        private string m_className;

        [SerializeField]
        private bool m_display = true;  //If true, will display in editor and in development builds.

        [SerializeField]
        private bool m_displayInBuild = true;   //If true, will display in release builds.

        [SerializeField]
        private Color m_textColor = Color.black;

        [SerializeField]
        private List<SubClassFilter> m_subClassFilters;

        public string ClassName { get { return m_className; } }
        public bool CanDisplay { get { return m_display; } }
        public bool CanDisplayInBuild { get { return m_displayInBuild; } }
        public Color TextColor { get { return m_textColor; } }

        public SubClassFilter GetSubFilter(RemedyFilterType type)
        {
            SubClassFilter filter = m_subClassFilters.Find(x => x.FilterType == type);
            if(filter == null)
            {
                filter = new SubClassFilter(type);
                m_subClassFilters.Add(filter);
                return filter;
            }
            return filter;
        }
    }
}
