﻿
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

using VRRefAssist.Editor.Extensions;
using KitKat.JetSim.FloatingOrigin.Runtime;

using System.Linq;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    public class FO_Installer
    {
        #region API

        public static void Install()
        {
            if (!ValidateParticleSystems()) return;
            var root = GetAllRootTransforms();
            if (!AddPrefabToScene()) return;
            ParentTransformsToWorldParent(root);
        }

        #endregion // API

        #region INTERNAL

        private static Transform[] GetAllRootTransforms()
        {
            GameObject[] rootGos = SceneManager.GetActiveScene().GetRootGameObjects();
            return rootGos.Select(x => x.transform).ToArray();
        }

        /// <returns>
        /// If the prefab was added.
        /// </returns>
        private static bool AddPrefabToScene()
        {
            var manager = UnityEditorExtensions.FindObjectOfTypeIncludeDisabled<FO_Manager>();
            if (manager)
            {
                FO_Debugger.LogWarning("Prefab was already present in the scene!", manager);
                Selection.activeObject = manager;
                EditorGUIUtility.PingObject(manager);
                return false;
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>("Packages/com.kitkat.jetsim.floating-origin/Runtime/Prefabs/FloatingOrigin.prefab");
            GameObject prefab = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            Undo.RegisterCreatedObjectUndo(prefab, "Created FloatingOrigin Prefab");

            manager = UnityEditorExtensions.FindObjectOfTypeIncludeDisabled<FO_Manager>();
            Selection.activeObject = manager;
            EditorGUIUtility.PingObject(manager);

            FO_Debugger.LogSuccess("Added Floating Origin prefab to scene.");
            return true;
        }

        private static void ParentTransformsToWorldParent(Transform[] objects)
        {
            if (objects == null) return;
            if (objects.Length == 0) return;

            var manager = UnityEditorExtensions.FindObjectOfTypeIncludeDisabled<FO_Manager>().transform;
            foreach (Transform t in objects) { t.parent = manager; }
        }

        private static bool ValidateParticleSystems()
        {
            ParticleSystem[] particleSystems = UnityEditorExtensions.FindObjectsOfTypeIncludeDisabled<ParticleSystem>().Where(p =>
                    p.main.simulationSpace == ParticleSystemSimulationSpace.Custom &&
                    p.main.customSimulationSpace == null
                ).ToArray();

            if (particleSystems.Length == 0) { FO_Debugger.LogSuccess("All particle systems validated."); return true; }

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
            if (input == 1) return false;

            ChangeSimulationSpace(particleSystems, input == 0 ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local);

            return true;
        }

        private static void ChangeSimulationSpace(ParticleSystem[] systems, ParticleSystemSimulationSpace space)
        {
            foreach (ParticleSystem particle in systems)
            {
                var particleSystemMain = particle.main;
                particleSystemMain.simulationSpace = space;
                FO_Debugger.Log($"Updated simulation space.", particle);
            }

            FO_Debugger.LogSuccess($"Updated simulation space on {systems.Length} ParticleSystems to {space}.");
        }

        #endregion // INTERNAL
    }
}