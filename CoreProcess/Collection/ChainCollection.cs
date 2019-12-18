using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Structure;
using System.Collections;

namespace CoreProcess.Collection
{
    public class ChainCollection : ICollection
    {
        private object syncRoot = null;

        public void CopyTo(Array array, int index)
        {
            lock (this.syncRoot)
            {
            }
        }
        public int Count
        {
            get { throw new NotImplementedException(); }
        }
        public bool IsSynchronized
        {
            get { return this.syncRoot != null; }
        }
        public object SyncRoot
        {
            get { return this.syncRoot; }
        }
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        ~ChainCollection()
        {
            this.syncRoot = null;
        }
    }
}
