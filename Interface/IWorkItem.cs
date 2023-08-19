using PangyaAPI.Player.Data;
using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.SuperSocket.SocketBase;
using PangyaAPI.Utilities;
using System.Collections.Generic;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// An item can be started and stopped
    /// </summary>
    public interface IWorkItemBase
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the server's config.
        /// </summary>
        /// <value>
        /// The server's config.
        /// </value>
        IServerConfig Config { get; }


        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>return true if start successfull, else false</returns>
        bool Start();

        /// <summary>
        /// Stops this server instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets the total session count.
        /// </summary>
        int SessionCount { get; }
        // public IFFHandle IFF { get; set; }
        //bool IFFLog { get; set; }
        //IniHandle Ini { get; set; }
        //ServerInfoEx m_si { get; set; }
        //List<TableMac> ListBlockMac { get; set; }
    }


    /// <summary>
    /// An item can be started and stopped
    /// </summary>
    public interface IWorkItem : IWorkItemBase
    {
       
        /// <summary>
        /// Gets the current state of the work item.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        ServerState State { get; }
    }
}