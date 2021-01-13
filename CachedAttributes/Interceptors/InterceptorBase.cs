using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace CachedAttributes.Interceptors
{
    public abstract class InterceptorBase<TAttr> : IAsyncInterceptor
        where TAttr : Attribute
    {
        private static readonly ConcurrentDictionary<string, TAttr> HasAttributeDictionary =
            new();

        public virtual void InterceptAsynchronous(IInvocation invocation)
        {
            var cacheAttribute = FindAttribute(invocation);
            var proceedInfo = invocation.CaptureProceedInfo();

            if (cacheAttribute == null)
            {
                proceedInfo.Invoke();
                invocation.ReturnValue = (Task) invocation.ReturnValue;
                return;
            }

            throw new NotImplementedException("Task dönen (Task<T> değil!) metotlarda neyi cache'leyicem :/ bu attribute kullanılamaz.");
        }

        public virtual void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            var proceedInfo = invocation.CaptureProceedInfo();
            var cacheAttribute = FindAttribute(invocation);

            if (cacheAttribute == null)
            {
                proceedInfo.Invoke();
                return;
            }

            var invocationResult = AsyncImpl<TResult>(invocation, proceedInfo, cacheAttribute);
            invocation.ReturnValue = invocationResult;
        }

        public virtual void InterceptSynchronous(IInvocation invocation)
        {
            //metot için tanımlı cache flag'i var mı kontrolü yapıldığı yer
            var cacheAttribute = FindAttribute(invocation);
            if (cacheAttribute == null) //eğer o metot cache işlemi uygulanmayacak bir metot ise process normal sürecinde devam ediyor
            {
                invocation.Proceed();
                return;
            }

            var invocationResult = SyncImpl(invocation, cacheAttribute);
            invocation.ReturnValue = invocationResult;
        }

        protected TAttr FindAttribute(IInvocation invocation)
        {
            var key = invocation.Method.DeclaringType.FullName + "." + invocation.Method.Name;
            if (HasAttributeDictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            var cacheAttribute = invocation.Method.GetCustomAttribute<TAttr>();
            HasAttributeDictionary[key] = cacheAttribute;
            return cacheAttribute;
        }

        protected abstract object SyncImpl(IInvocation invocation, TAttr cacheAttribute);

        protected abstract Task<TResult> AsyncImpl<TResult>(IInvocation invocation,
            IInvocationProceedInfo proceedInfo, TAttr cacheAttribute);
    }
}