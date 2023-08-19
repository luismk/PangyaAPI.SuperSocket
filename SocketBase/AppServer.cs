using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.Player.Data;
using PangyaAPI.Utilities;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using _smp = PangyaAPI.Utilities.Log;
using PangyaAPI.Utilities.BinaryModels;

namespace PangyaAPI.SuperSocket.SocketBase
{
    /// <summary>
    /// AppServer class
    /// </summary>
    public abstract partial class AppServer : AppServer<AppSession>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer"/> class.
        /// </summary>
        public AppServer()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer"/> class.
        /// </summary>
        /// <param name="receiveFilterFactory">The Receive filter factory.</param>
        public AppServer(IReceiveFilterFactory<StringRequestInfo> receiveFilterFactory)
            : base(receiveFilterFactory)
        {

        }
    }

    /// <summary>
    /// AppServer class
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    public abstract partial class AppServer<TAppSession> : AppServer<TAppSession, StringRequestInfo>
        where TAppSession : AppSession<TAppSession, StringRequestInfo>, IAppSession, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer&lt;TAppSession&gt;"/> class.
        /// </summary>
        public AppServer()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer&lt;TAppSession&gt;"/> class.
        /// </summary>
        /// <param name="receiveFilterFactory">The Receive filter factory.</param>
        public AppServer(IReceiveFilterFactory<StringRequestInfo> receiveFilterFactory)
            : base(receiveFilterFactory)
        {
        }
    }


    /// <summary>
    /// AppServer basic class
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract partial class AppServer<TAppSession, TRequestInfo> : AppServerBase<TAppSession, TRequestInfo>
        where TRequestInfo : class, IRequestInfo
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        public AppServer()
            : base()
        {
            //IFF = new IFFHandle("data//pangya_jp.iff");
            Ini = new IniHandle("server.ini");
            m_si = new ServerInfoEx();
        }

      public AppServer(string ServerName)
           : base()
        {
            //IFF = new IFFHandle("data//pangya_jp.iff");
            Ini = new IniHandle("server.ini");
            m_si = new ServerInfoEx();
        }

        #region
        /// <summary>
        /// metodo resposavel por envia o pacote de conexao com pangya " HELLO "
        /// </summary>
        /// <param name="session"></param>
        protected abstract void onAcceptCompleted(TAppSession session);
        #endregion

        public bool LoadingFiles()
        {
            return true;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AppServer&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        protected AppServer(IReceiveFilterFactory<TRequestInfo> protocol)
            : base(protocol)
        {
            //IFF = new IFFHandle("data//pangya_jp.iff");
            Ini = new IniHandle("server.ini");
            m_si = new ServerInfoEx();
        }

        internal override IReceiveFilterFactory<TRequestInfo> CreateDefaultReceiveFilterFactory()
        {
            return default;
        }

        /// <summary>
        /// Starts this AppServer instance.
        /// </summary>
        /// <returns></returns>
        public override bool Start()
        {
            if (!base.Start())
                return false;

            if (!Config.DisableSessionSnapshot)
                StartSessionSnapshotTimer();

            if (Config.ClearIdleSession)
                StartClearSessionTimer();

            //Inicia Thread para monitor
            var t2 = new Thread(new ThreadStart(AppMonitor));
            t2.Start();

           // CheckSessionLive();

            return true;
        }

        private ConcurrentDictionary<uint, TAppSession> m_SessionDict = new ConcurrentDictionary<uint, TAppSession>();

        /// <summary>
        /// Registers the session into the session container.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <param name="appSession">The app session.</param>
        /// <returns></returns>
        protected override bool RegisterSession(uint sessionID, TAppSession appSession)
        {
            if (m_SessionDict.TryAdd(sessionID, appSession))
                return true;

            _smp.Message_Pool.push("The session is refused because the it's ID already exists!");

            return false;
        }
        /// <summary>
        /// chama a configuração do servidor
        /// </summary>
        public virtual void ConfigInit()
        {
            m_si = new ServerInfoEx
            {
                Version = Ini.ReadString("SERVERINFO", "VERSION", "Pangya Server Csharp 1.0"),
                Version_Client = Ini.ReadString("SERVERINFO", "CLIENTVERSION", "JP.R7.962.00"),
                Name = Ini.ReadString("SERVERINFO", "NAME", "Pangya Server Csharp"),
                UID = Ini.ReadInt32("SERVERINFO", "GUID", 10103),
                Port = Ini.ReadInt32("SERVERINFO", "PORT", 10103),
                IP = Ini.ReadString("SERVERINFO", "IP", "127.0.0.1"),
                MaxUser = Ini.ReadInt32("SERVERINFO", "MAXUSER", 2001),
                Property = new uPropertyEx(Ini.ReadUInt32("SERVERINFO", "PROPERTY", 2048)),
                Auth_IP = Ini.ReadString("AUTHSERVER", "IP", "127.0.0.1"),
                Auth_Port = Ini.ReadUInt32("AUTHSERVER", "PORT", 7997),
                Rate = new RateConfigInfo()
            };
        }
        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        public TAppSession GetAppSessionByID(uint sessionID)
        {
            return GetSessionByID(sessionID);
        }

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        public override TAppSession GetSessionByID(uint sessionID)
        {
            if (sessionID == uint.MaxValue)
                return NullAppSession;

            TAppSession targetSession;
            m_SessionDict.TryGetValue(sessionID, out targetSession);
            return targetSession;
        }

        public override IAppSession GetSessionByNick(string Nick)
        {
            if (string.IsNullOrEmpty(Nick))
                return NullAppSession;

            foreach (var session in m_SessionDict.Values)
            {
                if (session.GetNickname() == Nick)
                {
                    return session;
                }
            }

            return NullAppSession;
        }

        public override IAppSession GetSessionByUserName(string ID)
        {
            if (string.IsNullOrEmpty(ID))
                return NullAppSession;

            foreach (var session in m_SessionDict.Values)
            {
                if (session.GetID() == ID)
                {
                    return session;
                }
            }

            return NullAppSession;
        }
        public void Send(TAppSession session, Packet packet)
        {
            session.Send(packet);
        }

        public void Send(TAppSession session, byte[] packet)
        {
            session.Send(packet);
        }
        public void SendToAll(Packet packet)
        {
            foreach (var session in m_SessionDict.Values)
            {
                session.Send(packet);
            }
        }

        public void SendToAll(byte[] packet)
        {
            foreach (var session in m_SessionDict.Values)
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

            SendToAll(response.GetBytes);

            Console.WriteLine("Mensagem enviada com sucesso");
        }


        public void TickerMessage(string message)
        {
            var response = new PangyaBinaryWriter();

            response.Write(new byte[] { 0xC9, 0x00 });
            response.WritePStr("@Admin");
            response.WritePStr(message);
            response.WriteZero(1);

            SendToAll(response.GetBytes);

            Console.WriteLine("Ticker enviado com sucesso");
        }




        public void BroadMessage(string message)
        {
            var response = new PangyaBinaryWriter();

            response.Write(new byte[] { 0x42, 0x00 });
            response.WritePStr("Aviso: " + message);

            SendToAll(response.GetBytes);
            Console.WriteLine("BroadCast enviado com sucesso");
        }
        #endregion

        /// <summary>
        /// Called when [socket session closed].
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="reason">The reason.</param>
        protected override void OnSessionClosed(TAppSession session, CloseReason reason)
        {
            uint sessionID = session.m_oid;

            if ((sessionID != uint.MaxValue))
            {
                TAppSession removedSession;
                if (!m_SessionDict.TryRemove(sessionID, out removedSession))
                {
                //    if (Logger.IsErrorEnabled)
                _smp.Message_Pool.push("Failed to remove this session, Because it has't been in session container!");
                }
            }

            base.OnSessionClosed(session, reason);
        }
      
        /// <summary>
        /// Gets the total session count.
        /// </summary>
        public override int SessionCount
        {
            get
            {
                return m_SessionDict.Count;
            }
        }

        #region Clear idle sessions

        private System.Threading.Timer m_ClearIdleSessionTimer = null;

        private void StartClearSessionTimer()
        {
            int interval = Config.ClearIdleSessionInterval * 1000;//in milliseconds
            m_ClearIdleSessionTimer = new System.Threading.Timer(ClearIdleSession, new object(), interval, interval);
        }

        /// <summary>
        /// Clears the idle session.
        /// </summary>
        /// <param name="state">The state.</param>
        private void ClearIdleSession(object state)
        {
            if (System.Threading.Monitor.TryEnter(state))
            {
                try
                {
                    var sessionSource = SessionSource;

                    if (sessionSource == null)
                        return;

                    DateTime now = DateTime.Now;
                    DateTime timeOut = now.AddSeconds(0 - Config.IdleSessionTimeOut);

                    var timeOutSessions = sessionSource.Where(s => s.Value.LastActiveTime <= timeOut).Select(s => s.Value);

                    Parallel.ForEach(timeOutSessions, s =>
                    {
                        _smp.Message_Pool.push(string.Format("The session will be closed for {0} timeout, the session start time: {1}, last active time: {2}!", now.Subtract(s.LastActiveTime).TotalSeconds, s.StartTime, s.LastActiveTime));
                        
                        s.Close(CloseReason.TimeOut);
                    });
                }
                catch (Exception e)
                {
                    _smp.Message_Pool.push("Clear idle session error!", e);
                }
                finally
                {
                    Monitor.Exit(state);
                }
            }
        }

        private KeyValuePair<uint, TAppSession>[] SessionSource
        {
            get
            {
                if (Config.DisableSessionSnapshot)
                    return m_SessionDict.ToArray();
                else
                    return m_SessionsSnapshot;
            }
        }

        #endregion

        #region Take session snapshot

        private System.Threading.Timer m_SessionSnapshotTimer = null;

        private KeyValuePair<uint, TAppSession>[] m_SessionsSnapshot = new KeyValuePair<uint, TAppSession>[0];
        private List<TAppSession> m_sessionsDel = new List<TAppSession>();

        private void StartSessionSnapshotTimer()
        {
            int interval = Math.Max(Config.SessionSnapshotInterval, 1) * 1000;//in milliseconds
            m_SessionSnapshotTimer = new System.Threading.Timer(TakeSessionSnapshot, new object(), interval, interval);
        }

        private void TakeSessionSnapshot(object state)
        {
            if (Monitor.TryEnter(state))
            {
                Interlocked.Exchange(ref m_SessionsSnapshot, m_SessionDict.ToArray());
                Monitor.Exit(state);
            }
        }

        #endregion

        #region Search session utils

        /// <summary>
        /// Gets the matched sessions from sessions snapshot.
        /// </summary>
        /// <param name="critera">The prediction critera.</param>
        /// <returns></returns>
        public override IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera)
        {
            var sessionSource = SessionSource;

            if (sessionSource == null)
                return null;

            return sessionSource.Select(p => p.Value).Where(critera);
        }

        /// <summary>
        /// Gets all sessions in sessions snapshot.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<TAppSession> GetAllSessions()
        {
            var sessionSource = SessionSource;

            if (sessionSource == null)
                return null;

            return sessionSource.Select(p => p.Value);
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            if (m_SessionSnapshotTimer != null)
            {
                m_SessionSnapshotTimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_SessionSnapshotTimer.Dispose();
                m_SessionSnapshotTimer = null;
            }

            if (m_ClearIdleSessionTimer != null)
            {
                m_ClearIdleSessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_ClearIdleSessionTimer.Dispose();
                m_ClearIdleSessionTimer = null;
            }

            m_SessionsSnapshot = null;

            var sessions = m_SessionDict.ToArray();

            if (sessions.Length > 0)
            {
                var tasks = new Task[sessions.Length];

                for (var i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = Task.Factory.StartNew((s) =>
                    {
                        var session = s as TAppSession;

                        if (session != null)
                        {
                            session.Close(CloseReason.ServerShutdown);
                        }

                    }, sessions[i].Value);
                }

                Task.WaitAll(tasks);
            }
        }

        public void SendAll(byte[] data)
        {

            foreach (var p in this.m_SessionDict.Values)
            {
                if (p != null &&p.SocketSession != null)
                {
                    p.Send(data);
                }
            }
        }
        #endregion
    }
}
