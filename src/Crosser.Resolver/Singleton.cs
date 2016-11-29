namespace Crosser.Resolve
{
    using System;
    using Model;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    
    /// <summary>
    /// Handles mapping for a interface and a concrete type
    /// 
    /// This is singleton objects...
    /// </summary>
    /// <typeparam name="TInterface">Interface to map to</typeparam>
    public static class Singleton<TInterface>
    {
        /// <summary>
        /// The singleton dependency instance
        /// </summary>
        private static readonly SingletonDependencyObject<TInterface> dependencyObject;
        
        static Singleton()
        {
            //Verify that TInterface is an interface
            if (!typeof(TInterface).IsInterface)
            {
                throw new ResolverException("TInterface is not an interface");
            }
            dependencyObject = new SingletonDependencyObject<TInterface>();
        }

        /// <summary>
        /// This will clear the mapping for TInterface.
        /// </summary>
        public static void Reset()
        {
            dependencyObject.Reset();
        }

        /// <summary>
        /// Maps the interface to a concrete singleton type
        /// </summary>
        /// <param name="creator">The func that creates the concrete type</param>
        /// <param name="rewritable">Pass in true to be able to change the mapping later</param>
        public static void As(Expression<Func<TInterface>> creator, bool rewritable = false, bool enabled = true, IDictionary<string, object> properties = null)
        {
            dependencyObject.Set(creator, rewritable, enabled, properties);            
        }

        /// <summary>
        /// Returns the mapped properties (metadata) for this <see cref="DependencyObject{T}"/> object
        /// </summary>
        public static IDictionary<string, object> Properties => dependencyObject.Properties;

        /// <summary>
        /// Returns the TInterface from the func passed in to the method <see cref="As(Func{TInterface}, bool, bool, IDictionary{string, object})"/>
        /// </summary>
        /// <exception cref="Exception">Throws if there is no mapping available for T</exception>
        /// <returns>Returns the concrete singleton type of T</returns>        
        public static TInterface Get()
        {
            return dependencyObject.Get();
        }

        /// <summary>
        /// Enables the singleton
        /// </summary>
        public static void Enable()
        {
            dependencyObject.Enabled = true;
        }

        /// <summary>
        /// Disabled the singleton
        /// </summary>
        public static void Disable()
        {
            dependencyObject.Enabled = false;
        }
    }
}
