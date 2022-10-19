using Newtonsoft.Json;
using System;
using System.Data;
using System.Windows.Forms;

namespace TEAM
{
    public partial class Form_Edit : FormBase
    {
        public DataGridViewCell EditedCell { get; set; }
        public DataWarehouseAutomation.DataObject EditedDataObject { get; set; }

        public Form_Edit()
        {
            InitializeComponent();
        }

        public Form_Edit(DataGridViewCell cell)
        {
            InitializeComponent();
            EditedCell = cell;

            DataRowView dataBoundItem = (DataRowView)EditedCell.OwningRow.DataBoundItem;

            EditedDataObject = (DataWarehouseAutomation.DataObject)dataBoundItem.Row[cell.ColumnIndex];


            // Set the tooltip.
            //DataGridViewCell cell = _dataGridViewDataObjects.Rows[e.RowIndex].Cells[e.ColumnIndex];
            //cell.ToolTipText = JsonConvert.SerializeObject(dataObject, Formatting.Indented);


            // Set the tooltip.
            //DataGridViewCell cell = _dataGridViewDataObjects.Rows[e.RowIndex].Cells[e.ColumnIndex];
            //cell.ToolTipText = JsonConvert.SerializeObject(dataObject, Formatting.Indented);

            richTextBoxFormContent.Text = JsonConvert.SerializeObject(EditedDataObject, Formatting.Indented);
        }
        
        #region Save button delegate
        delegate void ShowSaveButtonCallBack(bool showSaveButton);
        public void ShowSaveButton(bool showSaveButton)
        {
            if (buttonSave.InvokeRequired)
            {
                var d = new ShowSaveButtonCallBack(ShowSaveButton);
                try
                {
                    Invoke(d, showSaveButton);
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
                    buttonSave.Visible = false;
                }
                catch
                {
                    // ignored
                }
            }
        }
        #endregion
        
        #region Form name delegate
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

        // Multi threading delegate for updating the user back in the original form.
        delegate void SetTextCallBack(string text);
        public void SetText(string text)
        {
            if (richTextBoxFormContent.InvokeRequired)
            {
                var d = new SetTextCallBack(SetText);
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
                    richTextBoxFormContent.AppendText(text);
                }
                catch
                {
                    // ignored
                }
            }           
        }

        #region OnSave event for passing back values to main grid
        public delegate void OnSaveEventHandler(object sender, OnSaveEventArgs e);
        public event OnSaveEventHandler OnSave;

        public class OnSaveEventArgs : EventArgs
        {
            public string RichTextBoxContents { get; set; }
            public DataGridViewCell CurrentCell { get; set; }

            public OnSaveEventArgs(string value, DataGridViewCell cell)
            {
                RichTextBoxContents = value;
                CurrentCell = cell;
            }
        }

        private void SaveJson(object sender, EventArgs e)
        {
            if (OnSave == null)
                return;

            OnSaveEventArgs args = new OnSaveEventArgs(richTextBoxFormContent.Text, EditedCell);
            OnSave(this, args);

            Close();
        }
        #endregion

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
