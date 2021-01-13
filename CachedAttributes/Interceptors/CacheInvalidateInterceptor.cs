using System;
using System.Linq;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;

namespace CachedAttributes.Interceptors
{
    public interface ICacheInvalidatorForInterceptors
    {
        void Invalidate(string cacheKey);
    }

    public class CacheInvalidateInterceptor : ICacheInvalidatorForInterceptors
    {
        private readonly IAppCache _cacheProvider;

        public CacheInvalidateInterceptor(IAppCache cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public void Invalidate(string cacheKey)
        {
            var keys = _cacheProvider.GetKeys();
            var key = keys.FirstOrDefault(x => x.StartsWith(cacheKey));
            if (key == null)
            {
                CacheInterceptorsRegistrar.Log("Cannot find key in cache:" + key);
                return;
            }

            _cacheProvider.Remove(key);
        }
    }
}