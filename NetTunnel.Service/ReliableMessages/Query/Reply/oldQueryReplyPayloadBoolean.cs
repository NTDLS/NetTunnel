using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Query.Reply
{
    [Serializable]
    [ProtoContract]
    public class oldQueryReplyPayloadBoolean : IRmQueryReply
    {
        [ProtoMember(1)]
        public bool Value { get; set; }

        [ProtoMember(2)]
        public string Message { get; set; } = string.Empty;

        public oldQueryReplyPayloadBoolean()
        {
        }

        public oldQueryReplyPayloadBoolean(bool value)
        {
            Value = value;
        }

        public oldQueryReplyPayloadBoolean(Exception exception)
        {
            Value = false;
            Message = exception.Message;
        }
    }
}
