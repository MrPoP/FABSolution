using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using CoreProcess.Structure;
using System.Collections.ObjectModel;

namespace CoreProcess.Collection
{
    public class EmployeeCollection : ICollection
    {
        private Dictionary<UserFlags, Dictionary<int, Employee>> items = null;
        private long time = 0;
        private object syncRoot = null;
        private int ownerid = 0;

        public ReadOnlyCollection<Employee> ProtectedItems { get { return Array.AsReadOnly(this.items.Values.SelectMany(x => x.Values).ToArray()); } }
        public List<Employee> Employes { get { return this.items.Values.SelectMany(x => x.Values).ToList(); } }
        public List<int> EmployesIDs { get { return Employes.Select(x => x.ID).ToList(); } }
        public int OwnerID { get { return this.ownerid; } }
        public int GeneralManagerID { get { return 0; } }

        public EmployeeCollection(int _ownerid)
        {
            this.syncRoot = new object();
            this.ownerid = _ownerid;
            this.items = new Dictionary<UserFlags, Dictionary<int, Employee>>();
            this.items.Add(UserFlags.CrewMember, new Dictionary<int, Employee>());
            this.items.Add(UserFlags.Cashier, new Dictionary<int, Employee>());
            this.items.Add(UserFlags.CrewTrainer, new Dictionary<int, Employee>());
            this.items.Add(UserFlags.SuperVisor, new Dictionary<int, Employee>());
            this.items.Add(UserFlags.AssistantManager, new Dictionary<int, Employee>());
            this.items.Add(UserFlags.GeneralManager, new Dictionary<int, Employee>());
        }

