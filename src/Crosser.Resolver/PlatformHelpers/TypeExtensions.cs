namespace Crosser.Resolve.PlatformHelpers
{
#if DOTNETCORE
    using System.Reflection;
#endif

    public static class TypeExtensions
    {
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
