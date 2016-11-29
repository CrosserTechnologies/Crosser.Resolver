namespace Crosser.Resolve.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// The generic class for mapping One/Many instances
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DependencyObject<T>
    {
        /// <summary>
        /// Can the depencency be overwritten with another depencency?
        /// 
        /// By default false
        /// </summary>
        internal bool Rewritable { get; set; }

        /// <summary>
        /// If disabled you will not be able to get an instance of the dependency
        /// </summary>
        internal bool Enabled { get; set; }

        /// <summary>
        /// The func that is resposible for creating the dependency object
        /// </summary>
        internal Func<T> Creator { get; set; }

        /// <summary>
        /// The concrete type that the depencency object represents 
        /// </summary>
        internal Type InstanceType { get; set; }

        /// <summary>
        /// Information about the dependency object. Metadata or what ever you like
        /// </summary>
        internal IDictionary<string,object> Properties { get; set; }

        public DependencyObject(){}


        public DependencyObject(Expression<Func<T>> creator, bool rewritable = false, bool enabled = true, IDictionary<string,object> properties = null)
        {
            this.Creator = creator.Compile();
            this.InstanceType = creator.Body.Type;
            this.Enabled = enabled;
            this.Rewritable = rewritable;
            this.Properties = properties;
        }

        public virtual void Reset()
        {
            this.Creator = null;
            this.Rewritable = false;
            this.InstanceType = null;
            this.Enabled = true;
            
            this.Properties?.Clear();
        }

        /// <summary>
        /// Returns a new instance of the mapped object
        /// </summary>
        /// <returns></returns>
        public virtual T Get()
        {
            if (Enabled)
            {
                return this.Creator();
            }
            return default(T);
        }
    }
}