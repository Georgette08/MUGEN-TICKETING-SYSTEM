using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity; 
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MUGENTICKETSYSTEM;

namespace MUGEN_SYSTEM
{
    public partial class StaffDashboard : Form
    {
        private readonly UserAccount userLogIn;
        private List<ScheduleComboItem> AllStationsList = new List<ScheduleComboItem>();
        public StaffDashboard(UserAccount userlogIn)
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
            comboDeparture.DataSource = null;
            comboArrival.DataSource = null;

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    AllStationsList = db.Stations
                        .Select(s => new ScheduleComboItem { Id = s.StationID, Name = s.StationName })
                        .OrderBy(s => s.Name)
                        .ToList();
                }

                // Bind the data sources
                comboDeparture.DataSource = AllStationsList.ToList();
                comboDeparture.DisplayMember = "Name";
                comboDeparture.ValueMember = "Id";

                comboArrival.DataSource = AllStationsList.ToList();
                comboArrival.DisplayMember = "Name";
                comboArrival.ValueMember = "Id";

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
            var selectedDepStation = comboDeparture.SelectedItem as ScheduleComboItem;
            var selectedArrStation = comboArrival.SelectedItem as ScheduleComboItem;
            DateTime searchDate = monthCalendar.SelectionStart.Date;

            dataSearchGridView.DataSource = null;

            if (selectedDepStation == null || selectedArrStation == null)
            {
                MessageBox.Show("Please select both a Departure and Arrival Station.", "Missing Search Criteria", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    var availableTrips = db.Schedule
                        .Where(s =>
                            s.DepartureStationID == selectedDepStation.Id &&
                            s.ArrivalStationID == selectedArrStation.Id &&
                            System.Data.Entity.DbFunctions.TruncateTime(s.DepartureTime) == searchDate
                        )
                        .Select(s => new
                        {
                            // CRITICAL IDs
                            ScheduleID = s.ScheduleID,
                            DepartureStationID = s.DepartureStationID,
                            ArrivalStationID = s.ArrivalStationID,

                            // Display details (Using the most common Entity Framework Navigation Property names)
                            TrainName = s.Trains.TrainName,           // Corrected Navigation
                            DepartureStation = s.Stations.StationName, // Corrected Navigation
                            ArrivalStation = s.Stations1.StationName,  // Corrected Navigation
                            DepartureTime = s.DepartureTime,
                            ArrivalTime = s.ArrivalTime
                        })
                        .OrderBy(t => t.DepartureTime)
                        .ToList();

                    dataSearchGridView.DataSource = availableTrips;

                    // Hide ID columns after binding
                    if (dataSearchGridView.Columns["ScheduleID"] != null)
                        dataSearchGridView.Columns["ScheduleID"].Visible = false;
                    if (dataSearchGridView.Columns["DepartureStationID"] != null)
                        dataSearchGridView.Columns["DepartureStationID"].Visible = false;
                    if (dataSearchGridView.Columns["ArrivalStationID"] != null)
                        dataSearchGridView.Columns["ArrivalStationID"].Visible = false;

                    if (!availableTrips.Any())
                    {
                        MessageBox.Show($"No scheduled trains found...", "No Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during the search: {ex.Message}.", "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (dataSearchGridView.SelectedRows.Count != 1)
            {
                MessageBox.Show("Please select a single available train trip from the list to confirm.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dataSearchGridView.SelectedRows[0];

            try
            {
                // Safe Data Retrieval using Convert for robustness
                int scheduleId = Convert.ToInt32(selectedRow.Cells["ScheduleID"].Value);
                int depStationId = Convert.ToInt32(selectedRow.Cells["DepartureStationID"].Value);
                int arrStationId = Convert.ToInt32(selectedRow.Cells["ArrivalStationID"].Value);

                string trainName = selectedRow.Cells["TrainName"].Value?.ToString() ?? "N/A";
                string departureStationName = selectedRow.Cells["DepartureStation"].Value?.ToString() ?? "N/A";
                string arrivalStationName = selectedRow.Cells["ArrivalStation"].Value?.ToString() ?? "N/A";

                DateTime departureTime = Convert.ToDateTime(selectedRow.Cells["DepartureTime"].Value);

                int currentAgentId = SessionManager.CurrentAgentID;

                // Instantiate the PassengerForm with 8 Arguments
                PassengerForm passengerForm = new PassengerForm( userLogIn,
                    scheduleId, depStationId, arrStationId, trainName,
                    departureStationName, arrivalStationName, departureTime, currentAgentId
                );

                passengerForm.ShowDialog();

                // Reload the grid after the Passenger Form is closed/booking is done
                btnSearch_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error confirming selection and launching form. Detail: {ex.Message}", "Form Launch Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ... (Sh
        private void ShowandManageForm(Form newform)
        {
            this.Hide();

            newform.ShowDialog();

            this.Show();
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

        private void dataSearchGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
       
        private void btnCustomer_Click(object sender, EventArgs e)
        {
            CustomersDashboard customerDashboard = new CustomersDashboard(userLogIn);
            ShowandManageForm(customerDashboard);
        }
    }
    public class ScheduleComboItem
    {
        public int Id { get; set; } // The Station ID
        public string Name { get; set; } // The Station Name
        public override string ToString() => Name;
    }
    public class FareComboItem
    {
        public int FareId { get; set; }
        public string ClassName { get; set; }
        public decimal FareAmount { get; set; }
        public override string ToString() => ClassName; // Display the ClassName in the ComboBox

    }
    public static class SessionManager
    {
        public static int CurrentAgentID { get; set; }
    }
}