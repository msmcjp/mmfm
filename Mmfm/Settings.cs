using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mmfm
{
    public class Settings
    {
        public class FileManager
        {
            public bool ShowHiddenFiles
            {
                get;
                set;
            }

            public string Current
            {
                get;
                set;
            }
        }

        public string HotKey
        {
            get;
            set;
        }


        public IEnumerable<FileManager> FileManagers
        {
            get;
            set;
        }

        public ExpandoObject Plugins
        {
            get;
            set;
        }

        public Settings()
        {
            HotKey = "Ctrl+OemSemicolon";
        }
    }
}
