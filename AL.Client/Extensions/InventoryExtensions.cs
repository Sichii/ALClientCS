using System.Collections.Generic;
using System.Linq;
using AL.Client.Model;
using AL.SocketClient.Interfaces;
using Chaos.Core.Extensions;

// ReSharper disable ParameterTypeCanBeEnumerable.Global

namespace AL.Client.Extensions
{
    public static class InventoryExtensions
    {
        public static IEnumerable<IndexedItem> AsIndexed(this IReadOnlyList<IItem> items) =>
            items.Select((item, index) => new IndexedItem
            {
                Index = index,
                Item = item
            });

        public static bool ContainsItem(this IReadOnlyList<IItem> items, string itemName) =>
            items.Any(item => item.Name.EqualsI(itemName));

        public static int CountOf(this IReadOnlyList<IItem> items, string itemName) =>
            items.Where(item => item.Name.EqualsI(itemName)).Sum(item => item.Quantity);

        public static IndexedItem FindItem(
            this IReadOnlyList<IItem> items,
            string itemName,
            int levelMin = int.MinValue,
            int levelMax = int.MaxValue,
            int quantityMin = int.MinValue,
            int quantityMax = int.MaxValue)
        {
            var index = items.FindIndex(item =>
                (item.Level >= levelMin)
                && (item.Level <= levelMax)
                && (item.Quantity >= quantityMin)
                && (item.Quantity <= quantityMax)
                && item.Name.EqualsI(itemName));

            if (index == -1)
                return null;

            return new IndexedItem
            {
                Index = index,
                Item = items[index]
            };
        }
    }
}