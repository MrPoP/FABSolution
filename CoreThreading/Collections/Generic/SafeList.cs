using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreThreading.Collections.Generic
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class SafeList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback
    {
        private T[] m_list;
        [NonSerialized]
        private object syncRoot;
        private int capacity = -1;
        public int Capacity { get { return this.capacity; } }
        private volatile int count = 0;
        private SerializationInfo _siInfo;
        private bool Capable { get { return this.Capacity != -1; } }
        private bool Addable { get { return this.m_list.Length > this.m_list.Where(p => p.Equals(default(T))).Count(); } }
        public int Count { get { return this.count; } }
        protected SafeList(SerializationInfo info, StreamingContext context)
        {
            this.syncRoot = new object();
            this._siInfo = info;
        }
        protected SafeList(IEnumerable<T> collection, int _capacity)
        {
            this.syncRoot = new object();
            if (_capacity != 0)
                this.capacity = _capacity;
            else
                this.capacity = -1;
            if (collection != null)
                this.m_list = collection.ToArray();
            this.m_list = new T[_capacity];
            this.count = 0;
        }
        public SafeList(IEnumerable<T> collection)
            : this(collection, 0) { }
        public SafeList(int capacity)
            : this(null, capacity) { }
        public SafeList()
            : this(null, 0) { }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(T));
            info.AddValue("Capacity", this.Capacity);
            info.AddValue("m_list", this.m_list);
            info.AddValue("MembersCount", this.count);
        }
        public void OnDeserialization(object sender)
        {
            Debug.Assert(this._siInfo == null, "Null value exception", string.Format("{0}"), new ArgumentNullException("info"));
            if (this._siInfo == null)
                return;
            Debug.Assert(this._siInfo.GetType() != typeof(T), "Faild to deserialize.");
            this.capacity = this._siInfo.GetInt32("Capacity");
            this.m_list = (T[])this._siInfo.GetValue("m_list", typeof(T[]));
            this.count = this._siInfo.GetInt32("MembersCount");
            this._siInfo = null;
        }
        public int IndexOf(T item)
        {
            lock(this.syncRoot)
            {
                return Array.FindIndex(this.m_list, p => p.Equals(item));
            }
        }
        public void Insert(int index, T item)
        {
            Debug.Assert(IsFixedSize && this.m_list.Length == this.count, "FixedSize safelist can't accept more than it's capacity.");
            lock(this.syncRoot)
            {
                if (index < this.m_list.Length)
                {
                    T[] newarray = new T[0];
                    int indexer = 0;
                    Parallel.ForEach(this.m_list, obitem =>
                    {
                        while (indexer > newarray.Length - 1)
                            Array.Resize<T>(ref newarray, newarray.Length + 1);
                        if (indexer == index)
                        {
                            newarray[indexer++] = item;
                            while (indexer > newarray.Length - 1)
                                Array.Resize<T>(ref newarray, newarray.Length + 1);
                            newarray[indexer++] = obitem;
                        }
                        else
                            newarray[indexer++] = obitem;
                    });
                    this.m_list = newarray;
                    Trim();
                }
                else
                {
                    while (!Addable)
                        Array.Resize<T>(ref this.m_list, this.count + ((index - this.m_list.Length) < 0 ? 1 : (index - this.m_list.Length)));
                    if(this.m_list[index].Equals(default(T)))
                    {
                        this.m_list[index] = item;
                    }
                    else
                    {
                        T[] newarray = new T[0];
                        int indexer = 0;
                        Parallel.ForEach(this.m_list, obitem =>
                        {
                            while (indexer > newarray.Length - 1)
                                Array.Resize<T>(ref newarray, newarray.Length + 1);
                            if (indexer == index)
                            {
                                newarray[indexer++] = item;
                                while (indexer > newarray.Length - 1)
                                    Array.Resize<T>(ref newarray, newarray.Length + 1);
                                newarray[indexer++] = obitem;
                            }
                            else
                                newarray[indexer++] = obitem;
                        });
                        this.m_list = newarray;
                    }
                    Trim();
                }
            }
        }
        public void RemoveAt(int index)
        {
            Debug.Assert(index > this.m_list.Length - 1, "Index is greater than members count.");
            lock(this.syncRoot)
            {
                this.m_list[index - 1] = default(T);
                Trim();
            }
        }
        public T this[int index]
        {
            get
            {
                Debug.Assert(index > this.m_list.Length - 1, "index is out of avaliable range.");
                if (index < this.m_list.Length)
                    return this.m_list[index];
                else
                    return default(T);
            }
            set
            {
                if(index < this.m_list.Length)
                {
                    Add(value);
                }
                else
                {
                    Insert(index, value);
                }
            }
        }
        public void Add(T item)
        {
            Debug.Assert(IsFixedSize && this.m_list.Length == this.count, "FixedSize safelist can't accept more than it's capacity.");
            lock(this.syncRoot)
            {
                while (!Addable)
                    Array.Resize<T>(ref this.m_list, this.count + 1);
                this.m_list[this.count - 1] = item;
                Trim();
            }
        }
        public void Clear()
        {
            lock(this.syncRoot)
            {
                this.m_list = null;
                this.count = 0;
            }
        }
        public bool Contains(T item)
        {
            lock(this.syncRoot)
            {
                return this.m_list.Contains(item);
            }
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock(this.m_list)
            {
                this.m_list.CopyTo(array, arrayIndex);
            }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public bool Remove(T item)
        {
            Debug.Assert(!Contains(item), "Member can't be found in the list.");
            lock(this.syncRoot)
            {
                try
                {
                    int index = Array.FindIndex(this.m_list, p => p.Equals(item));
                    this.m_list[index] = default(T);
                    Trim();
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    Trim();
                }
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            lock (this.syncRoot)
            {
                return this.m_list.ToList().GetEnumerator();
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this.syncRoot)
            {
                return this.m_list.GetEnumerator();
            }
        }
        protected void Trim()
        {
            lock(this.m_list)
            {
                int newindex = 0;
                T[] newlist = new T[this.m_list.Where(p => p.Equals(default(T)) != false).Count()];
                Parallel.ForEach(this.m_list, p => 
                {
                    if (!p.Equals(default(T)))
                        newlist[newindex++] = p;
                });
                this.m_list = newlist;
                this.count = this.m_list.Where(p => !p.Equals(default(T))).Count();
            }
        }
        public int Add(object value)
        {
            Add((T)value);
            return IndexOf((T)value);
        }
        public bool Contains(object value)
        {
            return Contains((T)value);
        }
        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }
        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }
        public bool IsFixedSize
        {
            get { return this.Capable; }
        }
        public void Remove(object value)
        {
            Remove((T)value);
        }
        object IList.this[int index]
        {
            get
            {
                return (object)this[index];
            }
            set
            {
                this[index] = (T)value;
            }
        }
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
        public bool IsSynchronized
        {
            get { return true; }
        }
        public object SyncRoot
        {
            get { return this.syncRoot; }
        }
        public void Iterate(Action<T> method)
        {
            Parallel.ForEach<T>(this.m_list, method);
        }
    }
}
