
using UnityEngine;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    public static class FO_Logger
    {
        private const string _callerName = "FloatingOrigin";
        private const string _logColor = "magenta";
        public static void _print(string message, Object context = null)
        {
            Debug.Log($"[<color={_logColor}>{_callerName}</color>]: <color=lightblue>{message}</color>", context);
        }

        public static void _printSuccess(string message, Object context = null)
        {
            Debug.Log($"[<color={_logColor}>{_callerName}</color>]: <color=lime>{message}</color>", context);
        }

        public static void _printWarning(string message, Object context = null)
        {
            Debug.LogWarning($"[<color={_logColor}>{_callerName}</color>]: <color=orange>{message}</color>", context);
        }

        public static void _printError(string message, Object context = null)
        {
            Debug.LogError($"[<color={_logColor}>{_callerName}</color>]: <color=red>{message}</color>", context);
        }
    }
}