using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TEAM
{
    public partial class Form_Alert : FormBase
    {
        public Form_Alert()
        {
            InitializeComponent();
        }

        //Make the label and progressbar accessible from the main form for updates
        public string Message
        {
            set => labelProgressMessageFormAlert.Text = value;
        }

        #region Delegate & function for hiding the Progress Bar
        delegate void ShowProgressBarCallBack(bool showProgressBar);
        public void ShowProgressBar(bool showProgressBar)
        {
            if (progressBarFormAlert.InvokeRequired)
            {
                var d = new ShowProgressBarCallBack(ShowProgressBar);
                try
                {
                    Invoke(d, showProgressBar);
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
                    progressBarFormAlert.Visible = false;
                }
                catch
                {
                    // ignored
                }
            }
        }
        #endregion

        #region Delegate & function for hiding the Show Log button
        delegate void ShowLogButtonCallBack(bool showLogButton);
        public void ShowLogButton(bool showLogButton)
        {
            if (buttonShowLogFormAlert.InvokeRequired)
            {
                var d = new ShowLogButtonCallBack(ShowLogButton);
                try
                {
                    Invoke(d, showLogButton);
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
                    buttonShowLogFormAlert.Visible = false;
                }
                catch
                {
                    // ignored
                }
            }
        }
        #endregion

        #region Delegate & function for hiding the Cancel button

        delegate void ShowCancelButtonCallBack(bool showCancelButton);

        public void ShowCancelButton(bool showCancelButton)
        {
            if (buttonCancelFormAlert.InvokeRequired)
            {
                var d = new ShowCancelButtonCallBack(ShowCancelButton);
                try
                {
                    Invoke(d, showCancelButton);
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
                    if (showCancelButton)
                    {
                        buttonCancelFormAlert.Visible = true;
                    }
                    else
                    {
                        buttonCancelFormAlert.Visible = false;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        #endregion

        #region Delegate & function for hiding the Progress Label
        public void ShowProgressLabel(bool showProgressLabel)
        {
            if (labelProgressMessageFormAlert.InvokeRequired)
            {
                var d = new ShowCancelButtonCallBack(ShowProgressLabel);
                try
                {
                    Invoke(d, showProgressLabel);
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
                    labelProgressMessageFormAlert.Visible = false;
                }
                catch
                {
                    // ignored
                }
            }
        }
        #endregion
        
        public int ProgressValue
        {
            set { progressBarFormAlert.Value = value; }
        }

        #region Form Name delegate
        delegate void SetFormNameCallBack(string text);

        public void SetFormName(string text)
        {
            if (InvokeRequired)
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
                    Text = text;
                }
                catch
                {
                    // ignored
                }
            }
        }
        #endregion

        // Multi threading for updating the user
        delegate void SetTextCallBackLogging(string text);

        public void SetTextLogging(string text)
        {
            if (richTextBoxMetadataLogFormAlert.InvokeRequired)
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
                    richTextBoxMetadataLogFormAlert.AppendText(text);
                }
                catch
                {
                    // ignored
                }
            }           
        }

        delegate void SetTextCallBackLoggingMultiple(List<string> text);
        public void SetTextLoggingMultiple(List<string> text)
        {
            if (richTextBoxMetadataLogFormAlert.InvokeRequired)
            {
                var d = new SetTextCallBackLoggingMultiple(SetTextLoggingMultiple);
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
                    foreach (var localString in text)
                    {
                        richTextBoxMetadataLogFormAlert.AppendText(localString);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        public event EventHandler<EventArgs> Canceled;

        private void buttonCancelFormAlert_Click(object sender, EventArgs e)
        {
            EventHandler<EventArgs> ea = Canceled;
            if (ea != null)
            {
                ea(this, e);
            }
        }

        private void buttonCloseFormAlert_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonShowLogFormAlert_Click(object sender, EventArgs e)
        {
            //Check if the file exists, otherwise create a dummy / empty file   
            if (File.Exists(globalParameters.ConfigurationPath + @"\Error_Log.txt"))
            {
                Process.Start(globalParameters.ConfigurationPath + @"\Error_Log.txt");
            }
            else
            {
                MessageBox.Show(@"There is no error file. This is a good thing right?", @"No error file found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
