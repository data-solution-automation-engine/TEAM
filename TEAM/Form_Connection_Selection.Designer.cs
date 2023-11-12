namespace TEAM
{
    partial class Form_Connection_Selection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Connection_Selection));
            radioButtonSqlServer = new System.Windows.Forms.RadioButton();
            radioButtonSnowflake = new System.Windows.Forms.RadioButton();
            groupBox1 = new System.Windows.Forms.GroupBox();
            buttonSelect = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // radioButtonSqlServer
            // 
            radioButtonSqlServer.AutoSize = true;
            radioButtonSqlServer.Location = new System.Drawing.Point(5, 19);
            radioButtonSqlServer.Name = "radioButtonSqlServer";
            radioButtonSqlServer.Size = new System.Drawing.Size(111, 17);
            radioButtonSqlServer.TabIndex = 0;
            radioButtonSqlServer.TabStop = true;
            radioButtonSqlServer.Text = "SQL Server family";
            radioButtonSqlServer.UseVisualStyleBackColor = true;
            radioButtonSqlServer.CheckedChanged += radioButtonSqlServer_CheckedChanged;
            // 
            // radioButtonSnowflake
            // 
            radioButtonSnowflake.AutoSize = true;
            radioButtonSnowflake.Location = new System.Drawing.Point(5, 41);
            radioButtonSnowflake.Name = "radioButtonSnowflake";
            radioButtonSnowflake.Size = new System.Drawing.Size(79, 17);
            radioButtonSnowflake.TabIndex = 1;
            radioButtonSnowflake.TabStop = true;
            radioButtonSnowflake.Text = "Snowflake";
            radioButtonSnowflake.UseVisualStyleBackColor = true;
            radioButtonSnowflake.CheckedChanged += radioButtonSnowflake_CheckedChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(radioButtonSnowflake);
            groupBox1.Controls.Add(radioButtonSqlServer);
            groupBox1.Location = new System.Drawing.Point(10, 10);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(328, 87);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Connection Type";
            // 
            // buttonSelect
            // 
            buttonSelect.Location = new System.Drawing.Point(10, 105);
            buttonSelect.Name = "buttonSelect";
            buttonSelect.Size = new System.Drawing.Size(166, 36);
            buttonSelect.TabIndex = 3;
            buttonSelect.Text = "Proceed with selection";
            buttonSelect.UseVisualStyleBackColor = true;
            buttonSelect.Click += buttonSelect_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new System.Drawing.Point(182, 105);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(156, 36);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // Form_Connection_Selection
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(350, 152);
            Controls.Add(buttonCancel);
            Controls.Add(buttonSelect);
            Controls.Add(groupBox1);
            Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximumSize = new System.Drawing.Size(366, 191);
            MinimumSize = new System.Drawing.Size(366, 191);
            Name = "Form_Connection_Selection";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Select the type for the new connection";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonSqlServer;
        private System.Windows.Forms.RadioButton radioButtonSnowflake;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonSelect;
        private System.Windows.Forms.Button buttonCancel;
    }
}