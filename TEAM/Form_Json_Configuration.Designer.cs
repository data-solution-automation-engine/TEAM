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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormJsonConfiguration));
            menuStripMainMenu = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openConfigurationFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            openConfigurationDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            groupBoxDataItems = new System.Windows.Forms.GroupBox();
            checkBoxDataItemAddParentDataObject = new System.Windows.Forms.CheckBox();
            checkBoxDataItemDataType = new System.Windows.Forms.CheckBox();
            labelInformation = new System.Windows.Forms.Label();
            richTextBoxJsonExportInformation = new System.Windows.Forms.RichTextBox();
            toolTipJsonExtractConfiguration = new System.Windows.Forms.ToolTip(components);
            checkBoxSchemaExtension = new System.Windows.Forms.CheckBox();
            checkBoxDatabaseExtension = new System.Windows.Forms.CheckBox();
            checkBoxNextUpDataObjects = new System.Windows.Forms.CheckBox();
            checkBoxAddMetadataConnection = new System.Windows.Forms.CheckBox();
            checkBoxAddType = new System.Windows.Forms.CheckBox();
            checkBoxDataObjectDataItems = new System.Windows.Forms.CheckBox();
            checkBoxAddParentDataObject = new System.Windows.Forms.CheckBox();
            checkBoxAddDrivingKeyAsExtension = new System.Windows.Forms.CheckBox();
            groupBoxConnectivity = new System.Windows.Forms.GroupBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBoxDataObjectConnections = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            menuStripMainMenu.SuspendLayout();
            groupBoxDataItems.SuspendLayout();
            groupBoxConnectivity.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBoxDataObjectConnections.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // menuStripMainMenu
            // 
            menuStripMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem });
            menuStripMainMenu.Location = new System.Drawing.Point(0, 0);
            menuStripMainMenu.Name = "menuStripMainMenu";
            menuStripMainMenu.Size = new System.Drawing.Size(701, 24);
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
            openConfigurationFileToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            openConfigurationFileToolStripMenuItem.Text = "Open JSON Extract Settings File";
            openConfigurationFileToolStripMenuItem.Click += openConfigurationFileToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Image = Properties.Resources.SaveFile;
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            toolStripMenuItem2.Size = new System.Drawing.Size(274, 22);
            toolStripMenuItem2.Text = "Save JSON Extract Settings File";
            toolStripMenuItem2.Click += ToolStripMenuItemSaveSettings_Click;
            // 
            // openConfigurationDirectoryToolStripMenuItem
            // 
            openConfigurationDirectoryToolStripMenuItem.Image = Properties.Resources.OpenDirectoryIcon;
            openConfigurationDirectoryToolStripMenuItem.Name = "openConfigurationDirectoryToolStripMenuItem";
            openConfigurationDirectoryToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            openConfigurationDirectoryToolStripMenuItem.Text = "Open Configuration Directory";
            openConfigurationDirectoryToolStripMenuItem.Click += OpenConfigurationDirectoryToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(271, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Image = Properties.Resources.ExitApplication;
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            exitToolStripMenuItem.Text = "Close Window";
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // groupBoxDataItems
            // 
            groupBoxDataItems.Controls.Add(checkBoxDataItemAddParentDataObject);
            groupBoxDataItems.Controls.Add(checkBoxDataItemDataType);
            groupBoxDataItems.Location = new System.Drawing.Point(238, 27);
            groupBoxDataItems.Name = "groupBoxDataItems";
            groupBoxDataItems.Size = new System.Drawing.Size(226, 121);
            groupBoxDataItems.TabIndex = 6;
            groupBoxDataItems.TabStop = false;
            groupBoxDataItems.Text = "Data Items";
            // 
            // checkBoxDataItemAddParentDataObject
            // 
            checkBoxDataItemAddParentDataObject.AutoSize = true;
            checkBoxDataItemAddParentDataObject.Checked = true;
            checkBoxDataItemAddParentDataObject.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxDataItemAddParentDataObject.Location = new System.Drawing.Point(6, 42);
            checkBoxDataItemAddParentDataObject.Name = "checkBoxDataItemAddParentDataObject";
            checkBoxDataItemAddParentDataObject.Size = new System.Drawing.Size(148, 17);
            checkBoxDataItemAddParentDataObject.TabIndex = 11;
            checkBoxDataItemAddParentDataObject.Text = "Add parent Data Object";
            toolTipJsonExtractConfiguration.SetToolTip(checkBoxDataItemAddParentDataObject, "Enabling this option will add the parent data object to the available data items, but only in a reduced format without its data items.");
            checkBoxDataItemAddParentDataObject.UseVisualStyleBackColor = true;
            // 
            // checkBoxDataItemDataType
            // 
            checkBoxDataItemDataType.AutoSize = true;
            checkBoxDataItemDataType.Checked = true;
            checkBoxDataItemDataType.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxDataItemDataType.Location = new System.Drawing.Point(6, 19);
            checkBoxDataItemDataType.Name = "checkBoxDataItemDataType";
            checkBoxDataItemDataType.Size = new System.Drawing.Size(103, 17);
            checkBoxDataItemDataType.TabIndex = 9;
            checkBoxDataItemDataType.Text = "Add data types";
            toolTipJsonExtractConfiguration.SetToolTip(checkBoxDataItemDataType, "Enabling this option will add the data types for the data items where available.");
            checkBoxDataItemDataType.UseVisualStyleBackColor = true;
            // 
            // labelInformation
            // 
            labelInformation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            labelInformation.AutoSize = true;
            labelInformation.Location = new System.Drawing.Point(9, 264);
            labelInformation.Name = "labelInformation";
            labelInformation.Size = new System.Drawing.Size(68, 13);
            labelInformation.TabIndex = 28;
            labelInformation.Text = "Information";
            // 
            // richTextBoxJsonExportInformation
            // 
            richTextBoxJsonExportInformation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBoxJsonExportInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richTextBoxJsonExportInformation.Location = new System.Drawing.Point(12, 280);
            richTextBoxJsonExportInformation.Name = "richTextBoxJsonExportInformation";
            richTextBoxJsonExportInformation.Size = new System.Drawing.Size(677, 192);
            richTextBoxJsonExportInformation.TabIndex = 27;
            richTextBoxJsonExportInformation.Text = "";
            // 
            // checkBoxSchemaExtension
            // 
            checkBoxSchemaExtension.AutoSize = true;
            checkBoxSchemaExtension.Checked = true;
            checkBoxSchemaExtension.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxSchemaExtension.Location = new System.Drawing.Point(6, 42);
            checkBoxSchemaExtension.Name = "checkBoxSchemaExtension";
            checkBoxSchemaExtension.Size = new System.Drawing.Size(155, 17);
            checkBoxSchemaExtension.TabIndex = 28;
            checkBoxSchemaExtension.Text = "Add schema as Extension";
            toolTipJsonExtractConfiguration.SetToolTip(checkBoxSchemaExtension, "Enabling this option will add the schema name as an extension to the connection details (key or token), for all data objects.\r\n");
            checkBoxSchemaExtension.UseVisualStyleBackColor = true;
            // 
            // checkBoxDatabaseExtension
            // 
            checkBoxDatabaseExtension.AutoSize = true;
            checkBoxDatabaseExtension.Checked = true;
            checkBoxDatabaseExtension.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxDatabaseExtension.Location = new System.Drawing.Point(6, 19);
            checkBoxDatabaseExtension.Name = "checkBoxDatabaseExtension";
            checkBoxDatabaseExtension.Size = new System.Drawing.Size(164, 17);
            checkBoxDatabaseExtension.TabIndex = 27;
            checkBoxDatabaseExtension.Text = "Add database as Extension";
            toolTipJsonExtractConfiguration.SetToolTip(checkBoxDatabaseExtension, "Enabling this option will add the database name as an extension to the connection details (key or token), for all data objects.\r\n");
            checkBoxDatabaseExtension.UseVisualStyleBackColor = true;
            // 
            // checkBoxNextUpDataObjects
            // 
            checkBoxNextUpDataObjects.AutoSize = true;
            checkBoxNextUpDataObjects.Checked = true;
            checkBoxNextUpDataObjects.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxNextUpDataObjects.Location = new System.Drawing.Point(6, 42);
            checkBoxNextUpDataObjects.Name = "checkBoxNextUpDataObjects";
            checkBoxNextUpDataObjects.Size = new System.Drawing.Size(204, 17);
            checkBoxNextUpDataObjects.TabIndex = 26;
            checkBoxNextUpDataObjects.Text = "Add 'next-up' related Data Objects";
            toolTipJsonExtractConfiguration.SetToolTip(checkBoxNextUpDataObjects, "This option will retrieve the related data objects that are closest (next) in the lineage and add these as relatedDataObjects to the output Json file.");
            checkBoxNextUpDataObjects.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddMetadataConnection
            // 
            checkBoxAddMetadataConnection.AutoSize = true;
            checkBoxAddMetadataConnection.Checked = true;
            checkBoxAddMetadataConnection.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxAddMetadataConnection.Location = new System.Drawing.Point(6, 19);
            checkBoxAddMetadataConnection.Name = "checkBoxAddMetadataConnection";
            checkBoxAddMetadataConnection.Size = new System.Drawing.Size(161, 17);
            checkBoxAddMetadataConnection.TabIndex = 25;
            checkBoxAddMetadataConnection.Text = "Add metadata Connection";
            toolTipJsonExtractConfiguration.SetToolTip(checkBoxAddMetadataConnection, "This option will add the metadata connection to the resulting Json dataObjectMappingList file, as a relatedDataObject with the classification 'metadata'.\r\n");
            checkBoxAddMetadataConnection.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddType
            // 
            checkBoxAddType.AutoSize = true;
            checkBoxAddType.Checked = true;
            checkBoxAddType.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxAddType.Location = new System.Drawing.Point(6, 19);
            checkBoxAddType.Name = "checkBoxAddType";
            checkBoxAddType.Size = new System.Drawing.Size(157, 17);
            checkBoxAddType.TabIndex = 29;
            checkBoxAddType.Text = "Add type as Classification";
            toolTipJsonExtractConfiguration.SetToolTip(checkBoxAddType, "Enabling this option add the classification as defined in the source-target mapping. I.e. 'PSA', or 'HUB'.\r\n");
            checkBoxAddType.UseVisualStyleBackColor = true;
            // 
            // checkBoxDataObjectDataItems
            // 
            checkBoxDataObjectDataItems.AutoSize = true;
            checkBoxDataObjectDataItems.Checked = true;
            checkBoxDataObjectDataItems.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxDataObjectDataItems.Location = new System.Drawing.Point(6, 42);
            checkBoxDataObjectDataItems.Name = "checkBoxDataObjectDataItems";
            checkBoxDataObjectDataItems.Size = new System.Drawing.Size(104, 17);
            checkBoxDataObjectDataItems.TabIndex = 30;
            checkBoxDataObjectDataItems.Text = "Add Data Items";
            toolTipJsonExtractConfiguration.SetToolTip(checkBoxDataObjectDataItems, "Enabling this option will add the data items to the data objects.");
            checkBoxDataObjectDataItems.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddParentDataObject
            // 
            checkBoxAddParentDataObject.AutoSize = true;
            checkBoxAddParentDataObject.Checked = true;
            checkBoxAddParentDataObject.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxAddParentDataObject.Location = new System.Drawing.Point(6, 65);
            checkBoxAddParentDataObject.Name = "checkBoxAddParentDataObject";
            checkBoxAddParentDataObject.Size = new System.Drawing.Size(193, 17);
            checkBoxAddParentDataObject.TabIndex = 27;
            checkBoxAddParentDataObject.Text = "Add 'parent' related Data Object";
            toolTipJsonExtractConfiguration.SetToolTip(checkBoxAddParentDataObject, "This option will retrieve the related data objects that conceptually are the parent, and add these as relatedDataObjects to the output Json file.\r\n\r\nThe parent is the entity the object references to.");
            checkBoxAddParentDataObject.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddDrivingKeyAsExtension
            // 
            checkBoxAddDrivingKeyAsExtension.AutoSize = true;
            checkBoxAddDrivingKeyAsExtension.Checked = true;
            checkBoxAddDrivingKeyAsExtension.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxAddDrivingKeyAsExtension.Location = new System.Drawing.Point(6, 19);
            checkBoxAddDrivingKeyAsExtension.Name = "checkBoxAddDrivingKeyAsExtension";
            checkBoxAddDrivingKeyAsExtension.Size = new System.Drawing.Size(174, 17);
            checkBoxAddDrivingKeyAsExtension.TabIndex = 28;
            checkBoxAddDrivingKeyAsExtension.Text = "Add Driving Key as Extension";
            toolTipJsonExtractConfiguration.SetToolTip(checkBoxAddDrivingKeyAsExtension, "This option will add an extension to the business keys segment to add the parent key as extension value.");
            checkBoxAddDrivingKeyAsExtension.UseVisualStyleBackColor = true;
            // 
            // groupBoxConnectivity
            // 
            groupBoxConnectivity.Controls.Add(checkBoxDataObjectDataItems);
            groupBoxConnectivity.Controls.Add(checkBoxAddType);
            groupBoxConnectivity.Location = new System.Drawing.Point(12, 27);
            groupBoxConnectivity.Name = "groupBoxConnectivity";
            groupBoxConnectivity.Size = new System.Drawing.Size(220, 121);
            groupBoxConnectivity.TabIndex = 29;
            groupBoxConnectivity.TabStop = false;
            groupBoxConnectivity.Text = "Data Objects";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(checkBoxAddParentDataObject);
            groupBox1.Controls.Add(checkBoxNextUpDataObjects);
            groupBox1.Controls.Add(checkBoxAddMetadataConnection);
            groupBox1.Location = new System.Drawing.Point(470, 27);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(220, 121);
            groupBox1.TabIndex = 30;
            groupBox1.TabStop = false;
            groupBox1.Text = "Related Data Objects";
            // 
            // groupBoxDataObjectConnections
            // 
            groupBoxDataObjectConnections.Controls.Add(checkBoxDatabaseExtension);
            groupBoxDataObjectConnections.Controls.Add(checkBoxSchemaExtension);
            groupBoxDataObjectConnections.Location = new System.Drawing.Point(12, 154);
            groupBoxDataObjectConnections.Name = "groupBoxDataObjectConnections";
            groupBoxDataObjectConnections.Size = new System.Drawing.Size(220, 107);
            groupBoxDataObjectConnections.TabIndex = 31;
            groupBoxDataObjectConnections.TabStop = false;
            groupBoxDataObjectConnections.Text = "Connections";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(checkBoxAddDrivingKeyAsExtension);
            groupBox2.Location = new System.Drawing.Point(238, 154);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(226, 107);
            groupBox2.TabIndex = 32;
            groupBox2.TabStop = false;
            groupBox2.Text = "Data Vault";
            // 
            // FormJsonConfiguration
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(701, 485);
            Controls.Add(groupBox2);
            Controls.Add(groupBoxDataObjectConnections);
            Controls.Add(groupBox1);
            Controls.Add(groupBoxConnectivity);
            Controls.Add(labelInformation);
            Controls.Add(richTextBoxJsonExportInformation);
            Controls.Add(groupBoxDataItems);
            Controls.Add(menuStripMainMenu);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(717, 524);
            Name = "FormJsonConfiguration";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "JSON export configuration";
            menuStripMainMenu.ResumeLayout(false);
            menuStripMainMenu.PerformLayout();
            groupBoxDataItems.ResumeLayout(false);
            groupBoxDataItems.PerformLayout();
            groupBoxConnectivity.ResumeLayout(false);
            groupBoxConnectivity.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBoxDataObjectConnections.ResumeLayout(false);
            groupBoxDataObjectConnections.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStripMainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openConfigurationFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxDataItems;
        private System.Windows.Forms.CheckBox checkBoxDataItemDataType;
        private System.Windows.Forms.Label labelInformation;
        private System.Windows.Forms.RichTextBox richTextBoxJsonExportInformation;
        private System.Windows.Forms.ToolStripMenuItem openConfigurationDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolTip toolTipJsonExtractConfiguration;
        private System.Windows.Forms.GroupBox groupBoxConnectivity;
        private System.Windows.Forms.CheckBox checkBoxSchemaExtension;
        private System.Windows.Forms.CheckBox checkBoxDatabaseExtension;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxNextUpDataObjects;
        private System.Windows.Forms.CheckBox checkBoxAddMetadataConnection;
        private System.Windows.Forms.CheckBox checkBoxAddType;
        private System.Windows.Forms.CheckBox checkBoxDataItemAddParentDataObject;
        private System.Windows.Forms.CheckBox checkBoxDataObjectDataItems;
        private System.Windows.Forms.GroupBox groupBoxDataObjectConnections;
        private System.Windows.Forms.CheckBox checkBoxAddParentDataObject;
        private System.Windows.Forms.CheckBox checkBoxAddDrivingKeyAsExtension;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}