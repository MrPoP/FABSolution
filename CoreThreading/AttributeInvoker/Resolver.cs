using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;

namespace CoreThreading
{
    public static class Resolver
    {
        static CancellationTokenSource cancellationTokenSource;
        static readonly Dictionary<Guid, Tuple<Method, List<Property>>> items =
            new Dictionary<Guid, Tuple<Method, List<Property>>>();
        public static TaskStatus Status = TaskStatus.WaitingToRun;
        static readonly Func<Attribute, Guid> GetGuid = (p) => (p is Method) ? (p as Method).MethodGuid : ((p is Property) ? (p as Property).MethodGuid : Guid.Empty);
        public static Action<Attribute> Add = p =>
        {
            try
            {
                List<Property> properties = null;
                if (p is Method)
                {
                    if (!items.ContainsKey((p as Method).MethodGuid))
                    {
                        items.Add((p as Method).MethodGuid, null);
                    }
                    properties = items[(p as Method).MethodGuid].Item2;
                    items[(p as Method).MethodGuid] = Tuple.Create((p as Method), properties);
                    properties = null;
                }
                if (p is Property)
                {
                    if (!items.ContainsKey((p as Property).MethodGuid))
                    {
                        items.Add((p as Property).MethodGuid, null);
                    }
                    properties = items[(p as Property).MethodGuid].Item2;
                    items[(p as Property).MethodGuid] = Tuple.Create(default(Method), properties);
                    items[(p as Property).MethodGuid].Item2.Add((p as Property));
                    properties = null;
                }
            }
            catch (Exception es)
            {
                throw es;
            }
            finally
            {
                RunPool();
            }
        };
        public static void Start()
        {
            RunPool();
        }
        static async void RunPool()
        {
            if (Status == TaskStatus.Canceled)
                return;
            Dictionary<Method, List<Property>> toinvoke = null;
            goto GATHER_UNLOADED_DATA;
            #region GATHER_UNLOADED_DATA
        GATHER_UNLOADED_DATA:
            {
                while (Status == TaskStatus.WaitingToRun)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    using (Task<bool> task = GatherData(cancellationTokenSource.Token))
                    {
                        task.Start();
                        await task;
                        if (task.IsCompleted)
                        {
                            Status = task.Result == true ? TaskStatus.Created : TaskStatus.WaitingToRun;
                            goto CHECKING_PARAMS;
                        }
                    }
                }
            }
            #endregion
            #region CHECKING_PARAMS
        CHECKING_PARAMS:
            {
                while (Status == TaskStatus.Created)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    using (Task<Dictionary<Method, List<Property>>> task = CheckingParams(cancellationTokenSource.Token))
                    {
                        task.Start();
                        await task;
                        if (task.IsCompleted)
                        {
                            toinvoke = task.Result;
                            if (toinvoke != null)
                            {
                                Status = TaskStatus.WaitingForActivation;
                                goto INVOKE_METHODS;
                            }
                        }
                    }
                }
            }
            #endregion
            #region INVOKE_METHODS
        INVOKE_METHODS:
            {
                while (Status == TaskStatus.WaitingForActivation)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    using (Task<Exception> task = new Task<Exception>(() =>
                    {
                        try
                        {
                            CancellationTokenSource cancelsource = new CancellationTokenSource();
                            List<Task> tasks = new List<Task>();
                            foreach (var pair in toinvoke)
                            {
                                object instance = pair.Key.Instance;
                                if (pair.Value.Count > 0)
                                {
                                    tasks.Add(new Task(() =>
                                    {
                                        object[] parameters = pair.Value.Select(p => p.PropertyInfo.GetValue(p.Instance)).ToArray();
                                        pair.Key.MethodInfo.Invoke(instance, parameters);
                                        Task.Delay(pair.Key.Period).Wait();
                                    }));
                                }
                                else
                                {
                                    tasks.Add(new Task(() =>
                                    {
                                        pair.Key.MethodInfo.Invoke(instance, null);
                                        Task.Delay(pair.Key.Period).Wait();
                                    }));
                                }
                            }
                            Task.WaitAll(tasks.ToArray(), cancelsource.Token);
                            tasks = null;
                            return null;
                        }
                        catch (Exception e)
                        {
                            return e;
                        }
                    }, cancellationTokenSource.Token))
                    {
                        task.Start();
                        await task;
                        if (task.IsCompleted)
                        {
                            if (task.Result == null)
                            {
                                Status = TaskStatus.WaitingToRun;
                                goto GATHER_UNLOADED_DATA;
                            }
                            throw task.Result;
                        }
                    }
                }
            }
            #endregion
        }
        static Task<bool> GatherData(CancellationToken token)
        {
            return new Task<bool>(() =>
            {
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        foreach (Method method in type.GetCustomAttributes<Method>())
                        {
                            Add(method);
                        }
                        foreach (Property property in type.GetCustomAttributes<Property>())
                        {
                            Add(property);
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }, token);
        }
        static Task<Dictionary<Method, List<Property>>> CheckingParams(CancellationToken token)
        {
            return new Task<Dictionary<Method, List<Property>>>(() =>
            {
                try
                {
                    Dictionary<Method, List<Property>> assems = new Dictionary<Method, List<Property>>();
                    foreach (var value in items.Values)
                    {
                        assems.Add(value.Item1, new List<Property>());
                        value.Item2.ForEach(p =>
                        {
                            if (p.MethodGuid == value.Item1.MethodGuid)
                            {
                                if (value.Item1.MethodInfo.GetParameters().Count(e => p.PropertyInfo.GetType() == e.GetType()) > 0)
                                {
                                    assems[value.Item1].Add(p);
                                }
                            }
                        });
                    }
                    return assems;
                }
                catch
                {
                    return null;
                }
            }, token);
        }
    }
}
