using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TEAM_Library;
using static TEAM.FormBase;

namespace TEAM
{
    internal sealed class DataGridViewPhysicalModel : DataGridView
    {
        private readonly ContextMenuStrip contextMenuStrip;
        private readonly ContextMenuStrip contextMenuStripMultipleRows;

        internal Form Parent;

        public DataGridViewPhysicalModel(Form parent)
        {
            #region Basic properties
            Parent = parent;

            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BorderStyle = BorderStyle.None;

            // Disable resizing for performance, will be enabled after binding.

            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            EditMode = DataGridViewEditMode.EditOnKeystroke;

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

            // Database Name
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.databaseName.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.databaseName.ToString();
                column.HeaderText = @"Database Name";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.databaseName.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Schema Name
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.schemaName.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.schemaName.ToString();
                column.HeaderText = @"Schema Name";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.schemaName.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Table Name
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.tableName.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.tableName.ToString();
                column.HeaderText = @"Table Name";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.tableName.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Column Name
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.columnName.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.columnName.ToString();
                column.HeaderText = @"Column Name";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.columnName.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Data Type
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.dataType.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.dataType.ToString();
                column.HeaderText = @"Data Type";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.dataType.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Character Length
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.characterLength.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.characterLength.ToString();
                column.HeaderText = @"Character Length";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.characterLength.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Numeric Precision
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.numericPrecision.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.numericPrecision.ToString();
                column.HeaderText = @"Numeric Precision";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.numericPrecision.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Numeric Scale
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.numericScale.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.numericScale.ToString();
                column.HeaderText = @"Numeric Scale";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.numericScale.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Ordinal Position
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.ordinalPosition.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.ordinalPosition.ToString();
                column.HeaderText = @"Ordinal Position";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.ordinalPosition.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Primary Key Indicator
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.primaryKeyIndicator.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.primaryKeyIndicator.ToString();
                column.HeaderText = @"Primary Key Indicator";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.primaryKeyIndicator.ToString();
                column.Visible = true;
                Columns.Add(column);
            }

            // Multi Active Indicator
            if (!Controls.ContainsKey(PhysicalModelMappingMetadataColumns.multiActiveIndicator.ToString()))
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = PhysicalModelMappingMetadataColumns.multiActiveIndicator.ToString();
                column.HeaderText = @"Multi Active Indicator";
                column.DataPropertyName = PhysicalModelMappingMetadataColumns.multiActiveIndicator.ToString();
                column.Visible = true;
                Columns.Add(column);
            }
            #endregion

            #region Event Handlers

            KeyDown += DataGridView_KeyDown;
            MouseDown += DataGridView_MouseDown;
            RowPostPaint += OnRowPostPaint;
            DataError += OnDataError;

            #endregion

            #region Context menu

            #region Multiple rows context menu

            // Single cell context menu
            contextMenuStripMultipleRows = new ContextMenuStrip();
            contextMenuStripMultipleRows.SuspendLayout();

            // Modify JSON menu item
            var toolStripMenuItemDeleteMultipleRows = new ToolStripMenuItem();
            toolStripMenuItemDeleteMultipleRows.Name = "toolStripMenuItemDeleteMultipleRows";
            toolStripMenuItemDeleteMultipleRows.Size = new Size(143, 22);
            toolStripMenuItemDeleteMultipleRows.Text = @"Delete selected rows";
            toolStripMenuItemDeleteMultipleRows.Click += toolStripMenuItemDeleteMultipleRows_Click;

            contextMenuStripMultipleRows.Items.AddRange(new ToolStripItem[] {
                toolStripMenuItemDeleteMultipleRows
            });
            contextMenuStripMultipleRows.Name = $"{contextMenuStripMultipleRows.Name}";
            contextMenuStripMultipleRows.Size = new Size(144, 26);
            contextMenuStripMultipleRows.ResumeLayout(false);

            #endregion

            #region Single row context menu

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

