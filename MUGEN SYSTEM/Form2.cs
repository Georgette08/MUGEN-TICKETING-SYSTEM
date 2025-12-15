using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Entity; 

namespace MUGEN_SYSTEM
{
    public partial class StaffDashboard : Form
    {
        private UserAccount userLogIn;
        private List<ScheduleComboItem> AllStationsList = new List<ScheduleComboItem>();
        public StaffDashboard(UserAccount userLogIn)
        {
            InitializeComponent();
            this.userLogIn = userLogIn;
        }
        private void StaffDashboard_Load(object sender, EventArgs e)
        {
            monthCalendar.SelectionStart = DateTime.Today;
            PopulateStaffFormComboBoxes();
        }
        private void PopulateStaffFormComboBoxes()
        {
            AllStationsList.Clear();

            // Clear existing data sources before binding
            comboDeparture.DataSource = null;
            comboArrival.DataSource = null;

            try
            {
                using (var db = new MugenSystemDBEntities()) // Replace with your actual DB Context class name if needed
                {
                    // Load ALL stations from the DB
                    AllStationsList = db.Stations
                        .Select(s => new ScheduleComboItem { Id = s.StationID, Name = s.StationName })
                        .OrderBy(s => s.Name) // Order alphabetically
                        .ToList();
                }

                // Bind the Master List to both ComboBoxes
                comboDeparture.DataSource = AllStationsList.ToList();
                comboDeparture.DisplayMember = "Name";
                comboDeparture.ValueMember = "Id";

                comboArrival.DataSource = AllStationsList.ToList();
                comboArrival.DisplayMember = "Name";
                comboArrival.ValueMember = "Id";

                // Start with no selection
                comboDeparture.SelectedIndex = -1;
                comboArrival.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading station data: {ex.Message}", "Database Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            // Retrieve the selected Station Combo Items
            var selectedDepStation = comboDeparture.SelectedItem as ScheduleComboItem;
            var selectedArrStation = comboArrival.SelectedItem as ScheduleComboItem;

            // Get the selected travel date from the calendar control
            DateTime searchDate = monthCalendar.SelectionStart.Date;

            // 1. Initial Validation
            if (selectedDepStation == null || selectedArrStation == null)
            {
                MessageBox.Show("Please select both a Departure and Arrival Station.", "Missing Search Criteria", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dataSearchGridView.DataSource = null; // Assuming your DataGrid is named dgvAvailableTrains
                return;
            }

            // Check for same station
            if (selectedDepStation.Id == selectedArrStation.Id)
            {
                MessageBox.Show("Departure and Arrival Stations cannot be the same.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Perform the Database Search by Route and Date
            try
            {
                using (var db = new MugenSystemDBEntities()) // Replace with your actual DB Context class name if needed
                {
                    // Query the Schedule table: Filter by Departure ID, Arrival ID, and the date part of the DepartureTime.
                    var availableTrips = db.Schedule
                        .Where(s =>
                            s.DepartureStationID == selectedDepStation.Id &&
                            s.ArrivalStationID == selectedArrStation.Id &&

                            // FIX: Use DbFunctions.TruncateTime to correctly compare only the DATE part
                            System.Data.Entity.DbFunctions.TruncateTime(s.DepartureTime) == searchDate
                        )
                        .Select(s => new
                        {
                            // Select the necessary details to display
                            TrainName = s.Trains.TrainName, // Gets Train Name via navigation property
                            DepartureStation = s.Stations.StationName,
                            ArrivalStation = s.Stations1.StationName,
                            DepartureTime = s.DepartureTime,
                            ArrivalTime = s.ArrivalTime
                        })
                        .OrderBy(t => t.DepartureTime) // Order results chronologically
                        .ToList();

                    // 3. Bind results to the DataGrid
                    dataSearchGridView.DataSource = availableTrips;

                    if (!availableTrips.Any())
                    {
                        MessageBox.Show($"No scheduled trains found from {selectedDepStation.Name} to {selectedArrStation.Name} on {searchDate.ToShortDateString()}.",
                                        "No Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during the search: {ex.Message}", "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            // 1. Check if a row is actually selected in the DataGrid
            if (dataSearchGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a single available train trip from the list to confirm.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get the selected row data
            DataGridViewRow selectedRow = dataSearchGridView.SelectedRows[0];

            // We retrieve the values using the column names defined in the btnSearch_Click SELECT statement.
            try
            {
                string trainName = selectedRow.Cells["TrainName"].Value.ToString();
                string departureStationName = selectedRow.Cells["DepartureStation"].Value.ToString();
                string arrivalStationName = selectedRow.Cells["ArrivalStation"].Value.ToString();

                // It's best to pass the full DateTime objects if you need them for booking logic
                DateTime departureTime = (DateTime)selectedRow.Cells["DepartureTime"].Value;
                DateTime arrivalTime = (DateTime)selectedRow.Cells["ArrivalTime"].Value;

                // --- 3. Instantiate and Show the Passenger Form ---

                // You MUST define the constructor in your PassengerForm to accept these arguments.
                PassengerForm passengerForm = new PassengerForm(
                    departureStationName,
                    arrivalStationName,
                    departureTime,
                    arrivalTime,
                    trainName
                );

                // If you want the Passenger Form to replace the current form:
                // passengerForm.Show();
                // this.Hide(); 

                // If you want the Passenger Form to show modally:
                passengerForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error confirming selection: {ex.Message}. Ensure all required columns are visible in the grid.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    public class ScheduleComboItem
    {
        public int Id { get; set; } // The Station ID
        public string Name { get; set; } // The Station Name
        public override string ToString() => Name;
    }
}