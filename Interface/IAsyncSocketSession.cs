using PangyaAPI.SuperSocket.Interface;
using System.Net.Sockets;

namespace PangyaAPI.SuperSocket.Engine
{
    public interface IAsyncSocketSession
    {
        void ProcessReceive(SocketAsyncEventArgs e);
    }
}