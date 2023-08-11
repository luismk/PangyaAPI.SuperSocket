using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.SuperSocket.SocketBase;
namespace ServerConsole.Session
{
    /// <summary>
    /// class PLayer, aqui voce coloca todos os objetos e metodos que ira usar ao longo do desenvolvimento
    /// </summary>
    public partial class Player : AppSession<Player, IRequestInfo>, IAppSession
    {
     public   Player()
        {
        }
        public override string GetNickname()
        {
            return "LuisMK";
        }
    }
}
