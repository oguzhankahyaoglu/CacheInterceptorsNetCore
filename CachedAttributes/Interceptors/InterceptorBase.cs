using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace CachedAttributes.Interceptors
{
    public abstract class InterceptorBase : IAsyncInterceptor
    {
        public virtual void InterceptAsynchronous(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
        }

        public virtual void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
        }

        public abstract void InterceptSynchronous(IInvocation invocation);

        protected abstract Task InternalInterceptAsynchronous(IInvocation invocation);

        protected abstract Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation);
        
    }
}