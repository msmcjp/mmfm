using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Interop;

namespace Mmfm
{
    public class HwndSourceHookAttachedProperty
    {
        public static readonly DependencyProperty WndProcProperty = DependencyProperty.RegisterAttached(
            "WndProc",
            typeof(HwndSourceHook),
            typeof(HwndSourceHookAttachedProperty),
            new UIPropertyMetadata(null, HwndSource_PropertyChanged)
        );

        public static HwndSourceHook GetWndProc(DependencyObject obj)
        {
            return (HwndSourceHook)obj.GetValue(WndProcProperty);
        }

        public static void SetWndProc(DependencyObject obj, HwndSourceHook value)
        {
            obj.SetValue(WndProcProperty, value);
        }

        private static void HwndSource_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if(obj is Window == false)
            {
                return;
            }

            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper((Window)obj).Handle);
            if(e.OldValue as HwndSourceHook != null)
            {
                source.RemoveHook(e.OldValue as HwndSourceHook);
            }

            if(e.NewValue as HwndSourceHook != null)
            {
                source.AddHook(e.NewValue as HwndSourceHook);
            }
        }
    }
}
