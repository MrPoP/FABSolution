using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct RestaurantReport
    {
        public static RestaurantReport Empty = default(RestaurantReport);

        public long Time;
        public int ID;
        public int ChainID;
        public int AreaID;
        public int GeneralManagerID;
        public ContactDetails ContactDetails;
        public List<Tuple<string, byte, double>> Inventory;
        public List<int> InventoryReports;
        public List<int> Drawers;
        public List<int> DrawersReports;
        public List<int> Employees;
        public List<int> EmployeesReports;
        public List<int> Departments;
        public List<long> DepartmentsReports;

        public static RestaurantReport Create(int _ID, int _ChainID, int _AreaID, int _GeneralManagerID,
            ContactDetails _ContactDetails, List<Tuple<string, byte, double>> _Inventory, List<int> _InventoryReports,
            List<int> _Drawers, List<int> _DrawersReports, List<int> _Employees, List<int> _EmployeesReports, List<int> _Departments,
            List<long> _DepartmentsReports)
        {
            return new RestaurantReport()
            {
                ID = _ID,
                ChainID = _ChainID,
                AreaID = _AreaID,
                GeneralManagerID = _GeneralManagerID,
                ContactDetails = _ContactDetails,
                Inventory = _Inventory,
                InventoryReports = _InventoryReports,
                Drawers = _Drawers,
                DrawersReports = _DrawersReports,
                Employees = _Employees,
                EmployeesReports = _EmployeesReports,
                Departments = _Departments,
                DepartmentsReports = _DepartmentsReports
            };
        }
    }
}
