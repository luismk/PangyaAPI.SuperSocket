using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// The basic session interface
    /// </summary>
    public interface ISessionBase
    {
        byte m_key { get; }

        /// <summary>
        /// Gets the session ID.
        /// </summary>
        uint m_oid { get; set; }
        /// <summary>
        /// Gets the remote endpoint.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }
    }
}
