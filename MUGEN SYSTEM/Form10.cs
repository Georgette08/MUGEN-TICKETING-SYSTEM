using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MUGENTICKETSYSTEM;

namespace MUGEN_SYSTEM
{
    public partial class CustomersDashboard : Form
    {
        private UserAccount userLogIn;
        private int _selectedBookingId = 0;

        public CustomersDashboard(UserAccount userlogIn)
        {
            InitializeComponent();
            this.userLogIn = userLogIn;

            dataCustomerLogGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataCustomerLogGridView.DefaultCellStyle.ForeColor = Color.Black;
            dataCustomerLogGridView.RowsDefaultCellStyle.ForeColor = Color.Black;
        }

        private void CustomersDashboard_Load(object sender, EventArgs e)
        {
            LoadCustomerBookingLog();
        }
        private void LoadCustomerBookingLog()
        {
            try
            {
                using (var db = new MugenSystemDBEntities())
                {
                    var customerLog = db.Bookings
                        .Select(booking => new
                        {
                            BookingID = booking.BookingID,
                            CustomerName = booking.Passengers.FirstName + " " + booking.Passengers.LastName,
                            Contact = booking.Passengers.ContactNumber,
                            Email = booking.Passengers.Email,
                            ScheduleID = booking.ScheduleID,
                            Class = booking.Fare.ClassOfService,
                            Seat = booking.SeatNumber,
                            TotalFare = booking.TotalFarePaid,
                            BookingDate = booking.BookingDate
                        })
                        .OrderByDescending(log => log.BookingDate)
                        .ToList();

                    dataCustomerLogGridView.DataSource = customerLog;

                    if (!customerLog.Any())
                    {
                        MessageBox.Show("No customer booking records found.", "Empty Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customer log: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnBack_Click(object sender, EventArgs e)

        {
            this.Close();
        }

        private void dataCustomerLogGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                _selectedBookingId = 0;
                return;
            }

            DataGridViewRow selectedRow = dataCustomerLogGridView.Rows[e.RowIndex];
            int retrievedBookingId = 0;

            try
            {
                string bookingIdValue = selectedRow.Cells["BookingID"].Value?.ToString() ?? "0";

                if (int.TryParse(bookingIdValue, out retrievedBookingId))
                {
                    _selectedBookingId = retrievedBookingId;
                }
                else
                {
                    _selectedBookingId = 0;
                }
            }
            catch (Exception)
            {
                _selectedBookingId = 0;
            }
        }

        private void btndDelete_Click(object sender, EventArgs e)
        {
            if (_selectedBookingId <= 0)
            {
                MessageBox.Show("Please select a booking from the log to delete.", "Select Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Are you sure you want to permanently delete Booking ID {_selectedBookingId}?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error
            );

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    using (var db = new MugenSystemDBEntities())
                    {
                        var bookingToDelete = db.Bookings.Find(_selectedBookingId);

                        if (bookingToDelete != null)
                        {
                            db.Bookings.Remove(bookingToDelete);
                            db.SaveChanges();

                            MessageBox.Show("Booking deleted successfully.", "Deletion Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadCustomerBookingLog(); 
                            _selectedBookingId = 0; 
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during deletion: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}