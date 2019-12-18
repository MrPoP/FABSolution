using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.Collections.Generic
{
    public class AttributeInvocationMapper<mAction, mAttribute, mKey>
        where mAttribute : Attribute
        where mAction : class
        where mKey : struct
    {
        private readonly Func<mAttribute, mKey> Translator;
        private readonly SafeDictionary<mKey, mAction> mapper;
        public AttributeInvocationMapper(Func<mAttribute, mKey> translator)
        {
            this.mapper = new SafeDictionary<mKey, mAction>();
            this.Translator = translator;
            var assembly = Assembly.GetCallingAssembly();
            foreach (var types in assembly.GetTypes())
            {
                try
                {
                    var methods = types.GetMethods();
                    foreach (var method in methods)
                    {
                        foreach (mAttribute attribute in method.GetCustomAttributes<mAttribute>(true))
                        {
                            if (attribute != null)
                            {
                                mKey key = translator(attribute);
                                Delegate invoke = Delegate.CreateDelegate(typeof(mAction), method);
                                this.mapper.Add(key, (invoke as mAction));
                            }
                        }
                    }
                }
                catch (Exception e) { throw e; }
            }
        }
        public void LoadAssembly(Assembly assembly)
        {
            foreach (var types in assembly.GetTypes())
            {
                try
                {
                    var methods = types.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var method in methods)
                    {
                        foreach (mAttribute attribute in method.GetCustomAttributes<mAttribute>(true))
                        {
                            if (attribute != null)
                            {
                                mKey key = this.Translator(attribute);
                                Delegate invoke = Delegate.CreateDelegate(typeof(mAction), method);
                                this.mapper.Add(key, (invoke as mAction));
                            }
                        }
                    }
                }
                catch (Exception e) { throw e; }
            }
        }
        public mAction this[mKey key]
        {
            get
            {
                mAction val = null;
                if (this.mapper.ContainsKey(key))
                {
                    val = this.mapper[key];
                }
                return val;
            }
            set
            {
                if (this.mapper.ContainsKey(key))
                {
                    this.mapper[key] = value;
                }
                else
                {
                    this.mapper.Add(key, value);
                }
            }
        }

        public bool TryGetValue(mKey key, out mAction value)
        {
            return this.mapper.TryGetValue(key, out value);
        }
    }
}
