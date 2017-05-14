using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache
{
    public interface ICacheRepository
    {
        //Get by key
        //"T": Type of the cached object key: Cache key
        T Get<T>(string key);

        //Set by key
        //key: Cache key;  value:Value to be cached
        //Sliding expiration to use for cache
        void Set<T>(string key, T value, TimeSpan sliding);

        // Remove by key
        void Remove(string key);


        // Clear all cache
        void ClearAll();

        //Get or Set by Key: If key is not found, loader is invoked and the result is cached under the specified key.
        //key: Cache key
        //loader: Delegate to invoke if cached item is not found
        //sliding: Sliding expiration to use if object is loaded and cached.
        T GetOrSet<T>(string key, Func<T> loader, TimeSpan sliding);
    }
}
