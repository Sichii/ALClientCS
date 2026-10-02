namespace AL.Data;

/// <summary>
///     Marks a <see cref="GameData" /> static property as a root of the game data payload, bound by
///     <see cref="GameData.Bind" />.
/// </summary>
/// <remarks>
///     System.Text.Json cannot bind static members, so the roots are bound by hand.
/// </remarks>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Property)]
internal sealed class GameDataRootAttribute : Attribute;