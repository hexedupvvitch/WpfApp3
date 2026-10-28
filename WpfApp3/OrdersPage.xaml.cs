using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp3
{
    public partial class OrdersPage : Page
    {
        // Свойство для XAML (роль текущего юзера)
        public int UserRole => Manager.CurrentUser?.role_id ?? 0;

        public OrdersPage()
        {
            InitializeComponent();
            this.DataContext = this; // Чтобы XAML видел UserRole
            LoadOrders();
        }

        private void LoadOrders()
        {
            var context = IshbulatovShoesEntities2.GetContext();
            var orders = context.orders.ToList();

            // ЕСЛИ КЛИЕНТ (role_id == 3) — показываем только его заказы
            if (Manager.CurrentUser != null && Manager.CurrentUser.role_id == 3)
            {
                orders = orders.Where(o => o.user_id == Manager.CurrentUser.user_id).ToList();
            }

            OrdersListView.ItemsSource = orders;
        }

        private void OrdersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersListView.SelectedItem is orders selectedOrder)
            {
                Manager.MainFrame.Navigate(new OrderDetailsPage(selectedOrder));
            }
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as Button).Tag as orders;
            if (MessageBox.Show($"Удалить заказ №{order.order_number}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    var context = IshbulatovShoesEntities2.GetContext();
                    var dbOrder = context.orders.FirstOrDefault(o => o.order_id == order.order_id);
                    if (dbOrder != null)
                    {
                        context.orders.Remove(dbOrder);
                        context.SaveChanges();
                        LoadOrders();
                    }
                }
                catch (System.Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            }
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Редактирование заказа (только для Админа)");
        }
    }
}