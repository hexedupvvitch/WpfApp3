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
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;
        public AuthPage()
        {
            InitializeComponent();
        }
        private void GuestAuthButton_Click(object sender, RoutedEventArgs e)
        {
            LoginTextBox.Text = "";

            
            Manager.MainFrame.Navigate(new ProductPage(null));
        }
        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();

            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            users user = IshbulatovShoesEntities2.GetContext().users
                .FirstOrDefault(p => p.login == LoginTextBox.Text);

            if (user != null)
            {
                LoginTextBox.Text = "";

                // Сохраняем текущего пользователя (пригодится для корзины)
                Manager.CurrentUser = user;

                Manager.MainFrame.Navigate(new ProductPage(user)); // Переход ТОЛЬКО здесь
            }
            else
            {
                failedAttempts++;
                MessageBox.Show("Введены неверные данные!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginTextBox.Text = "";
            }
        }
    }
}
