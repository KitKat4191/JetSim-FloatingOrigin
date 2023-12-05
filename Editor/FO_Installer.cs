
using UnityEditor;
using UnityEngine;

using VRRefAssist.Editor.Extensions;
using KitKat.JetSim.FloatingOrigin.Runtime;

using System.Linq;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    public class FO_Installer
    {
        [MenuItem("KitKat/JetSim/Floating Origin/Install", priority = 80)]
        private static void Install()
        {
            AddPrefabToScene();
            ValidateParticleSystems();
        }

        private static void AddPrefabToScene()
        {
            var manager = UnityEditorExtensions.FindObjectOfTypeIncludeDisabled<FO_Manager>();
            if (manager)
            {
                FO_Logger._printWarning("Prefab was already present in the scene!", manager);
                Selection.activeObject = manager;
                EditorGUIUtility.PingObject(manager);
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>("Packages/com.kitkat.jetsim.floating-origin/Runtime/Prefabs/FloatingOrigin.prefab");
            GameObject prefab = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            Undo.RegisterCreatedObjectUndo(prefab, "Created FloatingOrigin Prefab");

            manager = UnityEditorExtensions.FindObjectOfTypeIncludeDisabled<FO_Manager>();
            Selection.activeObject = manager;
            EditorGUIUtility.PingObject(manager);

            FO_Logger._printSuccess("Added Floating Origin prefab to scene.");
        }

        private static void ValidateParticleSystems()
        {
            ParticleSystem[] particleSystems = UnityEditorExtensions.FindObjectsOfTypeIncludeDisabled<ParticleSystem>().Where(p =>
                    p.main.simulationSpace == ParticleSystemSimulationSpace.Custom &&
                    p.main.customSimulationSpace == null
                ).ToArray();

            if (particleSystems.Length == 0) { FO_Logger._printSuccess("All particle systems validated."); return; }

            int input = EditorUtility.DisplayDialogComplex(
                    title: "JetSim - FloatingOrigin",
                    message: $"Found {particleSystems.Length} particle systems with a null simulation space.\n" +
                    "This can cause issues. A null custom simulation space acts the same as a Local simulation space.\n" +
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
    }
}