using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CachedAttributes.Attributes;
using CachedAttributes.Interceptors;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace CachedAttributes
{
    public static class CacheInterceptorsRegistrar
    {
        private static string _projectNamespaceRoot;

        /// <summary>
        /// Register caching interceptors and all required services
        /// </summary>
        /// <param name="container"></param>
        /// <param name="projectNamespaceRoot"></param>
        public static void RegisterCacheInterceptors(this IWindsorContainer container, string projectNamespaceRoot)
        {
            _projectNamespaceRoot = projectNamespaceRoot;
            container.Register(Component.For<CacheInterceptor>().LifestyleTransient());
            container.Register(Component.For<CachePerRequestInterceptor>().LifestyleTransient());
            container.Register(Component.For<ICachingKeyBuilder>().ImplementedBy<CachingKeyBuilder>().LifestyleTransient());
            container.Register(Component.For(typeof(AbpAsyncDeterminationInterceptor<>)).LifestyleTransient());
            // iocManager.Register(typeof(AbpAsyncDeterminationInterceptor<CacheInterceptor>), DependencyLifeStyle.Transient);
            // iocManager.Register(typeof(AbpAsyncDeterminationInterceptor<CachePerRequestInterceptor>), DependencyLifeStyle.Transient);
            container.Kernel.ComponentRegistered += Kernel_ComponentRegistered;
        }

        private static void Kernel_ComponentRegistered(string key, IHandler handler)
        {
            if (_projectNamespaceRoot == null)
                throw new Exception(
                    "container.RegisterCacheInterceptors not called or projectNamespaceRoot is not given. projectNamespaceRoot is required to achieve filtering of project assemblies for registering interceptors");

            var implementation = handler.ComponentModel.Implementation;
            if (implementation.Namespace?.StartsWith(_projectNamespaceRoot) != true)
            {
                return;
            }

            if (ShouldIntercept<CachedPerRequestAttribute>(handler.ComponentModel))
            {
                Debug.WriteLine("[Intercepting CachePerRequest] " + implementation.Name);
                handler.ComponentModel.Interceptors
                    .Add(new InterceptorReference(typeof(AbpAsyncDeterminationInterceptor<CachePerRequestInterceptor>)));
            }

            if (ShouldIntercept<CachedAttribute>(handler.ComponentModel))
            {
                Debug.WriteLine("[Intercepting Cache] " + implementation.Name);
                handler.ComponentModel.Interceptors
                    .Add(new InterceptorReference(typeof(AbpAsyncDeterminationInterceptor<CacheInterceptor>)));
            }
        }


        internal static bool ShouldIntercept<TAttribute>(ComponentModel component)
        {
            var type = component.Implementation;
            var attributeType = typeof(TAttribute);
            if (type.GetTypeInfo().IsDefined(attributeType, true))
            {
                return true;
            }

            if (type.GetMethods().Any(m => m.IsDefined(attributeType, true)))
            {
                return true;
            }

            var anyServiceRegisteredWithAttribute = component.Services
                .Any(service => service.GetMethods().Any(m => m.IsDefined(attributeType, true)));
            if (anyServiceRegisteredWithAttribute)
                return true;
            return false;
        }
    }
}