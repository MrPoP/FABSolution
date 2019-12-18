using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class Property : Attribute
    {
        public readonly Guid MethodGuid;
        public readonly Type Type;
        private object instance = null;
        public Property(string methodguid, Type type)
        {
            Contract.Assert(methodguid == null, "MethodGuid is a must parameter.");
            this.MethodGuid = Guid.Parse(methodguid);
            this.Type = type;
            Resolver.Add(this);
        }
        public object Instance { get { return this.instance; } set { this.instance = value; } }
        public PropertyInfo PropertyInfo
        {
            get
            {
                PropertyInfo Founded = null;
                Exception foundexception = null;
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var types = assembly.GetTypes()
                        .Where(p => p.GetCustomAttributes(typeof(Property), false).Length > 0);
                    foreach (var type in types)
                    {
                        foreach (var property in type.GetProperties())
                        {
                            var attribute = property.GetCustomAttributes(typeof(Property), false).Where(p => ((Property)p).MethodGuid == this.MethodGuid);
                            if (attribute.Count() > 0)
                            {
                                Founded = property;
                                Instance = property.Name;
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
