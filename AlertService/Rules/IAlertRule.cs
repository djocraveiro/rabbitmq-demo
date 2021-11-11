namespace AlertService.Rules
{
    public interface IAlertRule<TIn, TOut>
    {
        bool Analyse(TIn input);

        TOut BuildOutput(TIn input);
    }
}
