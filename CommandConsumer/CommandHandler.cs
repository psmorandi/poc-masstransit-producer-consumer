namespace CommandConsumer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MassTransit;
    using Serilog;
    using SharedKernel;

    internal class CommandHandler : IConsumer<Command>
    {
        private static readonly Random Random = new Random();

        Task IConsumer<Command>.Consume(ConsumeContext<Command> context)
        {
            Thread.Sleep(Random.Next(200, 300));//simulates some medium process

            Log.Information($"{context.Message.InfoToProcess} -> Processed!");

            ExecutionResume.IncrementCount();

            return Task.CompletedTask;
        }
    }
}