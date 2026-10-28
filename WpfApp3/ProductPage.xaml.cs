using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static WpfApp3.Manager;

namespace WpfApp3
{
    public partial class ProductPage : Page
    {
        // Свойство для скрытия кнопок в XAML
        public bool IsAuthorized => Manager.CurrentUser != null && Manager.CurrentUser.role_id != 0;

        public ProductPage(users users)
        {
            InitializeComponent();
            this.DataContext = this;

            Manager.CurrentUser = users;

            if (users == null || users.role_id == 0)
            {
                TBoxSearch.Visibility = Visibility.Collapsed;
                ComboSort.Visibility = Visibility.Collapsed;

                FIOTB.Text = "Гость";
                RoleTB.Text = "Неавторизованный";
            }
            else
            {
                FIOTB.Text = $"{users.last_name} {users.first_name} {users.middle_name}";
                switch (users.role_id)
                {
                    case 3: RoleTB.Text = "Клиент"; break;
                    case 2: RoleTB.Text = "Менеджер"; break;
                    case 1: RoleTB.Text = "Администратор"; break;
                    default: RoleTB.Text = "Гость"; break;
                }
            }

            ComboSort.SelectedIndex = 0;
            UpdateProducts();
        }

        private void UpdateProducts()
        {
            var context = IshbulatovShoesEntities2.GetContext();

            // Загружаем товары вместе с размерами (Include)
            var currentProducts = context.products.Include("stock_items").ToList();

            // Поиск (проверяем, не пустой ли текст)
            if (!string.IsNullOrEmpty(TBoxSearch.Text))
            {
                currentProducts = currentProducts.Where(p => p.product_name.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();
            }

            // Сортировка
            if (ComboSort.SelectedIndex == 1)
                currentProducts = currentProducts.OrderBy(p => p.product_name).ToList();
            if (ComboSort.SelectedIndex == 2)
                currentProducts = currentProducts.OrderByDescending(p => p.product_name).ToList();

            ProductListView.ItemsSource = currentProducts;
            TableList = currentProducts;
            ChangePage(0, 0);
        }

        int CountRecords, CountPage, CurrentPage = 0;
        const int RecordsPage = 10;
        List<products> CurrentPageList = new List<products>();
        List<products> TableList;

        public void ChangePage(int direction, int? selectedPage)
        {
            if (TableList == null) return;

            CurrentPageList.Clear();
            CountRecords = TableList.Count;
            CountPage = (CountRecords + RecordsPage - 1) / RecordsPage;

            if (selectedPage.HasValue && selectedPage >= 0 && selectedPage < CountPage)
                CurrentPage = selectedPage.Value;
            else if (direction == 1 && CurrentPage > 0)
                CurrentPage--;
            else if (direction == 2 && CurrentPage < CountPage - 1)
                CurrentPage++;
            else
                return;

            int start = CurrentPage * RecordsPage;
            int end = Math.Min(start + RecordsPage, CountRecords);
            for (int i = start; i < end; i++)
                CurrentPageList.Add(TableList[i]);

            PageListBox.Items.Clear();
            for (int i = 1; i <= CountPage; i++)
                PageListBox.Items.Add(i);
            PageListBox.SelectedIndex = CurrentPage;
            ProductListView.ItemsSource = CurrentPageList;
            ProductListView.Items.Refresh();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateProducts();
        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateProducts();

        private void RightDirButton_Click(object sender, RoutedEventArgs e) => ChangePage(2, null);
        private void LeftDirButton_Click(object sender, RoutedEventArgs e) => ChangePage(1, null);

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PageListBox.SelectedItem != null)
                ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button.Tag as products;

            var parent = button.Parent as Grid;
            var sizeCombo = parent.Children.OfType<ComboBox>().FirstOrDefault(c => c.Name == "SizeComboBox");

            if (sizeCombo == null || sizeCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите размер!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedStock = sizeCombo.SelectedItem as stock_items;

            var existingItem = Manager.CurrentCart.FirstOrDefault(c => c.StockItem.stock_item_id == selectedStock.stock_item_id);

            if (existingItem != null)
            {
                if (existingItem.Quantity < selectedStock.quantity)
                    existingItem.Quantity++;
                else
                    MessageBox.Show($"Больше нет на складе! Максимум: {selectedStock.quantity}", "Стоп", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                Manager.CurrentCart.Add(new CartItem { StockItem = selectedStock, Quantity = 1 });
                MessageBox.Show($"Добавлено: {product.product_name}, Размер: {selectedStock.Size}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}