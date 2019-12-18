using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using CoreProcess.Structure;
using System.Collections.ObjectModel;

namespace CoreProcess.Collection
{
    public class DrawerCollection : ICollection
    {
        private Dictionary<DrawerFlag, List<CashDrawer>> items = null;
        private object syncRoot = null;
        private long time = 0;
        private int ownerid = 0, gmanagerid = 0;

        public ReadOnlyCollection<CashDrawer> DrawersReport { get { return Array.AsReadOnly(items.Values.SelectMany(x => x).ToArray()); } }
        public CashDrawer TotalBot
        {
            get 
            {
                CashDrawer drawer = null;
                try
                {
                    drawer = new CashDrawer(DrawerFlag.None, ownerid, gmanagerid, 0);
                    if (drawer < items.Values.SelectMany(x => x).ToArray())
                        return drawer;
                    else
                        return null;
                }
                catch
                {
                    return null;
                }
            }
        }
        public List<CashDrawer> Drawers { get { return items.Values.SelectMany(x => x).ToList(); } }
        public List<int> DrawersIDs { get { return Drawers.Select(x => x.ID).ToList(); } }
        public int OwnerID { get { return this.ownerid; } }
        public int GeneralManagerID { get { return this.gmanagerid; } }

        public DrawerCollection(int _ownerid, int _gmanagerid)
        {
            syncRoot = new object();
            ownerid = _ownerid;
            gmanagerid = _gmanagerid;
            items = new Dictionary<DrawerFlag, List<CashDrawer>>();
            items.Add(DrawerFlag.DineIn_Drawer, new List<CashDrawer>());
            items.Add(DrawerFlag.TakeOut_Drawer, new List<CashDrawer>());
            items.Add(DrawerFlag.Delivery_Drawer, new List<CashDrawer>());
        }

