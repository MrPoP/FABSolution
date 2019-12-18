using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public class Customer
    {
        private int id = 0;
        private string name = null;
        private Dictionary<int, Performance> ordersReport = null;
        private ContactDetails contactDetails = ContactDetails.Default;
        private double tradablepoints = 0.0;

        public int ID { get { return this.id; } }
        public string Name { get { return this.name; } }
        public double TradablePoints { get { return this.tradablepoints; } }
        public ContactDetails ContactDetails { get { return this.contactDetails; } }
        public List<int> Cheques { get { return this.ordersReport.Keys.ToList(); } }
        public List<Performance> Reports { get { return this.ordersReport.Values.ToList(); } }

        public Customer(string _name)
        {
            this.id = new Random().Next(-1, int.MaxValue);
            this.name = _name;
            this.ordersReport = new Dictionary<int, Performance>();
            this.contactDetails = ContactDetails.Default;
            this.tradablepoints = 0.0;
        }
        public Customer(string _name, ContactDetails _contactDetails)
        {
            this.id = new Random().Next(-1, int.MaxValue);
            this.name = _name;
            this.ordersReport = new Dictionary<int, Performance>();
            this.contactDetails = _contactDetails;
            this.tradablepoints = 0.0;
        }
        protected Customer(CustomerReport customer)
        {
            this.id = customer.ID;
            this.name = customer.Name;
            this.ordersReport = new Dictionary<int, Performance>();
            if (customer.Cheques.Count == customer.Reports.Count)
            {
                customer.Cheques.ForEach(p => this.ordersReport.Add(p, customer.Reports.ElementAt(customer.Cheques.FindIndex(x => x == p))));
            }
            this.contactDetails = customer.ContactDetails;
            this.tradablepoints = customer.TradablePoints;
        }

        public void SetContact(ContactDetails _contactdetails)
        {
            this.contactDetails = _contactdetails;
        }
        public void AddReport(int chequeid, Performance performance)
        {
            if (!this.ordersReport.ContainsKey(chequeid))
            {
                this.ordersReport.Add(chequeid, performance);
                return;
            }
        }

        ~Customer()
        {
            this.id = 0;
            this.name = null;
            this.ordersReport = null;
            this.contactDetails = ContactDetails.Default;
            this.tradablepoints = 0.0;
        }

        public static implicit operator CustomerReport(Customer customer)
        {
            return CustomerReport.Create(customer.id, customer.name, customer.tradablepoints, customer.contactDetails, customer.Cheques, customer.Reports.Select(p => (PerformanceReport)p).ToList());
        }
        public static implicit operator Customer(CustomerReport customer)
        {
            return new Customer(customer);
        }
    }
}
