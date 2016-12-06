namespace Crosser.Resolve.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// The generic class for mapping Singleton instances
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    public class SingletonDependencyObject<TInterface> : DependencyObject<TInterface>
    {
        // ReSharper disable once StaticMemberInGenericType
        private  static readonly object locker = new object();
        internal TInterface Instance { get; set; }

        public SingletonDependencyObject():base()
        {
        }

        //public SingletonDependencyObject(Expression<Func<TInterface>> creator, bool rewritable = false, bool enabled = true, IDictionary<string, object> properties = null) :base(creator, rewritable, enabled, properties){}

        public override void Reset()
        {
            this.Instance = default(TInterface);
            base.Reset();
        }

        public override TInterface Get()
        {            
            if (Enabled)
            {
                return this.Instance;
            }
            return default(TInterface);
        }

        public bool Set(Expression<Func<TInterface>> creator, bool rewritable = false, bool enabled = true, IDictionary<string, object> properties = null)
        {
            lock (locker)
            {
                if (this.Instance != null && this.Rewritable == false)
                {
                    //Throw if config says so...
                    if (ResolverConfig.ThrowErrorOnDeniedMapping)
                    {
                        throw new Exception(string.Format("The singleton type {0} has already been mapped and that mapping does not allow rewriting", typeof(TInterface).Name));
                    }
                    return false;
                }
                // Set the latest rule for overriding
                this.Rewritable = rewritable;
                this.Creator = creator.Compile();
                this.Instance = this.Creator();
                this.Enabled = enabled;
                this.Properties = properties;
                this.InstanceType = creator.Body.Type;
                return true;
            }
        }
    }
}