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
    public partial class PassengerForm : Form
    {

        private int _scheduleId;
        private int _depStationId;
        private int _arrStationId;
        private string _trainName;
        private string _depStationName;
        private string _arrStationName;
        private DateTime _departureTime;
        private int _agentId;

        private const int MAX_SEAT_CAPACITY = 100;
        public PassengerForm(int scheduleId, int depStationId, int arrStationId, string trainName, string depName,
            string arrName, DateTime departureTime, int agentId)
        {
            InitializeComponent();
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

                // FIX 7: Check your control names. Assuming 'txtContact' is txtContactNo, etc.
                string firstName = txtFirstName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                string contactNumber = txtContact.Text.Trim(); // ASSUMED: txtContact is txtContactNo
                string email = txtEmail.Text.Trim();     // ASSUMED: txtEmail is txtEmailAddress

                // b. Booking Details (Fetched from hidden/calculated fields)
                // ... (Validation for fareId, seatNumber, totalFarePaid are fine) ...
                if (!int.TryParse(txtFareID.Text, out int fareId)) { /* ... */ return; }

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

                if (!decimal.TryParse(txtTotalFare.Text.Replace("₱", "").Replace("$", ""), out decimal totalFarePaid)) { /* ... */ return; }
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName)) { /* ... */ return; }


                // 2. PASSENGER RECORD INSERT/LOOKUP
                int passengerId = 0;
                var existingPassenger = db.Passengers.FirstOrDefault(p => p.ContactNumber == contactNumber || p.Email == email);

                if (existingPassenger != null)
                {
                    passengerId = existingPassenger.PassengerID;
                }
                else
                {
                    // Insert new passenger record
                    var newPassenger = new Passengers
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        ContactNumber = contactNumber,
                        Email = email
                    };

                    db.Passengers.Add(newPassenger);
                    db.SaveChanges(); // Save to get the new PassengerID
                    passengerId = newPassenger.PassengerID;
                    txtPassengerID.Text = passengerId.ToString();
                }

                // 3. BOOKING RECORD INSERT
                try
                {
                    var newBooking = new Bookings
                    {
                        ScheduleID = _scheduleId,
                        PassengerID = passengerId,
                        FareID = fareId,
                        SeatNumber = seatNumber,
                        TotalFarePaid = totalFarePaid,
                        BookingDate = DateTime.Now,
                        AgentID = _agentId
                    };

                    db.Bookings.Add(newBooking);
                    db.SaveChanges();

                    txtBookingID.Text = newBooking.BookingID.ToString();
                    MessageBox.Show($"Booking successful! Booking ID: {newBooking.BookingID}", "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnConfirm.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Booking failed. Error: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public class FareComboItem
        {
            public int FareId { get; set; }
            public string ClassName { get; set; }
            public override string ToString() => ClassName;
        }
    }
}


