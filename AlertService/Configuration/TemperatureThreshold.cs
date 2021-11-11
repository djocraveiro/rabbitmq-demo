namespace AlertService.Configuration
{
    public record TemperatureThreshold
    {
        public int Low { get; init; }
        public int High { get; set; }
    }
}