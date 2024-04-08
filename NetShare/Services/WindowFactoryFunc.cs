using NetShare.ViewModels;
using System;
using System.Windows;

namespace NetShare.Services
{
    public class WindowFactoryFunc(Func<Type, Window?> factoryFunc) : IWindowFactory
    {
        private readonly Func<Type, Window?> factoryFunc = factoryFunc;

        public Window? GetWindow<T>() where T : ViewModelBase
        {
            return factoryFunc(typeof(T));
        }
    }
}
