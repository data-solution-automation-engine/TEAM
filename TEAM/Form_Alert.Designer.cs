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
            labelProgressMessageFormAlert = new System.Windows.Forms.Label();
            progressBarFormAlert = new System.Windows.Forms.ProgressBar();
            buttonCancelFormAlert = new System.Windows.Forms.Button();
            buttonCloseFormAlert = new System.Windows.Forms.Button();
            richTextBoxMetadataLogFormAlert = new System.Windows.Forms.RichTextBox();
            buttonShowLogFormAlert = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // labelProgressMessageFormAlert
            // 
            labelProgressMessageFormAlert.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            labelProgressMessageFormAlert.AutoSize = true;
            labelProgressMessageFormAlert.Location = new System.Drawing.Point(9, 703);
            labelProgressMessageFormAlert.Name = "labelProgressMessageFormAlert";
            labelProgressMessageFormAlert.Size = new System.Drawing.Size(51, 13);
            labelProgressMessageFormAlert.TabIndex = 0;
            labelProgressMessageFormAlert.Text = "Progress";
            // 
            // progressBarFormAlert
            // 
            progressBarFormAlert.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            progressBarFormAlert.Location = new System.Drawing.Point(12, 677);
            progressBarFormAlert.Name = "progressBarFormAlert";
            progressBarFormAlert.Size = new System.Drawing.Size(358, 23);
            progressBarFormAlert.TabIndex = 1;
            // 
            // buttonCancelFormAlert
            // 
            buttonCancelFormAlert.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancelFormAlert.Location = new System.Drawing.Point(964, 677);
            buttonCancelFormAlert.Name = "buttonCancelFormAlert";
            buttonCancelFormAlert.Size = new System.Drawing.Size(109, 40);
            buttonCancelFormAlert.TabIndex = 2;
            buttonCancelFormAlert.Text = "Cancel";
            buttonCancelFormAlert.UseVisualStyleBackColor = true;
            buttonCancelFormAlert.Click += buttonCancelFormAlert_Click;
            // 
            // buttonCloseFormAlert
            // 
            buttonCloseFormAlert.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCloseFormAlert.Location = new System.Drawing.Point(1080, 677);
            buttonCloseFormAlert.Name = "buttonCloseFormAlert";
            buttonCloseFormAlert.Size = new System.Drawing.Size(109, 40);
            buttonCloseFormAlert.TabIndex = 3;
            buttonCloseFormAlert.Text = "Close";
            buttonCloseFormAlert.UseVisualStyleBackColor = true;
            buttonCloseFormAlert.Click += buttonCloseFormAlert_Click;
            // 
            // richTextBoxMetadataLogFormAlert
            // 
            richTextBoxMetadataLogFormAlert.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBoxMetadataLogFormAlert.BackColor = System.Drawing.SystemColors.Info;
            richTextBoxMetadataLogFormAlert.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            richTextBoxMetadataLogFormAlert.HideSelection = false;
            richTextBoxMetadataLogFormAlert.Location = new System.Drawing.Point(12, 12);
            richTextBoxMetadataLogFormAlert.Name = "richTextBoxMetadataLogFormAlert";
            richTextBoxMetadataLogFormAlert.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            richTextBoxMetadataLogFormAlert.Size = new System.Drawing.Size(1177, 659);
            richTextBoxMetadataLogFormAlert.TabIndex = 4;
            richTextBoxMetadataLogFormAlert.Text = "";
            // 
            // buttonShowLogFormAlert
            // 
            buttonShowLogFormAlert.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonShowLogFormAlert.Location = new System.Drawing.Point(850, 677);
            buttonShowLogFormAlert.Name = "buttonShowLogFormAlert";
            buttonShowLogFormAlert.Size = new System.Drawing.Size(109, 40);
            buttonShowLogFormAlert.TabIndex = 5;
            buttonShowLogFormAlert.Text = "Show Log File";
            buttonShowLogFormAlert.UseVisualStyleBackColor = true;
            buttonShowLogFormAlert.Click += buttonShowLogFormAlert_Click;
            // 
            // Form_Alert
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(1201, 729);
            Controls.Add(buttonShowLogFormAlert);
            Controls.Add(richTextBoxMetadataLogFormAlert);
            Controls.Add(buttonCloseFormAlert);
            Controls.Add(buttonCancelFormAlert);
            Controls.Add(progressBarFormAlert);
            Controls.Add(labelProgressMessageFormAlert);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "Form_Alert";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Processing the metadata";
            ResumeLayout(false);
            PerformLayout();
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