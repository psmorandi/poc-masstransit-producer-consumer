namespace ApiProducer
{
    using System;
    using System.Collections.Generic;
    using GreenPipes;
    using MassTransit;
    using Serilog;
    using Serilog.Core;
    using SharedKernel;

    internal class Program
    {
        private static void Main(string[] args)
        {
            const int NumberOfItems = 1_000;

            Log.Logger = CreateLogger(args);

            Log.Information("Start of API producer");

            var busControl = CreateBusControl();

            busControl.Start();

            var sendEndpoint = busControl.GetSendEndpoint(new Uri(RabbitMqConfig.QueueAddress)).GetAwaiter().GetResult();

            for (var i = 0; i < NumberOfItems; i++) sendEndpoint.Send(new Command { InfoToProcess = $"Item_{i}" }).Wait();

            Log.Information($"Generated {NumberOfItems} items.");
            Log.Information("Press any key to exit");
            Console.ReadKey();

            busControl.Stop();
        }

        private static Logger CreateLogger(IReadOnlyList<string> args)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("Application", args.Count > 0 ? args[0] : "API_PRODUCER")
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: "{Timestamp:o} [{Level:u3}] ({Application}/{MachineName}/{ThreadId}) - {Message}{NewLine}{Exception}")
                .CreateLogger();
        }

        private static IBusControl CreateBusControl()
        {
            return Bus.Factory.CreateUsingRabbitMq(
                cfg =>
                {
                    cfg.Host(
                        new Uri(RabbitMqConfig.BaseAddress),
                        h =>
                        {
                            h.Username(RabbitMqConfig.User);
                            h.Password(RabbitMqConfig.Password);
                        });
                    cfg.UseRetry(r => r.Immediate(5));
                });
        }
    }
}