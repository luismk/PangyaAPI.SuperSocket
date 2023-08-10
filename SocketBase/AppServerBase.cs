using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.IFF.Handle;
using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.Player.Data;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.Utilities.Log;
using System.Text;
using System.Threading;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public abstract partial class AppServerBase<TAppSession, TRequestInfo> : IAppServer<TAppSession, TRequestInfo>, IRawDataProcessor<TAppSession>
        where TRequestInfo : class, IRequestInfo
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
    {
        /// <summary>
        /// Null appSession instance
        /// </summary>
        protected readonly TAppSession NullAppSession = default(TAppSession);

        /// <summary>
        /// Gets the server's config.
        /// </summary>
        public IServerConfig Config { get; private set; }

        //Server instance name
        private string m_Name;

        /// <summary>
        /// the current state's code
        /// </summary>
        private int m_StateCode = ServerStateConst.NotInitialized;

        /// <summary>
        /// Gets the current state of the work item.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public ServerState State
        {
            get
            {
                return (ServerState)m_StateCode;
            }
        }

        /// <summary>
        /// Gets or sets the receive filter factory.
        /// </summary>
        /// <value>
        /// The receive filter factory.
        /// </value>
        public virtual IReceiveFilterFactory<TRequestInfo> ReceiveFilterFactory { get; protected set; }

        /// <summary>
        /// Gets the Receive filter factory.
        /// </summary>
        object IAppServer.ReceiveFilterFactory
        {
            get { return this.ReceiveFilterFactory; }
        }

        private static bool m_ThreadPoolConfigured = false;

        private long m_TotalHandledRequests = 0;

        /// <summary>
        /// Gets the total handled requests number.
        /// </summary>
        protected long TotalHandledRequests
        {
            get { return m_TotalHandledRequests; }
        }

        /// <summary>
        /// Gets the started time of this server instance.
        /// </summary>
        /// <value>
        /// The started time.
        /// </value>
        public DateTime StartedTime { get; private set; }

        /// <summary>
        /// Gets the default text encoding.
        /// </summary>
        /// <value>
        /// The text encoding.
        /// </value>
        public Encoding TextEncoding { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServerBase&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        public AppServerBase()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServerBase&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="receiveFilterFactory">The Receive filter factory.</param>
        public AppServerBase(IReceiveFilterFactory<TRequestInfo> receiveFilterFactory)
        {
            this.ReceiveFilterFactory = receiveFilterFactory;
        }

        private IPAddress ParseIPAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip) || "Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                return IPAddress.Any;
            else if ("IPv6Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                return IPAddress.IPv6Any;
            else
                return IPAddress.Parse(ip);
        }

        /// <summary>
        /// Setups the listeners base on server configuration
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        private bool SetupListeners(IServerConfig config)
        {
            var listeners = new List<ListenerInfo>();

            try
            {
               
                return true;
            }
            catch (Exception e)
            {
                //if (Logger.IsErrorEnabled)
                //    //Logger.Error(e);

                return false;
            }
        }

        /// <summary>
        /// Gets the name of the server instance.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }
        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>
        /// return true if start successfull, else false
        /// </returns>
        public virtual bool Start()
        {
            var origStateCode = Interlocked.CompareExchange(ref m_StateCode, ServerStateConst.Starting, ServerStateConst.NotStarted);

            if (origStateCode != ServerStateConst.NotStarted)
            {
                if (origStateCode < ServerStateConst.NotStarted)
                    throw new Exception("You cannot start a server instance which has not been setup yet.");

                //if (Logger.IsErrorEnabled)
                //    //Logger.ErrorFormat("This server instance is in the state {0}, you cannot start it now.", (ServerState)origStateCode);

                return false;
            }
            //inicia

            //if (!m_SocketServer.Start())
            //{
            //    m_StateCode = ServerStateConst.NotStarted;
            //    return false;
            //}

            StartedTime = DateTime.Now;
            m_StateCode = ServerStateConst.Running;

            try
            {
                //Will be removed in the next version
#pragma warning disable 0612, 618
                OnStartup();
#pragma warning restore 0612, 618

                OnStarted();
            }
            catch (Exception e)
            {
                //if (Logger.IsErrorEnabled)
                //{
                //    //Logger.Error("One exception wa thrown in the method 'OnStartup()'.", e);
                //}
            }
            finally
            {
                //if (Logger.IsInfoEnabled)
                //    Logger.Info(string.Format("The server instance {0} has been started!", Name));
            }

            return true;
        }

        /// <summary>
        /// Called when [startup].
        /// </summary>
        [Obsolete("Use OnStarted() instead")]
        protected virtual void OnStartup()
        {

        }

        /// <summary>
        /// Called when [started].
        /// </summary>
        protected virtual void OnStarted()
        {

        }

        /// <summary>
        /// Called when [stopped].
        /// </summary>
        protected virtual void OnStopped()
        {

        }

        /// <summary>
        /// Stops this server instance.
        /// </summary>
        public virtual void Stop()
        {
            if (Interlocked.CompareExchange(ref m_StateCode, ServerStateConst.Stopping, ServerStateConst.Running)
                    != ServerStateConst.Running)
            {
                return;
            }
            //tcp 
            //m_SocketServer.Stop();

            m_StateCode = ServerStateConst.NotStarted;

            OnStopped();

          
        }

        private Func<TAppSession, byte[], int, int, bool> m_RawDataReceivedHandler;

        /// <summary>
        /// Gets or sets the raw binary data received event handler.
        /// TAppSession: session
        /// byte[]: receive buffer
        /// int: receive buffer offset
        /// int: receive lenght
        /// bool: whether process the received data further
        /// </summary>
        event Func<TAppSession, byte[], int, int, bool> IRawDataProcessor<TAppSession>.RawDataReceived
        {
            add { m_RawDataReceivedHandler += value; }
            remove { m_RawDataReceivedHandler -= value; }
        }

        /// <summary>
        /// Called when [raw data received].
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        internal bool OnRawDataReceived(IAppSession session, byte[] buffer, int offset, int length)
        {
            var handler = m_RawDataReceivedHandler;
            if (handler == null)
                return true;

            return handler((TAppSession)session, buffer, offset, length);
        }

        private RequestHandler<TAppSession, TRequestInfo> m_RequestHandler;

        /// <summary>
        /// Occurs when a full request item received.
        /// </summary>
        public virtual event RequestHandler<TAppSession, TRequestInfo> NewRequestReceived
        {
            add { m_RequestHandler += value; }
            remove { m_RequestHandler -= value; }
        }

        /// <summary>
        /// Executes the command for the session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        internal void ExecuteCommand(IAppSession session, TRequestInfo requestInfo)
        {
            this.ExecuteCommand((TAppSession)session, requestInfo);
        }

      
        /// <summary>
        /// Executes the connection filters.
        /// </summary>
        /// <param name="remoteAddress">The remote address.</param>
        /// <returns></returns>
        //private bool ExecuteConnectionFilters(IPEndPoint remoteAddress)
        //{
        //    if (m_ConnectionFilters == null)
        //        return true;

        //    for (var i = 0; i < m_ConnectionFilters.Count; i++)
        //    {
        //        var currentFilter = m_ConnectionFilters[i];
        //        if (!currentFilter.AllowConnect(remoteAddress))
        //        {
        //            if (Logger.IsInfoEnabled)
        //                Logger.InfoFormat("A connection from {0} has been refused by filter {1}!", remoteAddress, currentFilter.Name);
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        /// <summary>
        /// Creates the app session.
        /// </summary>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        IAppSession IAppServer.CreateAppSession(ISocketSession socketSession)
        {
            //if (!ExecuteConnectionFilters(socketSession.RemoteEndPoint))
            //    return NullAppSession;

            var appSession = CreateAppSession(socketSession);

            appSession.Initialize(this, socketSession);

            return appSession;
        }

        /// <summary>
        /// create a new TAppSession instance, you can override it to create the session instance in your own way
        /// </summary>
        /// <param name="socketSession">the socket session.</param>
        /// <returns>the new created session instance</returns>
        protected virtual TAppSession CreateAppSession(ISocketSession socketSession)
        {
            return new TAppSession();
        }

        /// <summary>
        /// Registers the new created app session into the appserver's session container.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        bool IAppServer.RegisterSession(IAppSession session)
        {
            var appSession = session as TAppSession;

            if (!RegisterSession(appSession.m_oid, appSession))
                return false;

            appSession.SocketSession.Closed += OnSocketSessionClosed;

            OnNewSessionConnected(appSession);
            return true;
        }

        /// <summary>
        /// Registers the session into session container.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <param name="appSession">The app session.</param>
        /// <returns></returns>
        protected virtual bool RegisterSession(uint sessionID, TAppSession appSession)
        {
            return true;
        }


        private SessionHandler<TAppSession> m_NewSessionConnected;

        /// <summary>
        /// The action which will be executed after a new session connect
        /// </summary>
        public event SessionHandler<TAppSession> NewSessionConnected
        {
            add { m_NewSessionConnected += value; }
            remove { m_NewSessionConnected -= value; }
        }

        /// <summary>
        /// Called when [new session connected].
        /// </summary>
        /// <param name="session">The session.</param>
        protected virtual void OnNewSessionConnected(TAppSession session)
        {
            var handler = m_NewSessionConnected;
            if (handler == null)
                return;

            handler.BeginInvoke(session, OnNewSessionConnectedCallback, handler);
        }

        private void OnNewSessionConnectedCallback(IAsyncResult result)
        {
            try
            {
                var handler = (SessionHandler<TAppSession>)result.AsyncState;
                handler.EndInvoke(result);
            }
            catch (Exception e)
            {
                //Logger.Error(e);
            }
        }

        /// <summary>
        /// Called when [socket session closed].
        /// </summary>
        /// <param name="session">The socket session.</param>
        /// <param name="reason">The reason.</param>
        private void OnSocketSessionClosed(ISocketSession session, CloseReason reason)
        {
            //Even if LogBasicSessionActivity is false, we also log the unexpected closing because the close reason probably be useful
          

            var appSession = session.AppSession as TAppSession;
            appSession.Connected = false;
            OnSessionClosed(appSession, reason);
        }

        private SessionHandler<TAppSession, CloseReason> m_SessionClosed;
        /// <summary>
        /// Gets/sets the session closed event handler.
        /// </summary>
        public event SessionHandler<TAppSession, CloseReason> SessionClosed
        {
            add { m_SessionClosed += value; }
            remove { m_SessionClosed -= value; }
        }

        /// <summary>
        /// Called when [session closed].
        /// </summary>
        /// <param name="session">The appSession.</param>
        /// <param name="reason">The reason.</param>
        protected virtual void OnSessionClosed(TAppSession session, CloseReason reason)
        {
            var handler = m_SessionClosed;

            if (handler != null)
            {
                handler.BeginInvoke(session, reason, OnSessionClosedCallback, handler);
            }

            session.OnSessionClosed(reason);
        }

        private void OnSessionClosedCallback(IAsyncResult result)
        {
            try
            {
                var handler = (SessionHandler<TAppSession, CloseReason>)result.AsyncState;
                handler.EndInvoke(result);
            }
            catch (Exception e)
            {
                ////Logger.Error(e);
            }
        }

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        public abstract TAppSession GetSessionByID(string sessionID);

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        IAppSession IAppServer.GetSessionByID(int sessionID)
        {
            return this.GetSessionByID(sessionID.ToString());
        }

        /// <summary>
        /// Gets the matched sessions from sessions snapshot.
        /// </summary>
        /// <param name="critera">The prediction critera.</param>
        public virtual IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets all sessions in sessions snapshot.
        /// </summary>
        public virtual IEnumerable<TAppSession> GetAllSessions()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the total session count.
        /// </summary>
        public abstract int SessionCount { get; }

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            if (m_StateCode == ServerStateConst.Running)
                Stop();
        }

        #endregion
    }
}
