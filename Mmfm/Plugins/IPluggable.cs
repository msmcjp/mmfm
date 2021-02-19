using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Msmc.Patterns.Messenger;

namespace Mmfm.Plugins
{
    public interface IPluggable<T>
    {
        /// <summary>
        /// A name of a plug-in
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Commands which a plug-in provides
        /// </summary>
        IEnumerable<ICommandItem> Commands { get; }

        /// <summary>
        /// Set Messenger object by host
        /// </summary>
        IMessenger Messenger { get; set; }

        /// <summary>
        /// Set a host object by a host
        /// </summary>
        T Host { set; }

        /// <summary>
        /// Settings object
        /// </summary>
        object Settings { get; set; }

        /// <summary>
        /// Reset Settings to default
        /// </summary>
        void ResetToDefault();

        /// <summary>
        /// Notify host of Settings is changed
        /// </summary>
        event EventHandler SettingsChanged;
    }
}
