using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.SuperSocket.SocketBase;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Web.Util;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// interface do servidor,criado por LuisMK
    /// </summary>
    public interface IAppServer : IWorkItem
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
        /// <summary>
        /// Creates the app session.
        /// </summary>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        IAppSession CreateAppSession(ISocketSession socketSession);
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
        IAppSession GetSessionByID(uint sessionID);

        IAppSession GetSessionByUserName(string User);

        IAppSession GetSessionByNick(string Nick);

        /// <summary>
        /// Gets the Receive filter factory.
        /// </summary>
        object ReceiveFilterFactory { get; }
        object SendingQueuePool { get; }
    }


    /// <summary>
    /// The raw data processor
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    public interface IRawDataProcessor<TAppSession>
        where TAppSession : IAppSession
    {
        /// <summary>
        /// Gets or sets the raw binary data received event handler.
        /// TAppSession: session
        /// byte[]: receive buffer
        /// int: receive buffer offset
        /// int: receive lenght
        /// bool: whether process the received data further
        /// </summary>
        event Func<TAppSession, byte[], int, int, bool> RawDataReceived;
    }

    /// <summary>
    /// The interface for AppServer
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    public interface IAppServer<TAppSession> : IAppServer
        where TAppSession : IAppSession
    {
        /// <summary>
        /// Gets the matched sessions from sessions snapshot.
        /// </summary>
        /// <param name="critera">The prediction critera.</param>
        /// <returns></returns>
        IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera);

        /// <summary>
        /// Gets all sessions in sessions snapshot.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TAppSession> GetAllSessions();

        /// <summary>
        /// Gets/sets the new session connected event handler.
        /// </summary>
        event SessionHandler<TAppSession> NewSessionConnected;

        /// <summary>
        /// Gets/sets the session closed event handler.
        /// </summary>
        event SessionHandler<TAppSession, CloseReason> SessionClosed;
    }

    /// <summary>
    /// The interface for AppServer
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IAppServer<TAppSession, TRequestInfo> : IAppServer<TAppSession>
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
    {
        /// <summary>
        /// Occurs when [request comming].
        /// </summary>
        event RequestHandler<TAppSession, TRequestInfo> NewRequestReceived;
    }

    /// <summary>
    /// The interface for handler of session request
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IRequestHandler<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        void HandleRequest(IAppSession session, TRequestInfo requestInfo);
    }

    /// <summary>
    /// SocketServer Accessor interface
    /// </summary>
    public interface ISocketServerAccessor
    {
        /// <summary>
        /// Gets the socket server.
        /// </summary>
        /// <value>
        /// The socket server.
        /// </value>
        ISocketServer SocketServer { get; }
    }

}
