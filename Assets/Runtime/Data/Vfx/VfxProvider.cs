using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Data.Abstract;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Runtime.Data.Vfx
{
    public class VfxProvider : DataProvider<ParticleSystem>
    {
        private readonly VfxsDatabase _database;
        private readonly Dictionary<string, ObjectPool<ParticleSystem>> _pools = new();
        
        public VfxProvider(VfxsDatabase database)
        {
            _database = database;
        }

        public override bool CanInject(string key) => _database.Contains(key);

        protected override async UniTask<ParticleSystem> InternalInjectAsync(string key, Handler handler, CancellationToken cancellationToken)
        {
            var config = _database.Get(key);
            var prefab = await handler.LoadAsset(config.Prefab);
            return prefab.GetComponent<ParticleSystem>();
        }

        public void PlayVfx(string id, Vector3 position)
        {
            if (!_pools.TryGetValue(id, out var pool))
            {
                var vfx = ResolveInstant(id);
                _pools[id] = pool = new ObjectPool<ParticleSystem>(() => Object.Instantiate(vfx));
            }

            var dispose = (IDisposable)pool.Get(out var next);
            next.transform.position = position;
            next.Play();

            UniTask.NextFrame().ContinueWith(dispose.Dispose).Forget();
        }
    }
}