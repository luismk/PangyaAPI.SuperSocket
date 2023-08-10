namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// The listener configuration interface
    /// </summary>
    public interface IListenerConfig
    {
        /// <summary>
        /// Gets the ip of listener
        /// </summary>
        string Ip { get; }

        /// <summary>
        /// Gets the port of listener
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the backlog.
        /// </summary>
        int Backlog { get; }
    }
}