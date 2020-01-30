using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TEAM
{
    public partial class FormManagePattern: FormBase
    {
        private bool _formLoading = true;
        private FormMain parentFormMain;
        public FormManagePattern()
        {
            InitializeComponent();
        }

        public FormManagePattern(FormMain parent) : base(parent)
        {
            this.parentFormMain = parent;
            InitializeComponent();



            _formLoading = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
