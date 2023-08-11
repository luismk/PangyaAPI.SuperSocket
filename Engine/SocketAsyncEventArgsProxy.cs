using System.Net.Sockets;
using System;

namespace PangyaAPI.SuperSocket.Engine
{
    public class SocketAsyncEventArgsProxy
    {
        public SocketAsyncEventArgs SocketEventArgs { get; private set; }

        public int OrigOffset { get; private set; }

        public bool IsRecyclable { get; private set; }

        private SocketAsyncEventArgsProxy()
        {
        }

        public SocketAsyncEventArgsProxy(SocketAsyncEventArgs socketEventArgs)
            : this(socketEventArgs, isRecyclable: true)
        {
        }

        public SocketAsyncEventArgsProxy(SocketAsyncEventArgs socketEventArgs, bool isRecyclable)
        {
            this.SocketEventArgs = socketEventArgs;
            this.OrigOffset = socketEventArgs.Offset;
            this.SocketEventArgs.Completed += SocketEventArgs_Completed;
            this.IsRecyclable = isRecyclable;
        }

        private static void SocketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            IAsyncSocketSession socketSession;
            socketSession = e.UserToken as IAsyncSocketSession;
            if (socketSession != null)
            {
                if (e.LastOperation != SocketAsyncOperation.Receive)
                {
                    throw new ArgumentException("The last operation completed on the socket was not a receive");
                }
                socketSession.ProcessReceive(e);
            }
        }

        public void Initialize(IAsyncSocketSession socketSession)
        {
            this.SocketEventArgs.UserToken = socketSession;
        }

        public void Reset()
        {
            this.SocketEventArgs.UserToken = null;
        }
    }
}