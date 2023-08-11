using PangyaAPI.SuperSocket.Interface;

namespace PangyaAPI.SuperSocket.Engine
{
    internal class SocketSessionClosedEventArgs
    {
        public string IdentityKey { get; set; }
        public CloseReason Reason { get; set; }
    }
}