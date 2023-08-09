using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.Interface
{
    public interface ITransactionItem
    {
		uint id { get; set; }
		uint _typeid { get; set; }

		byte type_iff { get; set; }     // Tipo que está no iff structure, tipo no Part.iff, 1 parte de baixo da roupa, 3 luva, 8 e 9 UCC etc
		byte type { get; set; }         // 2 Normal Item
		byte flag { get; set; }         // 1 Padrão item fornecido pelo server, 5 UCC_BLANK
		byte flag_time { get; set; }    // 6 rental(dia), 2 hora(acho), 4 minuto(acho)
		uint qntd { get; set; }

		string Name { get; set; }
		string Icon { get; set; }
	}
}
