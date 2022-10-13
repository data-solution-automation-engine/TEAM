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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelVersion = new System.Windows.Forms.Label();
            this.trackBarVersioning = new System.Windows.Forms.TrackBar();
            this.groupBoxMetadataCounts = new System.Windows.Forms.GroupBox();
            this.labelLsatCount = new System.Windows.Forms.Label();
            this.labelLnkCount = new System.Windows.Forms.Label();
            this.labelSatCount = new System.Windows.Forms.Label();
            this.labelHubCount = new System.Windows.Forms.Label();
            this.tabControlDataMappings = new System.Windows.Forms.TabControl();
            this.tabPageDataObjectMapping = new System.Windows.Forms.TabPage();
            this.contextMenuStripTableMapping = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteThisRowFromTheGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPageDataItemMapping = new System.Windows.Forms.TabPage();
            this.dataGridViewAttributeMetadata = new TEAM.CustomDataGridViewAttribute();
            this.contextMenuStripAttributeMapping = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteThisRowFromTheGridToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPagePhysicalModel = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkedListBoxReverseEngineeringAreas = new System.Windows.Forms.CheckedListBox();
            this.button2 = new System.Windows.Forms.Button();
            this.dataGridViewPhysicalModelMetadata = new TEAM.CustomDataGridViewPhysicalModel();
            this.contextMenuStripModel = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.displayTableScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteThisRowFromTheGridToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();

            
            this.outputGroupBoxVersioning = new System.Windows.Forms.GroupBox();
            this.radioButtonMinorRelease = new System.Windows.Forms.RadioButton();
            this.radiobuttonMajorRelease = new System.Windows.Forms.RadioButton();
            this.radiobuttonNoVersionChange = new System.Windows.Forms.RadioButton();
            this.MetadataGenerationGroupBox = new System.Windows.Forms.GroupBox();
            this.checkBoxShowJsonOutput = new System.Windows.Forms.CheckBox();
            this.checkBoxSaveInterfaceToJson = new System.Windows.Forms.CheckBox();
            this.checkBoxValidation = new System.Windows.Forms.CheckBox();
            this.labelInformation = new System.Windows.Forms.Label();
            this.richTextBoxInformation = new System.Windows.Forms.RichTextBox();
            this.menuStripMetadata = new System.Windows.Forms.MenuStrip();
            this.metadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openOutputDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigurationDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.businessKeyMetadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMetadataFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveTableMappingAsJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openAttributeMappingFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAttributeMappingAsJSONToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.openPhysicalModelFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportPhysicalModelFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.checkBoxMergeFiles = new System.Windows.Forms.CheckBox();
            this.checkBoxBackupFiles = new System.Windows.Forms.CheckBox();
            this.checkBoxResizeDataGrid = new System.Windows.Forms.CheckBox();
            this.backgroundWorkerValidationOnly = new System.ComponentModel.BackgroundWorker();
            this.groupBoxJsonOptions = new System.Windows.Forms.GroupBox();
            this.backgroundWorkerEventLog = new System.ComponentModel.BackgroundWorker();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVersioning)).BeginInit();
            this.groupBoxMetadataCounts.SuspendLayout();
            this.tabControlDataMappings.SuspendLayout();
            this.tabPageDataObjectMapping.SuspendLayout();
            this.contextMenuStripTableMapping.SuspendLayout();
            this.tabPageDataItemMapping.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAttributeMetadata)).BeginInit();
            this.contextMenuStripAttributeMapping.SuspendLayout();
            this.tabPagePhysicalModel.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPhysicalModelMetadata)).BeginInit();
            this.contextMenuStripModel.SuspendLayout();
            this.outputGroupBoxVersioning.SuspendLayout();
            this.MetadataGenerationGroupBox.SuspendLayout();
            this.menuStripMetadata.SuspendLayout();
            this.groupBoxJsonOptions.SuspendLayout();
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
            this.groupBox2.Location = new System.Drawing.Point(16, 604);
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
            this.textBoxFilterCriterion.DelayedTextChanged += new System.EventHandler(this.textBoxFilterCriterion_OnDelayedTextChanged);
            // 
            // labelResult
            // 
            this.labelResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelResult.AutoSize = true;
            this.labelResult.Location = new System.Drawing.Point(6, 134);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(38, 13);
            this.labelResult.TabIndex = 23;
            this.labelResult.Text = "Ready";
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Location = new System.Drawing.Point(6, 91);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(120, 40);
            this.buttonStart.TabIndex = 22;
            this.buttonStart.Text = "&Activate Metadata";
            this.toolTipMetadata.SetToolTip(this.buttonStart, "Activation of the metadata will process / upload the selected version into the ac" +
        "tive tool (similar to the slides on the main screen). \r\n\r\nThis allows for testin" +
        "g and troubleshooting.");
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.ButtonActivate_Click);
            // 
            // buttonSaveMetadataChanges
            // 
            this.buttonSaveMetadataChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveMetadataChanges.Location = new System.Drawing.Point(6, 93);
            this.buttonSaveMetadataChanges.Name = "buttonSaveMetadataChanges";
            this.buttonSaveMetadataChanges.Size = new System.Drawing.Size(120, 40);
            this.buttonSaveMetadataChanges.TabIndex = 1;
            this.buttonSaveMetadataChanges.Text = "&Save Metadata Changes";
            this.toolTipMetadata.SetToolTip(this.buttonSaveMetadataChanges, "Save the metadata (changes) to file.");
            this.buttonSaveMetadataChanges.UseVisualStyleBackColor = true;
            this.buttonSaveMetadataChanges.Click += new System.EventHandler(this.buttonSaveMetadata_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.labelVersion);
            this.groupBox1.Controls.Add(this.trackBarVersioning);
            this.groupBox1.Location = new System.Drawing.Point(247, 604);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(191, 85);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Version selection";
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(9, 57);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(42, 13);
            this.labelVersion.TabIndex = 18;
            this.labelVersion.Text = "Version";
            // 
            // trackBarVersioning
            // 
            this.trackBarVersioning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarVersioning.Location = new System.Drawing.Point(4, 24);
            this.trackBarVersioning.Name = "trackBarVersioning";
            this.trackBarVersioning.Size = new System.Drawing.Size(157, 45);
            this.trackBarVersioning.TabIndex = 4;
            this.trackBarVersioning.ValueChanged += new System.EventHandler(this.trackBarVersioning_ValueChanged);
            // 
            // groupBoxMetadataCounts
            // 
            this.groupBoxMetadataCounts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMetadataCounts.Controls.Add(this.labelLsatCount);
            this.groupBoxMetadataCounts.Controls.Add(this.labelLnkCount);
            this.groupBoxMetadataCounts.Controls.Add(this.labelSatCount);
            this.groupBoxMetadataCounts.Controls.Add(this.labelHubCount);
            this.groupBoxMetadataCounts.Location = new System.Drawing.Point(1132, 604);
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
            this.tabControlDataMappings.Controls.Add(this.tabPageDataObjectMapping);
            this.tabControlDataMappings.Controls.Add(this.tabPageDataItemMapping);
            this.tabControlDataMappings.Controls.Add(this.tabPagePhysicalModel);
            this.tabControlDataMappings.Location = new System.Drawing.Point(12, 27);
            this.tabControlDataMappings.Name = "tabControlDataMappings";
            this.tabControlDataMappings.SelectedIndex = 0;
            this.tabControlDataMappings.Size = new System.Drawing.Size(1114, 571);
            this.tabControlDataMappings.TabIndex = 15;

            // 
            // contextMenuStripTableMapping
            // 
            this.contextMenuStripTableMapping.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStripTableMapping.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem,
            this.deleteThisRowFromTheGridToolStripMenuItem});
            this.contextMenuStripTableMapping.Name = "contextMenuStripTableMapping";
            this.contextMenuStripTableMapping.Size = new System.Drawing.Size(340, 48);
            // 
            // exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem
            // 
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem.Name = "exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem";
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem.Text = "Export this row as Source-to-Target interface JSON";
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem.Click += new System.EventHandler(this.ExportThisRowAsSourceToTargetInterfaceJSONToolStripMenuItem_Click);
            // 
            // deleteThisRowFromTheGridToolStripMenuItem
            // 
            this.deleteThisRowFromTheGridToolStripMenuItem.Name = "deleteThisRowFromTheGridToolStripMenuItem";
            this.deleteThisRowFromTheGridToolStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.deleteThisRowFromTheGridToolStripMenuItem.Text = "Delete this row from the grid";
            this.deleteThisRowFromTheGridToolStripMenuItem.Click += new System.EventHandler(this.deleteThisRowFromTableDataGridToolStripMenuItem_Click);
            // 
            // tabPageDataItemMapping
            // 
            this.tabPageDataItemMapping.Controls.Add(this.dataGridViewAttributeMetadata);
            this.tabPageDataItemMapping.Location = new System.Drawing.Point(4, 22);
            this.tabPageDataItemMapping.Name = "tabPageDataItemMapping";
            this.tabPageDataItemMapping.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDataItemMapping.Size = new System.Drawing.Size(1106, 545);
            this.tabPageDataItemMapping.TabIndex = 1;
            this.tabPageDataItemMapping.Text = "Data Item (Attribute) Mappings";
            this.tabPageDataItemMapping.UseVisualStyleBackColor = true;
            // 
            // dataGridViewAttributeMetadata
            // 
            this.dataGridViewAttributeMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewAttributeMetadata.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewAttributeMetadata.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAttributeMetadata.ContextMenuStrip = this.contextMenuStripAttributeMapping;
            this.dataGridViewAttributeMetadata.Location = new System.Drawing.Point(2, 3);
            this.dataGridViewAttributeMetadata.Name = "dataGridViewAttributeMetadata";
            this.dataGridViewAttributeMetadata.Size = new System.Drawing.Size(1101, 543);
            this.dataGridViewAttributeMetadata.TabIndex = 1;
            this.dataGridViewAttributeMetadata.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGridViewAttributeMetadataKeyDown);
            this.dataGridViewAttributeMetadata.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGridViewAttributeMetadata_MouseDown);
            // 
            // contextMenuStripAttributeMapping
            // 
            this.contextMenuStripAttributeMapping.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStripAttributeMapping.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteThisRowFromTheGridToolStripMenuItem1});
            this.contextMenuStripAttributeMapping.Name = "contextMenuStripAttributeMapping";
            this.contextMenuStripAttributeMapping.Size = new System.Drawing.Size(226, 26);
            // 
            // deleteThisRowFromTheGridToolStripMenuItem1
            // 
            this.deleteThisRowFromTheGridToolStripMenuItem1.Name = "deleteThisRowFromTheGridToolStripMenuItem1";
            this.deleteThisRowFromTheGridToolStripMenuItem1.Size = new System.Drawing.Size(225, 22);
            this.deleteThisRowFromTheGridToolStripMenuItem1.Text = "Delete this row from the grid";
            this.deleteThisRowFromTheGridToolStripMenuItem1.Click += new System.EventHandler(this.deleteThisRowFromTheGridToolStripMenuItem1_Click);
            // 
            // tabPagePhysicalModel
            // 
            this.tabPagePhysicalModel.BackColor = System.Drawing.Color.Transparent;
            this.tabPagePhysicalModel.Controls.Add(this.groupBox4);
            this.tabPagePhysicalModel.Controls.Add(this.dataGridViewPhysicalModelMetadata);
            this.tabPagePhysicalModel.Location = new System.Drawing.Point(4, 22);
            this.tabPagePhysicalModel.Name = "tabPagePhysicalModel";
            this.tabPagePhysicalModel.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePhysicalModel.Size = new System.Drawing.Size(1106, 545);
            this.tabPagePhysicalModel.TabIndex = 2;
            this.tabPagePhysicalModel.Text = "Physical Model Snapshot";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.checkedListBoxReverseEngineeringAreas);
            this.groupBox4.Controls.Add(this.button2);
            this.groupBox4.Location = new System.Drawing.Point(966, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(134, 533);
            this.groupBox4.TabIndex = 27;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Target area";
            // 
            // checkedListBoxReverseEngineeringAreas
            // 
            this.checkedListBoxReverseEngineeringAreas.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBoxReverseEngineeringAreas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBoxReverseEngineeringAreas.FormattingEnabled = true;
            this.checkedListBoxReverseEngineeringAreas.Location = new System.Drawing.Point(8, 76);
            this.checkedListBoxReverseEngineeringAreas.Name = "checkedListBoxReverseEngineeringAreas";
            this.checkedListBoxReverseEngineeringAreas.Size = new System.Drawing.Size(120, 420);
            this.checkedListBoxReverseEngineeringAreas.TabIndex = 0;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(8, 24);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(120, 40);
            this.button2.TabIndex = 20;
            this.button2.Text = "Reverse Engineer Model";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ReverseEngineerMetadataButtonClick);
            // 
            // dataGridViewPhysicalModelMetadata
            // 
            this.dataGridViewPhysicalModelMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewPhysicalModelMetadata.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewPhysicalModelMetadata.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPhysicalModelMetadata.ContextMenuStrip = this.contextMenuStripModel;
            this.dataGridViewPhysicalModelMetadata.Location = new System.Drawing.Point(2, 3);
            this.dataGridViewPhysicalModelMetadata.Name = "dataGridViewPhysicalModelMetadata";
            this.dataGridViewPhysicalModelMetadata.Size = new System.Drawing.Size(958, 543);
            this.dataGridViewPhysicalModelMetadata.TabIndex = 2;
            this.dataGridViewPhysicalModelMetadata.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGridViewPhysicalModelMetadataKeyDown);
            this.dataGridViewPhysicalModelMetadata.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGridViewModelMetadata_MouseDown);
            // 
            // contextMenuStripModel
            // 
            this.contextMenuStripModel.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStripModel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displayTableScriptToolStripMenuItem,
            this.deleteThisRowFromTheGridToolStripMenuItem2});
            this.contextMenuStripModel.Name = "contextMenuStripModel";
            this.contextMenuStripModel.Size = new System.Drawing.Size(226, 48);
            // 
            // displayTableScriptToolStripMenuItem
            // 
            this.displayTableScriptToolStripMenuItem.Name = "displayTableScriptToolStripMenuItem";
            this.displayTableScriptToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.displayTableScriptToolStripMenuItem.Text = "Display table script";
            this.displayTableScriptToolStripMenuItem.Click += new System.EventHandler(this.displayTableScriptToolStripMenuItem_Click);
            // 
            // deleteThisRowFromTheGridToolStripMenuItem2
            // 
            this.deleteThisRowFromTheGridToolStripMenuItem2.Name = "deleteThisRowFromTheGridToolStripMenuItem2";
            this.deleteThisRowFromTheGridToolStripMenuItem2.Size = new System.Drawing.Size(225, 22);
            this.deleteThisRowFromTheGridToolStripMenuItem2.Text = "Delete this row from the grid";
            this.deleteThisRowFromTheGridToolStripMenuItem2.Click += new System.EventHandler(this.deleteThisRowFromTheGridToolStripMenuItem2_Click);

            // 
            // outputGroupBoxVersioning
            // 
            this.outputGroupBoxVersioning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputGroupBoxVersioning.Controls.Add(this.radioButtonMinorRelease);
            this.outputGroupBoxVersioning.Controls.Add(this.radiobuttonMajorRelease);
            this.outputGroupBoxVersioning.Controls.Add(this.radiobuttonNoVersionChange);
            this.outputGroupBoxVersioning.Controls.Add(this.buttonSaveMetadataChanges);
            this.outputGroupBoxVersioning.Location = new System.Drawing.Point(1132, 44);
            this.outputGroupBoxVersioning.Name = "outputGroupBoxVersioning";
            this.outputGroupBoxVersioning.Size = new System.Drawing.Size(140, 152);
            this.outputGroupBoxVersioning.TabIndex = 2;
            this.outputGroupBoxVersioning.TabStop = false;
            this.outputGroupBoxVersioning.Text = "Saving / Versioning";
            // 
            // radioButtonMinorRelease
            // 
            this.radioButtonMinorRelease.AutoSize = true;
            this.radioButtonMinorRelease.Location = new System.Drawing.Point(7, 70);
            this.radioButtonMinorRelease.Name = "radioButtonMinorRelease";
            this.radioButtonMinorRelease.Size = new System.Drawing.Size(111, 17);
            this.radioButtonMinorRelease.TabIndex = 2;
            this.radioButtonMinorRelease.Text = "Minor release (0.x)";
            this.radioButtonMinorRelease.UseVisualStyleBackColor = true;
            // 
            // radiobuttonMajorRelease
            // 
            this.radiobuttonMajorRelease.AutoSize = true;
            this.radiobuttonMajorRelease.Location = new System.Drawing.Point(7, 47);
            this.radiobuttonMajorRelease.Name = "radiobuttonMajorRelease";
            this.radiobuttonMajorRelease.Size = new System.Drawing.Size(111, 17);
            this.radiobuttonMajorRelease.TabIndex = 1;
            this.radiobuttonMajorRelease.Text = "Major release (x.0)";
            this.radiobuttonMajorRelease.UseVisualStyleBackColor = true;
            // 
            // radiobuttonNoVersionChange
            // 
            this.radiobuttonNoVersionChange.AutoSize = true;
            this.radiobuttonNoVersionChange.Location = new System.Drawing.Point(7, 24);
            this.radiobuttonNoVersionChange.Name = "radiobuttonNoVersionChange";
            this.radiobuttonNoVersionChange.Size = new System.Drawing.Size(115, 17);
            this.radiobuttonNoVersionChange.TabIndex = 0;
            this.radiobuttonNoVersionChange.Text = "No version change";
            this.radiobuttonNoVersionChange.UseVisualStyleBackColor = true;
            // 
            // MetadataGenerationGroupBox
            // 
            this.MetadataGenerationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MetadataGenerationGroupBox.Controls.Add(this.checkBoxShowJsonOutput);
            this.MetadataGenerationGroupBox.Controls.Add(this.checkBoxSaveInterfaceToJson);
            this.MetadataGenerationGroupBox.Controls.Add(this.checkBoxValidation);
            this.MetadataGenerationGroupBox.Controls.Add(this.buttonStart);
            this.MetadataGenerationGroupBox.Controls.Add(this.labelResult);
            this.MetadataGenerationGroupBox.Location = new System.Drawing.Point(1132, 202);
            this.MetadataGenerationGroupBox.Name = "MetadataGenerationGroupBox";
            this.MetadataGenerationGroupBox.Size = new System.Drawing.Size(140, 155);
            this.MetadataGenerationGroupBox.TabIndex = 3;
            this.MetadataGenerationGroupBox.TabStop = false;
            this.MetadataGenerationGroupBox.Text = "Activation / Processing";
            // 
            // checkBoxShowJsonOutput
            // 
            this.checkBoxShowJsonOutput.AutoSize = true;
            this.checkBoxShowJsonOutput.Location = new System.Drawing.Point(8, 68);
            this.checkBoxShowJsonOutput.Name = "checkBoxShowJsonOutput";
            this.checkBoxShowJsonOutput.Size = new System.Drawing.Size(124, 17);
            this.checkBoxShowJsonOutput.TabIndex = 10;
            this.checkBoxShowJsonOutput.Text = "Display JSON output";
            this.checkBoxShowJsonOutput.UseVisualStyleBackColor = true;
            // 
            // checkBoxSaveInterfaceToJson
            // 
            this.checkBoxSaveInterfaceToJson.AutoSize = true;
            this.checkBoxSaveInterfaceToJson.Checked = true;
            this.checkBoxSaveInterfaceToJson.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSaveInterfaceToJson.Location = new System.Drawing.Point(8, 45);
            this.checkBoxSaveInterfaceToJson.Name = "checkBoxSaveInterfaceToJson";
            this.checkBoxSaveInterfaceToJson.Size = new System.Drawing.Size(116, 17);
            this.checkBoxSaveInterfaceToJson.TabIndex = 11;
            this.checkBoxSaveInterfaceToJson.Text = "Save JSON to disk";
            this.toolTipMetadata.SetToolTip(this.checkBoxSaveInterfaceToJson, "Check this option if loaded files (JSON or XML) are added to existing data.\r\n\r\nNo" +
        "t having this option checked will overwrite the mapping information for the sele" +
        "cted version (in the datagrid).");
            this.checkBoxSaveInterfaceToJson.UseVisualStyleBackColor = true;
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
            this.labelInformation.Location = new System.Drawing.Point(439, 603);
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
            this.richTextBoxInformation.Location = new System.Drawing.Point(444, 620);
            this.richTextBoxInformation.Name = "richTextBoxInformation";
            this.richTextBoxInformation.Size = new System.Drawing.Size(675, 69);
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
            this.menuStripMetadata.Size = new System.Drawing.Size(1284, 24);
            this.menuStripMetadata.TabIndex = 3;
            this.menuStripMetadata.Text = "menuStrip1";
            // 
            // metadataToolStripMenuItem
            // 
            this.metadataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openOutputDirectoryToolStripMenuItem,
            this.openConfigurationDirectoryToolStripMenuItem,
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
            this.saveTableMappingAsJSONToolStripMenuItem,
            this.toolStripSeparator1,
            this.openAttributeMappingFileToolStripMenuItem,
            this.saveAttributeMappingAsJSONToolStripMenuItem1,
            this.toolStripSeparator2,
            this.openPhysicalModelFileToolStripMenuItem,
            this.exportPhysicalModelFileToolStripMenuItem,
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
            // saveTableMappingAsJSONToolStripMenuItem
            // 
            this.saveTableMappingAsJSONToolStripMenuItem.Image = global::TEAM.Properties.Resources.SaveFile;
            this.saveTableMappingAsJSONToolStripMenuItem.Name = "saveTableMappingAsJSONToolStripMenuItem";
            this.saveTableMappingAsJSONToolStripMenuItem.Size = new System.Drawing.Size(363, 22);
            this.saveTableMappingAsJSONToolStripMenuItem.Text = "Export Data Object Mapping Grid File";
            this.saveTableMappingAsJSONToolStripMenuItem.Click += new System.EventHandler(this.saveTableMappingAsJSONToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(360, 6);
            // 
            // openAttributeMappingFileToolStripMenuItem
            // 
            this.openAttributeMappingFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.openAttributeMappingFileToolStripMenuItem.Name = "openAttributeMappingFileToolStripMenuItem";
            this.openAttributeMappingFileToolStripMenuItem.Size = new System.Drawing.Size(363, 22);
            this.openAttributeMappingFileToolStripMenuItem.Text = "Import Data Item Mapping Grid File";
            this.openAttributeMappingFileToolStripMenuItem.Click += new System.EventHandler(this.openAttributeMappingFileToolStripMenuItem_Click);
            // 
            // saveAttributeMappingAsJSONToolStripMenuItem1
            // 
            this.saveAttributeMappingAsJSONToolStripMenuItem1.Image = global::TEAM.Properties.Resources.SaveFile;
            this.saveAttributeMappingAsJSONToolStripMenuItem1.Name = "saveAttributeMappingAsJSONToolStripMenuItem1";
            this.saveAttributeMappingAsJSONToolStripMenuItem1.Size = new System.Drawing.Size(363, 22);
            this.saveAttributeMappingAsJSONToolStripMenuItem1.Text = "Export Data Item Mapping Grid File";
            this.saveAttributeMappingAsJSONToolStripMenuItem1.Click += new System.EventHandler(this.saveAttributeMappingAsJSONToolStripMenuItem1_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(360, 6);
            // 
            // openPhysicalModelFileToolStripMenuItem
            // 
            this.openPhysicalModelFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.openPhysicalModelFileToolStripMenuItem.Name = "openPhysicalModelFileToolStripMenuItem";
            this.openPhysicalModelFileToolStripMenuItem.Size = new System.Drawing.Size(363, 22);
            this.openPhysicalModelFileToolStripMenuItem.Text = "Import Physical Model Grid File";
            this.openPhysicalModelFileToolStripMenuItem.Click += new System.EventHandler(this.openPhysicalModelFileToolStripMenuItem_Click);
            // 
            // exportPhysicalModelFileToolStripMenuItem
            // 
            this.exportPhysicalModelFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.SaveFile;
            this.exportPhysicalModelFileToolStripMenuItem.Name = "exportPhysicalModelFileToolStripMenuItem";
            this.exportPhysicalModelFileToolStripMenuItem.Size = new System.Drawing.Size(363, 22);
            this.exportPhysicalModelFileToolStripMenuItem.Text = "Export Physical Model Grid File";
            this.exportPhysicalModelFileToolStripMenuItem.Click += new System.EventHandler(this.exportPhysicalModelFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(360, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.toolStripMenuItem1.Name = "toolstripMenuItemImportDwhAutomationJsonFile";
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
            this.validateMetadataToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
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
            // checkBoxMergeFiles
            // 
            this.checkBoxMergeFiles.AutoSize = true;
            this.checkBoxMergeFiles.Location = new System.Drawing.Point(7, 19);
            this.checkBoxMergeFiles.Name = "checkBoxMergeFiles";
            this.checkBoxMergeFiles.Size = new System.Drawing.Size(113, 17);
            this.checkBoxMergeFiles.TabIndex = 9;
            this.checkBoxMergeFiles.Text = "Append to existing";
            this.toolTipMetadata.SetToolTip(this.checkBoxMergeFiles, "Check this option if loaded files (JSON or XML) are added to existing data.\r\n\r\nNo" +
        "t having this option checked will overwrite the mapping information for the sele" +
        "cted version (in the datagrid).");
            this.checkBoxMergeFiles.UseVisualStyleBackColor = true;
            // 
            // checkBoxBackupFiles
            // 
            this.checkBoxBackupFiles.AutoSize = true;
            this.checkBoxBackupFiles.Checked = true;
            this.checkBoxBackupFiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBackupFiles.Location = new System.Drawing.Point(7, 42);
            this.checkBoxBackupFiles.Name = "checkBoxBackupFiles";
            this.checkBoxBackupFiles.Size = new System.Drawing.Size(87, 17);
            this.checkBoxBackupFiles.TabIndex = 10;
            this.checkBoxBackupFiles.Text = "Auto backup";
            this.toolTipMetadata.SetToolTip(this.checkBoxBackupFiles, "Check this option to automatically create file backups when data changes.");
            this.checkBoxBackupFiles.UseVisualStyleBackColor = true;
            // 
            // checkBoxResizeDataGrid
            // 
            this.checkBoxResizeDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxResizeDataGrid.Checked = true;
            this.checkBoxResizeDataGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxResizeDataGrid.Location = new System.Drawing.Point(16, 656);
            this.checkBoxResizeDataGrid.Name = "checkBoxResizeDataGrid";
            this.checkBoxResizeDataGrid.Size = new System.Drawing.Size(108, 17);
            this.checkBoxResizeDataGrid.TabIndex = 27;
            this.checkBoxResizeDataGrid.Text = "Auto Resize Grid ";
            this.toolTipMetadata.SetToolTip(this.checkBoxResizeDataGrid, "Check this option if loaded files (JSON or XML) are added to existing data.\r\n\r\nNo" +
        "t having this option checked will overwrite the mapping information for the sele" +
        "cted version (in the datagrid).");
            this.checkBoxResizeDataGrid.UseVisualStyleBackColor = true;
            // 
            // backgroundWorkerValidationOnly
            // 
            this.backgroundWorkerValidationOnly.WorkerReportsProgress = true;
            this.backgroundWorkerValidationOnly.WorkerSupportsCancellation = true;
            this.backgroundWorkerValidationOnly.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerValidation_DoWork);
            this.backgroundWorkerValidationOnly.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerValidationOnly_ProgressChanged);
            this.backgroundWorkerValidationOnly.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerValidationOnly_RunWorkerCompleted);
            // 
            // groupBoxJsonOptions
            // 
            this.groupBoxJsonOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxJsonOptions.Controls.Add(this.checkBoxBackupFiles);
            this.groupBoxJsonOptions.Controls.Add(this.checkBoxMergeFiles);
            this.groupBoxJsonOptions.Location = new System.Drawing.Point(1133, 529);
            this.groupBoxJsonOptions.Name = "groupBoxJsonOptions";
            this.groupBoxJsonOptions.Size = new System.Drawing.Size(139, 66);
            this.groupBoxJsonOptions.TabIndex = 26;
            this.groupBoxJsonOptions.TabStop = false;
            this.groupBoxJsonOptions.Text = "JSON load options";
            // 
            // backgroundWorkerEventLog
            // 
            this.backgroundWorkerEventLog.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerEventLog_DoWork);
            this.backgroundWorkerEventLog.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerEventLog_ProgressChanged);
            this.backgroundWorkerEventLog.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerEventLog_RunWorkerCompleted);
            // 
            // FormManageMetadata
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 701);
            this.Controls.Add(this.checkBoxResizeDataGrid);
            this.Controls.Add(this.groupBoxJsonOptions);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxMetadataCounts);
            this.Controls.Add(this.tabControlDataMappings);
            this.Controls.Add(this.outputGroupBoxVersioning);
            this.Controls.Add(this.MetadataGenerationGroupBox);
            this.Controls.Add(this.labelInformation);
            this.Controls.Add(this.richTextBoxInformation);
            this.Controls.Add(this.menuStripMetadata);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStripMetadata;
            this.MinimumSize = new System.Drawing.Size(1278, 678);
            this.Name = "FormManageMetadata";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manage the automation metadata";
            this.Shown += new System.EventHandler(this.FormManageMetadata_Shown);
            this.SizeChanged += new System.EventHandler(this.FormManageMetadata_SizeChanged);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVersioning)).EndInit();
            this.groupBoxMetadataCounts.ResumeLayout(false);
            this.groupBoxMetadataCounts.PerformLayout();
            this.tabControlDataMappings.ResumeLayout(false);
            this.tabPageDataObjectMapping.ResumeLayout(false);
            this.contextMenuStripTableMapping.ResumeLayout(false);
            this.tabPageDataItemMapping.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAttributeMetadata)).EndInit();
            this.contextMenuStripAttributeMapping.ResumeLayout(false);
            this.tabPagePhysicalModel.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPhysicalModelMetadata)).EndInit();
            this.contextMenuStripModel.ResumeLayout(false);
            this.outputGroupBoxVersioning.ResumeLayout(false);
            this.outputGroupBoxVersioning.PerformLayout();
            this.MetadataGenerationGroupBox.ResumeLayout(false);
            this.MetadataGenerationGroupBox.PerformLayout();
            this.menuStripMetadata.ResumeLayout(false);
            this.menuStripMetadata.PerformLayout();
            this.groupBoxJsonOptions.ResumeLayout(false);
            this.groupBoxJsonOptions.PerformLayout();
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
        private System.Windows.Forms.GroupBox outputGroupBoxVersioning;
        private System.Windows.Forms.RadioButton radioButtonMinorRelease;
        private System.Windows.Forms.RadioButton radiobuttonMajorRelease;
        private System.Windows.Forms.RadioButton radiobuttonNoVersionChange;
        private System.Windows.Forms.TabControl tabControlDataMappings;
        private System.Windows.Forms.TabPage tabPageDataObjectMapping;
        private System.Windows.Forms.TabPage tabPageDataItemMapping;
        private CustomDataGridViewAttribute dataGridViewAttributeMetadata;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private System.Windows.Forms.GroupBox groupBoxMetadataCounts;
        private System.Windows.Forms.Label labelHubCount;
        private System.Windows.Forms.Label labelLsatCount;
        private System.Windows.Forms.Label labelLnkCount;
        private System.Windows.Forms.Label labelSatCount;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Button buttonSaveMetadataChanges;
        private System.Windows.Forms.ToolStripMenuItem businessKeyMetadataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMetadataFileToolStripMenuItem;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Label labelResult;
        private System.ComponentModel.BackgroundWorker backgroundWorkerMetadata;
        public System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.TrackBar trackBarVersioning;
        private System.Windows.Forms.ToolStripMenuItem saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox2;
        private CustomTimedTextBox textBoxFilterCriterion;
        private System.Windows.Forms.ToolStripMenuItem openOutputDirectoryToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPagePhysicalModel;
        private CustomDataGridViewPhysicalModel dataGridViewPhysicalModelMetadata;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTableMapping;
        private System.Windows.Forms.ToolStripMenuItem exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTipMetadata;
        private System.ComponentModel.BackgroundWorker backgroundWorkerValidationOnly;
        private System.Windows.Forms.GroupBox groupBoxJsonOptions;
        private System.Windows.Forms.CheckBox checkBoxMergeFiles;
        private System.Windows.Forms.CheckBox checkBoxBackupFiles;
        private System.Windows.Forms.CheckBox checkBoxResizeDataGrid;
        private System.Windows.Forms.CheckBox checkBoxSaveInterfaceToJson;
        private System.Windows.Forms.ToolStripMenuItem deleteThisRowFromTheGridToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAttributeMapping;
        private System.Windows.Forms.ToolStripMenuItem deleteThisRowFromTheGridToolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripModel;
        private System.Windows.Forms.ToolStripMenuItem deleteThisRowFromTheGridToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem displayTableScriptToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxShowJsonOutput;
        private System.Windows.Forms.ToolStripMenuItem openConfigurationDirectoryToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckedListBox checkedListBoxReverseEngineeringAreas;
 
        private System.Windows.Forms.ToolStripMenuItem saveTableMappingAsJSONToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openAttributeMappingFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAttributeMappingAsJSONToolStripMenuItem1;
   
        private System.Windows.Forms.ToolStripMenuItem openPhysicalModelFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportPhysicalModelFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jsonExportConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageJsonExportRulesToolStripMenuItem;
        private BackgroundWorker backgroundWorkerEventLog;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayEventLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem validateMetadataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateJsonInterfaceFilesOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem automapDataItemsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
    }
}