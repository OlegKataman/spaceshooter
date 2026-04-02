using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SpaceShooter.Runtime.Service
{
    public sealed class AssetService
    {
        private readonly Dictionary<string, UnityEngine.Object> _cache = new();
        
        public async UniTask<T> LoadAsync<T>(string path) where T : UnityEngine.Object
        {
            if (_cache.TryGetValue(path, out var cached))
                return cached as T;

            var request = Resources.LoadAsync<T>(path);
            await request.ToUniTask();

            if (request.asset == null)
                throw new Exception($"[AssetService] Asset not found at path: '{path}'");

            _cache[path] = request.asset;
            return request.asset as T;
        }
    }
}