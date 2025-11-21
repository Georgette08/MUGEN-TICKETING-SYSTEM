using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using MUGEN_SYSTEM;


namespace MUGENTICKETSYSTEM
{
    public partial class LogInForm : Form
    {
        private string connectionString = "Server=JayPerales;Initial catalog=MugenSystemDB;Integrated Security=True";
        private object usersTableAdapter;
        private object adapter;

        public object MUGENSYSTEMDataSet { get; private set; }
         
        public LogInForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UserAccounts();
        }  

        private void UserAccounts()
        {
           string connectionString = "Server=JayPerales;Initial catalog=MugenSystemDB;Integrated Security=True";
              string adminPassword = "admin123";
              string staffPassword = "staff123";

            string checkQuery = "SELECT COUNT(*) FROM UserAccounts";

            // SQL to insert the two accounts
            string insertQuery = @"
                INSERT INTO UserAccounts (Username, PasswordHash, FullName, Role, Status) VALUES 
                ('admin_sys', @AdminPass, 'System Administrator', 'Admin', 'Active'),
                ('staff_agent', @StaffPass, 'Booking Agent', 'Staff', 'Active');
             ";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // --- 2. Check if table is empty ---
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        int accountCount = (int)checkCmd.ExecuteScalar();

                        if (accountCount == 0)
                        {
                            // --- 3. Execute Insert if empty ---
                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                            {
                                // Add parameters securely (even though the value is plaintext here)
                                insertCmd.Parameters.AddWithValue("@AdminPass", adminPassword);
                                insertCmd.Parameters.AddWithValue("@StaffPass", staffPassword);

                                insertCmd.ExecuteNonQuery();
                                MessageBox.Show("Initial Admin and Staff accounts have been created successfully!", "Database Seeded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            //MessageBox.Show("UserAccounts table already contains data. Skipping seed process.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A critical error occurred while seeding the database: " + ex.Message, "Seeding Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {           
            string username = txtUsername.Text;
            string password = txtPassword.Text; 
            
            string query = "SELECT Role FROM UserAccounts WHERE Username = @Username AND PasswordHash = @Password";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        string role = (string)command.ExecuteScalar();

                        if (role != null)
                        {
                            if (role == "Admin")
                            {
                    
                                MessageBox.Show("Login Successful! Welcome, Admin.");

                                // Navigate to the Admin Dashboard (e.g., Form4/Form5)
                               AdminDashboard  ADMINFORM = new AdminDashboard();
                                ADMINFORM.Show();
                                this.Hide(); // Hide the login form
                            }
                            else if (role == "Staff" || role == "Customer")
                            {
                                // Route to Search for Trains (Form2)
                                MessageBox.Show("Login Successful! Welcome to Mugen Ticketing.");

                                // Navigate to the Search Form (Form2)
                                StaffDashboard searchForm = new StaffDashboard();
                                searchForm.Show();
                                this.Hide(); // Hide the login form
                            }
                        }
                        else
                        {            
                            MessageBox.Show("Invalid Username or Password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during login: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
