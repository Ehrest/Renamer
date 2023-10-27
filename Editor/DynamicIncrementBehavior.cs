using UnityEngine;

namespace Ehrest.Editor.Renamer
{
    public class DynamicIncrementBehavior : IIncrementBehavior
    {
        public bool UsePrefix => usePrefix;
        public bool usePrefix;

        public string PrefixStart => prefixStart;
        public string prefixStart;
        public string PrefixEnd => prefixEnd;
        public string prefixEnd;

        public string NumberFormat => numberFormat;
        public string numberFormat = "000";
        public int NumberOffset => numberOffset;
        public int numberOffset = 0;

        public bool UseSuffix => useSuffix;
        public bool useSuffix;
        public string SuffixStart => suffixStart;
        public string suffixStart;
        public string SuffixEnd => suffixEnd;
        public string suffixEnd;

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
    }
}