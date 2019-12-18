using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreDataStore
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class CoreProperty : Attribute
    {
        private bool unique = false;
        private bool autoIncreasment = false;
        public readonly string Name;
        public readonly string Table;
        public bool Unique { get { return this.unique; } set { this.unique = value; } }
        public bool AutoIncreasment { get { return this.autoIncreasment; } set { this.autoIncreasment = value; } }
        public readonly object DefaultValue;
        public CoreProperty(string tablename, string name, object defaultval)
        {
            this.Table = tablename;
            this.Name = name;
            this.DefaultValue = defaultval;
        }
    }
}
