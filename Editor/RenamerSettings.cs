using UnityEditor;
using UnityEngine;

namespace Ehrest.Editor.Renamer
{
    public class RenamerSettings : ScriptableObject
    {
        public const string Repository = "Ehrest/Renamer";
        public const string PackagePath = "Packages/com.ehrest.renamer/";
        public const string AssetPath = "Assets/Plugins/Renamer/";
        public const string DataPath = "Editor/Data/";
        public const string SettingsFile = "Renamer.Settings.asset";
        public const string PackageFullPathData = PackagePath + DataPath;
        public const string PackageFullPathSettings = PackageFullPathData + SettingsFile;
        public const string AssetFullPathData = AssetPath + DataPath;
        public const string AssetFullPathSettings = AssetFullPathData + SettingsFile;

        private const string baseIncrementFile = "IncrementBehavior.Format_000.asset";


        [SerializeField] IncrementBehavior _incrementBehavior;

        private static IncrementBehavior _incrementBehaviorAccess;
        public static IncrementBehavior IncrementBehavior
        {
            get
            {
                if(_incrementBehaviorAccess == null)
                {
                    _incrementBehaviorAccess = AssetDatabase.LoadAssetAtPath<IncrementBehavior>(PackageFullPathData + baseIncrementFile);

                    if(_incrementBehaviorAccess == null)
                    {
                        _incrementBehaviorAccess = AssetDatabase.LoadAssetAtPath<IncrementBehavior>(AssetFullPathData + baseIncrementFile);
                    }

                    Debug.LogWarning("Incremental Behaviour is null, fetching the default scheme XXX_000. Please consider configuring the behavior in the Projects Settings");
                }

                return _incrementBehaviorAccess;
            }
            private set
            {
                _incrementBehaviorAccess = value;
            }
        }

        public static void Refresh(RenamerSettings instance)
        {
            _incrementBehaviorAccess = instance._incrementBehavior;
        }

        [InitializeOnLoadMethod]
        public static void RefreshOnLoad()
        {
            Refresh(RenamerSettingsRegister.Load());
        }
    }
}