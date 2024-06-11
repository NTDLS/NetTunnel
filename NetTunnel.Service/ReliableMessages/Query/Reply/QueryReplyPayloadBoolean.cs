using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Query.Reply
{
    [Serializable]
    [ProtoContract]
    public class QueryReplyPayloadBoolean : IRmQueryReply
    {
        [ProtoMember(1)]
        public bool Value { get; set; }

        [ProtoMember(2)]
        public string Message { get; set; } = string.Empty;

        public QueryReplyPayloadBoolean()
        {
        }

        public QueryReplyPayloadBoolean(bool value)
        {
            Value = value;
        }

        public QueryReplyPayloadBoolean(Exception exception)
        {
            Value = false;
            Message = exception.Message;
        }
    }
}
