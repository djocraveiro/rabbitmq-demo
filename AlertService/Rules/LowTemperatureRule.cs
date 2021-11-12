using BrokerContract.Messages;

namespace AlertService.Rules
{
    public class LowTemperatureRule : IAlertRule<TemperatureReading, AlertNotification>
    {
        private readonly int _threshold;

        public LowTemperatureRule(int threshold)
        {
            _threshold = threshold;
        }

        public bool Analyse(TemperatureReading input)
        {
            return (input.Value <= _threshold);
        }

        public AlertNotification BuildOutput(TemperatureReading input)
        {
            return new AlertNotification()
            {
                Timestamp = input.Timestamp,
                Type = NotificationType.Warning,
                Value = input.Value,
                Message = "It's cold."
            };
        }
    }
}
