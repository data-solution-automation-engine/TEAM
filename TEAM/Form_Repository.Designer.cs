namespace TEAM
{
    partial class FormManageRepository
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManageRepository));
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.backgroundWorkerSampleData = new System.ComponentModel.BackgroundWorker();
            this.toolTipRepository = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxConfigurationSettings = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.buttonGenerateSampleMapping = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBoxPresentationConnection = new System.Windows.Forms.ComboBox();
            this.comboBoxIntegrationConnection = new System.Windows.Forms.ComboBox();
            this.comboBoxPsaConnection = new System.Windows.Forms.ComboBox();
            this.comboBoxStagingConnection = new System.Windows.Forms.ComboBox();
            this.comboBoxSourceConnection = new System.Windows.Forms.ComboBox();
            this.button3 = new System.Windows.Forms.Button();
            this.linkLabelSource = new System.Windows.Forms.LinkLabel();
            this.checkBoxCreateSamplePresentation = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateSamplePSA = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBoxCreateSampleIntegration = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateSampleStaging = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateSampleSource = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.backgroundWorkerMetadata = new System.ComponentModel.BackgroundWorker();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 85);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 15);
            this.label2.TabIndex = 23;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 88);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 15);
            this.label4.TabIndex = 25;
            // 
            // backgroundWorkerSampleData
            // 
            this.backgroundWorkerSampleData.WorkerReportsProgress = true;
            this.backgroundWorkerSampleData.WorkerSupportsCancellation = true;
            this.backgroundWorkerSampleData.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerSampleData_DoWork);
            this.backgroundWorkerSampleData.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerSampleData_ProgressChanged);
            // 
            // checkBoxConfigurationSettings
            // 
            this.checkBoxConfigurationSettings.AutoSize = true;
            this.checkBoxConfigurationSettings.Checked = true;
            this.checkBoxConfigurationSettings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxConfigurationSettings.Location = new System.Drawing.Point(10, 22);
            this.checkBoxConfigurationSettings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxConfigurationSettings.Name = "checkBoxConfigurationSettings";
            this.checkBoxConfigurationSettings.Size = new System.Drawing.Size(311, 19);
            this.checkBoxConfigurationSettings.TabIndex = 10;
            this.checkBoxConfigurationSettings.Text = "Update and save default TEAM configurations settings";
            this.toolTipRepository.SetToolTip(this.checkBoxConfigurationSettings, resources.GetString("checkBoxConfigurationSettings.ToolTip"));
            this.checkBoxConfigurationSettings.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(7, 48);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(162, 48);
            this.button2.TabIndex = 12;
            this.button2.Text = "Reset standard Configuration Settings";
            this.toolTipRepository.SetToolTip(this.button2, resources.GetString("button2.ToolTip"));
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ButtonSetStandardConfiguration);
            // 
            // buttonGenerateSampleMapping
            // 
            this.buttonGenerateSampleMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGenerateSampleMapping.Location = new System.Drawing.Point(10, 22);
            this.buttonGenerateSampleMapping.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonGenerateSampleMapping.Name = "buttonGenerateSampleMapping";
            this.buttonGenerateSampleMapping.Size = new System.Drawing.Size(162, 48);
            this.buttonGenerateSampleMapping.TabIndex = 2;
            this.buttonGenerateSampleMapping.Text = "Generate Sample Mapping Metadata";
            this.toolTipRepository.SetToolTip(this.buttonGenerateSampleMapping, "Generate sample source-to-target mapping metadata (table mapping, attribute mappi" +
        "ng and physical model snapshot).");
            this.buttonGenerateSampleMapping.UseVisualStyleBackColor = true;
            this.buttonGenerateSampleMapping.Click += new System.EventHandler(this.buttonClick_GenerateMetadata);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.buttonGenerateSampleMapping);
            this.groupBox1.Location = new System.Drawing.Point(14, 14);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(1003, 208);
            this.groupBox1.TabIndex = 72;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mapping Metadata";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 74);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(517, 15);
            this.label7.TabIndex = 88;
            this.label7.Text = "Copies sample JSON mapping metadata for data object, data item and physical model" +
    " metadata.";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboBoxPresentationConnection);
            this.groupBox2.Controls.Add(this.comboBoxIntegrationConnection);
            this.groupBox2.Controls.Add(this.comboBoxPsaConnection);
            this.groupBox2.Controls.Add(this.comboBoxStagingConnection);
            this.groupBox2.Controls.Add(this.comboBoxSourceConnection);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.linkLabelSource);
            this.groupBox2.Controls.Add(this.checkBoxCreateSamplePresentation);
            this.groupBox2.Controls.Add(this.checkBoxCreateSamplePSA);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.checkBoxCreateSampleIntegration);
            this.groupBox2.Controls.Add(this.checkBoxCreateSampleStaging);
            this.groupBox2.Controls.Add(this.checkBoxCreateSampleSource);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(14, 228);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(1003, 215);
            this.groupBox2.TabIndex = 9999;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Sample Database";
            // 
            // comboBoxPresentationConnection
            // 
            this.comboBoxPresentationConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPresentationConnection.FormattingEnabled = true;
            this.comboBoxPresentationConnection.Location = new System.Drawing.Point(572, 126);
            this.comboBoxPresentationConnection.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBoxPresentationConnection.Name = "comboBoxPresentationConnection";
            this.comboBoxPresentationConnection.Size = new System.Drawing.Size(237, 23);
            this.comboBoxPresentationConnection.TabIndex = 90;
            // 
            // comboBoxIntegrationConnection
            // 
            this.comboBoxIntegrationConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIntegrationConnection.FormattingEnabled = true;
            this.comboBoxIntegrationConnection.Location = new System.Drawing.Point(572, 99);
            this.comboBoxIntegrationConnection.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBoxIntegrationConnection.Name = "comboBoxIntegrationConnection";
            this.comboBoxIntegrationConnection.Size = new System.Drawing.Size(237, 23);
            this.comboBoxIntegrationConnection.TabIndex = 89;
            // 
            // comboBoxPsaConnection
            // 
            this.comboBoxPsaConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPsaConnection.FormattingEnabled = true;
            this.comboBoxPsaConnection.Location = new System.Drawing.Point(572, 73);
            this.comboBoxPsaConnection.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBoxPsaConnection.Name = "comboBoxPsaConnection";
            this.comboBoxPsaConnection.Size = new System.Drawing.Size(237, 23);
            this.comboBoxPsaConnection.TabIndex = 88;
            // 
            // comboBoxStagingConnection
            // 
            this.comboBoxStagingConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStagingConnection.FormattingEnabled = true;
            this.comboBoxStagingConnection.Location = new System.Drawing.Point(572, 46);
            this.comboBoxStagingConnection.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBoxStagingConnection.Name = "comboBoxStagingConnection";
            this.comboBoxStagingConnection.Size = new System.Drawing.Size(237, 23);
            this.comboBoxStagingConnection.TabIndex = 87;
            // 
            // comboBoxSourceConnection
            // 
            this.comboBoxSourceConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSourceConnection.FormattingEnabled = true;
            this.comboBoxSourceConnection.Location = new System.Drawing.Point(572, 20);
            this.comboBoxSourceConnection.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBoxSourceConnection.Name = "comboBoxSourceConnection";
            this.comboBoxSourceConnection.Size = new System.Drawing.Size(237, 23);
            this.comboBoxSourceConnection.TabIndex = 86;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(10, 24);
            this.button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(162, 48);
            this.button3.TabIndex = 4;
            this.button3.Text = "Generate Sample Database content";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.ButtonGenerateDatabaseSamples);
            // 
            // linkLabelSource
            // 
            this.linkLabelSource.AutoSize = true;
            this.linkLabelSource.Location = new System.Drawing.Point(883, 190);
            this.linkLabelSource.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabelSource.Name = "linkLabelSource";
            this.linkLabelSource.Size = new System.Drawing.Size(111, 15);
            this.linkLabelSource.TabIndex = 82;
            this.linkLabelSource.TabStop = true;
            this.linkLabelSource.Text = "Link to data models";
            this.linkLabelSource.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSource_LinkClicked);
            // 
            // checkBoxCreateSamplePresentation
            // 
            this.checkBoxCreateSamplePresentation.AutoSize = true;
            this.checkBoxCreateSamplePresentation.Checked = true;
            this.checkBoxCreateSamplePresentation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSamplePresentation.Location = new System.Drawing.Point(273, 128);
            this.checkBoxCreateSamplePresentation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxCreateSamplePresentation.Name = "checkBoxCreateSamplePresentation";
            this.checkBoxCreateSamplePresentation.Size = new System.Drawing.Size(246, 19);
            this.checkBoxCreateSamplePresentation.TabIndex = 9;
            this.checkBoxCreateSamplePresentation.Text = "Create Sample Presentation Layer content\r\n";
            this.checkBoxCreateSamplePresentation.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSamplePSA
            // 
            this.checkBoxCreateSamplePSA.AutoSize = true;
            this.checkBoxCreateSamplePSA.Checked = true;
            this.checkBoxCreateSamplePSA.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSamplePSA.Location = new System.Drawing.Point(273, 75);
            this.checkBoxCreateSamplePSA.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxCreateSamplePSA.Name = "checkBoxCreateSamplePSA";
            this.checkBoxCreateSamplePSA.Size = new System.Drawing.Size(270, 19);
            this.checkBoxCreateSamplePSA.TabIndex = 7;
            this.checkBoxCreateSamplePSA.Text = "Create Sample Persistent Staging Area content";
            this.checkBoxCreateSamplePSA.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 106);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 15);
            this.label6.TabIndex = 77;
            // 
            // checkBoxCreateSampleIntegration
            // 
            this.checkBoxCreateSampleIntegration.AutoSize = true;
            this.checkBoxCreateSampleIntegration.Checked = true;
            this.checkBoxCreateSampleIntegration.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleIntegration.Location = new System.Drawing.Point(273, 102);
            this.checkBoxCreateSampleIntegration.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxCreateSampleIntegration.Name = "checkBoxCreateSampleIntegration";
            this.checkBoxCreateSampleIntegration.Size = new System.Drawing.Size(238, 19);
            this.checkBoxCreateSampleIntegration.TabIndex = 8;
            this.checkBoxCreateSampleIntegration.Text = "Create Sample Integration Layer content";
            this.checkBoxCreateSampleIntegration.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSampleStaging
            // 
            this.checkBoxCreateSampleStaging.AutoSize = true;
            this.checkBoxCreateSampleStaging.Checked = true;
            this.checkBoxCreateSampleStaging.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleStaging.Location = new System.Drawing.Point(273, 48);
            this.checkBoxCreateSampleStaging.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxCreateSampleStaging.Name = "checkBoxCreateSampleStaging";
            this.checkBoxCreateSampleStaging.Size = new System.Drawing.Size(243, 19);
            this.checkBoxCreateSampleStaging.TabIndex = 6;
            this.checkBoxCreateSampleStaging.Text = "Create Sample Staging / Landing content";
            this.checkBoxCreateSampleStaging.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSampleSource
            // 
            this.checkBoxCreateSampleSource.AutoSize = true;
            this.checkBoxCreateSampleSource.Checked = true;
            this.checkBoxCreateSampleSource.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleSource.Location = new System.Drawing.Point(273, 22);
            this.checkBoxCreateSampleSource.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxCreateSampleSource.Name = "checkBoxCreateSampleSource";
            this.checkBoxCreateSampleSource.Size = new System.Drawing.Size(184, 19);
            this.checkBoxCreateSampleSource.TabIndex = 5;
            this.checkBoxCreateSampleSource.Text = "Create sample Source content";
            this.checkBoxCreateSampleSource.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 181);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(437, 30);
            this.label3.TabIndex = 73;
            this.label3.Text = "WARNING - the databases associated with the selected connections must already \r\ne" +
    "xist and existing information will be removed.";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.button2);
            this.groupBox4.Controls.Add(this.checkBoxConfigurationSettings);
            this.groupBox4.Location = new System.Drawing.Point(14, 450);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Size = new System.Drawing.Size(1003, 185);
            this.groupBox4.TabIndex = 88;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Generic options";
            // 
            // backgroundWorkerMetadata
            // 
            this.backgroundWorkerMetadata.WorkerReportsProgress = true;
            this.backgroundWorkerMetadata.WorkerSupportsCancellation = true;
            this.backgroundWorkerMetadata.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerMetadata_DoWork);
            // 
            // FormManageRepository
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1031, 820);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.MaximumSize = new System.Drawing.Size(1047, 859);
            this.MinimumSize = new System.Drawing.Size(1047, 859);
            this.Name = "FormManageRepository";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Create / Rebuild Repository";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.ComponentModel.BackgroundWorker backgroundWorkerSampleData;
        private System.Windows.Forms.ToolTip toolTipRepository;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        internal System.Windows.Forms.CheckBox checkBoxCreateSamplePresentation;
        internal System.Windows.Forms.CheckBox checkBoxCreateSamplePSA;
        private System.Windows.Forms.Label label6;
        internal System.Windows.Forms.CheckBox checkBoxCreateSampleIntegration;
        internal System.Windows.Forms.CheckBox checkBoxCreateSampleStaging;
        internal System.Windows.Forms.CheckBox checkBoxCreateSampleSource;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonGenerateSampleMapping;
        private System.Windows.Forms.LinkLabel linkLabelSource;
        internal System.Windows.Forms.CheckBox checkBoxConfigurationSettings;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.ComponentModel.BackgroundWorker backgroundWorkerMetadata;
        private System.Windows.Forms.ComboBox comboBoxPresentationConnection;
        private System.Windows.Forms.ComboBox comboBoxIntegrationConnection;
        private System.Windows.Forms.ComboBox comboBoxPsaConnection;
        private System.Windows.Forms.ComboBox comboBoxStagingConnection;
        private System.Windows.Forms.ComboBox comboBoxSourceConnection;
    }
}