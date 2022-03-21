using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Mono.Reflection;

namespace RemedyDebug
{
    public class RemedyEditor : EditorWindow
    {
        private static SerializedObject m_serializedObject;
        private static Vector2 m_windowSize = new Vector2(300, 500);
        private SplitterState m_splitterState;
        private static Vector2 m_classFiltersScrollPos;
        private static Vector2 m_keycodesScrollPos;

        SerializedProperty m_listProperty;
        SerializedProperty m_showDisplayInBuild;

        private RemedyEditor()
        {
            m_splitterState = new SplitterState(new[] { 60f, 40f }, new[] { 0, 0 }, null);
        }

        [MenuItem("HSTools/Remedy")]
        public static void Init()
        {
            RemedyEditor Instance = GetWindow<RemedyEditor>("RemedyEditor");
            Instance.Show(true);
            Instance.Focus();
            Instance.minSize = m_windowSize;
        }

        private void OnEnable()
        {
            if (!RemedyData.Instance)
            {
                CreateNewRemedyData();
            }
            m_serializedObject = new SerializedObject(RemedyData.Instance);
        }

        private void OnGUI()
        {
            m_serializedObject.Update();
            m_listProperty = m_serializedObject.FindProperty("ClassesList");
            m_showDisplayInBuild = m_serializedObject.FindProperty("m_showDisplayInBuild");

            Toolbar();
            Title();

            SplitterGUILayout.BeginVerticalSplit(m_splitterState);
            TopPanel();
            BottomPanel();
            SplitterGUILayout.EndVerticalSplit();

            m_serializedObject.ApplyModifiedProperties();
        }

        private void Toolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            m_showDisplayInBuild.boolValue = GUILayout.Toggle(m_showDisplayInBuild.boolValue, "Release Build Bools", EditorStyles.toolbarButton);
            if(GUILayout.Button("Reset All", EditorStyles.toolbarButton))
            {
                m_listProperty.arraySize = 0;
            }
            GUILayout.EndHorizontal();
        }

        private void Title()
        {
            GUIStyle TitleStyle = new GUIStyle();
            TitleStyle.richText = true;

            GUILayout.BeginHorizontal();
            Rect titleRect = EditorGUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("<size=30><b><color=purple> Remedy </color></b></size>", TitleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 0f, 1f);
            GUI.Box(titleRect, GUIContent.none);
            GUI.color = oldColor;
            GUILayout.EndHorizontal();
        }

        private void SectionTitle(string title)
        {
            GUIStyle TitleStyle = new GUIStyle();
            TitleStyle.richText = true;

            GUILayout.BeginHorizontal();
            Rect titleRect = EditorGUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("<size=12><b><color=black>" + title + "</color></b></size>", TitleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f);
            GUI.Box(titleRect, GUIContent.none);
            GUI.color = oldColor;
            GUILayout.Space(4.0f);
            GUILayout.EndHorizontal();
        }

