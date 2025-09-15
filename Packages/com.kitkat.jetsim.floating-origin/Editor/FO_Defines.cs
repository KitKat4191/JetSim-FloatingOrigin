
using UnityEditor;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    internal static class FO_Defines
    {
        private const string _PACKAGE_DEFINE = "JS_FLOATING_ORIGIN";
        private const string _DEBUG_DEFINE = _PACKAGE_DEFINE + "_ENABLE_LOGGING";

        [InitializeOnLoadMethod]
        private static void AddPackageDefines()
        {
            AddDefine(_PACKAGE_DEFINE);
            SetDebugDefine(FO_Preferences.GetOrCreate().EnableDebugMode);
        }

        private static void SetDebugDefine(bool enable)
        {
            if (enable) AddDefine(_DEBUG_DEFINE);
            else RemoveDefine(_DEBUG_DEFINE);
        }

        private static void AddDefine(string define)
        {
            BuildTargetGroup platform = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
            
            if (defines.Contains(define)) return;
            if (defines.Length > 0) defines += ";";
            defines += define;
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, defines);
        }

        private static void RemoveDefine(string define)
        {
            BuildTargetGroup platform = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
            
            if (!defines.Contains(define)) return;
            defines = defines.Replace(define + ";", "");
            defines = defines.Replace(define, "");
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, defines);
        }
    }
}
