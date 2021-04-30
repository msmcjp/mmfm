using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mmfm
{
    /// <summary>
    /// https://www.codeproject.com/Tips/1035207/Number-Only-Behavior-for-WPF
    /// </summary>
    public class NumericOnlyTextBoxBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += AssociatedObject_PreviewTextInput;
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            DataObject.AddPastingHandler(AssociatedObject, AssociatedObject_OnPaste);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewTextInput -= AssociatedObject_PreviewTextInput;
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            DataObject.RemovePastingHandler(AssociatedObject, AssociatedObject_OnPaste);
            base.OnDetaching();
        }

        private void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) e.Handled = true;
        }

        private void AssociatedObject_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Any(c => !char.IsDigit(c))) { e.Handled = true; }
        }

        private void AssociatedObject_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var text = Convert.ToString(e.DataObject.GetData(DataFormats.Text)).Trim();
                if (text.Any(c => !char.IsDigit(c))) { e.CancelCommand(); }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
