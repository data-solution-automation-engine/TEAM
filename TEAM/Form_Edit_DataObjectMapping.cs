using Newtonsoft.Json;
using System;
using System.Windows.Forms;
using DataWarehouseAutomation;

namespace TEAM
{
    public partial class Form_Edit_DataObjectMapping : FormBase
    {
        public DataGridViewRow EditedRow { get; set; }
        public DataObjectMapping EditedDataObjectMapping { get; set; }

        public Form_Edit_DataObjectMapping()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
        }

        public Form_Edit_DataObjectMapping(DataGridViewRow row, DataObjectMapping dataObjectMapping)
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            EditedRow = row;
            EditedDataObjectMapping = dataObjectMapping;

            //DataRowView dataBoundItem = (DataRowView)EditedRow.DataBoundItem;

            //EditedDataObjectMapping = (DataWarehouseAutomation.DataObjectMapping)dataBoundItem.Row[cell.ColumnIndex];

            //var dataObjectMapping = DataGridViewDataObjects.GetDataObjectMapping(row);
            //string dataObjectMappingJson = JsonConvert.SerializeObject(dataObjectMapping, Formatting.Indented);

            richTextBoxFormContent.Text = JsonConvert.SerializeObject(EditedDataObjectMapping, Formatting.Indented);
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

        #region Set text delegate

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

        #endregion

        #region OnSave event for passing back values to main grid

        public delegate void OnSaveEventHandler(object sender, OnSaveEventArgs e);
        public event OnSaveEventHandler OnSave;

        public class OnSaveEventArgs : EventArgs
        {
            public string RichTextBoxContents { get; set; }
            public DataGridViewRow CurrentRow { get; set; }

            public OnSaveEventArgs(string value, DataGridViewRow row)
            {
                RichTextBoxContents = value;
                CurrentRow = row;
            }
        }

        private void SaveJson(object sender, EventArgs e)
        {
            if (OnSave == null)
                return;

            OnSaveEventArgs args = new OnSaveEventArgs(richTextBoxFormContent.Text, EditedRow);
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
