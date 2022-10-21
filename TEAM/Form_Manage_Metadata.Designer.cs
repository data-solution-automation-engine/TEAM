using System.ComponentModel;

namespace TEAM
{
    partial class FormManageMetadata
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManageMetadata));
            this.backgroundWorkerMetadata = new System.ComponentModel.BackgroundWorker();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxFilterCriterion = new TEAM.CustomTimedTextBox();
            this.labelResult = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonSaveMetadataChanges = new System.Windows.Forms.Button();
            this.groupBoxMetadataCounts = new System.Windows.Forms.GroupBox();
            this.labelLsatCount = new System.Windows.Forms.Label();
            this.labelLnkCount = new System.Windows.Forms.Label();
            this.labelSatCount = new System.Windows.Forms.Label();
            this.labelHubCount = new System.Windows.Forms.Label();
            this.tabControlDataMappings = new System.Windows.Forms.TabControl();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkedListBoxReverseEngineeringAreas = new System.Windows.Forms.CheckedListBox();
            this.button2 = new System.Windows.Forms.Button();
            this.contextMenuStripModel = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.displayTableScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MetadataGenerationGroupBox = new System.Windows.Forms.GroupBox();
            this.checkBoxValidation = new System.Windows.Forms.CheckBox();
            this.labelInformation = new System.Windows.Forms.Label();
            this.richTextBoxInformation = new System.Windows.Forms.RichTextBox();
            this.menuStripMetadata = new System.Windows.Forms.MenuStrip();
            this.metadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openOutputDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigurationDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMetadataDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.businessKeyMetadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMetadataFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openAttributeMappingFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.automapDataItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.validationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageValidationRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.validateMetadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.jsonExportConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageJsonExportRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayEventLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTipMetadata = new System.Windows.Forms.ToolTip(this.components);
            this.backgroundWorkerValidationOnly = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorkerEventLog = new System.ComponentModel.BackgroundWorker();
            this.groupBoxPhysicalModel = new System.Windows.Forms.GroupBox();
            this.labelConnections = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.groupBoxMetadataCounts.SuspendLayout();
            this.MetadataGenerationGroupBox.SuspendLayout();
            this.menuStripMetadata.SuspendLayout();
            this.groupBoxPhysicalModel.SuspendLayout();
            this.SuspendLayout();
            // 
            // backgroundWorkerMetadata
            // 
            this.backgroundWorkerMetadata.WorkerReportsProgress = true;
            this.backgroundWorkerMetadata.WorkerSupportsCancellation = true;
            this.backgroundWorkerMetadata.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerMetadata_DoWorkMetadataActivation);
            this.backgroundWorkerMetadata.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerMetadata_ProgressChanged);
            this.backgroundWorkerMetadata.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerMetadata_RunWorkerCompleted);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.textBoxFilterCriterion);
            this.groupBox2.Location = new System.Drawing.Point(16, 739);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(225, 46);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Filter Criterion";
            // 
            // textBoxFilterCriterion
            // 
            this.textBoxFilterCriterion.DelayedTextChangedTimeout = 1000;
            this.textBoxFilterCriterion.Location = new System.Drawing.Point(6, 16);
            this.textBoxFilterCriterion.Name = "textBoxFilterCriterion";
            this.textBoxFilterCriterion.Size = new System.Drawing.Size(213, 20);
            this.textBoxFilterCriterion.TabIndex = 23;
            this.textBoxFilterCriterion.DelayedTextChanged += new System.EventHandler(this.TextBoxFilterCriterion_OnDelayedTextChanged);
            // 
            // labelResult
            // 
            this.labelResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelResult.AutoSize = true;
            this.labelResult.Location = new System.Drawing.Point(6, 136);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(38, 13);
            this.labelResult.TabIndex = 23;
            this.labelResult.Text = "Ready";
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Location = new System.Drawing.Point(9, 91);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(120, 40);
            this.buttonStart.TabIndex = 22;
            this.buttonStart.Text = "&Activate Metadata";
            this.toolTipMetadata.SetToolTip(this.buttonStart, "Activation of the metadata will process the available metadata and update the out" +
        "put JSON files.");
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.ButtonActivate_Click);
            // 
            // buttonSaveMetadataChanges
            // 
            this.buttonSaveMetadataChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveMetadataChanges.Location = new System.Drawing.Point(8, 45);
            this.buttonSaveMetadataChanges.Name = "buttonSaveMetadataChanges";
            this.buttonSaveMetadataChanges.Size = new System.Drawing.Size(120, 40);
            this.buttonSaveMetadataChanges.TabIndex = 1;
            this.buttonSaveMetadataChanges.Text = "&Save Metadata Changes";
            this.toolTipMetadata.SetToolTip(this.buttonSaveMetadataChanges, "Save the metadata (changes) to file.");
            this.buttonSaveMetadataChanges.UseVisualStyleBackColor = true;
            this.buttonSaveMetadataChanges.Click += new System.EventHandler(this.buttonSaveMetadata_Click);
            // 
            // groupBoxMetadataCounts
            // 
            this.groupBoxMetadataCounts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMetadataCounts.Controls.Add(this.labelLsatCount);
            this.groupBoxMetadataCounts.Controls.Add(this.labelLnkCount);
            this.groupBoxMetadataCounts.Controls.Add(this.labelSatCount);
            this.groupBoxMetadataCounts.Controls.Add(this.labelHubCount);
            this.groupBoxMetadataCounts.Location = new System.Drawing.Point(1383, 739);
            this.groupBoxMetadataCounts.Name = "groupBoxMetadataCounts";
            this.groupBoxMetadataCounts.Size = new System.Drawing.Size(141, 85);
            this.groupBoxMetadataCounts.TabIndex = 16;
            this.groupBoxMetadataCounts.TabStop = false;
            this.groupBoxMetadataCounts.Text = "This metadata contains:";
            // 
            // labelLsatCount
            // 
            this.labelLsatCount.AutoSize = true;
            this.labelLsatCount.Location = new System.Drawing.Point(6, 59);
            this.labelLsatCount.Name = "labelLsatCount";
            this.labelLsatCount.Size = new System.Drawing.Size(77, 13);
            this.labelLsatCount.TabIndex = 3;
            this.labelLsatCount.Text = "labelLsatCount";
            // 
            // labelLnkCount
            // 
            this.labelLnkCount.AutoSize = true;
            this.labelLnkCount.Location = new System.Drawing.Point(6, 46);
            this.labelLnkCount.Name = "labelLnkCount";
            this.labelLnkCount.Size = new System.Drawing.Size(75, 13);
            this.labelLnkCount.TabIndex = 2;
            this.labelLnkCount.Text = "labelLnkCount";
            // 
            // labelSatCount
            // 
            this.labelSatCount.AutoSize = true;
            this.labelSatCount.Location = new System.Drawing.Point(6, 33);
            this.labelSatCount.Name = "labelSatCount";
            this.labelSatCount.Size = new System.Drawing.Size(73, 13);
            this.labelSatCount.TabIndex = 1;
            this.labelSatCount.Text = "labelSatCount";
            // 
            // labelHubCount
            // 
            this.labelHubCount.AutoSize = true;
            this.labelHubCount.Location = new System.Drawing.Point(6, 20);
            this.labelHubCount.Name = "labelHubCount";
            this.labelHubCount.Size = new System.Drawing.Size(77, 13);
            this.labelHubCount.TabIndex = 0;
            this.labelHubCount.Text = "labelHubCount";
            // 
            // tabControlDataMappings
            // 
            this.tabControlDataMappings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlDataMappings.Location = new System.Drawing.Point(12, 27);
            this.tabControlDataMappings.Name = "tabControlDataMappings";
            this.tabControlDataMappings.SelectedIndex = 4;
            this.tabControlDataMappings.Size = new System.Drawing.Size(1364, 706);
            this.tabControlDataMappings.TabIndex = 15;
            // 
            // groupBox4
            // 
            this.groupBox4.Location = new System.Drawing.Point(0, 0);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(200, 100);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            // 
            // checkedListBoxReverseEngineeringAreas
            // 
            this.checkedListBoxReverseEngineeringAreas.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBoxReverseEngineeringAreas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBoxReverseEngineeringAreas.FormattingEnabled = true;
            this.checkedListBoxReverseEngineeringAreas.Location = new System.Drawing.Point(8, 85);
            this.checkedListBoxReverseEngineeringAreas.Name = "checkedListBoxReverseEngineeringAreas";
            this.checkedListBoxReverseEngineeringAreas.Size = new System.Drawing.Size(120, 420);
            this.checkedListBoxReverseEngineeringAreas.TabIndex = 0;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(5, 19);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(120, 40);
            this.button2.TabIndex = 20;
            this.button2.Text = "Reverse Engineer";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ReverseEngineerMetadataButtonClick);
            // 
            // contextMenuStripModel
            // 
            this.contextMenuStripModel.Name = "contextMenuStripModel";
            this.contextMenuStripModel.Size = new System.Drawing.Size(61, 4);
            // 
            // displayTableScriptToolStripMenuItem
            // 
            this.displayTableScriptToolStripMenuItem.Name = "displayTableScriptToolStripMenuItem";
            this.displayTableScriptToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.displayTableScriptToolStripMenuItem.Text = "Display table script";
            this.displayTableScriptToolStripMenuItem.Click += new System.EventHandler(this.displayTableScriptToolStripMenuItem_Click);
            // 
            // MetadataGenerationGroupBox
            // 
            this.MetadataGenerationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MetadataGenerationGroupBox.Controls.Add(this.checkBoxValidation);
            this.MetadataGenerationGroupBox.Controls.Add(this.buttonSaveMetadataChanges);
            this.MetadataGenerationGroupBox.Controls.Add(this.buttonStart);
            this.MetadataGenerationGroupBox.Controls.Add(this.labelResult);
            this.MetadataGenerationGroupBox.Location = new System.Drawing.Point(1383, 49);
            this.MetadataGenerationGroupBox.Name = "MetadataGenerationGroupBox";
            this.MetadataGenerationGroupBox.Size = new System.Drawing.Size(140, 161);
            this.MetadataGenerationGroupBox.TabIndex = 3;
            this.MetadataGenerationGroupBox.TabStop = false;
            this.MetadataGenerationGroupBox.Text = "Processing";
            // 
            // checkBoxValidation
            // 
            this.checkBoxValidation.AutoSize = true;
            this.checkBoxValidation.Checked = true;
            this.checkBoxValidation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxValidation.Location = new System.Drawing.Point(8, 22);
            this.checkBoxValidation.Name = "checkBoxValidation";
            this.checkBoxValidation.Size = new System.Drawing.Size(111, 17);
            this.checkBoxValidation.TabIndex = 10;
            this.checkBoxValidation.Text = "Validate metadata";
            this.checkBoxValidation.UseVisualStyleBackColor = true;
            // 
            // labelInformation
            // 
            this.labelInformation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInformation.AutoSize = true;
            this.labelInformation.Location = new System.Drawing.Point(247, 739);
            this.labelInformation.Name = "labelInformation";
            this.labelInformation.Size = new System.Drawing.Size(59, 13);
            this.labelInformation.TabIndex = 5;
            this.labelInformation.Text = "Information";
            // 
            // richTextBoxInformation
            // 
            this.richTextBoxInformation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxInformation.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBoxInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxInformation.Location = new System.Drawing.Point(247, 755);
            this.richTextBoxInformation.Name = "richTextBoxInformation";
            this.richTextBoxInformation.Size = new System.Drawing.Size(1122, 69);
            this.richTextBoxInformation.TabIndex = 2;
            this.richTextBoxInformation.Text = "";
            // 
            // menuStripMetadata
            // 
            this.menuStripMetadata.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStripMetadata.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.metadataToolStripMenuItem,
            this.businessKeyMetadataToolStripMenuItem,
            this.validationToolStripMenuItem,
            this.jsonExportConfigurationToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStripMetadata.Location = new System.Drawing.Point(0, 0);
            this.menuStripMetadata.Name = "menuStripMetadata";
            this.menuStripMetadata.Size = new System.Drawing.Size(1534, 24);
            this.menuStripMetadata.TabIndex = 3;
            this.menuStripMetadata.Text = "menuStrip1";
            // 
            // metadataToolStripMenuItem
            // 
            this.metadataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openOutputDirectoryToolStripMenuItem,
            this.openConfigurationDirectoryToolStripMenuItem,
            this.openMetadataDirectoryToolStripMenuItem,
            this.toolStripSeparator5,
            this.saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.metadataToolStripMenuItem.Name = "metadataToolStripMenuItem";
            this.metadataToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.metadataToolStripMenuItem.Text = "&File";
            // 
            // openOutputDirectoryToolStripMenuItem
            // 
            this.openOutputDirectoryToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenDirectoryIcon;
            this.openOutputDirectoryToolStripMenuItem.Name = "openOutputDirectoryToolStripMenuItem";
            this.openOutputDirectoryToolStripMenuItem.Size = new System.Drawing.Size(350, 22);
            this.openOutputDirectoryToolStripMenuItem.Text = "Open Output &Directory";
            this.openOutputDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openOutputDirectoryToolStripMenuItem_Click);
            // 
            // openConfigurationDirectoryToolStripMenuItem
            // 
            this.openConfigurationDirectoryToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenDirectoryIcon;
            this.openConfigurationDirectoryToolStripMenuItem.Name = "openConfigurationDirectoryToolStripMenuItem";
            this.openConfigurationDirectoryToolStripMenuItem.Size = new System.Drawing.Size(350, 22);
            this.openConfigurationDirectoryToolStripMenuItem.Text = "Open Configuration Directory";
            this.openConfigurationDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openConfigurationDirectoryToolStripMenuItem_Click);
            // 
            // openMetadataDirectoryToolStripMenuItem
            // 
            this.openMetadataDirectoryToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenDirectoryIcon;
            this.openMetadataDirectoryToolStripMenuItem.Name = "openMetadataDirectoryToolStripMenuItem";
            this.openMetadataDirectoryToolStripMenuItem.Size = new System.Drawing.Size(350, 22);
            this.openMetadataDirectoryToolStripMenuItem.Text = "Open Metadata Directory";
            this.openMetadataDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openMetadataDirectoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(347, 6);
            // 
            // saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem
            // 
            this.saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem.Image = global::TEAM.Properties.Resources.SaveFile;
            this.saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem.Name = "saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem";
            this.saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem.Size = new System.Drawing.Size(350, 22);
            this.saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem.Text = "&Save as Directional Graph Markup Language (DGML)";
            this.saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem.Click += new System.EventHandler(this.saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Image = global::TEAM.Properties.Resources.ExitApplication;
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(350, 22);
            this.closeToolStripMenuItem.Text = "&Close Window";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // businessKeyMetadataToolStripMenuItem
            // 
            this.businessKeyMetadataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMetadataFileToolStripMenuItem,
            this.openAttributeMappingFileToolStripMenuItem,
            this.toolStripSeparator1,
            this.toolStripSeparator4,
            this.toolStripMenuItem1,
            this.toolStripSeparator3,
            this.automapDataItemsToolStripMenuItem});
            this.businessKeyMetadataToolStripMenuItem.Name = "businessKeyMetadataToolStripMenuItem";
            this.businessKeyMetadataToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
            this.businessKeyMetadataToolStripMenuItem.Text = "Metadata";
            // 
            // openMetadataFileToolStripMenuItem
            // 
            this.openMetadataFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.openMetadataFileToolStripMenuItem.Name = "openMetadataFileToolStripMenuItem";
            this.openMetadataFileToolStripMenuItem.Size = new System.Drawing.Size(363, 22);
            this.openMetadataFileToolStripMenuItem.Text = "Import Data Object Mapping Grid File";
            this.openMetadataFileToolStripMenuItem.Click += new System.EventHandler(this.openMetadataFileToolStripMenuItem_Click_1);
            // 
            // openAttributeMappingFileToolStripMenuItem
            // 
            this.openAttributeMappingFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.openAttributeMappingFileToolStripMenuItem.Name = "openAttributeMappingFileToolStripMenuItem";
            this.openAttributeMappingFileToolStripMenuItem.Size = new System.Drawing.Size(363, 22);
            this.openAttributeMappingFileToolStripMenuItem.Text = "Import Data Item Mapping Grid File";
            this.openAttributeMappingFileToolStripMenuItem.Click += new System.EventHandler(this.openAttributeMappingFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(360, 6);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(360, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(363, 22);
            this.toolStripMenuItem1.Text = "Import Data Warehouse Automation Schema JSON File";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.ToolStripMenuItemImportDwhAutomationJsonFile_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(360, 6);
            // 
            // automapDataItemsToolStripMenuItem
            // 
            this.automapDataItemsToolStripMenuItem.Image = global::TEAM.Properties.Resources.CogIcon;
            this.automapDataItemsToolStripMenuItem.Name = "automapDataItemsToolStripMenuItem";
            this.automapDataItemsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.automapDataItemsToolStripMenuItem.Size = new System.Drawing.Size(363, 22);
            this.automapDataItemsToolStripMenuItem.Text = "Automap Data Items";
            this.automapDataItemsToolStripMenuItem.Click += new System.EventHandler(this.AutoMapDataItemsToolStripMenuItem_Click);
            // 
            // validationToolStripMenuItem
            // 
            this.validationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageValidationRulesToolStripMenuItem,
            this.validateMetadataToolStripMenuItem});
            this.validationToolStripMenuItem.Name = "validationToolStripMenuItem";
            this.validationToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.validationToolStripMenuItem.Text = "&Validation";
            // 
            // manageValidationRulesToolStripMenuItem
            // 
            this.manageValidationRulesToolStripMenuItem.Image = global::TEAM.Properties.Resources.DocumentationIcon;
            this.manageValidationRulesToolStripMenuItem.Name = "manageValidationRulesToolStripMenuItem";
            this.manageValidationRulesToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.manageValidationRulesToolStripMenuItem.Text = "Manage Validation Rules";
            this.manageValidationRulesToolStripMenuItem.Click += new System.EventHandler(this.manageValidationRulesToolStripMenuItem_Click);
            // 
            // validateMetadataToolStripMenuItem
            // 
            this.validateMetadataToolStripMenuItem.Image = global::TEAM.Properties.Resources.CogIcon;
            this.validateMetadataToolStripMenuItem.Name = "validateMetadataToolStripMenuItem";
            this.validateMetadataToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.validateMetadataToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.validateMetadataToolStripMenuItem.Text = "&Validate Metadata";
            this.validateMetadataToolStripMenuItem.Click += new System.EventHandler(this.validateMetadataToolStripMenuItem_Click);
            // 
            // jsonExportConfigurationToolStripMenuItem
            // 
            this.jsonExportConfigurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageJsonExportRulesToolStripMenuItem,
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem});
            this.jsonExportConfigurationToolStripMenuItem.Name = "jsonExportConfigurationToolStripMenuItem";
            this.jsonExportConfigurationToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.jsonExportConfigurationToolStripMenuItem.Text = "&JSON";
            // 
            // manageJsonExportRulesToolStripMenuItem
            // 
            this.manageJsonExportRulesToolStripMenuItem.Image = global::TEAM.Properties.Resources.DocumentationIcon;
            this.manageJsonExportRulesToolStripMenuItem.Name = "manageJsonExportRulesToolStripMenuItem";
            this.manageJsonExportRulesToolStripMenuItem.Size = new System.Drawing.Size(297, 22);
            this.manageJsonExportRulesToolStripMenuItem.Text = "Manage JSON Export Rules";
            this.manageJsonExportRulesToolStripMenuItem.Click += new System.EventHandler(this.manageJsonExportRulesToolStripMenuItem_Click);
            // 
            // generateJsonInterfaceFilesOnlyToolStripMenuItem
            // 
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem.Image = global::TEAM.Properties.Resources.CogIcon;
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem.Name = "generateJsonInterfaceFilesOnlyToolStripMenuItem";
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem.Size = new System.Drawing.Size(297, 22);
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem.Text = "&Generate JSON Interface Files Only";
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem.Click += new System.EventHandler(this.generateJsonInterfaceFilesOnlyToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displayEventLogToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // displayEventLogToolStripMenuItem
            // 
            this.displayEventLogToolStripMenuItem.Image = global::TEAM.Properties.Resources.log_file;
            this.displayEventLogToolStripMenuItem.Name = "displayEventLogToolStripMenuItem";
            this.displayEventLogToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.displayEventLogToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.displayEventLogToolStripMenuItem.Text = "Display &Event Log";
            this.displayEventLogToolStripMenuItem.Click += new System.EventHandler(this.displayEventLogToolStripMenuItem_Click);
            // 
            // backgroundWorkerValidationOnly
            // 
            this.backgroundWorkerValidationOnly.WorkerReportsProgress = true;
            this.backgroundWorkerValidationOnly.WorkerSupportsCancellation = true;
            this.backgroundWorkerValidationOnly.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerValidation_DoWork);
            this.backgroundWorkerValidationOnly.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerValidationOnly_ProgressChanged);
            this.backgroundWorkerValidationOnly.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerValidationOnly_RunWorkerCompleted);
            // 
            // backgroundWorkerEventLog
            // 
            this.backgroundWorkerEventLog.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerEventLog_DoWork);
            this.backgroundWorkerEventLog.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerEventLog_ProgressChanged);
            this.backgroundWorkerEventLog.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerEventLog_RunWorkerCompleted);
            // 
            // groupBoxPhysicalModel
            // 
            this.groupBoxPhysicalModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPhysicalModel.Controls.Add(this.labelConnections);
            this.groupBoxPhysicalModel.Controls.Add(this.checkedListBoxReverseEngineeringAreas);
            this.groupBoxPhysicalModel.Controls.Add(this.button2);
            this.groupBoxPhysicalModel.Location = new System.Drawing.Point(1386, 216);
            this.groupBoxPhysicalModel.Name = "groupBoxPhysicalModel";
            this.groupBoxPhysicalModel.Size = new System.Drawing.Size(138, 517);
            this.groupBoxPhysicalModel.TabIndex = 1;
            this.groupBoxPhysicalModel.TabStop = false;
            this.groupBoxPhysicalModel.Text = "Physical Model";
            // 
            // labelConnections
            // 
            this.labelConnections.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelConnections.AutoSize = true;
            this.labelConnections.Location = new System.Drawing.Point(6, 64);
            this.labelConnections.Name = "labelConnections";
            this.labelConnections.Size = new System.Drawing.Size(66, 13);
            this.labelConnections.TabIndex = 24;
            this.labelConnections.Text = "Connections";
            // 
            // FormManageMetadata
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1534, 836);
            this.Controls.Add(this.groupBoxPhysicalModel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBoxMetadataCounts);
            this.Controls.Add(this.tabControlDataMappings);
            this.Controls.Add(this.MetadataGenerationGroupBox);
            this.Controls.Add(this.labelInformation);
            this.Controls.Add(this.richTextBoxInformation);
            this.Controls.Add(this.menuStripMetadata);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStripMetadata;
            this.MinimumSize = new System.Drawing.Size(1550, 875);
            this.Name = "FormManageMetadata";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manage the automation metadata";
            this.Shown += new System.EventHandler(this.FormManageMetadata_Shown);
            this.ResizeEnd += new System.EventHandler(this.FormManageMetadata_SizeChanged);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBoxMetadataCounts.ResumeLayout(false);
            this.groupBoxMetadataCounts.PerformLayout();
            this.MetadataGenerationGroupBox.ResumeLayout(false);
            this.MetadataGenerationGroupBox.PerformLayout();
            this.menuStripMetadata.ResumeLayout(false);
            this.menuStripMetadata.PerformLayout();
            this.groupBoxPhysicalModel.ResumeLayout(false);
            this.groupBoxPhysicalModel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxInformation;
        private System.Windows.Forms.MenuStrip menuStripMetadata;
        private System.Windows.Forms.ToolStripMenuItem metadataToolStripMenuItem;
        private System.Windows.Forms.Label labelInformation;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.GroupBox MetadataGenerationGroupBox;
        private System.Windows.Forms.CheckBox checkBoxValidation;
        private System.Windows.Forms.ToolStripMenuItem validationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageValidationRulesToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControlDataMappings;
        //private DataGridViewDataItems dataGridViewAttributeMetadata;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private System.Windows.Forms.GroupBox groupBoxMetadataCounts;
        private System.Windows.Forms.Label labelHubCount;
        private System.Windows.Forms.Label labelLsatCount;
        private System.Windows.Forms.Label labelLnkCount;
        private System.Windows.Forms.Label labelSatCount;
        private System.Windows.Forms.Button buttonSaveMetadataChanges;
        private System.Windows.Forms.ToolStripMenuItem businessKeyMetadataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMetadataFileToolStripMenuItem;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Label labelResult;
        private System.ComponentModel.BackgroundWorker backgroundWorkerMetadata;
        private System.Windows.Forms.ToolStripMenuItem saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox2;
        private CustomTimedTextBox textBoxFilterCriterion;
        private System.Windows.Forms.ToolStripMenuItem openOutputDirectoryToolStripMenuItem;


        private System.Windows.Forms.Button button2;

        
        private System.Windows.Forms.ToolTip toolTipMetadata;
        private System.ComponentModel.BackgroundWorker backgroundWorkerValidationOnly;

        private System.Windows.Forms.ContextMenuStrip contextMenuStripModel;

        private System.Windows.Forms.ToolStripMenuItem displayTableScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openConfigurationDirectoryToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckedListBox checkedListBoxReverseEngineeringAreas;
        private System.Windows.Forms.ToolStripMenuItem openAttributeMappingFileToolStripMenuItem;
   

        private System.Windows.Forms.ToolStripMenuItem jsonExportConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageJsonExportRulesToolStripMenuItem;
        private BackgroundWorker backgroundWorkerEventLog;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayEventLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem validateMetadataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateJsonInterfaceFilesOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem automapDataItemsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openMetadataDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;


        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.GroupBox groupBoxPhysicalModel;
        private System.Windows.Forms.Label labelConnections;
    }
}