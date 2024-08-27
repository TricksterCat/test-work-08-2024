using Cysharp.Threading.Tasks;
using Runtime.Commands;
using Runtime.Controllers;
using Runtime.Data.Level;
using Runtime.Services.Player;
using Unity.Mathematics;
using UnityEngine;
using VContainer.Unity;
using VitalRouter;

namespace Runtime.Services
{
    [Routes]
    public partial class LevelService : ITickable
    {
        private readonly LevelsProvider _levelsProvider;
        private readonly string _levelID;

        private LevelController _current;
        private readonly PlayerService _playerService;
        private readonly EnemyService _enemyService;

        private readonly Camera _camera;

        public bool IsMatchActive { get; private set; }
        
        public LevelService(LevelsProvider levelsProvider, PlayerService playerService, EnemyService enemyService, string level, Camera camera)
        {
            _playerService = playerService;
            _levelsProvider = levelsProvider;
            _enemyService = enemyService;
            _levelID = level;
            _camera = camera;
        }

        [Route]
        private async UniTask On(PressStartLevelCommand command)
        {
            if(IsMatchActive)
                return;
            
            _current = _levelsProvider.CreateLevel(_levelID);
            await UniTask.NextFrame();
            
            SetMatchActive(true);
        }

        void ITickable.Tick()
        {
            if(!IsMatchActive)
                return;

            var time = Time.time;
            var deltaTime = Time.deltaTime;

            Vector2 position = Vector2.zero;
            int playersCount = 0;

            foreach (var player in _playerService.Players)
            {
                if (player.Enable.CurrentValue && player.Character != null)
                {
                    player.Character.OnBeginUpdate(time, deltaTime);
                    position += (Vector2)player.Character.transform.position;
                    playersCount++;
                }
            }

            UpdateCameraPosition(position, playersCount);

            var enemies = _enemyService.Enemies;
            for (var index = enemies.Count - 1; index >= 0; index--)
            {
                var enemy = enemies[index];
                var enemyRect = enemy.ResolveRect();
                bool alive = true;

                float distanceSq = float.PositiveInfinity;
                Vector2 target = default;
                
                //TODO: O(n * 2 * (m + 1)) ... Bad...
                foreach (var player in _playerService.Players)
                {
                    if(!player.Enable.CurrentValue || player.Character == null)
                        continue;
                    
                    foreach (var resolver in player.Character.ActiveWeapon.ResolveProjectiles())
                    {
                        if (enemyRect.Overlaps(resolver.Rect) && 
                            !enemy.PushDamage(resolver.UseDamage()))
                        {
                            alive = false;
                            break;
                        }
                    }
                    
                    if(!alive)
                        break;

                    var characterRect = player.Character.ResolveRect();
                    if (enemyRect.Overlaps(characterRect))
                    {
                        player.Character.PushDamage(enemy.Damage);
                        enemy.Destroy();
                        break;
                    }

                    var nextDistance = (enemyRect.center - characterRect.center).sqrMagnitude;
                    if(nextDistance < distanceSq)
                    {
                        distanceSq = nextDistance;
                        target = characterRect.center;
                    }
                }

                if (alive && !math.isinf(distanceSq))
                {
                    enemy.Move(target, deltaTime);
                }
            }
            
            playersCount = 0;
            foreach (var player in _playerService.Players)
            {
                if (player.Enable.CurrentValue && player.Character != null)
                {
                    player.Character.OnCompleteUpdate();
                    playersCount++;
                }
            }

            if (playersCount == 0)
            {
                SetMatchActive(false);
                return;
            }
            //TODO: may be replace to DateTime or custom time...
            _enemyService.TrySpawn(time);
        }

        private void SetMatchActive(bool value)
        {
            Router.Default.Enqueue(new MatchStatusChangedCommand(value));
            IsMatchActive = value;
        }
        
        private void UpdateCameraPosition(Vector2 position, int playersCount)
        {
            if(playersCount == 0)
                return;
            
            position /= playersCount;
            var nextCameraPos= _camera.transform.position;
            nextCameraPos.x = position.x;
            nextCameraPos.y = position.y;
            
            if (!math.any(math.isnan(nextCameraPos)))
            {
                _camera.transform.position = nextCameraPos;
            }
        }
    }
}