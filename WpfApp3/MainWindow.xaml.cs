using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Navigated += (s, e) =>
            {
                // 1. Если мы на странице логина — прячем ВСЕ кнопки
                if (e.Content is AuthPage)
                {
                    CartButton.Visibility = Visibility.Collapsed;
                    OrdersButton.Visibility = Visibility.Collapsed;
                    return;
                }

                var user = Manager.CurrentUser;

                // 2. ГОСТЬ (нет юзера или role_id = 0)
                if (user == null || user.role_id == 0)
                {
                    CartButton.Visibility = Visibility.Collapsed;   // Гость не видит корзину
                    OrdersButton.Visibility = Visibility.Collapsed; // Гость не видит заказы
                }
                // 3. КЛИЕНТ (role_id = 1)
                else if (user.role_id == 1)
                {
                    CartButton.Visibility = Visibility.Visible;     // Клиент видит корзину
                    OrdersButton.Visibility = Visibility.Visible;   // Клиент видит свои заказы
                }
                // 4. МЕНЕДЖЕР и АДМИН (role_id = 2 или 3)
                else if (user.role_id == 2 || user.role_id == 3)
                {
                    CartButton.Visibility = Visibility.Visible;     // Видят корзину
                    OrdersButton.Visibility = Visibility.Visible;   // Видят все заказы
                }
            };
            MainFrame.Navigate(new AuthPage());
            Manager.MainFrame = MainFrame;
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void MainFrame_ContentRendered(object sender, EventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                BtnBack.Visibility = Visibility.Visible;
            }
            else
            {
                BtnBack.Visibility = Visibility.Hidden;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Переходим на страницу корзины
            Manager.MainFrame.Navigate(new CartPage());
        }
        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new OrdersPage());
        }
    }
}
