using System;
using System.Collections.Generic;
using System.Windows;

namespace NetShare
{
    public static class Extensions
    {
        public static bool SetIfChanged<T>(this ref T field, T value) where T : unmanaged
        {
            if(EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            return true;
        }

        public static T? GetService<T>(this IServiceProvider? serviceProvider) where T : class
        {
            return serviceProvider?.GetService(typeof(T)) as T;
        }
    }
}
