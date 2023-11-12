using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TEAM
{
    public partial class Form_Connection_Selection : Form
    {
        private string _value { get; set; }

        public Form_Connection_Selection()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void radioButtonSqlServer_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSqlServer.Checked)
            {
                _value = "sqlserver";
            }
        }

        private void radioButtonSnowflake_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSnowflake.Checked)
            {
                _value = "snowflake";
            }
        }
        public string GetValue()
        {
            return this._value;
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
