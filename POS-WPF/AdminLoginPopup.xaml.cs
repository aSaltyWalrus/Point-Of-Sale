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

namespace POS_WPF
{
    /// <summary>
    /// Interaction logic for AdminLoginPopup.xaml
    /// </summary>
    public partial class AdminLoginPopup : Window
    {
        private Database db;
        private User theUser;

        public AdminLoginPopup(Database db, User theUser)
        {
            InitializeComponent();
            this.db = db;
            this.theUser = theUser;
            displayMessage.Text = "User " + theUser.EmployeeID + " does not have admin privilege. To access this page login to an admin account.";
        }

        private User CheckValues()
        {
            string idNum = IdNumBox.Text;
            string password = PasswordBox.Password;
            return db.CheckLogin(idNum, password);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            User tempUser = CheckValues();
            if (tempUser != null)
            {
                if (tempUser.EmpType != User.EmployeeType.Employee)
                {
                    MainWindow main = (MainWindow)this.Owner;
                    main.Main_Page.Visibility = Visibility.Collapsed;
                    main.Admin_Page.Visibility = Visibility.Visible;
                    this.Close();
                }

                else
                    ErrorMessage.Text = "inputted user is not an Admin";

            }
            else
            {
                ErrorMessage.Text = "invalid credentials";
            }
        }
    }
}
