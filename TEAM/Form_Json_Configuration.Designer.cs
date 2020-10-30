namespace TEAM
{
    partial class FormJsonConfiguration
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormJsonConfiguration));
            this.menuStripMainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigurationFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigurationDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openOutputDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxExistenceChecks = new System.Windows.Forms.GroupBox();
            this.checkBoxTargetConnectionKey = new System.Windows.Forms.CheckBox();
            this.checkBoxSourceConnectionKey = new System.Windows.Forms.CheckBox();
            this.checkBoxTargetDataType = new System.Windows.Forms.CheckBox();
            this.checkBoxSourceDataType = new System.Windows.Forms.CheckBox();
            this.labelInformation = new System.Windows.Forms.Label();
            this.richTextBoxInformation = new System.Windows.Forms.RichTextBox();
            this.toolTipJsonExtractConfiguration = new System.Windows.Forms.ToolTip(this.components);
            this.menuStripMainMenu.SuspendLayout();
            this.groupBoxExistenceChecks.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStripMainMenu
            // 
            this.menuStripMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStripMainMenu.Location = new System.Drawing.Point(0, 0);
            this.menuStripMainMenu.Name = "menuStripMainMenu";
            this.menuStripMainMenu.Size = new System.Drawing.Size(721, 24);
            this.menuStripMainMenu.TabIndex = 5;
            this.menuStripMainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openConfigurationFileToolStripMenuItem,
            this.toolStripMenuItem2,
            this.openConfigurationDirectoryToolStripMenuItem,
            this.toolStripSeparator1,
            this.openOutputDirectoryToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openConfigurationFileToolStripMenuItem
            // 
            this.openConfigurationFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.openConfigurationFileToolStripMenuItem.Name = "openConfigurationFileToolStripMenuItem";
            this.openConfigurationFileToolStripMenuItem.Size = new System.Drawing.Size(269, 22);
            this.openConfigurationFileToolStripMenuItem.Text = "Open Json Extract Settings File";
            this.openConfigurationFileToolStripMenuItem.Click += new System.EventHandler(this.openConfigurationFileToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Image = global::TEAM.Properties.Resources.SaveFile;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.toolStripMenuItem2.Size = new System.Drawing.Size(269, 22);
            this.toolStripMenuItem2.Text = "Save Json Extract Settings File";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItemSaveSettings_Click);
            // 
            // openConfigurationDirectoryToolStripMenuItem
            // 
            this.openConfigurationDirectoryToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenDirectoryIcon;
            this.openConfigurationDirectoryToolStripMenuItem.Name = "openConfigurationDirectoryToolStripMenuItem";
            this.openConfigurationDirectoryToolStripMenuItem.Size = new System.Drawing.Size(269, 22);
            this.openConfigurationDirectoryToolStripMenuItem.Text = "Open Configuration Directory";
            this.openConfigurationDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openConfigurationDirectoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(266, 6);
            // 
            // openOutputDirectoryToolStripMenuItem
            // 
            this.openOutputDirectoryToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenDirectoryIcon;
            this.openOutputDirectoryToolStripMenuItem.Name = "openOutputDirectoryToolStripMenuItem";
            this.openOutputDirectoryToolStripMenuItem.Size = new System.Drawing.Size(269, 22);
            this.openOutputDirectoryToolStripMenuItem.Text = "Open Output Directory";
            this.openOutputDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openOutputDirectoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(266, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::TEAM.Properties.Resources.ExitApplication;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(269, 22);
            this.exitToolStripMenuItem.Text = "Close Window";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // groupBoxExistenceChecks
            // 
            this.groupBoxExistenceChecks.Controls.Add(this.checkBoxTargetConnectionKey);
            this.groupBoxExistenceChecks.Controls.Add(this.checkBoxSourceConnectionKey);
            this.groupBoxExistenceChecks.Controls.Add(this.checkBoxTargetDataType);
            this.groupBoxExistenceChecks.Controls.Add(this.checkBoxSourceDataType);
            this.groupBoxExistenceChecks.Location = new System.Drawing.Point(12, 42);
            this.groupBoxExistenceChecks.Name = "groupBoxExistenceChecks";
            this.groupBoxExistenceChecks.Size = new System.Drawing.Size(226, 136);
            this.groupBoxExistenceChecks.TabIndex = 6;
            this.groupBoxExistenceChecks.TabStop = false;
            this.groupBoxExistenceChecks.Text = "Object generation";
            // 
            // checkBoxTargetConnectionKey
            // 
            this.checkBoxTargetConnectionKey.AutoSize = true;
            this.checkBoxTargetConnectionKey.Checked = true;
            this.checkBoxTargetConnectionKey.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTargetConnectionKey.Location = new System.Drawing.Point(6, 88);
            this.checkBoxTargetConnectionKey.Name = "checkBoxTargetConnectionKey";
            this.checkBoxTargetConnectionKey.Size = new System.Drawing.Size(133, 17);
            this.checkBoxTargetConnectionKey.TabIndex = 26;
            this.checkBoxTargetConnectionKey.Text = "Target connection key";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxTargetConnectionKey, "Enabling this option will generate the connection key for the target data object " +
        "(targetDataObject/dataConnection) in the target Json metadata file.");
            this.checkBoxTargetConnectionKey.UseVisualStyleBackColor = true;
            // 
            // checkBoxSourceConnectionKey
            // 
            this.checkBoxSourceConnectionKey.AutoSize = true;
            this.checkBoxSourceConnectionKey.Checked = true;
            this.checkBoxSourceConnectionKey.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSourceConnectionKey.Location = new System.Drawing.Point(6, 65);
            this.checkBoxSourceConnectionKey.Name = "checkBoxSourceConnectionKey";
            this.checkBoxSourceConnectionKey.Size = new System.Drawing.Size(136, 17);
            this.checkBoxSourceConnectionKey.TabIndex = 25;
            this.checkBoxSourceConnectionKey.Text = "Source connection key";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxSourceConnectionKey, "Enabling this option will generate the connection key for the source data object " +
        "(sourceDataObject/dataConnection) in the target Json metadata file.\r\n");
            this.checkBoxSourceConnectionKey.UseVisualStyleBackColor = true;
            // 
            // checkBoxTargetDataType
            // 
            this.checkBoxTargetDataType.AutoSize = true;
            this.checkBoxTargetDataType.Checked = true;
            this.checkBoxTargetDataType.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTargetDataType.Location = new System.Drawing.Point(6, 42);
            this.checkBoxTargetDataType.Name = "checkBoxTargetDataType";
            this.checkBoxTargetDataType.Size = new System.Drawing.Size(109, 17);
            this.checkBoxTargetDataType.TabIndex = 10;
            this.checkBoxTargetDataType.Text = "Target data types";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxTargetDataType, "Enabling this option will generate the data types for the target data item (targe" +
        "tDataItem object) in the target Json metadata file.\r\n");
            this.checkBoxTargetDataType.UseVisualStyleBackColor = true;
            // 
            // checkBoxSourceDataType
            // 
            this.checkBoxSourceDataType.AutoSize = true;
            this.checkBoxSourceDataType.Checked = true;
            this.checkBoxSourceDataType.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSourceDataType.Location = new System.Drawing.Point(6, 19);
            this.checkBoxSourceDataType.Name = "checkBoxSourceDataType";
            this.checkBoxSourceDataType.Size = new System.Drawing.Size(112, 17);
            this.checkBoxSourceDataType.TabIndex = 9;
            this.checkBoxSourceDataType.Text = "Source data types";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxSourceDataType, "Enabling this option will generate the data types for the source data item (sourc" +
        "eDataItem object) in the target Json metadata file.");
            this.checkBoxSourceDataType.UseVisualStyleBackColor = true;
            // 
            // labelInformation
            // 
            this.labelInformation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInformation.AutoSize = true;
            this.labelInformation.Location = new System.Drawing.Point(9, 387);
            this.labelInformation.Name = "labelInformation";
            this.labelInformation.Size = new System.Drawing.Size(59, 13);
            this.labelInformation.TabIndex = 28;
            this.labelInformation.Text = "Information";
            // 
            // richTextBoxInformation
            // 
            this.richTextBoxInformation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxInformation.Location = new System.Drawing.Point(12, 403);
            this.richTextBoxInformation.Name = "richTextBoxInformation";
            this.richTextBoxInformation.Size = new System.Drawing.Size(697, 69);
            this.richTextBoxInformation.TabIndex = 27;
            this.richTextBoxInformation.Text = "";
            // 
            // FormJsonConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(721, 485);
            this.Controls.Add(this.labelInformation);
            this.Controls.Add(this.richTextBoxInformation);
            this.Controls.Add(this.groupBoxExistenceChecks);
            this.Controls.Add(this.menuStripMainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormJsonConfiguration";
            this.Text = "Json export configuration";
            this.menuStripMainMenu.ResumeLayout(false);
            this.menuStripMainMenu.PerformLayout();
            this.groupBoxExistenceChecks.ResumeLayout(false);
            this.groupBoxExistenceChecks.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStripMainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openConfigurationFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem openOutputDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxExistenceChecks;
        private System.Windows.Forms.CheckBox checkBoxSourceConnectionKey;
        private System.Windows.Forms.CheckBox checkBoxSourceDataType;
        private System.Windows.Forms.Label labelInformation;
        private System.Windows.Forms.RichTextBox richTextBoxInformation;
        private System.Windows.Forms.ToolStripMenuItem openConfigurationDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.CheckBox checkBoxTargetConnectionKey;
        private System.Windows.Forms.ToolTip toolTipJsonExtractConfiguration;
        private System.Windows.Forms.CheckBox checkBoxTargetDataType;
    }
}