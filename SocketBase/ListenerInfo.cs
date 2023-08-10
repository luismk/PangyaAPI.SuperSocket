using System.Net;

namespace PangyaAPI.SuperSocket.SocketBase
{
    internal class ListenerInfo
    {
        public IPEndPoint EndPoint { get; set; }
        public int BackLog { get; set; }
        public object Security { get; set; }
    }
}