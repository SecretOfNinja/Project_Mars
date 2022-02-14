using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using System.Threading;

namespace Mars_Restaurant
{
    // This class handles user log ins.
    public partial class LoginForm : Form
    {
        public int _state = 0;
        public bool _managerLoggedIn = false; //Set to true if a manager is logged in
        public List<WaitStaff> _employees = new List<WaitStaff>();
        public List<StaffManager> _managers = new List<StaffManager>();
        public string _connectionString = @"Server=127.0.0.1:1443;Database=MarsRestaurant;User Id=sqlserver;Password=Blingdenstone42!";
        public string _loggedInPersonsName, _loggedInEmail = "";
        private long _logOnTime, _logOffTime; //held in DateTime.Now.Ticks
        public int _timeOutPeriodInSeconds = 5 * 60; //If there is a period of activity by a user longer than this, then the usered is logged out automatically


        // -------------------------------- 0 -------------------------------------------------

        // This is the constructor for the LoginForm.
        // This is the starting point of the program
        public LoginForm()
        {
            InitializeComponent();
            lblSplashName.Parent = picBoxSplash;
            lblMarsRestaurant.Parent = picBoxSplash;
            picBoxSplash.Visible = false;
            createConnectionString();
            removeNonLoggedInUsersFromTokens();
            removeTimedOutUsers();
            loadWaitStaffFromDatabase();
            loadManagersFromDatabase();
            // Fix the window size
            this.MinimumSize = new Size(this.Width, this.Height);
            this.MaximumSize = new Size(this.Width, this.Height);
        }

        // -------------------------------- 1 -------------------------------------------------

        // This checks whether the user with the email value passed in
        // is logged on.  It checks the LoggedIn table of the database
        // It returns true if he is, false if he isn't.
        public bool isUserLoggedIn(String email)
        {
            string connectionString;
            SqlDataReader dataReader;
            bool result = false;

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "isUserLoggedIn");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select * From LoggedIn Where Email=@email";
                    command.Parameters.AddWithValue("@email", email);
                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    //If there is an entry, then the user is logged in
                    while (dataReader.Read())
                    {
                        result = true;
                    }

                    dataReader.Close();
                    command.Dispose();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result = false;
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
            return result;
        }


        // -------------------------------- 2 -------------------------------------------------

