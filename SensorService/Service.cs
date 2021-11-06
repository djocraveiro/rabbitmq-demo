using BrokerContract;
using BrokerContract.Messages;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SensorService
{
    internal class Service
    {
        #region Properties

        private readonly string _connectionString;
        private readonly string _appId;
        private readonly string _publichTo;
        private CancellationTokenSource _tokenSrc;

        private readonly Random _random;

        #endregion


        #region Constructors

        public Service(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("RabbitMQ");
            _appId = config.GetValue<string>("AppId");
            _publichTo = config.GetValue<string>("PublishTo");

            _random = new Random();
        }

        #endregion


        #region Public Methods

        public Task Run()
        {
            _tokenSrc = new CancellationTokenSource();

            return Task.Run(async () =>
            {
                IBrokerContext brokerContext = null;

                try
                {
                    brokerContext = BrokerContextFactory.CreateContext(_connectionString, _appId);
                    await Run(brokerContext, _tokenSrc.Token);
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                }
                finally
                {
                    brokerContext?.Dispose();
                }
            },
            _tokenSrc.Token);
        }

        public void Stop()
        {
            if (_tokenSrc != null && !_tokenSrc.IsCancellationRequested)
            {
                _tokenSrc.Cancel();
                _tokenSrc.Dispose();
            }
        }

        #endregion


        #region Private Methods

        private async Task Run(IBrokerContext brokerContext, CancellationToken cancelToken)
        {
            brokerContext.DeclareQueue(_publichTo);
            brokerContext.DeclareForwardRouter(_publichTo);
            brokerContext.BindQueueToRouter(queueName: _publichTo, routerName: _publichTo);

            while (!cancelToken.IsCancellationRequested)
            {
                var message = ReadTemperature();
                brokerContext.Publish(message, _publichTo);

                WriteLog(message.ToJsonString());

                await Task.Delay(3000);
            }
        }

        private TemperatureReading ReadTemperature()
        {
            return new TemperatureReading()
            {
                Timestamp = DateTime.UtcNow,
                Value = _random.Next(-5, 50)
            };
        }

        private void WriteLog(object value)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("o")} - {value}");
        }

        #endregion
    }
}
