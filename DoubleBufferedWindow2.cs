using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;

namespace Mars_Restaurant
{
    // The DoubleBuffedWindow class is the heart of the application.  It contains most of the forms
    // such as the Menu edit form, and the Invoices form, and employees form.
    // This is part of the same class as is in the file DoubleBufferedWindows.cs.
    public partial class DoubleBufferedWindow : Form
    {

        //------------------------------------------------- 0 ----------------------------------------------------------

        // This accesses the TableInfo table of the database and fills out the
        // data for each of the dining tables in the "tables" List collection
        private void loadTableInfoFromDatabase()
        {
            int tableNumber, xPos, yPos, state, onFloor, imageNumber;
            string connectionString;
            SqlDataReader dataReader;

            connectionString = _parentForm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "LoadTableInfoFromDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select * From TableInfo";                  
                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        tableNumber = (int)dataReader.GetValue(0); 
                        xPos = (int)dataReader.GetValue(1); // x position of table
                        yPos = (int)dataReader.GetValue(2); // y position of table
                        state = (int)dataReader.GetValue(3); // is table empty, used or busy
                        onFloor = (int)dataReader.GetValue(4); // Is the table being used
                        imageNumber = (int)dataReader.GetValue(5); // table image number

                        tables[tableNumber - 1].setTablePosition(xPos, yPos);
                        tables[tableNumber - 1].setTableState((TableState)state);
                        tables[tableNumber - 1].OnTheFloor = false;

                        Table.ImageIndex = imageNumber;
                        tables[tableNumber - 1].AddImage(imageListTables.Images[Table.ImageIndex]);
                        if (onFloor == 1)
                        {
                            tables[tableNumber - 1].OnTheFloor = true;
                        }
                        if (onFloor == 1)
                        {
                            tables[tableNumber - 1].enableGui();
                            tables[tableNumber - 1].makeVisible();
                        }
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
        }

        //------------------------------------------------- 1 ----------------------------------------------------------

