using Runtime.Controllers;
using UnityEngine;

namespace Runtime.Data.Weapons
{
    public class WeaponModel
    {
        public WeaponModel(WeaponController controller, Sprite spritePreview, 
            WeaponConfig config)
        {
            Damage = config.Damage;
            Speed = config.Speed;
            Recharge = config.Recharge;
            EmitOnShot = config.EmitOnShot;
            Size = config.Size;
            ShotByParticles = config.ShotByParticles;
            ShotDelay = config.ShotDelay;
            Lifetime = config.Lifetime;

            ControllerPrefab = controller;
            SpritePreview = spritePreview;
        }


        public Sprite SpritePreview { get; }
        public WeaponController ControllerPrefab { get; }

        public float Recharge { get; }
        public float Speed { get; }
        public float Damage { get; }
        public int EmitOnShot { get; }
        public float Size { get; }
        public int ShotByParticles { get; }
        public float ShotDelay { get; }
        public float Lifetime { get; }
    }
}