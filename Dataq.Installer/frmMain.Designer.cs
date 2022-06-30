
namespace Dataq.Installer
{
    partial class frmMain
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
            this.btnInstall = new System.Windows.Forms.Button();
            this.lblPackageName = new System.Windows.Forms.Label();
            this.progBar = new System.Windows.Forms.ProgressBar();
            this.lblPercent = new System.Windows.Forms.Label();
            this.grpInst = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.grpInst.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnInstall
            // 
            this.btnInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInstall.Location = new System.Drawing.Point(220, 356);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(179, 48);
            this.btnInstall.TabIndex = 0;
            this.btnInstall.Text = "Install";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // lblPackageName
            // 
            this.lblPackageName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPackageName.Location = new System.Drawing.Point(19, 49);
            this.lblPackageName.Name = "lblPackageName";
            this.lblPackageName.Size = new System.Drawing.Size(572, 37);
            this.lblPackageName.TabIndex = 1;
            this.lblPackageName.Text = "Dataq Software installer";
            this.lblPackageName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progBar
            // 
            this.progBar.Location = new System.Drawing.Point(12, 302);
            this.progBar.Name = "progBar";
            this.progBar.Size = new System.Drawing.Size(567, 33);
            this.progBar.TabIndex = 2;
            // 
            // lblPercent
            // 
            this.lblPercent.AutoSize = true;
            this.lblPercent.BackColor = System.Drawing.Color.Transparent;
            this.lblPercent.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPercent.Location = new System.Drawing.Point(271, 280);
            this.lblPercent.Name = "lblPercent";
            this.lblPercent.Size = new System.Drawing.Size(0, 18);
            this.lblPercent.TabIndex = 1;
            // 
            // grpInst
            // 
            this.grpInst.Controls.Add(this.label3);
            this.grpInst.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpInst.Location = new System.Drawing.Point(79, 130);
            this.grpInst.Name = "grpInst";
            this.grpInst.Size = new System.Drawing.Size(466, 84);
            this.grpInst.TabIndex = 3;
            this.grpInst.TabStop = false;
            this.grpInst.Text = "Please, complete the following tasks:";
            this.grpInst.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(138, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 16);
            this.label3.TabIndex = 0;
            this.label3.Text = "Restart your computer";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 416);
            this.Controls.Add(this.grpInst);
            this.Controls.Add(this.lblPercent);
            this.Controls.Add(this.lblPackageName);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.progBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.Text = "Dataq Installer";
            this.grpInst.ResumeLayout(false);
            this.grpInst.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Label lblPackageName;
        private System.Windows.Forms.ProgressBar progBar;
        private System.Windows.Forms.Label lblPercent;
        private System.Windows.Forms.GroupBox grpInst;
        private System.Windows.Forms.Label label3;
    }
}

