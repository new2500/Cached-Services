using System;
using System.Threading;
using System.Threading.Tasks;
using Cache.Configuration;

namespace Cache
{
    //Generic Caching Mechanism
    interface IAsyncCacheRepository
    {
        //Set by Key
        //key: Cache key
        //value: cached obejct
        //expiration:expiration to use for cache
        //cancelToken:Cancellation Token
        Task SetAsync<T>(string key, T value, CacheExpiration expiration, CancellationToken cancelToken = default(CancellationToken));

        //Get by key
        //T: Type of Chache object
        //key: Cache key
        //cancelToken: Cancellation Token
        Task<T> GetAsync<T>(string key, CancellationToken cancelToken = default(CancellationToken));

        //Get or Set by Key: If key not found, loader is invloked and the result is cached.
        //T: Type of cached object
        //loader: Delegate to invoke if cached item is not found
        //expiration: Time expiration to use if object is loaded and cached
        //cancelToken: Cancellation Token
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> loader, CacheExpiration expiration, CancellationToken cancelToken = default(CancellationToken));

        //Remove by key
        //key:Cache key
        //cancelToken:Cancellation Token
        Task RemoveAsync(string key, CancellationToken cancelToken = default(CancellationToken));


        //Clear all cache
        //cancelToken:Cancellation Token
        Task ClearAllAsync(CancellationToken cancelToken = default(CancellationToken));

    }
}
