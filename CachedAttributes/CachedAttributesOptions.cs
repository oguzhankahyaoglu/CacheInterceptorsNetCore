using System;
using System.Diagnostics;

namespace CachedAttributes
{
    public class CachedAttributesOptions
    {
        public string ProjectNamespaceRoot { get; set; }
        public bool IsLoggingEnabled { get; set; }

        /// <summary>
        /// After timeout (default 10min), the async fetch operation is cancelled and exception is thrown to prevent caching.
        /// </summary>
        public TimeSpan AsyncTimeout { get; set; } = TimeSpan.FromMinutes(10);

        internal static Action<string> Log = message => { Debug.WriteLine("[CacheInterceptor] " + message); };

        private static CachedAttributesOptions _Instance { get; set; }

        internal static CachedAttributesOptions Instance
        {
            get => _Instance;
            set
            {
                _Instance = value;
                if (!_Instance.IsLoggingEnabled)
                    Log = message => { };
            }
        }
    }
}