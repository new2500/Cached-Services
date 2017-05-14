using System;
using System.Collections.Generic;
using Cache.Configuration;
using CacheRepository.Threading;
using System.Threading;
using System.Threading.Tasks;


namespace Cache.Imp
{
    public abstract class AsyncCacheRepositoryBase : IAsyncCacheRepository, ICacheRepository
    {
        private readonly KeyedSemaphoreSlim _keyedSemaphoreSlim = new KeyedSemaphoreSlim();

        private readonly ICacheSetting _cacheSettings;

        protected AsyncCacheRepositoryBase(ICacheSetting cacheSettings)
        {
            _cacheSettings = cacheSettings;
        }


        //Method
        public async Task<T> GetAsync<T>(string key, CancellationToken cancelToken)
        {
            var result = await TryGetAsync<T>(key, cancelToken).ConfigureAwait(false);
            return result.Item2;
        }

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> loader, CancellationToken cancelToken)
        {
            return GetOrSetAsync(key, loader, null, null, cancelToken);
        }

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> loader, DateTime expiration, CancellationToken cancelToken)
        {
            return GetOrSetAsync(key, loader, v => expiration, null, cancelToken);
        }

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> loader, Func<T, DateTime> expiration, CancellationToken cancelToken)
        {
            return GetOrSetAsync(key, loader, expiration, null, cancelToken);
        }

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> loader, TimeSpan sliding, CancellationToken cancelToken)
        {
            return GetOrSetAsync(key, loader, null, v => sliding, cancelToken);
        }

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> loader, Func<T, TimeSpan> sliding, CancellationToken cancelToken)
        {
            return GetOrSetAsync(key, loader, null, sliding, cancelToken);
        }

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> loader, CacheExpiration expiration, CancellationToken cancelToken)
        {
            return GetOrSetAsync(key, loader, v =>
            {
                var min = _cacheSettings.GetMinutes(expiration);
                return DateTime.UtcNow.AddMinutes(min);
            }, null, cancelToken);
        }

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> loader, Func<T, CacheExpiration> expiration, CancellationToken cancelToken)
        {
            return GetOrSetAsync(key, loader, v =>
            {
                var enumValue = expiration(v);
                var min = _cacheSettings.GetMinutes(enumValue);
                return DateTime.UtcNow.AddMinutes(min);
            }, null, cancelToken);
        }

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> loader, CacheSliding sliding, CancellationToken cancelToken)
        {
            return GetOrSetAsync(key, loader, null, v =>
            {
                var min = _cacheSettings.GetMinutes(sliding);
                return TimeSpan.FromMinutes(min);
            }, cancelToken);
        }

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> loader, Func<T, CacheSliding> sliding, CancellationToken cancelToken)
        {
            return GetOrSetAsync(key, loader, null, v =>
            {
                var enumValue = sliding(v);
                var min = _cacheSettings.GetMinutes(enumValue);
                return TimeSpan.FromMinutes(min);
            }, cancelToken);
        }

        public Task SetAsync<T>(string key, T value, CancellationToken cancelToken)
        {
            return SetAsync(key, value, null, null, cancelToken);
        }

        public Task SetAsync<T>(string key, T value, DateTime expiration, CancellationToken cancelToken)
        {
            return SetAsync(key, value, expiration, null, cancelToken);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan sliding, CancellationToken cancelToken)
        {
            return SetAsync(key, value, null, sliding, cancelToken);
        }

        public Task SetAsync<T>(string key, T value, CacheExpiration expiration, CancellationToken cancelToken)
        {
            var min = _cacheSettings.GetMinutes(expiration);
            var dateTime = DateTime.UtcNow.AddMinutes(min);
            return SetAsync(key, value, dateTime, null, cancelToken);
        }

        public Task SetAsync<T>(string key, T value, CacheSliding sliding, CancellationToken cancelToken)
        {
            var min = _cacheSettings.GetMinutes(sliding);
            var timeSpan = TimeSpan.FromMinutes(min);
            return SetAsync(key, value, null, timeSpan, cancelToken);
        }

        public abstract Task RemoveAsync(string key, CancellationToken cancelToken);

        public abstract Task ClearAllAsync(CancellationToken cancelToken);

        protected abstract Task SetAsync<T>(string key, T value, DateTime? expiration, TimeSpan? sliding, CancellationToken cancelToken);

        protected abstract Task<Tuple<bool, T>> TryGetAsync<T>(string key, CancellationToken cancelToken);

        private async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> loader, Func<T, DateTime> expirationFunc, Func<T, TimeSpan> slidingFunc, CancellationToken cancelToken)
        {
            // Get It
            var result = await TryGetAsync<T>(key, cancelToken).ConfigureAwait(false);

            // Got It or No Loader
            if (result.Item1 || loader == null)
                return result.Item2;

            // Lock the Key
            using (await _keyedSemaphoreSlim.WaitAsync(key, cancelToken).ConfigureAwait(false))
            {
                // Get It (Again)
                result = await TryGetAsync<T>(key, cancelToken).ConfigureAwait(false);

                // Already Loaded, Return
                if (result.Item1)
                    return result.Item2;

                // Load It (For Real)
                var value = await loader().ConfigureAwait(false);

                // This warning is disabled because a primitive type (int, bool, etc) will never 
                // be null, but that is okay because we want to cache those types of values.
                // ReSharper disable once CompareNonConstrainedGenericWithNull
                if (value != null)
                {
                    var expiration = expirationFunc == null
                        ? (DateTime?)null
                        : expirationFunc(value);

                    var sliding = slidingFunc == null
                        ? (TimeSpan?)null
                        : slidingFunc(value);

                    // Set It
                    await SetAsync(key, value, expiration, sliding, cancelToken).ConfigureAwait(false);
                }

                return value;
            }
        }

        public T Get<T>(string key)
        {
            var task = GetAsync<T>(key, CancellationToken.None);
            return task.Result;
        }

        public void Set<T>(string key, T value, TimeSpan sliding)
        {
            var task = SetAsync(key, value, sliding, CancellationToken.None);
            task.Wait();
        }

        public void Remove(string key)
        {
            var task = RemoveAsync(key, CancellationToken.None);
            task.Wait();
        }

        public void ClearAll()
        {
            var task = ClearAllAsync(CancellationToken.None);
            task.Wait();
        }

        public T GetOrSet<T>(string key, Func<T> loader, TimeSpan sliding)
        {
            var task = GetOrSetAsync(key, () => Task.FromResult(loader()), sliding, CancellationToken.None);
            return task.Result;
        }
    }
}
