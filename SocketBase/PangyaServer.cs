using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.SuperSocket.StructData;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public abstract class PangyaServer<T> : AppServer<T, Packet>
        where T : AppSession<T, Packet>, IAppSession, new()
    {
        #region
        protected abstract void SendKeyOnConnect(T session);
        #endregion
        public PangyaServer()
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

        protected PangyaServer(string servername)
          : base()
        {

        }
        public bool StartingServer()
        {

            try
            {
                var result = Start();

                if (result == false)
                {
                    Console.WriteLine("Failed to start!");
                    Console.ReadKey();
                }
                if (result)
                {

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
                Version = Ini.ReadString("Config", "VERSION", "Pangya Server Csharp 1.0"),
                Version_Client = Ini.ReadString("Config", "CLIENTVERSION", "JP.R7.962.00"),
                Name = Ini.ReadString("Config", "NAME", "Pangya Server Csharp"),
                UID = Ini.ReadInt32("Config", "GUID", 10103),
                Port = Ini.ReadInt32("Config", "PORT", 10103),
                IP = Ini.ReadString("Config", "IP", "127.0.0.1"),
                MaxUser = Ini.ReadInt32("Config", "MAXUSER", 2001),
                Property= new uPropertyEx(Ini.ReadUInt32("Config", "PROPERTY", 2048)),
                Auth_IP = Ini.ReadString("AUTHSERVER", "IP", "127.0.0.1"),
                Auth_Port = Ini.ReadUInt32("AUTHSERVER", "PORT", 7997),
                Rate = new RateConfigInfo()
            };
            IFFLog = Ini.ReadBool("Config", "IFFLog", false);
        }
        private void ProcessNewMessage(T session, Packet requestinfo)
        {
            session.Packet = requestinfo;
        }

        protected override void OnNewSessionConnected(T session)
        {
            SendKeyOnConnect(session);
            if (session.Connected)
            {
                WriteConsole.WriteLine($"[PLAYER_CONNETED]: ID => {session.ConnectionId}, Connection => {session?.Adress}:{session?.Port}", ConsoleColor.Green);
            }
            m_si.Curr_User = SessionCount;
        }

        protected override void OnSessionClosed(T session)
        {
            base.OnSessionClosed(session);
            WriteConsole.WriteLine($"[PLAYER_DISCONNETED]: ID => {session?.ConnectionId}, Connection => {session?.Adress}:{session?.Port}", ConsoleColor.Red);
            m_si.Curr_User = SessionCount;
        }

        public void NewSessionClosed(T session)
        {
            OnSessionClosed(session);
        }

    }
}
