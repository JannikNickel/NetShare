using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace NetShare.Behaviors
{
    internal class EnterSubmitBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyDown += KeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.KeyDown -= KeyDown;
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if(sender is DependencyObject depObj)
            {
                if(e.Key == Key.Enter)
                {
                    FocusManager.SetFocusedElement(FocusManager.GetFocusScope(depObj), null);
                    Keyboard.ClearFocus();
                }
            }
        }
    }
}
