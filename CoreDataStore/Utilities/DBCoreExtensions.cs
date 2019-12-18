using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreDataStore
{
    public static class DBCoreExtensions
    {
        public static int ExcutedCounter = 0;
        static POPThread XThread = new POPThread(x => PoPInvoker());
        static readonly List<Tuple<Type, CoreStruct, List<Tuple<PropertyInfo, CoreProperty, object>>>> constructors=
            new List<Tuple<Type,CoreStruct,List<Tuple<PropertyInfo,CoreProperty,object>>>>();
        static readonly Action<DBTableStruct> OnCreateTable = (dbtable) => OnCreateTableVoid(dbtable);
        static readonly Action<DBTableStruct> OnCreateColum = (dbtable) => OnCreateColumVoid(dbtable);
        static readonly Dictionary<string, DBTableStruct> Manager = new Dictionary<string, DBTableStruct>();
        public static int ConstructorsCount { get { return constructors.Count; } }
        static DBTableStruct CreateTable(this string name, bool crypted)
        {
            if (Manager.ContainsKey(name))
                throw new Exception("already existed table.");
            DBTableStruct table = DBTableStruct.Create(name, crypted);
            Manager.Add(name, table);
            if (OnCreateTable != null)
                OnCreateTable.Invoke(table);
            return table;
        }
        static DBTableStruct CreateColum(this DBTableStruct table, string name, object defaultvalue, bool autoincreasment = false, bool uniquekey = false)
        {
            if (table.Colums.ContainsKey(name))
                throw new Exception(string.Format("already existed table colum {0}.", name));
            DBColum Colum = table.Colums.GetOrAdd(name, DBColum.Create(name, defaultvalue, autoincreasment, uniquekey));
            if (OnCreateColum != null)
                OnCreateColum.Invoke(table);
            return table;
        }
        static DBColum Add(this DBColum colum, object newvalue)
        {
            Exception gotexceptions = null;
            Add(colum, CoreResolver.GetTime.Invoke(), newvalue, out gotexceptions);
            if (gotexceptions is DuplicatedUniqueValue)
            {
                int index = colum.Values.FindIndex(p => p == newvalue);
                if(index != -1)
                {
                    Set(colum, CoreResolver.GetTime.Invoke(), index, newvalue, out gotexceptions);
                }
            }
            return colum;
        }
        static void Add(this DBColum colum, long time, object newvalue, out Exception exp)
        {
            exp = null;
            try
            {
                if (!CoreResolver.CanUse(colum, newvalue))
                    throw new WrongParameterCodeType("{0} type isn't acceptable in colum {1}.", CoreResolver.ReturnType(newvalue.GetType()).ToString(), colum.Name);
                if (CoreResolver.UniqueDBKey(colum))
                    if (colum.Values.Contains(newvalue))
                        throw new DuplicatedUniqueValue("{0} unique value {1} existed already.", colum.Name, newvalue);
                colum.Records.Add(DBColumOrderRecord.Create(DBColumOrderRecordType.Add, time, colum, newvalue));
                colum.UpdatedValue = newvalue;
                colum.Values.Add(newvalue);
            }
            catch(Exception e)
            {
                exp = e;
            }
        }
        static DBColum Set(this DBColum colum, int index, object newvalue)
        {
            Exception gotexceptions = null;
            Set(colum, CoreResolver.GetTime.Invoke(), index, newvalue, out gotexceptions);
            return colum;
        }
        static void Set(this DBColum colum, long time, int index, object newvalue, out Exception exp)
        {
            exp = null;
            try
            {
                if (!CoreResolver.CanUse(colum, newvalue))
                    throw new WrongParameterCodeType("{0} type isn't acceptable in colum {1}.", CoreResolver.ReturnType(newvalue.GetType()).ToString(), colum.Name);
                if (CoreResolver.UniqueDBKey(colum))
                    if (colum.Values.Contains(newvalue))
                        throw new DuplicatedUniqueValue("{0} unique value {1} existed already.", colum.Name, newvalue);
                colum.Records.Add(DBColumOrderRecord.Create(DBColumOrderRecordType.Set, time, colum.Name, colum.Values[index], newvalue));
                colum.UpdatedValue = newvalue;
                colum.Values[index] = newvalue;
            }
            catch (Exception e)
            {
                exp = e;
            }
        }
        static object Get(this DBColum colum, long time, int index)
        {
            if (colum.Values.Count < index)
                return null;
            colum.Records.Add(DBColumOrderRecord.Create(DBColumOrderRecordType.Get, time, colum.Name, colum.Values[index], colum.DefaultValue));
            return colum.Values[index];
        }
        static async void OnCreateTableVoid(DBTableStruct table)
        {
            await Task.Run(() => { });
        }
        static async void OnCreateColumVoid(DBTableStruct table)
        {
            int maxValues = table.Colums.Values.OrderByDescending(p => p.Values.Count).FirstOrDefault().Values.Count;
            foreach (DBColum colum in table.Colums.Values.Where(p => p.Values.Count < maxValues))
            {
                int neededvalues = maxValues - colum.Values.Count;
                await Task.Run(() =>
                {
                    while(neededvalues > 0)
                    {
                        colum.Add(colum.DefaultValue);
                        --neededvalues;
                    }
                });
            }
        }
        public static T ValidateDB<T>(this T obj)
            where T : new()
        {
            List<Tuple<PropertyInfo, CoreProperty, object>> items = new List<Tuple<PropertyInfo, CoreProperty, object>>();
            if (obj.GetType().GetCustomAttributes(typeof(CoreStruct), false).Length > 0)
            {
                foreach (var properity in obj.GetType().GetProperties())
                {
                    if (properity.IsDefined(typeof(CoreProperty), false))
                    {
                        var pattribute = properity.GetCustomAttributes<CoreProperty>(false).Where(p => CoreResolver.DefinedAttribute(p));
                        if (pattribute.Count() > 0)
                        {
                            items.Add(Tuple.Create(properity, pattribute.FirstOrDefault(), properity.GetValue(obj)));
                        }
                    }
                }
                int index = constructors.FindIndex(0, e => e.Item1.Equals(obj));
                if (index == -1)
                {//add to constructors
                    constructors.Add(Tuple.Create(obj.GetType(), (CoreStruct)obj.GetType().GetCustomAttribute(typeof(CoreStruct), false), items));
                }
                else
                {//update parameters
                    constructors.RemoveAt(index);
                    constructors.Add(Tuple.Create(obj.GetType(), (CoreStruct)obj.GetType().GetCustomAttribute(typeof(CoreStruct), false), items));
                }
            }
            /*if (!XThread.IsAlive)
                XThread.Start();*/
            return obj;
        }
        static void PoPInvoker()
        {
            try
            {
                GetConstructors();
                GenerateDatabase();
            }
            finally
            {
                ExcutedCounter++;
            }
        }
        static void GetConstructors()
        {
        }
        static void GenerateDatabase()
        {

        }
    }
}
