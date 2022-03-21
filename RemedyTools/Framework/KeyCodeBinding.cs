using UnityEngine;

namespace RemedyDebug
{
	public delegate void OnKeyPressDelegate();

    public class KeyCodeBinding
    {
		private KeyCode m_bindedKeycode;
		private OnKeyPressDelegate m_bindedAction;
		private string m_description = "";
		private string m_className;
		private string m_functionName;
		private bool m_dontDestroyOnLoad = false;

		public KeyCode BindedKeyCode { get { return m_bindedKeycode; } set { m_bindedKeycode = value; } }
		public OnKeyPressDelegate BindedAction { get { return m_bindedAction; } set { m_bindedAction = value; } }
		public string Description { get { return m_description; } set { m_description = value; } }
		public string ClassName { get { return m_className; } set { m_className = value; } }
		public string FunctionName { get { return m_functionName; } set { m_functionName = value; } }
		public bool DontDestroyOnLoad { get { return m_dontDestroyOnLoad; } set { m_dontDestroyOnLoad = value; } }
    }
}
