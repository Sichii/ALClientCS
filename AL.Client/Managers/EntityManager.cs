#region
using AL.Client.Abstractions;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using Chaos.Time;
using CORE_CONSTANTS = AL.Core.Definitions.CONSTANTS;
#endregion

namespace AL.Client.Managers;

public sealed class EntityManager : AsyncDeltaLoop
{
    private readonly IntervalTimer ForceCharacterUpdateTimer = new(TimeSpan.FromSeconds(15), false);

    /// <summary>
    ///     The timer for requesting a full entity set. The server does not always name an entity it drops, which leaves the
    ///     entry frozen at its last hp; only a <c>type:"all"</c> frame rebuilds the set.
    /// </summary>
    private readonly IntervalTimer ForceEntitiesUpdateTimer = new(TimeSpan.FromSeconds(60), false);

    private IDisposable? OnCharacterSubscription;
    private IDisposable? OnEntitiesSubscription;
    protected override float PollingRate => ALClientSettings.PositionPollingRate;

    internal EntityManager(ALClient client)
        : base(client) { }

    internal void AttachListener()
    {
        OnCharacterSubscription?.Dispose();

        OnCharacterSubscription = Client.Socket.On<CharacterData>(
            ALSocketMessageType.Character,
            _ =>
            {
                ForceCharacterUpdateTimer.Reset();

                return TaskCache.FALSE;
            });

        OnEntitiesSubscription?.Dispose();

        //a login, map change or transport already carries a full set, so the timer measures time since the last one
        //rather than time since the last request
        OnEntitiesSubscription = Client.Socket.On<EntitiesData>(
            ALSocketMessageType.Entities,
            data =>
            {
                if (data.UpdateType == EntitiesUpdateType.All)
                    ForceEntitiesUpdateTimer.Reset();

                return TaskCache.FALSE;
            });
    }

    protected override async Task DoWorkAsync(TimeSpan delta, CancellationToken cancellationToken)
    {
        UpdatePlayers(delta);
        UpdateMonsters(delta);
        Client.Update(delta);

        ForceCharacterUpdateTimer.Update(delta);

        if (ForceCharacterUpdateTimer.IntervalElapsed)
            await Client.RequestCharacterAsync();

        ForceEntitiesUpdateTimer.Update(delta);

        if (ForceEntitiesUpdateTimer.IntervalElapsed)
            await Client.RequestEntitiesAsync();
    }

    private void UpdateMonsters(TimeSpan deltaTime)
    {
        List<string>? outOfSight = null;
        var wrongInstance = 0;

        foreach (var monster in Client.Monsters.Values)
        {
            monster.Update(deltaTime);

            if (Client.Character.DistanceWithInstanceCheck(monster) <= CORE_CONSTANTS.MAX_VISION)
                continue;

            //counted apart: a whole collection dropping at once is the instance check reading an empty map or instance
            if (!Client.Character.InSameInstanceAs(monster))
                wrongInstance++;

            (outOfSight ??= []).Add(monster.Id);
        }

        if (outOfSight is null)
            return;

        foreach (var id in outOfSight)
            Client.Monsters.Remove(id, out _);

        //only the instance check is worth a line, it empties the whole collection at once
        if (wrongInstance > 0)
            Client.Logger.Debug(
                $"Dropped {outOfSight.Count} monsters out of vision, {wrongInstance} of them on the instance check "
                + $"(this character is in '{Client.Character.In}' on '{Client.Character.Map}'); {Client.Monsters.Count} left");
    }

    private void UpdatePlayers(TimeSpan deltaTime)
    {
        List<string>? outOfSight = null;

        foreach (var player in Client.Players.Values)
        {
            player.Update(deltaTime);

            if (Client.Character.DistanceWithInstanceCheck(player) > CORE_CONSTANTS.MAX_VISION)
                (outOfSight ??= []).Add(player.Id);
        }

        if (outOfSight is null)
            return;

        foreach (var id in outOfSight)
            Client.Players.Remove(id, out _);
    }
}