using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.Collections.Concurrent
{
    public class CustomContainer<TKEY, mAction>
        where TKEY : class
        where mAction : class
    {
        private object syncRoot;
        private TKEY[] keys;
        private mAction[] Values;
        private bool[] ToRemove;
        private int Indexer;
        private int NextIndex { get { return Indexer - 1; } }
        public CustomContainer()
        {
            this.syncRoot = new object();
            this.Indexer = 0;
            this.keys = new TKEY[0];
            this.Values = new mAction[0];
            this.ToRemove = new bool[0];
        }
        public mAction this[TKEY key]
        {
            get {

                if (this.keys.Contains(key))
                {
                    int Index = GetPosition(this.keys, key);
                    if (this.ToRemove[Index])
                        throw new NullReferenceException();
                    return this.Values[Index];
                }
                else
                    throw new IndexOutOfRangeException();
            }
            set 
            { 
                if(this.keys.Contains(key))
                {
                    int Index = GetPosition(this.keys, key);
                    this.Values[Index] = value;
                    this.Values.SetValue(value, Index);
                }
                else
                {
                    Resize(Indexer + 1);
                    this.Values.SetValue(value, NextIndex);
                    this.keys.SetValue(key, NextIndex);
                    this.ToRemove.SetValue(false, NextIndex);
                }
                Update();
            }
        }
        protected int GetPosition<T>(T[] collection, T value)
        {
            Update();
            return Array.IndexOf<T>(collection, value, 0);
        }
        protected void Resize(int newLength)
        {
            lock(this.ToRemove)
                Array.Resize<bool>(ref this.ToRemove, newLength);
            lock (this.keys)
                Array.Resize<TKEY>(ref this.keys, newLength);
            lock (this.Values)
                Array.Resize<mAction>(ref this.Values, newLength);
            Indexer = newLength;
        }
        public IntPtr this[TKEY key, bool Unsafe]
        {
            get
            {
                if (this.keys.Contains(key))
                {
                    int Index = Array.IndexOf<TKEY>(this.keys, key, 0);
                    if (this.ToRemove[Index])
                        throw new NullReferenceException();
                    return Marshal.GetFunctionPointerForDelegate(this.Values[Index]);
                }
                else
                    throw new IndexOutOfRangeException();
            }
        }
        protected mAction Add(TKEY key, mAction Value)
        {
            this[key] = Value;
            return this[key];
        }
        public bool TryAdd(TKEY key, mAction Value)
        {
            lock (this.syncRoot)
            {
                return Value == Add(key, Value);
            }
        }
        public bool TryGetValue(TKEY key, out mAction Value)
        {
            lock(this.syncRoot)
            {
                try
                {
                    Value = this[key];
                    return true;
                }
                catch
                {
                    Value = default(mAction);
                    return false;
                }
                finally
                {
                    Update();
                }
            }
        }
        protected mAction Remove(TKEY key)
        {
            int Index = GetPosition(this.keys, key);
            this.ToRemove[Index] = true;
            mAction removed = this.Values[Index];
            return removed;
        }
        public bool TryRemove(TKEY key, out mAction Removed)
        {
            lock(this.syncRoot)
            {
                try
                {
                    Removed = Remove(key);
                    return Removed != default(mAction);
                }
                catch
                {
                    Removed = default(mAction);
                    return false;
                }
                finally
                {
                    Update();
                }
            }
        }
        protected void Update()
        {
            lock (this.syncRoot)
            {
                List<TKEY> keys = new List<TKEY>();
                List<mAction> values = new List<mAction>();
                for (int x = 0; x < Indexer; x++)
                {
                    if (this.ToRemove[x])
                        continue;
                    else
                    {
                        keys.Add(this.keys[x]);
                        values.Add(this.Values[x]);
                        continue;
                    }
                }
                lock (this.keys)
                    this.keys = keys.ToArray();
                lock (this.Values)
                    this.Values = values.ToArray();
                lock (this.ToRemove)
                    this.ToRemove  = new bool[this.keys.Length];
                Indexer = keys.Count;
                keys = null;
                values = null;
            }
            GC.Collect();
        }
        ~CustomContainer()
        {
            this.syncRoot = null;
            this.Indexer = 0;
            this.keys = null;
            this.Values = null;
            this.ToRemove = null;
        }
    }
}
