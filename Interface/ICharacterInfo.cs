using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.Interface
{
    public interface ICharacterInfo
    {
        /// <summary>
        /// identifica o tipo id
        /// </summary>
        uint TypeID { get; set; }
        /// <summary>
        /// indexador do item
        /// </summary>
        uint Index { get; set; }
        /// <summary>
        /// cor do cabelo do character
        /// </summary>
        byte HairColour { get; set; }
        byte Shirts { get; set; }
        /// <summary>
        /// presete ou nao
        /// </summary>
        byte GiftFlag { get; set; }
        /// <summary>
        /// comprado ou nao
        /// </summary>
        byte Purchase { get; set; }
        /// <summary>
        /// identifica os tipos ids
        /// </summary>
        uint[] EquipTypeID { get; set; }
        /// <summary>
        /// indexadores dos tipos ids
        /// </summary>
        uint[] EquipIndex { get; set; }
        byte[] Blank { get; set; }
        uint AuxPart { get; set; }
        uint AuxPart2 { get; set; }
        uint AuxPart3 { get; set; }
        uint AuxPart4 { get; set; }
        uint AuxPart5 { get; set; }
        uint Cutin { get; set; }
        uint Cutin2 { get; set; }
        uint Cutin3 { get; set; }
        uint Cutin4 { get; set; }
        byte Power { get; set; }
        byte Control { get; set; }
        byte Impact { get; set; }
        byte Spin { get; set; }
        byte Curve { get; set; }
        uint MasteryPoint { get; set; }
        uint[] Card_Character { get; set; }				// 4 Slot de card Character
        uint[] Card_Caddie { get; set; }             // 4 Slot de card Caddie
        uint[] Card_NPC { get; set; }
    }
}
