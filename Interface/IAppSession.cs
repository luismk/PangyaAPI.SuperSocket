using PangyaAPI.SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// interface usada pelo AppSession
    /// </summary>
    public interface IAppSession : Utilities.Interface.IDisposeable
    {
        /// <summary>
        /// Gets the app server.
        /// </summary>
        IServerBase AppServer { get; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        IDictionary<object, object> Items { get; }

        Socket AppClient { get; set; }

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
        /// Gets a value indicating whether this <see cref="IAppSession"/> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        bool m_Connected { get; set; }

        byte m_key { get; set; }

        uint m_oid { get; set; }

        string IP { get; }

        string Port { get; }


        /// <summary>
        /// Gets or sets the charset which is used for transfering text message.
        /// </summary>
        /// <value>The charset.</value>
        Encoding Charset { get; set; }

        void Send(Packet packet);
        void Send(byte[] message);
        void Send();

        void SendResponse(Packet packet);
        void SendResponse(byte[] message);
        void SendResponse();
        void SendCallback(IAsyncResult result);
        string GetNickname();
        uint GetUID();
    }
    /// <summary>
    /// The interface for appSession
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IAppSession<TAppSession, TPacket> : IAppSession
       where TPacket : IPacket
       where TAppSession : IAppSession, IAppSession<TAppSession, TPacket>, new()
    {
        void Initialize(IServerBase appServer, Socket client);

    }
}
