using AL.Client.Definitions;
using AL.Data;
using AL.Data.Items;
using AL.SocketClient.Interfaces;

namespace AL.Client.Extensions
{
    public static class ItemExtensions
    {
        public static Item GetData(this IItem item) => GameData.Items[item.Name];

        public static Grade GetGrade(this IItem item)
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

        public static bool IsCompoundable(this IItem item) => item.GetData().CompoundModifiers != null;
        public static bool IsStackable(this IItem item) => item.GetData().StackSize > 1;

        public static bool IsUpgradeable(this IItem item) => item.GetData().UpgradeModifiers != null;
    }
}