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
    public class CacheInvalidateInterceptor : InterceptorBase
    {
        private readonly IAppCache _cacheProvider;
        private readonly ICachingKeyBuilder _cachingKeyBuilder;

        private static readonly ConcurrentDictionary<string, CachedInvalidateAttribute> HasAttributeDictionary =
            new ConcurrentDictionary<string, CachedInvalidateAttribute>();

        public CacheInvalidateInterceptor(IAppCache cacheProvider, ICachingKeyBuilder cachingKeyBuilder)
        {
            _cacheProvider = cacheProvider;
            _cachingKeyBuilder = cachingKeyBuilder;
        }


        private static CachedInvalidateAttribute FindAttribute(IInvocation invocation)
        {
            var key = invocation.Method.DeclaringType.FullName + "." + invocation.Method.Name;
            if (HasAttributeDictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            var cacheAttribute = invocation.Method.GetCustomAttribute<CachedInvalidateAttribute>();
            HasAttributeDictionary[key] = cacheAttribute;
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
            var cacheKey = _cachingKeyBuilder.BuildCacheKey(invocation, cacheAttribute.InvalidateCacheMethodName);
            DebugLog($"{cacheKey}\nStarted intercepting CacheInvalidate");
            _cacheProvider.Remove(cacheKey);
            invocation.Proceed();
            DebugLog($"{cacheKey}\nFinished intercepting CacheInvalidate");
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
                var cacheKey = _cachingKeyBuilder.BuildCacheKey(invocation, cacheAttribute.InvalidateCacheMethodName);
                DebugLog($"{cacheKey}\nStarted intercepting CacheInvalidate");
                _cacheProvider.Remove(cacheKey);
                proceedInfo.Invoke();
                var taskResult = (Task<TResult>) invocation.ReturnValue;
                var methodResult = await taskResult.ConfigureAwait(false);
                DebugLog($"{cacheKey}\nFinished intercepting CacheInvalidate");
                return methodResult;
            }
        }

        private void DebugLog(string message)
        {
            if (CacheInterceptorsRegistrar.IsLoggingEnabled)
                Debug.WriteLine("[CacheInterceptor] " + message);
        }
    }
}