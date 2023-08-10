using PangyaAPI.SuperSocket.Interface;

namespace PangyaAPI.SuperSocket.SocketBase
{
    /// <summary>
    /// The interface for a Receive filter to adapt receiving buffer offset
    /// </summary>
    public interface IOffsetAdapter
    {
        /// <summary>
        /// Gets the offset delta.
        /// </summary>
        int OffsetDelta { get; }
    }
}