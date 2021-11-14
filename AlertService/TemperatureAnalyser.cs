using AlertService.Configuration;
using AlertService.Rules;
using BrokerContract.Messages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace AlertService
{

    public class TemperatureAnalyser : ITemperatureAnalyser<TemperatureReading, AlertNotification>
    {
        #region Properties

        private readonly IList<IAlertRule<TemperatureReading, AlertNotification>> _rules;
        private AlertNotification _lastNotification;

        #endregion


        #region Constructors

        public TemperatureAnalyser(IConfiguration config)
        {
            var warningThreshold = config.GetSection("WarningThreshold")
                                    .Get<TemperatureThreshold>();
            var criticalThreshold = config.GetSection("CritialThreshold")
                                    .Get<TemperatureThreshold>();

            _rules = new List<IAlertRule<TemperatureReading, AlertNotification>>()
            {
                new VeryHighTemperatureRule(criticalThreshold.High),
                new VeryLowTemperatureRule(criticalThreshold.Low),
                new HighTemperatureRule(warningThreshold.High),
                new LowTemperatureRule(warningThreshold.Low)
            };

            _lastNotification = null;
        }

        #endregion


        #region Public Methods

        public AlertNotification Analyse(TemperatureReading input)
        {
            AlertNotification newNotification = null;

            foreach (var rule in _rules)
            {
                bool triggerRule = rule.Analyse(input);
                if (triggerRule)
                {
                    newNotification = rule.BuildOutput(input);
                    break;
                }
            }

            if (newNotification != null && HasChanged(newNotification.Type))
            {
                _lastNotification = newNotification;
                return _lastNotification;
            }
            else if (newNotification == null && _lastNotification != null
                && HasChanged(NotificationType.Info))
            {
                _lastNotification = new AlertNotification()
                {
                    Timestamp = input.Timestamp,
                    Type = NotificationType.Info,
                    Value = input.Value,
                    Message = "temperature is normal."
                };

                return _lastNotification;
            }

            return null;
        }

        #endregion


        #region Private Methods

        private bool HasChanged(NotificationType newType)
        {
            if (_lastNotification == null)
            {
                return true;
            }

            return newType != _lastNotification.Type;
        }

        #endregion
    }
}
