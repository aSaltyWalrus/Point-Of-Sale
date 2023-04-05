using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace POS_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public string empID, empName, empType;
        Task task;
        User currUser;
        Order currOrder;
        Database db;
        int itemPageNum = 0;
        int catNum = 0;
        int selected = -1;
        Category currCategory;
        List<Button> itemButtons;
        List<ToggleButton> orderButtons;
        List<Category> categories;
        List<ComboBoxItem> categoryButtons;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            db = new Database();
            currUser = (User)e.Parameter;
            LoadPage();
            currCategory = categories[catNum];
            CategoryBox.ItemsSource = categoryButtons;

            /*
            string[] parameters = (string[])e.Parameter;
            empID = parameters[0];
            empName = parameters[1];
            empType = parameters[2];

            // load stock
            stock = new Stock();
            // hardcoded dummy data
            stock.addCategory("Food");
            stock.addItem(new Item(1, "Carrot", 10, 0.20, "assets/carrot.png", stock.getCategoryIDByName("Food")));
            stock.addItem(new Item(2, "Apple", 12, 0.50, "assets/apple.png", stock.getCategoryIDByName("Food")));
            stock.addItem(new Item(3, "Pineapple", 3, 1.20, "assets/pineapple.png", stock.getCategoryIDByName("Food")));

            // load users
            users = new Users();
            // hardcoded dummy data
            users.addUser(new User(1, "Fredrick", "Fredrickson", User.EmployeeType.Admin));
            users.addUser(new User(2, "Zouhair", "Bahaoui", User.EmployeeType.Manager));
            users.addUser(new User(3, "Ian", "Curtis", User.EmployeeType.Employee));

            // create stock/user paramater class
                
            stockUsers = new StockUsers(stock, users);*/
        }

        private void LoadPage()
        {
            currOrder = new Order(currUser.EmployeeID);
            categories = db.LoadCategories();
            itemButtons = new List<Button>();
            orderButtons = new List<ToggleButton>();

            foreach (Category cat in categories)
            {
                cat.Items = db.LoadItems(cat);
            }

            for (int i = 1; i <= 20; i++)
            {
                Button b = (Button)Main_Page.FindName("B" + i);
                b.Visibility = Visibility.Collapsed;
                itemButtons.Add(b);
            }

            categoryButtons = AddCategories();

            if (categoryButtons.Count > 0)
            {
                CategoryBox.SelectedIndex = catNum;
                LoadItems(categories[catNum]);
            }

        }

        private List<ComboBoxItem> AddCategories()
        {
            List<ComboBoxItem> temp = new List<ComboBoxItem>();

            foreach (Category cat in categories)
            {
                ComboBoxItem b = new ComboBoxItem
                {
                    Content = cat.Name
                };
                temp.Add(b);
            }

            return temp;
        }

        private void LoadItems(Category theCategory)
        {
            foreach (Button b in itemButtons)
            {
                b.Visibility = Visibility.Collapsed;
            }

            if (itemPageNum == 0)
            {
                Prev.Visibility = Visibility.Collapsed;
                List<Item> items = theCategory.Items;
                if (items.Count > 10)
                {
                    Next.Visibility = Visibility.Visible;
                    for (int i = 0; i < 10; i++)
                    {
                        TextBlock T = new TextBlock();
                        T.Text = items[i].Name;
                        T.FontSize = 30;
                        T.TextAlignment = TextAlignment.Center;
                        T.TextWrapping = TextWrapping.WrapWholeWords;
                        itemButtons[i].Content = T;
                        itemButtons[i].Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    Next.Visibility = Visibility.Collapsed;
                    for (int i = 0; i < items.Count; i++)
                    {
                        TextBlock T = new TextBlock();
                        T.Text = items[i].Name;
                        T.FontSize = 30;
                        T.TextAlignment = TextAlignment.Center;
                        T.TextWrapping = TextWrapping.WrapWholeWords;
                        itemButtons[i].Content = T;
                        itemButtons[i].Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                Prev.Visibility = Visibility.Visible;
                int itemNum = (itemPageNum * 10);
                List<Item> items = theCategory.Items;
                if (items.Count > itemNum + 10)
                {
                    Next.Visibility = Visibility.Visible;
                    for (int i = 0; i < 10; i++)
                    {
                        TextBlock T = new TextBlock();
                        T.Text = items[itemNum + i].Name;
                        T.FontSize = 30;
                        T.TextAlignment = TextAlignment.Center;
                        T.TextWrapping = TextWrapping.WrapWholeWords;
                        itemButtons[i].Content = T;
                        itemButtons[i].Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    Next.Visibility = Visibility.Collapsed;
                    for (int i = 0; i < items.Count - itemNum; i++)
                    {
                        TextBlock T = new TextBlock();
                        T.Text = items[itemNum + i].Name;
                        T.FontSize = 30;
                        T.TextAlignment = TextAlignment.Center;
                        T.TextWrapping = TextWrapping.WrapWholeWords;
                        itemButtons[i].Content = T;
                        itemButtons[i].Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void AddItemToOrder(object sender, RoutedEventArgs e)
        {
            try
            {
                string buttonClicked = ((Button)sender).Name;
                int itemNum = int.Parse(buttonClicked.Substring(1)) + (itemPageNum * 10) - 1;
                Item theItem = currCategory.Items[itemNum];
                selected = currOrder.addItemToOrder(theItem);
                UpdateOrderList();
                if (selected >= 0)
                {
                    OrderItem_Click(orderButtons[selected], e);
                }
            }
            catch (Exception)
            {

            }
        }

        private void UpdateOrderList()
        {
            orderButtons.Clear();
            stackPanel.Children.Clear();

            double orderTotal = 0;

            for (int i = 0; i < currOrder.OrderItems.Count; i++)
            {
                OrderItem item = currOrder.OrderItems[i];

                ToggleButton tb = new ToggleButton
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    MinHeight = 100,
                    Padding = new Thickness(10, 10, 10, 10),
                    Background = new SolidColorBrush(Color.FromArgb(255, 102, 102, 102)),
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    Name = "TB" + (i + 1)
                };
                tb.Click += OrderItem_Click;

                Grid grid = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 450
                };
                ColumnDefinition c1 = new ColumnDefinition
                {
                    Width = new GridLength(70)
                };
                ColumnDefinition c2 = new ColumnDefinition();
                ColumnDefinition c3 = new ColumnDefinition
                {
                    Width = new GridLength(100)
                };
                grid.ColumnDefinitions.Add(c1);
                grid.ColumnDefinitions.Add(c2);
                grid.ColumnDefinitions.Add(c3);

                TextBlock quantity = new TextBlock
                {
                    FontSize = 32,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Text = item.Quantity.ToString()
                };

                TextBlock itemName = new TextBlock
                {
                    FontSize = 32,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetColumn(itemName, 1);
                itemName.Text = item.Name;

                TextBlock price = new TextBlock
                {
                    FontSize = 32,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                Grid.SetColumn(price, 2);
                orderTotal += item.TotalItemPrice;
                string iPrice = item.TotalItemPrice.ToString();
                if (item.TotalItemPrice % 1 == 0)
                {
                    iPrice += ".00";
                }
                else if (item.TotalItemPrice % 0.1 == 0)
                {
                    iPrice += "0";
                }
                price.Text = iPrice;

                grid.Children.Add(quantity);
                grid.Children.Add(itemName);
                grid.Children.Add(price);

                tb.Content = grid;

                orderButtons.Add(tb);
                stackPanel.Children.Add(tb);
            }

            orderTotal = Math.Round(orderTotal * 100) / 100;

            string result = "";
            if (orderTotal % 1 == 0)
            {
                result = orderTotal + ".00";
            }
            else if (orderTotal % 0.1 == 0)
            {
                result = orderTotal + "0";
            }
            else
            {
                result = orderTotal.ToString();
            }

            TotalPrice.Text = "Total Price:      £" + result;
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            itemPageNum--;
            LoadItems(categories[catNum]);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            itemPageNum++;
            LoadItems(categories[catNum]);
        }

        private void CategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            catNum = CategoryBox.SelectedIndex;
            if (catNum < 0)
            {
                catNum = 0;
            }
            currCategory = categories[catNum];
            itemPageNum = 0;
            LoadItems(categories[catNum]);
        }

        private void OrderItem_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            foreach (ToggleButton b in orderButtons)
            {
                b.IsChecked = false;
            }

            selected = int.Parse(button.Name.Substring(2)) - 1;
            button.IsChecked = true;
        }

        private void ToAdminPage(object sender, RoutedEventArgs e)
        {
            if (Admin_Page.Visibility == Visibility.Collapsed)
            {
                User tempUser = currUser;
                if (tempUser.EmpType == User.EmployeeType.Admin)
                {
                    Main_Page.Visibility = Visibility.Collapsed;
                    Admin_Page.Visibility = Visibility.Visible;
                }
                else
                {
                    // add adminlogin popup
                    AdminLoginPopup.IsOpen = true;
                }
            }
        }

        private void ToLoginPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginScreen));
        }

        private void ToHomePage(object sender, RoutedEventArgs e)
        {
            Admin_Page.Visibility = Visibility.Collapsed;
            Main_Page.Visibility = Visibility.Visible;
            LoadPage();
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (task != null)
            {
                if (task.IsCompleted == false)
                {
                    task.Wait();
                }
            }

            if (currOrder.OrderItems.Count > 0)
            {
                Order prevOrder = currOrder;
                task = Task.Run(() =>
               {
                   prevOrder.CalcTotalPrice();
                   db.AddOrder(prevOrder, prevOrder.Serialize(), currUser);
                   for (int i = 0; i < prevOrder.OrderItems.Count; i++)
                   {
                       OrderItem item = prevOrder.OrderItems[i];
                       Item catItem = null;
                       foreach (Category category in categories)
                       {
                           if (category.Name.Equals(item.CategoryName))
                           {
                               catItem = category.getItemByID(item.ItemID);
                               break;
                           }
                       }
                       db.ChangeStock(ref catItem, -item.Quantity);
                   }
                   Thread.Sleep(4000);
               });

                currOrder = new Order(currUser.EmployeeID);
                selected = -1;
                UpdateOrderList();

            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            User theUser = CheckValues();
            if (theUser != null)
            {
                if (theUser.EmpType == User.EmployeeType.Admin)
                {
                    AdminLoginPopup.IsOpen = false;
                    Main_Page.Visibility = Visibility.Collapsed;
                    Admin_Page.Visibility = Visibility.Visible;
                    LoadPage();
                }

                else
                    ErrorMessage.Text = "inputted user is not an Admin";

            }
            else
            {
                ErrorMessage.Text = "invalid credentials";
            }

            theUser = null;
            PasswordBox.Password = "";
        }

        private User CheckValues()
        {
            string idNum = IdNumBox.Text;
            string password = PasswordBox.Password;
            return db.CheckLogin(idNum, password);
        }

        // Admin Page Methods below

        private List<Order> orderHistory;
        private int AdminCatNum = 0;
        private Category AdminCurrCat;
        private PopupType currPopup;
        private List<ComboBoxItem> stockCatItems = new List<ComboBoxItem>();
        private List<ComboBoxItem> itemCatItems = new List<ComboBoxItem>();
        private List<User> users = new List<User>();
        private ToggleButton stockItemSelected = null;
        private ToggleButton itemsItemSelected = null;
        private ToggleButton categorySelected = null;
        private ToggleButton userSelected = null;
        private enum PopupType
        {
            Stock,
            Items,
            Categories,
            Staff
        };


        private void OrderSearchClick(object sender, RoutedEventArgs e)
        {
            string searchTerm = AdminSearchTerm.Text;
            orderHistory = new List<Order>();
            switch (AdminSearchBy.SelectedIndex)
            {
                case 0: //Order Number
                    orderHistory = db.FindOrders(Database.OrderIdentifiers.OrderNum, searchTerm);
                    break;
                case 1: //Total Price
                    orderHistory = db.FindOrders(Database.OrderIdentifiers.Price, searchTerm);
                    break;
                case 2: //Employee ID
                    orderHistory = db.FindOrders(Database.OrderIdentifiers.IdTakenBy, searchTerm);
                    break;
            }

            AdminOrderList.Children.Clear();

            for (int i = 0; i < orderHistory.Count; i++)
            {
                Order temp = orderHistory[i];
                Button b = new Button()
                {
                    Name = "O" + (i + 1),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                b.Click += SelectOrder;

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
                    Text = temp.OrderNum.ToString(),
                    FontSize = 26,
                };

                double actualPrice = Math.Round(temp.TotalPrice * 100) / 100;

                TextBlock t2 = new TextBlock()
                {
                    Text = "£" + actualPrice,
                    FontSize = 26
                };

                if (actualPrice % 1 == 0)
                {
                    t2.Text += "00";
                }
                else if (actualPrice % 0.1 == 0)
                {
                    t2.Text += "0";
                }

                Grid.SetColumn(t1, 0);
                Grid.SetColumn(t2, 1);

                g.Children.Add(t1);
                g.Children.Add(t2);

                b.Content = g;

                AdminOrderList.Children.Add(b);
            }

        }

        private void SelectOrder(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            int num = int.Parse(b.Name.Substring(1)) - 1;

            Order theOrder = orderHistory[num];
            double actualPrice = Math.Round(theOrder.TotalPrice * 100) / 100;

            AdminOrderNum.Text = theOrder.OrderNum.ToString();
            AdminEmpId.Text = theOrder.empNum.ToString();

            if (actualPrice % 1 == 0)
            {
                AdminTotalPrice.Text = "£" + actualPrice + ".00";
            }
            else if (actualPrice % 0.1 == 0)
            {
                AdminTotalPrice.Text = "£" + actualPrice + "0";
            }
            else
            {
                AdminTotalPrice.Text = "£" + actualPrice;
            }

            AdminItemList.Text = "";

            foreach (OrderItem item in theOrder.OrderItems)
            {
                AdminItemList.Text += item.Quantity + " x " + item.Name + "\n";
            }

        }

        //General Popup Methods

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

        private void OpenPopup(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            AdminCatNum = 0;
            AdminCurrCat = null;
            switch (button.Name.ToLower())
            {
                case "popupstock":
                    if (ManageStockPopup.IsOpen == false)
                    {
                        currPopup = PopupType.Stock;
                        stockCatItems = AddCategories();
                        StockCat.ItemsSource = stockCatItems;
                        if (stockCatItems.Count > 0)
                        {
                            StockCat.SelectedIndex = 0;
                        }
                        ClosePopup(sender, e);
                        ManageStockPopup.IsOpen = true;
                    }
                    break;
                case "popupitems":
                    if (ManageItemsPopup.IsOpen == false)
                    {
                        currPopup = PopupType.Items;
                        itemCatItems = AddCategories();
                        ItemCat.ItemsSource = itemCatItems;
                        ItemCatNumBox.ItemsSource = AddCategories();
                        if (itemCatItems.Count > 0)
                        {
                            ItemCat.SelectedIndex = 0;
                        }
                        ClosePopup(sender, e);
                        ManageItemsPopup.IsOpen = true;

                        ItemOption_Click(ItemOption1, null);
                    }
                    break;
                case "popupcategories":
                    if (ManageCategoriesPopup.IsOpen == false)
                    {
                        currPopup = PopupType.Categories;
                        LoadCategoriesPopup();
                        ClosePopup(sender, e);
                        ManageCategoriesPopup.IsOpen = true;
                    }
                    break;
                case "popupusers":
                    if (ManageUsersPopup.IsOpen == false)
                    {
                        currPopup = PopupType.Staff;
                        LoadUsersPopup();
                        ClosePopup(sender, e);
                        ManageUsersPopup.IsOpen = true;
                    }
                    break;
            }
        }

        private void ClosePopup(object sender, RoutedEventArgs e)
        {
            if (AdminLoginPopup.IsOpen)
            {
                AdminLoginPopup.IsOpen = false;
            }
            else if (ManageStockPopup.IsOpen)
            {
                ManageStockPopup.IsOpen = false;
            }
            else if (ManageItemsPopup.IsOpen)
            {
                ManageItemsPopup.IsOpen = false;
            }
            else if (ManageCategoriesPopup.IsOpen)
            {
                ManageCategoriesPopup.IsOpen = false;
            }
            else if (ManageUsersPopup.IsOpen)
            {
                ManageUsersPopup.IsOpen = false;
            }
        }

        private void loadAdminItems()
        {
            switch (currPopup)
            {
                case PopupType.Stock:
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
                    break;
                case PopupType.Items:
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
                    break;
            }
        }

        private void SelectItemFromList(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;
            int num = int.Parse(b.Name.Substring(2)) - 1;
            Item theItem = AdminCurrCat.Items[num];
            db.GetStockFor(ref theItem);

            switch (currPopup)
            {
                case PopupType.Stock:
                    if (stockItemSelected != null && stockItemSelected.Equals(b) == false)
                    {
                        stockItemSelected.IsChecked = false;
                    }
                    stockItemSelected = b;
                    stockItemSelected.IsChecked = true;
                    StockIDBox.Text = theItem.ID.ToString();
                    StockNameBox.Text = theItem.Name.ToString();
                    StockAmountBox.Text = theItem.Stock.ToString();
                    break;
                case PopupType.Items:
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
                    break;
            }
        }

        private void CheckIsDigit(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => char.IsDigit(c) == false);
        }

        private void CheckIsDouble(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            bool valid = true;
            string input = args.NewText;
            Regex regex = new Regex(@"^(\d+\.?\d?\d?)$");
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
                valid = false;
            }

            args.Cancel = !valid;
        }

        //Stock Popup

        private void StockCategoryChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StockCat.SelectedIndex >= 0)
            {
                AdminCatNum = StockCat.SelectedIndex;
                AdminCurrCat = categories[AdminCatNum];
                loadAdminItems();

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

        //Item Popup

        private void ItemCategoryChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemCat.SelectedIndex >= 0)
            {
                AdminCatNum = ItemCat.SelectedIndex;
                AdminCurrCat = categories[AdminCatNum];
                loadAdminItems();

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

        //Categories Popup

        private void LoadCategoriesPopup()
        {
            AdminCategoriesList.Children.Clear();

            for (int i = 0; i < categories.Count; i++)
            {
                Category tempCat = categories[i];
                ToggleButton b = new ToggleButton()
                {
                    Name = "Ca" + (i + 1),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                b.Click += SelectCategoryFromList;

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
                    Text = tempCat.CatNum.ToString(),
                    FontSize = 26,
                };

                TextBlock t2 = new TextBlock()
                {
                    Text = tempCat.Name,
                    FontSize = 26
                };

                Grid.SetColumn(t1, 0);
                Grid.SetColumn(t2, 1);

                g.Children.Add(t1);
                g.Children.Add(t2);

                b.Content = g;
                AdminCategoriesList.Children.Add(b);
            }

            CategoryOption_Click(CategoryOption1, null);
        }

        private void SelectCategoryFromList(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;
            int num = int.Parse(b.Name.Substring(2)) - 1;
            Category theCategory = categories[num];

            if (categorySelected != null && categorySelected.Equals(b) == false)
            {
                categorySelected.IsChecked = false;
            }
            categorySelected = b;
            categorySelected.IsChecked = true;

            CategoryIdBox.Text = theCategory.CatNum.ToString();
            CategoryNameBox.Text = theCategory.Name.ToString();

            CategoryOption_Click(CategoryOption3, null);
        }

        private void CategoryOption_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = (ToggleButton)sender;

            if (tb != null)
            {
                switch (tb.Name.ToLower())
                {
                    case "categoryoption1":
                        if (categorySelected != null)
                        {
                            categorySelected.IsChecked = false;
                            categorySelected = null;
                        }
                        CategoryNameBox.IsEnabled = true;
                        CategoryIdBox.Text = "";
                        CategoryNameBox.Text = "";

                        CategoryOption1.IsChecked = true;
                        CategoryOption2.IsChecked = false;
                        CategoryOption3.IsChecked = false;
                        break;
                    case "categoryoption2":
                        if (categorySelected != null)
                        {
                            CategoryNameBox.IsEnabled = false;

                            CategoryOption1.IsChecked = false;
                            CategoryOption2.IsChecked = true;
                            CategoryOption3.IsChecked = false;
                        }
                        else
                        {
                            CategoryOption_Click(CategoryOption1, null);
                        }
                        break;
                    case "categoryoption3":
                        if (categorySelected != null)
                        {
                            CategoryNameBox.IsEnabled = true;

                            CategoryOption1.IsChecked = false;
                            CategoryOption2.IsChecked = false;
                            CategoryOption3.IsChecked = true;
                        }
                        else
                        {
                            CategoryOption_Click(CategoryOption1, null);
                        }
                        break;
                }

            }
        }

        private void CategorySubmitButtonClick(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            string catName = CategoryNameBox.Text;

            if (catName.Equals(""))
            {
                valid = false;
            }

            if (CategoryOption1.IsChecked == true)
            {
                if (valid)
                {
                    db.AddCategory(catName, ref categories);
                    LoadCategoriesPopup();
                    CategoryOption_Click(CategoryOption1, null);
                }
            }
            else if (CategoryOption2.IsChecked == true)
            {
                if (valid)
                {
                    int id = int.Parse(CategoryIdBox.Text);
                    Category theCategory = null;

                    foreach (Category temp in categories)
                    {
                        if (temp.CatNum == id)
                        {
                            theCategory = temp;
                            break;
                        }
                    }

                    if (theCategory != null)
                    {
                        if (theCategory.Items.Count == 0)
                        {
                            if (db.RemoveCategory(id))
                            {
                                categories.Remove(theCategory);
                                LoadCategoriesPopup();
                                CategoryOption_Click(CategoryOption1, null);
                            }
                        }
                    }
                }
            }
            else if (CategoryOption3.IsChecked == true)
            {
                if (valid)
                {
                    int id = int.Parse(CategoryIdBox.Text);
                    Category theCategory = null;

                    foreach (Category temp in categories)
                    {
                        if (temp.CatNum == id)
                        {
                            theCategory = temp;
                            break;
                        }
                    }

                    if (theCategory != null)
                    {
                        db.UpdateCategory(ref theCategory, catName);
                        LoadCategoriesPopup();
                        CategoryOption_Click(CategoryOption1, null);
                    }
                }
            }

        }

        //Users Popup

        private void LoadUsersPopup()
        {
            users = db.LoadUsers();

            AdminUsersList.Children.Clear();

            if (users != null)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    User tempUser = users[i];
                    ToggleButton b = new ToggleButton()
                    {
                        Name = "Us" + (i + 1),
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };

                    b.Click += SelectUserFromList;

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
                        Text = tempUser.EmployeeID.ToString(),
                        FontSize = 26,
                    };

                    TextBlock t2 = new TextBlock()
                    {
                        Text = tempUser.LastName + ", " + tempUser.FirstName,
                        FontSize = 26
                    };

                    Grid.SetColumn(t1, 0);
                    Grid.SetColumn(t2, 1);

                    g.Children.Add(t1);
                    g.Children.Add(t2);

                    b.Content = g;
                    AdminUsersList.Children.Add(b);
                }
            }

            UsersOption_Click(UserOption1, null);
        }

        private void ClearUsersFields(object sender, RoutedEventArgs e)
        {
            UsersOption_Click(UserOption1, null);
        }

        private void SelectUserFromList(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;
            int num = int.Parse(b.Name.Substring(2)) - 1;
            User theUser = users[num];

            if (userSelected != null && userSelected.Equals(b) == false)
            {
                userSelected.IsChecked = false;
            }
            userSelected = b;
            userSelected.IsChecked = true;

            UsersEmpIdBox.Text = theUser.EmployeeID.ToString();
            UsersFirstNameBox.Text = theUser.FirstName;
            UsersLastNameBox.Text = theUser.LastName;
            switch (theUser.EmpType)
            {
                case User.EmployeeType.Employee:
                    UsersEmpTypeBox.SelectedIndex = 0;
                    break;
                case User.EmployeeType.Manager:
                    UsersEmpTypeBox.SelectedIndex = 1;
                    break;
                case User.EmployeeType.Admin:
                    UsersEmpTypeBox.SelectedIndex = 2;
                    break;
            }
            UsersPassBox.Password = theUser.Password;

            UsersOption_Click(UserOption3, null);
        }

        private void UsersOption_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = (ToggleButton)sender;

            if (tb != null)
            {
                switch (tb.Name.ToLower())
                {
                    case "useroption1":
                        if (userSelected != null)
                        {
                            userSelected.IsChecked = false;
                            userSelected = null;
                        }

                        UsersFirstNameBox.IsEnabled = true;
                        UsersLastNameBox.IsEnabled = true;
                        UsersEmpTypeBox.IsEnabled = true;
                        UsersPassBox.IsEnabled = true;

                        UsersEmpIdBox.Text = "";
                        UsersFirstNameBox.Text = "";
                        UsersLastNameBox.Text = "";
                        UsersEmpTypeBox.SelectedIndex = 0;
                        UsersPassBox.Password = "";

                        UserOption1.IsChecked = true;
                        UserOption2.IsChecked = false;
                        UserOption3.IsChecked = false;
                        break;
                    case "useroption2":
                        if (userSelected != null)
                        {
                            UsersFirstNameBox.IsEnabled = false;
                            UsersLastNameBox.IsEnabled = false;
                            UsersEmpTypeBox.IsEnabled = false;
                            UsersPassBox.IsEnabled = false;

                            UserOption1.IsChecked = false;
                            UserOption2.IsChecked = true;
                            UserOption3.IsChecked = false;
                        }
                        else
                        {
                            UsersOption_Click(UserOption1, null);
                        }
                        break;
                    case "useroption3":
                        if (userSelected != null)
                        {
                            UsersFirstNameBox.IsEnabled = true;
                            UsersLastNameBox.IsEnabled = true;
                            UsersEmpTypeBox.IsEnabled = true;
                            UsersPassBox.IsEnabled = true;

                            UserOption1.IsChecked = false;
                            UserOption2.IsChecked = false;
                            UserOption3.IsChecked = true;
                        }
                        else
                        {
                            UsersOption_Click(UserOption1, null);
                        }
                        break;
                }

            }
        }

        private void UsersSubmitButtonClick(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            string firstname = UsersFirstNameBox.Text;
            string lastname = UsersLastNameBox.Text;
            string password = UsersPassBox.Password;
            string type = null;

            switch (UsersEmpTypeBox.SelectedIndex)
            {
                case 0:
                    type = "Employee";
                    break;
                case 1:
                    type = "Manager";
                    break;
                case 2:
                    type = "Admin";
                    break;
            }

            if (firstname.Equals(""))
            {
                valid = false;
            }

            if (lastname.Equals(""))
            {
                valid = false;
            }

            if (type.Equals(""))
            {
                valid = false;
            }

            if (UserOption1.IsChecked == true)
            {
                if (valid)
                {
                    db.AddUser(firstname, lastname, type, password);
                    LoadUsersPopup();
                    UsersOption_Click(UserOption1, null);
                }
            }
            else if (UserOption2.IsChecked == true)
            {
                int id = int.Parse(UsersEmpIdBox.Text);
                User theUser = null;

                foreach (User temp in users)
                {
                    if (temp.EmployeeID == id)
                    {
                        theUser = temp;
                        break;
                    }
                }

                if (theUser != null)
                {
                    if (db.RemoveUser(id))
                    {
                        users.Remove(theUser);
                        LoadUsersPopup();
                        UsersOption_Click(UserOption1, null);
                    }
                }
            }
            else if (UserOption3.IsChecked == true)
            {
                if (valid)
                {
                    int id = int.Parse(UsersEmpIdBox.Text);
                    User theUser = null;

                    foreach (User temp in users)
                    {
                        if (temp.EmployeeID == id)
                        {
                            theUser = temp;
                            break;
                        }
                    }

                    if (theUser != null)
                    {
                        db.UpdateUser(ref theUser, firstname, lastname, type, password);
                        LoadUsersPopup();
                        UsersOption_Click(UserOption1, null);
                    }
                }
            }

        }
    }
}