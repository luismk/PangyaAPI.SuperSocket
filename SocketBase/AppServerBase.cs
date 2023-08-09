using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.IFF.Handle;
using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.SuperSocket.Commom;
using PangyaAPI.SuperSocket.Interface;
namespace PangyaAPI.SuperSocket.SocketBase
{
    public abstract partial class AppServerBase<TAppSession, TPacket> : IServerBase<TAppSession, TPacket>
        where TPacket : class, IPacket
        where TAppSession : AppSession<TAppSession, TPacket>, IAppSession, new()
    {
        #region Fields
        /// <summary>
        /// Null appSession instance
        /// </summary>
        protected readonly TAppSession NullAppSession = default(TAppSession);
        public bool IsOpen { get; set; }
        public AppSessionManager Players { get; set; }
        public uint NextConnectionId { get; set; } = 1;
        protected TcpListener _server { get; set; }
        public bool _isRunning { get; set; }
        public IFFHandle IFF { get; set; }
        public bool IFFLog { get; set; }
        public IniHandle Ini { get; set; }
        public ServerInfoEx m_si { get; set; }
        public List<TableMac> ListBlockMac { get; set; }
        public DateTime StartedTime { get; }

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

        //Server instance name
        public string Name
        {
            get
            {
                return m_si.Name;
            }
        }
        #endregion

        #region Events

        public event RequestHandler<TAppSession, TPacket> NewRequestReceived;


        private SessionHandler<TAppSession> m_SessionClosed;

        /// <summary>
        /// Gets/sets the session closed event handler.
        /// </summary>
        public event SessionHandler<TAppSession> SessionClosed
        {
            add { m_SessionClosed += value; }
            remove { m_SessionClosed -= value; }
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

        #endregion

        #region Construtor
        /// <summary>
        /// construtura
        /// </summary>
        public AppServerBase()
        {
            try
            {
                //Inicia Servidor
                m_StateCode = ServerStateConst.Initializing;
                _isRunning = false;
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
        #endregion

        /// <summary>
        /// Called when [session closed].
        /// </summary>
        /// <param name="session">The AppClient.</param>
        /// <param name="reason">The reason.</param>
        protected virtual void OnSessionClosed(TAppSession session)
        {
            var handler = m_SessionClosed;

            if (handler != null)
            {
                handler.BeginInvoke(session, OnSessionClosedCallback, handler);
            }

            session.OnSessionClosed();
        }

        private void OnSessionClosedCallback(IAsyncResult result)
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
        /// Gets all sessions in sessions snapshot.
        /// </summary>
        public virtual List<TAppSession> GetAllSessions()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        IAppSession IServerBase.GetSessionByID(int sessionID)
        {
            return this.GetSessionByID(sessionID);
        }

        IAppSession IServerBase.GetSessionByNick(string Nick)
        {
            return this.GetSessionByNick(Nick);
        }

        IAppSession IServerBase.GetSessionByUserName(string User)
        {
            return this.GetSessionByUserName(User);
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
        /// Gets the total session count.
        /// </summary>
        public abstract int SessionCount { get; }

        /// <summary>
        /// Creates the app session.
        /// </summary>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        IAppSession IServerBase.CreateAppSession(Socket socketSession)
        {

            var appSession = CreateAppSession(socketSession);

            appSession.Initialize(this, socketSession);

            return appSession;
        }

        /// <summary>
        /// create a new TAppSession instance, you can override it to create the session instance in your own way
        /// </summary>
        /// <param name="socketSession">the socket session.</param>
        /// <returns>the new created session instance</returns>
        protected virtual TAppSession CreateAppSession(Socket socketSession)
        {
            //regeistra o novo player
          var appSession  = Players.AddSession(socketSession, this);

            return appSession as TAppSession;
        }


        public void HandleClient(object client)
        {
            //Recebe cliente a partir do parâmetro
            TcpClient tcpClient = (TcpClient)client;

            //Cria novo player
            var player = CreateAppSession(tcpClient.Client);

            OnNewSessionConnected(player);

            //laço de repeticao para verificar se ainda esta conectado, se for true, ele fica lendo o pacote
            while (player.m_Connected)
            {
                //lida com packet
                var packet = HandleReceived(player);
                if (packet.Message != null)
                {
                    //request 
                    NewRequestReceived?.Invoke(player, packet as TPacket);
                }
                else
                {
                    break;
                }
            }

            //desconecta caso for false
            OnSessionClosed(player);
        }

        /// <summary>
        /// Registers the new created app session into the appserver's session container.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        bool IServerBase.RegisterSession(IAppSession session)
        {
            var AppClient = session as TAppSession;

            if (!RegisterSession(AppClient.m_oid.ToString(), AppClient))
                return false;
            return true;
        }

        /// <summary>
        /// Registers the session into session container.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <param name="AppClient">The app session.</param>
        /// <returns></returns>
        public virtual bool RegisterSession(string sessionID, TAppSession AppClient)
        {  //Define no player qual servidor ele está
            AppClient.AppServer = this;
            return true;
        }

        public void OnNewSessionDisconnect(TAppSession session)
        {
            OnSessionClosed(session);
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

            if (session.m_Connected)
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
                // Logger.Error(e);
            }
        }


        public virtual bool Start()
        {
            if (m_StateCode == ServerStateConst.Initializing)
            {
                _server = new TcpListener(IPAddress.Parse(m_si.IP), m_si.Port);

                _server.Start(m_si.MaxUser);

                if (_server != null)
                {
                    //Abre servidor para os jogadores
                    IsOpen = true;
                    _isRunning = true;

                    m_StateCode = ServerStateConst.Running;
                    WriteConsole.WriteLine($"[SERVER_START]: PORT => {m_si.Port}", ConsoleColor.Green);



                    if (IFFLog)
                        IFF.Log();

                    return true;
                }
            }
            if (m_StateCode == ServerStateConst.Running && _isRunning)
            {
                return false;
            }
            else
            {
                Console.WriteLine("Failed to start!");
                Console.ReadKey();
                m_StateCode = ServerStateConst.NotStarted;
                IsOpen = false;
                _isRunning = false;
                return false;
            }
        }

        public bool Stop()
        {
            if (m_StateCode == ServerStateConst.Running && _isRunning)
            {
                if (_server != null)
                {
                    //fecha servidor para os jogadores
                    IsOpen = false;
                    _isRunning = false;

                    m_StateCode = ServerStateConst.Stopping;
                    WriteConsole.WriteLine($"[SERVER_STOP]: STATUS => {m_si.Port}:{ServerStateConst.Stopping.ToString().ToUpper()}", ConsoleColor.Green);
                    return true;
                }
            }
            Console.WriteLine("Failed to stop!");
            Console.ReadKey();
            m_StateCode = ServerStateConst.NotStarted;
            IsOpen = false;
            _isRunning = false;
            return false;
        }

        public bool Reset()
        {
            if (m_StateCode == ServerStateConst.Running && _isRunning)
            {
                if (_server != null)
                {

                    //fecha servidor para os jogadores
                    IsOpen = false;
                    _isRunning = false;

                    m_StateCode = ServerStateConst.NotStarted;
                    WriteConsole.WriteLine($"[SERVER_RESETED]: STATUS => {m_si.Port}:{50001}", ConsoleColor.Green);
                    return true;
                }
            }
            Console.WriteLine("Failed to stop!");
            Console.ReadKey();
            m_StateCode = ServerStateConst.NotStarted;
            IsOpen = false;
            _isRunning = false;
            return false;
        }

        public void Send(IAppSession session, Packet packet)
        {
            session.Send(packet);
        }

        public void Send(IAppSession session, byte[] packet)
        {
            session.Send(packet);
        }
        public void SendToAll(Packet packet)
        {
            foreach (var session in Players.Model)
            {
                session.Send(packet);
            }
        }

        public void SendToAll(byte[] packet)
        {
            foreach (var session in Players.Model)
            {
                session.Send(packet);
            }
        }
        #region Comandos no console
        public void ServerMessage(string message)
        {
            var response = new PangyaBinaryWriter();

            response.Write(new byte[] { 0x43, 0x00 });
            response.WritePStr(message);

            SendToAll(response.GetBytes());

            Console.WriteLine("Mensagem enviada com sucesso");
        }


        public void TickerMessage(string message)
        {
            var response = new PangyaBinaryWriter();

            response.Write(new byte[] { 0xC9, 0x00 });
            response.WritePStr("@Admin");
            response.WritePStr(message);
            response.WriteZero(1);

            SendToAll(response.GetBytes());

            Console.WriteLine("Ticker enviado com sucesso");
        }




        public void BroadMessage(string message)
        {
            var response = new PangyaBinaryWriter();

            response.Write(new byte[] { 0x42, 0x00 });
            response.WritePStr("Aviso: " + message);

            SendToAll(response.GetBytes());
            Console.WriteLine("BroadCast enviado com sucesso");
        }
        #endregion

        public Packet HandleReceived(IAppSession client)
        {
            try
            {
                var message = ProcessPacket(client as AppSession);

                if (message.Length >= 5)
                {
                    return new Packet(client.m_key, message, false);
                }
            }
            catch (Exception erro)
            {
                //System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(erro, true);

                //Console.WriteLine("[Exception] " + erro.Message);

                //Console.WriteLine(trace.GetFrame(0).GetMethod().ReflectedType.FullName);
                //Console.WriteLine("Method: " + erro.TargetSite);
                //Console.WriteLine("Line: " + trace.GetFrame(0).GetFileLineNumber());
                //Console.WriteLine("Column: " + trace.GetFrame(0).GetFileColumnNumber());

                // Logger.Error(erro);

            }
            //Sem Resposta
            return new Packet();
        }

        private byte[] ProcessPacket(AppSession session)
        {
            var socket = session.AppClient;
            try
            {
                if (socket != null && socket.Available > 0 && socket.Connected)
                {
                    var messageBufferRead = new byte[1024]; //Tamanho do BUFFER á ler
                                                            //Lê mensagem do cliente
                    int bytesRead = socket.Receive(messageBufferRead, messageBufferRead.Length, SocketFlags.None);

                    if (bytesRead > 0)
                    {
                        //variável para armazenar a mensagem recebida
                        byte[] message = new byte[bytesRead];

                        //Copia mensagem recebida
                        Buffer.BlockCopy(messageBufferRead, 0, message, 0, bytesRead);
                        return message;
                    }
                }
            }
            catch
            {
                return new byte[0];
            }
            return new byte[0];
        }


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


        #region Methods Abstracts
        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        public abstract TAppSession GetSessionByID(int sessionID);
        public abstract IAppSession GetSessionByNick(string Nick);

        public abstract IAppSession GetSessionByUserName(string User);

        #endregion fim
    }
}
