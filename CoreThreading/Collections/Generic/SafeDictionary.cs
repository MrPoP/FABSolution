using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreThreading.Collections
{
    [Serializable]
    public class SafeDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private object syncRoot = null;
        public TValue[] ValuesArray { get { return base.Values.ToArray(); } }
        public TKey[] KeysArray { get { return base.Keys.ToArray(); } }
        public SafeDictionary()
            :base()
        { }
        public SafeDictionary(int capacity)
            : base(capacity)
        { }
        public SafeDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        public SafeDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this()
        {
            Monitor.Enter(this.syncRoot);
            foreach (var pair in collection)
            {
                base[pair.Key] = pair.Value;
            }
            Monitor.Exit(this.syncRoot);
        }
        public void IterateKeys(Action<TKey> method)
        {
            Parallel.ForEach<TKey>(base.Keys, method);
        }
        public void IterateValues(Action<TValue> method)
        {
            Parallel.ForEach<TValue>(base.Values, method);
        }
    }
}
