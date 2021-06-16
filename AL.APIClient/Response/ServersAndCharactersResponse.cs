using AL.APIClient.Model;

namespace AL.APIClient.Response
{
    public record ServersAndCharactersResponse
    {
        public Character[] Characters { get; init; }
        public int Mail { get; init; }
        public Server[] Servers { get; init; }
    }
}