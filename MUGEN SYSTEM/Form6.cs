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
    public partial class TrainsDashboard : Form
    {
        private readonly UserAccount userLogIn;
        private int selectedTrainID = -1;

        private List<String> Series = new List<String>();
        private List<String> Length = new List<String>();
        private List<int> Capacity = new List<int>();
        public TrainsDashboard(UserAccount userLogin)
        {
            InitializeComponent();
            this.userLogIn = userLogin;
        }

        private void TrainsDashboard_Load(object sender, EventArgs e)
        {
            LoadTrainsDataGrid();
            comboTrainName.Items.Clear();
            comboStatus.Items.Clear();

            Series.Clear();
            Length.Clear();
            Capacity.Clear();

            comboTrainName.Items.Add("Nozomi");
            comboTrainName.Items.Add("Mizuho");
            comboTrainName.Items.Add("Sakura");
            comboTrainName.Items.Add("Hayabusa");
            comboTrainName.Items.Add("Hayate");
            comboTrainName.Items.Add("Toki");

            comboStatus.Items.Add("Active");
            comboStatus.Items.Add("Inactive");

            Series.Add("N700A Series");
            Series.Add("N700 Series");
            Series.Add("N700 Series");
            Series.Add("E5 Series");
            Series.Add("E5 Series");
            Series.Add("E3 Series");

            Length.Add("16 Cars");
            Length.Add("8 Cars");
            Length.Add("8 Cars");
            Length.Add("10 Cars");
            Length.Add("10 Cars");
            Length.Add("7 Cars");

            Capacity.Add(1324);
            Capacity.Add(639);
            Capacity.Add(639);
            Capacity.Add(731);
            Capacity.Add(731);
            Capacity.Add(402);

            if (comboTrainName.Items.Count > 0)
            {
                comboTrainName.SelectedIndex = 0;
            }
            if (comboStatus.Items.Count > 0)
            {
                comboStatus.SelectedIndex = 0;
            }
            UpdateDependentFields();
        }
        private void LoadTrainsDataGrid()
        {
            using (var db = new MugenSystemDBEntities())
            {
                var trainsList = db.Trains.Select(t => new
                {
                    t.TrainID,
                    t.TrainName,
                    t.Capacity,
                    t.Status,
                    t.Series,
                    t.TrainLength
                }).ToList();
                dataTrainsGridView.DataSource = trainsList;
               // dataTrainsGridView.ClearSelection();
            }
        }
        private void UpdateDependentFields()
        {
            int selectedIndex = comboTrainName.SelectedIndex;
            if (selectedIndex >= 0)
            {
                if (selectedIndex < Series.Count)
                    txtSeries.Text = Series[selectedIndex];
                if (selectedIndex < Length.Count)
                    txtLength.Text = Length[selectedIndex]; // ✅ FIX: Ensure txtLength is used
                if (selectedIndex < Capacity.Count)
                    txtCapacity.Text = Capacity[selectedIndex].ToString();
            }
            else
            {
                txtSeries.Clear();
                txtLength.Clear();
                txtCapacity.Clear();
            }
        }
        private void ClearInputFields()
        {
            selectedTrainID = -1;
            dataTrainsGridView.ClearSelection();

            // Clear TextBoxes
            txtSeries.Clear();
            txtLength.Clear();
            txtCapacity.Clear();

            // Reset ComboBoxes
            if (comboTrainName.Items.Count > 0)
                comboTrainName.SelectedIndex = 0;
            if (comboStatus.Items.Count > 0)
                comboStatus.SelectedIndex = 0;

            // Update dependent fields to show the default selection
            UpdateDependentFields();
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
          
            string trainName = comboTrainName.SelectedItem?.ToString();
            string status = comboStatus.SelectedItem?.ToString();
            string series = txtSeries.Text;
            string length = txtLength.Text; // Use value from read-only TextBox
            string capacityText = txtCapacity.Text;

            if (!int.TryParse(capacityText, out int CapacityValue))
            {
                MessageBox.Show("Capacity must be a valid whole number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(trainName) || string.IsNullOrEmpty(status) ||
                string.IsNullOrEmpty(series) || string.IsNullOrEmpty(length))
            {
                MessageBox.Show("Please ensure all fields are selected/filled.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            try
            {
                using (var dbTrains = new MugenSystemDBEntities())
                {
                    if (dbTrains.Trains.Any(t => t.TrainName == trainName))
                    {
                        MessageBox.Show("A train with this name already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    Trains newTrain = new Trains
                    {
                        TrainName = trainName,
                        Capacity = CapacityValue,
                        Status = status,
                        Series = series,
                        TrainLength = length // ✅ FIX: Map to the correct DB property
                    };
                    dbTrains.Trains.Add(newTrain);
                    dbTrains.SaveChanges();
                    MessageBox.Show("New train added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the train: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            LoadTrainsDataGrid();
            ClearInputFields();
        }

        private void dataTrainsGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dataTrainsGridView.Rows[e.RowIndex];

            // Set the tracking ID
            if (row.Cells["TrainID"].Value != null && int.TryParse(row.Cells["TrainID"].Value.ToString(), out int trainId))
            {
                selectedTrainID = trainId;
            }
            else
            {
                selectedTrainID = -1;
            }

            // Retrieve data from row (using explicit column names used in the anonymous type)
            string name = row.Cells["TrainName"].Value?.ToString();
            string series = row.Cells["Series"].Value?.ToString();
            string length = row.Cells["TrainLength"].Value?.ToString(); // Using the alias/correct name
            string capacity = row.Cells["Capacity"].Value?.ToString();
            string status = row.Cells["Status"].Value?.ToString();

            // Set primary ComboBox
            if (!string.IsNullOrEmpty(name))
            {
                comboTrainName.SelectedItem = name;
            }

            // Set read-only fields directly
            txtSeries.Text = series;
            txtLength.Text = length;
            txtCapacity.Text = capacity;

            // Set Status ComboBox
            if (!string.IsNullOrEmpty(status))
            {
                comboStatus.SelectedItem = status;
            }

            // Ensure Train Name is set to avoid null reference errors
            if (comboTrainName.SelectedIndex < 0 && comboTrainName.Items.Count > 0)
            {
                comboTrainName.SelectedIndex = 0;
            }
        }

        // Ensure this event is linked to the ComboBox SelectedIndexChanged event
        private void comboTrainName_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDependentFields();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedTrainID == -1)
            {
                MessageBox.Show("Please select a train from the grid to delete.", "Select Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure you want to delete this train?",
                                       "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    using (var dbTrains = new MugenSystemDBEntities())
                    {
                        var trainToDelete = dbTrains.Trains.Find(selectedTrainID);

                        if (trainToDelete != null)
                        {
                            dbTrains.Trains.Remove(trainToDelete);
                            dbTrains.SaveChanges();
                            MessageBox.Show("Train deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during deletion: {ex.InnerException?.Message ?? ex.Message}",
                                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                LoadTrainsDataGrid();
                ClearInputFields();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedTrainID == -1)
            {
                MessageBox.Show("Please select a train from the grid to update.", "Select Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string trainName = comboTrainName.SelectedItem?.ToString();
            string status = comboStatus.SelectedItem?.ToString();
            string capacityText = txtCapacity.Text;
            string series = txtSeries.Text;
            string length = txtLength.Text;

            if (!int.TryParse(capacityText, out int CapacityValue))
            {
                MessageBox.Show("Capacity must be a valid whole number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(trainName) || string.IsNullOrEmpty(status))
            {
                MessageBox.Show("Train Name and Status are required.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var dbTrains = new MugenSystemDBEntities())
                {
                    var trainToUpdate = dbTrains.Trains.Find(selectedTrainID);

                    if (trainToUpdate != null)
                    {
                        trainToUpdate.TrainName = trainName;
                        trainToUpdate.Status = status;
                        trainToUpdate.Series = series;
                        trainToUpdate.TrainLength = length; // ✅ FIX: Map to the correct DB property
                        trainToUpdate.Capacity = CapacityValue;

                        dbTrains.SaveChanges();
                        MessageBox.Show("Train updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during update: {ex.InnerException?.Message ?? ex.Message}",
                                "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            LoadTrainsDataGrid();
            ClearInputFields();
        }
        private void ShowandManageForm(Form newform)
        {
            this.Hide();

            newform.ShowDialog();

            this.Show();
        }

        private void btnStations_Click(object sender, EventArgs e)
        {
            StationsDashboard stations = new StationsDashboard(userLogIn);
            ShowandManageForm(stations);
        }

        private void btnSchecules_Click(object sender, EventArgs e)
        {
            SchedulesDashboard schedules = new SchedulesDashboard(userLogIn);  
            ShowandManageForm(schedules);
        }

        private void btnFares_Click(object sender, EventArgs e)
        {
            FaresDashboard fares = new FaresDashboard(userLogIn);
            ShowandManageForm(fares);
        }

        private void btnUserAccount_Click(object sender, EventArgs e)
        {
            UserDashboard users = new UserDashboard(userLogIn);
            ShowandManageForm(users);
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            AdminDashboard admin = new AdminDashboard(userLogIn);  
            ShowandManageForm(admin);
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
    }
}
