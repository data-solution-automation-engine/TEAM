using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DataWarehouseAutomation;
using TEAM_Library;
using static TEAM.FormBase;
using DataObject = DataWarehouseAutomation.DataObject;
using Event = TEAM_Library.Event;
using static TEAM_Library.MetadataHandling;
using ComboBox = System.Windows.Forms.ComboBox;

namespace TEAM
{
    internal sealed class DataGridViewDataObjects : DataGridView
    {
        private TeamConfiguration TeamConfiguration { get; }
        private JsonExportSetting JsonExportSetting { get; }

        private Form_Edit _modifyJson;

        private readonly ContextMenuStrip contextMenuStripFullRow;
        private readonly ContextMenuStrip contextMenuStripMultipleRows;
        private readonly ContextMenuStrip contextMenuStripSingleCell;

        public delegate void DataObjectParseHandler(object sender, ParseEventArgs e);
        public event DataObjectParseHandler OnDataObjectParse;

        /// <summary>
        /// The definition of the Data Grid View for table mappings (DataObject mappings).
        /// </summary>
        public DataGridViewDataObjects(TeamConfiguration teamConfiguration, JsonExportSetting jsonExportSetting)
        {
            TeamConfiguration = teamConfiguration;
            JsonExportSetting = jsonExportSetting;

            #region Basic properties

            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BorderStyle = BorderStyle.None;

            // Disable resizing for performance, will be enabled after binding.
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            EditMode = DataGridViewEditMode.EditOnEnter;
            
            var mySize = new Size(1100, 540);
            MinimumSize = mySize;
            Size = mySize;

            AutoGenerateColumns = false;
            ColumnHeadersVisible = true;

            Name = "dataGridViewTableMetadata";
            Location = new Point(2, 3);
            TabIndex = 1;

            #endregion

            #region Event handlers

            CellValidating += DataGridViewDataObjects_CellValidating;
            CellFormatting += DataGridViewDataObjects_CellFormatting;
            CellParsing += DataGridViewDataObjects_CellParsing;
            EditingControlShowing += DataGridViewDataObjects_EditingControlShowing;
            KeyDown += DataGridViewKeyDown;
            MouseDown += DataGridViewDataObjects_MouseDown;
            ColumnHeaderMouseClick += DataGridViewDataObjects_ColumnHeaderMouseClick;
            CellEnter += DataGridViewDataObjects_CellEnter; // Open Combo Boxes on first click
            DefaultValuesNeeded += DataGridViewDataObjectMapping_DefaultValuesNeeded;
            Sorted += TextBoxFilterCriterion_OnDelayedTextChanged;
            CellValueChanged += OnCheckBoxValueChanged;
            RowPostPaint += OnRowPostPaint;

            #endregion

            #region Columns

            // Enabled
            if (!Controls.ContainsKey(DataObjectMappingGridColumns.Enabled.ToString()))
            {
                DataGridViewCheckBoxColumn enabledIndicator = new DataGridViewCheckBoxColumn();
                enabledIndicator.Name = DataObjectMappingGridColumns.Enabled.ToString();
                enabledIndicator.HeaderText = DataObjectMappingGridColumns.Enabled.ToString();
                enabledIndicator.DataPropertyName = DataObjectMappingGridColumns.Enabled.ToString();
                Columns.Add(enabledIndicator);
            }

            // Hashkey
            if (!Controls.ContainsKey(DataObjectMappingGridColumns.HashKey.ToString()))
            {
                DataGridViewTextBoxColumn hashKey = new DataGridViewTextBoxColumn();
                hashKey.Name = DataObjectMappingGridColumns.HashKey.ToString();
                hashKey.HeaderText = DataObjectMappingGridColumns.HashKey.ToString();
                hashKey.DataPropertyName = DataObjectMappingGridColumns.HashKey.ToString();
                hashKey.Visible = false;
                Columns.Add(hashKey);
            }

            // Source Connection.
            DataGridViewComboBoxColumn sourceConnection = new DataGridViewComboBoxColumn();
            sourceConnection.Name = DataObjectMappingGridColumns.SourceConnection.ToString();
            sourceConnection.HeaderText = @"Source Connection";
            sourceConnection.DataPropertyName = DataObjectMappingGridColumns.SourceConnection.ToString();
            sourceConnection.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            sourceConnection.DataSource = LocalTeamConnection.GetConnections(FormBase.TeamConfiguration.ConnectionDictionary);
            sourceConnection.DisplayMember = "ConnectionKey";
            sourceConnection.ValueMember = "ConnectionId";
            Columns.Add(sourceConnection);
            
            // Source Data Object.
            DataGridViewTextBoxColumn sourceDataObject = new DataGridViewTextBoxColumn();
            sourceDataObject.Name = DataObjectMappingGridColumns.SourceDataObject.ToString();
            sourceDataObject.DataPropertyName = DataObjectMappingGridColumns.SourceDataObject.ToString();
            sourceDataObject.HeaderText = @"Source Data Object";
            sourceDataObject.SortMode = DataGridViewColumnSortMode.Programmatic;
            Columns.Add(sourceDataObject);

            // Target Data Object Connection.
            DataGridViewComboBoxColumn targetConnection = new DataGridViewComboBoxColumn();
            targetConnection.Name = DataObjectMappingGridColumns.TargetConnection.ToString();
            targetConnection.HeaderText = @"Target Connection";
            targetConnection.DataPropertyName = DataObjectMappingGridColumns.TargetConnection.ToString();
            targetConnection.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            targetConnection.DataSource = LocalTeamConnection.GetConnections(FormBase.TeamConfiguration.ConnectionDictionary);
            targetConnection.DisplayMember = "ConnectionKey";
            targetConnection.ValueMember = "ConnectionId";
            Columns.Add(targetConnection);

            // Target Data Object.
            DataGridViewTextBoxColumn targetDataObject = new DataGridViewTextBoxColumn();
            targetDataObject.Name = DataObjectMappingGridColumns.TargetDataObject.ToString();
            targetDataObject.DataPropertyName = DataObjectMappingGridColumns.TargetDataObject.ToString();
            targetDataObject.HeaderText = @"Target Data Object";
            targetDataObject.SortMode = DataGridViewColumnSortMode.Programmatic;
            Columns.Add(targetDataObject);

            DataGridViewTextBoxColumn businessKeyDefinition = new DataGridViewTextBoxColumn();
            businessKeyDefinition.Name = DataObjectMappingGridColumns.BusinessKeyDefinition.ToString();
            businessKeyDefinition.HeaderText = @"Business Key Definition";
            businessKeyDefinition.DataPropertyName = DataObjectMappingGridColumns.BusinessKeyDefinition.ToString();
            Columns.Add(businessKeyDefinition);

            DataGridViewTextBoxColumn drivingKeyDefinition = new DataGridViewTextBoxColumn();
            drivingKeyDefinition.Name = DataObjectMappingGridColumns.DrivingKeyDefinition.ToString();
            drivingKeyDefinition.HeaderText = @"Driving Key Definition";
            drivingKeyDefinition.DataPropertyName = DataObjectMappingGridColumns.DrivingKeyDefinition.ToString();
            Columns.Add(drivingKeyDefinition);

            DataGridViewTextBoxColumn filterCriterion = new DataGridViewTextBoxColumn();
            filterCriterion.Name = DataObjectMappingGridColumns.FilterCriterion.ToString();
            filterCriterion.HeaderText = @"Filter Criterion";
            filterCriterion.DataPropertyName = DataObjectMappingGridColumns.FilterCriterion.ToString();
            filterCriterion.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            filterCriterion.MinimumWidth = 100;
            Columns.Add(filterCriterion);

            // Filtering and back-end management only.
            DataGridViewTextBoxColumn sourceDataObjectName = new DataGridViewTextBoxColumn();
            sourceDataObjectName.Name = DataObjectMappingGridColumns.SourceDataObjectName.ToString();
            sourceDataObjectName.HeaderText = @"Source Data Object Name";
            sourceDataObjectName.DataPropertyName = DataObjectMappingGridColumns.SourceDataObjectName.ToString();
            sourceDataObjectName.Visible = false;
            Columns.Add(sourceDataObjectName);

            DataGridViewTextBoxColumn targetDataObjectName = new DataGridViewTextBoxColumn();
            targetDataObjectName.Name = DataObjectMappingGridColumns.TargetDataObjectName.ToString();
            targetDataObjectName.HeaderText = @"Target Data Object Name";
            targetDataObjectName.DataPropertyName = DataObjectMappingGridColumns.TargetDataObjectName.ToString();
            targetDataObjectName.Visible = false;
            Columns.Add(targetDataObjectName);

            DataGridViewTextBoxColumn previousTargetDataObjectName = new DataGridViewTextBoxColumn();
            previousTargetDataObjectName.Name = DataObjectMappingGridColumns.PreviousTargetDataObjectName.ToString();
            previousTargetDataObjectName.HeaderText = @"Target Data Object Name";
            previousTargetDataObjectName.DataPropertyName = DataObjectMappingGridColumns.PreviousTargetDataObjectName.ToString();
            previousTargetDataObjectName.Visible = false;
            Columns.Add(previousTargetDataObjectName);

            #endregion

            #region Context menu

            #region Full row context menu

            // Full row context menu
            contextMenuStripFullRow = new ContextMenuStrip();
            contextMenuStripFullRow.SuspendLayout();

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

            // Delete row menu item
            var deleteThisRowFromTheGridToolStripMenuItem = new ToolStripMenuItem();
            deleteThisRowFromTheGridToolStripMenuItem.Name = "deleteThisRowFromTheGridToolStripMenuItem";
            deleteThisRowFromTheGridToolStripMenuItem.Size = new Size(225, 22);
            deleteThisRowFromTheGridToolStripMenuItem.Text = @"Delete this row from the grid";
            deleteThisRowFromTheGridToolStripMenuItem.Click += DeleteThisRowFromTableDataGridToolStripMenuItem_Click;

            contextMenuStripFullRow.ImageScalingSize = new Size(24, 24);
            contextMenuStripFullRow.Items.AddRange(new ToolStripItem[] {
                parseThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem,
                exportThisRowAsSingleDataObjectMappingJsonToolStripMenuItem,
                exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem,
                deleteThisRowFromTheGridToolStripMenuItem
             });

            contextMenuStripFullRow.Name = "contextMenuStripTableMapping";
            contextMenuStripFullRow.Size = new Size(340, 48);
            contextMenuStripFullRow.ResumeLayout(false);

            #endregion

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
            contextMenuStripMultipleRows.Name = "contextMenuStripDataObjectMappingMultipleRows";
            contextMenuStripMultipleRows.Size = new Size(144, 26);
            contextMenuStripMultipleRows.ResumeLayout(false);

            #endregion

            #region Single cell context menu

            // Single cell context menu
            contextMenuStripSingleCell = new ContextMenuStrip();
            contextMenuStripSingleCell.SuspendLayout();

            // Modify JSON menu item
            var toolStripMenuItemModifyJson = new ToolStripMenuItem();
            toolStripMenuItemModifyJson.Name = "toolStripMenuItemModifyJson";
            toolStripMenuItemModifyJson.Size = new Size(143, 22);
            toolStripMenuItemModifyJson.Text = @"Modify JSON";
            toolStripMenuItemModifyJson.Click += toolStripMenuItemModifyJson_Click;

            contextMenuStripSingleCell.Items.AddRange(new ToolStripItem[] {
                toolStripMenuItemModifyJson
            });
            contextMenuStripSingleCell.Name = "contextMenuStripDataObjectMappingSingleCell";
            contextMenuStripSingleCell.Size = new Size(144, 26);
            contextMenuStripSingleCell.ResumeLayout(false);

            #endregion

            #endregion
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
            if (grid != null && grid.RowHeadersWidth < textSize.Width + 40)
            {
                grid.RowHeadersWidth = textSize.Width + 40;
            }

            if (grid != null)
            {
                var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
                e.Graphics.DrawString(rowIndex, Font, SystemBrushes.ControlText, headerBounds, centerFormat);
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

        /// <summary>
        /// This method is called from the context menu on the data grid. It deletes the row from the grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteThisRowFromTableDataGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in SelectedRows)
            {
                if (!row.IsNewRow)
                {
                    Rows.RemoveAt(row.Index);
                }
            }
        }

        public class ParseEventArgs : EventArgs
        {
            public string Text { get; private set; }

            public ParseEventArgs(string status)
            {
                Text = status;
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

        private void OnCheckBoxValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var selectedColumn = Columns[e.ColumnIndex];
            var selectedRow = Rows[e.RowIndex];

            if (selectedRow.IsNewRow)
                return;

            if (selectedColumn.Index.Equals((int)DataObjectMappingGridColumns.SourceConnection))
            {
                // The connection.
                var sourceConnectionInternalId = selectedRow.Cells[DataObjectMappingGridColumns.SourceConnection.ToString()].Value.ToString();
                var sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(sourceConnectionInternalId, TeamConfiguration, TeamEventLog);

                // The data object, to be updated
                var dataObject = new DataObject();

                if (selectedRow.Cells[DataObjectMappingGridColumns.SourceDataObject.ToString()].Value != DBNull.Value)
                {
                    dataObject = (DataObject)selectedRow.Cells[DataObjectMappingGridColumns.SourceDataObject.ToString()].Value;
                }

                JsonOutputHandling.SetDataObjectConnection(dataObject, sourceConnection, JsonExportSetting);
            }
            else if (selectedColumn.Index.Equals((int)DataObjectMappingGridColumns.TargetConnection))
            {
                // The connection.
                var targetConnectionInternalId = selectedRow.Cells[DataObjectMappingGridColumns.TargetConnection.ToString()].Value.ToString();
                var targetConnection = TeamConnection.GetTeamConnectionByConnectionId(targetConnectionInternalId, TeamConfiguration, TeamEventLog);

                // The data object, to be updated
                var dataObject = new DataObject();

                if (selectedRow.Cells[DataObjectMappingGridColumns.TargetDataObject.ToString()].Value != DBNull.Value)
                {
                    dataObject = (DataObject)selectedRow.Cells[DataObjectMappingGridColumns.TargetDataObject.ToString()].Value;
                }

                JsonOutputHandling.SetDataObjectConnection(dataObject, targetConnection, JsonExportSetting);
            }
        }

        /// <summary>
        /// Default row values for Data Object Mapping data grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewDataObjectMapping_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            DataObject sourceDataObject = new DataObject
            {
                name = "MyNewSourceDataObject"
            };

            DataObject targetDataObject = new DataObject
            {
                name = "MyNewTargetDataObject"
            };

            e.Row.Cells[DataObjectMappingGridColumns.Enabled.ToString()].Value = true;
            e.Row.Cells[DataObjectMappingGridColumns.SourceConnection.ToString()].Value = TeamConfiguration.MetadataConnection.ConnectionInternalId;
            e.Row.Cells[DataObjectMappingGridColumns.TargetConnection.ToString()].Value = TeamConfiguration.MetadataConnection.ConnectionInternalId;
            e.Row.Cells[DataObjectMappingGridColumns.SourceDataObject.ToString()].Value = sourceDataObject;
            e.Row.Cells[DataObjectMappingGridColumns.TargetDataObject.ToString()].Value = targetDataObject;
            e.Row.Cells[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].Value = "<business key definition>";
            e.Row.Cells[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].Value = sourceDataObject.name;
            e.Row.Cells[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].Value = targetDataObject.name;
        }

        /// <summary>
        /// Override to open up connection combo boxes on first click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewDataObjects_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            var selectedColumn = Columns[e.ColumnIndex];

            // Format the name of the data object, for a data object.
            if (selectedColumn.Index.Equals((int)DataObjectMappingGridColumns.SourceConnection) || selectedColumn.Index.Equals((int)DataObjectMappingGridColumns.TargetConnection))
            {
                BeginEdit(true);
                ((ComboBox)EditingControl).DroppedDown = true;
            }
        }