        //Pass in the email of the current user
        //return true if the current user was logged off, else return false
        public void removeTimedOutUsers()
        {
            double timeOut = (double)_timeOutPeriodInSeconds;// 15.0 * 60.0; //15 minutes
            long lastActive; // time last active
            String email;
            string connectionString;
            SqlDataReader dataReader;
            DateTime dt2 = DateTime.UtcNow;
            DateTime dt1;
            List<String> timedOutUsers = new List<string>();

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "removeTimedOutUsers");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select * From LoggedIn";
                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        email = (String)dataReader.GetValue(0);
                        lastActive = (long)dataReader.GetValue(1); // x position of table
                        dt1 = new DateTime(lastActive);
                        //Delta Time
                        double deltaTime;
                        deltaTime = (dt2 - dt1).TotalSeconds;
                        //Save timed out users to list
                        if (deltaTime > timeOut)
                        {
                            timedOutUsers.Add(email);
                        }

                    }
                    dataReader.Close();
                    command.Dispose();

                    //If a user has been timed out, log him out, i.e. remove him from the database as a current user
                    foreach (String eml in timedOutUsers)
                    {
                        command = connection.CreateCommand();
                        command.Connection = connection;
                        command.Transaction = transaction;

                        //Free slots where users' entries are in Tokens table
                        command.CommandText = "Update Tokens Set Using='free' Where Using=@using";
                        command.Parameters.Add("@using", SqlDbType.NVarChar);
                        command.Parameters["@using"].Value = eml;
                        command.ExecuteNonQuery();

                        command = connection.CreateCommand();
                        command.Connection = connection;
                        command.Transaction = transaction;

                        //Delete the users from the LoggedInTable
                        command.CommandText = "Delete From LoggedIn Where Email=@email";
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@email", eml);
                        command.ExecuteNonQuery();
                    }


                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
        }

        // -------------------------------- 3 -------------------------------------------------


        // This returns a list of logged in users from the LoggedIn table 
        // of the database.  The returned List is a list of emails of the users.
        private List<String> getLoggedInUsers()
        {
            String email;
            List<String> loggedInUsers = new List<String>();
            string connectionString;
            SqlDataReader dataReader;

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "getLoggedInUsers");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select * From LoggedIn";
                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    //If there is an entry, then the user is logged in
                    while (dataReader.Read())
                    {
                        email = (string)dataReader.GetValue(0);
                        loggedInUsers.Add(email);
                    }

                    dataReader.Close();
                    command.Dispose();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
            return loggedInUsers;
        }


        // -------------------------------- 4 -------------------------------------------------

        // This returns the state of all dining tables, i.e. whether it is free or in use
        // It returns an ordered list of the Using state for each of the 20 dining tables
        private String[] getTokensTableUsingStates()
        {
            List<String> usingValues2 = new List<String>();
            String[] usingValues = new string[20];
            string email, diningTable;
            string connectionString;
            SqlDataReader dataReader;


            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "getTokensTableUsingStates");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select * From Tokens";

                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    //If there is an entry, then the user is logged in
                    while (dataReader.Read())
                    {
                        diningTable = (string)dataReader.GetValue(0);
                        email = (string)dataReader.GetValue(1);

                        String tmp = diningTable.Substring(11);
                        int val;
                        if (int.TryParse(tmp, out val) == true)
                            usingValues[val - 1] = email;
                    }

                    dataReader.Close();
                    command.Dispose();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
            return usingValues;
        }

        // -------------------------------- 5 -------------------------------------------------

        //Free the slot for the table number passed in.
        // It updates the Tokens table for the dining table number passed in
        // and enters the value "free" to indicate the slot is free.
        public void freeTokensTableNumberSlot(int tableNumber)
        {
            string connectionString;
            String diningTable = "diningTable";

            //Table number must be in the range of 1 to 20
            if (tableNumber >= 1 && tableNumber <= 20)
            {
                diningTable += tableNumber.ToString();
            }
            else
            {
                return;
            }


            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "freeTokensTableNumberSlot");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    //Free slots where users' entries are in Tokens table
                    command.CommandText = "Update Tokens Set Using='free' Where Resource=@tableNumber";
                    command.Parameters.Add("@tableNumber", SqlDbType.NVarChar);
                    command.Parameters["@tableNumber"].Value = diningTable;
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
        }

        // -------------------------------- 6 -------------------------------------------------

        // This function cleans the Tokens table of users who are not logged in.
        //  It's a clean up function if the table ends up in a bad state. 
        private void removeNonLoggedInUsersFromTokens()
        {
            bool found = false;
            string email = "";
            //1) Get a list of emails of those who are logged in
            List<String> loggedInUsers = getLoggedInUsers();
            //2) Look at each of the 20 dining tables in the Tokens table and determine
            String[] usingValues = getTokensTableUsingStates();
            for (int i = 0; i < 20; i++)
            {
                found = false;
                email = usingValues[i];
                if (email != "free")
                {
                    foreach (String val in loggedInUsers)
                    {
                        if (val == email)
                        {
                            found = true;
                            break;
                        }
                    }
                    //3) If the person is not logged in, i.e. he's not found, then free up the slot
                    if (found == false)
                    {
                        freeTokensTableNumberSlot(i + 1);
                    }
                }
            }

        }


        // -------------------------------- 7 -------------------------------------------------

        // This creates the connection string for the remote SQL Server and 
        // mars restaurant database.   This string is used throughout the
        // entire program.
        private void createConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "starthree.database.windows.net"; // "startwo.database.windows.net";// "<your_server.database.windows.net>";
            builder.UserID = "adminStarThree";// "<your_username>";
            builder.Password = "Blingdenstone43!";
            builder.InitialCatalog = "MarsRestaurant_2021-07-06T18-28Z"; // "MarsRestaurant"; // "<your_database>";
            _connectionString = builder.ConnectionString;
        }



        // -------------------------------- 8 -------------------------------------------------

        // This loads the Managers from the Managers table of the database.
        // It inserts them into the "_managers" List control.
        private void loadManagersFromDatabase()
        {
            string firstName, middleName, lastName, email, password;
            string connectionString;
            SqlDataReader dataReader;

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "SelectManagers");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select * From Managers";

                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        StaffManager mangr = new StaffManager();
                        NameOfPerson nop = new NameOfPerson();

                        firstName = (string)dataReader.GetValue(0);
                        middleName = (string)dataReader.GetValue(1);
                        lastName = (string)dataReader.GetValue(2);
                        email = (string)dataReader.GetValue(3);
                        password = (string)dataReader.GetValue(4);

                        nop.setFirstName(firstName);
                        nop.setMiddleName(middleName);
                        nop.setLastName(lastName);
                        mangr.setEmployeeName(nop);
                        mangr.setEmail(email);
                        mangr.setPassword(password);
                        _managers.Add(mangr);
                    }

                    dataReader.Close();
                    command.Dispose();
                    // connection.Close();
                    transaction.Commit();
                    Console.WriteLine("Selection from managers table worked.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }

            if (_managers.Count > 0)
            {
                btnCreateAcctForm.Visible = false;
                btnCreateAcctForm.Enabled = false;
            }
            else
            {
                btnCreateAcctForm.Visible = true;
                btnCreateAcctForm.Enabled = true;
            }
        }


        // -------------------------------- 9 -------------------------------------------------

        // This loads the wait staff employees from the WaitStaff table of the database.
        // It inserts them into the "_employees" List control.
        private void loadWaitStaffFromDatabase()
        {
            string firstName, middleName, lastName, email, password, assignedTables;
            string connectionString;
            SqlDataReader dataReader;
            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.                
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "SelectWaitStaff");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    //WITH (TABLOCKX)
                    command.CommandText = "Select * From WaitStaff "; // With (TABLOCKX)";

                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        WaitStaff emp = new WaitStaff();
                        NameOfPerson nop = new NameOfPerson();

                        firstName = (string)dataReader.GetValue(0);
                        middleName = (string)dataReader.GetValue(1);
                        lastName = (string)dataReader.GetValue(2);
                        email = (string)dataReader.GetValue(3);
                        password = (string)dataReader.GetValue(4);
                        assignedTables = (string)dataReader.GetValue(5);

                        nop.setFirstName(firstName);
                        nop.setMiddleName(middleName);
                        nop.setLastName(lastName);
                        emp.setEmployeeName(nop);
                        emp.setEmail(email);
                        emp.setPassword(password);
                        _employees.Add(emp);
                    }

                    dataReader.Close();
                    command.Dispose();
                    transaction.Commit();
                    Console.WriteLine("Selection from managers table worked.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
        }


        // -------------------------------- 10 -------------------------------------------------

        // This function makes sure that the email and password entered by the user loggin in
        // match an employee or manager in the database.  It returns true if successful, else false.
        private bool matchLogin(string email, string password)
        {
            if (_employees.Count != 0)
            {
                foreach (Employee emp in _employees)
                {
                    if (emp.getEmail().ToLower() == email.ToLower() && password == emp.getPassword())
                    {
                        _managerLoggedIn = false;
                        return true;
                    }
                }
            }
            if (_managers.Count != 0)
            {
                foreach (StaffManager mangr in _managers)
                {
                    if (mangr.getEmail().ToLower() == email.ToLower() && password == mangr.getPassword())
                    {
                        _managerLoggedIn = true;
                        return true;
                    }
                }
            }
            return false;
        }


        // -------------------------------- 11 -------------------------------------------------
 
        // This function looks for the email and password paramters to see if the are in the database
        // If successful, it returns a string with the first, middle and last name of the employee
        // If unsuccessful, it returns an empty string.
        private string findEmployee(string email, string password)
        {
            if (_employees.Count != 0)
            {
                foreach (Employee emp in _employees)
                {
                    if (emp.getEmail().ToLower() == email.ToLower() && password == emp.getPassword())
                    {
                        string name;
                        NameOfPerson nop;
                        nop = emp.getEmployeeName();

                        if (nop.getMiddleName() == "")
                        {
                            name = nop.getFirstName() + " " + nop.getLastName();
                        }
                        else
                        {
                            name = nop.getFirstName() + " " + nop.getMiddleName() + " " + nop.getLastName();
                        }

                        return name;
                    }
                }
            }
            if (_managers.Count != 0)
            {
                foreach (Employee emp in _managers)
                {
                    if (emp.getEmail().ToLower() == email.ToLower() && password == emp.getPassword())
                    {
                        string name;
                        NameOfPerson nop;
                        nop = emp.getEmployeeName();

                        if (nop.getMiddleName() == "")
                        {
                            name = nop.getFirstName() + " " + nop.getLastName();
                        }
                        else
                        {
                            name = nop.getFirstName() + " " + nop.getMiddleName() + " " + nop.getLastName();
                        }

                        return name;
                    }
                }
            }

            return "";
        }

        // -------------------------------- 12 -------------------------------------------------

        // This updates the time to the current time for the logged in user in the LoggedIn table.
        // The email address is the user to be update.  
        public void updateLoggedInTime(String email)
        {
            DateTime dt = DateTime.UtcNow; // Gets the current time in Universal Time (greenwich mean time)
            string connectionString;

            connectionString = _connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "updateLoggedInTime");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.Connection = connection;
                    command.Transaction = transaction;
                    command.CommandText =
                        "Update LoggedIn Set LastActive=@lastActive Where Email=@email";

                    command.Parameters.Add("@lastActive", SqlDbType.BigInt);
                    command.Parameters.Add("@email", SqlDbType.NVarChar);

                    command.Parameters["@lastActive"].Value = dt.Ticks; ;
                    command.Parameters["@email"].Value = email;

                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
        }

        // -------------------------------- 13 -------------------------------------------------

        //Determines if the user is already logged in, if not, then inserted into the LoggedIn table
        // is the email of the user and the universal time when he logged in.  Returns true of the
        // users is logged in, else false.
        private bool alreadyLoggedIn(string email)
        {
            //Use stored procedure ... pass in the email and current time in utc time (universal time)
            string extractedEmail = "";
            string connectionString;
            SqlDataReader dataReader;
            bool result = true;
            DateTime dt = DateTime.UtcNow; // Gets the current time in Universal Time (greenwich mean time)

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "alreadyLoggedIn");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select Email From LoggedIn Where Email=@email";
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@email", email);
                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        extractedEmail = (string)dataReader.GetValue(0);
                    }

                    dataReader.Close();
                    command.Dispose();
                    //If this string is empty, then you can log in
                    if (extractedEmail == "")
                    {
                        command = connection.CreateCommand();
                        result = false;
                        command.Connection = connection;
                        command.Transaction = transaction;
                        command.CommandText = "Insert into loggedIn (Email, lastActive) VALUES (@email, @lastActive)"; 

                        command.CommandType = CommandType.Text;

                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@lastActive", dt.Ticks);

                        command.ExecuteNonQuery();
                    }


                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    result = true;
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
            return result;
        }

        // -------------------------------- 14 -------------------------------------------------

        // This deletes the user with the email value passed in from the LoggedIn table.
        private void deleteUserFromLoggedInTable(String email)
        {
            string connectionString;

            connectionString = _connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "deleteUserFromLoggedInTable");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {

                    command.Connection = connection;
                    command.Transaction = transaction;


                    command.CommandText = "Delete From loggedIn Where Email = @email";

                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@email", email);

                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
        }

        // -------------------------------- 15 -------------------------------------------------

        // After the user clicks on the "Login" button, this will log a user in
        // according to the values he has typed into the "Email" and "Password" text boxes.
        // If the information is not valid, he will not be logged in.  If valid, the
        // user either logs on as an "admin" or as an employee or manager.
        // This also runs a splash screen for logging on and off for employees and managers
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtBxEmail.Text == "")
            {
                return;
            }
            else if (txtBxEmail.Text.ToLower() == "admin" && txtBxPassword.Text == "password") //Launch the Admin form
            {
                AdminForm f = new AdminForm(this);
                f.ShowDialog(this);
                if (_managers.Count == 0)
                {
                    btnCreateAcctForm.Enabled = true;
                    btnCreateAcctForm.Visible = true;
                }
                else
                {
                    btnCreateAcctForm.Enabled = false;
                    btnCreateAcctForm.Visible = false;
                }
                return;
            }
            else if (matchLogin(txtBxEmail.Text, txtBxPassword.Text) == true)
            {
                if (alreadyLoggedIn(txtBxEmail.Text.ToLower()) == true)
                {
                    MessageBox.Show("This user is already logged in elsewhere");
                    return;
                }
                string name = findEmployee(txtBxEmail.Text, txtBxPassword.Text);
                // Make Splash Screen Visible
                picBoxSplash.Visible = true;
                picBoxSplash.BringToFront();
                lblSplashName.Text = "Welcome: " + name;
                this.Refresh();

                _logOnTime = DateTime.UtcNow.Ticks; // DateTime.Now.Ticks;
                Thread.Sleep(3000);

                _loggedInPersonsName = name;
                _loggedInEmail = txtBxEmail.Text;
                DoubleBufferedWindow dbw = new DoubleBufferedWindow(this);
                _state = 2;
                picBoxSplash.Visible = false;
                picBoxSplash.SendToBack();
                dbw.ShowDialog();


                // Display splash screen for logging off
                picBoxSplash.Visible = true;
                picBoxSplash.BringToFront();
                lblSplashName.Text = "Goodbye: " + name;
                this.Refresh();
                _logOffTime = DateTime.UtcNow.Ticks; // DateTime.Now.Ticks;
                addWorkTimeToDatabase(txtBxEmail.Text, _logOnTime, _logOffTime);
                Thread.Sleep(3000);
                picBoxSplash.Visible = false;
                picBoxSplash.SendToBack();
                deleteUserFromLoggedInTable(txtBxEmail.Text.ToLower());
            }
            else
            {
                MessageBox.Show("Logon Information Is Wrong");
            }
        }

        // -------------------------------- 16 -------------------------------------------------


        private void btnExit_Click(object sender, EventArgs e)
        {
           
            this.Close();
        }


        // -------------------------------- 17 -------------------------------------------------

        // The user clicks on the "Create Account" button, which is only present when
        // there is no manager.   This launches the "CreateAccountFrom" so that a
        // manager account can be created.
        private void btnCreateAcctForm_Click(object sender, EventArgs e)
        {
            CreateAccountForm f = new CreateAccountForm(this);
            f.ShowDialog(this);
            if (_managers.Count > 0)
            {
                btnCreateAcctForm.Visible = false;
                btnCreateAcctForm.Enabled = false;
            }
        }

        // -------------------------------- 18 -------------------------------------------------

        // Inserted hours/minutes worked for this session.  Used the DateTime.Now.Ticks to store the times
        // Pass the users email address.  Also loginTime in Ticks and logoutTime in ticks are passed to the function
        // This is inserted into the WorkTime table in the databse.
        private void addWorkTimeToDatabase(string email, long loginTime, long logoutTime)
        {
            string connectionString;
            connectionString = _connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "addWorkTimeToDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction

                try
                {
                    command.Connection = connection;
                    command.Transaction = transaction;


                    command.CommandText =
                        "Insert into WorkTime (email,login, logout) VALUES (@email, @login, @logout)";

                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@login", loginTime);
                    command.Parameters.AddWithValue("@logout", logoutTime);
                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
        }
    }
}
