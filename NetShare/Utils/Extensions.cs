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

        public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> enumerable)
        {
            int i = 0;
            foreach(T item in enumerable)
            {
                yield return (i, item);
            }
        }

        public static T? GetService<T>(this IServiceProvider? serviceProvider) where T : class
        {
            return serviceProvider?.GetService(typeof(T)) as T;
        }

        public static void CenterPositionToWindow(this Window window, Window other)
        {
            double lOff = window.Width * -0.5 + other.Width * 0.5;
            double tOff = window.Height * -0.5 + other.Height * 0.5;
            window.Left = other.Left + lOff;
            window.Top = other.Top + tOff;
        }
    }
}