        /// <summary>
        /// Managed custom sorting on DataObject class columns.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewDataObjects_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Sorting on source data object.
            if (Columns[e.ColumnIndex].Name.Equals(DataObjectMappingGridColumns.SourceDataObject.ToString()))
            {
                if (SortOrder == SortOrder.Ascending)
                {
                    Sort(Columns[(int)DataObjectMappingGridColumns.SourceDataObjectName], ListSortDirection.Descending);
                }
                else
                {
                    Sort(Columns[(int)DataObjectMappingGridColumns.SourceDataObjectName], ListSortDirection.Ascending);
                }
            }

            // Sorting on target data object.
            if (Columns[e.ColumnIndex].Name.Equals(DataObjectMappingGridColumns.TargetDataObject.ToString()))
            {
                if (SortOrder == SortOrder.Ascending)
                {
                    Sort(Columns[(int)DataObjectMappingGridColumns.TargetDataObjectName], ListSortDirection.Descending);
                }
                else
                {
                    Sort(Columns[(int)DataObjectMappingGridColumns.TargetDataObjectName], ListSortDirection.Ascending);
                }
            }
        }
        
        private void toolStripMenuItemModifyJson_Click(object sender, EventArgs e)
        {
            _modifyJson = new Form_Edit(CurrentCell);
            _modifyJson.SetFormName("Modify JSON");
            _modifyJson.Show();
            _modifyJson.OnSave += CommitJsonChanges;
        }

