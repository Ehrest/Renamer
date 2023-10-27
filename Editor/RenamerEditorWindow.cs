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

        IncrementBehavior _incrementBehavior;

        RenamerTarget _renamerTarget;
        Transform _renameParent;
        Vector2 _scrollPosition = Vector2.zero;

        GUIStyle _italicLabelStyle;
        GUIStyle _titleLabelStyle;

        static RenamerEditorWindow _window;
        static VersioningTool _versioningTool;

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

            content = new GUIContent("Refresh Versioning");
            menu.AddItem(content, false, RefreshVersioning);
        }

        private void OpenSettings()
        {
            SettingsService.OpenProjectSettings("Project/Ehrest.Renamer");
        }

        private static void RefreshVersioning()
        {
            _versioningTool.Refresh();
        }

        private void OnGUI()
        {
            if (_window == null)
                _window = GetWindow<RenamerEditorWindow>();

            if (_versioningTool == null)
                _versioningTool = new VersioningTool("com.ehrest.renamer");


            // Define styles
            _italicLabelStyle = new GUIStyle(GUI.skin.label);
            _italicLabelStyle.fontStyle = FontStyle.BoldAndItalic;

            _titleLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            _titleLabelStyle.fontSize = EditorStyles.largeLabel.fontSize;
            _titleLabelStyle.alignment = TextAnchor.MiddleCenter;
            _titleLabelStyle.margin = new RectOffset(0, 0, 0, 10);

            // Draw versioning widget
            _versioningTool.DrawWidget();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(_window.position.width), GUILayout.Height(_window.position.height));

            EditorGUILayout.Space();

            DrawTargetType();

            Separator();

            DrawReplace();

            Separator();
            
            DrawIncrement();

            Separator();
            
            DrawAdding();

            Separator();

            DrawTrim();

            GUILayout.EndScrollView();
        }

        private void Separator()
        {
            EditorGUILayout.Space();
            DrawLine();
            EditorGUILayout.Space();
        }

        private void DrawTargetType()
        {
            _renamerTarget = (RenamerTarget)EditorGUILayout.EnumPopup("Rename Target", _renamerTarget);

            if (_renamerTarget == RenamerTarget.Parent || _renamerTarget == RenamerTarget.ParentRecursif)
            {
                _renameParent = (Transform)EditorGUILayout.ObjectField("Label:", _renameParent, typeof(Transform), true);
            }
        }

        private void DrawReplace()
        {
            GUILayout.Label("Replace", _titleLabelStyle);
            _replaceMatch = EditorGUILayout.TextField(_replaceMatch);
            GUILayout.Label(" will become ");
            _replaceEntry = EditorGUILayout.TextField(_replaceEntry);

            GUILayout.Label($"{ComputeReplaceEntry()} entrie(s) found", _italicLabelStyle);

            if (GUILayout.Button("Run replace"))
            {
                ApplyReplaceEntry();
            }

            if (!string.IsNullOrEmpty(_replaceMatch))
                GUILayout.Label($"Replace : XXX{_replaceMatch}XXX -> XXX{_replaceEntry}XXX");
            else
                GUILayout.Label($"Replace : XXXXXX -> {_replaceEntry}");
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

        private void DrawIncrement()
        {
            GUILayout.Label("Increment", _titleLabelStyle);
            _incrementName = EditorGUILayout.TextField("Name ", _incrementName);
            
            if(_incrementBehavior == null)
                _incrementBehavior = RenamerSettings.IncrementBehavior;
            _incrementBehavior = (IncrementBehavior)EditorGUILayout.ObjectField("Increment Behavior ", _incrementBehavior, typeof(IncrementBehavior), false);

            GUILayout.Label($"{GetObjectCount()} entrie(s) will be modified", _italicLabelStyle);

            if (GUILayout.Button("Run increment"))
            {
                ApplyIncrement();
            }
            GUILayout.Label($"Increment : {GetIncrementExample()}");
        }

        private void ApplyIncrement()
        {
            GameObject[] entries = GetSortedTargetObjects();
            Undo.RecordObjects(entries, "Renamer Increment");

            string baseName = _incrementName;
            bool useEntryName = string.IsNullOrEmpty(_incrementName);
            for (int i = 0; i < entries.Length; i++)
            {
                if(useEntryName)
                    baseName = entries[i].transform.name;

                entries[i].transform.name = _incrementBehavior.Apply(baseName, i);
            }
        }

        private void DrawAdding()
        {
            GUILayout.Label("Add", _titleLabelStyle);
            _addedString = EditorGUILayout.TextField("Addition ", _addedString);
            GUILayout.Label($"{GetObjectCount()} entrie(s) will be modified", _italicLabelStyle);

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

        private void DrawTrim()
        {
            GUILayout.Label("Trim", _titleLabelStyle);
            GUILayout.Label($"{GetObjectCount()} entrie(s) will be modified", _italicLabelStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("4"))
            {
                ApplyTrim(isPrefix: true, value: 4);
            }
            if (GUILayout.Button("3"))
            {
                ApplyTrim(isPrefix: true, value: 3);
            }
            if (GUILayout.Button("2"))
            {
                ApplyTrim(isPrefix: true, value: 2);
            }
            if (GUILayout.Button("1"))
            {
                ApplyTrim(isPrefix: true, value: 1);
            }

            GUILayout.Space(20f);

            if (GUILayout.Button("1"))
            {
                ApplyTrim(isPrefix: false, value: 1);
            }
            if (GUILayout.Button("2"))
            {
                ApplyTrim(isPrefix: false, value: 2);
            }
            if (GUILayout.Button("3"))
            {
                ApplyTrim(isPrefix: false, value: 3);
            }
            if (GUILayout.Button("4"))
            {
                ApplyTrim(isPrefix: false, value: 4);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ApplyTrim(bool isPrefix, int value)
        {
            GameObject[] entries = GetTargetObjects();
            Undo.RecordObjects(entries, "Renamer Trim");

            if (!string.IsNullOrEmpty(_addedString))
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if(entries[i].transform.name.Length <= value)
                        continue;

                    if (isPrefix)
                        entries[i].transform.name = entries[i].transform.name.Substring(value);
                    else
                        entries[i].transform.name = entries[i].transform.name.Substring(0, entries[i].transform.name.Length - value);
                }
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