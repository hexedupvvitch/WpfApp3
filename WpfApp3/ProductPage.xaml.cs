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
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();
            var currentProducts = IshbulatovShoesEntities.GetContext().products.ToList();
            ProductListView.ItemsSource = currentProducts;

            ComboSort.SelectedIndex = 0;

            UpdateProducts();
        }
    private void UpdateProducts()
        {
            var currentProducts = IshbulatovShoesEntities.GetContext().products.ToList();

            string NormalniyProduct(string product_name) //с помощью replace заменяем пробелы на ничего получается простоо убираем их
            {
                return product_name.Replace(" ", "");
            }

            //приводим название к нижнему регистру и код просматривает есть ли буква в адресе почты или названии
            currentProducts = currentProducts.Where(p => p.product_name.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList(); //Tbox это само поле ввода, а text это переменная которая хранит то что написано внутри

            ProductListView.ItemsSource = currentProducts.ToList();

            if (ComboSort.SelectedIndex == 1)
                currentProducts = currentProducts.OrderBy(p => p.product_name).ToList(); //сортировка по возрастанию
            if (ComboSort.SelectedIndex == 2)
                currentProducts = currentProducts.OrderByDescending(p => p.product_name).ToList(); //сортировка по убыванию

            ProductListView.ItemsSource = currentProducts;
            TableList = currentProducts;
            ChangePage(0, 0);
        }

        int CountRecords, CountPage, CurrentPage = 0;
        const int RecordsPage = 10; //количество записей на странице всегда 10
        List<products> CurrentPageList = new List<products>();
        List<products> TableList;
        public void ChangePage(int direction, int? selectedPage)
        {
            CurrentPageList.Clear();
            CountRecords = TableList.Count; //записываем сколько всего записей в таблице
            CountPage = (CountRecords + RecordsPage - 1) / RecordsPage; // вычисляем сколько всего страниц получится

            if (selectedPage.HasValue && selectedPage >= 0 && selectedPage < CountPage)
                CurrentPage = selectedPage.Value; //проверка на выбор существующей страницы или номер страницы не отрицательный, если проверка удачная, переходим на выбранную страницу
            else if (direction == 1 && CurrentPage > 0)
                CurrentPage--; //иначе есил нажали влево и текущая страница не первая, то номер страницы уменьшается на 1
            else if (direction == 2 && CurrentPage < CountPage - 1)
                CurrentPage++; //иначе есил нажали вправо и текущая страница не последняя, то номер страницы увеличивается на 1
            else
                return;//если ни одно условие не подошло, то ничего не делается

            int start = CurrentPage * RecordsPage; //вычисляется индекс первого агента на странице
            int end = Math.Min(start + RecordsPage, CountRecords);//берем наименьшее из start+recordspage или countrecords чтобы не выйти за границы списка
            for (int i = start; i < end; i++)
                CurrentPageList.Add(TableList[i]);//берем агента из списка и добавляем его в текущую страницу

            PageListBox.Items.Clear();//очистка списка номеров страниц
            for (int i = 1; i <= CountPage; i++)
                PageListBox.Items.Add(i);//добавляем номер страницы в список
            PageListBox.SelectedIndex = CurrentPage;
            ProductListView.ItemsSource = CurrentPageList;
            ProductListView.Items.Refresh();//перезагрузка списка чтобы видеть изменения
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void AgentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1); //convert берет стринговую цифру и превращает ее в инт, отнимает 1 потому что компьютер начинает отсчет не с 1 а с 0
        }
    }
}
