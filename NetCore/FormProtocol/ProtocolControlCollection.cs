using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace NetCore.FormProtocol
{
    public class ProtocolControlCollection : ICollection<ProtocolControl>, IList<Control>
    {
        private Dictionary<int, ProtocolControl> pitems = null;
        private Dictionary<int, Control> items = null;
        private List<Control> listeditems { get { return this.items.Values.ToList(); } }
        private object syncRoot = null;

        public ProtocolControlCollection()
        {
            this.syncRoot = new object();
            this.items = new Dictionary<int, Control>();
            this.pitems = new Dictionary<int, ProtocolControl>();
        }

        public void Add(ProtocolControl item)
        {
            lock (this.syncRoot)
            {
                if (this.items.ContainsKey((int)item.ID))
                {
                    throw new Exception("Duplicated Control Tag Member.");
                }
                this.pitems.Add(item.ID, item);
                if (item.ParentID != 0)
                {
                    if (this.items.ContainsKey(item.ParentID))
                    {
                        this.items[item.ID].Parent = this.items[item.ParentID];
                    }
                }
                if (item.Sons.Count > 0)
                {
                    item.Sons.ForEach(p =>
                    {
                        this.items[p].Parent = this.items[item.ID];
                        this.items[item.ID].Controls.Add(this.items[p]);
                    });
                }
                Insert(item.ID, (Control)item);
            }
        }

        public void Clear()
        {
            lock (this.syncRoot)
            {
                this.items.Clear();
                this.pitems.Clear();
            }
        }

        public bool Contains(Control item)
        {
            lock (this.syncRoot)
            {
                return this.items.ContainsKey((int)item.Tag);
            }
        }

        public void CopyTo(Control[] array, int arrayIndex)
        {
            lock (this.syncRoot)
            {
                this.items.Values.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get { return this.listeditems.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(Control item)
        {
            return false;
        }

        public IEnumerator<Control> GetEnumerator()
        {
            return this.listeditems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.listeditems.GetEnumerator();
        }

        public int IndexOf(Control item)
        {
            return this.listeditems.IndexOf(item);
        }

        public void Insert(int index, Control item)
        {
            lock (this.syncRoot)
            {
                if (this.items.ContainsKey(index))
                {
                    throw new Exception("Duplicated Control Tag Member.");
                }
                this.items.Add(index, item);
                this.items[index].Tag = index;
                this.items[index].GotFocus += ProtocolInvokations.GotFocus;
                this.items[index].TextChanged += ProtocolInvokations.TextChanged;
                this.items[index].LostFocus += ProtocolInvokations.LostFocus;
                this.items[index].MouseClick += ProtocolInvokations.MouseClick;
                this.items[index].Validated += ProtocolInvokations.Validated;
                this.items[index].Validating += ProtocolInvokations.Validating;
            }
        }

        public void RemoveAt(int index)
        {
            return;
        }

        public Control this[int index]
        {
            get
            {
                return this.items[index];
            }
            set
            {
                if (!this.items.ContainsKey(index))
                {
                    Insert(index, value);
                }
            }
        }

        ~ProtocolControlCollection()
        {
            this.items = null;
            this.pitems = null;
            this.syncRoot = null;
        }

        IEnumerator<ProtocolControl> IEnumerable<ProtocolControl>.GetEnumerator()
        {
            return this.pitems.Values.GetEnumerator();
        }

        public bool Contains(ProtocolControl item)
        {
            return this.pitems.ContainsKey(item.ID);
        }

        public void CopyTo(ProtocolControl[] array, int arrayIndex)
        {
            this.pitems.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(ProtocolControl item)
        {
            return false;
        }

        public void Add(Control item)
        {
            Insert((int)item.Tag, item);
        }
    }
}
