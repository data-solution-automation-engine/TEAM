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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManageValidation));
            menuStripMainMenu = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openConfigurationFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            openConfigurationDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            groupBoxExistenceChecks = new System.Windows.Forms.GroupBox();
            checkBoxDataItemExistence = new System.Windows.Forms.CheckBox();
            checkBoxSourceBusinessKeyExistence = new System.Windows.Forms.CheckBox();
            checkBoxDataObjectExistence = new System.Windows.Forms.CheckBox();
            checkBoxLinkKeyOrder = new System.Windows.Forms.CheckBox();
            checkBoxLogicalGroup = new System.Windows.Forms.CheckBox();
            labelInformation = new System.Windows.Forms.Label();
            richTextBoxInformation = new System.Windows.Forms.RichTextBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            checkBoxLinkCompletion = new System.Windows.Forms.CheckBox();
            checkBoxBusinessKeySyntaxValidation = new System.Windows.Forms.CheckBox();
            groupBoxDataVaultValidation = new System.Windows.Forms.GroupBox();
            checkBoxBasicDataVaultValidation = new System.Windows.Forms.CheckBox();
            toolTipValidation = new System.Windows.Forms.ToolTip(components);
            checkBoxDuplicateDataObjectMappings = new System.Windows.Forms.CheckBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            menuStripMainMenu.SuspendLayout();
            groupBoxExistenceChecks.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBoxDataVaultValidation.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStripMainMenu
            // 
            menuStripMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem });
            menuStripMainMenu.Location = new System.Drawing.Point(0, 0);
            menuStripMainMenu.Name = "menuStripMainMenu";
            menuStripMainMenu.Size = new System.Drawing.Size(712, 24);
            menuStripMainMenu.TabIndex = 5;
            menuStripMainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openConfigurationFileToolStripMenuItem, toolStripMenuItem2, openConfigurationDirectoryToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openConfigurationFileToolStripMenuItem
            // 
            openConfigurationFileToolStripMenuItem.Image = Properties.Resources.OpenFileIcon;
            openConfigurationFileToolStripMenuItem.Name = "openConfigurationFileToolStripMenuItem";
            openConfigurationFileToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            openConfigurationFileToolStripMenuItem.Text = "Open Validation Settings File";
            openConfigurationFileToolStripMenuItem.Click += openConfigurationFileToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Image = Properties.Resources.SaveFile;
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            toolStripMenuItem2.Size = new System.Drawing.Size(259, 22);
            toolStripMenuItem2.Text = "Save Validation Settings File";
            toolStripMenuItem2.Click += toolStripSaveValidationSettings_Click;
            // 
            // openConfigurationDirectoryToolStripMenuItem
            // 
            openConfigurationDirectoryToolStripMenuItem.Image = Properties.Resources.OpenDirectoryIcon;
            openConfigurationDirectoryToolStripMenuItem.Name = "openConfigurationDirectoryToolStripMenuItem";
            openConfigurationDirectoryToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            openConfigurationDirectoryToolStripMenuItem.Text = "Open Configuration Directory";
            openConfigurationDirectoryToolStripMenuItem.Click += openConfigurationDirectoryToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(256, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Image = Properties.Resources.ExitApplication;
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            exitToolStripMenuItem.Text = "Close Window";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // groupBoxExistenceChecks
            // 
            groupBoxExistenceChecks.Controls.Add(checkBoxDataItemExistence);
            groupBoxExistenceChecks.Controls.Add(checkBoxSourceBusinessKeyExistence);
            groupBoxExistenceChecks.Controls.Add(checkBoxDataObjectExistence);
            groupBoxExistenceChecks.Location = new System.Drawing.Point(12, 42);
            groupBoxExistenceChecks.Name = "groupBoxExistenceChecks";
            groupBoxExistenceChecks.Size = new System.Drawing.Size(226, 136);
            groupBoxExistenceChecks.TabIndex = 6;
            groupBoxExistenceChecks.TabStop = false;
            groupBoxExistenceChecks.Text = "Object existence";
            // 
            // checkBoxDataItemExistence
            // 
            checkBoxDataItemExistence.AutoSize = true;
            checkBoxDataItemExistence.Checked = true;
            checkBoxDataItemExistence.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxDataItemExistence.Location = new System.Drawing.Point(6, 42);
            checkBoxDataItemExistence.Name = "checkBoxDataItemExistence";
            checkBoxDataItemExistence.Size = new System.Drawing.Size(190, 17);
            checkBoxDataItemExistence.TabIndex = 26;
            checkBoxDataItemExistence.Text = "Data Items (sources and targets)";
            checkBoxDataItemExistence.UseVisualStyleBackColor = true;
            // 
            // checkBoxSourceBusinessKeyExistence
            // 
            checkBoxSourceBusinessKeyExistence.AutoSize = true;
            checkBoxSourceBusinessKeyExistence.Checked = true;
            checkBoxSourceBusinessKeyExistence.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxSourceBusinessKeyExistence.Location = new System.Drawing.Point(6, 65);
            checkBoxSourceBusinessKeyExistence.Name = "checkBoxSourceBusinessKeyExistence";
            checkBoxSourceBusinessKeyExistence.Size = new System.Drawing.Size(177, 17);
            checkBoxSourceBusinessKeyExistence.TabIndex = 25;
            checkBoxSourceBusinessKeyExistence.Text = "Source business key attribute";
            checkBoxSourceBusinessKeyExistence.UseVisualStyleBackColor = true;
            // 
            // checkBoxDataObjectExistence
            // 
            checkBoxDataObjectExistence.AutoSize = true;
            checkBoxDataObjectExistence.Checked = true;
            checkBoxDataObjectExistence.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxDataObjectExistence.Location = new System.Drawing.Point(6, 19);
            checkBoxDataObjectExistence.Name = "checkBoxDataObjectExistence";
            checkBoxDataObjectExistence.Size = new System.Drawing.Size(202, 17);
            checkBoxDataObjectExistence.TabIndex = 9;
            checkBoxDataObjectExistence.Text = "Data Objects (sources and targets)";
            checkBoxDataObjectExistence.UseVisualStyleBackColor = true;
            // 
            // checkBoxLinkKeyOrder
            // 
            checkBoxLinkKeyOrder.AutoSize = true;
            checkBoxLinkKeyOrder.Checked = true;
            checkBoxLinkKeyOrder.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxLinkKeyOrder.Location = new System.Drawing.Point(6, 65);
            checkBoxLinkKeyOrder.Name = "checkBoxLinkKeyOrder";
            checkBoxLinkKeyOrder.Size = new System.Drawing.Size(214, 17);
            checkBoxLinkKeyOrder.TabIndex = 10;
            checkBoxLinkKeyOrder.Text = "Link Key Order (Metadata vs Physical)";
            checkBoxLinkKeyOrder.UseVisualStyleBackColor = true;
            // 
            // checkBoxLogicalGroup
            // 
            checkBoxLogicalGroup.AutoSize = true;
            checkBoxLogicalGroup.Checked = true;
            checkBoxLogicalGroup.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxLogicalGroup.Location = new System.Drawing.Point(6, 42);
            checkBoxLogicalGroup.Name = "checkBoxLogicalGroup";
            checkBoxLogicalGroup.Size = new System.Drawing.Size(135, 17);
            checkBoxLogicalGroup.TabIndex = 9;
            checkBoxLogicalGroup.Text = "Logical group (batch)";
            checkBoxLogicalGroup.UseVisualStyleBackColor = true;
            // 
            // labelInformation
            // 
            labelInformation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            labelInformation.AutoSize = true;
            labelInformation.Location = new System.Drawing.Point(9, 325);
            labelInformation.Name = "labelInformation";
            labelInformation.Size = new System.Drawing.Size(68, 13);
            labelInformation.TabIndex = 28;
            labelInformation.Text = "Information";
            // 
            // richTextBoxInformation
            // 
            richTextBoxInformation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBoxInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richTextBoxInformation.Location = new System.Drawing.Point(12, 341);
            richTextBoxInformation.Name = "richTextBoxInformation";
            richTextBoxInformation.Size = new System.Drawing.Size(688, 131);
            richTextBoxInformation.TabIndex = 27;
            richTextBoxInformation.Text = "";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(checkBoxLinkCompletion);
            groupBox2.Controls.Add(checkBoxLinkKeyOrder);
            groupBox2.Controls.Add(checkBoxBusinessKeySyntaxValidation);
            groupBox2.Controls.Add(checkBoxLogicalGroup);
            groupBox2.Location = new System.Drawing.Point(244, 42);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(226, 136);
            groupBox2.TabIndex = 27;
            groupBox2.TabStop = false;
            groupBox2.Text = "Metadata consistency";
            // 
            // checkBoxLinkCompletion
            // 
            checkBoxLinkCompletion.AutoSize = true;
            checkBoxLinkCompletion.Checked = true;
            checkBoxLinkCompletion.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxLinkCompletion.Location = new System.Drawing.Point(6, 88);
            checkBoxLinkCompletion.Name = "checkBoxLinkCompletion";
            checkBoxLinkCompletion.Size = new System.Drawing.Size(108, 17);
            checkBoxLinkCompletion.TabIndex = 11;
            checkBoxLinkCompletion.Text = "Link completion";
            toolTipValidation.SetToolTip(checkBoxLinkCompletion, resources.GetString("checkBoxLinkCompletion.ToolTip"));
            checkBoxLinkCompletion.UseVisualStyleBackColor = true;
            // 
            // checkBoxBusinessKeySyntaxValidation
            // 
            checkBoxBusinessKeySyntaxValidation.AutoSize = true;
            checkBoxBusinessKeySyntaxValidation.Checked = true;
            checkBoxBusinessKeySyntaxValidation.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxBusinessKeySyntaxValidation.Location = new System.Drawing.Point(6, 19);
            checkBoxBusinessKeySyntaxValidation.Name = "checkBoxBusinessKeySyntaxValidation";
            checkBoxBusinessKeySyntaxValidation.Size = new System.Drawing.Size(179, 17);
            checkBoxBusinessKeySyntaxValidation.TabIndex = 9;
            checkBoxBusinessKeySyntaxValidation.Text = "Business Key syntax validation";
            checkBoxBusinessKeySyntaxValidation.UseVisualStyleBackColor = true;
            // 
            // groupBoxDataVaultValidation
            // 
            groupBoxDataVaultValidation.Controls.Add(checkBoxBasicDataVaultValidation);
            groupBoxDataVaultValidation.Location = new System.Drawing.Point(12, 184);
            groupBoxDataVaultValidation.Name = "groupBoxDataVaultValidation";
            groupBoxDataVaultValidation.Size = new System.Drawing.Size(688, 138);
            groupBoxDataVaultValidation.TabIndex = 28;
            groupBoxDataVaultValidation.TabStop = false;
            groupBoxDataVaultValidation.Text = "Modelling validation";
            // 
            // checkBoxBasicDataVaultValidation
            // 
            checkBoxBasicDataVaultValidation.AutoSize = true;
            checkBoxBasicDataVaultValidation.Checked = true;
            checkBoxBasicDataVaultValidation.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxBasicDataVaultValidation.Location = new System.Drawing.Point(6, 19);
            checkBoxBasicDataVaultValidation.Name = "checkBoxBasicDataVaultValidation";
            checkBoxBasicDataVaultValidation.Size = new System.Drawing.Size(250, 17);
            checkBoxBasicDataVaultValidation.TabIndex = 9;
            checkBoxBasicDataVaultValidation.Text = "Validate basic Data Vault attribute existence";
            checkBoxBasicDataVaultValidation.UseVisualStyleBackColor = true;
            // 
            // checkBoxDuplicateDataObjectMappings
            // 
            checkBoxDuplicateDataObjectMappings.AutoSize = true;
            checkBoxDuplicateDataObjectMappings.Checked = true;
            checkBoxDuplicateDataObjectMappings.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxDuplicateDataObjectMappings.Location = new System.Drawing.Point(6, 19);
            checkBoxDuplicateDataObjectMappings.Name = "checkBoxDuplicateDataObjectMappings";
            checkBoxDuplicateDataObjectMappings.Size = new System.Drawing.Size(130, 17);
            checkBoxDuplicateDataObjectMappings.TabIndex = 9;
            checkBoxDuplicateDataObjectMappings.Text = "Duplicate Mappings";
            toolTipValidation.SetToolTip(checkBoxDuplicateDataObjectMappings, "This validator checks for any full row duplicates for the data object and data item mappings.\r\n");
            checkBoxDuplicateDataObjectMappings.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(checkBoxDuplicateDataObjectMappings);
            groupBox1.Location = new System.Drawing.Point(476, 42);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(226, 136);
            groupBox1.TabIndex = 28;
            groupBox1.TabStop = false;
            groupBox1.Text = "Generic";
            // 
            // FormManageValidation
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(712, 485);
            Controls.Add(groupBox1);
            Controls.Add(groupBoxDataVaultValidation);
            Controls.Add(groupBox2);
            Controls.Add(labelInformation);
            Controls.Add(richTextBoxInformation);
            Controls.Add(groupBoxExistenceChecks);
            Controls.Add(menuStripMainMenu);
            Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "FormManageValidation";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Metadata validation";
            menuStripMainMenu.ResumeLayout(false);
            menuStripMainMenu.PerformLayout();
            groupBoxExistenceChecks.ResumeLayout(false);
            groupBoxExistenceChecks.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBoxDataVaultValidation.ResumeLayout(false);
            groupBoxDataVaultValidation.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxDuplicateDataObjectMappings;
    }
}