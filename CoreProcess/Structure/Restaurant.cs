using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Collection;

namespace CoreProcess.Structure
{
    public class Restaurant
    {
        private int id = 0;
        private Inventory inventory = null;
        private int chainid = 0, areaid = 0, gmanagerid = 0;
        private InventoryReportCollection inventoryreports = null;
        private DrawerCollection drawers = null;
        private DrawersReportCollection drawersreports = null;
        private EmployeeCollection employees = null;
        private EmployeeReportCollection employeesreports = null;
        private DepartmentCollection departments = null;
        private DepartmentReportCollection derpartmentsreports = null;
        private ContactDetails contactdetails = ContactDetails.Default;

        public Inventory Inventory { get { return this.inventory; } }
        public InventoryReportCollection InventoryReports { get { return this.inventoryreports; } }
        public DrawerCollection Drawers { get { return this.drawers; } }
        public DrawersReportCollection DrawersReports { get { return this.drawersreports; } }
        public EmployeeCollection Employees { get { return this.employees; } }
        public EmployeeReportCollection EmployeesReports { get { return this.employeesreports; } }
        public DepartmentCollection Departments { get { return this.departments; } }
        public DepartmentReportCollection DerpartmentsReports { get { return this.derpartmentsreports; } }
        public int ID { get { return this.id; } }
        public int ChainID { get { return this.chainid; } }
        public int AreaID { get { return this.areaid; } }
        public int GeneralManagerID { get { return this.gmanagerid; } }
        public ContactDetails ContactDetails { get { return this.contactdetails; } }

        public Restaurant(int _id, int _gmanagerid, int _chainid, int _areaid)
        {
            this.id = _id;
            this.chainid = _chainid;
            this.areaid = _areaid;
            this.gmanagerid = _gmanagerid;
            this.inventory = new Inventory(this.id);
            this.inventoryreports = new InventoryReportCollection(this.id);
            this.drawers = new DrawerCollection(this.id, this.gmanagerid);
            this.drawersreports = new DrawersReportCollection(this.id, this.gmanagerid);
            this.employees = new EmployeeCollection(this.id);
            this.employeesreports = new EmployeeReportCollection(this.id);
            this.departments = new DepartmentCollection(this.id, this.gmanagerid);
            this.derpartmentsreports = new DepartmentReportCollection(this.id);
        }

        public void SetContact(ContactDetails _contactdetails)
        {
            this.contactdetails = _contactdetails;
        }

        ~Restaurant()
        {
            this.id = 0;
            this.chainid = 0;
            this.areaid = 0;
            this.gmanagerid = 0;
            this.inventory = null;
            this.inventoryreports = null;
            this.drawers = null;
            this.drawersreports = null;
            this.employees = null;
            this.employeesreports = null;
            this.departments = null;
            this.derpartmentsreports = null;
            this.contactdetails = default(ContactDetails);
        }

        public static implicit operator RestaurantReport(Restaurant restaurant)
        {
            return RestaurantReport.Create(restaurant.id, restaurant.chainid, restaurant.areaid, restaurant.gmanagerid,
                restaurant.contactdetails, restaurant.inventory.GetReport, restaurant.inventoryreports.CountReportsIDs,
                restaurant.drawers.DrawersIDs, restaurant.drawersreports.DrawersReportsIDs, restaurant.employees.EmployesIDs,
                restaurant.EmployeesReports.EmployeesReportsIDs, restaurant.departments.DepartmentsIDs, restaurant.derpartmentsreports.Keys);
        }
    }
}
