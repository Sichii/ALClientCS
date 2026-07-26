#region
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
#endregion

namespace AL.Tests.APIClient.Tests;

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