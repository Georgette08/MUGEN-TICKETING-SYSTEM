using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MUGEN_SYSTEM.SchedulesDashboard;
using MUGENTICKETSYSTEM;

namespace MUGEN_SYSTEM
{
    public partial class FaresDashboard : Form
    {
        private readonly UserAccount userLogIn;
        private int selectedFareId = -1;

        private Dictionary<string, List<string>> ValidRoutes = new Dictionary<string, List<string>>();
        private List<ScheduleComboItem> AllStationsList = new List<ScheduleComboItem>();
        private List<ScheduleComboItem> availableTrains = new List<ScheduleComboItem>();


        public FaresDashboard(UserAccount userLogIn)
        {
            InitializeComponent();
            this.userLogIn = userLogIn;
        }

        private void FaresDashboard_Load(object sender, EventArgs e)
        {
            DefineValidRoutes();
            LoadFareDataGrid();
            PopulateComboBoxes();
            LoadServiceClasses();
        }
        private void LoadFareDataGrid()
        {
            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    // Select all necessary Fare data
                    var fares = db.Fare
                        .Select(f => new
                        {
                            // 1. PRIMARY KEY (Essential for Update/Delete)
                            f.FareID,

                            // 2. DISPLAY COLUMNS
                            DepartureStation = f.Stations.StationName, // Use navigation property to get the Name
                            ArrivalStation = f.Stations1.StationName,  // Use navigation property to get the Name
                            f.ClassOfService, // Assuming you have added this column to your Fare table
                            f.FareAmount,

                            // 3. HIDDEN FOREIGN KEY COLUMNS (Essential for Update/Selection)
                            f.DepartureStationID,
                            f.ArrivalStationID
                        })
                        .ToList();

                    dataFareGridView.DataSource = fares;
                    dataFareGridView.ClearSelection();

                    // Optional: Hide the ID columns, as they are only needed for the code, not the user
                    if (dataFareGridView.Columns.Contains("FareID"))
                        dataFareGridView.Columns["FareID"].Visible = false;
                    if (dataFareGridView.Columns.Contains("DepartureStationID"))
                        dataFareGridView.Columns["DepartureStationID"].Visible = false;
                    if (dataFareGridView.Columns.Contains("ArrivalStationID"))
                        dataFareGridView.Columns["ArrivalStationID"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading fares data: {ex.Message}", "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public class ScheduleComboItem
        {
            public int Id { get; set; } // The ID that saves to the database (FK)
            public string Name { get; set; } // The Name displayed in the combo box
            public override string ToString() => Name;
        }

        private void DefineValidRoutes()
        {
            ValidRoutes.Clear();

            // *** FIX: Map Shinkansen Line Names to Valid Arrival Line Names ***

            // 1. Tokiado Shinkansen (e.g., Tokyo area) can connect to:
            ValidRoutes.Add("Tokaido Shinkansen", new List<string> { "Sanyo Shinkansen", "Tohoku Shinkansen", "Hokkaido Shinkansen" });

            // 2. Sanyo Shinkansen (e.g., Osaka/Kyoto area) can connect to:
            ValidRoutes.Add("Sanyo Shinkansen", new List<string> { "Tokaido Shinkansen", "Kyusu Shinkansen" });

            // 3. Kyusu Shinkansen (e.g., Fukuoka area) can connect to:
            ValidRoutes.Add("Kyusu Shinkansen", new List<string> { "Sanyo Shinkansen" });

            // 4. Tohoku Shinkansen can connect to:
            ValidRoutes.Add("Tohoku Shinkansen", new List<string> { "Tokaido Shinkansen", "Hokkaido Shinkansen" });

            // 5. Hokkaido Shinkansen can connect to:
            ValidRoutes.Add("Hokkaido Shinkansen", new List<string> { "Tohoku Shinkansen", "Tokaido Shinkansen" });

            // Add any other specific pairings based on your system's logic
        }
        private void PopulateComboBoxes()
        {
            availableTrains.Clear();
            AllStationsList.Clear(); // Clear the list before reloading

            comboDepartureStation.Items.Clear();
            comboArrivalStation.Items.Clear();

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    // 2A. LOAD TRAIN DATA from DB
                    availableTrains = db.Trains
                        .Select(t => new ScheduleComboItem { Id = t.TrainID, Name = t.TrainName })
                        .ToList();

                    // 2B. CRITICAL FIX: LOAD STATION DATA from DB (Since IDs change)
                    // You must verify the table name 'Stations' and the column 'StationID' in your DB model.
                    AllStationsList = db.Stations
                        .Select(s => new ScheduleComboItem { Id = s.StationID, Name = s.StationName })
                        .ToList();
                }


                // 4. BIND STATIONS (Using the master list loaded from the DB)
                comboDepartureStation.DataSource = AllStationsList.ToList();
                comboDepartureStation.DisplayMember = "Name";
                comboDepartureStation.ValueMember = "Id";

                comboArrivalStation.DataSource = AllStationsList.ToList();
                comboArrivalStation.DisplayMember = "Name";
                comboArrivalStation.ValueMember = "Id";
            }


            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadServiceClasses()
        {
            // Define the standardized Shinkansen service classes
            List<string> serviceClasses = new List<string>
            {
                "Ordinary Car",
                "Green Car",
                "Gran Class"
            };

            // Bind the list to the ComboBox
            comboClass.DataSource = serviceClasses;

            // Set a default selection
            if (serviceClasses.Any())
            {
                comboClass.SelectedIndex = 0;
            }
        }
        private void ClearInputFields()
        {
            comboDepartureStation.SelectedIndex = -1;
            comboArrivalStation.SelectedIndex = -1;
            comboClass.SelectedIndex = -1;

            // --- THIS LINE CLEARS THE FARE AMOUNT TEXTBOX ---
            txtAmount.Text = string.Empty;

            selectedFareId = -1;
            // Clear selection on the DataGridView
            dataFareGridView.ClearSelection();
        }
        private void comboDepartureStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Temporarily remove the handler to avoid recursive binding / filtering issues
            comboArrivalStation.SelectedIndexChanged -= comboDepartureStation_SelectedIndexChanged;

            // The selected item contains the StationID (Id) and the Line Name (Name)
            var selectedDeparture = comboDepartureStation.SelectedItem as ScheduleComboItem;

            // Check for null selection
            if (selectedDeparture == null || string.IsNullOrEmpty(selectedDeparture.Name))
            {
                comboArrivalStation.DataSource = null;
                return;
            }

            // 1. Look up the valid arrival names using the Departure Line Name
            if (ValidRoutes.TryGetValue(selectedDeparture.Name, out List<string> validArrivalNames))
            {
                // 2. Filter the complete AllStationsList to find the ScheduleComboItems 
                //    that match the valid arrival NAMES.
                var filteredArrivalStations = AllStationsList
                    .Where(s => validArrivalNames.Contains(s.Name))
                    .ToList();

                // 3. Rebind the Arrival ComboBox with the filtered list
                comboArrivalStation.DataSource = filteredArrivalStations;
                comboArrivalStation.DisplayMember = "Name";
                comboArrivalStation.ValueMember = "Id";

                // 4. Reset selection to the first item
                if (filteredArrivalStations.Any())
                    comboArrivalStation.SelectedIndex = 0;
                else
                    comboArrivalStation.DataSource = null; // Clear if no valid routes found
            }
            else
            {
                // If the selected Departure Station isn't defined in the map, clear the Arrival box
                comboArrivalStation.DataSource = null;
                comboArrivalStation.Items.Clear();
            }

            // Re-add the handler
            comboArrivalStation.SelectedIndexChanged += comboDepartureStation_SelectedIndexChanged;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var selectedDepStation = comboDepartureStation.SelectedItem as ScheduleComboItem;
            var selectedArrStation = comboArrivalStation.SelectedItem as ScheduleComboItem;
            string selectedClass = comboClass.SelectedItem?.ToString();

            // 1. Input Validation and Parsing
            if (selectedDepStation == null || selectedArrStation == null || string.IsNullOrEmpty(selectedClass))
            {
                MessageBox.Show("Please select both Departure/Arrival Stations and a Service Class.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal fareAmount;
            if (!decimal.TryParse(txtAmount.Text, out fareAmount) || fareAmount <= 0)
            {
                MessageBox.Show("Please enter a valid positive numerical value for the Fare Amount.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Prevent same station
            if (selectedDepStation.Id == selectedArrStation.Id)
            {
                MessageBox.Show("Departure and Arrival Stations cannot be the same.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    // 2. Business Logic Validation: Check for duplicate Fare (Same Route + Same Class)
                    bool fareExists = db.Fare.Any(f =>
                        f.DepartureStationID == selectedDepStation.Id &&
                        f.ArrivalStationID == selectedArrStation.Id &&
                        f.ClassOfService == selectedClass // Assuming you add this column
                    );

                    if (fareExists)
                    {
                        MessageBox.Show($"A fare record already exists for the route {selectedDepStation.Name} to {selectedArrStation.Name} with the Class: {selectedClass}.",
                                        "Duplicate Fare Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 3. Save New Fare Record
                    var newFare = new Fare
                    {
                        DepartureStationID = selectedDepStation.Id,
                        ArrivalStationID = selectedArrStation.Id,
                        FareAmount = fareAmount,
                        ClassOfService = selectedClass // Assuming this field exists
                    };

                    db.Fare.Add(newFare);
                    db.SaveChanges();

                    MessageBox.Show("Fare added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadFareDataGrid(); // Refresh grid
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the fare: {ex.InnerException?.Message ?? ex.Message}",
                                 "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedFareId == -1)
            {
                MessageBox.Show("Please select a Fare record from the table to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure you want to delete this fare record?",
                                                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    using (var db = new MugenSystemDBEntities())
                    {
                        var fareToDelete = db.Fare.SingleOrDefault(f => f.FareID == selectedFareId);

                        if (fareToDelete != null)
                        {
                            db.Fare.Remove(fareToDelete);
                            db.SaveChanges();

                            MessageBox.Show("Fare deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadFareDataGrid();
                            ClearInputFields();
                            selectedFareId = -1; // Reset selection ID
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the fare: {ex.InnerException?.Message ?? ex.Message}",
                                     "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedFareId == -1)
            {
                MessageBox.Show("Please select a Fare record from the table to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1. Input Validation and Parsing
            decimal fareAmount;
            if (!decimal.TryParse(txtAmount.Text, out fareAmount) || fareAmount <= 0)
            {
                MessageBox.Show("Please enter a valid positive numerical value for the Fare Amount.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var selectedDepStation = comboDepartureStation.SelectedItem as ScheduleComboItem;
            var selectedArrStation = comboArrivalStation.SelectedItem as ScheduleComboItem;
            string selectedClass = comboClass.SelectedItem?.ToString();

            if (selectedDepStation == null || selectedArrStation == null || string.IsNullOrEmpty(selectedClass))
            {
                MessageBox.Show("Please ensure all fields are selected/entered.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    var fareToUpdate = db.Fare.SingleOrDefault(f => f.FareID == selectedFareId);

                    if (fareToUpdate != null)
                    {
                        // 2. Update all fields (including FKs, allowing route change on update)
                        fareToUpdate.DepartureStationID = selectedDepStation.Id;
                        fareToUpdate.ArrivalStationID = selectedArrStation.Id;
                        fareToUpdate.FareAmount = fareAmount;
                        fareToUpdate.ClassOfService = selectedClass; // Assuming this field exists

                        db.SaveChanges();

                        MessageBox.Show("Fare updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadFareDataGrid();
                        ClearInputFields();
                        selectedFareId = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating the fare: {ex.InnerException?.Message ?? ex.Message}",
                                 "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void dataFareGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataFareGridView.Rows[e.RowIndex];

                // 1. Capture the primary key
                if (row.Cells["FareID"].Value != null)
                {
                    selectedFareId = (int)row.Cells["FareID"].Value;
                }

                // 2. Load station IDs (These columns must be present in your Select in LoadFareDataGrid)
                int depId = (int)row.Cells["DepartureStationID"].Value;
                int arrId = (int)row.Cells["ArrivalStationID"].Value;

                // 3. Find and select the correct items in the combo boxes
                comboDepartureStation.SelectedValue = depId;
                comboArrivalStation.SelectedValue = arrId;

                // 4. Load FareAmount (Must use the column name from the DGV source)
                txtAmount.Text = row.Cells["FareAmount"].Value.ToString();

                // 5. Load ClassService (Assuming you add this column to your LoadFareDataGrid select)
                // You will need a column named 'ClassService' or similar in your DGV
                // comboClass.SelectedItem = row.Cells["ClassService"].Value.ToString(); 
            }
        }
        private void ShowDashboard(Form dashboardForm)
        {
            dashboardForm.Show();
            this.Hide();
        }

        private void btnStations_Click(object sender, EventArgs e)
        {
            StationsDashboard stationsDashboard = new StationsDashboard(userLogIn);
            ShowDashboard(stationsDashboard);
        }

        private void btnTrains_Click(object sender, EventArgs e)
        {
            TrainsDashboard trainsDashboard = new TrainsDashboard(userLogIn);
            ShowDashboard(trainsDashboard);
        }

        private void btnSchedules_Click(object sender, EventArgs e)
        {
            SchedulesDashboard schedulesDashboard = new SchedulesDashboard(userLogIn);
            ShowDashboard(schedulesDashboard);
        }

        private void btnAccounts_Click(object sender, EventArgs e)
        {
            UserDashboard userDashboard = new UserDashboard(userLogIn);
            ShowDashboard(userDashboard);
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            AdminDashboard adminDashboard = new AdminDashboard(userLogIn);
            ShowDashboard(adminDashboard);
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to log out?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                LoginForm loginForm = new LoginForm();
                loginForm.Show();
                this.Hide();
            }
        }
    }
}