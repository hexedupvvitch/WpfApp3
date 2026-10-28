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
            LoadCart();
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

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.CurrentCart.Count == 0)
            {
                MessageBox.Show("Корзина пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Manager.CurrentUser == null)
            {
                MessageBox.Show("Оформить заказ может только авторизованный пользователь!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var context = IshbulatovShoesEntities2.GetContext();

                // 1. Создаем сам заказ
                var newOrder = new orders
                {
                    order_number = context.orders.Any() ? context.orders.Max(o => o.order_number) + 1 : 1, // Генерируем новый номер
                    order_date = System.DateTime.Now,
                    user_id = Manager.CurrentUser.user_id
                };

                context.orders.Add(newOrder);

                // 2. Переносим товары из корзины в order_items
                foreach (var item in Manager.CurrentCart)
                {
                    var newOrderItem = new order_items
                    {
                        orders = newOrder, // Привязываем к только что созданному заказу
                        stock_item_id = item.StockItem.stock_item_id,
                        quantity = item.Quantity,
                        price = item.StockItem.products.price
                    };
                    context.order_items.Add(newOrderItem);

                    // (Опционально) Уменьшаем остаток на складе
                    item.StockItem.quantity -= item.Quantity;
                }

                // 3. Сохраняем всё в БД
                context.SaveChanges();

                MessageBox.Show("Заказ успешно оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // 4. Очищаем корзину
                Manager.CurrentCart.Clear();
                LoadCart(); // Обновляем страницу (корзина станет пустой)
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}