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
using System.Windows.Navigation;
using System.Windows.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace POS_WPF
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class LoginScreen : Window
    {
        Database db;
        Task task;

        public LoginScreen()
        {
            InitializeComponent();
            db = new Database();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if(task != null)
            {
                if (task.IsCompleted == false)
                {
                    task.Wait();
                }
            }

            if (db.IsConnected())
            {
                User theUser = CheckValues();
                if (theUser != null)
                {
                    MainWindow main = new MainWindow(theUser);
                    ErrorMessage.Visibility = Visibility.Collapsed;
                    IdNumBox.Text = "";
                    main.Show();
                    this.Close();
                }
                else
                {
                    ErrorMessage.Visibility = Visibility.Visible;
                }

                theUser = null;
                PasswordBox.Password = "";
            }
            else
            {
                task = Task.Run(() =>
                {
                    db = new Database();
                });
                ErrorMessage.Visibility = Visibility.Visible;
            }            
        }

        private User CheckValues()
        {
            string idNum = IdNumBox.Text;
            string password = PasswordBox.Password;
            return db.CheckLogin(idNum, password);
        }
    }
}
