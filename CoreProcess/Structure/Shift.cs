using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public class Shift
    {
        private ShiftFlags flag = ShiftFlags.None;
        private int checkinhour = 0, checkinminute = 0, checkouthour = 0, checkoutminute = 0, dayofmonth = 0, monthofyear = 0;
        private Performance performance = Performance.Ideal;

        public ShiftFlags Flag { get { return flag; } }
        public string CheckInstr
        {
            get
            {
                return string.Format("{0}:{1}",
                    (checkinhour > 9 ? checkinhour.ToString() : "0" + checkinhour.ToString()),
                    (checkinminute > 9 ? checkinminute.ToString() : "0" + checkinminute.ToString())); 
            }
        }
        public string CheckOutstr
        {
            get
            {
                return string.Format("{0}:{1}",
                    (checkouthour > 9 ? checkouthour.ToString() : "0" + checkouthour.ToString()),
                    (checkoutminute > 9 ? checkoutminute.ToString() : "0" + checkoutminute.ToString()));
            }
        }
        public string Date { get { return string.Format("{0}/{1}", dayofmonth, monthofyear); } }
        public Performance Performance { get { return performance; } }

        public Shift(int _dayofmonth, int _monthofyear)
        {
            dayofmonth = _dayofmonth;
            monthofyear = _monthofyear;
        }
        public Shift(int _dayofmonth, int _checkinhour, int _checkinminute)
        {
            flag = ShiftFlags.Presence;
            dayofmonth = _dayofmonth;
            checkinhour = _checkinhour;
            checkinminute = _checkinminute;
        }

        public void AddIndicator(float speed = 0f, float service = 0f, float quality = 0f, float precision = 0f, float cleanless = 0f)
        {
            this.performance.AddSpeed(speed);
            this.performance.AddService(service);
            this.performance.AddQuality(quality);
            this.performance.AddPrecision(precision);
            this.performance.AddCleanless(cleanless);
        }
        public void RemoveIndicator(float speed = 0f, float service = 0f, float quality = 0f, float precision = 0f, float cleanless = 0f)
        {
            this.performance.RemoveSpeed(speed);
            this.performance.RemoveService(service);
            this.performance.RemoveQualiy(quality);
            this.performance.RemovePrecision(precision);
            this.performance.RemoveCleanless(cleanless);
        }
        public void CheckIn(int _checkinhour, int _checkinminute)
        {
            flag = ShiftFlags.Presence;
            checkinhour = _checkinhour;
            checkinminute = _checkinminute;
        }
        public void CheckOut(int _checkouthour, int _checkoutminute)
        {
            checkouthour = _checkouthour;
            checkoutminute = _checkoutminute;
        }

        ~Shift()
        {
            flag = ShiftFlags.None;
            checkinhour = 0;
            checkinminute = 0;
            checkouthour = 0;
            checkoutminute = 0;
            dayofmonth = 0;
            monthofyear = 0;
        }

        public static implicit operator ShiftReport(Shift shift)
        {
            return ShiftReport.Create((byte)shift.flag, shift.dayofmonth, shift.checkinhour, shift.checkinminute, shift.checkouthour,
                shift.checkoutminute, shift.monthofyear, shift.performance.SpeedIndicator, shift.performance.ServiceIndicator,
                shift.performance.QualityIndicator, shift.performance.PrecisionIndicator, shift.performance.CleanlessIndicator);
        }
    }
}
