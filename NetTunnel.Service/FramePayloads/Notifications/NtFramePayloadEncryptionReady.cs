using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.FramePayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadEncryptionReady : IRmNotification
    {
    }
}
