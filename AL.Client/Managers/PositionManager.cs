using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AL.Client.Abstractions;
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using CORE_CONSTANTS = AL.Core.Definitions.CONSTANTS;

namespace AL.Client.Managers
{
    internal sealed class PositionManager : AsyncDeltaLoop
    {
        private long LastCharacterUpdateDelta;
        private IAsyncDisposable? OnCharacterSubscription;
        protected override float PollingRate => ALClientSettings.PositionPollingRate;

        internal PositionManager(ALClient client)
            : base(client) =>
            LastCharacterUpdateDelta = DeltaTime.Value;

        internal async ValueTask AttachListenerAsync()
        {
            if (OnCharacterSubscription != null)
                try
                {
                    await OnCharacterSubscription.DisposeAsync().ConfigureAwait(false);
                } catch
                {
                    //ignored
                }

            OnCharacterSubscription = Client.Socket.On<CharacterData>(ALSocketMessageType.Character, _ =>
            {
                LastCharacterUpdateDelta = DeltaTime.Value;

                return TaskCache.FALSE;
            });
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var deltaTime = DeltaTime.Value;

            await UpdatePlayerPositions(deltaTime).ConfigureAwait(false);
            await UpdateMonsterPositions(deltaTime).ConfigureAwait(false);

            Client.Character.Update(deltaTime);

            if (deltaTime - LastCharacterUpdateDelta >= 1000 * 15)
            {
                await Client.RequestCharacterAsync().ConfigureAwait(false);
                LastCharacterUpdateDelta = DeltaTime.Value;
            }

            //remove stale projectiles
            foreach (var projectile in await Client.Projectiles.Values.ToArrayAsync().ConfigureAwait(false))
                if (DeltaTime.Value - projectile.Delta > projectile.ETA)
                    await Client.Projectiles.RemoveAsync(projectile.ProjectileId!).ConfigureAwait(false);
        }

        private ValueTask UpdateMonsterPositions(long deltaTime) => Client.Monsters.AssertAsync(dic =>
        {
            foreach (var monster in dic.Values)
                monster.Update(deltaTime);
        });

        private ValueTask UpdatePlayerPositions(long deltaTime) => Client.Players.AssertAsync(dic =>
        {
            foreach (var player in dic.Values)
                player.Update(deltaTime);
        });
    }
}