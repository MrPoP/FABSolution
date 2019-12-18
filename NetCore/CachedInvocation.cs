using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NetCore.Packets;

namespace NetCore
{
    public class CachedInvocation<ToPCode, Tobject> : ConcurrentDictionary<ToPCode, Tobject>
        where ToPCode : struct
        where Tobject : struct
    {
        public CachedInvocation(Func<Packet, ushort> translator)
        {
            try
            {
                Assembly myAssembly = Assembly.GetExecutingAssembly();
                var types = myAssembly.GetTypes();

                var attributes = Attribute.GetCustomAttributes(typeof(None), typeof(Packet));
                if(attributes.Any())
                {
                    foreach(var attribute in attributes)
                    {
                        
                    }
                }
            }
            catch
            {

            }
        }
    }
}
