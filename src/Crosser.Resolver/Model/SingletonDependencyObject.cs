namespace Crosser.Resolve.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// The generic class for mapping Singleton instances
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonDependencyObject<T> : DependencyObject<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private  static readonly object locker = new object();
        internal T  Instance { get; set; }

        public SingletonDependencyObject():base()
        {
        }

        public SingletonDependencyObject(Expression<Func<T>> creator, bool rewritable = false, bool enabled = true, IDictionary<string, object> properties = null) :base(creator, rewritable, enabled, properties){}

        public override void Reset()
        {
            this.Instance = default(T);
            base.Reset();
        }

        public override T Get()
        {            
            if (Enabled)
            {
                return this.Instance;
            }
            return default(T);
        }

        public void Set(Expression<Func<T>> creator, bool rewritable = false, bool enabled = true, IDictionary<string, object> properties = null)
        {
            lock (locker)
            {
                if (this.Instance != null && this.Rewritable == false)
                {                    
                    //Not allowed to override
                    throw new Exception(string.Format("The singleton type {0} has already been mapped and that mapping does not allow rewriting", typeof(T).Name));
                }
                // Set the latest rule for overriding
                this.Rewritable = rewritable;
                this.Creator = creator.Compile();
                this.Instance = this.Creator();
                this.Enabled = enabled;
                this.Properties = properties;
                this.InstanceType = creator.Body.Type; // creator.Method.ReturnType;//
            }
        }
    }
}