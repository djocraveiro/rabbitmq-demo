using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace BrokerContract.Messages
{
    public enum NotificationType
    {
        Info = 1,
        Warning = 2,
        Critical = 3
    }

    public sealed class AlertNotification : IMessage
    {
        [JsonProperty("ts")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("type")]
        public NotificationType Type { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }

        public string ToJsonString()
        {
            var json = JObject.FromObject(this);
            json["typeName"] = Type.ToString();

            return JsonConvert.SerializeObject(json);
        }
    }
}
