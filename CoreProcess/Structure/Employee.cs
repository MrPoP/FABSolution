using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Collection;

namespace CoreProcess.Structure
{
    public class Employee
    {
        private int id = 0;
        private string name = string.Empty, password = string.Empty;
        private HealthCertificate healthcertificate = HealthCertificate.Empty;
        private UserFlags flag = UserFlags.CrewMember;
        private ContactDetails contactdetails = ContactDetails.Default;
        private PeriodShifts shifts = null;

        public int ID { get { return this.id; } }
        public string Name { get { return this.name; } }
        public string PassWord { get { return this.password; } }
        public HealthCertificate HealthCertificate { get { return this.healthcertificate; } }
        public UserFlags UserFlag { get { return this.flag; } }
        public ContactDetails ContactDetails { get { return this.contactdetails; } }
        public PeriodShifts Shifts { get { return this.shifts; } }

        public Employee(string _name, UserFlags _flag)
        {
            id = new Random().Next(-1, int.MaxValue);
            name = _name;
            flag = _flag;
            shifts = new PeriodShifts(id);
        }
        public Employee(string _name, UserFlags _flag, HealthCertificate _healthcertificate)
        {
            id = new Random().Next(-1, int.MaxValue);
            name = _name;
            flag = _flag;
            healthcertificate = _healthcertificate;
            shifts = new PeriodShifts(id);
        }
        public Employee(int _id, string _name, UserFlags _flag)
        {
            id = _id;
            name = _name;
            flag = _flag;
            shifts = new PeriodShifts(id);
        }
        public Employee(int _id, string _name, UserFlags _flag, HealthCertificate _healthcertificate)
        {
            id = _id;
            name = _name;
            flag = _flag;
            healthcertificate = _healthcertificate;
            shifts = new PeriodShifts(id);
        }

        public void SetContact(ContactDetails _contactdetails)
        {
            this.contactdetails = _contactdetails;
        }
        public bool CheckDate(long time)
        {
            if (healthcertificate.To == 0 || healthcertificate.ID == 0 || healthcertificate.From == 0)
                return false;
            return ((DateTime.FromBinary(healthcertificate.From) >= DateTime.FromBinary(time)) &&
           (DateTime.FromBinary(time) <= DateTime.FromBinary(healthcertificate.To)));
        }
        public void SetCertification(HealthCertificate _healthcertificate)
        {
            healthcertificate = _healthcertificate;
        }
        public void SetFlag(UserFlags _flag)
        {
            flag = _flag;
        }
        public void SetShifts(PeriodShifts _shifts)
        {
            this.shifts = _shifts;
        }
        public void SetName(string _name)
        {
            name = _name;
        }
        public void SetID(int _id)
        {
            id = _id;
        }
        public void SetPassword(string _password)
        {
            password = _password;
        }
        public EmployeeReport GetReport(long Time)
        {
            return EmployeeReport.Create(Time, this.id, this.name, this.password, (byte)this.flag, this.shifts.Month, this.shifts.Year,
                this.healthcertificate, this.contactdetails, this.shifts.InnerValues.Select(p => (ShiftReport)p).ToArray());
        }

        ~Employee()
        {
            id = 0;
            name = string.Empty;
            password = string.Empty;
            flag = UserFlags.CrewMember;
            shifts = null;
            contactdetails = ContactDetails.Default;
        }
    }
    public struct HealthCertificate
    {
        public static readonly HealthCertificate Empty = Create();

        public int ID;
        public long From;
        public long To;

        static HealthCertificate Create()
        {
            return new HealthCertificate()
            {
                ID = 0,
                From = 0,
                To = 0
            };
        }
        public static HealthCertificate Create(int _id, long _from, long _to)
        {
            return new HealthCertificate()
            {
                ID = _id,
                From = _from,
                To = _to
            };
        }
    }
}
