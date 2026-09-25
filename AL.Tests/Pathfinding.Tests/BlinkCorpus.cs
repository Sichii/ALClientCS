#region
using AL.Core.Geometry;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     One route search's recorded answer for the blink corpus. Every member mirrors <c>blink-paths.json</c>.
/// </summary>
public sealed record BlinkCorpus(
    int Seed,
    string Config,
    float Floor,
    List<BlinkSetting> Settings,
    List<BlinkTrip> Trips);

public sealed record BlinkSetting(
    string Name,
    float Speed,
    float PenaltyMs,
    bool Tracked,
    float Mp,
    float MaxMp,
    float MpPerSecond,
    float Reserve)
{
    /// <summary>
    ///     Speed 60 and 120, the slow and fast ends of what characters move at; rested and just through a door; mana
    ///     untracked, and tracked with a full 4000 bar refilled at 250/s (<c>mpot1</c> per <c>use_mp</c> cooldown) and no
    ///     reserve.
    /// </summary>
    public static List<BlinkSetting> All()
    {
        var settings = new List<BlinkSetting>();

        foreach (var speed in new[]
                 {
                     60f,
                     120f
                 })
            foreach (var penalty in new[]
                     {
                         0f,
                         3200f
                     })
                foreach (var tracked in new[]
                         {
                             false,
                             true
                         })
                    settings.Add(
                        new BlinkSetting(
                            $"s{speed:F0}-{(penalty > 0 ? "door" : "rested")}-{(tracked ? "mana" : "free")}",
                            speed,
                            penalty,
                            tracked,
                            4000f,
                            4000f,
                            250f,
                            0f));

        return settings;
    }

    public ClockSettings ToClock()
        => new(
            Speed,
            PenaltyMs,
            0f,
            Tracked ? MpPerSecond : null,
            Mp,
            MaxMp,
            Reserve);
}

public sealed record BlinkTrip(
    int Id,
    BlinkPoint Start,
    BlinkPoint End,
    List<BlinkResult> Results);

public sealed record BlinkPoint(string Map, float X, float Y);

public sealed record BlinkResult(
    string Setting,
    bool Found,
    float Cost,
    double Micros,
    List<BlinkLeg> Legs);

public sealed record BlinkLeg(
    string Type,
    BlinkPoint Start,
    BlinkPoint End,
    float Cost)
{
    /// <summary>
    ///     The recorded leg as a <see cref="PathEdge" />, so a recorded route can be timed. A door leg's start comes back as a
    ///     plain <see cref="Location" />, not the exit it was.
    /// </summary>
    public PathEdge ToPathEdge()
        => new(
            Enum.Parse<EdgeType>(Type),
            new Location(Start.Map, Start.X, Start.Y),
            new Location(End.Map, End.X, End.Y),
            Cost);
}