using System;
using System.Windows;

namespace NetShare
{
    public static class Extensions
    {
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
