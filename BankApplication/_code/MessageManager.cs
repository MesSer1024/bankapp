using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp
{
    public interface IMessage
    {
    }

    public interface IMessageListener
    {
        void onMessage(IMessage message);
    }

    static class MessageManager
    {
        private static List<IMessageListener> _listeners = new List<IMessageListener>();

        public static void addListener(IMessageListener listener)
        {
            _listeners.Add(listener);
        }

        public static void removeListener(IMessageListener listener)
        {
            _listeners.Remove(listener);
        }

        public static void queueMessage(IMessage msg)
        {
            executeMessage(msg);
        }

        private static void executeMessage(IMessage msg)
        {
            var listeners = _listeners.ToArray();
            WpfUtils.toMainThread(() => {
                foreach (var l in listeners)
                {
                    l.onMessage(msg);
                }
            });

        }
    }
}