        /// <summary>
        /// Get the value changes / content from the Edit form, and commit back into the data object mapping grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommitJsonChanges(object sender, Form_Edit.OnSaveEventArgs e)
        {
            DataObject dataObject = JsonConvert.DeserializeObject<DataObject>(e.RichTextBoxContents);
            e.CurrentCell.Value = dataObject;

            // Also update the hidden name columns for sorting, filtering and validation.
            if (e.CurrentCell.ColumnIndex == (int)DataObjectMappingGridColumns.SourceDataObject && dataObject != null)
            {
                DataGridViewCell updateCell = this[(int)DataObjectMappingGridColumns.SourceDataObjectName, CurrentCell.RowIndex];
                updateCell.Value = dataObject.name;
            }

            if (e.CurrentCell.ColumnIndex == (int)DataObjectMappingGridColumns.TargetDataObject && dataObject != null)
            {
                DataGridViewCell updateCell = this[(int)DataObjectMappingGridColumns.TargetDataObjectName, CurrentCell.RowIndex];
                updateCell.Value = dataObject.name;
            }

            // Hack to quickly unselect and re-select the cell to apply parsing and formatting.
            DataGridViewCell cell = CurrentCell;
            DataGridViewCell dummyCell = this[CurrentCell.ColumnIndex, CurrentCell.RowIndex + 1];
            CurrentCell = dummyCell;
            CurrentCell = cell;
        }

        private void TextBoxFilterCriterion_OnDelayedTextChanged(object sender, EventArgs e)
        {
            ApplyDataGridViewFiltering();
        }

        public void ApplyDataGridViewFiltering()
        {
            foreach (DataGridViewRow dr in Rows)
            {
                dr.Visible = true;
            }

            foreach (DataGridViewRow dr in Rows)
            {
                if (dr.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value != null)
                {
                    if (!dr.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString()
                            .Contains(Text) && !dr
                            .Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString()
                            .Contains(Text))
                    {
                        CurrencyManager currencyManager1 = (CurrencyManager)BindingContext[DataSource];
                        currencyManager1.SuspendBinding();
                        dr.Visible = false;
                        currencyManager1.ResumeBinding();
                    }
                }
            }
        }

