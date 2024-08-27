using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.Data.UI
{
    [Serializable]
    public struct ViewConfig
    {
        [SerializeField]
        private bool _destroyOnHide;
        [SerializeField]
        private AssetReferenceGameObject _prefab;

        public AssetReferenceGameObject Prefab => _prefab;
        public bool DestroyOnHide => _destroyOnHide;
    }
}