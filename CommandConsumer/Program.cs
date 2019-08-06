namespace CommandConsumer
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
            Log.Logger = CreateLogger(args);

            Log.Information("Start");

            var busControl = CreateBusControl();

            busControl.Start();

            Log.Information("Press any key to exit");
            Console.ReadKey();

            Log.Information($"Processed {ExecutionResume.Count} items.");

            busControl.Stop();
        }

        private static Logger CreateLogger(IReadOnlyList<string> args)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("Application", args.Count > 0 ? args[0] : "CONSUMER_1")
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
                    var host = cfg.Host(
                        new Uri(RabbitMqConfig.BaseAddress),
                        h =>
                        {
                            h.Username(RabbitMqConfig.User);
                            h.Password(RabbitMqConfig.Password);
                        });
                    cfg.UseRetry(r => r.Immediate(5));
                    cfg.PrefetchCount = 1;
                    cfg.UseConcurrencyLimit(1);
                    cfg.ReceiveEndpoint(host, RabbitMqConfig.QueueName, e => { e.Consumer<CommandHandler>(); });
                });
        }
    }
}