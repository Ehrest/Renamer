using UnityEngine;

namespace Ehrest.Editor.Renamer
{
    public class RenamerSettings : ScriptableObject
    {
        [SerializeField]
        IncrementBehavior _incrementBehavior;

        public static IncrementBehavior IncrementBehavior { get; private set; }

        private void OnValidate()
        {
            RenamerSettings.Refresh(this);
        }

        public static void Refresh(RenamerSettings instance)
        {
            IncrementBehavior = instance._incrementBehavior;
        }
    }
}