using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TEAM_Library;
using static TEAM.DataGridViewDataObjects;
using static TEAM.FormBase;
using Utility = TEAM_Library.Utility;

namespace TEAM
{
    internal sealed class DataGridViewDataItems : DataGridView
    {
        private TeamConfiguration TeamConfiguration { get; }

        private readonly ContextMenuStrip contextMenuStrip;
        private readonly ContextMenuStrip contextMenuStripMultipleRows;

        public delegate void DataObjectParseHandler(object sender, ParseEventArgs e);
        public event DataObjectParseHandler OnDataObjectParse;

        public delegate void HeaderSortHandler(object sender, FilterEventArgs e);
        public event HeaderSortHandler OnHeaderSort;

        internal Form Parent;

        public DataGridViewDataItems(TeamConfiguration teamConfiguration, Form parent)
        {
            TeamConfiguration = teamConfiguration;
            Parent = parent;

            #region Basic properties

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
            Name = "dataGridViewDataItem";
            Location = new Point(2, 3);
            TabIndex = 3;

            #endregion

            #region Event Handlers

            KeyDown += DataGridView_KeyDown;
            MouseDown += DataGridView_MouseDown;
            CellFormatting += DataGridViewDataItems_CellFormatting;
            Sorted += DataGridViewDataItems_Sorted;
            RowPostPaint += OnRowPostPaint;

            #endregion

            #region Columns
            // Hashkey
            if (!Controls.ContainsKey(DataItemMappingGridColumns.HashKey.ToString()))
            {
                DataGridViewTextBoxColumn hashKey = new DataGridViewTextBoxColumn();
                hashKey.Name = DataItemMappingGridColumns.HashKey.ToString();
                hashKey.HeaderText = DataItemMappingGridColumns.HashKey.ToString();
                hashKey.DataPropertyName = DataItemMappingGridColumns.HashKey.ToString();
                hashKey.Visible = false;
                Columns.Add(hashKey);
            }

            // Source Data Object
            if (!Controls.ContainsKey(DataItemMappingGridColumns.SourceDataObject.ToString()))
            {
                DataGridViewTextBoxColumn sourceTable = new DataGridViewTextBoxColumn();
                sourceTable.Name = DataItemMappingGridColumns.SourceDataObject.ToString();
                sourceTable.HeaderText = @"Source Data Object";
                sourceTable.DataPropertyName = DataItemMappingGridColumns.SourceDataObject.ToString();
                sourceTable.Visible = true;
                Columns.Add(sourceTable);
            }

            // Source Column
            if (!Controls.ContainsKey(DataItemMappingGridColumns.SourceDataItem.ToString()))
            {
                DataGridViewTextBoxColumn sourceColumn = new DataGridViewTextBoxColumn();
                sourceColumn.Name = DataItemMappingGridColumns.SourceDataItem.ToString();
                sourceColumn.HeaderText = @"Source Data Item";
                sourceColumn.DataPropertyName = DataItemMappingGridColumns.SourceDataItem.ToString();
                sourceColumn.Visible = true;
                Columns.Add(sourceColumn);
            }

            // Target Data Object
            if (!Controls.ContainsKey(DataItemMappingGridColumns.TargetDataObject.ToString()))
            {
                DataGridViewTextBoxColumn targetTable = new DataGridViewTextBoxColumn();
                targetTable.Name = DataItemMappingGridColumns.TargetDataObject.ToString();
                targetTable.HeaderText = @"Target Data Object";
                targetTable.DataPropertyName = DataItemMappingGridColumns.TargetDataObject.ToString();
                targetTable.Visible = true;
                Columns.Add(targetTable);
            }

            // Target Column
            if (!Controls.ContainsKey(DataItemMappingGridColumns.TargetDataItem.ToString()))
            {
                DataGridViewTextBoxColumn targetColumn = new DataGridViewTextBoxColumn();
                targetColumn.Name = DataItemMappingGridColumns.TargetDataItem.ToString();
                targetColumn.HeaderText = DataItemMappingGridColumns.TargetDataItem.ToString();
                targetColumn.DataPropertyName = DataItemMappingGridColumns.TargetDataItem.ToString();
                targetColumn.Visible = true;
                Columns.Add(targetColumn);
            }

            // Notes
            if (!Controls.ContainsKey(DataItemMappingGridColumns.Notes.ToString()))
            {
                DataGridViewTextBoxColumn notes = new DataGridViewTextBoxColumn();
                notes.Name = DataItemMappingGridColumns.Notes.ToString();
                notes.HeaderText = @"Target Data Item";
                notes.DataPropertyName = DataItemMappingGridColumns.Notes.ToString();
                notes.Visible = false;
                Columns.Add(notes);
            }
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

            #region Single Row

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.SuspendLayout();

            // Delete row menu item
            var deleteRow = new ToolStripMenuItem();
            deleteRow.Name = "deleteThisRowFromTheGridToolStripMenuItem";
            deleteRow.Size = new Size(225, 22);
            deleteRow.Text = @"Delete this row from the grid";
            deleteRow.Click += DeleteRowFromGridToolStripMenuItem_Click;

            // Parse as DataObjectMappings JSON (collection) menu item
            var parseThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem = new ToolStripMenuItem();
            parseThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Name = "parseThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem";
            parseThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Size = new Size(339, 22);
            parseThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Text = @"Parse this row as Data Object Mapping Collection";
            parseThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Click += ParseThisRowAsJSONDataObjectMappingCollectionToolStripMenuItem_Click;

            // Show as DataObjectMappings JSON (collection) menu item
            var exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem = new ToolStripMenuItem();
            exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Name = "exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem";
            exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Size = new Size(339, 22);
            exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Text = @"Display this row as Data Object Mapping Collection";
            exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Click += DisplayThisRowAsJSONDataObjectMappingCollectionToolStripMenuItem_Click;

            // Show as single DataObjectMappings JSON menu item
            var exportThisRowAsSingleDataObjectMappingJsonToolStripMenuItem = new ToolStripMenuItem();
            exportThisRowAsSingleDataObjectMappingJsonToolStripMenuItem.Name = "exportThisRowAsSingleDataObjectMappingJsonToolStripMenuItem";
            exportThisRowAsSingleDataObjectMappingJsonToolStripMenuItem.Size = new Size(339, 22);
            exportThisRowAsSingleDataObjectMappingJsonToolStripMenuItem.Text = @"Display this row as single Data Object Mapping";
            exportThisRowAsSingleDataObjectMappingJsonToolStripMenuItem.Click += DisplayThisRowAsJSONSingleDataObjectMappingToolStripMenuItem_Click;


            contextMenuStrip.ImageScalingSize = new Size(24, 24);
            contextMenuStrip.Items.AddRange(new ToolStripItem[] {
                parseThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem,
                exportThisRowAsSingleDataObjectMappingJsonToolStripMenuItem,
                exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem,
                deleteRow
            });

            contextMenuStrip.Name = "contextMenuStripDataItemMapping";
            contextMenuStrip.Size = new Size(340, 48);

            contextMenuStrip.ResumeLayout(false);

            #endregion

            #endregion
        }

