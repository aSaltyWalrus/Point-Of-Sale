using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_UWP
{
    class Users
    {
        List<User> userList;

        public Users()
        {
            this.userList = new List<User>();
        }

        public List<User> UserList
        {
            get { return userList; }
            set { userList = value; }
        }

        public User getUserByID(int ID)
        {
            foreach (User U in userList)
            {
                if (U.EmployeeID == ID)
                    return U;
            }
            return null;
        }

        public int addUser(User user)
        {
            foreach (User u in userList)
            {
                if (u.EmployeeID == user.EmployeeID)
                    return 0; // id already taken
            }
            userList.Add(user);
            return 1;
        }

        public int removeUser(int ID)
        {
            foreach (User u in userList)
            {
                if (u.EmployeeID == ID)
                {
                    userList.Remove(u);
                    return 1;
                }
            }
            return 0; // id doesnt exist
        }
    }
}
