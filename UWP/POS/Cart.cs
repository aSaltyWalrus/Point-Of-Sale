using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_UWP
{
    class Cart
    {
        Stock stock = new Stock();
        List<Item> items;

        public Cart()
        {
            this.items = new List<Item>();
        }

        public List<Item> Items
        {
            get { return items; }
            set { items = value; }
        }

        public void addItemByID(int ID)
        {
            bool inCart = false;
            foreach (Item i in items)
            {
                if (i.ID == ID)
                {
                    i.Stock += 1;
                    inCart = true;
                    break;
                }
            }
            if (!inCart)
            {
                items.Add(stock.getItemByID(ID));
            }
        }

        public void removeItemByID(int ID)
        {
            foreach(Item i in items)
            {
                if (i.ID == ID)
                {
                    if (i.Stock > 1) 
                    {
                        i.Stock -= 1;
                    }
                    else 
                    { 
                        items.Remove(i); 
                    }
                    break;
                }
            }
            
        }

        public double getTotalPrice()
        {
            double total = 0;
            foreach(Item i in items)
            {
                total += i.Price * i.Stock;
            }
            return total;
        }
    }
}
