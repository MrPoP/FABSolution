using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Structure;
using System.Collections;

namespace CoreProcess.Collection
{
    public class CustomerCollection : ICollection
    {
        private Dictionary<int, Customer> items = null;
        private long time = 0;
        private int ownerid = 0;
        private object syncRoot = null;

        public int OwnerID { get { return this.ownerid; } }
        public int Count { get { return this.items.Count; } }
        public List<Customer> Customers { get { return this.items.Values.ToList(); } }
        public List<CustomerReport> CustomersReports { get { return this.items.Values.Select(x => (CustomerReport)x).ToList(); } }
        public List<int> CustomersIDs { get { return this.items.Keys.ToList(); } }

        public CustomerCollection(int _ownerid)
        {
            this.syncRoot = new object();
            this.ownerid = _ownerid;
            this.items = new Dictionary<int, Customer>();
            this.time = 0;
        }

        public void UpdateTime(long _time)
        {
            this.time = _time;
        }
        public Customer GetCustomer(int _ID)
        {
            Customer customer = null;
            if (this.items.TryGetValue(_ID, out customer))
            {
                return customer;
            }
            return null;
        }
        public CustomerReport GetReport(int _ID)
        {
            return GetCustomer(_ID) != null ? (CustomerReport)GetCustomer(_ID) : default(CustomerReport);
        }
        public void AddCustomer(CustomerReport customer)
        {
            if (!this.items.ContainsKey(customer.ID))
            {
                this.items.Add(customer.ID, customer);
            }
            else
            {
                this.items[customer.ID] = customer;
            }
        }
        public void AddReport(int _ID, int _ChequeID, PerformanceReport report)
        {
            Customer customer = null;
            if (this.items.TryGetValue(_ID, out customer))
            {
                customer.AddReport(_ChequeID, report);
            }
        }
        public void SetContact(int _ID, ContactDetails _contactdetails)
        {
            Customer customer = null;
            if (this.items.TryGetValue(_ID, out customer))
            {
                customer.SetContact(_contactdetails);
            }
        }
        public void CopyTo(Array array, int index)
        {
            lock (this.syncRoot)
            {
                this.CustomersReports.ToArray().CopyTo(array, index);
            }
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
            return this.CustomersReports.ToArray().GetEnumerator();
        }

        ~CustomerCollection()
        {
            this.syncRoot = null;
            this.ownerid = 0;
            this.items = null;
            this.time = 0;
        }
    }
}
