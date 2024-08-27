using UnityEngine;

namespace Runtime.Common
{
    public class ProjectileResolver
    {
        private float _damage;
        public Rect Rect { get; private set; }
        public bool IsAlive { get; private set;  }

        public ProjectileResolver With(Rect rect, float damage)
        {
            Rect = rect;
            _damage = damage;
            IsAlive = true;
            return this;
        }

        public float UseDamage()
        {
            IsAlive = false;
            return _damage;
        }
    }
}