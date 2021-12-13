namespace ReportService
{
    public interface IReportBuilder<TIn, TOut>
    {
        void Add(TIn input);

        (TOut Report, int MessageCount) Build();

        void Clear();
    }
}
