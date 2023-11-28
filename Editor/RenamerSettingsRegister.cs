using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ehrest.Editor.Renamer
{
    public static class RenamerSettingsRegister
    {
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
                        RenamerEditorWindow.ForceRepaint();
                    }
                },
            };
        }

        public static RenamerSettings Load()
        {
            string filePath;
            RenamerSettings settings = null;

            if(AssetDatabase.IsValidFolder(RenamerSettings.PackagePath))
            {
                filePath = RenamerSettings.PackageFullPathData;
                settings = AssetDatabase.LoadAssetAtPath<RenamerSettings>(RenamerSettings.PackageFullPathSettings);
            }
            else
            {
                filePath = RenamerSettings.AssetFullPathData;
                settings = AssetDatabase.LoadAssetAtPath<RenamerSettings>(RenamerSettings.AssetFullPathSettings);
            }

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<RenamerSettings>();
                AssetDatabase.CreateAsset(settings, filePath + RenamerSettings.SettingsFile);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }
    }
}