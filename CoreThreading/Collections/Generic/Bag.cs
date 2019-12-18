using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.Collections.Generic
{
    public static class Bag
    {
        internal static int CombineHashCodes(int h1, int h2)
        {
             return (((h1 << 5) + h1) ^ h2);
        }
        internal static int CombineHashCodes(int h1, int h2, int h3)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), h3);
        }
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4));
        }
    }
    [Serializable]
    public class Bag<T1> : IStructuralEquatable, IStructuralComparable, IComparable, IBagInternal, IBag, ISerializable, IDeserializationCallback
    {
        protected static readonly List<Type> paramtypes = new List<Type>() { typeof(T1) };
        protected static readonly Func<object, int> GetPlace = (obj) => paramtypes.FindIndex(p => p == obj.GetType());
        private T1[] T1_list;
        private SerializationInfo _siInfo;
        private volatile int nextIndex = -1;
        public int Count { get { return this.nextIndex - 1; } }
        [NonSerialized]
        private object syncRoot;
        public int Length { get { return this.T1_list == null ? 0 : this.T1_list.Length; } }
        public Bag()
        {
            this.syncRoot = new object();
            this.T1_list = new T1[0];
            this.nextIndex = 0;
        }
        protected Bag(IEnumerable<T1> elements)
        {
            this.syncRoot = new object();
            this.T1_list = elements.ToArray();
            this.nextIndex = 0;
        }
        protected Bag(SerializationInfo info, StreamingContext context)
        {
            this._siInfo = info;
        }
        /// <summary>
        /// gets or sets elements in the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="place">zero based indexer for place pointer</param>
        /// <returns></returns>
        public object this[int index, int place = 0]
        {
            get 
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch(place)
                {
                    case 0:
                        {
                            return this.T1_list[index];
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
            set
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch (place)
                {
                    case 0:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T1_list[index] = (T1)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
        }
        public void Foreach(Action<T1> action)
        {
            lock (this.T1_list)
            {
                Parallel.ForEach(this.T1_list, p => action.Invoke(p));
            }
        }
        public void Add(params object[] param)
        {
            lock(this.syncRoot)
            {
                foreach(object item in param)
                {
                    switch(GetPlace(item))
                    {
                        case 0:
                            {
                                Array.Resize<T1>(ref this.T1_list, this.nextIndex);
                                this.T1_list[this.nextIndex] = (T1)item;
                                break;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                this.nextIndex += 1;
                Trim();
            }
        }
        public void Remove(int index)
        {
            if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
            lock (this.syncRoot)
            {
                this.T1_list[index] = default(T1);
                Trim();
            }
        }
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
           if (other == null) return false;

           if (!(other is Bag<T1>))
            {
                return false;
            }

           return comparer.Equals(T1_list.GetHashCode(), (other as Bag<T1>).GetHashCode());
        }
        public int GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(T1_list);
        }
        public int CompareTo(object other, IComparer comparer)
        {
            return ((IStructuralComparable)this).CompareTo(other, comparer);
        }
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null) return 1;

            if (!(other is Bag<T1>))
            {
                throw new ArgumentException("Type of Bag isn't the same for other");
            }

            return comparer.Compare(T1_list, (other as Bag<T1>).T1_list);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ((IBagInternal)this).ToString(sb);
        }
        string IBagInternal.ToString(StringBuilder sb)
        {
            sb.Append(T1_list.ToString());
            sb.Append(')');
            return sb.ToString();
        }
        public int[] IndexsOf(params object[] param)
        {
            lock(this.syncRoot)
            {
                List<int> ids = new List<int>(param.Length);
                foreach (object item in param)
                {
                    switch (GetPlace(item))
                    {
                        case 0:
                            {
                                ids.Add(Array.FindIndex(this.T1_list, p => p.Equals(item)));
                                continue;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                return ids.ToArray();
            }
        }
        protected void Trim()
        {
            lock(this.T1_list)
            {
                List<T1> items = new List<T1>();
                foreach(T1 item in this.T1_list)
                {
                    if (item.Equals(default(T1)))
                        continue;
                }
                this.T1_list = items.ToArray();
                this.nextIndex = this.T1_list.Length;
            }
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("T1_list", this.T1_list, typeof(T1[]));
        }
        public void OnDeserialization(object sender)
        {
            Debug.Assert(this._siInfo == null, "Null value exception", string.Format("{0}"), new ArgumentNullException("info"));
            if (this._siInfo == null)
                return;
            this.T1_list = (T1[])this._siInfo.GetValue("T1_list", typeof(T1[]));
            this.nextIndex = this.T1_list.Length;
            this._siInfo = null;
        }
        public static implicit operator Bag<T1>(T1[] elements)
        {
            return new Bag<T1>(elements.AsEnumerable());
        }
    }
    [Serializable]
    public class Bag<T1, T2> : IStructuralEquatable, IStructuralComparable, IComparable, IBagInternal, IBag, ISerializable, IDeserializationCallback
    {
        protected static readonly List<Type> paramtypes = new List<Type>() { typeof(T1), typeof(T2) };
        protected static readonly Func<object, int> GetPlace = (obj) => paramtypes.FindIndex(p => p == obj.GetType());
        protected T1[] T1_list;
        protected T2[] T2_list;
        private SerializationInfo _siInfo;
        private volatile int nextIndex = -1;
        public int Count { get { return this.nextIndex - 1; } }
        [NonSerialized]
        private object syncRoot;
        public int Length { get { return this.T1_list == null ? 0 : this.T1_list.Length; } }
        public Bag()
        {
            this.syncRoot = new object();
            this.T1_list = new T1[0];
            this.T2_list = new T2[0];
            this.nextIndex = 0;
        }
        protected Bag(SerializationInfo info, StreamingContext context)
        {
            this._siInfo = info;
        }
        /// <summary>
        /// gets or sets elements in the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="place">zero based indexer for place pointer</param>
        /// <returns></returns>
        public object this[int index, int place]
        {
            get
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch (place)
                {
                    case 0:
                        {
                            return this.T1_list[index];
                        }
                    case 1:
                        {
                            return this.T2_list[index];
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
            set
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch (place)
                {
                    case 0:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T1_list[index] = (T1)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 1:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T2_list[index] = (T2)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
        }
        public void Add(params object[] param)
        {
            lock (this.syncRoot)
            {
                foreach (object item in param)
                {
                    switch (GetPlace(item))
                    {
                        case 0:
                            {
                                Array.Resize<T1>(ref this.T1_list, this.nextIndex);
                                this.T1_list[this.nextIndex] = (T1)item;
                                break;
                            }
                        case 1:
                            {
                                Array.Resize<T2>(ref this.T2_list, this.nextIndex);
                                this.T2_list[this.nextIndex] = (T2)item;
                                break;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                this.nextIndex += 1;
                Trim();
            }
        }
        public void Foreach(Action<T1, T2> action)
        {
            lock (this.T1_list)
            {
                for(int x = 0; x < this.Count; x++)
                {
                    action.Invoke(this.T1_list[x], this.T2_list[x]);
                }
            }
        }
        public void Remove(int index)
        {
            if (index > this.nextIndex - 1)
                throw new IndexOutOfRangeException();
            lock (this.syncRoot)
            {
                this.T1_list[index] = default(T1);
                this.T2_list[index] = default(T2);
                Trim();
            }
        }
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null) return false;
            if (!(other is Bag<T1, T2>))
            {
                return false;
            }
            return comparer.Equals(T1_list.GetHashCode(), (other as Bag<T1, T2>).GetHashCode());
        }
        public int GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(T1_list) | comparer.GetHashCode(T2_list);
        }
        public int CompareTo(object other, IComparer comparer)
        {
            return ((IStructuralComparable)this).CompareTo(other, comparer);
        }
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null) return 1;

            if (!(other is Bag<T1, T2>))
            {
                throw new ArgumentException("Type of Bag isn't the same for other");
            }

            return comparer.Compare(GetHashCode(), (other as Bag<T1, T2>).GetHashCode());
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ((IBagInternal)this).ToString(sb);
        }
        string IBagInternal.ToString(StringBuilder sb)
        {
            sb.Append(T1_list.ToString());
            sb.Append('|');
            sb.Append(T2_list.ToString());
            sb.Append(')');
            return sb.ToString();
        }
        public int[] IndexsOf(params object[] param)
        {
            lock (this.syncRoot)
            {
                List<int> ids = new List<int>(param.Length);
                foreach (object item in param)
                {
                    switch (GetPlace(item))
                    {
                        case 0:
                            {
                                ids.Add(Array.FindIndex(this.T1_list, p => p.Equals(item)));
                                continue;
                            }
                        case 1:
                            {
                                ids.Add(Array.FindIndex(this.T2_list, p => p.Equals(item)));
                                continue;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                return ids.ToArray();
            }
        }
        protected void Trim()
        {
            int mostcount = 0;
            lock (this.T1_list)
            {
                List<T1> items = new List<T1>();
                foreach (T1 item in this.T1_list)
                {
                    if (item.Equals(default(T1)))
                        continue;
                }
                this.T1_list = items.ToArray();
                if (this.T1_list.Length > mostcount)
                    mostcount = this.T1_list.Length;
            }
            lock(this.T2_list)
            {
                List<T2> items = new List<T2>();
                foreach (T2 item in this.T2_list)
                {
                    if (item.Equals(default(T2)))
                        continue;
                }
                this.T2_list = items.ToArray();
                if (this.T2_list.Length > mostcount)
                    mostcount = this.T2_list.Length;
            }
            if (this.T1_list.Length < mostcount)
                Array.Resize<T1>(ref this.T1_list, mostcount);
            if (this.T2_list.Length < mostcount)
                Array.Resize<T2>(ref this.T2_list, mostcount);
            this.nextIndex = mostcount;
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("T1_list", this.T1_list, typeof(T1[]));
            info.AddValue("T2_list", this.T2_list, typeof(T2[]));
        }
        public void OnDeserialization(object sender)
        {
            Debug.Assert(this._siInfo == null, "Null value exception", string.Format("{0}"), new ArgumentNullException("info"));
            if (this._siInfo == null)
                return;
            this.T1_list = (T1[])this._siInfo.GetValue("T1_list", typeof(T1[]));
            this.T2_list = (T2[])this._siInfo.GetValue("T2_list", typeof(T2[]));
            this.nextIndex = this.T1_list.Length;
            this._siInfo = null;
        }
    }
    [Serializable]
    public class Bag<T1, T2, T3> : IStructuralEquatable, IStructuralComparable, IComparable, IBagInternal, IBag, ISerializable, IDeserializationCallback
    {
        protected static readonly List<Type> paramtypes = new List<Type>() { typeof(T1), typeof(T2), typeof(T3) };
        protected static readonly Func<object, int> GetPlace = (obj) => paramtypes.FindIndex(p => p == obj.GetType());
        protected T1[] T1_list;
        protected T2[] T2_list;
        protected T3[] T3_list;
        private SerializationInfo _siInfo;
        private volatile int nextIndex = -1;
        public int Count { get { return this.nextIndex - 1; } }
        [NonSerialized]
        private object syncRoot;
        public int Length { get { return this.T1_list == null ? 0 : this.T1_list.Length; } }
        public Bag()
        {
            this.syncRoot = new object();
            this.T1_list = new T1[0];
            this.T2_list = new T2[0];
            this.T3_list = new T3[0];
            this.nextIndex = 0;
        }
        protected Bag(SerializationInfo info, StreamingContext context)
        {
            this._siInfo = info;
        }
        /// <summary>
        /// gets or sets elements in the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="place">zero based indexer for place pointer</param>
        /// <returns></returns>
        public object this[int index, int place]
        {
            get
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch (place)
                {
                    case 0:
                        {
                            return this.T1_list[index];
                        }
                    case 1:
                        {
                            return this.T2_list[index];
                        }
                    case 2:
                        {
                            return this.T3_list[index];
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
            set
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch (place)
                {
                    case 0:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T1_list[index] = (T1)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 1:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T2_list[index] = (T2)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 2:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T3_list[index] = (T3)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
        }
        public void Foreach(Action<T1, T2, T3> action)
        {
            lock (this.T1_list)
            {
                for (int x = 0; x < this.Count; x++)
                {
                    action.Invoke(this.T1_list[x], this.T2_list[x], this.T3_list[x]);
                }
            }
        }
        public void Add(params object[] param)
        {
            lock (this.syncRoot)
            {
                foreach (object item in param)
                {
                    switch (GetPlace(item))
                    {
                        case 0:
                            {
                                Array.Resize<T1>(ref this.T1_list, this.nextIndex);
                                this.T1_list[this.nextIndex] = (T1)item;
                                break;
                            }
                        case 1:
                            {
                                Array.Resize<T2>(ref this.T2_list, this.nextIndex);
                                this.T2_list[this.nextIndex] = (T2)item;
                                break;
                            }
                        case 2:
                            {
                                Array.Resize<T3>(ref this.T3_list, this.nextIndex);
                                this.T3_list[this.nextIndex] = (T3)item;
                                break;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                this.nextIndex += 1;
                Trim();
            }
        }
        public void Remove(int index)
        {
            if (index > this.nextIndex - 1)
                throw new IndexOutOfRangeException();
            lock (this.syncRoot)
            {
                this.T1_list[index] = default(T1);
                this.T2_list[index] = default(T2);
                this.T3_list[index] = default(T3);
                Trim();
            }
        }
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null) return false;
            if (!(other is Bag<T1, T2, T3>))
            {
                return false;
            }
            return comparer.Equals(T1_list.GetHashCode(), (other as Bag<T1, T2, T3>).GetHashCode());
        }
        public int GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(T1_list) | comparer.GetHashCode(T2_list) | comparer.GetHashCode(T3_list);
        }
        public int CompareTo(object other, IComparer comparer)
        {
            return ((IStructuralComparable)this).CompareTo(other, comparer);
        }
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null) return 1;

            if (!(other is Bag<T1, T2, T3>))
            {
                throw new ArgumentException("Type of Bag isn't the same for other");
            }

            return comparer.Compare(GetHashCode(), (other as Bag<T1, T2, T3>).GetHashCode());
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ((IBagInternal)this).ToString(sb);
        }
        string IBagInternal.ToString(StringBuilder sb)
        {
            sb.Append(T1_list.ToString());
            sb.Append('|');
            sb.Append(T2_list.ToString());
            sb.Append('|');
            sb.Append(T3_list.ToString());
            sb.Append(')');
            return sb.ToString();
        }
        public int[] IndexsOf(params object[] param)
        {
            lock (this.syncRoot)
            {
                List<int> ids = new List<int>(param.Length);
                foreach (object item in param)
                {
                    switch (GetPlace(item))
                    {
                        case 0:
                            {
                                ids.Add(Array.FindIndex(this.T1_list, p => p.Equals(item)));
                                continue;
                            }
                        case 1:
                            {
                                ids.Add(Array.FindIndex(this.T2_list, p => p.Equals(item)));
                                continue;
                            }
                        case 2:
                            {
                                ids.Add(Array.FindIndex(this.T3_list, p => p.Equals(item)));
                                continue;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                return ids.ToArray();
            }
        }
        protected void Trim()
        {
            int mostcount = 0;
            lock (this.T1_list)
            {
                List<T1> items = new List<T1>();
                foreach (T1 item in this.T1_list)
                {
                    if (item.Equals(default(T1)))
                        continue;
                }
                this.T1_list = items.ToArray();
                if (this.T1_list.Length > mostcount)
                    mostcount = this.T1_list.Length;
            }
            lock (this.T2_list)
            {
                List<T2> items = new List<T2>();
                foreach (T2 item in this.T2_list)
                {
                    if (item.Equals(default(T2)))
                        continue;
                }
                this.T2_list = items.ToArray();
                if (this.T2_list.Length > mostcount)
                    mostcount = this.T2_list.Length;
            }
            lock (this.T3_list)
            {
                List<T3> items = new List<T3>();
                foreach (T3 item in this.T3_list)
                {
                    if (item.Equals(default(T3)))
                        continue;
                }
                this.T3_list = items.ToArray();
                if (this.T3_list.Length > mostcount)
                    mostcount = this.T3_list.Length;
            }
            if (this.T1_list.Length < mostcount)
                Array.Resize<T1>(ref this.T1_list, mostcount);
            if (this.T2_list.Length < mostcount)
                Array.Resize<T2>(ref this.T2_list, mostcount);
            if (this.T3_list.Length < mostcount)
                Array.Resize<T3>(ref this.T3_list, mostcount);
            this.nextIndex = mostcount;
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("T1_list", this.T1_list, typeof(T1[]));
            info.AddValue("T2_list", this.T2_list, typeof(T2[]));
            info.AddValue("T3_list", this.T3_list, typeof(T3[]));
        }
        public void OnDeserialization(object sender)
        {
            Debug.Assert(this._siInfo == null, "Null value exception", string.Format("{0}"), new ArgumentNullException("info"));
            if (this._siInfo == null)
                return;
            this.T1_list = (T1[])this._siInfo.GetValue("T1_list", typeof(T1[]));
            this.T2_list = (T2[])this._siInfo.GetValue("T2_list", typeof(T2[]));
            this.T3_list = (T3[])this._siInfo.GetValue("T3_list", typeof(T3[]));
            this.nextIndex = this.T1_list.Length;
            this._siInfo = null;
        }
    }
    [Serializable]
    public class Bag<T1, T2, T3, T4> : IStructuralEquatable, IStructuralComparable, IComparable, IBagInternal, IBag, ISerializable, IDeserializationCallback
    {
        protected static readonly List<Type> paramtypes = new List<Type>() { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
        protected static readonly Func<object, int> GetPlace = (obj) => paramtypes.FindIndex(p => p == obj.GetType());
        protected T1[] T1_list;
        protected T2[] T2_list;
        protected T3[] T3_list;
        protected T4[] T4_list;
        private SerializationInfo _siInfo;
        private volatile int nextIndex = -1;
        public int Count { get { return this.nextIndex - 1; } }
        [NonSerialized]
        private object syncRoot;
        public int Length { get { return this.T1_list == null ? 0 : this.T1_list.Length; } }
        public Bag()
        {
            this.syncRoot = new object();
            this.T1_list = new T1[0];
            this.T2_list = new T2[0];
            this.T3_list = new T3[0];
            this.T4_list = new T4[0];
            this.nextIndex = 0;
        }
        protected Bag(SerializationInfo info, StreamingContext context)
        {
            this._siInfo = info;
        }
        /// <summary>
        /// gets or sets elements in the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="place">zero based indexer for place pointer</param>
        /// <returns></returns>
        public object this[int index, int place]
        {
            get
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch (place)
                {
                    case 0:
                        {
                            return this.T1_list[index];
                        }
                    case 1:
                        {
                            return this.T2_list[index];
                        }
                    case 2:
                        {
                            return this.T3_list[index];
                        }
                    case 3:
                        {
                            return this.T4_list[index];
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
            set
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch (place)
                {
                    case 0:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T1_list[index] = (T1)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 1:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T2_list[index] = (T2)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 2:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T3_list[index] = (T3)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 3:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T4_list[index] = (T4)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
        }
        public void Add(params object[] param)
        {
            lock (this.syncRoot)
            {
                foreach (object item in param)
                {
                    switch (GetPlace(item))
                    {
                        case 0:
                            {
                                Array.Resize<T1>(ref this.T1_list, this.nextIndex);
                                this.T1_list[this.nextIndex] = (T1)item;
                                break;
                            }
                        case 1:
                            {
                                Array.Resize<T2>(ref this.T2_list, this.nextIndex);
                                this.T2_list[this.nextIndex] = (T2)item;
                                break;
                            }
                        case 2:
                            {
                                Array.Resize<T3>(ref this.T3_list, this.nextIndex);
                                this.T3_list[this.nextIndex] = (T3)item;
                                break;
                            }
                        case 3:
                            {
                                Array.Resize<T4>(ref this.T4_list, this.nextIndex);
                                this.T4_list[this.nextIndex] = (T4)item;
                                break;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                this.nextIndex += 1;
                Trim();
            }
        }
        public void Foreach(Action<T1, T2, T3, T4> action)
        {
            lock (this.T1_list)
            {
                for (int x = 0; x < this.Count; x++)
                {
                    action.Invoke(this.T1_list[x], this.T2_list[x], this.T3_list[x], this.T4_list[x]);
                }
            }
        }
        public void Remove(int index)
        {
            if (index > this.nextIndex - 1)
                throw new IndexOutOfRangeException();
            lock (this.syncRoot)
            {
                this.T1_list[index] = default(T1);
                this.T2_list[index] = default(T2);
                this.T3_list[index] = default(T3);
                this.T4_list[index] = default(T4);
                Trim();
            }
        }
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null) return false;
            if (!(other is Bag<T1, T2, T3, T4>))
            {
                return false;
            }
            return comparer.Equals(T1_list.GetHashCode(), (other as Bag<T1, T2, T3, T4>).GetHashCode());
        }
        public int GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(T1_list) | comparer.GetHashCode(T2_list) | comparer.GetHashCode(T3_list) | comparer.GetHashCode(T4_list);
        }
        public int CompareTo(object other, IComparer comparer)
        {
            return ((IStructuralComparable)this).CompareTo(other, comparer);
        }
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null) return 1;

            if (!(other is Bag<T1, T2, T3, T4>))
            {
                throw new ArgumentException("Type of Bag isn't the same for other");
            }

            return comparer.Compare(GetHashCode(), (other as Bag<T1, T2, T3, T4>).GetHashCode());
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ((IBagInternal)this).ToString(sb);
        }
        string IBagInternal.ToString(StringBuilder sb)
        {
            sb.Append(T1_list.ToString());
            sb.Append('|');
            sb.Append(T2_list.ToString());
            sb.Append('|');
            sb.Append(T3_list.ToString());
            sb.Append('|');
            sb.Append(T4_list.ToString());
            sb.Append(')');
            return sb.ToString();
        }
        public int[] IndexsOf(params object[] param)
        {
            lock (this.syncRoot)
            {
                List<int> ids = new List<int>(param.Length);
                foreach (object item in param)
                {
                    switch (GetPlace(item))
                    {
                        case 0:
                            {
                                ids.Add(Array.FindIndex(this.T1_list, p => p.Equals(item)));
                                continue;
                            }
                        case 1:
                            {
                                ids.Add(Array.FindIndex(this.T2_list, p => p.Equals(item)));
                                continue;
                            }
                        case 2:
                            {
                                ids.Add(Array.FindIndex(this.T3_list, p => p.Equals(item)));
                                continue;
                            }
                        case 3:
                            {
                                ids.Add(Array.FindIndex(this.T4_list, p => p.Equals(item)));
                                continue;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                return ids.ToArray();
            }
        }
        protected void Trim()
        {
            int mostcount = 0;
            lock (this.T1_list)
            {
                List<T1> items = new List<T1>();
                foreach (T1 item in this.T1_list)
                {
                    if (item.Equals(default(T1)))
                        continue;
                }
                this.T1_list = items.ToArray();
                if (this.T1_list.Length > mostcount)
                    mostcount = this.T1_list.Length;
            }
            lock (this.T2_list)
            {
                List<T2> items = new List<T2>();
                foreach (T2 item in this.T2_list)
                {
                    if (item.Equals(default(T2)))
                        continue;
                }
                this.T2_list = items.ToArray();
                if (this.T2_list.Length > mostcount)
                    mostcount = this.T2_list.Length;
            }
            lock (this.T3_list)
            {
                List<T3> items = new List<T3>();
                foreach (T3 item in this.T3_list)
                {
                    if (item.Equals(default(T3)))
                        continue;
                }
                this.T3_list = items.ToArray();
                if (this.T3_list.Length > mostcount)
                    mostcount = this.T3_list.Length;
            }
            lock (this.T4_list)
            {
                List<T4> items = new List<T4>();
                foreach (T4 item in this.T4_list)
                {
                    if (item.Equals(default(T4)))
                        continue;
                }
                this.T4_list = items.ToArray();
                if (this.T4_list.Length > mostcount)
                    mostcount = this.T4_list.Length;
            }
            if (this.T1_list.Length < mostcount)
                Array.Resize<T1>(ref this.T1_list, mostcount);
            if (this.T2_list.Length < mostcount)
                Array.Resize<T2>(ref this.T2_list, mostcount);
            if (this.T3_list.Length < mostcount)
                Array.Resize<T3>(ref this.T3_list, mostcount);
            if (this.T4_list.Length < mostcount)
                Array.Resize<T4>(ref this.T4_list, mostcount);
            this.nextIndex = mostcount;
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("T1_list", this.T1_list, typeof(T1[]));
            info.AddValue("T2_list", this.T2_list, typeof(T2[]));
            info.AddValue("T3_list", this.T3_list, typeof(T3[]));
            info.AddValue("T4_list", this.T4_list, typeof(T4[]));
        }
        public void OnDeserialization(object sender)
        {
            Debug.Assert(this._siInfo == null, "Null value exception", string.Format("{0}"), new ArgumentNullException("info"));
            if (this._siInfo == null)
                return;
            this.T1_list = (T1[])this._siInfo.GetValue("T1_list", typeof(T1[]));
            this.T2_list = (T2[])this._siInfo.GetValue("T2_list", typeof(T2[]));
            this.T3_list = (T3[])this._siInfo.GetValue("T3_list", typeof(T3[]));
            this.T4_list = (T4[])this._siInfo.GetValue("T4_list", typeof(T4[]));
            this.nextIndex = this.T1_list.Length;
            this._siInfo = null;
        }
    }
    [Serializable]
    public class Bag<T1, T2, T3, T4, T5> : IStructuralEquatable, IStructuralComparable, IComparable, IBagInternal, IBag, ISerializable, IDeserializationCallback
    {
        protected static readonly List<Type> paramtypes = new List<Type>() { typeof(T1), typeof(T2), typeof(T3), typeof(T4)
        , typeof(T5)};
        protected static readonly Func<object, int> GetPlace = (obj) => paramtypes.FindIndex(p => p == obj.GetType());
        protected T1[] T1_list;
        protected T2[] T2_list;
        protected T3[] T3_list;
        protected T4[] T4_list;
        protected T5[] T5_list;
        private SerializationInfo _siInfo;
        private volatile int nextIndex = -1;
        public int Count { get { return this.nextIndex - 1; } }
        [NonSerialized]
        private object syncRoot;
        public int Length { get { return this.T1_list == null ? 0 : this.T1_list.Length; } }
        public Bag()
        {
            this.syncRoot = new object();
            this.T1_list = new T1[0];
            this.T2_list = new T2[0];
            this.T3_list = new T3[0];
            this.T4_list = new T4[0];
            this.T5_list = new T5[0];
            this.nextIndex = 0;
        }
        protected Bag(SerializationInfo info, StreamingContext context)
        {
            this._siInfo = info;
        }
        /// <summary>
        /// gets or sets elements in the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="place">zero based indexer for place pointer</param>
        /// <returns></returns>
        public object this[int index, int place]
        {
            get
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch (place)
                {
                    case 0:
                        {
                            return this.T1_list[index];
                        }
                    case 1:
                        {
                            return this.T2_list[index];
                        }
                    case 2:
                        {
                            return this.T3_list[index];
                        }
                    case 3:
                        {
                            return this.T4_list[index];
                        }
                    case 4:
                        {
                            return this.T5_list[index];
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
            set
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch (place)
                {
                    case 0:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T1_list[index] = (T1)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 1:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T2_list[index] = (T2)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 2:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T3_list[index] = (T3)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 3:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T4_list[index] = (T4)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 4:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T5_list[index] = (T5)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
        }
        public void Add(params object[] param)
        {
            lock (this.syncRoot)
            {
                foreach (object item in param)
                {
                    switch (GetPlace(item))
                    {
                        case 0:
                            {
                                Array.Resize<T1>(ref this.T1_list, this.nextIndex);
                                this.T1_list[this.nextIndex] = (T1)item;
                                break;
                            }
                        case 1:
                            {
                                Array.Resize<T2>(ref this.T2_list, this.nextIndex);
                                this.T2_list[this.nextIndex] = (T2)item;
                                break;
                            }
                        case 2:
                            {
                                Array.Resize<T3>(ref this.T3_list, this.nextIndex);
                                this.T3_list[this.nextIndex] = (T3)item;
                                break;
                            }
                        case 3:
                            {
                                Array.Resize<T4>(ref this.T4_list, this.nextIndex);
                                this.T4_list[this.nextIndex] = (T4)item;
                                break;
                            }
                        case 4:
                            {
                                Array.Resize<T5>(ref this.T5_list, this.nextIndex);
                                this.T5_list[this.nextIndex] = (T5)item;
                                break;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                this.nextIndex += 1;
                Trim();
            }
        }
        public void Foreach(Action<T1, T2, T3, T4, T5> action)
        {
            lock (this.T1_list)
            {
                for (int x = 0; x < this.Count; x++)
                {
                    action.Invoke(this.T1_list[x], this.T2_list[x], this.T3_list[x], this.T4_list[x]
                        , this.T5_list[x]);
                }
            }
        }
        public void Remove(int index)
        {
            if (index > this.nextIndex - 1)
                throw new IndexOutOfRangeException();
            lock (this.syncRoot)
            {
                this.T1_list[index] = default(T1);
                this.T2_list[index] = default(T2);
                this.T3_list[index] = default(T3);
                this.T4_list[index] = default(T4);
                this.T5_list[index] = default(T5);
                Trim();
            }
        }
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null) return false;
            if (!(other is Bag<T1, T2, T3, T4, T5>))
            {
                return false;
            }
            return comparer.Equals(T1_list.GetHashCode(), (other as Bag<T1, T2, T3, T4, T5>).GetHashCode());
        }
        public int GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(T1_list) | comparer.GetHashCode(T2_list) | comparer.GetHashCode(T3_list) | comparer.GetHashCode(T4_list)
                | comparer.GetHashCode(T5_list);
        }
        public int CompareTo(object other, IComparer comparer)
        {
            return ((IStructuralComparable)this).CompareTo(other, comparer);
        }
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null) return 1;

            if (!(other is Bag<T1, T2, T3, T4, T5>))
            {
                throw new ArgumentException("Type of Bag isn't the same for other");
            }

            return comparer.Compare(GetHashCode(), (other as Bag<T1, T2, T3, T4, T5>).GetHashCode());
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ((IBagInternal)this).ToString(sb);
        }
        string IBagInternal.ToString(StringBuilder sb)
        {
            sb.Append(T1_list.ToString());
            sb.Append('|');
            sb.Append(T2_list.ToString());
            sb.Append('|');
            sb.Append(T3_list.ToString());
            sb.Append('|');
            sb.Append(T4_list.ToString());
            sb.Append('|');
            sb.Append(T5_list.ToString());
            sb.Append(')');
            return sb.ToString();
        }
        public int[] IndexsOf(params object[] param)
        {
            lock (this.syncRoot)
            {
                List<int> ids = new List<int>(param.Length);
                foreach (object item in param)
                {
                    switch (GetPlace(item))
                    {
                        case 0:
                            {
                                ids.Add(Array.FindIndex(this.T1_list, p => p.Equals(item)));
                                continue;
                            }
                        case 1:
                            {
                                ids.Add(Array.FindIndex(this.T2_list, p => p.Equals(item)));
                                continue;
                            }
                        case 2:
                            {
                                ids.Add(Array.FindIndex(this.T3_list, p => p.Equals(item)));
                                continue;
                            }
                        case 3:
                            {
                                ids.Add(Array.FindIndex(this.T4_list, p => p.Equals(item)));
                                continue;
                            }
                        case 4:
                            {
                                ids.Add(Array.FindIndex(this.T5_list, p => p.Equals(item)));
                                continue;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                return ids.ToArray();
            }
        }
        protected void Trim()
        {
            int mostcount = 0;
            lock (this.T1_list)
            {
                List<T1> items = new List<T1>();
                foreach (T1 item in this.T1_list)
                {
                    if (item.Equals(default(T1)))
                        continue;
                }
                this.T1_list = items.ToArray();
                if (this.T1_list.Length > mostcount)
                    mostcount = this.T1_list.Length;
            }
            lock (this.T2_list)
            {
                List<T2> items = new List<T2>();
                foreach (T2 item in this.T2_list)
                {
                    if (item.Equals(default(T2)))
                        continue;
                }
                this.T2_list = items.ToArray();
                if (this.T2_list.Length > mostcount)
                    mostcount = this.T2_list.Length;
            }
            lock (this.T3_list)
            {
                List<T3> items = new List<T3>();
                foreach (T3 item in this.T3_list)
                {
                    if (item.Equals(default(T3)))
                        continue;
                }
                this.T3_list = items.ToArray();
                if (this.T3_list.Length > mostcount)
                    mostcount = this.T3_list.Length;
            }
            lock (this.T4_list)
            {
                List<T4> items = new List<T4>();
                foreach (T4 item in this.T4_list)
                {
                    if (item.Equals(default(T4)))
                        continue;
                }
                this.T4_list = items.ToArray();
                if (this.T4_list.Length > mostcount)
                    mostcount = this.T4_list.Length;
            }
            lock (this.T5_list)
            {
                List<T5> items = new List<T5>();
                foreach (T5 item in this.T5_list)
                {
                    if (item.Equals(default(T5)))
                        continue;
                }
                this.T5_list = items.ToArray();
                if (this.T5_list.Length > mostcount)
                    mostcount = this.T5_list.Length;
            }
            if (this.T1_list.Length < mostcount)
                Array.Resize<T1>(ref this.T1_list, mostcount);
            if (this.T2_list.Length < mostcount)
                Array.Resize<T2>(ref this.T2_list, mostcount);
            if (this.T3_list.Length < mostcount)
                Array.Resize<T3>(ref this.T3_list, mostcount);
            if (this.T4_list.Length < mostcount)
                Array.Resize<T4>(ref this.T4_list, mostcount);
            if (this.T5_list.Length < mostcount)
                Array.Resize<T5>(ref this.T5_list, mostcount);
            this.nextIndex = mostcount;
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("T1_list", this.T1_list, typeof(T1[]));
            info.AddValue("T2_list", this.T2_list, typeof(T2[]));
            info.AddValue("T3_list", this.T3_list, typeof(T3[]));
            info.AddValue("T4_list", this.T4_list, typeof(T4[]));
            info.AddValue("T5_list", this.T5_list, typeof(T5[]));
        }
        public void OnDeserialization(object sender)
        {
            Debug.Assert(this._siInfo == null, "Null value exception", string.Format("{0}"), new ArgumentNullException("info"));
            if (this._siInfo == null)
                return;
            this.T1_list = (T1[])this._siInfo.GetValue("T1_list", typeof(T1[]));
            this.T2_list = (T2[])this._siInfo.GetValue("T2_list", typeof(T2[]));
            this.T3_list = (T3[])this._siInfo.GetValue("T3_list", typeof(T3[]));
            this.T4_list = (T4[])this._siInfo.GetValue("T4_list", typeof(T4[]));
            this.T5_list = (T5[])this._siInfo.GetValue("T5_list", typeof(T5[]));
            this.nextIndex = this.T1_list.Length;
            this._siInfo = null;
        }
    }
    [Serializable]
    public class Bag<T1, T2, T3, T4, T5, T6> : IStructuralEquatable, IStructuralComparable, IComparable, IBagInternal, IBag, ISerializable, IDeserializationCallback
    {
        protected static readonly List<Type> paramtypes = new List<Type>() { typeof(T1), typeof(T2), typeof(T3), typeof(T4)
        , typeof(T5), typeof(T6)};
        protected static readonly Func<object, int> GetPlace = (obj) => paramtypes.FindIndex(p => p == obj.GetType());
        protected T1[] T1_list;
        protected T2[] T2_list;
        protected T3[] T3_list;
        protected T4[] T4_list;
        protected T5[] T5_list;
        protected T6[] T6_list;
        private SerializationInfo _siInfo;
        private volatile int nextIndex = -1;
        public int Count { get { return this.nextIndex - 1; } }
        [NonSerialized]
        private object syncRoot;
        public int Length { get { return this.T1_list == null ? 0 : this.T1_list.Length; } }
        public Bag()
        {
            this.syncRoot = new object();
            this.T1_list = new T1[0];
            this.T2_list = new T2[0];
            this.T3_list = new T3[0];
            this.T4_list = new T4[0];
            this.T5_list = new T5[0];
            this.T6_list = new T6[0];
            this.nextIndex = 0;
        }
        protected Bag(SerializationInfo info, StreamingContext context)
        {
            this._siInfo = info;
        }
        /// <summary>
        /// gets or sets elements in the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="place">zero based indexer for place pointer</param>
        /// <returns></returns>
        public object this[int index, int place]
        {
            get
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch (place)
                {
                    case 0:
                        {
                            return this.T1_list[index];
                        }
                    case 1:
                        {
                            return this.T2_list[index];
                        }
                    case 2:
                        {
                            return this.T3_list[index];
                        }
                    case 3:
                        {
                            return this.T4_list[index];
                        }
                    case 4:
                        {
                            return this.T5_list[index];
                        }
                    case 5:
                        {
                            return this.T6_list[index];
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
            set
            {
                if (index > this.nextIndex - 1)
                    throw new IndexOutOfRangeException();
                switch (place)
                {
                    case 0:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T1_list[index] = (T1)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 1:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T2_list[index] = (T2)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 2:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T3_list[index] = (T3)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 3:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T4_list[index] = (T4)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 4:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T5_list[index] = (T5)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    case 5:
                        {
                            if (index <= this.nextIndex - 1)
                            {
                                this.T6_list[index] = (T6)value;
                                break;
                            }
                            Add(value);
                            break;
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }
        }
        public void Add(params object[] param)
        {
            lock (this.syncRoot)
            {
                foreach (object item in param)
                {
                    switch (GetPlace(item))
                    {
                        case 0:
                            {
                                Array.Resize<T1>(ref this.T1_list, this.nextIndex);
                                this.T1_list[this.nextIndex] = (T1)item;
                                break;
                            }
                        case 1:
                            {
                                Array.Resize<T2>(ref this.T2_list, this.nextIndex);
                                this.T2_list[this.nextIndex] = (T2)item;
                                break;
                            }
                        case 2:
                            {
                                Array.Resize<T3>(ref this.T3_list, this.nextIndex);
                                this.T3_list[this.nextIndex] = (T3)item;
                                break;
                            }
                        case 3:
                            {
                                Array.Resize<T4>(ref this.T4_list, this.nextIndex);
                                this.T4_list[this.nextIndex] = (T4)item;
                                break;
                            }
                        case 4:
                            {
                                Array.Resize<T5>(ref this.T5_list, this.nextIndex);
                                this.T5_list[this.nextIndex] = (T5)item;
                                break;
                            }
                        case 5:
                            {
                                Array.Resize<T6>(ref this.T6_list, this.nextIndex);
                                this.T6_list[this.nextIndex] = (T6)item;
                                break;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                this.nextIndex += 1;
                Trim();
            }
        }
        public void Foreach(Action<T1, T2, T3, T4, T5, T6> action)
        {
            lock (this.T1_list)
            {
                for (int x = 0; x < this.Count; x++)
                {
                    action.Invoke(this.T1_list[x], this.T2_list[x], this.T3_list[x], this.T4_list[x]
                        , this.T5_list[x], this.T6_list[x]);
                }
            }
        }
        public void Remove(int index)
        {
            if (index > this.nextIndex - 1)
                throw new IndexOutOfRangeException();
            lock (this.syncRoot)
            {
                this.T1_list[index] = default(T1);
                this.T2_list[index] = default(T2);
                this.T3_list[index] = default(T3);
                this.T4_list[index] = default(T4);
                this.T5_list[index] = default(T5);
                this.T6_list[index] = default(T6);
                Trim();
            }
        }
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null) return false;
            if (!(other is Bag<T1, T2, T3, T4, T5, T6>))
            {
                return false;
            }
            return comparer.Equals(T1_list.GetHashCode(), (other as Bag<T1, T2, T3, T4, T5, T6>).GetHashCode());
        }
        public int GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(T1_list) | comparer.GetHashCode(T2_list) | comparer.GetHashCode(T3_list) | comparer.GetHashCode(T4_list)
                | comparer.GetHashCode(T5_list) | comparer.GetHashCode(T6_list);
        }
        public int CompareTo(object other, IComparer comparer)
        {
            return ((IStructuralComparable)this).CompareTo(other, comparer);
        }
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null) return 1;

            if (!(other is Bag<T1, T2, T3, T4, T5, T6>))
            {
                throw new ArgumentException("Type of Bag isn't the same for other");
            }

            return comparer.Compare(GetHashCode(), (other as Bag<T1, T2, T3, T4, T5, T6>).GetHashCode());
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ((IBagInternal)this).ToString(sb);
        }
        string IBagInternal.ToString(StringBuilder sb)
        {
            sb.Append(T1_list.ToString());
            sb.Append('|');
            sb.Append(T2_list.ToString());
            sb.Append('|');
            sb.Append(T3_list.ToString());
            sb.Append('|');
            sb.Append(T4_list.ToString());
            sb.Append('|');
            sb.Append(T5_list.ToString());
            sb.Append('|');
            sb.Append(T6_list.ToString());
            sb.Append(')');
            return sb.ToString();
        }
        public int[] IndexsOf(params object[] param)
        {
            lock (this.syncRoot)
            {
                List<int> ids = new List<int>(param.Length);
                foreach (object item in param)
                {
                    switch (GetPlace(item))
                    {
                        case 0:
                            {
                                ids.Add(Array.FindIndex(this.T1_list, p => p.Equals(item)));
                                continue;
                            }
                        case 1:
                            {
                                ids.Add(Array.FindIndex(this.T2_list, p => p.Equals(item)));
                                continue;
                            }
                        case 2:
                            {
                                ids.Add(Array.FindIndex(this.T3_list, p => p.Equals(item)));
                                continue;
                            }
                        case 3:
                            {
                                ids.Add(Array.FindIndex(this.T4_list, p => p.Equals(item)));
                                continue;
                            }
                        case 4:
                            {
                                ids.Add(Array.FindIndex(this.T5_list, p => p.Equals(item)));
                                continue;
                            }
                        case 5:
                            {
                                ids.Add(Array.FindIndex(this.T6_list, p => p.Equals(item)));
                                continue;
                            }
                        default:
                            throw new ArgumentException(string.Format("{0} type isn't valid for this class.", item.GetType()));
                    }
                }
                return ids.ToArray();
            }
        }
        protected void Trim()
        {
            int mostcount = 0;
            lock (this.T1_list)
            {
                List<T1> items = new List<T1>();
                foreach (T1 item in this.T1_list)
                {
                    if (item.Equals(default(T1)))
                        continue;
                }
                this.T1_list = items.ToArray();
                if (this.T1_list.Length > mostcount)
                    mostcount = this.T1_list.Length;
            }
            lock (this.T2_list)
            {
                List<T2> items = new List<T2>();
                foreach (T2 item in this.T2_list)
                {
                    if (item.Equals(default(T2)))
                        continue;
                }
                this.T2_list = items.ToArray();
                if (this.T2_list.Length > mostcount)
                    mostcount = this.T2_list.Length;
            }
            lock (this.T3_list)
            {
                List<T3> items = new List<T3>();
                foreach (T3 item in this.T3_list)
                {
                    if (item.Equals(default(T3)))
                        continue;
                }
                this.T3_list = items.ToArray();
                if (this.T3_list.Length > mostcount)
                    mostcount = this.T3_list.Length;
            }
            lock (this.T4_list)
            {
                List<T4> items = new List<T4>();
                foreach (T4 item in this.T4_list)
                {
                    if (item.Equals(default(T4)))
                        continue;
                }
                this.T4_list = items.ToArray();
                if (this.T4_list.Length > mostcount)
                    mostcount = this.T4_list.Length;
            }
            lock (this.T5_list)
            {
                List<T5> items = new List<T5>();
                foreach (T5 item in this.T5_list)
                {
                    if (item.Equals(default(T5)))
                        continue;
                }
                this.T5_list = items.ToArray();
                if (this.T5_list.Length > mostcount)
                    mostcount = this.T5_list.Length;
            }
            lock (this.T6_list)
            {
                List<T6> items = new List<T6>();
                foreach (T6 item in this.T6_list)
                {
                    if (item.Equals(default(T6)))
                        continue;
                }
                this.T6_list = items.ToArray();
                if (this.T6_list.Length > mostcount)
                    mostcount = this.T6_list.Length;
            }
            if (this.T1_list.Length < mostcount)
                Array.Resize<T1>(ref this.T1_list, mostcount);
            if (this.T2_list.Length < mostcount)
                Array.Resize<T2>(ref this.T2_list, mostcount);
            if (this.T3_list.Length < mostcount)
                Array.Resize<T3>(ref this.T3_list, mostcount);
            if (this.T4_list.Length < mostcount)
                Array.Resize<T4>(ref this.T4_list, mostcount);
            if (this.T5_list.Length < mostcount)
                Array.Resize<T5>(ref this.T5_list, mostcount);
            if (this.T6_list.Length < mostcount)
                Array.Resize<T6>(ref this.T6_list, mostcount);
            this.nextIndex = mostcount;
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("T1_list", this.T1_list, typeof(T1[]));
            info.AddValue("T2_list", this.T2_list, typeof(T2[]));
            info.AddValue("T3_list", this.T3_list, typeof(T3[]));
            info.AddValue("T4_list", this.T4_list, typeof(T4[]));
            info.AddValue("T5_list", this.T5_list, typeof(T5[]));
            info.AddValue("T6_list", this.T6_list, typeof(T6[]));
        }
        public void OnDeserialization(object sender)
        {
            Debug.Assert(this._siInfo == null, "Null value exception", string.Format("{0}"), new ArgumentNullException("info"));
            if (this._siInfo == null)
                return;
            this.T1_list = (T1[])this._siInfo.GetValue("T1_list", typeof(T1[]));
            this.T2_list = (T2[])this._siInfo.GetValue("T2_list", typeof(T2[]));
            this.T3_list = (T3[])this._siInfo.GetValue("T3_list", typeof(T3[]));
            this.T4_list = (T4[])this._siInfo.GetValue("T4_list", typeof(T4[]));
            this.T5_list = (T5[])this._siInfo.GetValue("T5_list", typeof(T5[]));
            this.T6_list = (T6[])this._siInfo.GetValue("T6_list", typeof(T6[]));
            this.nextIndex = this.T1_list.Length;
            this._siInfo = null;
        }
    }
}
