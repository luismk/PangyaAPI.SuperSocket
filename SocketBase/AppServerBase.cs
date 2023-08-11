using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.SuperSocket.Interface;
namespace PangyaAPI.SuperSocket.SocketBase
{
    /// <summary>
    /// AppServer base class
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract partial class AppServerBase<TAppSession, TRequestInfo> : IAppServer<TAppSession, TRequestInfo>, IRawDataProcessor<TAppSession>, IRequestHandler<TRequestInfo>, ISocketServerAccessor, IDisposable
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
        /// Gets the certificate of current server.
        /// </summary>
        public X509Certificate Certificate { get; private set; }

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

        private ISocketServerFactory m_SocketServerFactory;


        private static bool m_ThreadPoolConfigured = false;

        private List<Interface.IConnectionFilter> m_ConnectionFilters;

        private long m_TotalHandledRequests = 0;

        /// <summary>
        /// Gets the total handled requests number.
        /// </summary>
        protected long TotalHandledRequests
        {
            get { return m_TotalHandledRequests; }
        }

        private ListenerInfo[] m_Listeners;

        /// <summary>
        /// Gets or sets the listeners inforamtion.
        /// </summary>
        /// <value>
        /// The listeners.
        /// </value>
        public ListenerInfo[] Listeners
        {
            get { return m_Listeners; }
        }

        /// <summary>
        /// Gets the started time of this server instance.
        /// </summary>
        /// <value>
        /// The started time.
        /// </value>
        public DateTime StartedTime { get; private set; }


        ///// <summary>
        ///// Gets or sets the log factory.
        ///// </summary>
        ///// <value>
        ///// The log factory.
        ///// </value>
        //public ILogFactory LogFactory { get; private set; }


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

