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
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FormManageMetadata));
            backgroundWorkerParse = new BackgroundWorker();
            groupBox2 = new System.Windows.Forms.GroupBox();
            checkBoxShowStaging = new System.Windows.Forms.CheckBox();
            textBoxFilterCriterion = new TimedTextBox();
            labelResult = new System.Windows.Forms.Label();
            buttonStart = new System.Windows.Forms.Button();
            buttonSaveMetadata = new System.Windows.Forms.Button();
            groupBoxMetadataCounts = new System.Windows.Forms.GroupBox();
            labelLsatCount = new System.Windows.Forms.Label();
            labelLnkCount = new System.Windows.Forms.Label();
            labelSatCount = new System.Windows.Forms.Label();
            labelHubCount = new System.Windows.Forms.Label();
            tabControlDataMappings = new System.Windows.Forms.TabControl();
            groupBox4 = new System.Windows.Forms.GroupBox();
            checkedListBoxReverseEngineeringAreas = new System.Windows.Forms.CheckedListBox();
            buttonReverseEngineer = new System.Windows.Forms.Button();
            contextMenuStripModel = new System.Windows.Forms.ContextMenuStrip(components);
            displayTableScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            MetadataGenerationGroupBox = new System.Windows.Forms.GroupBox();
            checkBoxValidation = new System.Windows.Forms.CheckBox();
            labelInformation = new System.Windows.Forms.Label();
            richTextBoxInformation = new System.Windows.Forms.RichTextBox();
            menuStripMetadata = new System.Windows.Forms.MenuStrip();
            metadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openMetadataDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openConfigurationDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            businessKeyMetadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openMetadataFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openAttributeMappingFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importPhysicalModelGridFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            automapDataItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            generatePhysicalModelGridQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            validationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            manageValidationRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            validateMetadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            jsonExportConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            manageJsonExportRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            displayEventLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolTipMetadata = new System.Windows.Forms.ToolTip(components);
            backgroundWorkerValidationOnly = new BackgroundWorker();
            backgroundWorkerEventLog = new BackgroundWorker();
            groupBoxPhysicalModel = new System.Windows.Forms.GroupBox();
            labelConnections = new System.Windows.Forms.Label();
            openCoreDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            groupBox2.SuspendLayout();
            groupBoxMetadataCounts.SuspendLayout();
            MetadataGenerationGroupBox.SuspendLayout();
            menuStripMetadata.SuspendLayout();
            groupBoxPhysicalModel.SuspendLayout();
            SuspendLayout();
            // 
            // backgroundWorkerParse
            // 
            backgroundWorkerParse.WorkerReportsProgress = true;
            backgroundWorkerParse.WorkerSupportsCancellation = true;
            backgroundWorkerParse.DoWork += backgroundWorkerParse_DoWork;
            backgroundWorkerParse.ProgressChanged += backgroundWorkerParse_ProgressChanged;
            backgroundWorkerParse.RunWorkerCompleted += backgroundWorkerParse_RunWorkerCompleted;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            groupBox2.Controls.Add(checkBoxShowStaging);
            groupBox2.Controls.Add(textBoxFilterCriterion);
            groupBox2.Location = new System.Drawing.Point(19, 853);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(262, 98);
            groupBox2.TabIndex = 25;
            groupBox2.TabStop = false;
            groupBox2.Text = "Filter Criterion";
            // 
            // checkBoxShowStaging
            // 
            checkBoxShowStaging.AutoSize = true;
            checkBoxShowStaging.Checked = true;
            checkBoxShowStaging.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxShowStaging.Location = new System.Drawing.Point(7, 52);
            checkBoxShowStaging.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxShowStaging.Name = "checkBoxShowStaging";
            checkBoxShowStaging.Size = new System.Drawing.Size(166, 19);
            checkBoxShowStaging.TabIndex = 24;
            checkBoxShowStaging.Text = "Show Staging Layer details";
            toolTipMetadata.SetToolTip(checkBoxShowStaging, "Show (or hide) Staging Layer mappings is a broad filter that hides all source-to-staging and staging-to-persistent-staging data object mappings.");
            checkBoxShowStaging.UseVisualStyleBackColor = true;
            checkBoxShowStaging.CheckedChanged += checkBoxShowStaging_CheckedChanged;
            // 
            // textBoxFilterCriterion
            // 
            textBoxFilterCriterion.DelayedTextChangedTimeout = 1000;
            textBoxFilterCriterion.Location = new System.Drawing.Point(7, 18);
            textBoxFilterCriterion.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxFilterCriterion.Name = "textBoxFilterCriterion";
            textBoxFilterCriterion.Size = new System.Drawing.Size(248, 23);
            textBoxFilterCriterion.TabIndex = 23;
            textBoxFilterCriterion.DelayedTextChanged += TextBoxFilterCriterion_OnDelayedTextChanged;
            // 
            // labelResult
            // 
            labelResult.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelResult.AutoSize = true;
            labelResult.Location = new System.Drawing.Point(7, 155);
            labelResult.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelResult.Name = "labelResult";
            labelResult.Size = new System.Drawing.Size(39, 15);
            labelResult.TabIndex = 23;
            labelResult.Text = "Ready";
            // 
            // buttonStart
            // 
            buttonStart.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonStart.Location = new System.Drawing.Point(8, 105);
            buttonStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new System.Drawing.Size(140, 46);
            buttonStart.TabIndex = 22;
            buttonStart.Text = "&Parse Metadata";
            toolTipMetadata.SetToolTip(buttonStart, resources.GetString("buttonStart.ToolTip"));
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += ButtonParse_Click;
            // 
            // buttonSaveMetadata
            // 
            buttonSaveMetadata.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonSaveMetadata.Location = new System.Drawing.Point(7, 52);
            buttonSaveMetadata.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonSaveMetadata.Name = "buttonSaveMetadata";
            buttonSaveMetadata.Size = new System.Drawing.Size(140, 46);
            buttonSaveMetadata.TabIndex = 1;
            buttonSaveMetadata.Text = "&Save Metadata";
            toolTipMetadata.SetToolTip(buttonSaveMetadata, "Save the metadata changes to file. This will directly \r\nupdate the JSON files in the metadata directory.");
            buttonSaveMetadata.UseVisualStyleBackColor = true;
            buttonSaveMetadata.Click += ButtonSaveMetadata_Click;
            // 
            // groupBoxMetadataCounts
            // 
            groupBoxMetadataCounts.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            groupBoxMetadataCounts.Controls.Add(labelLsatCount);
            groupBoxMetadataCounts.Controls.Add(labelLnkCount);
            groupBoxMetadataCounts.Controls.Add(labelSatCount);
            groupBoxMetadataCounts.Controls.Add(labelHubCount);
            groupBoxMetadataCounts.Location = new System.Drawing.Point(1614, 853);
            groupBoxMetadataCounts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxMetadataCounts.Name = "groupBoxMetadataCounts";
            groupBoxMetadataCounts.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxMetadataCounts.Size = new System.Drawing.Size(163, 98);
            groupBoxMetadataCounts.TabIndex = 16;
            groupBoxMetadataCounts.TabStop = false;
            groupBoxMetadataCounts.Text = "This metadata contains:";
            // 
            // labelLsatCount
            // 
            labelLsatCount.AutoSize = true;
            labelLsatCount.Location = new System.Drawing.Point(7, 68);
            labelLsatCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelLsatCount.Name = "labelLsatCount";
            labelLsatCount.Size = new System.Drawing.Size(86, 15);
            labelLsatCount.TabIndex = 3;
            labelLsatCount.Text = "labelLsatCount";
            // 
            // labelLnkCount
            // 
            labelLnkCount.AutoSize = true;
            labelLnkCount.Location = new System.Drawing.Point(7, 53);
            labelLnkCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelLnkCount.Name = "labelLnkCount";
            labelLnkCount.Size = new System.Drawing.Size(84, 15);
            labelLnkCount.TabIndex = 2;
            labelLnkCount.Text = "labelLnkCount";
            // 
            // labelSatCount
            // 
            labelSatCount.AutoSize = true;
            labelSatCount.Location = new System.Drawing.Point(7, 38);
            labelSatCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelSatCount.Name = "labelSatCount";
            labelSatCount.Size = new System.Drawing.Size(81, 15);
            labelSatCount.TabIndex = 1;
            labelSatCount.Text = "labelSatCount";
            // 
            // labelHubCount
            // 
            labelHubCount.AutoSize = true;
            labelHubCount.Location = new System.Drawing.Point(7, 23);
            labelHubCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelHubCount.Name = "labelHubCount";
            labelHubCount.Size = new System.Drawing.Size(88, 15);
            labelHubCount.TabIndex = 0;
            labelHubCount.Text = "labelHubCount";
            // 
            // tabControlDataMappings
            // 
            tabControlDataMappings.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControlDataMappings.Location = new System.Drawing.Point(14, 31);
            tabControlDataMappings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabControlDataMappings.Name = "tabControlDataMappings";
            tabControlDataMappings.SelectedIndex = 4;
            tabControlDataMappings.Size = new System.Drawing.Size(1591, 815);
            tabControlDataMappings.TabIndex = 15;
            tabControlDataMappings.SelectedIndexChanged += tabControlDataMappings_SelectedIndexChanged;
            // 
            // groupBox4
            // 
            groupBox4.Location = new System.Drawing.Point(0, 0);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size(200, 100);
            groupBox4.TabIndex = 0;
            groupBox4.TabStop = false;
            // 
            // checkedListBoxReverseEngineeringAreas
            // 
            checkedListBoxReverseEngineeringAreas.BackColor = System.Drawing.SystemColors.Control;
            checkedListBoxReverseEngineeringAreas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            checkedListBoxReverseEngineeringAreas.FormattingEnabled = true;
            checkedListBoxReverseEngineeringAreas.Location = new System.Drawing.Point(9, 98);
            checkedListBoxReverseEngineeringAreas.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkedListBoxReverseEngineeringAreas.Name = "checkedListBoxReverseEngineeringAreas";
            checkedListBoxReverseEngineeringAreas.Size = new System.Drawing.Size(140, 468);
            checkedListBoxReverseEngineeringAreas.TabIndex = 0;
            // 
            // buttonReverseEngineer
            // 
            buttonReverseEngineer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonReverseEngineer.Location = new System.Drawing.Point(8, 22);
            buttonReverseEngineer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonReverseEngineer.Name = "buttonReverseEngineer";
            buttonReverseEngineer.Size = new System.Drawing.Size(140, 46);
            buttonReverseEngineer.TabIndex = 20;
            buttonReverseEngineer.Text = "&Reverse Engineer";
            toolTipMetadata.SetToolTip(buttonReverseEngineer, resources.GetString("buttonReverseEngineer.ToolTip"));
            buttonReverseEngineer.UseVisualStyleBackColor = true;
            buttonReverseEngineer.Click += ButtonReverseEngineerMetadataClick;
            // 
            // contextMenuStripModel
            // 
            contextMenuStripModel.Name = "contextMenuStripModel";
            contextMenuStripModel.Size = new System.Drawing.Size(61, 4);
            // 
            // displayTableScriptToolStripMenuItem
            // 
            displayTableScriptToolStripMenuItem.Name = "displayTableScriptToolStripMenuItem";
            displayTableScriptToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            displayTableScriptToolStripMenuItem.Text = "Display table script";
            displayTableScriptToolStripMenuItem.Click += displayTableScriptToolStripMenuItem_Click;
            // 
            // MetadataGenerationGroupBox
            // 
            MetadataGenerationGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            MetadataGenerationGroupBox.Controls.Add(checkBoxValidation);
            MetadataGenerationGroupBox.Controls.Add(buttonSaveMetadata);
            MetadataGenerationGroupBox.Controls.Add(buttonStart);
            MetadataGenerationGroupBox.Controls.Add(labelResult);
            MetadataGenerationGroupBox.Location = new System.Drawing.Point(1614, 57);
            MetadataGenerationGroupBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MetadataGenerationGroupBox.Name = "MetadataGenerationGroupBox";
            MetadataGenerationGroupBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MetadataGenerationGroupBox.Size = new System.Drawing.Size(163, 182);
            MetadataGenerationGroupBox.TabIndex = 3;
            MetadataGenerationGroupBox.TabStop = false;
            MetadataGenerationGroupBox.Text = "Processing";
            // 
            // checkBoxValidation
            // 
            checkBoxValidation.AutoSize = true;
            checkBoxValidation.Checked = true;
            checkBoxValidation.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxValidation.Location = new System.Drawing.Point(9, 25);
            checkBoxValidation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxValidation.Name = "checkBoxValidation";
            checkBoxValidation.Size = new System.Drawing.Size(120, 19);
            checkBoxValidation.TabIndex = 10;
            checkBoxValidation.Text = "Validate metadata";
            checkBoxValidation.UseVisualStyleBackColor = true;
            // 
            // labelInformation
            // 
            labelInformation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            labelInformation.AutoSize = true;
            labelInformation.Location = new System.Drawing.Point(288, 853);
            labelInformation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelInformation.Name = "labelInformation";
            labelInformation.Size = new System.Drawing.Size(70, 15);
            labelInformation.TabIndex = 5;
            labelInformation.Text = "Information";
            // 
            // richTextBoxInformation
            // 
            richTextBoxInformation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBoxInformation.BackColor = System.Drawing.SystemColors.Control;
            richTextBoxInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richTextBoxInformation.Location = new System.Drawing.Point(288, 871);
            richTextBoxInformation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            richTextBoxInformation.Name = "richTextBoxInformation";
            richTextBoxInformation.Size = new System.Drawing.Size(1309, 80);
            richTextBoxInformation.TabIndex = 2;
            richTextBoxInformation.Text = "";
            richTextBoxInformation.TextChanged += richTextBoxInformation_TextChanged;
            // 
            // menuStripMetadata
            // 
            menuStripMetadata.ImageScalingSize = new System.Drawing.Size(24, 24);
            menuStripMetadata.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { metadataToolStripMenuItem, businessKeyMetadataToolStripMenuItem, validationToolStripMenuItem, jsonExportConfigurationToolStripMenuItem, helpToolStripMenuItem });
            menuStripMetadata.Location = new System.Drawing.Point(0, 0);
            menuStripMetadata.Name = "menuStripMetadata";
            menuStripMetadata.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            menuStripMetadata.Size = new System.Drawing.Size(1790, 24);
            menuStripMetadata.TabIndex = 3;
            menuStripMetadata.Text = "menuStrip1";
            // 
            // metadataToolStripMenuItem
            // 
            metadataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openMetadataDirectoryToolStripMenuItem, openConfigurationDirectoryToolStripMenuItem, openCoreDirectoryToolStripMenuItem, toolStripSeparator5, saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem, closeToolStripMenuItem });
            metadataToolStripMenuItem.Name = "metadataToolStripMenuItem";
            metadataToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            metadataToolStripMenuItem.Text = "&File";
            // 
            // openMetadataDirectoryToolStripMenuItem
            // 
            openMetadataDirectoryToolStripMenuItem.Image = Properties.Resources.OpenDirectoryIcon;
            openMetadataDirectoryToolStripMenuItem.Name = "openMetadataDirectoryToolStripMenuItem";
            openMetadataDirectoryToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            openMetadataDirectoryToolStripMenuItem.Size = new System.Drawing.Size(358, 30);
            openMetadataDirectoryToolStripMenuItem.Text = "Open Metadata Directory";
            openMetadataDirectoryToolStripMenuItem.Click += openMetadataDirectoryToolStripMenuItem_Click;
            // 
            // openConfigurationDirectoryToolStripMenuItem
            // 
            openConfigurationDirectoryToolStripMenuItem.Image = Properties.Resources.OpenDirectoryIcon;
            openConfigurationDirectoryToolStripMenuItem.Name = "openConfigurationDirectoryToolStripMenuItem";
            openConfigurationDirectoryToolStripMenuItem.Size = new System.Drawing.Size(358, 30);
            openConfigurationDirectoryToolStripMenuItem.Text = "Open Configuration Directory";
            openConfigurationDirectoryToolStripMenuItem.Click += openConfigurationDirectoryToolStripMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(355, 6);
            // 
            // saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem
            // 
            saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem.Image = Properties.Resources.SaveFile;
            saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem.Name = "saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem";
            saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem.Size = new System.Drawing.Size(358, 30);
            saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem.Text = "&Save as Directional Graph Markup Language (DGML)";
            saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem.Click += saveAsDirectionalGraphMarkupLanguageDgmlToolStripMenuItem_Click;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Image = Properties.Resources.ExitApplication;
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new System.Drawing.Size(358, 30);
            closeToolStripMenuItem.Text = "&Close Window";
            closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
            // 
            // businessKeyMetadataToolStripMenuItem
            // 
            businessKeyMetadataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openMetadataFileToolStripMenuItem, openAttributeMappingFileToolStripMenuItem, importPhysicalModelGridFileToolStripMenuItem, toolStripSeparator1, automapDataItemsToolStripMenuItem, generatePhysicalModelGridQueryToolStripMenuItem });
            businessKeyMetadataToolStripMenuItem.Name = "businessKeyMetadataToolStripMenuItem";
            businessKeyMetadataToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
            businessKeyMetadataToolStripMenuItem.Text = "Metadata";
            // 
            // openMetadataFileToolStripMenuItem
            // 
            openMetadataFileToolStripMenuItem.Image = Properties.Resources.OpenFileIcon;
            openMetadataFileToolStripMenuItem.Name = "openMetadataFileToolStripMenuItem";
            openMetadataFileToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            openMetadataFileToolStripMenuItem.Text = "Import Data Object Mapping Grid File";
            openMetadataFileToolStripMenuItem.Click += openMetadataFileToolStripMenuItem_Click_1;
            // 
            // openAttributeMappingFileToolStripMenuItem
            // 
            openAttributeMappingFileToolStripMenuItem.Image = Properties.Resources.OpenFileIcon;
            openAttributeMappingFileToolStripMenuItem.Name = "openAttributeMappingFileToolStripMenuItem";
            openAttributeMappingFileToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            openAttributeMappingFileToolStripMenuItem.Text = "Import Data Item Mapping Grid File";
            openAttributeMappingFileToolStripMenuItem.Click += openDataItemMappingFileToolStripMenuItem_Click;
            // 
            // importPhysicalModelGridFileToolStripMenuItem
            // 
            importPhysicalModelGridFileToolStripMenuItem.Image = Properties.Resources.OpenFileIcon;
            importPhysicalModelGridFileToolStripMenuItem.Name = "importPhysicalModelGridFileToolStripMenuItem";
            importPhysicalModelGridFileToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            importPhysicalModelGridFileToolStripMenuItem.Text = "Import Physical Model Grid File";
            importPhysicalModelGridFileToolStripMenuItem.Click += importPhysicalModelGridFileToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(303, 6);
            // 
            // automapDataItemsToolStripMenuItem
            // 
            automapDataItemsToolStripMenuItem.Image = Properties.Resources.CogIcon;
            automapDataItemsToolStripMenuItem.Name = "automapDataItemsToolStripMenuItem";
            automapDataItemsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M;
            automapDataItemsToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            automapDataItemsToolStripMenuItem.Text = "Automap Data Items";
            automapDataItemsToolStripMenuItem.Click += AutoMapDataItemsToolStripMenuItem_Click;
            // 
            // generatePhysicalModelGridQueryToolStripMenuItem
            // 
            generatePhysicalModelGridQueryToolStripMenuItem.Image = Properties.Resources.ETLIcon;
            generatePhysicalModelGridQueryToolStripMenuItem.Name = "generatePhysicalModelGridQueryToolStripMenuItem";
            generatePhysicalModelGridQueryToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G;
            generatePhysicalModelGridQueryToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            generatePhysicalModelGridQueryToolStripMenuItem.Text = "&Generate Physical Model Grid Query";
            generatePhysicalModelGridQueryToolStripMenuItem.Click += generatePhysicalModelGridQueryToolStripMenuItem_Click;
            // 
            // validationToolStripMenuItem
            // 
            validationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { manageValidationRulesToolStripMenuItem, validateMetadataToolStripMenuItem });
            validationToolStripMenuItem.Name = "validationToolStripMenuItem";
            validationToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            validationToolStripMenuItem.Text = "&Validation";
            // 
            // manageValidationRulesToolStripMenuItem
            // 
            manageValidationRulesToolStripMenuItem.Image = Properties.Resources.DocumentationIcon;
            manageValidationRulesToolStripMenuItem.Name = "manageValidationRulesToolStripMenuItem";
            manageValidationRulesToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            manageValidationRulesToolStripMenuItem.Text = "Manage Validation Rules";
            manageValidationRulesToolStripMenuItem.Click += manageValidationRulesToolStripMenuItem_Click;
            // 
            // validateMetadataToolStripMenuItem
            // 
            validateMetadataToolStripMenuItem.Image = Properties.Resources.CogIcon;
            validateMetadataToolStripMenuItem.Name = "validateMetadataToolStripMenuItem";
            validateMetadataToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q;
            validateMetadataToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            validateMetadataToolStripMenuItem.Text = "&Validate Metadata";
            validateMetadataToolStripMenuItem.Click += validateMetadataToolStripMenuItem_Click;
            // 
            // jsonExportConfigurationToolStripMenuItem
            // 
            jsonExportConfigurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { manageJsonExportRulesToolStripMenuItem });
            jsonExportConfigurationToolStripMenuItem.Name = "jsonExportConfigurationToolStripMenuItem";
            jsonExportConfigurationToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            jsonExportConfigurationToolStripMenuItem.Text = "&JSON";
            // 
            // manageJsonExportRulesToolStripMenuItem
            // 
            manageJsonExportRulesToolStripMenuItem.Image = Properties.Resources.DocumentationIcon;
            manageJsonExportRulesToolStripMenuItem.Name = "manageJsonExportRulesToolStripMenuItem";
            manageJsonExportRulesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            manageJsonExportRulesToolStripMenuItem.Text = "Manage JSON Export Rules";
            manageJsonExportRulesToolStripMenuItem.Click += manageJsonExportRulesToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { displayEventLogToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // displayEventLogToolStripMenuItem
            // 
            displayEventLogToolStripMenuItem.Image = Properties.Resources.log_file;
            displayEventLogToolStripMenuItem.Name = "displayEventLogToolStripMenuItem";
            displayEventLogToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            displayEventLogToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            displayEventLogToolStripMenuItem.Text = "Display &Event Log";
            displayEventLogToolStripMenuItem.Click += displayEventLogToolStripMenuItem_Click;
            // 
            // backgroundWorkerValidationOnly
            // 
            backgroundWorkerValidationOnly.WorkerReportsProgress = true;
            backgroundWorkerValidationOnly.WorkerSupportsCancellation = true;
            backgroundWorkerValidationOnly.DoWork += BackgroundWorkerValidation_DoWork;
            backgroundWorkerValidationOnly.ProgressChanged += backgroundWorkerValidationOnly_ProgressChanged;
            backgroundWorkerValidationOnly.RunWorkerCompleted += backgroundWorkerValidationOnly_RunWorkerCompleted;
            // 
            // backgroundWorkerEventLog
            // 
            backgroundWorkerEventLog.DoWork += backgroundWorkerEventLog_DoWork;
            backgroundWorkerEventLog.ProgressChanged += backgroundWorkerEventLog_ProgressChanged;
            backgroundWorkerEventLog.RunWorkerCompleted += backgroundWorkerEventLog_RunWorkerCompleted;
            // 
            // groupBoxPhysicalModel
            // 
            groupBoxPhysicalModel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            groupBoxPhysicalModel.Controls.Add(labelConnections);
            groupBoxPhysicalModel.Controls.Add(checkedListBoxReverseEngineeringAreas);
            groupBoxPhysicalModel.Controls.Add(buttonReverseEngineer);
            groupBoxPhysicalModel.Location = new System.Drawing.Point(1614, 246);
            groupBoxPhysicalModel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxPhysicalModel.Name = "groupBoxPhysicalModel";
            groupBoxPhysicalModel.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxPhysicalModel.Size = new System.Drawing.Size(163, 600);
            groupBoxPhysicalModel.TabIndex = 1;
            groupBoxPhysicalModel.TabStop = false;
            groupBoxPhysicalModel.Text = "Physical Model";
            // 
            // labelConnections
            // 
            labelConnections.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelConnections.AutoSize = true;
            labelConnections.Location = new System.Drawing.Point(9, 74);
            labelConnections.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelConnections.Name = "labelConnections";
            labelConnections.Size = new System.Drawing.Size(74, 15);
            labelConnections.TabIndex = 24;
            labelConnections.Text = "Connections";
            // 
            // openCoreDirectoryToolStripMenuItem
            // 
            openCoreDirectoryToolStripMenuItem.Image = Properties.Resources.OpenDirectoryIcon;
            openCoreDirectoryToolStripMenuItem.Name = "openCoreDirectoryToolStripMenuItem";
            openCoreDirectoryToolStripMenuItem.Size = new System.Drawing.Size(358, 30);
            openCoreDirectoryToolStripMenuItem.Text = "Open Core Directory";
            openCoreDirectoryToolStripMenuItem.Click += openCoreDirectoryToolStripMenuItem_Click;
            // 
            // FormManageMetadata
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1790, 965);
            Controls.Add(groupBoxPhysicalModel);
            Controls.Add(groupBox2);
            Controls.Add(groupBoxMetadataCounts);
            Controls.Add(tabControlDataMappings);
            Controls.Add(MetadataGenerationGroupBox);
            Controls.Add(labelInformation);
            Controls.Add(richTextBoxInformation);
            Controls.Add(menuStripMetadata);
            HelpButton = true;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripMetadata;
            Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            MinimumSize = new System.Drawing.Size(1806, 1004);
            Name = "FormManageMetadata";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Manage the automation metadata";
            ResizeBegin += FormManageMetadata_ResizeBegin;
            ResizeEnd += FormManageMetadata_ResizeEnd;
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBoxMetadataCounts.ResumeLayout(false);
            groupBoxMetadataCounts.PerformLayout();
            MetadataGenerationGroupBox.ResumeLayout(false);
            MetadataGenerationGroupBox.PerformLayout();
            menuStripMetadata.ResumeLayout(false);
            menuStripMetadata.PerformLayout();
            groupBoxPhysicalModel.ResumeLayout(false);
            groupBoxPhysicalModel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.Button buttonSaveMetadata;
        private System.Windows.Forms.ToolStripMenuItem businessKeyMetadataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMetadataFileToolStripMenuItem;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Label labelResult;
        private System.ComponentModel.BackgroundWorker backgroundWorkerParse;
        private System.Windows.Forms.ToolStripMenuItem saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox2;
        private TimedTextBox textBoxFilterCriterion;


        private System.Windows.Forms.Button buttonReverseEngineer;


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
        private System.Windows.Forms.ToolStripMenuItem automapDataItemsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMetadataDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;


        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.GroupBox groupBoxPhysicalModel;
        private System.Windows.Forms.Label labelConnections;
        private System.Windows.Forms.ToolStripMenuItem generatePhysicalModelGridQueryToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxShowStaging;
        private System.Windows.Forms.ToolStripMenuItem importPhysicalModelGridFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openCoreDirectoryToolStripMenuItem;
    }
}