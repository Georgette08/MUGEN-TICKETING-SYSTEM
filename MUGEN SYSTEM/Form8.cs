using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using static MUGEN_SYSTEM.SchedulesDashboard;
using MUGENTICKETSYSTEM;

namespace MUGEN_SYSTEM
{
    public partial class FaresDashboard : Form
    {
        private readonly UserAccount userLogIn;
        private int? passedTrainID;
        private int selectedFareId = -1;

        private Dictionary<string, List<string>> ValidRoutes = new Dictionary<string, List<string>>();
        private List<ScheduleComboItem> AllStationsList = new List<ScheduleComboItem>();



        public FaresDashboard(UserAccount userLogIn, int? trainID = null)
        {
            InitializeComponent();
            this.userLogIn = userLogIn;
            this.passedTrainID = trainID;
        }
        private void FaresDashboard_Load(object sender, EventArgs e)
        {
            DefineValidRoutes();
            PopulateComboBoxes();
            LoadServiceClasses();
            LoadFareDataGrid();
        }
        private void UpdateFareAmountDisplay()
        {
            var dep = comboDepartureStation.SelectedItem as ScheduleComboItem;
            var arr = comboArrivalStation.SelectedItem as ScheduleComboItem;
            string selectedClass = comboClass.SelectedItem?.ToString();

            if (dep == null || arr == null || string.IsNullOrEmpty(selectedClass)) return;

            double baseFare = 0;
            string routeKey = $"{dep.Name}-{arr.Name}";

            switch (routeKey)
            {
                case "Tokaido Shinkansen-Sanyo Shinkansen": baseFare = 5450; break;
                case "Tokaido Shinkansen-Tohoku Shinkansen": baseFare = 4150; break;
                case "Tokaido Shinkansen-Hokkaido Shinkansen": baseFare = 9200; break;
                case "Sanyo Shinkansen-Tokaido Shinkansen": baseFare = 5450; break;
                case "Sanyo Shinkansen-Kyusu Shinkansen": baseFare = 5250; break;
                case "Kyusu Shinkansen-Sanyo Shinkansen": baseFare = 5250; break;
                case "Tohoku Shinkansen-Tokaido Shinkansen": baseFare = 4150; break;
                case "Tohoku Shinkansen-Hokkaido Shinkansen": baseFare = 8850; break;
                case "Hokkaido Shinkansen-Tohoku Shinkansen": baseFare = 8850; break;
                case "Hokkaido Shinkansen-Tokaido Shinkansen": baseFare = 9200; break;
                default: baseFare = 0; break;
            }

            double multiplier = 1.0;
            if (selectedClass == "Green Car") multiplier = 1.5;
            else if (selectedClass == "Gran Class") multiplier = 2.0;

            double finalFare = baseFare * multiplier;
            txtAmount.Text = finalFare > 0 ? finalFare.ToString("N2") : "0.00";
        }
        private void LoadFareDataGrid()
        {
            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                  
                    var fares = db.Fare
                        .Select(f => new
                        {
                           
                            f.FareID,

                            DepartureStation = f.Stations.StationName, 
                            ArrivalStation = f.Stations1.StationName,  
                            f.ClassOfService, 
                            f.FareAmount,

                           
                            f.DepartureStationID,
                            f.ArrivalStationID
                        })
                        .ToList();

                    dataFareGridView.DataSource = fares;
                    dataFareGridView.ClearSelection();

                    
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
            public int Id { get; set; } 
            public string Name { get; set; } 
            public override string ToString() => Name;
        }

        private void DefineValidRoutes()
        {
            ValidRoutes.Clear();
            ValidRoutes.Add("Tokaido Shinkansen", new List<string> { "Sanyo Shinkansen", "Tohoku Shinkansen", "Hokkaido Shinkansen" });
            ValidRoutes.Add("Sanyo Shinkansen", new List<string> { "Tokaido Shinkansen", "Kyusu Shinkansen" });
            ValidRoutes.Add("Kyusu Shinkansen", new List<string> { "Sanyo Shinkansen" });
            ValidRoutes.Add("Tohoku Shinkansen", new List<string> { "Tokaido Shinkansen", "Hokkaido Shinkansen" });
            ValidRoutes.Add("Hokkaido Shinkansen", new List<string> { "Tohoku Shinkansen", "Tokaido Shinkansen" });
        }
        private void PopulateComboBoxes()
        {
            AllStationsList.Clear();
            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                 
                    var activeSchedules = db.Schedule.Include("Stations").Include("Stations1").ToList();

                    if (!activeSchedules.Any()) return;

                    var departureStations = activeSchedules
                        .Where(s => s.DepartureStationID.HasValue)
                        .Select(s => new ScheduleComboItem
                        {
                            Id = (int)s.DepartureStationID, 
                            Name = s.Stations.StationName
                        });

                
                    var arrivalStations = activeSchedules
                        .Where(s => s.ArrivalStationID.HasValue)
                        .Select(s => new ScheduleComboItem
                        {
                            Id = (int)s.ArrivalStationID, 
                            Name = s.Stations1.StationName
                        });

                   
                    AllStationsList = departureStations.Union(arrivalStations)
                        .GroupBy(x => x.Id)
                        .Select(g => g.First())
                        .ToList();
                }

                comboDepartureStation.DataSource = AllStationsList.ToList();
                comboDepartureStation.DisplayMember = "Name";
                comboDepartureStation.ValueMember = "Id";

