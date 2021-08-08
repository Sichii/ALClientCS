using AL.Core.Definitions;

namespace AL.Core.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="System.Enum" />s
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Converts a <see cref="Slot" /> to an <see cref="EquipmentSlot" />.
        /// </summary>
        /// <param name="slot">The <see cref="Slot" /> to convert.</param>
        /// <returns>
        ///     <see cref="EquipmentSlot" />
        /// </returns>
        public static EquipmentSlot ToEquipmentSlot(this Slot slot) => (EquipmentSlot)(int)slot;

        /// <summary>
        ///     Converts an <see cref="EquipmentSlot" /> to a <see cref="Slot" />.
        /// </summary>
        /// <param name="equipmentSlot">The <see cref="EquipmentSlot" /> to convert.</param>
        /// <returns>
        ///     <see cref="Slot" />
        /// </returns>
        public static Slot ToSlot(this EquipmentSlot equipmentSlot) => (Slot)(int)equipmentSlot;

        /// <summary>
        ///     Converts an <see cref="TradeSlot" /> to a <see cref="Slot" />.
        /// </summary>
        /// <param name="tradeSlot">The <see cref="TradeSlot" /> to convert.</param>
        /// <returns>
        ///     <see cref="Slot" />
        /// </returns>
        public static Slot ToSlot(this TradeSlot tradeSlot) => (Slot)(int)tradeSlot;

        /// <summary>
        ///     Converts a <see cref="Slot" /> to an <see cref="TradeSlot" />.
        /// </summary>
        /// <param name="slot">The <see cref="Slot" /> to convert.</param>
        /// <returns>
        ///     <see cref="TradeSlot" />
        /// </returns>
        public static TradeSlot ToTradeSlot(this Slot slot) => (TradeSlot)(int)slot;
    }
}