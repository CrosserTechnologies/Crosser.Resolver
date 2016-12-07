namespace Crosser.Resolve
{
    using System;
    using System.Collections.Generic;
    using Model;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Collections.Concurrent;
    using PlatformHelpers;
    using System.Threading;

    /// <summary>
    /// Handles mapping between a interface and concrete type(s)
    /// 
    /// This is grouped transient objects...
    /// </summary>
    /// <typeparam name="TInterface">Interface to map to</typeparam>
    public static class Many<TInterface>
    {
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

        /// <summary>
        /// The Func that will return a new instance of the interface
        /// </summary>
        private static IList<DependencyObject<TInterface>> dependencies;

        /// <summary>
        /// To avoid checking for Enabled flags
        /// </summary>
        private static DependencyObject<TInterface>[] availableDependencies;

        /// <summary>
        /// A lookup dictionary for named instances
        /// </summary>
        private static ConcurrentDictionary<string, DependencyObject<TInterface>> lookUp;

        static Many()
        {
            //Verify that TInterface is an interface
            if (!TypeExtensions.IsInterface<TInterface>())
            {                
                throw new ArgumentException("TInterface is not an interface");
            }
            dependencies = new List<DependencyObject<TInterface>>();
            availableDependencies = dependencies.ToArray();
            lookUp = new ConcurrentDictionary<string, DependencyObject<TInterface>>();
        }

        /// <summary>
        /// This will clear the mapping.
        /// </summary>
        public static void Reset()
        {
            try
            {
                _semaphore.Wait();
                dependencies.Clear();
                availableDependencies = dependencies.ToArray();
                lookUp.Clear();
            }
            catch { }
            finally { _semaphore.Release(); }
        }

        /// <summary>
        /// Add a new transient mapping
        /// </summary>
        /// <param name="creator">The Func that creates your instance in the mapping</param>
        /// <param name="rewritable">If true the mapping will be overwritten if a new mapping with the same signature is added. If false there can be multiple mappings with the same signature</param>
        /// <param name="enabled">If false the mapping is disabled and the instance will not be created</param>
        /// <param name="namedInstance">Will allow to get a transient instance by a custom name</param>
        /// <param name="properties">Lets you set custom properties as metadata for a mapping signature</param>
        public static bool Add(Expression<Func<TInterface>> creator, bool rewritable = false, bool enabled = true, string namedInstance = null, IDictionary<string, object> properties = null)
        {
            try 
            {
                _semaphore.Wait();
                var i = new DependencyObject<TInterface>(creator, rewritable, enabled, properties);
                if (dependencies.Any(p => p.InstanceType == i.InstanceType))
                {
                    IList<DependencyObject<TInterface>> toRemove = new List<DependencyObject<TInterface>>();
                    foreach (var dependency in dependencies.Where(p => p.InstanceType == i.InstanceType))
                    {
                        if (!dependency.Rewritable) return false;
                        
                        toRemove.Add(dependency);                        
                    }

                    foreach (var removeMe in toRemove)
                    {
                        dependencies.Remove(removeMe);                        
                        if (!string.IsNullOrEmpty(namedInstance))
                        {
                            DependencyObject<TInterface> _;
                            if(!lookUp.TryRemove(namedInstance, out _))
                            {
                                return false;
                            }
                        }
                    }
                }
                dependencies.Add(i);
                if (!string.IsNullOrEmpty(namedInstance))
                {
                    if(!lookUp.TryAdd(namedInstance, i))
                    {
                        //if (ResolverConfig.ThrowErrorOnDeniedMapping)
                        //{
                        //    throw new Exception(string.Format("The type {0} has already been mapped and that mapping does not allow rewriting", typeof(TInterface).Name));
                        //}
                        return false;
                    }
                }

                availableDependencies = dependencies.ToArray();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Removes all mappings to the Type of TClass where TClass implements TInterface
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        public static void Remove<TClass>()
        {
            try
            {
                _semaphore.Wait();
                IList<DependencyObject<TInterface>> toRemove = new List<DependencyObject<TInterface>>();
                foreach (var dependency in dependencies.Where(p => p.InstanceType == typeof(TClass)))
                {
                    toRemove.Add(dependency);
                }
                foreach (var dependency in toRemove)
                {
                    dependencies.Remove(dependency);
                }
                availableDependencies = dependencies.ToArray();
            }
            catch{}
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Will return all enabled mapping for this interface
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TInterface> GetAll()
        {
            foreach (var obj in availableDependencies)
            {
                yield return obj.Creator();
            }
        }

        /// <summary>
        /// Returns a transient instance of type TInterface (if found)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TInterface GetNamedInstance(string name)
        {
            DependencyObject<TInterface> o;
            if (lookUp.TryGetValue(name, out o))
                return o.Creator();
            return default(TInterface);
        }

        /// <summary>
        /// Returns properties for the first type of TClass where TClass implements TInterface
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        /// <returns></returns>
        public static IDictionary<string, object> Properties<TClass>() where TClass : class, TInterface
        {
            return dependencies.First(p => p.InstanceType == typeof(TClass)).Properties;
        }

        /// <summary>
        /// Returns properties for a named instance
        /// </summary>
        /// <param name="namedInstance"></param>
        /// <returns></returns>
        public static IDictionary<string, object> Properties(string namedInstance)
        {
            DependencyObject<TInterface> o;
            if (lookUp.TryGetValue(namedInstance, out o))
                return o.Properties;
            return null;
        }

        /// <summary>
        /// Enables all mapped dependencies
        /// </summary>
        public static void EnableAll()
        {
            foreach (var dependency in dependencies)
                dependency.Enabled = true;

            availableDependencies = dependencies.ToArray();
        }

        /// <summary>
        /// Disables all mapped dependencies
        /// </summary>
        public static void DisableAll()
        {
            foreach (var dependency in dependencies)
                dependency.Enabled = false;

            availableDependencies = new DependencyObject<TInterface>[0];
        }

        /// <summary>
        /// Enables all types of a specific type TClass where TClass implements TInterface
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        public static void EnableAllOf<TClass>() where TClass : class, TInterface
        {
            foreach (var dependency in dependencies.Where(p => p.InstanceType == typeof(TClass)))
                dependency.Enabled = true;

            availableDependencies = dependencies.Where(p => p.Enabled).ToArray();
        }

        /// <summary>
        /// Disables all types of a specific type TClass where TClass implements TInterface
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        public static void DisableAllOf<TClass>() where TClass : class, TInterface
        {
            foreach (var dependency in dependencies.Where(p => p.InstanceType == typeof(TClass)))
                dependency.Enabled = false;

            availableDependencies = dependencies.Where(p => p.Enabled).ToArray();
        }
    }
}
