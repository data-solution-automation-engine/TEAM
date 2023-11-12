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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManageRepository));
            label2 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            backgroundWorkerSampleData = new System.ComponentModel.BackgroundWorker();
            toolTipRepository = new System.Windows.Forms.ToolTip(components);
            checkBoxConfigurationSettings = new System.Windows.Forms.CheckBox();
            button2 = new System.Windows.Forms.Button();
            buttonGenerateSampleMapping = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label7 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            comboBoxPresentationConnection = new System.Windows.Forms.ComboBox();
            comboBoxIntegrationConnection = new System.Windows.Forms.ComboBox();
            comboBoxPsaConnection = new System.Windows.Forms.ComboBox();
            comboBoxStagingConnection = new System.Windows.Forms.ComboBox();
            comboBoxSourceConnection = new System.Windows.Forms.ComboBox();
            button3 = new System.Windows.Forms.Button();
            linkLabelSource = new System.Windows.Forms.LinkLabel();
            checkBoxCreateSamplePresentation = new System.Windows.Forms.CheckBox();
            checkBoxCreateSamplePSA = new System.Windows.Forms.CheckBox();
            label6 = new System.Windows.Forms.Label();
            checkBoxCreateSampleIntegration = new System.Windows.Forms.CheckBox();
            checkBoxCreateSampleStaging = new System.Windows.Forms.CheckBox();
            checkBoxCreateSampleSource = new System.Windows.Forms.CheckBox();
            label3 = new System.Windows.Forms.Label();
            groupBox4 = new System.Windows.Forms.GroupBox();
            backgroundWorkerMetadata = new System.ComponentModel.BackgroundWorker();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 74);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(0, 13);
            label2.TabIndex = 23;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 76);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(0, 13);
            label4.TabIndex = 25;
            // 
            // backgroundWorkerSampleData
            // 
            backgroundWorkerSampleData.WorkerReportsProgress = true;
            backgroundWorkerSampleData.WorkerSupportsCancellation = true;
            backgroundWorkerSampleData.DoWork += backgroundWorkerSampleData_DoWork;
            backgroundWorkerSampleData.ProgressChanged += backgroundWorkerSampleData_ProgressChanged;
            // 
            // checkBoxConfigurationSettings
            // 
            checkBoxConfigurationSettings.AutoSize = true;
            checkBoxConfigurationSettings.Checked = true;
            checkBoxConfigurationSettings.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxConfigurationSettings.Location = new System.Drawing.Point(9, 19);
            checkBoxConfigurationSettings.Name = "checkBoxConfigurationSettings";
            checkBoxConfigurationSettings.Size = new System.Drawing.Size(307, 17);
            checkBoxConfigurationSettings.TabIndex = 10;
            checkBoxConfigurationSettings.Text = "Update and save default TEAM configurations settings";
            toolTipRepository.SetToolTip(checkBoxConfigurationSettings, resources.GetString("checkBoxConfigurationSettings.ToolTip"));
            checkBoxConfigurationSettings.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button2.Location = new System.Drawing.Point(6, 42);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(139, 42);
            button2.TabIndex = 12;
            button2.Text = "Reset standard Configuration Settings";
            toolTipRepository.SetToolTip(button2, resources.GetString("button2.ToolTip"));
            button2.UseVisualStyleBackColor = true;
            button2.Click += ButtonSetStandardConfiguration;
            // 
            // buttonGenerateSampleMapping
            // 
            buttonGenerateSampleMapping.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonGenerateSampleMapping.Location = new System.Drawing.Point(9, 19);
            buttonGenerateSampleMapping.Name = "buttonGenerateSampleMapping";
            buttonGenerateSampleMapping.Size = new System.Drawing.Size(139, 42);
            buttonGenerateSampleMapping.TabIndex = 2;
            buttonGenerateSampleMapping.Text = "Generate Sample Mapping Metadata";
            toolTipRepository.SetToolTip(buttonGenerateSampleMapping, "Generate sample source-to-target mapping metadata (table mapping, attribute mapping and physical model snapshot).");
            buttonGenerateSampleMapping.UseVisualStyleBackColor = true;
            buttonGenerateSampleMapping.Click += buttonClick_GenerateMetadata;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(buttonGenerateSampleMapping);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(860, 180);
            groupBox1.TabIndex = 72;
            groupBox1.TabStop = false;
            groupBox1.Text = "Mapping Metadata";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(12, 64);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(500, 13);
            label7.TabIndex = 88;
            label7.Text = "Copies sample JSON mapping metadata for data object, data item and physical model metadata.";
            label7.Click += label7_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(comboBoxPresentationConnection);
            groupBox2.Controls.Add(comboBoxIntegrationConnection);
            groupBox2.Controls.Add(comboBoxPsaConnection);
            groupBox2.Controls.Add(comboBoxStagingConnection);
            groupBox2.Controls.Add(comboBoxSourceConnection);
            groupBox2.Controls.Add(button3);
            groupBox2.Controls.Add(linkLabelSource);
            groupBox2.Controls.Add(checkBoxCreateSamplePresentation);
            groupBox2.Controls.Add(checkBoxCreateSamplePSA);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(checkBoxCreateSampleIntegration);
            groupBox2.Controls.Add(checkBoxCreateSampleStaging);
            groupBox2.Controls.Add(checkBoxCreateSampleSource);
            groupBox2.Controls.Add(label3);
            groupBox2.Location = new System.Drawing.Point(12, 198);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(860, 186);
            groupBox2.TabIndex = 9999;
            groupBox2.TabStop = false;
            groupBox2.Text = "Sample Database";
            // 
            // comboBoxPresentationConnection
            // 
            comboBoxPresentationConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxPresentationConnection.FormattingEnabled = true;
            comboBoxPresentationConnection.Location = new System.Drawing.Point(511, 109);
            comboBoxPresentationConnection.Name = "comboBoxPresentationConnection";
            comboBoxPresentationConnection.Size = new System.Drawing.Size(343, 21);
            comboBoxPresentationConnection.TabIndex = 90;
            // 
            // comboBoxIntegrationConnection
            // 
            comboBoxIntegrationConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxIntegrationConnection.FormattingEnabled = true;
            comboBoxIntegrationConnection.Location = new System.Drawing.Point(511, 86);
            comboBoxIntegrationConnection.Name = "comboBoxIntegrationConnection";
            comboBoxIntegrationConnection.Size = new System.Drawing.Size(343, 21);
            comboBoxIntegrationConnection.TabIndex = 89;
            // 
            // comboBoxPsaConnection
            // 
            comboBoxPsaConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxPsaConnection.FormattingEnabled = true;
            comboBoxPsaConnection.Location = new System.Drawing.Point(511, 63);
            comboBoxPsaConnection.Name = "comboBoxPsaConnection";
            comboBoxPsaConnection.Size = new System.Drawing.Size(343, 21);
            comboBoxPsaConnection.TabIndex = 88;
            // 
            // comboBoxStagingConnection
            // 
            comboBoxStagingConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxStagingConnection.FormattingEnabled = true;
            comboBoxStagingConnection.Location = new System.Drawing.Point(511, 40);
            comboBoxStagingConnection.Name = "comboBoxStagingConnection";
            comboBoxStagingConnection.Size = new System.Drawing.Size(343, 21);
            comboBoxStagingConnection.TabIndex = 87;
            // 
            // comboBoxSourceConnection
            // 
            comboBoxSourceConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxSourceConnection.FormattingEnabled = true;
            comboBoxSourceConnection.Location = new System.Drawing.Point(511, 17);
            comboBoxSourceConnection.Name = "comboBoxSourceConnection";
            comboBoxSourceConnection.Size = new System.Drawing.Size(343, 21);
            comboBoxSourceConnection.TabIndex = 86;
            // 
            // button3
            // 
            button3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button3.Location = new System.Drawing.Point(9, 21);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(139, 42);
            button3.TabIndex = 4;
            button3.Text = "Generate Sample Database content";
            button3.UseVisualStyleBackColor = true;
            button3.Click += ButtonGenerateDatabaseSamples;
            // 
            // linkLabelSource
            // 
            linkLabelSource.Location = new System.Drawing.Point(7, -29);
            linkLabelSource.Name = "linkLabelSource";
            linkLabelSource.Size = new System.Drawing.Size(86, 20);
            linkLabelSource.TabIndex = 91;
            // 
            // checkBoxCreateSamplePresentation
            // 
            checkBoxCreateSamplePresentation.AutoSize = true;
            checkBoxCreateSamplePresentation.Checked = true;
            checkBoxCreateSamplePresentation.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxCreateSamplePresentation.Location = new System.Drawing.Point(234, 111);
            checkBoxCreateSamplePresentation.Name = "checkBoxCreateSamplePresentation";
            checkBoxCreateSamplePresentation.Size = new System.Drawing.Size(239, 17);
            checkBoxCreateSamplePresentation.TabIndex = 9;
            checkBoxCreateSamplePresentation.Text = "Create Sample Presentation Layer content\r\n";
            checkBoxCreateSamplePresentation.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSamplePSA
            // 
            checkBoxCreateSamplePSA.AutoSize = true;
            checkBoxCreateSamplePSA.Checked = true;
            checkBoxCreateSamplePSA.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxCreateSamplePSA.Location = new System.Drawing.Point(234, 65);
            checkBoxCreateSamplePSA.Name = "checkBoxCreateSamplePSA";
            checkBoxCreateSamplePSA.Size = new System.Drawing.Size(264, 17);
            checkBoxCreateSamplePSA.TabIndex = 7;
            checkBoxCreateSamplePSA.Text = "Create Sample Persistent Staging Area content";
            checkBoxCreateSamplePSA.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(3, 92);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(0, 13);
            label6.TabIndex = 77;
            // 
            // checkBoxCreateSampleIntegration
            // 
            checkBoxCreateSampleIntegration.AutoSize = true;
            checkBoxCreateSampleIntegration.Checked = true;
            checkBoxCreateSampleIntegration.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxCreateSampleIntegration.Location = new System.Drawing.Point(234, 88);
            checkBoxCreateSampleIntegration.Name = "checkBoxCreateSampleIntegration";
            checkBoxCreateSampleIntegration.Size = new System.Drawing.Size(232, 17);
            checkBoxCreateSampleIntegration.TabIndex = 8;
            checkBoxCreateSampleIntegration.Text = "Create Sample Integration Layer content";
            checkBoxCreateSampleIntegration.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSampleStaging
            // 
            checkBoxCreateSampleStaging.AutoSize = true;
            checkBoxCreateSampleStaging.Checked = true;
            checkBoxCreateSampleStaging.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxCreateSampleStaging.Location = new System.Drawing.Point(234, 42);
            checkBoxCreateSampleStaging.Name = "checkBoxCreateSampleStaging";
            checkBoxCreateSampleStaging.Size = new System.Drawing.Size(237, 17);
            checkBoxCreateSampleStaging.TabIndex = 6;
            checkBoxCreateSampleStaging.Text = "Create Sample Staging / Landing content";
            checkBoxCreateSampleStaging.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSampleSource
            // 
            checkBoxCreateSampleSource.AutoSize = true;
            checkBoxCreateSampleSource.Checked = true;
            checkBoxCreateSampleSource.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxCreateSampleSource.Location = new System.Drawing.Point(234, 19);
            checkBoxCreateSampleSource.Name = "checkBoxCreateSampleSource";
            checkBoxCreateSampleSource.Size = new System.Drawing.Size(179, 17);
            checkBoxCreateSampleSource.TabIndex = 5;
            checkBoxCreateSampleSource.Text = "Create sample Source content";
            checkBoxCreateSampleSource.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(6, 157);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(426, 26);
            label3.TabIndex = 73;
            label3.Text = "WARNING - the databases associated with the selected connections must already \r\nexist and existing information will be removed.";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(button2);
            groupBox4.Controls.Add(checkBoxConfigurationSettings);
            groupBox4.Location = new System.Drawing.Point(12, 402);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size(860, 160);
            groupBox4.TabIndex = 88;
            groupBox4.TabStop = false;
            groupBox4.Text = "Generic options";
            // 
            // backgroundWorkerMetadata
            // 
            backgroundWorkerMetadata.WorkerReportsProgress = true;
            backgroundWorkerMetadata.WorkerSupportsCancellation = true;
            backgroundWorkerMetadata.DoWork += backgroundWorkerMetadata_DoWork;
            // 
            // FormManageRepository
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(884, 711);
            Controls.Add(groupBox4);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(label4);
            Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximumSize = new System.Drawing.Size(900, 750);
            MinimumSize = new System.Drawing.Size(900, 750);
            Name = "FormManageRepository";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Create / Rebuild Repository";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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