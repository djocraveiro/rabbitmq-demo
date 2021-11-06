namespace BrokerContract
{
    public static class BrokerContextFactory
    {
        public static IBrokerContext CreateContext(string connectionString, string appId)
        {
            return new RabbitMQContext(connectionString, appId);
        }
    }
}
