using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cache.Configuration.Imple;

namespace Cache.Imp
{
    public class EmptyCacheRepository : AsyncCacheRepositoryBase
    {
        public EmptyCacheRepository()
          : base(DefaultCacheSetting.Instance)
        {
        }

        public override Task RemoveAsync(string key, CancellationToken cancelToken)
        {
            return Task.FromResult(true);
        }

        public override Task ClearAllAsync(CancellationToken cancelToken)
        {
            return Task.FromResult(true);
        }

        protected override Task SetAsync<T>(string key, T value, DateTime? expiration, TimeSpan? sliding, CancellationToken cancelToken)
        {
            return Task.FromResult(true);
        }

        protected override Task<Tuple<bool, T>> TryGetAsync<T>(string key, CancellationToken cancelToken)
        {
            var result = Tuple.Create(false, default(T));
            return Task.FromResult(result);
        }
    }
}
