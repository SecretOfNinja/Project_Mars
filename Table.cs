using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
namespace Mars_Restaurant
{
    public enum TableState
    {
        empty,
        reserved,
        busy
    }

    public enum TableAction
    {
        move,  // Move the table on the floor
        add,  // Add a table to the floor
        remove // Takes the table off the floor
    }

    // This class retains the data for the dining tables
    // and provides functionality for things such as moving and deleting them
    public class Table
    {
       

        //private Button tableGui;
        private PictureBox _tableGui2; // This holds the image of the dining table look
        private Label _lblNumber; // Label to show the number for the table to the user.
        static private int _imageIndex = 0; //Defaults to zero, the image index for the table  This is a static variable, so it has the same value for all tables.

        private int _tableID; // The number of the table, which is identical with the _tableID.
        private bool _eventsAttached = false;
        private Point pointDelta = new Point(); // Offset position
        private bool _captureMouse=false;
        private Order _order = new Order(); // This holds the invoice order for the dining table
        private TableState _tableState = TableState.empty;



        // class variable
        private DoubleBufferedWindow _window;

        // Whether or not this table is being dragged to the right place
        private bool Dragging { get; set; }

        public bool OnTheFloor { get; set; } // set to true if the table is in use, i.e. on the floor

        public static TableAction Mode { get; set; }        
    

        public static int ImageIndex
        {
            get { return _imageIndex; }
            set { _imageIndex = value; }
        }

        public int TableID
        {
            get { return _tableID; }
            set { _tableID = value;
                _lblNumber.Text = _tableID.ToString();
            }
        }

        // Sets the table state, and then indicates
        // it by setting the color of the label _lblNumber
        public void setTableState(TableState ts)
        {
            _tableState = ts;
            //Set the label color
            if (ts == TableState.empty)
            {
                _lblNumber.ForeColor = Color.Black;
            }else if(ts == TableState.reserved)
            {
                _lblNumber.ForeColor = Color.Blue;
            }else if(ts == TableState.busy)
            {
                _lblNumber.ForeColor = Color.Green;
            }
        }

        public TableState getTableState()
        {
            return _tableState;
        }

        public Order getOrder()
        {
            return _order;
        }

        public void setOrder(ref Order order)
        {
            _order = order;
        }
       
        // When an attach or unattach occurs, this makes sure
        // that the dining tables can be moved with a mouse
        // or not depending on the MarsRestaurant floor's state.
        // If a manager is editing tables, then the tables will
        // be movable. 
        public void buttonEventsAttachUnattach(bool state)
        {
            if(state == true)
            {
                if (_eventsAttached == false)
                {
                    attachEventsToTableGui();
                }
            }
            else
            {
                if (_eventsAttached == true)
                {
                    removeEventsFromTableGui();
                }
            }
        }

        //  The constructor for the Table class.
        // Passed in is the DoubleBufferWindow class with the variable "window"  so
        // that access to it is available
        public Table(DoubleBufferedWindow window)
        {

            this._window = window;
            OnTheFloor = false;
            Mode = TableAction.move;
            initTableGui();
         
        }

        // Add a new table image
        public void AddImage(Image img)
        {
            _tableGui2.Image = img;
            _tableGui2.SizeMode = PictureBoxSizeMode.Zoom; 
            _tableGui2.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        // Makes the dining tables invisible to user
        public void makeInvisible()
        {
            _tableGui2.Visible = false;
            _lblNumber.Visible = false;
        }
        
 
        // Enable the dining table controls
        public void enableGui()
        {
            _tableGui2.Enabled = true;
            _lblNumber.Enabled = true;
        }

        // Disable the dining table controls
        public void disableGui()
        {
            _tableGui2.Enabled = false;
            _lblNumber.Enabled = false;
        }

        // Makes the dining tables visible to user
        public void makeVisible()
        {
            _tableGui2.Visible = true;
            _lblNumber.Visible = true;
        }

        // Initialize the controls for the dining tables
        private void initTableGui()
        {  

            _tableGui2 = new PictureBox();
            _tableGui2.Size = new Size(114, 63); 
            _tableGui2.BackColor = Color.Transparent;
            _tableGui2.Margin = new Padding(3);
            _tableGui2.Padding = new Padding(0);

            _tableGui2.Click += new System.EventHandler(btnLogin_Click);


            _window._pPicBox.Controls.Add(_tableGui2);
            _tableGui2.BringToFront();        
            _tableGui2.Visible = false;
            _tableGui2.Enabled = false;
            _lblNumber = new Label();
            _lblNumber.Font = new Font("Arial", 14, FontStyle.Bold);
            _lblNumber.Text = _tableID.ToString();
            _lblNumber.ForeColor = Color.Black;
            _window._pPicBox.Controls.Add(_lblNumber);
            _lblNumber.BackColor = Color.Transparent;
            _lblNumber.BringToFront();
            _lblNumber.Visible = false;
            _lblNumber.Enabled = false;
        }

        // Launch the table Manager panel so that the employee 
        // can take a customer's order
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (_window._appState != AppState.defaultView)
                return;
            _window._appState = AppState.reserveTable;
            _window.launchTableManagerPanel(TableID,_tableState);

            return;
        }
  
