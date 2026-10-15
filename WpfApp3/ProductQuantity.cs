using System.Linq;

namespace WpfApp3
{
    public partial class products
    {
        public int TotalQuantity => stock_items?.Sum(s => s.quantity) ?? 0;
    }
}