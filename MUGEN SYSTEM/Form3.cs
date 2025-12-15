using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MUGEN_SYSTEM
{
   
    public partial class PassengerForm : Form
    {
        private string _depStationName;
        private string _arrStationName;
        private DateTime _depTime;
        private DateTime _arrTime;
        private string _trainName;
        public PassengerForm(string depName, string arrName, DateTime depTime, DateTime arrTime, string trainName)
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
