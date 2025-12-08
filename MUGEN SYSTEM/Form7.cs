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
    public partial class SchedulesDashboard : Form
    {
        private readonly UserAccount userLogIn;
        public SchedulesDashboard(UserAccount userLogin)
        {
            InitializeComponent();
            this.userLogIn = userLogIn;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AdminDashboard adminDashboard = new AdminDashboard(userLogIn);  
            adminDashboard.Show();

            this.Hide();

        }

        private void SchedulesDashboard_Load(object sender, EventArgs e)
        {

        }
    }
}
