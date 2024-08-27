using System;
using TriInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.Data.Enemies
{
    [Serializable]
    [DeclareTabGroup("Tabs")]
    public struct EnemyConfig
    {
        [SerializeField]
        [Group("Tabs"), Tab("General")]
        private AssetReferenceGameObject _prefab;
        [SerializeField]
        [Group("Tabs"), Tab("General")]
        private bool _availableDefault;
        [SerializeField, Group("Tabs"), Tab("General")]
        private string _deadFx;
        
        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private float _hp;
        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private float _damage;
        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private float _speed;
        [SerializeField, Range(0f, 1f)]
        [Group("Tabs"), Tab("Stats")]
        private float _defense;

        public float Hp => _hp;
        public float Defense => _defense;

        public float Damage => _damage;
        public float Speed => _speed;
        public bool AvailableDefault => _availableDefault;
        public AssetReferenceGameObject Prefab => _prefab;

        public string DeadFx => _deadFx;
    }
}