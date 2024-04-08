using Microsoft.Xaml.Behaviors;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetShare.Behaviors
{
    public class NumericInputBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += PreviewTextInput;
            DataObject.AddPastingHandler(AssociatedObject, Paste);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= PreviewTextInput;
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(!Regex.IsMatch(e.Text, "[0-9]+"))
            {
                e.Handled = true;
            }
        }

        private void Paste(object sender, DataObjectPastingEventArgs e)
        {
            if(e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.DataObject.GetData(DataFormats.Text);
                if(!Regex.IsMatch(text, "[0-9]+"))
                {
                    e.CancelCommand();
                }
                return;
            }
            e.CancelCommand();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(sender is TextBox textBox)
            {
                if(e.Key == Key.Enter)
                {
                    FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBox), null);
                    Keyboard.ClearFocus();
                }
            }
        }
    }
}
