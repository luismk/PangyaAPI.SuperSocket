using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// contem toda informações sobre o caddie do player, como id, tipo, skin usada, tempo de termino e etc
    /// </summary>
    public interface ICaddieInfo
    {
        /// <summary>
      /// indexador do item ou item id
      /// </summary>
        uint Index { get; set; }
        /// <summary>
        /// tipo id do item
        /// </summary>
        uint TypeID { get; set; }
        /// <summary>
        /// skin tipo id 
        /// </summary>
        uint Skin_TypeID { get; set; }
        /// <summary>
        /// nivel do caddie
        /// </summary>
        byte Level { get; set; }
        /// <summary>
        /// quantidade de xp do cadadie
        /// </summary>
        uint Exp { get; set; }
        /// <summary>
        /// tipo, se e pang ou cookies
        /// </summary>
        byte Type { get; set; }
        /// <summary>
        /// quantidade de dias restantes para expirar
        /// </summary>
        UInt16 Day { get; set; }
        /// <summary>
        /// quantidade de dias restantes para expirar a skin
        /// </summary>
        UInt16 SkinDay { get; set; }
        /// <summary>
        /// bit desconhecido ou purchase
        /// </summary>
        Byte Purchase { get; set; }

       /// <summary>
       /// pagamento automatico ou pagamento manualmente
       /// </summary>
        UInt16 AutoPay { get; set; }
    }
}
