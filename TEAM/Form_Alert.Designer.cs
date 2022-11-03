namespace TEAM
{
    partial class Form_Alert
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Alert));
            this.labelProgressMessageFormAlert = new System.Windows.Forms.Label();
            this.progressBarFormAlert = new System.Windows.Forms.ProgressBar();
            this.buttonCancelFormAlert = new System.Windows.Forms.Button();
            this.buttonCloseFormAlert = new System.Windows.Forms.Button();
            this.richTextBoxMetadataLogFormAlert = new System.Windows.Forms.RichTextBox();
            this.buttonShowLogFormAlert = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelProgressMessageFormAlert
            // 
            this.labelProgressMessageFormAlert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgressMessageFormAlert.AutoSize = true;
            this.labelProgressMessageFormAlert.Location = new System.Drawing.Point(9, 456);
            this.labelProgressMessageFormAlert.Name = "labelProgressMessageFormAlert";
            this.labelProgressMessageFormAlert.Size = new System.Drawing.Size(48, 13);
            this.labelProgressMessageFormAlert.TabIndex = 0;
            this.labelProgressMessageFormAlert.Text = "Progress";
            // 
            // progressBarFormAlert
            // 
            this.progressBarFormAlert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBarFormAlert.Location = new System.Drawing.Point(12, 430);
            this.progressBarFormAlert.Name = "progressBarFormAlert";
            this.progressBarFormAlert.Size = new System.Drawing.Size(358, 23);
            this.progressBarFormAlert.TabIndex = 1;
            // 
            // buttonCancelFormAlert
            // 
            this.buttonCancelFormAlert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancelFormAlert.Location = new System.Drawing.Point(579, 430);
            this.buttonCancelFormAlert.Name = "buttonCancelFormAlert";
            this.buttonCancelFormAlert.Size = new System.Drawing.Size(109, 40);
            this.buttonCancelFormAlert.TabIndex = 2;
            this.buttonCancelFormAlert.Text = "Cancel";
            this.buttonCancelFormAlert.UseVisualStyleBackColor = true;
            this.buttonCancelFormAlert.Click += new System.EventHandler(this.buttonCancelFormAlert_Click);
            // 
            // buttonCloseFormAlert
            // 
            this.buttonCloseFormAlert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCloseFormAlert.Location = new System.Drawing.Point(694, 430);
            this.buttonCloseFormAlert.Name = "buttonCloseFormAlert";
            this.buttonCloseFormAlert.Size = new System.Drawing.Size(109, 40);
            this.buttonCloseFormAlert.TabIndex = 3;
            this.buttonCloseFormAlert.Text = "Close";
            this.buttonCloseFormAlert.UseVisualStyleBackColor = true;
            this.buttonCloseFormAlert.Click += new System.EventHandler(this.buttonCloseFormAlert_Click);
            // 
            // richTextBoxMetadataLogFormAlert
            // 
            this.richTextBoxMetadataLogFormAlert.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxMetadataLogFormAlert.BackColor = System.Drawing.SystemColors.Info;
            this.richTextBoxMetadataLogFormAlert.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxMetadataLogFormAlert.HideSelection = false;
            this.richTextBoxMetadataLogFormAlert.Location = new System.Drawing.Point(12, 12);
            this.richTextBoxMetadataLogFormAlert.Name = "richTextBoxMetadataLogFormAlert";
            this.richTextBoxMetadataLogFormAlert.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.richTextBoxMetadataLogFormAlert.Size = new System.Drawing.Size(791, 412);
            this.richTextBoxMetadataLogFormAlert.TabIndex = 4;
            this.richTextBoxMetadataLogFormAlert.Text = "";
            // 
            // buttonShowLogFormAlert
            // 
            this.buttonShowLogFormAlert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShowLogFormAlert.Location = new System.Drawing.Point(464, 430);
            this.buttonShowLogFormAlert.Name = "buttonShowLogFormAlert";
            this.buttonShowLogFormAlert.Size = new System.Drawing.Size(109, 40);
            this.buttonShowLogFormAlert.TabIndex = 5;
            this.buttonShowLogFormAlert.Text = "Show Log File";
            this.buttonShowLogFormAlert.UseVisualStyleBackColor = true;
            this.buttonShowLogFormAlert.Click += new System.EventHandler(this.buttonShowLogFormAlert_Click);
            // 
            // Form_Alert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(815, 482);
            this.Controls.Add(this.buttonShowLogFormAlert);
            this.Controls.Add(this.richTextBoxMetadataLogFormAlert);
            this.Controls.Add(this.buttonCloseFormAlert);
            this.Controls.Add(this.buttonCancelFormAlert);
            this.Controls.Add(this.progressBarFormAlert);
            this.Controls.Add(this.labelProgressMessageFormAlert);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_Alert";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Processing the metadata";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelProgressMessageFormAlert;
        private System.Windows.Forms.ProgressBar progressBarFormAlert;
        private System.Windows.Forms.Button buttonCancelFormAlert;
        private System.Windows.Forms.Button buttonCloseFormAlert;
        private System.Windows.Forms.RichTextBox richTextBoxMetadataLogFormAlert;
        private System.Windows.Forms.Button buttonShowLogFormAlert;
    }
}