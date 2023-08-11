using System.Net.Sockets;

namespace PangyaAPI.SuperSocket.Engine
{
    internal interface IAsyncSocketSessionBase
    {
        SocketAsyncEventArgsProxy SocketAsyncProxy { get; }

        Socket Client { get; }
    }
}