#region
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.SocketClient.Model;
#endregion

namespace AL.SocketClient.Json.SystemTextJson;

/// <summary>
///     Builds an <see cref="Inventory" /> from the wire's item array, which the built-in collection handling cannot
///     instantiate. <c>Character.OnDeserialized</c> sizes it afterwards.
/// </summary>
public sealed class InventoryConverter : JsonConverter<Inventory>
{
    public override Inventory? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var items = JsonSerializer.Deserialize<List<Item?>>(ref reader, options);

        return new Inventory(items);
    }

    public override void Write(Utf8JsonWriter writer, Inventory value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.Items, options);
}