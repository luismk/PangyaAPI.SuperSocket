﻿using PangyaAPI.SuperSocket.Ext;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.SuperSocket.SocketBase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading.Tasks;
using _smp = PangyaAPI.Utilities.Log;
namespace PangyaAPI.SuperSocket.Engine
{
    class AsyncSocketServer : TcpSocketServerBase, IActiveConnector
    {
        public AsyncSocketServer(IAppServer appServer, ListenerInfo[] listeners)
            : base(appServer, listeners)
        {

        }

        private BufferManager m_BufferManager;

        private ConcurrentStack<SocketAsyncEventArgsProxy> m_ReadWritePool;

        public override bool Start()
        {
            try
            {
                int bufferSize = AppServer.Config.ReceiveBufferSize;

                if (bufferSize <= 0)
                    bufferSize = 1024 * 4;

                m_BufferManager = new BufferManager(bufferSize * AppServer.Config.MaxConnectionNumber, bufferSize);

                try
                {
                    m_BufferManager.InitBuffer();
                }
                catch (Exception e)
                {
                    _smp.Message_Pool.push("Failed to allocate buffer for async socket communication, may because there is no enough memory, please decrease maxConnectionNumber in configuration!", e);
                    return false;
                }

                // preallocate pool of SocketAsyncEventArgs objects
                SocketAsyncEventArgs socketEventArg;

                var socketArgsProxyList = new List<SocketAsyncEventArgsProxy>(AppServer.Config.MaxConnectionNumber);

                for (int i = 0; i < AppServer.Config.MaxConnectionNumber; i++)
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    socketEventArg = new SocketAsyncEventArgs();
                    m_BufferManager.SetBuffer(socketEventArg);

                    socketArgsProxyList.Add(new SocketAsyncEventArgsProxy(socketEventArg));
                }

                m_ReadWritePool = new ConcurrentStack<SocketAsyncEventArgsProxy>(socketArgsProxyList);

                if (!base.Start())
                    return false;
                IsRunning = true;
                return true;
            }
            catch (Exception e)
            {
                _smp.Message_Pool.push("[AsyncSocketServer::Start][LogException]: " + e.Message);
                throw e;
            }
        }

        protected override void OnNewClientAccepted(ISocketListener listener, Socket client, object state)
        {
            if (IsStopped)
                return;

            ProcessNewClient(client);
        }

        private IAppSession ProcessNewClient(Socket client)
        {
            //Get the socket for the accepted client connection and put it into the 
            //ReadEventArg object user token
            SocketAsyncEventArgsProxy socketEventArgsProxy;
            if (!m_ReadWritePool.TryPop(out socketEventArgsProxy))
            {
                client.SafeClose();
                
                _smp.Message_Pool.push(string.Format("Max connection number {0} was reached!", AppServer.Config.MaxConnectionNumber));

                return null;
            }

            ISocketSession socketSession;

            socketSession = new AsyncSocketSession(client, socketEventArgsProxy);

            var session = CreateSession(client, socketSession);

            if (session == null)
            {
                socketEventArgsProxy.Reset();
                this.m_ReadWritePool.Push(socketEventArgsProxy);
                client.SafeClose();
                return null;
            }

            socketSession.Closed += SessionClosed;

            INegotiateSocketSession negotiateSession = socketSession as INegotiateSocketSession;

            if (negotiateSession == null)
            {
                if (RegisterSession(session))
                {
                    socketSession.Start();
                }

                return session;
            }

            negotiateSession.NegotiateCompleted += OnSocketSessionNegotiateCompleted;
            negotiateSession.Negotiate();
            return null;
        }

        private void OnSocketSessionNegotiateCompleted(object sender, EventArgs e)
        {
            var socketSession = sender as ISocketSession;
            var negotiateSession = socketSession as INegotiateSocketSession;

            if (!negotiateSession.Result)
            {
                socketSession.Close(CloseReason.SocketError);
                return;
            }

            if (RegisterSession(negotiateSession.AppSession))
            {
                socketSession.Start();
            }
        }

        private bool RegisterSession(IAppSession appSession)
        {
            if (AppServer.RegisterSession(appSession))
                return true;

            appSession.SocketSession.Close(CloseReason.InternalError);
            return false;
        }

        public override void ResetSessionSecurity(IAppSession session, SslProtocols security)
        {
            ISocketSession socketSession;

            var socketAsyncProxy = ((IAsyncSocketSessionBase)session.SocketSession).SocketAsyncProxy;

            socketSession = new AsyncSocketSession(session.SocketSession.m_Socket, socketAsyncProxy, true);

            socketSession.Initialize(session);
            socketSession.Start();
        }

        void SessionClosed(ISocketSession session, CloseReason reason)
        {
            var socketSession = session as IAsyncSocketSessionBase;
            if (socketSession == null)
                return;

            var proxy = socketSession.SocketAsyncProxy;
            proxy.Reset();
            var args = proxy.SocketEventArgs;

            var serverState = AppServer.State;
            var pool = this.m_ReadWritePool;

            if (pool == null || serverState == ServerState.Stopping || serverState == ServerState.NotStarted)
            {
                if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                    args.Dispose();
                return;
            }

            if (proxy.OrigOffset != args.Offset)
            {
                args.SetBuffer(proxy.OrigOffset, AppServer.Config.ReceiveBufferSize);
            }

            if (!proxy.IsRecyclable)
            {
                //cannot be recycled, so release the resource and don't return it to the pool
                args.Dispose();
                return;
            }

            pool.Push(proxy);
        }

        public override void Stop()
        {
            if (IsStopped)
                return;

            lock (SyncRoot)
            {
                if (IsStopped)
                    return;

                base.Stop();

                foreach (var item in m_ReadWritePool)
                    item.SocketEventArgs.Dispose();

                m_ReadWritePool = null;
                m_BufferManager = null;
                IsRunning = false;
            }
        }

        class ActiveConnectState
        {
            public TaskCompletionSource<ActiveConnectResult> TaskSource { get; private set; }

            public Socket Socket { get; private set; }

            public ActiveConnectState(TaskCompletionSource<ActiveConnectResult> taskSource, Socket socket)
            {
                TaskSource = taskSource;
                Socket = socket;
            }
        }

        Task<ActiveConnectResult> IActiveConnector.ActiveConnect(EndPoint targetEndPoint)
        {
            return ((IActiveConnector)this).ActiveConnect(targetEndPoint, null);
        }

        Task<ActiveConnectResult> IActiveConnector.ActiveConnect(EndPoint targetEndPoint, EndPoint localEndPoint)
        {
            var taskSource = new TaskCompletionSource<ActiveConnectResult>();
            var socket = new Socket(targetEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (localEndPoint != null)
            {
                socket.ExclusiveAddressUse = false;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.Bind(localEndPoint);
            }

            socket.BeginConnect(targetEndPoint, OnActiveConnectCallback, new ActiveConnectState(taskSource, socket));
            return taskSource.Task;
        }

        private void OnActiveConnectCallback(IAsyncResult result)
        {
            var connectState = result.AsyncState as ActiveConnectState;

            try
            {
                var socket = connectState.Socket;
                socket.EndConnect(result);

                var session = ProcessNewClient(socket);

                if (session == null)
                    connectState.TaskSource.SetException(new Exception("Failed to create session for this socket."));
                else
                    connectState.TaskSource.SetResult(new ActiveConnectResult { Result = true, Session = session });
            }
            catch (Exception e)
            {
                connectState.TaskSource.SetException(e);
            }
        }
    }
}
