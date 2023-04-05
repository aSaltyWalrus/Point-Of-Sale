using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_UWP
{
    class Category
    {
        List<Item> items;
        int catNum;
        string name;

        public Category(int catNum, string name)
        {
            this.catNum = catNum;
            this.name = name;
            this.items = new List<Item>();
        }

        public int CatNum 
        { 
            get { return catNum; }
            set { catNum = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<Item> Items
        {
            get { return items; }
            set { items = value; }
        }

        public void addItem(Item item)
        {
            items.Add(item);
        }

        public void removeItem(Item item)
        {
            items.Remove(item);
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
    }
}
