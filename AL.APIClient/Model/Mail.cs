using Newtonsoft.Json;

namespace AL.APIClient.Model
{
    /// <summary>
    ///     Represents a piece of mail in the user's mailbox.
    /// </summary>
    public record Mail
    {
        /// <summary>
        ///     The name of the person the mail is from.
        /// </summary>
        [JsonProperty("fro")]
        public string From { get; init; } = null!;

        /// <summary>
        ///     A unique identifier for this piece of mail.
        /// </summary>
        public string Id { get; init; } = null!;

        /// <summary>
        ///     If populated, there is an item in this mail. <br />
        ///     This is the name of the item.
        /// </summary>
        public string? Item { get; init; }

        public string Message { get; init; } = string.Empty;

        /// <summary>
        ///     Whether or not this user sent this mail.
        /// </summary>
        public bool Sent { get; init; }

        /// <summary>
        ///     Whether or not the item was taken from this mail.
        /// </summary>
        public bool Taken { get; init; }

        /// <summary>
        ///     The name of the person this mail is to.
        /// </summary>
        public string? To { get; init; }
    }
}