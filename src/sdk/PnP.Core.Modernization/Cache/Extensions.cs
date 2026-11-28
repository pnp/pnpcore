using Microsoft.Extensions.Caching.Distributed;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Cache
{
    /// <summary>
    /// Extensions methods to make it easier to work with the distributed cache
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts an object into a bytearray
        /// </summary>
        /// <param name="obj">Object to return as byte array</param>
        /// <returns>byte array</returns>
        public static byte[] ToByteArray(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            JsonSerializerOptions serializerOptions = GetSerializerOptions();

            var json = JsonSerializer.Serialize(obj, serializerOptions);
            return Encoding.UTF8.GetBytes(json);
        }

        private static JsonSerializerOptions GetSerializerOptions()
        {
            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new DictionaryIntConverter());
            serializerOptions.Converters.Add(new DictionaryGuidConverter());
            serializerOptions.Converters.Add(new DictionaryGuidTermDataConverter());
            return serializerOptions;
        }

        /// <summary>
        /// Converts a byte array to an object
        /// </summary>
        /// <typeparam name="T">Type of the object to return</typeparam>
        /// <param name="byteArray">Byte array</param>
        /// <returns>Object</returns>
        public static T FromByteArray<T>(this byte[] byteArray) where T : class
        {
            if (byteArray == null)
            {
                return default;
            }

            JsonSerializerOptions serializerOptions = GetSerializerOptions();

            var jsonString = Encoding.UTF8.GetString(byteArray);
            return JsonSerializer.Deserialize<T>(jsonString, serializerOptions);
        }

        /// <summary>
        /// Sets an object of type T in connected cache system
        /// </summary>
        /// <typeparam name="T">Type of the object to cache</typeparam>
        /// <param name="distributedCache">Connected cache system</param>
        /// <param name="key">Key of the object in the cache</param>
        /// <param name="value">Value to be cached</param>
        /// <param name="options">Caching options</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public async static Task SetAsync<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken)) where T: class
        {
            await distributedCache.SetAsync(key, value.ToByteArray(), options, token);
        }

        /// <summary>
        /// Sets an object of type T in connected cache system
        /// </summary>
        /// <typeparam name="T">Type of the object to cache</typeparam>
        /// <param name="distributedCache">Connected cache system</param>
        /// <param name="key">Key of the object in the cache</param>
        /// <param name="value">Value to be cached</param>
        /// <param name="options">Caching options</param>
        public static void Set<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions options) where T: class
        {
            distributedCache.Set(key, value.ToByteArray(), options);
        }

        /// <summary>
        /// Gets an object from the connected cache system
        /// </summary>
        /// <typeparam name="T">Type of the object to return from cache</typeparam>
        /// <param name="distributedCache">Connected cache system</param>
        /// <param name="key">Key of the object in the cache</param>
        /// <returns>Object of the type T</returns>
        public async static Task<T> GetAsync<T>(this IDistributedCache distributedCache, string key) where T : class
        {
            var result = await distributedCache.GetAsync(key);
            return result.FromByteArray<T>();
        }

        /// <summary>
        /// Gets an object from the connected cache system
        /// </summary>
        /// <typeparam name="T">Type of the object to return from cache</typeparam>
        /// <param name="distributedCache">Connected cache system</param>
        /// <param name="key">Key of the object in the cache</param>
        /// <returns>Object of the type T</returns>
        public static T Get<T>(this IDistributedCache distributedCache, string key) where T : class
        {
            var result = distributedCache.Get(key);
            return result.FromByteArray<T>();
        }

        /// <summary>
        /// Gets an object from the connected cache system. If not cached the object will be created
        /// </summary>
        /// <typeparam name="T">Type of the object to return from cache</typeparam>
        /// <param name="distributedCache">Connected cache system</param>
        /// <param name="key">Key of the object in the cache</param>
        /// <returns>Object of the type T</returns>
        public static T GetAndInitialize<T>(this IDistributedCache distributedCache, string key) where T : class, new()
        {
            var result = distributedCache.Get(key);
            var typedResult = result.FromByteArray<T>();

            if (typedResult == null)
            {
                return new T();
            }
            else
            {
                return typedResult;
            }
        }
    }
}
