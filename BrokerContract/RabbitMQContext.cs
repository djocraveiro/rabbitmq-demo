using BrokerContract.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net.Mime;
using System.Text;

namespace BrokerContract
{
    internal class RabbitMQContext : IBrokerContext
    {
        #region Properties

        private readonly IConnection _connection;
        private readonly IModel _channel;

        #endregion


        #region Constructors

        public RabbitMQContext(string connectionString, string appId)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(connectionString),
                ClientProvidedName = appId
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            //consume only a message at a time
            _channel.BasicQos(0, 1, false);
        }

        #endregion


        #region Public Methods

        public void Dispose()
        {
            if (_channel.IsOpen)
            {
                _channel.Close();
                _channel.Dispose();
            }

            if (_connection.IsOpen)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }

        public void DeclareQueue(string queueName, bool persistent = true)
        {
            ThrowIfInvalidQueueName(queueName);
            ThrowIfChannelClosed();

            _channel.QueueDeclare(queue: queueName,
                                 durable: persistent,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        public void DeclareForwardRouter(string routerName, bool persistent = true)
        {
            ThrowIfInvalidRouterName(routerName);
            ThrowIfChannelClosed();

            _channel.ExchangeDeclare(routerName,
                                    ExchangeType.Fanout,
                                    durable: persistent);
        }

        public void BindQueueToRouter(string queueName, string routerName, string routingKey = "")
        {
            ThrowIfInvalidQueueName(queueName);
            ThrowIfInvalidRouterName(routerName);
            ThrowIfChannelClosed();

            _channel.QueueBind(queue: queueName,
                            exchange: routerName,
                            routingKey: routingKey);
        }

        public void Send<T>(T message, string queueName, bool persistent = true) where T : IMessage
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            ThrowIfInvalidQueueName(queueName);
            ThrowIfChannelClosed();

            byte[] messageBytes = Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(message));

            _channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: BuildMessageProperties(persistent),
                                 body: messageBytes);
        }

        public void Publish<T>(T message, string routerName, string routingKey = "", bool persistent = true) where T : IMessage
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            ThrowIfInvalidRouterName(routerName);
            ThrowIfChannelClosed();

            byte[] messageBytes = Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(message));

            _channel.BasicPublish(exchange: routerName,
                                 routingKey: routingKey,
                                 basicProperties: BuildMessageProperties(persistent),
                                 body: messageBytes);
        }

        public void Consume<T>(string queueName, Func<T, bool> onMessageReceived, bool autoAck = false) where T : IMessage
        {
            ThrowIfInvalidQueueName(queueName);

            if (onMessageReceived == null)
            {
                throw new ArgumentNullException(nameof(onMessageReceived));
            }

            ThrowIfChannelClosed();

            _channel.BasicConsume(queue: queueName,
                                 autoAck: autoAck,
                                 consumer: BuildConsumer<T>(onMessageReceived));
        }

        #endregion


        #region Private Methods

        private void ThrowIfChannelClosed()
        {
            if (!_channel.IsOpen)
            {
                throw new InvalidOperationException("context is disconnect");
            }
        }

        private void ThrowIfInvalidQueueName(string queueName)
        {
            if (string.IsNullOrEmpty(queueName.Trim()))
            {
                throw new ArgumentException("invalid queue name");
            }
        }

        private void ThrowIfInvalidRouterName(string routerName)
        {
            if (string.IsNullOrEmpty(routerName.Trim()))
            {
                throw new ArgumentException("invalid router name");
            }
        }

        private IBasicProperties BuildMessageProperties(bool persistent)
        {
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = persistent;
            properties.ContentType = MediaTypeNames.Application.Json;
            properties.ContentEncoding = Encoding.UTF8.WebName;

            return properties;
        }

        private EventingBasicConsumer BuildConsumer<T>(Func<T, bool> onMessageReceived)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, eventArgs) =>
            {
                byte[] messageBytes = eventArgs.Body.ToArray();
                var message = JsonConvert.DeserializeObject<T>(
                    Encoding.UTF8.GetString(messageBytes));

                bool consumed = onMessageReceived(message);

                if (consumed)
                {
                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                }
                else
                {
                    _channel.BasicReject(eventArgs.DeliveryTag, true);
                }
            };

            return consumer;
        }

        #endregion
    }
}
