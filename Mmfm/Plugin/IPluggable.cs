using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Msmc.Patterns.Messenger;

namespace Mmfm.Plugin
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
        /// Call this method when a host plugged a plug-in
        /// </summary>
        void Plugged();

        /// <summary>
        /// Request to update InpugBindings
        /// </summary>
        event EventHandler RequestInputBindingsUpdate;
    }
}
