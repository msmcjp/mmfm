using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Linq;

namespace Mmfm
{
    public class ListViewExclusiveSelectionBehavior : Behavior<ListView>
    {
        private static ListView lastFocusedListView = null;
        private static Dictionary<ListView, object> lastSelectedItems = new Dictionary<ListView, object>();

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            AssociatedObject.IsKeyboardFocusWithinChanged += AssociatedObject_IsKeyboardFocusWithinChanged;
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
            lastSelectedItems.Add(AssociatedObject, null);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            AssociatedObject.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
            AssociatedObject.IsKeyboardFocusWithinChanged -= AssociatedObject_IsKeyboardFocusWithinChanged;
            lastSelectedItems.Remove(AssociatedObject);
            base.OnDetaching();
        }

        private ListView LastFocusedListView
        {
            get => lastFocusedListView;
            set
            {                
                foreach(var listView in lastSelectedItems.Keys.Where(x => x != value))
                {
                    listView.SelectedItem = null;
                }
                lastFocusedListView = value;
                if(lastFocusedListView == AssociatedObject)
                {
                    lastFocusedListView.SelectedItem = lastSelectedItems[lastFocusedListView];
                }
            }
        }

        private void AssociatedObject_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.NewValue == true)
            {
                LastFocusedListView = AssociatedObject;
            }    
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {            
            if (AssociatedObject == LastFocusedListView &&
                AssociatedObject.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                // https://stackoverflow.com/questions/7366961/listbox-scrollintoview-when-using-collectionviewsource-with-groupdescriptions-i
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    var container = AssociatedObject.ItemContainerGenerator.ContainerFromItem(AssociatedObject.SelectedItem);
                    (container as UIElement)?.Focus();                    
                }));
            }
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LastFocusedListView == AssociatedObject)
            {
                lastSelectedItems[AssociatedObject] = AssociatedObject.SelectedItem;
            }
            else if(AssociatedObject.SelectedItem != null)
            {
                AssociatedObject.SelectedItem = null;
            }
        }
    }
}
