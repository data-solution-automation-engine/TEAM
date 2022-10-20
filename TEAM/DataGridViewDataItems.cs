using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TEAM_Library;
using static TEAM.FormBase;

namespace TEAM
{
    internal sealed class DataGridViewDataItems : DataGridView
    {
        private ContextMenuStrip contextMenuStripAttributeMapping;
        private ToolStripMenuItem deleteThisRowFromTheGridToolStripMenuItem;

        public DataGridViewDataItems()
        {
            #region Basic properties
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BorderStyle = BorderStyle.None;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            EditMode = DataGridViewEditMode.EditOnEnter;

            var mySize = new Size(1100, 540);
            MinimumSize = mySize;
            Size = mySize;

            AutoGenerateColumns = false;
            ColumnHeadersVisible = true;
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Define grid view control.
            Name = "dataGridViewDataItem";
            Location = new Point(2, 3);
            TabIndex = 3;
            #endregion

            #region Event Handlers
            KeyDown += DataGridViewDataItems_KeyDown;
            MouseDown += DataGridViewDataItems_MouseDown;
            #endregion

            #region Columns
            // Hashkey
            if (!Controls.ContainsKey(DataItemMappingMetadataColumns.HashKey.ToString()))
            {
                DataGridViewTextBoxColumn hashKey = new DataGridViewTextBoxColumn();
                hashKey.Name = DataItemMappingMetadataColumns.HashKey.ToString();
                hashKey.HeaderText = DataItemMappingMetadataColumns.HashKey.ToString();
                hashKey.DataPropertyName = DataItemMappingMetadataColumns.HashKey.ToString();
                hashKey.Visible = false;
                Columns.Add(hashKey);
            }

            // Source Data Object
            if (!Controls.ContainsKey(DataItemMappingMetadataColumns.SourceTable.ToString()))
            {
                DataGridViewTextBoxColumn sourceTable = new DataGridViewTextBoxColumn();
                sourceTable.Name = DataItemMappingMetadataColumns.SourceTable.ToString();
                sourceTable.HeaderText = DataItemMappingMetadataColumns.SourceTable.ToString();
                sourceTable.DataPropertyName = DataItemMappingMetadataColumns.SourceTable.ToString();
                sourceTable.Visible = true;
                Columns.Add(sourceTable);
            }

            // Source Column
            if (!Controls.ContainsKey(DataItemMappingMetadataColumns.SourceColumn.ToString()))
            {
                DataGridViewTextBoxColumn sourceColumn = new DataGridViewTextBoxColumn();
                sourceColumn.Name = DataItemMappingMetadataColumns.SourceColumn.ToString();
                sourceColumn.HeaderText = DataItemMappingMetadataColumns.SourceColumn.ToString();
                sourceColumn.DataPropertyName = DataItemMappingMetadataColumns.SourceColumn.ToString();
                sourceColumn.Visible = true;
                Columns.Add(sourceColumn);
            }

            // Target Data Object
            if (!Controls.ContainsKey(DataItemMappingMetadataColumns.TargetTable.ToString()))
            {
                DataGridViewTextBoxColumn targetTable = new DataGridViewTextBoxColumn();
                targetTable.Name = DataItemMappingMetadataColumns.TargetTable.ToString();
                targetTable.HeaderText = DataItemMappingMetadataColumns.TargetTable.ToString();
                targetTable.DataPropertyName = DataItemMappingMetadataColumns.TargetTable.ToString();
                targetTable.Visible = true;
                Columns.Add(targetTable);
            }

            // Target Column
            if (!Controls.ContainsKey(DataItemMappingMetadataColumns.TargetColumn.ToString()))
            {
                DataGridViewTextBoxColumn targetColumn = new DataGridViewTextBoxColumn();
                targetColumn.Name = DataItemMappingMetadataColumns.TargetColumn.ToString();
                targetColumn.HeaderText = DataItemMappingMetadataColumns.TargetColumn.ToString();
                targetColumn.DataPropertyName = DataItemMappingMetadataColumns.TargetColumn.ToString();
                targetColumn.Visible = true;
                Columns.Add(targetColumn);
            }

            // Notes
            if (!Controls.ContainsKey(DataItemMappingMetadataColumns.Notes.ToString()))
            {
                DataGridViewTextBoxColumn notes = new DataGridViewTextBoxColumn();
                notes.Name = DataItemMappingMetadataColumns.Notes.ToString();
                notes.HeaderText = DataItemMappingMetadataColumns.Notes.ToString();
                notes.DataPropertyName = DataItemMappingMetadataColumns.Notes.ToString();
                notes.Visible = false;
                Columns.Add(notes);
            }
            #endregion

            #region Context menu
            contextMenuStripAttributeMapping = new ContextMenuStrip();
            contextMenuStripAttributeMapping.SuspendLayout();

            // Delete row menu item
            deleteThisRowFromTheGridToolStripMenuItem = new ToolStripMenuItem();
            deleteThisRowFromTheGridToolStripMenuItem.Name = "deleteThisRowFromTheGridToolStripMenuItem";
            deleteThisRowFromTheGridToolStripMenuItem.Size = new Size(225, 22);
            deleteThisRowFromTheGridToolStripMenuItem.Text = @"Delete this row from the grid";
            deleteThisRowFromTheGridToolStripMenuItem.Click += DeleteThisRowFromTableDataGridToolStripMenuItem_Click;

            contextMenuStripAttributeMapping.ImageScalingSize = new Size(24, 24);
            contextMenuStripAttributeMapping.Items.AddRange(new ToolStripItem[] {
                deleteThisRowFromTheGridToolStripMenuItem
            });

            contextMenuStripAttributeMapping.Name = "contextMenuStripDataItemMapping";
            contextMenuStripAttributeMapping.Size = new Size(340, 48);

            contextMenuStripAttributeMapping.ResumeLayout(false);
            #endregion
        }

