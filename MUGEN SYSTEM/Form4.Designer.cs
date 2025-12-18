namespace MUGEN_SYSTEM
{
    partial class AdminDashboard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdminDashboard));
            this.panelNav = new System.Windows.Forms.Panel();
            this.btnLogOut = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnUserAccounts = new System.Windows.Forms.Button();
            this.btnFares = new System.Windows.Forms.Button();
            this.btnSchedules = new System.Windows.Forms.Button();
            this.btnTrains = new System.Windows.Forms.Button();
            this.btnStations = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelMain = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panelPic1 = new System.Windows.Forms.Panel();
            this.panelPic2 = new System.Windows.Forms.Panel();
            this.btnDashboard = new System.Windows.Forms.Button();
            this.panelNav.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelNav
            // 
            this.panelNav.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(60)))), ((int)(((byte)(95)))));
            this.panelNav.Controls.Add(this.btnDashboard);
            this.panelNav.Controls.Add(this.btnLogOut);
            this.panelNav.Controls.Add(this.label5);
            this.panelNav.Controls.Add(this.label4);
            this.panelNav.Controls.Add(this.btnUserAccounts);
            this.panelNav.Controls.Add(this.btnFares);
            this.panelNav.Controls.Add(this.btnSchedules);
            this.panelNav.Controls.Add(this.btnTrains);
            this.panelNav.Controls.Add(this.btnStations);
            this.panelNav.Location = new System.Drawing.Point(3, 33);
            this.panelNav.Name = "panelNav";
            this.panelNav.Size = new System.Drawing.Size(194, 425);
            this.panelNav.TabIndex = 0;
            // 
            // btnLogOut
            // 
            this.btnLogOut.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogOut.Location = new System.Drawing.Point(102, 351);
            this.btnLogOut.Name = "btnLogOut";
            this.btnLogOut.Size = new System.Drawing.Size(78, 29);
            this.btnLogOut.TabIndex = 7;
            this.btnLogOut.Text = "LOG OUT";
            this.btnLogOut.UseVisualStyleBackColor = true;
            this.btnLogOut.Click += new System.EventHandler(this.btnLogOut_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label5.Location = new System.Drawing.Point(60, 63);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 17);
            this.label5.TabIndex = 6;
            this.label5.Text = "SYSTEM";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label4.Location = new System.Drawing.Point(19, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(161, 17);
            this.label4.TabIndex = 5;
            this.label4.Text = "MUGEN TICKETING ";
            // 
            // btnUserAccounts
            // 
            this.btnUserAccounts.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUserAccounts.Location = new System.Drawing.Point(22, 253);
            this.btnUserAccounts.Name = "btnUserAccounts";
            this.btnUserAccounts.Size = new System.Drawing.Size(145, 29);
            this.btnUserAccounts.TabIndex = 4;
            this.btnUserAccounts.Text = "USER ACCOUNTS";
            this.btnUserAccounts.UseVisualStyleBackColor = true;
            this.btnUserAccounts.Click += new System.EventHandler(this.btnUserAccounts_Click);
            // 
            // btnFares
            // 
            this.btnFares.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFares.Location = new System.Drawing.Point(22, 218);
            this.btnFares.Name = "btnFares";
            this.btnFares.Size = new System.Drawing.Size(145, 29);
            this.btnFares.TabIndex = 3;
            this.btnFares.Text = "MANAGE FARES";
            this.btnFares.UseVisualStyleBackColor = true;
            this.btnFares.Click += new System.EventHandler(this.btnFares_Click);
            // 
            // btnSchedules
            // 
            this.btnSchedules.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSchedules.Location = new System.Drawing.Point(22, 183);
            this.btnSchedules.Name = "btnSchedules";
            this.btnSchedules.Size = new System.Drawing.Size(145, 29);
            this.btnSchedules.TabIndex = 2;
            this.btnSchedules.Text = "MANAGE SCHEDULES";
            this.btnSchedules.UseVisualStyleBackColor = true;
            this.btnSchedules.Click += new System.EventHandler(this.btnSchedules_Click);
            // 
            // btnTrains
            // 
            this.btnTrains.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTrains.Location = new System.Drawing.Point(22, 148);
            this.btnTrains.Name = "btnTrains";
            this.btnTrains.Size = new System.Drawing.Size(145, 29);
            this.btnTrains.TabIndex = 1;
            this.btnTrains.Text = "MANAGE TRAINS";
            this.btnTrains.UseVisualStyleBackColor = true;
            this.btnTrains.Click += new System.EventHandler(this.btnTrains_Click);
            // 
            // btnStations
            // 
            this.btnStations.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStations.Location = new System.Drawing.Point(22, 113);
            this.btnStations.Name = "btnStations";
            this.btnStations.Size = new System.Drawing.Size(145, 29);
            this.btnStations.TabIndex = 0;
            this.btnStations.Text = "MANAGE STATIONS";
            this.btnStations.UseVisualStyleBackColor = true;
            this.btnStations.Click += new System.EventHandler(this.btnStations_Click);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panel4.Controls.Add(this.panel5);
            this.panel4.Controls.Add(this.label1);
            this.panel4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.panel4.Location = new System.Drawing.Point(3, 4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(892, 26);
            this.panel4.TabIndex = 3;
            // 
            // panel5
            // 
            this.panel5.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel5.BackgroundImage")));
            this.panel5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel5.Location = new System.Drawing.Point(3, 3);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(42, 20);
            this.panel5.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(51, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "ADMIN DASHBOARD";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 13);
            this.label2.TabIndex = 9;
            // 
            // panelMain
            // 
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panelMain.Controls.Add(this.label7);
            this.panelMain.Controls.Add(this.label6);
            this.panelMain.Controls.Add(this.label3);
            this.panelMain.Controls.Add(this.panel3);
            this.panelMain.Controls.Add(this.label2);
            this.panelMain.Location = new System.Drawing.Point(203, 33);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(692, 229);
            this.panelMain.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(75, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(424, 24);
            this.label7.TabIndex = 13;
            this.label7.Text = "SHINKANSEN TERMINAL BULLET TRAIN";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Palatino Linotype", 36F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(250, 148);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(188, 63);
            this.label6.TabIndex = 12;
            this.label6.Text = "Admin!";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Palatino Linotype", 36F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(68, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(218, 63);
            this.label3.TabIndex = 11;
            this.label3.Text = "Welcome";
            // 
            // panel3
            // 
            this.panel3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel3.BackgroundImage")));
            this.panel3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panel3.Location = new System.Drawing.Point(564, 27);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(107, 150);
            this.panel3.TabIndex = 10;
            // 
            // panelPic1
            // 
            this.panelPic1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelPic1.BackgroundImage")));
            this.panelPic1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panelPic1.Location = new System.Drawing.Point(203, 268);
            this.panelPic1.Name = "panelPic1";
            this.panelPic1.Size = new System.Drawing.Size(342, 190);
            this.panelPic1.TabIndex = 13;
            // 
            // panelPic2
            // 
            this.panelPic2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelPic2.BackgroundImage")));
            this.panelPic2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panelPic2.Location = new System.Drawing.Point(550, 268);
            this.panelPic2.Name = "panelPic2";
            this.panelPic2.Size = new System.Drawing.Size(345, 190);
            this.panelPic2.TabIndex = 14;
            // 
            // btnDashboard
            // 
            this.btnDashboard.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDashboard.Location = new System.Drawing.Point(22, 288);
            this.btnDashboard.Name = "btnDashboard";
            this.btnDashboard.Size = new System.Drawing.Size(145, 29);
            this.btnDashboard.TabIndex = 8;
            this.btnDashboard.Text = "DASHBOARD";
            this.btnDashboard.UseVisualStyleBackColor = true;
            this.btnDashboard.Click += new System.EventHandler(this.btnDashboard_Click);
            // 
            // AdminDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(60)))), ((int)(((byte)(95)))));
            this.ClientSize = new System.Drawing.Size(901, 467);
            this.Controls.Add(this.panelPic2);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panelPic1);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelNav);
            this.Name = "AdminDashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ADMIN DASHBOARD";
            this.Load += new System.EventHandler(this.AdminDashboard_Load);
            this.panelNav.ResumeLayout(false);
            this.panelNav.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelNav;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Button btnSchedules;
        private System.Windows.Forms.Button btnTrains;
        private System.Windows.Forms.Button btnStations;
        private System.Windows.Forms.Button btnUserAccounts;
        private System.Windows.Forms.Button btnFares;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnLogOut;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panelPic1;
        private System.Windows.Forms.Panel panelPic2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnDashboard;
    }
}