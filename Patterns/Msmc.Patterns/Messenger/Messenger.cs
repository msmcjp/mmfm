using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Msmc.Patterns.Messenger
{
    public class Messenger : IMessenger, IAsyncMessenger
    {
        static Messenger _defaultInstance = null;

        /// <summary>
        /// The default instance of Messenger.
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
        /// Reset default instance.
        /// </summary>
        static public void ResetDefault()
        {
            _defaultInstance = null;
        }

        private Dictionary<Type, List<(object Recipient, object Action)>> actions = new Dictionary<Type, List<(object, object)>>();
        private Dictionary<Type, List<(object Recipient, object Action)>> actionsSubclass = new Dictionary<Type, List<(object, object)>>();

        private void RegisterAny<TMessage>(object recipient, bool receiveDerivedMessagesToo, object action)
        {
            var messageType = typeof(TMessage);

            Dictionary<Type, List<(object Recipient, object Action)>> dict = receiveDerivedMessagesToo ? actionsSubclass : actions;

            if (dict.ContainsKey(messageType) == false)
            {
                dict[messageType] = new List<(object, object)>();
            }
            dict[messageType].Add((recipient, action));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            Register(recipient, false, action);
        }

        public void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            RegisterAny<TMessage>(recipient, receiveDerivedMessagesToo, action);
        }

        private IEnumerable<TAction> TargetActions<TAction, TMessage>()
        {
            var messageType = typeof(TMessage);
            var _actions = new List<TAction>();

            foreach (var type in actionsSubclass.Keys)
            {
                if (type.IsAssignableFrom(typeof(TMessage)))
                {
                    actionsSubclass[type]
                        .Where(x => x.Action is TAction)
                        .Select(x => x.Action)
                        .Aggregate(_actions, (a, x) => { a.Add((TAction)x); return a; });                   
                }
            }

            if (actions.ContainsKey(messageType))
            {
                actions[messageType]
                    .Where(x => x.Action is TAction)
                    .Select(x => x.Action)
                    .Aggregate(_actions, (a, x) => { a.Add((TAction)x); return a; });                
            }

            return _actions;
        }

        public void Send<TMessage>(TMessage message)
        {
            foreach(var action in TargetActions<Action<TMessage>, TMessage>())
            {
                action(message);
            }
        }

        public void RegisterAsyncMessage<TMessage>(object recipient, Func<TMessage, Task> action)
        {
            RegisterAsyncMessage<TMessage>(recipient, false, action);
        }

        public void RegisterAsyncMessage<TMessage>(object recipient, bool receiveDerivedMessagesToo, Func<TMessage, Task> action)
        {
            RegisterAny<TMessage>(recipient, receiveDerivedMessagesToo, action);
        }

        public async Task SendAsync<TMessage>(TMessage message)
        {
            await Task.WhenAll(
                TargetActions<Func<TMessage, Task>, TMessage>().Select(func => func(message)));
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
