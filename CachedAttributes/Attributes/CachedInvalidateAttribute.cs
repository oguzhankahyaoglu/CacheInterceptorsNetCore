using System;

namespace CachedAttributes.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CachedInvalidateAttribute : Attribute
    {
        public string InvalidateCacheMethodName { get; }

        public CachedInvalidateAttribute(string invalidateCacheMethodName)
        {
            InvalidateCacheMethodName = invalidateCacheMethodName;
        }
    }
}