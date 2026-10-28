using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfApp3
{
    internal class Manager
    {
        public static Frame MainFrame { get; set; }

        public static users CurrentUser { get; set; }

        public static List<CartItem> CurrentCart = new List<CartItem>();

        public class CartItem
        {
            public stock_items StockItem { get; set; } // <--- Вот оно! Должно называться именно StockItem
            public int Quantity { get; set; } = 1;

            public decimal TotalPrice => StockItem.products.price * Quantity;
        }
    }
}
