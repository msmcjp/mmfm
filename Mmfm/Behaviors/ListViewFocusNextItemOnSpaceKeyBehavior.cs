using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Mmfm.Behaviors
{
    public class ListViewFocusNextItemOnSpaceKeyBehavior : Behavior<ListView>
    {       
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyUp += ListView_KeyUp;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.KeyUp -= ListView_KeyUp;
            base.OnDetaching();
        }    

        private void ListView_KeyUp(object sender, KeyEventArgs e)
        { 
            if(e.Key == Key.Space)
            {
                var newIndex = AssociatedObject.SelectedIndex + 1;             
                if (newIndex < AssociatedObject.Items.Count)
                {
                    ((ListViewItem)AssociatedObject.ItemContainerGenerator.ContainerFromIndex(newIndex)).Focus();
                }
            }
        }
    }
}
