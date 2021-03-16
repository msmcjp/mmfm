using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Mmfm
{
    public class ListViewExclusiveSelectionBehavior : Behavior<ListView>
    {
        private static ListView lastFocusedListView = null;
        private static Dictionary<ListView, int> lastSelectedIndex = new Dictionary<ListView, int>();

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            AssociatedObject.IsKeyboardFocusWithinChanged += AssociatedObject_IsKeyboardFocusWithinChanged;
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
            lastSelectedIndex.Add(AssociatedObject, -1);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            AssociatedObject.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
            AssociatedObject.IsKeyboardFocusWithinChanged -= AssociatedObject_IsKeyboardFocusWithinChanged;
            lastSelectedIndex.Remove(AssociatedObject);
            base.OnDetaching();
        }

        private void AssociatedObject_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.NewValue == true)
            {
                lastFocusedListView = AssociatedObject;
                AssociatedObject.SelectedIndex = lastSelectedIndex[AssociatedObject];
            }
            else
            {
                lastSelectedIndex[AssociatedObject] = AssociatedObject.SelectedIndex;
                AssociatedObject.SelectedIndex = -1;
            }
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {            
            if (AssociatedObject == lastFocusedListView && AssociatedObject.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                // https://stackoverflow.com/questions/7366961/listbox-scrollintoview-when-using-collectionviewsource-with-groupdescriptions-i
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    var container = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(AssociatedObject.SelectedIndex);
                    (container as UIElement)?.Focus();
                }));
            }
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssociatedObject != lastFocusedListView && AssociatedObject.SelectedIndex != -1)
            {
                AssociatedObject.SelectedIndex = -1;
            }
        }
    }
}
