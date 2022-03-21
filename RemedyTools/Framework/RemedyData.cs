using System.Collections.Generic;
using UnityEngine;

namespace RemedyDebug
{
    public class RemedyData : ScriptableObject
    {
        public static string PATH = "RemedyData";
        private static RemedyData m_instance;
        public static RemedyData Instance
        {
            get
            {
                if (!m_instance)
                    m_instance = GetInstance();
                return m_instance;
            }
        }

        [SerializeField]
        private List<ClassFilter> ClassesList;

        [SerializeField]
        private bool m_showDisplayInBuild = false;  //For editor use only
        public bool ShowDisplayInBuild { get { return m_showDisplayInBuild; } }

        public static RemedyData GetInstance()
        {
            RemedyData found = Resources.Load<RemedyData>(PATH);
            if (found)
                m_instance = found;

            return found;
        }
        private void Awake()
        {
            m_instance = this;
        }

        ///<summary>
        ///</summary>
        public ClassFilter GetClassFilter(string className)
        {
            ClassFilter filter = ClassesList.Find(x => x.ClassName == className);
            if(filter == null)
            {
                ClassFilter newClass = new ClassFilter(className);
                ClassesList.Add(newClass);
                ClassesList.Sort(delegate (ClassFilter cf1, ClassFilter cf2) { return cf1.ClassName.CompareTo(cf2.ClassName); });
            }
            return filter;
        }

        ///<summary>
        ///</summary>
        public void UpdateClassFilters(List<string> foundReferences)
        {
            List<ClassFilter> updatedList = new List<ClassFilter>();

            foreach(string foundRef in foundReferences)
            {
                //If this ref is already in the updatedList, skip it.
                ClassFilter foundFilterInUpdateList = updatedList.Find(x => x.ClassName == foundRef);
                if(foundFilterInUpdateList != null)
                    continue;
                
                //If the ref exists in our old ClassesList, add that one.
                ClassFilter foundFilter = ClassesList.Find(x => x.ClassName == foundRef);
                //If not, make a new one.
                if(foundFilter == null)
                {
                    foundFilter = new ClassFilter(foundRef);
                }
                updatedList.Add(foundFilter);
            }

            updatedList.Sort(delegate (ClassFilter cf1, ClassFilter cf2) { return cf1.ClassName.CompareTo(cf2.ClassName); });
            ClassesList = updatedList;
        }

        // private Dictionary<KeyCode, List<OnKeyPressDelegate>> m_keyCodeChecks;
        // public Dictionary<KeyCode, List<OnKeyPressDelegate>> KeyCodeChecks { get { return m_keyCodeChecks; } }
        // ///<summary>
        // ///</summary>
        // public void OnKeyPress(KeyCode keyCode, OnKeyPressDelegate inFunc)
        // {
        //     List<OnKeyPressDelegate> listOfFunc;
        //     if (!m_keyCodeChecks.TryGetValue(keyCode, out listOfFunc))
        //     {
        //         listOfFunc = new List<OnKeyPressDelegate>();
        //         listOfFunc.Add(inFunc);
        //         m_keyCodeChecks.Add(keyCode, listOfFunc);
        //     }
        //     else
        //     {
        //         listOfFunc.Add(inFunc);
        //         m_keyCodeChecks[keyCode] = listOfFunc;
        //     }

        //     return;
        // }
    }
}