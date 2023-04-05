using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS_WPF
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainWindow : Window
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

        public MainWindow(User user)
        {
            InitializeComponent();
            db = new Database();
            currUser = user;
            LoadPage();
            currCategory = categories[catNum];
            CategoryBox.ItemsSource = categoryButtons;
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
                if (items.Count > 20)
                {
                    Next.Visibility = Visibility.Visible;
                    for (int i = 0; i < 20; i++)
                    {
                        TextBlock T = new TextBlock();
                        T.Text = items[i].Name;
                        T.FontSize = 30;
                        T.TextAlignment = TextAlignment.Center;
                        T.TextWrapping = TextWrapping.Wrap;
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
                        T.TextWrapping = TextWrapping.Wrap;
                        itemButtons[i].Content = T;
                        itemButtons[i].Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                Prev.Visibility = Visibility.Visible;
                int itemNum = (itemPageNum * 20);
                List<Item> items = theCategory.Items;
                if (items.Count > itemNum + 20)
                {
                    Next.Visibility = Visibility.Visible;
                    for (int i = 0; i < 20; i++)
                    {
                        TextBlock T = new TextBlock();
                        T.Text = items[itemNum + i].Name;
                        T.FontSize = 30;
                        T.TextAlignment = TextAlignment.Center;
                        T.TextWrapping = TextWrapping.Wrap;
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
                        T.TextWrapping = TextWrapping.Wrap;
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

        private void RemoveItemFromOrder(object sender, RoutedEventArgs e)
        {
            if (selected >= 0)
            {
                selected = currOrder.removeItemFromOrder(selected);
                UpdateOrderList();
                if (selected >= 0)
                {
                    OrderItem_Click(orderButtons[selected], e);
                }
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
                ColumnDefinition c2 = new ColumnDefinition
                {
                    Width = new GridLength(270)
                };
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

        private AdminLoginPopup loginPopup;

        private void ToAdminPage(object sender, RoutedEventArgs e)
        {
            if (Admin_Page.Visibility == Visibility.Collapsed)
            {
                if(currUser.EmpType == User.EmployeeType.Employee)
                {
                    loginPopup = new AdminLoginPopup(db, currUser);
                    loginPopup.Owner = this;
                    loginPopup.Show();
                }
                else
                {
                    Main_Page.Visibility = Visibility.Collapsed;
                    Admin_Page.Visibility = Visibility.Visible;
                }
            }
        }

        private void ToLoginPage(object sender, RoutedEventArgs e)
        {
            LoginScreen loginScreen = new LoginScreen();
            loginScreen.Show();
            this.Close();
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
                });

                currOrder = new Order(currUser.EmployeeID);
                selected = -1;
                UpdateOrderList();

            }
        }

        /*private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            User theUser = CheckValues();
            if (theUser != null)
            {
                if (theUser.EmpType == User.EmployeeType.Admin)
                {
                    AdminLoginPopup.Visibility = Visibility.Collapsed;
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
        }*/

        // Admin Page Methods below

        private AdminStockPopup stockPopup;
        private AdminItemPopup itemsPopup;
        private AdminCategoriesPopup categoriesPopup;
        private AdminUsersPopup usersPopup;

        private List<Order> orderHistory;

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

        private void EndOfDayButton_Click(object sender, RoutedEventArgs e)
        {
            if (task != null)
            {
                if (task.IsCompleted == false)
                {
                    task.Wait();
                }
            }

            task = Task.Run(() =>
            {
                List<Order> TodaysOrders = db.EndOfDay();
                List<int> itemIDs = new List<int>();
                Order allOrders = new Order();

                foreach (Order order in TodaysOrders)
                {
                    itemIDs.Sort();
                    allOrders.OrderItems.Sort();
                    order.OrderItems.Sort();
                    foreach (OrderItem orderItem in order.OrderItems)
                    {
                        if (itemIDs.Contains(orderItem.ItemID))
                        {
                            int index = itemIDs.IndexOf(orderItem.ItemID);
                            if (allOrders.OrderItems[index].ItemID == orderItem.ItemID)
                            {
                                allOrders.OrderItems[index].Quantity += orderItem.Quantity;
                            }
                        }
                        else
                        {
                            itemIDs.Add(orderItem.ItemID);
                            allOrders.OrderItems.Add(orderItem);
                        }
                    }
                }

                db.PrintEndOfDay(allOrders, TodaysOrders.Count);
            });
        }

        //General Popup Methods

        private void OpenPopup(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            switch (button.Name.ToLower())
            {
                case "popupstock":
                    stockPopup = new AdminStockPopup(db, ref categories, AddCategories());
                    if (stockPopup.IsActive == false)
                    {
                        stockPopup.Show();
                    }
                    break;
                case "popupitems":
                    itemsPopup = new AdminItemPopup(db, ref categories, AddCategories());
                    if (itemsPopup.IsActive == false)
                    {
                        itemsPopup.Show();
                    }
                    break;
                case "popupcategories":
                    categoriesPopup = new AdminCategoriesPopup(db, ref categories);
                    if (categoriesPopup.IsActive == false)
                    {
                        categoriesPopup.Show();
                    }
                    break;
                case "popupusers":
                    usersPopup = new AdminUsersPopup(db);
                    if (usersPopup.IsActive == false)
                    {
                        usersPopup.Show();
                    }
                    break;
            }
        }
    }
}