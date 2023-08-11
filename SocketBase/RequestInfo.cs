using PangyaAPI.SuperSocket.Interface;

namespace PangyaAPI.SuperSocket.SocketBase
{
    /// <summary>
    /// RequestInfo basic class
    /// </summary>
    /// <typeparam name="TRequestBody">The type of the request body.</typeparam>
    public class RequestInfo<TRequestBody> : IRequestInfo<TRequestBody>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestInfo&lt;TRequestBody&gt;"/> class.
        /// </summary>
        protected RequestInfo()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestInfo&lt;TRequestBody&gt;"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="body">The body.</param>
        public RequestInfo(uint key, TRequestBody body)
        {
            Initialize(key, body);
        }

        /// <summary>
        /// Initializes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="body">The body.</param>
        protected void Initialize(uint key, TRequestBody body)
        {
            m_oid = key;
            Body = body;
        }

        /// <summary>
        /// Gets the connectionID of this request.
        /// </summary>
        public uint m_oid { get; private set; }

        /// <summary>
        /// Gets the body.
        /// </summary>
        public TRequestBody Body { get; private set; }
        public byte[] Message { get; set; }
    }

    /// <summary>
    /// RequestInfo with header
    /// </summary>
    /// <typeparam name="TRequestHeader">The type of the request header.</typeparam>
    /// <typeparam name="TRequestBody">The type of the request body.</typeparam>
    public class RequestInfo<TRequestHeader, TRequestBody> : RequestInfo<TRequestBody>, IRequestInfo<TRequestHeader, TRequestBody>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestInfo&lt;TRequestHeader, TRequestBody&gt;"/> class.
        /// </summary>
        public RequestInfo()
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestInfo&lt;TRequestHeader, TRequestBody&gt;"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        public RequestInfo(uint key, TRequestHeader header, TRequestBody body)
            : base(key, body)
        {
            Header = header;
        }

        /// <summary>
        /// Initializes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        public void Initialize(uint key, TRequestHeader header, TRequestBody body)
        {
            base.Initialize(key, body);
            Header = header;
        }
        /// <summary>
        /// Gets the header.
        /// </summary>
        public TRequestHeader Header { get; private set; }
    }
}