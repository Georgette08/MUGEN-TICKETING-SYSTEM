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
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            var user = db.UserAccount.Where(u => u.Username.Equals(username)).FirstOrDefault();

            if (user == null || !user.Password.Equals(password))
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Text = "";
                return;
            }
                MessageBox.Show("Login successful! Welcome, " + user.Role, "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ClearFields();
                this.Hide();

            string userRole = user.Role;

            Form dashboard = null; 

            if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                dashboard = new AdminDashboard(user);
            }
            else if (userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                dashboard = new StaffDashboard(user);
            }
            else 
            {
                MessageBox.Show("User role not recognized. Access denied.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if (dashboard != null)
            {
                dashboard.ShowDialog();
            }

            this.Show();
        }
        private void ClearFields()
        {
            txtUsername.Text = "";
            txtPassword.Text = "";
        }
    }
} 