using UnityEngine;

namespace Ehrest.Editor.Renamer
{
    [CreateAssetMenu(fileName = "IncrementBehavior", menuName = "Tools/Renamer/IncrementBehavior", order = 0)]
    public class IncrementBehavior : ScriptableObject
    {
        public string NumberBefore = "_";
        public string NumberFormat = "000";
        public string NumberAfter = "";

        public string Apply(string target, int number)
        {
            return target + NumberBefore + number.ToString(NumberFormat) + NumberAfter;
        }
    }
}