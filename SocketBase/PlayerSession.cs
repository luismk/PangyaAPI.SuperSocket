//using PangyaAPI.SuperSocket.Interface;
//using PangyaAPI.SuperSocket.Server;
//using PangyaAPI.SuperSocket.SocketBase;
//using PangyaAPI.TcpServer.AppEngine;
//using PangyaAPI.TcpServer.AppPacket;
//using PangyaAPI.TcpServer.AppServer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace PangyaAPI.SuperSocket.SocketBase
//{
//    public abstract class PlayerSession<T> : AppSession<T, Packet>
//             where T : AppSession<T, Packet>, IAppSession, new()
//    {

//        public override AppServerBase<T, Packet> AppServer => base.AppServer;


//        public void SendNotCompress(byte[] packet)
//        {
//            try
//            {
//                if (Connected)
//                base.Stream.Write(packet, 0, packet.Length);
//            }
//            catch (Exception)
//            {
//                Connected = false;
//            }
//        }
//        public override void Send(Packet packet)
//        {
//            base.Send(packet);
//        }

//        public override void Send(byte[] packet)
//        {
//            base.Send(packet);
//        }

//        public override void SendResponse(Packet packet)
//        {
//            base.Send(packet);
//        }

//        public override void SendResponse(byte[] packet)
//        {
//            base.Send(packet);
//        }

//        public override void Send()
//        {
//            base.Send(Packet);
//            Packet.Clear();
//        }

//        public override void SendResponse()
//        {
//            Send(Packet);
//            Packet.Clear();
//        }

//        protected internal override void OnSessionClosed()
//        {
//            base.OnSessionClosed();
//        }

//        public void SetPangs(ulong pangs)
//        {
//            UserInfo.GetPang = pangs;
//        }
//        public void SetCookie(uint Cookie)
//        {
//            UserInfo.GetCookies = Cookie;
//        }

//        public bool SetAuthKey1(string TAUTH_KEY_1)
//        {
//            UserInfo.GetAuth1 = TAUTH_KEY_1;
//            return true;
//        }

//        public bool SetAuthKey2(string TAUTH_KEY_2)
//        {
//            UserInfo.GetAuth2 = TAUTH_KEY_2;
//            return true;
//        }

//        public bool SetCapabilities(uint TCapa)
//        {
//            UserInfo.GetCapability = TCapa;
//            if (TCapa == 4)
//            {
//                UserInfo.Visible = 0;
//            }
//            return true;
//        }

//        public void SetExp(uint Amount)
//        {
//            UserInfo.UserStatistic.EXP = Amount;
//        }

//        public void SetLevel(byte Amount)
//        {
//            UserInfo.UserStatistic.Level = Amount;
//        }

//        public bool SetLogin(string TLogin)
//        {
//            UserInfo.GetLogin = TLogin;
//            return true;
//        }

//        public bool SetNickname(string TNickname)
//        {
//            UserInfo.GetNickname = TNickname;
//            return true;
//        }

//        public bool SetSex(Byte TSex)
//        {
//            UserInfo.GetIDState = TSex;
//            return true;
//        }

//        public bool SetUID(uint TUID)
//        {
//            UserInfo.GetUID = TUID;
//            return true;
//        }

//    }
//}
