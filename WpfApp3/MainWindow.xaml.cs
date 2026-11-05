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
                if (e.Content is AuthPage)
                {
                    AnalyticsButton.Visibility = Visibility.Collapsed;
                    CartButton.Visibility = Visibility.Collapsed;
                    OrdersButton.Visibility = Visibility.Collapsed;
                    return;
                }

                var user = Manager.CurrentUser;

                if (user == null || user.role_id == 0) // Гость
                {
                    AnalyticsButton.Visibility = Visibility.Collapsed;
                    CartButton.Visibility = Visibility.Collapsed;
                    OrdersButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CartButton.Visibility = Visibility.Visible;
                    OrdersButton.Visibility = Visibility.Visible;

                    // АНАЛИТИКУ ВИДИТ ТОЛЬКО АДМИН (role_id == 1)
                    if (user.role_id == 1)
                        AnalyticsButton.Visibility = Visibility.Visible;
                    else
                        AnalyticsButton.Visibility = Visibility.Collapsed;
                }
            };
            MainFrame.Navigate(new AuthPage());
            Manager.MainFrame = MainFrame;
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.CurrentCart.Clear(); // Очищаем корзину старого пользователя
            Manager.MainFrame.GoBack();

        }
        private void AnalyticsButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AnalyticsPage());
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