        /// <summary>
        /// This method is called from the context menu, and applies all TEAM conventions to the Data Mapping collection (list / DataObjectMappings).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ParseThisRowAsJSONDataObjectMappingCollectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the value from the data item grid.
            int selectedRow = Rows.GetFirstRow(DataGridViewElementStates.Selected);

            var generationMetadataRow = ((DataRowView)Rows[selectedRow].DataBoundItem).Row;
            var targetDataObjectName = generationMetadataRow[DataItemMappingGridColumns.TargetDataObject.ToString()];

            try
            {
                // Get the value from the data object grid, based on the name.
                var dataObjectGridLookupRow = _dataGridViewDataObjects.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .Where(r => r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(targetDataObjectName))
                    .FirstOrDefault();

                var targetDataObject = (DataWarehouseAutomation.DataObject)dataObjectGridLookupRow.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value;


                var dataObjectMappings = _dataGridViewDataObjects.GetDataObjectMappings(targetDataObject);
                var vdwDataObjectMappingList = FormManageMetadata.GetVdwDataObjectMappingList(targetDataObject, dataObjectMappings);

                string output = JsonConvert.SerializeObject(vdwDataObjectMappingList, Formatting.Indented);
                File.WriteAllText(globalParameters.GetMetadataFilePath(targetDataObject.name), output);

                // Update the original form through the delegate/event handler.
                DataObjectsParse($"A parse action has been called from the context menu. The Data Object Mapping for '{targetDataObject.name}' has been saved.\r\n");
            }

