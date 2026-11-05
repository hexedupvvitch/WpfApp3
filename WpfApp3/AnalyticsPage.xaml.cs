using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace WpfApp3
{
    public partial class AnalyticsPage : Page
    {
        public AnalyticsPage()
        {
            InitializeComponent();

            // Доступ только для Админа (у тебя это role_id == 1)
            if (Manager.CurrentUser == null || Manager.CurrentUser.role_id != 1)
            {
                MessageBox.Show("Доступ только для администратора.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                Manager.MainFrame.Navigate(new ProductPage(Manager.CurrentUser));
                return;
            }

            DateTime now = DateTime.Now;
            DpStart.SelectedDate = new DateTime(now.Year, now.Month, 1);
            DpEnd.SelectedDate = now;

            LoadAnalytics();
        }

        private void Date_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (DpStart.SelectedDate == null || DpEnd.SelectedDate == null) return;
            LoadAnalytics();
        }

        private void LoadAnalytics()
        {
            if (DpStart.SelectedDate == null || DpEnd.SelectedDate == null) return;

            DateTime start = DpStart.SelectedDate.Value.Date;
            DateTime end = DpEnd.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1);

            if (start > end)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var context = IshbulatovShoesEntities2.GetContext();

                // Берем заказы за период
                var orders = context.orders
                    .Include("order_items")
                    .Where(o => o.order_date >= start && o.order_date <= end)
                    .ToList();

                TbTotalOrders.Text = orders.Count.ToString();

                decimal totalSum = orders
                    .SelectMany(o => o.order_items)
                    .Sum(i => (decimal)i.quantity * i.price);

                TbTotalSum.Text = $"{totalSum:N2} руб.";

                // Ищем Топ-5
                var top5 = orders
                    .SelectMany(o => o.order_items)
                    .GroupBy(i => i.stock_item_id)
                    .Select(g => new
                    {
                        StockId = g.Key,
                        TotalQuantity = g.Sum(x => x.quantity)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(5)
                    .ToList();

                var result = new List<TopItemView>();
                int rank = 1;

                foreach (var t in top5)
                {
                    var stock = context.stock_items
                        .Include("products")
                        .Include("products.manufacturers")
                        .FirstOrDefault(s => s.stock_item_id == t.StockId);

                    string productName = stock?.products?.product_name ?? "—";
                    string manufacture = stock?.products?.manufacturers?.manufacturer_name ?? "—";

                    result.Add(new TopItemView
                    {
                        Rank = rank++,
                        ProductName = productName,
                        ManufactureName = manufacture,
                        TotalQuantity = t.TotalQuantity
                    });
                }

                ListViewTop5.ItemsSource = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки аналитики: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            PdfSharp.Fonts.GlobalFontSettings.UseWindowsFontsUnderWindows = true;
            PdfSharp.PdfSharpLicense.SetAgplLicense(); // Обязательно для новых версий PdfSharp

            if (DpStart.SelectedDate == null || DpEnd.SelectedDate == null)
            {
                MessageBox.Show("Выберите период для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF файлы (*.pdf)|*.pdf",
                FileName = $"Отчет_по_продажам_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            try
            {
                var document = new PdfSharp.Pdf.PdfDocument();
                var page = document.AddPage();
                page.Orientation = PdfSharp.PageOrientation.Portrait;

                using (var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page))
                {
                    var fontTitle = new PdfSharp.Drawing.XFont("Arial", 18, PdfSharp.Drawing.XFontStyleEx.Bold);
                    var fontSubtitle = new PdfSharp.Drawing.XFont("Arial", 12, PdfSharp.Drawing.XFontStyleEx.Regular);
                    var fontBold = new PdfSharp.Drawing.XFont("Arial", 11, PdfSharp.Drawing.XFontStyleEx.Bold);
                    var fontRegular = new PdfSharp.Drawing.XFont("Arial", 11, PdfSharp.Drawing.XFontStyleEx.Regular);

                    var brushBlack = PdfSharp.Drawing.XBrushes.Black;
                    var brushGray = new PdfSharp.Drawing.XSolidBrush(PdfSharp.Drawing.XColor.FromArgb(100, 100, 100));

                    double y = 40;
                    double leftMargin = 40;
                    double rightMargin = 40;
                    double pageWidth = page.Width.Point;

                    gfx.DrawString("ОТЧЁТ ПО ПРОДАЖАМ", fontTitle, brushBlack, new PdfSharp.Drawing.XPoint(leftMargin, y));
                    y += 25;
                    gfx.DrawString("Торговая компания «Чудо Обувь»", fontSubtitle, brushGray, new PdfSharp.Drawing.XPoint(leftMargin, y));
                    y += 30;

                    gfx.DrawString($"Период: {DpStart.SelectedDate:dd.MM.yyyy} — {DpEnd.SelectedDate:dd.MM.yyyy}", fontBold, brushBlack, new PdfSharp.Drawing.XPoint(leftMargin, y));
                    y += 30;

                    gfx.DrawString($"Всего заказов: {TbTotalOrders.Text}", fontRegular, brushBlack, new PdfSharp.Drawing.XPoint(leftMargin, y));
                    y += 20;
                    gfx.DrawString($"Общая сумма продаж: {TbTotalSum.Text}", fontRegular, brushBlack, new PdfSharp.Drawing.XPoint(leftMargin, y));
                    y += 40;

                    gfx.DrawString("ТОП-5 МОДЕЛЕЙ ПО КОЛИЧЕСТВУ ПРОДАННЫХ ПАР", fontBold, brushBlack, new PdfSharp.Drawing.XPoint(leftMargin, y));
                    y += 20;

                    double col1 = leftMargin;
                    double col2 = leftMargin + 40;
                    double col3 = leftMargin + 280;
                    double col4 = pageWidth - rightMargin - 60;

                    gfx.DrawRectangle(PdfSharp.Drawing.XBrushes.LightGray, leftMargin, y - 12, pageWidth - leftMargin - rightMargin, 20);
                    gfx.DrawString("№", fontBold, brushBlack, new PdfSharp.Drawing.XPoint(col1, y));
                    gfx.DrawString("Модель", fontBold, brushBlack, new PdfSharp.Drawing.XPoint(col2, y));
                    gfx.DrawString("Производитель", fontBold, brushBlack, new PdfSharp.Drawing.XPoint(col3, y));
                    gfx.DrawString("Кол-во", fontBold, brushBlack, new PdfSharp.Drawing.XPoint(col4, y));
                    y += 25;

                    var topItems = ListViewTop5.ItemsSource as List<TopItemView>;
                    if (topItems != null)
                    {
                        foreach (var item in topItems)
                        {
                            gfx.DrawString(item.Rank.ToString(), fontRegular, brushBlack, new PdfSharp.Drawing.XPoint(col1, y));
                            string name = item.ProductName.Length > 35 ? item.ProductName.Substring(0, 32) + "..." : item.ProductName;
                            gfx.DrawString(name, fontRegular, brushBlack, new PdfSharp.Drawing.XPoint(col2, y));
                            string manuf = item.ManufactureName.Length > 25 ? item.ManufactureName.Substring(0, 22) + "..." : item.ManufactureName;
                            gfx.DrawString(manuf, fontRegular, brushBlack, new PdfSharp.Drawing.XPoint(col3, y));
                            gfx.DrawString(item.TotalQuantity.ToString(), fontRegular, brushBlack, new PdfSharp.Drawing.XPoint(col4, y));
                            y += 20;
                        }
                    }

                    y = page.Height.Point - 30;
                    gfx.DrawString($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}", fontSubtitle, brushGray, new PdfSharp.Drawing.XPoint(leftMargin, y));
                }

                document.Save(saveFileDialog.FileName);
                document.Close();

                MessageBox.Show($"Отчёт успешно сохранён!\n\n{saveFileDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{saveFileDialog.FileName}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании PDF: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class TopItemView
    {
        public int Rank { get; set; }
        public string ProductName { get; set; }
        public string ManufactureName { get; set; }
        public int TotalQuantity { get; set; }
    }
}