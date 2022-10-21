using System;
using System.Drawing;
using System.Windows.Forms;
using TEAM_Library;
using static TEAM.FormBase;

namespace TEAM
{
    internal sealed class DataGridViewPhysicalModel : DataGridView
    {
        private readonly ContextMenuStrip contextMenuStrip;

        public DataGridViewPhysicalModel()
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
            Name = "dataGridPhysicalModel";
            Location = new Point(2, 3);
            TabIndex = 3;
            #endregion

            #region Columns
            // Hash Key
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.HashKey.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.HashKey.ToString();
                column.HeaderText = PhysicalModelMappingMetadataColumns.HashKey.ToString();
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.HashKey.ToString();
                column.Visible = false;
                Columns.Add(column);
            }

            // Database Name
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.DatabaseName.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.DatabaseName.ToString();
                column.HeaderText = @"Database Name";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.DatabaseName.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Schema Name
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.SchemaName.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.SchemaName.ToString();
                column.HeaderText = @"Schema Name";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.SchemaName.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Table Name
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.TableName.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.TableName.ToString();
                column.HeaderText = @"Table Name";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.TableName.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Column Name
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.ColumnName.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.ColumnName.ToString();
                column.HeaderText = @"Column Name";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.ColumnName.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Data Type
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.DataType.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.DataType.ToString();
                column.HeaderText = @"Data Type";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.DataType.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Character Length
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.CharacterLength.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.CharacterLength.ToString();
                column.HeaderText = @"Character Length";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.CharacterLength.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Numeric Precision
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.NumericPrecision.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.NumericPrecision.ToString();
                column.HeaderText = @"Numeric Precision";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.NumericPrecision.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Numeric Scale
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.NumericScale.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.NumericScale.ToString();
                column.HeaderText = @"Numeric Scale";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.NumericScale.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Ordinal Position
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString();
                column.HeaderText = @"Ordinal Position";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Primary Key Indicator
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator.ToString();
                column.HeaderText = @"Primary Key Indicator";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Multi Active Indicator
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.MultiActiveIndicator.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.MultiActiveIndicator.ToString();
                column.HeaderText = @"Multi Active Indicator";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.MultiActiveIndicator.ToString();
                column.Visible = true;
                Columns.Add(column);
            }
            #endregion

            #region Event Handlers
            KeyDown += DataGridView_KeyDown;
            MouseDown += DataGridView_MouseDown;
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

                // Select the full row when the default column is right-clicked.
                Rows[hitTestInfo.RowIndex].Selected = true;
                ContextMenuStrip = contextMenuStrip;
            }
        }

        /// <summary>
        /// DataGridView OnKeyDown.
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
    }
}
