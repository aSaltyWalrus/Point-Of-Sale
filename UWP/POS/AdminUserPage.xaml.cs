using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace POS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminUserPage : Page
    {
        User currUser;
        Users users;
        Database db;
        public AdminUserPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            db = new Database();

            currUser = (User)e.Parameter;
            // load users
        }

        private void AddUser(object sender, RoutedEventArgs e)
        {
            addUserDebug.Text = addUser();
        }

        private string addUser()
        {
            int ID;
            if (textboxID.Text == "") 
                return "empty ID";
            try { ID = Convert.ToInt32(textboxID.Text); }
            catch { return "invalid ID"; }

            string surname, forename;
            if (textboxSurname.Text == "") 
                return "empty surname";
            surname = textboxSurname.Text;

            if (textboxForename.Text == "") 
                return "empty forename";
            forename = textboxForename.Text;

            if (textboxRank.SelectedIndex == -1) 
                return "no rank selected";

            User.EmployeeType type = User.EmployeeType.Employee;

            switch (textboxRank.SelectedIndex)
            {
                case -1:
                    return "no rank selected";
                case 0:
                    type = User.EmployeeType.Employee;
                    break;
                case 1:
                    type = User.EmployeeType.Manager;
                    break;
                case 2:
                    type = User.EmployeeType.Admin;
                    break;
            }

            User temp = new User(ID, forename, surname, type);
            if (db.AddUser(ref temp) == false) 
                return "ID " + ID + "is already taken";
            return "added " + surname;
        }

        private void EditUser(object sender, RoutedEventArgs e)
        {
            addUserDebug.Text = editUser();
        }

        private string editUser()
        {
            int ID;
            if (textboxID.Text == "") 
                return "empty ID";
            try { ID = Convert.ToInt32(textboxID.Text); }
            catch { return "invalid ID"; }

            User temp = db.FindUser(ID);
            if (temp == null) { return "ID doesn't exist"; }

            if (textboxForename.Text != "")
                temp.FirstName = textboxForename.Text;

            if (textboxSurname.Text != "")
                temp.LastName = textboxSurname.Text;

            switch (textboxRank.SelectedIndex)
            {
                case -1:
                    break;
                case 0:
                    temp.EmpType = User.EmployeeType.Employee;
                    break;
                case 1:
                    temp.EmpType = User.EmployeeType.Manager;
                    break;
                case 2:
                    temp.EmpType = User.EmployeeType.Admin;
                    break;
            }
            
            db.RemoveUser(ID);
            db.AddUser(ref temp);
            return "edited " + ID;
        }

        private void RemoveUser(object sender, RoutedEventArgs e)
        {
            deleteUserDebug.Text = removeUser();
        }

        private string removeUser()
        {
            try
            {
                int ID = Convert.ToInt32(textboxDeleteID.Text);
                if (db.RemoveUser(ID) == true)
                    return "removed " + ID;
                return "ID " + ID + " doesn't exist";
            }
            catch { return "invalid ID"; }
        }

        private void toItemPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminPage), currUser);
        }

        private void toMainPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), currUser);
        }

        private void toLoginPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginScreen));
        }
    }
}
