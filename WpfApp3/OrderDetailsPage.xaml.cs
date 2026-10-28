using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp3
{
    public partial class OrderDetailsPage : Page
    {
        // Кнопки видит только Админ (роль 1)
        public bool IsAdmin => Manager.CurrentUser != null && Manager.CurrentUser.role_id == 1;

        private orders _currentOrder;

        public OrderDetailsPage(orders selectedOrder)
        {
            InitializeComponent();
            this.DataContext = this; // ОБЯЗАТЕЛЬНО для работы триггеров в XAML
            _currentOrder = selectedOrder;

            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            TitleText.Text = $"Состав заказа №{_currentOrder.order_number}";

            var context = IshbulatovShoesEntities2.GetContext();

            // Важно: Include подгружает названия товаров и размеры
            var items = context.order_items
                .Include("stock_items.products")
                .Include("stock_items.sizes")
                .Where(oi => oi.order_id == _currentOrder.order_id).ToList();

            OrderItemsListView.ItemsSource = items;

            decimal total = items.Sum(i => i.price * i.quantity);
            TotalSumText.Text = total.ToString("0.00") + " ₽";
        }

        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin) return;
            var item = (sender as Button).Tag as order_items;
            item.quantity++;
            UpdateItemInDb(item);
        }

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin) return;
            var item = (sender as Button).Tag as order_items;
            if (item.quantity > 1)
            {
                item.quantity--;
                UpdateItemInDb(item);
            }
            else
            {
                MessageBox.Show("Количество не может быть меньше 1. Нажмите 'Удалить', чтобы убрать товар.");
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin) return;
            var item = (sender as Button).Tag as order_items;
            if (MessageBox.Show("Удалить этот товар из заказа?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var context = IshbulatovShoesEntities2.GetContext();
                var dbItem = context.order_items.FirstOrDefault(x => x.order_item_id == item.order_item_id);
                if (dbItem != null)
                {
                    context.order_items.Remove(dbItem);
                    context.SaveChanges();
                    LoadOrderDetails(); // Обновляем список и сумму
                }
            }
        }

        private void UpdateItemInDb(order_items item)
        {
            var context = IshbulatovShoesEntities2.GetContext();
            var dbItem = context.order_items.FirstOrDefault(x => x.order_item_id == item.order_item_id);
            if (dbItem != null)
            {
                dbItem.quantity = item.quantity;
                context.SaveChanges();
            }
            LoadOrderDetails(); // Пересчитываем сумму
        }

        private void EditOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вы можете менять количество кнопками + и - прямо в списке, либо удалить позицию.", "Инфо");
        }
    }
}