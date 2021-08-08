using System.Text.RegularExpressions;

namespace AL.Client.Helpers
{
    public static class RegexCache
    {
        public static readonly Regex SKILL_TIMEOUT =
            new(@"skill_timeout\s*\(['""](.+?)['""]\s*(?:,\s*(\d*(?:\.\d+)?))?\s*\)", RegexOptions.Compiled);
    }
}