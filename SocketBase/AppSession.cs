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

        public bool Connected  { get; set; }

        public Encoding Charset { get; set; }

        public IDictionary<object, object> Items { get; set; }

        public TcpClient AppClient { get; set; }
        public Packet Packet { get; set; }
        public byte Key { get ; set ; }
        public uint ConnectionId { get ; set ; }
       // public UserInfo UserInfo { get ; set ; }
        public NetworkStream Stream 
        {
        get { return AppClient.GetStream(); }
        }
        NetworkStream IAppSession.Stream { get { return this.Stream; } }

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
        public string Adress
        {
            get
            {
                if (Connected)
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
                if (Connected)
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
                Key = 0;
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
                    AppClient.Client.Shutdown(SocketShutdown.Both);
                    AppClient.Client.Disconnect(reuseSocket: false);
                    if (Packet != null)
                    {
                        Packet.Dispose();
                    }
                }
                finally
                {
                    Connected = false;
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
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(packet.GetBytes, Key);
            try
            {
                Stream.Write(GetBytes, 0, GetBytes.Length);
            }
            catch (Exception)
            {


            }
        }
        
        public virtual void Send(byte[] message)
        {
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(message, Key);
            try
            {
                Stream.Write(GetBytes, 0, GetBytes.Length);
            }
            catch (Exception)
            {


            } 
        }

        public virtual void SendResponse(Packet packet)
        {
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(packet.GetBytes, Key);
            try
            {
                Stream.Write(GetBytes, 0, GetBytes.Length);
            }
            catch (Exception)
            {


            }
        }
        
        public virtual void SendResponse(byte[] message)
        {
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(message, Key);
            try
            {
                Stream.Write(GetBytes, 0, GetBytes.Length);
            }
            catch (Exception)
            {


            }
        }
        public virtual void Send()
        {
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(Packet.GetBytes, Key);
            try
            {
                Stream.Write(GetBytes, 0, GetBytes.Length);
            }
            catch (Exception)
            {


            }
        }
        public virtual void SendResponse()
        {
            var GetBytes = Cryptor.HandlePacket.Pang.ServerEncrypt(Packet.GetBytes, Key);
            try
            {
                Stream.Write(GetBytes, 0, GetBytes.Length);
            }
            catch (Exception)
            {


            }
        }

        public void Initialize(IServerBase<TAppSession, TPacket> appServer, TcpClient client)
        {
            var castedAppServer = (AppServerBase<TAppSession, TPacket>)appServer;
            AppServer = castedAppServer;
            AppClient = client;
            StartTime = DateTime.Now;
            Connected = AppClient.Connected;
            RemoteEndPoint = (IPEndPoint)AppClient.Client.RemoteEndPoint;
        }

        
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

    }
}