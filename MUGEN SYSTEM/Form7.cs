using MUGENTICKETSYSTEM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MUGEN_SYSTEM
{
    public partial class SchedulesDashboard : Form
    {
        private readonly UserAccount userLogIn;
        private int selectedScheduleID = -1;

        // Maps Departure Station NAME (string) to a list of valid Arrival Station NAMES (string)
        private Dictionary<string, List<string>> ValidRoutes = new Dictionary<string, List<string>>();

        private List<ScheduleComboItem> AllStationsList = new List<ScheduleComboItem>();
        private List<ScheduleComboItem> availableTrains = new List<ScheduleComboItem>();

        public SchedulesDashboard(UserAccount userLogin)
        {
            InitializeComponent();
            this.userLogIn = userLogin;
        }
        private void SchedulesDashboard_Load(object sender, EventArgs e)
        {
            DefineValidRoutes();
            LoadSchedulesDataGrid();
            PopulateComboBoxes();
        }
        public class ScheduleComboItem
        {
            public int Id { get; set; } // The ID that saves to the database (FK)
            public string Name { get; set; } // The Name displayed in the combo box
            public override string ToString() => Name;
        }
        private void LoadSchedulesDataGrid()
        {
            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    var schedules = db.Schedule
                        .Select(s => new
                        {
                            s.ScheduleID,
                            TrainName = s.Trains.TrainName,
                            DepartureStation = s.Stations.StationName,
                            ArrivalStation = s.Stations1.StationName,
                            s.DepartureTime,
                            s.ArrivalTime,

                            s.TrainID,
                            s.DepartureStationID,
                            s.ArrivalStationID
                        })
                        .ToList();

                    dataScheduleGridView.DataSource = schedules;
                    dataScheduleGridView.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading schedules data: {ex.Message}", "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DefineValidRoutes()
        {
            ValidRoutes.Clear();

            // *** FIX: Map Shinkansen Line Names to Valid Arrival Line Names ***

            // 1. Tokiado Shinkansen (e.g., Tokyo area) can connect to:
            ValidRoutes.Add("Tokaido Shinkansen", new List<string> {"Sanyo Shinkansen", "Tohoku Shinkansen","Hokkaido Shinkansen"});

            // 2. Sanyo Shinkansen (e.g., Osaka/Kyoto area) can connect to:
            ValidRoutes.Add("Sanyo Shinkansen", new List<string> {"Tokaido Shinkansen","Kyusu Shinkansen"});

            // 3. Kyusu Shinkansen (e.g., Fukuoka area) can connect to:
            ValidRoutes.Add("Kyusu Shinkansen", new List<string> {"Sanyo Shinkansen"});

            // 4. Tohoku Shinkansen can connect to:
            ValidRoutes.Add("Tohoku Shinkansen", new List<string> {"Tokaido Shinkansen","Hokkaido Shinkansen"});

                    // 5. Hokkaido Shinkansen can connect to:
            ValidRoutes.Add("Hokkaido Shinkansen", new List<string> {"Tohoku Shinkansen","Tokaido Shinkansen"});

            // Add any other specific pairings based on your system's logic
        }
        private void PopulateComboBoxes()
        {
            // 1. CLEAR DATA LISTS AND CONTROLS
            availableTrains.Clear();
            AllStationsList.Clear(); // Clear the list before reloading

            comboTrainID.Items.Clear();
            comboDeparture.Items.Clear();
            comboArrival.Items.Clear();

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

                // 3. BIND TRAINS
                comboTrainID.DataSource = availableTrains;
                comboTrainID.DisplayMember = "Name";
                comboTrainID.ValueMember = "Id";

                // 4. BIND STATIONS (Using the master list loaded from the DB)
                comboDeparture.DataSource = AllStationsList.ToList();
                comboDeparture.DisplayMember = "Name";
                comboDeparture.ValueMember = "Id";

                comboArrival.DataSource = AllStationsList.ToList();
                comboArrival.DisplayMember = "Name";
                comboArrival.ValueMember = "Id";

                // 5. SET DEFAULT SELECTION
                if (availableTrains.Any())
                    comboTrainID.SelectedIndex = 0;
                if (AllStationsList.Any())
                {
                    comboDeparture.SelectedIndex = 0;
                    comboArrival.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ClearInputFields()
        {
            selectedScheduleID = -1;
            TextBox txtScheduleID = this.Controls.Find("txtScheduleID", true).FirstOrDefault() as TextBox;
            if (txtScheduleID != null) txtScheduleID.Clear();

            if (comboTrainID.Items.Count > 0) comboTrainID.SelectedIndex = 0;
            if (comboDeparture.Items.Count > 0) comboDeparture.SelectedIndex = 0;
            if (comboArrival.Items.Count > 0) comboArrival.SelectedIndex = 0;

            dtpDepartureTime.Value = DateTime.Now;
            dtpArrivalTime.Value = DateTime.Now.AddHours(1);

            dataScheduleGridView.ClearSelection();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // 1. Retrieve the selected ScheduleComboItem objects
            var selectedTrain = comboTrainID.SelectedItem as ScheduleComboItem;
            var selectedDepStation = comboDeparture.SelectedItem as ScheduleComboItem;
            var selectedArrStation = comboArrival.SelectedItem as ScheduleComboItem;

            // Define the required buffer time (e.g., 10 minutes)
            TimeSpan requiredBuffer = TimeSpan.FromMinutes(10);
            DateTime newDepartureTime = dtpDepartureTime.Value;

            // --- 2. INPUT VALIDATION ---

            // Check for missing data
            if (selectedTrain == null || selectedDepStation == null || selectedArrStation == null)
            {
                MessageBox.Show("Please select a Train and both Station names.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check for same station selection
            if (selectedDepStation.Id == selectedArrStation.Id)
            {
                MessageBox.Show("Departure and Arrival Stations cannot be the same.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- 3. LINQ FIX: Calculate Boundaries in C# (Client-Side) ---
            // Calculate the time boundaries BEFORE querying the database to avoid the 'NotSupportedException'.
            DateTime startTimeWindow = newDepartureTime.Subtract(requiredBuffer);
            DateTime endTimeWindow = newDepartureTime.Add(requiredBuffer);

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    // --- 4. TIME CONFLICT CHECK ---

                    // This query checks if any existing schedule for the SAME TRAINID 
                    // has a departure time within the calculated time window (10 minutes before to 10 minutes after).
                    bool timeConflict = db.Schedule.Any(s =>
                        s.TrainID == selectedTrain.Id &&
                        s.DepartureTime >= startTimeWindow &&
                        s.DepartureTime <= endTimeWindow
                    );

                    if (timeConflict)
                    {
                        MessageBox.Show($"A schedule conflict exists. Train '{selectedTrain.Name}' is already booked to depart within {requiredBuffer.TotalMinutes} minutes of the time you selected.",
                                        "Time Conflict Detected",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                        return; // Stop the function and do not save
                    }
                    // -----------------------------------------------------------

                    // 5. DATABASE SAVE (If no conflicts are found)
                    var newSchedule = new Schedule
                    {
                        TrainID = selectedTrain.Id,
                        DepartureStationID = selectedDepStation.Id,
                        ArrivalStationID = selectedArrStation.Id,
                        DepartureTime = newDepartureTime,
                        ArrivalTime = dtpArrivalTime.Value
                    };

                    db.Schedule.Add(newSchedule);
                    db.SaveChanges();

                    MessageBox.Show("Schedule added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 6. REFRESH DATA GRID AND INPUTS
                    LoadSchedulesDataGrid(); // Ensures the new data appears in the grid immediately
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected system errors (e.g., database connection issues)
                MessageBox.Show($"An error occurred: {ex.InnerException?.Message ?? ex.Message}",
                                 "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedScheduleID == -1)
            {
                MessageBox.Show("Please select a schedule from the grid to delete.", "Select Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure you want to delete this schedule?",
                                               "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    using (var db = new MugenSystemDBEntities())
                    {
                        var scheduleToDelete = db.Schedule.Find(selectedScheduleID);
                        if (scheduleToDelete != null)
                        {
                            db.Schedule.Remove(scheduleToDelete);
                            db.SaveChanges();
                            MessageBox.Show("Schedule deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadSchedulesDataGrid();
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
            if (selectedScheduleID == -1)
            {
                MessageBox.Show("Please select a schedule from the grid to update.", "Select Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedTrain = comboTrainID.SelectedItem as ScheduleComboItem;
            var selectedDepStation = comboDeparture.SelectedItem as ScheduleComboItem;
            var selectedArrStation = comboArrival.SelectedItem as ScheduleComboItem;

            if (selectedTrain == null || selectedDepStation == null || selectedArrStation == null)
            {
                MessageBox.Show("Please select a Train and both Station names.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (selectedDepStation.Id == selectedArrStation.Id)
            {
                MessageBox.Show("Departure and Arrival Stations cannot be the same.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    var scheduleToUpdate = db.Schedule.Find(selectedScheduleID);
                    if (scheduleToUpdate != null)
                    {
                        scheduleToUpdate.TrainID = selectedTrain.Id;
                        scheduleToUpdate.DepartureStationID = selectedDepStation.Id;
                        scheduleToUpdate.ArrivalStationID = selectedArrStation.Id;
                        scheduleToUpdate.DepartureTime = dtpDepartureTime.Value;
                        scheduleToUpdate.ArrivalTime = dtpArrivalTime.Value;

                        db.SaveChanges();
                        MessageBox.Show("Schedule updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadSchedulesDataGrid();
                        ClearInputFields();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during update: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataScheduleGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dataScheduleGridView.Rows[e.RowIndex];

            // 1. Set tracking ID
            if (row.Cells["ScheduleID"].Value != null && int.TryParse(row.Cells["ScheduleID"].Value.ToString(), out int scheduleId))
            {
                selectedScheduleID = scheduleId;
            }
            else
            {
                selectedScheduleID = -1;
                return;
            }

            // 2. Load primary keys (FKs) from the hidden columns in the grid
            int trainId = Convert.ToInt32(row.Cells["TrainID"].Value ?? 0);
            int depId = Convert.ToInt32(row.Cells["DepartureStationID"].Value ?? 0);
            int arrId = Convert.ToInt32(row.Cells["ArrivalStationID"].Value ?? 0);

            // 3. Select the ComboBox items using the FK IDs
            comboTrainID.SelectedValue = trainId;
            comboDeparture.SelectedValue = depId;
            comboArrival.SelectedValue = arrId;

            // 4. Load Times
            if (row.Cells["DepartureTime"] != null && row.Cells["DepartureTime"].Value is DateTime depTime)
            {
                dtpDepartureTime.Value = depTime;
            }
            if (row.Cells["ArrivalTime"] != null && row.Cells["ArrivalTime"].Value is DateTime arrTime)
            {
                dtpArrivalTime.Value = arrTime;
            }

            // Display the auto-generated ScheduleID (Assuming you have txtScheduleID control)
            TextBox txtScheduleID = this.Controls.Find("txtScheduleID", true).FirstOrDefault() as TextBox;
            if (txtScheduleID != null) txtScheduleID.Text = selectedScheduleID.ToString();
        }
        private void ShowandManageForm(Form newform)
        {
            this.Hide();

            newform.ShowDialog();

            this.Show();
        }
        private void btnStations_Click(object sender, EventArgs e)
        {
            StationsDashboard stationsDashboard = new StationsDashboard(userLogIn);
            ShowandManageForm(stationsDashboard);
        }

        private void btnTrains_Click(object sender, EventArgs e)
        {
            TrainsDashboard trainsDashboard = new TrainsDashboard(userLogIn);
            ShowandManageForm(trainsDashboard);
        }

        private void btnFares_Click(object sender, EventArgs e)
        {
            FaresDashboard faresDashboard = new FaresDashboard(userLogIn);
            ShowandManageForm(faresDashboard);
        }

        private void btnAccounts_Click(object sender, EventArgs e)
        {
            UserDashboard userDashboard = new UserDashboard(userLogIn);
            ShowandManageForm(userDashboard);
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            AdminDashboard adminDashboard = new AdminDashboard(userLogIn);
            ShowandManageForm(adminDashboard);
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

        private void comboTrainID_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboDeparture_SelectedIndexChanged(object sender, EventArgs e)
        {  // Temporarily remove the handler to avoid recursive binding/filtering issues
            comboArrival.SelectedIndexChanged -= comboDeparture_SelectedIndexChanged;

            // The selected item contains the StationID (Id) and the Line Name (Name)
            var selectedDeparture = comboDeparture.SelectedItem as ScheduleComboItem;

            // Check for null selection
            if (selectedDeparture == null || string.IsNullOrEmpty(selectedDeparture.Name))
            {
                comboArrival.DataSource = null;
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
                comboArrival.DataSource = filteredArrivalStations;
                comboArrival.DisplayMember = "Name";
                comboArrival.ValueMember = "Id";

                // 4. Reset selection to the first item
                if (filteredArrivalStations.Any())
                    comboArrival.SelectedIndex = 0;
                else
                    comboArrival.DataSource = null; // Clear if no valid routes found
            }
            else
            {
                // If the selected Departure Station isn't defined in the map, clear the Arrival box
                comboArrival.DataSource = null;
                comboArrival.Items.Clear();
            }

            // Re-add the handler
            comboArrival.SelectedIndexChanged += comboDeparture_SelectedIndexChanged;
        }
    }
}
