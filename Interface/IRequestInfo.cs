using PangyaAPI.SuperSocket.SocketBase;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// Request information interface
    /// </summary>
    public interface IRequestInfo
    {
        /// <summary>
        /// Gets the ID of this request.
        /// </summary>
        short PacketID { get; }
        Packet _packet { get; set; }


    }

    /// <summary>
    /// Request information interface
    /// </summary>
    /// <typeparam name="TRequestBody">The type of the request body.</typeparam>
    public interface IRequestInfo<TRequestBody> : IRequestInfo
    {
        /// <summary>
        /// Gets the body of this request.
        /// </summary>
        TRequestBody Body { get; }
    }


    /// <summary>
    /// Request information interface
    /// </summary>
    /// <typeparam name="TRequestHeader">The type of the request header.</typeparam>
    /// <typeparam name="TRequestBody">The type of the request body.</typeparam>
    public interface IRequestInfo<TRequestHeader, TRequestBody> : IRequestInfo<TRequestBody>
    {
        /// <summary>
        /// Gets the header of the request.
        /// </summary>
        TRequestHeader Header { get; }
    }
}