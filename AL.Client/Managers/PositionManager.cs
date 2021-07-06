using System.Threading.Tasks;
using AL.Client.Abstractions;
using AL.Core.Helpers;

namespace AL.Client.Managers
{
    public class PositionManager : AsyncManagerBase
    {
        public PositionManager(ALClient client)
            : base(client) { }

        protected override async Task DoWorkAsync()
        {
            await UpdatePlayerPositions();
            await UpdateMonsterPositions();

            var delta = DeltaTime.Value;
            var character = Client.Character;
            character.Update(delta - character.Delta);
        }

        private ValueTask UpdateMonsterPositions() => Client.Monsters.AssertAsync(dic =>
        {
            foreach (var monster in dic.Values)
            {
                var delta = DeltaTime.Value;
                monster.Update(delta - monster.Delta);
            }
        });

        private ValueTask UpdatePlayerPositions() => Client.Players.AssertAsync(dic =>
        {
            foreach (var player in dic.Values)
            {
                var delta = DeltaTime.Value;
                player.Update(delta - player.Delta);
            }
        });
    }
}