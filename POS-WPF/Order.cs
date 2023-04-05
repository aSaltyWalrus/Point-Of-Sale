using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace POS_WPF
{
    public class Order
    {
        public int OrderNum;
        public List<OrderItem> OrderItems = new List<OrderItem>();
        public double TotalPrice = 0;
        public int empNum;

        public Order(List<Item> items, int empNum)
        {
            this.empNum = empNum;

            foreach (Item i in items)
            {
                OrderItems.Add(new OrderItem(i));
            }

            foreach (OrderItem item in OrderItems)
            {
                TotalPrice += item.TotalItemPrice;
            }

            TotalPrice = Math.Round(TotalPrice * 100) / 100;
        }

        public Order(int empNum)
        {
            this.empNum = empNum;
        }

        public Order()
        {

        }

        public void CalcTotalPrice()
        {
            foreach (OrderItem item in OrderItems)
            {
                item.UpdateTotalPrice();
                TotalPrice += item.TotalItemPrice;
            }
        }

        public string Serialize() //Making the Json for the order to be put in the database
        {
            string json = JsonConvert.SerializeObject(OrderItems).ToString();

            return json;
        }

        public static List<OrderItem> Deserialize(string json) //Returning the order to the program
        {
            try
            {
                List<OrderItem> theOrder = new List<OrderItem>();
                theOrder = JsonConvert.DeserializeObject<List<OrderItem>>(json);

                return theOrder;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public int addItemToOrder(Item itemToAdd)
        {
            for (int i= 0; i < OrderItems.Count; i++)
            {
                OrderItem inOrder = OrderItems[i];
                if (inOrder.ItemID == itemToAdd.ID && inOrder.CategoryName.Equals(itemToAdd.Category.Name))
                {
                    inOrder.Quantity++;
                    inOrder.UpdateTotalPrice();
                    return i;
                }
            }

            try
            {
                OrderItem newItem = new OrderItem(itemToAdd);
                OrderItems.Add(newItem);
                return OrderItems.Count - 1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int removeItemFromOrder(int orderItemNum)
        {
            OrderItems.RemoveAt(orderItemNum);
            return OrderItems.Count - 1;
        }
    }

    public class OrderItem : IComparable
    {
        public int ItemID;
        public string Name;
        public double Price;
        public int Quantity;
        public double TotalItemPrice;
        public string CategoryName;

        public OrderItem(Item theItem)
        {
            ItemID = theItem.ID;
            this.Name = theItem.Name;
            this.Price = theItem.Price;
            this.Quantity = 1;
            TotalItemPrice = Price * Quantity;
            TotalItemPrice = Math.Round(TotalItemPrice * 100) / 100;
            CategoryName = theItem.Category.Name;
        }

        public OrderItem()
        {

        }

        public void UpdateTotalPrice()
        {
            TotalItemPrice = Price * Quantity;
            TotalItemPrice = Math.Round(TotalItemPrice * 100) / 100;
        }

        public int CompareTo(object obj)
        {
            OrderItem other = (OrderItem)obj;
            return this.ItemID.CompareTo(other.ItemID);
        }
    }
}