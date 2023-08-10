using PangyaAPI.SuperSocket.Interface;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public interface IReceiveFilter<TRequestInfo> where TRequestInfo : class, IRequestInfo
    {
    }
}