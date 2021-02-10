using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Mmfm
{
    /// <summary>
    /// https://stackoverflow.com/questions/9078886/gridview-column-width-does-not-update-when-changing-itemssource
    /// Represents a view mode that displays data items in columns for a System.Windows.Controls.ListView control with auto sized columns based on the column content     
    /// </summary>
    public class AutoSizedGridView : GridView
    {
        protected override void PrepareItem(ListViewItem item)
        {
            foreach (GridViewColumn column in Columns)
            {
                //setting NaN for the column width automatically determines the required width enough to hold the content completely.
                //if column width was set to NaN already, set it ActualWidth temporarily and set to NaN. This raises the property change event and re computes the width.
                if (double.IsNaN(column.Width)) column.Width = column.ActualWidth;
                column.Width = double.NaN;
            }
            base.PrepareItem(item);
        }
    }
}
