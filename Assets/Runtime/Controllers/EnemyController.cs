using System;
using Runtime.Commands;
using Runtime.Data.Enemies;
using UnityEngine;
using VitalRouter;

namespace Runtime.Controllers
{
    public class EnemyController : BaseEntry
    {
        private float _hp;
        private float _def;
        private float _damage;
        private float _speed;
        
        private string _deadFx;

        private Vector2 _size;
        private IDisposable _destroyHandler;
        
        public float Damage => _damage;

        private void Awake()
        {
            _size = GetComponent<SpriteRenderer>().bounds.size;
        }

        public void Setup(EnemyConfig config, IDisposable disposable)
        {
            _hp = config.Hp;
            _def = config.Defense;
            _damage = config.Damage;
            _speed = config.Speed;

            _deadFx = config.DeadFx;
            
            _destroyHandler = disposable;
        }
        
        public override Rect ResolveRect()
        {
            return new Rect(Vector2.zero, _size)
            {
                center = transform.position
            };
        }

        public override bool PushDamage(float value)
        {
            _hp -= value * (1f - _def);
            if (_hp < 0)
            {
                Destroy();
                return false;
            }

            return true;
        }

        public override void Destroy()
        {
            if (!string.IsNullOrEmpty(_deadFx))
            {
                Router.Default.Enqueue(new PlayVfxCommand(_deadFx, transform.position));
            }
            _destroyHandler?.Dispose();
            _destroyHandler = null;
        }

        public void Move(Vector2 target, float deltaTime)
        {
            transform.position += (Vector3)((target - (Vector2)transform.position).normalized * _speed * deltaTime);
        }
    }
}