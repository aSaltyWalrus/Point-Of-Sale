using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_WPF
{
    public class Stock
    {
        List<Item> items;
        List<Category> categories;

        public Stock()
        {
            this.items = new List<Item>();
            this.categories = new List<Category>();
        }

        public List<Item> Items
        {
            get { return items; }
            set { items = value; }
        }

        public List<Category> Categories
        {
            get { return categories; }
            set { categories = value; }
        }

        public Item getItemByID(int ID)
        {
            foreach (Item i in items)
            {
                if (i.ID == ID) 
                { 
                    return i;
                }
            }
            return null;
        }

        public int addItem(Item item)
        {
            foreach(Item i in items)
            {
                if(i.ID == item.ID)
                {
                    return 0; // id already taken
                }
            }
            items.Add(item); // each new item is initialised with a quantity value of 1
            foreach(Category c in categories)
            {
                if(c.CatNum == item.Category.CatNum)
                {
                    c.addItem(item);
                    return 1;
                }
            }
            return 0;

        }

        public int removeItem(int id)
        {
            foreach(Item i in items)
            {
                if (i.ID == id)
                {
                    foreach (Category c in categories)
                    {
                        if (c.CatNum == i.Category.CatNum)
                        {
                            c.removeItem(i);
                            break;
                        }
                    }
                    items.Remove(i);
                    return 1;
                }
            }
            return 0; // invalid id
        }

        public Category getCategoryIDByName(string name)
        {
            foreach (Category c in categories)
            {
                if (c.Name == name)
                {
                    return c;
                }
            }
            return null;
        }

        public int addCategory(string name)
        {
            foreach(Category c in categories)
            {
                if (c.Name == name)
                {
                    return 0; // name already taken
                }
            }
            categories.Add(new Category(categories.Count, name));
            return 1;
        }

        public int removeCategory(string name)
        {
            foreach(Category c in categories)
            {
                if (c.Name == name)
                {
                    categories.Remove(c);
                    return 1;
                }
            }
            return 0; // invalid name
        }
    }
}
