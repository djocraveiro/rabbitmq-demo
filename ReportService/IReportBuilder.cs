namespace ReportService
{
    public interface IReportBuilder<TIn, TOut>
    {
        void Add(TIn input);

        TOut Build();

        void Clear();
    }
}
