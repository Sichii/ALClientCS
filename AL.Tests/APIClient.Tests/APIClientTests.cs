#region
using AL.APIClient;
using FluentAssertions;
#endregion

namespace AL.Tests.APIClient.Tests;

/// <summary>
///     The game-data cache, checked against a host nothing answers on so the suite pays no download.
/// </summary>
/// <remarks>
///     Only the eviction half is worth a test - that two successful calls share one <c>Lazy</c> is what the type
///     does. A cached failure would be the expensive kind of bug: one blocked fetch at boot and every later caller
///     in the process gets the same exception back forever, with no request going out to say otherwise.
/// </remarks>
public class GameDataCacheTests
{
    private const string DEAD_HOST = "http://127.0.0.1:1";

    [Test]
    public async Task AFailedFetchIsNotCached()
    {
        var first = await Catch();
        var second = await Catch();

        first.Should()
             .NotBeNull();

        second.Should()
              .NotBeNull()
              .And
              .NotBeSameAs(first, "a cached failure would hand back the very same exception instance");

        return;

        static async Task<Exception?> Catch()
        {
            try
            {
                await AlApiClient.GetGameDataAsync(DEAD_HOST);

                return null;
            } catch (Exception e)
            {
                return e;
            }
        }
    }
}

public class APIClientTests : APITestBed
{
    [Test]
    public async Task GetMailTest()
    {
        var result = await APIClient.GetMailAsync()
                                    .ToListAsync();

        result.Should()
              .NotBeNull();
    }

    [Test]
    public async Task GetMerchantTest()
    {
        var result = await APIClient.GetMerchantsAsync()
                                    .ToListAsync();

        result.Should()
              .NotBeNull();

        (result.Count != 0).Should()
                           .BeTrue();
    }

    [Test]
    public async Task UpdateServersAndCharactersTest()
    {
        var serversAndCharacters = await APIClient.GetServersAndCharactersAsync();

        serversAndCharacters.Servers
                            .Should()
                            .NotBeNull();

        serversAndCharacters.Characters
                            .Should()
                            .NotBeNull();

        serversAndCharacters.Servers
                            .Any()
                            .Should()
                            .BeTrue();

        serversAndCharacters.Characters
                            .Any()
                            .Should()
                            .BeTrue();
    }
}