using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Commands;
using Runtime.Common;
using Runtime.Controllers;
using Runtime.Data.Abstract;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using VitalRouter;

namespace Runtime.Data.Enemies
{
    public class EnemiesProvider : DataProvider<EnemyController>
    {
        private readonly IObjectResolver _resolver;
        private readonly EnemyDatabase _database;

        private readonly Dictionary<string, ObjectPool<EnemyController>> _pools = new();
        
        public EnemiesProvider(EnemyDatabase database, IObjectResolver resolver)
        {
            _database = database;
            _resolver = resolver;
        }

        public override bool CanInject(string key) => _database.Contains(key);

        protected override async UniTask<EnemyController> InternalInjectAsync(string key, Handler handler, CancellationToken cancellationToken)
        {
            var config = _database.Get(key);
            var prefab = await handler.LoadAsset(config.Prefab);

            return prefab.GetComponent<EnemyController>();
        }

        public EnemyController CreateEnemy(string id, Vector2 position)
        {
            if (!_pools.TryGetValue(id, out var pool))
            {
                _pools[id] = pool = new ObjectPool<EnemyController>(() =>
                    {
                        var prefab = ResolveInstant(id);
                        return _resolver.Instantiate(prefab);
                    }, controller => controller.gameObject.SetActive(true),
                    controller =>
                    {
                        Router.Default.Enqueue(new EntryDestroyedCommand(EntryType.Enemy, id, controller));
                        controller.gameObject.SetActive(false);
                    }, controller =>
                    {
                        Router.Default.Enqueue(new EntryDestroyedCommand(EntryType.Enemy, id, controller));
                    });
            }

            var disposable = pool.Get(out var enemy);
            enemy.transform.position = position;
            enemy.Setup(_database.Get(id), disposable);
            return enemy;
        }
    }
}