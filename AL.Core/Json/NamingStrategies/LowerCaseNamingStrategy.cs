#region
using Newtonsoft.Json.Serialization;
#endregion

namespace AL.Core.Json.NamingStrategies;

/// <summary>
///     Lowercases a name in full, with no separators.
/// </summary>
/// <remarks>
///     The server's slot and pack keys are the bare lowercase member name - "mainhand", "trade1", "items0" -
///     so an enum serialized with its PascalCase name silently fails every server-side lookup that uses it.
/// </remarks>
/// <seealso cref="NamingStrategy" />
public sealed class LowerCaseNamingStrategy : NamingStrategy
{
    protected override string ResolvePropertyName(string name) => name.ToLowerInvariant();
}
