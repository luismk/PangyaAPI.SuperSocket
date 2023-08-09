using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public class Packet : AppPacketBase
    {
        public Packet()
        {
        }

        public Packet(byte[] message, bool IsClient = false) : base(message, IsClient)
        {
        }

        public Packet(byte key, byte[] message, bool IsClient = false) : base(key, message, IsClient)
        {
        }
    }
}
