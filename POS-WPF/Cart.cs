using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_WPF
{
    class Cart
    {
        Stock stock = new Stock(); //Creates New Stock List
        List<Item> items;

        public Cart()
        {
            this.items = new List<Item>();
        }

        public List<Item> Items //Creates Item List
        {
            get { return items; }
            set { items = value; }
        }

        public void addItemByID(int ID) //Adds Item into list
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
            if (!inCart) //If items in Cart it will be added to the list with the ocorrect ID
            {
                items.Add(stock.getItemByID(ID));
            }
        }

        public void removeItemByID(int ID) //Removes item from list
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

        public double getTotalPrice() //Outputs total price 
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
