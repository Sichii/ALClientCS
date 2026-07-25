using System.Runtime.Serialization;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.APIClient.Definitions
{
    [JsonConverter(typeof(TolerantStringEnumConverter))]
    public enum ServerId
    {
        None,
        Hardcore,
        I,
        II,
        III,
        PvP,
        Test,
        Dungeon
    }

    [JsonConverter(typeof(TolerantStringEnumConverter))]
    public enum ServerRegion
    {
        None,
        Asia,
        US,
        EU
    }

    [JsonConverter(typeof(TolerantStringEnumConverter))]
    internal enum APIMethod
    {
        [EnumMember(Value = "pull_mail")]
        PullMail,
        [EnumMember(Value = "read_mail")]
        ReadMail,
        [EnumMember(Value = "pull_merchants")]
        PullMerchants,
        [EnumMember(Value = "signup_or_login")]
        SignupOrLogin,
        [EnumMember(Value = "servers_and_characters")]
        ServersAndCharacters
    }
}