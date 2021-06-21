using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.Json.Converters;
using Newtonsoft.Json;

// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace AL.SocketClient.Receive
{
    [JsonConverter(typeof(GameResponseDataConverter))]
    public record GameResponseData : IGoldReceivedResponse, ISkillNameResponse, IBankOpxResponse, IBuySuccessResponse,
        ICooldownResponse, ICraftResponse, IDefeatedByAMonsterResponse, IGoldSentResponse, IItemSentResponse,
        ISeashellSuccessResponse, ITooFarResponse
    {
        [JsonProperty("name")]
        private readonly string _name;
        public int Amount { get; init; }
        [JsonIgnore]
        public bool ContainsData { get; init; }
        public float CooldownMS { get; init; }
        public int Cost { get; init; }
        public int Distance { get; init; }
        public string Place { get; init; }
        public int Quantity { get; init; }
        public string Reason { get; init; }
        public GameResponseType ResponseType { get; init; }
        public string SentItemName { get; init; }
        public int SlotNum { get; init; }
        public string Suffix { get; init; }
        public string TargetID { get; init; }
        public int XPLost { get; init; }
        public string From => _name;
        public string ItemName => _name;
        public string MonsterName => _name;
        public string NameInBank => _name;
        public string SentTo => _name;
        public string SkillName => _name;
        string INameResponse.Name => _name;
    }
}