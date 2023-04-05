using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_UWP
{
    class User
    {
        private int empID;
        private string firstName, lastName, password;
        public enum EmployeeType
        {
            Employee,
            Manager,
            Admin
        };
        private EmployeeType empType;

        public User(int id, string firstName, string lastName, EmployeeType empType)
        {
            empID = id;
            this.firstName = firstName;
            this.lastName = lastName;
            this.empType = empType;
        }

        public int EmployeeID
        {
            get { return empID; }
            set { empID = value; }
        }

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public EmployeeType EmpType
        {
            get { return empType; }
            set { empType = value; }
        }

    }
}
