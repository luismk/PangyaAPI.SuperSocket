using System.Net.Sockets;

namespace PangyaAPI.SuperSocket.Engine
{
    interface IAsyncSocketEventComplete
    {
        void HandleSocketEventComplete(object sender, SocketAsyncEventArgs e);
    }
}