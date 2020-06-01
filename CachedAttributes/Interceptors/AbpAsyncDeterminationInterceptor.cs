using Castle.DynamicProxy;

namespace CachedAttributes.Interceptors
{
    public class AbpAsyncDeterminationInterceptor<TInterceptor> : AsyncDeterminationInterceptor
        where TInterceptor : IAsyncInterceptor
    {
        public AbpAsyncDeterminationInterceptor(TInterceptor asyncInterceptor) : base(asyncInterceptor)
        {

        }
    }
}