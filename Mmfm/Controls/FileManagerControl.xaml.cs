using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mmfm
{
    /// <summary>
    /// FileManagerControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FileManagerControl : UserControl
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
            "IsActive", typeof(Boolean),
            typeof(FileManagerControl),
            new FrameworkPropertyMetadata(false, null)
        );

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public FileManagerControl()
        {
            InitializeComponent();
            this.IsKeyboardFocusWithinChanged += FileManagerControl_IsKeyboardFocusWithinChanged;
        }

        private void FileManagerControl_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            IsActive = (bool)e.NewValue;
        }
    }
}
