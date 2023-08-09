using PangyaAPI.SuperSocket.Interface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public abstract partial class AppSession<TAppSession, TPacket> : IAppSession, IAppSession<TAppSession, TPacket>
        where TAppSession : AppSession<TAppSession, TPacket>, IAppSession, new()
        where TPacket : class, IPacket
    {
        public bool Disposed { get; set; }

        protected IPEndPoint RemoteEndPoint  { get; set; }

        public DateTime LastActiveTime { get; set; }

        public DateTime StartTime  { get; set; }

        public bool m_Connected  { get; set; }

        public Encoding Charset { get; set; }

        public IDictionary<object, object> Items { get; set; }

        public Socket AppClient { get; set; }
        public Packet Packet { get; set; }
        public byte m_key { get ; set ; }
        public uint m_oid { get ; set ; }


        /// <summary>
        /// Gets the app server instance assosiated with the session.
        /// </summary>
        public virtual AppServerBase<TAppSession, TPacket> AppServer { get; set; }
        /// <summary>
        /// Gets the app server instance assosiated with the session.
        /// </summary>
        IServerBase IAppSession.AppServer
        {
            get { return this.AppServer; }
        }
        public string IP
        {
            get
            {
                if (m_Connected)
                {
                    return RemoteEndPoint.Address.ToString();
                }
                else
                {
                    return "IP NULL";
                }
            }
        }
        public string Port
        {
            get
            {
                if (m_Connected)
                {
                    return RemoteEndPoint.Port.ToString();
                }
                else
                {
                    return "PORT NULL";
                }
            }
        }


        public AppSession()
        {
            try
            {              
                m_key = 0;
                //UserInfo = new UserInfo();
            }
            catch (Exception)
            {


            }
        }
        public void Disconnect()
        {
            AppServer.OnNewSessionDisconnect(this as TAppSession);
        }
        /// <summary>
        /// Called when [session closed].
        /// </summary>
        /// <param name="reason">The reason.</param>
        internal protected virtual void OnSessionClosed()
        {
            Close();
        }
        public void Close()
        {
            if (AppClient != null && AppClient.Connected)
            {
                try
                {
                    AppClient.Shutdown(SocketShutdown.Both);
                    AppClient.Disconnect(reuseSocket: false);
                    if (Packet != null)
                    {
                        Packet.Dispose();
                    }
                }
                finally
                {
                    m_Connected = false;
                    AppClient.Close();
                }
            }
            Dispose();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                return;
            }
            if (disposing)
            {
                if (AppClient != null)
                {
                    AppClient.Dispose();
                    AppClient = null;
                }
                Console.WriteLine("Session.Dispose()");
            }
            Disposed = true;
        }


        public virtual void Send(Packet packet)
        {
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(packet.GetBytes, m_key);
            try
            {
                AppClient.BeginSend(GetBytes, 0, GetBytes.Length, SocketFlags.None, SendCallback, AppClient);
            }
            catch (Exception)
            {


            }
        }
        
        public virtual void Send(byte[] message)
        {
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(message, m_key);
            try
            {
                AppClient.BeginSend(GetBytes, 0, GetBytes.Length, SocketFlags.None, SendCallback, AppClient);
            }
            catch (Exception)
            {


            } 
        }

        public virtual void SendResponse(Packet packet)
        {
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(packet.GetBytes, m_key);
            try
            {
                AppClient.BeginSend(GetBytes, 0, GetBytes.Length, SocketFlags.None, SendCallback, AppClient);
            }
            catch (Exception)
            {


            }
        }
        
        public virtual void SendResponse(byte[] message)
        {
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(message, m_key);
            try
            {
                AppClient.BeginSend(GetBytes, 0, GetBytes.Length, SocketFlags.None, SendCallback, AppClient);
            }
            catch (Exception)
            {


            }
        }
        public virtual void Send()
        {
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(Packet.GetBytes, m_key);
            try
            {
                AppClient.BeginSend(GetBytes, 0, GetBytes.Length, SocketFlags.None, SendCallback, AppClient);
            }
            catch (Exception)
            {


            }
        }
        public virtual void SendResponse()
        {
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(Packet.GetBytes, m_key);
            try
            {
                AppClient.BeginSend(GetBytes, 0, GetBytes.Length, SocketFlags.None, SendCallback, AppClient);
            }
            catch (Exception)
            {


            }
        }

        
        public void Initialize(IServerBase appServer, Socket client)
        {
            var castedAppServer = (AppServerBase<TAppSession, TPacket>)appServer;
            AppServer = castedAppServer;
            AppClient = client;
            StartTime = DateTime.Now;
            m_Connected = AppClient.Connected;
            RemoteEndPoint = (IPEndPoint)AppClient.RemoteEndPoint;
        }

        public void SendCallback(IAsyncResult result)
        {
            try
            {
                var clientSocket = (Socket)result.AsyncState;
                int bytesSent = clientSocket.EndSend(result);
                Console.WriteLine("Sent " + bytesSent + " bytes to the server.");
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (logging, error messages, etc.)
                Console.WriteLine("An error occurred in the send callback: " + ex.Message);
            }
        }

        public virtual string GetNickname() { return ""; }

        public virtual uint GetUID() { return 0; }
    }
    public abstract class AppSession<TAppSession> : AppSession<TAppSession, Packet>
        where TAppSession : AppSession<TAppSession, Packet>, IAppSession, new()
    {/// <summary>
     /// Sends the specified message.
     /// </summary>
     /// <param name="message">The message.</param>
     /// <returns></returns>
        public override void Send(byte[] message)
        {
            base.Send(message);
        }
    }
    /// <summary>
    /// AppServer basic class for whose request infoe type is
    /// </summary>
    public class AppSession : AppSession<AppSession>
    {
        internal bool Clear()
        {
            return true;        }

        internal void SetOID(uint index)
        {
            m_oid = index;
        }

        internal void SetTimeStartAndTick(int tickCount)
        {
         //   throw new NotImplementedException();
        }
    }
}