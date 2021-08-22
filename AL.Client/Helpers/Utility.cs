using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Definitions;

namespace AL.Client.Helpers
{
    internal static class Utility
    {
        internal static IEnumerable<BankPack> GetAvailableBankPacks(string mapAccessor)
        {
            if (string.IsNullOrEmpty(mapAccessor))
                throw new ArgumentNullException(nameof(mapAccessor));

            return (mapAccessor switch
            {
                "bank"   => Enumerable.Range(1, 7),
                "bank_b" => Enumerable.Range(9, 15),
                "bank_u" => Enumerable.Range(25, 23),
                _        => throw new InvalidOperationException("Not in a bank")
            }).Select(index => (BankPack)index);
        }
    }
}