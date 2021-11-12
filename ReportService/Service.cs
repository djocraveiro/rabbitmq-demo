using BrokerContract;
using BrokerContract.Messages;
using Microsoft.Extensions.Configuration;
using System;
using System.Timers;

namespace ReportService
{
    public class Service
    {
        #region Properties

        const string publishRouter = "report";
        const string consumeQueue = "report-sensor-data";
        const string consumeRouter = "sensor-data";

        private readonly string _connectionString;
        private readonly string _appId;
        private IBrokerContext _brokerContext;

        private readonly IReportBuilder<TemperatureReading, TemperatureReport> _reportBuilder;
        private Timer _timer;

        #endregion


        #region Constructors

        public Service(IConfiguration config,
            IReportBuilder<TemperatureReading, TemperatureReport> reportBuilder)
        {
            _connectionString = config.GetConnectionString("RabbitMQ");
            _appId = config.GetValue<string>("AppId");

            _brokerContext = null;
            _reportBuilder = reportBuilder;

            var reportInterval = config.GetValue<int>("ReportInterval");
            _timer = new Timer(reportInterval);
            _timer.AutoReset = true;
            _timer.Elapsed += OnGenerateReport;
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
                Console.WriteLine($"AppId:{_appId}");

                _brokerContext = BrokerContextFactory.CreateContext(_connectionString, _appId);

                _brokerContext.DeclareForwardRouter(publishRouter);

                _brokerContext.DeclareQueue(consumeQueue);
                _brokerContext.DeclareForwardRouter(consumeRouter);
                _brokerContext.BindQueueToRouter(queueName: consumeQueue, routerName: consumeRouter);

                _brokerContext.Consume<TemperatureReading>(consumeQueue, OnMessageReceived);

                _timer.Start();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        public void Stop()
        {
            if (_brokerContext != null)
            {
                _brokerContext?.Dispose();
                _brokerContext = null;
            }

            _reportBuilder.Clear();
            _timer.Stop();
        }

        #endregion


        #region Private Methods

        private bool OnMessageReceived(TemperatureReading reading)
        {
            try
            {
                _reportBuilder.Add(reading);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                return false;
            }

            return true;
        }

        private void OnGenerateReport(object sender, ElapsedEventArgs e)
        {
            try
            {
                WriteLog("Generating report...");

                var report = _reportBuilder.Build();

                _brokerContext.Publish(report, publishRouter);
                WriteLog(report.ToJsonString());
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        private void WriteLog(object value)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("o")} - {value}");
        }

        #endregion
    }
}
