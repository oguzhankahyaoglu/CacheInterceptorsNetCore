using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;

namespace CachedAttributes
{
    public static class MemoryCacheExtensions
    {
        private static readonly Func<MemoryCache, object> GetEntriesCollection = Delegate.CreateDelegate(
            typeof(Func<MemoryCache, object>),
            typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true),
            throwOnBindFailure: true) as Func<MemoryCache, object>;

        public static IEnumerable GetKeys(this IMemoryCache memoryCache) =>
            ((IDictionary)GetEntriesCollection((MemoryCache)memoryCache)).Keys;

        public static IEnumerable<T> GetKeys<T>(this IMemoryCache memoryCache) =>
            GetKeys(memoryCache).OfType<T>();
        
        public static IEnumerable<string> GetKeys(this IAppCache appCache)
        {
            var cacheProvider = appCache.CacheProvider as MemoryCacheProvider;
            if (cacheProvider != null) //may be MockCacheProvider in tests 
            {
                var field = typeof(MemoryCacheProvider).GetField("cache", BindingFlags.NonPublic | BindingFlags.Instance);
                var memoryCache = field.GetValue(cacheProvider) as MemoryCache;
                return memoryCache.GetKeys<string>();
            }

            return new List<string>();
        }
    }
}