        // Remove events so that the dining tables can't be
        // moved around the restaurant surface.
        private void removeEventsFromTableGui()
        {

            _tableGui2.MouseMove -= TableGui_MouseMove;
            _tableGui2.MouseUp -= tableGui_MouseUp;
            _tableGui2.MouseDown -= tableGui_MouseDown;
            _eventsAttached = false;
        }

        // Add events so that the dining tables can be
        // moved around the restaurant surface.
        private void attachEventsToTableGui()
        {

            _tableGui2.MouseMove += TableGui_MouseMove;
            _tableGui2.MouseUp += tableGui_MouseUp;
            _tableGui2.MouseDown += tableGui_MouseDown;
            _eventsAttached = true;           
        }
           
        // Set the table position to x, y
        public void setTablePosition(int x, int y)
        {
            // Position the table image
            _tableGui2.Left = x; 
            _tableGui2.Top = y;

            // Position the number label for the table
            _lblNumber.Left = x + 46; 
            _lblNumber.Top = y + 60;
        }

        // Return the X position of the dining table
        public int getXpos()
        {
            return _tableGui2.Left;
        }

        // Return the Y position of the dining table
        public int getYpos()
        {
            return _tableGui2.Top;
        }

        //Setting the position from the mouse's click point
        public void tableGui_Position(int x,int y)
        {
            setTablePosition(x - 31, y);         
        }

        // User clicks on a table, the TableAction mode determines the action
        // Either deletes the dining table or captures table for moving on the floor.
        private void tableGui_MouseDown(object sender, MouseEventArgs e)
        {
            if (_window._appState != AppState.addDelMovTables)
                return;
            //Delete table?
            if(Mode == TableAction.remove)
            {

                //Make sure table is empty
                if(_tableState != TableState.empty)
                {
                    MessageBox.Show("Can't Delete Non-Empty Table.");
                    return;
                }
                //Make sure user is not using table
                if(_window.tableInUse2(_tableID) == true)
                {
                    MessageBox.Show("Table Is Being Used Elsewhere.");
                    return;
                }

                OnTheFloor = false;
                disableGui();
                makeInvisible();
                Mode = TableAction.move;// 0;
                _window.updateDiningTableInDatabase(_tableID);
                return;
            }
            var coords = _window._pPicBox.PointToClient(Cursor.Position);
            // Retain the offset position of the table with respect to the whole Restaurant Floor, i.e. _window.PictureBox1
            pointDelta.X = e.X;
            pointDelta.Y = e.Y;

            Console.WriteLine(" Table ID = {0}", TableID);

            _captureMouse = true;
           
        }

        // Release the captured dining table so that the mouse no longer
        // moves it.
        private void tableGui_MouseUp(object sender, MouseEventArgs e)
        {
            if (_captureMouse)
            {
                _captureMouse = false;
                // Update the position of the dining table in the database.
                _window.updateDiningTableInDatabase(_tableID);
            }
        }


        // Move the dining table around the Mars Restaurant floor via the mouse
        private void TableGui_MouseMove(object sender, MouseEventArgs e)
        {
            if(_captureMouse)
            {                
                var coords = _window._pPicBox.PointToClient(Cursor.Position);
                _tableGui2.Left = coords.X - pointDelta.X;
                _tableGui2.Top = coords.Y - pointDelta.Y;
                _lblNumber.Left = coords.X - pointDelta.X + 45;
                _lblNumber.Top = coords.Y - pointDelta.Y + 60;
            }
        }
 
    }
}