        //Load the menu from the database into the _menuItem list
        // and into the lstVwMenu control
        private void loadMenuFromDatabase()
        {
            string dataItem;
            _theMenu.Clear();
            MenuItem mi;            
            string connectionString;
            SqlDataReader dataReader;

            connectionString = _parentForm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "LoadMenuFromDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select * From Menu";

                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {   
                        mi = new MenuItem();
                        FoodType ft;

                        dataItem = (string)dataReader.GetValue(0); //Food Type
                        if (dataItem == "Main Course")
                        {
                            ft = FoodType.maincourse;
                        }
                        else if (dataItem == "Drink")
                        {
                            ft = FoodType.drink;
                        }
                        else
                        {
                            ft = FoodType.dessert;
                        }

                        mi.setFoodType(ft);

                        dataItem = (string)dataReader.GetValue(1); //The food
                        mi.setItemName(dataItem);

                        mi.setItemPrice(Convert.ToDouble(dataReader.GetValue(2)));
                        _theMenu.Add(mi);
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

            //Add the data to the ListView menu control, lsVwMenu
            lstVwMenu.Items.Clear();

            for (int i = 0; i < _theMenu.Count; i++)
            {
                string foodtype;

                ListViewItem lvi = new ListViewItem();
                lvi.Text = _theMenu[i].getItemName(); 


                if (_theMenu[i].getFoodType() == FoodType.maincourse)
                {
                    foodtype = "Main Course";
                }
                else if (_theMenu[i].getFoodType() == FoodType.drink)
                {
                    foodtype = "Drink";
                }
                else
                {
                    foodtype = "Dessert";
                }
                lvi.SubItems.Add(foodtype);
                lvi.SubItems.Add(_theMenu[i].getItemPrice().ToString());
                lstVwMenu.Items.Add(lvi);
            }
        }

        //------------------------------------------------- 2 ----------------------------------------------------------

        // Load the invoice for the table number passed in "tableNumber"
        // from the database table Invoices.
        // and add the order table number via the "tables" List container
        private void loadInvoiceForTableFromDatabase(int tableNumber)
        {
            Order order = new Order();
            string dataItem;
            _theMenu.Clear();
            MenuItem mi;
            string tableNumberStr = tableNumber.ToString();

            string connectionString;
            SqlDataReader dataReader;

            connectionString = _parentForm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "LoadInvoiceForTableFromDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select * From Invoices Where TableNumber = @tableNumber";
                    command.Parameters.AddWithValue("@tableNumber", tableNumber);
                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        // }
                        mi = new MenuItem();
                        FoodType ft;
                        int tblnumber;
                        tblnumber = (int)dataReader.GetValue(0); //Food Type
                        dataItem = (string)dataReader.GetValue(1);
                        if (dataItem == "Main Course")
                        {
                            ft = FoodType.maincourse;
                        }
                        else if (dataItem == "Drink")
                        {
                            ft = FoodType.drink;
                        }
                        else
                        {
                            ft = FoodType.dessert;
                        }

                        mi.setFoodType(ft);

                        dataItem = (string)dataReader.GetValue(2); //The food
                        mi.setItemName(dataItem);

                        mi.setItemPrice(Convert.ToDouble(dataReader.GetValue(3)));
                        order.addItemToOrderItemsInOrder(mi);
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
            //Add the order from the database
            tables[_currentTableNumber - 1].setOrder(ref order);
        }

        //------------------------------------------------- 3 ----------------------------------------------------------

        // Insert a new employee into the WaitStaff table.  Return false on falure.
        private bool insertWaitStaffIntoDatabase(string firstName, string middleName, string lastName, string email, string password, string tablesAssigned)
        {
            bool result = true;
            string connectionString;

            connectionString = _parentForm._connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "InsertWaitStaffIntoDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText =
                     "Insert into Waitstaff (FirstName, MiddleName, LastName, Email, Passw, TablesAssigned) VALUES (@fname,@mname,@lname,@email,@pword,@tablesAssigned)";

                    
                    //command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@mname", middleName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@pword", password);
                    command.Parameters.AddWithValue("@tablesAssigned", tablesAssigned);
                    command.ExecuteNonQuery();
                    transaction.Commit();
                   // Console.WriteLine("Selection from managers table worked.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        result = false;
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

        //------------------------------------------------- 4 ----------------------------------------------------------


        // Update the WaitStaff table.  Return false on falure.
        // Update the waitStaff employee info in the table.
        private bool updateWaitStaffInDatabase(string firstName, string middleName, string lastName, string email, string password, string tablesAssigned)
        {
            bool result = true;
            string connectionString;

            connectionString = _parentForm._connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "updateWaitStaffInDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.Connection = connection;
                    command.Transaction = transaction;


                    command.CommandText =
                        "Update Waitstaff Set FirstName=@firstName, MiddleName=@middleName, LastName=@lastName, Passw=@passw, TablesAssigned=@tablesAssigned  Where Email=@email";


                    command.Parameters.Add("@firstName", SqlDbType.NVarChar);
                    command.Parameters.Add("@middleName", SqlDbType.NVarChar);
                    command.Parameters.Add("@lastName", SqlDbType.NVarChar);                    
                    command.Parameters.Add("@passw", SqlDbType.NVarChar);
                    command.Parameters.Add("@tablesAssigned", SqlDbType.NVarChar);
                    command.Parameters.Add("@email", SqlDbType.NVarChar);

                    command.Parameters["@firstName"].Value = firstName;
                    command.Parameters["@middleName"].Value = middleName;
                    command.Parameters["@lastName"].Value = lastName;
                    command.Parameters["@passw"].Value = password;
                    command.Parameters["@tablesAssigned"].Value = tablesAssigned;
                    command.Parameters["@email"].Value = email;
              
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    // Console.WriteLine("Selection from managers table worked.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        result = false;
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

        //------------------------------------------------- 5 ----------------------------------------------------------

        //Delete employee from the waitStaff table.  Email is the primary key parameter used
        // to select the employee to delete.  
        private void deleteWaitStaffFromFromDatabase(string email)
        {
            string connectionString;

            connectionString = _parentForm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "deleteWaitStaffFromDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {

                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText = "Delete From Waitstaff Where Email=@email";
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@email", email);
                    command.ExecuteNonQuery();

                    command = connection.CreateCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;
                    //Delete the employee's entries in the WorkTime table for hours worked.
                    command.CommandText = "Delete from WorkTime Where Email=@email";
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


        //------------------------------------------------- 6 ----------------------------------------------------------

        // Insert a new menu item into the Menu table of the database.  The menu item is identified
        // by the parameters "category", "food", and "price".
        private void addMenuItemToDatabase(string category, string food, double price)
        {
            string connectionString;
            connectionString = _parentForm._connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "addMenuItemToDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.Connection = connection;
                    command.Transaction = transaction;


                    command.CommandText =
                        "Insert into Menu (Category, Food, Price) VALUES (@category, @food, @price)";


                    command.Parameters.AddWithValue("@category", category);
                    command.Parameters.AddWithValue("@food", food);
                    command.Parameters.AddWithValue("@price", price);
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    // Console.WriteLine("Selection from managers table worked.");
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

        //------------------------------------------------- 7 ----------------------------------------------------------

        //Delete menu item from the Menu table of the database.  Use the "category"
        // and "food" to select the item to delete.
        private void deleteMenuItemFromDatabase(string category, string food)
        {
            string connectionString;

            connectionString = _parentForm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "deleteMenuItemFromDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {

                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText = "Delete From Menu Where Food = @food and Category = @category";

                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@food", food);
                    command.Parameters.AddWithValue("@category", category);

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

        //------------------------------------------------- 8 ----------------------------------------------------------


        //Update the invoice table for the database for the table number, tableNumber, passed in
        // The data will be extracted from the "tables" list container, using the tableNumber is
        // the index of the table.  The "order" is retrieved from the table list container and
        // placed in the database.  But first the previous invoice values are deleted.
        private void updateTheInvoiceTableInDatabase(int tableNumber)
        {
            string category, food;
            double price;
            string connectionString;
            connectionString = _parentForm._connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "updateTheInvoiceTableInDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction


                try
                {
                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText = "Delete From Invoices Where TableNumber=@tableNumber";

                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@tableNumber", tableNumber);

                    command.ExecuteNonQuery();

                    Order order;
                    order = tables[tableNumber - 1].getOrder();
                    List<MenuItem> items = order.deepCopyMenuItems();
                    for (int i = 0; i < items.Count(); i++)
                    {
                        FoodType ft = items[i].getFoodType();
                        category = "Main Course";
                        if (ft == FoodType.maincourse)
                        {
                            category = "Main Course";
                        }
                        else if (ft == FoodType.drink)
                        {
                            category = "Drink";
                        }
                        else if (ft == FoodType.dessert)
                        {
                            category = "Dessert";
                        }

                        food = items[i].getItemName();
                        price = items[i].getItemPrice();

                        command = command = connection.CreateCommand();
                        command.Connection = connection;
                        command.Transaction = transaction;
                        command.CommandText =  "Insert into Invoices (TableNumber, Category, Food, Price) VALUES (@tableNumber, @category, @food, @price)";


                        command.CommandType = CommandType.Text;

                        command.Parameters.AddWithValue("@tableNumber", tableNumber);
                        command.Parameters.AddWithValue("@category", category);
                        command.Parameters.AddWithValue("@food", food);
                        command.Parameters.AddWithValue("@price", price);
                        
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    // Console.WriteLine("Selection from managers table worked.");
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


        //------------------------------------------------- 9 ----------------------------------------------------------

        // Update the the diningTable images... for now all of the tables retain the
        // same image, so all of the values are changed to the same value.  The TableInfo table
        // is updated.
        public void updateTableStyleInDatabase()
        {
            string connectionString;
            int imageNumber = Table.ImageIndex;


            connectionString = _parentForm._connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "updateTableStyle");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.Connection = connection;
                    command.Transaction = transaction;
                    command.CommandText =
                        "Update TableInfo Set ImageNumber=@imageNumber";

                    command.Parameters.Add("@imageNumber", SqlDbType.Int);

                    command.Parameters["@imageNumber"].Value = imageNumber;


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


        //------------------------------------------------- 10 ----------------------------------------------------------

        // Update the the diningTable indicated by tableNumber in the database
        // The TableInfo table is updated.
        public void updateDiningTableInDatabase(int tableNumber)
        {
            string connectionString;
            int i = tableNumber - 1;     
            int Xpos = tables[i].getXpos();
            int Ypos = tables[i].getYpos();
            int state = (int)tables[i].getTableState();
            int imageNumber = Table.ImageIndex;
            int onFloor = 0;

            if (tables[i].OnTheFloor)
            {
                onFloor = 1;
            }

            connectionString = _parentForm._connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "updateDiningTableInDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText =
                        "Update TableInfo Set Xpos=@xpos, Ypos=@ypos, State=@state, OnFloor=@onFloor, ImageNumber=@imageNumber  Where TableNumber=@tableNumber";
             
                    
                    command.Parameters.Add("@xpos", SqlDbType.Int);
                    command.Parameters.Add("@ypos", SqlDbType.Int);
                    command.Parameters.Add("@state", SqlDbType.Int);
                    command.Parameters.Add("@onFloor", SqlDbType.Int);
                    command.Parameters.Add("@imageNumber", SqlDbType.Int);
                    command.Parameters.Add("@tableNumber", SqlDbType.Int);
                   
                    command.Parameters["@xpos"].Value = Xpos;
                    command.Parameters["@ypos"].Value = Ypos;
                    command.Parameters["@state"].Value = state;
                    command.Parameters["@onfloor"].Value = onFloor;
                    command.Parameters["@imageNumber"].Value = imageNumber;
                    command.Parameters["@tableNumber"].Value = tableNumber;

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
