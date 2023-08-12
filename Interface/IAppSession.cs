using PangyaAPI.SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// interface usada pelo AppSession
    /// </summary>
    public interface IAppSession : ISessionBase
    {
        /// <summary>
        /// Gets the app server.
        /// </summary>
        IAppServer AppServer { get; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        IDictionary<object, object> Items { get; }
        /// <summary>
        /// Gets the local listening endpoint.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }
        ISocketSession SocketSession { get; }

        /// <summary>
        /// Gets or sets the last active time of the session.
        /// </summary>
        /// <value>
        /// The last active time.
        /// </value>
        DateTime LastActiveTime { get; set; }

        /// <summary>
        /// Gets the start time of the session.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Closes this session.
        /// </summary>
        void Close();

        /// <summary>
        /// Closes the session by the specified reason.
        /// </summary>
        /// <param name="reason">The close reason.</param>
        void Close(CloseReason reason);

        //// <summary>
        /// Gets a value indicating whether this <see cref="IAppSession"/> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        bool Connected { get; }

        string GetAdress { get; }

        /// <summary>
        /// Gets the config of the server.
        /// </summary>
        IServerConfig Config { get; }


        /// <summary>
        /// Gets or sets the charset which is used for transfering text message.
        /// </summary>
        /// <value>The charset.</value>
        Encoding Charset { get; set; }
        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <returns>return offset delta of next receiving buffer</returns>
        int ProcessRequest(byte[] readBuffer, int offset, int length, bool toBeCopied);
        /// <summary>
        /// Starts the session.
        /// </summary>
        void StartSession();
        //void Send(Packet packet);
        //void Send(byte[] message);
        //void Send();

        //void SendResponse(Packet packet);
        //void SendResponse(byte[] message);
        //void SendResponse();
        //void SendCallback(IAsyncResult result);
        string GetNickname();
        uint GetUID();
        string GetID();

        void HandleExceptionalError(Exception e);

    }
    /// <summary>
    /// The interface for appSession
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IAppSession<TAppSession, TRequestInfo> : IAppSession
         where TRequestInfo : IRequestInfo
         where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
    {
        void Initialize(IAppServer appServer, Socket client);
        void Initialize(IAppServer<TAppSession, TRequestInfo> server, ISocketSession socketSession);

    }
}
