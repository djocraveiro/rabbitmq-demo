using BrokerContract.Messages;
using System;

namespace BrokerContract
{
    public interface IBrokerContext : IDisposable
    {
        /// <summary>Declares queue to receive messages.</summary>
        /// <param name="queueName">The queue name.</param>
        /// <param name="persistent">The message is persistent.</param>
        void DeclareQueue(string queueName, bool persistent = true);

        /// <summary>Declares a router to forward messages to binded queues.</summary>
        /// <param name="routerName">The router name.</param>
        /// <param name="persistent">The message is persistent.</param>
        void DeclareForwardRouter(string routerName, bool persistent = true);

        /// <summary>Binds the specified queue to receive messages from the specified router.</summary>
        /// <param name="queueName">The queue name.</param>
        /// <param name="routerName">The router name.</param>
        /// <param name="routingKey">The routing key.</param>
        void BindQueueToRouter(string queueName, string routerName, string routingKey = "");

        /// <summary>Sends the message to the specified queue.</summary>
        /// <param name="message">The message to send.</param>
        /// <param name="queueName">The queue name.</param>
        /// <param name="persistent">The message is persistent.</param>
        void Send<T>(T message, string queueName, bool persistent = true) where T : IMessage;

        /// <summary>Publishses the message to the specified router.</summary>
        /// <param name="message">The message to publish.</param>
        /// <param name="routerName">The router name.</param>
        /// <param name="routingKey">The routing key.</param>
        /// <param name="persistent">The message is persistent.</param>
        void Publish<T>(T message, string routerName, string routingKey = "", bool persistent = true) where T : IMessage;

        /// <summary>Consumes messages from the specified queue.</summary>
        /// <param name="queueName">The queue name.</param>
        /// <param name="onMessageReceived">The message received callback.
        ///     <remarks>Must return true if message consumed successufully.</remarks>
        /// </param>
        /// <param name="autoAck">The confirmation acknowledge is automatic.</param>
        void Consume<T>(string queueName, Func<T, bool> onMessageReceived, bool autoAck = false) where T : IMessage;
    }
}