            catch (Exception exception)
            {
                DataObjectsParse($"An error occurred while generating the JSON. The error message is {exception.Message}\r\n");
            }
        }

        /// <summary>
        /// This method is called from the context menu on the data grid. It exports the selected row to JSON.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DisplayThisRowAsJSONDataObjectMappingCollectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the selected row from the data item grid.
            int selectedRow = Rows.GetFirstRow(DataGridViewElementStates.Selected);

            var generationMetadataRow = ((DataRowView)Rows[selectedRow].DataBoundItem).Row;
            var targetDataObjectName = generationMetadataRow[DataItemMappingGridColumns.TargetDataObject.ToString()];

            try
            {
                // Get the value from the data object grid, based on the name.
                var dataObjectGridLookupRow = _dataGridViewDataObjects.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .Where(r => r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(targetDataObjectName))
                    .FirstOrDefault();

                var targetDataObject = (DataWarehouseAutomation.DataObject)dataObjectGridLookupRow.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value;

                var dataObjectMappings = _dataGridViewDataObjects.GetDataObjectMappings(targetDataObject);

                string output = JsonConvert.SerializeObject(dataObjectMappings, Formatting.Indented);

                FormManageMetadata.ManageFormJsonInteraction(output);
            }
            catch (Exception exception)
            {
                DataObjectsParse($"An error occurred while generating the JSON. The error message is {exception.Message}\r\n");
            }
        }

        /// <summary>
        /// This method is called from the context menu on the data grid. It exports the selected row to JSON.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DisplayThisRowAsJSONSingleDataObjectMappingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the selected row from the data item grid.
            int selectedRow = Rows.GetFirstRow(DataGridViewElementStates.Selected);

            var generationMetadataRow = ((DataRowView)Rows[selectedRow].DataBoundItem).Row;
            var targetDataObjectName = generationMetadataRow[DataItemMappingGridColumns.TargetDataObject.ToString()];

            try
            {
                // Get the value from the data object grid, based on the name.
                var dataObjectGridLookupRow = _dataGridViewDataObjects.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .Where(r => r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(targetDataObjectName))
                    .FirstOrDefault();

                var dataObjectMapping = _dataGridViewDataObjects.GetDataObjectMapping(dataObjectGridLookupRow);

                string output = JsonConvert.SerializeObject(dataObjectMapping, Formatting.Indented);

                FormManageMetadata.ManageFormJsonInteraction(output);
            }
            catch (Exception exception)
            {
                DataObjectsParse($"An error occurred while generating the JSON. The error message is {exception.Message}\r\n");
            }
        }

        internal void DataObjectsParse(string text)
        {
            // Make sure something is listening to the event.
            if (OnDataObjectParse == null) return;

            // Pass through the custom arguments when this method is called.
            ParseEventArgs args = new ParseEventArgs(text);
            OnDataObjectParse(this, args);
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
        /// Work-around for known bug related to data grid view losing colour coding after sorting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewDataItems_Sorted(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in Rows)
            {
                if (!row.IsNewRow)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (Columns[cell.ColumnIndex].Name.Equals(DataItemMappingGridColumns.SourceDataItem.ToString()))
                        {
                            PaintDataItemCell(cell);
                        }
                    }
                }
            }

            // Callback to parent form.
            HeaderSort();
        }

        /// <summary>
        /// Cell formatting / colour coding for data item cells.
        /// </summary>
        /// <param name="cell"></param>
        private static void PaintDataItemCell(DataGridViewCell cell)
        {
            string dataItemName = cell.Value.ToString();

            if (dataItemName.StartsWith("`"))
            {
                cell.Style.BackColor = Color.AliceBlue;

                if (dataItemName.EndsWith("`"))
                {
                    cell.Style.ForeColor = Color.DarkBlue;
                }
                else
                {
                    // Show issue.
                    cell.Style.ForeColor = Color.Red;
                }
            }
            else
            {
                cell.Style.BackColor = Color.White;
                cell.Style.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// Manages the colour coding / formatting for the data item grid view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewDataItems_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == -1)
                return;

            // Retrieve the full row for the selected cell.
            DataGridViewRow selectedRow = Rows[e.RowIndex];
            DataGridViewColumn selectedColumn = Columns[e.ColumnIndex];

            if (selectedColumn.Index == (int)DataItemMappingGridColumns.HashKey || selectedColumn.Index == (int)DataItemMappingGridColumns.Notes)
                return;

            #region Source Data Objects

            // Format the name of the data object, for a source data object
            if (Columns[e.ColumnIndex].Name.Equals(DataItemMappingGridColumns.SourceDataObject.ToString()))
            {
                if (e.Value != null)
                {
                    DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];

                    string dataObjectName = e.Value.ToString();

                    // Colour coding / syntax highlighting for in source data objects.
                    if (dataObjectName.StartsWith("`"))
                    {
                        cell.Style.BackColor = Color.AliceBlue;

                        if (dataObjectName.EndsWith("`"))
                        {
                            cell.Style.ForeColor = Color.DarkBlue;
                        }
                        else
                        {
                            // Show issue.
                            cell.Style.ForeColor = Color.Red;
                        }
                    }
                }
            }

            #endregion

            #region Source Data Items

            // Format the name of the data object, for a source data object
            if (Columns[e.ColumnIndex].Name.Equals(DataItemMappingGridColumns.SourceDataItem.ToString()))
            {
                if (e.Value != null)
                {
                    DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                    PaintDataItemCell(cell);
                }
            }

            #endregion
            
            #region Target Data Items

            // Format the name of the data object, for a source data object
            if (Columns[e.ColumnIndex].Name.Equals(DataItemMappingGridColumns.TargetDataItem.ToString()))
            {
                if (e.Value != null)
                {
                    DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];

                    string dataObjectName = e.Value.ToString();

                    // Colour coding / syntax highlighting for in source data objects.
                    if (dataObjectName.StartsWith("`") || dataObjectName.EndsWith("`"))
                    {
                        // Show issue - you can't have a derived target data item.
                        cell.Style.ForeColor = Color.Red;
                    }
                    // Colour coding / syntax highlighting for in source data objects.
                    else
                    {
                        cell.Style.ForeColor = Color.Black;
                    }
                }
            }

            #endregion

            #region Target Data Object
            // Format the name of the data object, for a target data object
            if (Columns[e.ColumnIndex].Name.Equals(DataItemMappingGridColumns.TargetDataObject.ToString()))
            {
                if (e.Value != null)
                {
                    // Current cell
                    DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];

                    // Check what the 'parent' is in the data object mapping, for further evaluation. Return if nothing is found (this should be picked up by validation later on).
                    try
                    {
                        string targetdataObjectName = e.Value.ToString();
                        string sourceDataObjectName = "";

                        try
                        {
                            sourceDataObjectName = Rows[e.RowIndex].Cells[(int)DataItemMappingGridColumns.SourceDataObject].Value.ToString();
                        }
                        catch
                        {
                            sourceDataObjectName = "";
                        }

                        var dataObjectGridViewRow = _dataGridViewDataObjects.Rows
                            .Cast<DataGridViewRow>()
                            .Where(r => !r.IsNewRow)
                            .Where(r => r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(targetdataObjectName))
                            .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName))
                            .FirstOrDefault();

                        if (dataObjectGridViewRow != null)
                        {
                            var targetConnectionId = dataObjectGridViewRow.Cells[(int)DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                            TeamConnection targetConnection = TeamConnection.GetTeamConnectionByConnectionId(targetConnectionId, TeamConfiguration, TeamEventLog);

                            KeyValuePair<string, string> targetDataObjectFullyQualifiedKeyValuePair =
                                MetadataHandling.GetFullyQualifiedDataObjectName(targetdataObjectName, targetConnection).FirstOrDefault();

                            // Only the name (e.g. without the schema) should be evaluated.
                            string targetDataObjectNonQualifiedName = targetDataObjectFullyQualifiedKeyValuePair.Value;

                            if (targetDataObjectNonQualifiedName != null && selectedRow.IsNewRow == false)
                            {
                                var presentationLayerLabelArray = Utility.SplitLabelIntoArray(TeamConfiguration.PresentationLayerLabels);
                                var transformationLabelArray = Utility.SplitLabelIntoArray(TeamConfiguration.TransformationLabels);

                                // Hub
                                if (targetDataObjectNonQualifiedName.IsDataVaultHub(TeamConfiguration))
                                {
                                    cell.Style.BackColor = Color.CornflowerBlue;
                                }
                                // Link-Sat
                                else if (targetDataObjectNonQualifiedName.IsDataVaultLinkSatellite(TeamConfiguration))
                                {
                                    cell.Style.BackColor = Color.Gold;
                                }
                                // Context
                                else if (targetDataObjectNonQualifiedName.IsDataVaultSatellite(TeamConfiguration))
                                {
                                    cell.Style.BackColor = Color.Yellow;
                                }
                                // Natural Business Relationship
                                else if (targetDataObjectNonQualifiedName.IsDataVaultLink(TeamConfiguration))
                                {
                                    cell.Style.BackColor = Color.OrangeRed;
                                }
                                // PSA
                                else if (targetDataObjectNonQualifiedName.IsPsa(TeamConfiguration))
                                {
                                    cell.Style.BackColor = Color.AntiqueWhite;
                                }
                                // Staging
                                else if ((TeamConfiguration.TableNamingLocation == "Prefix" && targetDataObjectNonQualifiedName.StartsWith(TeamConfiguration.StgTablePrefixValue)) ||
                                         (TeamConfiguration.TableNamingLocation == "Suffix" && targetDataObjectNonQualifiedName.EndsWith(TeamConfiguration.StgTablePrefixValue)))
                                {
                                    cell.Style.BackColor = Color.WhiteSmoke;
                                }
                                // Presentation Layer
                                else if ((TeamConfiguration.TableNamingLocation == "Prefix" && presentationLayerLabelArray.Any(s => targetDataObjectNonQualifiedName.StartsWith(s))) ||
                                         (TeamConfiguration.TableNamingLocation == "Suffix" && presentationLayerLabelArray.Any(s => targetDataObjectNonQualifiedName.EndsWith(s))))
                                {
                                    cell.Style.BackColor = Color.Aquamarine;
                                }
                                // Derived objects / transformations
                                else if ((TeamConfiguration.TableNamingLocation == "Prefix" && transformationLabelArray.Any(s => targetDataObjectNonQualifiedName.StartsWith(s))) ||
                                         (TeamConfiguration.TableNamingLocation == "Suffix" && transformationLabelArray.Any(s => targetDataObjectNonQualifiedName.EndsWith(s))))
                                {
                                    cell.Style.BackColor = Color.LightGreen;
                                }
                                else
                                {
                                    // Catch
                                }
                            }
                        }
                    }
                    catch
                    {
                        // TBD.
                    }
                }
            }

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
                {
                    return;
                }

                // Select the full row when the default column is right-clicked.
                if (SelectedRows.Count <= 1)
                {

                    ClearSelection();

                    Rows[hitTestInfo.RowIndex].Selected = true;
                    ContextMenuStrip = contextMenuStrip;
                }
                else
                {
                    ContextMenuStrip = contextMenuStripMultipleRows;
                }
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
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, exceptionMessage ));
            }
        }

        internal void HeaderSort()
        {
            // Make sure something is listening to the event.
            if (OnHeaderSort == null) return;

            // Pass through the custom arguments when this method is called.
            FilterEventArgs args = new FilterEventArgs(true);
            OnHeaderSort(this, args);
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

                if (e.KeyCode == Keys.End)
                {
                    BeginEdit(true);

                    // Move to the end of the cell value
                    var selectedCell = CurrentCell;
                    ((TextBox)EditingControl).SelectionStart = CurrentCell.Value.ToString().Length;
                }
            }
            catch (Exception exception)
            {
                var exceptionMessage = $"Copy/paste has failed.";
                var targetControl = Parent.Controls.Find("richTextBoxInformation", true).FirstOrDefault() as RichTextBox;

                ThreadHelper.SetText(Parent, targetControl, $"{exceptionMessage}\r\n");
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, exceptionMessage));
            }
        }
    }
}
