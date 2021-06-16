using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Geometry;
using Common.Logging;
using LogManager = Common.Logging.LogManager;

namespace AL.Core.Helpers
{
    public static class LineHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LineHelper).FullName); 

        public static FlatLine[] FixLines(FlatLine[] lines, bool isX)
        {
            if (lines == null || lines.Length == 0)
                return Array.Empty<FlatLine>();

            var newLines = new HashSet<FlatLine>();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                for (var i2 = 0; i2 < lines.Length; i2++)
                    if (i != i2 && line.Overlaps(lines[i2]))
                    {
                        Logger.Trace($@"Merging lines
{line}
{lines[i2]}
");

                        line = line.Merge(lines[i2]);
                        break;
                    }

                newLines.Add(line with { IsX = isX });
            }

            return newLines.ToArray();
        }
    }
}