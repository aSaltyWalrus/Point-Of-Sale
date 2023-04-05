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
    /// Interaction logic for AdminCategoriesPopup.xaml
    /// </summary>
    public partial class AdminCategoriesPopup : Window
    {
        private ToggleButton categorySelected = null;
        private Database db;
        private List<Category> categories;

        public AdminCategoriesPopup(Database db, ref List<Category> categories)
        {
            InitializeComponent();

            this.categories = categories;
            this.db = db;
            LoadCategoriesPopup();
        }

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


    }
}
