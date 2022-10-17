using System;
using System.Windows.Forms;

namespace TEAM
{
    public class CustomTimedTextBox: TextBox
    {
        private Timer m_delayedTextChangedTimer;

        public event EventHandler DelayedTextChanged;

        public CustomTimedTextBox() : base()
        {
            this.DelayedTextChangedTimeout = 1000; //  seconds
        }

        protected override void Dispose(bool disposing)
        {
            if (m_delayedTextChangedTimer != null)
            {
                m_delayedTextChangedTimer.Stop();

                if (disposing)
                    m_delayedTextChangedTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        public int DelayedTextChangedTimeout { get; set; }

        protected virtual void OnDelayedTextChanged(EventArgs e)
        {
            if (DelayedTextChanged != null)
                DelayedTextChanged(this, e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            InitializeDelayedTextChangedEvent();
            base.OnTextChanged(e);
        }

        private void InitializeDelayedTextChangedEvent()
        {
            if (m_delayedTextChangedTimer != null)
                m_delayedTextChangedTimer.Stop();

            if (m_delayedTextChangedTimer == null || m_delayedTextChangedTimer.Interval != this.DelayedTextChangedTimeout)
            {
                m_delayedTextChangedTimer = new Timer();
                m_delayedTextChangedTimer.Tick += new EventHandler(HandleDelayedTextChangedTimerTick);
                m_delayedTextChangedTimer.Interval = this.DelayedTextChangedTimeout;
            }

            m_delayedTextChangedTimer.Start();
        }

        private void HandleDelayedTextChangedTimerTick(object sender, EventArgs e)
        {
            Timer timer = sender as Timer;
            timer.Stop();

            OnDelayedTextChanged(EventArgs.Empty);
        }
    }
}
