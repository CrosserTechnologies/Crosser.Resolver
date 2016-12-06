namespace Crosser.Resolve
{
    using System;
    using Model;
    using System.Collections.Generic;
    using PlatformHelpers;

    /// <summary>
    /// Handles mapping between a interface and concrete types
    /// 
    /// This is transient objects...
    /// </summary>
    /// <typeparam name="TInterface">Interface to map to</typeparam>
    public static class One<TInterface>
    {
        private static readonly DependencyObject<TInterface> dependencyObject;
        
        static One()
        {
            //Verify that TInterface is an interface
            if (!TypeExtensions.IsInterface<TInterface>())
            {
                throw new ArgumentException("TInterface is not an interface");
            }
            dependencyObject = new DependencyObject<TInterface>();            
        }

        /// <summary>
        /// This will clear the mapping.
        /// </summary>
        public static void Reset()
        {
            dependencyObject.Reset();
        }

        /// <summary>
        /// Maps the interface to a concrete type
        /// </summary>
        /// <param name="creator">The func that creates the concrete type</param>
        /// <param name="rewritable">Pass in true to be able to change the mapping later</param>        
        public static bool As(Func<TInterface> creator, bool rewritable = false, bool enabled = true, IDictionary<string, object> properties = null)
        {
            if (dependencyObject.Creator != null && dependencyObject.Rewritable == false)
            {
                if (ResolverConfig.ThrowErrorOnDeniedMapping)
                { 
                    throw  new Exception(string.Format("The type {0} has already been mapped and that mapping does not allow rewriting",typeof(TInterface).Name));
                }
                return false;
            }
            // Set the latest rule for overriding
            dependencyObject.Rewritable = rewritable;
            dependencyObject.Creator = creator;
            dependencyObject.Enabled = enabled;
            dependencyObject.Properties = properties;
            return true;
        }

        /// <summary>
        /// Returns a transient object for TInterface from the func passed in to the method <see cref="As(Func{TInterface}, bool, bool, IDictionary{string, object})"/>
        /// </summary>
        /// <returns>Returns the concrete type of TInterface</returns>        
        public static TInterface Get()
        {            
            return dependencyObject.Get();
        }

        /// <summary>
        /// Returns the mapped properties (metadata) for this <see cref="DependencyObject{TInterface}"/>
        /// </summary>
        public static IDictionary<string, object> Properties => dependencyObject.Properties;

        /// <summary>
        /// Enables the transient object
        /// </summary>
        public static void Enable()
        {
            dependencyObject.Enabled = true;
        }

        /// <summary>
        /// Disables the transient object
        /// </summary>
        public static void Disable()
        {
            dependencyObject.Enabled = false;
        }
    }
}
