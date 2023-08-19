using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using PangyaAPI.Player.Data;
using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.SuperSocket.Ext;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.Utilities;
using _smp = PangyaAPI.Utilities.Log;
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
        public IniHandle Ini { get; set; }
        public ServerInfoEx m_si { get; set; }
        public List<TableMac> ListBlockMac { get; set; }
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
        /// <summary>
        /// Gets the Receive filter factory.
        /// </summary>
        object IAppServer.SendingQueuePool
        {
            get { return this.m_SocketServer.SendingQueuePool; }
        }
        private static bool m_ThreadPoolConfigured = false;

        private List<IConnectionFilter> m_ConnectionFilters;

        private long m_TotalHandledRequests = 0;

        /// <summary>
        /// Gets the total handled requests number.
        /// </summary>
        protected long TotalHandledRequests
        {
            get { return m_TotalHandledRequests; }
        }

        private ListenerInfo[] m_Listeners;

        public uint NextConnectionID { get; set; } = 0;
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

        public bool IsRunning { get => m_StateCode == ServerStateConst.Running; }

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
            try
            {
                //Inicia Servidor
                StartedTime = DateTime.Now;
                ListBlockMac = new List<TableMac>();
                m_si = new ServerInfoEx();
            }
            catch (Exception erro)
            {
                Console.WriteLine(DateTime.Now.ToString() + $" Erro ao iniciar o servidor: {erro.Message}");
                Console.ReadKey();
                Environment.Exit(0);
            }

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServerBase&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="receiveFilterFactory">The Receive filter factory.</param>
        public AppServerBase(IReceiveFilterFactory<TRequestInfo> receiveFilterFactory)
        {
            this.ReceiveFilterFactory = receiveFilterFactory;
            this.m_ConnectionFilters = new List<IConnectionFilter>(); 
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

        private void SetupBasic(IRootConfig rootConfig, IServerConfig config)
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
                if (!TheadPoolEx.ResetThreadPool(rootConfig.MaxWorkingThreads >= 0 ? rootConfig.MaxWorkingThreads : new int?(),
                        rootConfig.MaxCompletionPortThreads >= 0 ? rootConfig.MaxCompletionPortThreads : new int?(),
                        rootConfig.MinWorkingThreads >= 0 ? rootConfig.MinWorkingThreads : new int?(),
                        rootConfig.MinCompletionPortThreads >= 0 ? rootConfig.MinCompletionPortThreads : new int?()))
                {
                    throw new Exception("Failed to configure thread pool!");
                }

                m_ThreadPoolConfigured = true;
            }

            m_Listeners = new ListenerInfo[]
            {
                new ListenerInfo( m_si.MaxUser,new IPEndPoint(ParseIPAddress(m_si.IP), m_si.Port))
            };
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
                ReceiveFilterFactory = CreateDefaultReceiveFilterFactory();//aqui pode ser um problema @! [RESOLVIDO]

                if (ReceiveFilterFactory == null)
                {
                    _smp.Message_Pool.push("receiveFilterFactory is required!");
                    return false;
                }
            }

            ServerConfig plainConfig = Config as ServerConfig;

            if (plainConfig == null)
            {
                plainConfig = new ServerConfig(Config);

                if (string.IsNullOrEmpty(plainConfig.Name))
                    plainConfig.Name = Name;

                Config = plainConfig;
            }


            return SetupSocketServer();
        }

        /// <summary>
        /// Setups with the specified port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns>return setup result</returns>
        public bool Setup(int port, string _name)
        {
            return Setup("Any", port, _name);
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
        public bool Setup(IServerConfig config, IReceiveFilterFactory<TRequestInfo> receiveFilterFactory = null, IEnumerable<IConnectionFilter> connectionFilters = null)
        {
            return Setup(new RootConfig(), config, receiveFilterFactory, connectionFilters);
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
        public bool Setup(IRootConfig rootConfig, IServerConfig config, IReceiveFilterFactory<TRequestInfo> receiveFilterFactory = null, IEnumerable<IConnectionFilter> connectionFilters = null)
        {
            TrySetInitializedState();

            SetupBasic(rootConfig, config);

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
        public bool Setup(string ip, int port, string _name, IReceiveFilterFactory<TRequestInfo> receiveFilterFactory = null, IEnumerable<IConnectionFilter> connectionFilters = null)
        {
            m_Name = _name;
            if (connectionFilters == null)
            {
                var connectionFilter = new ConnectionFilter();
                connectionFilter.Initialize(Name, this);
                connectionFilters = new List<ConnectionFilter>() { connectionFilter };
            }
            return Setup(new ServerConfig { Ip = ip, Port = port, Name = _name }, receiveFilterFactory, connectionFilters);
        }

        /// <summary>
        /// Setups the socket server.instance
        /// </summary>
        /// <returns></returns>
        private bool SetupSocketServer()
        {
            try
            {
                m_SocketServer = new SocketServerFactory().CreateSocketServer<TRequestInfo>(this, m_Listeners, Config);
                return m_SocketServer != null;
            }
            catch (Exception e)
            {
                _smp.Message_Pool.push(e);

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

                        _smp.Message_Pool.push("Port is required in config!");

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

                        _smp.Message_Pool.push("If you configured Ip and Port in server node, you cannot defined listener in listeners node any more!");

                        return false;
                    }

                }

                if (!listeners.Any())
                {

                    _smp.Message_Pool.push("No listener defined!");

                    return false;
                }

                m_Listeners = listeners.ToArray();

                return true;
            }
            catch (Exception e)
            {

                _smp.Message_Pool.push(e);

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

                _smp.Message_Pool.push(string.Format("This server instance is in the state {0}, you cannot start it now.", (ServerState)origStateCode));

                return false;
            }

            if (!m_SocketServer.Start())
            {
                m_StateCode = ServerStateConst.NotStarted;
                return false;
            }

            StartedTime = DateTime.Now;
            m_StateCode = ServerStateConst.Running;
            try
            {
                OnStartup();
                OnStarted();
            }
            catch (Exception e)
            {
                _smp.Message_Pool.push("[AppServer::Start()][LogException]: One exception wa thrown in the method 'OnStartup()'.", e);
            }
            finally
            {

                _smp.Message_Pool.push(string.Format("[AppServer::Start()][Log]: The server instance {0} has been started!", Name));
            }

            return true;
        }

        /// <summary>
        /// Called when [startup].
        /// </summary>
        protected virtual void OnStartup()
        {
            _smp.Message_Pool.push("[AppServer.OnStartup][Log]: Server starting...", _smp.type_msg.CL_ONLY_CONSOLE);
        }

        /// <summary>
        /// Called when [started].
        /// </summary>
        protected virtual void OnStarted()
        {
            _smp.Message_Pool.push("[AppServer.OnStarted][Log]: Start Server in Port: " + m_si.Port, _smp.type_msg.CL_ONLY_CONSOLE);
        }

        /// <summary>
        /// Called when [stopped].
        /// </summary>
        protected virtual void OnStopped()
        {
            _smp.Message_Pool.push("[AppServer.OnStopped][Log]: Server Stopped Mode: " + m_si.Port, _smp.type_msg.CL_ONLY_CONSOLE);
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


            _smp.Message_Pool.push(string.Format("The server instance {0} has been stopped!", Name));
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
            this.ExecuteRequest((TAppSession)session, requestInfo);
        }



        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        protected virtual void ExecuteRequest(TAppSession session, TRequestInfo requestInfo)
        {
            if (m_RequestHandler != null)
            {
                try
                {
                    var newRequest = (requestInfo as PangyaRequestInfo);
                    //var packet = new Packet(newRequest.Message);
                    //packet.unMake(session.m_key);
                    //newRequest.Message = packet.Message;
                    //chama novos pacotes/call new packets
                    m_RequestHandler(session, newRequest as TRequestInfo);
                }
                catch (Exception e)
                {
                    session.InternalHandleExcetion(e);
                }

                session.LastActiveTime = DateTime.Now.AddSeconds(10);
                Interlocked.Increment(ref m_TotalHandledRequests);
            }
            else
            {
                session.LastActiveTime = DateTime.Now.AddDays(-1);
                session.InternalHandleUnknownRequest(requestInfo);
            }
        }


        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        void IRequestHandler<TRequestInfo>.HandleRequest(IAppSession session, TRequestInfo requestInfo)
        {
            this.ExecuteRequest((TAppSession)session, requestInfo);
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
                var currentFilter = (ConnectionFilter)m_ConnectionFilters[i];
                if (!currentFilter.AllowConnect(remoteAddress))
                {
                    _smp.Message_Pool.push(string.Format("A connection from {0} has been refused by filter {1}!", remoteAddress, currentFilter.Name));
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
            {
                socketSession.Close(reason: CloseReason.SocketError);
                return NullAppSession;
            
            }
            var appSession = CreateAppSession(socketSession);
            appSession.Initialize(this, socketSession);
            NextConnectionID++;
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
                _smp.Message_Pool.push(e);
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
            #region _RELEASE
            _smp.Message_Pool.push(string.Format("[AppServer::OnSocketSessionClosed][Log]: This session was closed for {0}!", reason));
            #endregion
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
                _smp.Message_Pool.push(e);
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

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            if (m_StateCode == ServerStateConst.Running)
                Stop();
        }

        public abstract void onHeartBeat();

        public bool haveBanList(string _ip_address, string _mac_address, bool _check_mac = true)
        {
            if (_check_mac)
            {
                // Verifica primeiro se o MAC Address foi bloqueado

                // Cliente não enviou um MAC Address válido, bloquea essa conexão que é hacker que mudou o ProjectG
                if (string.IsNullOrEmpty(_mac_address))
                    return true;    // Cliente não enviou um MAC Address válido, bloquea essa conexão que é hacker que mudou o ProjectG

                foreach (var item in ListBlockMac)
                {
                    if (string.IsNullOrEmpty(item.Mac_Adress) == false && item.Mac_Adress == _mac_address)
                    {

                    }
                    return true;	// MAC Address foi bloqueado
                }
            }
            // IP Address inválido, bloquea essa conexão que é Hacker ou Bug

            if (string.IsNullOrEmpty(_ip_address))
                return true;

            return false;

        }
        private List<string> v_mac_ban_list;
        private List<string> v_ip_ban_list;
        public virtual void AppMonitor()
        {
            while (IsRunning)
            {
                if (_smp.Message_Pool.checkUpdateDayLog())
                    _smp.Message_Pool.push("[AppServer::monitor::UpdateLogFiles][Log] Atualizou os arquivos de Log por que trocou de dia.");
                try
                {
                    //cmdUpdateServerList();  // Pega a Lista de servidores online

                    //cmdUpdateListBlock_IP_MAC();    // Pega a List de IP e MAC Address que estão bloqueadas

                    onHeartBeat();
                }
                catch { }
            }
        }

        public abstract IAppSession GetSessionByUserName(string User);
        public abstract IAppSession GetSessionByNick(string Nick);


        #endregion
    }
}
