using BrokerContract;
using BrokerContract.Messages;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SensorService
{
    public class Service
    {
        #region Properties

        const string publishRouter = "sensor-data";

        private readonly string _connectionString;
        private readonly string _appId;
        private readonly int _readInterval;
        private CancellationTokenSource _tokenSrc;

        private readonly ITemperatureReader<float> _temperatureReader;

        #endregion


        #region Constructors

        public Service(IConfiguration config, ITemperatureReader<float> temperatureReader)
        {
            _connectionString = config.GetConnectionString("RabbitMQ");
            _appId = config.GetValue<string>("AppId");
            _readInterval = config.GetValue<int>("ReadInterval", 3000);

            _temperatureReader = temperatureReader;
        }

        #endregion


        #region Public Methods

        public Task Start()
        {
            _tokenSrc = new CancellationTokenSource();

            return Task.Run(async () =>
            {
                IBrokerContext brokerContext = null;

                try
                {
                    Console.WriteLine($"AppId:{_appId}");

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

                    _tokenSrc.Dispose();
                    _tokenSrc = null;
                }
            },
            _tokenSrc.Token);
        }

        public void Stop()
        {
            if (_tokenSrc != null && !_tokenSrc.IsCancellationRequested)
            {
                _tokenSrc.Cancel();
            }
        }

        #endregion


        #region Private Methods

        private async Task Run(IBrokerContext brokerContext, CancellationToken cancelToken)
        {
            brokerContext.DeclareForwardRouter(publishRouter);

            while (!cancelToken.IsCancellationRequested)
            {
                var reading = ReadTemperature();
                brokerContext.Publish(reading, publishRouter);

                WriteLog(reading.ToJsonString());

                await Task.Delay(_readInterval);
            }
        }

        private TemperatureReading ReadTemperature()
        {
            return new TemperatureReading()
            {
                Timestamp = DateTime.UtcNow,
                Value = _temperatureReader.ReadNext()
            };
        }

        private void WriteLog(object value)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("o")} - {value}");
        }

        #endregion
    }
}
