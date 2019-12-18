using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public class FABMessageManager : IReadOnlyCollection<FABMessage>
    {
        private ConcurrentDictionary<OpCode, FABMessage> _messages;
        public int Count { get { return _messages.Count; } }
        public FABMessage this[OpCode op] { get { FABMessage message = null; if (!_messages.TryGetValue(op, out message)) { message = _messages.AddOrUpdate(op, (o) => new FABMessage(o), null); } return message; } }

        public static event EventHandler<FABMessageEventArgs> MessageCreated;
        public static event EventHandler<FABMessageEventArgs> MessageRemoved;
        private static void OnMessageCreated(FABMessageManager messageManager, FABMessage message)
        {
            MessageCreated.Invoke(messageManager, new FABMessageEventArgs(message));
        }
        private static void OnMessageRemoved(FABMessageManager messageManager, FABMessage message)
        {
            MessageRemoved.Invoke(messageManager, new FABMessageEventArgs(message));
        }
        public FABMessageManager()
        {
            _messages = new ConcurrentDictionary<OpCode, FABMessage>();
        }
        public FABMessage GetOrCreate(OpCode op)
        {
            FABMessage message = null;
            if (!_messages.TryGetValue(op, out message))
            {
                if(_messages.TryAdd(op, new FABMessage(op)))
                {
                    if(_messages.TryGetValue(op, out message))
                        OnMessageCreated(this, message);
                }
            }
            return message;
        }
        internal bool Remove(FABMessage message)
        {
            FABMessage messageold;
            if (_messages.TryRemove(message.OpCode, out messageold))
            {
                OnMessageRemoved(this, messageold);
                return true;
            }
            return false;
        }
        public IEnumerator<FABMessage> GetEnumerator()
        {
            return _messages.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
