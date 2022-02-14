using System;
using ADODB;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using System.Security;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.IO;

namespace Mars_Restaurant
{
    public enum AppState
    {
        defaultView,
        createMenu,
        addDelMovTables,
        reserveTable,   
        employees
    }

    //------------------------------------------------- 0 ----------------------------------------------------------


    // The DoubleBuffedWindow class is the heart of the application.  It contains most of the forms
    // such as the Menu edit form, and the Invoices form, and employees form.
    // This is part of the same class as is in the file DoubleBufferedWindows2.cs.
    public partial class DoubleBufferedWindow : Form
    {

        private List<MenuItem> _theMenu = new List<MenuItem>(); //This holds the current menu items
        private LoginForm _parentForm;
        private readonly Point TOP_LEFT = new Point(0, 0);
        private readonly Color CLEAR_COLOR = Color.Black;
        private bool _capturePanel = false;
        private ToolBar toolBarMain;
        private Point pointDelta = new Point();
        private Cursor defaultCursor;
        public ListView _lstVwInvoiceDup; // duplicate the invoice to pass it to receiptForm

        // vector or arraylist 
        private List<Table> tables = new List<Table>(); //Initialize List to hold the dining tables

        // double buffer graphics for animation
        //  (so there is no flickering)

        FormTableImages _form; 

        public delegate void RepaintControlDelegate(Control control);
        public RepaintControlDelegate repaintControl;   
        //private PictureBox pbx1;
        public PictureBox _pPicBox;
        public AppState _appState = AppState.defaultView;
        private int _MaxTables = 20;
        private int _currentTableNumber = -1; // The number of the currently selected table, or -1 if no table is selected
        private int _lastEmployeeSelected =-1; // last lstVwEmployee selected
        private int _timerUpdateTableTickCount=0; // Times since last update
        private int _detectUserActivity = 0; // If user activity is detected, then this will be set to 1, and reset by the timer at specified intervals
        private int _timerUpdateUserActivity = 0;
        private bool _forcedLogOff = false; //Set to true when the user has timed out.


        //------------------------------------------------- 1 ----------------------------------------------------------


        // The constructor initializes variables for the form
        // the parentForm parameter holds information from the LoginForm object
        // which includes things like the database connection string
        public DoubleBufferedWindow(LoginForm parentForm)
        {
            _parentForm = parentForm; // Retain the LoginForm info at the member variable level
            _parentForm.Hide(); // Hide the LoginForm form
      
            //  constructor level variable
            InitializeComponent();
     
            // Create and initialize the ToolBar and ToolBarButton controls.
            toolBarMain = new ToolBar();
            ToolBarButton tbbLogout = new ToolBarButton();                      
            ToolBarButton tbbTools = new ToolBarButton();
            ToolBarButton tbbEmployees = new ToolBarButton();
 
            // Set the Text properties of the ToolBarButton controls.
            tbbLogout.Text = "Logout";            
            tbbTools.Text = "Manager";
            tbbEmployees.Text = "Employees";

            // Add the ToolBarButton controls to the ToolBar.
            toolBarMain.Buttons.Add(tbbLogout);

            if (_parentForm._managerLoggedIn)
            {
                toolBarMain.Buttons.Add(tbbTools);
                toolBarMain.Buttons.Add(tbbEmployees);
            }

            toolBarMain.ButtonClick += new ToolBarButtonClickEventHandler(toolBarMain_ButtonClick);
     
            // Add the ToolBar to the Form.
            Controls.Add(toolBarMain);

            panelTableManager.Visible = false;
            panelTableManager.Enabled = false;

            defaultCursor = this.Cursor; // Retain the default cursor state, so that it can be set to it after cursor change

            _pPicBox = pictureBox1; // Makes pictureBox1 publicly available to the whole program.  This holds the background image of the restaurant and will hold the dining tables

            //Initially the create menu panel is not visible and not enabled
            panelMakeMenu.Visible = false;
            panelMakeMenu.Enabled = false;
            _appState = AppState.defaultView;
            cmbBxFoodType.SelectedIndex = 0;

            panelReserveTable.Left = 100;
            panelReserveTable.Top = 100;
            panelReserveTable.Visible = false;
            panelReserveTable.Enabled = false;

            cmbBxFoodType2.SelectedIndex = 0;

            panelEmployees.Visible = false;
            panelEmployees.Enabled = false;
            loadEmployeesIntoForm();

            // Set up the date and time format for display in the status bar, bottom of screen
            string loginDateTime = DateTime.Now.ToString("dddd @ HH:mm");
            //Start the timer
            tmrClock.Start();

            // Display who is logged in
            toolStripStatusLabel2.Text = "Logged in: " + _parentForm._loggedInPersonsName + " on " + loginDateTime;
            // Refreshes the display so that it is visible
            statusStripMarsRest.Refresh();

            // Fix the window size
            this.MinimumSize = new Size(this.Width, this.Height);
            this.MaximumSize = new Size(this.Width, this.Height);
        }



        //------------------------------------------------- 2 ----------------------------------------------------------

        //This initializes the dining tables to their default state
        //The default state is that there are no tables on the restaurant
        //floor and the image index is set to zero.  It creates number _MaxTables
        // at the start.  So, it creates that many Table objects.
        private void initalizeTables()
        {
            int id;
            int i;

            id = 1;
            for(i=0; i < _MaxTables; i++)
            {
                Table newTable = new Table(this);
                newTable.TableID = id;
                id++;
                tables.Add(newTable);
            }

            updateTableImages(Table.ImageIndex);
        }


        //------------------------------------------------- 3 ----------------------------------------------------------

        // This loads the waitstaff employees into the ListViewItem 
        // control of the employees form
        private void loadEmployeesIntoForm()
        {
            if(_parentForm._employees.Count == 0)
            {
                return;
            }

           foreach(Employee emp in _parentForm._employees)
            {
                ListViewItem lvi = new ListViewItem();
                NameOfPerson nop = emp.getEmployeeName();             
                lvi.Text = nop.getFirstName();
                lvi.SubItems.Add(nop.getMiddleName());
                lvi.SubItems.Add(nop.getLastName());
                lvi.SubItems.Add(emp.getEmail());
                lvi.SubItems.Add(emp.getPassword());
                lstVwEmployees.Items.Add(lvi);
            }
        }


