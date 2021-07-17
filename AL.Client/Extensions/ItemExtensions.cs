using AL.Client.Definitions;
using AL.Data;
using AL.Data.Items;
using AL.SocketClient.Interfaces;

namespace AL.Client.Extensions
{
    public static class ItemExtensions
    {
        public static Item GetData(this IInventoryItem item) => GameData.Items[item.Name];

        public static Grade GetGrade(this IInventoryItem item)
        {
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

        public static bool IsCompoundable(this IInventoryItem item) => item.GetData().CompoundModifiers != null;
        public static bool IsStackable(this IInventoryItem item) => item.GetData().StackSize > 1;

        public static bool IsUpgradeable(this IInventoryItem item) => item.GetData().UpgradeModifiers != null;
    }
}