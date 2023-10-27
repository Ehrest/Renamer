using UnityEngine;

namespace Ehrest.Editor.Renamer
{
    public interface IIncrementBehavior
    {
        bool UsePrefix { get; }
        string PrefixStart { get; }
        string PrefixEnd { get; }

        int NumberOffset { get; }
        string NumberFormat { get; }

        bool UseSuffix { get; }
        string SuffixStart { get; }
        string SuffixEnd { get; }

        string Apply(string target, int number);
    }
}