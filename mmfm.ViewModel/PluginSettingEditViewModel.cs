using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm.ViewModel
{
    public class PluginSettingEditViewModel
    {
        public string Name
        {
            get;
            private set;
        }

        public object Content
        {
            get;
            set;
        }

        public PluginSettingEditViewModel(KeyValuePair<string, object> p)
        {
            Name = p.Key;
            Content = p.Value;
        }
    }
}