        public bool Open(DrawerFlag _flag, int _id, int _managerid, bool useterm = false, double _beginningterm = 0.0)
        {
            lock (this.syncRoot)
            {
                CashDrawer _drawer = null;
                if (TryGetDrawer(_flag, out _drawer, _id))
                {
                    return _drawer.Open(time, _managerid, useterm, _beginningterm);
                }
                return false;
            }
        }
        public bool Close(DrawerFlag _flag, int _id, int _managerid)
        {
            lock (this.syncRoot)
            {
                CashDrawer _drawer = null;
                if (TryGetDrawer(_flag, out _drawer, _id, _managerid))
                {
                    return _drawer.Close(time);
                }
                return false;
            }
        }
        public bool SignIn(DrawerFlag _flag, int _id, int _managerid, int _employeeid)
        {
            lock (this.syncRoot)
            {
                CashDrawer _drawer = null;
                if (TryGetDrawer(_flag, out _drawer, _id, _managerid))
                {
                    return _drawer.SignIn(_employeeid, time);
                }
                return false;
            }
        }
        public bool SignOut(DrawerFlag _flag, int _id, int _managerid, int _employeeid)
        {
            lock (this.syncRoot)
            {
                CashDrawer _drawer = null;
                if (TryGetDrawer(_flag, out _drawer, _id, _managerid, _employeeid))
                {
                    return _drawer.SignOut(time);
                }
                return false;
            }
        }
        public bool AddBill(DrawerFlag _flag, int _id, int _managerid, Bill _bill)
        {
            lock (this.syncRoot)
            {
                CashDrawer _drawer = null;
                if (TryGetDrawer(_flag, out _drawer, _id, _managerid))
                {
                    return _drawer.AddBill(time, _bill);
                }
                return false;
            }
        }
        public bool AddCheque(DrawerFlag _flag, int _id, int _managerid, int _employeeid, Cheque _cheque)
        {
            lock (this.syncRoot)
            {
                CashDrawer _drawer = null;
                if (TryGetDrawer(_flag, out _drawer, _id, _managerid, _employeeid))
                {
                    return _drawer.AddCheque(_cheque);
                }
                return false;
            }
        }
        public void UpdateTime(long _time)
        {
            this.time = _time;
        }
        public bool TryGetDrawer(DrawerFlag _flag, out CashDrawer _drawer, int _id = -255, int _managerid = -255, int _employeeid = -255)
        {
            _drawer = null;
            lock (this.syncRoot)
            {
                List<CashDrawer> drawers = null;
                if (this.items.TryGetValue(_flag, out drawers))
                {
                    if (_id != -255 && _managerid == -255 && _employeeid == -255)
                    {
                        _drawer = drawers.Find(p => p.ID == _id);
                        return true;
                    }
                    else if (_id != -255 && _managerid != -255 && _employeeid == -255)
                    {
                        _drawer = drawers.Find(p => p.ID == _id && p.ManagerID == _managerid);
                        return true;
                    }
                    else if (_managerid != -255 && _id == -255 && _employeeid == -255)
                    {
                        _drawer = drawers.Find(p => p.ManagerID == _managerid);
                        return true;
                    }
                    else if (_managerid != -255 && _id == -255 && _employeeid != -255)
                    {
                        _drawer = drawers.Find(p => p.ManagerID == _managerid && p.EmpolyeeID == _employeeid);
                        return true;
                    }
                    else if (_employeeid != -255 && _id == -255 && _managerid == -255)
                    {
                        _drawer = drawers.Find(p => p.EmpolyeeID == _employeeid);
                        return true;
                    }
                    else if (_employeeid != -255 && _id != -255 && _managerid == -255)
                    {
                        _drawer = drawers.Find(p => p.EmpolyeeID == _employeeid && p.ID == _id);
                        return true;
                    }
                    else
                    {
                        if (_id != -255 && _managerid != -255 && _employeeid != -255)
                        {
                            _drawer = drawers.Find(p => p.ManagerID == _managerid && p.ManagerID == _managerid && p.EmpolyeeID == _employeeid);
                            return _drawer != null;
                        }
                    }
                    _drawer = drawers.FirstOrDefault();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool TryGetDrawers(DrawerFlag _flag, out List<CashDrawer> _drawers, int _id = -255, int _managerid = -255, int _employeeid = -255)
        {
            _drawers = null;
            lock (this.syncRoot)
            {
                List<CashDrawer> drawers = null;
                if (this.items.TryGetValue(_flag, out drawers))
                {
                    if (_id != -255 && _managerid == -255 && _employeeid == -255)
                    {
                        _drawers = drawers.FindAll(p => p.ID == _id);
                        return true;
                    }
                    else if (_id != -255 && _managerid != -255 && _employeeid == -255)
                    {
                        _drawers = drawers.FindAll(p => p.ID == _id && p.ManagerID == _managerid);
                        return true;
                    }
                    else if (_managerid != -255 && _id == -255 && _employeeid == -255)
                    {
                        _drawers = drawers.FindAll(p => p.ManagerID == _managerid);
                        return true;
                    }
                    else if (_managerid != -255 && _id == -255 && _employeeid != -255)
                    {
                        _drawers = drawers.FindAll(p => p.ManagerID == _managerid && p.EmpolyeeID == _employeeid);
                        return true;
                    }
                    else if (_employeeid != -255 && _id == -255 && _managerid == -255)
                    {
                        _drawers = drawers.FindAll(p => p.EmpolyeeID == _employeeid);
                        return true;
                    }
                    else if (_employeeid != -255 && _id != -255 && _managerid == -255)
                    {
                        _drawers = drawers.FindAll(p => p.EmpolyeeID == _employeeid && p.ID == _id);
                        return true;
                    }
                    else
                    {
                        if (_id != -255 && _managerid != -255 && _employeeid != -255)
                        {
                            _drawers = drawers.FindAll(p => p.ManagerID == _managerid && p.ManagerID == _managerid && p.EmpolyeeID == _employeeid);
                            return _drawers != null;
                        }
                    }
                    _drawers = drawers;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public void AddDrawer(CashDrawer _drawer)
        {
            lock (this.syncRoot)
            {
                List<CashDrawer> drawers = null;
                if (this.items.TryGetValue(_drawer.Flag, out drawers))
                {
                    drawers.Add(_drawer);
                }
                else
                {
                    this.items.Add(_drawer.Flag, new List<CashDrawer>(){_drawer});
                }
            }
        }
        public void CopyTo(Array array, int index)
        {
            lock (this.syncRoot)
            {
                this.Drawers.ToArray().CopyTo(array, index);
            }
        }
        public int Count
        {
            get { return this.Drawers.Count(); }
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
            lock (this.syncRoot)
            {
                return this.items.Values.SelectMany(x => x).GetEnumerator();
            }
        }

        ~DrawerCollection()
        {
            this.items = null;
            this.syncRoot = null;
            this.time = 0;
            this.ownerid = 0;
            this.gmanagerid = 0;
        }

        public static bool operator <(DrawerCollection collection, CashDrawer drawer)
        {
            int oldcount = collection.Count;
            collection.AddDrawer(drawer);
            return collection.Count > oldcount;
        }
        public static bool operator >(DrawerCollection collection, CashDrawer drawer)
        {
            return false;
        }
        public static bool operator <(DrawerCollection collection, CashDrawer[] drawers)
        {
            int oldcount = collection.Count;
            foreach (CashDrawer drawer in drawers)
                collection.AddDrawer(drawer);
            return collection.Count > oldcount;
        }
        public static bool operator >(DrawerCollection collection, CashDrawer[] drawers)
        {
            return false;
        }
    }
}
