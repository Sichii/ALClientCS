using System.Collections.Generic;
using AL.APIClient.Model;

namespace AL.APIClient.Response
{
    /// <summary>
    /// Represents the data received when requesting mail from the server.
    /// </summary>
    public record MailResponse
    {
        /// <summary>
        /// The id of the cursor iterating the mail, if you have <see cref="More"/> mail.
        /// </summary>
        public string? Cursor { get; init; }
        
        /// <summary>
        /// Whether or not there is a <see cref="Cursor"/>.
        /// </summary>
        public bool Cursored { get; init; }
        
        /// <summary>
        /// Whether or not there is more mail.
        /// </summary>
        public bool More { get; init; }
        
        /// <summary>
        /// A list of mails in your inbox.
        /// </summary>
        public IReadOnlyList<Mail> Mail { get; } = new List<Mail>();
    }
}