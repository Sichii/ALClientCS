namespace AL.Tests;

/// <summary>
///     Constraint keys for <see cref="NotInParallelAttribute" />. TUnit runs every test concurrently by default; tests
///     sharing a key never overlap, and unkeyed tests keep running in parallel alongside them.
/// </summary>
/// <remarks>
///     Applied at class level, a key also serializes that class's own test methods against each other, which is what the
///     shared statics below need. Do not add a method-level
///     <c>
///         [NotInParallel]
///     </c>
///     inside a keyed class - method scope replaces the inherited class key rather than merging with it.
/// </remarks>
internal static class ParallelKeys
{
    //GameDataTestBed populates the GameData statics from the live API, and GameDataLoadCharacterization nulls
    //them, reloads from the committed snapshot and restores them, so every reader serializes against both
    internal const string GAME_DATA = "GameData";

    //SocketTestBed hands each test its own ALSocketClient, so these only ever serialize a class against itself
    internal const string SOCKET_DISPATCH = "SocketDispatch";

    internal const string SOCKET_MESSAGE_HANDLER = "SocketMessageHandler";
}