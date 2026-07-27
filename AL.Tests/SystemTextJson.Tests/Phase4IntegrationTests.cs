#region
using System.Text.Json;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json;
using AL.SocketClient.Json.SystemTextJson;
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.SystemTextJson.Tests;

/// <summary>
///     STJ Phase 4 focused guards for the parity gaps the entry-point cutover exposed once production ran through
///     <see cref="ALJson.Options" />/<see cref="SocketJson.Options" /> on the socket + G-data paths: tolerant enums as
///     dictionary keys, positional types whose constructor carries an unbound parameter, and the read-only
///     <see cref="Inventory" /> collection.
/// </summary>
public sealed class Phase4IntegrationTests
{
    [Test]
    public void Inventory_BindsFromItemArray()
    {
        // Inventory is a read-only IReadOnlyList<Item?> with only a list-taking ctor; the built-in collection
        // converter cannot instantiate it, so a dedicated converter reads the array and hands it to the ctor.
        var inventory = JsonSerializer.Deserialize<Inventory>("""[{"name":"hpot0"},null,{"name":"mpot0"}]""", SocketJson.Options)!;

        inventory.Count
                 .Should()
                 .Be(3);

        inventory[0]!.Name
                     .Should()
                     .Be("hpot0");

        inventory[1]
            .Should()
            .BeNull();

        inventory[2]!.Name
                     .Should()
                     .Be("mpot0");
    }

    [Test]
    public void StraightLine_BindsFromPositionalArray_DefaultingUnboundCtorParameter()
    {
        // StraightLine is [JsonArrayIndex] On/Start/End with a 4-arg ctor whose isVertical parameter maps to a
        // [JsonIgnore] property (enrichment sets it later). System.Text.Json's parameterized-object binding
        // rejects the unbindable parameter, so the converter constructs it directly and defaults isVertical.
        var line = JsonSerializer.Deserialize<StraightLine>("[10,20,30]", ALJson.Options)!;

        line.On
            .Should()
            .Be(10);

        line.Start
            .Should()
            .Be(20);

        line.End
            .Should()
            .Be(30);

        line.IsVertical
            .Should()
            .BeFalse();
    }

    [Test]
    public void TolerantEnum_BindsAsDictionaryKey()
    {
        // GClass.mainhand and Character.slots are keyed by a tolerant enum; STJ routes the KEY through the key
        // type's converter, which must implement ReadAsPropertyName or it throws NotSupportedException.
        var byWeapon = JsonSerializer.Deserialize<Dictionary<WeaponType, int>>("""{"spear":5,"bow":7}""", ALJson.Options)!;

        byWeapon.Should()
                .ContainKey(WeaponType.Spear)
                .WhoseValue
                .Should()
                .Be(5);

        byWeapon.Should()
                .ContainKey(WeaponType.Bow)
                .WhoseValue
                .Should()
                .Be(7);
    }

    [Test]
    public void TolerantEnum_UnknownDictionaryKey_DegradesToDefault()
    {
        // an unrecognized key degrades to the enum's zero member instead of throwing - a deliberate divergence from
        // the pre-migration behaviour, which threw, re-baselined in Phase 5. The default key does not collide with a
        // real one in this payload.
        var parsed = JsonSerializer.Deserialize<Dictionary<WeaponType, int>>("""{"totally_bogus":9}""", ALJson.Options)!;

        parsed.Should()
              .ContainKey(default)
              .WhoseValue
              .Should()
              .Be(9);
    }
}