using System.Text.RegularExpressions;

namespace AL.Client.Helpers
{
    internal static class RegexCache
    {
        internal static readonly Regex POT_TIMEOUT = new(@"pot_timeout\s*\((\d*\.?\d*)\)", RegexOptions.Compiled);
        internal static readonly Regex SKILL_TIMEOUT =
            new(@"skill_timeout\s*\(['""](.+?)['""]\s*(?:,\s*(\d*(?:\.\d+)?))?\s*\)", RegexOptions.Compiled);
    }
}