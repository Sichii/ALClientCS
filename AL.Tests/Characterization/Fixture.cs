#region
using System;
using System.IO;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Offline access to the committed game-data snapshot.
/// </summary>
/// <remarks>
///     The characterisation suite pins the exact behaviour of Newtonsoft.Json so the System.Text.Json
///     migration has a fixed target. That only works against a fixed input, so these tests read the
///     committed snapshot rather than <see cref="GameDataTestBed" />, which fetches from the live API and
///     changes whenever the game does.
/// </remarks>
public static class Fixture
{
    private static readonly Lazy<string> RawGameData = new(() => File.ReadAllText(GameDataPath));

    private static readonly Lazy<JObject> ParsedGameData = new(() => JObject.Parse(RawGameData.Value));

    /// <summary>
    ///     Path to the snapshot on disk.
    /// </summary>
    public static string GameDataPath => Path.Combine(AppContext.BaseDirectory, "Fixtures", "data.json");

    /// <summary>
    ///     The whole snapshot as text. ~10MB, parsed once per test run.
    /// </summary>
    public static string GameDataJson => RawGameData.Value;

    /// <summary>
    ///     The whole snapshot, parsed.
    /// </summary>
    public static JObject GameData => ParsedGameData.Value;

    /// <summary>
    ///     A top-level section of the snapshot, e.g. "items", "monsters", "maps".
    /// </summary>
    public static JToken Section(string name)
        => GameData[name] ?? throw new InvalidOperationException($@"Snapshot has no ""{name}"" section.");

    /// <summary>
    ///     A single entry from a top-level section, e.g. <c>Entry("items", "fireblade")</c>.
    /// </summary>
    public static JToken Entry(string section, string key)
        => Section(section)[key] ?? throw new InvalidOperationException($@"Snapshot has no ""{section}.{key}"".");

    /// <summary>
    ///     Writes a generated snapshot beside the test binary and returns its path. Used by the
    ///     characterisation tests that pin thousands of values and cannot hold them as inline literals.
    /// </summary>
    public static string WriteSnapshot(string fileName, string content)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "Fixtures", "snapshots");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, content);

        return path;
    }

    /// <summary>
    ///     Reads a snapshot committed under <c>AL.Tests/Fixtures/snapshots</c>, or null if it does not exist yet.
    /// </summary>
    public static string? ReadCommittedSnapshot(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "snapshots", fileName);

        return File.Exists(path) ? File.ReadAllText(path) : null;
    }
}
