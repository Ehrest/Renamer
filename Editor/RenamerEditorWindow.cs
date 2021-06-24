using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ehrest.Editor.Renamer
{
    public class RenamerEditorWindow : EditorWindow, IHasCustomMenu
    {
        public enum RenamerTarget
        {
            Selection,
            SelectionAndDirectChildren,
            SelectionRecursif,
            Parent,
            ParentRecursif
        }

        string _replaceMatch;
        string _replaceEntry;
        string _incrementName;
        string _addedString;

        RenamerTarget _renamerTarget;
        Transform _renameParent;
        Vector2 _scrollPosition = Vector2.zero;

        static RenamerEditorWindow _window;


        [MenuItem("Tools/Renamer")]
        private static void ShowWindow()
        {
            _window = GetWindow<RenamerEditorWindow>();
            _window.titleContent = new GUIContent("Renamer");
            _window.Show();
        }

        public static void ForceRepaint()
        {
            if (_window != null)
                _window.Repaint();
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            GUIContent content = new GUIContent(nameof(OpenSettings));
            menu.AddItem(content, false, OpenSettings);
        }

        private void OpenSettings()
        {
            SettingsService.OpenProjectSettings("Project/Ehrest.Renamer");
        }

        private void OnGUI()
        {
            if (_window == null)
                _window = GetWindow<RenamerEditorWindow>();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(_window.position.width), GUILayout.Height(_window.position.height));

            EditorGUILayout.Space();

            GUIStyle italicLabelStyle = new GUIStyle(GUI.skin.label);
            italicLabelStyle.fontStyle = FontStyle.BoldAndItalic;

            _renamerTarget = (RenamerTarget)EditorGUILayout.EnumPopup("Rename Target", _renamerTarget);

            if (_renamerTarget == RenamerTarget.Parent || _renamerTarget == RenamerTarget.ParentRecursif)
            {
                _renameParent = (Transform)EditorGUILayout.ObjectField("Label:", _renameParent, typeof(Transform), true);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawLine();
            EditorGUILayout.Space();
            GUILayout.Label("Replace part of name", EditorStyles.boldLabel);
            _replaceMatch = EditorGUILayout.TextField(_replaceMatch);
            GUILayout.Label(" will become ");
            _replaceEntry = EditorGUILayout.TextField(_replaceEntry);

            GUILayout.Label($"{ComputeReplaceEntry()} entrie(s) found", italicLabelStyle);

            if (GUILayout.Button("Run replace"))
            {
                ApplyReplaceEntry();
            }

            if (!string.IsNullOrEmpty(_replaceMatch))
                GUILayout.Label($"Replace : XXX{_replaceMatch}XXX -> XXX{_replaceEntry}XXX");
            else
                GUILayout.Label($"Replace : XXXXXX -> {_replaceEntry}");

            EditorGUILayout.Space();
            DrawLine();
            EditorGUILayout.Space();
            GUILayout.Label("Increment", EditorStyles.boldLabel);
            _incrementName = EditorGUILayout.TextField("Name ", _incrementName);

            GUILayout.Label($"{GetObjectCount()} entrie(s) will be modified", italicLabelStyle);

            if (GUILayout.Button("Run increment"))
            {
                ApplyIncrement();
            }
            GUILayout.Label($"Increment : {GetIncrementExample()}");

            EditorGUILayout.Space();
            DrawLine();
            EditorGUILayout.Space();
            GUILayout.Label("Adding", EditorStyles.boldLabel);
            _addedString = EditorGUILayout.TextField("Addition ", _addedString);
            GUILayout.Label($"{GetObjectCount()} entrie(s) will be modified", italicLabelStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Run as prefix"))
            {
                ApplyAdding(isPrefix: true);
            }
            if (GUILayout.Button("Run as suffix"))
            {
                ApplyAdding(isPrefix: false);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Prefix : {_addedString}XXXXX");
            GUILayout.Label($"Suffix : XXXXX{_addedString}");
            EditorGUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        private void ApplyAdding(bool isPrefix)
        {
            GameObject[] entries = GetTargetObjects();
            Undo.RecordObjects(entries, "Renamer Adding");

            if (!string.IsNullOrEmpty(_addedString))
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (isPrefix)
                        entries[i].transform.name = _addedString + entries[i].transform.name;
                    else
                        entries[i].transform.name = entries[i].transform.name + _addedString;
                }
            }
        }

        private void ApplyReplaceEntry()
        {
            GameObject[] entries = GetTargetObjects();
            Undo.RecordObjects(entries, "Renamer Replace");

            if (!string.IsNullOrEmpty(_replaceMatch))
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].transform.name.Contains(_replaceMatch))
                        entries[i].transform.name = entries[i].transform.name.Replace(_replaceMatch, _replaceEntry);
                }
            }
            else
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    entries[i].transform.name = _replaceEntry;
                }
            }
        }

        private void ApplyIncrement()
        {
            IncrementBehavior incrementBehavior = RenamerSettings.IncrementBehavior;
            GameObject[] entries = GetSortedTargetObjects();
            Undo.RecordObjects(entries, "Renamer Increment");

            string baseName = _incrementName;
            bool useEntryName = string.IsNullOrEmpty(_incrementName);
            for (int i = 0; i < entries.Length; i++)
            {
                if(useEntryName)
                    baseName = entries[i].transform.name;

                entries[i].transform.name = incrementBehavior.Apply(baseName, 0);
            }
        }

        private string GetIncrementExample()
        {
            IncrementBehavior incrementBehavior = RenamerSettings.IncrementBehavior;
            string baseName = string.IsNullOrEmpty(_incrementName) ? "XXX" : _incrementName;
            return incrementBehavior.Apply(baseName, 0);
        }

        private int ComputeReplaceEntry()
        {
            GameObject[] entries = GetTargetObjects();
            int matchCount = 0;

            if (!string.IsNullOrEmpty(_replaceMatch))
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].transform.name.Contains(_replaceMatch))
                        matchCount++;
                }
            }
            else
            {
                matchCount = entries.Length;
            }

            return matchCount;
        }

        private int GetObjectCount()
        {
            return GetTargetObjects().Length;
        }

        private GameObject[] GetSortedTargetObjects()
        {
            GameObject[] targets = GetTargetObjects();
            System.Array.Sort(targets, new UnityTransformSort());

            return targets;
        }

        private GameObject[] GetTargetObjects()
        {
            if (_renamerTarget == RenamerTarget.Selection)
            {
                return Selection.gameObjects;
            }
            else if (_renamerTarget == RenamerTarget.SelectionAndDirectChildren)
            {
                return GetSelectionAndChilds().ToArray();
            }
            else if (_renamerTarget == RenamerTarget.SelectionRecursif)
            {
                return GetSelectionAndChilds(true).ToArray();
            }
            else if (_renameParent != null)
            {
                if (_renamerTarget == RenamerTarget.Parent)
                {
                    return GetChilds(_renameParent, false).ToArray();
                }
                else if (_renamerTarget == RenamerTarget.ParentRecursif)
                {
                    return GetChilds(_renameParent, true).ToArray();
                }
            }

            return new GameObject[0];
        }

        private void DrawLine(int i_height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);

            rect.height = i_height;

            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        private List<GameObject> GetSelectionAndChilds(bool recursive = false)
        {
            List<GameObject> gos = new List<GameObject>();
            GameObject[] selectObjs = Selection.gameObjects;
            gos.AddRange(selectObjs);

            for (int i = 0; i < selectObjs.Length; i++)
            {
                gos.AddRange(GetChilds(selectObjs[i].transform, recursive));
            }

            return gos;
        }

        private List<GameObject> GetChilds(Transform parent, bool recursive = false)
        {
            List<GameObject> gos = new List<GameObject>();

            for (int i = 0; i < parent.childCount; i++)
            {
                gos.Add(parent.GetChild(i).gameObject);
            }

            if (recursive)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    gos.AddRange(GetChilds(parent.GetChild(i), true));
                }
            }

            return gos;
        }

        public class UnityTransformSort : System.Collections.Generic.IComparer<GameObject>
        {
            public int Compare(GameObject lhs, GameObject rhs)
            {
                if (lhs == rhs) return 0;
                if (lhs == null) return 1;
                if (rhs == null) return -1;
                if (lhs.transform.GetSiblingIndex() == rhs.transform.GetSiblingIndex()) return 0;
                return (lhs.transform.GetSiblingIndex() > rhs.transform.GetSiblingIndex()) ? 1 : -1;
            }
        }
    }
}