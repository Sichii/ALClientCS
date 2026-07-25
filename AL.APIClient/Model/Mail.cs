#region
using AL.APIClient.Json.Converters;
using Newtonsoft.Json;
#endregion

namespace AL.APIClient.Model;

/// <summary>
///     Represents a piece of mail in the user's mailbox.
/// </summary>
public sealed record Mail
{
    /// <summary>
    ///     The name of the person the mail is from.
    /// </summary>
    [JsonProperty("fro")]
    public string From { get; init; } = null!;

    /// <summary>
    ///     A unique identifier for this piece of mail (carries an <c>ML_</c> prefix).
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     If populated, the simplified item attached to this mail.
    /// </summary>
    [JsonProperty("item"), JsonConverter(typeof(StringOrObjectMailItemConverter))]
    public MailItem? Item { get; init; }

    public string Message { get; init; } = string.Empty;

    /// <summary>
    ///     When the mail was created, as the server's raw date string (a JS <c>Date.toString()</c>, not ISO-8601).
    /// </summary>
    public string? Sent { get; init; }

    /// <summary>
    ///     The subject line. Empty string when the mail has no subject.
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    ///     Whether or not the item was taken from this mail.
    /// </summary>
    public bool Taken { get; init; }

    /// <summary>
    ///     The name of the person this mail is to.
    /// </summary>
    public string? To { get; init; }
}
