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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelWorkingEnvironment = new System.Windows.Forms.Label();
            this.labelMetadataRepository = new System.Windows.Forms.Label();
            this.labelRepositoryDate = new System.Windows.Forms.Label();
            this.labelRepositoryUpdateDateTime = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.labelRepositoryVersion = new System.Windows.Forms.Label();
            this.groupBoxVersionSelection = new System.Windows.Forms.GroupBox();
            this.labelActiveVersion = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.labelDocumentationVersion = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.richTextBoxInformation = new System.Windows.Forms.RichTextBox();
            this.menuStripMainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openOutputDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.metadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMetadataFormToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generalSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.repositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createRebuildRepositoryToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox2.SuspendLayout();
            this.groupBoxVersionSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStripMainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.labelWorkingEnvironment);
            this.groupBox2.Controls.Add(this.labelMetadataRepository);
            this.groupBox2.Controls.Add(this.labelRepositoryDate);
            this.groupBox2.Controls.Add(this.labelRepositoryUpdateDateTime);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.labelRepositoryVersion);
            this.groupBox2.Location = new System.Drawing.Point(12, 577);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(299, 102);
            this.groupBox2.TabIndex = 22;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Repository version";
            // 
            // labelWorkingEnvironment
            // 
            this.labelWorkingEnvironment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWorkingEnvironment.AutoSize = true;
            this.labelWorkingEnvironment.Location = new System.Drawing.Point(6, 73);
            this.labelWorkingEnvironment.Name = "labelWorkingEnvironment";
            this.labelWorkingEnvironment.Size = new System.Drawing.Size(140, 13);
            this.labelWorkingEnvironment.TabIndex = 65;
            this.labelWorkingEnvironment.Text = "The working environment is:";
            // 
            // labelMetadataRepository
            // 
            this.labelMetadataRepository.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMetadataRepository.AutoSize = true;
            this.labelMetadataRepository.Location = new System.Drawing.Point(6, 57);
            this.labelMetadataRepository.Name = "labelMetadataRepository";
            this.labelMetadataRepository.Size = new System.Drawing.Size(200, 13);
            this.labelMetadataRepository.TabIndex = 64;
            this.labelMetadataRepository.Text = "Repository type in configuration is set to :";
            // 
            // labelRepositoryDate
            // 
            this.labelRepositoryDate.AutoSize = true;
            this.labelRepositoryDate.Location = new System.Drawing.Point(105, 41);
            this.labelRepositoryDate.Name = "labelRepositoryDate";
            this.labelRepositoryDate.Size = new System.Drawing.Size(27, 13);
            this.labelRepositoryDate.TabIndex = 21;
            this.labelRepositoryDate.Text = "N/A";
            // 
            // labelRepositoryUpdateDateTime
            // 
            this.labelRepositoryUpdateDateTime.AutoSize = true;
            this.labelRepositoryUpdateDateTime.Location = new System.Drawing.Point(6, 41);
            this.labelRepositoryUpdateDateTime.Name = "labelRepositoryUpdateDateTime";
            this.labelRepositoryUpdateDateTime.Size = new System.Drawing.Size(75, 13);
            this.labelRepositoryUpdateDateTime.TabIndex = 20;
            this.labelRepositoryUpdateDateTime.Text = "Latest update:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Repository version:";
            // 
            // labelRepositoryVersion
            // 
            this.labelRepositoryVersion.AutoSize = true;
            this.labelRepositoryVersion.Location = new System.Drawing.Point(105, 25);
            this.labelRepositoryVersion.Name = "labelRepositoryVersion";
            this.labelRepositoryVersion.Size = new System.Drawing.Size(27, 13);
            this.labelRepositoryVersion.TabIndex = 18;
            this.labelRepositoryVersion.Text = "N/A";
            // 
            // groupBoxVersionSelection
            // 
            this.groupBoxVersionSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxVersionSelection.Controls.Add(this.labelActiveVersion);
            this.groupBoxVersionSelection.Controls.Add(this.label1);
            this.groupBoxVersionSelection.Controls.Add(this.labelDocumentationVersion);
            this.groupBoxVersionSelection.Controls.Add(this.labelVersion);
            this.groupBoxVersionSelection.Location = new System.Drawing.Point(12, 684);
            this.groupBoxVersionSelection.Name = "groupBoxVersionSelection";
            this.groupBoxVersionSelection.Size = new System.Drawing.Size(299, 100);
            this.groupBoxVersionSelection.TabIndex = 20;
            this.groupBoxVersionSelection.TabStop = false;
            this.groupBoxVersionSelection.Text = "Version selection";
            // 
            // labelActiveVersion
            // 
            this.labelActiveVersion.AutoSize = true;
            this.labelActiveVersion.Location = new System.Drawing.Point(187, 41);
            this.labelActiveVersion.Name = "labelActiveVersion";
            this.labelActiveVersion.Size = new System.Drawing.Size(27, 13);
            this.labelActiveVersion.TabIndex = 21;
            this.labelActiveVersion.Text = "N/A";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(182, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Currently activated metadata version:";
            // 
            // labelDocumentationVersion
            // 
            this.labelDocumentationVersion.AutoSize = true;
            this.labelDocumentationVersion.Location = new System.Drawing.Point(6, 25);
            this.labelDocumentationVersion.Name = "labelDocumentationVersion";
            this.labelDocumentationVersion.Size = new System.Drawing.Size(150, 13);
            this.labelDocumentationVersion.TabIndex = 19;
            this.labelDocumentationVersion.Text = "Most recent metadata version:";
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(187, 25);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(27, 13);
            this.labelVersion.TabIndex = 18;
            this.labelVersion.Text = "N/A";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = global::TEAM.Properties.Resources.RavosLogo;
            this.pictureBox1.Location = new System.Drawing.Point(1027, 684);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(109, 100);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            // 
            // richTextBoxInformation
            // 
            this.richTextBoxInformation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxInformation.Location = new System.Drawing.Point(14, 59);
            this.richTextBoxInformation.Name = "richTextBoxInformation";
            this.richTextBoxInformation.ReadOnly = true;
            this.richTextBoxInformation.Size = new System.Drawing.Size(1122, 281);
            this.richTextBoxInformation.TabIndex = 2;
            this.richTextBoxInformation.Text = "";
            this.richTextBoxInformation.TextChanged += new System.EventHandler(this.richTextBoxInformation_TextChanged);
            this.richTextBoxInformation.Enter += new System.EventHandler(this.richTextBoxInformation_Enter);
            // 
            // menuStripMainMenu
            // 
            this.menuStripMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.metadataToolStripMenuItem,
            this.repositoryToolStripMenuItem,
            this.configurationToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStripMainMenu.Location = new System.Drawing.Point(0, 0);
            this.menuStripMainMenu.Name = "menuStripMainMenu";
            this.menuStripMainMenu.Size = new System.Drawing.Size(1148, 24);
            this.menuStripMainMenu.TabIndex = 4;
            this.menuStripMainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openOutputDirectoryToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openOutputDirectoryToolStripMenuItem
            // 
            this.openOutputDirectoryToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenDirectoryIcon;
            this.openOutputDirectoryToolStripMenuItem.Name = "openOutputDirectoryToolStripMenuItem";
            this.openOutputDirectoryToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openOutputDirectoryToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.openOutputDirectoryToolStripMenuItem.Text = "Open Output Directory";
            this.openOutputDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openOutputDirectoryToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::TEAM.Properties.Resources.ExitApplication;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.exitToolStripMenuItem.Text = "Exit Application";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // metadataToolStripMenuItem
            // 
            this.metadataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMetadataFormToolStripMenuItem});
            this.metadataToolStripMenuItem.Name = "metadataToolStripMenuItem";
            this.metadataToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
            this.metadataToolStripMenuItem.Text = "&Metadata";
            // 
            // openMetadataFormToolStripMenuItem
            // 
            this.openMetadataFormToolStripMenuItem.Image = global::TEAM.Properties.Resources.ETLIcon;
            this.openMetadataFormToolStripMenuItem.Name = "openMetadataFormToolStripMenuItem";
            this.openMetadataFormToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.openMetadataFormToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.openMetadataFormToolStripMenuItem.Text = "Manage &Metadata";
            this.openMetadataFormToolStripMenuItem.Click += new System.EventHandler(this.openMetadataFormToolStripMenuItem_Click);
            // 
            // configurationToolStripMenuItem
            // 
            this.configurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generalSettingsToolStripMenuItem});
            this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            this.configurationToolStripMenuItem.Size = new System.Drawing.Size(93, 20);
            this.configurationToolStripMenuItem.Text = "&Configuration";
            // 
            // generalSettingsToolStripMenuItem
            // 
            this.generalSettingsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("generalSettingsToolStripMenuItem.Image")));
            this.generalSettingsToolStripMenuItem.Name = "generalSettingsToolStripMenuItem";
            this.generalSettingsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.generalSettingsToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.generalSettingsToolStripMenuItem.Text = "General &Settings";
            this.generalSettingsToolStripMenuItem.Click += new System.EventHandler(this.generalSettingsToolStripMenuItem_Click);
            // 
            // repositoryToolStripMenuItem
            // 
            this.repositoryToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createRebuildRepositoryToolStripMenuItem1});
            this.repositoryToolStripMenuItem.Name = "repositoryToolStripMenuItem";
            this.repositoryToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.repositoryToolStripMenuItem.Text = "Repository";
            // 
            // createRebuildRepositoryToolStripMenuItem1
            // 
            this.createRebuildRepositoryToolStripMenuItem1.Image = global::TEAM.Properties.Resources.database_icon;
            this.createRebuildRepositoryToolStripMenuItem1.Name = "createRebuildRepositoryToolStripMenuItem1";
            this.createRebuildRepositoryToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.createRebuildRepositoryToolStripMenuItem1.Size = new System.Drawing.Size(259, 22);
            this.createRebuildRepositoryToolStripMenuItem1.Text = "&Create / Rebuild Repository";
            this.createRebuildRepositoryToolStripMenuItem1.Click += new System.EventHandler(this.createRebuildRepositoryToolStripMenuItem1_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.helpToolStripMenuItem.Text = "&About";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(104, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.ToolTipText = "Information about Virtual Enterprise Data Warehouse";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1148, 797);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBoxVersionSelection);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.richTextBoxInformation);
            this.Controls.Add(this.menuStripMainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStripMainMenu;
            this.MinimumSize = new System.Drawing.Size(1164, 835);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TEAM - Taxonomy for ETL Automation Metadata ";
            this.Activated += new System.EventHandler(this.FormMain_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBoxVersionSelection.ResumeLayout(false);
            this.groupBoxVersionSelection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStripMainMenu.ResumeLayout(false);
            this.menuStripMainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStripMainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openOutputDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem metadataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMetadataFormToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generalSettingsToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxVersionSelection;
        private System.Windows.Forms.Label labelDocumentationVersion;
        private System.Windows.Forms.Label labelVersion;
        internal System.Windows.Forms.RichTextBox richTextBoxInformation;
        private System.Windows.Forms.Label labelActiveVersion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label labelRepositoryDate;
        private System.Windows.Forms.Label labelRepositoryUpdateDateTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelRepositoryVersion;
        private System.Windows.Forms.Label labelMetadataRepository;
        private System.Windows.Forms.Label labelWorkingEnvironment;
        private System.Windows.Forms.ToolStripMenuItem repositoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createRebuildRepositoryToolStripMenuItem1;
    }
}

