using AL.Core.Definitions;

namespace AL.Data.Classes
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class ClassesDatum : DatumBase<GClass>
    {
        /// <summary>
        ///     Default info for the <see cref="ALClass">mage</see> class.
        /// </summary>
        public GClass Mage { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">merchant</see> class.
        /// </summary>
        public GClass Merchant { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">paladin</see> class.
        /// </summary>
        public GClass Paladin { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">priest</see> class.
        /// </summary>
        public GClass Priest { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">ranger</see> class.
        /// </summary>
        public GClass Ranger { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">rogue</see> class.
        /// </summary>
        public GClass Rogue { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">warrior</see> class.
        /// </summary>
        public GClass Warrior { get; set; } = null!;
    }
}