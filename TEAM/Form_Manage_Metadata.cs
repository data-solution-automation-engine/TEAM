using DataWarehouseAutomation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TEAM_Library;
using static TEAM.DataGridViewDataObjects;
using DataObject = DataWarehouseAutomation.DataObject;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace TEAM
{
    public partial class FormManageMetadata : FormBase
    {
        // Initialise various instances of the status/alert form.
        private Form_Alert _alertParse;
        private Form_Alert _alertValidation;
        private Form_Alert _generatedScripts;
        private static Form_Alert _generatedJsonInterface;
        private Form_Alert _alertEventLog;
        private Form_Alert _physicalModelQuery;

        // Create the Tab Pages.
        private TabPage tabPageDataObjectMapping;
        private TabPage tabPageDataItemMapping;
        private TabPage tabPagePhysicalModel;

        // Keeping track of which files contain which data object mappings
        //private TeamDataObjectMappingsFileCombinations TeamDataObjectMappingFileCombinations;

        // Preparing the Data Table to bind to something.
        private readonly BindingSource BindingSourceDataObjectMappings = new BindingSource();
        private readonly BindingSource BindingSourceDataItemMappings = new BindingSource();
        private readonly BindingSource BindingSourcePhysicalModel = new BindingSource();

        private Size formSize;

        private MetadataValidations metadataValidations;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parent"></param>
        public FormManageMetadata(FormMain parent) : base(parent)
        {
            // Standard call to get the designer controls in place.
            InitializeComponent();

            // Add the Data Object grid view to the tab.
            SetDataObjectGridView();
            SetDataItemGridView();
            SetPhysicalModelGridView();

            // Default setting and start setting of validation counters etc.
            metadataValidations = new MetadataValidations();

            labelHubCount.Text = @"0 Core Business Concepts";
            labelSatCount.Text = @"0 Context entities";
            labelLnkCount.Text = @"0 Relationships";
            labelLsatCount.Text = @"0 Relationship context entities";

            //  Load the grids from the repository
            richTextBoxInformation.Clear();

            // Load the data grids
            PopulateDataObjectMappingGrid();
            PopulateDataItemMappingGrid();
            PopulatePhysicalModelGrid();

            // Inform the user
            var userFeedback = $"The metadata has been loaded.";
            richTextBoxInformation.AppendText($"{userFeedback}\r\n");
            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"{userFeedback}"));

            AssertValidationDetails();

            // Ensure that the count of object types is updated based on whatever is in the data grid.
            ContentCounter();

            // Notify the user of any errors that were detected.
            var errors = TeamEventLog.ReportErrors(TeamEventLog);

            if (errors > 0)
            {
                richTextBoxInformation.AppendText($"{errors} error(s) have been found. Please check the Event Log in the menu.\r\n\r\n");
            }

            _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.Enabled].Width = 40;
            _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.SourceConnection].Width = 90;
            _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.SourceDataObject].Width = 250;
            _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.TargetConnection].Width = 90;
            _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.TargetDataObject].Width = 250;
            _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Width = 125;
            _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.DrivingKeyDefinition].Width = 50;
            _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.FilterCriterion].Width = 50;

            //_dataGridViewDataObjects.AutoResizeColumns();
            //GridAutoLayout();
        }

        /// <summary>
        /// Definition of the Data Object grid view.
        /// </summary>
        private void SetDataObjectGridView()
        {
            // Use custom grid view override class.
            _dataGridViewDataObjects = new DataGridViewDataObjects(TeamConfiguration, JsonExportSetting);
            ((ISupportInitialize)(_dataGridViewDataObjects)).BeginInit();

            _dataGridViewDataObjects.OnDataObjectParse += InformOnDataObjectsResult;
            _dataGridViewDataObjects.DoubleBuffered(true);

            // Add tab page.
            tabPageDataObjectMapping = new TabPage();
            tabPageDataObjectMapping.SuspendLayout();
            tabControlDataMappings.Controls.Add(tabPageDataObjectMapping);

            // Add grid view to tab page.
            tabPageDataObjectMapping.Controls.Add(_dataGridViewDataObjects);
            tabPageDataObjectMapping.Location = new Point(4, 22);
            tabPageDataObjectMapping.Name = "tabPageDataObjectMapping";
            tabPageDataObjectMapping.Padding = new Padding(3);
            tabPageDataObjectMapping.Size = new Size(1106, 545);
            tabPageDataObjectMapping.TabIndex = 0;
            tabPageDataObjectMapping.Text = @"Data Object (Table) Mappings";
            tabPageDataObjectMapping.UseVisualStyleBackColor = true;
            
            tabPageDataObjectMapping.ResumeLayout(false);
            ((ISupportInitialize)(_dataGridViewDataObjects)).EndInit();
        }

        private void InformOnDataObjectsResult(object sender, ParseEventArgs e)
        {
            richTextBoxInformation.Text = e.Text;
        }

        /// <summary>
        /// Definition of the Data Item grid view.
        /// </summary>
        private void SetDataItemGridView()
        {
            _dataGridViewDataItems = new DataGridViewDataItems(TeamConfiguration);
            ((ISupportInitialize)(_dataGridViewDataItems)).BeginInit();
            _dataGridViewDataItems.DoubleBuffered(true);

            // Add tab page.
            tabPageDataItemMapping = new TabPage();
            tabPageDataItemMapping.SuspendLayout();
            tabControlDataMappings.Controls.Add(tabPageDataItemMapping);

            // Add grid view to tab page.
            tabPageDataItemMapping.Controls.Add(_dataGridViewDataItems);
            tabPageDataItemMapping.Location = new Point(4, 22);
            tabPageDataItemMapping.Name = "tabPageDataItemMapping";
            tabPageDataItemMapping.Padding = new Padding(3);
            tabPageDataItemMapping.Size = new Size(1106, 545);
            tabPageDataItemMapping.TabIndex = 2;
            tabPageDataItemMapping.Text = @"Data Item (Column) Mappings";
            tabPageDataItemMapping.UseVisualStyleBackColor = true;

            tabPageDataItemMapping.ResumeLayout(false);
            ((ISupportInitialize)(_dataGridViewDataItems)).EndInit();
        }

        /// <summary>
        /// Definition of the physical model grid view.
        /// </summary>
        private void SetPhysicalModelGridView()
        {
            _dataGridViewPhysicalModel = new DataGridViewPhysicalModel();
            ((ISupportInitialize)(_dataGridViewPhysicalModel)).BeginInit();
            _dataGridViewPhysicalModel.DoubleBuffered(true);

            // Add tab page.
            tabPagePhysicalModel = new TabPage();
            tabPagePhysicalModel.SuspendLayout();
            tabControlDataMappings.Controls.Add(tabPagePhysicalModel);

            // Add grid view to tab page.
            tabPagePhysicalModel.Controls.Add(_dataGridViewPhysicalModel);
            tabPagePhysicalModel.Location = new Point(4, 22);
            tabPagePhysicalModel.Name = "tabPagePhysical";
            tabPagePhysicalModel.Padding = new Padding(3);
            tabPagePhysicalModel.Size = new Size(1106, 545);
            tabPagePhysicalModel.TabIndex = 3;
            tabPagePhysicalModel.Text = @"Physical Model Snapshot";
            tabPagePhysicalModel.UseVisualStyleBackColor = true;

            tabPagePhysicalModel.ResumeLayout(false);
            ((ISupportInitialize)(_dataGridViewPhysicalModel)).EndInit();
        }

        private void AssertValidationDetails()
        {
            // Make sure the validation information is available for this form.
            try
            {
                var validationFile = globalParameters.ConfigurationPath + globalParameters.ValidationFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.FileExtension;

                // If the config file does not exist yet, create it by calling the EnvironmentConfiguration Class.
                if (!File.Exists(validationFile))
                {
                    ValidationSetting.CreateDummyValidationFile(validationFile);
                }

                // Load the validation settings file using the paths retrieved from the application root contents (configuration path).
                ValidationSetting.LoadValidationFile(validationFile);

                richTextBoxInformation.AppendText($"The configuration file {validationFile} has been loaded.\r\n");
            }
            catch (Exception ex)
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"A validation file could not be loaded, so default (all) validation will be used. The exception message is {ex.Message}."));
            }

            // Make sure the json configuration information is available for this form.
            try
            {
                var jsonConfigurationFile = globalParameters.ConfigurationPath + globalParameters.JsonExportConfigurationFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.FileExtension;

                // If the config file does not exist yet, create it.
                if (!File.Exists(jsonConfigurationFile))
                {
                    JsonExportSetting.CreateDummyJsonConfigurationFile(jsonConfigurationFile, TeamEventLog);
                }

                // Load the validation settings file using the paths retrieved from the application root contents (configuration path).
                JsonExportSetting.LoadJsonConfigurationFile(jsonConfigurationFile, true);

                richTextBoxInformation.AppendText($"The configuration file {jsonConfigurationFile} has been loaded.\r\n");
            }
            catch (Exception ex)
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The JSON export configuration file could not be loaded, so default (all) validation will be used. The exception message is {ex.Message}."));
            }

            checkedListBoxReverseEngineeringAreas.CheckOnClick = true;
            checkedListBoxReverseEngineeringAreas.ValueMember = "Key";
            checkedListBoxReverseEngineeringAreas.DisplayMember = "Value";

            //var sourceConnections = _dataGridViewDataObjects.Rows
            //    .Cast<DataGridViewRow>()
            //    .Where(r => !r.IsNewRow)
            //    .Where(r => MetadataHandling.GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", TeamConfiguration) == MetadataHandling.DataObjectTypes.StagingArea)
            //    .Select(row => row.Cells[(int)DataObjectMappingGridColumns.SourceConnection].Value.ToString())
            //    .Distinct()
            //    .ToList();

            // Load the checkboxes for the reverse-engineering tab.
            foreach (var connection in TeamConfiguration.ConnectionDictionary)
            {
                // Only if it's not the metadata connection and not on the 'source' list as derived above.
                if (connection.Value != TeamConfiguration.MetadataConnection)
                {
                    var item = new KeyValuePair<TeamConnection, string>(connection.Value, connection.Value.ConnectionKey);
                    checkedListBoxReverseEngineeringAreas.Items.Add(item);
                }
            }

            for (int i = 0; i < checkedListBoxReverseEngineeringAreas.Items.Count; i++)
            {
                checkedListBoxReverseEngineeringAreas.SetItemChecked(i, true);
            }

        }

        /// <summary>
        /// Populate the Table Mapping DataGrid from an existing JSON file, through an underlying data table.
        /// </summary>
        private void PopulateDataObjectMappingGrid()
        {
            // Get the JSON files and load these into memory.
            var teamDataObjectMappingFileCombinations = new TeamDataObjectMappingsFileCombinations(globalParameters.MetadataPath);
            teamDataObjectMappingFileCombinations.GetMetadata(globalParameters);

            // Parse the JSON files into a data table that supports the grid view.
            var teamDataObjectMappings = new TeamDataObjectMappings(teamDataObjectMappingFileCombinations);

            // Merge events
            TeamEventLog.AddRange(teamDataObjectMappingFileCombinations.EventLog);

            teamDataObjectMappings.SetDataTable(TeamConfiguration);

            #region Assert combo box values

            // Handle unknown combo box values, by setting them to empty in the data table.
            var localConnectionKeyList = LocalTeamConnection.TeamConnectionKeyList(TeamConfiguration.ConnectionDictionary);
            List<string> userFeedbackList = new List<string>();

            foreach (DataRow row in teamDataObjectMappings.DataTable.Rows)
            {
                var comboBoxValueSource = row[(int) DataObjectMappingGridColumns.SourceConnection].ToString();
                var comboBoxValueTarget = row[(int) DataObjectMappingGridColumns.TargetConnection].ToString();

                if (!localConnectionKeyList.Contains(comboBoxValueSource))
                {
                    if (!userFeedbackList.Contains(comboBoxValueSource))
                    {
                        userFeedbackList.Add(comboBoxValueSource);
                    }

                    row[(int) DataObjectMappingGridColumns.SourceConnection] = DBNull.Value;
                }

                if (!localConnectionKeyList.Contains(comboBoxValueTarget))
                {
                    if (!userFeedbackList.Contains(comboBoxValueTarget))
                    {
                        userFeedbackList.Add(comboBoxValueTarget);
                    }

                    row[(int) DataObjectMappingGridColumns.TargetConnection] = DBNull.Value;
                }
            }

            #endregion

            // Provide user feedback is any of the connections have been invalidated.
            if (userFeedbackList.Count > 0)
            {
                foreach (string issue in userFeedbackList)
                {
                    richTextBoxInformation.AppendText($"The connection '{issue}' found in the metadata file does not seem to exist in TEAM. The value has been defaulted in the grid, but not saved yet.\r\n");
                }
            }

            // Make sure the changes in the data table so far are seen as committed, so that user changes can be detected later on.
            teamDataObjectMappings.DataTable.AcceptChanges();

            // Bind the data source.
            BindingSourceDataObjectMappings.DataSource = teamDataObjectMappings.DataTable;

            // Assign the data grid view to the data source.
            _dataGridViewDataObjects.DataSource = BindingSourceDataObjectMappings;

            _dataGridViewDataObjects.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dataGridViewDataObjects.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;

            // Auto resize the grid.
            GridAutoLayout(_dataGridViewDataObjects);
        }

        /// <summary>
        /// Populates the Attribute Mapping DataGrid directly from an existing JSON file.
        /// </summary>
        private void PopulateDataItemMappingGrid()
        { 
            // Load the file into memory (data table and json list)
            AttributeMapping.GetMetadata(TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName());

            //Make sure the changes are seen as committed, so that changes can be detected later on.
            AttributeMapping.DataTable.AcceptChanges();

            BindingSourceDataItemMappings.DataSource = AttributeMapping.DataTable;

            // Set the column header names.
            _dataGridViewDataItems.DataSource = BindingSourceDataItemMappings;

            _dataGridViewDataItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dataGridViewDataItems.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;

            // Resize the grid
            GridAutoLayout(_dataGridViewDataItems);
        }

        /// <summary>
        /// Populates the Physical Model DataGrid from an existing JSON file.
        /// </summary>
        private void PopulatePhysicalModelGrid()
        {
            // Load the file into memory (data table and json list)
            PhysicalModel.GetMetadata(TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName());

            //Make sure the changes are seen as committed, so that changes can be detected later on.
            PhysicalModel.DataTable.AcceptChanges();

            _dataGridViewPhysicalModel.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;

            BindingSourcePhysicalModel.DataSource = PhysicalModel.DataTable;

            // Data Grid View - set the column header names etc. for the data grid view.
            _dataGridViewPhysicalModel.DataSource = BindingSourcePhysicalModel;

            _dataGridViewPhysicalModel.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dataGridViewPhysicalModel.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;

            // Sort
            //DataView view = PhysicalModel.DataTable.DefaultView;
            //view.Sort = $"{PhysicalModelMappingMetadataColumns.databaseName} ASC, {PhysicalModelMappingMetadataColumns.schemaName} ASC, {PhysicalModelMappingMetadataColumns.tableName} ASC, {PhysicalModelMappingMetadataColumns.ordinalPosition} ASC";
            //BindingSourcePhysicalModel.DataSource = view;

            // Resize the grid
            GridAutoLayout(_dataGridViewPhysicalModel);
        }

        private DialogResult STAShowDialog(FileDialog dialog)
        {
            var state = new DialogState {FileDialog = dialog};
            var t = new Thread(state.ThreadProcShowDialog);
            t.SetApartmentState(ApartmentState.STA);

            t.Start();
            t.Join();

            return state.DialogResult;
        }

        public class DialogState
        {
            public DialogResult DialogResult;
            public FileDialog FileDialog;

            public void ThreadProcShowDialog()
            {
                DialogResult = FileDialog.ShowDialog();
            }
        }

        public static void GridAutoLayout()
        {
            GridAutoLayout(_dataGridViewDataObjects);
            GridAutoLayout(_dataGridViewDataItems);
            GridAutoLayout(_dataGridViewPhysicalModel);
        }

        private static void GridAutoLayout(DataGridView dataGridView)
        {
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView.Columns[dataGridView.ColumnCount - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Disable the auto size again (to enable manual resizing).
            for (var i = 0; i < dataGridView.Columns.Count - 1; i++)
            {
                dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView.Columns[i].Width = dataGridView.Columns[i].GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            }
        }

        /// <summary>
        /// Update the form with whatever object types are currently in the data grid.
        /// </summary>
        private void ContentCounter()
        {
            int gridViewRows = _dataGridViewDataObjects.RowCount;
            var counter = 0;

            var hubSet = new HashSet<string>();
            var satSet = new HashSet<string>();
            var lnkSet = new HashSet<string>();
            var lsatSet = new HashSet<string>();

            var inputTableMapping = (DataTable)BindingSourceDataObjectMappings.DataSource;

            foreach (DataRow row in inputTableMapping.Rows)
            {
                var targetDataObject = row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString();
                var targetDataObjectType = MetadataHandling.GetDataObjectType(targetDataObject, "", TeamConfiguration);

                if (gridViewRows != counter + 1 && targetDataObject.Length > 3)
                {
                    if (targetDataObjectType==MetadataHandling.DataObjectTypes.CoreBusinessConcept)
                    {
                        if (!hubSet.Contains(targetDataObject))
                        {
                            hubSet.Add(targetDataObject);
                        }
                    }
                    else if (targetDataObjectType == MetadataHandling.DataObjectTypes.Context)
                    {
                        if (!satSet.Contains(targetDataObject))
                        {
                            satSet.Add(targetDataObject);
                        }
                    }
                    else if (targetDataObjectType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext|| targetDataObjectType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
                    {
                        if (!lsatSet.Contains(targetDataObject))
                        {
                            lsatSet.Add(targetDataObject);
                        }
                    }
                    else if (targetDataObjectType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationship)
                    {
                        if (!lnkSet.Contains(targetDataObject))
                        {
                            lnkSet.Add(targetDataObject);
                        }
                    }
                }

                counter++;
            }

            labelHubCount.Text = $@"{hubSet.Count} Core Business Concepts";
            labelSatCount.Text = satSet.Count + @" Context";
            labelLnkCount.Text = lnkSet.Count + @" Relationships";
            labelLsatCount.Text = lsatSet.Count + @" Relationship Context";
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void manageValidationRulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcValidation);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void CloseValidationForm(object sender, FormClosedEventArgs e)
        {
            _myValidationForm = null;
        }

        private void CloseJsonForm(object sender, FormClosedEventArgs e)
        {
            _myJsonForm = null;
        }

        // Threads starting for other (sub) forms
        private FormManageValidation _myValidationForm;

        public void ThreadProcValidation()
        {
            if (_myValidationForm == null)
            {
                _myValidationForm = new FormManageValidation();
                _myValidationForm.Show();

                Application.Run();
            }

            else
            {
                if (_myValidationForm.InvokeRequired)
                {
                    // Thread Error
                    _myValidationForm.Invoke((MethodInvoker) delegate { _myValidationForm.Close(); });
                    _myValidationForm.FormClosed += CloseValidationForm;

                    _myValidationForm = new FormManageValidation();
                    _myValidationForm.Show();
                    Application.Run();
                }
                else
                {
                    // No invoke required - same thread
                    _myValidationForm.FormClosed += CloseValidationForm;

                    _myValidationForm = new FormManageValidation();
                    _myValidationForm.Show();
                    Application.Run();
                }
            }
        }

        // Threads starting for other (sub) forms
        private FormJsonConfiguration _myJsonForm;

        public void ThreadProcJson()
        {
            if (_myJsonForm == null)
            {
                _myJsonForm = new FormJsonConfiguration();
                _myJsonForm.Show();

                Application.Run();
            }

            else
            {
                if (_myJsonForm.InvokeRequired)
                {
                    // Thread Error
                    _myJsonForm.Invoke((MethodInvoker) delegate { _myJsonForm.Close(); });
                    _myJsonForm.FormClosed += CloseJsonForm;

                    _myJsonForm = new FormJsonConfiguration();
                    _myJsonForm.Show();
                    Application.Run();
                }
                else
                {
                    // No invoke required - same thread
                    _myJsonForm.FormClosed += CloseJsonForm;

                    _myJsonForm = new FormJsonConfiguration();
                    _myJsonForm.Show();
                    Application.Run();
                }
            }
        }

        /// <summary>
        ///   Clicking the 'save' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSaveMetadata_Click(object sender, EventArgs e)
        {
            bool validationIssue = false;

            if (backgroundWorkerReverseEngineering.IsBusy)
            {
                MessageBox.Show(@"The reverse engineer process is running, please wait for this to be completed before saving metadata.", @"Process is running", MessageBoxButtons.OK);
            }
            else
            {
                foreach (DataGridViewRow row in _dataGridViewDataObjects.Rows)
                {
                    if (!string.IsNullOrEmpty(row.ErrorText))
                    {
                        richTextBoxInformation.AppendText($"The Data Object Mapping from '{row.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value}' to '{row.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value}' has an unresolved validation warning.");

                        validationIssue = true;
                    }
                }

                if (validationIssue == false)
                {

                    richTextBoxInformation.Clear();

                    // Create a data table containing the changes, to check if there are changes made to begin with
                    var dataTableTableMappingChanges = ((DataTable)BindingSourceDataObjectMappings.DataSource).GetChanges();
                    var dataTableAttributeMappingChanges = ((DataTable)BindingSourceDataItemMappings.DataSource).GetChanges();
                    var dataTablePhysicalModelChanges = ((DataTable)BindingSourcePhysicalModel.DataSource).GetChanges();

                    // Check if there are any rows available in the grid view, and if changes have been detected at all.
                    if (_dataGridViewDataObjects.RowCount > 0 && dataTableTableMappingChanges != null && dataTableTableMappingChanges.Rows.Count > 0 ||
                        _dataGridViewDataItems.RowCount > 0 && dataTableAttributeMappingChanges != null && dataTableAttributeMappingChanges.Rows.Count > 0 ||
                        _dataGridViewPhysicalModel.RowCount > 0 && dataTablePhysicalModelChanges != null && dataTablePhysicalModelChanges.Rows.Count > 0)
                    {

                        // Perform the saving of the metadata, one for each grid.

                        if (_dataGridViewDataObjects.RowCount > 0 && dataTableTableMappingChanges != null && dataTableTableMappingChanges.Rows.Count > 0)
                        {
                            try
                            {
                                SaveDataObjectMappingJson(dataTableTableMappingChanges);

                                // Load the grids from the repository after being updated.This resets everything.
                                PopulateDataObjectMappingGrid();
                            }
                            catch (Exception exception)
                            {
                                richTextBoxInformation.Text += $@"The Data Object Mapping metadata wasn't saved. The reported error is: {exception.Message}.";
                            }
                        }

                        if (_dataGridViewDataItems.RowCount > 0 && dataTableAttributeMappingChanges != null && dataTableAttributeMappingChanges.Rows.Count > 0)
                        {
                            try
                            {
                                SaveDataItemMappingMetadata(dataTableAttributeMappingChanges);

                                // Load the grids from the repository after being updated.This resets everything.
                                PopulateDataItemMappingGrid();
                            }
                            catch (Exception exception)
                            {
                                richTextBoxInformation.Text += $@"The Data Item Mapping metadata wasn't saved. The reported error is: {exception.Message}.";
                            }
                        }

                        if (_dataGridViewPhysicalModel.RowCount > 0 && dataTablePhysicalModelChanges != null && dataTablePhysicalModelChanges.Rows.Count > 0)
                        {
                            try
                            {
                                SaveModelPhysicalModelMetadata(dataTablePhysicalModelChanges);

                                // Load the grids from the repository after being updated.This resets everything.
                                PopulatePhysicalModelGrid();
                            }
                            catch (Exception exception)
                            {
                                richTextBoxInformation.Text += $@"The Physical Model metadata wasn't saved. The reported error is: {exception.Message}.";
                            }
                        }

                        // Re-apply any filtering, if required.
                        ApplyDataGridViewFiltering();
                    }
                    else
                    {
                        richTextBoxInformation.Text += @"There is no metadata to save!";
                    }
                }
                else
                {
                    MessageBox.Show(@"There are unresolved validation issues, please check the information pane below.", @"Validation issues", MessageBoxButtons.OK);
                }
            }
        }

        /// <summary>
        /// Committing changes to the Data Object Mapping JSON files.
        /// </summary>
        /// <param name="dataTableChanges"></param>
        private void SaveDataObjectMappingJson(DataTable dataTableChanges)
        {
            if (dataTableChanges != null && dataTableChanges.Rows.Count > 0) // Double-check if there are any changes made at all.
            {
                foreach (DataRow row in dataTableChanges.Rows) // Start looping through the changes.
                {
                    #region Changed rows

                    if ((row.RowState & DataRowState.Modified) != 0)
                    {
                        // Figure out the current / previous file name based on the previous target data object name (pre-change).
                        var previousDataObjectName = (string)row[DataObjectMappingGridColumns.PreviousTargetDataObjectName.ToString()];

                        // Figure out the current / new file name based on the available data (post-change).
                        var newDataObject = (DataObject)row[DataObjectMappingGridColumns.TargetDataObject.ToString()];

                        // If there is no change in the file name / target data object name, the change must be made in an existing file.
                        // If there is a change, the values must be written to a new or other file and an existing segment must be removed.

                        if (previousDataObjectName == newDataObject.name)
                        {
                            // A file already exists, and must only be updated.
                            try
                            {
                                // A new file is created and/or an existing one updated to remove a segment.
                                WriteDataObjectMappingsToFile(newDataObject);
                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += $@"There were issues updating the JSON. The error message is {ex.Message}.";
                            }
                        }
                        else
                        {
                            try
                            {
                                // Write the new file.
                                WriteDataObjectMappingsToFile(newDataObject);

                                // Update the old file, and/or delete if there are no segments left
                                WriteDataObjectMappingsToFile(previousDataObjectName);
                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += $@"There were issues updating the JSON. The error message is {ex.Message}.";
                            }
                        }
                    }

                    #endregion

                    #region Inserted rows

                    //Inserted rows
                    if ((row.RowState & DataRowState.Added) != 0)
                    {
                        // Figure out the current / new file name based on the available data (post-change).
                        var newDataObject = (DataObject)row[DataObjectMappingGridColumns.TargetDataObject.ToString()];

                        try
                        {
                            // A new file is created and/or an existing one updated to remove a segment.
                            WriteDataObjectMappingsToFile(newDataObject);
                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += $@"There were issues updating the JSON. The error message is {ex.Message}.";
                        }
                    }

                    #endregion

                    #region Deleted rows

                    //Deleted rows
                    if ((row.RowState & DataRowState.Deleted) != 0)
                    {
                        // Figure out the current / new file name based on the available data (post-change).
                        var newDataObject = (DataObject)row[DataObjectMappingGridColumns.TargetDataObject.ToString(), DataRowVersion.Original];

                        try
                        {
                            // A new file is created and/or an existing one updated to remove a segment.
                            WriteDataObjectMappingsToFile(newDataObject);
                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += $@"There were issues updating the JSON. The error message is {ex.Message}.";
                        }
                    }

                    #endregion
                }
            }
        }

        /// <summary>
        /// Convenience method to wrap the creation of the data object mappings and addition of VDW specific context as well as writing to disk in one call.
        /// </summary>
        /// <param name="targetDataObject"></param>
        internal void WriteDataObjectMappingsToFile(DataObject targetDataObject)
        {
            var dataObjectMappings = _dataGridViewDataObjects.GetDataObjectMappings(targetDataObject);

            if (dataObjectMappings.Count > 0)
            {
                var vdwDataObjectMappingList = GetVdwDataObjectMappingList(targetDataObject, dataObjectMappings);

                string output = JsonConvert.SerializeObject(vdwDataObjectMappingList, Formatting.Indented);
                File.WriteAllText(globalParameters.GetMetadataFilePath(targetDataObject.name), output);

                ((DataTable)BindingSourceDataObjectMappings.DataSource).AcceptChanges();

                ThreadHelper.SetText(this, richTextBoxInformation, $"The Data Object Mapping for '{targetDataObject.name}' has been saved.\r\n");
            }
            else
            {
                var fileToDelete = globalParameters.GetMetadataFilePath(targetDataObject.name);
                File.Delete(fileToDelete);
            }
        }

        /// <summary>
        /// Override to be able to accept string name values for the data object.
        /// </summary>
        /// <param name="targetDataObjectName"></param>
        internal void WriteDataObjectMappingsToFile(string targetDataObjectName)
        {
            var dataObjectMappings = _dataGridViewDataObjects.GetDataObjectMappings(targetDataObjectName);

            if (dataObjectMappings.Count > 0)
            {
                var targetDataObject = dataObjectMappings[0].targetDataObject;
                var vdwDataObjectMappingList = GetVdwDataObjectMappingList(targetDataObject, dataObjectMappings);

                string output = JsonConvert.SerializeObject(vdwDataObjectMappingList, Formatting.Indented);
                File.WriteAllText(globalParameters.GetMetadataFilePath(targetDataObject.name), output);

                ((DataTable)BindingSourceDataObjectMappings.DataSource).AcceptChanges();

                richTextBoxInformation.Text += $"The Data Object Mapping for '{targetDataObject.name}' has been saved.\r\n";
            }
            else
            {
                var fileToDelete = globalParameters.GetMetadataFilePath(targetDataObjectName);
                File.Delete(fileToDelete);
            }
        }

        internal static VDW_DataObjectMappingList GetVdwDataObjectMappingList(DataObject targetDataObject, List<DataObjectMapping> dataObjectMappings)
        {
            // Create an instance of the non-generic information i.e. VDW specific. For example the generation date/time.
            GenerationSpecificMetadata vdwMetadata = new GenerationSpecificMetadata(targetDataObject);
            MetadataConfiguration metadataConfiguration = new MetadataConfiguration(TeamConfiguration);

            VDW_DataObjectMappingList sourceTargetMappingList = new VDW_DataObjectMappingList
            {
                dataObjectMappings = dataObjectMappings,
                generationSpecificMetadata = vdwMetadata,
                metadataConfiguration = metadataConfiguration
            };
            return sourceTargetMappingList;
        }

        private void SaveModelPhysicalModelMetadata(DataTable dataTableChanges)
        {
            if (TeamJsonHandling.JsonFileConfiguration.newFilePhysicalModel == "true")
            {
                TeamJsonHandling.RemoveExistingJsonFile(globalParameters.JsonModelMetadataFileName + ".json");
                TeamJsonHandling.CreatePlaceholderJsonFile(globalParameters.JsonModelMetadataFileName);
                TeamJsonHandling.JsonFileConfiguration.newFilePhysicalModel = "false";
            }

            //Grabbing the generic settings from the main forms
            if (dataTableChanges != null && dataTableChanges.Rows.Count > 0) //Check if there are any changes made at all
            {
                // Retrieve the physical model snapshot file.
                var inputFileName = TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName();
                var jsonArray = JsonConvert.DeserializeObject<PhysicalModelMetadataJson[]>(File.ReadAllText(inputFileName)).ToList();

                foreach (DataRow row in dataTableChanges.Rows) //Loop through the detected changes
                {
                    #region Changed rows

                    //Changed rows
                    if ((row.RowState & DataRowState.Modified) != 0)
                    {
                        //Grab the attributes into local variables
                        var databaseName = "";
                        var schemaName = "";
                        var tableName = "";
                        var columnName = "";
                        var dataType = "";
                        var characterLength = "";
                        var numericPrecision = "";
                        var numericScale = "";
                        int ordinalPosition = 0;
                        var primaryKeyIndicator = "";
                        var multiActiveIndicator = "";

                        if (row[PhysicalModelMappingMetadataColumns.databaseName.ToString()] != DBNull.Value)
                        {
                            databaseName = (string) row[PhysicalModelMappingMetadataColumns.databaseName.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.schemaName.ToString()] != DBNull.Value)
                        {
                            schemaName = (string) row[PhysicalModelMappingMetadataColumns.schemaName.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.tableName.ToString()] != DBNull.Value)
                        {
                            tableName = (string) row[PhysicalModelMappingMetadataColumns.tableName.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.columnName.ToString()] != DBNull.Value)
                        {
                            columnName = (string) row[PhysicalModelMappingMetadataColumns.columnName.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.dataType.ToString()] != DBNull.Value)
                        {
                            dataType = (string) row[PhysicalModelMappingMetadataColumns.dataType.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.characterLength.ToString()] != DBNull.Value)
                        {
                            characterLength = (string) row[PhysicalModelMappingMetadataColumns.characterLength.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.numericPrecision.ToString()] != DBNull.Value)
                        {
                            numericPrecision = (string) row[PhysicalModelMappingMetadataColumns.numericPrecision.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.numericScale.ToString()] != DBNull.Value)
                        {
                            numericScale = (string) row[PhysicalModelMappingMetadataColumns.numericScale.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.ordinalPosition.ToString()] != DBNull.Value)
                        {
                            ordinalPosition = (Int32) row[PhysicalModelMappingMetadataColumns.ordinalPosition.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.primaryKeyIndicator.ToString()] != DBNull.Value)
                        {
                            primaryKeyIndicator = (string) row[PhysicalModelMappingMetadataColumns.primaryKeyIndicator.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.multiActiveIndicator.ToString()] != DBNull.Value)
                        {
                            multiActiveIndicator = (string) row[PhysicalModelMappingMetadataColumns.multiActiveIndicator.ToString()];
                        }

                        try
                        {
                            var databaseNameOld = (string)row[PhysicalModelMappingMetadataColumns.databaseName.ToString(), DataRowVersion.Original];
                            var schemaNameOld = (string)row[PhysicalModelMappingMetadataColumns.schemaName.ToString(), DataRowVersion.Original];
                            var tableNameOld = (string)row[PhysicalModelMappingMetadataColumns.tableName.ToString(), DataRowVersion.Original];
                            var columnNameOld = (string)row[PhysicalModelMappingMetadataColumns.columnName.ToString(), DataRowVersion.Original];

                            //Checks if a matching 'old' JSON segment already exists.
                            var jsonSegmentForDelete = jsonArray.FirstOrDefault(obj => obj.databaseName == databaseNameOld && obj.schemaName == schemaNameOld && obj.tableName == tableNameOld && obj.columnName == columnNameOld);

                            if (jsonSegmentForDelete != null && !string.IsNullOrEmpty(jsonSegmentForDelete.columnName))
                            {
                                // Delete it first.
                                jsonArray.Remove(jsonSegmentForDelete);
                            }

                            // Add the values in the JSON segment
                            var jsonSegment = new PhysicalModelMetadataJson
                            {
                                databaseName = databaseName,
                                schemaName = schemaName,
                                tableName = tableName,
                                columnName = columnName,
                                dataType = dataType,
                                characterLength = characterLength,
                                numericPrecision = numericPrecision,
                                numericScale = numericScale,
                                ordinalPosition = ordinalPosition,
                                primaryKeyIndicator = primaryKeyIndicator,
                                multiActiveIndicator = multiActiveIndicator
                            };

                            jsonArray.Add(jsonSegment);
                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += $"There were issues applying the JSON update.\r\n{ex.Message}";
                        }
                    }

                    #endregion

                    #region Inserted rows

                    // Insert new rows
                    if ((row.RowState & DataRowState.Added) != 0)
                    {
                        string databaseName = "";
                        string schemaName = "";
                        string tableName = "";
                        string columnName = "";
                        string dataType = "";
                        string characterLength= "0";
                        string numericPrecision = "0";
                        string numericScale = "0";
                        int ordinalPosition = 0;
                        string primaryKeyIndicator = "";
                        string multiActiveIndicator = "";

                        if (row[(int) PhysicalModelMappingMetadataColumns.databaseName] != DBNull.Value)
                        {
                            databaseName = (string) row[(int) PhysicalModelMappingMetadataColumns.databaseName];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.schemaName] != DBNull.Value)
                        {
                            schemaName = (string) row[(int) PhysicalModelMappingMetadataColumns.schemaName];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.tableName] != DBNull.Value)
                        {
                            tableName = (string) row[(int) PhysicalModelMappingMetadataColumns.tableName];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.columnName] != DBNull.Value)
                        {
                            columnName = (string) row[(int) PhysicalModelMappingMetadataColumns.columnName];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.dataType] != DBNull.Value)
                        {
                            dataType = (string) row[(int) PhysicalModelMappingMetadataColumns.dataType];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.characterLength] != DBNull.Value)
                        {
                            characterLength = (string) row[(int) PhysicalModelMappingMetadataColumns.characterLength];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.numericPrecision] != DBNull.Value)
                        {
                            numericPrecision = (string) row[(int) PhysicalModelMappingMetadataColumns.numericPrecision];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.numericScale] != DBNull.Value)
                        {
                            numericScale = (string) row[(int) PhysicalModelMappingMetadataColumns.numericScale];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.ordinalPosition] != DBNull.Value)
                        {
                            ordinalPosition = (Int32) row[(int) PhysicalModelMappingMetadataColumns.ordinalPosition];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.primaryKeyIndicator] != DBNull.Value)
                        {
                            primaryKeyIndicator = (string) row[(int) PhysicalModelMappingMetadataColumns.primaryKeyIndicator];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.multiActiveIndicator] != DBNull.Value)
                        {
                            multiActiveIndicator = (string) row[(int) PhysicalModelMappingMetadataColumns.multiActiveIndicator];
                        }

                        try
                        {
                            //Checks if a matching JSON segment already exists.
                            var jsonSegmentListForDelete = jsonArray.Where(obj => obj.databaseName == databaseName && obj.schemaName == schemaName && obj.tableName == tableName && obj.columnName == columnName).ToList();

                            if (jsonSegmentListForDelete != null && jsonSegmentListForDelete.Any())
                            {
                                foreach (var jsonSegmentForDelete in jsonSegmentListForDelete)
                                {
                                    // Delete it first.
                                    jsonArray.Remove(jsonSegmentForDelete);
                                }
                            }

                            // Add the values in the JSON segment
                            var jsonSegment = new PhysicalModelMetadataJson
                            {
                                databaseName = databaseName,
                                schemaName = schemaName,
                                tableName = tableName,
                                columnName = columnName,
                                dataType = dataType,
                                characterLength = characterLength,
                                numericPrecision = numericPrecision,
                                numericScale = numericScale,
                                ordinalPosition = ordinalPosition,
                                primaryKeyIndicator = primaryKeyIndicator,
                                multiActiveIndicator = multiActiveIndicator
                            };

                            jsonArray.Add(jsonSegment);
                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += "There were issues inserting the JSON segment / record.\r\n" + ex.Message;
                        }
                    }

                    #endregion

                    #region Deleted rows

                    //Deleted rows
                    if ((row.RowState & DataRowState.Deleted) != 0)
                    {
                        var databaseName = row[PhysicalModelMappingMetadataColumns.databaseName.ToString(), DataRowVersion.Original].ToString();
                        var schemaName = row[PhysicalModelMappingMetadataColumns.schemaName.ToString(), DataRowVersion.Original].ToString();
                        var tableName = row[PhysicalModelMappingMetadataColumns.tableName.ToString(), DataRowVersion.Original].ToString();
                        var columnName = row[PhysicalModelMappingMetadataColumns.columnName.ToString(), DataRowVersion.Original].ToString();

                        try
                        {
                            //Retrieves the json segment in the file.
                            var jsonSegment = jsonArray.FirstOrDefault(obj => obj.databaseName == databaseName && obj.schemaName == schemaName && obj.tableName == tableName && obj.columnName == columnName);

                            jsonArray.Remove(jsonSegment);

                            if (jsonSegment.columnName == "")
                            {
                                richTextBoxInformation.Text += "The correct segment in the JSON file was not found.\r\n";
                            }
                            else
                            {
                                //Remove the segment from the JSON
                                jsonArray.Remove(jsonSegment);
                            }
                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += "There were issues applying the JSON update.\r\n" + ex;
                        }
                    }
                    #endregion
                }
                // All changes have been processed.

                #region Statement execution

                // Write the result.
                string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                string outputFileName = TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName();

                File.WriteAllText(outputFileName, output);

                //Committing the changes to the data table
                dataTableChanges.AcceptChanges();
                ((DataTable) BindingSourcePhysicalModel.DataSource).AcceptChanges();

                richTextBoxInformation.AppendText("The (physical) model metadata snapshot has been saved.\r\n");

                #endregion
            }
        }

        private void SaveDataItemMappingMetadata(DataTable dataTableChanges)
        {
            if (TeamJsonHandling.JsonFileConfiguration.newFileAttributeMapping == "true")
            {
                TeamJsonHandling.RemoveExistingJsonFile(globalParameters.JsonAttributeMappingFileName + ".json");
                TeamJsonHandling.CreatePlaceholderJsonFile(globalParameters.JsonAttributeMappingFileName);
                TeamJsonHandling.JsonFileConfiguration.newFileAttributeMapping = "false";
            }

            //Check if there are any changes made at all.
            if (dataTableChanges != null && (dataTableChanges.Rows.Count > 0))
            {
                // Loop through the changes captured in the data table
                foreach (DataRow row in dataTableChanges.Rows)
                {
                    #region Changes

                    if ((row.RowState & DataRowState.Modified) != 0)
                    {
                        //Grab the attributes into local variables
                        var hashKey = (string)row[DataItemMappingGridColumns.HashKey.ToString()];
                        var stagingTable = "";
                        var stagingColumn = "";
                        var integrationTable = "";
                        var integrationColumn = "";
                        var notes = "";

                        if (row[DataItemMappingGridColumns.SourceDataObject.ToString()] != DBNull.Value)
                        {
                            stagingTable = (string)row[DataItemMappingGridColumns.SourceDataObject.ToString()];
                        }

                        if (row[DataItemMappingGridColumns.SourceDataItem.ToString()] != DBNull.Value)
                        {
                            stagingColumn = (string)row[DataItemMappingGridColumns.SourceDataItem.ToString()];
                        }

                        if (row[DataItemMappingGridColumns.TargetDataObject.ToString()] != DBNull.Value)
                        {
                            integrationTable = (string)row[DataItemMappingGridColumns.TargetDataObject.ToString()];
                        }

                        if (row[DataItemMappingGridColumns.TargetDataItem.ToString()] != DBNull.Value)
                        {
                            integrationColumn = (string)row[DataItemMappingGridColumns.TargetDataItem.ToString()];
                        }

                        if (row[DataItemMappingGridColumns.Notes.ToString()] != DBNull.Value)
                        {
                            notes = (string)row[DataItemMappingGridColumns.Notes.ToString()];
                        }

                        try
                        {
                            var inputFileName = TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                            DataItemMappingJson[] jsonArray = JsonConvert.DeserializeObject<DataItemMappingJson[]>(File.ReadAllText(inputFileName));

                            //Retrieves the json segment in the file.
                            var jsonHash = jsonArray.FirstOrDefault(obj => obj.attributeMappingHash == hashKey);

                            if (jsonHash.attributeMappingHash == "")
                            {
                                richTextBoxInformation.Text += $"The correct segment in the JSON file was not found.\r\n";
                            }
                            else
                            {
                                // Update the values in the JSON segment
                                jsonHash.sourceTable = stagingTable;
                                jsonHash.sourceAttribute = stagingColumn;
                                jsonHash.targetTable = integrationTable;
                                jsonHash.targetAttribute = integrationColumn;
                                jsonHash.notes = notes;
                            }

                            string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                            string outputFileName = TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                            File.WriteAllText(outputFileName, output);

                            // Update the data object mapping.
                            WriteDataObjectMappingsToFile(integrationTable);
                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += $"There were issues applying the JSON update.\r\n{ex.Message}";
                        }
                    }

                    #endregion

                    #region Inserts

                    // Inserts
                    if ((row.RowState & DataRowState.Added) != 0)
                    {
                        var sourceTable = "";
                        var sourceColumn = "";
                        var targetTable = "";
                        var targetColumn = "";
                        var notes = "";

                        if (row[(int)DataItemMappingGridColumns.SourceDataObject] != DBNull.Value)
                        {
                            sourceTable = (string)row[(int)DataItemMappingGridColumns.SourceDataObject];
                        }

                        if (row[(int)DataItemMappingGridColumns.SourceDataItem] != DBNull.Value)
                        {
                            sourceColumn = (string)row[(int)DataItemMappingGridColumns.SourceDataItem];
                        }

                        if (row[(int)DataItemMappingGridColumns.TargetDataObject] != DBNull.Value)
                        {
                            targetTable = (string)row[(int)DataItemMappingGridColumns.TargetDataObject];
                        }

                        if (row[(int)DataItemMappingGridColumns.TargetDataItem] != DBNull.Value)
                        {
                            targetColumn = (string)row[(int)DataItemMappingGridColumns.TargetDataItem];
                        }

                        if (row[(int)DataItemMappingGridColumns.Notes] != DBNull.Value)
                        {
                            notes = (string)row[(int)DataItemMappingGridColumns.Notes];
                        }

                        try
                        {
                            var jsonAttributeMappingFull = new JArray();

                            // Load the file, if existing information needs to be merged
                            string inputFileName = TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                            DataItemMappingJson[] jsonArray = JsonConvert.DeserializeObject<DataItemMappingJson[]>(File.ReadAllText(inputFileName));

                            // Convert it into a JArray so segments can be added easily
                            if (jsonArray != null)
                            {
                                jsonAttributeMappingFull = JArray.FromObject(jsonArray);
                            }

                            string[] inputHashValue = new string[] { sourceTable, sourceColumn, targetTable, targetColumn, notes };
                            var hashKey = Utility.CreateMd5(inputHashValue, Utility.SandingElement);

                            JObject newJsonSegment = new JObject(
                                new JProperty("attributeMappingHash", hashKey),
                                new JProperty("sourceTable", sourceTable),
                                new JProperty("sourceAttribute", sourceColumn),
                                new JProperty("targetTable", targetTable),
                                new JProperty("targetAttribute", targetColumn),
                                new JProperty("notes", notes)
                            );

                            jsonAttributeMappingFull.Add(newJsonSegment);

                            string output = JsonConvert.SerializeObject(jsonAttributeMappingFull, Formatting.Indented);
                            string outputFileName = TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                            File.WriteAllText(outputFileName, output);

                            // Update the data object mapping.
                            WriteDataObjectMappingsToFile(targetTable);

                            //Making sure the hash key value is added to the data table as well
                            row[(int)DataItemMappingGridColumns.HashKey] = hashKey;

                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += "There were issues inserting the JSON segment / record.\r\n" + ex.Message;
                        }
                    }

                    #endregion

                    #region Deletes

                    // Deletes
                    if ((row.RowState & DataRowState.Deleted) != 0)
                    {
                        var hashKey = row[DataItemMappingGridColumns.HashKey.ToString(), DataRowVersion.Original].ToString();
                        var targetDataObject = row[DataItemMappingGridColumns.TargetDataObject.ToString(), DataRowVersion.Original].ToString();

                        try
                        {
                            string inputFileName = TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                            var jsonArray = JsonConvert.DeserializeObject<DataItemMappingJson[]>(File.ReadAllText(inputFileName)).ToList();

                            //Retrieves the json segment in the file for the given hash returns value or NULL
                            var jsonSegment = jsonArray.FirstOrDefault(obj => obj.attributeMappingHash == hashKey);

                            jsonArray.Remove(jsonSegment);

                            if (jsonSegment.attributeMappingHash == "")
                            {
                                richTextBoxInformation.Text += "The correct segment in the JSON file was not found.\r\n";
                            }
                            else
                            {
                                //Remove the segment from the JSON
                                jsonArray.Remove(jsonSegment);
                            }

                            string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                            string outputFileName = TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                            File.WriteAllText(outputFileName, output);

                            // Update the data object mapping.
                            WriteDataObjectMappingsToFile(targetDataObject);

                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += $"There were issues applying the JSON update.\r\n{ex.Message}";
                        }
                    }

                    #endregion
                }

                #region Statement execution

                //Committing the changes to the data table
                dataTableChanges.AcceptChanges();
                ((DataTable)BindingSourceDataItemMappings.DataSource).AcceptChanges();

                richTextBoxInformation.AppendText($"The Data Item Mapping metadata has been saved.\r\n");

                #endregion
            }
        }
        
        # region Parse process

        private void ButtonParse_Click(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();

            #region Preparation

            // Local boolean to manage whether activation is OK to go ahead.
            bool activationContinue = true;

            // Check if there are any outstanding saves / commits in the data grid
            var dataTableTableMappingChanges = ((DataTable) BindingSourceDataObjectMappings.DataSource).GetChanges();
            var dataTableAttributeMappingChanges = ((DataTable) BindingSourceDataItemMappings.DataSource).GetChanges();
            var dataTablePhysicalModelChanges = ((DataTable) BindingSourcePhysicalModel.DataSource).GetChanges();
            
            if (
                (dataTableTableMappingChanges != null && dataTableTableMappingChanges.Rows.Count > 0) ||
                (dataTableAttributeMappingChanges != null && dataTableAttributeMappingChanges.Rows.Count > 0) ||
                (dataTablePhysicalModelChanges != null && dataTablePhysicalModelChanges.Rows.Count > 0)
            )
            {
                string localMessage = "You have unsaved edits, please save your work before running the end-to-end update.";
                MessageBox.Show(localMessage);
                richTextBoxInformation.AppendText(localMessage);
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, localMessage));
                activationContinue = false;
            }

            #endregion

            #region Validation

            // The first thing to happen is to check if the validation needs to be run (and started if the answer to this is yes).
            if (checkBoxValidation.Checked && activationContinue)
            {
                if (BindingSourcePhysicalModel.Count == 0)
                {
                    richTextBoxInformation.Text = @"There is no physical model metadata available, please make sure the physical model grid contains data.";
                    activationContinue = false;
                }
                else
                {
                    if (backgroundWorkerValidationOnly.IsBusy) return;
                    // create a new instance of the alert form
                    _alertValidation = new Form_Alert();
                    _alertValidation.SetFormName("Validating the metadata");
                    _alertValidation.ShowLogButton(false);
                    _alertValidation.ShowCancelButton(true);
                    _alertValidation.Canceled += buttonCancelParse_Click;
                    _alertValidation.Show();

                    // Start the asynchronous operation.
                    backgroundWorkerValidationOnly.RunWorkerAsync();

                    while (backgroundWorkerValidationOnly.IsBusy)
                    {
                        Application.DoEvents();
                    }
                }
            }

            #endregion

            // After validation finishes, the parse thread / process should start.
            // Only if the validation is enabled AND there are no issues identified in earlier validation checks.

            #region Parse Thread

            if (!checkBoxValidation.Checked || (checkBoxValidation.Checked && metadataValidations.ValidationIssues == 0) && activationContinue)
            {
                if (backgroundWorkerParse.IsBusy) return;
                // create a new instance of the alert form
                _alertParse = new Form_Alert();
                _alertParse.SetFormName("Parsing the data object mappings");
                _alertParse.Canceled += buttonCancelParse_Click;
                _alertParse.ShowLogButton(false);
                _alertParse.ShowCancelButton(true);
                _alertParse.Show();

                // Temporarily disable event handling on binding source to avoid cross-thread issues.
                BindingSourceDataObjectMappings.SuspendBinding();

                // Start the asynchronous operation.
                backgroundWorkerParse.RunWorkerAsync();
            }
            else
            {
                richTextBoxInformation.AppendText("Validation found issues which should be investigated.");
            }

            #endregion
        }

        /// <summary>
        /// This event handler cancels the background worker, fired from Cancel button in AlertForm.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private void buttonCancelParse_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerParse.WorkerSupportsCancellation)
            {
                // Cancel the asynchronous operation.
                backgroundWorkerParse.CancelAsync();

                BindingSourceDataObjectMappings.ResumeBinding();

                // Close the AlertForm
                //_alertParse.Close();
            }
        }

        // This event handler deals with the results of the background operation.
        private void backgroundWorkerParse_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                labelResult.Text = @"Cancelled!";
            }
            else if (e.Error != null)
            {
                labelResult.Text = $@"Error: {e.Error.Message}.";
            }
            else
            {
                BindingSourceDataObjectMappings.ResumeBinding();

                //Load the grids from the repository after being updated.This resets everything.
                PopulateDataObjectMappingGrid();
                PopulateDataItemMappingGrid();
                PopulatePhysicalModelGrid();

                labelResult.Text = @"Done!";
                richTextBoxInformation.Text = @"The metadata was processed successfully!";
            }
        }

        // This event handler updates the progress.
        private void backgroundWorkerParse_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Show the progress in main form
            labelResult.Text = (e.ProgressPercentage + "%");

            // Pass the progress to AlertForm label and progress bar
            _alertParse.Message = "In progress, please wait... " + e.ProgressPercentage + "%";
            _alertParse.ProgressValue = e.ProgressPercentage;
        }

        /// <summary>
        /// The background worker where the heavy lift work is done for the parse process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerParse_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            LogMetadataEvent("Starting a parse of selected data object mappings.\r\n", EventTypes.Information);

            List<string> targetNameList = new List<string>();

            int counter = 0;

            var filteredRowSet = GetFilteredDataObjectMappingDataGridViewRows();

            foreach (DataGridViewRow dataObjectMappingGridViewRow in filteredRowSet)
            {
                // Manage cancellation.
                if (worker.CancellationPending)
                {
                    continue;
                }

                if (!dataObjectMappingGridViewRow.IsNewRow)
                {
                    var targetDataObjectName = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].Value.ToString();

                    if (!targetNameList.Contains(targetDataObjectName))
                    {
                        LogMetadataEvent($"Parsing '{targetDataObjectName}'.", EventTypes.Information);

                        try
                        {
                            var targetDataObject = (DataObject)dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.TargetDataObject.ToString()].Value;

                            WriteDataObjectMappingsToFile(targetDataObject);

                            LogMetadataEvent($"  --> Saved as '{globalParameters.GetMetadataFilePath(targetDataObject.name)}'.", EventTypes.Information);

                            targetNameList.Add(targetDataObjectName);
                        }
                        catch (JsonReaderException ex)
                        {
                            LogMetadataEvent($"There were issues updating the JSON. The error message is {ex.Message}.", EventTypes.Error);
                        }

                        // Normalize all values in array against a 0-100 scale to support the progress bar relative to the number of commands to execute.
                        var normalizedValue = 1 + (counter - 0) * (100 - 1) / (filteredRowSet.Count - 0);
                        worker?.ReportProgress(normalizedValue);
                        counter++;
                    }
                }
            }

            // Manage cancellation.
            if (worker.CancellationPending)
            {
                LogMetadataEvent($"The parsing was cancelled.", EventTypes.Warning);
            }

            worker?.ReportProgress(100);
        }

        #endregion

        private void LogMetadataEvent(string eventMessage, EventTypes eventType)
        {
            TeamEventLog.Add(Event.CreateNewEvent(eventType, eventMessage));
            _alertParse.SetTextLogging("\r\n" + eventMessage);
        }

        internal List<DataGridViewRow> GetFilteredDataObjectMappingDataGridViewRows()
        {
            var filteredRowSet = new List<DataGridViewRow>();

            if (!string.IsNullOrEmpty(textBoxFilterCriterion.Text))
            {
                filteredRowSet = _dataGridViewDataObjects.Rows.Cast<DataGridViewRow>()
                    .Where(row => !row.IsNewRow &&
                                  (row.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Contains(textBoxFilterCriterion.Text) ||
                                   row.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Contains(textBoxFilterCriterion.Text)))
                    .ToList();
            }
            else
            {
                filteredRowSet = _dataGridViewDataObjects.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow).ToList();
            }

            return filteredRowSet;
        }

        internal List<DataGridViewRow> GetFilteredDataItemMappingDataGridViewRows()
        {
            var filteredRowSet = new List<DataGridViewRow>();

            if (!string.IsNullOrEmpty(textBoxFilterCriterion.Text))
            {
                filteredRowSet = _dataGridViewDataItems.Rows.Cast<DataGridViewRow>()
                    .Where(row => !row.IsNewRow &&
                                  (row.Cells[(int)DataItemMappingGridColumns.TargetDataObject].Value.ToString().Contains(textBoxFilterCriterion.Text) ||
                                   row.Cells[(int)DataItemMappingGridColumns.SourceDataObject].Value.ToString().Contains(textBoxFilterCriterion.Text)))
                    .ToList();
            }
            else
            {
                filteredRowSet = _dataGridViewDataItems.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow).ToList();
            }

            return filteredRowSet;
        }

        internal List<DataRow> GetFilteredDataObjectMappingDataTableRows()
        {
            var filteredRowSet = new List<DataRow>();

            DataTable dataTable = (DataTable)BindingSourceDataObjectMappings.DataSource;

            if (!string.IsNullOrEmpty(textBoxFilterCriterion.Text))
            {
                filteredRowSet = dataTable.AsEnumerable().Where(row => row[(int)DataObjectMappingGridColumns.TargetDataObjectName].ToString().Contains(textBoxFilterCriterion.Text) ||
                                                                       row[(int)DataObjectMappingGridColumns.SourceDataObjectName].ToString().Contains(textBoxFilterCriterion.Text))
                    .ToList();
            }
            else
            {
                filteredRowSet = dataTable.AsEnumerable().ToList();
            }

            return filteredRowSet;
        }

        internal List<DataRow> GetFilteredDataItemMappingDataTableRows()
        {
            var filteredRowSet = new List<DataRow>();

            DataTable dataTable = (DataTable)BindingSourceDataItemMappings.DataSource;

            if (!string.IsNullOrEmpty(textBoxFilterCriterion.Text))
            {
                filteredRowSet = dataTable.AsEnumerable().Where(row => row[(int)DataItemMappingGridColumns.TargetDataObject].ToString().Contains(textBoxFilterCriterion.Text) ||
                                                                       row[(int)DataItemMappingGridColumns.SourceDataObject].ToString().Contains(textBoxFilterCriterion.Text))
                    .ToList();
            }
            else
            {
                filteredRowSet = dataTable.AsEnumerable().ToList();
            }

            return filteredRowSet;
        }

        private void saveAsDirectionalGraphMarkupLanguageDgmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var theDialog = new SaveFileDialog
            {
                Title = @"Save Metadata As Directional Graph File",
                Filter = @"DGML files|*.dgml",
                InitialDirectory = Application.StartupPath + @"\Configuration\"
            };

            var ret = STAShowDialog(theDialog);
            
            if (ret == DialogResult.OK)
            {
                var chosenFile = theDialog.FileName;

                int errorCounter = 0;

                if (_dataGridViewDataObjects != null) // There needs to be metadata available
                {
                    var connOmd = new SqlConnection
                    {
                        ConnectionString = TeamConfiguration.MetadataConnection.CreateSqlServerConnectionString(false)
                    };

                    //Write the DGML file
                    var dgmlExtract = new StringBuilder();
                    dgmlExtract.AppendLine("<?xml version=\"1.0\" encoding=\"utf - 8\"?>");
                    dgmlExtract.AppendLine("<DirectedGraph ZoomLevel=\" - 1\" xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">");

                    #region Table nodes

                    //Build up the list of nodes based on the data grid
                    List<string> nodeList = new List<string>();

                    for (int i = 0; i < _dataGridViewDataObjects.Rows.Count - 1; i++)
                    {
                        DataGridViewRow row = _dataGridViewDataObjects.Rows[i];
                        string sourceNode = row.Cells[(int) DataObjectMappingGridColumns.SourceDataObject].Value.ToString();
                        var sourceConnectionId = row.Cells[(int)DataObjectMappingGridColumns.SourceConnection].Value.ToString();
                        var sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(sourceConnectionId, TeamConfiguration, TeamEventLog);
                        KeyValuePair<string, string> fullyQualifiedObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(sourceNode, sourceConnection).FirstOrDefault();


                        string targetNode = row.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value.ToString();
                        var targetConnectionId = row.Cells[(int)DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                        var targetConnection = TeamConnection.GetTeamConnectionByConnectionId(targetConnectionId, TeamConfiguration, TeamEventLog);
                        KeyValuePair<string, string> fullyQualifiedObjectTarget = MetadataHandling.GetFullyQualifiedDataObjectName(targetNode, targetConnection).FirstOrDefault();


                        // Add source tables to Node List
                        if (!nodeList.Contains(fullyQualifiedObjectSource.Key+'.'+ fullyQualifiedObjectSource.Value))
                        {
                            nodeList.Add(fullyQualifiedObjectSource.Key + '.' + fullyQualifiedObjectSource.Value);
                        }

                        // Add target tables to Node List
                        if (!nodeList.Contains(fullyQualifiedObjectTarget.Key + '.' + fullyQualifiedObjectTarget.Value))
                        {
                            nodeList.Add(fullyQualifiedObjectTarget.Key + '.' + fullyQualifiedObjectTarget.Value);
                        }
                    }

                    dgmlExtract.AppendLine("  <Nodes>");

                    var edgeBuilder = new StringBuilder(); // Also create the links while iterating through the below set

                    var presentationLayerLabelArray = Utility.SplitLabelIntoArray(TeamConfiguration.PresentationLayerLabels);
                    
                    
                        
                    foreach (string node in nodeList)
                    {
                        if (node.Contains(TeamConfiguration.StgTablePrefixValue))
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Landing Area\" Group=\"Collapsed\" Label=\"" + node + "\" />");
                            edgeBuilder.AppendLine("     <Link Source=\"Staging Layer\" Target=\"" + node + "\" Category=\"Contains\" />");
                        }
                        else if (node.Contains(TeamConfiguration.PsaTablePrefixValue))
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Persistent Staging Area\" Group=\"Collapsed\" Label=\"" + node + "\" />");
                            edgeBuilder.AppendLine("     <Link Source=\"Staging Layer\" Target=\"" + node + "\" Category=\"Contains\" />");
                        }
                        else if (node.Contains(TeamConfiguration.HubTablePrefixValue))
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Hub\"  Label=\"" + node + "\" />");
                        }
                        else if (node.Contains(TeamConfiguration.LinkTablePrefixValue))
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Link\" Label=\"" + node + "\" />");
                        }
                        else if (node.Contains(TeamConfiguration.SatTablePrefixValue) ||
                                 node.Contains(TeamConfiguration.LsatTablePrefixValue))
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Satellite\" Group=\"Collapsed\" Label=\"" + node + "\" />");
                        }
                        else if (presentationLayerLabelArray.Any(s => node.Contains(s)))
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Presentation\" Label=\"" + node + "\" />");
                        }
                        else
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Sources\" Label=\"" + node + "\" />");
                            edgeBuilder.AppendLine("     <Link Source=\"Sources\" Target=\"" + node + "\" Category=\"Contains\" />");
                        }
                    }

                    #endregion

                    #region Attribute nodes

                    // Separate routine for attribute nodes, with some additional logic to allow for 'duplicate' nodes e.g. source and target attribute names
                    var sqlStatementForSatelliteAttributes = new StringBuilder();
                    sqlStatementForSatelliteAttributes.AppendLine("SELECT SOURCE_SCHEMA_NAME+'.'+SOURCE_NAME AS SOURCE_NAME, TARGET_SCHEMA_NAME+'.'+TARGET_NAME AS TARGET_NAME, SOURCE_ATTRIBUTE_NAME, TARGET_ATTRIBUTE_NAME");
                    sqlStatementForSatelliteAttributes.AppendLine("FROM [interface].[INTERFACE_SOURCE_SATELLITE_ATTRIBUTE_XREF]");

                    var satelliteAttributes = Utility.GetDataTable(ref connOmd, sqlStatementForSatelliteAttributes.ToString());
                    foreach (DataRow row in satelliteAttributes.Rows)
                    {
                        var sourceNodeLabel = (string) row["SOURCE_ATTRIBUTE_NAME"];
                        var sourceNode = "staging_" + sourceNodeLabel;
                        var targetNodeLabel = (string) row["TARGET_ATTRIBUTE_NAME"];
                        var targetNode = "dwh_" + targetNodeLabel;

                        // Add source tables to Node List
                        if (!nodeList.Contains(sourceNode))
                        {
                            nodeList.Add(sourceNode);
                        }

                        // Add target tables to Node List
                        if (!nodeList.Contains(targetNode))
                        {
                            nodeList.Add(targetNode);
                        }

                        dgmlExtract.AppendLine("     <Node Id=\"" + sourceNode + "\"  Category=\"Attribute\" Label=\"" + sourceNodeLabel + "\" />");
                        dgmlExtract.AppendLine("     <Node Id=\"" + targetNode + "\"  Category=\"Attribute\" Label=\"" + targetNodeLabel + "\" />");
                    }

                    #endregion

                    #region Category nodes

                    //Adding the category nodes
                    dgmlExtract.AppendLine("     <Node Id=\"Sources\" Group=\"Collapsed\" Label=\"Sources\"/>");
                    dgmlExtract.AppendLine("     <Node Id=\"Staging Layer\" Group=\"Collapsed\" Label=\"Staging Layer\"/>");
                    dgmlExtract.AppendLine("     <Node Id=\"Data Vault\" Group=\"Expanded\" Label=\"Data Vault\"/>");

                    #endregion

                    #region Subject Area nodes

                    // Add the subject area nodes
                    dgmlExtract.AppendLine("     <!-- Subject Area nodes -->");
                    var sqlStatementForSubjectAreas = new StringBuilder();
                    try
                    {
                        sqlStatementForSubjectAreas.AppendLine("SELECT DISTINCT SUBJECT_AREA");
                        sqlStatementForSubjectAreas.AppendLine("FROM [interface].[INTERFACE_SUBJECT_AREA]");

                        var modelRelationshipsLinksDataTable =
                            Utility.GetDataTable(ref connOmd, sqlStatementForSubjectAreas.ToString());

                        foreach (DataRow row in modelRelationshipsLinksDataTable.Rows)
                        {
                            //dgmlExtract.AppendLine("     <Link Source=\"" + (string)row["BUSINESS_CONCEPT"] + "\" Target=\"" + (string)row["CONTEXT_TABLE"] + "\" />");
                            dgmlExtract.AppendLine("     <Node Id=\"SubjectArea_" + (string) row["SUBJECT_AREA"] +
                                                   "\"  Group=\"Collapsed\" Category=\"Subject Area\" Label=\"" +
                                                   (string) row["SUBJECT_AREA"] + "\" />");
                            edgeBuilder.AppendLine("     <Link Source=\"Data Vault\" Target=\"SubjectArea_" +
                                                   (string) row["SUBJECT_AREA"] + "\" Category=\"Contains\" />");
                        }
                    }
                    catch (Exception)
                    {
                        TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The following query caused an issue when generating the DGML file: \r\n\r\n{sqlStatementForSubjectAreas}."));
                        errorCounter++;
                    }

                    #endregion

                    dgmlExtract.AppendLine("  </Nodes>");
                    //End of Nodes


                    //Edges and containers
                    dgmlExtract.AppendLine("  <Links>");
                    dgmlExtract.AppendLine("     <!-- Place regular nodes in layer containers ('contains') -->");
                    dgmlExtract.Append(
                        edgeBuilder); // Add the containers (e.g. STG and PSA to Staging Layer, Hubs, Links and Satellites to Data Vault


                    // Separate routine to create table / attribute relationships
                    dgmlExtract.AppendLine("     <!-- Table / Attribute relationships -->");
                    foreach (DataRow row in satelliteAttributes.Rows)
                    {
                        var sourceNodeSat = (string) row["TARGET_NAME"];
                        var targetNodeSat = "dwh_" + (string) row["TARGET_ATTRIBUTE_NAME"];
                        var sourceNodeStg = (string) row["SOURCE_NAME"];
                        var targetNodeStg = "staging_" + (string) row["SOURCE_ATTRIBUTE_NAME"];

                        // This is adding the attributes to the tables
                        dgmlExtract.AppendLine("     <Link Source=\"" + sourceNodeSat + "\" Target=\"" + targetNodeSat +
                                               "\" Category=\"Contains\" />");
                        dgmlExtract.AppendLine("     <Link Source=\"" + sourceNodeStg + "\" Target=\"" + targetNodeStg +
                                               "\" Category=\"Contains\" />");

                        // This is adding the edge between the attributes
                        dgmlExtract.AppendLine("     <Link Source=\"" + targetNodeStg + "\" Target=\"" + targetNodeSat +
                                               "\" />");
                    }

                    // Get the source / target model relationships for Hubs and Satellites
                    List<string> segmentNodeList = new List<string>();
                    var modelRelationshipsHubDataTable = new DataTable();
                    var sqlStatementForHubCategories = new StringBuilder();
                    try
                    {
                        sqlStatementForHubCategories.AppendLine("SELECT  TARGET_SCHEMA_NAME+'.'+TARGET_NAME AS TARGET_NAME");
                        sqlStatementForHubCategories.AppendLine("FROM [interface].[INTERFACE_SOURCE_SATELLITE_XREF]");
                        sqlStatementForHubCategories.AppendLine("WHERE TARGET_TYPE = 'Normal'");

                        modelRelationshipsHubDataTable = Utility.GetDataTable(ref connOmd, sqlStatementForHubCategories.ToString());
                    }
                    catch
                    {
                        TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The following query caused an issue when generating the DGML file: \r\n\r\n{sqlStatementForHubCategories}."));
                        errorCounter++;
                    }

                    foreach (DataRow row in modelRelationshipsHubDataTable.Rows)
                    {
                        var modelRelationshipsHub = (string) row["TARGET_NAME"];

                        if (!segmentNodeList.Contains(modelRelationshipsHub))
                        {
                            segmentNodeList.Add(modelRelationshipsHub);
                        }
                    }


                    //Add the relationships between core business concepts - from Hub to Link
                    dgmlExtract.AppendLine("     <!-- Hub / Link relationships -->");
                    var sqlStatementForRelationships = new StringBuilder();
                    try
                    {
                        sqlStatementForRelationships.AppendLine("SELECT DISTINCT [HUB_NAME], TARGET_SCHEMA_NAME+'.'+[TARGET_NAME] AS TARGET_NAME");
                        sqlStatementForRelationships.AppendLine("FROM [interface].[INTERFACE_HUB_LINK_XREF]");
                        sqlStatementForRelationships.AppendLine("WHERE HUB_NAME NOT IN ('N/A')");

                        var businessConceptsRelationships = Utility.GetDataTable(ref connOmd, sqlStatementForRelationships.ToString());

                        foreach (DataRow row in businessConceptsRelationships.Rows)
                        {
                            dgmlExtract.AppendLine("     <Link Source=\"" + (string) row["HUB_NAME"] + "\" Target=\"" + (string) row["TARGET_NAME"] + "\" />");
                        }
                    }
                    catch
                    {
                        TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The following query caused an issue when generating the DGML file: \r\n\r\n{sqlStatementForRelationships}."));
                        errorCounter++;
                    }


                    // Add the relationships to the context tables
                    dgmlExtract.AppendLine("     <!-- Relationships between Hubs/Links to context and their subject area -->");
                    var sqlStatementForLinkCategories = new StringBuilder();
                    try
                    {
                        sqlStatementForLinkCategories.AppendLine("SELECT *");
                        sqlStatementForLinkCategories.AppendLine("FROM [interface].[INTERFACE_SUBJECT_AREA]");

                        var modelRelationshipsLinksDataTable =
                            Utility.GetDataTable(ref connOmd, sqlStatementForLinkCategories.ToString());

                        foreach (DataRow row in modelRelationshipsLinksDataTable.Rows)
                        {
                            var businessConcept = (string) row["BUSINESS_CONCEPT"];

                            var contextTable = Utility.ConvertFromDBVal<string>(row["CONTEXT_TABLE"]);

                            dgmlExtract.AppendLine("     <Link Source=\"" + businessConcept + "\" Target=\"" +
                                                   contextTable + "\" />");

                            dgmlExtract.AppendLine("     <Link Source=\"SubjectArea_" + (string) row["SUBJECT_AREA"] +
                                                   "\" Target=\"" + businessConcept + "\" Category=\"Contains\" />");

                            if (contextTable != null)
                            {
                                dgmlExtract.AppendLine("     <Link Source=\"SubjectArea_" +
                                                       (string) row["SUBJECT_AREA"] + "\" Target=\"" + contextTable +
                                                       "\" Category=\"Contains\" />");
                            }
                        }

                    }
                    catch (Exception)
                    {
                        TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The following query caused an issue when generating the DGML file: \r\n\r\n{sqlStatementForLinkCategories}."));
                        errorCounter++;
                    }


                    // Add the regular source-to-target mappings as edges using the data grid
                    dgmlExtract.AppendLine("     <!-- Regular source-to-target mappings -->");
                    for (var i = 0; i < _dataGridViewDataObjects.Rows.Count - 1; i++)
                    {
                        var row = _dataGridViewDataObjects.Rows[i];
                        
                        string sourceNode = row.Cells[(int)DataObjectMappingGridColumns.SourceDataObject].Value.ToString();
                        var sourceConnectionId = row.Cells[(int)DataObjectMappingGridColumns.SourceConnection].Value.ToString();
                        var sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(sourceConnectionId, TeamConfiguration, TeamEventLog);
                        KeyValuePair<string, string> fullyQualifiedObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(sourceNode, sourceConnection).FirstOrDefault();

                        string targetNode = row.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value.ToString();
                        var targetConnectionId = row.Cells[(int)DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                        var targetConnection = TeamConnection.GetTeamConnectionByConnectionId(targetConnectionId, TeamConfiguration, TeamEventLog);
                        KeyValuePair<string, string> fullyQualifiedObjectTarget = MetadataHandling.GetFullyQualifiedDataObjectName(targetNode, targetConnection).FirstOrDefault();

                        var businessKey = row.Cells[(int) DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString();

                        dgmlExtract.AppendLine("     <Link Source=\"" + fullyQualifiedObjectSource.Key+'.'+ fullyQualifiedObjectSource.Value + "\" Target=\"" + fullyQualifiedObjectTarget.Key+'.'+fullyQualifiedObjectTarget.Value  + "\" BusinessKeyDefinition=\"" + businessKey + "\"/>");
                    }

                    dgmlExtract.AppendLine("  </Links>");
                    // End of edges and containers



                    //Add categories
                    dgmlExtract.AppendLine("  <Categories>");
                    dgmlExtract.AppendLine(
                        "    <Category Id = \"Sources\" Label = \"Sources\" Background = \"#FFE51400\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine(
                        "    <Category Id = \"Landing Area\" Label = \"Landing Area\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine(
                        "    <Category Id = \"Persistent Staging Area\" Label = \"Persistent Staging Area\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine("    <Category Id = \"Hub\" Label = \"Hub\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine("    <Category Id = \"Link\" Label = \"Link\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine(
                        "    <Category Id = \"Satellite\" Label = \"Satellite\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine(
                        "    <Category Id = \"Subject Area\" Label = \"Subject Area\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine("  </Categories>");

                    //Add category styles 
                    dgmlExtract.AppendLine("  <Styles >");

                    dgmlExtract.AppendLine(
                        "    <Style TargetType = \"Node\" GroupLabel = \"Sources\" ValueLabel = \"Has category\" >");
                    dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Sources')\" />");
                    dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                    dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FFFFFFFF\" />");
                    dgmlExtract.AppendLine(
                        "      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                    dgmlExtract.AppendLine("    </Style >");

                    dgmlExtract.AppendLine(
                        "    <Style TargetType = \"Node\" GroupLabel = \"Landing Area\" ValueLabel = \"Has category\" >");
                    dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Landing Area')\" />");
                    dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FE000000\" />");
                    dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FE6E6A69\" />");
                    dgmlExtract.AppendLine(
                        "      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                    dgmlExtract.AppendLine("    </Style >");

                    dgmlExtract.AppendLine(
                        "    <Style TargetType = \"Node\" GroupLabel = \"Persistent Staging Area\" ValueLabel = \"Has category\" >");
                    dgmlExtract.AppendLine(
                        "      <Condition Expression = \"HasCategory('Persistent Staging Area')\" />");
                    dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FA000000\" />");
                    dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FA6E6A69\" />");
                    dgmlExtract.AppendLine(
                        "      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                    dgmlExtract.AppendLine("    </Style >");

                    dgmlExtract.AppendLine(
                        "    <Style TargetType = \"Node\" GroupLabel = \"Hub\" ValueLabel = \"Has category\" >");
                    dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Hub')\" />");
                    dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                    dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FF6495ED\" />");
                    dgmlExtract.AppendLine(
                        "      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                    dgmlExtract.AppendLine("    </Style >");

                    dgmlExtract.AppendLine(
                        "    <Style TargetType = \"Node\" GroupLabel = \"Link\" ValueLabel = \"Has category\" >");
                    dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Link')\" />");
                    dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                    dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FFB22222\" />");
                    dgmlExtract.AppendLine(
                        "      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                    dgmlExtract.AppendLine("    </Style >");

                    dgmlExtract.AppendLine(
                        "    <Style TargetType = \"Node\" GroupLabel = \"Satellite\" ValueLabel = \"Has category\" >");
                    dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Satellite')\" />");
                    dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                    dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FFC0A000\" />");
                    dgmlExtract.AppendLine(
                        "      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                    dgmlExtract.AppendLine("    </Style >");

                    dgmlExtract.AppendLine(
                        "    <Style TargetType = \"Node\" GroupLabel = \"Subject Area\" ValueLabel = \"Has category\" >");
                    dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Subject Area')\" />");
                    dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                    dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FFFFFFFF\" />");
                    dgmlExtract.AppendLine(
                        "      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                    dgmlExtract.AppendLine("    </Style >");

                    dgmlExtract.AppendLine("  </Styles >");

                    dgmlExtract.AppendLine("</DirectedGraph>");
                    // End of graph file creation


                    // Error handling
                    if (errorCounter > 0)
                    {
                        richTextBoxInformation.AppendText("\r\nWarning! There were " + errorCounter + " error(s) found while generating the DGML file.\r\n");
                        richTextBoxInformation.AppendText("Please check the Event Log for details \r\n");

                    }
                    else
                    {
                        richTextBoxInformation.AppendText("\r\nNo errors were detected.\r\n");
                    }
                    
                    // Writing the output
                    using (StreamWriter outfile = new StreamWriter(chosenFile))
                    {
                        outfile.Write(dgmlExtract.ToString());
                        outfile.Close();
                    }

                    richTextBoxInformation.AppendText("The DGML metadata file file://" + chosenFile + " has been saved successfully.");
                }
                else
                {
                    richTextBoxInformation.AppendText("There was no metadata to create the graph with, is the grid view empty?");
                }
            }
        }

        /// <summary>
        ///   Method called when clicking the Reverse Engineer button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonReverseEngineerMetadataClick(object sender, EventArgs e)
        {
            // Select the physical model grid view.
            tabControlDataMappings.SelectedTab = tabPagePhysicalModel;

            richTextBoxInformation.Clear();
            richTextBoxInformation.Text = @"Commencing reverse-engineering the model metadata from the database. This may take a few minutes depending on the complexity of the database.";

            checkedListBoxReverseEngineeringAreas.Enabled = false;

            if (backgroundWorkerValidationOnly.IsBusy) 
                return;

            var changesDataTable = ((DataTable)BindingSourcePhysicalModel.DataSource).GetChanges();

            if (changesDataTable !=null && changesDataTable.Rows.Count>0)
            {
                MessageBox.Show(@"There are unsaved changes in the physical model, please save your changes first before reverse-engineering.", @"Please save changes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            backgroundWorkerReverseEngineering.RunWorkerAsync();
        }

        /// <summary>
        ///   Connect to a given database and return the data dictionary (catalog) information in the data grid.
        /// </summary>
        /// <param name="teamConnection"></param>
        /// <param name="filteredDataObjectMappingDataRows"></param>
        private DataTable ReverseEngineerModelMetadata(TeamConnection teamConnection, List<DataRow> filteredDataObjectMappingDataRows)
        {
            DataTable reverseEngineerResults = new DataTable();

            var conn = new SqlConnection { ConnectionString = teamConnection.CreateSqlServerConnectionString(false) };

            try
            {
                conn.Open();

                var sqlStatementForDataItems = SqlStatementForDataItems(GetDistinctFilteredDataObjects(filteredDataObjectMappingDataRows), teamConnection);

                reverseEngineerResults = Utility.GetDataTable(ref conn, sqlStatementForDataItems);
            }
            catch (Exception exception)
            {
                ThreadHelper.SetText(this, richTextBoxInformation, $@"An error has occurred uploading the model for the new version because the database could not be connected to. The error message is: {exception.Message}.");
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return reverseEngineerResults;
        }

        private List<DataRow> GetDistinctFilteredDataObjects(List<DataRow> filteredDataObjectMappingDataRows)
        {
            var tempFilterDataObjects = new List<DataRow>();

            if (filteredDataObjectMappingDataRows.Any())
            {
                tempFilterDataObjects.AddRange(filteredDataObjectMappingDataRows
                    .Distinct()
                    .ToList());
            }
            else
            {
                DataTable localDataTable = (DataTable)BindingSourceDataObjectMappings.DataSource;

                tempFilterDataObjects.AddRange(localDataTable.AsEnumerable()
                    .Distinct()
                    .ToList());
            }

            var filterDataObjects = tempFilterDataObjects.Distinct().ToList();
            return filterDataObjects;
        }

        private string SqlStatementForDataItems(List<DataRow> filterDataObjects, TeamConnection teamConnection, bool isJson = false)
        {
            var dwhKeyIdentifier = TeamConfiguration.KeyIdentifier; //Indicates _HSH, _SK etc.

            string databaseColumnName = PhysicalModelMappingMetadataColumns.databaseName.ToString();
            string schemaColumnName = PhysicalModelMappingMetadataColumns.schemaName.ToString();
            string tableColumnName = PhysicalModelMappingMetadataColumns.tableName.ToString();
            string columnColumnName = PhysicalModelMappingMetadataColumns.columnName.ToString();
            string dataTypeColumnName = PhysicalModelMappingMetadataColumns.dataType.ToString();
            string characterLengthColumnName = PhysicalModelMappingMetadataColumns.characterLength.ToString();
            string numericPrecisionColumnName = PhysicalModelMappingMetadataColumns.numericPrecision.ToString();
            string numericScaleColumnName = PhysicalModelMappingMetadataColumns.numericScale.ToString();
            string ordinalPositionColumnName = PhysicalModelMappingMetadataColumns.ordinalPosition.ToString();
            string primaryKeyColumnName = PhysicalModelMappingMetadataColumns.primaryKeyIndicator.ToString();
            string multiActiveKeyColumnName = PhysicalModelMappingMetadataColumns.multiActiveIndicator.ToString();
            
            // Prepare the query, depending on the type.
            // Create the attribute selection statement for the array.
            var sqlStatementForDataItems = new StringBuilder();

            if (teamConnection.ConnectionType == ConnectionTypes.Catalog || teamConnection.ConnectionType == ConnectionTypes.Custom)
            {
                // Catalog query.
                if (teamConnection.ConnectionType == ConnectionTypes.Catalog)
                {
                    var databaseName = teamConnection.DatabaseServer.DatabaseName;
                    
                    sqlStatementForDataItems.AppendLine($"-- Physical Model Snapshot query for {teamConnection.ConnectionKey}.");
                    sqlStatementForDataItems.AppendLine("SELECT * FROM");
                    sqlStatementForDataItems.AppendLine("(");
                    sqlStatementForDataItems.AppendLine("  SELECT ");

                    sqlStatementForDataItems.AppendLine($"    DB_NAME(DB_ID('{databaseName}')) AS [{databaseColumnName}],");
                    sqlStatementForDataItems.AppendLine($"    OBJECT_SCHEMA_NAME(main.OBJECT_ID) AS [{schemaColumnName}],");
                    sqlStatementForDataItems.AppendLine($"    OBJECT_NAME(main.OBJECT_ID) AS [{tableColumnName}], ");
                    sqlStatementForDataItems.AppendLine($"    main.[name] AS [{columnColumnName}], ");
                    sqlStatementForDataItems.AppendLine($"    t.[name] AS [{dataTypeColumnName}], ");
                    sqlStatementForDataItems.AppendLine("     CAST(COALESCE(");
                    sqlStatementForDataItems.AppendLine("        CASE WHEN UPPER(t.[name]) = 'NVARCHAR' THEN main.[max_length]/2"); //Exception for unicode
                    sqlStatementForDataItems.AppendLine("        ELSE main.[max_length]");
                    sqlStatementForDataItems.AppendLine("        END");
                    sqlStatementForDataItems.AppendLine($"    ,0) AS VARCHAR(100)) AS [{characterLengthColumnName}],");
                    sqlStatementForDataItems.AppendLine($"    CAST(COALESCE(main.[precision],0) AS VARCHAR(100)) AS [{numericPrecisionColumnName}], ");
                    sqlStatementForDataItems.AppendLine($"    CAST(COALESCE(main.[scale], 0) AS VARCHAR(100)) AS [{numericScaleColumnName}], ");
                    sqlStatementForDataItems.AppendLine($"    main.[column_id] AS [{ordinalPositionColumnName}], ");
                    sqlStatementForDataItems.AppendLine("     CASE ");
                    sqlStatementForDataItems.AppendLine("       WHEN keysub.COLUMN_NAME IS NULL ");
                    sqlStatementForDataItems.AppendLine("       THEN 'N' ");
                    sqlStatementForDataItems.AppendLine("       ELSE 'Y' ");
                    sqlStatementForDataItems.AppendLine($"    END AS {primaryKeyColumnName}, ");
                    sqlStatementForDataItems.AppendLine("     CASE ");
                    sqlStatementForDataItems.AppendLine("       WHEN ma.COLUMN_NAME IS NULL ");
                    sqlStatementForDataItems.AppendLine("       THEN 'N' ");
                    sqlStatementForDataItems.AppendLine("       ELSE 'Y' ");
                    sqlStatementForDataItems.AppendLine($"    END AS {multiActiveKeyColumnName} ");

                    sqlStatementForDataItems.AppendLine("  FROM [" + databaseName + "].sys.columns main");
                    sqlStatementForDataItems.AppendLine("  JOIN sys.types t ON main.user_type_id=t.user_type_id");
                    sqlStatementForDataItems.AppendLine("  -- Primary Key");
                    sqlStatementForDataItems.AppendLine("  LEFT OUTER JOIN (");
                    sqlStatementForDataItems.AppendLine("	  SELECT ");
                    sqlStatementForDataItems.AppendLine("	    sc.name AS TABLE_NAME,");
                    sqlStatementForDataItems.AppendLine("	    C.name AS COLUMN_NAME");
                    sqlStatementForDataItems.AppendLine("	  FROM [" + databaseName + "].sys.index_columns A");
                    sqlStatementForDataItems.AppendLine("	  JOIN [" + databaseName + "].sys.indexes B");
                    sqlStatementForDataItems.AppendLine("	    ON A.OBJECT_ID=B.OBJECT_ID AND A.index_id=B.index_id");
                    sqlStatementForDataItems.AppendLine("	  JOIN [" + databaseName + "].sys.columns C");
                    sqlStatementForDataItems.AppendLine("	   ON A.column_id=C.column_id AND A.OBJECT_ID=C.OBJECT_ID");
                    sqlStatementForDataItems.AppendLine("	  JOIN [" + databaseName + "].sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
                    sqlStatementForDataItems.AppendLine("	  WHERE is_primary_key=1 ");
                    sqlStatementForDataItems.AppendLine("  ) keysub");
                    sqlStatementForDataItems.AppendLine("  ON OBJECT_NAME(main.OBJECT_ID) = keysub.TABLE_NAME");
                    sqlStatementForDataItems.AppendLine("  AND main.[name] = keysub.COLUMN_NAME");

                    //Multi-active

                    var effectiveDateTimeAttribute =
                        TeamConfiguration.EnableAlternativeSatelliteLoadDateTimeAttribute == "True"
                            ? TeamConfiguration.AlternativeSatelliteLoadDateTimeAttribute
                            : TeamConfiguration.LoadDateTimeAttribute;

                    sqlStatementForDataItems.AppendLine("  -- Multi-Active");
                    sqlStatementForDataItems.AppendLine("  LEFT OUTER JOIN (");
                    sqlStatementForDataItems.AppendLine("	 SELECT ");
                    sqlStatementForDataItems.AppendLine("	   sc.name AS TABLE_NAME,");
                    sqlStatementForDataItems.AppendLine("	   C.name AS COLUMN_NAME");
                    sqlStatementForDataItems.AppendLine("	 FROM [" + databaseName + "].sys.index_columns A");
                    sqlStatementForDataItems.AppendLine("	 JOIN [" + databaseName + "].sys.indexes B");
                    sqlStatementForDataItems.AppendLine("	   ON A.OBJECT_ID=B.OBJECT_ID AND A.index_id=B.index_id");
                    sqlStatementForDataItems.AppendLine("	 JOIN [" + databaseName + "].sys.columns C");
                    sqlStatementForDataItems.AppendLine("	   ON A.column_id=C.column_id AND A.OBJECT_ID=C.OBJECT_ID");
                    sqlStatementForDataItems.AppendLine("	 JOIN [" + databaseName + "].sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
                    sqlStatementForDataItems.AppendLine("	 WHERE is_primary_key=1");
                    sqlStatementForDataItems.AppendLine("	 AND C.name NOT IN ('" + effectiveDateTimeAttribute + "')");
                    sqlStatementForDataItems.AppendLine("	 AND C.name NOT LIKE '" + dwhKeyIdentifier + "%'");
                    sqlStatementForDataItems.AppendLine("	 AND C.name NOT LIKE '%" + dwhKeyIdentifier + "'");

                    sqlStatementForDataItems.AppendLine("	 ) ma");
                    sqlStatementForDataItems.AppendLine("	 ON OBJECT_NAME(main.OBJECT_ID) = ma.TABLE_NAME");
                    sqlStatementForDataItems.AppendLine("	 AND main.[name] = ma.COLUMN_NAME");
                    sqlStatementForDataItems.AppendLine(") customSubQuery");
                }
                else if (teamConnection.ConnectionType == ConnectionTypes.Custom)
                {
                    // Use the custom query that was provided with the connection.
                    sqlStatementForDataItems.AppendLine($"-- User-provided (custom) Physical Model Snapshot query for {teamConnection.ConnectionKey}.");
                    sqlStatementForDataItems.AppendLine("SELECT * FROM");
                    sqlStatementForDataItems.AppendLine("(");
                    sqlStatementForDataItems.AppendLine(teamConnection.ConnectionCustomQuery);
                    sqlStatementForDataItems.AppendLine(") customSubQuery");
                }

                // Shared / generic.

                // Add the filtered objects.
                sqlStatementForDataItems.AppendLine("WHERE 1=1");
                sqlStatementForDataItems.AppendLine("  AND");
                sqlStatementForDataItems.AppendLine("   (");

                var filterList = new List<Tuple<string, TeamConnection>>();

                foreach (DataRow row in filterDataObjects)
                {
                    // Skip deleted rows.
                    if (row.RowState == DataRowState.Deleted)
                        continue;

                    string localInternalConnectionIdSource = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    TeamConnection localConnectionSource = TeamConnection.GetTeamConnectionByConnectionId(localInternalConnectionIdSource, TeamConfiguration, TeamEventLog);

                    string localInternalConnectionIdTarget = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                    TeamConnection localConnectionTarget = TeamConnection.GetTeamConnectionByConnectionId(localInternalConnectionIdTarget, TeamConfiguration, TeamEventLog);

                    var localTupleSource = new Tuple<string, TeamConnection>((string)row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()], localConnectionSource);

                    var localTupleTarget = new Tuple<string, TeamConnection>((string)row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()], localConnectionTarget);

                    if (!filterList.Contains(localTupleSource))
                    {
                        filterList.Add(localTupleSource);
                    }

                    if (!filterList.Contains(localTupleTarget))
                    {
                        filterList.Add(localTupleTarget);
                    }
                }

                foreach (var filter in filterList)
                {
                    var fullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(filter.Item1, filter.Item2).FirstOrDefault();

                    // Always add the 'regular' mapping.
                    sqlStatementForDataItems.AppendLine($"     ([{tableColumnName}] = '{fullyQualifiedName.Value}' AND [{schemaColumnName}] = '{fullyQualifiedName.Key}')");
                    sqlStatementForDataItems.AppendLine("     OR");
                }

                // Remove the last OR
                sqlStatementForDataItems.Remove(sqlStatementForDataItems.Length - 6, 6);

                sqlStatementForDataItems.AppendLine(")");
                sqlStatementForDataItems.AppendLine($"ORDER BY {tableColumnName}, {columnColumnName}, {ordinalPositionColumnName}");

                if (isJson)
                {
                    sqlStatementForDataItems.AppendLine("FOR JSON PATH");
                    sqlStatementForDataItems.AppendLine();
                }
            }
            else
            {
                richTextBoxInformation.Text += @"An exception has occurred while determining the connection type. The connection does not have a valid connection type (0, catalog or 1, custom).";
            }

            return sqlStatementForDataItems.ToString();
        }

        private void TextBoxFilterCriterion_OnDelayedTextChanged(object sender, EventArgs e)
        {
            ApplyDataGridViewFiltering();
        }

        private void ApplyDataGridViewFiltering()
        {
            var filterCriterion = textBoxFilterCriterion.Text;

            // Only update the grid view on the visible tab.
            if (tabControlDataMappings.SelectedIndex == 0)
            {
                foreach (DataGridViewRow row in _dataGridViewDataObjects.Rows)
                {
                    row.Visible = true;

                    if (row.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value != null)
                    {
                        if (!row.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Contains(filterCriterion) &&
                            !row.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Contains(filterCriterion))
                        {
                            CurrencyManager currencyManager = (CurrencyManager)BindingContext[_dataGridViewDataObjects.DataSource];
                            currencyManager.SuspendBinding();
                            row.Visible = false;
                            currencyManager.ResumeBinding();
                        }
                    }
                }
            }
            else if (tabControlDataMappings.SelectedIndex == 1)
            {
                foreach (DataGridViewRow row in _dataGridViewDataItems.Rows)
                {
                    row.Visible = true;

                    if (row.Cells[(int)DataItemMappingGridColumns.TargetDataObject].Value != null)
                    {
                        if (!row.Cells[(int)DataItemMappingGridColumns.SourceDataObject].Value.ToString().Contains(filterCriterion) &&
                            !row.Cells[(int)DataItemMappingGridColumns.SourceDataItem].Value.ToString().Contains(filterCriterion) &&
                            !row.Cells[(int)DataItemMappingGridColumns.TargetDataObject].Value.ToString().Contains(filterCriterion) &&
                            !row.Cells[(int)DataItemMappingGridColumns.TargetDataItem].Value.ToString().Contains(filterCriterion))
                        {
                            CurrencyManager currencyManager = (CurrencyManager)BindingContext[_dataGridViewDataItems.DataSource];
                            currencyManager.SuspendBinding();
                            row.Visible = false;
                            currencyManager.ResumeBinding();
                        }
                    }
                }
            }
            else if (tabControlDataMappings.SelectedIndex == 2)
            {
                var inputTableMappingPhysicalModel = (DataTable)BindingSourcePhysicalModel.DataSource;
                var currentFilter = inputTableMappingPhysicalModel.DefaultView.RowFilter;

                // Build the filter.
                if (string.IsNullOrEmpty(filterCriterion) && checkBoxShowStaging.Checked)
                {
                    // Don't worry about it. There's not filters to set, everything needs to be shown.
                    // Reset the filter.
                    inputTableMappingPhysicalModel.DefaultView.RowFilter = string.Empty;
                }
                else
                {
                    var filterCriterionPhysicalModel = "";

                    if (!string.IsNullOrEmpty(filterCriterion) && string.IsNullOrEmpty(currentFilter))
                    {
                        // There is no broad filter, but a user filter has been set.
                        // Apply text box filter to support user filtering.
                        filterCriterionPhysicalModel = $"[{PhysicalModelMappingMetadataColumns.databaseName}] LIKE '{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.tableName}] LIKE '{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.columnName}] LIKE '{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.schemaName}] LIKE '{filterCriterion}%'";inputTableMappingPhysicalModel.DefaultView.RowFilter = filterCriterionPhysicalModel;
                    }
                    else if (!string.IsNullOrEmpty(currentFilter) && !string.IsNullOrEmpty(filterCriterion))
                    {
                        // There is already a broad filter, and a user filter has also been set.

                        // Re-evaluate the need for the filter.
                        if (checkBoxShowStaging.Checked)
                        {
                            // Show everything - reset the filter.
                            currentFilter = "1=1 ";
                        }
                        else
                        {
                            // The target is not a STG process and not a PSA process.
                            currentFilter = $"[{PhysicalModelMappingMetadataColumns.tableName}] NOT LIKE '{TeamConfiguration.StgTablePrefixValue}%' AND [{PhysicalModelMappingMetadataColumns.tableName}] NOT LIKE '{TeamConfiguration.PsaTablePrefixValue}%'";
                        }

                        // Merge with existing filter.
                        filterCriterionPhysicalModel = currentFilter + $"AND [{PhysicalModelMappingMetadataColumns.databaseName}] LIKE '{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.tableName}] LIKE '{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.columnName}] LIKE '{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.schemaName}] LIKE '{filterCriterion}%'";
                    }

                    inputTableMappingPhysicalModel.DefaultView.RowFilter = filterCriterionPhysicalModel;
                }
            }
            else
            {
                // Exception - cannot happen.
                richTextBoxInformation.Text = $@"An incorrect data grid view was provided: '{tabControlDataMappings.TabPages[tabControlDataMappings.SelectedIndex]}'. This is a bug, please raise a Github issue.";
            }
        }

        /// <summary>
        ///   Run the validation checks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorkerValidation_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // Data Object Mapping filtered rows (starting point for validation).
            var filteredDataObjectMappingDataRows = GetFilteredDataObjectMappingDataTableRows();
            var filteredDataObjectMappingDataGridViewRows = GetFilteredDataObjectMappingDataGridViewRows();

            var dataObjectMappingDataTable = (DataTable)BindingSourceDataObjectMappings.DataSource;
            var dataObjectMappingDataRows = dataObjectMappingDataTable.AsEnumerable().ToList();

            // Data Item Mapping filtered rows (starting point for validation).
            var filteredDataItemDataRows = GetFilteredDataItemMappingDataTableRows();

            // Physical model snapshot.
            var dataObjectMappingGridViewRows = _dataGridViewDataObjects.Rows.Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .ToList();

            var physicalModelDataTable = (DataTable)BindingSourcePhysicalModel.DataSource;

            // Handling multi-threading
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                _alertValidation.SetTextLogging("Commencing validation on available metadata, according to settings in the validation screen.\r\n\r\n");

                // Set the validation issue counter to 0 to start a new validation round.
                metadataValidations.ValidationIssues = 0;

                if (ValidationSetting.DataObjectExistence == "True")
                {
                    _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateObjectExistence(filteredDataObjectMappingDataRows, physicalModelDataTable, TeamConfiguration, TeamEventLog, ref metadataValidations));
                }
                worker?.ReportProgress(10);

                if (ValidationSetting.SourceBusinessKeyExistence == "True")
                {
                    _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateBusinessKeyObject(filteredDataObjectMappingDataRows, TeamConfiguration, TeamEventLog, physicalModelDataTable, ref metadataValidations));
                }
                worker?.ReportProgress(20);

                if (ValidationSetting.DataItemExistence == "True")
                {
                    _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateDataItemExistence(filteredDataItemDataRows, dataObjectMappingDataTable, physicalModelDataTable, TeamConfiguration, TeamEventLog, ref metadataValidations));
                }
                worker?.ReportProgress(30);
                
                if (ValidationSetting.LogicalGroup == "True")
                {
                    _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateLogicalGroup(filteredDataObjectMappingDataRows, dataObjectMappingDataTable, TeamConfiguration, TeamEventLog, ref metadataValidations));
                }
                worker?.ReportProgress(40);

                if (ValidationSetting.LinkKeyOrder == "True")
                {
                    _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateLinkKeyOrder(filteredDataObjectMappingDataRows, dataObjectMappingDataTable, physicalModelDataTable, dataObjectMappingGridViewRows, TeamConfiguration, TeamEventLog, ref metadataValidations));
                }
                worker?.ReportProgress(45);

                if (ValidationSetting.LinkCompletion == "True")
                {
                    _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateLinkCompletion(filteredDataObjectMappingDataRows, TeamConfiguration, ref metadataValidations));
                }
                worker?.ReportProgress(50);

                _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateHardcodedFields(filteredDataObjectMappingDataRows, TeamConfiguration, ref metadataValidations));
                worker?.ReportProgress(60);


                _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateAttributeDataObjectsForTableMappings(filteredDataItemDataRows, dataObjectMappingDataRows, ref metadataValidations));
                worker?.ReportProgress(70);

                _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateSchemaConfiguration(filteredDataObjectMappingDataRows, TeamConfiguration, TeamEventLog, ref metadataValidations));
                worker?.ReportProgress(80);

                // Validate basic Data Vault settings
                if (ValidationSetting.BasicDataVaultValidation == "True")
                {
                    _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateBasicDataVaultAttributeExistence(filteredDataObjectMappingDataRows, TeamConfiguration, TeamEventLog, ref metadataValidations));
                }
                worker?.ReportProgress(90);

                // Check for duplicate data object mappings.
                if (ValidationSetting.DuplicateDataObjectMappings == "True")
                {
                    _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateDuplicateDataObjectMappings(filteredDataObjectMappingDataGridViewRows, ref metadataValidations));
                }
                worker?.ReportProgress(100);

                // Informing the user.
                _alertValidation.SetTextLogging("\r\n\r\nIn total " + metadataValidations.ValidationIssues + " validation issues have been found.");
            }
        }
     
        private void backgroundWorkerValidationOnly_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Show the progress in main form (GUI)
            labelResult.Text = (e.ProgressPercentage + @"%");

            // Pass the progress to AlertForm label and progressbar
            _alertValidation.Message = "In progress, please wait... " + e.ProgressPercentage + "%";
            _alertValidation.ProgressValue = e.ProgressPercentage;
        }

        private void backgroundWorkerValidationOnly_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                labelResult.Text = @"Cancelled!";
            }
            else if (e.Error != null)
            {
                labelResult.Text = $@"Error: {e.Error.Message}";
            }
            else
            {
                labelResult.Text = @"Done!";
                richTextBoxInformation.Text += "\r\nThe metadata was validated successfully!\r\n";
            }
        }
        
        private void displayTableScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Retrieve the index of the selected row
            Int32 selectedRow = _dataGridViewPhysicalModel.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            DataTable gridDataTable = (DataTable) BindingSourcePhysicalModel.DataSource;
            DataTable dt2 = gridDataTable.Clone();
            dt2.Columns[PhysicalModelMappingMetadataColumns.ordinalPosition.ToString()].DataType = Type.GetType("System.Int32");

            foreach (DataRow dr in gridDataTable.Rows)
            {
                dt2.ImportRow(dr);
            }

            dt2.AcceptChanges();

            // Make sure the output is sorted
            dt2.DefaultView.Sort = $"{PhysicalModelMappingMetadataColumns.tableName.ToString()} ASC, {PhysicalModelMappingMetadataColumns.ordinalPosition.ToString()} ASC";

            // Retrieve all rows relative to the selected row (e.g. all attributes for the table)
            IEnumerable<DataRow> rows = dt2.DefaultView.ToTable().AsEnumerable().Where(r =>
                r.Field<string>(PhysicalModelMappingMetadataColumns.tableName.ToString()) ==
                _dataGridViewPhysicalModel.Rows[selectedRow].Cells[4].Value.ToString()
                && r.Field<string>(PhysicalModelMappingMetadataColumns.schemaName.ToString()) ==
                _dataGridViewPhysicalModel.Rows[selectedRow].Cells[3].Value.ToString()
                && r.Field<string>(PhysicalModelMappingMetadataColumns.databaseName.ToString()) ==
                _dataGridViewPhysicalModel.Rows[selectedRow].Cells[2].Value.ToString()
            );

            // Create a form and display the results
            var results = new StringBuilder();

            _generatedScripts = new Form_Alert();
            _generatedScripts.SetFormName("Display model metadata");
            _generatedScripts.Canceled += buttonCancelParse_Click;
            _generatedScripts.Show();

            results.AppendLine("IF OBJECT_ID('[" + _dataGridViewPhysicalModel.Rows[selectedRow].Cells[4].Value + "]', 'U') IS NOT NULL");
            results.AppendLine("DROP TABLE [" + _dataGridViewPhysicalModel.Rows[selectedRow].Cells[4].Value + "]");
            results.AppendLine();
            results.AppendLine("CREATE TABLE [" + _dataGridViewPhysicalModel.Rows[selectedRow].Cells[4].Value + "]");
            results.AppendLine("(");

            int counter = 1;
            foreach (DataRow row in rows)
            {
                string commaSnippet;
                if (counter == 1)
                {
                    commaSnippet = "  ";
                }
                else
                {
                    commaSnippet = " ,";
                }

                counter++;
                results.AppendLine(commaSnippet + row[PhysicalModelMappingMetadataColumns.columnName.ToString()] +" -- with ordinal position of " +row[PhysicalModelMappingMetadataColumns.ordinalPosition.ToString()]);
            }

            results.AppendLine(")");

            _generatedScripts.SetTextLogging(results.ToString());
            _generatedScripts.ProgressValue = 100;
            _generatedScripts.Message = "Done";
        }

        /// <summary>
        /// Convenience method to do all the form stuff related to JSON generation, such as saving and showing the status form, in one go.
        /// </summary>
        /// <param name="jsonString"></param>
        internal static void ManageFormJsonInteraction(string jsonString)
        {
            _generatedJsonInterface = new Form_Alert();
            _generatedJsonInterface.SetFormName("Displaying the metadata as JSON");
            _generatedJsonInterface.ShowProgressBar(false);
            _generatedJsonInterface.ShowCancelButton(false);
            _generatedJsonInterface.ShowLogButton(false);
            _generatedJsonInterface.ShowProgressLabel(false);
            _generatedJsonInterface.Show();
            _generatedJsonInterface.SetTextLogging(jsonString + "\r\n\r\n");
        }

        private void openConfigurationDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (globalParameters.ConfigurationPath != "")
                {
                    Process.Start(globalParameters.ConfigurationPath);
                }
                else
                {
                    richTextBoxInformation.Text = @"There is no value given for the Configuration Path. Please enter a valid path name.";
                }
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = $@"An error has occurred while attempting to open the configuration directory. The error message is: {ex.Message}";
            }
        }
        private void openDataItemMappingFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Data Item Mapping Metadata File",
                Filter = @"Data Item Mapping files|*.json",
                InitialDirectory = globalParameters.MetadataPath
            };

            var ret = STAShowDialog(theDialog);

            if (ret == DialogResult.OK)
            {
                try
                {
                    var chosenFile = theDialog.FileName;
                    
                    // Load the file, convert it to a DataTable and bind it to the source
                    List<DataItemMappingJson> jsonArray = JsonConvert.DeserializeObject<List<DataItemMappingJson>>(File.ReadAllText(chosenFile));

                    if (jsonArray != null)
                    {
                        foreach (DataItemMappingJson dataItemMappingJson in jsonArray)
                        {
                            // Check if the row does not already exist.
                            var lookupRow = _dataGridViewDataItems.Rows
                                .Cast<DataGridViewRow>()
                                .FirstOrDefault(row => !row.IsNewRow && row.Cells[(int)DataItemMappingGridColumns.SourceDataObject].Value.ToString() == dataItemMappingJson.sourceTable && 
                                                       row.Cells[(int)DataItemMappingGridColumns.TargetDataObject].Value.ToString() == dataItemMappingJson.targetTable &&
                                                       row.Cells[(int)DataItemMappingGridColumns.SourceDataItem].Value.ToString() == dataItemMappingJson.sourceAttribute &&
                                                       row.Cells[(int)DataItemMappingGridColumns.TargetDataItem].Value.ToString() == dataItemMappingJson.targetAttribute);

                            // Add if it does not exist yet.
                            if (lookupRow == null)
                            {
                                DataTable dataTable = (DataTable)BindingSourceDataItemMappings.DataSource;

                                DataRow dataRow = dataTable.NewRow();

                                dataRow[(int)DataItemMappingGridColumns.HashKey] = dataItemMappingJson.attributeMappingHash;
                                dataRow[(int)DataItemMappingGridColumns.SourceDataObject] = dataItemMappingJson.sourceTable;
                                dataRow[(int)DataItemMappingGridColumns.SourceDataItem] = dataItemMappingJson.sourceAttribute;
                                dataRow[(int)DataItemMappingGridColumns.TargetDataObject] = dataItemMappingJson.targetTable;
                                dataRow[(int)DataItemMappingGridColumns.TargetDataItem] = dataItemMappingJson.targetAttribute;
                                dataRow[(int)DataItemMappingGridColumns.Notes] = dataItemMappingJson.notes;

                                dataTable.Rows.Add(dataRow);
                            }
                        }
                    }

                    GridAutoLayout(_dataGridViewDataItems);
                    richTextBoxInformation.AppendText($"The data item metadata has been loaded from file '{chosenFile}'.\r\n");
                    ContentCounter();
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.AppendText("An error has been encountered. Please check the Event Log for more details.\r\n");
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered: {ex.Message}."));
                }
            }
        }

        /// <summary>
        ///  Load a Data Object Mapping tabular JSON file into the data grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openMetadataFileToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = @"Open Data Object Mapping Metadata File",
                Filter = @"Data Object Mapping files|*.json",
                InitialDirectory = globalParameters.MetadataPath
            };

            var dialogResult = STAShowDialog(dialog);

            if (dialogResult == DialogResult.OK)
            {
                richTextBoxInformation.Clear();

                try
                {
                    // Get the selected file in tabular JSON format.
                    List<TableMappingJson> jsonArray = JsonConvert.DeserializeObject<List<TableMappingJson>>(File.ReadAllText(dialog.FileName));

                    #region Build the Data Table

                    var localDataTable = new DataTable();

                    localDataTable.Columns.Add(DataObjectMappingGridColumns.Enabled.ToString());
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.HashKey.ToString());
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.SourceConnection.ToString());
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.SourceDataObject.ToString(), typeof(DataObject));
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.TargetConnection.ToString());
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.TargetDataObject.ToString(), typeof(DataObject));
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.BusinessKeyDefinition.ToString());
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.DrivingKeyDefinition.ToString());
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.FilterCriterion.ToString());
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.SourceDataObjectName.ToString());
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.TargetDataObjectName.ToString());
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.PreviousTargetDataObjectName.ToString());
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.SurrogateKey.ToString());

                    TeamDataObjectMappings.SetDataTableColumnNames(localDataTable);

                    if (jsonArray != null)
                    {
                        foreach (TableMappingJson tableMappingJson in jsonArray)
                        {
                            var localSourceDataObject = new DataObject
                            {
                                name = tableMappingJson.sourceTable
                            };

                            var localTargetDataObject = new DataObject
                            {
                                name = tableMappingJson.targetTable
                            };

                            var newRow = localDataTable.NewRow();

                            newRow[(int)DataObjectMappingGridColumns.Enabled] = tableMappingJson.enabledIndicator;
                            newRow[(int)DataObjectMappingGridColumns.HashKey] = tableMappingJson.tableMappingHash;
                            newRow[(int)DataObjectMappingGridColumns.SourceConnection] = tableMappingJson.sourceConnectionKey;
                            newRow[(int)DataObjectMappingGridColumns.SourceDataObject] = localSourceDataObject;
                            newRow[(int)DataObjectMappingGridColumns.TargetConnection] = tableMappingJson.targetConnectionKey;
                            newRow[(int)DataObjectMappingGridColumns.TargetDataObject] = localTargetDataObject;
                            newRow[(int)DataObjectMappingGridColumns.BusinessKeyDefinition] = tableMappingJson.businessKeyDefinition;
                            newRow[(int)DataObjectMappingGridColumns.DrivingKeyDefinition] = tableMappingJson.drivingKeyDefinition;
                            newRow[(int)DataObjectMappingGridColumns.FilterCriterion] = tableMappingJson.filterCriteria;
                            newRow[(int)DataObjectMappingGridColumns.SourceDataObjectName] = localSourceDataObject.name;
                            newRow[(int)DataObjectMappingGridColumns.TargetDataObjectName] = localTargetDataObject.name;
                            newRow[(int)DataObjectMappingGridColumns.PreviousTargetDataObjectName] = localTargetDataObject.name;
                            newRow[(int)DataObjectMappingGridColumns.SurrogateKey] = "";

                            localDataTable.Rows.Add(newRow);
                        }
                    }

                    //Make sure the changes are seen as committed, so that changes can be detected later on.

                    localDataTable.AcceptChanges();

                    // Clear out the existing data from the grid
                    BindingSourceDataObjectMappings.DataSource = null;
                    BindingSourceDataObjectMappings.Clear();

                    _dataGridViewDataObjects.DataSource = null;

                    // Bind the data table to the grid view
                    BindingSourceDataObjectMappings.DataSource = localDataTable;

                    // Set the column header names
                    _dataGridViewDataObjects.DataSource = BindingSourceDataObjectMappings;

                    GridAutoLayout(_dataGridViewDataObjects);
                    ContentCounter();

                    richTextBoxInformation.AppendText($"The file '{dialog.FileName}' was loaded.\r\n");

                    #endregion

                    #region Generate the JSON files
                    foreach (DataRow row in localDataTable.Rows)
                    {
                        var newDataObject = (DataObject)row[DataObjectMappingGridColumns.TargetDataObject.ToString()];

                        try
                        {
                            // A new file is created and/or an existing one updated to remove a segment.
                            WriteDataObjectMappingsToFile(newDataObject);
                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += $@"There were issues updating the JSON. The error message is {ex.Message}.";
                        }
                    }
                    #endregion

                    #region Reload the full Data Grid

                    //Load the grids from the repository after being updated. This resets everything.
                    PopulateDataObjectMappingGrid();

                    // Notify the user of any errors that were detected.
                    var errors = TeamEventLog.ReportErrors(TeamEventLog);

                    if (errors > 0)
                    {
                        richTextBoxInformation.AppendText($"{errors} error(s) have been found. Please check the Event Log in the menu.\r\n\r\n");
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.AppendText($"An error has been encountered when attempting to save the file to disk. The reported error is: {ex.Message}\r\n");
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered: {ex.Message}"));
                }
            }
        }

        private void manageJsonExportRulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcJson);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void backgroundWorkerEventLog_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            var localEventLog = TeamEventLog;

            // Handle multi-threading
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                _alertEventLog.SetTextLogging("Event Log.\r\n\r\n");

                try
                {
                    foreach (var individualEvent in localEventLog)
                    {
                        _alertEventLog.SetTextLogging($"{individualEvent.eventTime} - {(EventTypes) individualEvent.eventCode}: {individualEvent.eventDescription}\r\n");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($@"An issue occurred displaying the event log. The error message is: {ex.Message}", @"An issue has occurred", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void backgroundWorkerEventLog_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _alertEventLog.Message = "In progress, please wait... " + e.ProgressPercentage + "%";
            _alertEventLog.ProgressValue = e.ProgressPercentage;
        }

        private void backgroundWorkerEventLog_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // Do nothing
            }
            else if (e.Error != null)
            {
                // Do nothing
            }
            else
            {
                // Do nothing
            }
        }

        private void displayEventLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerEventLog.IsBusy != true)
            {
                // create a new instance of the alert form
                _alertEventLog = new Form_Alert();
                _alertEventLog.Text = @"Event Log";
                _alertEventLog.ShowLogButton(false);
                _alertEventLog.ShowCancelButton(false);
                _alertEventLog.ShowProgressBar(false);
                _alertEventLog.ShowProgressLabel(false);
                _alertEventLog.Show();

                // Start the asynchronous operation.
                backgroundWorkerEventLog.RunWorkerAsync();
            }
        }

        class TeamDataItemMappingRow
        {
            internal string sourceDataObjectName { get; set; }
            internal string sourceDataObjectConnectionId { get; set; }
            internal string sourceDataItemName { get; set; }

            internal string targetDataObjectName { get; set; }
            internal string targetDataObjectConnectionId { get; set; }
            internal string targetDataItemName { get; set; }
        }

        private void validateMetadataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();

            if (BindingSourcePhysicalModel.Count == 0)
            {
                richTextBoxInformation.Text += @"There is no physical model metadata available. Validations cannot be done without a physical model snapshot.";
            }
            else
            {
                if (backgroundWorkerValidationOnly.IsBusy) return;

                _alertValidation = new Form_Alert();

                _alertValidation.Canceled += buttonCancelParse_Click;
                _alertValidation.Show();
                _alertValidation.SetFormName("Validating the design metadata");
                _alertValidation.ShowLogButton(false);
                _alertValidation.ShowCancelButton(false);

                backgroundWorkerValidationOnly.RunWorkerAsync();
            }
        }

        private void AutoMapDataItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControlDataMappings.SelectedTab = tabPageDataItemMapping;
            
            var dataTableAttributeMappingChanges = ((DataTable) BindingSourceDataItemMappings.DataSource).GetChanges();

            if (dataTableAttributeMappingChanges != null && dataTableAttributeMappingChanges.Rows.Count > 0)
            {
                string localMessage = "You have unsaved edits in the Data Item (attribute mapping) grid, please save your work before running the auto map.";
                MessageBox.Show(localMessage);
                richTextBoxInformation.AppendText(localMessage);
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, localMessage));
            }
            else
            {
                // Iterate across all Data Object Mappings, to see if there are corresponding Data Item Mappings.
                foreach (DataGridViewRow dataObjectRow in _dataGridViewDataObjects.Rows)
                {
                    // Skip if the row is a new row.
                    if (dataObjectRow.IsNewRow)
                    {
                        return;
                    }

                    // Source Data Object details
                    DataObject sourceDataObject = (DataObject)dataObjectRow.Cells[(int)DataObjectMappingGridColumns.SourceDataObject].Value;

                    var sourceConnectionId = dataObjectRow.Cells[(int)DataObjectMappingGridColumns.SourceConnection].Value.ToString();
                    TeamConnection sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(sourceConnectionId, TeamConfiguration, TeamEventLog);

                    var sourceDataObjectFullyQualifiedKeyValuePair = MetadataHandling.GetFullyQualifiedDataObjectName(sourceDataObject.name, sourceConnection).FirstOrDefault();

                    // Get the source details from the database
                    string tableFilterObjectsSource = $"OBJECT_ID(N'[{sourceConnection.DatabaseServer.DatabaseName}].{sourceDataObjectFullyQualifiedKeyValuePair.Key}.{sourceDataObjectFullyQualifiedKeyValuePair.Value}')";

                    var localSourceSqlConnection = new SqlConnection {ConnectionString = sourceConnection.CreateSqlServerConnectionString(false)};
                    var localSourceQuery = TeamPhysicalModel.PhysicalModelQuery(sourceConnection.DatabaseServer.DatabaseName, tableFilterObjectsSource);

                    DataTable localSourceDatabaseDataTable = Utility.GetDataTable(ref localSourceSqlConnection, localSourceQuery);

                    if (localSourceDatabaseDataTable == null || localSourceDatabaseDataTable.Rows.Count == 0)
                    {
                        TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"Source physical model structures could not be imported."));
                        return;
                    }

                    // Target Data Object details
                    DataObject targetDataObject = (DataObject)dataObjectRow.Cells[DataObjectMappingGridColumns.TargetDataObject.ToString()].Value;

                    var targetConnectionId = dataObjectRow.Cells[(int)DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                    TeamConnection targetConnection = TeamConnection.GetTeamConnectionByConnectionId(targetConnectionId, TeamConfiguration, TeamEventLog);

                    var targetDataObjectFullyQualifiedKeyValuePair = MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObject.name, targetConnection).FirstOrDefault();

                    // Get the target details from the database
                    string tableFilterObjectsTarget = $"OBJECT_ID(N'[{targetConnection.DatabaseServer.DatabaseName}].{targetDataObjectFullyQualifiedKeyValuePair.Key}.{targetDataObjectFullyQualifiedKeyValuePair.Value}')";

                    var localTargetSqlConnection = new SqlConnection {ConnectionString = targetConnection.CreateSqlServerConnectionString(false)};
                    var localTargetQuery = TeamPhysicalModel.PhysicalModelQuery(targetConnection.DatabaseServer.DatabaseName, tableFilterObjectsTarget);

                    DataTable localTargetDatabaseDataTable = Utility.GetDataTable(ref localTargetSqlConnection, localTargetQuery);

                    if (localTargetDatabaseDataTable == null || localTargetDatabaseDataTable.Rows.Count == 0)
                    {
                        TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"Target physical model structures could not be imported."));
                        return;
                    }

                    List<TeamDataItemMappingRow> localDataItemMappings = new List<TeamDataItemMappingRow>();

                    // For each source Data Object, check if there is a matching target
                    foreach (DataRow sourceDataObjectRow in localSourceDatabaseDataTable.Rows)
                    {
                        // Do the lookup in the target data table
                        var results = from localRow in localTargetDatabaseDataTable.AsEnumerable()
                            where localRow.Field<string>("COLUMN_NAME") == sourceDataObjectRow["COLUMN_NAME"].ToString()
                            select localRow;

                        if (results.FirstOrDefault() != null)
                        {
                            // There is a match and it's not a standard attribute.

                            string[] exclusionAttribute =
                            {
                                TeamConfiguration.RecordSourceAttribute,
                                TeamConfiguration.AlternativeRecordSourceAttribute,
                                TeamConfiguration.RowIdAttribute,
                                TeamConfiguration.RecordChecksumAttribute,
                                TeamConfiguration.ChangeDataCaptureAttribute,
                                TeamConfiguration.AlternativeLoadDateTimeAttribute,
                                TeamConfiguration.LoadDateTimeAttribute,
                                TeamConfiguration.EventDateTimeAttribute,
                                TeamConfiguration.ExpiryDateTimeAttribute,
                                TeamConfiguration.EtlProcessAttribute,
                                TeamConfiguration.EtlProcessUpdateAttribute,
                                TeamConfiguration.CurrentRowAttribute,
                                TeamConfiguration.AlternativeSatelliteLoadDateTimeAttribute,
                            };

                            if (!exclusionAttribute.Contains(sourceDataObjectRow["COLUMN_NAME"].ToString()))
                            {
                                var localMapping = new TeamDataItemMappingRow
                                {
                                    sourceDataObjectName = sourceDataObject.name,
                                    sourceDataObjectConnectionId = sourceConnectionId,
                                    sourceDataItemName = sourceDataObjectRow["COLUMN_NAME"].ToString(),
                                    targetDataObjectName = targetDataObject.name,
                                    targetDataObjectConnectionId = targetConnectionId,
                                    targetDataItemName = sourceDataObjectRow["COLUMN_NAME"].ToString() // Same as source, as it's a direct match on this value.
                                };

                                localDataItemMappings.Add(localMapping);
                            }
                        }
                    }

                    // Now, for each item in the matched list check if there is a corresponding Data Item Mapping in the grid already.
                    DataTable localDataItemDataTable = (DataTable) BindingSourceDataItemMappings.DataSource;

                    foreach (var matchedDataItemMappingFromDatabase in localDataItemMappings)
                    {
                        // Do the lookup in the target data item grid
                        var results =
                            from localRow in localDataItemDataTable.AsEnumerable()
                            where
                                localRow.Field<string>(DataItemMappingGridColumns.SourceDataObject.ToString()) == matchedDataItemMappingFromDatabase.sourceDataObjectName &&
                                localRow.Field<string>(DataItemMappingGridColumns.TargetDataObject.ToString()) == matchedDataItemMappingFromDatabase.targetDataObjectName &&
                                localRow.Field<string>(DataItemMappingGridColumns.SourceDataItem.ToString()) == matchedDataItemMappingFromDatabase.sourceDataItemName &&
                                localRow.Field<string>(DataItemMappingGridColumns.TargetDataItem.ToString()) == matchedDataItemMappingFromDatabase.targetDataItemName
                            select localRow;

                        if (results.FirstOrDefault() == null)
                        {
                            // There is NO match...
                            // Add the row as Data Item Mapping in the grid.

                            DataRow newRow = localDataItemDataTable.NewRow();

                            newRow[DataItemMappingGridColumns.HashKey.ToString()] = Utility.CreateMd5(new[] {Utility.GetRandomString(100)}, "#");
                            newRow[DataItemMappingGridColumns.SourceDataObject.ToString()] = matchedDataItemMappingFromDatabase.sourceDataObjectName;
                            newRow[DataItemMappingGridColumns.SourceDataItem.ToString()] = matchedDataItemMappingFromDatabase.sourceDataItemName;
                            newRow[DataItemMappingGridColumns.TargetDataObject.ToString()] = matchedDataItemMappingFromDatabase.targetDataObjectName;
                            newRow[DataItemMappingGridColumns.TargetDataItem.ToString()] = matchedDataItemMappingFromDatabase.targetDataItemName;
                            newRow[DataItemMappingGridColumns.Notes.ToString()] = "Automatically matched";

                            localDataItemDataTable.Rows.Add(newRow);
                        }
                    }
                }
            }
        }

        private void openMetadataDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            {
                try
                {
                    Process.Start(globalParameters.MetadataPath);
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.Text = $@"An error has occurred while attempting to open the metadata directory. The error message is: {ex.Message}.";
                }
            }
        }

        /// <summary>
        /// Menu item option to display the query that produces the physical model JSON.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void generatePhysicalModelGridQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _physicalModelQuery = new Form_Alert();
            _physicalModelQuery.SetFormName("Generating a physical model grid query");
            _physicalModelQuery.ShowLogButton(false);
            _physicalModelQuery.ShowCancelButton(false);
            _physicalModelQuery.ShowProgressBar(false);
            _physicalModelQuery.ShowProgressLabel(false);
            _physicalModelQuery.Canceled += buttonCancelParse_Click;
            _physicalModelQuery.Show();

            List<string> resultQueryList = new List<string>();

            var filteredDataObjectMappingDataRows = GetFilteredDataObjectMappingDataTableRows();

            foreach (var item in checkedListBoxReverseEngineeringAreas.CheckedItems)
            {
                var localConnectionObject = (KeyValuePair<TeamConnection, string>)item;
                resultQueryList.Add(SqlStatementForDataItems(GetDistinctFilteredDataObjects(filteredDataObjectMappingDataRows), localConnectionObject.Key, true));
            }

            foreach (var query in resultQueryList)
            {
                _physicalModelQuery.SetTextLogging(query);
            }
        }

        private void backgroundWorkerReverseEngineering_DoWork(object sender, DoWorkEventArgs e)
        {
            // The temporary merge data table.
            var interimDataTable = new DataTable();

            // The full data table.
            DataTable completeDataTable = (DataTable)BindingSourcePhysicalModel.DataSource;
            var existingFilter = completeDataTable.DefaultView.RowFilter;

            foreach (var checkedItem in checkedListBoxReverseEngineeringAreas.CheckedItems)
            {
                var localConnectionObject = (KeyValuePair<TeamConnection, string>)checkedItem;
                var filteredRows = GetFilteredDataObjectMappingDataTableRows();

                var reverseEngineerResults = ReverseEngineerModelMetadata(localConnectionObject.Key, filteredRows);

                if (reverseEngineerResults != null)
                {
                    
                    interimDataTable.Merge(reverseEngineerResults);
                }

                ThreadHelper.SetText(this, richTextBoxInformation,$"\r\n - Completed {localConnectionObject.Key.ConnectionKey} at {DateTime.Now:HH:mm:ss tt}.");
            }

            // Flag as new row so it's detected by the save button.
            foreach (DataRow row in interimDataTable.Rows)
            {
                row.SetAdded();
            }

            ThreadHelper.SetText(this, richTextBoxInformation, $"\r\n Added new records completed at {DateTime.Now:HH:mm:ss tt}.");

            completeDataTable.Merge(interimDataTable);
            ThreadHelper.SetText(this, richTextBoxInformation, $"\r\n Merge of data tables completed at {DateTime.Now:HH:mm:ss tt}.");

            // De-duplication.
            DataTable distinctTable = null;

            if (completeDataTable.Rows.Count > 0)
            {
                distinctTable = completeDataTable.AsEnumerable()
                    .GroupBy(row => new
                    {
                        databaseName = row.Field<string>(PhysicalModelMappingMetadataColumns.databaseName.ToString()),
                        schemaName = row.Field<string>(PhysicalModelMappingMetadataColumns.schemaName.ToString()),
                        tableName = row.Field<string>(PhysicalModelMappingMetadataColumns.tableName.ToString()),
                        columnName = row.Field<string>(PhysicalModelMappingMetadataColumns.columnName.ToString()),
                    })
                    .Select(y => y.First())
                    .CopyToDataTable();
            }

            ThreadHelper.SetText(this, richTextBoxInformation, $"\r\n Deduplication completed completed at {DateTime.Now:HH:mm:ss tt}.");

            // Sort and display the results on the data grid.
            if (distinctTable != null)
            {
                distinctTable.DefaultView.Sort = $"[{PhysicalModelMappingMetadataColumns.databaseName}] ASC, [{PhysicalModelMappingMetadataColumns.schemaName}] ASC, [{PhysicalModelMappingMetadataColumns.tableName}] ASC, [{PhysicalModelMappingMetadataColumns.ordinalPosition}] ASC";

                // Inherit the filter. Can't apply to the binding source yet because changes will need to be saved first.

                distinctTable.DefaultView.RowFilter = existingFilter;

                _dataGridViewPhysicalModel.Invoke((Action)(() => _dataGridViewPhysicalModel.DataSource = distinctTable));
            }
            else
            {
                ThreadHelper.SetText(this, richTextBoxInformation, $"\r\n There was nothing to process, and nothing to show.");
            }
        }

        private void backgroundWorkerReverseEngineering_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                richTextBoxInformation.Text = @"Cancelled!";
            }
            else if (e.Error != null)
            {
                richTextBoxInformation.Text = $@"There was an error detected while reverse-engineering. The error is {e.Error.Message}";
            }
            else
            {
                labelResult.Text = @"Done!";
                richTextBoxInformation.Text += "\r\nThe physical model was reverse-engineered into the data grid. Don't forget to save your changes if these records should be retained.\r\n";

                // Re-enable the checked list box.
                checkedListBoxReverseEngineeringAreas.Enabled = true;

                // Apply filtering.
                ApplyDataGridViewFiltering();

                // Resize the grid.
                GridAutoLayout(_dataGridViewPhysicalModel);
            }
        }

        private void FormManageMetadata_ResizeEnd(object sender, EventArgs e)
        {
            if (Size != formSize)
            {
                GridAutoLayout();
            }
        }

        private void FormManageMetadata_ResizeBegin(object sender, EventArgs e)
        {
            formSize = Size;
        }

        private void tabControlDataMappings_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyDataGridViewFiltering();
        }

        private void checkBoxShowStaging_CheckedChanged(object sender, EventArgs e)
        {
            var inputTableMappingDataObjectMappings = (DataTable)BindingSourceDataObjectMappings.DataSource;
            var inputTableMappingDataItemMappings = (DataTable)BindingSourceDataItemMappings.DataSource;
            var inputTableMappingPhysicalModel = (DataTable)BindingSourcePhysicalModel.DataSource;

            // Everything should be shown - no filters.
            if (checkBoxShowStaging.Checked)
            {
                inputTableMappingDataObjectMappings.DefaultView.RowFilter = string.Empty;
                inputTableMappingDataItemMappings.DefaultView.RowFilter = string.Empty;
                inputTableMappingPhysicalModel.DefaultView.RowFilter = string.Empty;

                ApplyDataGridViewFiltering();
            }
            // Everything BUT staging layer objects should be shown.
            else
            {
                // The target is not a STG process and not a PSA process.
                var filterCriterionDataObjectMappings = $"[TargetDataObjectName] NOT LIKE '{TeamConfiguration.StgTablePrefixValue}%' AND [TargetDataObjectName] NOT LIKE '{TeamConfiguration.PsaTablePrefixValue}%'";
                var filterCriterionDataItemMappings = $"[TargetDataObject] NOT LIKE '{TeamConfiguration.StgTablePrefixValue}%' AND [TargetDataObject] NOT LIKE '{TeamConfiguration.PsaTablePrefixValue}%'";
                var filterCriterionPhysicalModel = $"[{PhysicalModelMappingMetadataColumns.tableName}] NOT LIKE '{TeamConfiguration.StgTablePrefixValue}%' AND [{PhysicalModelMappingMetadataColumns.tableName}] NOT LIKE '{TeamConfiguration.PsaTablePrefixValue}%'";

                inputTableMappingDataObjectMappings.DefaultView.RowFilter = filterCriterionDataObjectMappings;
                inputTableMappingDataItemMappings.DefaultView.RowFilter = filterCriterionDataItemMappings;
                inputTableMappingPhysicalModel.DefaultView.RowFilter = filterCriterionPhysicalModel;

                ApplyDataGridViewFiltering();
            }
        }

        private void checkedListBoxReverseEngineeringAreas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (backgroundWorkerReverseEngineering.IsBusy)
            {
                MessageBox.Show(@"The reverse engineer process is running, please wait for this to be completed before changing any settings.", @"Process is running", MessageBoxButtons.OK);
            }
        }
    }
}