        private void TopPanel()
        {
            GUILayout.BeginVertical();
            Rect topRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);//need this to make it even for some reason.
            ClassFilters();
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            Color oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 1f);
            GUI.Box(topRect, GUIContent.none);
            GUI.color = oldColor;
            GUILayout.EndVertical();
        }

        private void BottomPanel()
        {
            GUILayout.BeginVertical();
            Rect bottomRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.BeginHorizontal();

            KeyCodeBindingsPanel();
            
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 0f, 0f);
            GUI.Box(bottomRect, GUIContent.none);
            GUI.color = oldColor;
            GUILayout.EndVertical();
        }

        private void ClassFilters()
        {
            GUILayout.BeginVertical();
            SectionTitle("Class Filters");
            m_classFiltersScrollPos = GUILayout.BeginScrollView(m_classFiltersScrollPos);
            GUILayout.BeginHorizontal();

            //===================================//
            DisplayCheckBox("Display" , "m_display");

            if(m_showDisplayInBuild.boolValue)
                DisplayCheckBox("In Build" , "m_displayInBuild");

            ClassNameBox();
            ColorBox();
             //===================================//

            GUILayout.EndHorizontal();
            FilterButtons();

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void ClassNameBox()
        {
            Rect classNameRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Class Name");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            for (int i = 0; i < m_listProperty.arraySize; i++)
            {
                SerializedProperty classFilter = m_listProperty.GetArrayElementAtIndex(i);
                GUILayout.Label(classFilter.FindPropertyRelative("m_className").stringValue, GUILayout.Width(110));

                SerializedProperty subFilters = classFilter.FindPropertyRelative("m_subClassFilters");
                if (subFilters.arraySize != 0)
                {
                    SerializedProperty classDisplayBoolSP = classFilter.FindPropertyRelative("m_display");
                    for (int j = 0; j < subFilters.arraySize; j++)
                    {
                        SerializedProperty subFilter = subFilters.GetArrayElementAtIndex(j);
                        string subFilterName = ((RemedyFilterType)subFilter.FindPropertyRelative("m_filterType").enumValueIndex).ToString();
                        subFilterName = " - " + subFilterName;
                        if(!classDisplayBoolSP.boolValue)
                        {
                            GUIStyle grayedStyle = new GUIStyle(GUI.skin.label);
                            grayedStyle.normal.textColor = Color.gray;
                            GUILayout.Label(subFilterName, grayedStyle, GUILayout.ExpandWidth(true));
                        }
                        else
                        {
                            GUILayout.Label(subFilterName, GUILayout.ExpandWidth(true));
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();
            GUI.Box(classNameRect, GUIContent.none);
        }

        private void DisplayCheckBox(string title, string DisplayBoolName)
        {
            Rect displayRect = EditorGUILayout.BeginVertical(GUILayout.Width(55f), GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(title);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();

            for (int i = 0; i < m_listProperty.arraySize; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                SerializedProperty classFilter = m_listProperty.GetArrayElementAtIndex(i);
                SerializedProperty classDisplayBoolSP = classFilter.FindPropertyRelative(DisplayBoolName);
                classDisplayBoolSP.boolValue = GUILayout.Toggle(classDisplayBoolSP.boolValue, " ");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                SerializedProperty subFilters = classFilter.FindPropertyRelative("m_subClassFilters");
                if (subFilters.arraySize != 0)
                {
                    for (int j = 0; j < subFilters.arraySize; j++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(" ");
                        SerializedProperty subFilter = subFilters.GetArrayElementAtIndex(j);
                        EditorGUI.BeginDisabledGroup (classDisplayBoolSP.boolValue == false);
                        subFilter.FindPropertyRelative(DisplayBoolName).boolValue = GUILayout.Toggle(subFilter.FindPropertyRelative(DisplayBoolName).boolValue, " ");
                        EditorGUI.EndDisabledGroup ();
                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUI.Box(displayRect, GUIContent.none);
        }

        private void ColorBox()
        {
            Rect colorRect = EditorGUILayout.BeginVertical(GUILayout.Width(75f), GUILayout.ExpandHeight(true));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label("Color");

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();

            for (int i = 0; i < m_listProperty.arraySize; i++)
            {
                SerializedProperty classFilter = m_listProperty.GetArrayElementAtIndex(i);
                GUILayout.BeginHorizontal();
                GUILayout.Space(10f);
                classFilter.FindPropertyRelative("m_textColor").colorValue = EditorGUILayout.ColorField(classFilter.FindPropertyRelative("m_textColor").colorValue);
                GUILayout.Space(10f);
                GUILayout.EndHorizontal();

                SerializedProperty subFilters = classFilter.FindPropertyRelative("m_subClassFilters");
                if (subFilters.arraySize != 0)
                {
                    SerializedProperty classDisplayBoolSP = classFilter.FindPropertyRelative("m_display");
                    for (int j = 0; j < subFilters.arraySize; j++)
                    {
                        SerializedProperty subFilter = subFilters.GetArrayElementAtIndex(j);

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10f);
                        EditorGUI.BeginDisabledGroup (classDisplayBoolSP.boolValue == false);
                        subFilter.FindPropertyRelative("m_textColor").colorValue = EditorGUILayout.ColorField(subFilter.FindPropertyRelative("m_textColor").colorValue);
                        EditorGUI.EndDisabledGroup ();
                        GUILayout.Space(10f);
                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUI.Box(colorRect, GUIContent.none);
        }

        private void FilterButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("All On", GUILayout.Width(100f)))
            {
                ToggleAllBooleans(true);
            }
            if (GUILayout.Button("All Off", GUILayout.Width(100f)))
            {
                ToggleAllBooleans(false);
            }
            GUILayout.FlexibleSpace();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(100f)))
            {
                RefreshClassFilters();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);
        }

        private void RefreshClassFilters()
        {
            FindAllReferences();
        }

        private void ToggleAllBooleans(bool toggle)
        {
            for (int i = 0; i < m_listProperty.arraySize; i++)
            {
                m_listProperty.GetArrayElementAtIndex(i).FindPropertyRelative("m_display").boolValue = toggle;
            }
        }

        private void KeyCodeBindingsPanel()
        {
            GUILayout.BeginVertical();
            SectionTitle("KeyCode Bindings");
            m_keycodesScrollPos = GUILayout.BeginScrollView(m_keycodesScrollPos);
            Rect bindingsRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Space(4.0f);

            if(!Application.isPlaying)
            {
                GUILayout.Label("Only available in Play mode!");
            }
            else
            {
                Dictionary<KeyCode, List<KeyCodeBinding>> allBindings = Remedy.AllKeyCodeBindings;
                if(allBindings == null)
                {
                    GUILayout.Label("There's nothing.");
                }
                else
                {
                    List<KeyCode> keys = new List<KeyCode>(allBindings.Keys);
                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.richText = true;
                    style.normal.textColor = new Color(0,1,1);
                    for(int i = 0; i < allBindings.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        string keyTitle = "   KeyCode: [" + keys[i].ToString() + "]";
                        keyTitle = Remedy.FormatStringWithColor(keyTitle, new Color(1,0.5f,0), true);
                        GUILayout.Label(keyTitle, style);
                        GUILayout.EndHorizontal();

                        foreach(KeyCodeBinding binding in allBindings[keys[i]])
                        {
                            string line = string.Format("      <b>[{0}.{1}()]</b>: {2}",binding.ClassName, binding.FunctionName, binding.Description);
                            GUILayout.Label(line, style);
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 0f, 0f);
            GUI.Box(bindingsRect, GUIContent.none);
            GUI.color = oldColor;

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

        }

        private void CreateNewRemedyData()
        {
            string[] directories = Directory.GetDirectories("Assets", "Resources", SearchOption.TopDirectoryOnly);

            if (directories.Length == 0)
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            RemedyData newInstance = CreateInstance<RemedyData>();
            AssetDatabase.CreateAsset(newInstance, string.Format("{0}/{1}", "Assets/Resources", RemedyData.PATH + ".asset"));

            AssetDatabase.Refresh();
        }

        private static List<MonoScript> GetAllScriptAssets()
        {
            List<MonoScript> monoScripts = new List<MonoScript>();

            string projectPath = Application.dataPath.Replace("Assets", "");
            DirectoryInfo dirInfo = new DirectoryInfo("Assets");
            List<FileInfo> fileInfos = new List<FileInfo>(dirInfo.GetFiles("*.cs", SearchOption.AllDirectories));
            foreach (FileInfo fileInfo in fileInfos)
            {
                UnityEngine.Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fileInfo.FullName.Remove(0, projectPath.ToCharArray().Length));
                MonoScript script = o as MonoScript;
                //Debug.Log(script.GetClass().ToString());
                if (script.GetClass() != null && script.GetType() != typeof(Shader))
                {
                    //Debug.Log(script.GetClass().ToString());
                    monoScripts.Add(script);
                }
            }
            return monoScripts;
        }

        ///<summary>
        /// Loops through the list of all our scripts and finds all references to Remedy.Log.
        /// Sends all references to RemedyData.UpdateClassFilters().
        /// Cannot search through classes that cannot be instanced and turned into MonoScripts.
        /// Currently cannot find SubFilters for ClassFilters.
        ///</summary>
        private void FindAllReferences()
        {
            List<string> foundReferencesOfLog = new List<string>();

            List<MonoScript> allScripts = GetAllScriptAssets();
            for(int i = 0; i < allScripts.Count; i ++)
            {
                MonoScript script = allScripts[i];
                MethodBase[] methodInfos = script.GetClass().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                //Debug.Log("====" + script.GetClass().ToString());
                foreach (MethodBase method in methodInfos)
                {
                    if (method.GetMethodBody() == null)
                        continue;

                    //Debug.Log("==" + method.Name);
                    IList<Mono.Reflection.Instruction> instructions = MethodBaseRocks.GetInstructions(method);
                    foreach (Mono.Reflection.Instruction instruction in instructions)
                    {
                        MethodInfo calledMethod = instruction.Operand as MethodInfo;
                        if (calledMethod == null)
                            continue;

                        //Debug.Log(calledMethod.DeclaringType.FullName + "     " + calledMethod.Name); 
                        if (calledMethod.DeclaringType.FullName == "Remedy" && calledMethod.Name == "Log")
                        {
                            //Debug.Log(calledMethod.DeclaringType.FullName + "     " + calledMethod.Name);
                            foundReferencesOfLog.Add(script.GetClass().ToString());
                            //Debug.Log(new MethodBodyReader(method).GetVariable(instruction,0).ToString());
                        }
                    }

                }
                EditorUtility.DisplayProgressBar(string.Format("Parsing all scripts. {0}/{1}", i, allScripts.Count), script.GetClass().ToString(), (float)i / (float)allScripts.Count);
            }

            RemedyData.Instance.UpdateClassFilters(foundReferencesOfLog);

            EditorUtility.ClearProgressBar();
        }
    }
}
