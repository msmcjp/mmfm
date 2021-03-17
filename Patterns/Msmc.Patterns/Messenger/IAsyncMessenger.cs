using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Msmc.Patterns.Messenger
{
    public interface IAsyncMessenger
    {
        /// <summary>
        /// Register asyncronous message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message</typeparam>
        /// <param name="recipient">The object receives a message</param>
        /// <param name="action">The asyncronous action</param>
        void RegisterAsyncMessage<TMessage>(object recipient, Func<TMessage, Task> action);

        /// <summary>
        /// Register asyncronous message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message</typeparam>
        /// <param name="recipient">The object receives a message</param>
        /// <param name="receiveDerivedMessagesToo">TMessageを継承する型もメッセージを受信する場合はtrue</param>
        /// <param name="action">The asyncronous action</param>
        void RegisterAsyncMessage<TMessage>(object recipient, bool receiveDerivedMessagesToo, Func<TMessage, Task> action);

        /// <summary>
        /// Unregister asynchronous message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message</typeparam>
        /// <param name="recipient">The object to unregister</param>
        void Unregister<TMessage>(object recipient);

        /// <summary>
        /// Send message asyncronously.
        /// </summary>
        /// <typeparam name="TMessage">Type of the message</typeparam>
        /// <param name="message">the message object</param>
        /// <returns>The task to send message asyncronously.</returns>
        Task SendAsync<TMessage>(TMessage message);
    }
}
