using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate | AttributeTargets.Event, AllowMultiple = true, Inherited = false)]
    public class Method : Attribute
    {
        public readonly Guid MethodGuid;
        public readonly int Period;
        private ThreadType _type;
        private long lastExcution = 0;
        private object instance = null;
        public Method(string guid, int Period)
        {
            Contract.Assert(guid == null, "MethodGuid is a must parameter.");
            this.MethodGuid = Guid.Parse(guid);
            this.Period = Period;
            Resolver.Add(this);
        }
        public object Instance { get { return this.instance; } set { this.instance = value; } }
        public ThreadType ThreadFlag { get { return this._type; } set { this._type = value; } }
        public DateTime ExcutionStamp { get { return DateTime.FromBinary(this.lastExcution); } set { this.lastExcution = value.ToBinary(); } }
        public MethodInfo MethodInfo
        {
            get
            {
                MethodInfo Founded = null;
                Exception foundexception = null;
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var types = assembly.GetTypes()
                        .Where(p => p.GetCustomAttributes(typeof(Method), false).Length > 0);
                    foreach (var type in types)
                    {
                        foreach (var method in type.GetMethods())
                        {
                            var attribute = method.GetCustomAttributes(typeof(Method), false).Where(p => ((Method)p).MethodGuid == this.MethodGuid);
                            if (attribute.Count() > 0)
                            {
                                Founded = method;
                                Instance = type;
                                break;
                            }
                        }
                        if (Founded != null)
                            break;
                    }
                }
                catch (Exception e)
                {
                    foundexception = e;
                }
                finally
                {
                    if (foundexception != null)
                    {
                        Founded = null;
                    }
                }
                return Founded;
            }
        }
    }
}