        /// <summary>
        /// This method is called from the context menu, and applies all TEAM conventions to the Data Mapping collection (list / DataObjectMappings).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ParseThisRowAsJSONDataObjectMappingCollectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int selectedRow = Rows.GetFirstRow(DataGridViewElementStates.Selected);
            var generationMetadataRow = ((DataRowView)Rows[selectedRow].DataBoundItem).Row;
            var targetDataObject = (DataObject)generationMetadataRow[DataObjectMappingGridColumns.TargetDataObject.ToString()];

            var dataObjectMappings = _dataGridViewDataObjects.GetDataObjectMappings(targetDataObject);
            var vdwDataObjectMappingList = FormManageMetadata.GetVdwDataObjectMappingList(targetDataObject, dataObjectMappings);

            string output = JsonConvert.SerializeObject(vdwDataObjectMappingList, Formatting.Indented);
            File.WriteAllText(globalParameters.GetMetadataFilePath(targetDataObject.name), output);

            // Update the original form through the delegate/event handler.
            DataObjectsParse($"A parse action has been called from the context menu. The Data Object Mapping for '{targetDataObject.name}' has been saved.\r\n");
        }

        /// <summary>
        /// This method is called from the context menu on the data grid. It exports the selected row to JSON.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DisplayThisRowAsJSONDataObjectMappingCollectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int selectedRow = Rows.GetFirstRow(DataGridViewElementStates.Selected);

            var generationMetadataRow = ((DataRowView)Rows[selectedRow].DataBoundItem).Row;
            var targetDataObject = (DataObject)generationMetadataRow[DataObjectMappingGridColumns.TargetDataObject.ToString()];

            var dataObjectMappings = GetDataObjectMappings(targetDataObject);

            string output = JsonConvert.SerializeObject(dataObjectMappings, Formatting.Indented);

            FormManageMetadata.ManageFormJsonInteraction(output);
        }

        /// <summary>
        /// This method is called from the context menu on the data grid. It exports the selected row to JSON.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DisplayThisRowAsJSONSingleDataObjectMappingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var row = _dataGridViewDataObjects.SelectedRows[0];

            var dataObjectMapping = GetDataObjectMapping(row);

            string output = JsonConvert.SerializeObject(dataObjectMapping, Formatting.Indented);

            FormManageMetadata.ManageFormJsonInteraction(output);
        }
        
        private void DataGridViewDataObjects_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTestInfo = HitTest(e.X, e.Y);

                // For now, do nothing when any of the column headers are right-clicked.
                if (hitTestInfo.RowIndex == -1)
                    return;

                if (hitTestInfo.ColumnIndex == -1)
                {
                    // Select the full row when the default column is right-clicked.
                    if (SelectedRows.Count == 1)
                    {
                        ClearSelection();
                        Rows[hitTestInfo.RowIndex].Selected = true;
                        ContextMenuStrip = contextMenuStripFullRow;
                    }
                    else
                    {
                        ContextMenuStrip = contextMenuStripMultipleRows;
                    }
                }
                else
                {
                    ClearSelection();

                    // Evaluate which cell is clicked.
                    var cell = this[hitTestInfo.ColumnIndex, hitTestInfo.RowIndex];

                    if (hitTestInfo.ColumnIndex == (int)DataObjectMappingGridColumns.SourceDataObject || hitTestInfo.ColumnIndex == (int)DataObjectMappingGridColumns.TargetDataObject)
                    {
                        CurrentCell = cell;
                        ContextMenuStrip = contextMenuStripSingleCell;
                    }
                    else
                    {
                        Rows[hitTestInfo.RowIndex].Selected = true;
                        ContextMenuStrip = contextMenuStripFullRow;
                    }
                }
            }
        }

        private void PasteClipboardDataObjects()
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
            }
            catch (FormatException ex)
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"A cell formatting exception has been encountered: {ex.Message}."));
            }
        }

        private void DataGridViewKeyDown(object sender, KeyEventArgs e)
        {
            // Only works when not in edit mode.
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.V:
                            PasteClipboardDataObjects();
                            break;
                        case Keys.C:
                            if (sender.GetType() == typeof(DataGridViewComboBoxEditingControl))
                            {
                                var temp = (DataGridViewComboBoxEditingControl)sender;
                                Clipboard.SetText(temp.SelectedValue.ToString());
                            }

                            break;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(@"Pasting into the data grid has failed", @"Copy/Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DataGridViewDataObjects_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.CellStyle.BackColor = Color.Transparent;

            if (e.Control is DataGridViewComboBoxEditingControl tb)
            {
                tb.KeyDown -= DataGridViewKeyDown;
                tb.KeyDown += DataGridViewKeyDown;
            }
        }

        /// <summary>
        /// Validation event on Table Metadata data grid view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewDataObjects_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            var valueLength = e.FormattedValue.ToString().Length;

            DataGridViewCell targetDataObject = Rows[e.RowIndex].Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName];

            // Source Table (Source)
            if (e.ColumnIndex == (int)DataObjectMappingGridColumns.SourceDataObject)
            {
                Rows[e.RowIndex].ErrorText = "";

                if (e.FormattedValue == DBNull.Value || valueLength == 0)
                {
                    e.Cancel = true;
                    Rows[e.RowIndex].ErrorText = "The Source (Source) table cannot be empty!";
                }
            }

            // Target Table
            if (e.ColumnIndex == (int)DataObjectMappingGridColumns.TargetDataObject)
            {
                Rows[e.RowIndex].ErrorText = "";

                if (e.FormattedValue == DBNull.Value || valueLength == 0)
                {
                    e.Cancel = true;
                    Rows[e.RowIndex].ErrorText = "The Target (Integration Layer) table cannot be empty!";
                    CancelEdit();
                }
            }

            // Business Key
            if (e.ColumnIndex == (int)DataObjectMappingGridColumns.BusinessKeyDefinition && !targetDataObject.Value.ToString().IsDataVaultLinkSatellite(TeamConfiguration) && !targetDataObject.Value.ToString().IsDataVaultSatellite(TeamConfiguration))
            {
                Rows[e.RowIndex].ErrorText = "";

                if (e.FormattedValue == DBNull.Value || valueLength == 0)
                {
                    Rows[e.RowIndex].ErrorText = "The Business Key cannot be empty!";
                    CancelEdit();
                    e.Cancel = true;
                    EndEdit();
                }
            }

            // Filter criteria
            if (e.ColumnIndex == (int)DataObjectMappingGridColumns.FilterCriterion)
            {
                Rows[e.RowIndex].ErrorText = "";
                //int newInteger;
                var equalSignIndex = e.FormattedValue.ToString().IndexOf('=') + 1;

                if (valueLength > 0 && valueLength < 3)
                {
                    e.Cancel = true;
                    Rows[e.RowIndex].ErrorText = "The filter criterion cannot only be just one or two characters as it translates into a WHERE clause.";
                }

                if (valueLength > 0)
                {
                    //Check if an '=' is there
                    if (e.FormattedValue.ToString() == "=")
                    {
                        e.Cancel = true;
                        Rows[e.RowIndex].ErrorText = "The filter criterion cannot only be '=' as it translates into a WHERE clause.";
                    }

                    // If there are value in the filter, and the filter contains an equal sign but it's the last then cancel
                    if (valueLength > 2 && (e.FormattedValue.ToString().Contains("=") && !(equalSignIndex < valueLength)))
                    {
                        e.Cancel = true;
                        Rows[e.RowIndex].ErrorText = "The filter criterion include values either side of the '=' sign as it is expressed as a WHERE clause.";
                    }
                }
            }
        }

        /// <summary>
        /// Manages the colour coding and sets the ToolTip text for cells in the Data Object grid view (hover over).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewDataObjects_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == -1)
                return;

            // Retrieve the full row for the selected cell.
            DataGridViewRow selectedRow = Rows[e.RowIndex];
            DataGridViewColumn selectedColumn = Columns[e.ColumnIndex];

            if (selectedColumn.Index == (int)DataObjectMappingGridColumns.SourceConnection || 
                selectedColumn.Index == (int)DataObjectMappingGridColumns.TargetConnection ||
                selectedColumn.Index == (int)DataObjectMappingGridColumns.TargetDataObjectName ||
                selectedColumn.Index == (int)DataObjectMappingGridColumns.Enabled ||
                selectedColumn.Index == (int)DataObjectMappingGridColumns.PreviousTargetDataObjectName ||
                selectedColumn.Index == (int)DataObjectMappingGridColumns.FilterCriterion ||
                selectedColumn.Index == (int)DataObjectMappingGridColumns.SourceDataObjectName ||
                selectedColumn.Index == (int)DataObjectMappingGridColumns.SurrogateKey
                )
                return;

            #region Source Data Objects

            // Format the name of the data object, for a source data object
            if (Columns[e.ColumnIndex].Name.Equals(DataObjectMappingGridColumns.SourceDataObject.ToString()))
            {
                if (e.Value != null)
                {
                    DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];

                    // Set the tooltip.
                    try
                    {
                        DataObject fullDataObject = (DataObject)e.Value;
                        cell.ToolTipText = JsonConvert.SerializeObject(fullDataObject, Formatting.Indented);
                    }
                    catch (Exception ex)
                    {
                        cell.ToolTipText = $"The value could not be visualised in JSON. The error message is {ex.Message}.";
                    }

                    FormatDataObject(e);

                    string dataObjectName = e.Value.ToString();

                    // Colour coding
                    //Syntax highlighting for in source data objects.
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
                    else
                    {
                        cell.Style.ForeColor = Color.Black;
                        cell.Style.BackColor = Color.White;
                    }
                }
            }

            #endregion

            #region Target Data Object
            // Format the name of the data object, for a target data object
            if (Columns[e.ColumnIndex].Name.Equals(DataObjectMappingGridColumns.TargetDataObject.ToString()))
            {
                if (e.Value != null)
                {
                    // Current cell
                    DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];

                    // Set the tooltip.
                    try
                    {
                        DataObject fullDataObject = (DataObject)e.Value;
                        cell.ToolTipText = JsonConvert.SerializeObject(fullDataObject, Formatting.Indented);
                    }
                    catch (Exception ex)
                    {
                        cell.ToolTipText = $"The value could not be visualised in JSON. The error message is {ex.Message}.";
                    }

                    FormatDataObject(e);

                    string dataObjectName = e.Value.ToString();

                    var targetConnectionId = selectedRow.Cells[(int)DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                    TeamConnection targetConnection = TeamConnection.GetTeamConnectionByConnectionId(targetConnectionId, TeamConfiguration, TeamEventLog);
                    KeyValuePair<string, string> targetDataObjectFullyQualifiedKeyValuePair = MetadataHandling.GetFullyQualifiedDataObjectName(dataObjectName, targetConnection).FirstOrDefault();

                    // Only the name (e.g. without the schema) should be evaluated.
                    string targetDataObjectNonQualifiedName = targetDataObjectFullyQualifiedKeyValuePair.Value;

                    var businessKeySyntax = selectedRow.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value;

                    if (targetDataObjectNonQualifiedName != null && businessKeySyntax != null && selectedRow.IsNewRow == false)
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
                            cell.Style.BackColor = Color.LightCyan;
                        }
                    }
                }
            }

            #endregion
            
            #region Business Key Definition
            if (e.ColumnIndex != -1)
            {
                if (Columns[e.ColumnIndex].Name.Equals(DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()))
                {
                    if (e.Value != null)
                    {
                        DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];

                        if (cell.Value.ToString().Contains("CONCATENATE") || cell.Value.ToString().Contains("COMPOSITE"))
                        {
                            cell.Style.ForeColor = Color.DarkBlue;
                            cell.Style.BackColor = Color.AliceBlue;
                        }
                    }
                }
            }
            #endregion

            #region Driving Key Definition

            if (Columns[e.ColumnIndex].Name.Equals(DataObjectMappingGridColumns.DrivingKeyDefinition.ToString()) && !Rows[e.RowIndex].IsNewRow)
            {
                DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];

                string targetDataObjectName = "";

                if (Rows[e.RowIndex].Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value != DBNull.Value &&
                    Rows[e.RowIndex].Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value != null)
                {
                    targetDataObjectName = Rows[e.RowIndex].Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString();
                }

                if (targetDataObjectName.IsDataVaultLinkSatellite(TeamConfiguration))
                {
                    cell.ReadOnly = false;
                    cell.Style.SelectionForeColor = Color.Empty;
                    cell.Style.SelectionBackColor = Color.Empty;
                    cell.Style.BackColor = Color.Empty;
                }
                else
                {
                    cell.ReadOnly = true;
                    cell.Style.SelectionForeColor = Color.LightGray;
                    cell.Style.SelectionBackColor = Color.LightGray;
                    cell.Style.BackColor = Color.LightGray;
                }
            }
            
            #endregion
        }

        private void DataGridViewDataObjects_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            var selectedColumn = Columns[e.ColumnIndex];

            // Format the name of the data object, for a data object.
            if (selectedColumn.Index.Equals((int)DataObjectMappingGridColumns.SourceDataObject) || selectedColumn.Index.Equals((int)DataObjectMappingGridColumns.TargetDataObject))
            {
                try
                {
                    // Get the cell value and cast it as data object.
                    DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];

                    DataObject dataObject = new DataObject();

                    // If the underlying value exists it should be copied for the update, otherwise it's a new value.
                    if (cell.Value != DBNull.Value)
                    {
                        dataObject = (DataObject)cell.Value;
                    }

                    // Update the data object name.
                    dataObject.name = e.Value.ToString();

                    // Set the updated value.
                    e.Value = dataObject;

                    // Also update the hidden name column for sorting and validation purposes.
                    if (selectedColumn.Index.Equals((int)DataObjectMappingGridColumns.SourceDataObject))
                    {
                        Rows[e.RowIndex].Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value = dataObject.name;
                    }
                    else if (selectedColumn.Index.Equals((int)DataObjectMappingGridColumns.TargetDataObject))
                    {
                        // Update the hidden string target object name for filtering purposes.
                        Rows[e.RowIndex].Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value = dataObject.name;
                    }

                    e.ParsingApplied = true;
                }
                catch (FormatException)
                {
                    e.ParsingApplied = false;
                }
            }
        }

        /// <summary>
        /// Ensure only the name of the DataObject is shown in the grid view.
        /// </summary>
        /// <param name="formatting"></param>
        private static void FormatDataObject(DataGridViewCellFormattingEventArgs formatting)
        {
            if (formatting.Value != DBNull.Value)
            {
                try
                {
                    var dataObject = (DataObject)formatting.Value;

                    formatting.Value = dataObject.name;
                    formatting.FormattingApplied = true;
                }
                catch (FormatException)
                {
                    // Set to false in case there are other handlers interested trying to format this DataGridViewCellFormattingEventArgs instance.
                    formatting.FormattingApplied = false;
                }
            }
        }

        /// <summary>
        /// Return the collection of data object mappings relative to the selected (target) data object.
        /// </summary>
        /// <param name="targetDataObject"></param>
        /// <returns></returns>
        internal List<DataObjectMapping> GetDataObjectMappings(DataObject targetDataObject)
        {
            List<DataObjectMapping> dataObjectMappings = new List<DataObjectMapping>();

            foreach (DataGridViewRow row in Rows)
            {
                if (!row.IsNewRow)
                {
                    if (row.Cells[DataObjectMappingGridColumns.TargetDataObjectName.ToString()]?.Value.ToString() == targetDataObject.name)
                    {
                        var dataObjectMapping = GetDataObjectMapping(row);
                        dataObjectMappings.Add(dataObjectMapping);
                    }
                }
            }

            return dataObjectMappings;
        }

        /// <summary>
        /// Override to be able to accept a string value name for the data object.
        /// </summary>
        /// <param name="targetDataObjectName"></param>
        /// <returns></returns>
        internal List<DataObjectMapping> GetDataObjectMappings(string targetDataObjectName)
        {
            List<DataObjectMapping> dataObjectMappings = new List<DataObjectMapping>();

            foreach (DataGridViewRow row in Rows)
            {
                if (!row.IsNewRow)
                {
                    if (row.Cells[DataObjectMappingGridColumns.TargetDataObjectName.ToString()]?.Value.ToString() == targetDataObjectName)
                    {
                        var dataObjectMapping = GetDataObjectMapping(row);
                        dataObjectMappings.Add(dataObjectMapping);
                    }
                }
            }

            return dataObjectMappings;
        }

        /// <summary>
        /// Construct a Data Object Mapping from the available metadata.
        /// </summary>
        /// <param name="dataObjectMappingGridViewRow"></param>
        /// <returns></returns>
        private DataObjectMapping GetDataObjectMapping(DataGridViewRow dataObjectMappingGridViewRow)
        {
            var targetDataObjectName = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].Value.ToString();

            // Initial setting of the new object. Details will likely be overwritten by copying the full object.
            DataObjectMapping dataObjectMapping = new DataObjectMapping
            {
                mappingName = targetDataObjectName
            };

            #region Enabled

            var enabledIntermediate = "False";

            if (dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.Enabled.ToString()].Value != DBNull.Value)
            {
                enabledIntermediate = (string)dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.Enabled.ToString()].Value;
            }

            bool enabled = false;
            if (enabledIntermediate == "True")
            {
                enabled = true;
            }

            dataObjectMapping.enabled = enabled;

            #endregion

            #region Target Data Object

            string targetConnectionInternalId = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.TargetConnection.ToString()].Value.ToString();
            var targetConnection = TeamConnection.GetTeamConnectionByConnectionId(targetConnectionInternalId, TeamConfiguration, TeamEventLog);

            var targetDataObject = (DataObject)dataObjectMappingGridViewRow.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value;
            dataObjectMapping.targetDataObject = targetDataObject;

            // Grab the grids
            var dataGridViewRowsPhysicalModel = _dataGridViewPhysicalModel.Rows.Cast<DataGridViewRow>()
                        .Where(row => !row.IsNewRow)
                        .ToList();

            var dataGridViewRowsDataObjects = _dataGridViewDataObjects.Rows.Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .ToList();

            // Manage classifications
            JsonOutputHandling.SetDataObjectTypeClassification(targetDataObject, JsonExportSetting, TeamConfiguration);

            // Manage connections
            JsonOutputHandling.SetDataObjectConnection(targetDataObject, targetConnection, JsonExportSetting);

            // Manage connection extensions
            JsonOutputHandling.SetDataObjectConnectionDatabaseExtension(targetDataObject, targetConnection, JsonExportSetting);
            JsonOutputHandling.SetDataObjectConnectionSchemaExtension(targetDataObject, targetConnection, JsonExportSetting);

            // Data items
            JsonOutputHandling.SetDataObjectDataItems(targetDataObject, targetConnection, TeamConfiguration, JsonExportSetting, dataGridViewRowsPhysicalModel);

            #endregion

            #region Mapping Level Classification

            try
            {
                var drivingKeyValue = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.DrivingKeyDefinition.ToString()].Value.ToString();

                var mappingClassifications = JsonOutputHandling.SetMappingClassifications(targetDataObjectName, JsonExportSetting, TeamConfiguration, drivingKeyValue);
                dataObjectMapping.mappingClassifications = mappingClassifications;
            }
            catch
            {
                //
            }

            #endregion

            #region Source Data Objects

            try
            {
                string sourceDataObjectName = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].Value.ToString();

                List<dynamic> sourceDataObjects = new List<dynamic>();


                if (sourceDataObjectName.IsDataQuery())
                {
                    // Create a data query
                    DataQuery sourceDataQuery = new DataQuery();
                    sourceDataQuery.dataQueryCode = sourceDataObjectName.Replace("`", "");

                    // Manage connections
                    var sourceConnectionInternalId = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.SourceConnection.ToString()].Value.ToString();
                    var sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(sourceConnectionInternalId, TeamConfiguration, TeamEventLog);

                    JsonOutputHandling.SetDataQueryConnection(sourceDataQuery, sourceConnection, JsonExportSetting);

                    // Manage connection extensions
                    JsonOutputHandling.SetDataQueryConnectionDatabaseExtension(sourceDataQuery, sourceConnection, JsonExportSetting);
                    JsonOutputHandling.SetDataQueryConnectionSchemaExtension(sourceDataQuery, sourceConnection, JsonExportSetting);

                    sourceDataObjects.Add(sourceDataQuery);
                }
                else
                {
                    // Get the data item info.
                    dynamic sourceDataObject = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.SourceDataObject.ToString()].Value;

                    // Manage classifications
                    JsonOutputHandling.SetDataObjectTypeClassification(sourceDataObject, JsonExportSetting, TeamConfiguration);

                    // Data items
                    JsonOutputHandling.SetDataObjectDataItems(sourceDataObject, targetConnection, TeamConfiguration, JsonExportSetting, dataGridViewRowsPhysicalModel);

                    // Manage connections
                    var sourceConnectionInternalId = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.SourceConnection.ToString()].Value.ToString();
                    var sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(sourceConnectionInternalId, TeamConfiguration, TeamEventLog);

                    JsonOutputHandling.SetDataObjectConnection(sourceDataObject, sourceConnection, JsonExportSetting);

                    // Manage connection extensions
                    JsonOutputHandling.SetDataObjectConnectionDatabaseExtension(sourceDataObject, sourceConnection, JsonExportSetting);
                    JsonOutputHandling.SetDataObjectConnectionSchemaExtension(sourceDataObject, sourceConnection, JsonExportSetting);

                    sourceDataObjects.Add(sourceDataObject);
                }

                dataObjectMapping.sourceDataObjects = sourceDataObjects;
            }
            catch
            {
                //
            }

            #endregion

            #region Related Data Objects

            var relatedDataObjects = JsonOutputHandling.SetRelatedDataObjects(targetDataObjectName, this, JsonExportSetting, TeamConfiguration, TeamEventLog, dataGridViewRowsPhysicalModel);
            if (relatedDataObjects != null && relatedDataObjects.Count > 0)
            {
                dataObjectMapping.relatedDataObjects = relatedDataObjects;
            }

            #endregion

            #region Data Item Mappings

            try
            {
                // Add the data item mappings.
                List<DataItemMapping> dataItemMappings = new List<DataItemMapping>();
                List<string> targetDataItemNames = new List<string>();

                #region Manual map

                // Manually mapped data items (from the grid).
                foreach (DataGridViewRow dataItemMappingRow in _dataGridViewDataItems.Rows)
                {
                    if (!dataItemMappingRow.IsNewRow)
                    {
                        dynamic sourceDataObject = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.SourceDataObject.ToString()].Value;

                        var localSourceDataObject = dataItemMappingRow.Cells[DataItemMappingGridColumns.SourceDataObject.ToString()].Value.ToString();
                        var localTargetDataObject = dataItemMappingRow.Cells[DataItemMappingGridColumns.TargetDataObject.ToString()].Value.ToString();

                        if (localSourceDataObject == sourceDataObject.name && localTargetDataObject == targetDataObject.name)
                        {
                            var localSourceDataItem = dataItemMappingRow.Cells[DataItemMappingGridColumns.SourceDataItem.ToString()].Value.ToString();
                            var localTargetDataItem = dataItemMappingRow.Cells[DataItemMappingGridColumns.TargetDataItem.ToString()].Value.ToString();

                            // Creating a single source-to-target Data Item mapping.
                            List<dynamic> sourceDataItems = new List<dynamic>();

                            #region Target Data Item

                            var targetDataItem = new DataItem
                            {
                                name = localTargetDataItem
                            };

                            #region Multi-Active Key

                            // If the source data item is part of the key, a MAK classification can be added.
                            // This is done using a lookup against the physical model.

                            var physicalModelGridViewRow = _dataGridViewPhysicalModel.Rows
                                .Cast<DataGridViewRow>()
                                .Where(r => !r.IsNewRow)
                                .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString().Equals(localTargetDataItem))
                                .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.Table_Name].Value.ToString().Equals(localTargetDataObject))
                                .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.Primary_Key_Indicator].Value.ToString().Equals("Y"))
                                .FirstOrDefault();

                            if (physicalModelGridViewRow != null)
                            {
                                List<Classification> classificationList = new List<Classification>();
                                Classification classification = new Classification();
                                classification.classification = "MultiActiveKey";
                                classification.notes = "The attribute that supports granularity shift in describing context.";
                                classificationList.Add(classification);
                                targetDataItem.dataItemClassification = classificationList;
                            }


                            #endregion

                            // Add data types to Data Item that are part of a data item mapping.
                            var targetDataItemConnectionInternalId = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.TargetConnection.ToString()].Value.ToString();
                            var targetDataItemConnection = TeamConnection.GetTeamConnectionByConnectionId(targetDataItemConnectionInternalId, TeamConfiguration, TeamEventLog);
                            JsonOutputHandling.SetDataItemMappingDataType(targetDataItem, targetDataObject, targetDataItemConnection, JsonExportSetting, dataGridViewRowsPhysicalModel);

                            // Add parent Data Object to the Data Item.
                            JsonOutputHandling.SetParentDataObjectToDataItem(targetDataItem, dataObjectMapping.targetDataObject, JsonExportSetting);

                            #endregion

                            #region Source Data Item or Query

                            if (localSourceDataItem.IsDataQuery())
                            {
                                var sourceDataItem = new DataQuery();
                                sourceDataItem.dataQueryCode = localSourceDataItem.Replace("`", "");

                                sourceDataItems.Add(sourceDataItem);
                            }
                            else
                            {
                                var sourceDataItem = new DataItem();
                                sourceDataItem.name = localSourceDataItem;

                                // Add data types to Data Item that are part of a data item mapping.
                                var sourceDataItemConnectionInternalId = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.SourceConnection.ToString()].Value.ToString();
                                var sourceDataItemConnection = TeamConnection.GetTeamConnectionByConnectionId(sourceDataItemConnectionInternalId, TeamConfiguration, TeamEventLog);
                                JsonOutputHandling.SetDataItemMappingDataType(sourceDataItem, sourceDataObject, sourceDataItemConnection, JsonExportSetting, dataGridViewRowsPhysicalModel);

                                // Add parent Data Object to the Data Item.
                                JsonOutputHandling.SetParentDataObjectToDataItem(sourceDataItem, sourceDataObject, JsonExportSetting);

                                // Populate the list of source Data Items.
                                sourceDataItems.Add(sourceDataItem);
                            }

                            #endregion

                            // Create a Data Item Mapping.
                            DataItemMapping dataItemMapping = new DataItemMapping
                            {
                                sourceDataItems = sourceDataItems,
                                targetDataItem = targetDataItem
                            };

                            // Add to a list that is more easily searched.
                            targetDataItemNames.Add(targetDataItem.name);

                            // Add the Data Items Mapping to the list of mappings.
                            dataItemMappings.Add(dataItemMapping);
                        }
                    }
                }

                #endregion

                #region Auto map

                // For presentation layer, only manual mappings are supported.
                if (dataObjectMapping.mappingClassifications[0].classification != DataObjectTypes.Presentation.ToString())
                {

                    // Auto-map any data items that are not yet manually mapped, but exist in source and target.
                    var physicalModelDataGridViewRows = _dataGridViewPhysicalModel.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => !r.IsNewRow)
                        .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.Table_Name].Value.ToString().Equals(targetDataObject.name))
                        .ToList();

                    foreach (var row in physicalModelDataGridViewRows)
                    {
                        try
                        {
                            dynamic sourceDataObject = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.SourceDataObject.ToString()].Value;

                            var autoMappedTargetDataItemName = row.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString();

                            // If already exists as a target mapping it can be ignored.
                            if (targetDataItemNames.Contains(autoMappedTargetDataItemName))
                                continue;

                            // If there is no source data item to be found in the physical model, it can be ignored.
                            var physicalModelSourceDataItemLookup = _dataGridViewPhysicalModel.Rows
                                .Cast<DataGridViewRow>()
                                .Where(r => !r.IsNewRow)
                                .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.Table_Name].Value.ToString().Equals(sourceDataObject.name))
                                .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString().Equals(autoMappedTargetDataItemName))
                                .FirstOrDefault();

                            if (physicalModelSourceDataItemLookup == null)
                                continue;

                            // If the data item is not on an exception list, it can also be ignored.
                            var businessKeyDefinition = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].Value.ToString();
                            var sourceDataObjectName = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].Value.ToString();

                            var targetDataItemConnectionInternalId = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.TargetConnection.ToString()].Value.ToString();
                            var targetDataItemConnection = TeamConnection.GetTeamConnectionByConnectionId(targetDataItemConnectionInternalId, TeamConfiguration, TeamEventLog);

                            var dataObjectType = GetDataObjectType(targetDataObject.name, "", FormBase.TeamConfiguration);


                            var surrogateKey = JsonOutputHandling.GetSurrogateKey(targetDataObject.name, sourceDataObjectName, businessKeyDefinition, targetDataItemConnection, TeamConfiguration, dataGridViewRowsDataObjects);

                            if (!autoMappedTargetDataItemName.IsIncludedDataItem(dataObjectType, surrogateKey, targetDataItemConnection, TeamConfiguration))
                                continue;

                            // Otherwise, create a data item for both source and target, and add it.
                            List<dynamic> sourceDataItems = new List<dynamic>();
                            var autoMappedSourceDataItem = new DataItem();
                            sourceDataItems.Add(autoMappedSourceDataItem);

                            var autoMappedTargetDataItem = new DataItem();

                            // One to one mapping.
                            autoMappedSourceDataItem.name = autoMappedTargetDataItemName;
                            autoMappedTargetDataItem.name = autoMappedTargetDataItemName;

                            // Add data types to Data Item that are part of a data item mapping.
                            var sourceDataItemConnectionInternalId = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.SourceConnection.ToString()].Value.ToString();
                            var sourceDataItemConnection = TeamConnection.GetTeamConnectionByConnectionId(sourceDataItemConnectionInternalId, TeamConfiguration, TeamEventLog);

                            JsonOutputHandling.SetDataItemMappingDataType(autoMappedSourceDataItem, sourceDataObject, sourceDataItemConnection, JsonExportSetting, dataGridViewRowsPhysicalModel);
                            JsonOutputHandling.SetDataItemMappingDataType(autoMappedTargetDataItem, targetDataObject, targetDataItemConnection, JsonExportSetting, dataGridViewRowsPhysicalModel);

                            // Add parent Data Object to the Data Item.
                            JsonOutputHandling.SetParentDataObjectToDataItem(autoMappedSourceDataItem, sourceDataObject, JsonExportSetting);
                            JsonOutputHandling.SetParentDataObjectToDataItem(autoMappedTargetDataItem, dataObjectMapping.targetDataObject, JsonExportSetting);

                            // Create a Data Item Mapping.
                            DataItemMapping dataItemMapping = new DataItemMapping
                            {
                                sourceDataItems = sourceDataItems,
                                targetDataItem = autoMappedTargetDataItem
                            };

                            // Add to a list that is more easily searched.
                            targetDataItemNames.Add(autoMappedTargetDataItem.name);

                            // Add the Data Items Mapping to the list of mappings.
                            dataItemMappings.Add(dataItemMapping);

                        }
                        catch (Exception exception)
                        {
                            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered: {exception.Message}."));
                        }
                    }
                }

                #endregion

                // Add the data item mappings to the data object mapping.
                if (dataItemMappings.Count > 0)
                {
                    dataObjectMapping.dataItemMappings = dataItemMappings;
                }

            }
            catch
            {
                //
            }

            #endregion

            #region Filter Criterion

            var filterCriterion = dataObjectMappingGridViewRow.Cells[(int)DataObjectMappingGridColumns.FilterCriterion].Value.ToString();

            dataObjectMapping.filterCriterion = filterCriterion;

            #endregion

            #region Business Key

            try
            {
                var businessKeyDefinition = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].Value.ToString();
                var sourceDataObjectName = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].Value.ToString();
                var drivingKeyValue = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.DrivingKeyDefinition.ToString()].Value.ToString();
                JsonOutputHandling.SetBusinessKeys(dataObjectMapping, businessKeyDefinition, sourceDataObjectName, targetConnection, TeamConfiguration, drivingKeyValue, dataGridViewRowsDataObjects, dataGridViewRowsPhysicalModel, TeamEventLog);
            }
            catch (Exception)
            {
                // Catch TBD
            }

            #endregion

            return dataObjectMapping;
        }
    }
}
