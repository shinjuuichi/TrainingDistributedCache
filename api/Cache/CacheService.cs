
using Newtonsoft.Json;
using StackExchange.Redis;


namespace api.Cache
{
    public class CacheService : ICacheService
    {
        private IDatabase _db;

        public CacheService()
        {
            _db = ConnectionHelper.Connection.GetDatabase();
        }

        T ICacheService.GetData<T>(string key)
        {
            var value = _db.StringGet(key);
            if (!value.IsNull)
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            return default;
        }

      public  object RemoveData(string key)
        {
            bool isExist =_db.KeyExists(key);

            if (isExist)
            {
                return _db.KeyDelete(key);
            }
            return false;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            TimeSpan expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
            var isSet =_db.StringSet(key, JsonConvert.SerializeObject(value), expirationTime - DateTimeOffset.Now);
            return isSet;
        }
    }
}
