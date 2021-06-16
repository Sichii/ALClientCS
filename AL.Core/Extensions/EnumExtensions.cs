using AL.Core.Definitions;

namespace AL.Core.Extensions
{
    public static class EnumExtensions
    {
        public static EquipmentSlot ToEquipmentSlot(this Slot slot) => (EquipmentSlot) (int) slot;
        public static Slot ToSlot(this EquipmentSlot equipmentSlot) => (Slot) (int) equipmentSlot;
        public static Slot ToSlot(this TradeSlot tradeSlot) => (Slot) (int) tradeSlot;
        public static TradeSlot ToTradeSlot(this Slot slot) => (TradeSlot) (int) slot;
    }
}