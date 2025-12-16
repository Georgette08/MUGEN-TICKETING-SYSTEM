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
using MUGENTICKETSYSTEM;

namespace MUGEN_SYSTEM
{
    public partial class PassengerForm : Form
    {
        private readonly UserAccount userlogIn;

        private int _scheduleId;
        private int _depStationId;
        private int _arrStationId;
        private string _trainName;
        private string _depStationName;
        private string _arrStationName;
        private DateTime _departureTime;
        private int _agentId;

        private const int MAX_SEAT_CAPACITY = 100;
        public PassengerForm(UserAccount userlogIn,int scheduleId, int depStationId, int arrStationId, string trainName, string depName,
            string arrName, DateTime departureTime, int agentId)
        {
            InitializeComponent();
            this.userlogIn = userlogIn;
            _scheduleId = scheduleId;
            _depStationId = depStationId;
            _arrStationId = arrStationId;
            _trainName = trainName;
            _depStationName = depName;
            _arrStationName = arrName;
            _departureTime = departureTime;
            _agentId = agentId; // Store the Agent ID

            DisplayTripDetails();
            LoadFareClasses();
            LoadAvailableSeats();
        }
        private void DisplayTripDetails()
        {
            txtScheduleID.Text = _scheduleId.ToString();
            txtTrainName.Text = _trainName; // ASSUMED: txtTrainName should be txtTrain based on image_809aa4
            txtDeparture.Text = _depStationName;
            txtArrival.Text = _arrStationName;
            txtDate.Text = _departureTime.ToShortDateString();
            txtRoute.Text = $"{_depStationName} to {_arrStationName}";
            txtStaff.Text = _agentId.ToString();

            // Initializing Fare/Seat
            txtTotalFare.Text = "0.00";
        }

