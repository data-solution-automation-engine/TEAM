namespace TEAM
{
    partial class FormManageValidation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManageValidation));
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
            this.checkBoxBusinessKeyExistence = new System.Windows.Forms.CheckBox();
            this.checkBoxTargetObjectExistence = new System.Windows.Forms.CheckBox();
            this.checkBoxSourceObjectExistence = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBoxLogicalGroup = new System.Windows.Forms.CheckBox();
            this.labelInformation = new System.Windows.Forms.Label();
            this.richTextBoxInformation = new System.Windows.Forms.RichTextBox();
            this.menuStripMainMenu.SuspendLayout();
            this.groupBoxExistenceChecks.SuspendLayout();
            this.groupBox1.SuspendLayout();
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
            this.openConfigurationFileToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.openConfigurationFileToolStripMenuItem.Text = "Open Validation Settings File";
            this.openConfigurationFileToolStripMenuItem.Click += new System.EventHandler(this.openConfigurationFileToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Image = global::TEAM.Properties.Resources.SaveFile;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(231, 22);
            this.toolStripMenuItem2.Text = "Save Validation Settings File";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // openConfigurationDirectoryToolStripMenuItem
            // 
            this.openConfigurationDirectoryToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenDirectoryIcon;
            this.openConfigurationDirectoryToolStripMenuItem.Name = "openConfigurationDirectoryToolStripMenuItem";
            this.openConfigurationDirectoryToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.openConfigurationDirectoryToolStripMenuItem.Text = "Open Configuration Directory";
            this.openConfigurationDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openConfigurationDirectoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(228, 6);
            // 
            // openOutputDirectoryToolStripMenuItem
            // 
            this.openOutputDirectoryToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenDirectoryIcon;
            this.openOutputDirectoryToolStripMenuItem.Name = "openOutputDirectoryToolStripMenuItem";
            this.openOutputDirectoryToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.openOutputDirectoryToolStripMenuItem.Text = "Open Output Directory";
            this.openOutputDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openOutputDirectoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(228, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::TEAM.Properties.Resources.ExitApplication;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // groupBoxExistenceChecks
            // 
            this.groupBoxExistenceChecks.Controls.Add(this.checkBoxBusinessKeyExistence);
            this.groupBoxExistenceChecks.Controls.Add(this.checkBoxTargetObjectExistence);
            this.groupBoxExistenceChecks.Controls.Add(this.checkBoxSourceObjectExistence);
            this.groupBoxExistenceChecks.Location = new System.Drawing.Point(12, 42);
            this.groupBoxExistenceChecks.Name = "groupBoxExistenceChecks";
            this.groupBoxExistenceChecks.Size = new System.Drawing.Size(226, 92);
            this.groupBoxExistenceChecks.TabIndex = 6;
            this.groupBoxExistenceChecks.TabStop = false;
            this.groupBoxExistenceChecks.Text = "Object existence";
            // 
            // checkBoxBusinessKeyExistence
            // 
            this.checkBoxBusinessKeyExistence.AutoSize = true;
            this.checkBoxBusinessKeyExistence.Checked = true;
            this.checkBoxBusinessKeyExistence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBusinessKeyExistence.Location = new System.Drawing.Point(6, 65);
            this.checkBoxBusinessKeyExistence.Name = "checkBoxBusinessKeyExistence";
            this.checkBoxBusinessKeyExistence.Size = new System.Drawing.Size(165, 17);
            this.checkBoxBusinessKeyExistence.TabIndex = 25;
            this.checkBoxBusinessKeyExistence.Text = "Source business key attribute";
            this.checkBoxBusinessKeyExistence.UseVisualStyleBackColor = true;
            // 
            // checkBoxTargetObjectExistence
            // 
            this.checkBoxTargetObjectExistence.AutoSize = true;
            this.checkBoxTargetObjectExistence.Checked = true;
            this.checkBoxTargetObjectExistence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTargetObjectExistence.Location = new System.Drawing.Point(6, 42);
            this.checkBoxTargetObjectExistence.Name = "checkBoxTargetObjectExistence";
            this.checkBoxTargetObjectExistence.Size = new System.Drawing.Size(89, 17);
            this.checkBoxTargetObjectExistence.TabIndex = 10;
            this.checkBoxTargetObjectExistence.Text = "Target object";
            this.checkBoxTargetObjectExistence.UseVisualStyleBackColor = true;
            // 
            // checkBoxSourceObjectExistence
            // 
            this.checkBoxSourceObjectExistence.AutoSize = true;
            this.checkBoxSourceObjectExistence.Checked = true;
            this.checkBoxSourceObjectExistence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSourceObjectExistence.Location = new System.Drawing.Point(6, 19);
            this.checkBoxSourceObjectExistence.Name = "checkBoxSourceObjectExistence";
            this.checkBoxSourceObjectExistence.Size = new System.Drawing.Size(92, 17);
            this.checkBoxSourceObjectExistence.TabIndex = 9;
            this.checkBoxSourceObjectExistence.Text = "Source object";
            this.checkBoxSourceObjectExistence.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox2);
            this.groupBox1.Controls.Add(this.checkBox3);
            this.groupBox1.Controls.Add(this.checkBoxLogicalGroup);
            this.groupBox1.Location = new System.Drawing.Point(12, 203);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(226, 92);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Consistency";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(6, 65);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(48, 17);
            this.checkBox2.TabIndex = 25;
            this.checkBox2.Text = "TBD";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Location = new System.Drawing.Point(6, 42);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(48, 17);
            this.checkBox3.TabIndex = 10;
            this.checkBox3.Text = "TBD";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBoxLogicalGroup
            // 
            this.checkBoxLogicalGroup.AutoSize = true;
            this.checkBoxLogicalGroup.Checked = true;
            this.checkBoxLogicalGroup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLogicalGroup.Location = new System.Drawing.Point(6, 19);
            this.checkBoxLogicalGroup.Name = "checkBoxLogicalGroup";
            this.checkBoxLogicalGroup.Size = new System.Drawing.Size(126, 17);
            this.checkBoxLogicalGroup.TabIndex = 9;
            this.checkBoxLogicalGroup.Text = "Logical group (batch)";
            this.checkBoxLogicalGroup.UseVisualStyleBackColor = true;
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
            // FormManageValidation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(721, 485);
            this.Controls.Add(this.labelInformation);
            this.Controls.Add(this.richTextBoxInformation);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxExistenceChecks);
            this.Controls.Add(this.menuStripMainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormManageValidation";
            this.Text = "Metadata validation";
            this.menuStripMainMenu.ResumeLayout(false);
            this.menuStripMainMenu.PerformLayout();
            this.groupBoxExistenceChecks.ResumeLayout(false);
            this.groupBoxExistenceChecks.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.CheckBox checkBoxBusinessKeyExistence;
        private System.Windows.Forms.CheckBox checkBoxTargetObjectExistence;
        private System.Windows.Forms.CheckBox checkBoxSourceObjectExistence;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBoxLogicalGroup;
        private System.Windows.Forms.Label labelInformation;
        private System.Windows.Forms.RichTextBox richTextBoxInformation;
        private System.Windows.Forms.ToolStripMenuItem openConfigurationDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}