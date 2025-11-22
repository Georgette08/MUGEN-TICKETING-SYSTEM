using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;
using System.Data.Entity;

namespace MUGEN_SYSTEM
{
    public partial class StationsDashboard : Form
    {  
        private MugenSystemDBEntities dbStations;
        private readonly UserAccount userLogIn;
        public StationsDashboard(UserAccount userLogIn  )
        {
            InitializeComponent();
            this.userLogIn = userLogIn;
        }

        private void StationsDashboard_Load(object sender, EventArgs e)
        {
            using (var dbStations = new MugenSystemDBEntities())
            {
                var stationsList = dbStations.Stations.ToList();
                dgvStations.DataSource = stationsList;
            }
        }
        private void btnADD_Click(object sender, EventArgs e)
        {
          
        }
    }
}
