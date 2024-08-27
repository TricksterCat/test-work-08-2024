using System.Collections.Generic;
using System.Linq;
using R3;
using Runtime.Commands;
using Runtime.Common;
using Runtime.Controllers;
using Runtime.Data.Enemies;
using UnityEngine;
using VitalRouter;

namespace Runtime.Services
{
    [Routes]
    public partial class EnemyService
    {
        private const float SPAWN_DELAY = 0.1f;
        //Здесь костыльное магическое число, чтобы не получать Rect юнита до его создания.
        //Можно было бы передать ось и множитель направления и после создания подвинуть, или по нормальному сначало определиться с юнитом, а после считать где его заспамить, но уже чуть устал.
        private const float FIXED_OFFSET = 1.5f;
        
        public IList<EnemyController> Enemies => _enemies;
        public ReactiveProperty<int> SpawnMaxUnits { get; } = new (10);

        private readonly List<EnemyController> _enemies = new();
        private readonly EnemiesProvider _enemiesProvider;
        private readonly Camera _camera;

        private float _nextSpawnTime;
        private string[] _enemiesIds;

        private Unity.Mathematics.Random _random;


        public EnemyService(EnemiesProvider enemiesProvider, Camera camera)
        {
            _enemiesProvider = enemiesProvider;
            _camera = camera;
        }

        [Route]
        private void On(MatchStatusChangedCommand command)
        {
            if (command.Status)
            {
                _random = new Unity.Mathematics.Random(42);
                _enemiesIds = _enemiesProvider.EnumerableInjectedEntries().ToArray();
            }
            else
            {
                var enemies = _enemies;
                for (var index = enemies.Count - 1; index >= 0; index--)
                {
                    var enemy = enemies[index];
                    enemy.Destroy();
                }
            }
        }
        
        [Route]
        private void On(EntryDestroyedCommand command)
        {
            if(command.Type is not EntryType.Enemy)
                return;
            
            _enemies.Remove((EnemyController)command.Entry);
        }

        public void TrySpawn(float time)
        {
            if(_enemies.Count >= SpawnMaxUnits.CurrentValue || 
               time < _nextSpawnTime ||
               _enemiesIds.Length == 0)
                return;

            _nextSpawnTime = time + SPAWN_DELAY;

            var rect = new Rect
            {
                size = Vector2.one * ((_camera.orthographicSize + FIXED_OFFSET) * 2),
                center = _camera.transform.position
            };

            Vector2 position = _random.NextFloat2(rect.min, rect.max);

            //horizontal or vertical
            var axis = _random.NextInt(0, 2);
            position[axis] = (_random.NextBool() ? rect.min : rect.max)[axis];

            var enemy = _enemiesProvider.CreateEnemy(_enemiesIds[_random.NextInt(_enemiesIds.Length)], position);
            _enemies.Add(enemy);
        }
    }
}