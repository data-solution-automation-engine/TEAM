using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TEAM
{
    public partial class FormAlert : FormBase
    {
        //Make the label and progressbar accessible from the main form for updates
        public string Message
        {
            set { labelMessage.Text = value; }
        }

        public string Log
        {
            set { richTextBoxMetadataLog.Text += value; }
        }

        public int ProgressValue
        {
            set { progressBar1.Value = value; }
        }

        public FormAlert()
        {
            InitializeComponent();
        }

        delegate void SetFormNameCallBack(string text);

        public void SetFormName(string text)
        {
            if (this.InvokeRequired)
            {
                var d = new SetFormNameCallBack(SetFormName);
                try
                {
                    Invoke(d, text);
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                try
                {
                    this.Text = text;
                }
                catch
                {
                    // ignored
                }
            }
        }


        // Multi threading for updating the user
        delegate void SetTextCallBackLogging(string text);
        public void SetTextLogging(string text)
        {
            if (richTextBoxMetadataLog.InvokeRequired)
            {
                var d = new SetTextCallBackLogging(SetTextLogging);
                try
                {
                    Invoke(d, text);
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                try
                {
                    richTextBoxMetadataLog.AppendText(text);
                }
                catch
                {
                    // ignored
                }
            }           
        }

        public event EventHandler<EventArgs> Canceled;

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            EventHandler<EventArgs> ea = Canceled;
            if (ea != null)
            {
                ea(this, e);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonShowLog_Click(object sender, EventArgs e)
        {
            //Check if the file exists, otherwise create a dummy / empty file   
            if (File.Exists(GlobalParameters.ConfigurationPath + @"\Error_Log.txt"))
            {
                Process.Start(GlobalParameters.ConfigurationPath + @"\Error_Log.txt");
            }
            else
            {
                MessageBox.Show("There is no error file. This is a good thing right?", "No error file found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
