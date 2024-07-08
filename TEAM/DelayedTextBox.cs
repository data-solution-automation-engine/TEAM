using System;
using System.Windows.Forms;

namespace TEAM
{
    public class DelayedTextBox: TextBox
    {
        private Timer delayTimer;
        private int delayInterval;

        public event EventHandler DelayedTextChanged;

        public DelayedTextBox()
        {
            delayInterval = 1000; // Default delay interval in milliseconds
            InitializeDelayTimer();
        }

        public int DelayInterval
        {
            get { return delayInterval; }
            set
            {
                delayInterval = value;
                delayTimer.Interval = delayInterval;
            }
        }

        private void InitializeDelayTimer()
        {
            delayTimer = new Timer();
            delayTimer.Interval = delayInterval;
            delayTimer.Tick += DelayTimer_Tick;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            delayTimer.Stop();
            delayTimer.Start();
        }

        private void DelayTimer_Tick(object sender, EventArgs e)
        {
            delayTimer.Stop();
            DelayedTextChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
