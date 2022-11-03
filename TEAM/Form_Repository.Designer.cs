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
            this.label2.Location = new System.Drawing.Point(3, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 13);
            this.label2.TabIndex = 23;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 13);
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
            this.checkBoxConfigurationSettings.Location = new System.Drawing.Point(9, 19);
            this.checkBoxConfigurationSettings.Name = "checkBoxConfigurationSettings";
            this.checkBoxConfigurationSettings.Size = new System.Drawing.Size(284, 17);
            this.checkBoxConfigurationSettings.TabIndex = 10;
            this.checkBoxConfigurationSettings.Text = "Update and save default TEAM configurations settings";
            this.toolTipRepository.SetToolTip(this.checkBoxConfigurationSettings, resources.GetString("checkBoxConfigurationSettings.ToolTip"));
            this.checkBoxConfigurationSettings.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(6, 42);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(139, 42);
            this.button2.TabIndex = 12;
            this.button2.Text = "Reset standard Configuration Settings";
            this.toolTipRepository.SetToolTip(this.button2, resources.GetString("button2.ToolTip"));
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ButtonSetStandardConfiguration);
            // 
            // buttonGenerateSampleMapping
            // 
            this.buttonGenerateSampleMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGenerateSampleMapping.Location = new System.Drawing.Point(9, 19);
            this.buttonGenerateSampleMapping.Name = "buttonGenerateSampleMapping";
            this.buttonGenerateSampleMapping.Size = new System.Drawing.Size(139, 42);
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
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(860, 180);
            this.groupBox1.TabIndex = 72;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mapping Metadata";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 64);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(340, 52);
            this.label7.TabIndex = 88;
            this.label7.Text = "Generates the sample mapping metadata.\r\n\r\nIn virtual mode, only this metadata is " +
    "required to prepare the metadata. \r\nFor physical mode, the database need to exis" +
    "ts as well (see below).";
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
            this.groupBox2.Location = new System.Drawing.Point(12, 198);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(860, 186);
            this.groupBox2.TabIndex = 9999;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Sample Database";
            // 
            // comboBoxPresentationConnection
            // 
            this.comboBoxPresentationConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPresentationConnection.FormattingEnabled = true;
            this.comboBoxPresentationConnection.Location = new System.Drawing.Point(490, 109);
            this.comboBoxPresentationConnection.Name = "comboBoxPresentationConnection";
            this.comboBoxPresentationConnection.Size = new System.Drawing.Size(204, 21);
            this.comboBoxPresentationConnection.TabIndex = 90;
            // 
            // comboBoxIntegrationConnection
            // 
            this.comboBoxIntegrationConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIntegrationConnection.FormattingEnabled = true;
            this.comboBoxIntegrationConnection.Location = new System.Drawing.Point(490, 86);
            this.comboBoxIntegrationConnection.Name = "comboBoxIntegrationConnection";
            this.comboBoxIntegrationConnection.Size = new System.Drawing.Size(204, 21);
            this.comboBoxIntegrationConnection.TabIndex = 89;
            // 
            // comboBoxPsaConnection
            // 
            this.comboBoxPsaConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPsaConnection.FormattingEnabled = true;
            this.comboBoxPsaConnection.Location = new System.Drawing.Point(490, 63);
            this.comboBoxPsaConnection.Name = "comboBoxPsaConnection";
            this.comboBoxPsaConnection.Size = new System.Drawing.Size(204, 21);
            this.comboBoxPsaConnection.TabIndex = 88;
            // 
            // comboBoxStagingConnection
            // 
            this.comboBoxStagingConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStagingConnection.FormattingEnabled = true;
            this.comboBoxStagingConnection.Location = new System.Drawing.Point(490, 40);
            this.comboBoxStagingConnection.Name = "comboBoxStagingConnection";
            this.comboBoxStagingConnection.Size = new System.Drawing.Size(204, 21);
            this.comboBoxStagingConnection.TabIndex = 87;
            // 
            // comboBoxSourceConnection
            // 
            this.comboBoxSourceConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSourceConnection.FormattingEnabled = true;
            this.comboBoxSourceConnection.Location = new System.Drawing.Point(490, 17);
            this.comboBoxSourceConnection.Name = "comboBoxSourceConnection";
            this.comboBoxSourceConnection.Size = new System.Drawing.Size(204, 21);
            this.comboBoxSourceConnection.TabIndex = 86;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(9, 21);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(139, 42);
            this.button3.TabIndex = 4;
            this.button3.Text = "Generate Sample Database content";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.ButtonGenerateDatabaseSamples);
            // 
            // linkLabelSource
            // 
            this.linkLabelSource.AutoSize = true;
            this.linkLabelSource.Location = new System.Drawing.Point(757, 165);
            this.linkLabelSource.Name = "linkLabelSource";
            this.linkLabelSource.Size = new System.Drawing.Size(99, 13);
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
            this.checkBoxCreateSamplePresentation.Location = new System.Drawing.Point(234, 111);
            this.checkBoxCreateSamplePresentation.Name = "checkBoxCreateSamplePresentation";
            this.checkBoxCreateSamplePresentation.Size = new System.Drawing.Size(225, 17);
            this.checkBoxCreateSamplePresentation.TabIndex = 9;
            this.checkBoxCreateSamplePresentation.Text = "Create Sample Presentation Layer content\r\n";
            this.checkBoxCreateSamplePresentation.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSamplePSA
            // 
            this.checkBoxCreateSamplePSA.AutoSize = true;
            this.checkBoxCreateSamplePSA.Checked = true;
            this.checkBoxCreateSamplePSA.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSamplePSA.Location = new System.Drawing.Point(234, 65);
            this.checkBoxCreateSamplePSA.Name = "checkBoxCreateSamplePSA";
            this.checkBoxCreateSamplePSA.Size = new System.Drawing.Size(247, 17);
            this.checkBoxCreateSamplePSA.TabIndex = 7;
            this.checkBoxCreateSamplePSA.Text = "Create Sample Persistent Staging Area content";
            this.checkBoxCreateSamplePSA.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 92);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 13);
            this.label6.TabIndex = 77;
            // 
            // checkBoxCreateSampleIntegration
            // 
            this.checkBoxCreateSampleIntegration.AutoSize = true;
            this.checkBoxCreateSampleIntegration.Checked = true;
            this.checkBoxCreateSampleIntegration.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleIntegration.Location = new System.Drawing.Point(234, 88);
            this.checkBoxCreateSampleIntegration.Name = "checkBoxCreateSampleIntegration";
            this.checkBoxCreateSampleIntegration.Size = new System.Drawing.Size(216, 17);
            this.checkBoxCreateSampleIntegration.TabIndex = 8;
            this.checkBoxCreateSampleIntegration.Text = "Create Sample Integration Layer content";
            this.checkBoxCreateSampleIntegration.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSampleStaging
            // 
            this.checkBoxCreateSampleStaging.AutoSize = true;
            this.checkBoxCreateSampleStaging.Checked = true;
            this.checkBoxCreateSampleStaging.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleStaging.Location = new System.Drawing.Point(234, 42);
            this.checkBoxCreateSampleStaging.Name = "checkBoxCreateSampleStaging";
            this.checkBoxCreateSampleStaging.Size = new System.Drawing.Size(222, 17);
            this.checkBoxCreateSampleStaging.TabIndex = 6;
            this.checkBoxCreateSampleStaging.Text = "Create Sample Staging / Landing content";
            this.checkBoxCreateSampleStaging.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSampleSource
            // 
            this.checkBoxCreateSampleSource.AutoSize = true;
            this.checkBoxCreateSampleSource.Checked = true;
            this.checkBoxCreateSampleSource.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleSource.Location = new System.Drawing.Point(234, 19);
            this.checkBoxCreateSampleSource.Name = "checkBoxCreateSampleSource";
            this.checkBoxCreateSampleSource.Size = new System.Drawing.Size(169, 17);
            this.checkBoxCreateSampleSource.TabIndex = 5;
            this.checkBoxCreateSampleSource.Text = "Create sample Source content";
            this.checkBoxCreateSampleSource.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 157);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(399, 26);
            this.label3.TabIndex = 73;
            this.label3.Text = "WARNING - the databases associated with the selected connections must already \r\ne" +
    "xist and existing information will be removed.";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.button2);
            this.groupBox4.Controls.Add(this.checkBoxConfigurationSettings);
            this.groupBox4.Location = new System.Drawing.Point(12, 390);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(860, 160);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 711);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(900, 750);
            this.MinimumSize = new System.Drawing.Size(900, 750);
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