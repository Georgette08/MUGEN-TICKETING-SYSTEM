using MUGENTICKETSYSTEM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MUGEN_SYSTEM
{
    public partial class UserDashboard : Form
    {
        private readonly UserAccount userLogIn;
        private int selectedUserID = -1;
        public UserDashboard(UserAccount userLogIn)
        {
            InitializeComponent();
            this.userLogIn = userLogIn;
        }

        private void UserDashboard_Load(object sender, EventArgs e)
        {
            LoadUsersDataGrid();
            comboRole.Items.Clear();
            comboStatus.Items.Clear();

            comboRole.Items.Add("Admin");
            comboRole.Items.Add("Staff");

            comboStatus.Items.Add("Active");
            comboStatus.Items.Add("Inactive");

            if (comboRole.Items.Count > 0)
            {
                comboRole.SelectedIndex = 0;
            }
            if (comboStatus.Items.Count > 0)
            {
                comboStatus.SelectedIndex = 0;
            }
        }
        private void LoadUsersDataGrid()
        {
            using (var db = new MugenSystemDBEntities())
            {
                var users = db.UserAccount.Select(u => new
                {
                    u.UserID,
                    u.Username,
                    u.FullName,
                    u.Password,
                    u.Role,
                    u.Status
                }).ToList();
                dataGridViewUsers.DataSource = users;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string Username = txtUsername.Text.Trim();
            string Password = txtPassword.Text.Trim();
            string FullName = txtFullName.Text;
            string Role = comboRole.SelectedItem?.ToString();
            string Status = comboStatus.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) ||
                 string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Role) || string.IsNullOrEmpty(Status))
            {
                MessageBox.Show("All fields must be filled in.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var dbUser = new MugenSystemDBEntities())
                {
                    if (dbUser.UserAccount.Any(s => s.Username == Username))
                    {
                        MessageBox.Show($"UserAcoount '{Username}' already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var newUser = new UserAccount
                    {
                        Password = Password,
                        Username = Username,
                        FullName = FullName,
                        Role = Role,
                        Status = Status
                    };

                    dbUser.UserAccount.Add(newUser);
                    dbUser.SaveChanges();

                    MessageBox.Show("New User Account added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadUsersDataGrid();
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the account: {ex.InnerException?.Message ?? ex.Message}",
                         "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            {
                if (selectedUserID == -1)
                {
                    MessageBox.Show("Please select an account from the grid to update.", "Select Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }              
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();
                string fullName = txtFullName.Text.Trim();
                string role = comboRole.SelectedItem?.ToString();
                string status = comboStatus.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
                {
                    MessageBox.Show("Username and Role are required for the update.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using (var db = new MugenSystemDBEntities())
                    {
                        var userToUpdate = db.UserAccount.Find(selectedUserID); 

                        if (userToUpdate != null)
                        {

                            userToUpdate.Username = username;
                            userToUpdate.FullName = fullName;
                            userToUpdate.Role = role;
                            userToUpdate.Status = status;

                            if (!string.IsNullOrEmpty(password))
                            {
                                userToUpdate.Password = password;
                            }

                            db.SaveChanges();

                            MessageBox.Show("User account updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadUsersDataGrid();
                            ClearInputFields(); 
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during update: {ex.InnerException?.Message ?? ex.Message}",
                                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
 
            if (selectedUserID == -1)
            {
                MessageBox.Show("Please select an account from the grid to delete.", "Select Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure you want to delete this user account?",
                                               "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    using (var db = new MugenSystemDBEntities())
                    {
                        var userToDelete = db.UserAccount.Find(selectedUserID); 

                        if (userToDelete != null)
                        {
                            db.UserAccount.Remove(userToDelete);
                            db.SaveChanges();

                            MessageBox.Show("User account deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadUsersDataGrid();
                            ClearInputFields();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during deletion: {ex.InnerException?.Message ?? ex.Message}",
                                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridViewUsers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex == dataGridViewUsers.NewRowIndex)
            {
                selectedUserID = -1;
                return;
            }

            DataGridViewRow row = dataGridViewUsers.Rows[e.RowIndex];

            if (row.Cells["UserID"].Value != null)
            {
                if (int.TryParse(row.Cells["UserID"].Value.ToString(), out int userID))
                {
                    selectedUserID = userID;
                }
                else
                {
                    selectedUserID = -1;
                }
            }

            UserID.Text = row.Cells["UserID"].Value?.ToString();
            txtFullName.Text = row.Cells["FullName"].Value?.ToString();
            txtUsername.Text = row.Cells["Username"].Value?.ToString();
            txtPassword.Text = row.Cells["Password"].Value?.ToString();

            comboRole.SelectedItem = row.Cells["Role"].Value?.ToString();
            comboStatus.SelectedItem = row.Cells["Status"].Value?.ToString();
        }
        private void ClearInputFields()
        {

            selectedUserID = -1;

            UserID.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            txtFullName.Clear();

            if (comboRole.Items.Count > 0)
            {
                comboRole.SelectedIndex = 0;
            }
            if (comboStatus.Items.Count > 0)
            {
               comboStatus.SelectedIndex = 0;
            }        
        }
    }
}




