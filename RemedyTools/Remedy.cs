using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using RemedyDebug;

public class Remedy : Singleton<Remedy>
{
    private void Enable()
    {  
        SceneManager.sceneLoaded += DeleteKeyCodesOnNewScene;
    }
    private void Disable()
    {
        SceneManager.sceneLoaded -= DeleteKeyCodesOnNewScene;
    }
    void Update()
    {
        KeyPressUpdate();
    }

    #region RemedyLog

    ///<summary>
    ///	Filtered Debug.Log which will only log if the specified class is not filtered out from RemedyConsole.
    ///</summary>
    public static void Log(string str, RemedyFilterType subFilterType = RemedyFilterType.Temporary)
    {
        StackFrame frame = new StackFrame(1, true);
        string callingClass = frame.GetMethod().DeclaringType.ToString();

        ClassFilter classFilter = RemedyData.Instance.GetClassFilter(callingClass);

        //Can we display it?
        bool canDisplay = false;
        if(Application.isEditor)
        {
            if(classFilter.CanDisplay)
            {
                canDisplay = true;
            }
        }
        else
        {
            if(UnityEngine.Debug.isDebugBuild)
            {
                if(classFilter.CanDisplay)
                {
                    canDisplay = true;
                }
            }
            else
            {
                if(classFilter.CanDisplayInBuild)
                {
                    canDisplay = true;
                }
            }
        }

        if(!canDisplay)
            return;

        if (RemedyData.Instance)
        {
            //Get class name prettied up.
            callingClass = "[" + callingClass + "]";
            string formattedClassString = FormatStringWithColor(callingClass, classFilter.TextColor, true);

            //Get subfilter data.
            SubClassFilter subFilter = classFilter.GetSubFilter(subFilterType);
            if (!subFilter.CanDisplay)
                return;

            str = FormatStringWithColor(str, subFilter.TextColor);

            //Print it!
            UnityEngine.Debug.Log(string.Format("{0} {1}", formattedClassString, str));
        }
    }
    
    ///<summary>
    /// Formats the calling class to use rich text for color and bolding.
    ///</summary>
    public static string FormatStringWithColor(string str, Color textColor, bool isBold = false)
    {
        string hexColor = ColorToHex(textColor);

        string colorTagBeginning = RemedyConstants.ColorTagStart(hexColor);
        string colorTagEnd = RemedyConstants.ColorTagEnd;

        if(isBold)
        {
            colorTagBeginning = RemedyConstants.BoldTagStart + colorTagBeginning;
            colorTagEnd = colorTagEnd + RemedyConstants.BoldTagEnd;
        }

        string formatted = string.Format("{0} {1} {2}", colorTagBeginning, str, colorTagEnd);

        return formatted;
    }

    ///<summary>
    /// Simple conversion from Color to Hex string.
    ///</summary>
    private static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }
    #endregion

    #region KeyPress
    private static Dictionary<KeyCode, List<KeyCodeBinding>> m_allKeyCodeBindings;
    public static Dictionary<KeyCode, List<KeyCodeBinding>> AllKeyCodeBindings { get { return m_allKeyCodeBindings; } }

    ///<summary>
    ///</summary>
    public static void OnKeyPress(KeyCode keyCode, OnKeyPressDelegate inAction, string description = "", bool dontDestroyOnLoad = false)
    {
        //Instantiate here b/c we can't use Awake() b/c Singleton<T> uses it and we can't guarantee Start() will be early enough.
        if(m_allKeyCodeBindings == null)
        {
            m_allKeyCodeBindings = new Dictionary<KeyCode, List<KeyCodeBinding>>();
        }

        KeyCodeBinding binding = new KeyCodeBinding();
        binding.BindedKeyCode = keyCode;
        binding.BindedAction = inAction;
        binding.Description = description;
        binding.DontDestroyOnLoad = dontDestroyOnLoad;

        StackFrame frame = new StackFrame(1, true);
        System.Reflection.MethodBase method = frame.GetMethod();
        binding.FunctionName = method.Name;
        binding.ClassName = method.DeclaringType.ToString();
        if(binding.Description == "")
        {
            binding.Description = "line " + frame.GetFileLineNumber().ToString();
        }


        List<KeyCodeBinding> bindingsForKeyCode;
        if(!m_allKeyCodeBindings.TryGetValue(keyCode, out bindingsForKeyCode))
        {
            bindingsForKeyCode = new List<KeyCodeBinding>();
            bindingsForKeyCode.Add(binding);
            m_allKeyCodeBindings.Add(keyCode, bindingsForKeyCode);
        }
        else
        {
            bindingsForKeyCode.Add(binding);
        }

        //UnityEngine.Debug.Log(string.Format("{0} {1}.{2}(): {3}",binding.BindedKeyCode.ToString(), binding.ClassName, binding.FunctionName, binding.Description));
    }

    ///<summary>
    ///</summary>
    private void KeyPressUpdate()
    {
        //Check the Remedy.KeyCodeChecks list and call any necessary functions.
        if (!Input.anyKeyDown)
            return;
        List<KeyCode> keyCodes = new List<KeyCode>(m_allKeyCodeBindings.Keys);
        for (int i = 0; i < keyCodes.Count; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                foreach (KeyCodeBinding binding in m_allKeyCodeBindings[keyCodes[i]])
                {
                    binding.BindedAction();
                }
            }
        }
    }

    ///<summary>
    ///</summary>
    public void DeleteKeyCodesOnNewScene(Scene newScene, LoadSceneMode sceneMode)
    {
        foreach(List<KeyCodeBinding> bindingList in m_allKeyCodeBindings.Values)
        {
            foreach(KeyCodeBinding binding in bindingList)
            {
                if(!binding.DontDestroyOnLoad)
                {
                    bindingList.Remove(binding);
                }
            }
        } 
    }

    #endregion
}
