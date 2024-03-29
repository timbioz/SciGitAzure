﻿using System.Collections.Generic;
using Contrib.Cache.Models;
using Orchard;

namespace Contrib.Cache.Services {
    public interface IOutputCacheStorageProvider : IDependency {
        /// <summary>
        /// Adds a new <see cref="CacheItem"/> or substitute it with a new one if the 
        /// cache key is already used.
        /// </summary>
        /// <param name="key">The unique key representing the <see cref="CacheItem"/>.</param>
        /// <param name="cacheItem">The <see cref="CacheItem"/> instance to add to the cache.</param>
        void Set(string key, CacheItem cacheItem);

        void Remove(string key);
        void RemoveAll();
        
        CacheItem GetCacheItem(string key);
        IEnumerable<CacheItem> GetCacheItems(int skip, int count);
        int GetCacheItemsCount();
    }
}