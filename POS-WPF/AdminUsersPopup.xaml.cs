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
    /// Interaction logic for AdminUsersPopup.xaml
    /// </summary>
    public partial class AdminUsersPopup : Window
    {
        private List<User> users = new List<User>();
        private ToggleButton userSelected = null;
        private Database db;

        public AdminUsersPopup(Database db)
        {
            InitializeComponent();

            this.db = db;
            LoadUsersPopup();
        }

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
