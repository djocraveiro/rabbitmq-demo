using Newtonsoft.Json;
using System;

namespace BrokerContract.Messages
{
    public sealed class TemperatureReport : IMessage
    {
        [JsonProperty("ts")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("maxVal")]
        public float MaxValue { get; set; }

        [JsonProperty("avgVal")]
        public float AvgValue { get; set; }

        [JsonProperty("minVal")]
        public float MinValue { get; set; }

        [JsonProperty("sd")]
        public DateTime StartDate { get; set; }

        [JsonProperty("ed")]
        public DateTime EndDate { get; set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
