namespace MUGEN_SYSTEM
{
    partial class Dashboard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Dashboard));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panelPic1 = new System.Windows.Forms.Panel();
            this.panelPic2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Location = new System.Drawing.Point(5, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(698, 229);
            this.panel1.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(75, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(424, 24);
            this.label7.TabIndex = 14;
            this.label7.Text = "SHINKANSEN TERMINAL BULLET TRAIN";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Palatino Linotype", 36F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(68, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(218, 63);
            this.label3.TabIndex = 15;
            this.label3.Text = "Welcome";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Palatino Linotype", 36F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(250, 148);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(188, 63);
            this.label6.TabIndex = 16;
            this.label6.Text = "Admin!";
            // 
            // panel3
            // 
            this.panel3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel3.BackgroundImage")));
            this.panel3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panel3.Location = new System.Drawing.Point(564, 27);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(107, 150);
            this.panel3.TabIndex = 17;
            // 
            // panelPic1
            // 
            this.panelPic1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelPic1.BackgroundImage")));
            this.panelPic1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panelPic1.Location = new System.Drawing.Point(5, 239);
            this.panelPic1.Name = "panelPic1";
            this.panelPic1.Size = new System.Drawing.Size(342, 190);
            this.panelPic1.TabIndex = 14;
            // 
            // panelPic2
            // 
            this.panelPic2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelPic2.BackgroundImage")));
            this.panelPic2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panelPic2.Location = new System.Drawing.Point(353, 239);
            this.panelPic2.Name = "panelPic2";
            this.panelPic2.Size = new System.Drawing.Size(350, 190);
            this.panelPic2.TabIndex = 15;
            // 
            // Dashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(60)))), ((int)(((byte)(95)))));
            this.ClientSize = new System.Drawing.Size(705, 439);
            this.Controls.Add(this.panelPic2);
            this.Controls.Add(this.panelPic1);
            this.Controls.Add(this.panel1);
            this.Name = "Dashboard";
            this.Text = "ADMIN DASHBOARD";
            this.Load += new System.EventHandler(this.DASHBOARD_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panelPic1;
        private System.Windows.Forms.Panel panelPic2;
    }
}