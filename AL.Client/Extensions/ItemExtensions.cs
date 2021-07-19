using System;
using AL.APIClient.Interfaces;
using AL.Client.Definitions;
using AL.Data;
using AL.Data.Items;

namespace AL.Client.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="ISimpleItem" />s and <see cref="ICommonItem" />s.
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        ///     Gets the "G" data for this item.
        /// </summary>
        /// <param name="item">The item to get the data for.</param>
        /// <returns>
        ///     <see cref="GItem" /> <br />
        ///     The "G" data for this item from <see cref="GameData" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">item</exception>
        public static GItem? GetData(this ISimpleItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return GameData.Items[item.Name];
        }

        /// <summary>
        ///     Calculates the grade of the item.
        /// </summary>
        /// <param name="item">The item to calculate the grade for.</param>
        /// <returns>
        ///     <see cref="Grade" /> <br />
        ///     The grade of the item.
        /// </returns>
        /// <exception cref="ArgumentNullException">item</exception>
        public static Grade GetGrade(this ICommonItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var data = item.GetData();

            if ((data == null) || (data.Grades == null))
                return Grade.None;

            var grade = 0;

            foreach (var level in data.Grades)
                if (item.Level < level)
                    break;
                else
                    grade++;

            return (Grade) grade;
        }

        /// <summary>
        ///     Checks if the item is compoundable.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if the item is compoundable, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">item</exception>
        public static bool IsCompoundable(this ISimpleItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return item.GetData()?.CompoundModifiers != null;
        }

        /// <summary>
        ///     Checks if the item is stackable.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if the item is stackable, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">item</exception>
        public static bool IsStackable(this ISimpleItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return item.GetData()?.StackSize > 1;
        }

        /// <summary>
        ///     Checks if the item is upgradeable.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if the item is upgradeable, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">item</exception>
        public static bool IsUpgradeable(this ISimpleItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return item.GetData()?.UpgradeModifiers != null;
        }
    }
}