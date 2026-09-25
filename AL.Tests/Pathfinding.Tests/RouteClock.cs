#region
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     The starting state and pricing inputs one trip is timed under. <c>MinBlinkCost</c> is the least a cast is priced at
///     in walk units, the route's blink floor; zero prices a cast at its time alone.
/// </summary>
public sealed record ClockSettings(
    float Speed,
    float PenaltyMs,
    float BlinkReadyInMs,
    float? MpPerSecond,
    float Mp,
    float MaxMp,
    float Reserve,
    float MinBlinkCost = 0f);

/// <summary>
///     Times a route under the server's rules, restated here rather than read from the search, so a wrong search cannot
///     agree with itself.
/// </summary>
public static class RouteClock
{
    private const double DOOR_PENALTY_MS = 3200;
    private const double EFFECT_PENALTY_MS = 812;
    private const double PENALTY_CAP_MS = 120000;
    private const double PENALTY_CHARGE_CAP_MS = 10000;
    private const double BLINK_COOLDOWN_MS = 1200;
    private const double BLINK_MP = 1600;
    private const double BLINK_LANDING_MS = 200;
    private const double TOWN_CHANNEL_MS = 3000;
    private const double TOWN_RISK_PREMIUM = 1.2;

    /// <summary>
    ///     The seconds the route takes, and its price in walk units: those seconds times speed, plus each door leg's own cost
    ///     and the recall's premium, and whatever lifts a cast's time to <see cref="ClockSettings.MinBlinkCost" />.
    /// </summary>
    public static (double Seconds, double Price) Measure(IReadOnlyList<PathEdge> legs, ClockSettings settings)
    {
        var elapsedMs = 0.0;
        var extras = 0.0;
        double readyMs = settings.BlinkReadyInMs;
        double penaltyMs = settings.PenaltyMs;
        double mp = settings.Mp;
        var tracked = settings.MpPerSecond is not null;
        var rate = settings.MpPerSecond ?? 0f;

        void Pass(double ms)
        {
            elapsedMs += ms;
            readyMs = Math.Max(0, readyMs - ms);
            penaltyMs = Math.Max(0, penaltyMs - ms);

            if (tracked)
                mp = Math.Min(settings.MaxMp, mp + rate * ms / 1000.0);
        }

        foreach (var leg in legs)
            switch (leg.Type)
            {
                case EdgeType.Walk:
                    Pass(leg.Cost / settings.Speed * 1000.0);

                    break;
                case EdgeType.Door:
                case EdgeType.Transport:
                case EdgeType.Leave:
                    extras += leg.Cost;
                    penaltyMs = Math.Min(PENALTY_CAP_MS, penaltyMs + DOOR_PENALTY_MS);

                    break;
                case EdgeType.Town:
                    extras += TOWN_CHANNEL_MS / 1000.0 * settings.Speed * (TOWN_RISK_PREMIUM - 1);
                    Pass(TOWN_CHANNEL_MS);
                    penaltyMs = Math.Min(PENALTY_CAP_MS, penaltyMs + EFFECT_PENALTY_MS);

                    break;
                case EdgeType.Blink:
                {
                    var waitMs = readyMs;

                    if (tracked)
                    {
                        var need = BLINK_MP + settings.Reserve;

                        if (need > settings.MaxMp)
                            return (double.PositiveInfinity, double.PositiveInfinity);

                        if (mp < need)
                        {
                            if (rate <= 0)
                                return (double.PositiveInfinity, double.PositiveInfinity);

                            waitMs = Math.Max(waitMs, (need - mp) / rate * 1000.0);
                        }
                    }

                    Pass(waitMs);
                    readyMs = Math.Min(penaltyMs, PENALTY_CHARGE_CAP_MS) + BLINK_COOLDOWN_MS;

                    if (tracked)
                        mp -= BLINK_MP;

                    Pass(BLINK_LANDING_MS);
                    extras += Math.Max(0, settings.MinBlinkCost - (waitMs + BLINK_LANDING_MS) / 1000.0 * settings.Speed);
                    penaltyMs = Math.Min(PENALTY_CAP_MS, penaltyMs + EFFECT_PENALTY_MS);

                    break;
                }
            }

        var seconds = elapsedMs / 1000.0;

        return (seconds, seconds * settings.Speed + extras);
    }
}