        public bool CheckIn(long _time, UserFlags _flag, int _id)
        {
            Employee _employee = null;
            if (TryGetEmployee(_flag, out _employee, _id))
            {
                DateTime Time = DateTime.FromBinary(_time);
                if (_employee.Shifts == null)
                {
                    _employee.SetShifts(new PeriodShifts(_employee.ID, Time.Month, Time.Year, DateTime.DaysInMonth(Time.Year, Time.Month)));
                }
                if (_employee.Shifts.DaysCount == 0 || _employee.Shifts.Month != Time.Month || _employee.Shifts.Year != Time.Year)
                {
                    _employee.Shifts.SetData(Time.Month, Time.Year, DateTime.DaysInMonth(Time.Year, Time.Month));
                }
                _employee.Shifts.CheckIn(Time.Day, Time.Hour, Time.Minute);
                return true;
            }
            else
                return false;
        }
        public bool CheckOut(long _time, UserFlags _flag, int _id)
        {
            Employee _employee = null;
            if (TryGetEmployee(_flag, out _employee, _id))
            {
                DateTime Time = DateTime.FromBinary(_time);
                if (_employee.Shifts == null)
                {
                    _employee.SetShifts(new PeriodShifts(_employee.ID, Time.Month, Time.Year, DateTime.DaysInMonth(Time.Year, Time.Month)));
                }
                if (_employee.Shifts.DaysCount == 0 || _employee.Shifts.Month != Time.Month || _employee.Shifts.Year != Time.Year)
                {
                    _employee.Shifts.SetData(Time.Month, Time.Year, DateTime.DaysInMonth(Time.Year, Time.Month));
                }
                if (!_employee.Shifts.CheckOut(Time.Hour, Time.Minute))
                {
                    _employee.Shifts.CheckOut(Time.Day, Time.Hour, Time.Minute);
                }
                return true;
            }
            else
                return false;
        }
        public bool TryGetEmployee(UserFlags _flag, out Employee _employee, int _id = -255, string _name = null, long _validcertificationdate = -255)
        {
            _employee = null;
            lock (this.syncRoot)
            {
                Dictionary<int, Employee> Values = null;
                List<Employee> employees = null;
                if (this.items.TryGetValue(_flag, out Values))
                {
                    employees = Values.Select(x => x.Value).ToList();
                    if (_id != -255 && _name == null && _validcertificationdate == -255)
                    {
                        _employee = employees.Find(p => p.ID == _id);
                        return true;
                    }
                    else if (_id != -255 && _name != null && _validcertificationdate == -255)
                    {
                        _employee = employees.Find(p => p.ID == _id && p.Name == _name);
                        return true;
                    }
                    else if (_name != null && _id == -255 && _validcertificationdate == -255)
                    {
                        _employee = employees.Find(p => p.Name == _name);
                        return true;
                    }
                    else if (_name != null && _id == -255 && _validcertificationdate != -255)
                    {
                        _employee = employees.Find(p => p.Name == _name && p.CheckDate(_validcertificationdate));
                        return true;
                    }
                    else if (_validcertificationdate != -255 && _id == -255 && _name != null)
                    {
                        _employee = employees.Find(p => p.CheckDate(_validcertificationdate));
                        return true;
                    }
                    else if (_validcertificationdate != -255 && _id != -255 && _name == null)
                    {
                        _employee = employees.Find(p => p.CheckDate(_validcertificationdate) && p.ID == _id);
                        return true;
                    }
                    else
                    {
                        if (_id != -255 && _validcertificationdate != -255 && _name != null)
                        {
                            _employee = employees.Find(p => p.CheckDate(_validcertificationdate) && p.ID == _id && p.Name == _name);
                            return _employee != null;
                        }
                    }
                    _employee = employees.FirstOrDefault();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool TryGetEmployees(UserFlags _flag, out List<Employee> _employees, int _id = -255, string _name = null, long _validcertificationdate = -255)
        {
            _employees = null;
            lock (this.syncRoot)
            {
                Dictionary<int, Employee> Values = null;
                List<Employee> employees = null;
                if (this.items.TryGetValue(_flag, out Values))
                {
                    employees = Values.Select(x => x.Value).ToList();
                    if (_id != -255 && _name == null && _validcertificationdate == -255)
                    {
                        _employees = employees.FindAll(p => p.ID == _id);
                        return true;
                    }
                    else if (_id != -255 && _name != null && _validcertificationdate == -255)
                    {
                        _employees = employees.FindAll(p => p.ID == _id && p.Name == _name);
                        return true;
                    }
                    else if (_name != null && _id == -255 && _validcertificationdate == -255)
                    {
                        _employees = employees.FindAll(p => p.Name == _name);
                        return true;
                    }
                    else if (_name != null && _id == -255 && _validcertificationdate != -255)
                    {
                        _employees = employees.FindAll(p => p.Name == _name && p.CheckDate(_validcertificationdate));
                        return true;
                    }
                    else if (_validcertificationdate != -255 && _id == -255 && _name != null)
                    {
                        _employees = employees.FindAll(p => p.CheckDate(_validcertificationdate));
                        return true;
                    }
                    else if (_validcertificationdate != -255 && _id != -255 && _name == null)
                    {
                        _employees = employees.FindAll(p => p.CheckDate(_validcertificationdate) && p.ID == _id);
                        return true;
                    }
                    else
                    {
                        if (_id != -255 && _validcertificationdate != -255 && _name != null)
                        {
                            _employees = employees.FindAll(p => p.CheckDate(_validcertificationdate) && p.ID == _id && p.Name == _name);
                            return _employees != null;
                        }
                    }
                    _employees = employees;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public void UpdateTime(long _time)
        {
            this.time = _time;
        }
        public void CopyTo(Array array, int index)
        {
            lock (this.syncRoot)
            {
                this.Employes.ToArray().CopyTo(array, index);
            }
        }
        public int Count
        {
            get { return this.Employes.Count(); }
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

        ~EmployeeCollection()
        {
            this.syncRoot = null;
            this.ownerid = 0;
            this.items = null;
            this.time = 0;
        }
    }
}
