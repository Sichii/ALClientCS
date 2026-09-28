#region
using AL.Data.Images;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     The two per-sprite placement tables the wardrobe carries beside its defaults.
/// </summary>
public class GCosmeticsTests
{
    /// <summary>
    ///     Both bind by sprite name, as design/cosmetics.js writes them, and a payload without them leaves both empty.
    /// </summary>
    [Test]
    public void HeadYAndBackBindBySpriteName()
    {
        var cosmetics = TestJson.Data<GCosmetics>("""{"head_y":{"mbody4b":1},"back":{"backpacks00":1,"backpacks201":1}}""")!;

        cosmetics.HeadY
                 .Should()
                 .ContainSingle()
                 .Which
                 .Should()
                 .Be(new KeyValuePair<string, double>("mbody4b", 1));

        cosmetics.Back["backpacks201"]
                 .Should()
                 .Be(1);

        cosmetics.Back
                 .Should()
                 .HaveCount(2);

        var bare = TestJson.Data<GCosmetics>("{}")!;

        bare.HeadY
            .Should()
            .BeEmpty();

        bare.Back
            .Should()
            .BeEmpty();
    }
}