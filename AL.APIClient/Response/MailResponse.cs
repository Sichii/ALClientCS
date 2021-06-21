using System.Collections.Generic;
using AL.APIClient.Model;

namespace AL.APIClient.Response
{
    public record MailResponse
    {
        public string Cursor { get; init; }
        public bool Cursored { get; init; }
        public IEnumerable<Mail> Mail { get; init; } = new List<Mail>();
        public bool More { get; init; }
    }
}