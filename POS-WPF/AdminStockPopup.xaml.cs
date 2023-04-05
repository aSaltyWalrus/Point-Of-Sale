using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS_WPF
{
    /// <summary>
    /// Interaction logic for AdminStockPopup.xaml
    /// </summary>
    public partial class AdminStockPopup : Window
    {
        private ToggleButton stockItemSelected = null;
        private List<ComboBoxItem> stockCatItems = new List<ComboBoxItem>();
        private Category AdminCurrCat;
        private int AdminCatNum = 0;
        private Database db;
        private List<Category> categories;

        public AdminStockPopup(Database db, ref List<Category> categories, List<ComboBoxItem> stockCatItems)
        {
            InitializeComponent();

            this.categories = categories;
            this.db = db;
            if(this.categories.Count > 0)
            {
                AdminCurrCat = categories[0];
            }
            this.stockCatItems = stockCatItems;
            StockCat.ItemsSource = stockCatItems;
            if (stockCatItems.Count > 0)
            {
                StockCat.SelectedIndex = 0;
            }
        }

        public void LoadAdminItems()
        {
            StockAdminItems.Children.Clear();

            for (int i = 0; i < AdminCurrCat.Items.Count; i++)
            {
                Item temp = AdminCurrCat.Items[i];
                ToggleButton b = new ToggleButton()
                {
                    Name = "St" + (i + 1),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                b.Click += SelectItemFromList;

                Grid g = new Grid();
                ColumnDefinition c1 = new ColumnDefinition()
                {
                    Width = new GridLength(80)
                };

                ColumnDefinition c2 = new ColumnDefinition()
                {
                    Width = new GridLength(270)
                };

                g.ColumnDefinitions.Add(c1);
                g.ColumnDefinitions.Add(c2);

                TextBlock t1 = new TextBlock()
                {
                    Text = temp.ID.ToString(),
                    FontSize = 26,
                };

                TextBlock t2 = new TextBlock()
                {
                    Text = temp.Name,
                    FontSize = 26
                };

                Grid.SetColumn(t1, 0);
                Grid.SetColumn(t2, 1);

                g.Children.Add(t1);
                g.Children.Add(t2);

                b.Content = g;
                StockAdminItems.Children.Add(b);
            }
        }

        public void SelectItemFromList(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;
            int num = int.Parse(b.Name.Substring(2)) - 1;
            Item theItem = AdminCurrCat.Items[num];
            db.GetStockFor(ref theItem);

            if (stockItemSelected != null && stockItemSelected.Equals(b) == false)
            {
                stockItemSelected.IsChecked = false;
            }
            stockItemSelected = b;
            stockItemSelected.IsChecked = true;
            StockIDBox.Text = theItem.ID.ToString();
            StockNameBox.Text = theItem.Name.ToString();
            StockAmountBox.Text = theItem.Stock.ToString();
        }

        private void StockCategoryChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StockCat.SelectedIndex >= 0)
            {
                AdminCatNum = StockCat.SelectedIndex;
                AdminCurrCat = categories[AdminCatNum];
                LoadAdminItems();

                stockItemSelected = null;
                StockIDBox.Text = "";
                StockNameBox.Text = "";
                StockAmountBox.Text = "";
            }
        }

        private void ChangeStockAmount(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            int amount = int.Parse(StockChangeBox.Text);

            if (b.Name.ToLower().Equals("removestock"))
            {
                amount = -amount;
            }

            Item theItem = findItem(int.Parse(StockIDBox.Text));
            if (theItem != null)
            {
                db.ChangeStock(ref theItem, amount);
                StockAmountBox.Text = theItem.Stock.ToString();
            }

            StockChangeBox.Text = "";

        }

        private void CheckIsDigit(Object sender, TextChangedEventArgs args)
        {
            TextBox textBox = (TextBox)sender;
            bool valid = true;
            string input = textBox.Text;
            int index = input.Length - 1;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (Char.IsDigit(c) == false)
                {
                    index = i;
                    valid = false;
                    break;
                }
            }

            if (valid == false)
            {
                textBox.Text = input.Remove(index, 1);
            }
        }

        private Item findItem(int IdNum)
        {
            foreach (Item item in AdminCurrCat.Items)
            {
                if (item.ID == IdNum)
                {
                    return item;
                }
            }
            return null;
        }

    }
}
