using PangyaAPI.Player.Data;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.SuperSocket.SocketBase;
using ServerConsole.Server;
using System;
using System.Net.Sockets;

namespace ServerConsole.Session
{
    /// <summary>
    /// class PLayer, aqui voce coloca todos os objetos e metodos que ira usar ao longo do desenvolvimento
    /// </summary>
    public partial class Player : AppSession<Player, PangyaRequestInfo>, IAppSession
    {
        public PlayerInfo m_pi { get; set; }
        public string m_ip => GetAdress;
        public Player()
        {
            m_pi = new PlayerInfo();
        }
        public override string GetNickname()
        {
            return m_pi.nickname;
        }

        public override uint GetUID()
        {
            return (uint)m_pi.uid;
        }

        public override string GetID()
        {
            return m_pi.id;
        }

        public void SendMessage(LoginMessages.IMessage message)
        {
            Send(message.ToBytes());
        }
    }
}
