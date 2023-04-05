using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

namespace POS_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginScreen : Page
    {
        Database db;
        Task task;

        public LoginScreen()
        {
            this.InitializeComponent();
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
                    this.Frame.Navigate(typeof(MainPage), theUser);
                    ErrorMessage.Visibility = Visibility.Collapsed;
                    IdNumBox.Text = "";
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
