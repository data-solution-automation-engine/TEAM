using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace TEAM
{
    internal class CustomDataGridViewTable : DataGridView 
    {
        private void DataGridView_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
           // var test = (DataGridView) sender;

           var bla =Rows[anError.RowIndex].Cells[anError.ColumnIndex].ErrorText;

            MessageBox.Show("Error " + anError.Context);

            if (anError.Context == DataGridViewDataErrorContexts.Commit)
            {
                MessageBox.Show("Commit error");
            }
            if (anError.Context == DataGridViewDataErrorContexts.CurrentCellChange)
            {
                MessageBox.Show("Cell change");
            }
            if (anError.Context == DataGridViewDataErrorContexts.Parsing)
            {
                MessageBox.Show("parsing error");
            }
            if (anError.Context == DataGridViewDataErrorContexts.LeaveControl)
            {
                MessageBox.Show("leave control error");
            }

            if ((anError.Exception) is ConstraintException)
            {
                DataGridView view = (DataGridView)sender;
                view.Rows[anError.RowIndex].ErrorText = "an error";
                view.Rows[anError.RowIndex].Cells[anError.ColumnIndex].ErrorText = "an error";

                anError.ThrowException = false;
            }
        }

        public CustomDataGridViewTable()
        {
            AutoGenerateColumns = false;
            ColumnHeadersVisible = true;
            EditMode = DataGridViewEditMode.EditOnEnter;

            DataError += (DataGridView_DataError);

            DataGridViewCheckBoxColumn enabledIndicator = new DataGridViewCheckBoxColumn();
            enabledIndicator.Name = TableMetadataColumns.Enabled.ToString();
            enabledIndicator.HeaderText = TableMetadataColumns.Enabled.ToString();
            enabledIndicator.DataPropertyName = TableMetadataColumns.Enabled.ToString();
            Columns.Add(enabledIndicator);

            DataGridViewTextBoxColumn hashKey = new DataGridViewTextBoxColumn();
            hashKey.Name = TableMetadataColumns.HashKey.ToString();
            hashKey.HeaderText = TableMetadataColumns.HashKey.ToString();
            hashKey.DataPropertyName = TableMetadataColumns.HashKey.ToString();
            hashKey.Visible = false;
            Columns.Add(hashKey);

            DataGridViewTextBoxColumn versionId = new DataGridViewTextBoxColumn();
            versionId.Name = TableMetadataColumns.VersionId.ToString();
            versionId.HeaderText = TableMetadataColumns.VersionId.ToString();
            versionId.DataPropertyName = TableMetadataColumns.VersionId.ToString();
            versionId.Visible = false;
            Columns.Add(versionId);

            DataGridViewTextBoxColumn sourceTable = new DataGridViewTextBoxColumn();
            sourceTable.Name = TableMetadataColumns.SourceTable.ToString();
            sourceTable.HeaderText = "Source Data Object";
            sourceTable.DataPropertyName = TableMetadataColumns.SourceTable.ToString();
            Columns.Add(sourceTable);

            DataGridViewComboBoxColumn sourceConnection = new DataGridViewComboBoxColumn();
            sourceConnection.Name = TableMetadataColumns.SourceConnection.ToString();
            sourceConnection.HeaderText = "Source Connection";
            sourceConnection.DataPropertyName = TableMetadataColumns.SourceConnection.ToString();
            sourceConnection.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            sourceConnection.DataSource = LocalConnection.GetConnections(FormBase.ConfigurationSettings.connectionDictionary);
            sourceConnection.DisplayMember = "ConnectionKey";
            sourceConnection.ValueMember = "ConnectionKey";
            sourceConnection.ValueType = typeof(string);
            Columns.Add(sourceConnection);

        

            DataGridViewTextBoxColumn targetTable = new DataGridViewTextBoxColumn();
            targetTable.Name = TableMetadataColumns.TargetTable.ToString();
            targetTable.HeaderText = "Target Data Object";
            targetTable.DataPropertyName = TableMetadataColumns.TargetTable.ToString();
            Columns.Add(targetTable);

            DataGridViewComboBoxColumn targetConnection = new DataGridViewComboBoxColumn();
            targetConnection.Name = TableMetadataColumns.TargetConnection.ToString();
            targetConnection.HeaderText = "Target Connection";
            targetConnection.DataPropertyName = TableMetadataColumns.TargetConnection.ToString();
            targetConnection.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            targetConnection.DataSource = LocalConnection.GetConnections(FormBase.ConfigurationSettings.connectionDictionary);
            targetConnection.DisplayMember = "ConnectionKey";
            targetConnection.ValueMember = "ConnectionKey";
            targetConnection.ValueType = typeof(string);
            Columns.Add(targetConnection);

            DataGridViewTextBoxColumn businessKeyDefinition = new DataGridViewTextBoxColumn();
            businessKeyDefinition.Name = TableMetadataColumns.BusinessKeyDefinition.ToString();
            businessKeyDefinition.HeaderText = "Business Key Definition";
            businessKeyDefinition.DataPropertyName = TableMetadataColumns.BusinessKeyDefinition.ToString();
            Columns.Add(businessKeyDefinition);

            DataGridViewTextBoxColumn drivingKeyDefinition = new DataGridViewTextBoxColumn();
            drivingKeyDefinition.Name = TableMetadataColumns.DrivingKeyDefinition.ToString();
            drivingKeyDefinition.HeaderText = "Driving Key Definition";
            drivingKeyDefinition.DataPropertyName = TableMetadataColumns.DrivingKeyDefinition.ToString();
            Columns.Add(drivingKeyDefinition);

            DataGridViewTextBoxColumn filterCriterion = new DataGridViewTextBoxColumn();
            filterCriterion.Name = TableMetadataColumns.FilterCriterion.ToString();
            filterCriterion.HeaderText = "Filter Criterion";
            filterCriterion.DataPropertyName = TableMetadataColumns.FilterCriterion.ToString();
            Columns.Add(filterCriterion);
        }

      //  protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
      //  {

            //MessageBox.Show("Key Press Detected");

         //   if ((keyData == (Keys.Control | Keys.C)))
         //   {
                //Copy data

          //  }


            //try
            //{
            //    if (e.Modifiers == Keys.Control)
            //    {
            //        switch (e.KeyCode)
            //        {
            //            case Keys.V:
            //                PasteClipboardTableMetadata();
            //                break;
            //            //case Keys.C:
            //            //    Clipboard.SetText(e.ToString());
            //            //    break;
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show("Pasting into the data grid has failed", "Copy/Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}

          //  return base.ProcessCmdKey(ref msg, keyData);
       // }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ColourGridView();
        }

        private void ColourGridView()
        {
            var counter = 0;

            foreach (DataGridViewRow row in Rows)
            {
                var targetTable = row.Cells[(int)TableMetadataColumns.TargetTable].Value;
                var businessKeySyntax = row.Cells[(int)TableMetadataColumns.BusinessKeyDefinition].Value;

                if (targetTable != null && businessKeySyntax != null && row.IsNewRow == false)
                {
                    // Hub
                    if (
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.ConfigurationSettings.HubTablePrefixValue)) ||
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.ConfigurationSettings.HubTablePrefixValue) )
                        )
                    {
                        this[(int)TableMetadataColumns.TargetTable, counter].Style.BackColor = Color.CornflowerBlue;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    // Link-Sat
                    else if (
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.ConfigurationSettings.LsatTablePrefixValue)) ||
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.ConfigurationSettings.LsatTablePrefixValue))
                        )
                    {
                        this[(int)TableMetadataColumns.TargetTable, counter].Style.BackColor = Color.Gold;
                    }
                    // Sat
                    else if (
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.ConfigurationSettings.SatTablePrefixValue)) ||
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.ConfigurationSettings.SatTablePrefixValue))
                    )
                    {
                        this[(int)TableMetadataColumns.TargetTable, counter].Style.BackColor = Color.Yellow;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    else if (
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.ConfigurationSettings.LinkTablePrefixValue)) ||
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.ConfigurationSettings.LinkTablePrefixValue))
                    )
                    {
                        this[(int)TableMetadataColumns.TargetTable, counter].Style.BackColor = Color.OrangeRed;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    else if (
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.ConfigurationSettings.PsaTablePrefixValue)) ||
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.ConfigurationSettings.PsaTablePrefixValue))
                    )
                    {
                        this[(int)TableMetadataColumns.TargetTable, counter].Style.BackColor = Color.AntiqueWhite;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    else if (
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.ConfigurationSettings.StgTablePrefixValue)) ||
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.ConfigurationSettings.StgTablePrefixValue))
                    )
                    {
                        this[(int)TableMetadataColumns.TargetTable, counter].Style.BackColor = Color.WhiteSmoke;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    else if (
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith("DIM_")) ||
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith("_DIM"))
                    )
                    {
                        this[(int)TableMetadataColumns.TargetTable, counter].Style.BackColor = Color.Aqua;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    else if (
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith("FACT_")) ||
                        (FormBase.ConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith("_FACT"))
                    )
                    {
                        this[(int)TableMetadataColumns.TargetTable, counter].Style.BackColor = Color.MediumAquamarine;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }

                    //Syntax highlighting for code
                    if (businessKeySyntax.ToString().Contains("CONCATENATE") || businessKeySyntax.ToString().Contains("COMPOSITE"))
                    {      
                        this[(int)TableMetadataColumns.BusinessKeyDefinition, counter].Style.ForeColor = Color.DarkBlue;
                        this[(int)TableMetadataColumns.BusinessKeyDefinition, counter].Style.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
                    }
                }

                counter++;
            }
        }
    }
}
