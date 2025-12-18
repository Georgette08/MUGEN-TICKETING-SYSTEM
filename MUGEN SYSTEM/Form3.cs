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
            _agentId = agentId;

            
            comboAvailableSeats.SelectedIndexChanged += comboAvailableSeats_SelectedIndexChanged;

            DisplayTripDetails();
            LoadFareClasses();
            LoadAvailableSeats();
        }
        private void DisplayTripDetails()
        {
            txtScheduleID.Text = _scheduleId.ToString();
            txtTrainName.Text = _trainName;
            txtDeparture.Text = _depStationName;
            txtArrival.Text = _arrStationName;
            txtDate.Text = _departureTime.ToShortDateString();
            txtRoute.Text = $"{_depStationName} to {_arrStationName}";
            txtStaff.Text = _agentId.ToString();
            txtTotalFare.Text = "0.00";

        
            txtSeats.ReadOnly = true;
        }

        private void LoadFareClasses()
        {
            comboClass.DataSource = null;

            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    var fareClasses = db.Fare
                        .Where(f => f.DepartureStationID == _depStationId && f.ArrivalStationID == _arrStationId)
                        .Select(f => new FareComboItem
                        {
                            FareId = f.FareID,
                            ClassName = f.ClassOfService                           
                        })
                        .ToList();

                    comboClass.DataSource = fareClasses;
                    comboClass.DisplayMember = "ClassName";
                    comboClass.ValueMember = "FareId";

                    comboClass.SelectedIndexChanged += comboClass_SelectedIndexChanged;

                    if (fareClasses.Any())
                    {
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
                    var currentSchedule = db.Schedule.FirstOrDefault(s => s.ScheduleID == _scheduleId);

                    if (currentSchedule != null)
                    {
                        var linkedTrain = db.Trains.FirstOrDefault(t => t.TrainID == currentSchedule.TrainID);

                        if (linkedTrain != null)
                        {
                          
                            int actualCapacity = (int)linkedTrain.Capacity;
                       
                            var bookedSeats = db.Bookings
                                .Where(b => b.ScheduleID == _scheduleId)
                                .Select(b => b.SeatNumber)
                                .ToList();

                            for (int i = 1; i <= actualCapacity; i++)
                            {
                                string seat = i.ToString();
                                if (!bookedSeats.Contains(seat))
                                {
                                    comboAvailableSeats.Items.Add(seat);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
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
                try
                {
                    string firstName = txtFirstName.Text.Trim();
                    string lastName = txtLastName.Text.Trim();
                    string contactNumber = txtContact.Text.Trim();
                    string email = txtEmail.Text.Trim();
                    string seatNumber = txtSeats.Text; 

                    if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrEmpty(seatNumber))
                    {
                        MessageBox.Show("Please fill in the Passenger Name and select a Seat Number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!int.TryParse(txtFareID.Text, out int fareId) || !decimal.TryParse(txtTotalFare.Text.Replace("₱", "").Replace(",", ""), out decimal totalFare))
                    {
                        MessageBox.Show("Invalid Fare or Class selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    bool isSeatTaken = db.Bookings.Any(b => b.ScheduleID == _scheduleId && b.SeatNumber == seatNumber);
                    if (isSeatTaken)
                    {
                        MessageBox.Show("This seat was just booked by another agent. Please select a different seat.", "Seat Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        LoadAvailableSeats(); 
                        return;
                    }
                    var existingPassenger = db.Passengers.FirstOrDefault(p => p.ContactNumber == contactNumber || p.Email == email);
                    int finalPassengerId;

                    if (existingPassenger != null)
                    {
                        finalPassengerId = existingPassenger.PassengerID;
                    }
                    else
                    {
                        var newPassenger = new Passengers
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            ContactNumber = contactNumber,
                            Email = email
                        };
                        db.Passengers.Add(newPassenger);
                        db.SaveChanges(); 
                        finalPassengerId = newPassenger.PassengerID;
                    }

                    var newBooking = new Bookings
                    {
                        ScheduleID = _scheduleId,
                        PassengerID = finalPassengerId,
                        FareID = fareId,
                        SeatNumber = seatNumber,
                        TotalFarePaid = totalFare,
                        BookingDate = DateTime.Now,
                        AgentID = _agentId 
                    };

                    db.Bookings.Add(newBooking);
                    db.SaveChanges();

                    txtBookingID.Text = newBooking.BookingID.ToString();
                    txtPassengerID.Text = finalPassengerId.ToString();

                    MessageBox.Show($"Booking confirmed successfully!\n\nBooking ID: {newBooking.BookingID}\nSeat: {seatNumber}\nPassenger: {firstName} {lastName}",
                                    "Mugen System - Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    btnConfirm.Enabled = false;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"System Error during booking: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string contactNumber = txtContact.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("First Name and Last Name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var db = new MugenSystemDBEntities())
                {

                    var passengerToUpdate = db.Passengers.FirstOrDefault(p => p.PassengerID == passengerId);

                    if (passengerToUpdate != null)
                    {
                        // 2. Apply the updates
                        passengerToUpdate.FirstName = firstName;
                        passengerToUpdate.LastName = lastName;
                        passengerToUpdate.ContactNumber = contactNumber;
                        passengerToUpdate.Email = email;

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
                LoginForm loginForm = new LoginForm();
                loginForm.Show();

                List<Form> formsToClose = new List<Form>();

                foreach (Form form in Application.OpenForms)
                {
                   
                    if (form != loginForm)
                    {
                        formsToClose.Add(form);
                    }
                }

                foreach (Form form in formsToClose)
                {
                    form.Close();
                }
                this.Close();
            }
        }

        private void comboAvailableSeats_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboAvailableSeats.SelectedItem != null)
            {
                txtSeats.Text = comboAvailableSeats.SelectedItem.ToString();
            }
        }
    }
}


