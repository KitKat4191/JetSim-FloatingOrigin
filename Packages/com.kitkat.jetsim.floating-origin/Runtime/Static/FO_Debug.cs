
//#define DO_LOGGING TODO: make preferences checkbox that toggles this as a project define symbol. 

using UnityEngine;

namespace KitKat.JetSim.FloatingOrigin.Runtime
{
    internal static class FO_Debug
    {
        private const string _LOG_IDENTIFIER = "[<color=purple>Floating Origin</color>]";
        
        internal static void Log(string message, Object context = null)
        {
#if DO_LOGGING
            Debug.Log($"{_LOG_IDENTIFIER} <color=lightblue>{message}</color>", context);
#endif
        }

        internal static void LogSuccess(string message, Object context = null)
        {
#if DO_LOGGING
            Debug.Log($"{_LOG_IDENTIFIER} <color=lime>{message}</color>", context);
#endif
        }

        internal static void LogWarning(string message, Object context = null)
        {
#if DO_LOGGING
            Debug.LogWarning($"{_LOG_IDENTIFIER} <color=orange>{message}</color>", context);
#endif
        }

        internal static void LogError(string message, Object context = null)
        {
#if DO_LOGGING
            Debug.LogError($"{_LOG_IDENTIFIER} <color=red>{message}</color>", context);
#endif
        }
    }
}