            #endregion
        }

        private void OnDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            try
            {
                Rows[e.RowIndex].ErrorText = $"{e.Exception}";
                e.Cancel = true;
            }
            catch (Exception exception)
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered formatting the physical model grid: {exception.Message}."));
            }
        }

        private void toolStripMenuItemDeleteMultipleRows_Click(object sender, EventArgs e)
        {

            foreach (DataGridViewColumn column in Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }

            foreach (DataGridViewRow row in SelectedRows)
            {
                if (!row.IsNewRow)
                {
                    Rows.RemoveAt(row.Index);
                }
            }
        }

        private void OnRowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIndex = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // Right-align the number value.
                Alignment = StringAlignment.Center,

                LineAlignment = StringAlignment.Center
            };

            Size textSize = TextRenderer.MeasureText(rowIndex, Font);

            // Resize iff the header width is smaller than the string width.
            if (grid.RowHeadersWidth < textSize.Width + 40)
            {
                grid.RowHeadersWidth = textSize.Width + 40;
            }

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIndex, Font, SystemBrushes.ControlText, headerBounds, centerFormat);
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
                {
                    return;
                }


                // Select the full row when the default column is right-clicked.
                if (SelectedRows.Count <= 1)
                {
                    // Clear existing selection.
                    ClearSelection();

                    // Select the full row when the default column is right-clicked.
                    Rows[hitTestInfo.RowIndex].Selected = true;
                    ContextMenuStrip = contextMenuStrip;
                }
                else
                {
                    ContextMenuStrip = contextMenuStripMultipleRows;
                }
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
            catch
            {
                MessageBox.Show(@"Pasting into the data grid has failed", @"Copy/Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PasteClipboard()
        {
            try
            {
                string clipboardText = Clipboard.GetText();
                List<string> clipboardTextLines = clipboardText.Split('\n').ToList();

                for (int i = 0; i < clipboardTextLines.Count; i++)
                {
                    if (clipboardTextLines[i] == "")
                    {
                        clipboardTextLines.RemoveAt(i);
                    }
                }

                int currentRowIndex = CurrentCell.RowIndex;
                int currentColumnIndex = CurrentCell.ColumnIndex;

                // Add rows to the grid.
                if (currentRowIndex + clipboardTextLines.Count > Rows.Count - 1)
                {
                    int newRowCount = currentRowIndex + clipboardTextLines.Count - Rows.Count;

                    BindingSource bindingSource = DataSource as BindingSource;
                    DataTable dataTable = (DataTable)bindingSource.DataSource;

                    if (newRowCount > 0)
                    {
                        foreach (var value in clipboardTextLines)
                        {
                            // Add the row(s) to the underlying data table.
                            DataRow drToAdd = dataTable.NewRow();
                            dataTable.Rows.Add(drToAdd);
                        }
                    }
                    else
                    {
                        // Add the row(s) to the underlying data table.
                        DataRow drToAdd = dataTable.NewRow();
                        dataTable.Rows.Add(drToAdd);
                    }

                    // Remove superfluous rows.
                    Rows.RemoveAt(Rows.Count - 2);
                }

                // Adding the values.
                foreach (string line in clipboardTextLines)
                {
                    if (currentRowIndex < RowCount && line.Length > 0)
                    {
                        string[] sCells = line.Split('\t');

                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            if (currentColumnIndex + i < ColumnCount)
                            {
                                var cell = this[currentColumnIndex + i, currentRowIndex];
                                cell.Value = Convert.ChangeType(sCells[i].Replace("\r", ""), cell.ValueType);
                            }
                            else
                            {
                                break;
                            }
                        }

                        currentRowIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                var exceptionMessage = $"An exception has been encountered: {exception.Message}.";
                var targetControl = Parent.Controls.Find("richTextBoxInformation", true).FirstOrDefault() as RichTextBox;

                ThreadHelper.SetText(Parent, targetControl, $"{exceptionMessage}\r\n");
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, exceptionMessage));
            }
        }
    }
}
