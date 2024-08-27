using System;
using TriInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.Data.Weapons
{
    [Serializable]
    [DeclareTabGroup("Tabs")]
    public struct WeaponConfig
    {
        [SerializeField]
        [Group("Tabs"), Tab("General")]
        private AssetReferenceSprite _preview;
        [SerializeField]
        [Group("Tabs"), Tab("General")]
        private AssetReferenceGameObject _projectile;

        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private float _size;
        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private float _speed;
        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private float _recharge;
        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private float _damage;
        [Group("Tabs"), Tab("Stats")]
        [SerializeField]
        private int _emitOnShot;
        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private int _shotByParticles;
        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private float _shotDelay;
        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private float _lifetime;

        public AssetReferenceSprite Preview => _preview;
        public AssetReferenceGameObject Projectile => _projectile;

        public float Speed => _speed;
        public float Size => _size;
        public float Recharge => _recharge;
        public float Damage => _damage;
        public int EmitOnShot => _emitOnShot;

        public int ShotByParticles => _shotByParticles;
        public float ShotDelay => _shotDelay;
        public float Lifetime => _lifetime;
    }
}