        //------------------------------------------------- 4 ----------------------------------------------------------

        // When a toolbar button at the top of the application is clicked on, this handles that event.
        private void toolBarMain_ButtonClick(Object sender, ToolBarButtonClickEventArgs e)
        {
            switch (toolBarMain.Buttons.IndexOf(e.Button))
            {
                case 0: // Logout was selected
                    this.Close();
                    break;
                case 1: // the table manager window was selected
                    if (groupTableMenu.Visible == false && _appState == AppState.defaultView)
                    {
                        _appState = AppState.addDelMovTables;
                        panelTableManager.Visible = true;
                        panelTableManager.Enabled = true;
                        attachEventsToButtons(true);
                        panelTableManager.BringToFront();
                    }  
                    
                    break;
                case 2: // The Employees window was selected
                    if (_appState == AppState.defaultView)
                    {
                        _appState = AppState.employees;

                        panelEmployees.Visible = true;
                        panelEmployees.Enabled = true;
                        panelEmployees.BringToFront();
                        panelEmployees.Left = 100;
                        panelEmployees.Top = 100;
                    }
                    break;
            }
        }


        //------------------------------------------------- 5 ----------------------------------------------------------

        // Attach or unattach events to/from buttons so that they
        // are responsive or not to mouse clicks
        private void attachEventsToButtons(bool state)
        {
            int i;
           
            for (i = 0; i < tables.Count; i++)
            {
                tables[i].buttonEventsAttachUnattach(state);
            }            
        }


        //------------------------------------------------- 6 ----------------------------------------------------------

        // When the form loads the dining tables are initialized 
        private void Form1_Load(object sender, EventArgs e)
        {
            initalizeTables();
            loadTableInfoFromDatabase();
            return;
        }



        //------------------------------------------------- 7 ----------------------------------------------------------

        // This activates the add table action.  It sets up
        // Table.Mode to TableAction.add state so that when
        // the surface is subsequently clicked with the mouse
        // the table will be added
        private void btnAddTable_Click(object sender, EventArgs e)
        {
            if(_appState != AppState.addDelMovTables)
                return;
            this.Cursor = defaultCursor;
            Table.Mode = TableAction.add; 

            return; 
        }


        //------------------------------------------------- 8 ----------------------------------------------------------

        // Exiting the Restaurant Floor and going back to the logon form, unless the action is canceled
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {          

            // if _forcedLogOff is true, then the user was automatically logged off
            if (_forcedLogOff)
            {
                // If dining table is in use, make sure the Tokens table slot is free before exiting
                if (_appState == AppState.reserveTable)
                    diningTableBackButton();
                _parentForm.Show();
                return;
            }

            // Show the dialog box to ask user if he wants to log off
            DialogResult dialogResult = MessageBox.Show("Logoff?", "Mars Restaurant", MessageBoxButtons.YesNo);
            // If the users doesn't want to logoff, cancel the windows form close action
            if (dialogResult == DialogResult.No)
            {
                //Cancel the windows form close action            
                e.Cancel = true;
            }
            else // Log off has been chosen
            {
                // If dining table is in use, make sure the Tokens table slot is free before exiting
                if (_appState == AppState.reserveTable)
                    diningTableBackButton();
                // Make the _parentForm visible
                _parentForm.Show();
            }
        }


        //------------------------------------------------- 9 ----------------------------------------------------------

        // Launch the FormTableImages form, which presents the different
        // table styles to the user allowing the user to select one.
        private void btnImages_Click(object sender, EventArgs e)
        {
            Table.Mode = TableAction.move;
            this.Cursor = defaultCursor;
            //Launch the images form
            _form = new FormTableImages(this, ref imageListTables);           
            _form.Show();                    
        }


        //------------------------------------------------- 10 ----------------------------------------------------------

        // When a new table style is selected, the images for
        // those tables are updated here.  "index" is the index
        // of the newly selected table style
        public void updateTableImages(int index)
        {
            if (index >= 0)
            {           
                Table.ImageIndex = index;
                int i;
                for (i = 0; i < tables.Count; i++)
                {
                    tables[i].AddImage(imageListTables.Images[Table.ImageIndex]);
                }
            }
        }

        //------------------------------------------------- 11 ----------------------------------------------------------

        // This sets the Table.Mode to TableAction.remove, so that the
        // next table clicked on will be deleted.  The cursor is changed
        // to a Cross shape
        private void btnRemoveTable_Click(object sender, EventArgs e)
        {
            if (_appState != AppState.addDelMovTables)
                return;
            Table.Mode = TableAction.remove; // 1; // remove
            this.Cursor = Cursors.Cross;
        }


        //------------------------------------------------- 12 ----------------------------------------------------------

        // When the panelTableManager is clicked on, this is selected
        // It allows the user to select the panelTableManager panel 
        // so that it can be moved around with the mouse.
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {

            var coords = this.PointToClient(Cursor.Position);
            pointDelta.X = e.X;
            pointDelta.Y = e.Y;
            _capturePanel = true;
            Console.WriteLine("Window: x = {0}, y = {1}", coords.X, coords.Y);
            return;                     
        }


        //------------------------------------------------- 13 ----------------------------------------------------------

        // After the panelTableManager is selected with the mouse via panel1_MouseDown()
        // this function moves the table manager around the surface while the mouse button
        // is pressed down
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {           

            if (_capturePanel)
            {
               var coords = this.PointToClient(Cursor.Position);
               panelTableManager.Left = coords.X - pointDelta.X;
               panelTableManager.Top = coords.Y - pointDelta.Y;

            }
        }



        //------------------------------------------------- 14 ----------------------------------------------------------

        // When the user releases the previous pressed mouse button this function
        // is activated, releasing PanelTableManager from the move action.
        // This usually happen after panel1_MouseMove() has been activated.
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
           _capturePanel = false;
        }

        //------------------------------------------------- 15 ----------------------------------------------------------


        // This adds a table to the Mars Restaurant floor
        private void addTable()
        {
            int i;
            Table.Mode = TableAction.move; 
            this.Cursor = defaultCursor; 

            // Find a free table to use
            for (i = 0; i < tables.Count; i++)
            {
                if (tables[i].OnTheFloor == false)
                {
                    break;
                }
            }

            // All tables are in use
            if (i == tables.Count)
                return;

            tables[i].enableGui();
            tables[i].makeVisible();
            //tables[i].startDragging();
            tables[i].OnTheFloor = true;
            var coords = pictureBox1.PointToClient(Cursor.Position);
            tables[i].tableGui_Position(coords.X,coords.Y);

            Console.WriteLine("Window: x = {0}, y = {1}", coords.X, coords.Y);
            updateDiningTableInDatabase(i + 1);            
        }


