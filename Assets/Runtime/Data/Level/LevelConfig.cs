using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.Data.Level
{
    [Serializable]
    public struct LevelConfig
    {
        [SerializeField]
        private AssetReferenceGameObject _prefab;

        public AssetReferenceGameObject Prefab => _prefab;
    }
}