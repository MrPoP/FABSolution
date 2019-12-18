using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Structure;
using System.Collections;

namespace CoreProcess.Collection
{
    public class DepartmentCollection : ICollection
    {
        private Dictionary<DepartmentFlag, Dictionary<int, Department>> items = null;
        private long time = 0;
        private int ownerid = 0, gmanagerid = 0;
        private object syncRoot = null;

        public int OwnerID { get { return this.ownerid; } }
        public int GeneralManagerID { get { return this.gmanagerid; } }
        public int DepartmentsCount { get { return this.Departments.Count; } }
        public List<Department> Departments { get { return items.Values.SelectMany(x => x.Values).ToList(); } }
        public List<int> DepartmentsIDs { get { return this.Departments.Select(x => x.ID).ToList(); } }

        public DepartmentCollection(int _ownerid, int _gmanagerid)
        {
            this.syncRoot = new object();
            this.ownerid = _ownerid;
            this.gmanagerid = _gmanagerid;
            this.items = new Dictionary<DepartmentFlag, Dictionary<int, Department>>();
            this.items.Add(DepartmentFlag.Cash_Section, new Dictionary<int, Department>());
            this.items.Add(DepartmentFlag.Pack_Section, new Dictionary<int, Department>());
            this.items.Add(DepartmentFlag.MainCourse_Section, new Dictionary<int, Department>());
            this.items.Add(DepartmentFlag.SideItem_Section, new Dictionary<int, Department>());
            this.items.Add(DepartmentFlag.Delivery_Section, new Dictionary<int, Department>());
        }

        public void UpdateTime(long _time)
        {
            this.time = _time;
        }
        public bool TryGetDrawer(DepartmentFlag flag, int id, out Department department)
        {
            Dictionary<int, Department> values = null;
            department = null;
            if(this.items.TryGetValue(flag, out values))
            {
                return values.TryGetValue(id, out department);
            }
            return false;
        }
        public bool TryGetDrawers(out List<Department> departments, DepartmentFlag flag = DepartmentFlag.None, int id = 0)
        {
            departments = null;
            if (flag == DepartmentFlag.None && id == 0)
            {
                departments = this.Departments;
                return departments != null;
            }
            else if (flag != DepartmentFlag.None && id == 0)
            {
                departments = this.Departments.Where(p => p.Flag == flag).ToList();
                return departments != null;
            }
            else if (flag == DepartmentFlag.None && id != 0)
            {
                departments = this.Departments.Where(p => p.ID == id).ToList();
                return departments != null;
            }
            else
            {
                departments = this.Departments.Where(p => p.Flag == flag && p.ID == id).ToList();
                return departments != null;
            }
        }
        public void Process(DepartmentFlag flag, int id, Cheque cheque, out List<Exception> thrownexception)
        {
            Department department = null;
            thrownexception = null;
            if (TryGetDrawer(flag, id, out department))
            {
                department.Process(cheque, out thrownexception);
            }
        }
        public void ProcessDepartments(DepartmentFlag flag, int id, Cheque cheque, out List<Exception> thrownexception)
        {
            List<Department> departments = null;
            thrownexception = null;
            if (TryGetDrawers(out departments, flag, id))
            {
                List<Exception> exceptions = null;
                foreach (Department department in departments)
                {
                    department.Process(cheque, out exceptions);
                    if (exceptions != null)
                    {
                        if (thrownexception == null)
                            thrownexception = new List<Exception>();
                        thrownexception.AddRange(exceptions.AsEnumerable());
                    }
                }
            }
        }
        public void EndOrder(DepartmentFlag flag, int id, int index, long time)
        {
            Department department = null;
            if (TryGetDrawer(flag, id, out department))
            {
                department.EndOrder(index, time);
            }
        }
        public void CopyTo(Array array, int index)
        {
            lock (this.syncRoot)
            {
                this.Departments.ToArray().CopyTo(array, index);
            }
        }
        public int Count
        {
            get { return this.Departments.Count; }
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
            return this.Departments.ToArray().GetEnumerator();
        }

        ~DepartmentCollection()
        {
            this.syncRoot = null;
            this.ownerid = 0;
            this.gmanagerid = 0;
            this.items = null;
            this.time = 0;
        }
    }
}
