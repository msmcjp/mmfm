using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyFileManager.Behaviors
{
    public class ListViewFocusOnSelectItemBehavior : Behavior<ListView>
    {
        private int lastSelectedIndex = 0;
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.IsKeyboardFocusWithinChanged += AssociatedObject_IsKeyboardFocusWithinChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.IsKeyboardFocusWithinChanged -= AssociatedObject_IsKeyboardFocusWithinChanged;
            base.OnDetaching();
        }

        private void AssociatedObject_IsKeyboardFocusWithinChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                lastSelectedIndex = AssociatedObject.SelectedIndex;
                AssociatedObject.SelectedIndex = -1;
            }
            else
            {
                AssociatedObject.SelectedIndex = lastSelectedIndex;
            }
        }
    }
}
