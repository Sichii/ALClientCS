#region
using System.Text.Json;
using AL.Core.Json;
#endregion

namespace AL.SocketClient.Json.SystemTextJson;

/// <summary>
///     Provides the socket transport's JSON options: <see cref="ALJson.Options" /> plus the converters for types in this
///     assembly.
/// </summary>
/// <remarks>
///     The socket converters go at the front so they win over the base factories; otherwise the trailing
///     <c>ForcedObjectConverterFactory</c> would claim <c>BankInfo</c> and bind its flat gold/pack shape by member name.
/// </remarks>
public static class SocketJson
{
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(ALJson.Options);

        options.Converters.Insert(0, new EventAndBossDataConverter());
        options.Converters.Insert(1, new DisappearDataConverter());
        options.Converters.Insert(2, new TradeHistoryEntryConverter());
        options.Converters.Insert(3, new BankDataConverter());

        //must precede the built-in collection handling
        options.Converters.Insert(4, new InventoryConverter());

        return options;
    }
}