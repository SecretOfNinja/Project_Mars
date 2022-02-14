using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace Mars_Restaurant
{
    public partial class AdminForm : Form
    {

        LoginForm _loginFrm; // Member variable for the login form object

        // The constuctor for AdminForm.
        // the parameter passed, loginFrm, gives us access to
        // information from the login form.
        public AdminForm(LoginForm loginFrm)
        {
            _loginFrm = loginFrm; 

            InitializeComponent();
            //Populate lstVwManagers
            loadManagersIntoForm();

            // Fix the window size
            this.MinimumSize = new Size(this.Width, this.Height);
            this.MaximumSize = new Size(this.Width, this.Height);
        }


        // -------------------------------- 0 -------------------------------------------------

        // List the managers in the ListViewItem control.
        // The managers are acquired from the LoginForm and passed here.
        // Note, we only allow one manager for this version,
        // so that will be all that is displayed.
        private void loadManagersIntoForm()
        {
            if (_loginFrm._managers.Count == 0)
            {
                return;
            }

            foreach (Employee emp in _loginFrm._managers)
            {
                ListViewItem lvi = new ListViewItem();
                NameOfPerson nop = emp.getEmployeeName();
                lvi.Text = nop.getFirstName();
                lvi.SubItems.Add(nop.getMiddleName());
                lvi.SubItems.Add(nop.getLastName());
                lvi.SubItems.Add(emp.getEmail());
                lvi.SubItems.Add(emp.getPassword());
                lstVwManagers.Items.Add(lvi);
            }
        }

        // -------------------------------- 1 -------------------------------------------------

        //Delete the manager entry
        private void lstVwManagers_DoubleClick(object sender, EventArgs e)
        {
            string firstName, lastName, middleName, email, password;
            string message;

            if (lstVwManagers.SelectedItems.Count == 0)
                return;

            int indx = lstVwManagers.Items.IndexOf(lstVwManagers.SelectedItems[0]);

            firstName = lstVwManagers.Items[indx].SubItems[0].Text;
            middleName = lstVwManagers.Items[indx].SubItems[1].Text;
            lastName = lstVwManagers.Items[indx].SubItems[2].Text;
            email = lstVwManagers.Items[indx].SubItems[3].Text;
            password = lstVwManagers.Items[indx].SubItems[4].Text;

            message = "Do you want to remove " + firstName + " " + lastName + " from the database?";

            DialogResult dialogResult = MessageBox.Show(message, "Mars Restaurant", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                //Save the state of the dinning tables here                
                lstVwManagers.Items.RemoveAt(indx);
                int i;
                //Also must remove the manager from the _managers list
                for(i=0; i < _loginFrm._managers.Count(); i++)
                {
                    if (_loginFrm._managers[i].getEmail() == email)
                    {
                        _loginFrm._managers.RemoveAt(i);
                        break;
                    }
                }
                deleteManagerFromDatabase(email);
            }
        }

        // -------------------------------- 2 -------------------------------------------------

        //Delete manager from database.  Email is a primary key
        private void deleteManagerFromDatabase(string email)
        {
            string connectionString;

            connectionString = _loginFrm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("DeleteManagerFromDatabase");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {

                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText = "Delete From Managers Where Email=@email";
                    command.CommandType = CommandType.Text;                    
                    command.Parameters.AddWithValue("@email", email);
                    command.ExecuteNonQuery();

                    command = connection.CreateCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;

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



        // -------------------------------- 3 -------------------------------------------------

        // Update the WaitStaff table.  Return false on failure.
        private bool updateManagersPasswordInDatabase(string email, string password)
        {
            bool result = true;
            string connectionString;

            connectionString = _loginFrm._connectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("updateManagersPassword");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
               // command.Connection = connection;
               // command.Transaction = transaction;

                try
                {
                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText =
                        "Update Managers Set Passw=@passw Where Email=@email";


                    command.Parameters.Add("@passw", SqlDbType.NVarChar);

                    command.Parameters.Add("@email", SqlDbType.NVarChar);

                    command.Parameters["@passw"].Value = password;

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


        // -------------------------------- 4 -------------------------------------------------

        // Upon clicking the "Change" button on the form this is activated to change the password
        // of the manager,  it updates the lstVwManagers control and database with the new password.
        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            if(lstVwManagers.Items.Count == 0)
            {
                MessageBox.Show("There is no manager", "Mars Restaurant", MessageBoxButtons.OK);
                return;
            }

            //confirm password
            if (txtBxPassword.Text.Length < 8)
            {
                MessageBox.Show("Password must be at lest 8 characters long", "Error", MessageBoxButtons.OK);
                return;
            }

            string firstName, lastName, email;
            firstName = _loginFrm._managers[0].getEmployeeName().getFirstName();
            lastName = _loginFrm._managers[0].getEmployeeName().getLastName();
            email = _loginFrm._managers[0].getEmail();
            string message = "Do you want to change the password for " + firstName + " " + lastName + "?";

            DialogResult dialogResult = MessageBox.Show(message, "Mars Restaurant", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                //Update the lstVwControl
                lstVwManagers.Items[0].SubItems[4].Text = txtBxPassword.Text;
                //Save the state of the dinning tables here                

                _loginFrm._managers[0].setPassword(txtBxPassword.Text);
                updateManagersPasswordInDatabase(email,txtBxPassword.Text);
            }
        }
    }
}
