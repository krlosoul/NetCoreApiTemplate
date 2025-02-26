namespace Infrastructure.Services
{
    using Core.Interfaces.Services;
    using StackExchange.Redis;
    using Newtonsoft.Json;
    using Core.Dtos.SecretsDto;

    public class RedisCacheService : IRedisCacheService
    {
        #region Properties 
        private readonly IDatabase _db;
        private readonly RedisSecretDto? _redisDto;
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly long _maxMemoryBytes;
        #endregion

        public RedisCacheService(RedisSecretDto redisSecretDto)
        {
            _redisDto = redisSecretDto;
            ConfigurationOptions configuration = new(){
                EndPoints = { _redisDto.ConnectionString! },
                User = _redisDto.User,
                Password = _redisDto.Password,
                AllowAdmin = _redisDto.AllowAdmin,
            };
            _redisConnection = ConnectionMultiplexer.Connect(configuration);
            _maxMemoryBytes = _redisDto.MaxLimitBytes * 1024 * 1024;
            _db = _redisConnection.GetDatabase(_redisDto.DataBase);
        }

        public async Task<bool> SetCacheAsync<T>(string key, T data, TimeSpan? expiration = null)
        {
            try{
                var serializedValue = JsonConvert.SerializeObject(data);
                return await _db.StringSetAsync(key, serializedValue, expiration);
            }
            catch (RedisException re)
            {
                throw new RedisException($"Error set cache to Redis: {re.Message}", re);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
            }
        }

        public async Task<T?> GetCacheAsync<T>(string key)
        {
            try{
                var value = await _db.StringGetAsync(key);
                return value.IsNullOrEmpty  ? default : JsonConvert.DeserializeObject<T>(value!);
            }
            catch (RedisException re)
            {
                throw new RedisException($"Error get cache to Redis: {re.Message}", re);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteCacheAsync(string key)
        {
            try{
                return await _db.KeyDeleteAsync(key);
            }
            catch (RedisException re)
            {
                throw new RedisException($"Error delete cache to Redis: {re.Message}", re);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
            }
        }

        public async Task<bool> ExistsCacheAsync(string key)
        {
            try{
                return await _db.KeyExistsAsync(key);
            }
            catch (RedisException re)
            {
                throw new RedisException($"Error exits cache to Redis: {re.Message}", re);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
            }
        }

        public async Task<string> CheckMemoryUsageAsync()
        {
            try{
                var server = _redisConnection.GetServer(_redisConnection.GetEndPoints()[0]);
                if(server.IsConnected)
                {
                    var info = await server.InfoAsync("memory");
                    var usedMemoryBytes = info.SelectMany(group => group)
                                                .Where(pair => pair.Key == "used_memory")
                                                .Select(pair => Convert.ToInt64(pair.Value))
                                                .FirstOrDefault();
                    double memoryUsagePercentage = (double)usedMemoryBytes / _maxMemoryBytes * 100;
                    if(memoryUsagePercentage > _redisDto?.MemoryUsagePercentage){
                        return $"Memory usage exceeds 80%. Used memory: {usedMemoryBytes / (1024 * 1024)} MB out of {_maxMemoryBytes} MB ({memoryUsagePercentage:0.00}% of the limit).";
                    }
                }
                return string.Empty;
            }
            catch (RedisException re)
            {
                throw new RedisException($"Error check memory cache to Redis: {re.Message}", re);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
            }
        }
    }
}