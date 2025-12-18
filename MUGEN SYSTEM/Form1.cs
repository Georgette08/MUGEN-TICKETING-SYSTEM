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
            string username = txtUsername.Text.Trim(); 
            string password = txtPassword.Text;

            try
            { 
                using (var db = new MugenSystemDBEntities())
                {

                    var user = db.UserAccount.FirstOrDefault(u => u.Username == username);

                    if (user == null || !user.Password.Equals(password))
                    {
                        MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPassword.Clear();
                        return;
                    }

                    if (user.Status != null && user.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Your account is currently inactive. Please contact your Administrator.",
                                        "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        txtPassword.Clear();
                        return;
                    }

                    MessageBox.Show($"Login successful! Welcome, {user.FullName}", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ClearFields();
                    SessionManager.CurrentAgentID = user.UserID; 

                    Form dashboard = null;

                    if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        dashboard = new AdminDashboard(user);
                    }
                    else if (user.Role.Equals("Staff", StringComparison.OrdinalIgnoreCase))
                    {
                        dashboard = new StaffDashboard(user);
                    }
                    else
                    {
                        MessageBox.Show("User role not recognized. Access denied.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    if (dashboard != null)
                    {
                        this.Hide();
                        dashboard.ShowDialog();
                        this.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ClearFields()
        {
            txtUsername.Text = "";
            txtPassword.Text = "";
        }
    }
} 