using PangyaAPI.SuperSocket.Engine;
using PangyaAPI.SuperSocket.Interface;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Web;
using System;
using System.Linq;
using _smp = PangyaAPI.Utilities.Log;
namespace PangyaAPI.SuperSocket.Engine
{
    internal class AsyncSocketSession : SocketSession, IAsyncSocketSession, IAsyncSocketSessionBase
    {
        private bool m_IsReset;

        private SocketAsyncEventArgs m_SocketEventArgSend;

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; private set; }

        public override int OrigReceiveOffset => this.SocketAsyncProxy.OrigOffset;

        public AsyncSocketSession(Socket client, SocketAsyncEventArgsProxy socketAsyncProxy)
            : this(client, socketAsyncProxy, isReset: false)
        {
        }

        public AsyncSocketSession(Socket client, SocketAsyncEventArgsProxy socketAsyncProxy, bool isReset)
            : base(client)
        {
            this.SocketAsyncProxy = socketAsyncProxy;
            this.m_IsReset = isReset;
        }

        public override void Initialize(IAppSession appSession)
        {
            base.Initialize(appSession);
            this.SocketAsyncProxy.Initialize(this);
            if (!base.SyncSend)
            {
                this.m_SocketEventArgSend = new SocketAsyncEventArgs();
                this.m_SocketEventArgSend.Completed += OnSendingCompleted;
            }
        }

        public override void Start()
        {
            this.StartReceive(this.SocketAsyncProxy.SocketEventArgs);
            if (!this.m_IsReset)
            {
                this.StartSession();
            }
        }

        private bool ProcessCompleted(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    return true;
                }
            }
            else
            {
               _smp.Message_Pool.push((int)e.SocketError, "ProcessCompleted", 76);
            }
            return false;
        }

        private void OnSendingCompleted(object sender, SocketAsyncEventArgs e)
        {
            object userToken;
            userToken = e.UserToken;
            SendingQueue val;
            val = (SendingQueue)((userToken is SendingQueue) ? userToken : null);
            if (!this.ProcessCompleted(e))
            {
                this.ClearPrevSendState(e);
                base.OnSendError(val, (CloseReason)5);
                return;
            }
            int num;
            num = val.Sum((ArraySegment<byte> q) => q.Count);
            if (num != e.BytesTransferred)
            {
                val.InternalTrim(e.BytesTransferred);
               _smp.Message_Pool.push(string.Format("{0} of {1} were transferred, send the rest {2} bytes right now.", (object)e.BytesTransferred, (object)num, (object)((IEnumerable<ArraySegment<byte>>)val).Sum((ArraySegment<byte> q) => q.Count)));
                this.ClearPrevSendState(e);
                this.SendAsync(val);
            }
            else
            {
                this.ClearPrevSendState(e);
                base.OnSendingCompleted(val);
            }
        }

        private void ClearPrevSendState(SocketAsyncEventArgs e)
        {
            e.UserToken = null;
            if (e.Buffer != null)
            {
                e.SetBuffer(null, 0, 0);
            }
            else if (e.BufferList != null)
            {
                e.BufferList = null;
            }
        }

        private void StartReceive(SocketAsyncEventArgs e)
        {
            this.StartReceive(e, 0);
        }

        private void StartReceive(SocketAsyncEventArgs e, int offsetDelta)
        {
            bool flag;
            try
            {
                if (offsetDelta < 0 || offsetDelta >= base.Config.ReceiveBufferSize)
                {
                    throw new ArgumentException($"Illigal offsetDelta: {offsetDelta}", "offsetDelta");
                }
                int num;
                num = this.SocketAsyncProxy.OrigOffset + offsetDelta;
                if (e.Offset != num)
                {
                    e.SetBuffer(num, base.Config.ReceiveBufferSize - offsetDelta);
                }
                if (!base.OnReceiveStarted())
                {
                    return;
                }
                
                flag = base.m_Socket.ReceiveAsync(e);
            }
            catch (Exception exception)
            {
                _smp.Message_Pool.push("StartReceive", exception);
                base.OnReceiveTerminated((CloseReason)5);
                return;
            }
            if (!flag)
            {
                this.ProcessReceive(e);
            }
        }

        protected override void SendSync(SendingQueue queue)
        {
            try
            {
                for (int i = 0; i < queue.Count(); i++)
                {
                    ArraySegment<byte> arraySegment;
                    arraySegment = queue[i];
                    Socket client = m_Socket;
                    if (client == null || !client.Connected)
                    {
                        return;
                    }
                    client.Send(arraySegment.Array, arraySegment.Offset, arraySegment.Count, SocketFlags.None);
                }
                this.OnSendingCompleted(queue);
            }
            catch (Exception exception)
            {
                _smp.Message_Pool.push("SendSync", exception);
                base.OnSendError(queue, (CloseReason)5);
            }
        }

        protected override void SendAsync(SendingQueue queue)
        {
            try
            {
                this.m_SocketEventArgSend.UserToken = queue;
                if (queue.Count() > 1)
                {
                    this.m_SocketEventArgSend.BufferList = queue;
                }
                else
                {
                    ArraySegment<byte> arraySegment;
                    arraySegment = queue[0];
                    this.m_SocketEventArgSend.SetBuffer(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
                }
                Socket client;
                client = base.m_Socket;
                if (client == null)
                {
                    base.OnSendError(queue, (CloseReason)5);
                }
                else if (!client.SendAsync(this.m_SocketEventArgSend))
                {
                    this.OnSendingCompleted(client, this.m_SocketEventArgSend);
                }
            }
            catch (Exception exception)
            {
                _smp.Message_Pool.push("SendAsync", exception);
                this.ClearPrevSendState(this.m_SocketEventArgSend);
                base.OnSendError(queue, (CloseReason)5);
            }
        }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (!this.ProcessCompleted(e))
            {
                base.OnReceiveTerminated((CloseReason)((e.SocketError == SocketError.Success) ? 2 : 5));
                return;
            }
            base.OnReceiveEnded();
            int offsetDelta =0;
            try
            {
                if (e.BytesTransferred > 0)
                {
                    offsetDelta = base.AppSession.ProcessRequest(e.Buffer, e.Offset, e.BytesTransferred, true);
                }
                else
                {
                    Console.WriteLine("No response received from client within the timeout. Disconnecting.");
                    this.Close((CloseReason)7);
                }
            }
            catch (Exception exception)
            {
                _smp.Message_Pool.push("ProtocolError", exception, "ProcessReceive", 244);
                this.Close((CloseReason)7);
                return;
            }
            this.StartReceive(e, offsetDelta);
        }

        protected override void OnClosed(CloseReason reason)
        {
            SocketAsyncEventArgs socketEventArgSend;
            socketEventArgSend = this.m_SocketEventArgSend;
            if (socketEventArgSend == null)
            {
                base.OnClosed(reason);
            }
            else if (Interlocked.CompareExchange(ref this.m_SocketEventArgSend, null, socketEventArgSend) == socketEventArgSend)
            {
                socketEventArgSend.Dispose();
                base.OnClosed(reason);
            }
        }

    }
}