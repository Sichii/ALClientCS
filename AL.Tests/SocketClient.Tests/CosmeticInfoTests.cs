#region
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     The <c>cx</c> block on a player frame. A slot the character wears nothing in sends no key at all, so a
///     missing key has to read as null - a non-nullable member there reports an empty string as a worn cosmetic.
///     <see cref="CosmeticInfo" /> carries which ten slots exist and why.
/// </summary>
public class CosmeticInfoTests
{
    /// <summary>
    ///     Every slot the server can fill, in one frame. <c>tail</c>, <c>back</c> and <c>gravestone</c> were the
    ///     three the model used to drop on the floor.
    /// </summary>
    [Test]
    public void EveryServerSlotSurvivesDeserialization()
    {
        const string ALL_TEN
            = @"{""chin"":""beard1"",""face"":""face2"",""gravestone"":""stone3"",""hair"":""hair4"",""hat"":""hat5"","
              + @"""head"":""head6"",""makeup"":""makeup7"",""back"":""wings8"",""tail"":""tail9"",""upper"":""armor10""}";

        var obj = TestJson.Socket<CosmeticInfo>(ALL_TEN);

        obj.Should()
           .NotBeNull();

        obj.Chin
           .Should()
           .Be("beard1");

        obj.Face
           .Should()
           .Be("face2");

        obj.Gravestone
           .Should()
           .Be("stone3");

        obj.Hair
           .Should()
           .Be("hair4");

        obj.Hat
           .Should()
           .Be("hat5");

        obj.Head
           .Should()
           .Be("head6");

        obj.Makeup
           .Should()
           .Be("makeup7");

        obj.Back
           .Should()
           .Be("wings8");

        obj.Tail
           .Should()
           .Be("tail9");

        obj.Upper
           .Should()
           .Be("armor10");
    }

    /// <summary>
    ///     The common case: almost every character wears something in one or two slots and nothing in the rest.
    /// </summary>
    [Test]
    public void AnEmptySlotIsNullRatherThanEmpty()
    {
        var obj = TestJson.Socket<CosmeticInfo>(@"{""hat"":""hat5""}");

        obj.Should()
           .NotBeNull();

        obj.Hat
           .Should()
           .Be("hat5");

        obj.Tail
           .Should()
           .BeNull("an unworn slot sends no key, and no key is not an empty string");

        obj.Gravestone
           .Should()
           .BeNull();

        obj.Upper
           .Should()
           .BeNull();
    }
}
