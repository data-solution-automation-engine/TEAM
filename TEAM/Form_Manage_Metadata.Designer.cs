using System;

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
            this.textBoxFilterCriterion = new System.Windows.Forms.TextBox();
            this.buttonValidation = new System.Windows.Forms.Button();
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dataGridViewTableMetadata = new TEAM.CustomDataGridViewTable();
            this.contextMenuStripTableMapping = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridViewAttributeMetadata = new TEAM.CustomDataGridViewAttribute();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxIntegrationLayer = new System.Windows.Forms.CheckBox();
            this.checkBoxStagingLayer = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.dataGridViewPhysicalModelMetadata = new TEAM.CustomDataGridViewTable();
            this.outputGroupBoxVersioning = new System.Windows.Forms.GroupBox();
            this.radioButtonMinorRelease = new System.Windows.Forms.RadioButton();
            this.radiobuttonMajorRelease = new System.Windows.Forms.RadioButton();
            this.radiobuttonNoVersionChange = new System.Windows.Forms.RadioButton();
            this.MetadataGenerationGroupBox = new System.Windows.Forms.GroupBox();
            this.checkBoxIgnoreVersion = new System.Windows.Forms.CheckBox();
            this.checkBoxValidation = new System.Windows.Forms.CheckBox();
            this.checkBoxClearMetadata = new System.Windows.Forms.CheckBox();
            this.labelInformation = new System.Windows.Forms.Label();
            this.richTextBoxInformation = new System.Windows.Forms.RichTextBox();
            this.menuStripMetadata = new System.Windows.Forms.MenuStrip();
            this.metadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openOutputDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.businessKeyMetadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMetadataFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMetadataFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveTableMappingAsJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.attributeMappingMetadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAttributeMappingAsJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.physicalModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openModelMetadataFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveModelMetadataFileAsJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.validationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageValidationRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTipMetadata = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxMergeFiles = new System.Windows.Forms.CheckBox();
            this.checkBoxBackupFiles = new System.Windows.Forms.CheckBox();
            this.backgroundWorkerValidationOnly = new System.ComponentModel.BackgroundWorker();
            this.groupBoxJsonOptions = new System.Windows.Forms.GroupBox();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVersioning)).BeginInit();
            this.groupBoxMetadataCounts.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTableMetadata)).BeginInit();
            this.contextMenuStripTableMapping.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAttributeMetadata)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPhysicalModelMetadata)).BeginInit();
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
            this.groupBox2.Location = new System.Drawing.Point(22, 764);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(225, 46);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Filter Criterion";
            // 
            // textBoxFilterCriterion
            // 
            this.textBoxFilterCriterion.Location = new System.Drawing.Point(6, 16);
            this.textBoxFilterCriterion.Name = "textBoxFilterCriterion";
            this.textBoxFilterCriterion.Size = new System.Drawing.Size(213, 20);
            this.textBoxFilterCriterion.TabIndex = 23;
            this.textBoxFilterCriterion.TextChanged += new System.EventHandler(this.textBoxFilterCriterion_TextChanged);
            // 
            // buttonValidation
            // 
            this.buttonValidation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonValidation.Location = new System.Drawing.Point(1376, 329);
            this.buttonValidation.Name = "buttonValidation";
            this.buttonValidation.Size = new System.Drawing.Size(120, 40);
            this.buttonValidation.TabIndex = 24;
            this.buttonValidation.Text = "&Validate Only";
            this.buttonValidation.UseVisualStyleBackColor = true;
            this.buttonValidation.Click += new System.EventHandler(this.buttonValidation_Click);
            // 
            // labelResult
            // 
            this.labelResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelResult.AutoSize = true;
            this.labelResult.Location = new System.Drawing.Point(1249, 372);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(38, 13);
            this.labelResult.TabIndex = 23;
            this.labelResult.Text = "Ready";
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Location = new System.Drawing.Point(1252, 329);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(120, 40);
            this.buttonStart.TabIndex = 22;
            this.buttonStart.Text = "&Activate Metadata";
            this.toolTipMetadata.SetToolTip(this.buttonStart, "Activation of the metadata will process / upload the selected version into the ac" +
        "tive tool (similar to the slides on the main screen). \r\n\r\nThis allows for testin" +
        "g and troubleshooting.");
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonSaveMetadataChanges
            // 
            this.buttonSaveMetadataChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveMetadataChanges.Location = new System.Drawing.Point(1252, 149);
            this.buttonSaveMetadataChanges.Name = "buttonSaveMetadataChanges";
            this.buttonSaveMetadataChanges.Size = new System.Drawing.Size(120, 40);
            this.buttonSaveMetadataChanges.TabIndex = 1;
            this.buttonSaveMetadataChanges.Text = "&Save Metadata Changes";
            this.buttonSaveMetadataChanges.UseVisualStyleBackColor = true;
            this.buttonSaveMetadataChanges.Click += new System.EventHandler(this.buttonSubmitVersion_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.labelVersion);
            this.groupBox1.Controls.Add(this.trackBarVersioning);
            this.groupBox1.Location = new System.Drawing.Point(277, 764);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(352, 85);
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
            this.trackBarVersioning.Location = new System.Drawing.Point(6, 23);
            this.trackBarVersioning.Name = "trackBarVersioning";
            this.trackBarVersioning.Size = new System.Drawing.Size(338, 45);
            this.trackBarVersioning.TabIndex = 4;
            this.trackBarVersioning.Scroll += new System.EventHandler(this.trackBarVersioning_Scroll);
            this.trackBarVersioning.ValueChanged += new System.EventHandler(this.trackBarVersioning_ValueChanged);
            // 
            // groupBoxMetadataCounts
            // 
            this.groupBoxMetadataCounts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMetadataCounts.Controls.Add(this.labelLsatCount);
            this.groupBoxMetadataCounts.Controls.Add(this.labelLnkCount);
            this.groupBoxMetadataCounts.Controls.Add(this.labelSatCount);
            this.groupBoxMetadataCounts.Controls.Add(this.labelHubCount);
            this.groupBoxMetadataCounts.Location = new System.Drawing.Point(1252, 764);
            this.groupBoxMetadataCounts.Name = "groupBoxMetadataCounts";
            this.groupBoxMetadataCounts.Size = new System.Drawing.Size(244, 86);
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
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(16, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1230, 731);
            this.tabControl1.TabIndex = 15;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataGridViewTableMetadata);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1222, 705);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Table Mappings";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dataGridViewTableMetadata
            // 
            this.dataGridViewTableMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewTableMetadata.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTableMetadata.ContextMenuStrip = this.contextMenuStripTableMapping;
            this.dataGridViewTableMetadata.Location = new System.Drawing.Point(2, 3);
            this.dataGridViewTableMetadata.MinimumSize = new System.Drawing.Size(964, 511);
            this.dataGridViewTableMetadata.Name = "dataGridViewTableMetadata";
            this.dataGridViewTableMetadata.Size = new System.Drawing.Size(1217, 699);
            this.dataGridViewTableMetadata.TabIndex = 1;
            this.dataGridViewTableMetadata.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridViewTableMetadata_CellValidating);
            this.dataGridViewTableMetadata.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGridViewTableMetadataKeyDown);
            // 
            // contextMenuStripTableMapping
            // 
            this.contextMenuStripTableMapping.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem});
            this.contextMenuStripTableMapping.Name = "contextMenuStripTableMapping";
            this.contextMenuStripTableMapping.Size = new System.Drawing.Size(340, 26);
            // 
            // exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem
            // 
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem.Name = "exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem";
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem.Size = new System.Drawing.Size(339, 22);
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem.Text = "Export this row as Source-to-Target interface JSON";
            this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem.Click += new System.EventHandler(this.exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridViewAttributeMetadata);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1051, 622);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Attribute Mappings";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridViewAttributeMetadata
            // 
            this.dataGridViewAttributeMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewAttributeMetadata.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAttributeMetadata.Location = new System.Drawing.Point(2, 3);
            this.dataGridViewAttributeMetadata.Name = "dataGridViewAttributeMetadata";
            this.dataGridViewAttributeMetadata.Size = new System.Drawing.Size(975, 519);
            this.dataGridViewAttributeMetadata.TabIndex = 1;
            this.dataGridViewAttributeMetadata.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGridViewAttributeMetadataKeyDown);
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage3.Controls.Add(this.groupBox3);
            this.tabPage3.Controls.Add(this.button2);
            this.tabPage3.Controls.Add(this.dataGridViewPhysicalModelMetadata);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1051, 622);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Physical Model";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.checkBoxIntegrationLayer);
            this.groupBox3.Controls.Add(this.checkBoxStagingLayer);
            this.groupBox3.Location = new System.Drawing.Point(854, 55);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(120, 92);
            this.groupBox3.TabIndex = 26;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Target area";
            // 
            // checkBoxIntegrationLayer
            // 
            this.checkBoxIntegrationLayer.AutoSize = true;
            this.checkBoxIntegrationLayer.Checked = true;
            this.checkBoxIntegrationLayer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxIntegrationLayer.Location = new System.Drawing.Point(6, 42);
            this.checkBoxIntegrationLayer.Name = "checkBoxIntegrationLayer";
            this.checkBoxIntegrationLayer.Size = new System.Drawing.Size(105, 17);
            this.checkBoxIntegrationLayer.TabIndex = 10;
            this.checkBoxIntegrationLayer.Text = "Integration Layer";
            this.checkBoxIntegrationLayer.UseVisualStyleBackColor = true;
            // 
            // checkBoxStagingLayer
            // 
            this.checkBoxStagingLayer.AutoSize = true;
            this.checkBoxStagingLayer.Checked = true;
            this.checkBoxStagingLayer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxStagingLayer.Location = new System.Drawing.Point(6, 19);
            this.checkBoxStagingLayer.Name = "checkBoxStagingLayer";
            this.checkBoxStagingLayer.Size = new System.Drawing.Size(91, 17);
            this.checkBoxStagingLayer.TabIndex = 9;
            this.checkBoxStagingLayer.Text = "Staging Layer";
            this.checkBoxStagingLayer.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(854, 9);
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
            this.dataGridViewPhysicalModelMetadata.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPhysicalModelMetadata.Location = new System.Drawing.Point(2, 3);
            this.dataGridViewPhysicalModelMetadata.Name = "dataGridViewPhysicalModelMetadata";
            this.dataGridViewPhysicalModelMetadata.Size = new System.Drawing.Size(845, 519);
            this.dataGridViewPhysicalModelMetadata.TabIndex = 2;
            this.dataGridViewPhysicalModelMetadata.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGridViewPhysicalModelMetadataKeyDown);
            // 
            // outputGroupBoxVersioning
            // 
            this.outputGroupBoxVersioning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputGroupBoxVersioning.Controls.Add(this.radioButtonMinorRelease);
            this.outputGroupBoxVersioning.Controls.Add(this.radiobuttonMajorRelease);
            this.outputGroupBoxVersioning.Controls.Add(this.radiobuttonNoVersionChange);
            this.outputGroupBoxVersioning.Location = new System.Drawing.Point(1252, 49);
            this.outputGroupBoxVersioning.Name = "outputGroupBoxVersioning";
            this.outputGroupBoxVersioning.Size = new System.Drawing.Size(243, 94);
            this.outputGroupBoxVersioning.TabIndex = 2;
            this.outputGroupBoxVersioning.TabStop = false;
            this.outputGroupBoxVersioning.Text = "Versioning";
            // 
            // radioButtonMinorRelease
            // 
            this.radioButtonMinorRelease.AutoSize = true;
            this.radioButtonMinorRelease.Location = new System.Drawing.Point(7, 67);
            this.radioButtonMinorRelease.Name = "radioButtonMinorRelease";
            this.radioButtonMinorRelease.Size = new System.Drawing.Size(111, 17);
            this.radioButtonMinorRelease.TabIndex = 2;
            this.radioButtonMinorRelease.Text = "Minor release (0.x)";
            this.radioButtonMinorRelease.UseVisualStyleBackColor = true;
            // 
            // radiobuttonMajorRelease
            // 
            this.radiobuttonMajorRelease.AutoSize = true;
            this.radiobuttonMajorRelease.Location = new System.Drawing.Point(7, 44);
            this.radiobuttonMajorRelease.Name = "radiobuttonMajorRelease";
            this.radiobuttonMajorRelease.Size = new System.Drawing.Size(111, 17);
            this.radiobuttonMajorRelease.TabIndex = 1;
            this.radiobuttonMajorRelease.Text = "Major release (x.0)";
            this.radiobuttonMajorRelease.UseVisualStyleBackColor = true;
            // 
            // radiobuttonNoVersionChange
            // 
            this.radiobuttonNoVersionChange.AutoSize = true;
            this.radiobuttonNoVersionChange.Location = new System.Drawing.Point(7, 21);
            this.radiobuttonNoVersionChange.Name = "radiobuttonNoVersionChange";
            this.radiobuttonNoVersionChange.Size = new System.Drawing.Size(115, 17);
            this.radiobuttonNoVersionChange.TabIndex = 0;
            this.radiobuttonNoVersionChange.Text = "No version change";
            this.radiobuttonNoVersionChange.UseVisualStyleBackColor = true;
            // 
            // MetadataGenerationGroupBox
            // 
            this.MetadataGenerationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MetadataGenerationGroupBox.Controls.Add(this.checkBoxIgnoreVersion);
            this.MetadataGenerationGroupBox.Controls.Add(this.checkBoxValidation);
            this.MetadataGenerationGroupBox.Controls.Add(this.checkBoxClearMetadata);
            this.MetadataGenerationGroupBox.Location = new System.Drawing.Point(1252, 218);
            this.MetadataGenerationGroupBox.Name = "MetadataGenerationGroupBox";
            this.MetadataGenerationGroupBox.Size = new System.Drawing.Size(243, 105);
            this.MetadataGenerationGroupBox.TabIndex = 3;
            this.MetadataGenerationGroupBox.TabStop = false;
            this.MetadataGenerationGroupBox.Text = "Metadata activation options";
            // 
            // checkBoxIgnoreVersion
            // 
            this.checkBoxIgnoreVersion.AutoSize = true;
            this.checkBoxIgnoreVersion.Checked = true;
            this.checkBoxIgnoreVersion.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxIgnoreVersion.Location = new System.Drawing.Point(6, 65);
            this.checkBoxIgnoreVersion.Name = "checkBoxIgnoreVersion";
            this.checkBoxIgnoreVersion.Size = new System.Drawing.Size(219, 17);
            this.checkBoxIgnoreVersion.TabIndex = 25;
            this.checkBoxIgnoreVersion.Text = "Use live database / ignore model version";
            this.checkBoxIgnoreVersion.UseVisualStyleBackColor = true;
            // 
            // checkBoxValidation
            // 
            this.checkBoxValidation.AutoSize = true;
            this.checkBoxValidation.Checked = true;
            this.checkBoxValidation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxValidation.Location = new System.Drawing.Point(6, 42);
            this.checkBoxValidation.Name = "checkBoxValidation";
            this.checkBoxValidation.Size = new System.Drawing.Size(164, 17);
            this.checkBoxValidation.TabIndex = 10;
            this.checkBoxValidation.Text = "Validate generation metadata";
            this.checkBoxValidation.UseVisualStyleBackColor = true;
            // 
            // checkBoxClearMetadata
            // 
            this.checkBoxClearMetadata.AutoSize = true;
            this.checkBoxClearMetadata.Location = new System.Drawing.Point(6, 19);
            this.checkBoxClearMetadata.Name = "checkBoxClearMetadata";
            this.checkBoxClearMetadata.Size = new System.Drawing.Size(150, 17);
            this.checkBoxClearMetadata.TabIndex = 9;
            this.checkBoxClearMetadata.Text = "Clear generation metadata";
            this.checkBoxClearMetadata.UseVisualStyleBackColor = true;
            this.checkBoxClearMetadata.CheckedChanged += new System.EventHandler(this.checkBoxClearMetadata_CheckedChanged);
            // 
            // labelInformation
            // 
            this.labelInformation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInformation.AutoSize = true;
            this.labelInformation.Location = new System.Drawing.Point(654, 764);
            this.labelInformation.Name = "labelInformation";
            this.labelInformation.Size = new System.Drawing.Size(59, 13);
            this.labelInformation.TabIndex = 5;
            this.labelInformation.Text = "Information";
            // 
            // richTextBoxInformation
            // 
            this.richTextBoxInformation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxInformation.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBoxInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxInformation.Location = new System.Drawing.Point(657, 780);
            this.richTextBoxInformation.Name = "richTextBoxInformation";
            this.richTextBoxInformation.Size = new System.Drawing.Size(589, 69);
            this.richTextBoxInformation.TabIndex = 2;
            this.richTextBoxInformation.Text = "";
            // 
            // menuStripMetadata
            // 
            this.menuStripMetadata.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.metadataToolStripMenuItem,
            this.businessKeyMetadataToolStripMenuItem,
            this.attributeMappingMetadataToolStripMenuItem,
            this.physicalModelToolStripMenuItem,
            this.validationToolStripMenuItem});
            this.menuStripMetadata.Location = new System.Drawing.Point(0, 0);
            this.menuStripMetadata.Name = "menuStripMetadata";
            this.menuStripMetadata.Size = new System.Drawing.Size(1507, 24);
            this.menuStripMetadata.TabIndex = 3;
            this.menuStripMetadata.Text = "menuStrip1";
            // 
            // metadataToolStripMenuItem
            // 
            this.metadataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openOutputDirectoryToolStripMenuItem,
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
            this.saveMetadataFileToolStripMenuItem,
            this.saveTableMappingAsJSONToolStripMenuItem});
            this.businessKeyMetadataToolStripMenuItem.Name = "businessKeyMetadataToolStripMenuItem";
            this.businessKeyMetadataToolStripMenuItem.Size = new System.Drawing.Size(151, 20);
            this.businessKeyMetadataToolStripMenuItem.Text = "Table &Mapping Metadata";
            // 
            // openMetadataFileToolStripMenuItem
            // 
            this.openMetadataFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.openMetadataFileToolStripMenuItem.Name = "openMetadataFileToolStripMenuItem";
            this.openMetadataFileToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.openMetadataFileToolStripMenuItem.Text = "Open Table Mapping file";
            this.openMetadataFileToolStripMenuItem.Click += new System.EventHandler(this.openMetadataFileToolStripMenuItem_Click_1);
            // 
            // saveMetadataFileToolStripMenuItem
            // 
            this.saveMetadataFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.SaveFile;
            this.saveMetadataFileToolStripMenuItem.Name = "saveMetadataFileToolStripMenuItem";
            this.saveMetadataFileToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.saveMetadataFileToolStripMenuItem.Text = "Save Table Mapping as XML";
            this.saveMetadataFileToolStripMenuItem.Click += new System.EventHandler(this.saveBusinessKeyMetadataFileToolStripMenuItem_Click);
            // 
            // saveTableMappingAsJSONToolStripMenuItem
            // 
            this.saveTableMappingAsJSONToolStripMenuItem.Image = global::TEAM.Properties.Resources.SaveFile;
            this.saveTableMappingAsJSONToolStripMenuItem.Name = "saveTableMappingAsJSONToolStripMenuItem";
            this.saveTableMappingAsJSONToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.saveTableMappingAsJSONToolStripMenuItem.Text = "Save Table Mapping as JSON";
            this.saveTableMappingAsJSONToolStripMenuItem.Click += new System.EventHandler(this.saveTableMappingAsJSONToolStripMenuItem_Click);
            // 
            // attributeMappingMetadataToolStripMenuItem
            // 
            this.attributeMappingMetadataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.saveAttributeMappingAsJSONToolStripMenuItem});
            this.attributeMappingMetadataToolStripMenuItem.Name = "attributeMappingMetadataToolStripMenuItem";
            this.attributeMappingMetadataToolStripMenuItem.Size = new System.Drawing.Size(170, 20);
            this.attributeMappingMetadataToolStripMenuItem.Text = "A&ttribute Mapping Metadata";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(244, 22);
            this.toolStripMenuItem1.Text = "Open Attribute Mapping File";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.OpenAttributeFileMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Image = global::TEAM.Properties.Resources.SaveFile;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(244, 22);
            this.toolStripMenuItem2.Text = "Save Attribute Mapping as XML";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.saveAttributeMetadataMenuItem_Click);
            // 
            // saveAttributeMappingAsJSONToolStripMenuItem
            // 
            this.saveAttributeMappingAsJSONToolStripMenuItem.Image = global::TEAM.Properties.Resources.SaveFile;
            this.saveAttributeMappingAsJSONToolStripMenuItem.Name = "saveAttributeMappingAsJSONToolStripMenuItem";
            this.saveAttributeMappingAsJSONToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.saveAttributeMappingAsJSONToolStripMenuItem.Text = "Save Attribute Mapping as JSON";
            this.saveAttributeMappingAsJSONToolStripMenuItem.Click += new System.EventHandler(this.saveAttributeMappingAsJSONToolStripMenuItem_Click);
            // 
            // physicalModelToolStripMenuItem
            // 
            this.physicalModelToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openModelMetadataFileToolStripMenuItem,
            this.saveModelMetadataFileAsJSONToolStripMenuItem});
            this.physicalModelToolStripMenuItem.Name = "physicalModelToolStripMenuItem";
            this.physicalModelToolStripMenuItem.Size = new System.Drawing.Size(99, 20);
            this.physicalModelToolStripMenuItem.Text = "&Physical Model";
            // 
            // openModelMetadataFileToolStripMenuItem
            // 
            this.openModelMetadataFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.openModelMetadataFileToolStripMenuItem.Name = "openModelMetadataFileToolStripMenuItem";
            this.openModelMetadataFileToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.openModelMetadataFileToolStripMenuItem.Text = "Open Model Metadata File";
            // 
            // saveModelMetadataFileAsJSONToolStripMenuItem
            // 
            this.saveModelMetadataFileAsJSONToolStripMenuItem.Image = global::TEAM.Properties.Resources.SaveFile;
            this.saveModelMetadataFileAsJSONToolStripMenuItem.Name = "saveModelMetadataFileAsJSONToolStripMenuItem";
            this.saveModelMetadataFileAsJSONToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.saveModelMetadataFileAsJSONToolStripMenuItem.Text = "Save Model Metadata file as JSON";
            this.saveModelMetadataFileAsJSONToolStripMenuItem.Click += new System.EventHandler(this.saveModelMetadataFileAsJSONToolStripMenuItem_Click);
            // 
            // validationToolStripMenuItem
            // 
            this.validationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageValidationRulesToolStripMenuItem});
            this.validationToolStripMenuItem.Name = "validationToolStripMenuItem";
            this.validationToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.validationToolStripMenuItem.Text = "&Validation";
            // 
            // manageValidationRulesToolStripMenuItem
            // 
            this.manageValidationRulesToolStripMenuItem.Image = global::TEAM.Properties.Resources.DocumentationIcon;
            this.manageValidationRulesToolStripMenuItem.Name = "manageValidationRulesToolStripMenuItem";
            this.manageValidationRulesToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.manageValidationRulesToolStripMenuItem.Text = "Manage validation rules";
            this.manageValidationRulesToolStripMenuItem.Click += new System.EventHandler(this.manageValidationRulesToolStripMenuItem_Click);
            // 
            // checkBoxMergeFiles
            // 
            this.checkBoxMergeFiles.AutoSize = true;
            this.checkBoxMergeFiles.Location = new System.Drawing.Point(6, 19);
            this.checkBoxMergeFiles.Name = "checkBoxMergeFiles";
            this.checkBoxMergeFiles.Size = new System.Drawing.Size(211, 17);
            this.checkBoxMergeFiles.TabIndex = 9;
            this.checkBoxMergeFiles.Text = "Merge loaded files with existing content";
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
            this.checkBoxBackupFiles.Location = new System.Drawing.Point(6, 42);
            this.checkBoxBackupFiles.Name = "checkBoxBackupFiles";
            this.checkBoxBackupFiles.Size = new System.Drawing.Size(181, 17);
            this.checkBoxBackupFiles.TabIndex = 10;
            this.checkBoxBackupFiles.Text = "Automatically create file backups";
            this.toolTipMetadata.SetToolTip(this.checkBoxBackupFiles, "Check this option if loaded files (JSON or XML) are added to existing data.\r\n\r\nNo" +
        "t having this option checked will overwrite the mapping information for the sele" +
        "cted version (in the datagrid).");
            this.checkBoxBackupFiles.UseVisualStyleBackColor = true;
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
            this.groupBoxJsonOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxJsonOptions.Controls.Add(this.checkBoxBackupFiles);
            this.groupBoxJsonOptions.Controls.Add(this.checkBoxMergeFiles);
            this.groupBoxJsonOptions.Location = new System.Drawing.Point(1252, 401);
            this.groupBoxJsonOptions.Name = "groupBoxJsonOptions";
            this.groupBoxJsonOptions.Size = new System.Drawing.Size(243, 69);
            this.groupBoxJsonOptions.TabIndex = 26;
            this.groupBoxJsonOptions.TabStop = false;
            this.groupBoxJsonOptions.Text = "Json / XML load options";
            // 
            // FormManageMetadata
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1507, 861);
            this.Controls.Add(this.groupBoxJsonOptions);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonValidation);
            this.Controls.Add(this.labelResult);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.buttonSaveMetadataChanges);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxMetadataCounts);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.outputGroupBoxVersioning);
            this.Controls.Add(this.MetadataGenerationGroupBox);
            this.Controls.Add(this.labelInformation);
            this.Controls.Add(this.richTextBoxInformation);
            this.Controls.Add(this.menuStripMetadata);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStripMetadata;
            this.MinimumSize = new System.Drawing.Size(1500, 850);
            this.Name = "FormManageMetadata";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manage the automation metadata";
            this.Load += new System.EventHandler(this.FormManageMetadata_Load);
            this.SizeChanged += new System.EventHandler(this.FormManageMetadata_SizeChanged);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVersioning)).EndInit();
            this.groupBoxMetadataCounts.ResumeLayout(false);
            this.groupBoxMetadataCounts.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTableMetadata)).EndInit();
            this.contextMenuStripTableMapping.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAttributeMetadata)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPhysicalModelMetadata)).EndInit();
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
        private System.Windows.Forms.CheckBox checkBoxClearMetadata;
        private System.Windows.Forms.CheckBox checkBoxValidation;
        private System.Windows.Forms.ToolStripMenuItem validationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageValidationRulesToolStripMenuItem;
        private System.Windows.Forms.GroupBox outputGroupBoxVersioning;
        private System.Windows.Forms.RadioButton radioButtonMinorRelease;
        private System.Windows.Forms.RadioButton radiobuttonMajorRelease;
        private System.Windows.Forms.RadioButton radiobuttonNoVersionChange;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private CustomDataGridViewAttribute dataGridViewAttributeMetadata;
        private CustomDataGridViewTable dataGridViewTableMetadata;
        private System.Windows.Forms.GroupBox groupBoxMetadataCounts;
        private System.Windows.Forms.Label labelHubCount;
        private System.Windows.Forms.Label labelLsatCount;
        private System.Windows.Forms.Label labelLnkCount;
        private System.Windows.Forms.Label labelSatCount;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Button buttonSaveMetadataChanges;
        private System.Windows.Forms.ToolStripMenuItem businessKeyMetadataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMetadataFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMetadataFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem attributeMappingMetadataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Label labelResult;
        private System.ComponentModel.BackgroundWorker backgroundWorkerMetadata;
        public System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.TrackBar trackBarVersioning;
        private System.Windows.Forms.Button buttonValidation;
        private System.Windows.Forms.CheckBox checkBoxIgnoreVersion;
        private System.Windows.Forms.ToolStripMenuItem saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBoxFilterCriterion;
        private System.Windows.Forms.ToolStripMenuItem saveTableMappingAsJSONToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openOutputDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAttributeMappingAsJSONToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage3;
        private CustomDataGridViewTable dataGridViewPhysicalModelMetadata;
        private System.Windows.Forms.ToolStripMenuItem physicalModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openModelMetadataFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveModelMetadataFileAsJSONToolStripMenuItem;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxIntegrationLayer;
        private System.Windows.Forms.CheckBox checkBoxStagingLayer;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTableMapping;
        private System.Windows.Forms.ToolStripMenuItem exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTipMetadata;
        private System.ComponentModel.BackgroundWorker backgroundWorkerValidationOnly;
        private System.Windows.Forms.GroupBox groupBoxJsonOptions;
        private System.Windows.Forms.CheckBox checkBoxMergeFiles;
        private System.Windows.Forms.CheckBox checkBoxBackupFiles;
    }
}