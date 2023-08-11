using PangyaAPI.SuperSocket.Interface;
using System.Net.Sockets;

namespace PangyaAPI.SuperSocket.Engine
{
    class SaeState : BufferBaseState
    {
        public ISocketSession SocketSession { get; internal set; }

        public SocketAsyncEventArgs Sae { get; private set; }

        public SaeState(SocketAsyncEventArgs sae)
        {
            Sae = sae;
        }
    }
}