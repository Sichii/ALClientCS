using AL.APIClient.Interfaces;
using AL.SocketClient.Interfaces;

namespace AL.Client
{
    public class Priest : ALClient
    {
        public Priest(string characterName, IALAPIClient apiClient, IALSocketClient socketClient)
            : base(characterName, apiClient, socketClient) { }
    }
}