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
        private Form_Alert _alert;
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
        private TeamDataObjectMappingsFileCombinations TeamDataObjectMappingFileCombinations;

        // Preparing the Data Table to bind to something.
        private readonly BindingSource BindingSourceDataObjectMappings = new BindingSource();
        private readonly BindingSource BindingSourceDataItemMappings = new BindingSource();
        private readonly BindingSource BindingSourcePhysicalModel = new BindingSource();

        public FormManageMetadata()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Default constructor
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

            // Default setting and start setting of counters etc.
            MetadataValidations.ValidationIssues = 0;
            MetadataValidations.ValidationRunning = false;

            labelHubCount.Text = @"0 Core Business Concepts";
            labelSatCount.Text = @"0 Context entities";
            labelLnkCount.Text = @"0 Relationships";
            labelLsatCount.Text = @"0 Relationship context entities";

            //  Load the grids from the repository
            richTextBoxInformation.Clear();

            // Load the data grids
            // Get the JSON files and load these into memory.
            TeamDataObjectMappingFileCombinations = new TeamDataObjectMappingsFileCombinations(GlobalParameters.MetadataPath);
            TeamDataObjectMappingFileCombinations.GetMetadata();

            PopulateDataObjectMappingGrid(TeamDataObjectMappingFileCombinations);
            PopulateDataItemMappingGrid();
            PopulatePhysicalModelGrid();

            // Inform the user
            string userFeedback = $"The metadata has been loaded.";
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
                var validationFile = GlobalParameters.ConfigurationPath + GlobalParameters.ValidationFileName + '_' + GlobalParameters.ActiveEnvironmentKey + GlobalParameters.FileExtension;

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
                var jsonConfigurationFile = GlobalParameters.ConfigurationPath + GlobalParameters.JsonExportConfigurationFileName + '_' + GlobalParameters.ActiveEnvironmentKey + GlobalParameters.FileExtension;

                // If the config file does not exist yet, create it.
                if (!File.Exists(jsonConfigurationFile))
                {
                    JsonExportSetting.CreateDummyJsonConfigurationFile(jsonConfigurationFile);
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

            // Load the checkboxes for the reverse-engineering tab
            foreach (var connection in TeamConfiguration.ConnectionDictionary)
            {
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
        private void PopulateDataObjectMappingGrid(TeamDataObjectMappingsFileCombinations teamDataObjectMappingsFileCombinations)
        {
            // Parse the JSON files into a data table that supports the grid view.
            var teamDataObjectMappings = new TeamDataObjectMappings(teamDataObjectMappingsFileCombinations);

            // Merge events
            TeamEventLog.AddRange(teamDataObjectMappingsFileCombinations.EventLog);

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

            //AttributeMapping.SetDataTableColumns();
            //AttributeMapping.SetDataTableSorting();

            BindingSourceDataItemMappings.DataSource = AttributeMapping.DataTable;

            // Set the column header names.
            _dataGridViewDataItems.DataSource = BindingSourceDataItemMappings;

            _dataGridViewDataItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dataGridViewDataItems.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;

            //richTextBoxInformation.AppendText($"The file {TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName()} was loaded.\r\n");

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

        private void GridAutoLayout()
        {
            GridAutoLayout(_dataGridViewDataObjects);
            GridAutoLayout(_dataGridViewDataItems);
            GridAutoLayout(_dataGridViewPhysicalModel);
        }

        private void GridAutoLayout(DataGridView dataGridView)
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
                _myValidationForm = new FormManageValidation(this);
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

                    _myValidationForm = new FormManageValidation(this);
                    _myValidationForm.Show();
                    Application.Run();
                }
                else
                {
                    // No invoke required - same thread
                    _myValidationForm.FormClosed += CloseValidationForm;

                    _myValidationForm = new FormManageValidation(this);
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
                _myJsonForm = new FormJsonConfiguration(this);
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

                    _myJsonForm = new FormJsonConfiguration(this);
                    _myJsonForm.Show();
                    Application.Run();
                }
                else
                {
                    // No invoke required - same thread
                    _myJsonForm.FormClosed += CloseJsonForm;

                    _myJsonForm = new FormJsonConfiguration(this);
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
        private void buttonSaveMetadata_Click(object sender, EventArgs e)
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
                try
                {
                    SaveDataObjectMappingJson(dataTableTableMappingChanges);
                }
                catch (Exception exception)
                {
                    richTextBoxInformation.Text += $@"The Data Object Mapping metadata wasn't saved. The reported error is: {exception.Message}.";
                }

                try
                {
                    SaveAttributeMappingMetadata(dataTableAttributeMappingChanges);
                }
                catch (Exception exception)
                {
                    richTextBoxInformation.Text += $@"The Data Item Mapping metadata wasn't saved. The reported error is: {exception.Message}.";
                }

                try
                {
                    SaveModelPhysicalModelMetadata(dataTablePhysicalModelChanges);
                }
                catch (Exception exception)
                {
                    richTextBoxInformation.Text += $@"The Physical Model metadata wasn't saved. The reported error is: {exception.Message}.";
                }

                // Get the JSON files and load these into memory.
                TeamDataObjectMappingFileCombinations = new TeamDataObjectMappingsFileCombinations(GlobalParameters.MetadataPath);
                TeamDataObjectMappingFileCombinations.GetMetadata();

                //Load the grids from the repository after being updated.This resets everything.
                PopulateDataObjectMappingGrid(TeamDataObjectMappingFileCombinations);
                PopulateDataItemMappingGrid();
                PopulatePhysicalModelGrid();
            }
            else
            {
                richTextBoxInformation.Text += @"There is no metadata to save!";
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
                File.WriteAllText(targetDataObject.name.GetMetadataFilePath(), output);

                ((DataTable)BindingSourceDataObjectMappings.DataSource).AcceptChanges();

                ThreadHelper.SetText(this, richTextBoxInformation, $"The Data Object Mapping for '{targetDataObject.name}' has been saved.\r\n");
            }
            else
            {
                var fileToDelete = targetDataObject.name.GetMetadataFilePath();
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
                File.WriteAllText(targetDataObject.name.GetMetadataFilePath(), output);

                ((DataTable)BindingSourceDataObjectMappings.DataSource).AcceptChanges();

                richTextBoxInformation.Text += $"The Data Object Mapping for '{targetDataObject.name}' has been saved.\r\n";
            }
            else
            {
                var fileToDelete = targetDataObjectName.GetMetadataFilePath();
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
                TeamJsonHandling.RemoveExistingJsonFile(GlobalParameters.JsonModelMetadataFileName + ".json");
                TeamJsonHandling.CreatePlaceholderJsonFile(GlobalParameters.JsonModelMetadataFileName);
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
                        var ordinalPosition = "";
                        var primaryKeyIndicator = "";
                        var multiActiveIndicator = "";

                        if (row[PhysicalModelMappingMetadataColumns.Database_Name.ToString()] != DBNull.Value)
                        {
                            databaseName = (string) row[PhysicalModelMappingMetadataColumns.Database_Name.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.Schema_Name.ToString()] != DBNull.Value)
                        {
                            schemaName = (string) row[PhysicalModelMappingMetadataColumns.Schema_Name.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.Table_Name.ToString()] != DBNull.Value)
                        {
                            tableName = (string) row[PhysicalModelMappingMetadataColumns.Table_Name.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.Column_Name.ToString()] != DBNull.Value)
                        {
                            columnName = (string) row[PhysicalModelMappingMetadataColumns.Column_Name.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.Data_Type.ToString()] != DBNull.Value)
                        {
                            dataType = (string) row[PhysicalModelMappingMetadataColumns.Data_Type.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.Character_Length.ToString()] != DBNull.Value)
                        {
                            characterLength = (string) row[PhysicalModelMappingMetadataColumns.Character_Length.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.Numeric_Precision.ToString()] != DBNull.Value)
                        {
                            numericPrecision = (string) row[PhysicalModelMappingMetadataColumns.Numeric_Precision.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.Numeric_Scale.ToString()] != DBNull.Value)
                        {
                            numericScale = (string) row[PhysicalModelMappingMetadataColumns.Numeric_Scale.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.Ordinal_Position.ToString()] != DBNull.Value)
                        {
                            ordinalPosition = (string) row[PhysicalModelMappingMetadataColumns.Ordinal_Position.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.Primary_Key_Indicator.ToString()] != DBNull.Value)
                        {
                            primaryKeyIndicator = (string) row[PhysicalModelMappingMetadataColumns.Primary_Key_Indicator.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.Multi_Active_Indicator.ToString()] != DBNull.Value)
                        {
                            multiActiveIndicator = (string) row[PhysicalModelMappingMetadataColumns.Multi_Active_Indicator.ToString()];
                        }

                        try
                        {
                            var databaseNameOld = (string)row[PhysicalModelMappingMetadataColumns.Database_Name.ToString(), DataRowVersion.Original];
                            var schemaNameOld = (string)row[PhysicalModelMappingMetadataColumns.Schema_Name.ToString(), DataRowVersion.Original];
                            var tableNameOld = (string)row[PhysicalModelMappingMetadataColumns.Table_Name.ToString(), DataRowVersion.Original];
                            var columnNameOld = (string)row[PhysicalModelMappingMetadataColumns.Column_Name.ToString(), DataRowVersion.Original];

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
                        string ordinalPosition = "0";
                        string primaryKeyIndicator = "";
                        string multiActiveIndicator = "";

                        if (row[(int) PhysicalModelMappingMetadataColumns.Database_Name] != DBNull.Value)
                        {
                            databaseName = (string) row[(int) PhysicalModelMappingMetadataColumns.Database_Name];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.Schema_Name] != DBNull.Value)
                        {
                            schemaName = (string) row[(int) PhysicalModelMappingMetadataColumns.Schema_Name];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.Table_Name] != DBNull.Value)
                        {
                            tableName = (string) row[(int) PhysicalModelMappingMetadataColumns.Table_Name];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.Column_Name] != DBNull.Value)
                        {
                            columnName = (string) row[(int) PhysicalModelMappingMetadataColumns.Column_Name];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.Data_Type] != DBNull.Value)
                        {
                            dataType = (string) row[(int) PhysicalModelMappingMetadataColumns.Data_Type];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.Character_Length] != DBNull.Value)
                        {
                            characterLength = (string) row[(int) PhysicalModelMappingMetadataColumns.Character_Length];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.Numeric_Precision] != DBNull.Value)
                        {
                            numericPrecision = (string) row[(int) PhysicalModelMappingMetadataColumns.Numeric_Precision];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.Numeric_Scale] != DBNull.Value)
                        {
                            numericScale = (string) row[(int) PhysicalModelMappingMetadataColumns.Numeric_Scale];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.Ordinal_Position] != DBNull.Value)
                        {
                            ordinalPosition = (string) row[(int) PhysicalModelMappingMetadataColumns.Ordinal_Position];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.Primary_Key_Indicator] != DBNull.Value)
                        {
                            primaryKeyIndicator = (string) row[(int) PhysicalModelMappingMetadataColumns.Primary_Key_Indicator];
                        }

                        if (row[(int) PhysicalModelMappingMetadataColumns.Multi_Active_Indicator] != DBNull.Value)
                        {
                            multiActiveIndicator = (string) row[(int) PhysicalModelMappingMetadataColumns.Multi_Active_Indicator];
                        }

                        try
                        {
                            //Checks if a matching JSON segment already exists.
                            var jsonSegmentForDelete = jsonArray.FirstOrDefault(obj => obj.databaseName == databaseName && obj.schemaName == schemaName && obj.tableName == tableName && obj.columnName == columnName);

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
                            richTextBoxInformation.Text += "There were issues inserting the JSON segment / record.\r\n" + ex.Message;
                        }
                    }

                    #endregion

                    #region Deleted rows

                    //Deleted rows
                    if ((row.RowState & DataRowState.Deleted) != 0)
                    {
                        var databaseName = row[PhysicalModelMappingMetadataColumns.Database_Name.ToString(), DataRowVersion.Original].ToString();
                        var schemaName = row[PhysicalModelMappingMetadataColumns.Schema_Name.ToString(), DataRowVersion.Original].ToString();
                        var tableName = row[PhysicalModelMappingMetadataColumns.Table_Name.ToString(), DataRowVersion.Original].ToString();
                        var columnName = row[PhysicalModelMappingMetadataColumns.Column_Name.ToString(), DataRowVersion.Original].ToString();

                        try
                        {
                            //string inputFileName = TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName();
                            //var jsonArray = JsonConvert.DeserializeObject<PhysicalModelMetadataJson[]>(File.ReadAllText(inputFileName)).ToList();

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

                            //string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                            //string outputFileName = TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName();
                            //File.WriteAllText(outputFileName, output);

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

        private void SaveAttributeMappingMetadata(DataTable dataTableChanges)
        {
            if (TeamJsonHandling.JsonFileConfiguration.newFileAttributeMapping == "true")
            {
                TeamJsonHandling.RemoveExistingJsonFile(GlobalParameters.JsonAttributeMappingFileName + ".json");
                TeamJsonHandling.CreatePlaceholderJsonFile(GlobalParameters.JsonAttributeMappingFileName);
                TeamJsonHandling.JsonFileConfiguration.newFileAttributeMapping = "false";
            }

            //Check if there are any changes made at all.
            if (dataTableChanges != null && (dataTableChanges.Rows.Count > 0))
            {
                // Loop through the changes captured in the data table
                foreach (DataRow row in dataTableChanges.Rows)
                {
                    #region Updates in Attribute Mapping

                    // Updates
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
                            AttributeMappingJson[] jsonArray = JsonConvert.DeserializeObject<AttributeMappingJson[]>(File.ReadAllText(inputFileName));

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
                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += $"There were issues applying the JSON update.\r\n{ex.Message}";
                        }
                    }

                    #endregion

                    #region Inserts in Attribute Mapping

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
                            AttributeMappingJson[] jsonArray = JsonConvert.DeserializeObject<AttributeMappingJson[]>(File.ReadAllText(inputFileName));

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

                            //Making sure the hash key value is added to the data table as well
                            row[(int)DataItemMappingGridColumns.HashKey] = hashKey;

                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += "There were issues inserting the JSON segment / record.\r\n" + ex.Message;
                        }
                    }

                    #endregion

                    #region Deletes in Attribute Mapping

                    // Deletes
                    if ((row.RowState & DataRowState.Deleted) != 0)
                    {
                        var hashKey = row[DataItemMappingGridColumns.HashKey.ToString(), DataRowVersion.Original].ToString();

                        try
                        {
                            string inputFileName = TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                            var jsonArray = JsonConvert.DeserializeObject<AttributeMappingJson[]>(File.ReadAllText(inputFileName)).ToList();

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

        /// <summary>
        /// Returns the source, and target connection for a given input source and target mapping.
        /// Item 1 is the enabled flag, item 2 is the source, item 3 the source connection, item 4 the target and Item 5 is the target connection.
        /// </summary>
        /// <param name="tableMappingDataTable"></param>
        /// <param name="sourceTable"></param>
        /// <param name="targetTable"></param>
        /// <returns></returns>
        private static Tuple<string, string, TeamConnection, string, TeamConnection> GetDataObjectMappingFromDataItemMapping(DataTable tableMappingDataTable, string sourceTable, string targetTable)
        {
            // Default return value
            Tuple<string, string, TeamConnection, string, TeamConnection> returnTuple = new Tuple<string, string, TeamConnection, string, TeamConnection>
                (
                    "False",
                    sourceTable,
                    null,
                    targetTable,
                    null
                );

            // Find the corresponding row in the Data Object Mapping grid
            DataRow[] DataObjectMappings = tableMappingDataTable.Select("[" + DataObjectMappingGridColumns.SourceDataObjectName +
                                                                        "] = '" + sourceTable + "' AND" +
                                                                        "[" + DataObjectMappingGridColumns.TargetDataObjectName +
                                                                        "] = '" + targetTable + "'");

            if (DataObjectMappings is null || DataObjectMappings.Length == 0)
            {
                // There is no matching row found in the Data Object Mapping grid. Validation should pick this up!
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"While processing the Data Item mappings, no matching Data Object mapping was found."));

            }
            else if (DataObjectMappings.Length > 1)
            {
                // There are too many entries! There should be only a single mapping from source to target
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"While processing the Data Item mappings, to many (more than 1) matching Data Object mapping were found."));
            }
            else
            {
                var connectionInternalIdSource = DataObjectMappings[0][DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                var connectionInternalIdTarget = DataObjectMappings[0][DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                TeamConnection sourceConnection = GetTeamConnectionByConnectionId(connectionInternalIdSource);
                TeamConnection targetConnection = GetTeamConnectionByConnectionId(connectionInternalIdTarget);

                // Set the right values
                returnTuple = new Tuple<string, string, TeamConnection, string, TeamConnection>
                (
                    DataObjectMappings[0][DataObjectMappingGridColumns.Enabled.ToString()].ToString(),
                    sourceTable,
                    sourceConnection,
                    targetTable,
                    targetConnection
                );

            }

            return returnTuple;
        }

        # region Background worker
        private void ButtonActivate_Click(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();

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

            #region Validation

            // The first thing to happen is to check if the validation needs to be run (and started if the answer to this is yes)
            if (checkBoxValidation.Checked && activationContinue)
            {
                if (BindingSourcePhysicalModel.Count == 0)
                {
                    richTextBoxInformation.Text += "There is no physical model metadata available, please make sure the physical model grid contains data.\r\n ";
                    activationContinue = false;
                }
                else
                {
                    if (backgroundWorkerValidationOnly.IsBusy) return;
                    // create a new instance of the alert form
                    _alertValidation = new Form_Alert();
                    _alertValidation.SetFormName("Validating the metadata");
                    _alertValidation.ShowLogButton(false);
                    _alertValidation.ShowCancelButton(false);
                    _alertValidation.Canceled += buttonCancel_Click;
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

            // After validation finishes, the activation thread / process should start.
            // Only if the validation is enabled AND there are no issues identified in earlier validation checks.

            #region Activation

            if (!checkBoxValidation.Checked || (checkBoxValidation.Checked && MetadataValidations.ValidationIssues == 0) && activationContinue)
            {
                if (backgroundWorkerMetadata.IsBusy) return;
                // create a new instance of the alert form
                _alert = new Form_Alert();
                _alert.Canceled += buttonCancel_Click;
                _alert.ShowLogButton(false);
                _alert.ShowCancelButton(false);
                _alert.Show();
                // Start the asynchronous operation.
                backgroundWorkerMetadata.RunWorkerAsync();
            }
            else
            {
                richTextBoxInformation.AppendText("Validation found issues which should be investigated. If you would like to continue, please uncheck the validation and parse the metadata again.\r\n");
            }

            #endregion
        }

        /// <summary>
        /// This event handler cancels the background worker, fired from Cancel button in AlertForm.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerMetadata.WorkerSupportsCancellation)
            {
                // Cancel the asynchronous operation.
                backgroundWorkerMetadata.CancelAsync();
                // Close the AlertForm
                _alertValidation.Close();
            }
        }

        // This event handler deals with the results of the background operation.
        private void backgroundWorkerMetadata_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                // Reload the data grids.
                // Get the JSON files and load these into memory.
                TeamDataObjectMappingFileCombinations = new TeamDataObjectMappingsFileCombinations(GlobalParameters.MetadataPath);
                TeamDataObjectMappingFileCombinations.GetMetadata();

                //Load the grids from the repository after being updated.This resets everything.
                PopulateDataObjectMappingGrid(TeamDataObjectMappingFileCombinations);
                PopulateDataItemMappingGrid();
                PopulatePhysicalModelGrid();

                labelResult.Text = @"Done!";
                richTextBoxInformation.Text = "The metadata was processed successfully!\r\n";
            }
        }

        // This event handler updates the progress.
        private void backgroundWorkerMetadata_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Show the progress in main form
            labelResult.Text = (e.ProgressPercentage + "%");

            // Pass the progress to AlertForm label and progress bar
            _alert.Message = "In progress, please wait... " + e.ProgressPercentage + "%";
            _alert.ProgressValue = e.ProgressPercentage;
        }

        # endregion

        /// <summary>
        /// The background worker where the heavy lift work is done for the activation process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerMetadata_DoWorkMetadataActivation(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            LogMetadataEvent("Starting an end-to-end parse of all metadata.\r\n", EventTypes.Information);

            List<string> targetNameList = new List<string>();

            int counter = 0;
            foreach (DataGridViewRow dataObjectMappingGridViewRow in _dataGridViewDataObjects.Rows)
            {
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

                            LogMetadataEvent($"  --> Saved as '{targetDataObject.name.GetMetadataFilePath()}'.", EventTypes.Information);

                            targetNameList.Add(targetDataObjectName);
                        }
                        catch (JsonReaderException ex)
                        {
                            LogMetadataEvent($"There were issues updating the JSON. The error message is {ex.Message}.", EventTypes.Error);
                        }

                        // Normalize all values in array against a 0-100 scale to support the progress bar relative to the number of commands to execute.
                        var normalizedValue = 1 + (counter - 0) * (100 - 1) / (_dataGridViewDataObjects.Rows.Count - 0);
                        worker?.ReportProgress(normalizedValue);
                        counter++;
                    }
                }
            }
            worker?.ReportProgress(100);
        }
        
        private void LogMetadataEvent(string eventMessage, EventTypes eventType)
        {
            TeamEventLog.Add(Event.CreateNewEvent(eventType, eventMessage));
            _alert.SetTextLogging("\r\n" + eventMessage);
        }

        private void FormManageMetadata_SizeChanged(object sender, EventArgs e)
        {
            GridAutoLayout();
        }

        public DateTime ActivationMetadata()
        {
            DateTime mostRecentActivationDateTime = DateTime.MinValue;

            var connOmd = new SqlConnection
            {
                ConnectionString = TeamConfiguration.MetadataConnection.CreateSqlServerConnectionString(false)
            };

            var sqlStatementForActivationMetadata = new StringBuilder();
            sqlStatementForActivationMetadata.AppendLine("SELECT [VERSION_NAME], MAX([ACTIVATION_DATETIME]) AS [ACTIVATION_DATETIME]");
            sqlStatementForActivationMetadata.AppendLine("FROM [dbo].[MD_MODEL_METADATA]");
            sqlStatementForActivationMetadata.AppendLine("GROUP BY [VERSION_NAME]");

            var activationMetadata = Utility.GetDataTable(ref connOmd, sqlStatementForActivationMetadata.ToString());

            if (activationMetadata != null)
            {
                foreach (DataRow row in activationMetadata.Rows)
                {
                    mostRecentActivationDateTime = (DateTime) row["ACTIVATION_DATETIME"];
                }
            }

            return mostRecentActivationDateTime;
        }


        private void saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime activationDateTime = ActivationMetadata();

            if (activationDateTime == DateTime.MinValue)
            {
                richTextBoxInformation.Text = "The metadata was not activated, so the graph is constructed only from the raw mappings.";
            }
            else
            {
                richTextBoxInformation.Text =
                    $"DGML will be generated following the most recent activation metadata, as per ({activationDateTime}).";
            }

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
                        var sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionId);
                        KeyValuePair<string, string> fullyQualifiedObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(sourceNode, sourceConnection).FirstOrDefault();


                        string targetNode = row.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value.ToString();
                        var targetConnectionId = row.Cells[(int)DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                        var targetConnection = GetTeamConnectionByConnectionId(targetConnectionId);
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
                        var sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionId);
                        KeyValuePair<string, string> fullyQualifiedObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(sourceNode, sourceConnection).FirstOrDefault();

                        string targetNode = row.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value.ToString();
                        var targetConnectionId = row.Cells[(int)DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                        var targetConnection = GetTeamConnectionByConnectionId(targetConnectionId);
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
        private void ReverseEngineerMetadataButtonClick(object sender, EventArgs e)
        {
            // Select the physical model grid view.
            tabControlDataMappings.SelectedTab = tabPagePhysicalModel;

            richTextBoxInformation.Clear();
            richTextBoxInformation.Text = @"Commencing reverse-engineering the model metadata from the database. This may take a few minutes depending on the complexity of the database.";

            if (backgroundWorkerValidationOnly.IsBusy) 
                return;

            backgroundWorkerReverseEngineering.RunWorkerAsync();
        }
        
        /// <summary>
        ///   Connect to a given database and return the data dictionary (catalog) information in the data grid.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="databaseName"></param>
        private DataTable ReverseEngineerModelMetadata(SqlConnection conn, string databaseName)
        {
            try
            {
                conn.Open();
            }
            catch (Exception exception)
            {
                richTextBoxInformation.Text += $@"An error has occurred uploading the model for the new version because the database could not be connected to. The error message is: {exception.Message}.";
            }

            var sqlStatementForDataItems = SqlStatementForDataItems(databaseName);

            var reverseEngineerResults = Utility.GetDataTable(ref conn, sqlStatementForDataItems);
            conn.Close();

            return reverseEngineerResults;
        }

        private string SqlStatementForDataItems(string databaseName, bool isJson = false)
        {
            // Get everything as local variables to reduce multi-threading issues
            var effectiveDateTimeAttribute =
                TeamConfiguration.EnableAlternativeSatelliteLoadDateTimeAttribute == "True"
                    ? TeamConfiguration.AlternativeSatelliteLoadDateTimeAttribute
                    : TeamConfiguration.LoadDateTimeAttribute;

            var dwhKeyIdentifier = TeamConfiguration.DwhKeyIdentifier; //Indicates _HSH, _SK etc.
            var keyIdentifierLocation = TeamConfiguration.KeyNamingLocation;

            // Create the attribute selection statement for the array
            var sqlStatementForDataItems = new StringBuilder();

            string databaseColumnName = PhysicalModelMappingMetadataColumns.Database_Name.ToString();
            string schemaColumnName = PhysicalModelMappingMetadataColumns.Schema_Name.ToString();
            string tableColumnName = PhysicalModelMappingMetadataColumns.Table_Name.ToString();
            string columnColumnName = PhysicalModelMappingMetadataColumns.Column_Name.ToString();
            string dataTypeColumnName = PhysicalModelMappingMetadataColumns.Data_Type.ToString();
            string characterLengthColumnName = PhysicalModelMappingMetadataColumns.Character_Length.ToString();
            string numericPrecisionColumnName = PhysicalModelMappingMetadataColumns.Numeric_Precision.ToString();
            string numericScaleColumnName = PhysicalModelMappingMetadataColumns.Numeric_Scale.ToString();
            string ordinalPositionColumnName = PhysicalModelMappingMetadataColumns.Ordinal_Position.ToString();
            string primaryKeyColumnName = PhysicalModelMappingMetadataColumns.Primary_Key_Indicator.ToString();
            string multiActiveKeyColumnName = PhysicalModelMappingMetadataColumns.Multi_Active_Indicator.ToString();

            if (isJson)
            {
                databaseColumnName = "databaseName";
                schemaColumnName = "schemaName";
                tableColumnName = "tableName";
                columnColumnName = "columnName";
                dataTypeColumnName = "dataType";
                characterLengthColumnName = "characterLength";
                numericPrecisionColumnName = "numericPrecision";
                numericScaleColumnName = "numericScale";
                ordinalPositionColumnName = "ordinalPosition";
                primaryKeyColumnName = "primaryKeyIndicator";
                multiActiveKeyColumnName = "multiActiveIndicator";
            }


            sqlStatementForDataItems.AppendLine("SELECT ");

            sqlStatementForDataItems.AppendLine($"  DB_NAME(DB_ID('{databaseName}')) AS [{databaseColumnName}],");
            sqlStatementForDataItems.AppendLine($"  OBJECT_SCHEMA_NAME(main.OBJECT_ID) AS [{schemaColumnName}],");
            sqlStatementForDataItems.AppendLine($"  OBJECT_NAME(main.OBJECT_ID) AS [{tableColumnName}], ");
            sqlStatementForDataItems.AppendLine($"  main.[name] AS [{columnColumnName}], ");
            sqlStatementForDataItems.AppendLine($"  t.[name] AS [{dataTypeColumnName}], ");
            sqlStatementForDataItems.AppendLine("  CAST(COALESCE(");
            sqlStatementForDataItems.AppendLine("    CASE WHEN UPPER(t.[name]) = 'NVARCHAR' THEN main.[max_length]/2"); //Exception for unicode
            sqlStatementForDataItems.AppendLine("    ELSE main.[max_length]");
            sqlStatementForDataItems.AppendLine("    END");
            sqlStatementForDataItems.AppendLine($"     ,0) AS VARCHAR(100)) AS [{characterLengthColumnName}],");
            sqlStatementForDataItems.AppendLine($"  CAST(COALESCE(main.[precision],0) AS VARCHAR(100)) AS [{numericPrecisionColumnName}], ");
            sqlStatementForDataItems.AppendLine($"  CAST(COALESCE(main.[scale], 0) AS VARCHAR(100)) AS [{numericScaleColumnName}], ");
            sqlStatementForDataItems.AppendLine($"  CAST(main.[column_id] AS VARCHAR(100)) AS [{ordinalPositionColumnName}], ");
            sqlStatementForDataItems.AppendLine("  CASE ");
            sqlStatementForDataItems.AppendLine("    WHEN keysub.COLUMN_NAME IS NULL ");
            sqlStatementForDataItems.AppendLine("    THEN 'N' ");
            sqlStatementForDataItems.AppendLine("    ELSE 'Y' ");
            sqlStatementForDataItems.AppendLine($"  END AS {primaryKeyColumnName}, ");
            sqlStatementForDataItems.AppendLine("  CASE ");
            sqlStatementForDataItems.AppendLine("    WHEN ma.COLUMN_NAME IS NULL ");
            sqlStatementForDataItems.AppendLine("    THEN 'N' ");
            sqlStatementForDataItems.AppendLine("    ELSE 'Y' ");
            sqlStatementForDataItems.AppendLine($"  END AS {multiActiveKeyColumnName} ");

            sqlStatementForDataItems.AppendLine("FROM [" + databaseName + "].sys.columns main");
            sqlStatementForDataItems.AppendLine("JOIN sys.types t ON main.user_type_id=t.user_type_id");
            sqlStatementForDataItems.AppendLine("-- Primary Key");
            sqlStatementForDataItems.AppendLine("LEFT OUTER JOIN (");
            sqlStatementForDataItems.AppendLine("	SELECT ");
            sqlStatementForDataItems.AppendLine("	  sc.name AS TABLE_NAME,");
            sqlStatementForDataItems.AppendLine("	  C.name AS COLUMN_NAME");
            sqlStatementForDataItems.AppendLine("	FROM [" + databaseName + "].sys.index_columns A");
            sqlStatementForDataItems.AppendLine("	JOIN [" + databaseName + "].sys.indexes B");
            sqlStatementForDataItems.AppendLine("	ON A.OBJECT_ID=B.OBJECT_ID AND A.index_id=B.index_id");
            sqlStatementForDataItems.AppendLine("	JOIN [" + databaseName + "].sys.columns C");
            sqlStatementForDataItems.AppendLine("	ON A.column_id=C.column_id AND A.OBJECT_ID=C.OBJECT_ID");
            sqlStatementForDataItems.AppendLine("	JOIN [" + databaseName + "].sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
            sqlStatementForDataItems.AppendLine("	WHERE is_primary_key=1 ");
            sqlStatementForDataItems.AppendLine(") keysub");
            sqlStatementForDataItems.AppendLine("   ON OBJECT_NAME(main.OBJECT_ID) = keysub.TABLE_NAME");
            sqlStatementForDataItems.AppendLine("  AND main.[name] = keysub.COLUMN_NAME");

            //Multi-active
            sqlStatementForDataItems.AppendLine("-- Multi-Active");
            sqlStatementForDataItems.AppendLine("LEFT OUTER JOIN (");
            sqlStatementForDataItems.AppendLine("	SELECT ");
            sqlStatementForDataItems.AppendLine("		sc.name AS TABLE_NAME,");
            sqlStatementForDataItems.AppendLine("		C.name AS COLUMN_NAME");
            sqlStatementForDataItems.AppendLine("	FROM [" + databaseName + "].sys.index_columns A");
            sqlStatementForDataItems.AppendLine("	JOIN [" + databaseName + "].sys.indexes B");
            sqlStatementForDataItems.AppendLine("	ON A.OBJECT_ID=B.OBJECT_ID AND A.index_id=B.index_id");
            sqlStatementForDataItems.AppendLine("	JOIN [" + databaseName + "].sys.columns C");
            sqlStatementForDataItems.AppendLine("	ON A.column_id=C.column_id AND A.OBJECT_ID=C.OBJECT_ID");
            sqlStatementForDataItems.AppendLine("	JOIN [" + databaseName + "].sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
            sqlStatementForDataItems.AppendLine("	WHERE is_primary_key=1");
            sqlStatementForDataItems.AppendLine("	AND C.name NOT IN ('" + effectiveDateTimeAttribute + "')");

            if (keyIdentifierLocation == "Prefix")
            {
                sqlStatementForDataItems.AppendLine("	AND C.name NOT LIKE '" + dwhKeyIdentifier + "_%'");
            }
            else
            {
                sqlStatementForDataItems.AppendLine("	AND C.name NOT LIKE '%_" + dwhKeyIdentifier + "'");
            }

            sqlStatementForDataItems.AppendLine("	) ma");
            sqlStatementForDataItems.AppendLine("	ON OBJECT_NAME(main.OBJECT_ID) = ma.TABLE_NAME");
            sqlStatementForDataItems.AppendLine("	AND main.[name] = ma.COLUMN_NAME");
            sqlStatementForDataItems.AppendLine("WHERE 1=1");

            sqlStatementForDataItems.AppendLine("  AND (");

            var filterList = new List<Tuple<string, TeamConnection>>();

            foreach (DataRow row in ((DataTable)BindingSourceDataObjectMappings.DataSource).Rows)
            {
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                string localInternalConnectionIdSource = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                TeamConnection localConnectionSource = GetTeamConnectionByConnectionId(localInternalConnectionIdSource);

                string localInternalConnectionIdTarget = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                TeamConnection localConnectionTarget = GetTeamConnectionByConnectionId(localInternalConnectionIdTarget);

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
                sqlStatementForDataItems.AppendLine("  (OBJECT_NAME(main.OBJECT_ID) = '" + fullyQualifiedName.Value + "' AND OBJECT_SCHEMA_NAME(main.OBJECT_ID) = '" + fullyQualifiedName.Key + "')");
                sqlStatementForDataItems.AppendLine("  OR");
            }

            sqlStatementForDataItems.Remove(sqlStatementForDataItems.Length - 6, 6);
            sqlStatementForDataItems.AppendLine();
            sqlStatementForDataItems.AppendLine("  )");
            sqlStatementForDataItems.AppendLine("ORDER BY main.column_id");

            if (isJson)
            {
                sqlStatementForDataItems.AppendLine("FOR JSON PATH");
            }

            return sqlStatementForDataItems.ToString();
        }

        private void TextBoxFilterCriterion_OnDelayedTextChanged(object sender, EventArgs e)
        {
            ApplyDataGridViewFiltering(textBoxFilterCriterion.Text);
        }

        private void ApplyDataGridViewFiltering(string filterCriterion)
        {
            foreach (DataGridViewRow row in _dataGridViewDataObjects.Rows)
            {
                row.Visible = true;

                if (row.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value != null)
                {
                    if (!row.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Contains(filterCriterion) && !row.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Contains(filterCriterion))
                    {
                        CurrencyManager currencyManager = (CurrencyManager)BindingContext[_dataGridViewDataObjects.DataSource];
                        currencyManager.SuspendBinding();
                        row.Visible = false;
                        currencyManager.ResumeBinding();
                    }
                }
            }

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

            foreach (DataGridViewRow row in _dataGridViewPhysicalModel.Rows)
            {
                row.Visible = true;

                if (row.Cells[(int)PhysicalModelMappingMetadataColumns.Table_Name].Value != null)
                {
                    if (!row.Cells[(int)PhysicalModelMappingMetadataColumns.Database_Name].Value.ToString().Contains(filterCriterion) &&
                        !row.Cells[(int)PhysicalModelMappingMetadataColumns.Table_Name].Value.ToString().Contains(filterCriterion) &&
                        !row.Cells[(int)PhysicalModelMappingMetadataColumns.Schema_Name].Value.ToString().Contains(filterCriterion) &&
                        !row.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString().Contains(filterCriterion))
                    {
                        CurrencyManager currencyManager = (CurrencyManager)BindingContext[_dataGridViewPhysicalModel.DataSource];
                        currencyManager.SuspendBinding();
                        row.Visible = false;
                        currencyManager.ResumeBinding();
                    }
                }
            }
        }

        /// <summary>
        ///   Run the validation checks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerValidation_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            DataTable dataTable = (DataTable)BindingSourceDataObjectMappings.DataSource;

            // Handling multi-threading
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                _alertValidation.SetTextLogging("Commencing validation on available metadata according to settings in in the validation screen.\r\n\r\n");
                MetadataValidations.ValidationIssues = 0;

                if (ValidationSetting.DataObjectExistence == "True")
                {
                    ValidateObjectExistence(dataTable);
                }

                worker?.ReportProgress(10);


                if (ValidationSetting.SourceBusinessKeyExistence == "True")
                {
                    ValidateBusinessKeyObject();
                }

                worker?.ReportProgress(20);


                if (ValidationSetting.DataItemExistence == "True")
                {
                    ValidateDataItemExistence();
                }
                worker?.ReportProgress(30);


                if (ValidationSetting.LogicalGroup == "True")
                {
                    ValidateLogicalGroup();
                }
                worker?.ReportProgress(40);


                if (ValidationSetting.LinkKeyOrder == "True")
                {
                    ValidateLinkKeyOrder();
                }
                worker?.ReportProgress(45);

                if (ValidationSetting.LinkCompletion == "True")
                {
                    ValidateLinkCompletion();
                }
                worker?.ReportProgress(50);

                ValidateHardcodedFields();

                ValidateAttributeDataObjectsForTableMappings();

                ValidateSchemaConfiguration();

                worker?.ReportProgress(80);

                if (ValidationSetting.BasicDataVaultValidation == "True")
                {
                    ValidateBasicDataVaultAttributeExistence();
                }

                

                worker?.ReportProgress(100);

                // Informing the user.
                _alertValidation.SetTextLogging("\r\n\r\nIn total " + MetadataValidations.ValidationIssues + " validation issues have been found.");
            }
        }


        /// <summary>
        /// This method runs a check against the Column Mappings DataGrid to assert if model metadata is available for the attributes. The column needs to exist somewhere, either in the physical model or in the model metadata in order for activation to run successfully.
        /// </summary>
        private void ValidateSchemaConfiguration()
        {
            var localDataTable = (DataTable) BindingSourceDataObjectMappings.DataSource;

            // Informing the user.
            _alertValidation.SetTextLogging($"--> Commencing the validation to check if connection settings align with schemas entered in the Data Object mapping grid.\r\n");

            int resultCounter = 0;
            
            foreach (DataRow row in localDataTable.Rows)
            {
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True") // If row is enabled
                {
                    string localSourceConnectionInternalId = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    string localTargetConnectionInternalId = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();

                    TeamConnection sourceConnection = GetTeamConnectionByConnectionId(localSourceConnectionInternalId);
                    TeamConnection targetConnection = GetTeamConnectionByConnectionId(localTargetConnectionInternalId);

                    // The values in the data grid, fully qualified. This means the default schema is added if necessary.
                    var sourceDataObject = MetadataHandling
                        .GetFullyQualifiedDataObjectName(row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString(), sourceConnection)
                        .FirstOrDefault();
                    var targetDataObject = MetadataHandling
                        .GetFullyQualifiedDataObjectName(
                            row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString(), targetConnection)
                        .FirstOrDefault();

                    // The values as defined in the associated connections
                    var sourceSchemaNameForConnection = TeamConfiguration.GetTeamConnectionByInternalId(localSourceConnectionInternalId, TeamConfiguration.ConnectionDictionary).DatabaseServer.SchemaName.Replace("[", "").Replace("]", "");

                    var targetSchemaNameForConnection = TeamConfiguration.GetTeamConnectionByInternalId(localTargetConnectionInternalId, TeamConfiguration.ConnectionDictionary).DatabaseServer.SchemaName.Replace("[", "").Replace("]", "");


                    if (sourceDataObject.Key.Replace("[", "").Replace("]", "") != sourceSchemaNameForConnection)
                    {
                        _alertValidation.SetTextLogging($"--> Inconsistency detected for '{sourceDataObject.Key}.{sourceDataObject.Value}' between the schema definition in the table grid '{sourceDataObject.Key}' and its assigned connection '{sourceConnection.ConnectionName}' which has been configured as '{sourceSchemaNameForConnection}'.\r\n");
                        resultCounter++;
                    }

                    if (targetDataObject.Key.Replace("[", "").Replace("]", "") != targetSchemaNameForConnection)
                    {
                        _alertValidation.SetTextLogging($"--> Inconsistency for '{sourceDataObject.Key}.{sourceDataObject.Value}' detected between the schema definition in the table grid {targetDataObject.Key} and its assigned connection ''{targetConnection.ConnectionName}'' which has been configured as '{targetSchemaNameForConnection}'.\r\n");
                        resultCounter++;
                    }
                }
            }

            if (resultCounter == 0)
            {
                _alertValidation.SetTextLogging("     There were no validation issues related to schema configuration.\r\n\r\n");
            }

        }

        private void ValidateLinkCompletion()
        {
            // Informing the user.
            _alertValidation.SetTextLogging($"--> Commencing the validation to determine if Link concept is correctly defined.\r\n");

            var localConnectionDictionary = LocalConnectionDictionary.GetLocalConnectionDictionary(TeamConfiguration.ConnectionDictionary);

            // Creating a list of unique Link business key combinations from the data grid / data table
            var localDataTableTableMappings = (DataTable)BindingSourceDataObjectMappings.DataSource;
            var objectList = new List<Tuple<string, string, string, string>>(); // Source, Target, Business Key, Target Connection

            foreach (DataRow row in localDataTableTableMappings.Rows)
            {
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                // Only process enabled mappings.
                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() != "True") continue;

                // Only select the lines that relate to a Link target.
                if (row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString().StartsWith(TeamConfiguration.LinkTablePrefixValue))
                {
                    // Derive the business key.
                    var businessKey = row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].ToString().Replace("''''", "'");

                    // Derive the connection
                    localConnectionDictionary.TryGetValue(row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString(), out var targetConnectionValue);

                    var newValidationObject = new Tuple<string, string, string, string>
                    (
                        row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString(),
                        row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString(),
                        businessKey,
                        targetConnectionValue
                    );

                    if (!objectList.Contains(newValidationObject))
                    {
                        objectList.Add(newValidationObject);
                    }
                }
            }
            
            // Execute the validation check using the list of unique objects
            var resultList = new Dictionary<string, bool>();

            foreach (var linkObject in objectList)
            {
                 // The validation check returns a Dictionary
                var validatedObject = MetadataValidation.ValidateLinkBusinessKeyForCompletion(linkObject);

                // Looping through the dictionary to evaluate results.
                foreach (var pair in validatedObject)
                {
                    if (pair.Value == false)
                    {
                        if (!resultList.ContainsKey(pair.Key)) // Prevent incorrect links to be added multiple times
                        {
                            resultList.Add(pair.Key, pair.Value); // Add objects that did not pass the test
                        }
                    }
                }
            }

            // Return the results back to the user
            if (resultList.Count > 0)
            {
                foreach (var sourceObjectResult in resultList)
                {
                    _alertValidation.SetTextLogging("     " + sourceObjectResult.Key + " is tested with this outcome: " + sourceObjectResult.Value + ". This means there is an issue with the Link definition, and in particular the Business Key. Are two Hubs assigned?\r\n");
                }

                MetadataValidations.ValidationIssues = MetadataValidations.ValidationIssues + resultList.Count();
                _alertValidation.SetTextLogging("\r\n");
            }
            else
            {
                _alertValidation.SetTextLogging("     There were no validation issues related to the definition of Link tables.\r\n\r\n");
            }


        }

        /// <summary>
        /// This method runs a check against the Attribute Mappings DataGrid to assert if model metadata is available for the attributes. The attribute needs to exist somewhere, either in the physical model or in the model metadata in order for activation to run successfully.
        /// </summary>
        private void ValidateDataItemExistence()
        {
            // Informing the user.
            _alertValidation.SetTextLogging($"--> Commencing the validation to determine if the data items (columns) in the metadata exists in the model.\r\n");

            var resultList = new Dictionary<string, string>();

            var localDataItemTable = (DataTable) BindingSourceDataItemMappings.DataSource;
            var localDataObjectTable = (DataTable) BindingSourceDataObjectMappings.DataSource;

            foreach (DataRow row in localDataItemTable.Rows)
            {
                // Look for the corresponding Data Object Mapping row.
                var dataObjectRow = GetDataObjectMappingFromDataItemMapping(localDataObjectTable, row[DataItemMappingGridColumns.SourceDataObject.ToString()].ToString(), row[DataItemMappingGridColumns.TargetDataObject.ToString()].ToString());

                if (dataObjectRow.Item1 == "True") //If the corresponding Data Object is enabled
                {
                    var validationObjectSource = row[DataItemMappingGridColumns.SourceDataObject.ToString()].ToString();
                    TeamConnection sourceConnection = dataObjectRow.Item3;
                    var validationAttributeSource = row[DataItemMappingGridColumns.SourceDataItem.ToString()].ToString();

                    var validationObjectTarget = row[DataItemMappingGridColumns.TargetDataObject.ToString()].ToString();
                    TeamConnection targetConnection = dataObjectRow.Item5;
                    var validationAttributeTarget = row[DataItemMappingGridColumns.TargetDataItem.ToString()].ToString();

                    var sourceDataObjectType = MetadataHandling.GetDataObjectType(validationObjectSource, "", TeamConfiguration).ToString();

                    // No need to evaluate the operational system (real sources), or if the source is a data query (logic).
                    if (sourceDataObjectType != MetadataHandling.DataObjectTypes.Source.ToString() && !validationAttributeSource.IsDataQuery()) 
                    {
                        var objectValidated = MetadataValidation.ValidateAttributeExistence(validationObjectSource, validationAttributeSource, sourceConnection, (DataTable) BindingSourcePhysicalModel.DataSource);

                        // Add negative results to dictionary
                        if (objectValidated == "False" && !resultList.ContainsKey(validationAttributeSource))
                        {
                            resultList.Add(validationAttributeSource, validationObjectSource); // Add objects that did not pass the test
                        }

                        objectValidated = MetadataValidation.ValidateAttributeExistence(validationObjectTarget, validationAttributeTarget, targetConnection, (DataTable) BindingSourcePhysicalModel.DataSource);

                        // Add negative results to dictionary
                        if (objectValidated == "False" && !resultList.ContainsKey(validationAttributeTarget))
                        {
                            resultList.Add(validationAttributeTarget, validationObjectTarget); // Add objects that did not pass the test
                        }
                    }
                }
            }

            // Return the results back to the user
            if (resultList.Count > 0)
            {
                foreach (var objectValidationResult in resultList)
                {
                    _alertValidation.SetTextLogging($"     {objectValidationResult.Key} belonging to {objectValidationResult.Value} does not exist in the physical model.\r\n");
                }

                MetadataValidations.ValidationIssues = MetadataValidations.ValidationIssues + resultList.Count;

                _alertValidation.SetTextLogging("\r\n");
            }
            else
            {
                _alertValidation.SetTextLogging($"     There were no validation issues related to the existence of the attribute(s).\r\n\r\n");
            }
        }

        /// <summary>
        /// This method runs a check against the DataGrid to assert if model metadata is available for the object. The object needs to exist somewhere, either in the physical model or in the model metadata in order for activation to run succesfully.
        /// </summary>
        private void ValidateObjectExistence(DataTable dataTable)
        {
            // Informing the user.
            _alertValidation.SetTextLogging($"--> Commencing the validation to determine if the defined Data Objects exists in the model.\r\n");

            var resultList = new Dictionary<string, string>();

            // Iterating over the grid
            foreach (DataRow row in dataTable.Rows)
            {
                // Skip deleted rows
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if ((string)row[(int)DataObjectMappingGridColumns.Enabled] == "True")
                {
                    // Sources
                    var validationObjectSource = row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString();
                    var validationObjectSourceConnectionId = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    var sourceConnection = GetTeamConnectionByConnectionId(validationObjectSourceConnectionId);
                    KeyValuePair<string, string> fullyQualifiedValidationObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(validationObjectSource, sourceConnection).FirstOrDefault();

                    // Targets
                    var validationObjectTarget = row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString();
                    var validationObjectTargetConnectionId = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                    var targetConnection = GetTeamConnectionByConnectionId(validationObjectTargetConnectionId);
                    KeyValuePair<string, string> fullyQualifiedValidationObjectTarget = MetadataHandling.GetFullyQualifiedDataObjectName(validationObjectTarget, targetConnection).FirstOrDefault();

                    // No need to evaluate the operational system (real sources))
                    if (MetadataHandling.GetDataObjectType(validationObjectSource, "", TeamConfiguration).ToString() != MetadataHandling.DataObjectTypes.Source.ToString())
                    {
                        string objectValidated;
                        //if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode)
                        //{
                        //    try
                        //    {
                        //        objectValidated = MetadataValidation.ValidateObjectExistencePhysical(fullyQualifiedValidationObjectSource, sourceConnection);

                        //        // Add negative results to dictionary
                        //        if (objectValidated == "False" && !resultList.ContainsKey(fullyQualifiedValidationObjectSource.Key + '.' + fullyQualifiedValidationObjectSource.Value))
                        //        {
                        //            resultList.Add(fullyQualifiedValidationObjectSource.Key + '.' + fullyQualifiedValidationObjectSource.Value, objectValidated); 
                        //        }
                                
                        //        objectValidated = MetadataValidation.ValidateObjectExistencePhysical(fullyQualifiedValidationObjectTarget, targetConnection);

                        //        // Add negative results to dictionary
                        //        if (objectValidated == "False" && !resultList.ContainsKey(fullyQualifiedValidationObjectTarget.Key + '.' + fullyQualifiedValidationObjectTarget.Value))
                        //        {
                        //            resultList.Add(fullyQualifiedValidationObjectTarget.Key + '.' + fullyQualifiedValidationObjectTarget.Value, objectValidated); 
                        //        }
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,$"An issue occurred connecting to the database: \r\n\r\n {ex}."));
                        //    }
                        //}

                        //if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode)
                        //{
                            objectValidated = MetadataValidation.ValidateObjectExistence(validationObjectSource, sourceConnection, (DataTable) BindingSourcePhysicalModel.DataSource);

                            if (objectValidated == "False" && !resultList.ContainsKey(fullyQualifiedValidationObjectSource.Key + '.' + fullyQualifiedValidationObjectSource.Value))
                            {
                                resultList.Add(fullyQualifiedValidationObjectSource.Key + '.' + fullyQualifiedValidationObjectSource.Value, objectValidated); // Add objects that did not pass the test
                            }

                            objectValidated = MetadataValidation.ValidateObjectExistence(validationObjectTarget, targetConnection, (DataTable) BindingSourcePhysicalModel.DataSource);

                            if (objectValidated == "False" && !resultList.ContainsKey(fullyQualifiedValidationObjectTarget.Key + '.' + fullyQualifiedValidationObjectTarget.Value))
                            {
                                resultList.Add(fullyQualifiedValidationObjectTarget.Key + '.' + fullyQualifiedValidationObjectTarget.Value, objectValidated); // Add objects that did not pass the test
                            }
                        //}
                        //else
                        //{
                        //    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning,$"The validation approach (physical/virtual) could not be asserted."));
                        //}
                    }
                }
            }

            // Return the results back to the user
            if (resultList.Count > 0)
            {
                foreach (var objectValidationResult in resultList)
                {
                    _alertValidation.SetTextLogging($"     {objectValidationResult.Key} is tested with outcome {objectValidationResult.Value}. This may be because the schema is defined differently in the connection, or because it simply does not exist.\r\n");
                }

                MetadataValidations.ValidationIssues = MetadataValidations.ValidationIssues + resultList.Count;
                _alertValidation.SetTextLogging("\r\n");
            }
            else
            {
                _alertValidation.SetTextLogging($"     There were no validation issues related to the (physical) existence of the defined Data Object in the model.\r\n\r\n");
            }

        }

        /// <summary>
        /// Hard-coded fields in Staging Layer data objects are not supported. Instead, an attribute mapping should be created.
        /// </summary>
        internal void ValidateHardcodedFields()
        {
            _alertValidation.SetTextLogging(
                $"--> Commencing the validation to see if any hard-coded fields are not correctly set in enabled mappings.\r\n");

            int issueCounter = 0;
            var localDataTable = (DataTable) BindingSourceDataObjectMappings.DataSource;
            foreach (DataRow row in localDataTable.Rows)
            {
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                // If enabled and is a Staging Layer object
                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True" && 
                    MetadataHandling.GetDataObjectType((string) row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()], "", TeamConfiguration).In(MetadataHandling.DataObjectTypes.StagingArea, MetadataHandling.DataObjectTypes.PersistentStagingArea))
                {
                    if (row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].ToString().Contains("'"))
                    {
                        issueCounter++;
                        _alertValidation.SetTextLogging(
                            $"     Data Object {(string) row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()]} should not contain hard-coded values in the Business Key definition. This can not be supported in the Staging Layer (Staging Area and Persistent Staging Area)");
                    }
                }
            }

            if (issueCounter == 0)
            {
                _alertValidation.SetTextLogging($"     There were no validation issues related to the definition of hard-coded Business Key components.\r\n\r\n");
            }

            MetadataValidations.ValidationIssues = MetadataValidations.ValidationIssues + issueCounter;
        }

        internal void ValidateAttributeDataObjectsForTableMappings()
        {
            _alertValidation.SetTextLogging($"--> Commencing the validation to see if all data item (attribute) mappings exist as data object (table) mapping also (if enabled in the grid).\r\n");
            int issueCounter = 0;

            var localDataTableTableMappings = (DataTable) BindingSourceDataObjectMappings.DataSource;
            var localDataTableAttributeMappings = (DataTable) BindingSourceDataItemMappings.DataSource;

            // Create a list of all sources and targets for the Data Object mappings
            List<Tuple<string,string>> sourceDataObjectListTableMapping = new List<Tuple<string, string>>();
            List<Tuple<string, string>> targetDataObjectListTableMapping = new List<Tuple<string, string>>();

            foreach (DataRow row in localDataTableTableMappings.Rows)
            {
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                var sourceDataObjectTuple = new Tuple<string, string>((string)row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()], row[DataObjectMappingGridColumns.Enabled.ToString()].ToString());
                var targetDataObjectTuple = new Tuple<string, string>((string)row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()], row[DataObjectMappingGridColumns.Enabled.ToString()].ToString());

                if (!sourceDataObjectListTableMapping.Contains(sourceDataObjectTuple))
                {
                    sourceDataObjectListTableMapping.Add(sourceDataObjectTuple);
                }

                if (!targetDataObjectListTableMapping.Contains(targetDataObjectTuple))
                {
                    targetDataObjectListTableMapping.Add(targetDataObjectTuple);
                }
            }

            foreach (DataRow row in localDataTableAttributeMappings.Rows)
            {
                var localSource = (string) row[DataItemMappingGridColumns.SourceDataObject.ToString()];
                var localTarget = (string) row[DataItemMappingGridColumns.TargetDataObject.ToString()];

                // If the value exists, but is disabled just a warning is sufficient.
                // If the value does not exist for an enabled mapping or at all, then it's an error.
                
                if (sourceDataObjectListTableMapping.Contains(new Tuple<string, string>(localSource, "False")))
                {
                    //_alertValidation.SetTextLogging($"     Data Object {localSource} in the attribute mappings exists in the table mappings for an disabled mapping. This can be disregarded.\r\n");
                }
                else if (sourceDataObjectListTableMapping.Contains(new Tuple<string, string>(localSource, "True")))
                {
                    // No problem, it's found
                }
                else 
                {
                    _alertValidation.SetTextLogging($"     Data Object {localSource} in the attribute mappings (source) does not seem to exist in the table mappings for an enabled mapping. Please check if this name is mapped at table level in the grid also.\r\n");
                    issueCounter++;
                }

                if (targetDataObjectListTableMapping.Contains(new Tuple<string, string>(localTarget, "False")))
                {
                    //_alertValidation.SetTextLogging($"     Data Object {localTarget} in the attribute mappings exists in the table mappings for an disabled mapping. This can be disregarded.\r\n");
                }
                else if (targetDataObjectListTableMapping.Contains(new Tuple<string, string>(localTarget, "True")))
                {
                    // No problem, it's found
                }
                else
                {
                    _alertValidation.SetTextLogging($"     Data Object {localTarget} in the attribute mappings (target) does not seem to exist in the table mappings for an enabled mapping. Please check if this name is mapped at table level in the grid also.\r\n");
                    issueCounter++;
                }
            }

            if (issueCounter == 0)
            {
                _alertValidation.SetTextLogging($"     There were no validation issues related to the existence of Data Objects related to defined Data Item Mappings.\r\n\r\n");
            }

            MetadataValidations.ValidationIssues = MetadataValidations.ValidationIssues + issueCounter;
        }

        /// <summary>
        /// This method will check if the order of the keys in the Link is consistent with the physical table structures.
        /// </summary>
        internal void ValidateLinkKeyOrder()
        {
            #region Retrieving the Links

            // Informing the user.
            _alertValidation.SetTextLogging("--> Commencing the validation to ensure the order of Business Keys in the Link metadata corresponds with the physical model.\r\n");

            var localConnectionDictionary = LocalConnectionDictionary.GetLocalConnectionDictionary(TeamConfiguration.ConnectionDictionary);

            // Creating a list of unique Link business key combinations from the data grid / data table
            var localDataTableTableMappings = (DataTable)BindingSourceDataObjectMappings.DataSource;
            var objectList = new List<Tuple<string, string, string, string>>();
            
            foreach (DataRow row in localDataTableTableMappings.Rows)
            {
                // Skip deleted rows
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True")
                {
                    // Only select the lines that relate to a Link target.
                    if (row[DataObjectMappingGridColumns.TargetDataObject.ToString()].ToString().StartsWith(TeamConfiguration.LinkTablePrefixValue))
                    {
                        // Derive the business key.
                        var businessKey = row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].ToString().Replace("''''", "'");

                        // Derive the connection
                        localConnectionDictionary.TryGetValue(row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString(), out var connectionValue);
                        
                        var newValidationObject = new Tuple<string, string, string, string>
                        (
                            row[DataObjectMappingGridColumns.SourceDataObject.ToString()].ToString(),
                            row[DataObjectMappingGridColumns.TargetDataObject.ToString()].ToString(),
                            businessKey,
                            connectionValue
                        );

                        if (!objectList.Contains(newValidationObject))
                        {
                            objectList.Add(newValidationObject);
                        }
                    }
                }
            }

            // Execute the validation check using the list of unique objects
            var resultList = new Dictionary<string, bool>();

            foreach (var sourceObject in objectList)
            {
                // The validation check returns a Dictionary
                var sourceObjectValidated = MetadataValidation.ValidateLinkKeyOrder(sourceObject, (DataTable) BindingSourceDataObjectMappings.DataSource, (DataTable) BindingSourcePhysicalModel.DataSource);

                // Looping through the dictionary
                foreach (var pair in sourceObjectValidated)
                {
                    if (pair.Value == false)
                    {
                        if (!resultList.ContainsKey(pair.Key)) // Prevent incorrect links to be added multiple times
                        {
                            resultList.Add(pair.Key, pair.Value); // Add objects that did not pass the test
                        }
                    }
                }
            }

            #endregion

            // Return the results back to the user
            if (resultList.Count > 0)
            {
                foreach (var sourceObjectResult in resultList)
                {
                    _alertValidation.SetTextLogging("     " + sourceObjectResult.Key +
                                                    " is tested with this outcome: " + sourceObjectResult.Value +
                                                    "\r\n");
                }

                MetadataValidations.ValidationIssues = MetadataValidations.ValidationIssues + resultList.Count();
                _alertValidation.SetTextLogging("\r\n");
            }
            else
            {
                _alertValidation.SetTextLogging("     There were no validation issues related to order of business keys in the Link tables.\r\n\r\n");
            }
        }
        internal void ValidateBasicDataVaultAttributeExistence()
        {
            // Informing the user.
            _alertValidation.SetTextLogging("--> Commencing the validation to check if basic Data Vault attributes are present.\r\n");

            List<Tuple<string,string,bool>> masterResultList = new List<Tuple<string, string, bool>>();
            
            var localDataTable = (DataTable)BindingSourceDataObjectMappings.DataSource;
            foreach (DataRow row in localDataTable.Rows)
            {               
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True")
                {
                    var localDataObjectSourceName = row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString();
                    var localDataObjectSourceConnectionId = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    var localDataObjectSourceConnection = GetTeamConnectionByConnectionId(localDataObjectSourceConnectionId);
                    var localDataObjectSourceTableType = MetadataHandling.GetDataObjectType(localDataObjectSourceName, "", TeamConfiguration);
                    
                    var localDataObjectTargetName = row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString();
                    var localDataObjectTargetConnectionId = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                    var localDataObjectTargetConnection = GetTeamConnectionByConnectionId(localDataObjectTargetConnectionId);
                    var localDataObjectTargetTableType = MetadataHandling.GetDataObjectType(localDataObjectTargetName, "", TeamConfiguration);

                    // Source
                    if (localDataObjectSourceTableType == MetadataHandling.DataObjectTypes.CoreBusinessConcept ||
                        localDataObjectSourceTableType == MetadataHandling.DataObjectTypes.Context ||
                        localDataObjectSourceTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationship ||
                        localDataObjectSourceTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext ||
                        localDataObjectSourceTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
                    {
                        var result = MetadataValidation.BasicDataVaultValidation(localDataObjectSourceName, localDataObjectSourceConnection, localDataObjectSourceTableType);
                        masterResultList.AddRange(result);
                        
                    }

                    // Target
                    if (localDataObjectTargetTableType == MetadataHandling.DataObjectTypes.CoreBusinessConcept ||
                        localDataObjectTargetTableType == MetadataHandling.DataObjectTypes.Context ||
                        localDataObjectTargetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationship ||
                        localDataObjectTargetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext ||
                        localDataObjectTargetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
                    {
                        var result = MetadataValidation.BasicDataVaultValidation(localDataObjectTargetName, localDataObjectTargetConnection, localDataObjectTargetTableType);
                        masterResultList.AddRange(result);

                    }
                }
            }
            
            // Evaluate the results
            int resultsCounter = 0;

            // Deduplicate
            List<Tuple<string,string,bool>> deduplicatedResultList = masterResultList.Distinct().ToList();

            foreach (var result in deduplicatedResultList)
            {
                if (result.Item3 == false)
                {
                    _alertValidation.SetTextLogging($"     Warning - {result.Item1} was evaluated as a Data Vault object but the expected attribute {result.Item2} was not found in the table.\r\n");
                    resultsCounter++;
                }
            }

            if (resultsCounter == 0)
            {
                _alertValidation.SetTextLogging("     There were no validation issues related Data Vault attribute existence.\r\n\r\n");
            }


        }

        /// <summary>
        /// Checks if all the supporting mappings are available (e.g. a Context table also needs a Core Business Concept present).
        /// </summary>
        internal void ValidateLogicalGroup()
        {
            #region Retrieving the Integration Layer tables

            // Informing the user.
            _alertValidation.SetTextLogging("--> Commencing the validation to check if the functional dependencies (logical group / unit of work) are present.\r\n");

            // Creating a list of tables which are dependent on other tables being present
            var objectList = new List<Tuple<string, string, string, string>>(); // Source Name, Target Name, Business Key, FilterCriterion

            var localDataTable = (DataTable) BindingSourceDataObjectMappings.DataSource;
            
            foreach (DataRow row in localDataTable.Rows)
            {
                // Skip deleted rows
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True")
                {
                    var targetDataObjectName = row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString();
                    var targetConnectionInternalId = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                    var targetConnection = GetTeamConnectionByConnectionId(targetConnectionInternalId);
                    var targetFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObjectName, targetConnection).FirstOrDefault();
                    var targetTableType = MetadataHandling.GetDataObjectType(targetDataObjectName, "", TeamConfiguration);
                    var targetFilterCriterion = row[DataObjectMappingGridColumns.FilterCriterion.ToString()].ToString();

                    var sourceDataObjectName = row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString();
                    var sourceConnectionInternalId = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    var sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionInternalId);
                    var sourceFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(sourceDataObjectName, sourceConnection).FirstOrDefault();


                    if (targetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationship || targetTableType == MetadataHandling.DataObjectTypes.Context || targetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext)
                    {
                        var businessKey = row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].ToString().Replace("''''", "'");
                        
                        if (!objectList.Contains(new Tuple<string, string, string, string>
                            (
                            sourceFullyQualifiedName.Key+'.'+sourceFullyQualifiedName.Value, targetFullyQualifiedName.Key + '.' + targetFullyQualifiedName.Value, businessKey, targetFilterCriterion)
                            )
                        )
                        {
                            objectList.Add(new Tuple<string, string, string, string>
                                (
                                    sourceFullyQualifiedName.Key + '.' + sourceFullyQualifiedName.Value, targetFullyQualifiedName.Key + '.' + targetFullyQualifiedName.Value, businessKey, targetFilterCriterion)
                            );
                        }
                    }
                }
            }

            // Execute the validation check using the list of unique objects
            var resultList = new Dictionary<string, bool>();

            foreach (var validationObject in objectList)
            {
                // The validation check returns a Dictionary
                var sourceObjectValidated = MetadataValidation.ValidateLogicalGroup(validationObject, (DataTable) BindingSourceDataObjectMappings.DataSource);

                // Looping through the dictionary
                foreach (var pair in sourceObjectValidated)
                {
                    if (pair.Value == false)
                    {
                        if (!resultList.ContainsKey(pair.Key)) // Prevent incorrect links to be added multiple times
                        {
                            resultList.Add(pair.Key, pair.Value); // Add objects that did not pass the test
                        }
                    }
                }
            }

            #endregion

            // Return the results back to the user
            if (resultList.Count > 0)
            {
                foreach (var sourceObjectResult in resultList)
                {
                    _alertValidation.SetTextLogging("     " + sourceObjectResult.Key + " is tested with this outcome: " + sourceObjectResult.Value + "." +
                                                    "\r\n     This means there is a Link or Satellite without it's supporting Hub(s) defined." +
                                                    "\r\n     If a source loads a Link or Satellite, this source should also load a Hub that relates to the Link or Satellite.\r\n");
                }

                _alertValidation.SetTextLogging("\r\n");
                MetadataValidations.ValidationIssues = MetadataValidations.ValidationIssues + resultList.Count();
            }
            else
            {
                _alertValidation.SetTextLogging("     There were no validation issues related to the logical grouping of objects.\r\n\r\n");
            }
        }

        /// <summary>
        ///   A validation check to make sure the Business Key is available in the source model.
        /// </summary>
        private void ValidateBusinessKeyObject()
        {
            var resultList = new Dictionary<Tuple<string, string>, bool>();

            // Informing the user.
            _alertValidation.SetTextLogging("--> Commencing the validation to determine if the Business Key metadata attributes exist in the physical model.\r\n");


            var localDataTable = (DataTable) BindingSourceDataObjectMappings.DataSource;
            foreach (DataRow row in localDataTable.Rows)
            {
                // Skip deleted rows
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True") // If row is enabled
                {
                    Dictionary<Tuple<string, string>, bool> objectValidated = new Dictionary<Tuple<string, string>, bool>();

                    // Source table and business key definitions.
                    string validationObject = row[DataObjectMappingGridColumns.SourceDataObject.ToString()].ToString();
                    string validationConnectionId = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    TeamConnection validationConnection = GetTeamConnectionByConnectionId(validationConnectionId);
                    string businessKeyDefinition = row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].ToString();

                    // Exclude a lookup to the source
                    if (MetadataHandling.GetDataObjectType(validationObject, "", TeamConfiguration).ToString() != MetadataHandling.DataObjectTypes.Source.ToString())
                    {
                        objectValidated = MetadataValidation.ValidateSourceBusinessKeyExistenceVirtual(validationObject, businessKeyDefinition, validationConnection, (DataTable) BindingSourcePhysicalModel.DataSource);
                    }

                    // Add negative results to dictionary
                    foreach (var objectValidationTuple in objectValidated)
                    {
                        if (objectValidationTuple.Value == false && !resultList.ContainsKey(objectValidationTuple.Key))
                        {
                            resultList.Add(objectValidationTuple.Key, false); // Add objects that did not pass the test
                        }
                    }
                }
            }

            // Return the results back to the user
            if (resultList.Count > 0)
            {
                foreach (var sourceObjectResult in resultList)
                {
                    _alertValidation.SetTextLogging("     Table " + sourceObjectResult.Key.Item1 +
                                                    " does not contain Business Key attribute " +
                                                    sourceObjectResult.Key.Item2 + ".\r\n");
                }

                MetadataValidations.ValidationIssues = MetadataValidations.ValidationIssues + resultList.Count();
            }
            else
            {
                _alertValidation.SetTextLogging(
                    "     There were no validation issues related to the existence of the business keys in the Source tables.\r\n");
            }

            _alertValidation.SetTextLogging("\r\n");
        }

        private void backgroundWorkerValidationOnly_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Show the progress in main form (GUI)
            labelResult.Text = (e.ProgressPercentage + "%");

            // Pass the progress to AlertForm label and progressbar
            _alertValidation.Message = "In progress, please wait... " + e.ProgressPercentage + "%";
            _alertValidation.ProgressValue = e.ProgressPercentage;
        }

        private void backgroundWorkerValidationOnly_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                labelResult.Text = "Cancelled!";
            }
            else if (e.Error != null)
            {
                labelResult.Text = "Error: " + e.Error.Message;
            }
            else
            {
                labelResult.Text = "Done!";
                richTextBoxInformation.Text += "\r\nThe metadata was validated successfully!\r\n";
            }
        }

        private void FormManageMetadata_Shown(object sender, EventArgs e)
        {
            GridAutoLayout();
        }

        private void displayTableScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Retrieve the index of the selected row
            Int32 selectedRow = _dataGridViewPhysicalModel.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            DataTable gridDataTable = (DataTable) BindingSourcePhysicalModel.DataSource;
            DataTable dt2 = gridDataTable.Clone();
            dt2.Columns[PhysicalModelMappingMetadataColumns.Ordinal_Position.ToString()].DataType =
                Type.GetType("System.Int32");

            foreach (DataRow dr in gridDataTable.Rows)
            {
                dt2.ImportRow(dr);
            }

            dt2.AcceptChanges();

            // Make sure the output is sorted
            dt2.DefaultView.Sort =
                $"{PhysicalModelMappingMetadataColumns.Table_Name.ToString()} ASC, {PhysicalModelMappingMetadataColumns.Ordinal_Position.ToString()} ASC";

            // Retrieve all rows relative to the selected row (e.g. all attributes for the table)
            IEnumerable<DataRow> rows = dt2.DefaultView.ToTable().AsEnumerable().Where(r =>
                r.Field<string>(PhysicalModelMappingMetadataColumns.Table_Name.ToString()) ==
                _dataGridViewPhysicalModel.Rows[selectedRow].Cells[4].Value.ToString()
                && r.Field<string>(PhysicalModelMappingMetadataColumns.Schema_Name.ToString()) ==
                _dataGridViewPhysicalModel.Rows[selectedRow].Cells[3].Value.ToString()
                && r.Field<string>(PhysicalModelMappingMetadataColumns.Database_Name.ToString()) ==
                _dataGridViewPhysicalModel.Rows[selectedRow].Cells[2].Value.ToString()
            );

            // Create a form and display the results
            var results = new StringBuilder();

            _generatedScripts = new Form_Alert();
            _generatedScripts.SetFormName("Display model metadata");
            _generatedScripts.Canceled += buttonCancel_Click;
            _generatedScripts.Show();

            results.AppendLine("IF OBJECT_ID('[" + _dataGridViewPhysicalModel.Rows[selectedRow].Cells[4].Value +
                               "]', 'U') IS NOT NULL");
            results.AppendLine(
                "DROP TABLE [" + _dataGridViewPhysicalModel.Rows[selectedRow].Cells[4].Value + "]");
            results.AppendLine();
            results.AppendLine("CREATE TABLE [" + _dataGridViewPhysicalModel.Rows[selectedRow].Cells[4].Value +
                               "]");
            results.AppendLine("(");

            int counter = 1;
            foreach (DataRow row in rows)
            {
                var commaSnippet = "";
                if (counter == 1)
                {
                    commaSnippet = "  ";
                }
                else
                {
                    commaSnippet = " ,";
                }

                counter++;
                results.AppendLine(commaSnippet + row[PhysicalModelMappingMetadataColumns.Column_Name.ToString()] +" -- with ordinal position of " +row[PhysicalModelMappingMetadataColumns.Ordinal_Position.ToString()]);
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
                if (GlobalParameters.ConfigurationPath != "")
                {
                    Process.Start(GlobalParameters.ConfigurationPath);
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
        private void openAttributeMappingFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Attribute Mapping Metadata File",
                Filter = @"Attribute Mapping files|*.xml;*.json",
                InitialDirectory = GlobalParameters.MetadataPath
            };

            var ret = STAShowDialog(theDialog);

            if (ret == DialogResult.OK)
            {
                try
                {
                    var chosenFile = theDialog.FileName;
                    var dataSet = new DataSet();

                    string fileExtension = Path.GetExtension(theDialog.FileName);

                    if (fileExtension == ".xml")
                    {
                        dataSet.ReadXml(chosenFile);

                        _dataGridViewDataItems.DataSource = dataSet.Tables[0];
                        BindingSourceDataItemMappings.DataSource = _dataGridViewDataItems.DataSource;
                    }
                    else if (fileExtension == ".json")
                    {
                        // Load the file, convert it to a DataTable and bind it to the source
                        List<AttributeMappingJson> jsonArray = JsonConvert.DeserializeObject<List<AttributeMappingJson>>(File.ReadAllText(chosenFile));
                        DataTable dt = Utility.ConvertToDataTable(jsonArray);

                        // Set the column names in the datatable.
                        //SetTeamDataTableProperties.SetAttributeDataTableColumns(dt);
                        // Sort the columns in the datatable.
                        //SetTeamDataTableProperties.SetAttributeDatTableSorting(dt);

                        // Clear out the existing data from the grid
                        BindingSourceDataItemMappings.DataSource = null;
                        BindingSourceDataItemMappings.Clear();
                        _dataGridViewDataItems.DataSource = null;

                        // Bind the datatable to the gridview
                        BindingSourceDataItemMappings.DataSource = dt;

                        if (jsonArray != null)
                        {
                            // Set the column header names.
                            _dataGridViewDataItems.DataSource = BindingSourceDataItemMappings;
                            _dataGridViewDataItems.ColumnHeadersVisible = true;
                            _dataGridViewDataItems.Columns[0].Visible = false;
                            _dataGridViewDataItems.Columns[1].Visible = false;
                            _dataGridViewDataItems.Columns[6].ReadOnly = false;

                            _dataGridViewDataItems.Columns[0].HeaderText = "Hash Key";
                            _dataGridViewDataItems.Columns[1].HeaderText = "Version ID";
                            _dataGridViewDataItems.Columns[2].HeaderText = "Source Table";
                            _dataGridViewDataItems.Columns[3].HeaderText = "Source Column";
                            _dataGridViewDataItems.Columns[4].HeaderText = "Target Table";
                            _dataGridViewDataItems.Columns[5].HeaderText = "Target Column";
                            _dataGridViewDataItems.Columns[6].HeaderText = "Notes";
                        }
                    }

                    GridAutoLayout(_dataGridViewDataItems);
                    richTextBoxInformation.AppendText("The metadata has been loaded from file.\r\n");
                    ContentCounter();
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.AppendText("An error has been encountered. Please check the Event Log for more details.\r\n");
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered: {ex}."));
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
                InitialDirectory = GlobalParameters.MetadataPath
            };

            var dialogResult = STAShowDialog(dialog);

            if (dialogResult == DialogResult.OK)
            {
                richTextBoxInformation.Clear();

                try
                {
                    #region Build the Data Table
                    var fileName = dialog.FileName;

                    List<TableMappingJson> jsonArray = JsonConvert.DeserializeObject<List<TableMappingJson>>(File.ReadAllText(fileName));

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

                    richTextBoxInformation.AppendText($"The file '{fileName}' was loaded.\r\n");
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
                    // Get the JSON files and load these into memory.
                    TeamDataObjectMappingFileCombinations = new TeamDataObjectMappingsFileCombinations(GlobalParameters.MetadataPath);
                    TeamDataObjectMappingFileCombinations.GetMetadata();

                    //Load the grids from the repository after being updated.This resets everything.
                    PopulateDataObjectMappingGrid(TeamDataObjectMappingFileCombinations);

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
                _alertEventLog.Text = "Event Log";
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
                richTextBoxInformation.Text += "There is no physical model metadata available.\r\n ";
            }
            else
            {
                if (backgroundWorkerValidationOnly.IsBusy) return;

                _alertValidation = new Form_Alert();

                _alertValidation.Canceled += buttonCancel_Click;
                _alertValidation.Show();
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
                    // Cancel if the row is a new row.
                    if (dataObjectRow.IsNewRow)
                    {
                        return;
                    }

                    // Source Data Object details
                    DataObject sourceDataObject = (DataObject)dataObjectRow.Cells[(int)DataObjectMappingGridColumns.SourceDataObject].Value;

                    var sourceConnectionId = dataObjectRow.Cells[(int)DataObjectMappingGridColumns.SourceConnection].Value.ToString();
                    TeamConnection sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionId);

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
                    TeamConnection targetConnection = GetTeamConnectionByConnectionId(targetConnectionId);

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

                            newRow[DataItemMappingGridColumns.HashKey.ToString()] = Utility.CreateMd5(new string[] {Utility.GetRandomString(100)}, "#");
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
                    Process.Start(GlobalParameters.MetadataPath);
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
            _physicalModelQuery.Canceled += buttonCancel_Click;
            _physicalModelQuery.Show();

            List<string> resultQueryList = new List<string>();

            foreach (var item in checkedListBoxReverseEngineeringAreas.CheckedItems)
            {
                var localConnectionObject = (KeyValuePair<TeamConnection, string>)item;
                resultQueryList.Add(SqlStatementForDataItems(localConnectionObject.Key.DatabaseServer.DatabaseName, true));
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

            foreach (var checkedItem in checkedListBoxReverseEngineeringAreas.CheckedItems)
            {
                var localConnectionObject = (KeyValuePair<TeamConnection, string>)checkedItem;

                var localSqlConnection = new SqlConnection { ConnectionString = localConnectionObject.Key.CreateSqlServerConnectionString(false) };
                var reverseEngineerResults = ReverseEngineerModelMetadata(localSqlConnection, localConnectionObject.Key.DatabaseServer.DatabaseName);

                if (reverseEngineerResults != null)
                {
                    interimDataTable.Merge(reverseEngineerResults);
                }
            }

            interimDataTable.DefaultView.Sort = "[DATABASE_NAME] ASC, [SCHEMA_NAME] ASC, [TABLE_NAME] ASC, [ORDINAL_POSITION] ASC";

            // Flag as new row so it's detected by the save button.
            foreach (DataRow row in interimDataTable.Rows)
            {
                row.SetAdded();
            }

            completeDataTable.Merge(interimDataTable);

            DataTable distinctTable = completeDataTable.DefaultView.ToTable( /*distinct*/ true);

            // Display the results on the data grid.

            //BindingSourcePhysicalModel.In.Invoke((MethodInvoker)delegate {
            //    // Running on the UI thread
            //    form.Label.Text = newText;
            //});

            //BindingSourcePhysicalModel.DataSource = distinctTable;

            _dataGridViewPhysicalModel.Invoke((Action)(() => _dataGridViewPhysicalModel.DataSource = distinctTable));

            //SetDGVValue(distinctTable);


        }

        //private delegate void SetDGVValueDelegate(BindingList<PhysicalModelMetadataJson> items);

        //private void SetDGVValue(DataTable dt)
        //{
        //    if (_dataGridViewPhysicalModel.InvokeRequired)
        //    {
        //        _dataGridViewPhysicalModel.Invoke(new SetDGVValueDelegate(SetDGVValue), dt);
        //    }
        //    else
        //    {
        //        _dataGridViewPhysicalModel.DataSource = dt;
        //    }
        //}

        private void backgroundWorkerReverseEngineering_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                labelResult.Text = "Cancelled!";
            }
            else if (e.Error != null)
            {
                labelResult.Text = "Error: " + e.Error.Message;
            }
            else
            {
                labelResult.Text = "Done!";
                richTextBoxInformation.Text += "\r\nThe phyiscal model was reverse-engineered into the data grid. Don't forget to save your changes if these records should be retained.\r\n";
                // Resize the grid
                GridAutoLayout(_dataGridViewPhysicalModel);
            }
        }
    }
}