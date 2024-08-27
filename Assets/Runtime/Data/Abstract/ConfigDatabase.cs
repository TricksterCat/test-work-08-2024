using System;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Data.Abstract
{
    public abstract class ConfigDatabase<TConfig> : ScriptableObject, ISerializationCallbackReceiver, IEnumerable<KeyValuePair<string, TConfig>>
    {
        [SerializeField]
        [TableList(AlwaysExpanded = true, Draggable = false)]
        private List<ItemBox> _data = new ();
        
        private readonly Dictionary<string, TConfig> _map = new();

        [Serializable]
        private struct ItemBox
        {
            [FormerlySerializedAs("_key")] [SerializeField, Required]
            public string Key;
            [FormerlySerializedAs("_value")] [SerializeField]
            public TConfig Value;
        }
        
        public bool Contains(string key)
        {
            return _map.ContainsKey(key);
        }
        
        public bool TryGet(string key, out TConfig config)
        {
            return _map.TryGetValue(key, out config);
        }
        
        public TConfig Get(string key)
        {
            return _map[key];
        }

        private void OnEnable()
        {
            ((ISerializationCallbackReceiver)this).OnAfterDeserialize();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _map.Clear();
            foreach (var box in _data)
            {
                _map[box.Key] = box.Value;
            }
        }

        public IEnumerator<KeyValuePair<string, TConfig>> GetEnumerator()
        {
            foreach (var box in _data)
            {
                yield return new KeyValuePair<string, TConfig>(box.Key, box.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
