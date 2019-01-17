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
            this.buttonTruncate = new System.Windows.Forms.Button();
            this.buttonDeploy = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.backgroundWorkerRepository = new System.ComponentModel.BackgroundWorker();
            this.labelResult = new System.Windows.Forms.Label();
            this.checkBoxRetainManualMapping = new System.Windows.Forms.CheckBox();
            this.labelMetadataRepository = new System.Windows.Forms.Label();
            this.backgroundWorkerSampleData = new System.ComponentModel.BackgroundWorker();
            this.toolTipRepository = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxDIRECT = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxCreateMetadataMapping = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateSamplePresLayer = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateSamplePSA = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBoxCreateSampleDV = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateSampleStaging = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateSampleSource = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonTruncate
            // 
            this.buttonTruncate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTruncate.Location = new System.Drawing.Point(6, 19);
            this.buttonTruncate.Name = "buttonTruncate";
            this.buttonTruncate.Size = new System.Drawing.Size(139, 42);
            this.buttonTruncate.TabIndex = 20;
            this.buttonTruncate.Text = "Truncate All Metadata";
            this.toolTipRepository.SetToolTip(this.buttonTruncate, "Truncate all information in the metadata repository.");
            this.buttonTruncate.UseVisualStyleBackColor = true;
            this.buttonTruncate.Click += new System.EventHandler(this.buttonTruncate_Click);
            // 
            // buttonDeploy
            // 
            this.buttonDeploy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDeploy.Location = new System.Drawing.Point(9, 19);
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
            this.label1.Location = new System.Drawing.Point(160, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(240, 26);
            this.label1.TabIndex = 22;
            this.label1.Text = "Create a new repository in the selected database.\r\nWARNING - will remove all exis" +
    "ting tables first.";
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
            this.labelResult.Location = new System.Drawing.Point(734, 639);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(38, 13);
            this.labelResult.TabIndex = 30;
            this.labelResult.Text = "Ready";
            // 
            // checkBoxRetainManualMapping
            // 
            this.checkBoxRetainManualMapping.AutoSize = true;
            this.checkBoxRetainManualMapping.Location = new System.Drawing.Point(6, 67);
            this.checkBoxRetainManualMapping.Name = "checkBoxRetainManualMapping";
            this.checkBoxRetainManualMapping.Size = new System.Drawing.Size(197, 17);
            this.checkBoxRetainManualMapping.TabIndex = 61;
            this.checkBoxRetainManualMapping.Text = "Retain manual mapping tables / files";
            this.toolTipRepository.SetToolTip(this.checkBoxRetainManualMapping, resources.GetString("checkBoxRetainManualMapping.ToolTip"));
            this.checkBoxRetainManualMapping.UseVisualStyleBackColor = true;
            // 
            // labelMetadataRepository
            // 
            this.labelMetadataRepository.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMetadataRepository.AutoSize = true;
            this.labelMetadataRepository.Location = new System.Drawing.Point(12, 639);
            this.labelMetadataRepository.Name = "labelMetadataRepository";
            this.labelMetadataRepository.Size = new System.Drawing.Size(197, 13);
            this.labelMetadataRepository.TabIndex = 62;
            this.labelMetadataRepository.Text = "Repository type in configuration is set to ";
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
            this.checkBoxDIRECT.Location = new System.Drawing.Point(267, 170);
            this.checkBoxDIRECT.Name = "checkBoxDIRECT";
            this.checkBoxDIRECT.Size = new System.Drawing.Size(247, 17);
            this.checkBoxDIRECT.TabIndex = 81;
            this.checkBoxDIRECT.Text = "Experimental - generate for DIRECT integration";
            this.toolTipRepository.SetToolTip(this.checkBoxDIRECT, resources.GetString("checkBoxDIRECT.ToolTip"));
            this.checkBoxDIRECT.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxRetainManualMapping);
            this.groupBox1.Controls.Add(this.buttonTruncate);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 143);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(757, 115);
            this.groupBox1.TabIndex = 72;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Truncate Metadata";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.linkLabel4);
            this.groupBox2.Controls.Add(this.linkLabel3);
            this.groupBox2.Controls.Add(this.linkLabel2);
            this.groupBox2.Controls.Add(this.linkLabel1);
            this.groupBox2.Controls.Add(this.checkBoxDIRECT);
            this.groupBox2.Controls.Add(this.checkBoxCreateMetadataMapping);
            this.groupBox2.Controls.Add(this.checkBoxCreateSamplePresLayer);
            this.groupBox2.Controls.Add(this.checkBoxCreateSamplePSA);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.checkBoxCreateSampleDV);
            this.groupBox2.Controls.Add(this.checkBoxCreateSampleStaging);
            this.groupBox2.Controls.Add(this.checkBoxCreateSampleSource);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Location = new System.Drawing.Point(12, 296);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(757, 217);
            this.groupBox2.TabIndex = 73;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Sample Schema";
            // 
            // checkBoxCreateMetadataMapping
            // 
            this.checkBoxCreateMetadataMapping.AutoSize = true;
            this.checkBoxCreateMetadataMapping.Checked = true;
            this.checkBoxCreateMetadataMapping.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateMetadataMapping.Location = new System.Drawing.Point(267, 134);
            this.checkBoxCreateMetadataMapping.Name = "checkBoxCreateMetadataMapping";
            this.checkBoxCreateMetadataMapping.Size = new System.Drawing.Size(219, 17);
            this.checkBoxCreateMetadataMapping.TabIndex = 80;
            this.checkBoxCreateMetadataMapping.Text = "Create Sample Mapping Metadata (DML)";
            this.checkBoxCreateMetadataMapping.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSamplePresLayer
            // 
            this.checkBoxCreateSamplePresLayer.AutoSize = true;
            this.checkBoxCreateSamplePresLayer.Enabled = false;
            this.checkBoxCreateSamplePresLayer.Location = new System.Drawing.Point(267, 111);
            this.checkBoxCreateSamplePresLayer.Name = "checkBoxCreateSamplePresLayer";
            this.checkBoxCreateSamplePresLayer.Size = new System.Drawing.Size(186, 17);
            this.checkBoxCreateSamplePresLayer.TabIndex = 79;
            this.checkBoxCreateSamplePresLayer.Text = "Create Sample Presentation Layer";
            this.checkBoxCreateSamplePresLayer.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSamplePSA
            // 
            this.checkBoxCreateSamplePSA.AutoSize = true;
            this.checkBoxCreateSamplePSA.Checked = true;
            this.checkBoxCreateSamplePSA.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSamplePSA.Location = new System.Drawing.Point(267, 65);
            this.checkBoxCreateSamplePSA.Name = "checkBoxCreateSamplePSA";
            this.checkBoxCreateSamplePSA.Size = new System.Drawing.Size(208, 17);
            this.checkBoxCreateSamplePSA.TabIndex = 78;
            this.checkBoxCreateSamplePSA.Text = "Create Sample Persistent Staging Area";
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
            // checkBoxCreateSampleDV
            // 
            this.checkBoxCreateSampleDV.AutoSize = true;
            this.checkBoxCreateSampleDV.Checked = true;
            this.checkBoxCreateSampleDV.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleDV.Location = new System.Drawing.Point(267, 88);
            this.checkBoxCreateSampleDV.Name = "checkBoxCreateSampleDV";
            this.checkBoxCreateSampleDV.Size = new System.Drawing.Size(148, 17);
            this.checkBoxCreateSampleDV.TabIndex = 76;
            this.checkBoxCreateSampleDV.Text = "Create Sample Data Vault";
            this.checkBoxCreateSampleDV.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSampleStaging
            // 
            this.checkBoxCreateSampleStaging.AutoSize = true;
            this.checkBoxCreateSampleStaging.Checked = true;
            this.checkBoxCreateSampleStaging.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleStaging.Location = new System.Drawing.Point(267, 42);
            this.checkBoxCreateSampleStaging.Name = "checkBoxCreateSampleStaging";
            this.checkBoxCreateSampleStaging.Size = new System.Drawing.Size(159, 17);
            this.checkBoxCreateSampleStaging.TabIndex = 75;
            this.checkBoxCreateSampleStaging.Text = "Create Sample Staging Area";
            this.checkBoxCreateSampleStaging.UseVisualStyleBackColor = true;
            // 
            // checkBoxCreateSampleSource
            // 
            this.checkBoxCreateSampleSource.AutoSize = true;
            this.checkBoxCreateSampleSource.Checked = true;
            this.checkBoxCreateSampleSource.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateSampleSource.Location = new System.Drawing.Point(267, 19);
            this.checkBoxCreateSampleSource.Name = "checkBoxCreateSampleSource";
            this.checkBoxCreateSampleSource.Size = new System.Drawing.Size(169, 17);
            this.checkBoxCreateSampleSource.TabIndex = 74;
            this.checkBoxCreateSampleSource.Text = "Create sample Source content";
            this.checkBoxCreateSampleSource.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(225, 78);
            this.label3.TabIndex = 73;
            this.label3.Text = "Generates the sample mapping metadata.\r\nWARNING - the databases must already exis" +
    "t.\r\nAny existing information will be removed.\r\n\r\nWhile not required, it is recom" +
    "mended to\r\n use an empty database.";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(6, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(139, 42);
            this.button1.TabIndex = 72;
            this.button1.Text = "Generate Sample Metadata";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.buttonDeploy);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(757, 97);
            this.groupBox3.TabIndex = 74;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Create Metadata Repository";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(479, 20);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(118, 13);
            this.linkLabel1.TabIndex = 82;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "https://bit.ly/2ARcCTw";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(479, 43);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(114, 13);
            this.linkLabel2.TabIndex = 83;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "https://bit.ly/2VY4Os3";
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Location = new System.Drawing.Point(479, 66);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(111, 13);
            this.linkLabel3.TabIndex = 84;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "https://bit.ly/2SX0Xth";
            // 
            // linkLabel4
            // 
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.Location = new System.Drawing.Point(479, 89);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(117, 13);
            this.linkLabel4.TabIndex = 85;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "https://bit.ly/2FuWBq5";
            // 
            // FormManageRepository
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 662);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.labelMetadataRepository);
            this.Controls.Add(this.labelResult);
            this.Controls.Add(this.label4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(800, 700);
            this.MinimumSize = new System.Drawing.Size(800, 700);
            this.Name = "FormManageRepository";
            this.Text = "Create / Rebuild Repository";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
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
        internal System.Windows.Forms.CheckBox checkBoxRetainManualMapping;
        private System.Windows.Forms.Label labelMetadataRepository;
        private System.ComponentModel.BackgroundWorker backgroundWorkerSampleData;
        private System.Windows.Forms.ToolTip toolTipRepository;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        internal System.Windows.Forms.CheckBox checkBoxDIRECT;
        internal System.Windows.Forms.CheckBox checkBoxCreateMetadataMapping;
        internal System.Windows.Forms.CheckBox checkBoxCreateSamplePresLayer;
        internal System.Windows.Forms.CheckBox checkBoxCreateSamplePSA;
        private System.Windows.Forms.Label label6;
        internal System.Windows.Forms.CheckBox checkBoxCreateSampleDV;
        internal System.Windows.Forms.CheckBox checkBoxCreateSampleStaging;
        internal System.Windows.Forms.CheckBox checkBoxCreateSampleSource;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}