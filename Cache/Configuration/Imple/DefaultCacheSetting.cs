using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache.Configuration.Imple
{
    class DefaultCacheSetting : ICacheSetting
    {
        public static readonly DefaultCacheSetting Instance = new DefaultCacheSetting();

        public int GetMinutes(CacheExpiration expiration)
        {
            return (int)expiration;
        }

        public int GetMinutes(CacheSliding sliding)
        {
            return (int)sliding;
        }
    }
}
