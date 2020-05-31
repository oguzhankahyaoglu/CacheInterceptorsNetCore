﻿using System;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CacheInterceptorsNetCore
{
    public interface ICachingKeyBuilder
    {
        string BuildCacheKeyFromRequest(IInvocation invocation);
        string BuildCacheKey(IInvocation invocation);
    }

    public class CachingKeyBuilder : ICachingKeyBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CachingKeyBuilder> _logger;

        public CachingKeyBuilder(IHttpContextAccessor httpContextAccessor, ILogger<CachingKeyBuilder> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }


        public string BuildCacheKeyFromRequest(IInvocation invocation)
        {
            string CacheKeyFromRequest()
            {
                var key = DateTime.Now.AddMinutes(1).ToString("dd/MM/yyyy HH:mm");
                if (_httpContextAccessor == null)
                {
                    //1dk içinde isteğin cevap döneceğini tahmin ediyoruz eğer web request yoksa
                    _logger.LogError($"NO HTTP CONTEXT AVAILABLE for caching, returning key '{key}'");
                    return key;
                }

                key = _httpContextAccessor.HttpContext.TraceIdentifier;
                var requestUrl = _httpContextAccessor.HttpContext.Request.Path.ToString();
                _logger.LogDebug($"CACHEKEY: {key} URL: {requestUrl}");
                return key;
            }

            var methodName = invocation.Method.ToString();

            var arguments = JsonConvert.SerializeObject(invocation.Arguments);
            var argsString = string.Join(",", arguments);
            var httpCacheKey = CacheKeyFromRequest();
            var cacheKey = $"{httpCacheKey}\n{methodName}:\n{argsString}";
            return cacheKey;
        }

        public string BuildCacheKey(IInvocation invocation)
        {
            var methodName = invocation.Method.ToString();
            var arguments = JsonConvert.SerializeObject(invocation.Arguments);
            var argsString = string.Join(",", arguments);
            var cacheKey = $"{methodName}:\n{argsString}";
            return cacheKey;
        }
    }
}