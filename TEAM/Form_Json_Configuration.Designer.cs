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
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxDataItems = new System.Windows.Forms.GroupBox();
            this.checkBoxDataItemAddParentDataObject = new System.Windows.Forms.CheckBox();
            this.checkBoxDataItemDataType = new System.Windows.Forms.CheckBox();
            this.labelInformation = new System.Windows.Forms.Label();
            this.richTextBoxJsonExportInformation = new System.Windows.Forms.RichTextBox();
            this.toolTipJsonExtractConfiguration = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxSchemaExtension = new System.Windows.Forms.CheckBox();
            this.checkBoxDatabaseExtension = new System.Windows.Forms.CheckBox();
            this.checkBoxNextUpDataObjects = new System.Windows.Forms.CheckBox();
            this.checkBoxAddMetadataConnection = new System.Windows.Forms.CheckBox();
            this.checkBoxAddType = new System.Windows.Forms.CheckBox();
            this.checkBoxDataObjectDataItems = new System.Windows.Forms.CheckBox();
            this.groupBoxConnectivity = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBoxDataObjectConnections = new System.Windows.Forms.GroupBox();
            this.menuStripMainMenu.SuspendLayout();
            this.groupBoxDataItems.SuspendLayout();
            this.groupBoxConnectivity.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxDataObjectConnections.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStripMainMenu
            // 
            this.menuStripMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStripMainMenu.Location = new System.Drawing.Point(0, 0);
            this.menuStripMainMenu.Name = "menuStripMainMenu";
            this.menuStripMainMenu.Size = new System.Drawing.Size(701, 24);
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
            this.openConfigurationFileToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.openConfigurationFileToolStripMenuItem.Text = "Open JSON Extract Settings File";
            this.openConfigurationFileToolStripMenuItem.Click += new System.EventHandler(this.openConfigurationFileToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Image = global::TEAM.Properties.Resources.SaveFile;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.toolStripMenuItem2.Size = new System.Drawing.Size(274, 22);
            this.toolStripMenuItem2.Text = "Save JSON Extract Settings File";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.ToolStripMenuItemSaveSettings_Click);
            // 
            // openConfigurationDirectoryToolStripMenuItem
            // 
            this.openConfigurationDirectoryToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenDirectoryIcon;
            this.openConfigurationDirectoryToolStripMenuItem.Name = "openConfigurationDirectoryToolStripMenuItem";
            this.openConfigurationDirectoryToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.openConfigurationDirectoryToolStripMenuItem.Text = "Open Configuration Directory";
            this.openConfigurationDirectoryToolStripMenuItem.Click += new System.EventHandler(this.OpenConfigurationDirectoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(271, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::TEAM.Properties.Resources.ExitApplication;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.exitToolStripMenuItem.Text = "Close Window";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // groupBoxDataItems
            // 
            this.groupBoxDataItems.Controls.Add(this.checkBoxDataItemAddParentDataObject);
            this.groupBoxDataItems.Controls.Add(this.checkBoxDataItemDataType);
            this.groupBoxDataItems.Location = new System.Drawing.Point(238, 27);
            this.groupBoxDataItems.Name = "groupBoxDataItems";
            this.groupBoxDataItems.Size = new System.Drawing.Size(226, 106);
            this.groupBoxDataItems.TabIndex = 6;
            this.groupBoxDataItems.TabStop = false;
            this.groupBoxDataItems.Text = "Data Items";
            // 
            // checkBoxDataItemAddParentDataObject
            // 
            this.checkBoxDataItemAddParentDataObject.AutoSize = true;
            this.checkBoxDataItemAddParentDataObject.Checked = true;
            this.checkBoxDataItemAddParentDataObject.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDataItemAddParentDataObject.Location = new System.Drawing.Point(6, 42);
            this.checkBoxDataItemAddParentDataObject.Name = "checkBoxDataItemAddParentDataObject";
            this.checkBoxDataItemAddParentDataObject.Size = new System.Drawing.Size(138, 17);
            this.checkBoxDataItemAddParentDataObject.TabIndex = 11;
            this.checkBoxDataItemAddParentDataObject.Text = "Add parent Data Object";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxDataItemAddParentDataObject, "Enabling this option will add the parent data object to the available data items," +
        " but only in a reduced format without its data items.");
            this.checkBoxDataItemAddParentDataObject.UseVisualStyleBackColor = true;
            // 
            // checkBoxDataItemDataType
            // 
            this.checkBoxDataItemDataType.AutoSize = true;
            this.checkBoxDataItemDataType.Checked = true;
            this.checkBoxDataItemDataType.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDataItemDataType.Location = new System.Drawing.Point(6, 19);
            this.checkBoxDataItemDataType.Name = "checkBoxDataItemDataType";
            this.checkBoxDataItemDataType.Size = new System.Drawing.Size(97, 17);
            this.checkBoxDataItemDataType.TabIndex = 9;
            this.checkBoxDataItemDataType.Text = "Add data types";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxDataItemDataType, "Enabling this option will add the data types for the data items where available.");
            this.checkBoxDataItemDataType.UseVisualStyleBackColor = true;
            // 
            // labelInformation
            // 
            this.labelInformation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInformation.AutoSize = true;
            this.labelInformation.Location = new System.Drawing.Point(9, 264);
            this.labelInformation.Name = "labelInformation";
            this.labelInformation.Size = new System.Drawing.Size(59, 13);
            this.labelInformation.TabIndex = 28;
            this.labelInformation.Text = "Information";
            // 
            // richTextBoxJsonExportInformation
            // 
            this.richTextBoxJsonExportInformation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxJsonExportInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxJsonExportInformation.Location = new System.Drawing.Point(12, 280);
            this.richTextBoxJsonExportInformation.Name = "richTextBoxJsonExportInformation";
            this.richTextBoxJsonExportInformation.Size = new System.Drawing.Size(677, 192);
            this.richTextBoxJsonExportInformation.TabIndex = 27;
            this.richTextBoxJsonExportInformation.Text = "";
            // 
            // checkBoxSchemaExtension
            // 
            this.checkBoxSchemaExtension.AutoSize = true;
            this.checkBoxSchemaExtension.Checked = true;
            this.checkBoxSchemaExtension.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSchemaExtension.Location = new System.Drawing.Point(6, 42);
            this.checkBoxSchemaExtension.Name = "checkBoxSchemaExtension";
            this.checkBoxSchemaExtension.Size = new System.Drawing.Size(148, 17);
            this.checkBoxSchemaExtension.TabIndex = 28;
            this.checkBoxSchemaExtension.Text = "Add schema as Extension";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxSchemaExtension, "Enabling this option will add the schema name as an extension to the connection d" +
        "etails (key or token), for all data objects.\r\n");
            this.checkBoxSchemaExtension.UseVisualStyleBackColor = true;
            // 
            // checkBoxDatabaseExtension
            // 
            this.checkBoxDatabaseExtension.AutoSize = true;
            this.checkBoxDatabaseExtension.Checked = true;
            this.checkBoxDatabaseExtension.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDatabaseExtension.Location = new System.Drawing.Point(6, 19);
            this.checkBoxDatabaseExtension.Name = "checkBoxDatabaseExtension";
            this.checkBoxDatabaseExtension.Size = new System.Drawing.Size(155, 17);
            this.checkBoxDatabaseExtension.TabIndex = 27;
            this.checkBoxDatabaseExtension.Text = "Add database as Extension";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxDatabaseExtension, "Enabling this option will add the database name as an extension to the connection" +
        " details (key or token), for all data objects.\r\n");
            this.checkBoxDatabaseExtension.UseVisualStyleBackColor = true;
            // 
            // checkBoxNextUpDataObjects
            // 
            this.checkBoxNextUpDataObjects.AutoSize = true;
            this.checkBoxNextUpDataObjects.Checked = true;
            this.checkBoxNextUpDataObjects.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxNextUpDataObjects.Location = new System.Drawing.Point(6, 42);
            this.checkBoxNextUpDataObjects.Name = "checkBoxNextUpDataObjects";
            this.checkBoxNextUpDataObjects.Size = new System.Drawing.Size(187, 17);
            this.checkBoxNextUpDataObjects.TabIndex = 26;
            this.checkBoxNextUpDataObjects.Text = "Add \'next-up\' related Data Objects";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxNextUpDataObjects, "This option will retrieve the related data objects that are closest (next) in the" +
        " lineage and add these as relatedDataObjects to the output Json file.");
            this.checkBoxNextUpDataObjects.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddMetadataConnection
            // 
            this.checkBoxAddMetadataConnection.AutoSize = true;
            this.checkBoxAddMetadataConnection.Checked = true;
            this.checkBoxAddMetadataConnection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddMetadataConnection.Location = new System.Drawing.Point(6, 19);
            this.checkBoxAddMetadataConnection.Name = "checkBoxAddMetadataConnection";
            this.checkBoxAddMetadataConnection.Size = new System.Drawing.Size(149, 17);
            this.checkBoxAddMetadataConnection.TabIndex = 25;
            this.checkBoxAddMetadataConnection.Text = "Add metadata Connection";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxAddMetadataConnection, "This option will add the metadata connection to the resulting Json dataObjectMapp" +
        "ingList file, as a relatedDataObject with the classification \'metadata\'.\r\n");
            this.checkBoxAddMetadataConnection.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddType
            // 
            this.checkBoxAddType.AutoSize = true;
            this.checkBoxAddType.Checked = true;
            this.checkBoxAddType.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddType.Location = new System.Drawing.Point(6, 19);
            this.checkBoxAddType.Name = "checkBoxAddType";
            this.checkBoxAddType.Size = new System.Drawing.Size(146, 17);
            this.checkBoxAddType.TabIndex = 29;
            this.checkBoxAddType.Text = "Add type as Classification";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxAddType, "Enabling this option add the classification as defined in the source-target mappi" +
        "ng. I.e. \'PSA\', or \'HUB\'.\r\n");
            this.checkBoxAddType.UseVisualStyleBackColor = true;
            // 
            // checkBoxDataObjectDataItems
            // 
            this.checkBoxDataObjectDataItems.AutoSize = true;
            this.checkBoxDataObjectDataItems.Checked = true;
            this.checkBoxDataObjectDataItems.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDataObjectDataItems.Location = new System.Drawing.Point(6, 42);
            this.checkBoxDataObjectDataItems.Name = "checkBoxDataObjectDataItems";
            this.checkBoxDataObjectDataItems.Size = new System.Drawing.Size(99, 17);
            this.checkBoxDataObjectDataItems.TabIndex = 30;
            this.checkBoxDataObjectDataItems.Text = "Add Data Items";
            this.toolTipJsonExtractConfiguration.SetToolTip(this.checkBoxDataObjectDataItems, "Enabling this option will add the data items to the data objects.");
            this.checkBoxDataObjectDataItems.UseVisualStyleBackColor = true;
            // 
            // groupBoxConnectivity
            // 
            this.groupBoxConnectivity.Controls.Add(this.checkBoxDataObjectDataItems);
            this.groupBoxConnectivity.Controls.Add(this.checkBoxAddType);
            this.groupBoxConnectivity.Location = new System.Drawing.Point(12, 27);
            this.groupBoxConnectivity.Name = "groupBoxConnectivity";
            this.groupBoxConnectivity.Size = new System.Drawing.Size(220, 106);
            this.groupBoxConnectivity.TabIndex = 29;
            this.groupBoxConnectivity.TabStop = false;
            this.groupBoxConnectivity.Text = "Data Objects";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxNextUpDataObjects);
            this.groupBox1.Controls.Add(this.checkBoxAddMetadataConnection);
            this.groupBox1.Location = new System.Drawing.Point(470, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(220, 106);
            this.groupBox1.TabIndex = 30;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Related Data Objects";
            // 
            // groupBoxDataObjectConnections
            // 
            this.groupBoxDataObjectConnections.Controls.Add(this.checkBoxDatabaseExtension);
            this.groupBoxDataObjectConnections.Controls.Add(this.checkBoxSchemaExtension);
            this.groupBoxDataObjectConnections.Location = new System.Drawing.Point(12, 139);
            this.groupBoxDataObjectConnections.Name = "groupBoxDataObjectConnections";
            this.groupBoxDataObjectConnections.Size = new System.Drawing.Size(220, 76);
            this.groupBoxDataObjectConnections.TabIndex = 31;
            this.groupBoxDataObjectConnections.TabStop = false;
            this.groupBoxDataObjectConnections.Text = "Connections";
            // 
            // FormJsonConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(701, 485);
            this.Controls.Add(this.groupBoxDataObjectConnections);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxConnectivity);
            this.Controls.Add(this.labelInformation);
            this.Controls.Add(this.richTextBoxJsonExportInformation);
            this.Controls.Add(this.groupBoxDataItems);
            this.Controls.Add(this.menuStripMainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(717, 524);
            this.Name = "FormJsonConfiguration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JSON export configuration";
            this.menuStripMainMenu.ResumeLayout(false);
            this.menuStripMainMenu.PerformLayout();
            this.groupBoxDataItems.ResumeLayout(false);
            this.groupBoxDataItems.PerformLayout();
            this.groupBoxConnectivity.ResumeLayout(false);
            this.groupBoxConnectivity.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxDataObjectConnections.ResumeLayout(false);
            this.groupBoxDataObjectConnections.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}