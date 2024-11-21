#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AL.Client.Abstractions;
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
    private IDisposable? OnCharacterSubscription;
    protected override float PollingRate => ALClientSettings.PositionPollingRate;

    internal EntityManager(ALClient client)
        : base(client) { }

    internal void AttachListener()
    {
        if (OnCharacterSubscription != null)
            try
            {
                OnCharacterSubscription.Dispose();
            } catch
            {
                //ignored
            }

        OnCharacterSubscription = Client.Socket.On<CharacterData>(
            ALSocketMessageType.Character,
            _ =>
            {
                ForceCharacterUpdateTimer.Reset();

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
    }

    private void UpdateMonsters(TimeSpan deltaTime)
    {
        foreach (var monster in Client.Monsters.Values)
            monster.Update(deltaTime);
    }

    private void UpdatePlayers(TimeSpan deltaTime)
    {
        foreach (var player in Client.Players.Values)
            player.Update(deltaTime);
    }
}