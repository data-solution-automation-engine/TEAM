﻿namespace TEAM
{
    partial class Form_Edit_DataObject
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Edit_DataObject));
            buttonSave = new System.Windows.Forms.Button();
            buttonClose = new System.Windows.Forms.Button();
            richTextBoxFormContent = new System.Windows.Forms.RichTextBox();
            SuspendLayout();
            // 
            // buttonSave
            // 
            buttonSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonSave.Location = new System.Drawing.Point(1057, 582);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new System.Drawing.Size(109, 40);
            buttonSave.TabIndex = 2;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += SaveJson;
            // 
            // buttonClose
            // 
            buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonClose.Location = new System.Drawing.Point(1172, 582);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(109, 40);
            buttonClose.TabIndex = 3;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // richTextBoxFormContent
            // 
            richTextBoxFormContent.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBoxFormContent.BackColor = System.Drawing.SystemColors.Window;
            richTextBoxFormContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            richTextBoxFormContent.HideSelection = false;
            richTextBoxFormContent.Location = new System.Drawing.Point(12, 12);
            richTextBoxFormContent.Name = "richTextBoxFormContent";
            richTextBoxFormContent.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            richTextBoxFormContent.Size = new System.Drawing.Size(1269, 564);
            richTextBoxFormContent.TabIndex = 4;
            richTextBoxFormContent.Text = "";
            // 
            // Form_Edit_DataObject
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(1293, 634);
            Controls.Add(richTextBoxFormContent);
            Controls.Add(buttonClose);
            Controls.Add(buttonSave);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "Form_Edit_DataObject";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Edit";
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.RichTextBox richTextBoxFormContent;
    }
}