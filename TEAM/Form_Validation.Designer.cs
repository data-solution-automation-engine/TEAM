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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManageValidation));
            this.menuStripMainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigurationFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigurationDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxExistenceChecks = new System.Windows.Forms.GroupBox();
            this.checkBoxDataItemExistence = new System.Windows.Forms.CheckBox();
            this.checkBoxSourceBusinessKeyExistence = new System.Windows.Forms.CheckBox();
            this.checkBoxDataObjectExistence = new System.Windows.Forms.CheckBox();
            this.checkBoxLinkKeyOrder = new System.Windows.Forms.CheckBox();
            this.checkBoxLogicalGroup = new System.Windows.Forms.CheckBox();
            this.labelInformation = new System.Windows.Forms.Label();
            this.richTextBoxInformation = new System.Windows.Forms.RichTextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxLinkCompletion = new System.Windows.Forms.CheckBox();
            this.checkBoxBusinessKeySyntaxValidation = new System.Windows.Forms.CheckBox();
            this.groupBoxDataVaultValidation = new System.Windows.Forms.GroupBox();
            this.checkBoxBasicDataVaultValidation = new System.Windows.Forms.CheckBox();
            this.toolTipValidation = new System.Windows.Forms.ToolTip(this.components);
            this.menuStripMainMenu.SuspendLayout();
            this.groupBoxExistenceChecks.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBoxDataVaultValidation.SuspendLayout();
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
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openConfigurationFileToolStripMenuItem
            // 
            this.openConfigurationFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.openConfigurationFileToolStripMenuItem.Name = "openConfigurationFileToolStripMenuItem";
            this.openConfigurationFileToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.openConfigurationFileToolStripMenuItem.Text = "Open Validation Settings File";
            this.openConfigurationFileToolStripMenuItem.Click += new System.EventHandler(this.openConfigurationFileToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Image = global::TEAM.Properties.Resources.SaveFile;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.toolStripMenuItem2.Size = new System.Drawing.Size(259, 22);
            this.toolStripMenuItem2.Text = "Save Validation Settings File";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // openConfigurationDirectoryToolStripMenuItem
            // 
            this.openConfigurationDirectoryToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenDirectoryIcon;
            this.openConfigurationDirectoryToolStripMenuItem.Name = "openConfigurationDirectoryToolStripMenuItem";
            this.openConfigurationDirectoryToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.openConfigurationDirectoryToolStripMenuItem.Text = "Open Configuration Directory";
            this.openConfigurationDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openConfigurationDirectoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(256, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::TEAM.Properties.Resources.ExitApplication;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.exitToolStripMenuItem.Text = "Close Window";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // groupBoxExistenceChecks
            // 
            this.groupBoxExistenceChecks.Controls.Add(this.checkBoxDataItemExistence);
            this.groupBoxExistenceChecks.Controls.Add(this.checkBoxSourceBusinessKeyExistence);
            this.groupBoxExistenceChecks.Controls.Add(this.checkBoxDataObjectExistence);
            this.groupBoxExistenceChecks.Location = new System.Drawing.Point(12, 42);
            this.groupBoxExistenceChecks.Name = "groupBoxExistenceChecks";
            this.groupBoxExistenceChecks.Size = new System.Drawing.Size(226, 136);
            this.groupBoxExistenceChecks.TabIndex = 6;
            this.groupBoxExistenceChecks.TabStop = false;
            this.groupBoxExistenceChecks.Text = "Object existence";
            // 
            // checkBoxDataItemExistence
            // 
            this.checkBoxDataItemExistence.AutoSize = true;
            this.checkBoxDataItemExistence.Checked = true;
            this.checkBoxDataItemExistence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDataItemExistence.Location = new System.Drawing.Point(6, 42);
            this.checkBoxDataItemExistence.Name = "checkBoxDataItemExistence";
            this.checkBoxDataItemExistence.Size = new System.Drawing.Size(179, 17);
            this.checkBoxDataItemExistence.TabIndex = 26;
            this.checkBoxDataItemExistence.Text = "Data Items (sources and targets)";
            this.checkBoxDataItemExistence.UseVisualStyleBackColor = true;
            // 
            // checkBoxSourceBusinessKeyExistence
            // 
            this.checkBoxSourceBusinessKeyExistence.AutoSize = true;
            this.checkBoxSourceBusinessKeyExistence.Checked = true;
            this.checkBoxSourceBusinessKeyExistence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSourceBusinessKeyExistence.Location = new System.Drawing.Point(6, 65);
            this.checkBoxSourceBusinessKeyExistence.Name = "checkBoxSourceBusinessKeyExistence";
            this.checkBoxSourceBusinessKeyExistence.Size = new System.Drawing.Size(165, 17);
            this.checkBoxSourceBusinessKeyExistence.TabIndex = 25;
            this.checkBoxSourceBusinessKeyExistence.Text = "Source business key attribute";
            this.checkBoxSourceBusinessKeyExistence.UseVisualStyleBackColor = true;
            // 
            // checkBoxDataObjectExistence
            // 
            this.checkBoxDataObjectExistence.AutoSize = true;
            this.checkBoxDataObjectExistence.Checked = true;
            this.checkBoxDataObjectExistence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDataObjectExistence.Location = new System.Drawing.Point(6, 19);
            this.checkBoxDataObjectExistence.Name = "checkBoxDataObjectExistence";
            this.checkBoxDataObjectExistence.Size = new System.Drawing.Size(190, 17);
            this.checkBoxDataObjectExistence.TabIndex = 9;
            this.checkBoxDataObjectExistence.Text = "Data Objects (sources and targets)";
            this.checkBoxDataObjectExistence.UseVisualStyleBackColor = true;
            // 
            // checkBoxLinkKeyOrder
            // 
            this.checkBoxLinkKeyOrder.AutoSize = true;
            this.checkBoxLinkKeyOrder.Checked = true;
            this.checkBoxLinkKeyOrder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLinkKeyOrder.Location = new System.Drawing.Point(6, 65);
            this.checkBoxLinkKeyOrder.Name = "checkBoxLinkKeyOrder";
            this.checkBoxLinkKeyOrder.Size = new System.Drawing.Size(206, 17);
            this.checkBoxLinkKeyOrder.TabIndex = 10;
            this.checkBoxLinkKeyOrder.Text = "Link Key Order (Metadata vs Physical)";
            this.checkBoxLinkKeyOrder.UseVisualStyleBackColor = true;
            // 
            // checkBoxLogicalGroup
            // 
            this.checkBoxLogicalGroup.AutoSize = true;
            this.checkBoxLogicalGroup.Checked = true;
            this.checkBoxLogicalGroup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLogicalGroup.Location = new System.Drawing.Point(6, 42);
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
            this.richTextBoxInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxInformation.Location = new System.Drawing.Point(12, 403);
            this.richTextBoxInformation.Name = "richTextBoxInformation";
            this.richTextBoxInformation.Size = new System.Drawing.Size(697, 69);
            this.richTextBoxInformation.TabIndex = 27;
            this.richTextBoxInformation.Text = "";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxLinkCompletion);
            this.groupBox2.Controls.Add(this.checkBoxLinkKeyOrder);
            this.groupBox2.Controls.Add(this.checkBoxBusinessKeySyntaxValidation);
            this.groupBox2.Controls.Add(this.checkBoxLogicalGroup);
            this.groupBox2.Location = new System.Drawing.Point(244, 42);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(226, 136);
            this.groupBox2.TabIndex = 27;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Metadata consistency";
            // 
            // checkBoxLinkCompletion
            // 
            this.checkBoxLinkCompletion.AutoSize = true;
            this.checkBoxLinkCompletion.Checked = true;
            this.checkBoxLinkCompletion.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLinkCompletion.Location = new System.Drawing.Point(6, 88);
            this.checkBoxLinkCompletion.Name = "checkBoxLinkCompletion";
            this.checkBoxLinkCompletion.Size = new System.Drawing.Size(100, 17);
            this.checkBoxLinkCompletion.TabIndex = 11;
            this.checkBoxLinkCompletion.Text = "Link completion";
            this.toolTipValidation.SetToolTip(this.checkBoxLinkCompletion, resources.GetString("checkBoxLinkCompletion.ToolTip"));
            this.checkBoxLinkCompletion.UseVisualStyleBackColor = true;
            // 
            // checkBoxBusinessKeySyntaxValidation
            // 
            this.checkBoxBusinessKeySyntaxValidation.AutoSize = true;
            this.checkBoxBusinessKeySyntaxValidation.Checked = true;
            this.checkBoxBusinessKeySyntaxValidation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBusinessKeySyntaxValidation.Location = new System.Drawing.Point(6, 19);
            this.checkBoxBusinessKeySyntaxValidation.Name = "checkBoxBusinessKeySyntaxValidation";
            this.checkBoxBusinessKeySyntaxValidation.Size = new System.Drawing.Size(170, 17);
            this.checkBoxBusinessKeySyntaxValidation.TabIndex = 9;
            this.checkBoxBusinessKeySyntaxValidation.Text = "Business Key syntax validation";
            this.checkBoxBusinessKeySyntaxValidation.UseVisualStyleBackColor = true;
            // 
            // groupBoxDataVaultValidation
            // 
            this.groupBoxDataVaultValidation.Controls.Add(this.checkBoxBasicDataVaultValidation);
            this.groupBoxDataVaultValidation.Location = new System.Drawing.Point(12, 184);
            this.groupBoxDataVaultValidation.Name = "groupBoxDataVaultValidation";
            this.groupBoxDataVaultValidation.Size = new System.Drawing.Size(458, 136);
            this.groupBoxDataVaultValidation.TabIndex = 28;
            this.groupBoxDataVaultValidation.TabStop = false;
            this.groupBoxDataVaultValidation.Text = "Modelling validation";
            // 
            // checkBoxBasicDataVaultValidation
            // 
            this.checkBoxBasicDataVaultValidation.AutoSize = true;
            this.checkBoxBasicDataVaultValidation.Checked = true;
            this.checkBoxBasicDataVaultValidation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBasicDataVaultValidation.Location = new System.Drawing.Point(6, 19);
            this.checkBoxBasicDataVaultValidation.Name = "checkBoxBasicDataVaultValidation";
            this.checkBoxBasicDataVaultValidation.Size = new System.Drawing.Size(234, 17);
            this.checkBoxBasicDataVaultValidation.TabIndex = 9;
            this.checkBoxBasicDataVaultValidation.Text = "Validate basic Data Vault attribute existence";
            this.checkBoxBasicDataVaultValidation.UseVisualStyleBackColor = true;
            // 
            // FormManageValidation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(721, 485);
            this.Controls.Add(this.groupBoxDataVaultValidation);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.labelInformation);
            this.Controls.Add(this.richTextBoxInformation);
            this.Controls.Add(this.groupBoxExistenceChecks);
            this.Controls.Add(this.menuStripMainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormManageValidation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Metadata validation";
            this.menuStripMainMenu.ResumeLayout(false);
            this.menuStripMainMenu.PerformLayout();
            this.groupBoxExistenceChecks.ResumeLayout(false);
            this.groupBoxExistenceChecks.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBoxDataVaultValidation.ResumeLayout(false);
            this.groupBoxDataVaultValidation.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStripMainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openConfigurationFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxExistenceChecks;
        private System.Windows.Forms.CheckBox checkBoxSourceBusinessKeyExistence;
        private System.Windows.Forms.CheckBox checkBoxDataObjectExistence;
        private System.Windows.Forms.CheckBox checkBoxLinkKeyOrder;
        private System.Windows.Forms.CheckBox checkBoxLogicalGroup;
        private System.Windows.Forms.Label labelInformation;
        private System.Windows.Forms.RichTextBox richTextBoxInformation;
        private System.Windows.Forms.ToolStripMenuItem openConfigurationDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxBusinessKeySyntaxValidation;
        private System.Windows.Forms.CheckBox checkBoxDataItemExistence;
        private System.Windows.Forms.GroupBox groupBoxDataVaultValidation;
        private System.Windows.Forms.CheckBox checkBoxBasicDataVaultValidation;
        private System.Windows.Forms.CheckBox checkBoxLinkCompletion;
        private System.Windows.Forms.ToolTip toolTipValidation;
    }
}