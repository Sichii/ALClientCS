#region
using System.Text.Json;
using AL.Core.Json;
#endregion

namespace AL.SocketClient.Json.SystemTextJson;

/// <summary>
///     The socket transport's System.Text.Json configuration: the canonical <see cref="ALJson.Options" /> layered with the
///     socket-only converters that AL.Core cannot reference (they target types in this assembly). Used by the SocketIO
///     serializer,
///     <c>
///         HandleEventAsync
///     </c>
///     , and every message deserialize.
/// </summary>
/// <remarks>
///     The socket converters are inserted at the front so they win the first-CanConvert-wins resolution over the base
///     factories. This is not merely cosmetic for <see cref="BankDataConverter" />:
///     <c>
///         BankInfo
///     </c>
///     is an
///     <c>
///         IReadOnlyDictionary
///     </c>
///     carrying
///     <c>
///         [JsonForcedObject]
///     </c>
///     , which the base options' trailing
///     <c>
///         ForcedObjectConverterFactory
///     </c>
///     would otherwise claim and bind by named member — wrong for its flat gold/pack shape.
/// </remarks>
public static class SocketJson
{
    /// <summary>
    ///     The socket options instance. Built once from a copy of the shared options.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(ALJson.Options);

        options.Converters.Insert(0, new EventAndBossDataConverter());
        options.Converters.Insert(1, new DisappearDataConverter());
        options.Converters.Insert(2, new TradeHistoryEntryConverter());
        options.Converters.Insert(3, new BankDataConverter());

        //Inventory is IReadOnlyList<Item?> with only a list-taking ctor; without this System.Text.Json routes it
        //to the collection converter and fails to instantiate it. Must precede the built-in collection handling.
        options.Converters.Insert(4, new InventoryConverter());

        return options;
    }
}