using System.Diagnostics;
using System.Windows.Forms;

namespace TEAM
{
    partial class FormAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAbout));
            buttonClose = new Button();
            textBox1 = new TextBox();
            linkLabel1 = new LinkLabel();
            SuspendLayout();
            // 
            // buttonClose
            // 
            buttonClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonClose.Location = new System.Drawing.Point(110, 259);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(109, 40);
            buttonClose.TabIndex = 1;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // textBox1
            // 
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            textBox1.Location = new System.Drawing.Point(41, 56);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(264, 13);
            textBox1.TabIndex = 2;
            textBox1.Text = "Taxonomy of ETL Automation Metadata";
            textBox1.TextAlign = HorizontalAlignment.Center;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new System.Drawing.Point(13, 216);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new System.Drawing.Size(315, 13);
            linkLabel1.TabIndex = 21;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "https://github.com/data-solution-automation-engine/TEAM";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked_1;
            // 
            // FormAbout
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.ControlLightLight;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new System.Drawing.Size(334, 311);
            ControlBox = false;
            Controls.Add(linkLabel1);
            Controls.Add(textBox1);
            Controls.Add(buttonClose);
            DoubleBuffered = true;
            Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(350, 350);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(350, 350);
            Name = "FormAbout";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About";
            ResumeLayout(false);
            PerformLayout();
        }

        private void richTextBox1_LinkClicked(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        #endregion
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.TextBox textBox1;
        private LinkLabel linkLabelTeam;
        private LinkLabel linkLabel1;
    }
}