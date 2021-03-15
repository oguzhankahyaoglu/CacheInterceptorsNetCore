using System;
using System.Linq;
using LazyCache;

namespace CachedAttributes.Interceptors
{
    public interface ICacheInvalidatorForInterceptors
    {
        void Invalidate(Type classType,string cacheKey);
    }

    public class CacheInvalidateInterceptor : ICacheInvalidatorForInterceptors
    {
        private readonly IAppCache _cacheProvider;

        public CacheInvalidateInterceptor(IAppCache cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public void Invalidate(Type classType, string cacheKey)
        {
            var keys = _cacheProvider.GetKeys();
            var typeName = classType.Name.Replace("Proxy","");
            cacheKey = $"{typeName}.{cacheKey}";
            var key = keys.FirstOrDefault(x => x.StartsWith(cacheKey));
            if (key == null)
            {
                CachedAttributesOptions.Log("Cannot find key in cache:" + key);
                return;
            }

            _cacheProvider.Remove(key);
        }
    }
}