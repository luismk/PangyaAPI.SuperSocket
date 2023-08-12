using PangyaAPI.Player.Data;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.SocketBase
{

    public abstract partial class PangyaServer<T> : AppServer<T, PangyaRequestInfo>
     where T : AppSession<T, PangyaRequestInfo>, IAppSession, new()
    {
        public PangyaServer() :base(new DefaultReceiveFilterFactory<PangyaReceiveFilter, PangyaRequestInfo>())
        {
            try
            {
                if (LoadingFiles())
                {
                    ConfigInit();
                    //Logger.SetFileName(m_si.Name + ".log");
                    NewRequestReceived += ProcessNewMessage;
                }
            }
            catch (Exception ex)
            {

                WriteConsole.Error(ex.Message);
            }
        }
        private void ProcessNewMessage(T session, PangyaRequestInfo requestInfo)
        {
        }

        public bool StartingServer()
        {

            try
            {
                var result = Setup(m_si.IP, m_si.Port);
                if (result == false)
                {
                    Console.WriteLine("Failed to Setup!");
                    Console.ReadKey();
                }
                result = Start();
                if (result == false)
                {
                    Console.WriteLine("Failed to start!");
                    Console.ReadKey();
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteConsole.Error(ex.Message);
                return false;
            }
        }

        protected virtual void ConfigInit()
        {
            m_si = new ServerInfoEx
            {
                Version = Ini.ReadString("SERVERINFO", "VERSION", "Pangya Server Csharp 1.0"),
                Version_Client = Ini.ReadString("SERVERINFO", "CLIENTVERSION", "JP.R7.962.00"),
                Name = Ini.ReadString("SERVERINFO", "NAME", "Pangya Server Csharp"),
                UID = Ini.ReadInt32("SERVERINFO", "GUID", 10103),
                Port = Ini.ReadInt32("SERVERINFO", "PORT", 10103),
                IP = Ini.ReadString("SERVERINFO", "IP", "127.0.0.1"),
                MaxUser = Ini.ReadInt32("SERVERINFO", "MAXUSER", 2001),
                Property= new uPropertyEx(Ini.ReadUInt32("SERVERINFO", "PROPERTY", 2048)),
                Auth_IP = Ini.ReadString("AUTHSERVER", "IP", "127.0.0.1"),
                Auth_Port = Ini.ReadUInt32("AUTHSERVER", "PORT", 7997),
                Rate = new RateConfigInfo()
            };
            //IFFLog = Ini.ReadBool("SERVERINFO", "IFFLog", false);

            //Players = new AppSessionManager(m_si.MaxUser);

        }

        protected override void OnNewSessionConnected(T session)
        {
            onAcceptCompleted(session);
            if (session.Connected)
            {
                WriteConsole.WriteLine($"[PLAYER_CONNETED]: ID => {session.m_oid}, Connection => {session?.GetAdress}", ConsoleColor.Green);
            }
            m_si.Curr_User = SessionCount;
        }

        protected override void OnSessionClosed(T session, CloseReason reason = CloseReason.ClientClosing)
        {
            base.OnSessionClosed(session, reason);
            WriteConsole.WriteLine($"[PLAYER_DISCONNETED]: ID => {session?.m_oid}, Connection => {session?.GetAdress}", ConsoleColor.Red);
            m_si.Curr_User = SessionCount;
        }
    }
}
