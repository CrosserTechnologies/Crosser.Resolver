namespace Crosser.Resolve.Model
{
    using System;
    using System.Runtime.Serialization;

    public class ResolverException : Exception
    {
        public ResolverException():base(){}

        public ResolverException(string message):base(message){}

        public ResolverException(string message, Exception innerexception) : base(message, innerexception){}

        public ResolverException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
