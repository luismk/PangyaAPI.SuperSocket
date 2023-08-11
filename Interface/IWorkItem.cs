using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.SuperSocket.SocketBase;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// An item can be started and stopped
    /// </summary>
    public interface IWorkItemBase : IStatusInfoSource, ISystemEndPoint
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
    }


    /// <summary>
    /// An item can be started and stopped
    /// </summary>
    public interface IWorkItem : IWorkItemBase, IStatusInfoSource
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