namespace SharedKernel
{
    public class RabbitMqConfig
    {
        public static string BaseAddress = "rabbitmq://localhost/WORK";
        public static string Password = "adm123";
        public static string QueueName = "QUEUE";
        public static string QueueAddress = $"{BaseAddress}/{QueueName}";
        public static string User = "pocs";
    }
}