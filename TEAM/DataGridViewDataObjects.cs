using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TEAM_Library;
using DataObject = DataWarehouseAutomation.DataObject;

namespace TEAM
{
    internal class DataGridViewDataObjects : DataGridView
    {
        /// <summary>
        /// The definition of the Data Grid View for table mappings (DataObject mappings).
        /// </summary>
        public DataGridViewDataObjects()
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BorderStyle = BorderStyle.None;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            EditMode = DataGridViewEditMode.EditOnEnter;
            MinimumSize = new Size(965, 515);
            Size = new Size(1100, 545);

            AutoGenerateColumns = false;
            ColumnHeadersVisible = true;
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

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
        }
    }
}
