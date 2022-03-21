using System;
using UnityEngine;

public enum RemedyFilterType
{
    Structure,
    Temporary
}
namespace RemedyDebug
{

    [Serializable]
    public class SubClassFilter
    {
        public SubClassFilter(RemedyFilterType type)
        {
            m_filterType = type;

            //Preset SubFilterType.Structure to have a green color. 
            if (m_filterType == RemedyFilterType.Structure)
            {
                m_textColor = new Color(0.0784f, 0.8235f, 0.0784f, 1);
            }
        }
        [SerializeField]
        private RemedyFilterType m_filterType;

        [SerializeField]
        private bool m_display = true;

        [SerializeField]
        private bool m_displayInBuild = true;

        [SerializeField]
        private Color m_textColor = Color.gray;

        public RemedyFilterType FilterType { get { return m_filterType; } }
        public bool CanDisplay { get { return m_display; } }
        public bool CanDisplayInBuild { get { return m_displayInBuild; } }
        public Color TextColor { get { return m_textColor; } }
    }
}