
using System;
using System.Linq;

using UnityEngine;
using UnityEditor;

using VRC.Udon;
using VRC.SDKBase;
using UdonSharpEditor;

using KitKat.JetSim.FloatingOrigin.Runtime;
using VRC.SDK3.Components;
using VRRefAssist;

using VRCStation = VRC.SDKBase.VRCStation;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    public class FO_BuildValidator : UnityEditor.Editor
    {
        [RunOnBuild(int.MinValue + 1000)]
        public static void RunOnBuild()
        {
            if (!UsingFloatingOrigin()) return;
            if (FoundObjectSyncIssues()) return;

            RemoveAllStationNotifiers();
            SetUpStationNotifiers();
            SetUpParticleSystems();
            ConfigureSceneDescriptor();
        }

        public static void DisableStatics()
        {
            var objects = FindObjectsOfType<GameObject>(true);
            foreach (var obj in objects) { obj.isStatic = false; }
            FO_Debugger.LogSuccess("Cleared All Static Flags.");
        }

        private static void ConfigureSceneDescriptor()
        {
            var descriptor = FindObjectOfType<VRC_SceneDescriptor>(true);
            descriptor.RespawnHeightY = -1000000f;
            EditorUtility.SetDirty(descriptor);
        }

        #region STATION NOTIFIERS

        public static void SetUpStationNotifiers()
        {
            VRCStation[] stations = FindObjectsOfType<VRCStation>(true);
            stations = stations.Where(s => s.GetComponent<FO_PlayerStation>() == null).ToArray();
            
            if (stations.Length == 0) return;

            FO_Debugger.Log($"{stations.Length} stations found.");

            foreach (VRCStation station in stations)
                station.gameObject.AddUdonSharpComponent(typeof(FO_StationNotifier));

            FO_Debugger.LogSuccess($"Created {stations.Length} StationNotifier{(stations.Length > 1 ? "s" : "")}.");

            // Match the sync mode to the existing behaviours on the station.
            var notifiers = FindObjectsOfType<FO_StationNotifier>(true);
            foreach (var notifier in notifiers)
            {
                UdonBehaviour backingNotifier = UdonSharpEditorUtility.GetBackingUdonBehaviour(notifier);

                Networking.SyncType syncType = Networking.SyncType.None;

                var behaviours = backingNotifier.GetComponents<UdonBehaviour>().Where(x => x != backingNotifier).ToArray();

                if (behaviours.Length > 0)
                    if (behaviours[0])
                        syncType = behaviours[0].SyncMethod;

                backingNotifier.SyncMethod = syncType;
            }
        }

        public static void RemoveAllStationNotifiers()
        {
            var notifiers = FindObjectsOfType<FO_StationNotifier>(true);
            foreach (var notifier in notifiers)
                UdonSharpEditorUtility.DestroyImmediate(notifier);

            if (notifiers.Length == 0)
                FO_Debugger.Log("There were no StationNotifiers to remove.");
            if (notifiers.Length > 0)
                FO_Debugger.LogSuccess($"Removed {notifiers.Length} StationNotifier{(notifiers.Length > 1 ? "s" : "")}.");
        }

        #endregion // STATION NOTIFIERS

        #region PARTICLES

        /// <summary>
        /// Finds all particle systems with simulation space world and changes them to custom.
        /// </summary>
        public static void SetUpParticleSystems()
        {
            Transform anchor = FindObjectOfType<FO_Manager>(true).anchor;
            ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>(true).Where(p => p.main.simulationSpace == ParticleSystemSimulationSpace.World).ToArray();

            foreach (ParticleSystem particle in particleSystems)
            {
                ParticleSystem.MainModule particleSystemMain = particle.main;

                particleSystemMain.simulationSpace = ParticleSystemSimulationSpace.Custom;
                particleSystemMain.customSimulationSpace = anchor;
                FO_Debugger.Log("Updated simulation space.", particle);
                EditorUtility.SetDirty(particle);
            }

            if (particleSystems.Length == 0) { FO_Debugger.Log("There were no particle systems to set up."); return; }
            FO_Debugger.LogSuccess($"Updated simulation space to Custom on {particleSystems.Length} ParticleSystems.");
        }
        
        public static void RestoreParticleSimulationSpaces()
        {
            Transform anchor = FindObjectOfType<FO_Manager>(true).anchor;
            ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>(true).Where(p => 
                p.main.simulationSpace == ParticleSystemSimulationSpace.Custom &&
                p.main.customSimulationSpace == anchor
            ).ToArray();

            foreach (ParticleSystem particle in particleSystems)
            {
                ParticleSystem.MainModule particleSystemMain = particle.main;
                particleSystemMain.simulationSpace = ParticleSystemSimulationSpace.World;
                FO_Debugger.Log("Updated simulation space.", particle);
                EditorUtility.SetDirty(particle);
            }

            if (particleSystems.Length == 0) { FO_Debugger.Log("There were no particle systems to restore."); return; }
            FO_Debugger.LogSuccess($"Updated simulation space to World on {particleSystems.Length} ParticleSystems.");
        }

        #endregion // PARTICLES

        #region INTERNAL

        private static bool UsingFloatingOrigin()
        {
            FO_Manager[] floatingOriginManagers = FindObjectsOfType<FO_Manager>(true);

            if (floatingOriginManagers.Length == 0) return false; // I'm assuming you don't want to use the floating origin system if you haven't set it up.
            if (floatingOriginManagers.Length > 1)
            {
                FO_Debugger.LogError($"{floatingOriginManagers.Length} Floating Origin Managers found! Please ensure there is only one instance of it in the scene.");
                throw new Exception("Setup was invalid, there are several FO_Managers present in the scene.");
            }

            return true;
        }

        private static bool FoundObjectSyncIssues()
        {
            var settings = FO_Preferences.GetOrCreate();

            VRCObjectSync[] syncs = FindObjectsOfType<VRCObjectSync>(true);
            if (syncs.Length == 0) return false;

            if (settings.ShowObjectSyncWarning)
            {
                FO_Debugger.LogWarning("You are using VRCObjectSync in your project! These objects will be desynced compared to the world and players. Is this intentional?");
                foreach (VRCObjectSync sync in syncs) { FO_Debugger.LogWarning("Click me to highlight the object with VRCObjectSync!", sync); }
            }

            #region MODAL WINDOW

            if (!settings.ShowObjectSyncPopup) return false;

            int input = EditorUtility.DisplayDialogComplex(
                    title: "JetSim - FloatingOrigin",
                    message: "You are using VRCObjectSync in your project!\n" +
                    "VRCObjectSync only works in world space.\n" +
                    "These objects will be desynced compared to the world and players.",
                    // Buttons:
                    ok: "Don't show again",
                    cancel: "Okay",
                    alt: "Abort Build");

            if (input == 0)
            {
                settings.ShowObjectSyncPopup = false;
                EditorUtility.SetDirty(settings);
            }

            if (input == 2)
            {
                FO_Debugger.LogError("Build aborted by user.");
                throw new Exception("Build aborted by user.");
            }

            return false;

            #endregion // MODAL WINDOW
        }

        #endregion // INTERNAL
    }
}