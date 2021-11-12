using BrokerContract.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ReportService
{
    public class ReportBuilder : IReportBuilder<TemperatureReading, TemperatureReport>
    {
        #region Properties

        private readonly ConcurrentQueue<TemperatureReading> _queue;

        #endregion


        #region Constructors

        public ReportBuilder()
        {
            _queue = new ConcurrentQueue<TemperatureReading>();
        }

        #endregion


        #region Public Methods

        public void Add(TemperatureReading reading)
        {
            _queue.Enqueue(reading);
        }

        public TemperatureReport Build()
        {
            var reportDate = DateTime.UtcNow;
            var readingList = new List<TemperatureReading>();

            while (true)
            {
                if (!_queue.TryPeek(out TemperatureReading reading))
                {
                    continue;
                }

                if (reading.Timestamp <= reportDate)
                {
                    readingList.Add(reading);
                    _queue.TryDequeue(out _);
                }
                else
                {
                    break;
                }
            }

            return new TemperatureReport()
            {
                Timestamp = reportDate,
                MaxValue = readingList.Max(x => x.Value),
                AvgValue = readingList.Average(x => x.Value),
                MinValue = readingList.Min(x => x.Value),
                StartDate = readingList.Min(x => x.Timestamp),
                EndDate = readingList.Max(x => x.Timestamp)
            };
        }

        public void Clear()
        {
            _queue.Clear();
        }

        #endregion
    }
}
