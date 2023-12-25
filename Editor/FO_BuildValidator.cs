
using System;
using System.Linq;

using UnityEngine;
using UnityEditor;

using VRC.Udon;
using VRC.SDKBase;
using UdonSharpEditor;

using KitKat.JetSim.FloatingOrigin.Runtime;

using VRRefAssist;
using VRRefAssist.Editor.Extensions;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    public class FO_BuildValidator : UnityEditor.Editor
    {
        [RunOnBuild(int.MinValue)]
        [MenuItem("KitKat/JetSim/Floating Origin/Execute All Automation", priority = 100)]
        public static void RunOnBuild()
        {
            if (!UsingFloatingOrigin()) return;
            if (ObjectSyncIssues()) return;

            RemoveAllStationNotifiers();
            SetUpStationNotifiers();
            SetUpParticleSystems();
        }

        private static bool UsingFloatingOrigin()
        {
            FO_Manager[] floatingOriginManagers = UnityEditorExtensions.FindObjectsOfTypeIncludeDisabled<FO_Manager>();

            if (floatingOriginManagers.Length == 0) return false; // I'm assuming you don't want to use the floating origin system if you haven't set it up.
            if (floatingOriginManagers.Length > 1)
            {
                FO_Logger._printError($"{floatingOriginManagers.Length} Floating Origin Managers found! Please ensure there is only one instance of it in the scene.");
                throw new Exception("Setup was invalid, there are several FO_Managers present in the scene.");
            }

            return true;
        }

        private static bool ObjectSyncIssues()
        {
            var settings = FO_Preferences.GetOrCreate();

            var syncs = UnityEditorExtensions.FindObjectsOfTypeIncludeDisabled<VRC.SDK3.Components.VRCObjectSync>();
            if (syncs.Length == 0) return false;

            if (settings.ShowObjectSyncWarning)
            {
                FO_Logger._printWarning($"You are using VRCObjectSync in your project! These objects will be desynced compared to the world and players. Is this intentional?");
                
                foreach (var sync in syncs)
                {
                    FO_Logger._printWarning("Click me to highlight the object with VRCObjectSync!", sync);
                }
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
                FO_Logger._printError("Build aborted by user.");
                throw new Exception("Build aborted by user.");
            }
            
            return false;

            #endregion // MODAL WINDOW
        }


        #region STATION NOTIFIERS

        [MenuItem("KitKat/JetSim/Floating Origin/Set Up StationNotifiers", priority = 120)]
        private static void SetUpStationNotifiers()
        {
            VRCStation[] stations = UnityEditorExtensions.FindObjectsOfTypeIncludeDisabled<VRCStation>();
            stations = stations.Where(s => s.GetComponent<FO_PlayerStation>() == null).ToArray();
            
            if (stations.Length == 0) return;

            FO_Logger._print($"{stations.Length} stations found.");

            foreach (VRCStation station in stations)
                station.gameObject.AddUdonSharpComponent(typeof(FO_StationNotifier));

            FO_Logger._printSuccess($"Created {stations.Length} StationNotifier{(stations.Length > 1 ? "s" : "")}.");

            // Match the sync mode to the existing behaviours on the station.
            var notifiers = UnityEditorExtensions.FindObjectsOfTypeIncludeDisabled<FO_StationNotifier>();
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

        [MenuItem("KitKat/JetSim/Floating Origin/Remove All StationNotifiers", priority = 121)]
        private static void RemoveAllStationNotifiers()
        {
            var notifiers = UnityEditorExtensions.FindObjectsOfTypeIncludeDisabled<FO_StationNotifier>();
            foreach (var notifier in notifiers)
                UdonSharpEditorUtility.DestroyImmediate(notifier);

            if (notifiers.Length == 0)
                FO_Logger._print("There were no StationNotifiers to remove.");
            if (notifiers.Length > 0)
                FO_Logger._printSuccess($"Removed {notifiers.Length} StationNotifier{(notifiers.Length > 1 ? "s" : "")}.");
        }

        #endregion // STATION NOTIFIERS

        #region PARTICLES

        [MenuItem("KitKat/JetSim/Floating Origin/Set Up Particle Simulation Spaces", priority = 140)]
        private static void SetUpParticleSystems()
        {
            Transform anchor = UnityEditorExtensions.FindObjectOfTypeIncludeDisabled<FO_Manager>().transform.GetChild(0);

            int particleSystemsChanged = 0;
            foreach (ParticleSystem particle in UnityEditorExtensions.FindObjectsOfTypeIncludeDisabled<ParticleSystem>())
            {
                ParticleSystem.MainModule particleSystemMain = particle.main;
                if (particleSystemMain.simulationSpace != ParticleSystemSimulationSpace.World) continue; // We only care about world-space particle systems

                particleSystemMain.simulationSpace = ParticleSystemSimulationSpace.Custom;
                particleSystemMain.customSimulationSpace = anchor;

                EditorUtility.SetDirty(particle);
                particleSystemsChanged++;
            }
            if (particleSystemsChanged == 0) { FO_Logger._print("No particle systems changed."); return; }
            FO_Logger._printSuccess($"Set up simulation space of {particleSystemsChanged} ParticleSystem{(particleSystemsChanged == 1 ? "" : "s")}!");
        }
        [MenuItem("KitKat/JetSim/Floating Origin/Restore Particle Simulation Spaces", priority = 141)]
        private static void RestoreParticleSimulationSpaces()
        {
            Transform anchor = UnityEditorExtensions.FindObjectOfTypeIncludeDisabled<FO_Manager>().transform.GetChild(0);

            int particleSystemsChanged = 0;
            foreach (ParticleSystem particle in UnityEditorExtensions.FindObjectsOfTypeIncludeDisabled<ParticleSystem>())
            {
                ParticleSystem.MainModule particleSystemMain = particle.main;
                if (particleSystemMain.simulationSpace != ParticleSystemSimulationSpace.Custom) continue;
                if (particleSystemMain.customSimulationSpace != anchor) continue;

                particleSystemMain.simulationSpace = ParticleSystemSimulationSpace.World;

                EditorUtility.SetDirty(particle);
                particleSystemsChanged++;
            }
            if (particleSystemsChanged == 0) { FO_Logger._print("There were no particle systems to restore."); return; }
            FO_Logger._printSuccess($"Restored simulation space of {particleSystemsChanged} ParticleSystem{(particleSystemsChanged == 1 ? "" : "s")}!");
        }

        [MenuItem("KitKat/JetSim/Floating Origin/Particle System Repair", priority = 152)]
        private static void ParticleSystemRepair()
        {
            ParticleSystem[] particleSystems = UnityEditorExtensions.FindObjectsOfTypeIncludeDisabled<ParticleSystem>().Where(p => 
                    p.main.simulationSpace == ParticleSystemSimulationSpace.Custom && 
                    p.main.customSimulationSpace == null
                ).ToArray();

            if (particleSystems.Length == 0) { FO_Logger._print("No particle systems to repair."); return; }

            Selection.objects = particleSystems;

            int input = EditorUtility.DisplayDialogComplex(
                    title: "JetSim - FloatingOrigin",
                    message: $"Found {particleSystems.Length} particle systems with a custom simulation space set to null.\n" +
                    "Which simulation space would you like to change them to?",
                    // Buttons:
                    ok: "World",
                    alt: "Local",
                    cancel: "Cancel");

            // Cancel == 1
            if (input == 1) return;

            foreach (ParticleSystem particle in particleSystems)
            {
                ParticleSystem.MainModule particleSystemMain = particle.main;

                particleSystemMain.simulationSpace = 
                    input == 0 ? 
                    ParticleSystemSimulationSpace.World : 
                    ParticleSystemSimulationSpace.Local;

                FO_Logger._print($"Updated simulation space.", particle);
            }

            FO_Logger._printSuccess($"Updated simulation space on {particleSystems.Length} ParticleSystems.");
        }

        #endregion // PARTICLES
    }
}