using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mars_Restaurant
{
    public class Employee
    {
        private long _employeeID;
        private string _password;
        private NameOfPerson _employeeName = new NameOfPerson();
        private string _email;

        private List<Table> _tablesAssignedTo = new List<Table>();    
        private double _payPerHour;
        private bool _paidThisBillingCycle;

        public string getPassword()
        {
            return _password;
        }

        public void setPassword(string pass)
        {
            _password = pass;
        }

        public string getEmail()
        {
            return _email;
        }

        public void setEmail(string email)
        {
            _email = email;
        }

        public long getEmployeeID()
        {
            return _employeeID;
        }

        public void setEmployeeID(long ID)
        {
            _employeeID = ID;
        }

        public NameOfPerson getEmployeeName()
        {
            return _employeeName;
        }

        public void setEmployeeName(NameOfPerson name)
        {
            _employeeName = name;
        }

        public int getAttribute()
        {
            return 0;
        }

        public void setAttribute(int val)
        {

        }

        public void deepCopyTablesAssignedTo()
        {

        }

        public void setTableAssignedTo(ref List<Table> table)
        {

        }

        public int checkOutTable(int tableNumber)
        {
            return 0;
        }

        public void addItemsToTableOrder(int items, int tableNumber)
        {

        }

        public bool loginOrLockSystemIfFailed()
        {
            return true;
        }

        public bool isSessionValid()
        {
            return true;
        }

        public bool resetPassword()
        {
            return true;
        }

        public double getPayPerHour()
        {
            return _payPerHour;
        }

        public void setPayPerHour(double payPerHour)
        {
            _payPerHour = payPerHour;
        }

        public bool wasPaidThisBillingCycle()
        {
            return _paidThisBillingCycle;
        }

        public void setPaidFlagForThisBillingCycle(bool paid)
        {
            _paidThisBillingCycle = paid;
        }
    }
}
