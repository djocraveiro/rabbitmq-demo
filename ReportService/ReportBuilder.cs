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

        public (TemperatureReport Report, int MessageCount) Build()
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

            var report = new TemperatureReport()
            {
                Timestamp = reportDate
            };

            if (readingList.Count > 0)
            {
                report.MaxValue = readingList.Max(x => x.Value);
                report.AvgValue = readingList.Average(x => x.Value);
                report.MinValue = readingList.Min(x => x.Value);
                report.StartDate = readingList.Min(x => x.Timestamp);
                report.EndDate = readingList.Max(x => x.Timestamp);
            }

            return (report, readingList.Count);
        }

        public void Clear()
        {
            _queue.Clear();
        }

        #endregion
    }
}
