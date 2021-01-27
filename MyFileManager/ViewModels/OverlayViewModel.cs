using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFileManager
{
    public class OverlayViewModel
    {
        public object Content
        {
            get;
            private set;
        }

        public OverlayViewModel(object content)
        {
            Content = content;
        }
    }
}
