using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ehrest.Editor.Renamer
{
    public static class RenamerSettingsRegister
    {
        const string SettingsPath = "Packages/com.ehrest.renamer/Editor/Data/Renamer.Settings.asset";

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Project/Ehrest.Renamer", SettingsScope.Project)
            {
                guiHandler = (searchContext) =>
                {
                    var settings = Load();
                    var serialized = new SerializedObject(settings);

                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.PropertyField(serialized.FindProperty("_incrementBehavior"));

                    if (EditorGUI.EndChangeCheck())
                    {
                        serialized.ApplyModifiedProperties();
                        RenamerSettings.Refresh(settings);
                    }
                },
            };
        }

        public static RenamerSettings Load()
        {
            var settings = AssetDatabase.LoadAssetAtPath<RenamerSettings>(SettingsPath);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<RenamerSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }
    }
}