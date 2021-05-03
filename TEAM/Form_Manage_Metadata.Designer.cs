using System;
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
            this.dataGridViewTableMetadata = new TEAM.CustomDataGridViewTable();
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
            this.dataGridViewComboBoxColumn33 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn98 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn34 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn99 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn100 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn101 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn31 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn94 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn32 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn95 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn96 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn97 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn10 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn84 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn85 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn86 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn29 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn87 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn30 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn88 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn89 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn90 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn9 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn77 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn78 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn79 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn27 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn80 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn28 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn81 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn82 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn83 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn8 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn70 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn71 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn72 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn25 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn73 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn26 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn74 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn75 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn76 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn7 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn63 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn64 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn65 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn23 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn66 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn24 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn67 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn68 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn69 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn6 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn52 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn53 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn54 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn21 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn59 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn22 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn60 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn61 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn62 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn19 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn55 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn20 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn56 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn57 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn58 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn5 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn41 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn42 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn43 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn17 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn48 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn18 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn49 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn50 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn51 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn15 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn44 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn16 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn45 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn46 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn47 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn4 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn34 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn35 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn36 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn13 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn37 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn14 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn38 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn39 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn40 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn3 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn17 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn11 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn30 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn12 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn31 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn32 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn33 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn9 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn26 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn10 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn27 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn28 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn29 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn7 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn22 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn8 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn23 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn24 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn25 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn5 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn18 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn6 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn19 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn20 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn21 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn3 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn4 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxColumn2 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTableMetadata)).BeginInit();
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
            // tabPageDataObjectMapping
            // 
            this.tabPageDataObjectMapping.Controls.Add(this.dataGridViewTableMetadata);
            this.tabPageDataObjectMapping.Location = new System.Drawing.Point(4, 22);
            this.tabPageDataObjectMapping.Name = "tabPageDataObjectMapping";
            this.tabPageDataObjectMapping.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDataObjectMapping.Size = new System.Drawing.Size(1106, 545);
            this.tabPageDataObjectMapping.TabIndex = 0;
            this.tabPageDataObjectMapping.Text = "Data Object (Table) Mappings";
            this.tabPageDataObjectMapping.UseVisualStyleBackColor = true;
            // 
            // dataGridViewTableMetadata
            // 
            this.dataGridViewTableMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewTableMetadata.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewTableMetadata.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTableMetadata.ContextMenuStrip = this.contextMenuStripTableMapping;
            this.dataGridViewTableMetadata.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridViewTableMetadata.Location = new System.Drawing.Point(2, 3);
            this.dataGridViewTableMetadata.MinimumSize = new System.Drawing.Size(964, 511);
            this.dataGridViewTableMetadata.Name = "dataGridViewTableMetadata";
            this.dataGridViewTableMetadata.Size = new System.Drawing.Size(1101, 543);
            this.dataGridViewTableMetadata.TabIndex = 1;
            this.dataGridViewTableMetadata.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGridViewTableMetadata_CellFormatting);
            this.dataGridViewTableMetadata.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridViewTableMetadata_CellValidating);
            this.dataGridViewTableMetadata.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridViewTableMetadata_EditingControlShowing);
            this.dataGridViewTableMetadata.Sorted += new System.EventHandler(this.textBoxFilterCriterion_OnDelayedTextChanged);
            this.dataGridViewTableMetadata.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGridViewTableMetadataKeyDown);
            this.dataGridViewTableMetadata.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGridViewTableMetadata_MouseDown);
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
            // dataGridViewComboBoxColumn33
            // 
            this.dataGridViewComboBoxColumn33.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn33.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn33.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn33.Name = "dataGridViewComboBoxColumn33";
            // 
            // dataGridViewTextBoxColumn98
            // 
            this.dataGridViewTextBoxColumn98.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn98.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn98.Name = "dataGridViewTextBoxColumn98";
            // 
            // dataGridViewComboBoxColumn34
            // 
            this.dataGridViewComboBoxColumn34.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn34.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn34.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn34.Name = "dataGridViewComboBoxColumn34";
            // 
            // dataGridViewTextBoxColumn99
            // 
            this.dataGridViewTextBoxColumn99.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn99.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn99.Name = "dataGridViewTextBoxColumn99";
            // 
            // dataGridViewTextBoxColumn100
            // 
            this.dataGridViewTextBoxColumn100.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn100.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn100.Name = "dataGridViewTextBoxColumn100";
            // 
            // dataGridViewTextBoxColumn101
            // 
            this.dataGridViewTextBoxColumn101.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn101.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn101.Name = "dataGridViewTextBoxColumn101";
            // 
            // dataGridViewComboBoxColumn31
            // 
            this.dataGridViewComboBoxColumn31.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn31.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn31.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn31.Name = "dataGridViewComboBoxColumn31";
            // 
            // dataGridViewTextBoxColumn94
            // 
            this.dataGridViewTextBoxColumn94.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn94.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn94.Name = "dataGridViewTextBoxColumn94";
            // 
            // dataGridViewComboBoxColumn32
            // 
            this.dataGridViewComboBoxColumn32.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn32.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn32.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn32.Name = "dataGridViewComboBoxColumn32";
            // 
            // dataGridViewTextBoxColumn95
            // 
            this.dataGridViewTextBoxColumn95.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn95.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn95.Name = "dataGridViewTextBoxColumn95";
            // 
            // dataGridViewTextBoxColumn96
            // 
            this.dataGridViewTextBoxColumn96.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn96.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn96.Name = "dataGridViewTextBoxColumn96";
            // 
            // dataGridViewTextBoxColumn97
            // 
            this.dataGridViewTextBoxColumn97.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn97.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn97.Name = "dataGridViewTextBoxColumn97";
            // 
            // dataGridViewCheckBoxColumn10
            // 
            this.dataGridViewCheckBoxColumn10.DataPropertyName = "Enabled";
            this.dataGridViewCheckBoxColumn10.HeaderText = "Enabled";
            this.dataGridViewCheckBoxColumn10.Name = "dataGridViewCheckBoxColumn10";
            // 
            // dataGridViewTextBoxColumn84
            // 
            this.dataGridViewTextBoxColumn84.DataPropertyName = "HashKey";
            this.dataGridViewTextBoxColumn84.HeaderText = "HashKey";
            this.dataGridViewTextBoxColumn84.Name = "dataGridViewTextBoxColumn84";
            this.dataGridViewTextBoxColumn84.Visible = false;
            // 
            // dataGridViewTextBoxColumn85
            // 
            this.dataGridViewTextBoxColumn85.DataPropertyName = "VersionId";
            this.dataGridViewTextBoxColumn85.HeaderText = "VersionId";
            this.dataGridViewTextBoxColumn85.Name = "dataGridViewTextBoxColumn85";
            this.dataGridViewTextBoxColumn85.Visible = false;
            // 
            // dataGridViewTextBoxColumn86
            // 
            this.dataGridViewTextBoxColumn86.DataPropertyName = "SourceTable";
            this.dataGridViewTextBoxColumn86.HeaderText = "Source Data Object";
            this.dataGridViewTextBoxColumn86.Name = "dataGridViewTextBoxColumn86";
            // 
            // dataGridViewComboBoxColumn29
            // 
            this.dataGridViewComboBoxColumn29.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn29.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn29.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn29.Name = "dataGridViewComboBoxColumn29";
            // 
            // dataGridViewTextBoxColumn87
            // 
            this.dataGridViewTextBoxColumn87.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn87.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn87.Name = "dataGridViewTextBoxColumn87";
            // 
            // dataGridViewComboBoxColumn30
            // 
            this.dataGridViewComboBoxColumn30.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn30.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn30.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn30.Name = "dataGridViewComboBoxColumn30";
            // 
            // dataGridViewTextBoxColumn88
            // 
            this.dataGridViewTextBoxColumn88.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn88.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn88.Name = "dataGridViewTextBoxColumn88";
            // 
            // dataGridViewTextBoxColumn89
            // 
            this.dataGridViewTextBoxColumn89.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn89.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn89.Name = "dataGridViewTextBoxColumn89";
            // 
            // dataGridViewTextBoxColumn90
            // 
            this.dataGridViewTextBoxColumn90.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn90.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn90.Name = "dataGridViewTextBoxColumn90";
            // 
            // dataGridViewCheckBoxColumn9
            // 
            this.dataGridViewCheckBoxColumn9.DataPropertyName = "Enabled";
            this.dataGridViewCheckBoxColumn9.HeaderText = "Enabled";
            this.dataGridViewCheckBoxColumn9.Name = "dataGridViewCheckBoxColumn9";
            // 
            // dataGridViewTextBoxColumn77
            // 
            this.dataGridViewTextBoxColumn77.DataPropertyName = "HashKey";
            this.dataGridViewTextBoxColumn77.HeaderText = "HashKey";
            this.dataGridViewTextBoxColumn77.Name = "dataGridViewTextBoxColumn77";
            this.dataGridViewTextBoxColumn77.Visible = false;
            // 
            // dataGridViewTextBoxColumn78
            // 
            this.dataGridViewTextBoxColumn78.DataPropertyName = "VersionId";
            this.dataGridViewTextBoxColumn78.HeaderText = "VersionId";
            this.dataGridViewTextBoxColumn78.Name = "dataGridViewTextBoxColumn78";
            this.dataGridViewTextBoxColumn78.Visible = false;
            // 
            // dataGridViewTextBoxColumn79
            // 
            this.dataGridViewTextBoxColumn79.DataPropertyName = "SourceTable";
            this.dataGridViewTextBoxColumn79.HeaderText = "Source Data Object";
            this.dataGridViewTextBoxColumn79.Name = "dataGridViewTextBoxColumn79";
            // 
            // dataGridViewComboBoxColumn27
            // 
            this.dataGridViewComboBoxColumn27.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn27.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn27.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn27.Name = "dataGridViewComboBoxColumn27";
            // 
            // dataGridViewTextBoxColumn80
            // 
            this.dataGridViewTextBoxColumn80.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn80.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn80.Name = "dataGridViewTextBoxColumn80";
            // 
            // dataGridViewComboBoxColumn28
            // 
            this.dataGridViewComboBoxColumn28.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn28.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn28.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn28.Name = "dataGridViewComboBoxColumn28";
            // 
            // dataGridViewTextBoxColumn81
            // 
            this.dataGridViewTextBoxColumn81.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn81.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn81.Name = "dataGridViewTextBoxColumn81";
            // 
            // dataGridViewTextBoxColumn82
            // 
            this.dataGridViewTextBoxColumn82.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn82.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn82.Name = "dataGridViewTextBoxColumn82";
            // 
            // dataGridViewTextBoxColumn83
            // 
            this.dataGridViewTextBoxColumn83.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn83.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn83.Name = "dataGridViewTextBoxColumn83";
            // 
            // dataGridViewCheckBoxColumn8
            // 
            this.dataGridViewCheckBoxColumn8.DataPropertyName = "Enabled";
            this.dataGridViewCheckBoxColumn8.HeaderText = "Enabled";
            this.dataGridViewCheckBoxColumn8.Name = "dataGridViewCheckBoxColumn8";
            // 
            // dataGridViewTextBoxColumn70
            // 
            this.dataGridViewTextBoxColumn70.DataPropertyName = "HashKey";
            this.dataGridViewTextBoxColumn70.HeaderText = "HashKey";
            this.dataGridViewTextBoxColumn70.Name = "dataGridViewTextBoxColumn70";
            this.dataGridViewTextBoxColumn70.Visible = false;
            // 
            // dataGridViewTextBoxColumn71
            // 
            this.dataGridViewTextBoxColumn71.DataPropertyName = "VersionId";
            this.dataGridViewTextBoxColumn71.HeaderText = "VersionId";
            this.dataGridViewTextBoxColumn71.Name = "dataGridViewTextBoxColumn71";
            this.dataGridViewTextBoxColumn71.Visible = false;
            // 
            // dataGridViewTextBoxColumn72
            // 
            this.dataGridViewTextBoxColumn72.DataPropertyName = "SourceTable";
            this.dataGridViewTextBoxColumn72.HeaderText = "Source Data Object";
            this.dataGridViewTextBoxColumn72.Name = "dataGridViewTextBoxColumn72";
            // 
            // dataGridViewComboBoxColumn25
            // 
            this.dataGridViewComboBoxColumn25.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn25.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn25.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn25.Name = "dataGridViewComboBoxColumn25";
            // 
            // dataGridViewTextBoxColumn73
            // 
            this.dataGridViewTextBoxColumn73.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn73.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn73.Name = "dataGridViewTextBoxColumn73";
            // 
            // dataGridViewComboBoxColumn26
            // 
            this.dataGridViewComboBoxColumn26.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn26.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn26.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn26.Name = "dataGridViewComboBoxColumn26";
            // 
            // dataGridViewTextBoxColumn74
            // 
            this.dataGridViewTextBoxColumn74.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn74.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn74.Name = "dataGridViewTextBoxColumn74";
            // 
            // dataGridViewTextBoxColumn75
            // 
            this.dataGridViewTextBoxColumn75.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn75.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn75.Name = "dataGridViewTextBoxColumn75";
            // 
            // dataGridViewTextBoxColumn76
            // 
            this.dataGridViewTextBoxColumn76.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn76.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn76.Name = "dataGridViewTextBoxColumn76";
            // 
            // dataGridViewCheckBoxColumn7
            // 
            this.dataGridViewCheckBoxColumn7.DataPropertyName = "Enabled";
            this.dataGridViewCheckBoxColumn7.HeaderText = "Enabled";
            this.dataGridViewCheckBoxColumn7.Name = "dataGridViewCheckBoxColumn7";
            // 
            // dataGridViewTextBoxColumn63
            // 
            this.dataGridViewTextBoxColumn63.DataPropertyName = "HashKey";
            this.dataGridViewTextBoxColumn63.HeaderText = "HashKey";
            this.dataGridViewTextBoxColumn63.Name = "dataGridViewTextBoxColumn63";
            this.dataGridViewTextBoxColumn63.Visible = false;
            // 
            // dataGridViewTextBoxColumn64
            // 
            this.dataGridViewTextBoxColumn64.DataPropertyName = "VersionId";
            this.dataGridViewTextBoxColumn64.HeaderText = "VersionId";
            this.dataGridViewTextBoxColumn64.Name = "dataGridViewTextBoxColumn64";
            this.dataGridViewTextBoxColumn64.Visible = false;
            // 
            // dataGridViewTextBoxColumn65
            // 
            this.dataGridViewTextBoxColumn65.DataPropertyName = "SourceTable";
            this.dataGridViewTextBoxColumn65.HeaderText = "Source Data Object";
            this.dataGridViewTextBoxColumn65.Name = "dataGridViewTextBoxColumn65";
            // 
            // dataGridViewComboBoxColumn23
            // 
            this.dataGridViewComboBoxColumn23.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn23.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn23.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn23.Name = "dataGridViewComboBoxColumn23";
            // 
            // dataGridViewTextBoxColumn66
            // 
            this.dataGridViewTextBoxColumn66.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn66.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn66.Name = "dataGridViewTextBoxColumn66";
            // 
            // dataGridViewComboBoxColumn24
            // 
            this.dataGridViewComboBoxColumn24.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn24.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn24.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn24.Name = "dataGridViewComboBoxColumn24";
            // 
            // dataGridViewTextBoxColumn67
            // 
            this.dataGridViewTextBoxColumn67.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn67.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn67.Name = "dataGridViewTextBoxColumn67";
            // 
            // dataGridViewTextBoxColumn68
            // 
            this.dataGridViewTextBoxColumn68.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn68.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn68.Name = "dataGridViewTextBoxColumn68";
            // 
            // dataGridViewTextBoxColumn69
            // 
            this.dataGridViewTextBoxColumn69.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn69.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn69.Name = "dataGridViewTextBoxColumn69";
            // 
            // dataGridViewCheckBoxColumn6
            // 
            this.dataGridViewCheckBoxColumn6.DataPropertyName = "Enabled";
            this.dataGridViewCheckBoxColumn6.HeaderText = "Enabled";
            this.dataGridViewCheckBoxColumn6.Name = "dataGridViewCheckBoxColumn6";
            // 
            // dataGridViewTextBoxColumn52
            // 
            this.dataGridViewTextBoxColumn52.DataPropertyName = "HashKey";
            this.dataGridViewTextBoxColumn52.HeaderText = "HashKey";
            this.dataGridViewTextBoxColumn52.Name = "dataGridViewTextBoxColumn52";
            this.dataGridViewTextBoxColumn52.Visible = false;
            // 
            // dataGridViewTextBoxColumn53
            // 
            this.dataGridViewTextBoxColumn53.DataPropertyName = "VersionId";
            this.dataGridViewTextBoxColumn53.HeaderText = "VersionId";
            this.dataGridViewTextBoxColumn53.Name = "dataGridViewTextBoxColumn53";
            this.dataGridViewTextBoxColumn53.Visible = false;
            // 
            // dataGridViewTextBoxColumn54
            // 
            this.dataGridViewTextBoxColumn54.DataPropertyName = "SourceTable";
            this.dataGridViewTextBoxColumn54.HeaderText = "Source Data Object";
            this.dataGridViewTextBoxColumn54.Name = "dataGridViewTextBoxColumn54";
            // 
            // dataGridViewComboBoxColumn21
            // 
            this.dataGridViewComboBoxColumn21.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn21.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn21.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn21.Name = "dataGridViewComboBoxColumn21";
            // 
            // dataGridViewTextBoxColumn59
            // 
            this.dataGridViewTextBoxColumn59.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn59.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn59.Name = "dataGridViewTextBoxColumn59";
            // 
            // dataGridViewComboBoxColumn22
            // 
            this.dataGridViewComboBoxColumn22.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn22.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn22.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn22.Name = "dataGridViewComboBoxColumn22";
            // 
            // dataGridViewTextBoxColumn60
            // 
            this.dataGridViewTextBoxColumn60.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn60.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn60.Name = "dataGridViewTextBoxColumn60";
            // 
            // dataGridViewTextBoxColumn61
            // 
            this.dataGridViewTextBoxColumn61.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn61.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn61.Name = "dataGridViewTextBoxColumn61";
            // 
            // dataGridViewTextBoxColumn62
            // 
            this.dataGridViewTextBoxColumn62.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn62.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn62.Name = "dataGridViewTextBoxColumn62";
            // 
            // dataGridViewComboBoxColumn19
            // 
            this.dataGridViewComboBoxColumn19.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn19.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn19.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn19.Name = "dataGridViewComboBoxColumn19";
            // 
            // dataGridViewTextBoxColumn55
            // 
            this.dataGridViewTextBoxColumn55.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn55.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn55.Name = "dataGridViewTextBoxColumn55";
            // 
            // dataGridViewComboBoxColumn20
            // 
            this.dataGridViewComboBoxColumn20.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn20.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn20.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn20.Name = "dataGridViewComboBoxColumn20";
            // 
            // dataGridViewTextBoxColumn56
            // 
            this.dataGridViewTextBoxColumn56.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn56.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn56.Name = "dataGridViewTextBoxColumn56";
            // 
            // dataGridViewTextBoxColumn57
            // 
            this.dataGridViewTextBoxColumn57.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn57.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn57.Name = "dataGridViewTextBoxColumn57";
            // 
            // dataGridViewTextBoxColumn58
            // 
            this.dataGridViewTextBoxColumn58.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn58.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn58.Name = "dataGridViewTextBoxColumn58";
            // 
            // dataGridViewCheckBoxColumn5
            // 
            this.dataGridViewCheckBoxColumn5.DataPropertyName = "Enabled";
            this.dataGridViewCheckBoxColumn5.HeaderText = "Enabled";
            this.dataGridViewCheckBoxColumn5.Name = "dataGridViewCheckBoxColumn5";
            // 
            // dataGridViewTextBoxColumn41
            // 
            this.dataGridViewTextBoxColumn41.DataPropertyName = "HashKey";
            this.dataGridViewTextBoxColumn41.HeaderText = "HashKey";
            this.dataGridViewTextBoxColumn41.Name = "dataGridViewTextBoxColumn41";
            this.dataGridViewTextBoxColumn41.Visible = false;
            // 
            // dataGridViewTextBoxColumn42
            // 
            this.dataGridViewTextBoxColumn42.DataPropertyName = "VersionId";
            this.dataGridViewTextBoxColumn42.HeaderText = "VersionId";
            this.dataGridViewTextBoxColumn42.Name = "dataGridViewTextBoxColumn42";
            this.dataGridViewTextBoxColumn42.Visible = false;
            // 
            // dataGridViewTextBoxColumn43
            // 
            this.dataGridViewTextBoxColumn43.DataPropertyName = "SourceTable";
            this.dataGridViewTextBoxColumn43.HeaderText = "Source Data Object";
            this.dataGridViewTextBoxColumn43.Name = "dataGridViewTextBoxColumn43";
            // 
            // dataGridViewComboBoxColumn17
            // 
            this.dataGridViewComboBoxColumn17.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn17.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn17.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn17.Name = "dataGridViewComboBoxColumn17";
            // 
            // dataGridViewTextBoxColumn48
            // 
            this.dataGridViewTextBoxColumn48.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn48.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn48.Name = "dataGridViewTextBoxColumn48";
            // 
            // dataGridViewComboBoxColumn18
            // 
            this.dataGridViewComboBoxColumn18.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn18.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn18.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn18.Name = "dataGridViewComboBoxColumn18";
            // 
            // dataGridViewTextBoxColumn49
            // 
            this.dataGridViewTextBoxColumn49.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn49.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn49.Name = "dataGridViewTextBoxColumn49";
            // 
            // dataGridViewTextBoxColumn50
            // 
            this.dataGridViewTextBoxColumn50.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn50.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn50.Name = "dataGridViewTextBoxColumn50";
            // 
            // dataGridViewTextBoxColumn51
            // 
            this.dataGridViewTextBoxColumn51.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn51.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn51.Name = "dataGridViewTextBoxColumn51";
            // 
            // dataGridViewComboBoxColumn15
            // 
            this.dataGridViewComboBoxColumn15.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn15.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn15.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn15.Name = "dataGridViewComboBoxColumn15";
            // 
            // dataGridViewTextBoxColumn44
            // 
            this.dataGridViewTextBoxColumn44.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn44.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn44.Name = "dataGridViewTextBoxColumn44";
            // 
            // dataGridViewComboBoxColumn16
            // 
            this.dataGridViewComboBoxColumn16.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn16.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn16.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn16.Name = "dataGridViewComboBoxColumn16";
            // 
            // dataGridViewTextBoxColumn45
            // 
            this.dataGridViewTextBoxColumn45.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn45.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn45.Name = "dataGridViewTextBoxColumn45";
            // 
            // dataGridViewTextBoxColumn46
            // 
            this.dataGridViewTextBoxColumn46.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn46.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn46.Name = "dataGridViewTextBoxColumn46";
            // 
            // dataGridViewTextBoxColumn47
            // 
            this.dataGridViewTextBoxColumn47.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn47.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn47.Name = "dataGridViewTextBoxColumn47";
            // 
            // dataGridViewCheckBoxColumn4
            // 
            this.dataGridViewCheckBoxColumn4.DataPropertyName = "Enabled";
            this.dataGridViewCheckBoxColumn4.HeaderText = "Enabled";
            this.dataGridViewCheckBoxColumn4.Name = "dataGridViewCheckBoxColumn4";
            // 
            // dataGridViewTextBoxColumn34
            // 
            this.dataGridViewTextBoxColumn34.DataPropertyName = "HashKey";
            this.dataGridViewTextBoxColumn34.HeaderText = "HashKey";
            this.dataGridViewTextBoxColumn34.Name = "dataGridViewTextBoxColumn34";
            this.dataGridViewTextBoxColumn34.Visible = false;
            // 
            // dataGridViewTextBoxColumn35
            // 
            this.dataGridViewTextBoxColumn35.DataPropertyName = "VersionId";
            this.dataGridViewTextBoxColumn35.HeaderText = "VersionId";
            this.dataGridViewTextBoxColumn35.Name = "dataGridViewTextBoxColumn35";
            this.dataGridViewTextBoxColumn35.Visible = false;
            // 
            // dataGridViewTextBoxColumn36
            // 
            this.dataGridViewTextBoxColumn36.DataPropertyName = "SourceTable";
            this.dataGridViewTextBoxColumn36.HeaderText = "Source Data Object";
            this.dataGridViewTextBoxColumn36.Name = "dataGridViewTextBoxColumn36";
            // 
            // dataGridViewComboBoxColumn13
            // 
            this.dataGridViewComboBoxColumn13.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn13.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn13.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn13.Name = "dataGridViewComboBoxColumn13";
            // 
            // dataGridViewTextBoxColumn37
            // 
            this.dataGridViewTextBoxColumn37.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn37.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn37.Name = "dataGridViewTextBoxColumn37";
            // 
            // dataGridViewComboBoxColumn14
            // 
            this.dataGridViewComboBoxColumn14.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn14.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn14.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn14.Name = "dataGridViewComboBoxColumn14";
            // 
            // dataGridViewTextBoxColumn38
            // 
            this.dataGridViewTextBoxColumn38.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn38.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn38.Name = "dataGridViewTextBoxColumn38";
            // 
            // dataGridViewTextBoxColumn39
            // 
            this.dataGridViewTextBoxColumn39.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn39.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn39.Name = "dataGridViewTextBoxColumn39";
            // 
            // dataGridViewTextBoxColumn40
            // 
            this.dataGridViewTextBoxColumn40.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn40.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn40.Name = "dataGridViewTextBoxColumn40";
            // 
            // dataGridViewCheckBoxColumn3
            // 
            this.dataGridViewCheckBoxColumn3.DataPropertyName = "Enabled";
            this.dataGridViewCheckBoxColumn3.HeaderText = "Enabled";
            this.dataGridViewCheckBoxColumn3.Name = "dataGridViewCheckBoxColumn3";
            // 
            // dataGridViewTextBoxColumn15
            // 
            this.dataGridViewTextBoxColumn15.DataPropertyName = "HashKey";
            this.dataGridViewTextBoxColumn15.HeaderText = "HashKey";
            this.dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
            this.dataGridViewTextBoxColumn15.Visible = false;
            // 
            // dataGridViewTextBoxColumn16
            // 
            this.dataGridViewTextBoxColumn16.DataPropertyName = "VersionId";
            this.dataGridViewTextBoxColumn16.HeaderText = "VersionId";
            this.dataGridViewTextBoxColumn16.Name = "dataGridViewTextBoxColumn16";
            this.dataGridViewTextBoxColumn16.Visible = false;
            // 
            // dataGridViewTextBoxColumn17
            // 
            this.dataGridViewTextBoxColumn17.DataPropertyName = "SourceTable";
            this.dataGridViewTextBoxColumn17.HeaderText = "Source Data Object";
            this.dataGridViewTextBoxColumn17.Name = "dataGridViewTextBoxColumn17";
            // 
            // dataGridViewComboBoxColumn11
            // 
            this.dataGridViewComboBoxColumn11.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn11.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn11.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn11.Name = "dataGridViewComboBoxColumn11";
            // 
            // dataGridViewTextBoxColumn30
            // 
            this.dataGridViewTextBoxColumn30.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn30.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn30.Name = "dataGridViewTextBoxColumn30";
            // 
            // dataGridViewComboBoxColumn12
            // 
            this.dataGridViewComboBoxColumn12.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn12.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn12.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn12.Name = "dataGridViewComboBoxColumn12";
            // 
            // dataGridViewTextBoxColumn31
            // 
            this.dataGridViewTextBoxColumn31.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn31.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn31.Name = "dataGridViewTextBoxColumn31";
            // 
            // dataGridViewTextBoxColumn32
            // 
            this.dataGridViewTextBoxColumn32.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn32.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn32.Name = "dataGridViewTextBoxColumn32";
            // 
            // dataGridViewTextBoxColumn33
            // 
            this.dataGridViewTextBoxColumn33.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn33.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn33.Name = "dataGridViewTextBoxColumn33";
            // 
            // dataGridViewComboBoxColumn9
            // 
            this.dataGridViewComboBoxColumn9.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn9.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn9.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn9.Name = "dataGridViewComboBoxColumn9";
            // 
            // dataGridViewTextBoxColumn26
            // 
            this.dataGridViewTextBoxColumn26.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn26.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn26.Name = "dataGridViewTextBoxColumn26";
            // 
            // dataGridViewComboBoxColumn10
            // 
            this.dataGridViewComboBoxColumn10.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn10.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn10.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn10.Name = "dataGridViewComboBoxColumn10";
            // 
            // dataGridViewTextBoxColumn27
            // 
            this.dataGridViewTextBoxColumn27.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn27.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn27.Name = "dataGridViewTextBoxColumn27";
            // 
            // dataGridViewTextBoxColumn28
            // 
            this.dataGridViewTextBoxColumn28.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn28.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn28.Name = "dataGridViewTextBoxColumn28";
            // 
            // dataGridViewTextBoxColumn29
            // 
            this.dataGridViewTextBoxColumn29.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn29.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn29.Name = "dataGridViewTextBoxColumn29";
            // 
            // dataGridViewComboBoxColumn7
            // 
            this.dataGridViewComboBoxColumn7.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn7.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn7.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn7.Name = "dataGridViewComboBoxColumn7";
            // 
            // dataGridViewTextBoxColumn22
            // 
            this.dataGridViewTextBoxColumn22.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn22.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn22.Name = "dataGridViewTextBoxColumn22";
            // 
            // dataGridViewComboBoxColumn8
            // 
            this.dataGridViewComboBoxColumn8.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn8.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn8.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn8.Name = "dataGridViewComboBoxColumn8";
            // 
            // dataGridViewTextBoxColumn23
            // 
            this.dataGridViewTextBoxColumn23.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn23.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn23.Name = "dataGridViewTextBoxColumn23";
            // 
            // dataGridViewTextBoxColumn24
            // 
            this.dataGridViewTextBoxColumn24.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn24.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn24.Name = "dataGridViewTextBoxColumn24";
            // 
            // dataGridViewTextBoxColumn25
            // 
            this.dataGridViewTextBoxColumn25.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn25.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn25.Name = "dataGridViewTextBoxColumn25";
            // 
            // dataGridViewComboBoxColumn5
            // 
            this.dataGridViewComboBoxColumn5.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn5.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn5.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn5.Name = "dataGridViewComboBoxColumn5";
            // 
            // dataGridViewTextBoxColumn18
            // 
            this.dataGridViewTextBoxColumn18.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn18.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn18.Name = "dataGridViewTextBoxColumn18";
            // 
            // dataGridViewComboBoxColumn6
            // 
            this.dataGridViewComboBoxColumn6.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn6.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn6.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn6.Name = "dataGridViewComboBoxColumn6";
            // 
            // dataGridViewTextBoxColumn19
            // 
            this.dataGridViewTextBoxColumn19.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn19.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn19.Name = "dataGridViewTextBoxColumn19";
            // 
            // dataGridViewTextBoxColumn20
            // 
            this.dataGridViewTextBoxColumn20.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn20.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn20.Name = "dataGridViewTextBoxColumn20";
            // 
            // dataGridViewTextBoxColumn21
            // 
            this.dataGridViewTextBoxColumn21.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn21.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn21.Name = "dataGridViewTextBoxColumn21";
            // 
            // dataGridViewCheckBoxColumn2
            // 
            this.dataGridViewCheckBoxColumn2.DataPropertyName = "Enabled";
            this.dataGridViewCheckBoxColumn2.HeaderText = "Enabled";
            this.dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.DataPropertyName = "HashKey";
            this.dataGridViewTextBoxColumn8.HeaderText = "HashKey";
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            this.dataGridViewTextBoxColumn8.Visible = false;
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.DataPropertyName = "VersionId";
            this.dataGridViewTextBoxColumn9.HeaderText = "VersionId";
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            this.dataGridViewTextBoxColumn9.Visible = false;
            // 
            // dataGridViewTextBoxColumn10
            // 
            this.dataGridViewTextBoxColumn10.DataPropertyName = "SourceTable";
            this.dataGridViewTextBoxColumn10.HeaderText = "Source Data Object";
            this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            // 
            // dataGridViewComboBoxColumn3
            // 
            this.dataGridViewComboBoxColumn3.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn3.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn3.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn3.Name = "dataGridViewComboBoxColumn3";
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn11.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            // 
            // dataGridViewComboBoxColumn4
            // 
            this.dataGridViewComboBoxColumn4.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn4.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn4.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn4.Name = "dataGridViewComboBoxColumn4";
            // 
            // dataGridViewTextBoxColumn12
            // 
            this.dataGridViewTextBoxColumn12.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn12.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            // 
            // dataGridViewTextBoxColumn13
            // 
            this.dataGridViewTextBoxColumn13.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn13.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            // 
            // dataGridViewTextBoxColumn14
            // 
            this.dataGridViewTextBoxColumn14.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn14.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.DataPropertyName = "Enabled";
            this.dataGridViewCheckBoxColumn1.HeaderText = "Enabled";
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "HashKey";
            this.dataGridViewTextBoxColumn1.HeaderText = "HashKey";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Visible = false;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "VersionId";
            this.dataGridViewTextBoxColumn2.HeaderText = "VersionId";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Visible = false;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "SourceTable";
            this.dataGridViewTextBoxColumn3.HeaderText = "Source Data Object";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewComboBoxColumn1
            // 
            this.dataGridViewComboBoxColumn1.DataPropertyName = "SourceConnection";
            this.dataGridViewComboBoxColumn1.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn1.HeaderText = "Source Connection";
            this.dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "TargetTable";
            this.dataGridViewTextBoxColumn4.HeaderText = "Target Data Object";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // dataGridViewComboBoxColumn2
            // 
            this.dataGridViewComboBoxColumn2.DataPropertyName = "TargetConnection";
            this.dataGridViewComboBoxColumn2.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.dataGridViewComboBoxColumn2.HeaderText = "Target Connection";
            this.dataGridViewComboBoxColumn2.Name = "dataGridViewComboBoxColumn2";
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "BusinessKeyDefinition";
            this.dataGridViewTextBoxColumn5.HeaderText = "Business Key Definition";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.DataPropertyName = "DrivingKeyDefinition";
            this.dataGridViewTextBoxColumn6.HeaderText = "Driving Key Definition";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.DataPropertyName = "FilterCriterion";
            this.dataGridViewTextBoxColumn7.HeaderText = "Filter Criterion";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
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
            this.checkBoxShowJsonOutput.Size = new System.Drawing.Size(118, 17);
            this.checkBoxShowJsonOutput.TabIndex = 10;
            this.checkBoxShowJsonOutput.Text = "Display Json output";
            this.checkBoxShowJsonOutput.UseVisualStyleBackColor = true;
            // 
            // checkBoxSaveInterfaceToJson
            // 
            this.checkBoxSaveInterfaceToJson.AutoSize = true;
            this.checkBoxSaveInterfaceToJson.Checked = true;
            this.checkBoxSaveInterfaceToJson.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSaveInterfaceToJson.Location = new System.Drawing.Point(8, 45);
            this.checkBoxSaveInterfaceToJson.Name = "checkBoxSaveInterfaceToJson";
            this.checkBoxSaveInterfaceToJson.Size = new System.Drawing.Size(110, 17);
            this.checkBoxSaveInterfaceToJson.TabIndex = 11;
            this.checkBoxSaveInterfaceToJson.Text = "Save Json to disk";
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
            this.openMetadataFileToolStripMenuItem.Size = new System.Drawing.Size(236, 30);
            this.openMetadataFileToolStripMenuItem.Text = "Import Data Object Mapping";
            this.openMetadataFileToolStripMenuItem.Click += new System.EventHandler(this.openMetadataFileToolStripMenuItem_Click_1);
            // 
            // saveTableMappingAsJSONToolStripMenuItem
            // 
            this.saveTableMappingAsJSONToolStripMenuItem.Image = global::TEAM.Properties.Resources.SaveFile;
            this.saveTableMappingAsJSONToolStripMenuItem.Name = "saveTableMappingAsJSONToolStripMenuItem";
            this.saveTableMappingAsJSONToolStripMenuItem.Size = new System.Drawing.Size(236, 30);
            this.saveTableMappingAsJSONToolStripMenuItem.Text = "Export Data Object Mapping";
            this.saveTableMappingAsJSONToolStripMenuItem.Click += new System.EventHandler(this.saveTableMappingAsJSONToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(233, 6);
            // 
            // openAttributeMappingFileToolStripMenuItem
            // 
            this.openAttributeMappingFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.openAttributeMappingFileToolStripMenuItem.Name = "openAttributeMappingFileToolStripMenuItem";
            this.openAttributeMappingFileToolStripMenuItem.Size = new System.Drawing.Size(236, 30);
            this.openAttributeMappingFileToolStripMenuItem.Text = "Import Data Item Mapping";
            this.openAttributeMappingFileToolStripMenuItem.Click += new System.EventHandler(this.openAttributeMappingFileToolStripMenuItem_Click);
            // 
            // saveAttributeMappingAsJSONToolStripMenuItem1
            // 
            this.saveAttributeMappingAsJSONToolStripMenuItem1.Image = global::TEAM.Properties.Resources.SaveFile;
            this.saveAttributeMappingAsJSONToolStripMenuItem1.Name = "saveAttributeMappingAsJSONToolStripMenuItem1";
            this.saveAttributeMappingAsJSONToolStripMenuItem1.Size = new System.Drawing.Size(236, 30);
            this.saveAttributeMappingAsJSONToolStripMenuItem1.Text = "Export Data Item Mapping";
            this.saveAttributeMappingAsJSONToolStripMenuItem1.Click += new System.EventHandler(this.saveAttributeMappingAsJSONToolStripMenuItem1_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(233, 6);
            // 
            // openPhysicalModelFileToolStripMenuItem
            // 
            this.openPhysicalModelFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.OpenFileIcon;
            this.openPhysicalModelFileToolStripMenuItem.Name = "openPhysicalModelFileToolStripMenuItem";
            this.openPhysicalModelFileToolStripMenuItem.Size = new System.Drawing.Size(236, 30);
            this.openPhysicalModelFileToolStripMenuItem.Text = "Import Physical Model";
            this.openPhysicalModelFileToolStripMenuItem.Click += new System.EventHandler(this.openPhysicalModelFileToolStripMenuItem_Click);
            // 
            // exportPhysicalModelFileToolStripMenuItem
            // 
            this.exportPhysicalModelFileToolStripMenuItem.Image = global::TEAM.Properties.Resources.SaveFile;
            this.exportPhysicalModelFileToolStripMenuItem.Name = "exportPhysicalModelFileToolStripMenuItem";
            this.exportPhysicalModelFileToolStripMenuItem.Size = new System.Drawing.Size(236, 30);
            this.exportPhysicalModelFileToolStripMenuItem.Text = "Export Physical Model";
            this.exportPhysicalModelFileToolStripMenuItem.Click += new System.EventHandler(this.exportPhysicalModelFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(233, 6);
            // 
            // automapDataItemsToolStripMenuItem
            // 
            this.automapDataItemsToolStripMenuItem.Image = global::TEAM.Properties.Resources.CogIcon;
            this.automapDataItemsToolStripMenuItem.Name = "automapDataItemsToolStripMenuItem";
            this.automapDataItemsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.automapDataItemsToolStripMenuItem.Size = new System.Drawing.Size(236, 30);
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
            this.manageValidationRulesToolStripMenuItem.Text = "Manage validation rules";
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
            this.jsonExportConfigurationToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
            this.jsonExportConfigurationToolStripMenuItem.Text = "&Json";
            // 
            // manageJsonExportRulesToolStripMenuItem
            // 
            this.manageJsonExportRulesToolStripMenuItem.Image = global::TEAM.Properties.Resources.DocumentationIcon;
            this.manageJsonExportRulesToolStripMenuItem.Name = "manageJsonExportRulesToolStripMenuItem";
            this.manageJsonExportRulesToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.manageJsonExportRulesToolStripMenuItem.Text = "Manage Json export rules";
            this.manageJsonExportRulesToolStripMenuItem.Click += new System.EventHandler(this.manageJsonExportRulesToolStripMenuItem_Click);
            // 
            // generateJsonInterfaceFilesOnlyToolStripMenuItem
            // 
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem.Image = global::TEAM.Properties.Resources.CogIcon;
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem.Name = "generateJsonInterfaceFilesOnlyToolStripMenuItem";
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.generateJsonInterfaceFilesOnlyToolStripMenuItem.Text = "&Generate Json Interface Files Only";
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
            this.groupBoxJsonOptions.Text = "Json / XML load options";
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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTableMetadata)).EndInit();
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
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn18;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn19;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn20;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn21;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn22;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn23;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn24;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn25;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn26;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn10;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn27;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn28;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn29;
        private System.Windows.Forms.ToolStripMenuItem saveTableMappingAsJSONToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openAttributeMappingFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAttributeMappingAsJSONToolStripMenuItem1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn16;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn17;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn11;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn30;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn12;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn31;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn32;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn33;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn34;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn35;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn36;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn13;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn37;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn14;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn38;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn39;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn40;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn15;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn44;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn16;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn45;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn46;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn47;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn41;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn42;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn43;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn17;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn48;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn18;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn49;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn50;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn51;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn19;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn55;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn20;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn56;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn57;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn58;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn52;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn53;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn54;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn21;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn59;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn22;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn60;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn61;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn62;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn63;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn64;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn65;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn23;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn66;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn24;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn67;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn68;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn69;
        private System.Windows.Forms.ToolStripMenuItem openPhysicalModelFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportPhysicalModelFileToolStripMenuItem;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn70;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn71;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn72;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn25;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn73;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn26;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn74;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn75;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn76;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn77;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn78;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn79;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn27;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn80;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn28;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn81;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn82;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn83;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn10;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn84;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn85;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn86;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn29;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn87;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn30;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn88;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn89;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn90;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn31;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn94;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn32;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn95;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn96;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn97;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn33;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn98;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn34;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn99;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn100;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn101;
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
    }
}