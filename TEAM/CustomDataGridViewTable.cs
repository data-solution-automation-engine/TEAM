using System;
using System.Data;
using System.Drawing;
using System.Linq;
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
            enabledIndicator.Name = TableMappingMetadataColumns.Enabled.ToString();
            enabledIndicator.HeaderText = TableMappingMetadataColumns.Enabled.ToString();
            enabledIndicator.DataPropertyName = TableMappingMetadataColumns.Enabled.ToString();
            Columns.Add(enabledIndicator);

            DataGridViewTextBoxColumn hashKey = new DataGridViewTextBoxColumn();
            hashKey.Name = TableMappingMetadataColumns.HashKey.ToString();
            hashKey.HeaderText = TableMappingMetadataColumns.HashKey.ToString();
            hashKey.DataPropertyName = TableMappingMetadataColumns.HashKey.ToString();
            hashKey.Visible = false;
            Columns.Add(hashKey);

            DataGridViewTextBoxColumn versionId = new DataGridViewTextBoxColumn();
            versionId.Name = TableMappingMetadataColumns.VersionId.ToString();
            versionId.HeaderText = TableMappingMetadataColumns.VersionId.ToString();
            versionId.DataPropertyName = TableMappingMetadataColumns.VersionId.ToString();
            versionId.Visible = false;
            Columns.Add(versionId);

            DataGridViewTextBoxColumn sourceTable = new DataGridViewTextBoxColumn();
            sourceTable.Name = TableMappingMetadataColumns.SourceTable.ToString();
            sourceTable.HeaderText = "Source Data Object";
            sourceTable.DataPropertyName = TableMappingMetadataColumns.SourceTable.ToString();
            Columns.Add(sourceTable);

            DataGridViewComboBoxColumn sourceConnection = new DataGridViewComboBoxColumn();
            sourceConnection.Name = TableMappingMetadataColumns.SourceConnection.ToString();
            sourceConnection.HeaderText = "Source Connection";
            sourceConnection.DataPropertyName = TableMappingMetadataColumns.SourceConnection.ToString();
            sourceConnection.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            sourceConnection.DataSource = LocalTeamConnection.GetConnections(FormBase.TeamConfigurationSettings.ConnectionDictionary);
            sourceConnection.DisplayMember = "ConnectionKey";
            sourceConnection.ValueMember = "ConnectionId";
            sourceConnection.ValueType = typeof(string);
            Columns.Add(sourceConnection);

        

            DataGridViewTextBoxColumn targetTable = new DataGridViewTextBoxColumn();
            targetTable.Name = TableMappingMetadataColumns.TargetTable.ToString();
            targetTable.HeaderText = "Target Data Object";
            targetTable.DataPropertyName = TableMappingMetadataColumns.TargetTable.ToString();
            Columns.Add(targetTable);

            DataGridViewComboBoxColumn targetConnection = new DataGridViewComboBoxColumn();
            targetConnection.Name = TableMappingMetadataColumns.TargetConnection.ToString();
            targetConnection.HeaderText = "Target Connection";
            targetConnection.DataPropertyName = TableMappingMetadataColumns.TargetConnection.ToString();
            targetConnection.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            targetConnection.DataSource = LocalTeamConnection.GetConnections(FormBase.TeamConfigurationSettings.ConnectionDictionary);
            targetConnection.DisplayMember = "ConnectionKey";
            targetConnection.ValueMember = "ConnectionId";
            targetConnection.ValueType = typeof(string);
            Columns.Add(targetConnection);

            DataGridViewTextBoxColumn businessKeyDefinition = new DataGridViewTextBoxColumn();
            businessKeyDefinition.Name = TableMappingMetadataColumns.BusinessKeyDefinition.ToString();
            businessKeyDefinition.HeaderText = "Business Key Definition";
            businessKeyDefinition.DataPropertyName = TableMappingMetadataColumns.BusinessKeyDefinition.ToString();
            Columns.Add(businessKeyDefinition);

            DataGridViewTextBoxColumn drivingKeyDefinition = new DataGridViewTextBoxColumn();
            drivingKeyDefinition.Name = TableMappingMetadataColumns.DrivingKeyDefinition.ToString();
            drivingKeyDefinition.HeaderText = "Driving Key Definition";
            drivingKeyDefinition.DataPropertyName = TableMappingMetadataColumns.DrivingKeyDefinition.ToString();
            Columns.Add(drivingKeyDefinition);

            DataGridViewTextBoxColumn filterCriterion = new DataGridViewTextBoxColumn();
            filterCriterion.Name = TableMappingMetadataColumns.FilterCriterion.ToString();
            filterCriterion.HeaderText = "Filter Criterion";
            filterCriterion.DataPropertyName = TableMappingMetadataColumns.FilterCriterion.ToString();
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

            string[] PresentationLayerLabelArray = FormBase.TeamConfigurationSettings.PresentationLayerLabels.Replace(" ", "").Split(',');

            foreach (DataGridViewRow row in Rows)
            {
                var targetTable = row.Cells[(int)TableMappingMetadataColumns.TargetTable].Value;
                var businessKeySyntax = row.Cells[(int)TableMappingMetadataColumns.BusinessKeyDefinition].Value;

                if (targetTable != null && businessKeySyntax != null && row.IsNewRow == false)
                {
                    // Hub
                    if (
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.TeamConfigurationSettings.HubTablePrefixValue)) ||
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.TeamConfigurationSettings.HubTablePrefixValue) )
                        )
                    {
                        this[(int)TableMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.CornflowerBlue;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    // Link-Sat
                    else if (
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.TeamConfigurationSettings.LsatTablePrefixValue)) ||
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.TeamConfigurationSettings.LsatTablePrefixValue))
                        )
                    {
                        this[(int)TableMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.Gold;
                    }
                    // Context
                    else if (
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.TeamConfigurationSettings.SatTablePrefixValue)) ||
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.TeamConfigurationSettings.SatTablePrefixValue))
                    )
                    {
                        this[(int)TableMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.Yellow;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    // Natural Business Relationship
                    else if (
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.TeamConfigurationSettings.LinkTablePrefixValue)) ||
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.TeamConfigurationSettings.LinkTablePrefixValue))
                    )
                    {
                        this[(int)TableMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.OrangeRed;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    // PSA
                    else if (
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.TeamConfigurationSettings.PsaTablePrefixValue)) ||
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.TeamConfigurationSettings.PsaTablePrefixValue))
                    )
                    {
                        this[(int)TableMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.AntiqueWhite;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    // Staging
                    else if (
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Prefix" && targetTable.ToString().StartsWith(FormBase.TeamConfigurationSettings.StgTablePrefixValue)) ||
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Suffix" && targetTable.ToString().EndsWith(FormBase.TeamConfigurationSettings.StgTablePrefixValue))
                    )
                    {
                        this[(int)TableMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.WhiteSmoke;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    // Presentation Layer
                    else if (
                        
                        (FormBase.TeamConfigurationSettings.TableNamingLocation == "Prefix" && PresentationLayerLabelArray.Any(s => targetTable.ToString().StartsWith(s)) )
                        ||
                       ( FormBase.TeamConfigurationSettings.TableNamingLocation == "Suffix" && PresentationLayerLabelArray.Any(s => targetTable.ToString().EndsWith(s)))
                    )
                    {
                        this[(int)TableMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.Aquamarine;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].ReadOnly = true;
                        row.Cells[(int)TableMappingMetadataColumns.DrivingKeyDefinition].Style.BackColor = Color.LightGray;
                    }
                    else
                    {
                        // Catch
                    }


                    //Syntax highlighting for code
                    if (businessKeySyntax.ToString().Contains("CONCATENATE") || businessKeySyntax.ToString().Contains("COMPOSITE"))
                    {      
                        this[(int)TableMappingMetadataColumns.BusinessKeyDefinition, counter].Style.ForeColor = Color.DarkBlue;
                        this[(int)TableMappingMetadataColumns.BusinessKeyDefinition, counter].Style.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
                    }
                }

                counter++;
            }
        }
    }
}
