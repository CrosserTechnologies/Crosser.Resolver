﻿namespace Crosser.Resolve
{
    using System;
    using System.Collections.Generic;
    using Model;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Linq.Expressions;    
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Handles mapping between a interface and a concrete type
    /// 
    /// This is grouped transient objects...
    /// </summary>
    /// <typeparam name="TInterface">Interface to map to</typeparam>
    public static class Many<TInterface>
    {
        /// <summary>
        /// The Func that will return a new instance of the interface
        /// </summary>
        private static IList<DependencyObject<TInterface>> dependencies;

        /// <summary>
        /// A lookup dictionary for named instances
        /// </summary>
        private static ConcurrentDictionary<string, DependencyObject<TInterface>> lookUp;

        static Many()
        {
            //Verify that T is an interface
            if (!typeof(TInterface).IsInterface)
            {
                throw new ResolverException("T is not an interface");
            }
            dependencies = new List<DependencyObject<TInterface>>();
            lookUp = new ConcurrentDictionary<string, DependencyObject<TInterface>>();
        }

        /// <summary>
        /// This will clear the mapping.
        /// </summary>
        public static void Reset()
        {
            dependencies.Clear();
            lookUp.Clear();
        }

        /// <summary>
        /// Add a new transient mapping
        /// </summary>
        /// <param name="creator">The Func that creates your instance in the mapping</param>
        /// <param name="rewritable">If true the mapping will be overwritten if a new mapping with the same signature is added. If false there can be multiple mappings with the same signature</param>
        /// <param name="enabled">If false the mapping is disabled and the instance will not be created</param>
        /// <param name="namedInstance">Will allow to get a transient instance by a custom name</param>
        /// <param name="properties">Lets you set custom properties as metadata for a mapping signature</param>
        public static void Add(Expression<Func<TInterface>> creator, bool rewritable = false, bool enabled = true, string namedInstance = null, IDictionary<string, object> properties = null)
        {
            var i = new DependencyObject<TInterface>(creator, rewritable, enabled, properties);
            if (dependencies.Any(p => p.InstanceType == i.InstanceType))
            {
                IList<DependencyObject<TInterface>> toRemove = new List<DependencyObject<TInterface>>();
                foreach (var dependency in dependencies.Where(p => p.InstanceType == i.InstanceType))
                {
                    if (dependency.Rewritable)
                    {
                        toRemove.Add(dependency);
                    }
                }

                foreach (var removeMe in toRemove)
                {
                    dependencies.Remove(removeMe);
                    if (!string.IsNullOrEmpty(namedInstance))
                    {
                        DependencyObject<TInterface> _;
                        lookUp.TryRemove(namedInstance, out _);
                    }
                }
            }
            dependencies.Add(i);
            if (!string.IsNullOrEmpty(namedInstance))
            {
                lookUp.TryAdd(namedInstance, i);
            }
        }

        /// <summary>
        /// Removes all mappings to the Type of TClass where TClass implements TInterface
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        public static void Remove<TClass>()
        {
            IList<DependencyObject<TInterface>> toRemove = new List<DependencyObject<TInterface>>();
            foreach (var dependency in dependencies.Where(p => p.InstanceType == typeof(TClass)))
            {
                toRemove.Add(dependency);
            }
            foreach (var dependency in toRemove)
            {
                dependencies.Remove(dependency);
            }
        }

        /// <summary>
        /// Will return all enabled mapping for this interface
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TInterface> GetAll()
        {
            for (var i = 0; i < dependencies.Count; i++)
            {
                if (dependencies[i].Enabled)
                    yield return dependencies[i].Creator();
            }
        }

        /// <summary>
        /// Returns a transient instance of type T (if found)
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
        }

        /// <summary>
        /// Disables all mapped dependencies
        /// </summary>
        public static void DisableAll()
        {
            foreach (var dependency in dependencies)
                dependency.Enabled = false;
        }

        /// <summary>
        /// Enables all types of a specific type TClass where TClass implements TInterface
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        public static void EnableAllOf<TClass>() where TClass : class, TInterface
        {
            foreach (var dependency in dependencies.Where(p => p.InstanceType == typeof(TClass)))
                dependency.Enabled = true;
        }

        /// <summary>
        /// Disables all types of a specific type TClass where TClass implements TInterface
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        public static void DisableAllOf<TClass>() where TClass : class, TInterface
        {
            foreach (var dependency in dependencies.Where(p => p.InstanceType == typeof(TClass)))
                dependency.Enabled = false;
        }
    }
}
