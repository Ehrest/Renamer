using UnityEngine;
using UnityEditor;

namespace Ehrest.Editor.Renamer
{
    [CustomEditor(typeof(IncrementBehavior))]
    [CanEditMultipleObjects]
    public class IncrementBehaviorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            IncrementBehavior myTarget = (IncrementBehavior)target;

            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();


            for (int i = 0; i < 15; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(myTarget.Apply("Test", i));
                }
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}