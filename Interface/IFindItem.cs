using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// interface responsavel por identificar o item
    /// </summary>
    public interface IFindItem
    {
        /// <summary>
        /// indexador do item ou item id
        /// </summary>
        uint Index { get; set; }
        /// <summary>
        /// tipo id do item
        /// </summary>
        uint TypeID { get; set; }
    }
}