        /// <summary>
        /// This method is called from the context menu on the data grid. It deletes the row from the grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteThisRowFromTableDataGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRows = SelectedRows;

            foreach (DataGridViewRow row in selectedRows)
            {
                if (row.IsNewRow)
                {

                }
                else
                {
                    Int32 rowToDelete = Rows.GetFirstRow(DataGridViewElementStates.Selected);
                    Rows.RemoveAt(rowToDelete);
                }
            }
        }

        private void DataGridViewDataItems_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTestInfo = HitTest(e.X, e.Y);

                // For now, do nothing when any of the column headers are right-clicked.
                if (hitTestInfo.RowIndex == -1)
                    return;

                // Clear existing selection.
                ClearSelection();

                //if (hitTestInfo.ColumnIndex == -1)
                //{
                    // Select the full row when the default column is right-clicked.
                    Rows[hitTestInfo.RowIndex].Selected = true;
                    ContextMenuStrip = contextMenuStripAttributeMapping;
                //}
                //else
                //{
                //    // Evaluate which cell is clicked.
                //    var cell = this[hitTestInfo.ColumnIndex, hitTestInfo.RowIndex];

                //    //if (cell.ReadOnly)
                //    //{
                //    //    // Do nothing / ignore.
                //    //}
                //    if (hitTestInfo.ColumnIndex == (int)DataObjectMappingGridColumns.SourceDataObject || hitTestInfo.ColumnIndex == (int)DataObjectMappingGridColumns.TargetDataObject)
                //    {
                //        CurrentCell = cell;
                //        ContextMenuStrip = contextMenuStripDataObjectMappingSingleCell;
                //    }
                //    else
                //    {
                //        Rows[hitTestInfo.RowIndex].Selected = true;
                //        ContextMenuStrip = contextMenuStripDataObjectMappingFullRow;
                //    }
                //}
            }
        }

        private void PasteClipboardAttributeMetadata()
        {
            try
            {
                string s = Clipboard.GetText();
                string[] lines = s.Split('\n');

                int iRow = CurrentCell.RowIndex;
                int iCol = CurrentCell.ColumnIndex;
                DataGridViewCell oCell;
                if (iRow + lines.Length > Rows.Count - 1)
                {
                    bool bFlag = false;
                    foreach (string sEmpty in lines)
                    {
                        if (sEmpty == "")
                        {
                            bFlag = true;
                        }
                    }

                    int iNewRows = iRow + lines.Length - Rows.Count;
                    if (iNewRows > 0)
                    {
                        if (bFlag)
                            Rows.Add(iNewRows);
                        else
                            Rows.Add(iNewRows + 1);
                    }
                    else
                        Rows.Add(iNewRows + 1);
                }

                foreach (string line in lines)
                {
                    if (iRow < RowCount && line.Length > 0)
                    {
                        string[] sCells = line.Split('\t');
                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            if (iCol + i < ColumnCount)
                            {
                                oCell = this[iCol + i, iRow];
                                oCell.Value = Convert.ChangeType(sCells[i].Replace("\r", ""), oCell.ValueType);
                            }
                            else
                            {
                                break;
                            }
                        }

                        iRow++;
                    }
                    else
                    {
                        break;
                    }
                }

                //Clipboard.Clear();
            }
            catch (FormatException ex)
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered: {ex.Message}."));
            }
        }

        /// <summary>
        /// DataGridView OnKeyDown event for _dataGridViewDataItems
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewDataItems_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.V:
                            PasteClipboardAttributeMetadata();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Pasting into the data grid has failed", "Copy/Paste", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            ColourGridViewAttribute();
        }

        private void ColourGridViewAttribute()
        {
            var counter = 0;

            var hubIdentifier = "";
            var satIdentifier = "";
            var lnkIdentifier = "";
            var lsatIdentifier = "";
            var stgIdentifier = "";
            var psaIdentifier = "";
            var dimIdentifier = "";
            var factIdentifier = "";

            if (FormBase.TeamConfiguration.TableNamingLocation == "Prefix")
            {
                hubIdentifier = FormBase.TeamConfiguration.HubTablePrefixValue;
                satIdentifier = FormBase.TeamConfiguration.SatTablePrefixValue ;
                lnkIdentifier = FormBase.TeamConfiguration.LinkTablePrefixValue ;
                lsatIdentifier = FormBase.TeamConfiguration.LsatTablePrefixValue;
                stgIdentifier = FormBase.TeamConfiguration.StgTablePrefixValue ;
                psaIdentifier = FormBase.TeamConfiguration.PsaTablePrefixValue ;
                dimIdentifier = "DIM_";
                factIdentifier = "FACT_";
            }
            else
            {
                hubIdentifier = FormBase.TeamConfiguration.HubTablePrefixValue;
                satIdentifier =FormBase.TeamConfiguration.SatTablePrefixValue;
                lnkIdentifier =  FormBase.TeamConfiguration.LinkTablePrefixValue;
                lsatIdentifier = FormBase.TeamConfiguration.LsatTablePrefixValue;
                stgIdentifier = FormBase.TeamConfiguration.StgTablePrefixValue;
                psaIdentifier = FormBase.TeamConfiguration.PsaTablePrefixValue;
                dimIdentifier = "_DIM";
                factIdentifier = "_FACT";
            }

            foreach (DataGridViewRow row in Rows)
            {
                var integrationTable = row.Cells[(int)DataItemMappingMetadataColumns.TargetTable].Value;

                if (integrationTable != null && row.IsNewRow == false)
                {
                    // Backcolour for Integration Layer tables
                    if (Regex.Matches(integrationTable.ToString(), hubIdentifier).Count>0)
                    {
                        this[(int)DataItemMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.CornflowerBlue;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), lsatIdentifier).Count > 0)
                    {
                        this[(int)DataItemMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.Gold;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), satIdentifier).Count > 0)
                    {
                        this[(int)DataItemMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.Yellow;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), lnkIdentifier).Count > 0)
                    {
                        this[(int)DataItemMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.OrangeRed;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), psaIdentifier).Count > 0)
                    {
                        this[(int)DataItemMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.AntiqueWhite;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), stgIdentifier).Count > 0)
                    {
                        this[(int)DataItemMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.WhiteSmoke;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), dimIdentifier).Count > 0)
                    {
                        this[(int)DataItemMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.Aqua;
                    }
                    else if (Regex.Matches(integrationTable.ToString(), factIdentifier).Count > 0)
                    {
                        this[(int)DataItemMappingMetadataColumns.TargetTable, counter].Style.BackColor = Color.MediumAquamarine;
                    }
                }

                counter++;
            }
        }
    }
}
