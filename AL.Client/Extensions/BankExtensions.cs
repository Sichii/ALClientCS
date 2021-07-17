using System.Collections.Generic;
using System.Linq;
using AL.Client.Model;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Client.Extensions
{
    public static class BankExtensions
    {
        public static IEnumerable<BankedItem> AsIndexed(
            this IReadOnlyDictionary<BankPack, IReadOnlyList<IInventoryItem>> dic) =>
            dic.SelectMany(kvp => kvp.Value.Select((item, index) => new BankedItem
            {
                BankPack = kvp.Key,
                Index = index,
                Item = item
            }));

        public static bool ContainsItem(
            this IReadOnlyDictionary<BankPack, IReadOnlyList<IInventoryItem>> dic,
            string itemName) =>
            dic.Values.Any(items => items.Any(item => item.Name.EqualsI(itemName)));

        public static int CountOf(
            this IReadOnlyDictionary<BankPack, IReadOnlyList<IInventoryItem>> dic,
            string itemName) => dic.Values.SelectMany(items => items)
            .Where(item => item.Name.EqualsI(itemName))
            .Sum(item => item.Quantity);

        public static BankedItem FindItem(
            this IReadOnlyDictionary<BankPack, IReadOnlyList<IInventoryItem>> dic,
            string itemName,
            int levelMin = int.MinValue,
            int levelMax = int.MaxValue,
            int quantityMin = int.MinValue,
            int quantityMax = int.MaxValue)
        {
            foreach ((var bankPack, var items) in dic)
            {
                var index = items.FindIndex(item =>
                    (item.Level >= levelMin)
                    && (item.Level <= levelMax)
                    && (item.Quantity >= quantityMin)
                    && (item.Quantity <= quantityMax)
                    && item.Name.EqualsI(itemName));

                if (index == -1)
                    continue;

                return new BankedItem
                {
                    BankPack = bankPack,
                    Index = index,
                    Item = items[index]
                };
            }

            return null;
        }
    }
}