using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreDataStore
{
    
    public class DataCoreBuilder
    {
        public readonly string Name;
        public readonly string FileName;
        public readonly string AssemblyName;
        private AssemblyBuilder builder;
        public bool TargetExisted { get { return File.Exists(this.Name); } }
        public DataCoreBuilder(string fullname)
        {
            this.Name = fullname;
            this.FileName = Path.GetFileName(fullname);
            this.AssemblyName = Path.GetFileNameWithoutExtension(fullname);
            AssemblyName aName = new AssemblyName(this.AssemblyName);
            List<Task> tasks = new List<Task>();
            tasks.Add(CreateOrLoad(aName));
        }
        private Task CreateOrLoad(AssemblyName name)
        {
            return new Task(() =>
                {
                    if (TargetExisted)
                    {
                        this.builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
                    }
                    else
                    {

                    }
                }, new CancellationToken(false));
        }
    }
}
