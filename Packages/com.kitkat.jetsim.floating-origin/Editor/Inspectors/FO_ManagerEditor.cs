
using UnityEngine;
using UnityEditor;

using KitKat.JetSim.FloatingOrigin.Runtime;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    [CustomEditor(typeof(FO_Manager))]
    public class FO_ManagerEditor : UnityEditor.Editor
    {
        [InitializeOnLoadMethod]
        private static void Hook()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            Shader.SetGlobalVector("_Udon_FO_WorldOffset", Vector3.zero);
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            Shader.SetGlobalVector("_Udon_FO_WorldOffset", Vector3.zero);
        }

        public override void OnInspectorGUI()
        {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            style.fontSize = 20;

            EditorGUILayout.TextArea("KitKat's <color=purple>Floating Origin</color>", style);

            base.OnInspectorGUI();
        }
    }
}