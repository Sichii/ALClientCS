﻿using System.Collections.Generic;
using AL.SocketClient.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.Receive
{
    [JsonConverter(typeof(CharacterConverter<CharacterData>))]
    public record CharacterData : Character
    {
        [JsonProperty("hitchhikers")]
        public IReadOnlyList<JArray> ExtraEvents { get; init; }
    }
}