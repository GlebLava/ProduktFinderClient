using System.Collections.Generic;


namespace ProduktFinderClient.DataTypes
{
    public class BidirectionalDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _keyValueDict;
        private readonly Dictionary<TValue, TKey> _valueKeyDict;

        public BidirectionalDictionary()
        {
            _keyValueDict = new Dictionary<TKey, TValue>();
            _valueKeyDict = new Dictionary<TValue, TKey>();
        }

        public void Add(TKey key, TValue value)
        {
            _keyValueDict.Add(key, value);
            _valueKeyDict.Add(value, key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _keyValueDict.TryGetValue(key, out value);
        }

        public bool TryGetKey(TValue value, out TKey key)
        {
            return _valueKeyDict.TryGetValue(value, out key);
        }

        public bool ContainsKey(TKey key)
        {
            return _keyValueDict.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return _valueKeyDict.ContainsKey(value);
        }

        public void Clear()
        {
            _keyValueDict.Clear();
            _valueKeyDict.Clear();
        }
    }
}
