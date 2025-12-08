using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;
using System.Data.Entity;
using MUGENTICKETSYSTEM;

namespace MUGEN_SYSTEM
{
    public partial class StationsDashboard : Form
    {
        private int selectedStationID = -1;
        private readonly UserAccount userLogIn;
        private List<string> City = new List<string>();
        private List<string> LineServed = new List<string>();

        public StationsDashboard(UserAccount userLogIn)
        {
            InitializeComponent();
            this.userLogIn = userLogIn;
        }

        private void StationsDashboard_Load(object sender, EventArgs e)
        {
            LoadStationsDataGrid();
            comboStationName.Items.Clear();       

            //  Predefined station names 
            comboStationName.Items.Add("Tokyo Station");
            comboStationName.Items.Add("Kyoto Station");
            comboStationName.Items.Add("Shinagawa Station");
            comboStationName.Items.Add("Hakata Station");
            comboStationName.Items.Add("Shin-Osaka Station");
            comboStationName.Items.Add("Nagoya Station");

            LineServed.Add("Tokaido Shinkansen");
            LineServed.Add("Sanyo Shinkansen");
            LineServed.Add("Kyushu Shinkansen");
            LineServed.Add("Tohoku Shinkansen");
            LineServed.Add("Hokkaido Shinkansen");
            LineServed.Add("Joetsu Shinkansen");

            City.Add("Chiyoda Ward, Tokyo City");
            City.Add(" Kyoto City");
            City.Add("Minato Ward, Tokyo City");
            City.Add("Fukuoka, Kyushu City");
            City.Add("Yodogawa Ward, Osaka City");
            City.Add("Chubu, Central Japan");

            if (comboStationName.Items.Count > 0)
            {
                comboStationName.SelectedIndex = 0;
            }
            UpdateDependentFields();
        }
      
        private void UpdateDependentFields()
        {
            int selectedIndex = comboStationName.SelectedIndex;

            if (selectedIndex >= 0)
            {
                if (selectedIndex < City.Count)
                    txtCity.Text = City[selectedIndex].ToString();

                if (selectedIndex < LineServed.Count)
                    txtLineServed.Text = LineServed[selectedIndex].ToString(); // Use the item from the reference list
            }
            else
            {
                txtCity.Clear();
                txtLineServed.Clear();
            }
        }
        private void LoadStationsDataGrid()
        {
            using (var dbStations = new MugenSystemDBEntities())
            {
                var stationsList = dbStations.Stations
                                             .Select(s => new
                                             {
                                                 s.StationID,
                                                 s.StationName,
                                                 s.City,
                                                 s.LineServed
                                             })
                                             .ToList();

                dgvStations.DataSource = stationsList;
                dgvStations.ClearSelection();
            }
        }
        private void ClearInputFields()
        {
            selectedStationID = -1;
            dgvStations.ClearSelection();

            
            txtCity.Clear();
            txtLineServed.Clear();
          
            if (comboStationName.Items.Count > 0)
            {
                comboStationName.SelectedIndex = 0;
            }
           
            UpdateDependentFields();
        }
        private void btnADD_Click(object sender, EventArgs e)
        {
            string stationName = comboStationName.SelectedItem?.ToString();
            string city = txtCity.Text;
            string LineServed = txtLineServed.Text;

            if (string.IsNullOrEmpty(stationName) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(LineServed))
            {
                MessageBox.Show("Please fill in all station details.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (var dbStations = new MugenSystemDBEntities())
                {
                    if (dbStations.Stations.Any(s => s.StationName == stationName))
                    {
                        MessageBox.Show($"Station '{stationName}' already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var newStation = new Stations
                    {
                        StationName = stationName,
                        City = city,
                        LineServed = LineServed
                    };

                    dbStations.Stations.Add(newStation);
                    dbStations.SaveChanges();

                    MessageBox.Show("New station added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadStationsDataGrid();
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the station: {ex.InnerException?.Message ?? ex.Message}",
                                "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void dgvStations_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvStations.Rows[e.RowIndex];

            if (row.Cells["StationID"].Value != null && int.TryParse(row.Cells["StationID"].Value.ToString(), out int stationId))
            {
                selectedStationID = stationId;
            }
            else
            {
                selectedStationID = -1;
            }

            string name = row.Cells["StationName"].Value?.ToString();
            string city = row.Cells["City"].Value?.ToString();
            string line = row.Cells["LineServed"].Value?.ToString();

            if (!string.IsNullOrEmpty(name))
            {
                comboStationName.SelectedItem = name;
            }

            txtCity.Text = city;
            txtLineServed.Text = line;
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedStationID == -1)
            {
                MessageBox.Show("Please select a station from the grid to delete.", "Select Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure you want to delete this station?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    using (var dbStations = new MugenSystemDBEntities())
                    {
                        var stationToDelete = dbStations.Stations.Find(selectedStationID);

                        if (stationToDelete != null)
                        {
                            dbStations.Stations.Remove(stationToDelete);
                            dbStations.SaveChanges();

                            MessageBox.Show("Station deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadStationsDataGrid();
                            ClearInputFields();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during deletion: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedStationID == -1)
            {
                MessageBox.Show("Please select a station from the grid to update.", "Select Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ FIX: Retrieve data from TextBoxes
            string stationName = comboStationName.SelectedItem?.ToString();
            string city = txtCity.Text;
            string lineServed = txtLineServed.Text;

            if (string.IsNullOrEmpty(stationName) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(lineServed))
            {
                MessageBox.Show("Please fill in all station details for the update.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var dbStations = new MugenSystemDBEntities())
                {
                    var stationToUpdate = dbStations.Stations.Find(selectedStationID);

                    if (stationToUpdate != null)
                    {
                        stationToUpdate.StationName = stationName;
                        stationToUpdate.City = city;
                        stationToUpdate.LineServed = lineServed;

                        dbStations.SaveChanges();

                        MessageBox.Show("Station updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadStationsDataGrid();
                        ClearInputFields();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during update: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboStationName_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            UpdateDependentFields();
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(
               "Are you sure you want to log out?",
               "Confirm Logout",
               MessageBoxButtons.YesNo,
               MessageBoxIcon.Question
           );

            if (confirmResult == DialogResult.Yes)
            {
                LoginForm login = new LoginForm();

                ShowandManageForm(login);
            }
        }

        private void ShowandManageForm(Form newform)
        {
            this.Hide();

            newform.ShowDialog();

            this.Show();
        }

        private void btnTrains_Click(object sender, EventArgs e)
        {
           TrainsDashboard trains = new TrainsDashboard(userLogIn);
            ShowandManageForm(trains);


            this.Close();
        }

        private void btnSchedules_Click(object sender, EventArgs e)
        {
            SchedulesDashboard schedules = new SchedulesDashboard(userLogIn);
            ShowandManageForm(schedules);


            this.Close();
        }

        private void btnFares_Click(object sender, EventArgs e)
        {
            FaresDashboard fares = new FaresDashboard(userLogIn);  
            ShowandManageForm(fares);


            this.Close();
        }

        private void btnAccounts_Click(object sender, EventArgs e)
        {
            UserDashboard Account = new UserDashboard(userLogIn);
            ShowandManageForm(Account);


            this.Close();
        }
        private void btnDashboard_Click(object sender, EventArgs e)
        {
            AdminDashboard adminDashboard = new AdminDashboard(userLogIn);
            ShowandManageForm(adminDashboard);


            this.Close();
        }
    }
}




