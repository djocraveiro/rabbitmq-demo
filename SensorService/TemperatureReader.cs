using Microsoft.Extensions.Configuration;
using System;

namespace SensorService
{
    public interface ITemperatureReader<T>
    {
        T ReadNext();
    }


    public class TemperatureReader : ITemperatureReader<float>
    {
        #region Properties

        private readonly Random _random;
        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly int _maxVariation;
        private float _lastValue;

        #endregion


        #region Constructors

        public TemperatureReader(IConfiguration config)
            : this(config.GetValue<int>("MinValue", -10),
                  config.GetValue<int>("MaxValue", 55),
                  config.GetValue<int>("MaxVariation", 5))
        {
        }

        public TemperatureReader(int minValue, int maxValue, int maxVariation)
        {
            _random = new Random();
            _minValue = minValue;
            _maxValue = maxValue;
            _maxVariation = maxVariation;
            _lastValue = float.MinValue;
        }

        #endregion


        #region Public Methods

        public float ReadNext()
        {
            if (_lastValue == float.MinValue)
            {
                _lastValue = _random.Next(_minValue, _maxValue);
            }
            else
            {
                int multiplier = (DateTime.UtcNow.Millisecond % 2 == 0 ? 1 : -1);
                int delta = _random.Next(1, _maxVariation);
                _lastValue += (multiplier * delta);
            }

            _lastValue = SanitizeValue(_lastValue);
            return _lastValue;
        }

        private float SanitizeValue(float value)
        {
            if (value > _maxValue)
            {
                return _maxValue;
            }
            else if (value < _minValue)
            {
                return _minValue;
            }

            return value;
        }
        
        #endregion
    }
}
