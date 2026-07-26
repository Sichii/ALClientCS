#region
using System.Runtime.Serialization;
using StjConverters = AL.Core.Json.SystemTextJson;
using StjJson = System.Text.Json.Serialization;
#endregion

namespace AL.APIClient.Definitions;

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
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

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum ServerRegion
{
    None,
    Asia,
    US,
    EU
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
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