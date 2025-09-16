
using UdonSharp;
using UnityEditor;
using UnityEditor.Compilation;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    internal static class FO_Defines
    {
        private const string _PACKAGE_DEFINE = "JS_FLOATING_ORIGIN";
        private const string _DEBUG_DEFINE = _PACKAGE_DEFINE + "_ENABLE_LOGGING";

        [InitializeOnLoadMethod]
        internal static void UpdatePackageDefines()
        {
            bool definesChanged = false;
            definesChanged |= TryAddDefine(_PACKAGE_DEFINE);
            definesChanged |= SetDebugDefine(FO_Preferences.GetOrCreate().EnableDebugMode);

            if (definesChanged)
            {
                CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
                UdonSharpProgramAsset.CompileAllCsPrograms(true);
            }
        }

        private static bool SetDebugDefine(bool enable)
        {
            return enable ? TryAddDefine(_DEBUG_DEFINE) : TryRemoveDefine(_DEBUG_DEFINE);
        }

        private static bool TryAddDefine(string define)
        {
            BuildTargetGroup platform = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
            
            if (defines.Contains(define)) return false;
            if (defines.Length > 0) defines += ";";
            defines += define;
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, defines);
            
            return true;
        }

        private static bool TryRemoveDefine(string define)
        {
            BuildTargetGroup platform = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
            
            if (!defines.Contains(define)) return false;
            defines = defines.Replace(define + ";", "");
            defines = defines.Replace(define, "");
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, defines);
            
            return true;
        }
    }
}
