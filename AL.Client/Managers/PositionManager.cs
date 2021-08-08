using System.Threading.Tasks;
using AL.Client.Abstractions;
using AL.Core.Helpers;
using CORE_CONSTANTS = AL.Core.Definitions.CONSTANTS;

namespace AL.Client.Managers
{
    internal sealed class PositionManager : AsyncDeltaLoop
    {
        protected override float PollingRate => ALClientSettings.PositionPollingRate;

        internal PositionManager(ALClient client)
            : base(client) { }

        protected override async Task DoWorkAsync()
        {
            var deltaTime = DeltaTime.Value;

            await UpdatePlayerPositions(deltaTime);
            await UpdateMonsterPositions(deltaTime);

            Client.Character.Update(deltaTime);
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