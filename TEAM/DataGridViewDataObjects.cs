using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TEAM_Library;
using static TEAM.FormBase;
using DataObject = DataWarehouseAutomation.DataObject;

namespace TEAM
{
    internal sealed class DataGridViewDataObjects : DataGridView
    {
        private TeamConfiguration TeamConfiguration { get; }

        private Form_Edit _modifyJson;

        private readonly ContextMenuStrip contextMenuStripDataObjectMappingFullRow;
        private readonly ContextMenuStrip contextMenuStripDataObjectMappingSingleCell;

        /// <summary>
        /// The definition of the Data Grid View for table mappings (DataObject mappings).
        /// </summary>
        public DataGridViewDataObjects(TeamConfiguration teamConfiguration)
        {
            TeamConfiguration = teamConfiguration;

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
            CellEnter += DataGridViewDataObjects_CellEnter;
            DefaultValuesNeeded += DataGridViewDataObjectMapping_DefaultValuesNeeded;
            Sorted += TextBoxFilterCriterion_OnDelayedTextChanged;
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
            sourceConnection.ValueType = typeof(string);
            Columns.Add(sourceConnection);

            // Source Data Object.
            DataGridViewTextBoxColumn sourceDataObject = new DataGridViewTextBoxColumn();
            sourceDataObject.Name = DataObjectMappingGridColumns.SourceDataObject.ToString();
            sourceDataObject.DataPropertyName = DataObjectMappingGridColumns.SourceDataObject.ToString();
            sourceDataObject.ValueType = typeof(DataObject);
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
            targetConnection.ValueType = typeof(string);
            Columns.Add(targetConnection);

            // Target Data Object.
            DataGridViewTextBoxColumn targetDataObject = new DataGridViewTextBoxColumn();
            targetDataObject.Name = DataObjectMappingGridColumns.TargetDataObject.ToString();
            targetDataObject.DataPropertyName = DataObjectMappingGridColumns.TargetDataObject.ToString();
            targetDataObject.ValueType = typeof(DataObject);
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
            #endregion

            #region Context menu
            // Full row context menu
            contextMenuStripDataObjectMappingFullRow = new ContextMenuStrip();
            contextMenuStripDataObjectMappingFullRow.SuspendLayout();

            // Export as JSON menu item
            var exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem = new ToolStripMenuItem();
            exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Name = "exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem";
            exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Size = new Size(339, 22);
            exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Text = @"Export this row as Source-to-Target interface JSON";
            exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem.Click += ExportThisRowAsSourceToTargetInterfaceJSONToolStripMenuItem_Click;

            // Delete row menu item
            var deleteThisRowFromTheGridToolStripMenuItem = new ToolStripMenuItem();
            deleteThisRowFromTheGridToolStripMenuItem.Name = "deleteThisRowFromTheGridToolStripMenuItem";
            deleteThisRowFromTheGridToolStripMenuItem.Size = new Size(225, 22);
            deleteThisRowFromTheGridToolStripMenuItem.Text = @"Delete this row from the grid";
            deleteThisRowFromTheGridToolStripMenuItem.Click += DeleteThisRowFromTableDataGridToolStripMenuItem_Click;

            contextMenuStripDataObjectMappingFullRow.ImageScalingSize = new Size(24, 24);
            contextMenuStripDataObjectMappingFullRow.Items.AddRange(new ToolStripItem[] {
                exportThisRowAsSourceToTargetInterfaceJsonToolStripMenuItem,
                deleteThisRowFromTheGridToolStripMenuItem
             });

            contextMenuStripDataObjectMappingFullRow.Name = "contextMenuStripTableMapping";
            contextMenuStripDataObjectMappingFullRow.Size = new Size(340, 48);
            contextMenuStripDataObjectMappingFullRow.ResumeLayout(false);

            // Single cell context menu
            contextMenuStripDataObjectMappingSingleCell = new ContextMenuStrip();
            contextMenuStripDataObjectMappingSingleCell.SuspendLayout();

            // Modify JSON menu item
            var toolStripMenuItemModifyJson = new ToolStripMenuItem();
            toolStripMenuItemModifyJson.Name = "toolStripMenuItemModifyJson";
            toolStripMenuItemModifyJson.Size = new Size(143, 22);
            toolStripMenuItemModifyJson.Text = @"Modify JSON";
            toolStripMenuItemModifyJson.Click += toolStripMenuItemModifyJson_Click;

            contextMenuStripDataObjectMappingSingleCell.Items.AddRange(new ToolStripItem[] {
                toolStripMenuItemModifyJson
            });
            contextMenuStripDataObjectMappingSingleCell.Name = "contextMenuStripDataObjectMappingSingleCell";
            contextMenuStripDataObjectMappingSingleCell.Size = new Size(144, 26);
            contextMenuStripDataObjectMappingSingleCell.ResumeLayout(false);
            #endregion
        }
        /// <summary>
        /// Default row values for Data Object Mapping data grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewDataObjectMapping_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            DataObject dataObject = new DataObject
            {
                name = "MyNewDataObject"
            };

