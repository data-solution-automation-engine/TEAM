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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManageRepository));
            this.buttonTruncate = new System.Windows.Forms.Button();
            this.buttonDeploy = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.backgroundWorkerRepository = new System.ComponentModel.BackgroundWorker();
            this.labelResult = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBoxRetainManualMapping = new System.Windows.Forms.CheckBox();
            this.labelMetadataRepository = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxCreateSampleSource = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateSampleStaging = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateSampleDV = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBoxCreateSamplePSA = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateSamplePresLayer = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateMetadataMapping = new System.Windows.Forms.CheckBox();
            this.backgroundWorkerSampleData = new System.ComponentModel.BackgroundWorker();
            this.checkBoxDIRECT = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonTruncate
            // 
            this.buttonTruncate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTruncate.Location = new System.Drawing.Point(15, 117);
            this.buttonTruncate.Name = "buttonTruncate";
            this.buttonTruncate.Size = new System.Drawing.Size(139, 42);
            this.buttonTruncate.TabIndex = 20;
            this.buttonTruncate.Text = "Truncate All Metadata";
            this.buttonTruncate.UseVisualStyleBackColor = true;
            this.buttonTruncate.Click += new System.EventHandler(this.buttonTruncate_Click);
            // 
            // buttonDeploy
            // 
            this.buttonDeploy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDeploy.Location = new System.Drawing.Point(15, 14);
            this.buttonDeploy.Name = "buttonDeploy";
            this.buttonDeploy.Size = new System.Drawing.Size(139, 42);
            this.buttonDeploy.TabIndex = 21;
            this.buttonDeploy.Text = "Deploy Metadata Repository";
            this.buttonDeploy.UseVisualStyleBackColor = true;
            this.buttonDeploy.Click += new System.EventHandler(this.buttonDeploy_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(240, 26);
            this.label1.TabIndex = 22;
            this.label1.Text = "Create a new repository in the selected database.\r\nWARNING - will remove existing" +
    " tables first.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 162);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(244, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "Truncate all information in the metadata repository.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 13);
            this.label4.TabIndex = 25;
            // 
            // backgroundWorkerRepository
            // 
            this.backgroundWorkerRepository.WorkerReportsProgress = true;
            this.backgroundWorkerRepository.WorkerSupportsCancellation = true;
            this.backgroundWorkerRepository.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerRepository_DoWork);
            this.backgroundWorkerRepository.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerRepository_ProgressChanged);
            this.backgroundWorkerRepository.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerRepository_RunWorkerCompleted);
            // 
            // labelResult
            // 
            this.labelResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelResult.AutoSize = true;
            this.labelResult.Location = new System.Drawing.Point(534, 439);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(38, 13);
            this.labelResult.TabIndex = 30;
            this.labelResult.Text = "Ready";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 262);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(225, 39);
            this.label3.TabIndex = 32;
            this.label3.Text = "Generates the sample mapping metadata.\r\nWARNING - the databases must already exis" +
    "t.\r\nAny existing information will be removed.";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(15, 217);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(139, 42);
            this.button1.TabIndex = 31;
            this.button1.Text = "Generate Sample Metadata";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBoxRetainManualMapping
            // 
            this.checkBoxRetainManualMapping.AutoSize = true;
            this.checkBoxRetainManualMapping.Location = new System.Drawing.Point(275, 117);
            this.checkBoxRetainManualMapping.Name = "checkBoxRetainManualMapping";
            this.checkBoxRetainManualMapping.Size = new System.Drawing.Size(197, 17);
            this.checkBoxRetainManualMapping.TabIndex = 61;
            this.checkBoxRetainManualMapping.Text = "Retain manual mapping tables / files";
            this.checkBoxRetainManualMapping.UseVisualStyleBackColor = true;
            // 
            // labelMetadataRepository
            // 
            this.labelMetadataRepository.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMetadataRepository.AutoSize = true;
            this.labelMetadataRepository.Location = new System.Drawing.Point(12, 439);
            this.labelMetadataRepository.Name = "labelMetadataRepository";
            this.labelMetadataRepository.Size = new System.Drawing.Size(197, 13);
            this.labelMetadataRepository.TabIndex = 62;
            this.labelMetadataRepository.Text = "Repository type in configuration is set to ";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(275, 137);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(152, 26);
            this.label5.TabIndex = 63;
            this.label5.Text = "- MD_TABLE_MAPPING\r\n- MD_ATTRIBUTE_MAPPING";
            // 
            // checkBoxCreateSampleSource
            // 
            this.checkBoxCreateSampleSource.AutoSize = true;
            this.checkBoxCreateSampleSource.Checked = true;
            this.checkBoxCreateSampleSource.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleSource.Location = new System.Drawing.Point(275, 217);
            this.checkBoxCreateSampleSource.Name = "checkBoxCreateSampleSource";
            this.checkBoxCreateSampleSource.Size = new System.Drawing.Size(169, 17);
            this.checkBoxCreateSampleSource.TabIndex = 64;
            this.checkBoxCreateSampleSource.Text = "Create sample Source content";
            this.checkBoxCreateSampleSource.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSampleStaging
            // 
            this.checkBoxCreateSampleStaging.AutoSize = true;
            this.checkBoxCreateSampleStaging.Checked = true;
            this.checkBoxCreateSampleStaging.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleStaging.Location = new System.Drawing.Point(275, 240);
            this.checkBoxCreateSampleStaging.Name = "checkBoxCreateSampleStaging";
            this.checkBoxCreateSampleStaging.Size = new System.Drawing.Size(159, 17);
            this.checkBoxCreateSampleStaging.TabIndex = 65;
            this.checkBoxCreateSampleStaging.Text = "Create Sample Staging Area";
            this.checkBoxCreateSampleStaging.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSampleDV
            // 
            this.checkBoxCreateSampleDV.AutoSize = true;
            this.checkBoxCreateSampleDV.Checked = true;
            this.checkBoxCreateSampleDV.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleDV.Location = new System.Drawing.Point(275, 286);
            this.checkBoxCreateSampleDV.Name = "checkBoxCreateSampleDV";
            this.checkBoxCreateSampleDV.Size = new System.Drawing.Size(148, 17);
            this.checkBoxCreateSampleDV.TabIndex = 66;
            this.checkBoxCreateSampleDV.Text = "Create Sample Data Vault";
            this.checkBoxCreateSampleDV.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 278);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 13);
            this.label6.TabIndex = 67;
            // 
            // checkBoxCreateSamplePSA
            // 
            this.checkBoxCreateSamplePSA.AutoSize = true;
            this.checkBoxCreateSamplePSA.Checked = true;
            this.checkBoxCreateSamplePSA.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSamplePSA.Location = new System.Drawing.Point(275, 263);
            this.checkBoxCreateSamplePSA.Name = "checkBoxCreateSamplePSA";
            this.checkBoxCreateSamplePSA.Size = new System.Drawing.Size(208, 17);
            this.checkBoxCreateSamplePSA.TabIndex = 68;
            this.checkBoxCreateSamplePSA.Text = "Create Sample Persistent Staging Area";
            this.checkBoxCreateSamplePSA.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSamplePresLayer
            // 
            this.checkBoxCreateSamplePresLayer.AutoSize = true;
            this.checkBoxCreateSamplePresLayer.Checked = true;
            this.checkBoxCreateSamplePresLayer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSamplePresLayer.Location = new System.Drawing.Point(275, 309);
            this.checkBoxCreateSamplePresLayer.Name = "checkBoxCreateSamplePresLayer";
            this.checkBoxCreateSamplePresLayer.Size = new System.Drawing.Size(186, 17);
            this.checkBoxCreateSamplePresLayer.TabIndex = 69;
            this.checkBoxCreateSamplePresLayer.Text = "Create Sample Presentation Layer";
            this.checkBoxCreateSamplePresLayer.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateMetadataMapping
            // 
            this.checkBoxCreateMetadataMapping.AutoSize = true;
            this.checkBoxCreateMetadataMapping.Checked = true;
            this.checkBoxCreateMetadataMapping.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateMetadataMapping.Location = new System.Drawing.Point(275, 332);
            this.checkBoxCreateMetadataMapping.Name = "checkBoxCreateMetadataMapping";
            this.checkBoxCreateMetadataMapping.Size = new System.Drawing.Size(187, 17);
            this.checkBoxCreateMetadataMapping.TabIndex = 70;
            this.checkBoxCreateMetadataMapping.Text = "Create Sample Mapping Metadata";
            this.checkBoxCreateMetadataMapping.UseVisualStyleBackColor = true;
            // 
            // backgroundWorkerSampleData
            // 
            this.backgroundWorkerSampleData.WorkerReportsProgress = true;
            this.backgroundWorkerSampleData.WorkerSupportsCancellation = true;
            this.backgroundWorkerSampleData.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerSampleData_DoWork);
            this.backgroundWorkerSampleData.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerSampleData_ProgressChanged);
            this.backgroundWorkerSampleData.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerSampleData_RunWorkerCompleted);
            // 
            // checkBoxDIRECT
            // 
            this.checkBoxDIRECT.AutoSize = true;
            this.checkBoxDIRECT.Location = new System.Drawing.Point(274, 383);
            this.checkBoxDIRECT.Name = "checkBoxDIRECT";
            this.checkBoxDIRECT.Size = new System.Drawing.Size(247, 17);
            this.checkBoxDIRECT.TabIndex = 71;
            this.checkBoxDIRECT.Text = "Experimental - generate for DIRECT integration";
            this.checkBoxDIRECT.UseVisualStyleBackColor = true;
            // 
            // FormManageRepository
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 461);
            this.Controls.Add(this.checkBoxDIRECT);
            this.Controls.Add(this.checkBoxCreateMetadataMapping);
            this.Controls.Add(this.checkBoxCreateSamplePresLayer);
            this.Controls.Add(this.checkBoxCreateSamplePSA);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.checkBoxCreateSampleDV);
            this.Controls.Add(this.checkBoxCreateSampleStaging);
            this.Controls.Add(this.checkBoxCreateSampleSource);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.labelMetadataRepository);
            this.Controls.Add(this.checkBoxRetainManualMapping);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelResult);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonDeploy);
            this.Controls.Add(this.buttonTruncate);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(600, 500);
            this.MinimumSize = new System.Drawing.Size(600, 500);
            this.Name = "FormManageRepository";
            this.Text = "Create / Rebuild Repository";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonTruncate;
        private System.Windows.Forms.Button buttonDeploy;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.ComponentModel.BackgroundWorker backgroundWorkerRepository;
        private System.Windows.Forms.Label labelResult;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        internal System.Windows.Forms.CheckBox checkBoxRetainManualMapping;
        private System.Windows.Forms.Label labelMetadataRepository;
        private System.Windows.Forms.Label label5;
        internal System.Windows.Forms.CheckBox checkBoxCreateSampleSource;
        internal System.Windows.Forms.CheckBox checkBoxCreateSampleStaging;
        internal System.Windows.Forms.CheckBox checkBoxCreateSampleDV;
        private System.Windows.Forms.Label label6;
        internal System.Windows.Forms.CheckBox checkBoxCreateSamplePSA;
        internal System.Windows.Forms.CheckBox checkBoxCreateSamplePresLayer;
        internal System.Windows.Forms.CheckBox checkBoxCreateMetadataMapping;
        private System.ComponentModel.BackgroundWorker backgroundWorkerSampleData;
        internal System.Windows.Forms.CheckBox checkBoxDIRECT;
    }
}