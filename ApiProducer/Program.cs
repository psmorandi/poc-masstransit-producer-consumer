namespace ApiProducer
{
    using System;
    using System.Threading;
    using GreenPipes;
    using MassTransit;
    using Serilog;
    using SharedKernel;

    internal class Program
    {
        private static void Main(string[] args)
        {
            const int NUMBER_OF_ITEMS = 1_000;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("Application", args.Length > 0 ? args[0] : "API_PRODUCER")
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: "{Timestamp:o} [{Level:u3}] ({Application}/{MachineName}/{ThreadId}) - {Message}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Start of API producer");

            var busControl = Bus.Factory.CreateUsingRabbitMq(
                cfg =>
                {
                    var host = cfg.Host(
                        new Uri("rabbitmq://localhost/WORK"),
                        h =>
                        {
                            h.Username("c4m-user");
                            h.Password("adm123");
                        });
                    cfg.UseRetry(r => r.Immediate(5));
                });

            busControl.Start();

            var address = "rabbitmq://localhost/WORK/QUEUE";
            var sendEndpoint = busControl.GetSendEndpoint(new Uri(address)).GetAwaiter().GetResult();

            for (var i = 0; i < NUMBER_OF_ITEMS; i++)
            {
                sendEndpoint.Send(new Command { InfoToProcess = $"Item_{i}" }).Wait();
            }

            Log.Information($"Generated {NUMBER_OF_ITEMS} items.");
            Log.Information("Press any key to exit");
            Console.ReadKey();

            busControl.Stop();
        }
    }
}