            e.Row.Cells[DataObjectMappingGridColumns.SourceConnection.ToString()].Value = TeamConfiguration.MetadataConnection.ConnectionInternalId;
            e.Row.Cells[DataObjectMappingGridColumns.TargetConnection.ToString()].Value = TeamConfiguration.MetadataConnection.ConnectionInternalId;
            e.Row.Cells[DataObjectMappingGridColumns.SourceDataObject.ToString()].Value = dataObject;
            e.Row.Cells[DataObjectMappingGridColumns.TargetDataObject.ToString()].Value = dataObject;
            e.Row.Cells[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].Value = "<business key definition>";
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
        /// This method is called from the context menu on the data grid. It exports the selected row to JSON.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ExportThisRowAsSourceToTargetInterfaceJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //richTextBoxInformation.Clear();

            // Check if any cells were clicked / selected.
            Int32 selectedRow = Rows.GetFirstRow(DataGridViewElementStates.Selected);

            var generationMetadataRow = ((DataRowView)Rows[selectedRow].DataBoundItem).Row;

            var targetDataObjectName = generationMetadataRow[DataObjectMappingGridColumns.TargetDataObject.ToString()].ToString();
            var tableType = MetadataHandling.GetDataObjectType(targetDataObjectName, "", TeamConfiguration);

            if (tableType != MetadataHandling.DataObjectTypes.Presentation)
            {
                List<DataRow> generationMetadataList = new List<DataRow>();
                generationMetadataList.Add(generationMetadataRow);
                //GenerateJsonFromPattern(generationMetadataList, JsonExportSetting);
            }
            else
            {
                //ManageFormJsonInteraction(targetDataObjectName, JsonExportSetting);
            }
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

        private void DataGridViewDataObjects_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTestInfo = HitTest(e.X, e.Y);

                // For now, do nothing when any of the column headers are right-clicked.
                if (hitTestInfo.RowIndex == -1)
                    return;

                // Clear existing selection.
                ClearSelection();

                if (hitTestInfo.ColumnIndex == -1)
                {
                    // Select the full row when the default column is right-clicked.
                    Rows[hitTestInfo.RowIndex].Selected = true;
                    ContextMenuStrip = contextMenuStripDataObjectMappingFullRow;
                }
                else
                {
                    // Evaluate which cell is clicked.
                    var cell = this[hitTestInfo.ColumnIndex, hitTestInfo.RowIndex];

                    //if (cell.ReadOnly)
                    //{
                    //    // Do nothing / ignore.
                    //}
                    if (hitTestInfo.ColumnIndex == (int)DataObjectMappingGridColumns.SourceDataObject || hitTestInfo.ColumnIndex == (int)DataObjectMappingGridColumns.TargetDataObject)
                    {
                        CurrentCell = cell;
                        ContextMenuStrip = contextMenuStripDataObjectMappingSingleCell;
                    }
                    else
                    {
                        Rows[hitTestInfo.RowIndex].Selected = true;
                        ContextMenuStrip = contextMenuStripDataObjectMappingFullRow;
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
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered: {ex.Message}."));
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
        /// Sets the ToolTip text for cells in the Data Object grid view (hover over).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewDataObjects_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var presentationLayerLabelArray = Utility.SplitLabelIntoArray(TeamConfiguration.PresentationLayerLabels);
            var transformationLabelArray = Utility.SplitLabelIntoArray(TeamConfiguration.TransformationLabels);

            // Retrieve the full row for the selected cell.
            DataGridViewRow selectedRow = Rows[e.RowIndex];

            #region Source Data Objects
            // Format the name of the data object, for a source data object
            if (e.ColumnIndex != -1)
            {
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
                            // TBD.
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

                    }
                }
            }
            #endregion

            #region Target Data Object
            if (e.ColumnIndex != -1)
            {
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
                            // TBD.
                        }

                        FormatDataObject(e);

                        string dataObjectName = e.Value.ToString();

                        var targetConnectionId = selectedRow.Cells[(int)DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                        TeamConnection targetConnection = GetTeamConnectionByConnectionId(targetConnectionId);
                        KeyValuePair<string, string> targetDataObjectFullyQualifiedKeyValuePair = MetadataHandling.GetFullyQualifiedDataObjectName(dataObjectName, targetConnection).FirstOrDefault();

                        // Only the name (e.g. without the schema) should be evaluated.
                        string targetDataObjectNonQualifiedName = targetDataObjectFullyQualifiedKeyValuePair.Value;

                        var businessKeySyntax = selectedRow.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value;

                        if (targetDataObjectNonQualifiedName != null && businessKeySyntax != null && selectedRow.IsNewRow == false)
                        {
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
            if (e.ColumnIndex != -1)
            {
                if (Columns[e.ColumnIndex].Name.Equals(DataObjectMappingGridColumns.DrivingKeyDefinition.ToString())
                    && !Rows[e.RowIndex].IsNewRow)
                {
                    DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                    string targetDataObjectName = Rows[e.RowIndex].Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString();

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

                    if (selectedColumn.Index.Equals((int)DataObjectMappingGridColumns.TargetDataObject))
                    {
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
    }
}
