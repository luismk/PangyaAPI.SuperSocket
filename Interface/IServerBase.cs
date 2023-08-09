using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// interface do servidor,criado por LuisMK
    /// </summary>
    public interface IServerBase
    {
        /// <summary>
        /// Gets the started time.
        /// </summary>
        /// <value>
        /// The started time.
        /// </value>
        DateTime StartedTime { get; }

        /// <summary>
        /// Creates the app session.
        /// </summary>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        IAppSession CreateAppSession(Socket socketSession);

        /// <summary>
        /// Registers the new created app session into the appserver's session container.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        bool RegisterSession(IAppSession session);

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        IAppSession GetSessionByID(int sessionID);

        IAppSession GetSessionByUserName(string User);

        IAppSession GetSessionByNick(string Nick);
        
        bool Start();

        bool Stop();
        bool Reset();
        /// <summary>
        /// cria e lida com player
        /// </summary>
        /// <param name="client"> tcpclient</param>
        void HandleClient(object client);
        /// <summary>
        /// retorna a leitura do pacote
        /// </summary>
        /// <param name="client">client</param>
        /// <returns></returns>
        Packet HandleReceived(IAppSession client);
        void Send(IAppSession client, Packet packet);
        void SendToAll(Packet packet);
        void Send(IAppSession client, byte[] packet);
        void SendToAll(byte[] packet);

        void BroadMessage(string message);
        void TickerMessage(string message);
        void ServerMessage(string message);
    }
    /// <summary>
    /// The interface for AppServer
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    public interface IServerBase<TAppSession> : IServerBase
       where TAppSession : IAppSession
    {
        /// <summary>
        /// Gets the matched sessions from sessions snapshot.
        /// </summary>
        /// <param name="critera">The prediction critera.</param>
        /// <returns></returns>

        /// <summary>
        /// Gets all sessions in sessions snapshot.
        /// </summary>
        /// <returns></returns>
        List<TAppSession> GetAllSessions();

        /// <summary>
        /// Gets/sets the new session connected event handler.
        /// </summary>
        event SessionHandler<TAppSession> NewSessionConnected;

        /// <summary>
        /// Gets/sets the session closed event handler.
        /// </summary>
        event SessionHandler<TAppSession> SessionClosed;
    }
    /// <summary>
    /// The interface for ServerBase
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IServerBase<TAppSession, TPacket> : IServerBase<TAppSession>
        where TPacket : IPacket
        where TAppSession : IAppSession, IAppSession<TAppSession, TPacket>, new()
    {
        /// <summary>
        /// Occurs when [request comming].
        /// </summary>
        event RequestHandler<TAppSession, TPacket> NewRequestReceived;
    }

}
