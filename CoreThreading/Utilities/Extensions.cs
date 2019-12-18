using CoreThreading.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading
{
    public static class Extensions
    {
        public static T InitializeDefaultValues<T>(this T obj) where T : new()
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var d = prop.GetCustomAttribute<DefaultValueAttribute>();
                if (d != null)
                    prop.SetValue(obj, d.Value);
            }
            return obj;
        }
        public static T3 InitializeGetValue<T, T2, T3>(this T obj, T2 key) where T : struct
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var d = prop.GetCustomAttribute<DefaultPropertyAttribute>();
                if (d != null)
                    if (d.Name == key.ToString())
                        return (T3)prop.GetValue(obj);
            }
            return default(T3);
        }
        public static void InitializeSetValue<T, T2, T3>(this T obj, T2 key, T3 value) where T : struct
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var d = prop.GetCustomAttribute<DefaultPropertyAttribute>();
                if (d != null)
                    if (d.Name == key.ToString())
                    {
                        var d2 = prop.GetCustomAttribute<DefaultValueAttribute>();
                        if (d2 != null)
                        {
                            if (typeof(T3) == d2.Value.GetType())
                            {
                                prop.SetValue(obj, value);
                                break;
                            }
                            else
                            {
                                prop.SetValue(obj, Convert.ChangeType(value, d2.Value.GetType()));
                            }
                        }
                    }
            }
        }
        public static int RandGet(int nMax, bool bRealRand)
        {
            if (nMax <= 0)
            {
                nMax = 1;
            }
            if (bRealRand)
            {
                Native.srand((ulong)Environment.TickCount);
            }
            return (int)(Native.rand() % ((long)nMax));
        }
        public static double RandomRateGet(double dRange)
        {
            double num = 3.1415926;
            int num2 = RandGet(0x3e7, false) + 1;
            double d = Math.Sin((num2 * num) / 1000.0);
            if (num2 >= 90)
            {
                return ((1.0 + dRange) - (Math.Sqrt(Math.Sqrt(d)) * dRange));
            }
            return ((1.0 - dRange) + (Math.Sqrt(Math.Sqrt(d)) * dRange));
        }
    }
}
