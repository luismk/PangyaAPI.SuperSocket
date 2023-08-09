using PangyaAPI.IFF.Handle;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.SuperSocket.StructData;
using PangyaAPI.Utilities;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public abstract class AppServer<TAppSession, TPacket> : AppServerBase<TAppSession, TPacket>
        where TPacket : class, IPacket
        where TAppSession : AppSession<TAppSession, TPacket>, IAppSession, new()
    {

        public AppServer()
            : base()
        {
            IFF = new IFFHandle("data//pangya_jp.iff");
            Ini = new IniHandle("Config//Server.ini");
            m_si = new ServerInfoEx();
        }
        public AppServer(string ServerName)
           : base()
        {

        }

        /// <summary>
        /// verificar se tudo esta okay antes de iniciar o server!
        /// </summary>
        /// <returns></returns>
        public bool LoadingFiles()
        {
            return true;
        }
        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        [Obsolete("Use the method GetSessionByID instead")]
        public TAppSession GeTAppSessionByID(int sessionID)
        {
            return GetSessionByID(sessionID);
        }

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        public override TAppSession GetSessionByID(int sessionID)
        {
            return (TAppSession)Players.Model.First(c => c.ConnectionId == sessionID);
        }

        /// <summary>
        /// Called when [socket session closed].
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="reason">The reason.</param>
        protected override void OnSessionClosed(TAppSession session)
        {
            if (NullAppSession != session)
            {
                if (!Players.Remove(session))
                {
                    //if (Logger.IsErrorEnabled)
                    //    Logger.Error(session, "Failed to remove this session, Because it has't been in session container!");
                }
            }

            base.OnSessionClosed(session);
        }

        public override bool RegisterSession(string sessionID, TAppSession AppClient)
        {
            AppClient.ConnectionId = uint.Parse(sessionID);
            if (Players.Add(AppClient))
                return true;

              //  Logger.Error("The session is refused because the it's ID already exists!");

            return false;
        }

        public override bool Start()
        {
            if (!base.Start())
                return false;



            //Inicia Thread para escuta de clientes
            var WaitConnectionsThread = new Thread(new ThreadStart(WaitConnections));
            WaitConnectionsThread.Start();
            //Logger.Write("Server started.");
            return true;
        }

        private void WaitConnections()
        {
#if DEBUG
            Console.WriteLine($"WAITING_CONNECTIONS", ConsoleColor.Green);

#endif

            while (_isRunning)
            {
                // Inicia Escuta de novas conexões (Quando player se conecta).
                TcpClient newClient = _server.AcceptTcpClient();

                // Cliente conectado
                // Cria uma Thread para manusear a comunicação (uma thread por cliente)
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(newClient);
            }
        }
        public virtual void Command(string[] Command) { }

        /// <summary>
        /// Gets the total session count.
        /// </summary>
        public override int SessionCount
        {
            get
            {
                return Players.Count;
            }
        }

    }
}
