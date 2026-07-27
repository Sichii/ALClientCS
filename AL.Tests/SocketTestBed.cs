#region
using AL.Core.Helpers;
using AL.SocketClient;
using Common.Logging;
#endregion

namespace AL.Tests;

/// <summary>
///     A never-connected <see cref="ALSocketClient" /> to dispatch raw frames through. It is built per test rather than
///     per class so a subscription one test forgets to dispose cannot be seen by the next.
/// </summary>
public abstract class SocketTestBed
{
    protected const string ACTION_FRAME = @"[
   ""action"",
   {
      ""attacker"":""2144160"",
      ""target"":""Moneybaggers"",
      ""type"":""attack"",
      ""source"":""attack"",
      ""x"":595.7417319170224,
      ""y"":1091.179435638155,
      ""eta"":400,
      ""m"":361,
      ""pid"":""wMhQBT"",
      ""projectile"":""stone"",
      ""damage"":25
   }
]";

    protected ALSocketClient Socket { get; private set; } = null!;

    [After(Test)]
    public async Task DisposeSocketAsync() => await Socket.DisposeAsync();

    [Before(Test)]
    public void InitializeSocket() => Socket = new ALSocketClient(new FormattedLogger("test", LogManager.GetLogger<ALSocketClient>()));
}