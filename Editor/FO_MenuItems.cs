
using UnityEngine;
using UnityEditor;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    public static class FO_MenuItems
    {
        private const string _FOLDER_PATH = "KitKat/JetSim/Floating Origin/";


        [MenuItem(_FOLDER_PATH + "📝 README", priority = -10000)]
        private static void OpenReadme() => Application.OpenURL("https://github.com/KitKat4191/JetSim-FloatingOrigin/blob/main/README.md");

        ////////////////////////

        [MenuItem(_FOLDER_PATH + "Install", priority = 80)]
        private static void Install() => FO_Installer.Install();

        ////////////////////////

        [MenuItem(_FOLDER_PATH + "Execute All Automation", priority = 100)]
        private static void ExecuteAutomation() => FO_BuildValidator.RunOnBuild();

        ////////////////////////
        
        [MenuItem(_FOLDER_PATH + "Add Station Notifiers", priority = 120)]
        private static void AddStationNotifiers() => FO_BuildValidator.SetUpStationNotifiers();

        [MenuItem(_FOLDER_PATH + "Remove Station Notifiers", priority = 121)]
        private static void RemoveStationNotifiers() => FO_BuildValidator.RemoveAllStationNotifiers();

        ////////////////////////

        [MenuItem(_FOLDER_PATH + "Fix Particle Simulation Spaces", priority = 140)]
        private static void SetUpParticleSystems() => FO_BuildValidator.SetUpParticleSystems();

        [MenuItem(_FOLDER_PATH + "Restore Particle Simulation Spaces", priority = 141)]
        private static void RestoreParticleSimulationSpaces() => FO_BuildValidator.RestoreParticleSimulationSpaces();

        ////////////////////////

        [MenuItem(_FOLDER_PATH + "Particle System Repair", priority = 200)]
        private static void ParticleSystemRepair() => FO_BuildValidator.ParticleSystemRepair();

        ////////////////////////

        [MenuItem(_FOLDER_PATH + "Disable All Static Flags", priority = 400)]
        private static void DisableStaticFlags() => FO_BuildValidator.DisableStatics();
    }
}