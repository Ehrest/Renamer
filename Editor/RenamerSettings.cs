using UnityEditor;
using UnityEngine;

namespace Ehrest.Editor.Renamer
{
    public class RenamerSettings : ScriptableObject
    {
        [SerializeField] IncrementBehavior _incrementBehavior;

        public static IncrementBehavior IncrementBehavior { get; private set; }

        public static void Refresh(RenamerSettings instance)
        {
            IncrementBehavior = instance._incrementBehavior;
        }

        [InitializeOnLoadMethod]
        public static void RefreshOnLoad()
        {
            Refresh(RenamerSettingsRegister.Load());
        }
    }
}