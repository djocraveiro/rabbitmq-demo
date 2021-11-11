using Newtonsoft.Json;
using System;

namespace BrokerContract.Messages
{
    public sealed class TemperatureReading : IMessage
    {
        [JsonProperty("ts")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("val")]
        public float Value { get; set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
