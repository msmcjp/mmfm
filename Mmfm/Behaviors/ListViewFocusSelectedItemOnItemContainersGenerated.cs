using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Mmfm.Behaviors
{
    public class ListViewFocusSelectedItemOnItemContainersGenerated : Behavior<ListView>
    {        
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;            
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
            base.OnDetaching();
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (AssociatedObject.IsFocused && AssociatedObject.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                // https://stackoverflow.com/questions/7366961/listbox-scrollintoview-when-using-collectionviewsource-with-groupdescriptions-i
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    var container = AssociatedObject.ItemContainerGenerator.ContainerFromItem(AssociatedObject.SelectedItem);
                    (container as UIElement)?.Focus();
                }));
            }
        }
    }
}
