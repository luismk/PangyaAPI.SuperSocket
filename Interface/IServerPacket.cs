using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.Interface
{
   public interface IServerPacket
    {
        /// <summary>
        ///id da resposta
        /// </summary>
        ushort PacketID { get; set; }
        /// <summary>
        /// quantidade de item
        /// </summary>
        ushort PacketAmount { get; set; }
        /// <summary>
        /// total de itens
        /// </summary>
        ushort TotalAmount { get; set; }

        IItemDataManager Manager { get; set; }
    }
}
