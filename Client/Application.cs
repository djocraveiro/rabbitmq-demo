using BrokerContract;
using BrokerContract.Messages;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;

namespace Client
{
    public class Application
    {
        #region Properties

        const string alertRouter = "alert-notification";
        const string alertQueue = "alert-notification";
        const string reportRouter = "report";
        const string reportQueue = "report";

        private readonly string _connectionString;
        private readonly string _appId;
        private IBrokerContext _brokerContext;

        #endregion


        #region Constructors

        public Application(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("RabbitMQ");
            _appId = config.GetValue<string>("AppId");

            _brokerContext = null;
        }

        #endregion


        #region Public Methods

        public void Run()
        {
            if (_brokerContext != null)
            {
                throw new InvalidOperationException("already started");
            }

            try
            {
                Console.WriteLine($"AppId:{_appId}");

                _brokerContext = BrokerContextFactory.CreateContext(_connectionString, _appId);

                _brokerContext.DeclareQueue(alertQueue);
                _brokerContext.DeclareForwardRouter(alertRouter);
                _brokerContext.BindQueueToRouter(queueName: alertQueue, routerName: alertRouter);
                _brokerContext.Consume<AlertNotification>(alertQueue, OnAlertMessageReceived);

                _brokerContext.DeclareQueue(reportQueue);
                _brokerContext.DeclareForwardRouter(reportRouter);
                _brokerContext.BindQueueToRouter(queueName: reportQueue, routerName: reportRouter);
                _brokerContext.Consume<TemperatureReport>(reportQueue, OnReportMessageReceived);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        public void Finish()
        {
            if (_brokerContext != null)
            {
                _brokerContext?.Dispose();
                _brokerContext = null;
            }
        }

        #endregion


        #region Private Methods

        private bool OnAlertMessageReceived(AlertNotification notification)
        {
            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = GetNotificationColor(notification);
            var builder = new StringBuilder();
            builder.Append($"{notification.Timestamp} | {notification.Type}");
            builder.AppendLine($" | {notification.Value}°C - {notification.Message}");
            Console.WriteLine(builder.ToString());

            Console.ForegroundColor = originalColor;
            return true;
        }

        private bool OnReportMessageReceived(TemperatureReport report)
        {
            var builder = new StringBuilder();
            builder.AppendLine("======= Temperature. Report =====================");
            builder.AppendLine($"[{report.StartDate.ToLocalTime().ToString("s")} - {report.EndDate.ToLocalTime().ToString("s")}]");
            builder.AppendLine($"Max:{report.MaxValue} \t\tAvg:{report.AvgValue} \t\tMin:{report.MinValue}");
            builder.AppendLine("=================================================");

            Console.WriteLine(builder.ToString());
            return true;
        }

        private void WriteLog(object value)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("o")} - {value}");
        }

        private static ConsoleColor GetNotificationColor(AlertNotification notification)
        {
            switch (notification.Type)
            {
                case NotificationType.Critical:
                    return Console.ForegroundColor = ConsoleColor.Red;

                case NotificationType.Warning:
                    return Console.ForegroundColor = ConsoleColor.Yellow;

                default:
                case NotificationType.Info:
                    return Console.ForegroundColor;
            }
        }

        #endregion
    }
}
