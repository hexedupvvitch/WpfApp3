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
            users guestUser = new users
            {
                user_id = 0,
                login = "guest",
                first_name = "Гость",
                last_name = "",
                middle_name = "",
                role_id = 0
            };

            LoginTextBox.Text = "";

            // Переходим на страницу товаров
            Manager.MainFrame.Navigate(new ProductPage(guestUser));
        }
        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Поиск пользователя в БД
            users user = IshbulatovShoesEntities1.GetContext().users
                .FirstOrDefault(p => p.login == LoginTextBox.Text);

            if (user != null)
            {
                // Успешный вход
                LoginTextBox.Text = "";
                Manager.MainFrame.Navigate(new ProductPage(user));
            }
            else
            {
                // Неверный логин или пароль
                failedAttempts++;

                if (failedAttempts == 1)
                {
                    // Первая неудачная попытка — показываем капчу
                    MessageBox.Show("Введены неверные данные!",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                LoginTextBox.Text = "";
            }
        }
    }
}
