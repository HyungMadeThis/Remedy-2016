using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;

/// <summary>
/// Replacement LogHandler.
/// If both FilteredClassTypes and FilteredNamespaces are null, no logs will be filtered.
/// If either is not null, all logs will be filtered.
/// Debug.Log's are never filtered.
/// </summary>
//[UnityEditor.InitializeOnLoad]
public class Remedy : ILogHandler
{
    private static ILogHandler m_defaultLogHandler = UnityEngine.Debug.unityLogger.logHandler;
    private static MD5 m_md5 = MD5.Create();

    public static List<Type> FilteredClassTypes = null;
    public static List<string> FilteredNamespaces = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InstantiateRemedy()
    {
        UnityEngine.Debug.unityLogger.logHandler = new Remedy();
    }

    public static void Log(string str) { Log(LogType.Log, null, 2, "{0}", str); }
    public static void LogWarning(string str) { Log(LogType.Warning, null, 2, "{0}", str); }
    public static void LogError(string str) { Log(LogType.Error, null, 2, "{0}", str); }

    public static void LogFormat(string format, params object[] args) { Log(LogType.Log, null, 2, format, args); }
    public static void LogWarningFormat(string format, params object[] args) { Log(LogType.Warning, null, 2, format, args); }
    public static void LogErrorFormat(string format, params object[] args) { Log(LogType.Error, null, 2, format, args); }

    /// <summary>
    /// This replaces the default Debug.Log~
    /// </summary>
    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        Log(logType, context, 3, format, args);
    }

    /// <summary>
    /// Simple passthrough to original way of logging exceptions.
    /// </summary>
    public void LogException(Exception exception, UnityEngine.Object context)
    {
        m_defaultLogHandler.LogException(exception, context);
    }

    private static void Log(LogType logType, UnityEngine.Object context, int stackLevel, string format, params object[] args)
    {
        StackFrame frame = new StackFrame(stackLevel, true);
        Type callingClass = frame.GetMethod().ReflectedType;

        if (!CheckFilter(callingClass))
            return;

#if UNITY_EDITOR
        string formatWithColor = FormatStringWithColor(format as string, GetLogTypeColor(logType), true);
        string formattedCallingClass = FormatStringWithColor("[" + callingClass.Name + "] ", (callingClass != typeof(UnityEngine.Debug)) ? GetColorFromString(callingClass.Name) : Color.gray, true);
        m_defaultLogHandler.LogFormat(logType, context, formattedCallingClass + formatWithColor, args);
#else
        m_defaultLogHandler.LogFormat(logType, context, "[" + callingClass.Name + "] " + format, args);
#endif
    }

    private static bool CheckFilter(Type classType)
    {
        return (classType == typeof(UnityEngine.Debug))
            || (FilteredClassTypes == null && FilteredNamespaces == null)
            || (FilteredClassTypes != null && FilteredClassTypes.Contains(classType))
            || (FilteredNamespaces != null && FilteredNamespaces.Contains(classType.Namespace));
    }

    #region Formatting and Color

    ///<summary>
    /// Formats the calling class to use rich text for color and bolding.
    ///</summary>
    private static string FormatStringWithColor(string str, Color textColor, bool isBold = false)
    {
        if (isBold)
            return "<b><color=#" + ColorToHex(textColor) + "FF>" + str + "</color></b>";
        else
            return "<color=#" + ColorToHex(textColor) + "FF>" + str + "</color>";
    }

    /// <summary>
    /// Get a unique but deterministic color from any string.
    /// </summary>
    private static Color GetColorFromString(string value)
    {
        var hash = m_md5.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Color.HSVToRGB(hash[0] / 256f, 1, 1);
    }

    ///<summary>
    /// Simple conversion from Color to Hex string.
    ///</summary>
    private static string ColorToHex(Color32 color)
    {
        return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
    }

    public static Color GetLogTypeColor(LogType logType)
    {
        switch (logType)
        {
            case LogType.Log:
                return Color.black;
            case LogType.Warning:
                return Color.yellow;
            case LogType.Error:
                return Color.red;
            default:
                return Color.gray;
        }
    }
    #endregion
}