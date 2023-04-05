using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for AdminItemPopup.xaml
    /// </summary>
    public partial class AdminItemPopup : Window
    {
        private ToggleButton itemsItemSelected = null;
        private List<ComboBoxItem> itemCatItems = new List<ComboBoxItem>();
        private List<ComboBoxItem> theItemCategories = new List<ComboBoxItem>();
        private Category AdminCurrCat;
        private int AdminCatNum = 0;
        private Database db;
        private List<Category> categories;

        public AdminItemPopup(Database db, ref List<Category> categories, List<ComboBoxItem> itemCatItems)
        {
            InitializeComponent();

            this.categories = categories;
            this.db = db;
            if (this.categories.Count > 0)
            {
                AdminCurrCat = categories[0];
            }
            this.itemCatItems = itemCatItems;

            foreach (Category cat in categories)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = cat.Name;

                theItemCategories.Add(comboBoxItem);
            }

            ItemCat.ItemsSource = this.itemCatItems;
            ItemCatNumBox.ItemsSource = theItemCategories;
            if (itemCatItems.Count > 0)
            {
                ItemCat.SelectedIndex = 0;
            }

            LoadAdminItems();
            ItemOption_Click(ItemOption1, null);
        }

        public void LoadAdminItems()
        {
            ItemAdminItems.Children.Clear();

            for (int i = 0; i < AdminCurrCat.Items.Count; i++)
            {
                Item temp = AdminCurrCat.Items[i];
                ToggleButton b = new ToggleButton()
                {
                    Name = "It" + (i + 1),
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
                ItemAdminItems.Children.Add(b);
            }
        }

        public void SelectItemFromList(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;
            int num = int.Parse(b.Name.Substring(2)) - 1;
            Item theItem = AdminCurrCat.Items[num];
            db.GetItemInfo(ref theItem);

            if (itemsItemSelected != null && itemsItemSelected.Equals(b) == false)
            {
                itemsItemSelected.IsChecked = false;
            }
            itemsItemSelected = b;
            itemsItemSelected.IsChecked = true;
            ItemIDBox.Text = theItem.ID.ToString();
            ItemNameBox.Text = theItem.Name.ToString();
            ItemStockBox.Text = theItem.Stock.ToString();
            ItemPriceBox.Text = theItem.Price.ToString();

            string[] split = theItem.Price.ToString().Split('.');
            if (split.Length == 2)
            {
                if (split[1].Length == 1)
                {
                    ItemPriceBox.Text += "0";
                }
            }
            else
            {
                ItemPriceBox.Text += ".00";
            }

            ItemCatNumBox.SelectedIndex = AdminCatNum;
            ItemOption_Click(ItemOption3, null);
        }

        private void ItemCategoryChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemCat.SelectedIndex >= 0)
            {
                AdminCatNum = ItemCat.SelectedIndex;
                AdminCurrCat = categories[AdminCatNum];
                LoadAdminItems();

                ItemOption_Click(ItemOption1, null);
            }
        }

        private void ItemOption_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = (ToggleButton)sender;

            if (tb != null)
            {
                switch (tb.Name.ToLower())
                {
                    case "itemoption1":
                        if (itemsItemSelected != null)
                        {
                            itemsItemSelected.IsChecked = false;
                            itemsItemSelected = null;
                        }
                        ItemNameBox.IsEnabled = true;
                        ItemStockBox.IsEnabled = true;
                        ItemPriceBox.IsEnabled = true;
                        ItemCatNumBox.IsEnabled = true;
                        ItemIDBox.Text = "";
                        ItemNameBox.Text = "";
                        ItemStockBox.Text = "";
                        ItemPriceBox.Text = "";
                        ItemCatNumBox.SelectedIndex = -1;

                        ItemOption1.IsChecked = true;
                        ItemOption2.IsChecked = false;
                        ItemOption3.IsChecked = false;
                        break;
                    case "itemoption2":
                        if (itemsItemSelected != null)
                        {
                            ItemNameBox.IsEnabled = false;
                            ItemStockBox.IsEnabled = false;
                            ItemPriceBox.IsEnabled = false;
                            ItemCatNumBox.IsEnabled = false;

                            ItemOption1.IsChecked = false;
                            ItemOption2.IsChecked = true;
                            ItemOption3.IsChecked = false;
                        }
                        else
                        {
                            ItemOption_Click(ItemOption1, null);
                        }
                        break;
                    case "itemoption3":
                        if (itemsItemSelected != null)
                        {
                            ItemNameBox.IsEnabled = true;
                            ItemStockBox.IsEnabled = false;
                            ItemPriceBox.IsEnabled = true;
                            ItemCatNumBox.IsEnabled = true;

                            ItemOption1.IsChecked = false;
                            ItemOption2.IsChecked = false;
                            ItemOption3.IsChecked = true;
                        }
                        else
                        {
                            ItemOption_Click(ItemOption1, null);
                        }
                        break;
                }

            }
        }

        private void ClearItemFields(object sender, RoutedEventArgs e)
        {
            ItemOption_Click(ItemOption1, null);
        }

        private void ItemSubmitButtonClick(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            string itemName = ItemNameBox.Text;
            int stock = -1;
            double price = -1;
            Category category = null;

            if (itemName.Equals(""))
            {
                valid = false;
            }

            if (ItemStockBox.Text.Equals(""))
            {
                valid = false;
            }
            else
            {
                stock = int.Parse(ItemStockBox.Text);
            }

            if (ItemPriceBox.Text.Equals(""))
            {
                valid = false;
            }
            else
            {
                price = double.Parse(ItemPriceBox.Text);
            }

            if (ItemCatNumBox.SelectedIndex == -1)
            {
                valid = false;
            }
            else
            {
                category = categories[ItemCatNumBox.SelectedIndex];
            }

            if (ItemOption1.IsChecked == true)
            {
                if (valid)
                {
                    db.AddItem(itemName, stock, price, ref category);
                    if (AdminCurrCat.Equals(category))
                    {
                        ItemCategoryChanged(null, null);
                    }
                }

            }
            else if (ItemOption2.IsChecked == true)
            {
                if (valid)
                {
                    int id = int.Parse(ItemIDBox.Text);
                    Item theItem = findItem(id);
                    if (db.RemoveItem(id))
                    {
                        category.removeItem(theItem);
                        if (AdminCurrCat.Equals(category))
                        {
                            ItemCategoryChanged(null, null);
                        }
                    }
                }
            }
            else if (ItemOption3.IsChecked == true)
            {
                if (valid)
                {
                    int id = int.Parse(ItemIDBox.Text);
                    Item theItem = findItem(id);
                    if (theItem != null)
                    {
                        if (db.UpdateItem(ref theItem, itemName, price, category))
                        {
                            ItemCategoryChanged(null, null);
                        }
                    }
                }
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

        private void CheckIsDouble(Object sender, TextChangedEventArgs args)
        {
            TextBox textBox = (TextBox)sender;
            bool valid = true;
            string input = textBox.Text;
            Regex regex = new Regex(@"^(\d+\.?\d?\d?)$");
            int index = input.Length - 1;
            if (regex.IsMatch(input))
            {
                valid = true;
            }
            else if (input.Equals(""))
            {
                valid = true;
            }
            else
            {
                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
                    if (!(Char.IsDigit(c) || c.Equals('.')))
                    {
                        index = i;
                        break;
                    }
                }
                valid = false;
            }

            if (!valid)
            {
                textBox.Text = input.Remove(index, 1);
            }
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
    }
}
