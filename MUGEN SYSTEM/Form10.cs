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
        public CustomersDashboard(UserAccount userlogIn)
        {
            InitializeComponent();
            this.userLogIn = userLogIn;
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
                    // Join Bookings and Passengers to see who booked what.
                    var customerLog = db.Bookings
                        .Join(
                            db.Passengers,
                            booking => booking.PassengerID, // Key from Bookings
                            passenger => passenger.PassengerID, // Key from Passengers
                            (booking, passenger) => new // Result Selector
                            {
                                // Select all fields you want to display in the grid
                                BookingID = booking.BookingID,
                                CustomerName = passenger.FirstName + " " + passenger.LastName,
                                Contact = passenger.ContactNumber,
                                Email = passenger.Email,
                                ScheduleID = booking.ScheduleID,
                                Class = booking.Fare.ClassOfService, // Access Class Name via Fare FK
                                Seat = booking.SeatNumber,
                                TotalFare = booking.TotalFarePaid,
                                BookingDate = booking.BookingDate
                            }
                        )
                        .OrderByDescending(log => log.BookingDate) // Show newest bookings first
                        .ToList();

                    // Bind the results to the DataGrid
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
    }
}
