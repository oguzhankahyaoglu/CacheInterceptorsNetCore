using System;

namespace CacheInterceptorsNetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CachePerRequestAttribute : Attribute
    {

    }
}