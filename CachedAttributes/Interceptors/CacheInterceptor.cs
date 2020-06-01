using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using CachedAttributes.Attributes;
using Castle.DynamicProxy;
using LazyCache;

namespace CachedAttributes.Interceptors
{
    public class CacheInterceptor : InterceptorBase
    {
        private readonly IAppCache _cacheProvider;
        private readonly ICachingKeyBuilder _cachingKeyBuilder;

        public CacheInterceptor(IAppCache cacheProvider, ICachingKeyBuilder cachingKeyBuilder)
        {
            _cacheProvider = cacheProvider;
            _cachingKeyBuilder = cachingKeyBuilder;
        }


        private static CachedAttribute FindAttribute(IInvocation invocation)
        {
            var cacheAttribute = invocation.Method.GetCustomAttribute<CachedAttribute>();
            return cacheAttribute;
        }

        public override void InterceptSynchronous(IInvocation invocation)
        {
            //metot için tanımlı cache flag'i var mı kontrolü yapıldığı yer
            var cacheAttribute = FindAttribute(invocation);
            if (cacheAttribute == null) //eğer o metot cache işlemi uygulanmayacak bir metot ise process normal sürecinde devam ediyor
            {
                invocation.Proceed();
                return;
            }

            //eğer o metot cache işlemlerinin yapılması gereken bir metot ise ilk olarak dynamic olarak aşağıdaki gibi bir cacheKey oluşturuyoruz
            var cacheKey = _cachingKeyBuilder.BuildCacheKey(invocation);
            DebugLog($"{cacheKey}\nStarted intercepting SYNC: ");
            var result = _cacheProvider.GetOrAdd(cacheKey, () =>
            {
                DebugLog($"{cacheKey}\nFetching data to cache SYNC");
                invocation.Proceed();
                DebugLog($"{cacheKey}\nFetched data to cache SYNC");
                return invocation.ReturnValue;
            }, cacheAttribute.GetExpires());
            DebugLog($"{cacheKey}\nReturning cached data SYNC");
            invocation.ReturnValue = result;
        }

        protected override async Task InternalInterceptAsynchronous(IInvocation invocation)
        {
            var cacheAttribute = FindAttribute(invocation);
            var proceedInfo = invocation.CaptureProceedInfo();

            if (cacheAttribute == null)
            {
                proceedInfo.Invoke();
                var task = (Task) invocation.ReturnValue;
                await task.ConfigureAwait(false);
                return;
            }

            throw new NotImplementedException("Task dönen (Task<T> değil!) metotlarda neyi cache'leyicem :/ bu attribute kullanılamaz.");
        }

        protected override async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            var proceedInfo = invocation.CaptureProceedInfo();
            var cacheAttribute = FindAttribute(invocation);

            if (cacheAttribute == null)
            {
                proceedInfo.Invoke();
                var taskResult = (Task<TResult>) invocation.ReturnValue;
                return await taskResult.ConfigureAwait(false);
            }

            {
                //eğer o metot cache işlemlerinin yapılması gereken bir metot ise ilk olarak dynamic olarak aşağıdaki gibi bir cacheKey oluşturuyoruz
                var cacheKey = _cachingKeyBuilder.BuildCacheKey(invocation);
                DebugLog($"{cacheKey}\nStarted intercepting ASYNC: ");
                var result = await _cacheProvider.GetOrAddAsync(cacheKey, async () =>
                {
                    DebugLog($"{cacheKey}\nFetching data to cache ASYNC");
                    proceedInfo.Invoke();
                    var taskResult = (Task<TResult>) invocation.ReturnValue;
                    var methodResult = await taskResult.ConfigureAwait(false);
                    DebugLog($"{cacheKey}\nFetched data to cache ASYNC");
                    return methodResult;
                }, cacheAttribute.GetExpires());
                DebugLog($"{cacheKey}\nReturning cached data ASYNC");
                return result;
            }
        }

        private void DebugLog(string message)
        {
            Debug.WriteLine("[CacheInterceptor] " + message);
        }
    }
}