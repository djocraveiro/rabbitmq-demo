using AlertService.Configuration;
using BrokerContract;
using BrokerContract.Messages;
using Microsoft.Extensions.Configuration;
using System;

namespace AlertService
{
    public class Service
    {
        #region Properties

        const string publishRouter = "alert-notification";
        const string consumeQueue = "alert-sensor-data";
        const string consumeRouter = "sensor-data";

        private readonly string _connectionString;
        private readonly string _appId;
        private IBrokerContext _brokerContext;
        private readonly ITemperatureAnalyser<TemperatureReading, AlertNotification> _analyser;

        #endregion


        #region Constructors

        public Service(IConfiguration config,
            ITemperatureAnalyser<TemperatureReading, AlertNotification> analyser)
        {
            _connectionString = config.GetConnectionString("RabbitMQ");
            _appId = config.GetValue<string>("AppId");

            _brokerContext = null;
            _analyser = analyser;
        }

        #endregion


        #region Public Methods

        public void Start()
        {
            if (_brokerContext != null)
            {
                throw new InvalidOperationException("already started");
            }

            try
            {
                _brokerContext = BrokerContextFactory.CreateContext(_connectionString, _appId);

                _brokerContext.DeclareForwardRouter(publishRouter);

                _brokerContext.DeclareQueue(consumeQueue);
                _brokerContext.DeclareForwardRouter(consumeRouter);
                _brokerContext.BindQueueToRouter(queueName: consumeQueue, routerName: consumeRouter);

                _brokerContext.Consume<TemperatureReading>(consumeQueue, OnMessageReceived);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        public void Stop()
        {
            _brokerContext?.Dispose();
            _brokerContext = null;
        }

        #endregion


        #region Private Methods

        private bool OnMessageReceived(TemperatureReading reading)
        {
            try
            {
                var notification = _analyser.Analyse(reading);

                if (notification != null)
                {
                    _brokerContext.Publish(notification, publishRouter);
                    WriteLog(notification.ToJsonString());
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                return false;
            }

            return true;
        }

        private void WriteLog(object value)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("o")} - {value}");
        }

        #endregion
    }
}
