using System;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using VContainer.Unity;

namespace Runtime.Common
{
    [Serializable]
    public abstract class ResolveStateHelper<TSource, TTarget> : IEnumerable<TTarget>, ISerializationCallbackReceiver
    {
        [SerializeField, TableList(AlwaysExpanded = true)]
        private Item[] _data;
        private Dictionary<TSource, TTarget> _map;
        
        [Serializable]
        private struct Item
        {
            [SerializeField]
            private TSource _key;
            [SerializeField]
            private TTarget _value;

            public TTarget Value => _value;

            public void Inject(IDictionary<TSource, TTarget> dictionary)
            {
                dictionary[_key] = _value;
            }
        }
        
        public bool TryResolve(TSource source, out TTarget result)
        {
            return _map.TryGetValue(source, out result);
        }
        
        public IEnumerator<TTarget> GetEnumerator()
        {
            foreach (var item in _data)
            {
                yield return item.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _map = new Dictionary<TSource, TTarget>(_data.Length);
            foreach (var item in _data)
            {
                item.Inject(_map);
            }
        }
    }
}