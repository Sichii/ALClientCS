using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AL.APIClient.Definitions
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServerId
    {
        None,
        Hardcore,
        I,
        II,
        III,
        PvP
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServerRegion
    {
        None,
        Asia,
        US,
        EU
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
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