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
    public partial class AdminDashboard : Form
    {
        private MugenSystemDBEntities dbAdmin;
        private readonly UserAccount userLogIn;
        public AdminDashboard(UserAccount userLogIn)
        {
            InitializeComponent();
            this.userLogIn = userLogIn;
        }

        private void AdminDashboard_Load(object sender, EventArgs e)
        {
            dbAdmin = new MugenSystemDBEntities();
        }
        private void OpenChildForm(Form childForm)
        {
            panelMain.Visible = false; 
            panelPic1.Visible = false; 
            panelPic2.Visible = false; 

           
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;

            childForm.Size = new Size(735, 900); 
            childForm.Location = new Point(190, 30);

            this.Controls.Add(childForm);
            childForm.BringToFront();
            childForm.Show();
        }
        private void HighlightButton(Button activeBtn)
        {
            foreach (Control ctrl in panelNav.Controls) 
            {
                if (ctrl is Button btn)
                {
                   
                    btn.BackColor = Color.White;
                    btn.ForeColor = Color.MidnightBlue;
                }
            }

           
            activeBtn.BackColor = Color.LightSkyBlue; 
            activeBtn.ForeColor = Color.Black;
        }
        private void btnStations_Click(object sender, EventArgs e)
        {
            HighlightButton((Button)sender);
            StationsDashboard stations = new StationsDashboard(userLogIn);
            OpenChildForm(stations);
           

        }

        private void btnTrains_Click(object sender, EventArgs e)
        {
            HighlightButton((Button)sender);
            TrainsDashboard trains = new TrainsDashboard(this.userLogIn);
            OpenChildForm(trains);
         
        }

        private void btnSchedules_Click(object sender, EventArgs e)
        {
            HighlightButton((Button)sender);
            SchedulesDashboard schedules = new SchedulesDashboard(this.userLogIn);
            OpenChildForm(schedules);
            
        }

        private void btnFares_Click(object sender, EventArgs e)
        {
            HighlightButton((Button)sender);
            FaresDashboard fares = new FaresDashboard(this.userLogIn);
            OpenChildForm(fares);
        }

        private void btnUserAccounts_Click(object sender, EventArgs e)
        {
            HighlightButton((Button)sender);
            UserDashboard Account = new UserDashboard(this.userLogIn);
            OpenChildForm(Account);
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            HighlightButton((Button)sender);
            var confirmResult = MessageBox.Show(
               "Are you sure you want to log out?",
               "Confirm Logout",
               MessageBoxButtons.YesNo,
               MessageBoxIcon.Question
            );

            if (confirmResult == DialogResult.Yes)
            {
               
                LoginForm login = new LoginForm();
                login.Show();
                this.Close();
            }
        }
        private void btnDashboard_Click(object sender, EventArgs e)
        {
            HighlightButton((Button)sender);
            Dashboard adminDashboard = new Dashboard(this.userLogIn);
            OpenChildForm(adminDashboard);
        }
    }
}
