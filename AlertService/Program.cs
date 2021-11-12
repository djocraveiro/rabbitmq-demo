using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace AlertService
{
    public static class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = ReadConfiguration(args);

            var service = new Service(config, new TemperatureAnalyser(config));
            service.Start();

            WaitForExitCommand();

            service.Stop();
            Console.WriteLine("Stopping...");
        }

        public static void WaitForExitCommand()
        {
            while (true)
            {
                Console.WriteLine("Press 'e' to exit...");
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.E)
                {
                    break;
                }
            }
        }

        private static IConfiguration ReadConfiguration(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

            if (args != null)
                builder.AddCommandLine(args);

            return builder.Build();
        }
    }
}
