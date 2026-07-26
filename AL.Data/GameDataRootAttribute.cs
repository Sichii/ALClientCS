#region
using System;
#endregion

namespace AL.Data;

/// <summary>
///     Marks a <see cref="GameData" /> static as a root of the G-data payload, so
///     <c>
///         GameData.Bind
///     </c>
///     knows to drive it from the wire.
/// </summary>
/// <remarks>
///     System.Text.Json cannot bind static members at all, so the roots are bound by hand and need a selector of their
///     own. Deliberately NOT a serialization attribute: one the serializer provably ignores reads as dead decoration, and
///     deleting it would reduce the entire G-data load to a silent no-op — the exact hazard that reflecting over
///     Newtonsoft's
///     <c>
///         [JsonProperty]
///     </c>
///     used to carry.
/// </remarks>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Property)]
internal sealed class GameDataRootAttribute : Attribute;