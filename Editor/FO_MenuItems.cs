
using UnityEngine;
using UnityEditor;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    public static class FO_MenuItems
    {
        public const string FOLDER_PATH = "KitKat/JetSim/Floating Origin/";


        [MenuItem(FOLDER_PATH + "📝 README", priority = -10000)]
        private static void OpenReadme() => Application.OpenURL("https://github.com/KitKat4191/JetSim-FloatingOrigin/blob/main/README.md");

        ////////////////////////

        [MenuItem(FOLDER_PATH + "Install", priority = 80)]
        private static void Install() => FO_Installer.Install();

        ////////////////////////

        [MenuItem(FOLDER_PATH + "Execute All Automation", priority = 100)]
        private static void ExecuteAutomation() => FO_BuildValidator.RunOnBuild();

        ////////////////////////
        
        [MenuItem(FOLDER_PATH + "Add Station Notifiers", priority = 120)]
        private static void AddStationNotifiers() => FO_BuildValidator.SetUpStationNotifiers();

        [MenuItem(FOLDER_PATH + "Remove Station Notifiers", priority = 121)]
        private static void RemoveStationNotifiers() => FO_BuildValidator.RemoveAllStationNotifiers();

        ////////////////////////

        [MenuItem(FOLDER_PATH + "Validate Particle Systems", priority = 140)]
        private static void ValidateParticleSystems() => FO_Installer.ValidateParticleSystems();

        [MenuItem(FOLDER_PATH + "Restore Simulation Spaces", priority = 141)]
        private static void RestoreParticleSimulationSpaces() => FO_BuildValidator.RestoreParticleSimulationSpaces();

        ////////////////////////

        [MenuItem(FOLDER_PATH + "Disable All Static Flags", priority = 400)]
        private static void DisableStaticFlags() => FO_BuildValidator.DisableStatics();
    }
}