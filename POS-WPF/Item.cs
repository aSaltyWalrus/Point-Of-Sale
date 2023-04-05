using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_WPF
{
    public class Item 
    {
        int id, stock;
        string name, image;
        double price;
        Category category;

        public Item(int id, string name, int stock, double price, string image, Category category) //Creates Item
        {
            this.id = id;
            this.stock = stock;
            this.name = name;
            this.image = image;
            this.category = category;
            this.price = price;
        }

        public int ID //ID 
        {
            get { return id; }  
            set { id = value; } 
        }

        public int Stock //Stock
        {
            get { return stock; }
            set { stock = value; }
        }

        public string Name //Username
        {
            get { return name; }
            set { name = value; } 
        }

        public string Image 
        {
            get { return image; }
            set { image = value; }
        }

        public Category Category
        {
            get { return category; }
            set { category = value; }
        }

        public double Price
        {
            get { return price; }
            set { price = value; }
        }
    }
}
