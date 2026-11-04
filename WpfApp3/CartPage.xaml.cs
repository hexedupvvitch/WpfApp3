using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static WpfApp3.Manager;

namespace WpfApp3
{
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();

            // Если зашел НЕ менеджер и НЕ админ, прячем выбор клиента
            if (Manager.CurrentUser != null && Manager.CurrentUser.role_id != 2 && Manager.CurrentUser.role_id != 1)
            {
                ClientPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Если менеджер/админ — загружаем список клиентов (роль 3)
                var context = IshbulatovShoesEntities2.GetContext();
                var clients = context.users.Where(u => u.role_id == 3).ToList();
                ClientComboBox.ItemsSource = clients;

                // Выбираем первого по умолчанию
                if (clients.Count > 0) ClientComboBox.SelectedIndex = 0;
            }

            LoadCart();
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.CurrentCart.Count == 0)
            {
                MessageBox.Show("Корзина пуста!");
                return;
            }

            // Определяем, на кого оформляем заказ
            int targetUserId;
            if (Manager.CurrentUser.role_id == 2 || Manager.CurrentUser.role_id == 1)
            {
                // Менеджер/Админ должны выбрать клиента
                if (ClientComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Выберите клиента для оформления заказа!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                targetUserId = (int)ClientComboBox.SelectedValue;
            }
            else
            {
                // Клиент оформляет на себя
                targetUserId = Manager.CurrentUser.user_id;
            }

            try
            {
                var context = IshbulatovShoesEntities2.GetContext();

                // 1. Создаем заказ
                var newOrder = new orders
                {
                    order_number = context.orders.Any() ? context.orders.Max(o => o.order_number) + 1 : 1,
                    order_date = System.DateTime.Now,
                    user_id = targetUserId // Записываем выбранного клиента
                };

                context.orders.Add(newOrder);

                // 2. Переносим товары
                foreach (var item in Manager.CurrentCart)
                {
                    context.order_items.Add(new order_items
                    {
                        orders = newOrder,
                        stock_item_id = item.StockItem.stock_item_id,
                        quantity = item.Quantity,
                        price = item.StockItem.products.price
                    });

                    // Списываем со склада
                    item.StockItem.quantity -= item.Quantity;
                }

                context.SaveChanges();

                MessageBox.Show("Заказ успешно оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                Manager.CurrentCart.Clear();
                LoadCart();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCart()
        {
            CartListView.ItemsSource = null;
            CartListView.ItemsSource = Manager.CurrentCart;

            // Считаем итоговую сумму
            decimal total = Manager.CurrentCart.Sum(c => c.TotalPrice);
            TotalSumText.Text = total.ToString("0.00") + " ₽";
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).Tag as CartItem;
            Manager.CurrentCart.Remove(item);
            LoadCart();
        }
    }
}