using PangyaAPI.SuperSocket.Interface;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Security.Authentication;
using System;
using PangyaAPI.SuperSocket.Ext;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PangyaAPI.SuperSocket.Engine
{
    /// <summary>
    /// Socket Session, all application session should base on this class
    /// </summary>
    internal abstract class SocketSession : ISocketSession, ISessionBase
    {
        private const string m_GeneralErrorMessage = "Unexpected error";

        private const string m_GeneralSocketErrorMessage = "Unexpected socket error: {0}";

        private const string m_CallerInformation = "Caller: {0}, file path: {1}, line number: {2}";

        private const int m_CloseReasonMagic = 256;

        protected readonly object SyncRoot = new object();

        private int m_State = 0;

        private ISmartPool<SendingQueue> m_SendingQueuePool;

        private SendingQueue m_SendingQueue;

        private Socket m_Client;

        public IAppSession AppSession { get; private set; }

        protected bool SyncSend { get; private set; }

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        /// <value>The session ID.</value>
        public uint m_oid { get; set; }

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        /// <value>The session ID.</value>
        public byte m_key { get; set; }

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public IServerConfig Config { get; set; }

        /// <summary>
        /// Occurs when [closed].
        /// </summary>
        public Action<ISocketSession, CloseReason> Closed { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        public Socket Client => this.m_Client;

        protected bool IsInClosingOrClosed => this.m_State >= 16;

        protected bool IsClosed => this.m_State >= 16777216;

        /// <summary>
        /// Gets the local end point.
        /// </summary>
        /// <value>The local end point.</value>
        public virtual IPEndPoint LocalEndPoint { get; protected set; }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>The remote end point.</value>
        public virtual IPEndPoint RemoteEndPoint { get; protected set; }

        public abstract int OrigReceiveOffset { get; }

        private void AddStateFlag(int stateValue)
        {
            this.AddStateFlag(stateValue, notClosing: false);
        }

        private bool AddStateFlag(int stateValue, bool notClosing)
        {
            int state;
            do
            {
                state = this.m_State;
                if (notClosing && state >= 16)
                {
                    return false;
                }
            }
            while (Interlocked.CompareExchange(value: this.m_State | stateValue, location1: ref this.m_State, comparand: state) != state);
            return true;
        }

        private bool TryAddStateFlag(int stateValue)
        {
            int state;
            int num;
            do
            {
                state = this.m_State;
                num = this.m_State | stateValue;
                if (state == num)
                {
                    return false;
                }
            }
            while (Interlocked.CompareExchange(ref this.m_State, num, state) != state);
            return true;
        }

        private void RemoveStateFlag(int stateValue)
        {
            int state;
            do
            {
                state = this.m_State;
            }
            while (Interlocked.CompareExchange(value: this.m_State & ~stateValue, location1: ref this.m_State, comparand: state) != state);
        }

        private bool CheckState(int stateValue)
        {
            return (this.m_State & stateValue) == stateValue;
        }

        public SocketSession(Socket client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            this.m_Client = client;
            this.LocalEndPoint = (IPEndPoint)client.LocalEndPoint;
            this.RemoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
        }

        public SocketSession(uint sessionID)
        {
            this.m_oid = sessionID;
        }

        public virtual void Initialize(IAppSession appSession)
        {
            //IL_003f:
            this.AppSession = appSession;
            this.Config = appSession.Config;
            this.SyncSend = this.Config.SyncSend;
            if (this.m_SendingQueuePool == null)
            {
                this.m_SendingQueuePool = (ISmartPool<SendingQueue>)appSession.AppServer.SendingQueuePool;
            }
            SendingQueue val = default(SendingQueue);
            if (this.m_SendingQueuePool.TryGet(out val))
            {
                this.m_SendingQueue = val;
                val.StartEnqueue();
            }
        }

        /// <summary>
        /// Starts this session.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Says the welcome information when a client connectted.
        /// </summary>
        protected virtual void StartSession()
        {
            this.AppSession.StartSession();
        }

        /// <summary>
        /// Called when [close].
        /// </summary>
        protected virtual void OnClosed(CloseReason reason)
        {
            //IL_006e:
            if (!this.TryAddStateFlag(16777216))
            {
                return;
            }
            while (true)
            {
                SendingQueue sendingQueue;
                sendingQueue = this.m_SendingQueue;
                if (sendingQueue == null)
                {
                    break;
                }
                if (Interlocked.CompareExchange(ref this.m_SendingQueue, null, sendingQueue) == sendingQueue)
                {
                    sendingQueue.Clear();
                    this.m_SendingQueuePool.Push(sendingQueue);
                    break;
                }
            }
            this.Closed?.Invoke((ISocketSession)(object)this, reason);
        }

        /// <summary>
        /// Tries to send array segment.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns></returns>
        public bool TrySend(IList<ArraySegment<byte>> segments)
        {
            if (this.IsClosed)
            {
                return false;
            }
            SendingQueue sendingQueue;
            sendingQueue = this.m_SendingQueue;
            if (sendingQueue == null)
            {
                return false;
            }
            ushort trackID;
            trackID = sendingQueue.TrackID;
            if (!sendingQueue.Enqueue(segments, trackID))
            {
                return false;
            }
            this.StartSend(sendingQueue, trackID, initial: true);
            return true;
        }

        /// <summary>
        /// Tries to send array segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns></returns>
        public bool TrySend(ArraySegment<byte> segment)
        {
            if (this.IsClosed)
            {
                return false;
            }
            SendingQueue sendingQueue;
            sendingQueue = this.m_SendingQueue;
            if (sendingQueue == null)
            {
                return false;
            }
            ushort trackID;
            trackID = sendingQueue.TrackID;
            if (!sendingQueue.Enqueue(segment, trackID))
            {
                return false;
            }
            this.StartSend(sendingQueue, trackID, initial: true);
            return true;
        }

        /// <summary>
        /// Sends in async mode.
        /// </summary>
        /// <param name="queue">The queue.</param>
        protected abstract void SendAsync(SendingQueue queue);

        /// <summary>
        /// Sends in sync mode.
        /// </summary>
        /// <param name="queue">The queue.</param>
        protected abstract void SendSync(SendingQueue queue);

        private void Send(SendingQueue queue)
        {
            if (this.SyncSend)
            {
                this.SendSync(queue);
            }
            else
            {
                this.SendAsync(queue);
            }
        }

        private void StartSend(SendingQueue queue, int sendingTrackID, bool initial)
        {
            if (initial)
            {
                if (!this.TryAddStateFlag(1))
                {
                    return;
                }
                SendingQueue sendingQueue;
                sendingQueue = this.m_SendingQueue;
                if (sendingQueue != queue || sendingTrackID != sendingQueue.TrackID)
                {
                    this.OnSendEnd();
                    return;
                }
            }
            SendingQueue val = default(SendingQueue);
            if (this.IsInClosingOrClosed && this.TryValidateClosedBySocket(out var _))
            {
                this.OnSendEnd(isInClosingOrClosed: true);
            }
            else if (!this.m_SendingQueuePool.TryGet(out val))
            {
                this.OnSendEnd(isInClosingOrClosed: false);
                this.Close((CloseReason)8);
            }
            else if (!object.ReferenceEquals(Interlocked.CompareExchange(ref this.m_SendingQueue, val, queue), queue))
            {
                if (val != null)
                {
                    this.m_SendingQueuePool.Push(val);
                }
                if (this.IsInClosingOrClosed)
                {
                    this.OnSendEnd(isInClosingOrClosed: true);
                    return;
                }
                this.OnSendEnd(isInClosingOrClosed: false);
                this.Close((CloseReason)8);
            }
            else
            {
                val.StartEnqueue();
                queue.StopEnqueue();
                if (queue.Count == 0)
                {
                    this.m_SendingQueuePool.Push(queue);
                    this.OnSendEnd(isInClosingOrClosed: false);
                    this.Close((CloseReason)8);
                }
                else
                {
                    this.Send(queue);
                }
            }
        }

        private void OnSendEnd()
        {
            this.OnSendEnd(this.IsInClosingOrClosed);
        }

        private void OnSendEnd(bool isInClosingOrClosed)
        {
            this.RemoveStateFlag(1);
            if (!isInClosingOrClosed)
            {
                return;
            }
            if (!this.TryValidateClosedBySocket(out var socket))
            {
                SendingQueue sendingQueue;
                sendingQueue = this.m_SendingQueue;
                if (sendingQueue != null && sendingQueue.Count == 0)
                {
                    if (socket != null)
                    {
                        this.InternalClose(socket, this.GetCloseReasonFromState(), setCloseReason: false);
                    }
                    else
                    {
                        this.OnClosed(this.GetCloseReasonFromState());
                    }
                }
            }
            else if (this.ValidateNotInSendingReceiving())
            {
                this.FireCloseEvent();
            }
        }

        protected virtual void OnSendingCompleted(SendingQueue queue)
        {
            queue.Clear();
            this.m_SendingQueuePool.Push(queue);
            SendingQueue sendingQueue;
            sendingQueue = this.m_SendingQueue;
            if (this.IsInClosingOrClosed)
            {
                if (sendingQueue.Count > 0 && !this.TryValidateClosedBySocket(out var _))
                {
                    this.StartSend(sendingQueue, sendingQueue.TrackID, initial: false);
                }
                else
                {
                    this.OnSendEnd(isInClosingOrClosed: true);
                }
            }
            else if (sendingQueue.Count == 0)
            {
                this.OnSendEnd();
                if (sendingQueue.Count > 0)
                {
                    this.StartSend(sendingQueue, sendingQueue.TrackID, initial: true);
                }
            }
            else
            {
                this.StartSend(sendingQueue, sendingQueue.TrackID, initial: false);
            }
        }

        public abstract void ApplySecureProtocol();

        public Stream GetUnderlyStream()
        {
            return new NetworkStream(this.Client);
        }

        protected virtual bool TryValidateClosedBySocket(out Socket socket)
        {
            socket = this.m_Client;
            return socket == null;
        }

        public virtual void Close(CloseReason reason)
        {
            if (this.TryAddStateFlag(16) && !this.TryValidateClosedBySocket(out var socket))
            {
                if (this.CheckState(1))
                {
                    this.AddStateFlag(this.GetCloseReasonValue(reason));
                }
                else if (socket != null)
                {
                    this.InternalClose(socket, reason, setCloseReason: true);
                }
                else
                {
                    this.OnClosed(reason);
                }
            }
        }

        private void InternalClose(Socket client, CloseReason reason, bool setCloseReason)
        {
            if (Interlocked.CompareExchange(ref this.m_Client, null, client) == client)
            {
                if (setCloseReason)
                {
                    this.AddStateFlag(this.GetCloseReasonValue(reason));
                }
                client.SafeCloseClientSocket();
                if (this.ValidateNotInSendingReceiving())
                {
                    this.OnClosed(reason);
                }
            }
        }

        protected void OnSendError(SendingQueue queue, CloseReason closeReason)
        {
            queue.Clear();
            this.m_SendingQueuePool.Push(queue);
            this.OnSendEnd();
            this.ValidateClosed(closeReason);
        }

        protected void OnReceiveTerminated(CloseReason closeReason)
        {
            this.OnReceiveEnded();
            this.ValidateClosed(closeReason);
        }

        protected bool OnReceiveStarted()
        {
            return this.AddStateFlag(2, notClosing: true);
        }

        protected void OnReceiveEnded()
        {
            this.RemoveStateFlag(2);
        }

        /// <summary>
        /// Validates the socket is not in the sending or receiving operation.
        /// </summary>
        /// <returns></returns>
        private bool ValidateNotInSendingReceiving()
        {
            int state;
            state = this.m_State;
            if ((state & -4) == state)
            {
                return true;
            }
            return false;
        }

        private int GetCloseReasonValue(CloseReason reason)
        {
            return (int)reason;
        }

        private CloseReason GetCloseReasonFromState()
        {
            return (CloseReason)m_State;
        }

        private void FireCloseEvent()
        {
            this.OnClosed(this.GetCloseReasonFromState());
        }

        private void ValidateClosed(CloseReason closeReason)
        {   if (this.IsClosed)
            {
                return;
            }
            if (this.CheckState(16))
            {
                if (this.ValidateNotInSendingReceiving())
                {
                    this.FireCloseEvent();
                }
            }
            else
            {
                this.Close(closeReason);
            }
        }

        protected virtual bool IsIgnorableSocketError(int socketErrorCode)
        {
            if (socketErrorCode == 10004 || socketErrorCode == 10053 || socketErrorCode == 10054 || socketErrorCode == 10058 || socketErrorCode == 10060 || socketErrorCode == 995 || socketErrorCode == -1073741299)
            {
                return true;
            }
            return false;
        }

        protected virtual bool IsIgnorableException(Exception e, out int socketErrorCode)
        {
            socketErrorCode = 0;
            if (e is ObjectDisposedException || e is NullReferenceException)
            {
                return true;
            }
            SocketException ex;
            if (e is IOException)
            {
                if (e.InnerException is ObjectDisposedException || e.InnerException is NullReferenceException)
                {
                    return true;
                }
                ex = e.InnerException as SocketException;
            }
            else
            {
                ex = e as SocketException;
            }
            if (ex == null)
            {
                return false;
            }
            socketErrorCode = ex.ErrorCode;
            //if (this.Config.LogAllSocketException())
            //{
            //    return false;
            //}
            return this.IsIgnorableSocketError(socketErrorCode);
        }
    }
}