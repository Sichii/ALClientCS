using System.Collections.Generic;
using AL.APIClient.Model;

namespace AL.APIClient.Response
{
    public record ServersAndCharactersResponse
    {
        public IReadOnlyList<Character> Characters { get; init; }
        public int Mail { get; init; }
        public IReadOnlyList<Server> Servers { get; init; }
    }
}