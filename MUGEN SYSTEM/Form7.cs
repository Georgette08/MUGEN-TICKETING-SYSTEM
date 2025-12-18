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

        private Dictionary<string, string> TrainDefaultLines = new Dictionary<string, string>()
        {
            { "Nozomi", "Tokaido Shinkansen" },
            { "Sakura", "Sanyo Shinkansen" },
            { "Hayabusa", "Tohoku Shinkansen" },
            { "Hayate", "Kyusu Shinkansen" },
            { "Toki", "Hokkaido Shinkansen" }
        };

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
            // Corrected keys to match your station names exactly
            ValidRoutes.Add("Tokaido Shinkansen", new List<string> { "Sanyo Shinkansen", "Tohoku Shinkansen", "Hokkaido Shinkansen" });
            ValidRoutes.Add("Sanyo Shinkansen", new List<string> { "Tokaido Shinkansen", "Kyusu Shinkansen" });
            ValidRoutes.Add("Kyusu Shinkansen", new List<string> { "Sanyo Shinkansen" });
            ValidRoutes.Add("Tohoku Shinkansen", new List<string> { "Tokaido Shinkansen", "Hokkaido Shinkansen" });
            ValidRoutes.Add("Hokkaido Shinkansen", new List<string> { "Tohoku Shinkansen", "Tokaido Shinkansen" });
        }
        private void PopulateComboBoxes()
        {
            availableTrains.Clear();
            AllStationsList.Clear();

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    // 1. Fetch data from DB
                    AllStationsList = db.Stations
                        .Select(s => new ScheduleComboItem { Id = s.StationID, Name = s.StationName })
                        .ToList();

                    availableTrains = db.Trains
                        .Where(t => t.Status == "Active")
                        .Select(t => new ScheduleComboItem { Id = t.TrainID, Name = t.TrainName })
                        .ToList();
                }

                // 2. BIND DEPARTURE (Set Members BEFORE DataSource)
                comboDeparture.DisplayMember = "Name";
                comboDeparture.ValueMember = "Id";
                comboDeparture.DataSource = AllStationsList.ToList();

                // 3. BIND ARRIVAL
                comboArrival.DisplayMember = "Name";
                comboArrival.ValueMember = "Id";
                comboArrival.DataSource = AllStationsList.ToList();

                // 4. BIND TRAINS LAST
                comboTrainID.DisplayMember = "Name";
                comboTrainID.ValueMember = "Id";
                comboTrainID.DataSource = availableTrains;

                // 5. MANUALLY TRIGGER THE CHAIN
                if (availableTrains.Any())
                {
                    comboTrainID.SelectedIndex = 0;
                    // Explicitly call to ensure Departure and Arrival filter immediately on load
                    comboTrainID_SelectedIndexChanged(comboTrainID, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
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
            var selectedTrain = comboTrainID.SelectedItem as ScheduleComboItem;
            var selectedDepStation = comboDeparture.SelectedItem as ScheduleComboItem;
            var selectedArrStation = comboArrival.SelectedItem as ScheduleComboItem;

            DateTime depTime = dtpDepartureTime.Value;
            DateTime arrTime = dtpArrivalTime.Value;

            // 1. BASIC VALIDATION
            if (selectedTrain == null || selectedDepStation == null || selectedArrStation == null)
            {
                MessageBox.Show("Please complete all selections.", "Error");
                return;
            }

            // 2. TIME LOGIC: Arrival must be after Departure
            if (arrTime <= depTime)
            {
                MessageBox.Show("Arrival Time must be later than Departure Time.", "Invalid Time", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    // 3. STATION OCCUPANCY CHECK
                    // Checks if ANOTHER train is already using the Departure Station at this time
                    bool stationBusy = db.Schedule.Any(s =>
                        s.DepartureStationID == selectedDepStation.Id &&
                        s.DepartureTime == depTime &&
                        s.TrainID != selectedTrain.Id); // Allows the same train, blocks others

                    if (stationBusy)
                    {
                        MessageBox.Show("This station is already occupied by another train at this time.",
                                        "Station Conflict", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 4. TRAIN CONFLICT CHECK (Existing Logic)
                    // Prevent the same train from being in two places at once
                    TimeSpan buffer = TimeSpan.FromMinutes(10);
                    DateTime startBuf = depTime.Subtract(buffer);
                    DateTime endBuf = depTime.Add(buffer);

                    bool trainBusy = db.Schedule.Any(s =>
                        s.TrainID == selectedTrain.Id &&
                        s.DepartureTime >= startBuf &&
                        s.DepartureTime <= endBuf);

                    if (trainBusy)
                    {
                        MessageBox.Show($"Train {selectedTrain.Name} is already scheduled near this time.");
                        return;
                    }

                    // 5. SAVE RECORD
                    var newSchedule = new Schedule
                    {
                        TrainID = selectedTrain.Id,
                        DepartureStationID = selectedDepStation.Id,
                        ArrivalStationID = selectedArrStation.Id,
                        DepartureTime = depTime,
                        ArrivalTime = arrTime
                    };

                    db.Schedule.Add(newSchedule);
                    db.SaveChanges();

                    MessageBox.Show("Schedule added successfully!");
                    LoadSchedulesDataGrid();
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
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
     //   private void ShowandManageForm(Form newform)
     //   {
         //   this.Hide();
//
         //   newform.ShowDialog();

         //   this.Show();
      //  }
       // private void btnStations_Click(object sender, EventArgs e)
       // {
       //     StationsDashboard stationsDashboard = new StationsDashboard(userLogIn);
       //     ShowandManageForm(stationsDashboard);
       // }

       // private void btnTrains_Click(object sender, EventArgs e)
    //    {
         //   TrainsDashboard trainsDashboard = new TrainsDashboard(userLogIn);
         //   ShowandManageForm(trainsDashboard);
       // }

       // private void btnFares_Click(object sender, EventArgs e)
       // {
        //    FaresDashboard faresDashboard = new FaresDashboard(userLogIn);
         //   ShowandManageForm(faresDashboard);
       // }

      //  private void btnAccounts_Click(object sender, EventArgs e)
    //    {
          //  UserDashboard userDashboard = new UserDashboard(userLogIn);
          //  ShowandManageForm(userDashboard);
       // }

       // private void btnDashboard_Click(object sender, EventArgs e)
      //  {
          /// <summary>
          ///  AdminDashboard adminDashboard = new AdminDashboard(userLogIn);
          /// </summary>
          /// <param //name="sender"></param>
          /// <param// name="e"></param>
          //  ShowandManageForm(adminDashboard);
       // }

      //  private void btnLogOut_Click(object sender, EventArgs e)
      //  {
         //   var confirmResult = MessageBox.Show(
            // "Are you sure you want to log out?",
            // "Confirm Logout",
            // MessageBoxButtons.YesNo,
            // MessageBoxIcon.Question
         //);
//            if (confirmResult == DialogResult.Yes)
           // {
            //    LoginForm login = new LoginForm();

            //    ShowandManageForm(login);
         //   }
       // }

        private void comboTrainID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(comboTrainID.SelectedItem is ScheduleComboItem selectedTrain)) return;

            // 2. Look up the designated line (e.g., Nozomi -> Tokaido Shinkansen)
            if (TrainDefaultLines.TryGetValue(selectedTrain.Name, out string designatedLine))
            {
                // 3. Find the matching station in your master list
                var targetStation = AllStationsList.FirstOrDefault(s => s.Name == designatedLine);

                if (targetStation != null)
                {
                    // Temporarily detach events to prevent logic loops
                    comboDeparture.SelectedIndexChanged -= comboDeparture_SelectedIndexChanged;

                    // 4. LOCK DEPARTURE: Filter list to show only the 1 valid line
                    var lockedDepList = AllStationsList.Where(s => s.Name == designatedLine).ToList();
                    comboDeparture.DataSource = lockedDepList;
                    comboDeparture.DisplayMember = "Name";
                    comboDeparture.ValueMember = "Id";
                    comboDeparture.Enabled = false; // "Read Only" lock

                    // 5. FILTER ARRIVAL: Display ONLY suggested stations for this line
                    UpdateArrivalStations(designatedLine);

                    // Re-attach event handler
                    comboDeparture.SelectedIndexChanged += comboDeparture_SelectedIndexChanged;
                }
            }
            else
            {
                // Reset if no specific train mapping is found
                comboDeparture.Enabled = true;
                comboDeparture.DataSource = AllStationsList.ToList();
                comboArrival.DataSource = AllStationsList.ToList();
            }
        }
        private void comboDeparture_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboDeparture.SelectedItem is ScheduleComboItem selectedDep)
            {
                UpdateArrivalStations(selectedDep.Name);
            }
        }
        private void UpdateArrivalStations(string departureLineName)
        {
            if (ValidRoutes.TryGetValue(departureLineName, out List<string> suggestedNames))
            {
                var filteredArrivals = AllStationsList
                    .Where(s => suggestedNames.Contains(s.Name))
                    .ToList();

                comboArrival.DataSource = filteredArrivals;
                comboArrival.DisplayMember = "Name";
                comboArrival.ValueMember = "Id";

                if (filteredArrivals.Any())
                {
                    comboArrival.SelectedIndex = 0;
                }
            }
        }

        private void dtpArrivalTime_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
