using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreDataStore
{
    [AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public class CoreStruct : Attribute
    {
        public readonly string Name;
        private bool _crypted = false;
        public bool Crypted { get { return this._crypted; } set { this._crypted = value; } }
        public CoreStruct(string name)
        {
            this.Name = name;
        }
    }
}
