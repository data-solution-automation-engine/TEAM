﻿using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace TEAM
{
    public partial class FormAbout : FormBase
    {
        public FormAbout()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
   
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Specify that the link was visited. 
            linkLabelTeam.LinkVisited = true;
            // Navigate to a URL.
            Process.Start("http://www.roelantvos.com");
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Specify that the link was visited. 
            linkLabelTeam.LinkVisited = true;
            // Navigate to a URL.
            Process.Start("https://github.com/RoelantVos/TEAM");
        }
    }
}
