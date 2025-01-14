using StackExchange.Redis;

namespace api.Cache
{

    public class ConnectionHelper
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection;
        static ConnectionHelper()
        {
            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                string redisConnectionString = "redis-16206.c289.us-west-1-2.ec2.redns.redis-cloud.com:16206,password=2AAIcTfnhSpo4S8hjmw5QFiKDb5JnS8T,abortConnect=false";

                return ConnectionMultiplexer.Connect(redisConnectionString);
            });
        }
        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}

