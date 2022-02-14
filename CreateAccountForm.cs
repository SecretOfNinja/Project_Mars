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
    public partial class CreateAccountForm : Form
    {

        LoginForm _loginFrm;



        // -------------------------------- 0 -------------------------------------------------

        // The constuctor for CreateAccountForm.
        // the parameter passed, loginFrm, gives us access to
        // information from the login form.
        public CreateAccountForm(LoginForm loginFrm)
        {
            _loginFrm = loginFrm;

            InitializeComponent();
            // Fix the window size
            this.MinimumSize = new Size(this.Width, this.Height);
            this.MaximumSize = new Size(this.Width, this.Height);
        }


        // -------------------------------- 1 -------------------------------------------------

        // Make sure that the email meets the minimum criteria
        //  If it does, return true, else false.
        private bool validateEmail(string email)
        {
            //
            if(email.Length < 5)
            {
                //Email has to have a pattern like chr_@_chr_.chr, at minimum 5 characters
                return false;
            }

            string[] parts = email.Split('@');
            if (parts.Length != 2)
            {
                return false;
            }
            int pos = parts[1].IndexOf('.');
            if(pos == 0 || pos == parts[1].Length - 1)
            {
                return false;
            }

            return true;
        }

        // -------------------------------- 2 -------------------------------------------------

        // The "Create Account" button on the from was clicked.
        // This function first validates the form data, and if it's
        // valid it creates a Manager account and adds it to the 
        // database.
        private void btnCreateAccount_Click(object sender, EventArgs e)
        {
            string error = ""; 

            //Make sure that the information is correct
            if(txtBxFirstName.Text.Length == 0)
            {
                error = "No First Name Entered";
            }
            else if(txtBxLastName.Text.Length == 0)
            {
                error = "No Last Name Entered";
            }else if(validateEmail(txtBxEmail.Text) == false)
            {
                error = "Invalid Email Address";
            }else if(txtBxPassword.Text.Length == 0)
            {
                error = "No Passwored Entered";
            }else if(txtBxPasswordConfirm.Text.Length == 0)
            {
                error = "No Confirmation Password Entered";
            }
            if(error != "")
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK);
                return;
            }
            else
            {
                //confirm password
                if(txtBxPassword.Text.Length < 8)
                {
                    MessageBox.Show("Password must be at lest 8 characters long", "Error", MessageBoxButtons.OK);
                    return;
                }
                if(txtBxPassword.Text != txtBxPasswordConfirm.Text)
                {
                    MessageBox.Show("Invaid password confirmation", "Error", MessageBoxButtons.OK);
                    return;
                }

                //Determine if account already exists
                foreach(Employee empl in _loginFrm._managers)
                {
                    if(empl.getEmail().ToLower() == txtBxEmail.Text.ToLower())
                    {
                        MessageBox.Show("An Account already exists under this email", "Error", MessageBoxButtons.OK);
                        return;
                    }
                }

                //Assume all is valid at this point:
                //Output data employee list

                StaffManager emp = new StaffManager();
                NameOfPerson nop = new NameOfPerson();
                nop.setFirstName(txtBxFirstName.Text);
                nop.setMiddleName(txtBxMiddleName.Text);
                nop.setLastName(txtBxLastName.Text);
                emp.setEmployeeName(nop);
                emp.setEmail(txtBxEmail.Text);
                emp.setPassword(txtBxPassword.Text);
                addNewManagerAccountToDatabase(nop.getFirstName(), nop.getMiddleName(), nop.getLastName(), emp.getEmail(), emp.getPassword());
                _loginFrm._managers.Add(emp);

                Close();
            }
        }

        // -------------------------------- 3 -------------------------------------------------

        // This adds a new account Manager to the Managers table of the database.
        private void addNewManagerAccountToDatabase(string firstName, string middleName, string lastName, string email, string password)
        {
            string connectionString;

            connectionString = _loginFrm._connectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction(IsolationLevel.Serializable, "InsertManager");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
               
                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText =
                    "Insert into Managers (FirstName, MiddleName, LastName, Email, Passw) VALUES (@fname,@mname,@lname,@email,@pword)";
   

                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@fname", firstName);                
                    command.Parameters.AddWithValue("@mname",middleName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@email",email);
                    command.Parameters.AddWithValue("@pword", password);
                    command.ExecuteNonQuery();
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

        // -------------------------------- 4 -------------------------------------------------

        // This is envoked when the "Back" button on the form is clicked
        // The function closes the current form.
        private void btnBack_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
