﻿using AL.APIClient.Definitions;
using Newtonsoft.Json;

namespace AL.APIClient.Model
{
    public record Server
    {
        [JsonProperty("addr")]
        public string IPAddress { get; init; }
        public string Key { get; init; }
        public ServerId Name { get; init; }
        public int Players { get; init; }
        public int Port { get; init; }
        public ServerRegion Region { get; init; }
    }
}