using PangyaAPI.SuperSocket.Interface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public abstract partial class AppSession<TAppSession, TPacket> : IAppSession, IAppSession<TAppSession, TPacket>
        where TAppSession : AppSession<TAppSession, TPacket>, IAppSession, new()
        where TPacket : class, IPacket
    {
        public bool Disposed { get; set; }

        public DateTime LastActiveTime { get; set; }

        public DateTime StartTime  { get; set; }
        private bool m_Connected = false;

        /// <summary>
        /// Gets a value indicating whether this <see cref="IAppSession"/> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        public bool Connected
        {
            get { return m_Connected; }
            internal set { m_Connected = value; }
        }
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
        IAppServer IAppSession.AppServer
        {
            get { return this.AppServer; }
        }

        public string GetAdress
        {
            get
            {
                if (Connected)
                {
                    return RemoteEndPoint.Port.ToString()+ ":" + RemoteEndPoint.Address.ToString();
                }
                else
                {
                    return "0.0.0.0:00";
                }
            }
        }

        /// <summary>
        /// Gets the local listening endpoint.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return SocketSession.LocalEndPoint; }
        }
        /// <summary>
        /// Gets the remote endpoint of client.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return SocketSession.RemoteEndPoint; }
        }

        /// <summary>
        /// Gets the socket session of the AppSession.
        /// </summary>
        public ISocketSession SocketSession { get; private set; }

        /// <summary>
        /// Gets the config of the server.
        /// </summary>
        public IServerConfig Config
        {
            get { return AppServer.Config; }
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
        //public void Close()
        //{
        //    if (AppClient != null && AppClient.Connected)
        //    {
        //        try
        //        {
        //            AppClient.Shutdown(SocketShutdown.Both);
        //            AppClient.Disconnect(reuseSocket: false);
        //            if (Packet != null)
        //            {
        //                Packet.Dispose();
        //            }
        //        }
        //        finally
        //        {
        //            Connected = false;
        //            AppClient.Close();
        //        }
        //    }
        //    Dispose();
        //}
        /// <summary>
        /// Closes this session.
        /// </summary>
        public virtual void Close()
        {
            Close(CloseReason.ServerClosing);
        }
        #region Sending processing

        /// <summary>
        /// Try to send the message to client.
        /// </summary>
        /// <param name="message">The message which will be sent.</param>
        /// <returns>Indicate whether the message was pushed into the sending queue</returns>
        public virtual bool TrySend(string message)
        {
            var data = this.Charset.GetBytes(message);
            return InternalTrySend(new ArraySegment<byte>(data, 0, data.Length));
        }

        /// <summary>
        /// Sends the message to client.
        /// </summary>
        /// <param name="message">The message which will be sent.</param>
        public virtual void Send(string message)
        {
            var data = this.Charset.GetBytes(message);
            Send(data, 0, data.Length);
        }

        /// <summary>
        /// Try to send the data to client.
        /// </summary>
        /// <param name="data">The data which will be sent.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>Indicate whether the message was pushed into the sending queue</returns>
        public virtual bool TrySend(byte[] data, int offset, int length)
        {
            return InternalTrySend(new ArraySegment<byte>(data, offset, length));
        }

        /// <summary>
        /// Sends the data to client.
        /// </summary>
        /// <param name="data">The data which will be sent.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public virtual void Send(byte[] data, int offset, int length)
        {
            InternalSend(new ArraySegment<byte>(data, offset, length));
        }

        private bool InternalTrySend(ArraySegment<byte> segment)
        {
            if (!SocketSession.TrySend(segment))
                return false;

            LastActiveTime = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Try to send the data segment to client.
        /// </summary>
        /// <param name="segment">The segment which will be sent.</param>
        /// <returns>Indicate whether the message was pushed into the sending queue</returns>
        public virtual bool TrySend(ArraySegment<byte> segment)
        {
            if (!m_Connected)
                return false;

            return InternalTrySend(segment);
        }


        private void InternalSend(ArraySegment<byte> segment)
        {
            if (!m_Connected)
                return;

            if (InternalTrySend(segment))
                return;

            var sendTimeOut = Config.SendTimeOut;

            //Don't retry, timeout directly
            if (sendTimeOut < 0)
            {
                throw new TimeoutException("The sending attempt timed out");
            }

            var timeOutTime = sendTimeOut > 0 ? DateTime.Now.AddMilliseconds(sendTimeOut) : DateTime.Now;

            var spinWait = new SpinWait();

            while (m_Connected)
            {
                spinWait.SpinOnce();

                if (InternalTrySend(segment))
                    return;

                //If sendTimeOut = 0, don't have timeout check
                if (sendTimeOut > 0 && DateTime.Now >= timeOutTime)
                {
                    throw new TimeoutException("The sending attempt timed out");
                }
            }
        }

        /// <summary>
        /// Sends the data segment to client.
        /// </summary>
        /// <param name="segment">The segment which will be sent.</param>
        public virtual void Send(ArraySegment<byte> segment)
        {
            InternalSend(segment);
        }

        private bool InternalTrySend(IList<ArraySegment<byte>> segments)
        {
            if (!SocketSession.TrySend(segments))
                return false;

            LastActiveTime = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Try to send the data segments to client.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>Indicate whether the message was pushed into the sending queue; if it returns false, the sending queue may be full or the socket is not connected</returns>
        public virtual bool TrySend(IList<ArraySegment<byte>> segments)
        {
            if (!m_Connected)
                return false;

            return InternalTrySend(segments);
        }

        private void InternalSend(IList<ArraySegment<byte>> segments)
        {
            if (!m_Connected)
                return;

            if (InternalTrySend(segments))
                return;

            var sendTimeOut = Config.SendTimeOut;

            //Don't retry, timeout directly
            if (sendTimeOut < 0)
            {
                throw new TimeoutException("The sending attempt timed out");
            }

            var timeOutTime = sendTimeOut > 0 ? DateTime.Now.AddMilliseconds(sendTimeOut) : DateTime.Now;

            var spinWait = new SpinWait();

            while (m_Connected)
            {
                spinWait.SpinOnce();

                if (InternalTrySend(segments))
                    return;

                //If sendTimeOut = 0, don't have timeout check
                if (sendTimeOut > 0 && DateTime.Now >= timeOutTime)
                {
                    throw new TimeoutException("The sending attempt timed out");
                }
            }
        }

        /// <summary>
        /// Sends the data segments to client.
        /// </summary>
        /// <param name="segments">The segments.</param>
        public virtual void Send(IList<ArraySegment<byte>> segments)
        {
            InternalSend(segments);
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message which will be sent.</param>
        /// <param name="paramValues">The parameter values.</param>
        public virtual void Send(string message, params object[] paramValues)
        {
            var data = this.Charset.GetBytes(string.Format(message, paramValues));
            InternalSend(new ArraySegment<byte>(data, 0, data.Length));
        }

        #endregion

        #region Receiving processing

        /// <summary>
        /// Sets the next Receive filter which will be used when next data block received
        /// </summary>
        /// <param name="nextReceiveFilter">The next receive filter.</param>
        protected void SetNextReceiveFilter(IReceiveFilter<TRequestInfo> nextReceiveFilter)
        {
            m_ReceiveFilter = nextReceiveFilter;
        }

        /// <summary>
        /// Gets the maximum allowed length of the request.
        /// </summary>
        /// <returns></returns>
        protected virtual int GetMaxRequestLength()
        {
            return AppServer.Config.MaxRequestLength;
        }

        /// <summary>
        /// Filters the request.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="rest">The rest, the size of the data which has not been processed</param>
        /// <param name="offsetDelta">return offset delta of next receiving buffer.</param>
        /// <returns></returns>
        TRequestInfo FilterRequest(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest, out int offsetDelta)
        {
            if (!AppServer.OnRawDataReceived(this, readBuffer, offset, length))
            {
                rest = 0;
                offsetDelta = 0;
                return null;
            }

            var currentRequestLength = m_ReceiveFilter.LeftBufferSize;

            var requestInfo = m_ReceiveFilter.Filter(readBuffer, offset, length, toBeCopied, out rest);

            if (m_ReceiveFilter.State == FilterState.Error)
            {
                rest = 0;
                offsetDelta = 0;
                Close(CloseReason.ProtocolError);
                return null;
            }

            var offsetAdapter = m_ReceiveFilter as IOffsetAdapter;

            offsetDelta = offsetAdapter != null ? offsetAdapter.OffsetDelta : 0;

            if (requestInfo == null)
            {
                //current buffered length
                currentRequestLength = m_ReceiveFilter.LeftBufferSize;
            }
            else
            {
                //current request length
                currentRequestLength = currentRequestLength + length - rest;
            }

            var maxRequestLength = GetMaxRequestLength();

            if (currentRequestLength >= maxRequestLength)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(this, string.Format("Max request length: {0}, current processed length: {1}", maxRequestLength, currentRequestLength));

                Close(CloseReason.ProtocolError);
                return null;
            }

            //If next Receive filter wasn't set, still use current Receive filter in next round received data processing
            if (m_ReceiveFilter.NextReceiveFilter != null)
                m_ReceiveFilter = m_ReceiveFilter.NextReceiveFilter;

            return requestInfo;
        }

        /// <summary>
        /// Processes the request data.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <returns>
        /// return offset delta of next receiving buffer
        /// </returns>
        int IAppSession.ProcessRequest(byte[] readBuffer, int offset, int length, bool toBeCopied)
        {
            int rest, offsetDelta;

            while (true)
            {
                var requestInfo = FilterRequest(readBuffer, offset, length, toBeCopied, out rest, out offsetDelta);

                if (requestInfo != null)
                {
                    try
                    {
                        AppServer.ExecuteCommand(this, requestInfo);
                    }
                    catch (Exception e)
                    {
                        HandleException(e);
                    }
                }

                if (rest <= 0)
                {
                    return offsetDelta;
                }

                //Still have data has not been processed
                offset = offset + length - rest;
                length = rest;
            }
        }

        #endregion

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

        
        public void Initialize(IAppServer appServer, Socket client)
        {
            var castedAppServer = (AppServerBase<TAppSession, TPacket>)appServer;
            AppServer = castedAppServer;
            AppClient = client;
            StartTime = DateTime.Now;
            Connected = AppClient.Connected;
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

        public int ProcessRequest(byte[] readBuffer, int offset, int length, bool toBeCopied)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Closes the session by the specified reason.
        /// </summary>
        /// <param name="reason">The close reason.</param>
        public virtual void Close(CloseReason reason)
        {
            this.SocketSession.Close(reason);
        }

        public void StartSession()
        {
            throw new NotImplementedException();
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