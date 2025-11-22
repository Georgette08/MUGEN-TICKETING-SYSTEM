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
    public partial class AdminDashboard : Form
    {
        private MugenSystemDBEntities dbAdmin;
        private readonly UserAccount userLogIn;
        public AdminDashboard(UserAccount userLogIn)
        {
            InitializeComponent();
            this.userLogIn =userLogIn;
        }

        private void AdminDashboard_Load(object sender, EventArgs e)
        {
            dbAdmin = new MugenSystemDBEntities();
        }
        private void ShowandManageForm(Form newform)
        {
            this.Hide();

            newform.ShowDialog();

            this.Show();
        }       

        private void btnStations_Click(object sender, EventArgs e)
        {
            StationsDashboard stations  = new StationsDashboard(userLogIn); 
            ShowandManageForm(stations);    

        }

        private void btnTrains_Click(object sender, EventArgs e)
        {
            TrainsDashnoard trains = new TrainsDashnoard(); 
            ShowandManageForm(trains); 
        }

        private void btnSchedules_Click(object sender, EventArgs e)
        {
            SchedulesDashboard schedules = new SchedulesDashboard(); 
            ShowandManageForm(schedules);
        }

        private void btnFares_Click(object sender, EventArgs e)
        {
            FaresDashboard fares = new FaresDashboard();   
            ShowandManageForm(fares);
        }

        private void btnUserAccounts_Click(object sender, EventArgs e)
        {
            UserDashboard Account = new UserDashboard();
            ShowandManageForm(Account);
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
                this.Close();         
            }
        }
    }
}
