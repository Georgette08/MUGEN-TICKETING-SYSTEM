using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using MUGEN_SYSTEM; 

namespace MUGENTICKETSYSTEM
{
    public partial class LoginForm : Form
    {
        private static MugenSystemDBEntities db = new MugenSystemDBEntities();

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
        public LoginForm()
        {
            InitializeComponent();
        }
        private void btnLogIn_Click(object sender, EventArgs e)
        {
            {
                string username = txtUsername.Text;
                string password = txtPassword.Text;

                // Assuming 'db' is your MugenSystemDBEntities instance
                var user = db.UserAccount.Where(u => u.Username.Equals(username)).FirstOrDefault();

                if (user == null || !user.Password.Equals(password))
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Text = "";
                    return;
                }

                MessageBox.Show("Login successful! Welcome, " + user.Role, "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearFields();
                // It's generally better to hide the current form after the new form is shown successfully
                // this.Hide(); 

                string userRole = user.Role;

                Form dashboard = null;

                if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    // ASSUMPTION: The UserAccount class has a property named UserID or AgentID for the PK.
                    // If Admin roles need to perform staff-like actions later, they should also set the SessionManager.
                    SessionManager.CurrentAgentID = user.UserID; // Set the Agent ID for the session
                    dashboard = new AdminDashboard(user);
                }
                else if (userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
                {
                    // CRITICAL FIX: Assign the authenticated user's ID to the Session Manager.
                    // This is what prevents AgentID from being 0 in the booking table.
                    SessionManager.CurrentAgentID = user.UserID; // <-- The essential line!

                    dashboard = new StaffDashboard(user);
                }
                else
                {
                    MessageBox.Show("User role not recognized. Access denied.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (dashboard != null)
                {
                    this.Hide(); // Now hide the login form
                    dashboard.ShowDialog();
                }

                // This ensures the LoginForm reappears after the dashboard is closed (if ShowDialog was used)
                this.Show();
            }
        }
        private void ClearFields()
        {
            txtUsername.Text = "";
            txtPassword.Text = "";
        }
    }
} 