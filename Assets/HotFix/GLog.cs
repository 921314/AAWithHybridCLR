using UnityEngine;
using System.Text;
using System.IO;

public class GLog : MonoBehaviour
{
    public static bool s_debugLogEnable = true;
    public static bool s_warningLogEnable = true;
    public static bool s_errorLogEnable = true;

    private static StringBuilder s_logStr = new StringBuilder();
    private static string s_logFileSavePath;

    private static bool s_isSelfLog = false;
    public static void Init()
    {
        var sDate = System.DateTime.Now.ToString("yyyyMMddhhmmss");
#if UNITY_STANDALONE || UNITY_EDITOR
        var sLogDir = string.Format("{0}/../gamelog/", Application.dataPath);
#else
        var sLogDir = string.Format("{0}/gamelog/", Application.persistentDataPath);
#endif
        if (!Directory.Exists(sLogDir))
            Directory.CreateDirectory(sLogDir);
        s_logFileSavePath = string.Format("{0}/output_{1}.txt", sLogDir, sDate);
        Application.logMessageReceived += OnLogCallBack;
    }

    private static void OnLogCallBack(string sCondition, string sStackTrace, LogType logType)
    {
        s_logStr.Append(sCondition);
        s_logStr.Append("\n");
        if (logType == LogType.Error || logType == LogType.Exception)
        {
            s_logStr.Append(sStackTrace);
            s_logStr.Append("\n");
        }

        if (s_logStr.Length <= 0) return;
        if (!File.Exists(s_logFileSavePath))
        {
            var fs = File.Create(s_logFileSavePath);
            fs.Close();
        }
        using (var sw = File.AppendText(s_logFileSavePath))
        {
            sw.WriteLine(s_logStr.ToString());
        }
        s_logStr.Remove(0, s_logStr.Length);
    }

    public static void Log(object message, Object context = null)
    {
        if (!s_debugLogEnable) return;
        Debug.unityLogger.logEnabled = true;
        Debug.Log(message, context);
        if (s_isSelfLog)
        {
            Debug.unityLogger.logEnabled = false;
        }
    }

    public static void Warn(object message, Object context = null)
    {
        if (!s_warningLogEnable) return;
        Debug.LogWarning(message, context);
    }

    public static void Error(object message, Object context = null)
    {
        if (!s_errorLogEnable) return;
        Debug.LogError(message, context);
    }

    public static void LogFormat(string format, params object[] args)
    {
        if (!s_debugLogEnable) return;
        Debug.LogFormat(format, args);
    }

    public static void LogWithColor(object message, string color, object context = null)
    {
        if (!s_debugLogEnable) return;
        Debug.Log(FmtColor(color, message));
    }

    public static void LogRed(object message, Object context = null)
    {
        if (!s_debugLogEnable) return;
        Debug.Log(FmtColor("red", message), context);
    }

    public static void LogGreen(object message, Object context = null)
    {
        if (!s_debugLogEnable) return;
        Debug.Log(FmtColor("#00ff00", message), context);
    }

    public static void LogYellow(object message, Object context = null)
    {
        if (!s_debugLogEnable) return;
        Debug.Log(FmtColor("yellow", message), context);
    }

    public static void LogCyan(object message, Object context = null)
    {
        if (!s_debugLogEnable) return;
        Debug.Log(FmtColor("#00ffff", message), context);
    }

    public static void LogFormatWithColor(string format, string color, params object[] args)
    {
        if (!s_debugLogEnable) return;
        Debug.LogFormat((string)FmtColor(color, format), args);
    }

    private static object FmtColor(string color, object obj)
    {
        if (obj is string)
        {
#if !UNITY_EDITOR
            return obj;
#else
            return FmtColor(color, (string)obj);
#endif
        }
        else
        {
#if !UNITY_EDITOR
            return obj;
#else
            return string.Format("<color={0}>{1}</color>", color, obj);
#endif
        }
    }

    private static object FmtColor(string color, string msg)
    {
#if !UNITY_EDITOR
        return msg;
#else
        int p = msg.IndexOf('\n');
        if (p >= 0) p = msg.IndexOf('\n', p + 1);// 可以同时显示两行
        if (p < 0 || p >= msg.Length - 1) return string.Format("<color={0}>{1}</color>", color, msg);
        if (p > 2 && msg[p - 1] == '\r') p--;
        return string.Format("<color={0}>{1}</color>{2}", color, msg.Substring(0, p), msg.Substring(p));
#endif
    }

    public static bool Assert(bool conodition, string errorMsg)
    {
        if (!conodition)
            Error(errorMsg);
        return conodition;
    }

    #region 解决日志双击溯源问题
#if UNITY_EDITOR
    [UnityEditor.Callbacks.OnOpenAssetAttribute(0)]
    static bool OnOpenAsset(int instanceID, int line)
    {
        string stackTrace = GetStackTrace();
        if (!string.IsNullOrEmpty(stackTrace) && stackTrace.Contains("GameLogger:Log"))
        {
            // 使用正则表达式匹配at的哪个脚本的哪一行
            var matches = System.Text.RegularExpressions.Regex.Match(stackTrace, @"\(at (.+)\)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            string pathLine = "";
            while (matches.Success)
            {
                pathLine = matches.Groups[1].Value;

                if (!pathLine.Contains("GameLogger.cs"))
                {
                    int splitIndex = pathLine.LastIndexOf(":");
                    // 脚本路径
                    string path = pathLine.Substring(0, splitIndex);
                    // 行号
                    line = System.Convert.ToInt32(pathLine.Substring(splitIndex + 1));
                    string fullPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                    fullPath = fullPath + path;
                    // 跳转到目标代码的特定行
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath.Replace('/', '\\'), line);
                    break;
                }
                matches = matches.NextMatch();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取当前日志窗口选中的日志的堆栈信息
    /// </summary>
    /// <returns></returns>
    static string GetStackTrace()
    {
        // 通过反射获取ConsoleWindow类
        var ConsoleWindowType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
        // 获取窗口实例
        var fieldInfo = ConsoleWindowType.GetField("ms_ConsoleWindow",
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.NonPublic);
        var consoleInstance = fieldInfo.GetValue(null);
        if (consoleInstance != null)
        {
            if ((object)UnityEditor.EditorWindow.focusedWindow == consoleInstance)
            {
                // 获取m_ActiveText成员
                fieldInfo = ConsoleWindowType.GetField("m_ActiveText",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
                // 获取m_ActiveText的值
                string activeText = fieldInfo.GetValue(consoleInstance).ToString();
                return activeText;
            }
        }
        return null;
    }
#endif
    #endregion 解决日志双击溯源问题

}