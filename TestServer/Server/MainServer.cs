using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.SuperSocket.SocketBase;
using PangyaAPI.Utilities;
using ServerConsole.Session;
using System;
using System.Diagnostics;
using _smp = PangyaAPI.Utilities.Log;
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

        public override void onHeartBeat()
        {
            try
            {
                // Server ainda não está totalmente iniciado
                if (this.State != ServerState.NotInitialized)
                    return;

                // Tirei o list IP/MAC block daqui e coloquei no monitor no server, por que agora eles são da classe server
            }
            catch (exception e)
            {
                _smp.Message_Pool.push("[login_server::onHeartBeat][ErrorSystem] " + e.getFullMessageError(), _smp.type_msg.CL_FILE_LOG_AND_CONSOLE);
            }
        }

        //public override PangyaAPI.SuperSocket.Interface.IAppSession GetSessionByNick(string Nick)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public override PangyaAPI.SuperSocket.Interface.IAppSession GetSessionByUserName(string User)
        //{
        //    throw new System.NotImplementedException();
        //}

        protected override void onAcceptCompleted(Player _session)
        {
            try
            {
                _session.Send(new byte[]
                {0x00, 0x0b, 0x00, 0x00, 0x00, 0x00, _session.m_key, 0x00, 0x00, 0x00, 0x75, 0x27, 0x00, 0x00});
            }
            catch (Exception e) {
               
            }

        }
    }
}
