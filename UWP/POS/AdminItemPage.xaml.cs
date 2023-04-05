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
    public sealed partial class AdminPage : Page
    {
        User currUser;
        Database db;
        List<Category> categories;
        Stock stock;
        public AdminPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            db = new Database();
            categories = db.LoadCategories();

            foreach (Category cat in categories)
            {
                cat.Items = db.LoadItems(cat);
            }

            currUser = (User)e.Parameter;
            // load stock
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            addItemDebug.Text = addItem();
        }

        private string addItem()
        {
            int ID;
            if (textboxID.Text == "") 
                return "empty ID";
            try { ID = Convert.ToInt32(textboxID.Text); }
            catch { return "invalid ID"; }

            double price;
            if (textboxPrice.Text == "")
                return "empty price";
            try { price = Convert.ToDouble(textboxPrice.Text); }
            catch { return "invalid price"; }

            string name, image, categoryName;
            if (textboxName.Text == "")
                return "empty name";
            name = textboxName.Text;

            if (textboxImage.Text == "")
                return "empty image";
            image = textboxImage.Text;

            if (textboxItemCategory.Text == "")
                return "empty category";
            categoryName = textboxItemCategory.Text;
            Category category = null;
            foreach (Category c in categories)
            {
                if (c.Name == categoryName)
                    category = c;
            }

            if (category == null)
                return "category " + categoryName + " doesn't exist";

            Item item = new Item(ID, name, 1, price, image, category);
            if ( db.AddItem(ref item) == false )
                return "ID " + ID + "is already taken";
            return "added " + name;
        }

        private void EditItem(object sender, RoutedEventArgs e)
        {
            addItemDebug.Text = editItem();
        }

        private string editItem()
        {
            int ID;
            try { ID = Convert.ToInt32(textboxID.Text); }
            catch { return "invalid ID"; }
            Item temp = null;
            foreach (Category c in categories)
            {
                foreach(Item i in c.Items)
                {
                    if (i.ID == ID)
                        temp = i;
                }
            }
            if (temp == null)
                return "ID " + ID + " doesn't exist";
            if (textboxPrice.Text != "")
            {
                try { temp.Price = Convert.ToDouble(textboxPrice.Text); }
                catch { return "invalid price"; }
            }

            if (textboxName.Text != "")
                temp.Name = textboxName.Text;

            if (textboxImage.Text != "")
                temp.Image = textboxImage.Text;

            if (textboxCategory.Text != "")
            {
                string categoryName = textboxItemCategory.Text;
                Category category = null;
                foreach (Category c in categories)
                {
                    if (c.Name == categoryName)
                        category = c;
                }

                if (category == null)
                    return "category " + categoryName + " doesn't exist";
                temp.Category = category;
            }

            db.RemoveItem(ID);
            db.AddItem(ref temp);
            // may need to reload catorgies
            return "edited " + temp.Name;
        }

        private void RemoveItem(object sender, RoutedEventArgs e)
        {
            deleteItemDebug.Text = removeItem();
        }

        private string removeItem()
        {
            try 
            { 
                int ID = Convert.ToInt32(textboxDeleteID.Text);
                if (db.RemoveItem(ID) == true)
                    return "removed " + ID;
                return "ID " + ID + " doesn't exist";
            }
            catch { return "invalid ID"; }
            
        }

        private void AddCategory(object sender, RoutedEventArgs e)
        {
            categoryDebug.Text = addCategory();
        }

        private string addCategory()
        {
            string name = textboxCategory.Text;
            Category category = new Category((categories.Count + 1), name);
            if (db.AddCategory(ref category) == true)
                return "added " + name;
            return "category " + name + " already exist";
        }

        private void RemoveCategory(object sender, RoutedEventArgs e)
        {
            categoryDebug.Text = removeCategory();
        }

        private string removeCategory()
        {
            string name = textboxCategory.Text;
            int catIndex = -1;
            foreach (Category c in categories)
            {
                if (c.Name == name)
                    catIndex = categories.IndexOf(c);
            }
            if (db.RemoveCategory(catIndex) == false)
                return "removed " + name;
            return "category " + name + " doesn't exist";
        }

        private void ToUserPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminUserPage), currUser);
        }

        private void ToMainPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), currUser);
        }

        private void ToLoginPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginScreen));
        }

    }
}
