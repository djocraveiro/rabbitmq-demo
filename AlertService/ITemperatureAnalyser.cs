namespace AlertService
{
    public interface ITemperatureAnalyser<TIn, TOut>
    {
        TOut Analyse(TIn input);
    }
}
