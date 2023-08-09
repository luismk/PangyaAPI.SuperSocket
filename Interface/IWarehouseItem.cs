namespace PangyaAPI.SuperSocket.Interface
{
   public interface IWarehouseItem
    {
        uint id { get; set; }
        uint _typeid { get; set; }
        uint ano { get; set; }            // acho que seja só tempo que o item ainda tem
        ushort[] c { get; set; }      // Stats do item ctrl, força etc, se não usa isso o [0] é a quantidade
        byte purchase { get; set; }
        byte flag { get; set; }
        long apply_date { get; set; }
        long end_date { get; set; }
        byte item_type { get; set; }
    }
}