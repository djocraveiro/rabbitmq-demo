using System;

namespace BrokerContract.Messages
{
    public class TemperatureReading : IMessage
    {
        public DateTime Timestamp { get; set; }
        public float Value { get; set; }

        public string ToJsonString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
