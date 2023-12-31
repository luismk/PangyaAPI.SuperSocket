﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// Server instance configuation interface
    /// </summary>
    public partial interface IServerConfig
    {
        /// <summary>
        /// Gets the name of the server type this appServer want to use.
        /// </summary>
        /// <value>
        /// The name of the server type.
        /// </value>
        string ServerTypeName { get; }

        /// <summary>
        /// Gets the type definition of the appserver.
        /// </summary>
        /// <value>
        /// The type of the server.
        /// </value>
        string ServerType { get; }

        /// <summary>
        /// Gets the Receive filter factory.
        /// </summary>
        string ReceiveFilterFactory { get; }

        /// <summary>
        /// Gets the ip.
        /// </summary>
        string Ip { get; }

        /// <summary>
        /// Gets the port.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        NameValueCollection Options { get; }


        /// <summary>
        /// Gets the option elements.
        /// </summary>
        NameValueCollection OptionElements { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IServerConfig"/> is disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disabled; otherwise, <c>false</c>.
        /// </value>
        bool Disabled { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the mode.
        /// </summary>
        SocketMode Mode { get; }

        /// <summary>
        /// Gets the send time out.
        /// </summary>
        int SendTimeOut { get; }

        /// <summary>
        /// Gets the max connection number.
        /// </summary>
        int MaxConnectionNumber { get; }

        /// <summary>
        /// Gets the size of the receive buffer.
        /// </summary>
        /// <value>
        /// The size of the receive buffer.
        /// </value>
        int ReceiveBufferSize { get; }

        /// <summary>
        /// Gets the size of the send buffer.
        /// </summary>
        /// <value>
        /// The size of the send buffer.
        /// </value>
        int SendBufferSize { get; }


        /// <summary>
        /// Gets a value indicating whether sending is in synchronous mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sync send]; otherwise, <c>false</c>.
        /// </value>
        bool SyncSend { get; }

        /// <summary>
        /// Gets a value indicating whether clear idle session.
        /// </summary>
        /// <value><c>true</c> if clear idle session; otherwise, <c>false</c>.</value>
        bool ClearIdleSession { get; }

        /// <summary>
        /// Gets the clear idle session interval, in seconds.
        /// </summary>
        /// <value>The clear idle session interval.</value>
        int ClearIdleSessionInterval { get; }


        /// <summary>
        /// Gets the idle session timeout time length, in seconds.
        /// </summary>
        /// <value>The idle session time out.</value>
        int IdleSessionTimeOut { get; }


        /// <summary>
        /// Gets the length of the max request.
        /// </summary>
        /// <value>
        /// The length of the max request.
        /// </value>
        int MaxRequestLength { get; }


        /// <summary>
        /// Gets a value indicating whether [disable session snapshot].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [disable session snapshot]; otherwise, <c>false</c>.
        /// </value>
        bool DisableSessionSnapshot { get; }
        /// <summary>
        /// Gets the interval to taking snapshot for all live sessions.
        /// </summary>
        int SessionSnapshotInterval { get; }

        /// <summary>
        /// Gets the connection filters used by this server instance.
        /// </summary>
        /// <value>
        /// The connection filter's name list, seperated by comma
        /// </value>
        string ConnectionFilter { get; }
        /// <summary>
        /// Gets the start keep alive time, in seconds
        /// </summary>
        int KeepAliveTime { get; }


        /// <summary>
        /// Gets the keep alive interval, in seconds.
        /// </summary>
        int KeepAliveInterval { get; }


        /// <summary>
        /// Gets the backlog size of socket listening.
        /// </summary>
        int ListenBacklog { get; }


        /// <summary>
        /// Gets the startup order of the server instance.
        /// </summary>
        int StartupOrder { get; }


        /// <summary>
        /// Gets the child config.
        /// </summary>
        /// <typeparam name="TConfig">The type of the config.</typeparam>
        /// <param name="childConfigName">Name of the child config.</param>
        /// <returns></returns>
        TConfig GetChildConfig<TConfig>(string childConfigName)
            where TConfig : ConfigurationElement, new();


        /// <summary>
        /// Gets the listeners' configuration.
        /// </summary>
        IEnumerable<IListenerConfig> Listeners { get; }
        /// <summary>
        /// Gets the size of the sending queue.
        /// </summary>
        /// <value>
        /// The size of the sending queue.
        /// </value>
        int SendingQueueSize { get; }
    }
}