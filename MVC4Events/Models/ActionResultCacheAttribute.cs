﻿using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;

namespace mvc4events
{
    /// <summary>
    /// Caches the result of an action method.
    /// NOTE: you'll need refs to System.Web.Mvc and System.Runtime.Caching
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ActionResultCacheAttribute : System.Web.Mvc.OutputCacheAttribute
    {
        private static readonly Dictionary<string, string[]> _varyByParamsSplitCache = new Dictionary<string, string[]>();
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private static readonly MemoryCache _cache = new MemoryCache("ActionResultCacheAttribute");

        public string CacheProfile { get; set; }
        /// <summary>
        /// The comma separated parameters to vary the caching by.
        /// </summary>
        public string VaryByParam { get; set; }
        /// <summary>
        /// The sliding expiration, in seconds.
        /// </summary>
        public int SlidingExpiration { get; set; }
        /// <summary>
        /// The duration to cache before expiration, in seconds.
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// Occurs when an action is executing.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Create the cache key
            var cacheKey = CreateCacheKey(filterContext.RouteData.Values, filterContext.ActionParameters);
            // Try and get the action method result from cache
            var result = _cache.Get(cacheKey) as ActionResult;
            if (result != null)
            {
                // Set the result
                filterContext.Result = result;
                return;
            }
            // Store to HttpContext Items
            filterContext.HttpContext.Items["__actionresultcacheattribute_cachekey"] = cacheKey;
        }
        /// <summary>
        /// Occurs when an action has executed.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // Don't cache errors
            if (filterContext.Exception != null)
            {
                return;
            }
            // Get the cache key from HttpContext Items
            var cacheKey = filterContext.HttpContext.Items["__actionresultcacheattribute_cachekey"] as string;
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return;
            }
            // Cache the result of the action method
            if (SlidingExpiration != 0)
            {
                //_cache.Add(cacheKey, filterContext.Result, TimeSpan.FromSeconds(SlidingExpiration));
                DateTimeOffset dOffset= new DateTimeOffset();
                dOffset.AddSeconds(SlidingExpiration);
                _cache.Add(cacheKey, filterContext.Result, dOffset);
                return;
            }
            if (Duration != 0)
            {
                _cache.Add(cacheKey, filterContext.Result, DateTime.UtcNow.AddSeconds(Duration));
                return;
            }
            // Default to 1 hour
            _cache.Add(cacheKey, filterContext.Result, DateTime.UtcNow.AddSeconds(60 * 60));
        }
        /// <summary>
        /// Creates the cache key.
        /// </summary>
        /// <param name="routeValues">The route values.</param>
        /// <returns>The cache key.</returns>
        private string CreateCacheKey(RouteValueDictionary routeValues, IDictionary<string, object> actionParameters)
        {
            // Create the cache key prefix as the controller and action method
            var sb = new StringBuilder(routeValues["controller"].ToString());
            sb.Append("_").Append(routeValues["action"].ToString());
            if (string.IsNullOrWhiteSpace(VaryByParam))
            {
                return sb.ToString();
            }
            // Append the cache key from the vary by parameters
            object varyByParamObject = null;
            string[] varyByParamsSplit = null;
            bool gotValue = false;
            _lock.EnterReadLock();
            try
            {
                gotValue = _varyByParamsSplitCache.TryGetValue(VaryByParam, out varyByParamsSplit);
            }
            finally
            {
                _lock.ExitReadLock();
            }
            if (!gotValue)
            {
                _lock.EnterWriteLock();
                try
                {
                    varyByParamsSplit = VaryByParam.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    _varyByParamsSplitCache[VaryByParam] = varyByParamsSplit;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            foreach (var varyByParam in varyByParamsSplit)
            {
                // Skip invalid parameters
                if (!actionParameters.TryGetValue(varyByParam, out varyByParamObject))
                {
                    continue;
                }
                // Sometimes a parameter will be null
                if (varyByParamObject == null)
                {
                    continue;
                }
                sb.Append("_").Append(varyByParamObject.ToString());
            }
            return sb.ToString();
        }
    }
}