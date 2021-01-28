using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Msmc.Patterns.Messenger
{
    public class Messenger : IMessenger
    {
        static Messenger _defaultInstance = null;

        /// <summary>
        /// デフォルトインスタンス
        /// </summary>
        static public Messenger Default
        {
            get
            {
                if(_defaultInstance == null)
                {
                    _defaultInstance = new Messenger();
                }

                return _defaultInstance;
            }
        }

        /// <summary>
        /// デフォルトインスタンスを再生成する
        /// </summary>
        static public void ResetDefault()
        {
            _defaultInstance = null;
        }

        private Dictionary<Type, List<(object Recipient, object Action)>> actions = new Dictionary<Type, List<(object, object)>>();
        private Dictionary<Type, List<(object Recipient, object Action)>> actionsSubclass = new Dictionary<Type, List<(object, object)>>();

        /// <summary>
        /// 
        /// </summary>
        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            Register(recipient, false, action);
        }

        public void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            var messageType = typeof(TMessage);

            Dictionary<Type, List<(object Recipient, object Action)>> dict = receiveDerivedMessagesToo ? actionsSubclass : actions ;
          
            if (dict.ContainsKey(messageType) == false)
            {
                dict[messageType] = new List<(object, object)>();
            }
            dict[messageType].Add((recipient, action));
        }

        public void Send<TMessage>(TMessage message)
        {
            var messageType = typeof(TMessage);

            foreach (var type in actionsSubclass.Keys)
            {
                if (type.IsAssignableFrom(typeof(TMessage)))
                {
                    foreach (var action in actionsSubclass[type])
                    {
                        (action.Action as Action<TMessage>)?.Invoke(message);
                    }
                }
            }

            if(actions.ContainsKey(messageType) == false)
            {
                return;
            }

            foreach(var action in actions[messageType])
            {
                (action.Action as Action<TMessage>)?.Invoke(message);
            }
        }

        public void Unregister<TMessage>(object recipient)
        {
            var messageType = typeof(TMessage);

            if (actions.ContainsKey(messageType))
            {
                actions[messageType] = actions[messageType].Where(action => action.Recipient != recipient).ToList();
            }
        }
    }
}
