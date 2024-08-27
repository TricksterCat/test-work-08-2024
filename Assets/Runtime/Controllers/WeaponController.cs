using System.Collections.Generic;
using Runtime.Common;
using Runtime.Data.Weapons;
using UnityEngine;

namespace Runtime.Controllers
{
    public abstract class WeaponController : MonoBehaviour
    {
        protected WeaponState State { get; private set; }
        public bool CanShot => Time.time > State.CooldownComplete;
        
        public virtual void Setup(WeaponState state)
        {
            State = state;
        }

        public abstract IEnumerable<ProjectileResolver> ResolveProjectiles();

        public virtual void Shot(Vector2 source, Vector2 direction)
        {
            State.FullCooldown(Time.time);
        }

        public virtual void OnBeginUpdate()
        {
            
        }
        
        public virtual void OnCompleteUpdate()
        {
            
        }
    }
}