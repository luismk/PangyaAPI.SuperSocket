using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.Ext
{
    public static class SocketEx
    {
        public static void SafeClose(this Socket client)
        {
            if (client == null)
                return;

            if (!client.Connected)
                return;

            try
            {
                client.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                //if (logger != null)
                //    logger.LogError(e);
            }

            try
            {
                client.Close();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                //if (logger != null)
                //    logger.LogError(e);
            }
        }

        public static void SendData(this Socket client, byte[] data)
        {
            SendData(client, data, 0, data.Length);
        }

        public static void SendData(this Socket client, byte[] data, int offset, int length)
        {
            int sent = 0;
            int thisSent = 0;

            while ((length - sent) > 0)
            {
                thisSent = client.Send(data, offset + sent, length - sent, SocketFlags.None);
                sent += thisSent;
            }
        }
    }
}
