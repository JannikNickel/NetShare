using System;

namespace NetShare
{
    public static class Extensions
    {
        public static T? GetService<T>(this IServiceProvider? serviceProvider) where T : class
        {
            return serviceProvider?.GetService(typeof(T)) as T;
        }
    }
}
