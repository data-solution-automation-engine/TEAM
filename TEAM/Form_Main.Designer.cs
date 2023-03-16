namespace TEAM
{
    partial class FormMain
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
        /// 
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            groupBox2 = new System.Windows.Forms.GroupBox();
            labelWorkingEnvironment = new System.Windows.Forms.Label();
            labelWorkingEnvironmentType = new System.Windows.Forms.Label();
            richTextBoxInformation = new System.Windows.Forms.RichTextBox();
            menuStripMainMenu = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openMetadataDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            metadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openMetadataFormToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            generalSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            deployMetadataExamplesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            displayEventLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            backgroundWorkerEventLog = new System.ComponentModel.BackgroundWorker();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            openCoreDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            groupBox2.SuspendLayout();
            menuStripMainMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // groupBox2
            // 
            groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            groupBox2.Controls.Add(labelWorkingEnvironment);
            groupBox2.Controls.Add(labelWorkingEnvironmentType);
            groupBox2.Location = new System.Drawing.Point(14, 578);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(294, 57);
            groupBox2.TabIndex = 22;
            groupBox2.TabStop = false;
            groupBox2.Text = "Environment";
            // 
            // labelWorkingEnvironment
            // 
            labelWorkingEnvironment.AutoSize = true;
            labelWorkingEnvironment.Location = new System.Drawing.Point(164, 24);
            labelWorkingEnvironment.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelWorkingEnvironment.Name = "labelWorkingEnvironment";
            labelWorkingEnvironment.Size = new System.Drawing.Size(29, 15);
            labelWorkingEnvironment.TabIndex = 67;
            labelWorkingEnvironment.Text = "N/A";
            // 
            // labelWorkingEnvironmentType
            // 
            labelWorkingEnvironmentType.AutoSize = true;
            labelWorkingEnvironmentType.Location = new System.Drawing.Point(7, 24);
            labelWorkingEnvironmentType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelWorkingEnvironmentType.Name = "labelWorkingEnvironmentType";
            labelWorkingEnvironmentType.Size = new System.Drawing.Size(157, 15);
            labelWorkingEnvironmentType.TabIndex = 65;
            labelWorkingEnvironmentType.Text = "The working environment is:";
            // 
            // richTextBoxInformation
            // 
            richTextBoxInformation.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBoxInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richTextBoxInformation.Location = new System.Drawing.Point(16, 68);
            richTextBoxInformation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            richTextBoxInformation.Name = "richTextBoxInformation";
            richTextBoxInformation.ReadOnly = true;
            richTextBoxInformation.Size = new System.Drawing.Size(884, 591);
            richTextBoxInformation.TabIndex = 2;
            richTextBoxInformation.Text = "";
            richTextBoxInformation.TextChanged += richTextBoxInformation_TextChanged;
            richTextBoxInformation.Enter += richTextBoxInformation_Enter;
            // 
            // menuStripMainMenu
            // 
            menuStripMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, metadataToolStripMenuItem, configurationToolStripMenuItem, helpToolStripMenuItem });
            menuStripMainMenu.Location = new System.Drawing.Point(0, 0);
            menuStripMainMenu.Name = "menuStripMainMenu";
            menuStripMainMenu.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            menuStripMainMenu.Size = new System.Drawing.Size(915, 24);
            menuStripMainMenu.TabIndex = 4;
            menuStripMainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openMetadataDirectoryToolStripMenuItem, openCoreDirectoryToolStripMenuItem, toolStripSeparator3, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // openMetadataDirectoryToolStripMenuItem
            // 
            openMetadataDirectoryToolStripMenuItem.Image = Properties.Resources.OpenDirectoryIcon;
            openMetadataDirectoryToolStripMenuItem.Name = "openMetadataDirectoryToolStripMenuItem";
            openMetadataDirectoryToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            openMetadataDirectoryToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            openMetadataDirectoryToolStripMenuItem.Text = "&Open Metadata Directory";
            openMetadataDirectoryToolStripMenuItem.Click += openMetadataDirectoryToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(247, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Image = Properties.Resources.ExitApplication;
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            exitToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            exitToolStripMenuItem.Text = "E&xit Application";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // metadataToolStripMenuItem
            // 
            metadataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openMetadataFormToolStripMenuItem });
            metadataToolStripMenuItem.Name = "metadataToolStripMenuItem";
            metadataToolStripMenuItem.Size = new System.Drawing.Size(108, 20);
            metadataToolStripMenuItem.Text = "Design &Metadata";
            // 
            // openMetadataFormToolStripMenuItem
            // 
            openMetadataFormToolStripMenuItem.Image = Properties.Resources.CubeIcon;
            openMetadataFormToolStripMenuItem.Name = "openMetadataFormToolStripMenuItem";
            openMetadataFormToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M;
            openMetadataFormToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            openMetadataFormToolStripMenuItem.Text = "Manage &Metadata";
            openMetadataFormToolStripMenuItem.Click += openMetadataFormToolStripMenuItem_Click;
            // 
            // configurationToolStripMenuItem
            // 
            configurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { generalSettingsToolStripMenuItem, toolStripSeparator2, deployMetadataExamplesToolStripMenuItem });
            configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            configurationToolStripMenuItem.Size = new System.Drawing.Size(93, 20);
            configurationToolStripMenuItem.Text = "&Configuration";
            // 
            // generalSettingsToolStripMenuItem
            // 
            generalSettingsToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("generalSettingsToolStripMenuItem.Image");
            generalSettingsToolStripMenuItem.Name = "generalSettingsToolStripMenuItem";
            generalSettingsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            generalSettingsToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            generalSettingsToolStripMenuItem.Text = "&Settings";
            generalSettingsToolStripMenuItem.Click += generalSettingsToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(203, 6);
            // 
            // deployMetadataExamplesToolStripMenuItem
            // 
            deployMetadataExamplesToolStripMenuItem.Image = Properties.Resources.database_icon;
            deployMetadataExamplesToolStripMenuItem.Name = "deployMetadataExamplesToolStripMenuItem";
            deployMetadataExamplesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D;
            deployMetadataExamplesToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            deployMetadataExamplesToolStripMenuItem.Text = "&Deploy Examples";
            deployMetadataExamplesToolStripMenuItem.Click += deployMetadataExamplesToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { helpToolStripMenuItem1, displayEventLogToolStripMenuItem, toolStripSeparator1, aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // helpToolStripMenuItem1
            // 
            helpToolStripMenuItem1.Image = Properties.Resources.HelpIconSmall;
            helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
            helpToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H;
            helpToolStripMenuItem1.Size = new System.Drawing.Size(207, 22);
            helpToolStripMenuItem1.Text = "&Help";
            helpToolStripMenuItem1.Click += helpToolStripMenuItem_Click;
            // 
            // displayEventLogToolStripMenuItem
            // 
            displayEventLogToolStripMenuItem.Image = Properties.Resources.log_file;
            displayEventLogToolStripMenuItem.Name = "displayEventLogToolStripMenuItem";
            displayEventLogToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            displayEventLogToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            displayEventLogToolStripMenuItem.Text = "Display &Event Log";
            displayEventLogToolStripMenuItem.Click += displayEventLogToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(204, 6);
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Image = Properties.Resources.RavosLogo;
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.ToolTipText = "Information about TEAM";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // backgroundWorkerEventLog
            // 
            backgroundWorkerEventLog.WorkerReportsProgress = true;
            backgroundWorkerEventLog.WorkerSupportsCancellation = true;
            backgroundWorkerEventLog.DoWork += backgroundWorkerEventLog_DoWork;
            backgroundWorkerEventLog.ProgressChanged += backgroundWorkerEventLog_ProgressChanged;
            backgroundWorkerEventLog.RunWorkerCompleted += backgroundWorkerEventLog_RunWorkerCompleted;
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            pictureBox1.Image = Properties.Resources.RavosLogo;
            pictureBox1.Location = new System.Drawing.Point(774, 517);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(127, 115);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 15;
            pictureBox1.TabStop = false;
            // 
            // openCoreDirectoryToolStripMenuItem
            // 
            openCoreDirectoryToolStripMenuItem.Image = Properties.Resources.OpenDirectoryIcon;
            openCoreDirectoryToolStripMenuItem.Name = "openCoreDirectoryToolStripMenuItem";
            openCoreDirectoryToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            openCoreDirectoryToolStripMenuItem.Text = "Open Core Directory";
            openCoreDirectoryToolStripMenuItem.Click += openCoreDirectoryToolStripMenuItem_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            ClientSize = new System.Drawing.Size(915, 647);
            Controls.Add(groupBox2);
            Controls.Add(pictureBox1);
            Controls.Add(richTextBoxInformation);
            Controls.Add(menuStripMainMenu);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripMainMenu;
            Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            MinimumSize = new System.Drawing.Size(697, 456);
            Name = "FormMain";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Taxonomy for ETL Automation Metadata ";
            FormClosed += FormMain_FormClosed;
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            menuStripMainMenu.ResumeLayout(false);
            menuStripMainMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStripMainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem metadataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMetadataFormToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generalSettingsToolStripMenuItem;
        internal System.Windows.Forms.RichTextBox richTextBoxInformation;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label labelWorkingEnvironmentType;
        private System.ComponentModel.BackgroundWorker backgroundWorkerEventLog;
        private System.Windows.Forms.Label labelWorkingEnvironment;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem displayEventLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem openMetadataDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        internal System.Windows.Forms.ToolStripMenuItem deployMetadataExamplesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openCoreDirectoryToolStripMenuItem;
    }
}

