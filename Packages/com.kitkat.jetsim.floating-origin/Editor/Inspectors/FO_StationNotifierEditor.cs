
using UnityEditor;
using UnityEngine;

using UdonSharp;
using UdonSharpEditor;

using VRC.Udon;
using VRC.SDKBase;

using System.Linq;

using KitKat.JetSim.FloatingOrigin.Runtime;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    [CustomEditor(typeof(FO_StationNotifier))]
    public class FO_StationNotifierEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawInfobox();
            MatchSyncMode(target);
        }

        private void DrawInfobox()
        {
            // Rich text in help box: https://stackoverflow.com/questions/24961469/how-to-achieve-editorgui-helpbox-look-with-guistyle
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            style.fontSize = 17;

            EditorGUILayout.TextArea("This script was automatically added by KitKat's <color=#CC8080>Floating Origin</color>.", style);
        }

        private void MatchSyncMode(Object target)
        {
            UdonBehaviour backingNotifier = UdonSharpEditorUtility.GetBackingUdonBehaviour((UdonSharpBehaviour)target);

            Networking.SyncType syncType = Networking.SyncType.None;

            var behaviours = backingNotifier.GetComponents<UdonBehaviour>().Where(x => x != backingNotifier).ToArray();
            
            if (behaviours.Length > 0)
                if (behaviours[0])
                    syncType = behaviours[0].SyncMethod;

            backingNotifier.SyncMethod = syncType;
        }
    }
}