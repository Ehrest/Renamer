using UnityEngine;

namespace Ehrest.Editor.Renamer
{
    [CreateAssetMenu(fileName = "IncrementBehavior", menuName = "Tools/Renamer/IncrementBehavior", order = 0)]
    public class IncrementBehavior : ScriptableObject, IIncrementBehavior
    {
        public bool UsePrefix => usePrefix;
        [SerializeField] private bool usePrefix;
        public string PrefixStart => prefixStart;
        [SerializeField] private string prefixStart;
        public string PrefixEnd => prefixEnd;
        [SerializeField] private string prefixEnd;

        public string NumberFormat => numberFormat;
        [SerializeField] private string numberFormat = "000";
        public int NumberOffset => numberOffset;
        [SerializeField] private int numberOffset = 0;

        public bool UseSuffix => useSuffix;
        [SerializeField] private bool useSuffix;
        public string SuffixStart => suffixStart;
        [SerializeField] private string suffixStart;
        public string SuffixEnd => suffixEnd;
        [SerializeField] private string suffixEnd;

        public string Apply(string target, int number)
        {
            string prefix = "";
            string suffix = "";

            if(UsePrefix)
                prefix = PrefixStart + (number + NumberOffset).ToString(NumberFormat) + PrefixEnd;

            if(UseSuffix)
                suffix = SuffixStart + (number + NumberOffset).ToString(NumberFormat) + SuffixEnd;

            return prefix + target + suffix;
        }

        public DynamicIncrementBehavior ConvertToDynamicBehavior()
        {
            return new DynamicIncrementBehavior()
            {
                usePrefix = this.usePrefix,
                prefixStart = this.prefixStart,
                prefixEnd = this.prefixEnd,

                numberFormat = this.numberFormat,
                numberOffset = this.numberOffset,

                useSuffix = this.useSuffix,
                suffixStart = this.suffixStart,
                suffixEnd = this.suffixEnd,
            };
        }

        public void CopyFromDynamicBehavior(DynamicIncrementBehavior reference)
        {
            usePrefix = reference.UsePrefix;
            prefixStart = reference.PrefixStart;
            prefixEnd = reference.PrefixEnd;

            numberFormat = reference.NumberFormat;
            numberOffset = reference.NumberOffset;

            useSuffix = reference.UseSuffix;
            suffixStart = reference.SuffixStart;
            suffixEnd = reference.SuffixEnd;
        }
    }
}