                comboArrivalStation.DataSource = AllStationsList.ToList();
                comboArrivalStation.DisplayMember = "Name";
                comboArrivalStation.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filter Error: {ex.Message}");
            }
        }
        private void LoadServiceClasses()
        {
            List<string> serviceClasses = new List<string> { "Ordinary Car", "Green Car", "Gran Class" };
            comboClass.DataSource = serviceClasses;
            comboClass.SelectedIndex = 0;
        }
        private void ClearInputFields()
        {
            comboDepartureStation.SelectedIndex = -1;
            comboArrivalStation.SelectedIndex = -1;
            comboClass.SelectedIndex = -1;

        
            txtAmount.Text = string.Empty;

            selectedFareId = -1;
          
            dataFareGridView.ClearSelection();
        }
        private void comboDepartureStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboArrivalStation.SelectedIndexChanged -= comboArrivalStation_SelectedIndexChanged;

            var selectedDeparture = comboDepartureStation.SelectedItem as ScheduleComboItem;
            if (selectedDeparture != null && ValidRoutes.TryGetValue(selectedDeparture.Name, out List<string> validArrivalNames))
            {
                var filteredArrivalStations = AllStationsList
                    .Where(s => validArrivalNames.Contains(s.Name))
                    .ToList();

                comboArrivalStation.DataSource = filteredArrivalStations;
                comboArrivalStation.DisplayMember = "Name";
                comboArrivalStation.ValueMember = "Id";

                if (filteredArrivalStations.Any()) comboArrivalStation.SelectedIndex = 0;
            }         
            comboArrivalStation.SelectedIndexChanged += comboArrivalStation_SelectedIndexChanged;
            UpdateFareAmountDisplay();
        }
        private void comboArrivalStation_SelectedIndexChanged(object sender, EventArgs e)
        {
          
            UpdateFareAmountDisplay();
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            var selectedDepStation = comboDepartureStation.SelectedItem as ScheduleComboItem;
            var selectedArrStation = comboArrivalStation.SelectedItem as ScheduleComboItem;
            string selectedClass = comboClass.SelectedItem?.ToString();

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

            if (selectedDepStation.Id == selectedArrStation.Id)
            {
                MessageBox.Show("Departure and Arrival Stations cannot be the same.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    
                    bool fareExists = db.Fare.Any(f =>
                        f.DepartureStationID == selectedDepStation.Id &&
                        f.ArrivalStationID == selectedArrStation.Id &&
                        f.ClassOfService == selectedClass 
                    );

                    if (fareExists)
                    {
                        MessageBox.Show($"A fare record already exists for the route {selectedDepStation.Name} to {selectedArrStation.Name} with the Class: {selectedClass}.",
                                        "Duplicate Fare Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                  
                    var newFare = new Fare
                    {
                        DepartureStationID = selectedDepStation.Id,
                        ArrivalStationID = selectedArrStation.Id,
                        FareAmount = fareAmount,
                        ClassOfService = selectedClass 
                    };

                    db.Fare.Add(newFare);
                    db.SaveChanges();

                    MessageBox.Show("Fare added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadFareDataGrid(); 
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
                            selectedFareId = -1;
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
            var selectedDep = comboDepartureStation.SelectedItem as ScheduleComboItem;
            var selectedArr = comboArrivalStation.SelectedItem as ScheduleComboItem;
            string selectedClass = comboClass.SelectedItem?.ToString();

            if (selectedDep == null || selectedArr == null || string.IsNullOrEmpty(selectedClass))
            {
                MessageBox.Show("Please select a valid scheduled route and service class.", "Input Required");
                return;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal fareAmount) || fareAmount <= 0)
            {
                MessageBox.Show("Fare amount must be a positive value.", "Pricing Error");
                return;
            }

            try
            {
                using (var db = new MugenSystemDBEntities())
                {

                    bool alreadyExists = db.Fare.Any(f =>
                        f.DepartureStationID == selectedDep.Id &&
                        f.ArrivalStationID == selectedArr.Id &&
                        f.ClassOfService == selectedClass);

                    if (alreadyExists)
                    {
                        MessageBox.Show("This route and class combination already has a fare set.", "Duplicate Record");
                        return;
                    }
                    var newFare = new Fare
                    {
                        DepartureStationID = selectedDep.Id,
                        ArrivalStationID = selectedArr.Id,
                        FareAmount = fareAmount,
                        ClassOfService = selectedClass
                    };

                    db.Fare.Add(newFare);
                    db.SaveChanges();

                    MessageBox.Show($"Fare for {selectedDep.Name} to {selectedArr.Name} added successfully!", "Success");

                    LoadFareDataGrid();
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}");
            }
        }
        private void dataFareGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataFareGridView.Rows[e.RowIndex];


                if (row.Cells["FareID"].Value != null)
                {
                    selectedFareId = (int)row.Cells["FareID"].Value;
                }

                int depId = (int)row.Cells["DepartureStationID"].Value;
                int arrId = (int)row.Cells["ArrivalStationID"].Value;

                comboDepartureStation.SelectedValue = depId;
                comboArrivalStation.SelectedValue = arrId;


                txtAmount.Text = row.Cells["FareAmount"].Value.ToString();
            }
        }

        private void comboClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFareAmountDisplay();
        }
    }
}