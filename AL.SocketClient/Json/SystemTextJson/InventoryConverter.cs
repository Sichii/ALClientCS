#region
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.SocketClient.Model;
#endregion

namespace AL.SocketClient.Json.SystemTextJson;

/// <summary>
///     Builds an <see cref="Inventory" /> from the wire's item array. <see cref="Inventory" /> is a read-only
///     <c>
///         IReadOnlyList&lt;Item?&gt;
///     </c>
///     with a single constructor taking its backing list; System.Text.Json would otherwise route it to the collection
///     converter and fail to instantiate it ("collection type is read only"). Newtonsoft used the type's
///     <c>
///         [JsonConstructor]
///     </c>
///     . Reads the array as a
///     <c>
///         List&lt;Item?&gt;
///     </c>
///     and hands it to the constructor;
///     <c>
///         Character.OnDeserialized
///     </c>
///     later sizes it.
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