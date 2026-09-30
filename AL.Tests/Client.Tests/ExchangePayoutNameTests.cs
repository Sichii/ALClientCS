#region
using AL.Client;
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     An exchange payout log names the item by display name, so the prize slot is found by rendering each candidate the
///     way the server does: title-cased property and <c>+level</c> for a single item, the bare name for a stack.
/// </summary>
public class ExchangePayoutNameTests : GameDataTestBed
{
    [Test]
    public void ASingleItemMatchesItsDisplayName()
        => ALClient.IsAnnouncedItem(Single("tshirt0"), "T-Shirt (Int)", 1)
                   .Should()
                   .BeTrue();

    [Test]
    public void AnItemHandedOverMidExchangeDoesNotMatch()
        => ALClient.IsAnnouncedItem(Single("hpamulet"), "T-Shirt (Int)", 1)
                   .Should()
                   .BeFalse();

    [Test]
    public void ALevelAndATitleAreRendered()
        => ALClient.IsAnnouncedItem(
                       Single("wattire", 2) with
                       {
                           Prediction = new Prediction
                           {
                               Title = "shiny"
                           }
                       },
                       "Shiny Wanderer's Attire +2",
                       1)
                   .Should()
                   .BeTrue();

    [Test]
    public void AStackIsNamedBare()
        => ALClient.IsAnnouncedItem(
                       Single("cscroll0") with
                       {
                           Quantity = 5
                       },
                       "Compound Scroll",
                       3)
                   .Should()
                   .BeTrue();

    private static Item Single(string name, int level = 0)
        => new()
        {
            Name = name,
            Level = level
        };
}