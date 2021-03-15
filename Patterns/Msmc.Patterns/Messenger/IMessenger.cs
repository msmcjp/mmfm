   using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Msmc.Patterns.Messenger
{
    /// <summary>
    /// メッセージ送受信インターフェース
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// メッセージの受信を登録する
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型</typeparam>
        /// <param name="recipient">メッセージを受信するオブジェクト</param>
        /// <param name="action">メッセージを受信したら実行するアクション</param>
        void Register<TMessage>(object recipient, Action<TMessage> action);

        /// <summary>
        /// メッセージの受信を登録する
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型</typeparam>
        /// <param name="recipient">メッセージを受信するオブジェクト</param>
        /// <param name="receiveDerivedMessagesToo">TMessageを継承する型もメッセージを受信する場合はtrue</param>
        /// <param name="action">メッセージを受信したら実行するアクション</param>
        void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action);

        /// <summary>
        /// メッセージの受信を解除する
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型</typeparam>
        /// <param name="recipient">メッセージを受信するオブジェクト</param>
        void Unregister<TMessage>(object recipient);

        /// <summary>
        /// メッセージを発信する
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型</typeparam>
        /// <param name="message">発信するメッセージオブジェクト</param>
        void Send<TMessage>(TMessage message);
    }
}
