using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.SuperSocket.SocketBase;
using ServerConsole.Session;

namespace ServerConsole.Server
{
    /// <summary>
    /// Vai ser a class usada no Program.cs
    /// </summary>
    public class MainServer : PangyaServer<Player>

    {
        public MainServer()
        {
        }

        //public override PangyaAPI.SuperSocket.Interface.IAppSession GetSessionByNick(string Nick)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public override PangyaAPI.SuperSocket.Interface.IAppSession GetSessionByUserName(string User)
        //{
        //    throw new System.NotImplementedException();
        //}

        protected override void SendKeyOnConnect(Player session)
        {
            throw new System.NotImplementedException();
        }
    }
}
