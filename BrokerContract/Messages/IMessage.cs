using System;

namespace BrokerContract.Messages
{
    public interface IMessage
    {
        DateTime Timestamp { get; set; }

        string ToJsonString();
    }
}
