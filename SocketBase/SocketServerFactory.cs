using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.SuperSocket.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.SocketBase
{
    /// <summary>
    /// Default socket server factory
    /// </summary>
    public class SocketServerFactory : ISocketServerFactory
    {
        #region ISocketServerFactory Members

        /// <summary>
        /// Creates the socket server.
        /// </summary>
        /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
        /// <param name="appServer">The app server.</param>
        /// <param name="listeners">The listeners.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        public ISocketServer CreateSocketServer<TRequestInfo>(IAppServer appServer, ListenerInfo[] listeners, IServerConfig config)
            where TRequestInfo : IRequestInfo
        {
            if (appServer == null)
                throw new ArgumentNullException("appServer");

            if (listeners == null)
                throw new ArgumentNullException("listeners");

            if (config == null)
                throw new ArgumentNullException("config");

            return new AsyncSocketServer(appServer, listeners);
        }
        #endregion
    }
}
