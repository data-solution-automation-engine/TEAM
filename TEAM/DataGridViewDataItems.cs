using System;
using System.Drawing;
using System.Windows.Forms;
using TEAM_Library;
using static TEAM.FormBase;

namespace TEAM
{
    internal sealed class DataGridViewDataItems : DataGridView
    {
        private readonly ContextMenuStrip contextMenuStrip;

        public DataGridViewDataItems()
        {
            #region Basic properties
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BorderStyle = BorderStyle.None;

            // Disable resizing for performance, will be enabled after binding.
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            EditMode = DataGridViewEditMode.EditOnEnter;

            var mySize = new Size(1100, 540);
            MinimumSize = mySize;
            Size = mySize;

            AutoGenerateColumns = false;
            ColumnHeadersVisible = true;


            // Define grid view control.
            Name = "dataGridViewDataItem";
            Location = new Point(2, 3);
            TabIndex = 3;
            #endregion

            #region Event Handlers
            KeyDown += DataGridView_KeyDown;
            MouseDown += DataGridView_MouseDown;
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
                sourceTable.HeaderText = @"Source Data Object";
                sourceTable.DataPropertyName = DataItemMappingMetadataColumns.SourceTable.ToString();
                sourceTable.Visible = true;
                Columns.Add(sourceTable);
            }

            // Source Column
            if (!Controls.ContainsKey(DataItemMappingMetadataColumns.SourceColumn.ToString()))
            {
                DataGridViewTextBoxColumn sourceColumn = new DataGridViewTextBoxColumn();
                sourceColumn.Name = DataItemMappingMetadataColumns.SourceColumn.ToString();
                sourceColumn.HeaderText = @"Source Data Item";
                sourceColumn.DataPropertyName = DataItemMappingMetadataColumns.SourceColumn.ToString();
                sourceColumn.Visible = true;
                Columns.Add(sourceColumn);
            }

            // Target Data Object
            if (!Controls.ContainsKey(DataItemMappingMetadataColumns.TargetTable.ToString()))
            {
                DataGridViewTextBoxColumn targetTable = new DataGridViewTextBoxColumn();
                targetTable.Name = DataItemMappingMetadataColumns.TargetTable.ToString();
                targetTable.HeaderText = @"Target Data Object";
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
                notes.HeaderText = @"Target Data Item";
                notes.DataPropertyName = DataItemMappingMetadataColumns.Notes.ToString();
                notes.Visible = false;
                Columns.Add(notes);
            }
            #endregion

            #region Context menu
            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.SuspendLayout();

            // Delete row menu item
            var deleteRow = new ToolStripMenuItem();
            deleteRow.Name = "deleteThisRowFromTheGridToolStripMenuItem";
            deleteRow.Size = new Size(225, 22);
            deleteRow.Text = @"Delete this row from the grid";
            deleteRow.Click += DeleteRowFromGridToolStripMenuItem_Click;

            contextMenuStrip.ImageScalingSize = new Size(24, 24);
            contextMenuStrip.Items.AddRange(new ToolStripItem[] {
                deleteRow
            });

            contextMenuStrip.Name = "contextMenuStripDataItemMapping";
            contextMenuStrip.Size = new Size(340, 48);

            contextMenuStrip.ResumeLayout(false);
            #endregion
        }

        /// <summary>
        /// This method is called from the context menu on the data grid. It deletes the row from the grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteRowFromGridToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void DataGridView_MouseDown(object sender, MouseEventArgs e)
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
                    ContextMenuStrip = contextMenuStrip;
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

        private void PasteClipboard()
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
        private void DataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.V:
                            PasteClipboard();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(@"Pasting into the data grid has failed", @"Copy/Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
