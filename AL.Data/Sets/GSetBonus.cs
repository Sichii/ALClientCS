#region
using AL.Core.Abstractions;
#endregion

namespace AL.Data.Sets;

/// <summary>
///     One bundle of stats attached to a set: either a single tier's own line, or the running total through that
///     tier.
///     <br />
///     <inheritdoc cref="AttributedRecordBase" />
/// </summary>
/// <remarks>
///     <b>Read this through <see cref="AttributedRecordBase.Attributes" />, never through the declared stat
///     properties.</b> A bonus parsed off the wire fills both, but <see cref="GSetTier.InEffect" /> is summed in
///     <c>GameData.EnrichSets</c> rather than deserialized, so on that one only the dictionary is populated and
///     every declared property reads nought. The dictionary is the only representation both paths agree on, and it
///     is what the explorer renders.
/// </remarks>
/// <seealso cref="AttributedRecordBase" />
public sealed record GSetBonus : AttributedRecordBase;
