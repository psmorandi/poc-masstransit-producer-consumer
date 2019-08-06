namespace CommandConsumer
{
    using System;
    using GreenPipes;
    using MassTransit;
    using Serilog;
    using SharedKernel;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("Application", args.Length > 0 ? args[0] : "CONSUMER")
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: "{Timestamp:o} [{Level:u3}] ({Application}/{MachineName}/{ThreadId}) - {Message}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Start");

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
                    cfg.PrefetchCount = 1;
                    cfg.UseConcurrencyLimit(1);
                    cfg.ReceiveEndpoint(host, "QUEUE", e => { e.Consumer<CommandHandler>(); });
                });

            busControl.Start();

            Log.Information("Press any key to exit");
            Console.ReadKey();

            Log.Information($"Processed {ExecutionResume.Count} items.");

            busControl.Stop();
        }
    }
}