        private void LoadFareClasses()
        {
            comboClass.DataSource = null;

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    // Get all unique Fare Classes that apply to this route (Dep/Arr Station IDs)
                    var fareClasses = db.Fare
                        .Where(f => f.DepartureStationID == _depStationId && f.ArrivalStationID == _arrStationId)
                        .Select(f => new FareComboItem
                        {
                            FareId = f.FareID,
                            ClassName = f.ClassOfService // ASSUMED: f.ClassOfService is f.ClassName
                            // FIX 5: Removed the conflicting 'FareAmount = f.FareAmount' line (CS0117 fix)
                        })
                        .ToList();

                    // ... (Binding logic is fine) ...
                    comboClass.DataSource = fareClasses;
                    comboClass.DisplayMember = "ClassName";
                    comboClass.ValueMember = "FareId";

                    // Attach the event handler to calculate fare when class changes
                    comboClass.SelectedIndexChanged += comboClass_SelectedIndexChanged;

                    if (fareClasses.Any())
                    {
                        // Select the first class by default and trigger the change event
                        comboClass.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading fare classes: {ex.Message}", "Database Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadAvailableSeats()
        {
            comboAvailableSeats.Items.Clear();

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    var bookedSeatNumbers = db.Bookings
                                             .Where(b => b.ScheduleID == _scheduleId)
                                             .Select(b => b.SeatNumber)
                                             .ToList();

                    int bookedSeatsCount = bookedSeatNumbers.Count;
                    int availableSeats = MAX_SEAT_CAPACITY - bookedSeatsCount;

                    // FIX 6: Changed txtSeats to comboAvailableSeats (Dropdown)
                    // If you have a separate textbox to DISPLAY the total count:
                    // txtAvailableSeatsDisplay.Text = availableSeats.ToString();

                    if (availableSeats > 0)
                    {
                        for (int i = 1; i <= MAX_SEAT_CAPACITY; i++)
                        {
                            string seatNum = i.ToString();
                            if (!bookedSeatNumbers.Contains(seatNum))
                            {
                                comboAvailableSeats.Items.Add(seatNum);
                            }
                        }
                    }

                    if (comboAvailableSeats.Items.Count > 0)
                    {
                        comboAvailableSeats.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("This train is fully booked!", "No Seats Available", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading available seats: {ex.Message}", "Database Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void comboClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboClass.SelectedItem is FareComboItem selectedFare)
            {
                int fareId = selectedFare.FareId;

                try
                {
                    using (var db = new MugenSystemDBEntities())
                    {
                        decimal? fareAmount = db.Fare
                                                .Where(f => f.FareID == fareId)
                                                .Select(f => f.FareAmount)
                                                .FirstOrDefault();

                        if (fareAmount.HasValue)
                        {
                            txtTotalFare.Text = fareAmount.Value.ToString("N2");
                            // Assuming you have a txtFareID control:
                            txtFareID.Text = fareId.ToString();
                        }
                        else
                        {
                            txtTotalFare.Text = "N/A";
                            txtFareID.Text = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error calculating fare: {ex.Message}", "Fare Calculation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                txtTotalFare.Text = "0.00";
                txtFareID.Text = "";
            }
        }
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            using (var db = new MugenSystemDBEntities())
            {
                // 1. DATA VALIDATION AND RETRIEVAL

                string firstName = txtFirstName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                string contactNumber = txtContact.Text.Trim(); // Use your actual control name
                string email = txtEmail.Text.Trim();       // Use your actual control name

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    MessageBox.Show("Please enter the passenger's full name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Retrieve Booking Details
                if (!int.TryParse(txtFareID.Text, out int fareId))
                {
                    MessageBox.Show("Fare Class must be selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string seatNumber;
                if (comboAvailableSeats.SelectedItem != null)
                {
                    seatNumber = comboAvailableSeats.SelectedItem.ToString();
                }
                else
                {
                    MessageBox.Show("Please select a Seat Number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!decimal.TryParse(txtTotalFare.Text.Replace("₱", "").Replace("$", ""), out decimal totalFarePaid))
                {
                    MessageBox.Show("Total Fare is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                // 2. PASSENGER RECORD INSERT/LOOKUP
                int passengerId = 0;

                var existingPassenger = db.Passengers.FirstOrDefault(p => p.ContactNumber == contactNumber || p.Email == email);

                if (existingPassenger != null)
                {
                    passengerId = existingPassenger.PassengerID;
                }
                else
                {
                    // Use the correct singular Entity type name: Passenger
                    var newPassenger = new Passengers
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        ContactNumber = contactNumber,
                        Email = email
                    };

                    db.Passengers.Add(newPassenger);
                    db.SaveChanges(); // CRITICAL: Save to get the new PassengerID

                    passengerId = newPassenger.PassengerID;
                    txtPassengerID.Text = passengerId.ToString();
                }


                // 3. BOOKING RECORD INSERT
                try
                {
                    // Use the correct singular Entity type name: Booking
                    var newBooking = new Bookings
                    {
                        ScheduleID = _scheduleId,
                        PassengerID = passengerId,
                        FareID = fareId,
                        SeatNumber = seatNumber,
                        TotalFarePaid = totalFarePaid,
                        BookingDate = DateTime.Now,
                        AgentID = _agentId                // This value is now correct (not 0)
                    };

                    db.Bookings.Add(newBooking);
                    db.SaveChanges();

                    // Success: Update UI and close form
                    txtBookingID.Text = newBooking.BookingID.ToString();

                    MessageBox.Show($"Booking successful! Booking ID: {newBooking.BookingID}", "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    btnConfirm.Enabled = false;
                    this.Close(); // Return control to the StaffDashboard

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Booking failed. Please check data constraints. Error: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnCustomers_Click(object sender, EventArgs e)
        {
            this.Hide();

            CustomersDashboard customersDashboard = new CustomersDashboard(userlogIn);

           
            customersDashboard.ShowDialog();

           
            this.Show();
        }
  
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtPassengerID.Text, out int passengerId) || passengerId == 0)
            {
                MessageBox.Show("Cannot update. No existing Passenger ID is loaded.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Retrieve the new data from the form fields
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string contactNumber = txtContact.Text.Trim();
            string email = txtEmail.Text.Trim();

            // Basic Validation
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("First Name and Last Name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    // 1. Find the existing Passenger entity
                    // FIX: Using db.Passengers (assuming plural collection name) and singular type Passenger
                    var passengerToUpdate = db.Passengers.FirstOrDefault(p => p.PassengerID == passengerId);

                    if (passengerToUpdate != null)
                    {
                        // 2. Apply the updates
                        passengerToUpdate.FirstName = firstName;
                        passengerToUpdate.LastName = lastName;
                        passengerToUpdate.ContactNumber = contactNumber;
                        passengerToUpdate.Email = email;

                        // 3. Save the changes to the database
                        db.SaveChanges();

                        MessageBox.Show($"Passenger ID {passengerId} details updated successfully.", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Passenger ID {passengerId} not found in the database.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating passenger data: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PassengerForm_Load(object sender, EventArgs e)
        {

        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(
        "Are you sure you want to log out? This will close the application and return to the login screen.",
        "Confirm Logout",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
    );

            if (confirmResult == DialogResult.Yes)
            {
                // 1. Instantiate and show the login form
                LoginForm loginForm = new LoginForm();
                loginForm.Show();

                // 2. CRITICAL: Close ALL related forms (including hidden parents like StaffDashboard)
                // We iterate through all open forms and close them except the new LoginForm.
                // Use a list to avoid modifying the collection while iterating
                List<Form> formsToClose = new List<Form>();

                foreach (Form form in Application.OpenForms)
                {
                    // Do not close the new LoginForm
                    if (form != loginForm)
                    {
                        formsToClose.Add(form);
                    }
                }

                // 3. Close the identified forms (PassengerForm, StaffDashboard, etc.)
                foreach (Form form in formsToClose)
                {
                    form.Close();
                }

                // The PassengerForm will close itself here via the formsToClose loop, 
                // but adding this.Close() one last time is harmless if it hasn't closed yet.
                this.Close();
            }
        }
    }
}


