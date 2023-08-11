using PangyaAPI.SuperSocket.SocketBase;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// StatusInfo source interface
    /// </summary>
    public interface IStatusInfoSource
    {
        /// <summary>
        /// Gets the server status metadata.
        /// </summary>
        /// <returns></returns>
        SocketBase.StatusInfoAttribute[] GetServerStatusMetadata();

        /// <summary>
        /// Collects the bootstrap status.
        /// </summary>
        /// <param name="bootstrapStatus">The bootstrap status.</param>
        /// <returns></returns>
        StatusInfoCollection CollectServerStatus(StatusInfoCollection bootstrapStatus);
    }
}