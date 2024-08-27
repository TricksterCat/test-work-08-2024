using System;
using TriInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.Data.Characters
{
    [Serializable]
    [DeclareTabGroup("Tabs")]
    public struct CharacterConfig
    {
        [SerializeField, Group("Tabs"), Tab("General")]
        private bool _availableDefault;
        [SerializeField, Group("Tabs"), Tab("General")]
        private string _name;
        [SerializeField, Group("Tabs"), Tab("General")]
        private string _description;
        [SerializeField, Group("Tabs"), Tab("General")]
        private AssetReferenceGameObject _prefab;
        [SerializeField, Group("Tabs"), Tab("General")]
        private AssetReferenceSprite _preview;

        [SerializeField, Group("Tabs"), Tab("General")]
        private string[] _weapons;

        [SerializeField, Group("Tabs"), Tab("General")]
        private string _deadFx;

        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private float _hp;
        [SerializeField]
        [Group("Tabs"), Tab("Stats")]
        private float _speed;
        [SerializeField, Range(0f, 1f)]
        [Group("Tabs"), Tab("Stats")]
        private float _defense;

        public bool AvailableDefault => _availableDefault;

        public string Name => _name;
        public string Description => _description;
        public AssetReferenceGameObject Prefab => _prefab;

        public AssetReferenceSprite Preview => _preview;
        
        public float Hp => _hp;
        public float Speed => _speed;
        public float Defense => _defense;
        public string[] Weapons => _weapons;

        public string DeadFx => _deadFx;
    }
}