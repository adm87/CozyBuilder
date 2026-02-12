namespace Cozy.Builder.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IMessageReceiver<TMessage>
    {
        void OnMessageReceived(TMessage message);
    }

    public static class Messenger
    {
        private static readonly Dictionary<Type, List<object>> Receivers = new();

        public static void Subscribe<TMessage>(IMessageReceiver<TMessage> receiver)
        {
            var messageType = typeof(TMessage);
            if (!Receivers.ContainsKey(messageType))
            {
                Receivers[messageType] = new List<object>();
            }

            Receivers[messageType].Add(receiver);
        }

        public static void Unsubscribe<TMessage>(IMessageReceiver<TMessage> receiver)
        {
            var messageType = typeof(TMessage);
            if (Receivers.ContainsKey(messageType))
            {
                Receivers[messageType].Remove(receiver);
                if (Receivers[messageType].Count == 0)
                {
                    Receivers.Remove(messageType);
                }
            }
        }

        public static void Publish<TMessage>(TMessage message)
        {
            var messageType = typeof(TMessage);
            if (Receivers.ContainsKey(messageType))
            {
                foreach (var receiver in Receivers[messageType].Cast<IMessageReceiver<TMessage>>())
                {
                    receiver.OnMessageReceived(message);
                }
            }
        }
    }
}