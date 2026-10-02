#region
using AL.Core.Abstractions;
#endregion

namespace AL.Data.Sets;

/// <summary>
///     Represents one bundle of stats attached to a set: either a single tier's own line, or the running total through
///     that tier.
///     <br />
///     <inheritdoc cref="AttributedRecordBase" />
/// </summary>
/// <remarks>
///     Read it through <see cref="AttributedRecordBase.Attributes" />; on <see cref="GSetTier.InEffect" /> the declared stat
///     properties are all zero.
/// </remarks>
/// <seealso cref="AttributedRecordBase" />
public sealed record GSetBonus : AttributedRecordBase;