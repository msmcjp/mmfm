using Microsoft.Xaml.Behaviors;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mmfm
{
    public class HotKeyEditBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            base.OnDetaching();
        }

        private void AssociatedObject_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;

            var modifiers = Keyboard.Modifiers;
            var key = e.Key;

            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            if (modifiers == ModifierKeys.None && 
                (key == Key.Delete || key == Key.Back || key == Key.Escape))
            {
                AssociatedObject.Text = "";
                return;
            }

            var ignoreList = new Key[]
            {
                Key.LeftCtrl,
                Key.RightCtrl,
                Key.LeftAlt,
                Key.RightAlt,
                Key.LeftShift,
                Key.RightShift,
                Key.LWin,
                Key.RWin,
                Key.Clear,
                Key.OemClear,
                Key.Apps,
            };

            if (ignoreList.Contains(key))
            {
                return;
            }

            try
            {
                var keyGesture = new KeyGesture(key, modifiers);
                AssociatedObject.Text = new KeyGestureConverter().ConvertToString(keyGesture);
            }
            catch
            {
            }       
        }
    }
}