        /// <summary>
        /// Setups the specified root config.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        protected virtual bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            return true;
        }

        partial void SetDefaultCulture(IRootConfig rootConfig, IServerConfig config);

        private void SetupBasic(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory)
        {
          
            if (config == null)
                throw new ArgumentNullException("config");

            if (!string.IsNullOrEmpty(config.Name))
                m_Name = config.Name;
            else
                m_Name = string.Format("{0}-{1}", this.GetType().Name, Math.Abs(this.GetHashCode()));

            Config = config;

            SetDefaultCulture(rootConfig, config);

            if (!m_ThreadPoolConfigured)
            {
                //if (!TheadPoolEx.ResetThreadPool(rootConfig.MaxWorkingThreads >= 0 ? rootConfig.MaxWorkingThreads : new Nullable<int>(),
                //        rootConfig.MaxCompletionPortThreads >= 0 ? rootConfig.MaxCompletionPortThreads : new Nullable<int>(),
                //        rootConfig.MinWorkingThreads >= 0 ? rootConfig.MinWorkingThreads : new Nullable<int>(),
                //        rootConfig.MinCompletionPortThreads >= 0 ? rootConfig.MinCompletionPortThreads : new Nullable<int>()))
                //{
                //    throw new Exception("Failed to configure thread pool!");
                //}

                m_ThreadPoolConfigured = true;
            }

            if (socketServerFactory == null)
            {
                var socketServerFactoryType = new SocketServerFactory();

                socketServerFactory = (ISocketServerFactory)socketServerFactoryType.CreateSocketServer<IRequestInfo>(this, Listeners, config);
            }

            m_SocketServerFactory = socketServerFactory;

            TextEncoding = new ASCIIEncoding();
        }

        private bool SetupMedium(IReceiveFilterFactory<TRequestInfo> receiveFilterFactory, IEnumerable<IConnectionFilter> connectionFilters)
        {
            if (receiveFilterFactory != null)
                ReceiveFilterFactory = receiveFilterFactory;

            if (connectionFilters != null && connectionFilters.Any())
            {
                if (m_ConnectionFilters == null)
                    m_ConnectionFilters = new List<IConnectionFilter>();

                m_ConnectionFilters.AddRange(connectionFilters);
            }
            return true;
        }

        private bool SetupAdvanced(IServerConfig config)
        {

            if (!SetupListeners(config))
                return false;

          
            return true;
        }

        internal abstract IReceiveFilterFactory<TRequestInfo> CreateDefaultReceiveFilterFactory();

        private bool SetupFinal()
        {
            //Check receiveFilterFactory
            if (ReceiveFilterFactory == null)
            {
                ReceiveFilterFactory = CreateDefaultReceiveFilterFactory();

                if (ReceiveFilterFactory == null)
                {
                    //if (//Logger.IsErrorEnabled)
                        //Logger.Error("receiveFilterFactory is required!");

                    return false;
                }
            }

            var plainConfig = Config as ServerConfig;

            if (plainConfig == null)
            {
                //Using plain config model instead of .NET configuration element to improve performance
                plainConfig = new ServerConfig(Config);

                if (string.IsNullOrEmpty(plainConfig.Name))
                    plainConfig.Name = Name;

                Config = plainConfig;
            }

            try
            {
                m_ServerStatus = new StatusInfoCollection();
                m_ServerStatus.Name = Name;
                m_ServerStatus.Tag = Name;
                m_ServerStatus[StatusInfoKeys.MaxConnectionNumber] = Config.MaxConnectionNumber;
                m_ServerStatus[StatusInfoKeys.Listeners] = m_Listeners;
            }
            catch (Exception e)
            {
                //if (//Logger.IsErrorEnabled)
                    //Logger.Error("Failed to create ServerSummary instance!", e);

                return false;
            }

            return SetupSocketServer();
        }

        /// <summary>
        /// Setups with the specified port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns>return setup result</returns>
        public bool Setup(int port)
        {
            return Setup("Any", port);
        }

        private void TrySetInitializedState()
        {
            if (Interlocked.CompareExchange(ref m_StateCode, ServerStateConst.Initializing, ServerStateConst.NotInitialized)
                    != ServerStateConst.NotInitialized)
            {
                throw new Exception("The server has been initialized already, you cannot initialize it again!");
            }
        }

        /// <summary>
        /// Setups with the specified config.
        /// </summary>
        /// <param name="config">The server config.</param>
        /// <param name="socketServerFactory">The socket server factory.</param>
        /// <param name="receiveFilterFactory">The receive filter factory.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <param name="connectionFilters">The connection filters.</param>
        /// <param name="commandLoaders">The command loaders.</param>
        /// <returns></returns>
        public bool Setup(IServerConfig config, ISocketServerFactory socketServerFactory = null, IReceiveFilterFactory<TRequestInfo> receiveFilterFactory = null, IEnumerable<IConnectionFilter> connectionFilters = null)
        {
            return Setup(new SocketBase.RootConfig(), config, socketServerFactory, receiveFilterFactory, connectionFilters);
        }

        /// <summary>
        /// Setups the specified root config, this method used for programming setup
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="config">The server config.</param>
        /// <param name="socketServerFactory">The socket server factory.</param>
        /// <param name="receiveFilterFactory">The Receive filter factory.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <param name="connectionFilters">The connection filters.</param>
        /// <param name="commandLoaders">The command loaders.</param>
        /// <returns></returns>
        public bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory = null, IReceiveFilterFactory<TRequestInfo> receiveFilterFactory = null, IEnumerable<IConnectionFilter> connectionFilters = null)
        {
            TrySetInitializedState();

            SetupBasic(rootConfig, config, socketServerFactory);

           

            if (!SetupMedium(receiveFilterFactory, connectionFilters))
                return false;

            if (!SetupAdvanced(config))
                return false;

            if (!Setup(rootConfig, config))
                return false;

            if (!SetupFinal())
                return false;

            m_StateCode = ServerStateConst.NotStarted;
            return true;
        }

        /// <summary>
        /// Setups with the specified ip and port.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="socketServerFactory">The socket server factory.</param>
        /// <param name="receiveFilterFactory">The Receive filter factory.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <param name="connectionFilters">The connection filters.</param>
        /// <param name="commandLoaders">The command loaders.</param>
        /// <returns>return setup result</returns>
        public bool Setup(string ip, int port, ISocketServerFactory socketServerFactory = null, IReceiveFilterFactory<TRequestInfo> receiveFilterFactory = null, IEnumerable<IConnectionFilter> connectionFilters = null)
        {
            return Setup(new ServerConfig
            {
                Ip = ip,
                Port = port
            },
                          socketServerFactory,
                          receiveFilterFactory,
                          connectionFilters);
        }

        private TProvider GetSingleProviderInstance<TProvider>(ProviderFactoryInfo[] factories, ProviderKey key)
        {
            var factory = factories.FirstOrDefault(p => p.Key.Name == key.Name);

            if (factory == null)
                return default(TProvider);

            return factory.ExportFactory.CreateExport<TProvider>();
        }

        private bool TryGetProviderInstances<TProvider>(ProviderFactoryInfo[] factories, ProviderKey key, Func<Type, object> creator, Func<TProvider, ProviderFactoryInfo, bool> initializer, out IEnumerable<TProvider> providers)
            where TProvider : class
        {
            IEnumerable<ProviderFactoryInfo> selectedFactories = factories.Where(p => p.Key.Name == key.Name);

            if (!selectedFactories.Any())
            {
                providers = null;
                return true;
            }

            providers = new List<TProvider>();

            var list = (List<TProvider>)providers;

            foreach (var f in selectedFactories)
            {
                var provider = creator == null ? f.ExportFactory.CreateExport<TProvider>() : f.ExportFactory.CreateExport<TProvider>(creator);

                if (!initializer(provider, f))
                    return false;

                list.Add(provider);
            }

            return true;
        }

        private IEnumerable<TProvider> GetProviderInstances<TProvider>(ProviderFactoryInfo[] factories, ProviderKey key)
            where TProvider : class
        {
            return GetProviderInstances<TProvider>(factories, key, null);
        }

        private IEnumerable<TProvider> GetProviderInstances<TProvider>(ProviderFactoryInfo[] factories, ProviderKey key, Func<Type, object> creator)
            where TProvider : class
        {
            IEnumerable<TProvider> providers;
            TryGetProviderInstances<TProvider>(factories, key, creator, (p, f) => true, out providers);
            return providers;
        }

        /// <summary>
        /// Setups the socket server.instance
        /// </summary>
        /// <returns></returns>
        private bool SetupSocketServer()
        {
            try
            {
                m_SocketServer = m_SocketServerFactory.CreateSocketServer<TRequestInfo>(this, m_Listeners, Config);
                return m_SocketServer != null;
            }
            catch (Exception e)
            {
                //if (//Logger.IsErrorEnabled)
                    //Logger.Error(e);

                return false;
            }
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
                if (config.Port > 0)
                {
                    listeners.Add(new ListenerInfo
                    {
                        EndPoint = new IPEndPoint(ParseIPAddress(config.Ip), config.Port),
                        BackLog = config.ListenBacklog
                    });
                }
                else
                {
                    //Port is not configured, but ip is configured
                    if (!string.IsNullOrEmpty(config.Ip))
                    {
                        //if (//Logger.IsErrorEnabled)
                            //Logger.Error("Port is required in config!");

                        return false;
                    }
                }

                //There are listener defined
                if (config.Listeners != null && config.Listeners.Any())
                {
                    //But ip and port were configured in server node
                    //We don't allow this case
                    if (listeners.Any())
                    {
                        //if (//Logger.IsErrorEnabled)
                            //Logger.Error("If you configured Ip and Port in server node, you cannot defined listener in listeners node any more!");

                        return false;
                    }

                }

                if (!listeners.Any())
                {
                    //if (//Logger.IsErrorEnabled)
                        //Logger.Error("No listener defined!");

                    return false;
                }

                m_Listeners = listeners.ToArray();

                return true;
            }
            catch (Exception e)
            {
                //if (//Logger.IsErrorEnabled)
                    //Logger.Error(e);

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

        private ISocketServer m_SocketServer;

        /// <summary>
        /// Gets the socket server.
        /// </summary>
        /// <value>
        /// The socket server.
        /// </value>
        ISocketServer ISocketServerAccessor.SocketServer
        {
            get { return m_SocketServer; }
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

                //if (//Logger.IsErrorEnabled)
                    //Logger.ErrorFormat("This server instance is in the state {0}, you cannot start it now.", (ServerState)origStateCode);

                return false;
            }

            if (!m_SocketServer.Start())
            {
                m_StateCode = ServerStateConst.NotStarted;
                return false;
            }

            StartedTime = DateTime.Now;
            m_StateCode = ServerStateConst.Running;

            m_ServerStatus[StatusInfoKeys.IsRunning] = true;
            m_ServerStatus[StatusInfoKeys.StartedTime] = StartedTime;

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
                //if (//Logger.IsErrorEnabled)
                //{
                    //Logger.Error("One exception wa thrown in the method 'OnStartup()'.", e);
                //}
            }
            finally
            {
                //if (//Logger.IsInfoEnabled)
                    //Logger.Info(string.Format("The server instance {0} has been started!", Name));
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

            m_SocketServer.Stop();

            m_StateCode = ServerStateConst.NotStarted;

            OnStopped();

            m_ServerStatus[StatusInfoKeys.IsRunning] = false;
            m_ServerStatus[StatusInfoKeys.StartedTime] = null;

            //if (//Logger.IsInfoEnabled)
                //Logger.Info(string.Format("The server instance {0} has been stopped!", Name));
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
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        void IRequestHandler<TRequestInfo>.ExecuteCommand(IAppSession session, TRequestInfo requestInfo)
        {
            this.ExecuteCommand((TAppSession)session, requestInfo);
        }

        /// <summary>
        /// Gets or sets the server's connection filter
        /// </summary>
        /// <value>
        /// The server's connection filters
        /// </value>
        public IEnumerable<IConnectionFilter> ConnectionFilters
        {
            get { return m_ConnectionFilters; }
        }

        /// <summary>
        /// Executes the connection filters.
        /// </summary>
        /// <param name="remoteAddress">The remote address.</param>
        /// <returns></returns>
        private bool ExecuteConnectionFilters(IPEndPoint remoteAddress)
        {
            if (m_ConnectionFilters == null)
                return true;

            for (var i = 0; i < m_ConnectionFilters.Count; i++)
            {
                var currentFilter = m_ConnectionFilters[i];
                if (!currentFilter.AllowConnect(remoteAddress))
                {
                    //if (//Logger.IsInfoEnabled)
                        //Logger.InfoFormat("A connection from {0} has been refused by filter {1}!", remoteAddress, currentFilter.Name);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the app session.
        /// </summary>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        IAppSession IAppServer.CreateAppSession(ISocketSession socketSession)
        {
            if (!ExecuteConnectionFilters(socketSession.RemoteEndPoint))
                return NullAppSession;

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
        /// Resets the session's security protocol.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="security">The security protocol.</param>
        public void ResetSessionSecurity(IAppSession session, SslProtocols security)
        {
            m_SocketServer.ResetSessionSecurity(session, security);
        }

        /// <summary>
        /// Called when [socket session closed].
        /// </summary>
        /// <param name="session">The socket session.</param>
        /// <param name="reason">The reason.</param>
        private void OnSocketSessionClosed(ISocketSession session, CloseReason reason)
        {
            //Even if LogBasicSessionActivity is false, we also log the unexpected closing because the close reason probably be useful
            //if (//Logger.IsInfoEnabled && (Config.LogBasicSessionActivity || (reason != CloseReason.ServerClosing && reason != CloseReason.ClientClosing && reason != CloseReason.ServerShutdown && reason != CloseReason.SocketError)))
                //Logger.Info(session, string.Format("This session was closed for {0}!", reason));

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
                //Logger.Error(e);
            }
        }

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        public abstract TAppSession GetSessionByID(uint sessionID);

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        IAppSession IAppServer.GetSessionByID(uint sessionID)
        {
            return this.GetSessionByID(sessionID);
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

       
        #region IActiveConnector

      

        #endregion ISystemEndPoint

        #region IStatusInfoSource

        private StatusInfoCollection m_ServerStatus;


        /// <summary>
        /// Updates the summary of the server.
        /// </summary>
        /// <param name="serverStatus">The server status.</param>
        protected virtual void UpdateServerStatus(StatusInfoCollection serverStatus)
        {
            DateTime now = DateTime.Now;

            serverStatus[StatusInfoKeys.IsRunning] = m_StateCode == ServerStateConst.Running;
            serverStatus[StatusInfoKeys.TotalConnections] = this.SessionCount;

            var totalHandledRequests0 = serverStatus.GetValue<long>(StatusInfoKeys.TotalHandledRequests, 0);

            var totalHandledRequests = this.TotalHandledRequests;

            serverStatus[StatusInfoKeys.RequestHandlingSpeed] = ((totalHandledRequests - totalHandledRequests0) / now.Subtract(serverStatus.CollectedTime).TotalSeconds);
            serverStatus[StatusInfoKeys.TotalHandledRequests] = totalHandledRequests;

            if (State == ServerState.Running)
            {
                var sendingQueuePool = m_SocketServer.SendingQueuePool;
                serverStatus[StatusInfoKeys.AvialableSendingQueueItems] = sendingQueuePool.AvialableItemsCount;
                serverStatus[StatusInfoKeys.TotalSendingQueueItems] = sendingQueuePool.TotalItemsCount;
            }
            else
            {
                serverStatus[StatusInfoKeys.AvialableSendingQueueItems] = 0;
                serverStatus[StatusInfoKeys.TotalSendingQueueItems] = 0;
            }

            serverStatus.CollectedTime = now;
        }

        /// <summary>
        /// Called when [server status collected].
        /// </summary>
        /// <param name="bootstrapStatus">The bootstrapStatus status.</param>
        /// <param name="serverStatus">The server status.</param>
        protected virtual void OnServerStatusCollected(StatusInfoCollection bootstrapStatus, StatusInfoCollection serverStatus)
        {

        }

        #endregion IStatusInfoSource

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            if (m_StateCode == ServerStateConst.Running)
                Stop();
        }

        public void TransferSystemMessage(string messageType, object messageData)
        {
            throw new NotImplementedException();
        }

        public StatusInfoAttribute[] GetServerStatusMetadata()
        {
            throw new NotImplementedException();
        }

        public StatusInfoCollection CollectServerStatus(StatusInfoCollection bootstrapStatus)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