        //------------------------------------------------- 16 ----------------------------------------------------------

        // Click mouse button onto Mars Restaurant surface to add
        // table if TableAction.add is the mode set.   And to put
        // the table in TableAction.move state, so that the table
        // can be moved around with the mouse.
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //return;
            //Add Table     
            if (Table.Mode == TableAction.add)
            {
                addTable();
            }

            Table.Mode = TableAction.move; // 0;
            this.Cursor = defaultCursor;
        }

        //------------------------------------------------- 17 ----------------------------------------------------------

        // Determine if the item passed in already exists in the listview control
        // Return true if it does, else false.  This is a case insensitive test.
        private bool duplicateItems(string item)
        {
            int i;  

            for (i = 0; i < lstVwMenu.Items.Count; i++)
            {
                if (lstVwMenu.Items[i].Text.ToLower() == item.ToLower())
                    return true;
            }
            return false;
        }

        //------------------------------------------------- 18 ----------------------------------------------------------

        //Add a food item to the menu
        private void btnAddFood_Click(object sender, EventArgs e)
        {
            double price;
            string category;

            if(txtBxFood.Text == "" || txtBxPrice.Text == "")
            {
                MessageBox.Show("Invalid Text", "Add Food", MessageBoxButtons.OK);
                return;
            }

            try
            {
                price = double.Parse(txtBxPrice.Text);
            }catch
            {
                MessageBox.Show("Invalid Number", "Add Food", MessageBoxButtons.OK);
                return;
            }

            int idx = cmbBxFoodType.SelectedIndex;

            // If item already exists, don't add it
            if(duplicateItems(txtBxFood.Text) == true)
            {
                return;
            }
            // Add the item to the ListViewItem control
            ListViewItem lvi = new ListViewItem();
            lvi.Text = txtBxFood.Text; 
            lvi.SubItems.Add(cmbBxFoodType.SelectedItem.ToString());
            lvi.SubItems.Add(txtBxPrice.Text);
            lstVwMenu.Items.Add(lvi);

            MenuItem mi = new MenuItem();

            mi.setItemName(txtBxFood.Text);
            mi.setItemPrice(price);
            mi.setIsCustomItem(false);
            FoodType ft;
            if(idx == 0)
            {
                ft = FoodType.maincourse;
                category = "Main Course";
            }else if (idx == 1)
            {
                ft = FoodType.drink;
                category = "Drink";
            }else
            {
                ft = FoodType.dessert;
                category = "Dessert";
            }

            mi.setFoodType(ft);
            // Add the item to the current menu item List container, which is independent of the ListViewItem control
            _theMenu.Add(mi);

            //Update database, add the menu item to the database
            addMenuItemToDatabase(category, txtBxFood.Text, price);
        }


        //------------------------------------------------- 19 ----------------------------------------------------------

        //The back button for the make menu panel
        // After the backbutton for the menu is pressed, made the 
        // panelMakeMenu invisible and disable it.  
        private void btnMakeMenuBack_Click(object sender, EventArgs e)
        {
            panelMakeMenu.Visible = false;
            panelMakeMenu.Enabled = false;

            _appState = AppState.addDelMovTables;
        }

        //------------------------------------------------- 20 ----------------------------------------------------------


        // When the "Create Menu" button is clicked, load in the
        // latest menu from the database, and fill the lstVwMenu control with the data.
        // Then make the panelMakeMenu visible and enabled.
        private void btnMakeMenu_Click(object sender, EventArgs e)
        {
            this.Cursor = defaultCursor;
            loadMenuFromDatabase();
            lstVwMenu.Enabled = true;
            lstVwMenu.View = View.Details;
            panelMakeMenu.Visible = true;
            panelMakeMenu.Enabled = true;

            panelMakeMenu.Left = 100;
            panelMakeMenu.Top = 100;
            panelMakeMenu.BringToFront();
            _appState = AppState.createMenu;
        }

        //------------------------------------------------- 21 ----------------------------------------------------------

        // When an item in the lstVwMenu is clicked, i.e. a food item,
        // this is activated and fills in the text controls with the
        // item selected.   This is for the panelMakeMenu panel.
        private void lstVwMenu_Click(object sender, EventArgs e)
        {
            int item;

            if (lstVwMenu.SelectedItems.Count == 0)
            {
                return;
            }

            item = lstVwMenu.Items.IndexOf(lstVwMenu.SelectedItems[0]);
            txtBxFood.Text = (string)lstVwMenu.Items[item].Text;
            txtBxPrice.Text = lstVwMenu.SelectedItems[0].SubItems[2].Text;          
        }


        //------------------------------------------------- 22 ----------------------------------------------------------

        // The "Empty" button in the panelReserveTable panel is clicked.
        // Set the table to TableState.empty if the invoice has no
        // food items in it.
        private void btnEmpty_Click(object sender, EventArgs e)
        {
            //Can't make table empty until the invoice is cleared
            if(lstVwInvoice.Items.Count != 0)
            {
                return;
            }
            btnEmpty.BackColor = Color.Green;
            btnReserved.BackColor = Color.Gray;
            btnBusy.BackColor = Color.Gray;
            tables[_currentTableNumber - 1].setTableState(TableState.empty);
            lstVwTableMenu.Enabled = false;
            lstVwInvoice.Enabled = false;
            txtBxTotalBalance.Enabled = false;
            cmbBxFoodType2.Enabled = false;
        }

        //------------------------------------------------- 23 ----------------------------------------------------------

        // The "Reserved" button in the panelReserveTable panel is clicked.
        // Set the table state to TableState.reserved.
        private void btnReserved_Click(object sender, EventArgs e)
        {
            btnEmpty.BackColor = Color.Gray;
            btnReserved.BackColor = Color.Green;
            btnBusy.BackColor = Color.Gray;
            tables[_currentTableNumber - 1].setTableState(TableState.reserved);
            lstVwTableMenu.Enabled = true;
            lstVwInvoice.Enabled = true;
            txtBxTotalBalance.Enabled = true;
            cmbBxFoodType2.Enabled = true;
        }

        //------------------------------------------------- 24 ----------------------------------------------------------

        // The "Busy" button in the panelReservedTable panel is clicked.
        // Set the table state to TableState.busy.
        private void btnBusy_Click(object sender, EventArgs e)
        {
            btnEmpty.BackColor = Color.Gray;
            btnReserved.BackColor = Color.Gray;
            btnBusy.BackColor = Color.Green;
            tables[_currentTableNumber - 1].setTableState(TableState.busy);
            lstVwTableMenu.Enabled = true;
            lstVwInvoice.Enabled = true;
            txtBxTotalBalance.Enabled = true;
            cmbBxFoodType2.Enabled = true;
        }

        //------------------------------------------------- 25 ----------------------------------------------------------


        // Pass in the FoodType, ft, which will be extracted from the lstVwMenu and 
        //displayed in the lstVwTableMenu
        private void displayFoodType(FoodType ft)
        {
            string theFoodType;
            lstVwTableMenu.Items.Clear();

            if(ft == FoodType.maincourse)
            {
                theFoodType = "Main Course";
            }else if(ft == FoodType.drink)
            {
                theFoodType = "Drink";
            }
            else
            {
                theFoodType = "Dessert";
            }


            foreach (ListViewItem itm in lstVwMenu.Items)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = itm.Text;

                if(itm.SubItems[1].Text == theFoodType)
                {
                    lvi.SubItems.Add(itm.SubItems[2].Text);
                    lstVwTableMenu.Items.Add(lvi);
                }                
            }
        }

        //------------------------------------------------- 26 ----------------------------------------------------------

        // Determines if table is not being used concurrently
        // returns true if it is, false otherwise
        public bool tableInUse2(int tableNumber)
        {
            //Use stored procedure ... pass in the email and current time in utc time (universal time)
            string extractedEmail = "";
            string connectionString;
            SqlDataReader dataReader;
            bool result = true;
            DateTime dt = DateTime.UtcNow;
            String tableName;

            tableName = "diningTable" + tableNumber.ToString();

            connectionString = _parentForm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "tableInUse2");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select Using From Tokens Where Resource=@resource";
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@resource", tableName);
                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        extractedEmail = (string)dataReader.GetValue(0);
                    }

                    dataReader.Close();
                    command.Dispose();
                    //The table is free if it has the text "free" in it
                    if (extractedEmail == "free")
                    {
                        result = false;
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


        //------------------------------------------------- 27 ----------------------------------------------------------

        // Determines if table is in use.  If it is it returns a true
        // If it isn't, then the table Tokens is updated with the
        // email of the user and false is returned
        private bool tableInUse(int tableNumber, string email)
        {
            //Use stored procedure ... pass in the email and current time in utc time (universal time)
            string extractedEmail = "";
            string connectionString;
            SqlDataReader dataReader;
            bool result = true;
            DateTime dt = DateTime.UtcNow;
            String tableName;

            tableName = "diningTable" + tableNumber.ToString();

            connectionString = _parentForm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "tableInUse");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "Select Using From Tokens Where Resource=@resource";
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@resource", tableName);
                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        extractedEmail = (string)dataReader.GetValue(0);
                    }

                    dataReader.Close();
                    command.Dispose();
                    //The table is free if it has the text "free" in it
                    if (extractedEmail == "free")
                    {
                        command = connection.CreateCommand();
                        result = false;
                        command.Connection = connection;
                        command.Transaction = transaction;
                        command.CommandText = "Update Tokens Set Using=@email Where Resource=@resource"; 

                        command.CommandType = CommandType.Text;

                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@resource", tableName);

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

        //------------------------------------------------- 28 ----------------------------------------------------------

        // Free up the diningTable resource, since it is no longer being used.
        // The "diningTable" parameter determines which table to change the
        // state of to "free" in the database Tokens table.
        private void freeDiningTableFromTokensTable(String diningTable)
        {
            //Use stored procedure ... pass in the email and current time in utc time (universal time)
            string connectionString;

            connectionString = _parentForm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "freeDiningTableFromTokensTable");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                { 
                    command.Connection = connection;
                    command.Transaction = transaction;
                    command.CommandText = "Update Tokens Set Using='free' Where Resource=@resource";                     
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@resource", diningTable);
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

        //------------------------------------------------- 29 ----------------------------------------------------------

        // When a user clicks on a dining table, this function runs.  It launches
        // the panelResrveTable panel, which allows the user to fill out a food
        // invoice for a customer.
        public void launchTableManagerPanel(int tableNumber, TableState ts)
        {
            // If this tableis already being used by another user, indicate
            // this to the user with a MessageBox and exit the function without
            // launching the dining table Manager.
            if(tableInUse(tableNumber,_parentForm._loggedInEmail) == true)
            {
                _appState = AppState.defaultView;
                string msg = "diningTable" + tableNumber.ToString() + " is already in use.";
                MessageBox.Show(msg);                
                return;
            }            
            _currentTableNumber = tableNumber;
            lblTableNumber.Text = "Table Number  " + tableNumber.ToString();
            panelReserveTable.Visible = true;
            panelReserveTable.Enabled = true;
            panelReserveTable.Left = 100;
            panelReserveTable.Top = 100;
            panelReserveTable.BringToFront();

            //Load the menu from the database to make sure it's updated with the latest info,
            //since this application can have mulitple instances running
            loadMenuFromDatabase();

            FoodType ft;
            if(cmbBxFoodType2.SelectedIndex == 0)
            {
                ft = FoodType.maincourse;
                lstVwTableMenu.Columns[0].Text = "Main Course";
            }else if(cmbBxFoodType2.SelectedIndex == 1)
            {
                ft = FoodType.drink;
                lstVwTableMenu.Columns[0].Text = "Drink";
            }
            else
            {
                ft = FoodType.dessert;
                lstVwTableMenu.Columns[0].Text = "Dessert";
            }

            displayFoodType(ft);
     
            btnEmpty.BackColor = Color.Gray;
            btnReserved.BackColor = Color.Gray;
            btnBusy.BackColor = Color.Gray;

            lstVwTableMenu.Enabled = true;
            lstVwInvoice.Enabled = true;
            txtBxTotalBalance.Enabled = true;
            cmbBxFoodType2.Enabled = true;

            if (ts == TableState.empty)
            {
                btnEmpty.BackColor = Color.Green;
                lstVwTableMenu.Enabled = false;
                lstVwInvoice.Enabled = false;
                txtBxTotalBalance.Enabled = false;
                cmbBxFoodType2.Enabled = false;
            }
            else if(ts == TableState.reserved)
            {
                btnReserved.BackColor = Color.Green;
            }else if(ts == TableState.busy)
            {
                btnBusy.BackColor = Color.Green;
            }

            loadInvoiceForTableFromDatabase(tableNumber);
            Order order;

            order = tables[_currentTableNumber - 1].getOrder();
            order.getItemsForListView(ref lstVwInvoice);

            //Provide the updated balance
            sumInvoiceBalance();
        }

        //------------------------------------------------- 30 ----------------------------------------------------------

        //The back button for the table manager panels was pressed
        private void btnTableBack_Click(object sender, EventArgs e)
        {
            diningTableBackButton();           
        }


        //------------------------------------------------- 31 ----------------------------------------------------------

        // After the back button is pressed for the dining table manager,
        // this updates the order, and updates the database Invoices table
        // with the customers order.   Frees up the Tokens table so that
        // someone else can serve the dining table and updates the state
        // of the diningTable in the TableInfo table.
        private void diningTableBackButton()
        {
            panelReserveTable.Visible = false;
            panelReserveTable.Enabled = false;
            //_currentTableNumber
            //Find the selected table and update it with the invoice
            Order order = new Order();

            order.addListViewItems(lstVwInvoice);

            tables[_currentTableNumber - 1].setOrder(ref order);

            updateTheInvoiceTableInDatabase(_currentTableNumber);

            lstVwTableMenu.Items.Clear();
            lstVwInvoice.Items.Clear();

            //Update the InfoTable database
            updateDiningTableInDatabase(_currentTableNumber);
            string diningTable = "diningTable" + _currentTableNumber.ToString();
            freeDiningTableFromTokensTable(diningTable);
            //Delete the elements in the invoices table for the current table number
            //Add elements to the invoices table for the current table number
            _appState = AppState.defaultView;
        }

        //------------------------------------------------- 32 ----------------------------------------------------------

        // Delete a food item from the lstVwMenu, and from _theMenu list and from
        // the Menu table in the database.
        private void btnDeleteFood_Click(object sender, EventArgs e)
        {
            if(lstVwMenu.SelectedItems.Count == 0)
            {
                return;
            }
            
            int indx = lstVwMenu.Items.IndexOf(lstVwMenu.SelectedItems[0]);

            string food = lstVwMenu.SelectedItems[0].SubItems[0].Text;
            string category = lstVwMenu.SelectedItems[0].SubItems[1].Text;
            lstVwMenu.Items.RemoveAt(indx);

            removeFromMenuList(food);
            //Update database
            deleteMenuItemFromDatabase(category, food);
        }

        //------------------------------------------------- 33 ----------------------------------------------------------

        //Remove menu item from the _theMenu list
        void removeFromMenuList(string food)
        {
            int i;
            for(i=0; i < _theMenu.Count(); i++)
            {
                if(_theMenu[i].getItemName().ToLower() == food)
                {
                    _theMenu.RemoveAt(i);
                }
            }
        }

        //------------------------------------------------- 34 ----------------------------------------------------------

        //This selects an item clicked on in the lstVwTableMenu control and adds it to
        // the LstVwInvoice control.  In other words, it transfers a menu item to the
        // invoice.   It then updates the price for the food items selected.
        private void lstVwTableMenu_DoubleClick(object sender, EventArgs e)
        {
            if (lstVwTableMenu.SelectedItems.Count == 0)
            {
                return;
            }

            string food, foodType, price;

            food = lstVwTableMenu.SelectedItems[0].Text;
            foodType = cmbBxFoodType2.SelectedItem.ToString();
            price = lstVwTableMenu.SelectedItems[0].SubItems[1].Text;

            ListViewItem lvi = new ListViewItem();
            lvi.Text = food;
            lvi.SubItems.Add(foodType);
            lvi.SubItems.Add(price);
            lstVwInvoice.Items.Add(lvi);

            //Provide the updated balance
            sumInvoiceBalance();
        }

        //------------------------------------------------- 35 ----------------------------------------------------------

        // Sum the total value of the order invoice and put it
        // in the text box
        private void sumInvoiceBalance()
        {
            string txtPrice;
            double price;
            double sum = 0.0;
            foreach (ListViewItem itm in lstVwInvoice.Items)
            {
                txtPrice = itm.SubItems[2].Text;
                price = double.Parse(txtPrice);
                sum += price;                
            }
            
            txtBxTotalBalance.Text = String.Format("{0:N2}", sum); 
        }

        //------------------------------------------------- 36 ----------------------------------------------------------


        // Delete item from the invoice, sum total
        private void lstVwInvoice_DoubleClick(object sender, EventArgs e)
        {
            string category, food;

            if (lstVwInvoice.Items.Count == 0)
                return;
            if (lstVwEmployees.SelectedItems.Count == 0)
            {
                return;
            }

            if (lstVwInvoice.SelectedItems.Count == 0)
            {
                return;
            }

            int indx = lstVwInvoice.Items.IndexOf(lstVwInvoice.SelectedItems[0]);

            food = lstVwInvoice.Items[indx].SubItems[0].Text;
            category = lstVwInvoice.Items[indx].SubItems[1].Text;
            lstVwInvoice.Items.RemoveAt(indx);

            //Provide the updated balance
            sumInvoiceBalance();
        }

        //------------------------------------------------- 37 ----------------------------------------------------------

        // Exit from the manager window
        private void btnBackManager_Click(object sender, EventArgs e)
        {
            _appState = AppState.defaultView;
            panelTableManager.Visible = false;
            panelTableManager.Enabled = false;
            this.Cursor = defaultCursor;
            attachEventsToButtons(false);
        }


        //------------------------------------------------- 38 ----------------------------------------------------------

        // The "Back" button was pressed for the panelEmployees panel
        // Set the view back to AppState.defaultView, make the panelEmployees
        // panel invisible and disable it
        private void btnBackEmployees_Click(object sender, EventArgs e)
        {
            _appState = AppState.defaultView;                      
            panelEmployees.Visible = false;
            panelEmployees.Enabled = false;
        }

        //------------------------------------------------- 39 ----------------------------------------------------------

        // When a new comBxFoodType2 is selected, this loads the lstVwTableMenu
        // with that category of itmes, i.e. Main Course, Drink or Dessert.
        // This is within the panelReserveTable panel.
        private void cmbBxFoodType2_SelectedIndexChanged(object sender, EventArgs e)
        {
            FoodType ft;
            if (cmbBxFoodType2.SelectedIndex == 0)
            {
                ft = FoodType.maincourse;
                lstVwTableMenu.Columns[0].Text = "Main Course";
            }
            else if (cmbBxFoodType2.SelectedIndex == 1)
            {
                ft = FoodType.drink;
                lstVwTableMenu.Columns[0].Text = "Drink";
            }
            else
            {
                ft = FoodType.dessert;
                lstVwTableMenu.Columns[0].Text = "Dessert";
            }

            displayFoodType(ft);
        }

        //------------------------------------------------- 40 ----------------------------------------------------------

        // This erases all of the elements in the lstvwInvoice control
        // and sets the balance to 0.  First, however, it asks the user if
        // he wants to execute the action.
        private void btnClearInvoice_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to clear the invoice list?", "Mars Restaurant", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.No)
            {
                return;
            }
           
            lstVwInvoice.Items.Clear();
            txtBxTotalBalance.Text = "0.0";
        }

        //------------------------------------------------- 41 ----------------------------------------------------------

        //Find person in _employee list. Since the email is a primary key,
        //you just need to find it to match employee
        private int getIndexOfEmployee(string email)
        {
            int idx = 0;
            foreach(Employee emp in _parentForm._employees)
            {
                if(email == emp.getEmail())
                {
                    return idx;
                }
                idx++;
            }
            return -1;
        }


        //------------------------------------------------- 42 ----------------------------------------------------------


        // Make sure that the employee data meets these minimum criterion
        // it it does, return true, else return false
        private bool validateEmployeeData()
        {
            string error = ""; 

            //Make sure that the information is correct
            if (txtBxEmpFirstName.Text.Length == 0)
            {
                error = "No First Name Entered";
            }
            else if (txtBxEmpLastName.Text.Length == 0)
            {
                error = "No Last Name Entered";
            }
            else if (txtBxEmpEmail.Text.Length == 0)
            {
                error = "No Email Entered";
            }
            else if (txtBxEmpPassword.Text.Length == 0)
            {
                error = "No Passwored Entered";
            }
            
            if (error != "")
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK);
                return false;
            }

            return true;
        }

        //------------------------------------------------- 43 ----------------------------------------------------------

        //Save edited employee data to the _employees list and to the lstVwEmployees control
        private void updateEmployee()
        {
            if (lstVwEmployees.Items.Count == 0)
            {
                return;
            }

            if (validateEmployeeData() == false)
            {
                return;
            }


            int i; 


            //See which email matches, since email is a primary key in the table
            // to find the index of the entry to update
            for (i = 0; i < lstVwEmployees.Items.Count; i++)
            {                
                if(lstVwEmployees.Items[i].SubItems[3].Text.ToLower() == txtBxEmpEmail.Text.ToLower())
                {
                    break;
                }
            }

            int idx = getIndexOfEmployee(lstVwEmployees.Items[i].SubItems[3].Text);

            lstVwEmployees.Items[i].SubItems[0].Text = txtBxEmpFirstName.Text;
            lstVwEmployees.Items[i].SubItems[1].Text = txtBxEmpMiddleName.Text;
            lstVwEmployees.Items[i].SubItems[2].Text = txtBxEmpLastName.Text;
            lstVwEmployees.Items[i].SubItems[3].Text = txtBxEmpEmail.Text.ToLower();
            lstVwEmployees.Items[i].SubItems[4].Text = txtBxEmpPassword.Text;

            NameOfPerson nop = new NameOfPerson();
            nop.setFirstName(txtBxEmpFirstName.Text);
            nop.setMiddleName(txtBxEmpMiddleName.Text);
            nop.setLastName(txtBxEmpLastName.Text);

            _parentForm._employees[idx].setEmployeeName(nop);
            _parentForm._employees[idx].setEmail(txtBxEmpEmail.Text.ToLower());
            _parentForm._employees[idx].setPassword(txtBxEmpPassword.Text);

            string tablesAssigned = "";

            updateWaitStaffInDatabase(nop.getFirstName(), nop.getMiddleName(), nop.getLastName(), 
                        _parentForm._employees[idx].getEmail().ToLower(),txtBxEmpPassword.Text, tablesAssigned);
        }


        //------------------------------------------------- 44 ----------------------------------------------------------

        // Check the database LoggedIn table to see if the employee with the "email" address is current logged in.
        // If he is, return true, otherwise return false.
        private bool isEmployeeLoggedIn(string email)
        {
            //Use stored procedure ... pass in the email and current time in utc time (universal time)
            string extractedEmail = "";
            string connectionString;
            SqlDataReader dataReader;  // for select only 
            bool result = true;
            DateTime dt = DateTime.UtcNow;

            connectionString = _parentForm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "isEmployeeLoggedIn");

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
                    //If this string is empty, then the employee is not logged in
                    if (extractedEmail == "")
                    {                      
                        result = false;                      
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

        //------------------------------------------------- 45 ----------------------------------------------------------

        // Delete the employee selected in lstVwEmployee control and
        // from the database.
        private void btnEmployeeDelete_Click(object sender, EventArgs e)
        {
            string email;
            // No employees, return without doing anything.
            if (lstVwEmployees.Items.Count == 0)
            {
                return;
            }
            
            int i = _lastEmployeeSelected;
            if (i == -1) // No employee selected, return without doing anything.
            {
                return;
            }

            int idx = getIndexOfEmployee(lstVwEmployees.Items[i].SubItems[3].Text);

            email = lstVwEmployees.Items[i].SubItems[3].Text;
            if(isEmployeeLoggedIn(email)== true)
            {
                MessageBox.Show("Can't Edit Employee While He's Logged In.");
                return;
            }
            //Check if employee is online
            // Remove the element from the list, _employees
            _parentForm._employees.RemoveAt(idx);

            // remove the element from the ListView, listVwEmployees
             int indx = lstVwEmployees.Items.IndexOf(lstVwEmployees.SelectedItems[0]);
            lstVwEmployees.Items.RemoveAt(i);
            deleteWaitStaffFromFromDatabase(email);
        }

        //------------------------------------------------- 46 ----------------------------------------------------------

        //Return true if the Email is already in the list        
        private bool duplicateEmail(string email)
        {
            foreach(Employee emp in _parentForm._employees)
            {
                if(emp.getEmail().ToLower() == email.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        //------------------------------------------------- 47 ----------------------------------------------------------


        // Add new employee to both the _employees list and to the lstVwEmployees control
        private void btnEmployeeAdd_Click(object sender, EventArgs e)
        { 

            if (validateEmployeeData() == false)
            {
                return;
            }

            if(duplicateEmail(txtBxEmpEmail.Text)==true){
                //Update, since the email already exists
                if (isEmployeeLoggedIn(txtBxEmpEmail.Text.ToLower()) == true)
                {
                    MessageBox.Show("Can't Edit Employee While He's Logged In.");
                    return;
                }
                updateEmployee();
                return;
            }

            //Add the employee to the _employees list
            NameOfPerson nop = new NameOfPerson();
            nop.setFirstName(txtBxEmpFirstName.Text);
            nop.setMiddleName(txtBxEmpMiddleName.Text);
            nop.setLastName(txtBxEmpLastName.Text);
            WaitStaff emp = new WaitStaff();
            emp.setEmployeeName(nop);
            emp.setEmail(txtBxEmpEmail.Text.ToLower()); //Make sure email is always lower case, since it's used as a primary key
            emp.setPassword(txtBxEmpPassword.Text);

            string assignedTables = "";
            bool result = insertWaitStaffIntoDatabase(nop.getFirstName(), nop.getMiddleName(), nop.getLastName(), emp.getEmail(), emp.getPassword(), assignedTables);
            if(result == false)
            {
                MessageBox.Show("Could not add employee.  Make sure the email address is unique.", "Error", MessageBoxButtons.OK);
                return;
            }
            _parentForm._employees.Add(emp);

            //Add the new employee to the lstVwEmployees control
            ListViewItem lvi = new ListViewItem();
 
            lvi.Text = nop.getFirstName();
            lvi.SubItems.Add(nop.getMiddleName());
            lvi.SubItems.Add(nop.getLastName());
            lvi.SubItems.Add(emp.getEmail());
            lvi.SubItems.Add(emp.getPassword());
            lstVwEmployees.Items.Add(lvi);
        }

        //------------------------------------------------- 48 ----------------------------------------------------------

        // load the WorkTime table from the database for user with passed in email string
        private void loadFromWorkTimeTable(string email,string firstName, string middleName, string lastName)
        {
            long tstart, tend, totalTicks=0;
            string connectionString;
            DateTime dt1, dt2;
            SqlDataReader dataReader;
            TimeSpan elapsedSpan;
  
            long startTime, endTime;
            startTime = dateTimePickerFrom.Value.Ticks;
            dt1 = new DateTime(dateTimePickerFrom.Value.Year, dateTimePickerFrom.Value.Month, dateTimePickerFrom.Value.Day, 0, 0, 0, 0);
 
            dt2 = new DateTime(dateTimePickerTo.Value.Year, dateTimePickerTo.Value.Month, dateTimePickerTo.Value.Day, 0, 0, 0, 0);
            dt2 = dt2.AddDays(1.0);

            startTime = dt1.Ticks;
            endTime = dt2.Ticks;
            if (endTime <= startTime)
            {
                //If endTime is less than startTime, do a week
                dt2 = dateTimePickerFrom.Value.AddDays(7.0);

            }
            startTime = dt1.ToUniversalTime().Ticks;
            endTime = dt2.ToUniversalTime().Ticks;

            connectionString = _parentForm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "loadFromWorkTimeTable");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;
                txtBxWorkHours.Text = "";
                string header;
                if (middleName != "")
                {
                    header = " Hours Worked by " + firstName + " " + middleName + " " + lastName + ":\r\n\r\n";
                }
                else
                {
                    header = " Hours Worked by " + firstName + " " + lastName + ":\r\n\r\n";
                }
                txtBxWorkHours.Text = header;

                try
                {                    

                    command.CommandText = "select login, logout from WorkTime where email=@email order by login asc";
                    command.Parameters.AddWithValue("@email", email);
                    // Attempt to commit the transaction.
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        tstart = (long)dataReader.GetValue(0);
                        tend = (long)dataReader.GetValue(1); // x position of table

               
                        //Only output the range selected
                        if (tstart >= startTime && tstart <= endTime)
                        {
                            dt1 = new DateTime(tstart);
                            dt1 = dt1.ToLocalTime();
                            dt2 = new DateTime(tend);
                            dt2 = dt2.ToLocalTime();
                            totalTicks += tend - tstart;
                            txtBxWorkHours.Text += "From: " + dt1.ToString("ddd, dd MMMM yyyy HH:mm") + "   To: " + dt2.ToString("ddd, dd MMMM yyyy HH:mm") + "\r\n";
                            
                        }                    
                    }
                    elapsedSpan = new TimeSpan(totalTicks);
                    string footer;


                    int hours = (int)(elapsedSpan.TotalHours);
                    int minutes = (int)(elapsedSpan.TotalMinutes) - (int)(elapsedSpan.TotalHours) *60; 
                    
                    footer = "\r\n  Total Time worked -- Hours: " + hours.ToString() + " Minutes: " + minutes.ToString() + "\r\n";
                    txtBxWorkHours.Text += footer;

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

        //------------------------------------------------- 49 ----------------------------------------------------------

        // Display the employee selected in the lstVwEmployees control by
        // filling out the text boxes contained in the panelEmployees panel        
        private void displayEmployee()
        {
            int item;

            if (lstVwEmployees.SelectedItems.Count == 0)
            {
                return;
            }

            item = lstVwEmployees.Items.IndexOf(lstVwEmployees.SelectedItems[0]);
            _lastEmployeeSelected = item;
            txtBxEmpFirstName.Text = (string)lstVwEmployees.Items[item].Text;
            txtBxEmpMiddleName.Text = lstVwEmployees.SelectedItems[0].SubItems[1].Text;
            txtBxEmpLastName.Text = lstVwEmployees.SelectedItems[0].SubItems[2].Text;
            txtBxEmpEmail.Text = lstVwEmployees.SelectedItems[0].SubItems[3].Text;
            txtBxEmpPassword.Text = lstVwEmployees.SelectedItems[0].SubItems[4].Text;

            loadFromWorkTimeTable(txtBxEmpEmail.Text, txtBxEmpFirstName.Text, txtBxEmpMiddleName.Text, txtBxEmpLastName.Text);
        }

        //------------------------------------------------- 50  ----------------------------------------------------------

        // When an employee is clicked on in the lstVwEmployees control,
        // that employee is displayed via this function.
        private void lstVwEmployees_Click(object sender, EventArgs e)
        {
            displayEmployee();
        }

        //------------------------------------------------- 51 ----------------------------------------------------------

        // When the "Print" button is pressed in the panelReservedTable panel
        // this function is activated to launch a window to print the invoice receipt.
        private void btnPrintInvoice_Click(object sender, EventArgs e)
        {
            _lstVwInvoiceDup = lstVwInvoice;
            ReceiptForm f = new ReceiptForm(this);
            f.ShowDialog(this);
        }

        //------------------------------------------------- 52 ----------------------------------------------------------

        // This is the function called by the timer.
        // This should be called once per second.  The function is predicated on that.
        // This function regularly updates the dining table data from the database,
        // and regularly checks to see if a user has been inactive long enough to
        // be logged out.  If a user has been inactive too long, the _forcedLogOff flag
        // is set true, and the form is exited.
        private void tmrClock_Tick(object sender, EventArgs e)
        {
            string dateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");          
            toolStripStatusLabel1.Text = dateTime;
            statusStripMarsRest.Refresh();
            _timerUpdateTableTickCount++;
            //Update the dining tables every 5 ticks
            if(_timerUpdateTableTickCount >= 5 && _appState == AppState.defaultView)                
            {
                _timerUpdateTableTickCount = 0;
                //Update the tables;
                loadTableInfoFromDatabase();
            }
            _timerUpdateUserActivity++;
            if(_timerUpdateUserActivity >= 20)
            {                
                _timerUpdateUserActivity = 0;                
                 //Log user out... remove user from database and force program exit
                _parentForm.removeTimedOutUsers();
                if (_parentForm.isUserLoggedIn(_parentForm._loggedInEmail) == false)
                {
                    //Force log out
                    _forcedLogOff = true;
                    this.Close();
                }else if (_detectUserActivity == 1)
                {
                    _parentForm.updateLoggedInTime(_parentForm._loggedInEmail);
                }
                _detectUserActivity = 0;
            }
        }

        //------------------------------------------------- 53 ----------------------------------------------------------

        // If the dateTimePickerFrom control value changes, the employee data is
        // upated accordingly.
        private void dateTimePickerFrom_ValueChanged(object sender, EventArgs e)
        {
            displayEmployee();
        }

        //------------------------------------------------- 54 ----------------------------------------------------------

        // If the dateTimePickerTo control value changes, the employee data is
        // upated accordingly.
        private void dateTimePickerTo_ValueChanged(object sender, EventArgs e)
        {
            displayEmployee();
        }

        //------------------------------------------------- 55 ----------------------------------------------------------

        // When the "Print Times" button is pressed in the panelEmployees panel,
        // this launches a printDialog() box, which is provided standard with C# Windows forms.
        //  It allows the user to print out the work hours/minutes for the selected employee.
        private void btnPrintWorkTimes_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            PrintDocument documentToPrint = new PrintDocument();
            printDialog.Document = documentToPrint;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {

                documentToPrint.PrintPage += new PrintPageEventHandler(DocumentToPrint_PrintPage);
                documentToPrint.Print();
            }
        }

        //------------------------------------------------- 56 ----------------------------------------------------------

        // This creates the document for employee work hours/minutes to be printed out.
        // It lays out the text fonts and positions.
        private void DocumentToPrint_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            FontFamily ff = FontFamily.GenericMonospace;
            StringReader reader = new StringReader(txtBxWorkHours.Text);
            float LinesPerPage = 0;
            float YPosition = 0;
            int Count = 0;
            float LeftMargin = e.MarginBounds.Left + 150;
            float TopMargin = e.MarginBounds.Top;
            string Line = null;
            Font PrintFont = txtBxWorkHours.Font;
            SolidBrush PrintBrush = new SolidBrush(Color.Black);

            LinesPerPage = e.MarginBounds.Height / PrintFont.GetHeight(e.Graphics);

            YPosition = TopMargin;
            while (Count < LinesPerPage && ((Line = reader.ReadLine()) != null))
            {
                e.Graphics.DrawString(Line, PrintFont, PrintBrush, LeftMargin, YPosition, new StringFormat());
                YPosition += PrintFont.GetHeight(e.Graphics);
                Count++;
            }

            if (Line != null)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
            }
            PrintBrush.Dispose();
        }

        //------------------------------------------------- 57 ----------------------------------------------------------


        // This is meant to detect user activity by mouse moves
        // in the pictureBox1 control.
        // If the mouse moves, the _detectUserActivity flag is set to 1
        private void pictureBox1_MouseMove_1(object sender, MouseEventArgs e)
        {
            _detectUserActivity = 1;
        }

        //------------------------------------------------- 58 ----------------------------------------------------------

        // This is meant to detect user activity by mouse moves
        // in the panelEmployees panel.
        // If the mouse moves, the _detectUserActivity flag is set to 1
        private void panelEmployees_MouseMove(object sender, MouseEventArgs e)
        {
            _detectUserActivity = 1;
        }

        //------------------------------------------------- 59 ----------------------------------------------------------

        // This is meant to detect user activity by mouse moves
        // in the panelReservedTable panel.
        // If the mouse moves, the _detectUserActivity flag is set to 1
        private void panelReserveTable_MouseMove(object sender, MouseEventArgs e)
        {
            _detectUserActivity = 1;
        }

        //------------------------------------------------- 60 ----------------------------------------------------------

        // This is meant to detect user activity by mouse moves
        // in the panelMenu panel.
        // If the mouse moves, the _detectUserActivity flag is set to 1
        private void panelMakeMenu_MouseMove(object sender, MouseEventArgs e)
        {
            _detectUserActivity = 1;
        }
    }
}
