using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_UWP
{
    class Database
    {
        private MySqlConnection db = OpenConnection();

        public enum OrderIdentifiers
        {
            OrderNum,
            Price,
            IdTakenBy
        };

        public enum StaffIdentifiers
        {
            EmpId,
            FirstName,
            LastName,
            EmpType
        };

        public enum StockIdentifiers
        {
            ItemID,
            ItemName,
            StockAmount,
            ItemPrice,
            CatNum
        };

        private static MySqlConnection OpenConnection()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
            {
                Server = "localhost",
                Database = "store_system",
                UserID = "root"
            };

            MySqlConnection database = new MySqlConnection(builder.ConnectionString);
            try
            {
                database.Open();
                if (database.Ping())
                {
                    database.Close();
                    return database;
                }
            }
            catch (Exception)
            {
                database.Close();
                return null;
            }

            return null;
        }

        public bool IsConnected()
        {
            if (db != null)
            {
                try
                {
                    db.Open();
                    if (db.Ping())
                    {
                        db.Close();
                        return true;
                    }
                }
                catch (Exception)
                {
                    db.Close();
                    return false;
                }
                db.Close();
            }
            return false;
        }

        public User CheckLogin(string idNum, string password)
        {
            if (db != null)
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                if (password.Equals(""))
                {
                    command.CommandText = "SELECT `first_Name`, `last_Name`, `employee_Type` FROM `staff` WHERE `employee_ID` = '" + idNum + "' AND `password` IS NULL";
                }
                else
                {
                    command.CommandText = "SELECT `first_Name`, `last_Name`, `employee_Type` FROM `staff` WHERE `employee_ID` = '" + idNum + "' AND `password` = '" + password + "'";
                }

                MySqlDataReader result = command.ExecuteReader();

                if (result.Read())
                {
                    int Id = int.Parse(idNum);
                    string firstName = result.GetString(0);
                    string lastName = result.GetString(1);
                    string empType = result.GetString(2);
                    result.Close();

                    User.EmployeeType type = User.EmployeeType.Employee;

                    switch (empType)
                    {
                        case "Employee":
                            type = User.EmployeeType.Employee;
                            break;
                        case "Manager":
                            type = User.EmployeeType.Manager;
                            break;
                        case "Admin":
                            type = User.EmployeeType.Admin;
                            break;
                    }

                    User theUser = new User(Id, firstName, lastName, type);

                    command.CommandText = "COMMIT";
                    command.ExecuteNonQuery();

                    db.Close();

                    return theUser;
                }

                db.Close();
            }

            return null;
        }

        //Left to do adding removing and finding stock

        public List<Category> LoadCategories()
        {
            try
            {
                db.Open();

                List<Category> categories = new List<Category>();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM `category` ORDER BY `category_Num` ASC";
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int catNum = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    Category category = new Category(catNum, name);
                    categories.Add(category);
                }

                reader.Close();

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();

                return categories;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return null;
            }
        }

        public List<Item> LoadItems(Category theCategory)
        {
            try
            {
                db.Open();

                List<Item> items = new List<Item>();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT `item_ID`, `item_Name`, `item_Stock`, `item_Price` FROM `stock` WHERE " +
                    "`category_Num` = '" + theCategory.CatNum + "' ORDER BY `item_ID` ASC";
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    double price = reader.GetDouble(3);
                    price = Math.Round(price * 100) / 100;
                    items.Add(new Item(id, name, stock, price, "", theCategory));
                }

                reader.Close();

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();

                return items;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return null;
            }
        }

        public List<User> LoadUsers()
        {
            try
            {
                db.Open();

                List<User> theUsers = new List<User>();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT `employee_ID`, `first_Name`, `last_Name`, `employee_Type`, `password` FROM `staff` ORDER BY `employee_ID` ASC";
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string firstName = reader.GetString(1);
                    string lastName = reader.GetString(2);
                    string empType = reader.GetString(3);
                    string password;

                    if (reader.IsDBNull(4))
                    {
                        password = "";
                    }
                    else
                    {
                        password = reader.GetString(4);
                    }

                    User tempUser = null;
                    switch (empType.ToLower())
                    {
                        case "admin":
                            tempUser = new User(id, firstName, lastName, User.EmployeeType.Admin);
                            break;
                        case "employee":
                            tempUser = new User(id, firstName, lastName, User.EmployeeType.Employee);
                            break;
                        case "manager":
                            tempUser = new User(id, firstName, lastName, User.EmployeeType.Manager);
                            break;
                    }
                    if (tempUser != null)
                    {
                        tempUser.Password = password;
                        theUsers.Add(tempUser);
                    }
                    else
                    {
                        reader.Close();
                        throw new Exception();
                    }
                }

                reader.Close();

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();

                return theUsers;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return null;
            }
        }

        public int GetStockFor(ref Item theItem)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT `item_Stock` FROM `stock` WHERE `item_ID` = '" + theItem.ID + "'";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    theItem.Stock = reader.GetInt32(0);
                }

                reader.Close();

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();

                return theItem.Stock;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();

                return -1;
            }
        }

        public bool ChangeStock(ref Item theItem, int amountToAdd)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT `item_Stock` FROM `stock` WHERE `item_ID` = '" + theItem.ID + "' FOR UPDATE";
                MySqlDataReader reader = command.ExecuteReader();
                int stock = 0;

                if (reader.Read())
                {
                    stock = reader.GetInt32(0);
                }

                reader.Close();

                stock += amountToAdd;

                command.CommandText = "UPDATE `stock` SET `item_Stock` = '" + stock + "' WHERE `item_ID` = '" + theItem.ID + "'";
                command.ExecuteNonQuery();
                theItem.Stock = stock;


                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();


                db.Close();
                return true;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public int AddOrder(Order theOrder, string Json, User currUser)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO `order_history`(`order_TotalPrice`, `order_Info`, `employee_ID`) " +
                    "VALUES ('" + theOrder.TotalPrice + "', '" + Json + "', '" + theOrder.empNum + "')";
                command.ExecuteNonQuery();

                long ID = command.LastInsertedId;
                int orderID = Convert.ToInt32(ID);

                //PrintReceipt(theOrder, orderID, currUser);

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();
                return orderID;
            }
            catch (Exception e)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return -1;
            }
        }

        /*

        public void PrintReceipt(Order theOrder, int orderNum, User currUser)
        {
            double height = 240;

            string text = "(Company Name)\n" + DateTime.Now.ToString("g") + "\nOrder Number: " + orderNum + "\nEmployee: " + currUser.FirstName +
            " " + currUser.LastName[0] + "\n\t";


            if (File.Exists("LastOrderReceipt.pdf"))
            {
                File.Delete("LastOrderReceipt.pdf");
            }

            Document document1 = new Document();
            document1.Info.Title = "Order Receipt";

            Style style = document1.Styles.Normal;
            style.Font.Name = "Verdana";
            style.Font.Size = 11;

            Section section1 = document1.AddSection();
            section1.PageSetup.PageWidth = Unit.FromPoint(200);
            section1.PageSetup.LeftMargin = Unit.FromPoint(20);
            section1.PageSetup.RightMargin = Unit.FromPoint(20);
            section1.PageSetup.TopMargin = Unit.FromPoint(20);
            section1.PageSetup.BottomMargin = Unit.FromPoint(20);


            Paragraph top = section1.AddParagraph();
            top.Format.Alignment = ParagraphAlignment.Center;
            top.Format.LineSpacingRule = LineSpacingRule.AtLeast;
            top.Format.LineSpacing = Unit.FromPoint(22);
            top.AddText(text);

            style = document1.AddStyle("Tables", "Normal");
            style.Font.Size = 9;
            style.ParagraphFormat.SpaceBefore = Unit.FromPoint(5);
            style.ParagraphFormat.SpaceAfter = Unit.FromPoint(5);

            Table table = section1.AddTable();
            table.Borders.Color = Colors.Black;
            table.Style = "Tables";
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;

            Column column = table.AddColumn(Unit.FromPoint(40));
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn(Unit.FromPoint(80));
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn(Unit.FromPoint(40));
            column.Format.Alignment = ParagraphAlignment.Center;

            Row row = table.AddRow();
            row.Cells[0].AddParagraph("Amount");
            row.Cells[1].AddParagraph("Item Name");
            row.Cells[2].AddParagraph("Item Price");

            foreach (OrderItem item in theOrder.OrderItems)
            {
                row = table.AddRow();
                row.Cells[0].AddParagraph(item.Quantity.ToString());
                row.Cells[1].AddParagraph(item.Name);
                string price = item.Price.ToString();
                if (price.Split('.').Length < 2)
                {
                    price += ".00";
                }
                else
                {
                    if (price.Split('.')[1].Length < 2)
                    {
                        price += "0";
                    }
                }
                row.Cells[2].AddParagraph("£" + price);
                height += 24;
            }

            section1.PageSetup.PageHeight = Unit.FromPoint(height);

            string totalPrice = theOrder.TotalPrice.ToString();
            if (totalPrice.Split('.').Length < 2)
            {
                totalPrice += ".00";
            }
            else
            {
                if (totalPrice.Split('.')[1].Length < 2)
                {
                    totalPrice += "0";
                }
            }

            Paragraph total = section1.AddParagraph("Total Price: £" + totalPrice);
            total.Format.SpaceBefore = Unit.FromPoint(5);
            total.Format.Alignment = ParagraphAlignment.Right;

            PdfDocumentRenderer renderer = new PdfDocumentRenderer();
            renderer.Document = document1;

            renderer.RenderDocument();

            renderer.PdfDocument.Save("LastOrderReceipt.pdf");

        }        

        */

        public Order FindOrder(int OrderNum)
        {
            try
            {
                db.Open();

                Order theOrder = new Order();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT `order_TotalPrice`, `order_Info`, `employee_ID` FROM `order_history` WHERE `order_Num` = '" + OrderNum + "'";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    theOrder.TotalPrice = reader.GetDouble(0);
                    theOrder.TotalPrice = Math.Round(theOrder.TotalPrice * 100) / 100;
                    string info = reader.GetString(1);
                    theOrder.OrderItems = Order.Deserialize(info);
                    theOrder.empNum = reader.GetInt32(2);
                }

                reader.Close();

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();
                return theOrder;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return null;
            }
        }

        public List<Order> FindOrders(OrderIdentifiers searchFor, string searchTerm)
        {
            try
            {
                db.Open();

                List<Order> result = new List<Order>();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                switch (searchFor)
                {
                    case OrderIdentifiers.OrderNum:
                        if (searchTerm.Equals(""))
                        {
                            command.CommandText = "SELECT `order_Num`, `order_TotalPrice`, `order_Info`, `employee_ID` FROM `order_history` ORDER BY `order_Num` ASC";
                        }
                        else
                        {
                            command.CommandText = "SELECT `order_Num`, `order_TotalPrice`, `order_Info`, `employee_ID` FROM `order_history` WHERE `order_Num` = '" + searchTerm + "' ORDER BY `order_Num` ASC";
                        }
                        break;
                    case OrderIdentifiers.Price:
                        command.CommandText = "SELECT `order_Num`, `order_TotalPrice`, `order_Info`, `employee_ID` FROM `order_history` WHERE `order_TotalPrice` LIKE '" + searchTerm + "' ORDER BY `order_Num` ASC";
                        break;
                    case OrderIdentifiers.IdTakenBy:
                        command.CommandText = "SELECT `order_Num`, `order_TotalPrice`, `order_Info`, `employee_ID` FROM `order_history` WHERE `employee_ID` = '" + searchTerm + "' ORDER BY `order_Num` ASC";
                        break;
                    default:
                        throw new Exception();
                }

                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Order theOrder = new Order();
                    theOrder.OrderNum = reader.GetInt32(0);
                    theOrder.TotalPrice = reader.GetDouble(1);
                    theOrder.TotalPrice = Math.Round(theOrder.TotalPrice * 100) / 100;
                    string info = reader.GetString(2);
                    theOrder.OrderItems = Order.Deserialize(info);
                    theOrder.empNum = reader.GetInt32(3);
                    result.Add(theOrder);
                }

                reader.Close();

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();
                return result;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return null;
            }
        }

        public bool AddItem(ref Item theItem)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM `stock` WHERE `item_Name` = '" + theItem.Name + "' AND `category_Num`='" + theItem.Category.CatNum + "'";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read() == false) //Item doesnt already exist
                {
                    reader.Close();
                    command.CommandText = "INSERT INTO `stock` (`item_Name`,`item_Stock`,`item_Price`,`category_Num`) " +
                        "VALUES ('" + theItem.Name + "', '" + theItem.Stock + "', '" + theItem.Price + "', '" + theItem.Category.CatNum + "')";
                    command.ExecuteNonQuery();

                    long Id = command.LastInsertedId;
                    theItem.ID = Convert.ToInt32(Id);
                }
                else //Does Exist
                {
                    reader.Close();
                    throw new Exception();
                }

                reader.Close();

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();
                return true;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public bool AddItem(string itemName, int itemStock, double itemPrice, ref Category category)
        {
            try
            {
                db.Open();
                Item theItem = null;

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM `stock` WHERE `item_Name` = '" + itemName + "' AND `category_Num`='" + category.CatNum + "'";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read() == false) //Item doesnt already exist
                {
                    reader.Close();
                    command.CommandText = "INSERT INTO `stock`(`item_Name`,`item_Stock`,`item_Price`,`category_Num`) VALUES ('" + itemName + "','"
                        + itemStock + "','" + itemPrice + "','" + category.CatNum + "')";
                    command.ExecuteNonQuery();

                    long Id = command.LastInsertedId;
                    int ItemId = Convert.ToInt32(Id);
                    theItem = new Item(ItemId, itemName, itemStock, itemPrice, null, category);

                }
                else //Does Exist
                {
                    reader.Close();
                    throw new Exception();
                }

                reader.Close();

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                if (theItem != null)
                {
                    category.addItem(theItem);
                }

                db.Close();
                return true;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public bool RemoveItem(int ItemId)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM `stock` WHERE `item_ID` = '" + ItemId + "'";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read()) //Checks if item exists
                {
                    reader.Close();

                    command.CommandText = "DELETE FROM `stock` WHERE `item_ID` = '" + ItemId + "'";
                    command.ExecuteNonQuery();

                    command.CommandText = "COMMIT";
                    command.ExecuteNonQuery();

                    db.Close();
                    return true;
                }

                reader.Close();

                throw new Exception();
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public bool UpdateItem(ref Item theItem, string name, double price, Category category)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM `stock` WHERE `item_ID` = '" + theItem.ID + "' FOR UPDATE";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read()) //Checks if item exists
                {
                    reader.Close();

                    command.CommandText = "UPDATE `stock` SET `item_Name`='" + name + "',`item_Price`='" + price + "'," +
                        "`category_Num`='" + category.CatNum + "' WHERE `item_ID`='" + theItem.ID + "'";
                    command.ExecuteNonQuery();

                    theItem.Name = name;
                    theItem.Price = price;

                    if (theItem.Category.Equals(category) == false)
                    {
                        theItem.Category.removeItem(theItem);
                        theItem.Category = category;
                        theItem.Category.addItem(theItem);
                    }

                    command.CommandText = "COMMIT";
                    command.ExecuteNonQuery();

                    db.Close();
                    return true;
                }

                reader.Close();

                throw new Exception();
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public bool AddCategory(ref Category theCategory) //UNUSED
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM `category` WHERE `category_Name` = '" + theCategory.Name + "'";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read()) //Category already exists
                {
                    reader.Close();

                    command.CommandText = "ROLLBACK";
                    command.ExecuteNonQuery();

                    db.Close();
                    return false;
                }

                reader.Close();

                command.CommandText = "SELECT * FROM `category` WHERE `category_Num` = '" + theCategory.CatNum + "'";
                reader = command.ExecuteReader();   //Checks if the category number is taken

                if (reader.Read()) //Is taken
                { //Lets the database assign the category number
                    reader.Close();

                    command.CommandText = "INSERT INTO `category`(`category_Name`) VALUES ('" + theCategory.Name + "')";
                    command.ExecuteNonQuery();

                    long catNum = command.LastInsertedId;
                    theCategory.CatNum = Convert.ToInt32(catNum);

                    reader.Close();
                }
                else //Isnt taken
                { //Takes the category number assigned by the user
                    reader.Close();

                    command.CommandText = "INSERT INTO `category`(`category_Num`, `category_Name`) VALUES ('" + theCategory.CatNum + "', '" + theCategory.Name + "')";
                    command.ExecuteNonQuery();
                }

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();
                return true;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public bool AddCategory(string CatName, ref List<Category> categories)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM `category` WHERE `category_Name` = '" + CatName + "'";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read()) //Category already exists
                {
                    reader.Close();

                    throw new Exception();
                }

                reader.Close();

                command.CommandText = "INSERT INTO `category`(`category_Name`) VALUES ('" + CatName + "')";
                command.ExecuteNonQuery();

                long id = command.LastInsertedId;
                int CatNum = Convert.ToInt32(id);

                Category theCategory = new Category(CatNum, CatName);
                categories.Add(theCategory);


                categories.Sort((a, b) => a.CatNum.CompareTo(b.CatNum));

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();
                return true;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public bool RemoveCategory(int CatNum)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                //Checks no items are assigned to that category
                command.CommandText = "SELECT * FROM `stock` WHERE `category_Num` = '" + CatNum + "'";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    reader.Close();
                    throw new Exception();
                }

                reader.Close();

                command.CommandText = "SELECT * FROM `category` WHERE `category_Num` = '" + CatNum + "'";
                reader = command.ExecuteReader();

                if (reader.Read()) //Checks if category exists
                {
                    reader.Close();

                    command.CommandText = "DELETE FROM `category` WHERE `category_Num` = '" + CatNum + "'";
                    command.ExecuteNonQuery();

                    command.CommandText = "COMMIT";
                    command.ExecuteNonQuery();

                    db.Close();
                    return true;
                }

                reader.Close();

                throw new Exception();
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public bool UpdateCategory(ref Category theCategory, string name)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM `category` WHERE `category_Name`='" + name + "'";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read()) //Checks if category with that name already exists
                {
                    reader.Close();

                    throw new Exception();
                }

                reader.Close();

                command.CommandText = "SELECT * FROM `category` WHERE `category_Num` = '" + theCategory.CatNum + "'";
                reader = command.ExecuteReader();

                if (reader.Read()) //Checks if category exists
                {
                    reader.Close();

                    command.CommandText = "UPDATE `category` SET `category_Name`='" + name + "' WHERE `category_Num`='" + theCategory.CatNum + "'";
                    command.ExecuteNonQuery();

                    command.CommandText = "COMMIT";
                    command.ExecuteNonQuery();

                    theCategory.Name = name;

                    db.Close();
                    return true;
                }

                reader.Close();

                throw new Exception();
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public bool AddUser(ref User theUser)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT `employee_ID` FROM `staff` WHERE `employee_ID` = '" + theUser.EmployeeID + "'";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read()) //Employee Id taken
                {
                    reader.Close();

                    command.CommandText = "INSERT INTO `staff` (`first_Name`, `last_Name`, `employee_Type`) " +
                        "VALUES ('" + theUser.FirstName + "', '" + theUser.LastName + "', '" + theUser.EmpType.ToString() + "')";
                    command.ExecuteNonQuery();

                    long IdNum = command.LastInsertedId;
                    theUser.EmployeeID = Convert.ToInt32(IdNum);
                }
                else
                {
                    reader.Close();

                    command.CommandText = "INSERT INTO `staff` (`employee_ID`, `first_Name`, `last_Name`, `employee_Type`) " +
                        "VALUES ('" + theUser.EmployeeID + "', '" + theUser.FirstName + "', '" + theUser.LastName + "', '" + theUser.EmpType.ToString() + "')";
                    command.ExecuteNonQuery();
                }

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();
                return true;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public User AddUser(string firstname, string lastname, string type, string password)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                if (password.Equals(""))
                {
                    command.CommandText = "INSERT INTO `staff`(`first_Name`, `last_Name`, `employee_Type`, `password`) " +
                        "VALUES ('" + firstname + "','" + lastname + "','" + type + "', NULL)";
                }
                else
                {
                    command.CommandText = "INSERT INTO `staff`(`first_Name`, `last_Name`, `employee_Type`, `password`) " +
                        "VALUES ('" + firstname + "','" + lastname + "','" + type + "','" + password + "')";
                }
                command.ExecuteNonQuery();

                long id = command.LastInsertedId;
                int empId = Convert.ToInt32(id);

                User temp = null;

                switch (type.ToLower())
                {
                    case "employee":
                        temp = new User(empId, firstname, lastname, User.EmployeeType.Employee);
                        temp.Password = password;
                        break;
                    case "manager":
                        temp = new User(empId, firstname, lastname, User.EmployeeType.Manager);
                        temp.Password = password;
                        break;
                    case "admin":
                        temp = new User(empId, firstname, lastname, User.EmployeeType.Admin);
                        temp.Password = password;
                        break;
                }

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();
                return temp;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return null;
            }
        }

        public bool RemoveUser(int IdNum)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM staff WHERE `employee_ID` = '" + IdNum + "' FOR UPDATE";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    reader.Close();
                    command.CommandText = "DELETE FROM staff WHERE `employee_ID` = '" + IdNum + "'";
                    command.ExecuteNonQuery();

                    command.CommandText = "COMMIT";
                    command.ExecuteNonQuery();
                }
                else
                {
                    reader.Close();

                    throw new Exception();
                }

                db.Close();
                return true;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public bool UpdateUser(ref User theUser, string firstname, string lastname, string type, string password)
        {
            try
            {
                db.Open();

                MySqlCommand command = db.CreateCommand();

                command.CommandText = "START TRANSACTION";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM `staff` WHERE `employee_ID`='" + theUser.EmployeeID + "'";
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    reader.Close();

                    if (password.Equals(""))
                    {

                        command.CommandText = "UPDATE `staff` SET `first_Name`='" + firstname + "',`last_Name`='" + lastname + "'," +
                            "`employee_Type`='" + type + "',`password`= NULL WHERE `employee_ID`='" + theUser.EmployeeID + "'";
                    }
                    else
                    {
                        command.CommandText = "UPDATE `staff` SET `first_Name`='" + firstname + "',`last_Name`='" + lastname + "'," +
                            "`employee_Type`='" + type + "',`password`='" + password + "' WHERE `employee_ID`='" + theUser.EmployeeID + "'";
                    }
                    command.ExecuteNonQuery();
                }
                else
                {
                    reader.Close();

                    throw new Exception();
                }

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                theUser.FirstName = firstname;
                theUser.LastName = lastname;
                theUser.Password = password;

                switch (type.ToLower())
                {
                    case "employee":
                        theUser.EmpType = User.EmployeeType.Employee;
                        break;
                    case "manager":
                        theUser.EmpType = User.EmployeeType.Manager;
                        break;
                    case "admin":
                        theUser.EmpType = User.EmployeeType.Admin;
                        break;
                }

                db.Close();
                return true;
            }
            catch (Exception)
            {
                MySqlCommand command = db.CreateCommand();

                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();

                db.Close();
                return false;
            }
        }

        public User FindUser(int IdNum)
        {
            db.Open();

            MySqlCommand command = db.CreateCommand();

            command.CommandText = "START TRANSACTION";
            command.ExecuteNonQuery();

            command.CommandText = "SELECT * FROM staff WHERE `employee_ID` = '" + IdNum + "'";
            MySqlDataReader result = command.ExecuteReader();

            if (result.Read())
            {
                int Id = IdNum;
                string firstName = result.GetString(0);
                string lastName = result.GetString(1);
                string empType = result.GetString(2);
                result.Close();

                User.EmployeeType type = User.EmployeeType.Employee;

                switch (empType)
                {
                    case "Employee":
                        type = User.EmployeeType.Employee;
                        break;
                    case "Manager":
                        type = User.EmployeeType.Manager;
                        break;
                    case "Admin":
                        type = User.EmployeeType.Admin;
                        break;
                }

                User theUser = new User(Id, firstName, lastName, type);

                command.CommandText = "COMMIT";
                command.ExecuteNonQuery();

                db.Close();

                return theUser;
            }

            db.Close();
            return null;

        }

        public void ManualCommand()
        {
            db.Open();

            MySqlCommand command = db.CreateCommand();

            command.CommandText = "START TRANSACTION";
            command.ExecuteNonQuery();

            Console.WriteLine("Enter Commands Below: ");
            string input = Console.ReadLine();

            command.CommandText = input;
            command.ExecuteNonQuery();

            command.CommandText = "COMMIT";
            command.ExecuteNonQuery();

            db.Close();
        }
    }
}