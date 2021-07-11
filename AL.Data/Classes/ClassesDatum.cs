using AL.Core.Definitions;

namespace AL.Data.Classes
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class ClassesDatum : DatumBase<Class>
    {
        /// <summary>
        ///     Default info for the <see cref="ALClass">mage</see> class.
        /// </summary>
        public Class Mage { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">merchant</see> class.
        /// </summary>
        public Class Merchant { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">paladin</see> class.
        /// </summary>
        public Class Paladin { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">priest</see> class.
        /// </summary>
        public Class Priest { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">ranger</see> class.
        /// </summary>
        public Class Ranger { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">rogue</see> class.
        /// </summary>
        public Class Rogue { get; set; } = null!;
        /// <summary>
        ///     Default info for the <see cref="ALClass">warrior</see> class.
        /// </summary>
        public Class Warrior { get; set; } = null!;
    }
}