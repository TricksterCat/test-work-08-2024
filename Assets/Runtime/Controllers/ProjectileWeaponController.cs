using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Common;
using Runtime.Services;
using Unity.Collections;
using UnityEngine;
using VContainer;

namespace Runtime.Controllers
{
    public class ProjectileWeaponController : WeaponController
    {
        [SerializeField]
        private ParticleSystem _particle;
        
        [Inject]
        private EnemyService _enemyService;

        private NativeList<ParticleSystem.Particle> _list;
        private int _particlesCount;

        private readonly ProjectileResolver _resolver = new();

        public override IEnumerable<ProjectileResolver> ResolveProjectiles()
        {
            var resolver = _resolver;
            for (int i = 0; i < _particlesCount; i++)
            {
                var particle = _list.ElementAt(i);
                
                yield return resolver.With(new Rect
                {
                    size = Vector2.one * particle.startSize,
                    center = particle.position
                }, State.Model.Damage);
                
                if (!resolver.IsAlive)
                {
                    _list.RemoveAtSwapBack(i);
                    _particlesCount--;
                }
            }
        }

        private void Awake()
        {
            _list = new NativeList<ParticleSystem.Particle>(128, Allocator.Persistent);
            _list.AddTo(destroyCancellationToken);
        }

        public override void Shot(Vector2 source, Vector2 direction)
        {
            base.Shot(source, direction);

            if (State.Model.EmitOnShot <= State.Model.ShotByParticles)
            {
                PushParticles(source, direction, State.Model.EmitOnShot);
            }
            else
            {
                ShortMany(source, direction, State.Model.EmitOnShot, destroyCancellationToken).Forget();
            }
        }

        private async UniTaskVoid ShortMany(Vector2 source, Vector2 direction, int count, CancellationToken cancellationToken)
        {
            while (count > 0)
            {
                var next = Mathf.Min(count, State.Model.ShotByParticles);
                PushParticles(source, direction, next);
                count -= next;
                
                await UniTask.Delay(TimeSpan.FromSeconds(State.Model.ShotDelay), cancellationToken: cancellationToken);
            }
        }

        private void PushParticles(Vector2 position, Vector2 direction, int count)
        {
            _particle.Emit(new ParticleSystem.EmitParams
            {
                position = position,
                startSize = State.Model.Size,
                startLifetime = State.Model.Lifetime,
                velocity = direction * State.Model.Speed,
                applyShapeToPosition = true
            }, count);
        }

        public override void OnBeginUpdate()
        {
            var count = _particle.particleCount;
            if (_list.Capacity < count)
            {
                _list.SetCapacity(count);
            }

            _list.Length = count;
            _particlesCount = _particle.GetParticles(_list);
        }

        public override void OnCompleteUpdate()
        {
            _particle.SetParticles(_list.AsArray());
        }
    }
}