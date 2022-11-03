using System;
using System.Windows.Forms;

namespace TEAM
{
    public static class ThreadHelper
    {
        delegate void SetControlCallback(Form form, Control control, string text);

        /// <summary>
        /// Set text property of various controls.
        /// </summary>
        /// <param name="form">The calling form</param>
        /// <param name="control"></param>
        /// <param name="text"></param>
        public static void SetText(Form form, Control control, string text)
        {
            // InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (control.InvokeRequired)
            {
                SetControlCallback callBack = SetText;
                form.Invoke(callBack, form, control, text);
            }
            else
            {
                control.Text = text;
            }
        }
    }
}