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

namespace MUGEN_SYSTEM
{
    public partial class StationsDashboard : Form
    {
        private int selectedStationID = -1;
        private readonly UserAccount userLogIn;
        public StationsDashboard(UserAccount userLogIn)
        {
            InitializeComponent();
            this.userLogIn = userLogIn;
        }

        private void StationsDashboard_Load(object sender, EventArgs e)
        {
            LoadStationsDataGrid();
            comboStationName.Items.Clear();
            combolLineServed.Items.Clear();

            comboStationName.Items.Add("Tokyo Station");
            comboStationName.Items.Add("Kyoto Station");
            comboStationName.Items.Add("Shinagawa Station");
            comboStationName.Items.Add("Hakata Station");
            comboStationName.Items.Add("Shin-Osaka Station");
            comboStationName.Items.Add("Nagoya Station");
            
            combolLineServed.Items.Add("Tokaido Shinkansen"); 
            combolLineServed.Items.Add("Sanyo Shinkansen"); 
            combolLineServed.Items.Add("Kyushu Shinkansen");
            combolLineServed.Items.Add("Tohoku Shinkansen");    
            combolLineServed.Items.Add("Hokkaido Shinkansen");  
            combolLineServed.Items.Add("Joetsu Shinkansen");    

            comboCity.Items.Add("Chiyoda Ward, Tokyo City");
            comboCity.Items.Add(" Kyoto City");
            comboCity.Items.Add("Minato Ward, Tokyo City");
            comboCity.Items.Add("Fukuoka, Kyushu City");
            comboCity.Items.Add("Yodogawa Ward, Osaka City");
            comboCity.Items.Add("Chubu, Central Japan");

            if (comboStationName.Items.Count > 0)
            {
                comboStationName.SelectedIndex = 0;
            }
            if (combolLineServed.Items.Count > 0)
            {
                combolLineServed.SelectedIndex = 0;
            }
            if (comboCity.Items.Count > 0)
            {
                comboCity.SelectedIndex = 0;
            }
        }
        private void LoadStationsDataGrid()
        {
            using (var dbStations = new MugenSystemDBEntities())
            {
                // Use .Select() to pull only the basic columns needed for the grid.
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
            }
        }
        private void btnADD_Click(object sender, EventArgs e)
        {
            // 1. Retrieve and Validate Data
            string stationName = comboStationName.SelectedItem?.ToString();
            string city = comboCity.SelectedItem?.ToString();
            string LineServed = combolLineServed.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(stationName) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(LineServed))
            {
                MessageBox.Show("Please fill in all station details (Name, City, Line Served).",
                                "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (var dbStations = new MugenSystemDBEntities())
                {
                    if (dbStations.Stations.Any(s => s.StationName == stationName))
                    {
                        MessageBox.Show($"Station '{stationName}' already exists.",
                                        "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // Get the selected row
            DataGridViewRow row = dgvStations.Rows[e.RowIndex];

            // Read the StationID (assuming the column is named "StationID")
            // and store it for Update/Delete operations.
            if (row.Cells["StationID"].Value != null)
            {
                // Try to parse the ID
                if (int.TryParse(row.Cells["StationID"].Value.ToString(), out int stationId))
                {
                    selectedStationID = stationId;
                }
            }

            // Load data into input fields for editing
            string name = row.Cells["StationName"].Value?.ToString();
            string city = row.Cells["City"].Value?.ToString();
            string line = row.Cells["LineServed"].Value?.ToString();

            // Set ComboBox selection based on the row value
            if (!string.IsNullOrEmpty(name))
            {
                comboStationName.SelectedItem = name;
            }
        }      
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedStationID == -1)
            {
                MessageBox.Show("Please select a station from the grid to delete.", "Select Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmation before deleting
            var confirmResult = MessageBox.Show("Are you sure you want to delete this station?",
                                               "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    using (var dbStations = new MugenSystemDBEntities())
                    {
                        // 1. Find the station to delete
                        var stationToDelete = dbStations.Stations.Find(selectedStationID);

                        if (stationToDelete != null)
                        {
                            // 2. Remove the entity
                            dbStations.Stations.Remove(stationToDelete);

                            // 3. Save changes
                            dbStations.SaveChanges();

                            MessageBox.Show("Station deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // 4. Refresh and clear
                            LoadStationsDataGrid();
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

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedStationID == -1)
            {
                MessageBox.Show("Please select a station from the grid to update.", "Select Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Retrieve updated values (assuming you use the ComboBox for name)
            string stationName = comboStationName.SelectedItem?.ToString();
            string city = comboCity.SelectedItem?.ToString();
            string lineServed = combolLineServed.SelectedItem?.ToString(); 

            if (string.IsNullOrEmpty(stationName) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(lineServed))
            {
                MessageBox.Show("Please fill in all station details for the update.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var dbStations = new MugenSystemDBEntities())
                {
                    // 1. Find the existing entity in the context
                    var stationToUpdate = dbStations.Stations.Find(selectedStationID);

                    if (stationToUpdate != null)
                    {
                        // 2. Update its properties
                        stationToUpdate.StationName = stationName;
                        stationToUpdate.City = city;
                        // Note: The Fare relationship/FK must be handled separately if you are updating it.

                        // 3. Save changes
                        dbStations.SaveChanges();

                        MessageBox.Show("Station updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // 4. Refresh and clear
                        LoadStationsDataGrid();
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

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputFields();
            MessageBox.Show("Input fields cleared.", "Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ClearInputFields()
        {
            // Reset selection state
            selectedStationID = -1;
            dgvStations.ClearSelection();

            // Reset ComboBox to the first item (or a default state)
            if (comboStationName.Items.Count > 0)
            {
                comboStationName.SelectedIndex = 0;
            }
        }    
    }

}




