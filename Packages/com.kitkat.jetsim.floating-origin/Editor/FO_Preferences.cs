
using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections.Generic;

namespace KitKat.JetSim.FloatingOrigin.Editor
{
    public class FO_Preferences : ScriptableObject
    {
        public const string PreferencesPath = "Assets/KitKat/JetSim/FloatingOrigin/FloatingOriginPreferences.asset";

        public bool ShowObjectSyncWarning = true;
        public bool ShowObjectSyncPopup = true;

        internal static FO_Preferences GetOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<FO_Preferences>(PreferencesPath);
            if (!settings)
            {
                if (!AssetDatabase.IsValidFolder(Path.GetDirectoryName(PreferencesPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(PreferencesPath));

                settings = CreateInstance<FO_Preferences>();
                AssetDatabase.CreateAsset(settings, PreferencesPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerialized()
        {
            return new SerializedObject(GetOrCreate());
        }
    }

    static class FO_EditorPreferencesProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider("Project/KitKat/Floating Origin Preferences", SettingsScope.Project)
            {
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var settings = FO_Preferences.GetSerialized();
                    EditorGUILayout.PropertyField(settings.FindProperty("ShowObjectSyncWarning"));
                    EditorGUILayout.PropertyField(settings.FindProperty("ShowObjectSyncPopup"));
                    // TODO setting for debug mode
                    settings.ApplyModifiedPropertiesWithoutUndo();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "FloatingOrigin", "Floating", "Origin", "ObjectSync" })
            };

            return provider;
        }
    }
}