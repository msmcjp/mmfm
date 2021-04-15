using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace Mmfm
{
    public class DragAdorner : Adorner
    {
        private FrameworkElement adornedElement, element;
        private VisualBrush brush;

        public static readonly DependencyProperty PointProperty = DependencyProperty.Register(
            "Point",
            typeof(Point),
            typeof(DragAdorner),
            new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public Point Point
        {
            get => (Point)GetValue(PointProperty);
            set => SetValue(PointProperty, value);               
        }

        public DragAdorner(FrameworkElement adornedElement, FrameworkElement element) : base(adornedElement)
        {
            this.adornedElement = adornedElement;
            this.element = element;
            brush = new VisualBrush(element);

            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var p = Point;
            p.Offset(-32, 0);
            drawingContext.DrawRectangle(brush, null, new Rect(p, element.RenderSize));
        }
    }

    public class DragDropBehavior : Behavior<FrameworkElement>
    {
        #region DataObjectProperty
        public static readonly DependencyProperty DataObjectProperty = DependencyProperty.RegisterAttached(
            "DataObject",
            typeof(IDataObject),
            typeof(DragDropBehavior),
            new FrameworkPropertyMetadata(null)
        );

        public static object GetDataObject(DependencyObject obj) => obj.GetValue(DataObjectProperty);
        public static void SetDataObject(DependencyObject obj, object value) => obj.SetValue(DataObjectProperty, value);
        #endregion

        #region CommandProperty
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(DragDropBehavior),
            new FrameworkPropertyMetadata(null)
        );

        public static ICommand GetCommand(DependencyObject obj) => obj.GetValue(CommandProperty) as ICommand;
        public static void SetCommand(DependencyObject obj, ICommand value) => obj.SetValue(CommandProperty, value);
        #endregion

        #region AdornerDataTemplateProperty
        public static readonly DependencyProperty AdornerDataTemplateProperty = DependencyProperty.RegisterAttached(
            "AdornerDataTemplate",
            typeof(DataTemplate),
            typeof(DragDropBehavior),
            new FrameworkPropertyMetadata(null)
        );

        public static DataTemplate GetAdornerDataTemplate(DependencyObject obj) => obj.GetValue(AdornerDataTemplateProperty) as DataTemplate;
        public static void SetAdornerDataTemplate(DependencyObject obj, DataTemplate value) => obj.SetValue(AdornerDataTemplateProperty, value);
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Drop += AssociatedObject_Drop;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.DragEnter += AssociatedObject_DragEnter;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Drop -= AssociatedObject_Drop;
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.DragEnter -= AssociatedObject_DragEnter;
            base.OnDetaching();
        }

        private bool? isDragging = null;
        private Point startPoint;
        private DragAdorner adorner;

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null || GetDataObject(element) == null)
            {
                return;
            }
 
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(element);
                if (isDragging == null)
                {
                    startPoint = p;
                    isDragging = false;
                }
 
                double l = Math.Sqrt(Math.Pow(p.X - startPoint.X, 2) + Math.Pow(p.Y - startPoint.Y, 2));
                if(l > 10.0 && isDragging == false)
                {
                    isDragging = true;

                    var dataObject = GetDataObject(element);
                    var dataTemplate = GetAdornerDataTemplate(element);
                    var content = dataTemplate?.LoadContent() as FrameworkElement;
                    if(content != null)
                    {
                        content.DataContext = dataObject;
                        adorner = new DragAdorner(element, content);
                        adorner.Point = e.GetPosition(element);
                        AdornerLayer.GetAdornerLayer(element).Add(adorner);
                    }
                    var hookId = NativeMethods.HookMouseMove(point => adorner.Point = AssociatedObject.PointFromScreen(point));                        
                    try
                    {
                        DragDrop.DoDragDrop(element, dataObject, DragDropEffects.Copy);
                    }
                    finally
                    {
                        NativeMethods.RemoveHook(hookId);
                    }

                    if(content != null)
                    {
                        AdornerLayer.GetAdornerLayer(adorner.AdornedElement).Remove(adorner);
                        adorner = null;
                    }
                }
            }
            else
            {
                isDragging = null;
            }
        }

        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            AssociatedObject.Focus();
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            var command = GetCommand(AssociatedObject);
            if (command?.CanExecute(e.Data) == true)
            {
                command?.Execute(e.Data);
            }
        }
    }
}
