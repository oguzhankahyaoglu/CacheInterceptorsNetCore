using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using CachedAttributes.Attributes;
using Castle.DynamicProxy;
using LazyCache;

namespace CachedAttributes.Interceptors
{
    public class CacheInterceptor : InterceptorBase<CachedAttribute>
    {
        private readonly IAppCache _cacheProvider;
        private readonly ICachingKeyBuilder _cachingKeyBuilder;

        public CacheInterceptor(IAppCache cacheProvider, ICachingKeyBuilder cachingKeyBuilder)
        {
            _cacheProvider = cacheProvider;
            _cachingKeyBuilder = cachingKeyBuilder;
        }

        protected override object SyncImpl(IInvocation invocation, CachedAttribute cachedAttribute)
        {
            //eğer o metot cache işlemlerinin yapılması gereken bir metot ise ilk olarak dynamic olarak aşağıdaki gibi bir cacheKey oluşturuyoruz
            var cacheKey = _cachingKeyBuilder.BuildCacheKey(invocation, null);
            CachedAttributesOptions.Log($"{cacheKey}\nStarted intercepting SYNC: ");
            var result = _cacheProvider.GetOrAdd(cacheKey, () =>
            {
                CachedAttributesOptions.Log($"{cacheKey}\nFetching data to cache SYNC");
                invocation.Proceed();
                CachedAttributesOptions.Log($"{cacheKey}\nFetched data to cache SYNC");
                return invocation.ReturnValue;
            }, cachedAttribute.GetExpires());
            CachedAttributesOptions.Log($"{cacheKey}\nReturning cached data SYNC");
            return result;
        }
        
        protected override Task<TResult> AsyncImpl<TResult>(IInvocation invocation,
            IInvocationProceedInfo proceedInfo, CachedAttribute cacheAttribute)
        {
            //eğer o metot cache işlemlerinin yapılması gereken bir metot ise ilk olarak dynamic olarak aşağıdaki gibi bir cacheKey oluşturuyoruz
            var cacheKey = _cachingKeyBuilder.BuildCacheKey(invocation, null);
            CachedAttributesOptions.Log($"{cacheKey}\nStarted intercepting ASYNC: ");
            var result = _cacheProvider.GetOrAddAsync(cacheKey, async () =>
            {
                CachedAttributesOptions.Log($"{cacheKey}\nFetching data to cache ASYNC");
                proceedInfo.Invoke();
                var taskResult = (Task<TResult>) invocation.ReturnValue;
                var timeoutTask = taskResult.TimeoutAfter();
                var methodResult = await timeoutTask;
                CachedAttributesOptions.Log($"{cacheKey}\nFetched data to cache ASYNC");
                return methodResult;
            }, cacheAttribute.GetExpires());
            CachedAttributesOptions.Log($"{cacheKey}\nReturning cached data ASYNC");
            return result;
        }
    }
}