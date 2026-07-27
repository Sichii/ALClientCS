#region
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.SocketClient.Model;
#endregion

namespace AL.SocketClient.Json.SystemTextJson;

/// <summary>
///     Deserializes <see cref="BankInfo" /> from a flat object: the
///     <c>
///         gold
///     </c>
///     key -&gt; Gold, every other key that parses to a <see cref="BankPack" /> -&gt; an item array for that pack. The
///     System.Text.Json replacement for the Newtonsoft
///     <c>
///         BankDataConverter
///     </c>
///     .
/// </summary>
public sealed class BankDataConverter : JsonConverter<BankInfo>
{
    public override BankInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var obj = JsonNode.Parse(ref reader)
                          ?.AsObject();

        if (obj is null)
            return null;

        var dic = new Dictionary<BankPack, IReadOnlyList<Item?>>();
        var gold = 0L;

        foreach ((var key, var node) in obj)
            if (key == "gold")
                gold = node!.GetValue<long>();
            else if (EnumHelper.TryParse(key, out BankPack bankPack))
                dic[bankPack] = node!.Deserialize<Item[]>(options) ?? throw new InvalidOperationException("Failed to deserialize items.");

        return new BankInfo
        {
            Gold = gold,
            Items = new ReadOnlyDictionary<BankPack, IReadOnlyList<Item?>>(dic)
        };
    }

    public override void Write(Utf8JsonWriter writer, BankInfo value, JsonSerializerOptions options) => throw new NotSupportedException();
}