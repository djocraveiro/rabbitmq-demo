using BrokerContract.Messages;

namespace AlertService.Rules
{
    public class VeryLowTemperatureRule : IAlertRule<TemperatureReading, AlertNotification>
    {
        private readonly int _threshold;

        public VeryLowTemperatureRule(int threshold)
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
                Type = NotificationType.Critical,
                Value = input.Value,
                Message = "It's freezing."
            };
        }
    }
}
