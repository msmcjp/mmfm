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

namespace Mmfm.Behaviors
{
    public class ListViewFocusSelectedIndexOnItemContainersGenerated : Behavior<ListView>
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
            if (AssociatedObject.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                if(AssociatedObject.SelectedIndex < 0)
                {
                    return;
                }
                var container = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(AssociatedObject.SelectedIndex);
                (container as UIElement)?.Focus();
            }
        }
    }
}
