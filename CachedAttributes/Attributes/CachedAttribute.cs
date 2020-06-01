using System;

namespace CachedAttributes.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CachedAttribute : Attribute
    {
        private readonly CacheDuration _duration;
        private readonly TimeSpan? _expires;

        public CachedAttribute(CacheDuration duration = CacheDuration.Short)
        {
            _duration = duration;
        }

        public CachedAttribute(TimeSpan expires)
        {
            _expires = expires;
        }

        public TimeSpan GetExpires()
        {
            if (_expires != null)
                return _expires.Value;
            
            switch (_duration)
            {
                case CacheDuration.Short:
                    return TimeSpan.FromMinutes(10);
                case CacheDuration.Medium:
                    return TimeSpan.FromDays(1);
                case CacheDuration.Long:
                    return TimeSpan.FromDays(31);
                default:
                    throw new NotImplementedException("bu duration implement edilmedi:" + _duration);
            }
        }
    }

    public enum CacheDuration
    {
        /// <summary>
        /// like 5-10 minutes
        /// </summary>
        Short,

        /// <summary>
        /// 1 day
        /// </summary>
        Medium,

        /// <summary>
        /// Persistent like 30 days
        /// </summary>
        Long
    }
}