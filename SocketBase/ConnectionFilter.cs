using PangyaAPI.SuperSocket.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public class ConnectionFilter : IConnectionFilter
    {
        internal IAppServer AppServer { get; private set; }
        private int maxConnections;
        private readonly Dictionary<IPAddress, int> clientConnections = new Dictionary<IPAddress, int>();
        public ConnectionFilter(int maxConnections, string name, IAppServer appServer)
        {
            this.maxConnections = maxConnections;
            Name = name;
            AppServer = appServer;
            Allow = true;
        }
        public ConnectionFilter() { }
        public bool Initialize(string name, IAppServer appServer)
        {
            maxConnections = 1;//maximo de conexão vindo de um unico cliente!
            Name = name;
            AppServer = appServer;
            Allow = true;
            return true;
        }

        public string Name { get; private set; }

        public bool Allow { get; set; }

        private int m_ConnectedCount = 0;

        public int ConnectedCount
        {
            get { return m_ConnectedCount; }
        }

        public bool AllowConnect(IPEndPoint remoteAddress)
        {
            if (!Allow)
                return false;

            IPAddress clientAddress = remoteAddress.Address;
            lock (clientConnections)
            {
                if (!clientConnections.ContainsKey(clientAddress))
                {
                    clientConnections[clientAddress] = 0;
                }

                if (clientConnections[clientAddress] < maxConnections)
                {
                    clientConnections[clientAddress]++;
                    Interlocked.Increment(ref m_ConnectedCount);
                    return true;
                }
                else
                {
                    Interlocked.Decrement(ref m_ConnectedCount);
                    return false;
                }
            }
        }
    }
}