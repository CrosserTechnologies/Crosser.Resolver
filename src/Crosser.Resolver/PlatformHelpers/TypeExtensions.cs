namespace Crosser.Resolve.PlatformHelpers
{
#if DOTNETCORE
    using System.Reflection;
#endif

    /// <summary>
    /// Extensions for getting type information
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// returns true if T is an interface, otherwise false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsInterface<T>()
        {
#if DOTNETCORE
            return typeof(T).GetTypeInfo().IsInterface;
#endif
#if NET
            return typeof(T).IsInterface;
#endif
        }
    }
}
