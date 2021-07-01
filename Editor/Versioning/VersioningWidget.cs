using System;
using System.Collections;
using System.IO;
using System.Net;

using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.Requests;


namespace Ehrest.Editor.Renamer
{
    public class VersioningWidget
    {
        private int _selectedVersion;
        
        public void Draw(VersioningTool versioning)
        {
            if (!versioning.IsReady)
            {
                GUILayout.Label("Loading versioning...", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label($"Current version {versioning.CurrentVersion}", EditorStyles.boldLabel);
                _selectedVersion = EditorGUILayout.Popup(_selectedVersion, versioning.Versions);

                GUI.enabled = !versioning.Versions[_selectedVersion].Equals(versioning.CurrentVersion);
                if (GUILayout.Button($"Update to {versioning.Versions[_selectedVersion]}"))
                {
                    versioning.ChangeVersionTo(versioning.Versions[_selectedVersion]);
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}