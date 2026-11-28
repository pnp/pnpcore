using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace PnP.Core.Modernization.Cache
{
    /// <summary>
    /// MemoryDistributedCache options class
    /// </summary>
    /// <summary>
    /// MemoryDistributedCache options class
    /// </summary>
    public class CacheOptions : MemoryDistributedCacheOptions, IOptions<MemoryDistributedCacheOptions>, ICacheOptions
    {
        public CacheOptions()
        {
            this.EntryOptions = new DistributedCacheEntryOptions() { };
        }

        MemoryDistributedCacheOptions IOptions<MemoryDistributedCacheOptions>.Value => this;

        /// <summary>
        /// Prefix value that will be prepended to the provided key value
        /// </summary>
        public string KeyPrefix { get; set; }

        /// <summary>
        /// Default cache entry configuration, will be used to save items to the cache
        /// </summary>
        public DistributedCacheEntryOptions EntryOptions { get; set; }

        /// <summary>
        /// Returns the key value to use by the caching system, in this case this will mean prepending the KeyPrefix
        /// </summary>
        /// <param name="key">Provided key</param>
        /// <returns>Key to use by the caching system</returns>
        public string GetKey(string key)
        {
            if (!string.IsNullOrEmpty(KeyPrefix))
            {
                return $"{KeyPrefix}|{key}";
            }
            else
            {
                return key;
            }
        }

    }
}
