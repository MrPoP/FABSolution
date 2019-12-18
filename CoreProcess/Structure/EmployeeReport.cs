using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct EmployeeReport
    {
        public static EmployeeReport Empty = default(EmployeeReport);

        public long Time;
        public int ID;
        public string Name;
        public string PassWord;
        public byte UserFlag;
        public int MonthID;
        public int YearID;
        public HealthCertificate HealthCertificate;
        public ContactDetails ContactDetails;
        public ShiftReport[] ShiftsReport;

        public static EmployeeReport Create(long _Time, int _ID, string _Name, string _PassWord, byte _UserFlag, int _MonthID, int _YearID,
            HealthCertificate _HealthCertificate, ContactDetails _ContactDetails, ShiftReport[] _ShiftsReport)
        {
            return new EmployeeReport()
            {
                Time = _Time,
                ID = _ID,
                Name = _Name,
                PassWord = _PassWord,
                UserFlag = _UserFlag,
                MonthID = _MonthID,
                YearID = _YearID,
                HealthCertificate = _HealthCertificate,
                ContactDetails = _ContactDetails,
                ShiftsReport = _ShiftsReport
            };
        }
    }
}
