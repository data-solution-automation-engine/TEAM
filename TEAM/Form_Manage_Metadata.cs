using DataWarehouseAutomation;
using Newtonsoft.Json;
using Snowflake.Data.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.SqlServer.Management.Dmf;
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

        private bool isStartUp = true;

        private bool isFiltered = false;

        private bool isSorted = false;
        public FormManageMetadata()
        {
            // Placeholder.
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parent"></param>
        public FormManageMetadata(FormMain parent) : base(parent)
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            // Standard call to get the designer controls in place.
            InitializeComponent();

            // Add the Data Object grid view to the tab.
            SetDataObjectGridView();
            SetDataItemGridView();
            SetPhysicalModelGridView();

            // Default setting and start setting of validation counters etc.
            metadataValidations = new MetadataValidations();

            labelHubCount.Text = @"0 Business Concepts";
            labelSatCount.Text = @"0 Context entities";
            labelLnkCount.Text = @"0 Relationships";

            //  Load the grids from the repository.
            richTextBoxInformation.Clear();

            LoadMetadata();

            isStartUp = false;
        }

        private void LoadMetadata()
        {
            // Load the data grids.
            PopulateDataObjectMappingGrid();
            PopulateDataItemMappingGrid();
            PopulatePhysicalModelGrid();

            // Inform the user
            var userFeedback = $"The metadata has been loaded.";
            richTextBoxInformation.AppendText($"{userFeedback}\r\n");
            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"{userFeedback}"));

            // Make sure the validators are good to go.
            AssertValidationDetails();

            // Ensure that the count of object types is updated based on whatever is in the data grid.
            ContentCounter();

            DisplayErrors();

            if (!isStartUp)
            {
                ApplyDataGridViewFiltering();
            }
        }

        /// <summary>
        /// Notify the user of any errors that were detected in the Event Log.
        /// </summary>
        private void DisplayErrors()
        {
            var errors = TeamEventLog.ReportErrors(TeamEventLog);

            if (errors > 0)
            {
                richTextBoxInformation.AppendText($"\r\nPlease note: {errors} error(s) have been found! Please check the Event Log in the menu.\r\n\r\n");
            }
        }

        /// <summary>
        /// Cross-thread event to update text on main form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InformOnDataObjectsResult(object sender, ParseEventArgs e)
        {
            richTextBoxInformation.Text = e.Text;
        }

        /// <summary>
        /// Definition of the Data Object grid view.
        /// </summary>
        private void SetDataObjectGridView()
        {
            // Use custom grid view override class.
            _dataGridViewDataObjects = null;
            _dataGridViewDataObjects = new DataGridViewDataObjects(TeamConfiguration, JsonExportSetting, isStartUp);
            ((ISupportInitialize)(_dataGridViewDataObjects)).BeginInit();

            _dataGridViewDataObjects.OnDataObjectParse += InformOnDataObjectsResult;
            _dataGridViewDataObjects.OnHeaderSort += ApplyFilterOnHeaderSort;
            // TODO This attempt to re-apply the filter on sorted rows failed. Additional rows are still visible in the grid when adding a row to a filtered grid.
            _dataGridViewDataObjects.OnRowExit += ApplyFilterOnRowExit;
            _dataGridViewDataObjects.OnErrorReporting += DisplayErrors;
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
            tabPageDataObjectMapping.Text = @"Data Object Mappings";
            tabPageDataObjectMapping.UseVisualStyleBackColor = true;

            tabPageDataObjectMapping.ResumeLayout(false);
            ((ISupportInitialize)(_dataGridViewDataObjects)).EndInit();
        }

        private void ApplyFilterOnHeaderSort(object sender, FilterEventArgs e)
        {
            if (e.DoFilter)
            {
                ApplyDataGridViewFiltering();

                // Set a sorted flag, all kinds of weird things happen when sorted in the data grid.
                isSorted = true;
            }
        }

        private void ApplyFilterOnRowExit(object sender, FilterEventArgs e)
        {
            if (e.DoFilter)
            {
                ApplyDataGridViewFiltering();
            }
        }

        private void ApplyFilter(object sender, FilterEventArgs e)
        {
            if (e.DoFilter)
            {
                ApplyDataGridViewFiltering();
            }
        }

        /// <summary>
        /// Definition of the Data Item grid view.
        /// </summary>
        private void SetDataItemGridView()
        {
            _dataGridViewDataItems = new DataGridViewDataItems(TeamConfiguration, this);
            ((ISupportInitialize)(_dataGridViewDataItems)).BeginInit();

            _dataGridViewDataItems.OnDataObjectParse += InformOnDataObjectsResult;
            _dataGridViewDataItems.OnHeaderSort += ApplyFilter;
            _dataGridViewDataItems.OnErrorReporting += DisplayErrors;
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
            tabPageDataItemMapping.Text = @"Data Item Mappings";
            tabPageDataItemMapping.UseVisualStyleBackColor = true;

            tabPageDataItemMapping.ResumeLayout(false);
            ((ISupportInitialize)(_dataGridViewDataItems)).EndInit();
        }

        /// <summary>
        /// Definition of the physical model grid view.
        /// </summary>
        private void SetPhysicalModelGridView()
        {
            _dataGridViewPhysicalModel = new DataGridViewPhysicalModel(this);
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

        /// <summary>
        /// Make sure the validators are prepared, files are available etc.
        /// </summary>
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
            checkedListBoxReverseEngineeringAreas.Items.Clear();

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
                var comboBoxValueSource = row[(int)DataObjectMappingGridColumns.SourceConnection].ToString();
                var comboBoxValueTarget = row[(int)DataObjectMappingGridColumns.TargetConnection].ToString();

                var targetDataObjectName = row[(int)DataObjectMappingGridColumns.TargetDataObjectName].ToString();

                if (!localConnectionKeyList.Contains(comboBoxValueSource))
                {
                    if (!userFeedbackList.Contains(comboBoxValueSource))
                    {
                        userFeedbackList.Add(comboBoxValueSource + $", related to {targetDataObjectName}, ");
                    }

                    row[(int)DataObjectMappingGridColumns.SourceConnection] = DBNull.Value;
                }

                if (!localConnectionKeyList.Contains(comboBoxValueTarget))
                {
                    if (!userFeedbackList.Contains(comboBoxValueTarget))
                    {
                        userFeedbackList.Add(comboBoxValueTarget + $", related to {targetDataObjectName}, ");
                    }

                    row[(int)DataObjectMappingGridColumns.TargetConnection] = DBNull.Value;
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
            _dataGridViewDataObjects.DataSource = BindingSourceDataObjectMappings;

            try
            {
                _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.Enabled].Width = 50;
                _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.SourceConnection].Width = 100;
                _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.SourceDataObject].Width = 340;
                _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.TargetConnection].Width = 100;
                _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.TargetDataObject].Width = 340;
                _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Width = 150;
                _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.DrivingKeyDefinition].Width = 80;
                _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.FilterCriterion].Width = 70;

                _dataGridViewDataObjects.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                _dataGridViewDataObjects.Columns[(int)DataObjectMappingGridColumns.FilterCriterion].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch
            {
                // Do nothing.
            }
        }

        /// <summary>
        /// Populates the Attribute Mapping DataGrid directly from an existing JSON file.
        /// </summary>
        private void PopulateDataItemMappingGrid()
        {
            var dataItemMappingFileName = TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();

            if (!File.Exists(dataItemMappingFileName))
            {
                richTextBoxInformation.AppendText("\r\nA new data item mapping file is created, because it did not exist yet.");
                TeamDataItemMapping.CreateEmptyDataItemMappingJson(dataItemMappingFileName, TeamEventLog);
            }

            // Load the file into memory (data table and json list)
            AttributeMapping.GetMetadata(dataItemMappingFileName);

            //Make sure the changes are seen as committed, so that changes can be detected later on.
            AttributeMapping.DataTable.AcceptChanges();

            // Bind the data source.
            BindingSourceDataItemMappings.DataSource = AttributeMapping.DataTable;
            _dataGridViewDataItems.DataSource = BindingSourceDataItemMappings;

            try
            {
                _dataGridViewDataItems.Columns[(int)DataItemMappingGridColumns.SourceDataObject].Width = 350;
                _dataGridViewDataItems.Columns[(int)DataItemMappingGridColumns.SourceDataItem].Width = 350;
                _dataGridViewDataItems.Columns[(int)DataItemMappingGridColumns.TargetDataObject].Width = 350;
                _dataGridViewDataItems.Columns[(int)DataItemMappingGridColumns.TargetDataItem].Width = 350;

                _dataGridViewDataItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                _dataGridViewDataItems.Columns[(int)DataItemMappingGridColumns.TargetDataItem].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch
            {
                // Do nothing.
            }
        }

        /// <summary>
        /// Populates the Physical Model DataGrid from an existing JSON file.
        /// </summary>
        private void PopulatePhysicalModelGrid()
        {
            var physicalModelDirectory = globalParameters.MetadataPath + globalParameters.PhysicalModelDirectory;

            if (!Directory.Exists(physicalModelDirectory))
            {
                richTextBoxInformation.AppendText("\r\nA new physical model directory is created, because it did not exist yet.");
                Directory.CreateDirectory(physicalModelDirectory);
            }

            // Load the file into memory (data table and json list)
            PhysicalModel.GetMetadata(physicalModelDirectory);

            //Make sure the changes are seen as committed, so that changes can be detected later on.
            PhysicalModel.DataTable.AcceptChanges();

            // Bind the data source.
            BindingSourcePhysicalModel.DataSource = PhysicalModel.DataTable;
            _dataGridViewPhysicalModel.DataSource = BindingSourcePhysicalModel;

            // Only apply the sort if there is anything to sort - otherwise the binding source runs into an exception.
            if (PhysicalModel.DataTable.Rows.Count > 0)
            {
                BindingSourcePhysicalModel.Sort = $"{PhysicalModelMappingMetadataColumns.databaseName} ASC, {PhysicalModelMappingMetadataColumns.schemaName} ASC, {PhysicalModelMappingMetadataColumns.tableName} ASC, {PhysicalModelMappingMetadataColumns.ordinalPosition} ASC";
            }

            try
            {
                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.databaseName].Width = 120;
                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.schemaName].Width = 50;
                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.tableName].Width = 305;
                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.columnName].Width = 305;
                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.dataType].Width = 120;
                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.characterLength].Width = 60;
                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.numericPrecision].Width = 60;
                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.numericScale].Width = 60;
                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.ordinalPosition].Width = 60;
                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.primaryKeyIndicator].Width = 60;
                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.multiActiveIndicator].Width = 60;

                _dataGridViewPhysicalModel.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                _dataGridViewPhysicalModel.Columns[(int)PhysicalModelMappingMetadataColumns.multiActiveIndicator].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch
            {
                // Do nothing.
            }
        }

        private DialogResult STAShowDialog(FileDialog dialog)
        {
            var state = new DialogState { FileDialog = dialog };
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

        public void GridAutoLayout()
        {
            richTextBoxInformation.Clear();
            GridAutoLayout(_dataGridViewDataObjects);
            GridAutoLayout(_dataGridViewDataItems);
            GridAutoLayout(_dataGridViewPhysicalModel);
        }

        private void GridAutoLayout(DataGridView dataGridView)
        {
            richTextBoxInformation.Text += $"\r\nThe {dataGridView.Name} grid is being reformatted.";
            try
            {
                //dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                //dataGridView.Columns[dataGridView.ColumnCount - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                // Disable the auto size again (to enable manual resizing).
                for (var i = 0; i < dataGridView.Columns.Count - 1; i++)
                {
                    dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dataGridView.Columns[i].Width = dataGridView.Columns[i].GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
                }
            }
            catch
            {
                // Ignore it for now.
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
                    if (targetDataObjectType == MetadataHandling.DataObjectTypes.CoreBusinessConcept)
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
                    else if (targetDataObjectType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext || targetDataObjectType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
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

            labelHubCount.Text = $@"{hubSet.Count} Business Concepts";
            labelSatCount.Text = (satSet.Count+lsatSet.Count) + @" Context";
            labelLnkCount.Text = lnkSet.Count + @" Relationships";
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
                    _myValidationForm.Invoke((MethodInvoker)delegate { _myValidationForm.Close(); });
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
                    _myJsonForm.Invoke((MethodInvoker)delegate { _myJsonForm.Close(); });
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

            //if (backgroundWorkerReverseEngineering.IsBusy)
            //{
            //    MessageBox.Show(@"The reverse engineer process is running, please wait for this to be completed before saving metadata.", @"Process is running", MessageBoxButtons.OK);
            //}
            //else
            //{
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
            //}
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
                        var previousDataObjectName = "";

                        if (row[DataObjectMappingGridColumns.PreviousTargetDataObjectName.ToString()] != DBNull.Value)
                        {
                            previousDataObjectName = (string)row[DataObjectMappingGridColumns.PreviousTargetDataObjectName.ToString()];
                        }
                        else
                        {
                            previousDataObjectName = (string)row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()];
                        }

                        // Figure out the current / new file name based on the available data (post-change).
                        var newDataObject = (DataObject)row[DataObjectMappingGridColumns.TargetDataObject.ToString()];

                        // If there is no change in the file name / target data object name, the change must be made in an existing file.
                        // If there is a change, the values must be written to a new or other file and an existing segment must be removed.

                        // Note that case is ignored here, because this will cause issues on the file system (which is not case-sensitive by default).
                        if (string.Equals(previousDataObjectName, newDataObject.Name, StringComparison.OrdinalIgnoreCase))
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

                                // Update the old file, and/or delete if there are no segments left.
                                WriteDataObjectMappingsToFile(previousDataObjectName);
                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += $@"There were issues updating the JSON. The error message is {ex.Message}.";
                            }
                        }

                        ((DataTable)BindingSourceDataObjectMappings.DataSource).AcceptChanges();
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

                        ((DataTable)BindingSourceDataObjectMappings.DataSource).AcceptChanges();
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

            ((DataTable)BindingSourceDataObjectMappings.DataSource).AcceptChanges();
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

                string outputFileName = "";
                var schemaExtension = targetDataObject.DataObjectConnection.Extensions.Where(x => x.Key.Equals("schema")).FirstOrDefault();
                if (schemaExtension != null)
                {
                    //TODO - set this up at a suitable time, just before release.
                    //outputFileName = schemaExtension.Value + "." + targetDataObject.Name;
                    outputFileName = targetDataObject.Name;
                }
                else
                {
                    outputFileName = targetDataObject.Name;
                }

                string output = JsonConvert.SerializeObject(vdwDataObjectMappingList, Formatting.Indented);
                string outputFilePath = globalParameters.GetMetadataFilePath(outputFileName);

                try
                {
                    File.WriteAllText(outputFilePath, output);

                    ThreadHelper.SetText(this, richTextBoxInformation, $"The Data Object Mapping for '{targetDataObject.Name}' has been saved.\r\n");
                }
                catch (Exception exception)
                {
                    ThreadHelper.SetText(this, richTextBoxInformation, $"There was an issue saving the Data Object Mapping for '{targetDataObject.Name}', the reported exception is {exception.Message}\r\n");
                }
            }
            else
            {
                //// Deleting a file that has been renamed, removed or otherwise emptied.
                try
                {
                    var fileToDelete = globalParameters.GetMetadataFilePath(targetDataObject.Name);
                    File.Delete(fileToDelete);
                    ThreadHelper.SetText(this, richTextBoxInformation, $"Data Object Mapping for '{targetDataObject.Name}' has been removed.\r\n");
                }
                catch (Exception exception)
                {
                    ThreadHelper.SetText(this, richTextBoxInformation, $"There was an issue deleting the Data Object Mapping for '{targetDataObject.Name}', the reported exception is {exception.Message}\r\n");
                }
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
                var targetDataObject = dataObjectMappings[0].TargetDataObject;
                var vdwDataObjectMappingList = GetVdwDataObjectMappingList(targetDataObject, dataObjectMappings);

                string outputFileName = "";
                var schemaExtension = targetDataObject.DataObjectConnection.Extensions.Where(x => x.Key.Equals("schema")).FirstOrDefault();
                if (schemaExtension != null)
                {
                    //TODO - set this up at a suitable time, just before release.
                    //outputFileName = schemaExtension.Value + "." + targetDataObject.Name;
                    outputFileName = targetDataObject.Name;
                }
                else
                {
                    outputFileName = targetDataObject.Name;
                }


                string output = JsonConvert.SerializeObject(vdwDataObjectMappingList, Formatting.Indented);
                File.WriteAllText(globalParameters.GetMetadataFilePath(outputFileName), output);

                ((DataTable)BindingSourceDataObjectMappings.DataSource).AcceptChanges();

                richTextBoxInformation.Text += $"The Data Object Mapping for '{targetDataObject.Name}' has been saved.\r\n";
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
                DataObjectMappings = dataObjectMappings,
                generationSpecificMetadata = vdwMetadata,
                metadataConfiguration = metadataConfiguration
            };
            return sourceTargetMappingList;
        }

        private void SaveModelPhysicalModelMetadata(DataTable dataTableChanges)
        {
            //Grabbing the generic settings from the main forms
            if (dataTableChanges != null && dataTableChanges.Rows.Count > 0) //Check if there are any changes made at all
            {
                // Create a table exception list to avoid processing the same object multiple times.
                List<string> exceptionList = new List<string>();

                foreach (DataRow row in dataTableChanges.Rows) //Loop through the detected changes.
                {
                    #region Changes and Inserted rows

                    if ((row.RowState & DataRowState.Modified) != 0 || (row.RowState & DataRowState.Added) != 0)
                    {
                        //Grab the attributes into local variables
                        var databaseName = "";
                        var schemaName = "";
                        var tableName = "";

                        if (row[PhysicalModelMappingMetadataColumns.databaseName.ToString()] != DBNull.Value)
                        {
                            databaseName = (string)row[PhysicalModelMappingMetadataColumns.databaseName.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.schemaName.ToString()] != DBNull.Value)
                        {
                            schemaName = (string)row[PhysicalModelMappingMetadataColumns.schemaName.ToString()];
                        }

                        if (row[PhysicalModelMappingMetadataColumns.tableName.ToString()] != DBNull.Value)
                        {
                            tableName = (string)row[PhysicalModelMappingMetadataColumns.tableName.ToString()];
                        }

                        // Save the file. 
                        if (!exceptionList.Contains(databaseName + schemaName + tableName))
                        {
                            WritePhysicalModelToFile(databaseName, schemaName, tableName);
                            exceptionList.Add(databaseName + schemaName + tableName);
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

                        WritePhysicalModelToFile(databaseName, schemaName, tableName);
                    }
                    #endregion
                }
                // All changes have been processed.

                #region Statement execution

                //Committing the changes to the data table
                dataTableChanges.AcceptChanges();
                ((DataTable)BindingSourcePhysicalModel.DataSource).AcceptChanges();

                // Reset all data bound items etc. etc.
                PopulatePhysicalModelGrid();

                richTextBoxInformation.AppendText("The (physical) model metadata snapshot has been saved.\r\n");

                #endregion
            }
        }

        private void SaveDataItemMappingMetadata(DataTable dataTableChanges)
        {
            //Check if there are any changes made at all.
            if (dataTableChanges != null && (dataTableChanges.Rows.Count > 0))
            {
                string dataItemMappingFileName = TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();

                var jsonArray = JsonConvert.DeserializeObject<DataItemMappingJson[]>(File.ReadAllText(dataItemMappingFileName)).ToList();

                List<string> processesDataObjects = new List<string>();

                // Loop through the changes captured in the data table.
                foreach (DataRow row in dataTableChanges.Rows)
                {
                    #region Changes

                    if ((row.RowState & DataRowState.Modified) != 0)
                    {
                        var sourceDataObject = "";
                        var sourceDataItem = "";
                        var targetDataObject = "";
                        var targetDataItem = "";
                        var notes = "";

                        if (row[DataItemMappingGridColumns.SourceDataObject.ToString()] != DBNull.Value)
                        {
                            sourceDataObject = (string)row[DataItemMappingGridColumns.SourceDataObject.ToString()];
                        }

                        if (row[DataItemMappingGridColumns.SourceDataItem.ToString()] != DBNull.Value)
                        {
                            sourceDataItem = (string)row[DataItemMappingGridColumns.SourceDataItem.ToString()];
                        }

                        if (row[DataItemMappingGridColumns.TargetDataObject.ToString()] != DBNull.Value)
                        {
                            targetDataObject = (string)row[DataItemMappingGridColumns.TargetDataObject.ToString()];
                        }

                        if (row[DataItemMappingGridColumns.TargetDataItem.ToString()] != DBNull.Value)
                        {
                            targetDataItem = (string)row[DataItemMappingGridColumns.TargetDataItem.ToString()];
                        }

                        if (row[DataItemMappingGridColumns.Notes.ToString()] != DBNull.Value)
                        {
                            notes = (string)row[DataItemMappingGridColumns.Notes.ToString()];
                        }

                        try
                        {
                            //Retrieves the json segment in the file.
                            string hashKey = "";

                            if (row[DataItemMappingGridColumns.HashKey.ToString()] != DBNull.Value)
                            {
                                hashKey = (string)row[DataItemMappingGridColumns.HashKey.ToString()];
                            }
                            else
                            {
                                string[] inputHashValue = new[] { sourceDataObject, sourceDataItem, targetDataObject, targetDataItem, notes, DateTime.Now.ToString(), Utility.KeyGenerator.GetUniqueKey() };
                                hashKey = Utility.CreateMd5(inputHashValue, Utility.SandingElement);
                            }

                            var jsonHash = jsonArray.FirstOrDefault(obj => obj.attributeMappingHash == hashKey);

                            if (jsonHash == null || jsonHash.attributeMappingHash == "")
                            {
                                richTextBoxInformation.Text += $"The correct segment in the JSON file was not found.\r\n";
                            }
                            else
                            {
                                // Update the values in the JSON segment.
                                jsonHash.sourceTable = sourceDataObject;
                                jsonHash.sourceAttribute = sourceDataItem;
                                jsonHash.targetTable = targetDataObject;
                                jsonHash.targetAttribute = targetDataItem;
                                jsonHash.notes = notes;

                                // Add the change to the list of data object mappings to be refreshed at the end of the process.
                                if (!processesDataObjects.Contains(targetDataObject))
                                {
                                    processesDataObjects.Add(targetDataObject);
                                }
                            }
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
                            string[] inputHashValue = { sourceTable, sourceColumn, targetTable, targetColumn, notes, DateTime.Now.ToString(), Utility.KeyGenerator.GetUniqueKey() };
                            var hashKey = Utility.CreateMd5(inputHashValue, Utility.SandingElement);

                            // Add the values in the JSON segment
                            var jsonSegment = new DataItemMappingJson()
                            {
                                attributeMappingHash = hashKey,
                                sourceTable = sourceTable,
                                sourceAttribute = sourceColumn,
                                targetTable = targetTable,
                                targetAttribute = targetColumn,
                                notes = notes
                            };

                            jsonArray.Add(jsonSegment);

                            // Add the change to the list of data object mappings to be refreshed at the end of the process.
                            if (!processesDataObjects.Contains(targetTable))
                            {
                                processesDataObjects.Add(targetTable);
                            }

                            //Making sure the hash key value is added to the data table as well
                            row[(int)DataItemMappingGridColumns.HashKey] = hashKey;
                        }
                        catch (JsonReaderException exception)
                        {
                            richTextBoxInformation.Text += $"There were issues inserting the JSON segment / record.\r\nThe error message is {exception.Message}";
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
                            //Retrieves the json segment in the file for the given hash returns value or NULL
                            var jsonSegment = jsonArray.FirstOrDefault(obj => obj.attributeMappingHash == hashKey);

                            jsonArray.Remove(jsonSegment);

                            if (jsonSegment != null)
                            {
                                if (string.IsNullOrEmpty(jsonSegment.attributeMappingHash))
                                {
                                    richTextBoxInformation.Text += "The correct segment in the JSON file was not found.\r\n";
                                }
                                else
                                {
                                    //Remove the segment from the JSON
                                    jsonArray.Remove(jsonSegment);
                                }
                            }

                            // Add the change to the list of data object mappings to be refreshed at the end of the process.
                            if (!processesDataObjects.Contains(targetDataObject))
                            {
                                processesDataObjects.Add(targetDataObject);
                            }

                        }
                        catch (JsonReaderException ex)
                        {
                            richTextBoxInformation.Text += $"There were issues applying the JSON update.\r\n{ex.Message}";
                        }
                    }

                    #endregion
                }

                #region Statement execution

                string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                File.WriteAllText(dataItemMappingFileName, output);

                // Update the impacted data object mappings.
                foreach (var dataObject in processesDataObjects)
                {
                    WriteDataObjectMappingsToFile(dataObject);
                }

                //Committing the changes to the data table
                dataTableChanges.AcceptChanges();
                ((DataTable)BindingSourceDataItemMappings.DataSource).AcceptChanges();

                richTextBoxInformation.AppendText($"The Data Item Mapping metadata has been saved.\r\n");

                #endregion
            }
        }

        #region Parse process

        private void ButtonParse_Click(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();

            #region Preparation

            // Local boolean to manage whether activation is OK to go ahead.
            bool activationContinue = true;

            // Check if there are any outstanding saves / commits in the data grid
            var dataTableTableMappingChanges = ((DataTable)BindingSourceDataObjectMappings.DataSource).GetChanges();
            var dataTableAttributeMappingChanges = ((DataTable)BindingSourceDataItemMappings.DataSource).GetChanges();
            var dataTablePhysicalModelChanges = ((DataTable)BindingSourcePhysicalModel.DataSource).GetChanges();

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

            ApplyDataGridViewFiltering();
            DisplayErrors();
        }

        // This event handler updates the progress.
        private void backgroundWorkerParse_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Show the progress in main form
            labelResult.Text = (e.ProgressPercentage + @"%");

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
                try
                {
                    // Manage cancellation.
                    if (worker.CancellationPending)
                    {
                        continue;
                    }

                    if (!dataObjectMappingGridViewRow.IsNewRow)
                    {
                        var targetDataObjectName = "";

                        if (!string.IsNullOrEmpty(dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].Value.ToString()))
                        {
                            targetDataObjectName = dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].Value.ToString();
                        }
                        else
                        {
                            DataObject targetDataObject = (DataObject)dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.TargetDataObject.ToString()].Value;
                            targetDataObjectName = targetDataObject.Name;
                        }

                        if (!targetNameList.Contains(targetDataObjectName))
                        {
                            LogMetadataEvent($"Parsing '{targetDataObjectName}'.", EventTypes.Information);

                            try
                            {
                                var targetDataObject = (DataObject)dataObjectMappingGridViewRow.Cells[DataObjectMappingGridColumns.TargetDataObject.ToString()].Value;

                                WriteDataObjectMappingsToFile(targetDataObject);

                                LogMetadataEvent($"  --> Saved as '{globalParameters.GetMetadataFilePath(targetDataObject.Name)}'.", EventTypes.Information);

                                targetNameList.Add(targetDataObjectName);
                            }
                            catch (JsonReaderException ex)
                            {
                                LogMetadataEvent($"There were issues updating the JSON. The error message is {ex.Message}.", EventTypes.Error);
                            }

                            // Normalize all values in array against a 0-100 scale to support the progress bar relative to the number of commands to execute.
                            var normalizedValue = 1 + (counter - 0) * (100 - 1) / (filteredRowSet.Count - 0);
                            worker.ReportProgress(normalizedValue);
                            counter++;
                        }
                    }
                }
                catch (Exception exception)
                {
                    LogMetadataEvent($"A row in the grid could not be parsed. The reported exception is {exception.Message}.", EventTypes.Error);
                }
            }

            ((DataTable)BindingSourceDataObjectMappings.DataSource).AcceptChanges();

            // Manage cancellation.
            if (worker != null && worker.CancellationPending)
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
                                  (row.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Contains(textBoxFilterCriterion.Text, StringComparison.OrdinalIgnoreCase) ||
                                   row.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Contains(textBoxFilterCriterion.Text, StringComparison.OrdinalIgnoreCase)))
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
                                  (row.Cells[(int)DataItemMappingGridColumns.TargetDataObject].Value.ToString().Contains(textBoxFilterCriterion.Text, StringComparison.OrdinalIgnoreCase) ||
                                   row.Cells[(int)DataItemMappingGridColumns.SourceDataObject].Value.ToString().Contains(textBoxFilterCriterion.Text, StringComparison.OrdinalIgnoreCase)))
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
                filteredRowSet = dataTable.AsEnumerable().Where(row => row[(int)DataObjectMappingGridColumns.TargetDataObjectName].ToString().Contains(textBoxFilterCriterion.Text, StringComparison.OrdinalIgnoreCase) ||
                                                                       row[(int)DataObjectMappingGridColumns.SourceDataObjectName].ToString().Contains(textBoxFilterCriterion.Text, StringComparison.OrdinalIgnoreCase))
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
                filteredRowSet = dataTable.AsEnumerable().Where(row => row[(int)DataItemMappingGridColumns.TargetDataObject].ToString().Contains(textBoxFilterCriterion.Text, StringComparison.OrdinalIgnoreCase) ||
                                                                       row[(int)DataItemMappingGridColumns.SourceDataObject].ToString().Contains(textBoxFilterCriterion.Text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else
            {
                filteredRowSet = dataTable.AsEnumerable().ToList();
            }

            return filteredRowSet;
        }

        private string GetDgmlCategory(string node)
        {
            var returnValue = "";

            if (node.IsPsa(TeamConfiguration))
            {
                returnValue = "Persistent Staging Area";
            }
            else if (node.IsDataVaultHub(TeamConfiguration))
            {
                returnValue = "Core Business Concept";
            }
            else if (node.IsDataVaultLink(TeamConfiguration))
            {
                returnValue = "Natural Business Relationship";
            }
            else if (node.IsDataVaultSatellite(TeamConfiguration))
            {
                returnValue = "Context";
            }
            else if (node.IsDataVaultLinkSatellite(TeamConfiguration))
            {
                returnValue = "Context";
            }
            return returnValue;
        }

        /// <summary>
        /// Save existing metadata as a Directed Graph Markup Language (DGML) file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                var selectedFile = theDialog.FileName;

                bool skipSource = false;
                bool skipPsa = false;
                bool skipRelatedDataObjects = false;

                // Get the JSON files and load these into memory.
                var teamDataObjectMappingFileCombinations = new TeamDataObjectMappingsFileCombinations(globalParameters.MetadataPath);
                teamDataObjectMappingFileCombinations.GetMetadata(globalParameters);

                // DGML part strings - nodes.
                var nodeBuilder = new List<string> { "  <Nodes>" };

                // DGML part strings - edges.
                var edgeBuilder = new List<string> { "  <Links>" };

                // Create a filtered list to limit DGML output, if available.
                var filteredDataObjectGridViewRows = GetFilteredDataObjectMappingDataGridViewRows();
                List<string> filteredTargetDataObjects = filteredDataObjectGridViewRows.Select(x => (DataObject)x.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value).Select(y => y.Name).ToList();

                // Create all the nodes and edges.
                foreach (var fileCombination in teamDataObjectMappingFileCombinations.DataObjectMappingsFileCombinations)
                {
                    List<DataObjectMapping> dataObjectMappings = fileCombination.DataObjectMappings.DataObjectMappings;

                    foreach (var dataObjectMapping in dataObjectMappings)
                    {
                        // Do not render mappings that are not enabled.
                        if (dataObjectMapping.Enabled == false)
                            continue;

                        DataClassification classification = dataObjectMapping.MappingClassifications.FirstOrDefault();

                        if (skipPsa && classification.Classification == "PersistentStagingArea")
                            continue;

                        if (skipSource && classification.Classification == "StagingArea")
                            continue;

                        // Do not render if not in filtered list.
                        if (!filteredTargetDataObjects.Contains(dataObjectMapping.TargetDataObject.Name))
                            continue;

                        #region Target node (data object)

                        // The target is set once for the mapping.
                        DataObject targetDataObject = dataObjectMapping.TargetDataObject;
                        var targetConnectionKey = targetDataObject.DataObjectConnection.DataConnectionString;
                        var targetConnection = TeamConnection.GetTeamConnectionByConnectionKey(targetConnectionKey, TeamConfiguration, TeamEventLog);
                        KeyValuePair<string, string> fullyQualifiedObjectTarget = MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObject, targetConnection).FirstOrDefault();

                        var targetNodeName = fullyQualifiedObjectTarget.Key + '.' + fullyQualifiedObjectTarget.Value;

                        var targetCategoryName = GetDgmlCategory(fullyQualifiedObjectTarget.Value);

                        // Add the target node, if not existing already.
                        var localTargetNode = "     <Node Id=\"" + targetNodeName + "\" Category=\"" + targetCategoryName + "\" Group=\"Collapsed\" Label=\"" + fullyQualifiedObjectTarget.Value + "\" />";
                        if (!nodeBuilder.Contains(localTargetNode))
                        {
                            nodeBuilder.Add(localTargetNode);
                        }

                        var targetConnectionName = targetConnectionKey;

                        // Add the connection node, if not existing already.
                        var localTargetConnectionNode = "     <Node Id=\"" + targetConnectionName + "\" Group=\"Collapsed\" Label=\"" + targetConnectionName + "\" />";
                        if (!nodeBuilder.Contains(localTargetConnectionNode))
                        {
                            nodeBuilder.Add(localTargetConnectionNode);
                        }

                        // Add the target node to the connection (contains), if this hasn't been done already.
                        var targetEdge = "     <Link Source=\"" + targetConnectionName + "\" Target=\"" + targetNodeName + "\" Category=\"Contains\"/>";
                        if (!edgeBuilder.Contains(targetEdge))
                        {
                            edgeBuilder.Add(targetEdge);
                        }

                        #endregion

                        #region Source node (data object)

                        try
                        {
                            foreach (var sourceDataObjectDynamic in dataObjectMapping.SourceDataObjects)
                            {
                                var intermediateJson = JsonConvert.SerializeObject(sourceDataObjectDynamic);

                                if (JsonConvert.DeserializeObject(intermediateJson).ContainsKey("dataQueryCode"))
                                {
                                    // If the source is a query.
                                    DataQuery tempDataItem = JsonConvert.DeserializeObject<DataQuery>(intermediateJson);

                                    if (tempDataItem.DataQueryConnection != null)
                                    {
                                        var sourceNodeNameDataQuery = tempDataItem.DataQueryConnection.DataConnectionString + '.' + tempDataItem.DataQueryCode;
                                        var connectionNameDataQuery = tempDataItem.DataQueryConnection.DataConnectionString;

                                        // Add the source node, if not existing already.
                                        var localSourceNode = "     <Node Id=\"" + sourceNodeNameDataQuery + "\" Category=\"" + "" + "" + "\" Group=\"Collapsed\" Label=\"" + sourceNodeNameDataQuery + "\" />";
                                        if (!nodeBuilder.Contains(localSourceNode))
                                        {
                                            nodeBuilder.Add(localSourceNode);
                                        }

                                        // Add the connection node, if not existing already.
                                        var localDataQueryNode = "     <Node Id=\"" + connectionNameDataQuery + "\" Group=\"Collapsed\" Label=\"" + connectionNameDataQuery + "\" />";
                                        if (!nodeBuilder.Contains(localDataQueryNode))
                                        {
                                            nodeBuilder.Add(localDataQueryNode);
                                        }

                                        // Add the source node to the connection, if this hasn't been done already.
                                        var dataQueryEdge = "     <Link Source=\"" + connectionNameDataQuery + "\" Target=\"" + sourceNodeNameDataQuery + "\" Category=\"Contains\"/>";
                                        if (!edgeBuilder.Contains(dataQueryEdge))
                                        {
                                            edgeBuilder.Add(dataQueryEdge);
                                        }
                                    }
                                }
                                else
                                {
                                    // The source is an object.
                                    var singleSourceDataObject = (DataObject)JsonConvert.DeserializeObject<DataObject>(intermediateJson);

                                    if (singleSourceDataObject.DataObjectConnection != null)
                                    {
                                        string sourceConnectionString = singleSourceDataObject.DataObjectConnection.DataConnectionString;
                                        var sourceConnectionInternalId = TeamConnection.GetTeamConnectionByConnectionKey(sourceConnectionString, TeamConfiguration, TeamEventLog).ConnectionInternalId;

                                        var sourceConnection = TeamConnection.GetTeamConnectionByConnectionInternalId(sourceConnectionInternalId, TeamConfiguration, TeamEventLog);

                                        Dictionary<string, string> fullyQualifiedObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(singleSourceDataObject, sourceConnection);

                                        var uniqueValue = fullyQualifiedObjectSource.FirstOrDefault();
                                        var sourceNodeNameFullyQualified = uniqueValue.Key + '.' + uniqueValue.Value;
                                        var sourceCategoryName = GetDgmlCategory(uniqueValue.Value);

                                        // Add the source node, if not existing already.
                                        var localSourceNode = "     <Node Id=\"" + sourceNodeNameFullyQualified + "\" Category=\"" + sourceCategoryName + "\" Group=\"Collapsed\" Label=\"" + uniqueValue.Value + "\" />";
                                        if (!nodeBuilder.Contains(localSourceNode))
                                        {
                                            nodeBuilder.Add(localSourceNode);
                                        }

                                        var sourceConnectionName = sourceConnectionString;

                                        // Add the connection node, if not existing already.
                                        var localSourceConnectionNode = "     <Node Id=\"" + sourceConnectionName + "\" Group=\"Collapsed\" Label=\"" + sourceConnectionName + "\" />";
                                        if (!nodeBuilder.Contains(localSourceConnectionNode))
                                        {
                                            nodeBuilder.Add(localSourceConnectionNode);
                                        }

                                        // Add the source node to the connection, if this hasn't been done already.
                                        var sourceConnectionEdge = "     <Link Source=\"" + sourceConnectionName + "\" Target=\"" + sourceNodeNameFullyQualified + "\" Category=\"Contains\"/>";
                                        if (!edgeBuilder.Contains(sourceConnectionEdge))
                                        {
                                            edgeBuilder.Add(sourceConnectionEdge);
                                        }

                                        // Build the source-target relationship between the data objects.
                                        var dataObjectMappingEdge = "     <Link Source=\"" + sourceNodeNameFullyQualified + "\" Target=\"" + targetNodeName + "\" />";
                                        //var dataObjectMappingEdge = "     <Link Source=\"" + sourceNodeNameFullyQualified + "\" Target=\"" + targetNodeName + "\" BusinessKeyDefinition=\"@" + JsonConvert.SerializeObject(dataObjectMapping.BusinessKeys) + "\" />";
                                        if (!edgeBuilder.Contains(dataObjectMappingEdge))
                                        {
                                            edgeBuilder.Add(dataObjectMappingEdge);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered generating the DGML file, when building the nodes: {exception.Message}"));
                        }

                        #endregion

                        if (!skipRelatedDataObjects)
                        {
                            #region RelatedDataObjects

                            try
                            {
                                if (dataObjectMapping.RelatedDataObjects != null)
                                {
                                    foreach (var relatedDataObject in dataObjectMapping.RelatedDataObjects)
                                    {
                                        if (relatedDataObject.DataObjectConnection != null && relatedDataObject.Name != "Metadata")
                                        {
                                            var relatedDataObjectConnectionKey = relatedDataObject.DataObjectConnection.DataConnectionString;
                                            var relatedDataObjectConnection = TeamConnection.GetTeamConnectionByConnectionKey(relatedDataObjectConnectionKey, TeamConfiguration, TeamEventLog);
                                            KeyValuePair<string, string> fullyQualifiedRelatedDataObjectName =
                                                MetadataHandling.GetFullyQualifiedDataObjectName(relatedDataObject, relatedDataObjectConnection).FirstOrDefault();

                                            var fullyQualifiedRelatedDataObjectNodeName = fullyQualifiedRelatedDataObjectName.Key + '.' + fullyQualifiedRelatedDataObjectName.Value;

                                            var relatedDataObjectEdge = "     <Link Source=\"" + targetNodeName + "\" Target=\"" + fullyQualifiedRelatedDataObjectNodeName + "\" RelatedDataObject=\"" +
                                                                        targetNodeName + "\" />";
                                            if (!edgeBuilder.Contains(relatedDataObjectEdge))
                                            {
                                                edgeBuilder.Add(relatedDataObjectEdge);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered generating the DGML file, when building the related data objects: {exception.Message}"));
                            }

                            #endregion
                        }

                        #region Data Item mappings

                        try
                        {
                            if (dataObjectMapping.DataItemMappings != null)
                            {
                                foreach (var dataItemMapping in dataObjectMapping.DataItemMappings)
                                {
                                    var sourceDataObject = dataObjectMapping.SourceDataObjects.FirstOrDefault();

                                    var sourceDataObjectName = "";
                                    var sourceDataObjectFullyQualified = "";

                                    // Check if the source data object is a query, or a data object.
                                    var intermediateJson = JsonConvert.SerializeObject(sourceDataObject);

                                    if (!JsonConvert.DeserializeObject(intermediateJson).ContainsKey("dataQueryCode"))
                                    {
                                        // It's an object.
                                        var singleSourceDataObject = (DataObject)JsonConvert.DeserializeObject<DataObject>(intermediateJson);

                                        string sourceConnectionString = singleSourceDataObject.DataObjectConnection.DataConnectionString;
                                        var sourceConnectionInternalId = TeamConnection.GetTeamConnectionByConnectionKey(sourceConnectionString, TeamConfiguration, TeamEventLog).ConnectionInternalId;
                                        var sourceConnection = TeamConnection.GetTeamConnectionByConnectionInternalId(sourceConnectionInternalId, TeamConfiguration, TeamEventLog);

                                        Dictionary<string, string> fullyQualifiedObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(singleSourceDataObject, sourceConnection);
                                        var uniqueValue = fullyQualifiedObjectSource.FirstOrDefault();
                                        sourceDataObjectFullyQualified = uniqueValue.Key + '.' + uniqueValue.Value;
                                    }
                                    else
                                    {
                                        // It's a query.
                                        var singleSourceDataObject = (DataQuery)JsonConvert.DeserializeObject<DataQuery>(intermediateJson);
                                        sourceDataObjectFullyQualified = singleSourceDataObject.DataQueryCode;
                                    }

                                    //* Data Items **/

                                    var sourceDataItemNameFullyQualified = "";

                                    // Check if the source data item is a query, or a data object.
                                    var sourceDataItem = dataItemMapping.SourceDataItems.FirstOrDefault();
                                    var intermediateDataItemJson = JsonConvert.SerializeObject(sourceDataItem);

                                    if (!JsonConvert.DeserializeObject(intermediateDataItemJson).ContainsKey("dataQueryCode"))
                                    {
                                        // It's an object.
                                        sourceDataItem = (DataItem)JsonConvert.DeserializeObject<DataItem>(intermediateDataItemJson);
                                        sourceDataItemNameFullyQualified = sourceDataObjectFullyQualified + "." + sourceDataItem.Name;
                                    }
                                    else
                                    {
                                        // It's a query.
                                        sourceDataItem = (DataQuery)JsonConvert.DeserializeObject<DataQuery>(intermediateDataItemJson);
                                        sourceDataItemNameFullyQualified = sourceDataObjectFullyQualified + "." + sourceDataItem.DataQueryCode;
                                    }

                                    var targetDataItemName = dataItemMapping.TargetDataItem.Name;
                                    var targetDataItemNameFullyQualified = targetNodeName + "." + targetDataItemName;

                                    // Add the source node, if not existing already.
                                    var localSourceDataItemNode = "     <Node Id=\"" + sourceDataItemNameFullyQualified + "\" Label=\"" + sourceDataItem.Name + "\" />";
                                    if (!nodeBuilder.Contains(localSourceDataItemNode))
                                    {
                                        nodeBuilder.Add(localSourceDataItemNode);
                                    }

                                    // Add the target node, if not existing already.
                                    var localTargetDataItemNode = "     <Node Id=\"" + targetDataItemNameFullyQualified + "\" Label=\"" + targetDataItemName + "\" />";
                                    if (!nodeBuilder.Contains(localTargetDataItemNode))
                                    {
                                        nodeBuilder.Add(localTargetDataItemNode);
                                    }

                                    // Build the source-target relationship between the data items.
                                    var dataItemMappingEdge = "     <Link Source=\"" + sourceDataItemNameFullyQualified + "\" Target=\"" + targetDataItemNameFullyQualified + "\" />";
                                    if (!edgeBuilder.Contains(dataItemMappingEdge))
                                    {
                                        edgeBuilder.Add(dataItemMappingEdge);
                                    }

                                    // Add the source data item to the data object, if this hasn't been done already.
                                    var sourceDataItemDataObjectEdge = "     <Link Source=\"" + sourceDataObjectFullyQualified + "\" Target=\"" + sourceDataItemNameFullyQualified + "\" Category=\"Contains\"/>";
                                    if (!edgeBuilder.Contains(sourceDataItemDataObjectEdge))
                                    {
                                        edgeBuilder.Add(sourceDataItemDataObjectEdge);
                                    }

                                    // Add the target data item to the data object, if this hasn't been done already.
                                    var targetDataItemDataObjectEdge = "     <Link Source=\"" + targetNodeName + "\" Target=\"" + targetDataItemNameFullyQualified + "\" Category=\"Contains\"/>";
                                    if (!edgeBuilder.Contains(targetDataItemDataObjectEdge))
                                    {
                                        edgeBuilder.Add(targetDataItemDataObjectEdge);
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered generating the DGML file, when adding data item mappings: {exception.Message}"));
                        }

                        #endregion

                        #region Business Keys

                        try
                        {
                            if (dataObjectMapping.BusinessKeys != null)
                            {
                                var sourceDataObject = dataObjectMapping.SourceDataObjects.FirstOrDefault();
                                var sourceDataObjectFullyQualified = "";

                                // Check if the source data object is a query, or a data object.
                                var intermediateJson = JsonConvert.SerializeObject(sourceDataObject);

                                if (!JsonConvert.DeserializeObject(intermediateJson).ContainsKey("dataQueryCode"))
                                {
                                    // It's an object.
                                    var singleSourceDataObject = (DataObject)JsonConvert.DeserializeObject<DataObject>(intermediateJson);

                                    string sourceConnectionString = singleSourceDataObject.DataObjectConnection.DataConnectionString;
                                    var sourceConnectionInternalId = TeamConnection.GetTeamConnectionByConnectionKey(sourceConnectionString, TeamConfiguration, TeamEventLog).ConnectionInternalId;
                                    var sourceConnection = TeamConnection.GetTeamConnectionByConnectionInternalId(sourceConnectionInternalId, TeamConfiguration, TeamEventLog);

                                    Dictionary<string, string> fullyQualifiedObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(singleSourceDataObject, sourceConnection);
                                    var uniqueValue = fullyQualifiedObjectSource.FirstOrDefault();
                                    sourceDataObjectFullyQualified = uniqueValue.Key + '.' + uniqueValue.Value;
                                }
                                else
                                {
                                    // It's a query.
                                    var singleSourceDataObject = (DataQuery)JsonConvert.DeserializeObject<DataQuery>(intermediateJson);
                                    sourceDataObjectFullyQualified = singleSourceDataObject.DataQueryCode;
                                }

                                //** Business Key **/

                                foreach (var businessKey in dataObjectMapping.BusinessKeys)
                                {
                                    var surrogateKeyFullyQualified = targetNodeName + "." + businessKey.SurrogateKey;

                                    // Add the surrogate key node, if not existing already.
                                    var localSurrogateKeyNode = "     <Node Id=\"" + surrogateKeyFullyQualified + "\" Label=\"" + businessKey.SurrogateKey + "\" />";
                                    if (!nodeBuilder.Contains(localSurrogateKeyNode))
                                    {
                                        nodeBuilder.Add(localSurrogateKeyNode);
                                    }

                                    // Add the surrogate to the data object, if this hasn't been done already.
                                    var targetSurrogateKeyDataObjectEdge = "     <Link Source=\"" + targetNodeName + "\" Target=\"" + surrogateKeyFullyQualified + "\" Category=\"Contains\"/>";
                                    if (!edgeBuilder.Contains(targetSurrogateKeyDataObjectEdge))
                                    {
                                        edgeBuilder.Add(targetSurrogateKeyDataObjectEdge);
                                    }

                                    // For each of the business key components, create the source and target items as well as a mapping to the surrogate key.
                                    foreach (var businessKeyComponentMapping in businessKey.BusinessKeyComponentMapping)
                                    {
                                        var sourceBusinessKeyDataItemNameFullyQualified = "";

                                        // Check if the source data item is a query, or a data object.
                                        var sourceBusinessKeyDataItem = businessKeyComponentMapping.SourceDataItems.FirstOrDefault();
                                        var intermediateDataItemJson = JsonConvert.SerializeObject(sourceBusinessKeyDataItem);

                                        if (!JsonConvert.DeserializeObject(intermediateDataItemJson).ContainsKey("dataQueryCode"))
                                        {
                                            // It's an object.
                                            sourceBusinessKeyDataItem = (DataItem)JsonConvert.DeserializeObject<DataItem>(intermediateDataItemJson);
                                            sourceBusinessKeyDataItemNameFullyQualified = sourceDataObjectFullyQualified + "." + sourceBusinessKeyDataItem.Name;
                                        }
                                        else
                                        {
                                            // It's a query.
                                            sourceBusinessKeyDataItem = (DataQuery)JsonConvert.DeserializeObject<DataQuery>(intermediateDataItemJson);
                                            sourceBusinessKeyDataItemNameFullyQualified = sourceDataObjectFullyQualified + "." + sourceBusinessKeyDataItem.DataQueryCode;
                                        }

                                        var targetBusinessKeyDataItemName = businessKeyComponentMapping.TargetDataItem.Name;
                                        var targetBusinessKeyDataItemNameFullyQualified = targetNodeName + "." + targetBusinessKeyDataItemName;

                                        // Add the source node, if not existing already.
                                        var localSourceDataItemNode = "     <Node Id=\"" + sourceBusinessKeyDataItemNameFullyQualified + "\" Label=\"" + sourceBusinessKeyDataItem.Name + "\" />";
                                        if (!nodeBuilder.Contains(localSourceDataItemNode))
                                        {
                                            nodeBuilder.Add(localSourceDataItemNode);
                                        }

                                        // Add the source data item to the data object, if this hasn't been done already.
                                        var sourceDataItemDataObjectEdge = "     <Link Source=\"" + sourceDataObjectFullyQualified + "\" Target=\"" + sourceBusinessKeyDataItemNameFullyQualified + "\" Category=\"Contains\"/>";
                                        if (!edgeBuilder.Contains(sourceDataItemDataObjectEdge))
                                        {
                                            edgeBuilder.Add(sourceDataItemDataObjectEdge);
                                        }

                                        if (!dataObjectMapping.TargetDataObject.Name.IsDataVaultLink(TeamConfiguration))
                                        {
                                            // Add the target node, if not existing already.
                                            var localTargetDataItemNode = "     <Node Id=\"" + targetBusinessKeyDataItemNameFullyQualified + "\" Label=\"" + targetBusinessKeyDataItemName + "\" />";
                                            if (!nodeBuilder.Contains(localTargetDataItemNode))
                                            {
                                                nodeBuilder.Add(localTargetDataItemNode);
                                            }

                                            // Add the target data item to the data object, if this hasn't been done already.
                                            var targetDataItemDataObjectEdge = "     <Link Source=\"" + targetNodeName + "\" Target=\"" + targetBusinessKeyDataItemNameFullyQualified +
                                                                               "\" Category=\"Contains\"/>";
                                            if (!edgeBuilder.Contains(targetDataItemDataObjectEdge))
                                            {
                                                edgeBuilder.Add(targetDataItemDataObjectEdge);
                                            }

                                            // Build the source-target relationship between the data items.
                                            var dataItemMappingEdge = "     <Link Source=\"" + sourceBusinessKeyDataItemNameFullyQualified + "\" Target=\"" + targetBusinessKeyDataItemNameFullyQualified + "\" />";
                                            if (!edgeBuilder.Contains(dataItemMappingEdge))
                                            {
                                                edgeBuilder.Add(dataItemMappingEdge);
                                            }
                                        }

                                        // Build the source-target relationship between the source data item and the surrogate key.
                                        var surrogateKeyEdge = "     <Link Source=\"" + sourceBusinessKeyDataItemNameFullyQualified + "\" Target=\"" + surrogateKeyFullyQualified + "\" />";
                                        if (!edgeBuilder.Contains(surrogateKeyEdge))
                                        {
                                            edgeBuilder.Add(surrogateKeyEdge);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered generating the DGML file, when adding data item mappings: {exception.Message}"));
                        }

                        #endregion
                    }
                }

                // End of edges and containers.
                nodeBuilder.Add("  </Nodes>");
                edgeBuilder.Add("  </Links>");

                // Start the creation of the final the DGML file.
                var dgmlExtract = new StringBuilder();
                dgmlExtract.AppendLine("<?xml version=\"1.0\" encoding=\"utf - 8\"?>");
                dgmlExtract.AppendLine("<DirectedGraph ZoomLevel=\" - 1\" xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">");

                foreach (var node in nodeBuilder)
                {
                    dgmlExtract.AppendLine(node);
                }

                foreach (var edge in edgeBuilder)
                {
                    dgmlExtract.AppendLine(edge);
                }

                // Add categories.
                dgmlExtract.AppendLine("  <Categories>");
                dgmlExtract.AppendLine("    <Category Id = \"Sources\" Label = \"Sources\" Background = \"#FFE51400\" IsTag = \"True\" /> ");
                dgmlExtract.AppendLine("    <Category Id = \"Landing Area\" Label = \"Landing Area\" IsTag = \"True\" /> ");
                dgmlExtract.AppendLine("    <Category Id = \"Persistent Staging Area\" Label = \"Persistent Staging Area\" IsTag = \"True\" /> ");
                dgmlExtract.AppendLine("    <Category Id = \"Hub\" Label = \"Hub\" IsTag = \"True\" /> ");
                dgmlExtract.AppendLine("    <Category Id = \"Link\" Label = \"Link\" IsTag = \"True\" /> ");
                dgmlExtract.AppendLine("    <Category Id = \"Satellite\" Label = \"Satellite\" IsTag = \"True\" /> ");
                dgmlExtract.AppendLine("    <Category Id = \"Subject Area\" Label = \"Subject Area\" IsTag = \"True\" /> ");
                dgmlExtract.AppendLine("  </Categories>");

                // Add properties.
                dgmlExtract.AppendLine("  <Properties>");
                dgmlExtract.AppendLine("    <Property Id=\"BusinessKeyDefinition\" DataType=\"System.String\" />");
                dgmlExtract.AppendLine("    <Property Id=\"RelatedDataObject\" DataType=\"System.String\" />");
                dgmlExtract.AppendLine("  </Properties>");

                // Add category styles.
                dgmlExtract.AppendLine("  <Styles >");

                dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Source\" ValueLabel = \"Has category\" >");
                dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Source')\" />");
                dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FFFFFFFF\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                dgmlExtract.AppendLine("    </Style >");

                dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Landing Area\" ValueLabel = \"Has category\" >");
                dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Landing Area')\" />");
                dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FE000000\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FE6E6A69\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                dgmlExtract.AppendLine("    </Style >");

                dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Persistent Staging Area\" ValueLabel = \"Has category\" >");
                dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Persistent Staging Area')\" />");
                dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FA000000\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FA6E6A69\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                dgmlExtract.AppendLine("    </Style >");

                dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Core Business Concept\" ValueLabel = \"Has category\" >");
                dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Core Business Concept')\" />");
                dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FF6495ED\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                dgmlExtract.AppendLine("    </Style >");

                dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Natural Business Relationship\" ValueLabel = \"Has category\" >");
                dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Natural Business Relationship')\" />");
                dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FFB22222\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                dgmlExtract.AppendLine("    </Style >");

                dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Context\" ValueLabel = \"Has category\" >");
                dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Context')\" />");
                dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FFC0A000\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                dgmlExtract.AppendLine("    </Style >");

                dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Subject Area\" ValueLabel = \"Has category\" >");
                dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Subject Area')\" />");
                dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FFFFFFFF\" />");
                dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                dgmlExtract.AppendLine("    </Style >");

                dgmlExtract.AppendLine("  </Styles >");

                dgmlExtract.AppendLine("</DirectedGraph>");
                // End of graph file creation.

                // Writing the output.
                using (StreamWriter outfile = new StreamWriter(selectedFile))
                {
                    outfile.Write(dgmlExtract.ToString());
                    outfile.Close();
                }

                richTextBoxInformation.AppendText("The DGML metadata file file://" + selectedFile + " has been saved successfully.");
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
            richTextBoxInformation.Text += $"\r\nThe process has started at {DateTime.Now:HH:mm:ss tt}, and the screen may become unresponsive in the meantime.";

            checkedListBoxReverseEngineeringAreas.Enabled = false;

            if (backgroundWorkerValidationOnly.IsBusy)
            {
                MessageBox.Show(@"A validation process is still running, please wait for this to complete and try again.", @"Validation in progress", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var changesDataTable = ((DataTable)BindingSourcePhysicalModel.DataSource).GetChanges();

            if (changesDataTable != null && changesDataTable.Rows.Count > 0)
            {
                MessageBox.Show(@"There are unsaved changes in the physical model, please save your changes first before reverse-engineering.", @"Please save changes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // The temporary merge data table.
            var interimDataTable = new DataTable();

            // The full data table, to start with.
            DataTable completeDataTable = (DataTable)BindingSourcePhysicalModel.DataSource;
            var existingFilter = completeDataTable.DefaultView.RowFilter;

            try
            {
                // Loop through the checked connections on the reverse-engineering form.
                foreach (var checkedItem in checkedListBoxReverseEngineeringAreas.CheckedItems)
                {
                    if (checkedItem != null)
                    {
                        // Get the connection details.
                        var teamConnection = (KeyValuePair<TeamConnection, string>)checkedItem;

                        var updateMessage = $"Reverse-engineering is now attempted for connection '{teamConnection.Value}'";
                        TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, updateMessage));
                        ThreadHelper.SetText(this, richTextBoxInformation, $"\r\n{updateMessage}");

                        try
                        {
                            // Get the subset of mappings for which the data objects need ot be reverse-engineered.
                            var filteredRows = GetFilteredDataObjectMappingDataTableRows();

                            // Get the information from the database catalog.
                            var reverseEngineerResults = ReverseEngineerModelMetadata(teamConnection.Key, filteredRows);

                            // For Snowflake only, convert INT64 back to INT32.
                            if (teamConnection.Key.TechnologyConnectionType == TechnologyConnectionType.Snowflake && reverseEngineerResults != null && reverseEngineerResults.Rows.Count>0)
                            {
                                DataTable dtCloned = reverseEngineerResults.Clone();
                                dtCloned.Columns["ordinalPosition"].DataType = typeof(Int32);
                                foreach (DataRow row in reverseEngineerResults.Rows)
                                {
                                    dtCloned.ImportRow(row);
                                }

                                reverseEngineerResults = dtCloned;
                                dtCloned.Dispose();
                            }

                            if (reverseEngineerResults == null || reverseEngineerResults.Rows.Count == 0)
                            {
                                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"Reverse-engineering against connection '{teamConnection.Value}' did not return any results."));
                            }

                            if (reverseEngineerResults != null)
                            {
                                interimDataTable.Merge(reverseEngineerResults);
                            }

                            Thread.CurrentThread.Join(0);

                            ThreadHelper.SetText(this, richTextBoxInformation, $"\r\n - Completed '{teamConnection.Key.ConnectionKey}' reverse-engineering at {DateTime.Now:HH:mm:ss tt}.");
                        }
                        catch (Exception exception)
                        {
                            ThreadHelper.SetText(this, richTextBoxInformation, $"\r\n - There was an issue reverse engineering '{teamConnection.Key.ConnectionKey}'. The error is {exception.Message}.");
                            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"Reverse-engineering failed for connection '{teamConnection.Value}'. The error message is {exception.Message}"));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ThreadHelper.SetText(this, richTextBoxInformation, $"\r\n - There was an issue reverse engineering. The error is {exception.Message}.");
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"Reverse-engineering failed. The error message is {exception.Message}"));

                checkedListBoxReverseEngineeringAreas.Enabled = true;
            }

            // Merge the results with the full table.
            if (interimDataTable.Rows.Count > 0)
            {
                ThreadHelper.SetText(this, richTextBoxInformation, $"\r\nAdded new records completed at {DateTime.Now:HH:mm:ss tt}.");
                completeDataTable.Merge(interimDataTable);
            }

            ThreadHelper.SetText(this, richTextBoxInformation, $"\r\nMerge of data tables completed at {DateTime.Now:HH:mm:ss tt}.");

            // De-duplication.
            // Unfortunately, CopyToDataTable does not preserve the row state, so this has to be evaluated later again row by row.
            // https://learn.microsoft.com/en-us/dotnet/api/system.data.datatableextensions.copytodatatable?view=net-7.0
            DataTable distinctTable = null;
            try
            {
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
                else
                {
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"Reverse-engineering does not have any rows to deduplicate."));
                }

                ThreadHelper.SetText(this, richTextBoxInformation, $"\r\nDeduplication completed completed at {DateTime.Now:HH:mm:ss tt}.");
            }
            catch (Exception exception)
            {
                ThreadHelper.SetText(this, richTextBoxInformation, $"\r\n - There was an issue deduplicating the result set. The error is {exception.Message}.");
            }

            // Sort and display the results on the data grid.
            if (distinctTable != null)
            {
                distinctTable.DefaultView.Sort = $"[{PhysicalModelMappingMetadataColumns.databaseName}] ASC, [{PhysicalModelMappingMetadataColumns.schemaName}] ASC, [{PhysicalModelMappingMetadataColumns.tableName}] ASC, [{PhysicalModelMappingMetadataColumns.ordinalPosition}] ASC";

                // Inherit the filter. Can't apply to the binding source yet because changes will need to be saved first.
                distinctTable.DefaultView.RowFilter = existingFilter;

                BindingSourcePhysicalModel.DataSource = distinctTable;
            }
            else
            {
                ThreadHelper.SetText(this, richTextBoxInformation, $"\r\nThere was nothing to process, and nothing to show.");
            }

            labelResult.Text = @"Done!";

            richTextBoxInformation.Text += "\r\n\r\nCommencing the steps to display the results in the grid.\r\n";

            // And then we have to re-set the changes in the full data table, so that they can be seen as saved.
            if (interimDataTable != null && interimDataTable.Rows != null && interimDataTable.Rows.Count > 0)
            {
                foreach (DataRow changeRow in interimDataTable.Rows)
                {
                    var databaseName = changeRow[(int)PhysicalModelMappingMetadataColumns.databaseName].ToString();
                    var schemaName = changeRow[(int)PhysicalModelMappingMetadataColumns.schemaName].ToString();
                    var tableName = changeRow[(int)PhysicalModelMappingMetadataColumns.tableName].ToString();
                    var columnName = changeRow[(int)PhysicalModelMappingMetadataColumns.columnName].ToString();

                    var sourceColumnsDataTable = ((DataTable)BindingSourcePhysicalModel.DataSource).AsEnumerable()
                        .Where(row => row[(int)PhysicalModelMappingMetadataColumns.databaseName].ToString() == databaseName &&
                                      row[(int)PhysicalModelMappingMetadataColumns.schemaName].ToString() == schemaName &&
                                      row[(int)PhysicalModelMappingMetadataColumns.tableName].ToString() == tableName &&
                                      row[(int)PhysicalModelMappingMetadataColumns.columnName].ToString() == columnName)
                        .FirstOrDefault();

                    if (sourceColumnsDataTable != null && sourceColumnsDataTable.RowState == DataRowState.Unchanged)
                    {
                        sourceColumnsDataTable.SetAdded();
                    }
                }

                richTextBoxInformation.Text += "The changes have been added to the grid.\r\n";
            }

            // Re-enable the checked list box.
            checkedListBoxReverseEngineeringAreas.Enabled = true;

            richTextBoxInformation.Text += "\r\n\r\nDon't forget to save your changes if these records should be retained.\r\n";

            // Apply filtering.
            ApplyDataGridViewFiltering();
        }

        /// <summary>
        ///   Connect to a given database and return the data dictionary (catalog) information in the data grid.
        /// </summary>
        /// <param name="teamConnection"></param>
        /// <param name="filteredDataObjectMappingDataRows"></param>
        private DataTable ReverseEngineerModelMetadata(TeamConnection teamConnection, List<DataRow> filteredDataObjectMappingDataRows)
        {
            // The return value, a data table containing all results.
            DataTable reverseEngineerResults = new DataTable();

            var filteredObjects = GetDistinctFilteredDataObjects(filteredDataObjectMappingDataRows);
            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The list of filtered objects has been prepared."));

            if (teamConnection.TechnologyConnectionType == TechnologyConnectionType.SqlServer)
            {
                var connSql = new SqlConnection { ConnectionString = teamConnection.CreateSqlServerConnectionString(false) };

                try
                {
                    connSql.Open();
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"Connection {teamConnection.ConnectionKey} was opened using '{connSql.ConnectionString}'."));
                    
                    var sqlStatementForDataItems = SqlServerStatementForDataItems(filteredObjects, teamConnection);
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The reverse-engineering query run for connection '{teamConnection.ConnectionKey}' is \r\n {sqlStatementForDataItems}"));

                    // Load the data table with the query results.
                    reverseEngineerResults = Utility.GetDataTable(ref connSql, sqlStatementForDataItems);
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The data table containing the reverse engineering results has been created."));
                }
                catch (Exception exception)
                {
                    ThreadHelper.SetText(this, richTextBoxInformation, $@"An error has occurred uploading the model for the new version because the database could not be connected to. The error message is: {exception.Message}.");
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The reverse-engineering query run for connection '{teamConnection.ConnectionKey}' encountered an error. The reported error is {exception.Message}"));
                }
                finally
                {
                    connSql.Close();
                    connSql.Dispose();
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"Connection '{teamConnection.ConnectionKey}' has been closed."));
                }
            }
            else if (teamConnection.TechnologyConnectionType == TechnologyConnectionType.Snowflake)
            {
                IDbConnection conn = new SnowflakeDbConnection();
                conn.ConnectionString = teamConnection.CreateSnowflakeSSOConnectionString(false);

                try
                {
                    conn.Open();

                    // Catalog queries are run in one go, to keep things somewhat speedy.
                    if (teamConnection.CatalogConnectionType == CatalogConnectionTypes.Catalog)
                    {
                        RunSnowflakeReverseEngineeringQuery(teamConnection, filteredObjects, conn, reverseEngineerResults);
                    }
                    else // Customer queries, need to be executed separately for Snowflake. This is slow.
                    {
                        foreach (var dataObject in filteredObjects)
                        {
                            List<DataRow> listOfOne = new List<DataRow>();
                            listOfOne.Add(dataObject);

                            RunSnowflakeReverseEngineeringQuery(teamConnection, listOfOne, conn, reverseEngineerResults);
                        }
                    }
                }
                catch (Exception exception)
                {
                    ThreadHelper.SetText(this, richTextBoxInformation, $@"An error has occurred uploading the model because the database could not be connected to. The error message is: {exception.Message}.");
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The reverse-engineering query run for connection '{teamConnection.ConnectionKey}' encountered an error. The reported error is {exception.Message}"));
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                ThreadHelper.SetText(this, richTextBoxInformation, $@"The connection type could not be successfully evaluated. No connection can be established.");
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"he connection type could not be successfully evaluated. No connection can be established."));
            }

            return reverseEngineerResults;
        }

        private void RunSnowflakeReverseEngineeringQuery(TeamConnection teamConnection, List<DataRow> filteredObjects, IDbConnection conn, DataTable reverseEngineerResults)
        {
            var sqlStatementForDataItems = SnowflakeStatementForDataItems(filteredObjects, teamConnection);
            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The reverse-engineering query run for connection '{teamConnection.ConnectionKey}' is \r\n {sqlStatementForDataItems}"));

            // Load the data table with the catalog details.
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"USE WAREHOUSE {teamConnection.DatabaseServer.Warehouse}";
            cmd.ExecuteNonQuery();
            // Support multiple statements for this session.
            cmd.CommandText = "ALTER SESSION SET MULTI_STATEMENT_COUNT = 0;";
            cmd.ExecuteNonQuery();
            // Regular generated query.

            if (teamConnection.CatalogConnectionType == CatalogConnectionTypes.Catalog)
            {
                cmd.CommandText = sqlStatementForDataItems;
                var dataReader = cmd.ExecuteReader();
                reverseEngineerResults.Load(dataReader);
            }
            else
            {
                string[] statements = (sqlStatementForDataItems.Trim()).Split(";");
                statements = statements.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                foreach (string statement in statements)
                {
                    if (statement != statements.Last())
                    {
                        cmd.CommandText = $"{statement};";
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        cmd.CommandText = $"{statement};";
                        var dataReader = cmd.ExecuteReader();
                        reverseEngineerResults.Load(dataReader);
                    }
                }
            }

            // Update the Primary and Multi-Active keys.
            DataTable reverseEngineerKeys = new DataTable();
            IDbCommand cmdKeys = conn.CreateCommand();
            cmdKeys.CommandText = $"USE WAREHOUSE {teamConnection.DatabaseServer.Warehouse}";
            cmdKeys.ExecuteNonQuery();
            cmdKeys.CommandText = $"SHOW PRIMARY KEYS;";
            cmdKeys.ExecuteNonQuery();
            cmdKeys.CommandText = "SELECT \"table_name\",\"column_name\"\r\nFROM TABLE(RESULT_SCAN(LAST_QUERY_ID()))\r\nORDER BY \"table_name\";";
            var dataReaderKeys = cmdKeys.ExecuteReader();
            reverseEngineerKeys.Load(dataReaderKeys);

            // Apply the keys to the main reverse-engineering result set.
            foreach (DataRow keyRow in reverseEngineerResults.Rows)
            {
                var results = from localRow in reverseEngineerKeys.AsEnumerable()
                    where localRow.Field<string>("table_name") == keyRow["tableName"].ToString() &&
                          localRow.Field<string>("column_name") == keyRow["columnName"].ToString()
                    select localRow;

                if (results.FirstOrDefault() != null)
                {
                    // Set the PK value to 'Y'
                    keyRow[PhysicalModelMappingMetadataColumns.primaryKeyIndicator.ToString()] = 'Y';

                    // Determine if the key column is a MA column.
                    var columnName = results.FirstOrDefault()?["column_name"].ToString();

                    if (IsMultiActiveKey(columnName, TeamConfiguration))
                    {
                        keyRow[PhysicalModelMappingMetadataColumns.multiActiveIndicator.ToString()] = 'Y';
                    }
                }
            }
        }

        private bool IsMultiActiveKey(string columnName, TeamConfiguration teamConfiguration)
        {
            bool returnValue = false;

            var keyPosition = "prefix";
            if (teamConfiguration.KeyPattern.IndexOf("{keyIdentifier}")>1)
            {
                keyPosition = "suffix";
            }

            bool isBusinessKey = false;
            if (keyPosition == "prefix")
            {
                if (columnName.StartsWith(teamConfiguration.KeyIdentifier))
                {
                    isBusinessKey = true;
                }
            }

            if (keyPosition == "suffix")
            {
                if (columnName.EndsWith(teamConfiguration.KeyIdentifier))
                {
                    isBusinessKey = true;
                }
            }

            if (columnName != TeamConfiguration.LoadDateTimeAttribute && !isBusinessKey)
            {
                returnValue = true;
            }

            return returnValue;
        }

        private List<DataRow> GetDistinctFilteredDataObjects(List<DataRow> filteredDataObjectMappingDataRows)
        {
            var tempFilterDataObjects = new List<DataRow>();

            if (filteredDataObjectMappingDataRows.Any())
            {
                tempFilterDataObjects.AddRange(filteredDataObjectMappingDataRows.Distinct().ToList());
            }
            else
            {
                DataTable localDataTable = (DataTable)BindingSourceDataObjectMappings.DataSource;

                tempFilterDataObjects.AddRange(localDataTable.AsEnumerable().Distinct().ToList());
            }

            var filterDataObjects = tempFilterDataObjects.Distinct().ToList();
            return filterDataObjects;
        }

        private string SqlServerStatementForDataItems(List<DataRow> filterDataObjects, TeamConnection teamConnection, bool isJson = false)
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

            if (teamConnection.CatalogConnectionType == CatalogConnectionTypes.Catalog || teamConnection.CatalogConnectionType == CatalogConnectionTypes.Custom)
            {
                // Catalog query.
                if (teamConnection.CatalogConnectionType == CatalogConnectionTypes.Catalog)
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

                    // Multi-active

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
                else if (teamConnection.CatalogConnectionType == CatalogConnectionTypes.Custom)
                {
                    // Use the custom query that was provided with the connection.
                    sqlStatementForDataItems.AppendLine($"-- User-provided (custom) Physical Model Snapshot query for {teamConnection.ConnectionKey}.");
                    sqlStatementForDataItems.AppendLine("SELECT * FROM");
                    sqlStatementForDataItems.AppendLine("(");
                    sqlStatementForDataItems.AppendLine(teamConnection.ConnectionCustomQuery);
                    sqlStatementForDataItems.AppendLine(") customSubQuery");
                }

                // Add the filtered objects.
                sqlStatementForDataItems.AppendLine("WHERE 1=1");
                sqlStatementForDataItems.AppendLine("  AND");
                sqlStatementForDataItems.AppendLine("   (");

                // Object, connection, schema
                var filterList = new List<Tuple<DataObject, TeamConnection, string>>();

                foreach (DataRow row in filterDataObjects)
                {
                    // Skip deleted rows.
                    if (row.RowState == DataRowState.Deleted)
                        continue;

                    string localInternalConnectionIdSource = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    TeamConnection localConnectionSource = TeamConnection.GetTeamConnectionByConnectionInternalId(localInternalConnectionIdSource, TeamConfiguration, TeamEventLog);

                    string localInternalConnectionIdTarget = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                    TeamConnection localConnectionTarget = TeamConnection.GetTeamConnectionByConnectionInternalId(localInternalConnectionIdTarget, TeamConfiguration, TeamEventLog);

                    DataObject sourceDataObject = (DataObject)row[(int)DataObjectMappingGridColumns.SourceDataObject];
                    var sourceSchema = "dbo";
                    sourceSchema = sourceDataObject.DataObjectConnection?.Extensions?.Where(x => x.Key.Equals("schema")).FirstOrDefault().Value;

                    DataObject targetDataObject = (DataObject)row[(int)DataObjectMappingGridColumns.TargetDataObject];
                    var targetSchema = "dbo";
                    targetSchema = targetDataObject.DataObjectConnection?.Extensions?.Where(x => x.Key.Equals("schema")).FirstOrDefault().Value;

                    var localTupleSource = new Tuple<DataObject, TeamConnection, string>(sourceDataObject, localConnectionSource, sourceSchema);
                    var localTupleTarget = new Tuple<DataObject, TeamConnection, string>(targetDataObject, localConnectionTarget, targetSchema);

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

                    // Override the schema, if required.
                    var schema = "dbo";
                    if (fullyQualifiedName.Key != filter.Item3)
                    {
                        schema = filter.Item3;
                    }
                    else
                    {
                        schema = fullyQualifiedName.Key;
                    }

                    // Always add the 'regular' mapping.
                    sqlStatementForDataItems.AppendLine($"     ([{tableColumnName}] = '{fullyQualifiedName.Value}' AND [{schemaColumnName}] = '{schema}')");
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

        private string SnowflakeStatementForDataItems(List<DataRow> filterDataObjects, TeamConnection teamConnection)
        {
            string schemaColumnName = PhysicalModelMappingMetadataColumns.schemaName.ToString();
            string tableColumnName = PhysicalModelMappingMetadataColumns.tableName.ToString();
            string columnColumnName = PhysicalModelMappingMetadataColumns.columnName.ToString();
            string ordinalPositionColumnName = PhysicalModelMappingMetadataColumns.ordinalPosition.ToString();

            // Prepare the query, depending on the type.
            // Create the attribute selection statement for the array.
            var sqlStatementForDataItems = new StringBuilder();

            if (teamConnection.CatalogConnectionType == CatalogConnectionTypes.Catalog || teamConnection.CatalogConnectionType == CatalogConnectionTypes.Custom)
            {
                // Catalog query.
                if (teamConnection.CatalogConnectionType == CatalogConnectionTypes.Catalog)
                {
                    var databaseName = teamConnection.DatabaseServer.DatabaseName;

                    sqlStatementForDataItems.AppendLine($"-- Auto-generated physical model snapshot query for {teamConnection.ConnectionKey}.");
                    sqlStatementForDataItems.AppendLine("SELECT * FROM");
                    sqlStatementForDataItems.AppendLine("(");
                    sqlStatementForDataItems.AppendLine("  SELECT");
                    sqlStatementForDataItems.AppendLine("       TABLE_CATALOG AS \"databaseName\"");
                    sqlStatementForDataItems.AppendLine("      ,TABLE_SCHEMA AS \"schemaName\"");
                    sqlStatementForDataItems.AppendLine("      ,TABLE_NAME AS \"tableName\"");
                    sqlStatementForDataItems.AppendLine("      ,COLUMN_NAME AS \"columnName\"");
                    sqlStatementForDataItems.AppendLine("      ,DATA_TYPE AS \"dataType\"");
                    sqlStatementForDataItems.AppendLine("      ,TO_VARCHAR(COALESCE(CHARACTER_MAXIMUM_LENGTH,0)) AS \"characterLength\"");
                    sqlStatementForDataItems.AppendLine("      ,TO_VARCHAR(COALESCE(NUMERIC_PRECISION,0)) AS \"numericPrecision\"");
                    sqlStatementForDataItems.AppendLine("      ,TO_VARCHAR(COALESCE(NUMERIC_SCALE,0)) AS \"numericScale\"");
                    sqlStatementForDataItems.AppendLine("      ,AS_INTEGER(TO_VARIANT(ORDINAL_POSITION)) AS \"ordinalPosition\"");
                    sqlStatementForDataItems.AppendLine("      ,'N' AS \"primaryKeyIndicator\"");
                    sqlStatementForDataItems.AppendLine("      ,'N' AS \"multiActiveIndicator\"");
                    sqlStatementForDataItems.AppendLine("  FROM INFORMATION_SCHEMA.COLUMNS main");
                    sqlStatementForDataItems.AppendLine(") customSubQuery");

                    // Add the filtered objects.
                    sqlStatementForDataItems.AppendLine("WHERE 1=1");
                    sqlStatementForDataItems.AppendLine("  AND");
                    sqlStatementForDataItems.AppendLine("   (");

                    ///Object, connection, schema
                    var filterList = new List<Tuple<DataObject, TeamConnection, string>>();

                    foreach (DataRow row in filterDataObjects)
                    {
                        // Skip deleted rows.
                        if (row.RowState == DataRowState.Deleted)
                            continue;

                        string localInternalConnectionIdSource = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                        TeamConnection localConnectionSource = TeamConnection.GetTeamConnectionByConnectionInternalId(localInternalConnectionIdSource, TeamConfiguration, TeamEventLog);

                        string localInternalConnectionIdTarget = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                        TeamConnection localConnectionTarget = TeamConnection.GetTeamConnectionByConnectionInternalId(localInternalConnectionIdTarget, TeamConfiguration, TeamEventLog);

                        DataObject sourceDataObject = (DataObject)row[(int)DataObjectMappingGridColumns.SourceDataObject];
                        var sourceSchema = "dbo";
                        sourceSchema = sourceDataObject.DataObjectConnection?.Extensions?.Where(x => x.Key.Equals("schema")).FirstOrDefault().Value;

                        DataObject targetDataObject = (DataObject)row[(int)DataObjectMappingGridColumns.TargetDataObject];
                        var targetSchema = "dbo";
                        targetSchema = targetDataObject.DataObjectConnection?.Extensions?.Where(x => x.Key.Equals("schema")).FirstOrDefault().Value;

                        var localTupleSource = new Tuple<DataObject, TeamConnection, string>(sourceDataObject, localConnectionSource, sourceSchema);
                        var localTupleTarget = new Tuple<DataObject, TeamConnection, string>(targetDataObject, localConnectionTarget, targetSchema);

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

                        // Override the schema, if required.
                        var schema = "dbo";
                        if (fullyQualifiedName.Key != filter.Item3)
                        {
                            schema = filter.Item3;
                        }
                        else
                        {
                            schema = fullyQualifiedName.Key;
                        }

                        // Always add the 'regular' mapping.
                        sqlStatementForDataItems.AppendLine($"     (\"{tableColumnName}\" = '{fullyQualifiedName.Value}' AND \"{schemaColumnName}\" = '{schema}')");
                        sqlStatementForDataItems.AppendLine("     OR");
                    }

                    // Remove the last OR.
                    sqlStatementForDataItems.Remove(sqlStatementForDataItems.Length - 6, 6);

                    sqlStatementForDataItems.AppendLine(")");
                    sqlStatementForDataItems.AppendLine($"ORDER BY \"{tableColumnName}\", \"{columnColumnName}\", \"{ordinalPositionColumnName}\"");

                }
                else if (teamConnection.CatalogConnectionType == CatalogConnectionTypes.Custom)
                {
                    // Use the custom Snowflake-specific query that was provided with the connection.

                    // Get the data object

                    // Skip deleted rows.
                    string localInternalConnectionIdSource = filterDataObjects[0][DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    TeamConnection localConnectionSource = TeamConnection.GetTeamConnectionByConnectionInternalId(localInternalConnectionIdSource, TeamConfiguration, TeamEventLog);

                    string localInternalConnectionIdTarget = filterDataObjects[0][DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                    TeamConnection localConnectionTarget = TeamConnection.GetTeamConnectionByConnectionInternalId(localInternalConnectionIdTarget, TeamConfiguration, TeamEventLog);

                    DataObject sourceDataObject = (DataObject)filterDataObjects[0][(int)DataObjectMappingGridColumns.SourceDataObject];
                    var sourceSchema = "dbo";
                    sourceSchema = sourceDataObject.DataObjectConnection?.Extensions?.Where(x => x.Key.Equals("schema")).FirstOrDefault().Value;

                    DataObject targetDataObject = (DataObject)filterDataObjects[0][(int)DataObjectMappingGridColumns.TargetDataObject];
                    var targetSchema = "dbo";
                    targetSchema = targetDataObject.DataObjectConnection?.Extensions?.Where(x => x.Key.Equals("schema")).FirstOrDefault().Value;

                    DataObject dataObject = new DataObject();

                    if (teamConnection == localConnectionSource)
                    {
                        dataObject = sourceDataObject;
                    }
                    else
                    {
                        dataObject = targetDataObject;
                    }
                    
                    // Get any input value extensions.
                    var inputValue = dataObject.Extensions.FirstOrDefault(x => x.Key == "inputvalue").Value;

                    // Parse any placeholders in the query.
                    var customQuery = teamConnection.ConnectionCustomQuery
                        .Replace("CALL ", $"CALL {teamConnection.DatabaseServer.DatabaseName}.{teamConnection.DatabaseServer.SchemaName}.")
                        .Replace("{dataObject.name}", dataObject.Name)
                        .Replace("{dataObject.dataObjectConnection.extensions[0].value}", teamConnection.DatabaseServer.DatabaseName)
                        .Replace("{dataObject.dataObjectConnection.extensions[1].value}", teamConnection.DatabaseServer.SchemaName)
                        .Replace("{inputvalue}", $"({inputValue})");

                    sqlStatementForDataItems.AppendLine($"-- User-provided (custom) physical model query for {teamConnection.ConnectionKey}.");
                    sqlStatementForDataItems.AppendLine(customQuery);
                }
            }
            else
            {
                // Should never be possible.
                richTextBoxInformation.Text += @"An exception has occurred while determining the connection type. The connection does not have a valid connection type (0, catalog or 1, custom).";
            }

            return sqlStatementForDataItems.ToString();
        }

        /// <summary>
        /// Custom textbox that has a built-in delay for filtering.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxFilterCriterion_OnDelayedTextChanged(object sender, EventArgs e)
        {
            ApplyDataGridViewFiltering();
        }

        private void ApplyDataGridViewFiltering()
        {
            try
            {
                if (isStartUp == true) return;

                var filterCriterion = textBoxFilterCriterion.Text;

                // Create a list of connections so that only the filtered ones are checked.
                List<TeamConnection> connectionList = new List<TeamConnection>();

                // Only update the grid view on the visible tab.
                if (tabControlDataMappings.SelectedIndex == 0)
                {
                    foreach (DataGridViewRow row in _dataGridViewDataObjects.Rows)
                    {
                        row.Visible = true;

                        if (row.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value != null)
                        {
                            if (!row.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Contains(filterCriterion, StringComparison.OrdinalIgnoreCase) &&
                                !row.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Contains(filterCriterion, StringComparison.OrdinalIgnoreCase))
                            {
                                CurrencyManager currencyManager = (CurrencyManager)BindingContext[_dataGridViewDataObjects.DataSource];
                                currencyManager.SuspendBinding();
                                row.Visible = false;
                                currencyManager.ResumeBinding();
                            }
                            else
                            {
                                // Add the connection to the list.
                                var sourceConnectionId = row.Cells[(int)DataObjectMappingGridColumns.SourceConnection].Value.ToString();
                                TeamConnection sourceConnection = TeamConnection.GetTeamConnectionByConnectionInternalId(sourceConnectionId, TeamConfiguration, TeamEventLog);

                                var targetConnectionId = row.Cells[(int)DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                                TeamConnection targetConnection = TeamConnection.GetTeamConnectionByConnectionInternalId(targetConnectionId, TeamConfiguration, TeamEventLog);

                                if (!connectionList.Contains(sourceConnection))
                                {
                                    connectionList.Add(sourceConnection);
                                }

                                if (!connectionList.Contains(targetConnection))
                                {
                                    connectionList.Add(targetConnection);
                                }
                            }
                        }
                    }

                    // Reset the physical model filter. This is important because some checks need access to all the information.
                    var inputTableMappingPhysicalModel = (DataTable)BindingSourcePhysicalModel.DataSource;
                    inputTableMappingPhysicalModel.DefaultView.RowFilter = string.Empty;

                    ApplyFilterOnConnectionListCheckBox(connectionList);
                }
                else if (tabControlDataMappings.SelectedIndex == 1)
                {
                    foreach (DataGridViewRow row in _dataGridViewDataItems.Rows)
                    {
                        row.Visible = true;

                        if (row.Cells[(int)DataItemMappingGridColumns.TargetDataObject].Value != null)
                        {
                            if (!row.Cells[(int)DataItemMappingGridColumns.SourceDataObject].Value.ToString().Contains(filterCriterion, StringComparison.OrdinalIgnoreCase) &&
                                !row.Cells[(int)DataItemMappingGridColumns.SourceDataItem].Value.ToString().Contains(filterCriterion, StringComparison.OrdinalIgnoreCase) &&
                                !row.Cells[(int)DataItemMappingGridColumns.TargetDataObject].Value.ToString().Contains(filterCriterion, StringComparison.OrdinalIgnoreCase) &&
                                !row.Cells[(int)DataItemMappingGridColumns.TargetDataItem].Value.ToString().Contains(filterCriterion, StringComparison.OrdinalIgnoreCase))
                            {
                                CurrencyManager currencyManager = (CurrencyManager)BindingContext[_dataGridViewDataItems.DataSource];
                                currencyManager.SuspendBinding();
                                row.Visible = false;
                                currencyManager.ResumeBinding();
                            }
                            else
                            {
                                // Add the connection to the list.
                                var sourceConnectionId = row.Cells[(int)DataObjectMappingGridColumns.SourceConnection].Value.ToString();
                                TeamConnection sourceConnection = TeamConnection.GetTeamConnectionByConnectionInternalId(sourceConnectionId, TeamConfiguration, TeamEventLog);

                                var targetConnectionId = row.Cells[(int)DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                                TeamConnection targetConnection = TeamConnection.GetTeamConnectionByConnectionInternalId(targetConnectionId, TeamConfiguration, TeamEventLog);

                                if (!connectionList.Contains(sourceConnection))
                                {
                                    connectionList.Add(sourceConnection);
                                }

                                if (!connectionList.Contains(targetConnection))
                                {
                                    connectionList.Add(targetConnection);
                                }
                            }
                        }
                    }

                    // Reset the physical model filter. This is important because some checks need access to all the information.
                    var inputTableMappingPhysicalModel = (DataTable)BindingSourcePhysicalModel.DataSource;
                    inputTableMappingPhysicalModel.DefaultView.RowFilter = string.Empty;

                    ApplyFilterOnConnectionListCheckBox(connectionList);
                }
                else if (tabControlDataMappings.SelectedIndex == 2) // Physical model
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
                            filterCriterionPhysicalModel =
                                $"[{PhysicalModelMappingMetadataColumns.databaseName}] LIKE '%{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.tableName}] LIKE '%{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.columnName}] LIKE '%{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.schemaName}] LIKE '%{filterCriterion}%'";
                            inputTableMappingPhysicalModel.DefaultView.RowFilter = filterCriterionPhysicalModel;
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
                                currentFilter =
                                    $"[{PhysicalModelMappingMetadataColumns.tableName}] NOT LIKE '%{TeamConfiguration.StgTablePrefixValue}%' AND [{PhysicalModelMappingMetadataColumns.tableName}] NOT LIKE '%{TeamConfiguration.PsaTablePrefixValue}%'";
                            }

                            // Merge with existing filter.
                            filterCriterionPhysicalModel = currentFilter +
                                                           $"AND [{PhysicalModelMappingMetadataColumns.databaseName}] LIKE '%{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.tableName}] LIKE '%{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.columnName}] LIKE '%{filterCriterion}%' OR [{PhysicalModelMappingMetadataColumns.schemaName}] LIKE '%{filterCriterion}%'";
                        }

                        inputTableMappingPhysicalModel.DefaultView.RowFilter = filterCriterionPhysicalModel;
                    }
                }
                else
                {
                    // Exception - cannot happen.
                    richTextBoxInformation.Text =
                        $@"An incorrect data grid view was provided: '{tabControlDataMappings.TabPages[tabControlDataMappings.SelectedIndex]}'. This is a bug, please raise a Github issue.";
                }
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = $@"An error occurred when displaying the filter: '{ex.Message}'.";

            }
        }

        private void ApplyFilterOnConnectionListCheckBox(List<TeamConnection> connectionList)
        {
            // Uncheck everything, where not yet unchecked.
            while (checkedListBoxReverseEngineeringAreas.CheckedIndices.Count > 0)
            {
                checkedListBoxReverseEngineeringAreas.SetItemChecked(checkedListBoxReverseEngineeringAreas.CheckedIndices[0], false);
            }

            for (int i = 0; i < checkedListBoxReverseEngineeringAreas.Items.Count; i++)
            {
                var localConnectionObject = (KeyValuePair<TeamConnection, string>)checkedListBoxReverseEngineeringAreas.Items[i];

                if (connectionList.Contains(localConnectionObject.Key))
                {
                    checkedListBoxReverseEngineeringAreas.SetItemChecked(i, true);
                }
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

                // Validate basic Data Vault settings
                if (ValidationSetting.BasicDataVaultValidation == "True")
                {
                    _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateBasicDataVaultAttributeExistence(filteredDataObjectMappingDataRows, TeamConfiguration, TeamEventLog, ref metadataValidations));
                }
                worker?.ReportProgress(80);

                // Check for duplicate data item mappings
                if (ValidationSetting.DuplicateDataObjectMappings == "True")
                {
                    _alertValidation.SetTextLoggingMultiple(TeamValidation.ValidateDuplicateDataItemMappings(filteredDataItemDataRows, ref metadataValidations));
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

                DisplayErrors();
            }
        }

        private void displayTableScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Retrieve the index of the selected row
            Int32 selectedRow = _dataGridViewPhysicalModel.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            DataTable gridDataTable = (DataTable)BindingSourcePhysicalModel.DataSource;
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
                results.AppendLine(commaSnippet + row[PhysicalModelMappingMetadataColumns.columnName.ToString()] + " -- with ordinal position of " + row[PhysicalModelMappingMetadataColumns.ordinalPosition.ToString()]);
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
                    var psi = new ProcessStartInfo() { FileName = globalParameters.ConfigurationPath, UseShellExecute = true };
                    Process.Start(psi);
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

                    //GridAutoLayout(_dataGridViewDataItems);
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
                    // The below are hidden in the main table, but can be set via the JSON editor
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.DataObjectMappingExtension.ToString());
                    localDataTable.Columns.Add(DataObjectMappingGridColumns.DataObjectMappingClassification.ToString());
                    // The below are hidden, for sorting and back-end management only.
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
                                Name = tableMappingJson.sourceTable
                            };

                            var localTargetDataObject = new DataObject
                            {
                                Name = tableMappingJson.targetTable
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
                            newRow[(int)DataObjectMappingGridColumns.DataObjectMappingExtension] = "";
                            newRow[(int)DataObjectMappingGridColumns.DataObjectMappingClassification] = "";
                            newRow[(int)DataObjectMappingGridColumns.SourceDataObjectName] = localSourceDataObject.Name;
                            newRow[(int)DataObjectMappingGridColumns.TargetDataObjectName] = localTargetDataObject.Name;
                            newRow[(int)DataObjectMappingGridColumns.PreviousTargetDataObjectName] = localTargetDataObject.Name;
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

                    //GridAutoLayout(_dataGridViewDataObjects);
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

                    ((DataTable)BindingSourceDataObjectMappings.DataSource).AcceptChanges();

                    #endregion

                    #region Reload the full Data Grid

                    //Load the grids from the repository after being updated. This resets everything.
                    PopulateDataObjectMappingGrid();

                    DisplayErrors();

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

            // Handle multi-threading.
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
                        _alertEventLog.SetTextLogging($"{individualEvent.eventTime} - {(EventTypes)individualEvent.eventCode}: {individualEvent.eventDescription}\r\n");
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

            var dataTableAttributeMappingChanges = ((DataTable)BindingSourceDataItemMappings.DataSource).GetChanges();

            if (dataTableAttributeMappingChanges != null && dataTableAttributeMappingChanges.Rows.Count > 0)
            {
                string localMessage = "You have unsaved edits in the Data Item (attribute mapping) grid, please save your work before running the auto mapper.";
                MessageBox.Show(localMessage);
                richTextBoxInformation.AppendText(localMessage);
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, localMessage));
            }
            else
            {
                // Get the datatable for the physical model.
                DataTable physicalModelDataTable = (DataTable)BindingSourcePhysicalModel.DataSource;

                var exceptionColumns = TeamConfiguration.GetExceptionColumns();

                // Iterate across all Data Object Mappings, to see if there are corresponding Data Item Mappings.
                var filteredDataObjectMappingGridViewRows = GetFilteredDataObjectMappingDataGridViewRows();


                foreach (DataGridViewRow dataObjectRow in filteredDataObjectMappingGridViewRows)
                {
                    // Skip if the row is a new row.
                    if (dataObjectRow.IsNewRow)
                    {
                        return;
                    }

                    #region Source Data Object

                    var sourceConnectionId = dataObjectRow.Cells[(int)DataObjectMappingGridColumns.SourceConnection].Value.ToString();
                    TeamConnection sourceConnection = TeamConnection.GetTeamConnectionByConnectionInternalId(sourceConnectionId, TeamConfiguration, TeamEventLog);

                    DataObject sourceDataObject = (DataObject)dataObjectRow.Cells[(int)DataObjectMappingGridColumns.SourceDataObject].Value;
                    var sourceDataObjectFullyQualifiedKeyValuePair = MetadataHandling.GetFullyQualifiedDataObjectName(sourceDataObject, sourceConnection).FirstOrDefault();

                    var sourceColumnsDataTable = physicalModelDataTable.AsEnumerable()
                        .Where(row => row[(int)PhysicalModelMappingMetadataColumns.databaseName].ToString() == sourceConnection.DatabaseServer.DatabaseName &&
                               row[(int)PhysicalModelMappingMetadataColumns.schemaName].ToString() == sourceDataObjectFullyQualifiedKeyValuePair.Key &&
                               row[(int)PhysicalModelMappingMetadataColumns.tableName].ToString() == sourceDataObjectFullyQualifiedKeyValuePair.Value)
                        .CopyToDataTable();

                    if (sourceColumnsDataTable != null && sourceColumnsDataTable.Rows.Count == 0)
                    {
                        TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The source physical model structures could not be queried."));
                        return;
                    }

                    #endregion

                    #region Target Data Object

                    // Target Data Object details
                    DataObject targetDataObject = (DataObject)dataObjectRow.Cells[DataObjectMappingGridColumns.TargetDataObject.ToString()].Value;

                    var targetConnectionId = dataObjectRow.Cells[(int)DataObjectMappingGridColumns.TargetConnection].Value.ToString();
                    TeamConnection targetConnection = TeamConnection.GetTeamConnectionByConnectionInternalId(targetConnectionId, TeamConfiguration, TeamEventLog);

                    var targetDataObjectFullyQualifiedKeyValuePair = MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObject, targetConnection).FirstOrDefault();

                    var targetColumnDataTable = physicalModelDataTable.AsEnumerable()
                        .Where(row => row[(int)PhysicalModelMappingMetadataColumns.databaseName].ToString() == targetConnection.DatabaseServer.DatabaseName &&
                                      row[(int)PhysicalModelMappingMetadataColumns.schemaName].ToString() == targetDataObjectFullyQualifiedKeyValuePair.Key &&
                                      row[(int)PhysicalModelMappingMetadataColumns.tableName].ToString() == targetDataObjectFullyQualifiedKeyValuePair.Value)
                        .CopyToDataTable();

                    if (targetColumnDataTable != null && targetColumnDataTable.Rows.Count == 0)
                    {
                        TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The target physical model structures could not be queried."));
                        return;
                    }

                    #endregion

                    List<TeamDataItemMappingRow> localDataItemMappings = new List<TeamDataItemMappingRow>();

                    // For each source Data Object, check if there is a matching target
                    foreach (DataRow sourceDataObjectRow in sourceColumnsDataTable.Rows)
                    {
                        // Do the lookup in the target data table
                        var results = from localRow in targetColumnDataTable.AsEnumerable()
                                      where localRow.Field<string>(PhysicalModelMappingMetadataColumns.columnName.ToString()) == sourceDataObjectRow[PhysicalModelMappingMetadataColumns.columnName.ToString()].ToString()
                                      select localRow;

                        if (results.FirstOrDefault() != null)
                        {
                            // There is a match and it's not a standard attribute.

                            var sourceColumn = sourceDataObjectRow[PhysicalModelMappingMetadataColumns.columnName.ToString()].ToString();

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

                            if (!exclusionAttribute.Contains(sourceColumn) && !exceptionColumns.Contains(sourceColumn))
                            {
                                var localMapping = new TeamDataItemMappingRow
                                {
                                    sourceDataObjectName = sourceDataObject.Name,
                                    sourceDataObjectConnectionId = sourceConnectionId,
                                    sourceDataItemName = sourceDataObjectRow[PhysicalModelMappingMetadataColumns.columnName.ToString()].ToString(),
                                    targetDataObjectName = targetDataObject.Name,
                                    targetDataObjectConnectionId = targetConnectionId,
                                    targetDataItemName = sourceDataObjectRow[PhysicalModelMappingMetadataColumns.columnName.ToString()].ToString() // Same as source, as it's a direct match on this value.
                                };

                                localDataItemMappings.Add(localMapping);
                            }
                        }
                    }

                    // Now, for each item in the matched list check if there is a corresponding Data Item Mapping in the grid already.
                    DataTable localDataItemDataTable = (DataTable)BindingSourceDataItemMappings.DataSource;

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

                            newRow[DataItemMappingGridColumns.HashKey.ToString()] = Utility.CreateMd5(new[] { Utility.GetRandomString(100) }, "#");
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
                    var psi = new ProcessStartInfo() { FileName = globalParameters.MetadataPath, UseShellExecute = true };
                    Process.Start(psi);
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

                if (localConnectionObject.Key.TechnologyConnectionType == TechnologyConnectionType.SqlServer)
                {
                    resultQueryList.Add(SqlServerStatementForDataItems(GetDistinctFilteredDataObjects(filteredDataObjectMappingDataRows), localConnectionObject.Key, true));
                }
                else if (localConnectionObject.Key.TechnologyConnectionType == TechnologyConnectionType.Snowflake)
                {
                    resultQueryList.Add(SnowflakeStatementForDataItems(GetDistinctFilteredDataObjects(filteredDataObjectMappingDataRows), localConnectionObject.Key));
                }
                else
                {
                    // Report error.
                    richTextBoxInformation.AppendText($"An error has been encountered when attempting to generate the reverse-engineering query. An unknown connection {localConnectionObject.Key.TechnologyConnectionType.ToString()} has been encountered.\r\n");
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An unknown connection {localConnectionObject.Key.TechnologyConnectionType.ToString()} has been encountered."));
                }
            }

            foreach (var query in resultQueryList)
            {
                _physicalModelQuery.SetTextLogging(query);
                _physicalModelQuery.SetTextLogging("\r\n");
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

        private void richTextBoxInformation_TextChanged(object sender, EventArgs e)
        {
            richTextBoxInformation.SelectionStart = richTextBoxInformation.Text.Length;
            richTextBoxInformation.ScrollToCaret();
        }

        private void importPhysicalModelGridFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControlDataMappings.SelectedIndex = 2;

            var dialog = new OpenFileDialog
            {
                Title = @"Open Physical Model File",
                Filter = @"Physical Model files|*.json",
                InitialDirectory = globalParameters.MetadataPath
            };

            var dialogResult = STAShowDialog(dialog);

            if (dialogResult == DialogResult.OK)
            {
                richTextBoxInformation.Clear();

                try
                {
                    // Get the selected file in tabular JSON format.
                    List<PhysicalModelGridRow> jsonArray = JsonConvert.DeserializeObject<List<PhysicalModelGridRow>>(File.ReadAllText(dialog.FileName));

                    #region Build the Data Table

                    var localDataTable = new DataTable();

                    localDataTable.Columns.Add(PhysicalModelMappingMetadataColumns.databaseName.ToString());
                    localDataTable.Columns.Add(PhysicalModelMappingMetadataColumns.schemaName.ToString());
                    localDataTable.Columns.Add(PhysicalModelMappingMetadataColumns.tableName.ToString());
                    localDataTable.Columns.Add(PhysicalModelMappingMetadataColumns.columnName.ToString());
                    localDataTable.Columns.Add(PhysicalModelMappingMetadataColumns.characterLength.ToString());
                    localDataTable.Columns.Add(PhysicalModelMappingMetadataColumns.dataType.ToString());
                    localDataTable.Columns.Add(PhysicalModelMappingMetadataColumns.numericPrecision.ToString());
                    localDataTable.Columns.Add(PhysicalModelMappingMetadataColumns.numericScale.ToString());
                    localDataTable.Columns.Add(PhysicalModelMappingMetadataColumns.ordinalPosition.ToString());
                    localDataTable.Columns.Add(PhysicalModelMappingMetadataColumns.multiActiveIndicator.ToString());
                    localDataTable.Columns.Add(PhysicalModelMappingMetadataColumns.primaryKeyIndicator.ToString());

                    //TeamPhysicalModel.SetDataTableColumnNames(localDataTable);

                    if (jsonArray != null)
                    {
                        foreach (PhysicalModelGridRow physicalModelColumn in jsonArray)
                        {
                            var newRow = localDataTable.NewRow();

                            newRow[(int)PhysicalModelMappingMetadataColumns.databaseName] = physicalModelColumn.databaseName;
                            newRow[(int)PhysicalModelMappingMetadataColumns.schemaName] = physicalModelColumn.schemaName;
                            newRow[(int)PhysicalModelMappingMetadataColumns.tableName] = physicalModelColumn.tableName;
                            newRow[(int)PhysicalModelMappingMetadataColumns.columnName] = physicalModelColumn.columnName;
                            newRow[(int)PhysicalModelMappingMetadataColumns.characterLength] = physicalModelColumn.characterLength;
                            newRow[(int)PhysicalModelMappingMetadataColumns.dataType] = physicalModelColumn.dataType;
                            newRow[(int)PhysicalModelMappingMetadataColumns.numericPrecision] = physicalModelColumn.numericPrecision;
                            newRow[(int)PhysicalModelMappingMetadataColumns.numericScale] = physicalModelColumn.numericScale;
                            newRow[(int)PhysicalModelMappingMetadataColumns.ordinalPosition] = physicalModelColumn.ordinalPosition;
                            newRow[(int)PhysicalModelMappingMetadataColumns.multiActiveIndicator] = physicalModelColumn.multiActiveIndicator;
                            newRow[(int)PhysicalModelMappingMetadataColumns.primaryKeyIndicator] = physicalModelColumn.primaryKeyIndicator;

                            localDataTable.Rows.Add(newRow);
                        }
                    }

                    //Make sure the changes are seen as committed, so that changes can be detected later on.
                    localDataTable.AcceptChanges();

                    #endregion

                    #region Update the data table

                    // Clear out the existing data from the grid
                    BindingSourcePhysicalModel.Sort = string.Empty;
                    BindingSourcePhysicalModel.DataSource = null;
                    BindingSourcePhysicalModel.Clear();

                    _dataGridViewPhysicalModel.DataSource = null;

                    // Bind the data table to the grid view
                    BindingSourcePhysicalModel.DataSource = localDataTable;

                    // Set the column header names
                    _dataGridViewPhysicalModel.DataSource = BindingSourcePhysicalModel;

                    richTextBoxInformation.AppendText($"The file '{dialog.FileName}' was loaded.\r\n");

                    #endregion

                    #region Generate the JSON files

                    List<string> tableExceptionList = new List<string>();

                    foreach (DataRow row in localDataTable.Rows)
                    {
                        var database = row[PhysicalModelMappingMetadataColumns.databaseName.ToString()].ToString();
                        var schema = row[PhysicalModelMappingMetadataColumns.schemaName.ToString()].ToString();
                        var table = row[PhysicalModelMappingMetadataColumns.tableName.ToString()].ToString();

                        // Process all columns only once per table.
                        if (!tableExceptionList.Contains(table))
                        {
                            try
                            {
                                // A new file is created and/or an existing one updated to remove a segment.
                                WritePhysicalModelToFile(database, schema, table);
                                tableExceptionList.Add(table);
                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += $@"There were issues updating the JSON. The error message is {ex.Message}.";
                            }
                        }
                    }

                    #endregion

                    #region Reload the full Data Grid

                    //Load the grids from the repository after being updated. This resets everything.
                    PopulatePhysicalModelGrid();

                    DisplayErrors();

                    #endregion
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.AppendText($"An error has been encountered when attempting to save the file to disk. The reported error is: {ex.Message}\r\n");
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An exception has been encountered: {ex.Message}"));
                }
            }
        }

        /// <summary>
        /// Convenience method to wrap the creation and deletion of physical model objects in one pass.
        /// </summary>
        /// <param name="database"></param>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        private void WritePhysicalModelToFile(string database, string schema, string table)
        {
            // Create a list of grid rows based on the database, schema and table names.
            //var physicalModelRows = _dataGridViewPhysicalModel.Rows.Cast<DataGridViewRow>()
            //    .Where(row => !row.IsNewRow &&
            //                  (row.Cells[(int)PhysicalModelMappingMetadataColumns.databaseName].Value.ToString() == database &&
            //                   row.Cells[(int)PhysicalModelMappingMetadataColumns.schemaName].Value.ToString() == schema &&
            //                   row.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString() == table))
            //    .ToList();

            var physicalModelRows = ((DataTable)BindingSourcePhysicalModel.DataSource).AsEnumerable()
                .Where(row => row[(int)PhysicalModelMappingMetadataColumns.databaseName].ToString() == database &&
                              row[(int)PhysicalModelMappingMetadataColumns.schemaName].ToString() == schema &&
                              row[(int)PhysicalModelMappingMetadataColumns.tableName].ToString() == table).AsDataView();

            // If there are rows to process, a file can be constructed. Otherwise the file must be deleted as there are no columns.
            if (physicalModelRows.Count > 0)
            {
                string output = "";
                try
                {
                    var physicalModelTable = new PhysicalModelTable();
                    physicalModelTable.database = database;
                    physicalModelTable.name = table;
                    physicalModelTable.schema = schema;

                    // Construct a save file for the physical model.
                    foreach (DataRowView row in physicalModelRows)
                    {
                        var physicalModelColumn = new PhysicalModelColumn();

                        physicalModelColumn.name = row[(int)PhysicalModelMappingMetadataColumns.columnName].ToString();
                        physicalModelColumn.dataType = row[(int)PhysicalModelMappingMetadataColumns.dataType].ToString();
                        physicalModelColumn.ordinalPosition = (int)int.Parse(row[(int)PhysicalModelMappingMetadataColumns.ordinalPosition].ToString());
                        physicalModelColumn.characterLength = row[(int)PhysicalModelMappingMetadataColumns.characterLength].ToString();
                        physicalModelColumn.numericScale = row[(int)PhysicalModelMappingMetadataColumns.numericScale].ToString();
                        physicalModelColumn.numericPrecision = row[(int)PhysicalModelMappingMetadataColumns.numericPrecision].ToString();
                        physicalModelColumn.multiActiveIndicator = row[(int)PhysicalModelMappingMetadataColumns.multiActiveIndicator].ToString();
                        physicalModelColumn.primaryKeyIndicator = row[(int)PhysicalModelMappingMetadataColumns.primaryKeyIndicator].ToString();
                        physicalModelTable.columns.Add(physicalModelColumn);
                    }

                    output = JsonConvert.SerializeObject(physicalModelTable, Formatting.Indented);

                    // Create the paths, if not existing already.
                    FileHandling.InitialisePath(globalParameters.MetadataPath + globalParameters.PhysicalModelDirectory, TeamPathTypes.OtherPath, TeamEventLog);
                    FileHandling.InitialisePath(globalParameters.MetadataPath + globalParameters.PhysicalModelDirectory + database + @"\", TeamPathTypes.OtherPath, TeamEventLog);
                    FileHandling.InitialisePath(globalParameters.MetadataPath + globalParameters.PhysicalModelDirectory + database + @"\" + schema + @"\", TeamPathTypes.OtherPath, TeamEventLog);
                }
                catch (Exception exception)
                {
                    ThreadHelper.SetText(this, richTextBoxInformation, $"The Physical Model object '{table}' could not be created. The exception message is {exception.Message}.\r\n");
                }

                string outputFilePath = globalParameters.MetadataPath + globalParameters.PhysicalModelDirectory + database + @"\" + schema + @"\" + table + ".json";

                try
                {
                    if (!string.IsNullOrEmpty(output))
                    {
                        File.WriteAllText(outputFilePath, output);
                        ThreadHelper.SetText(this, richTextBoxInformation, $"The Physical Model object '{table}' has been saved in '{outputFilePath}'.\r\n");
                    }
                }
                catch (Exception exception)
                {
                    ThreadHelper.SetText(this, richTextBoxInformation, $"There was an issue saving the Physical model object '{table}', the reported exception is {exception.Message}\r\n");
                }
            }
            else
            {
                // Deleting a file that has been renamed, removed or otherwise emptied.
                try
                {
                    var fileToDelete = globalParameters.MetadataPath + globalParameters.PhysicalModelDirectory + database + @"\" + schema + @"\" + table + ".json";

                    if (File.Exists(fileToDelete))
                    {
                        File.Delete(fileToDelete);
                        ThreadHelper.SetText(this, richTextBoxInformation, $"The Physical Model object '{table}' has been removed.\r\n");
                    }
                }
                catch (Exception exception)
                {
                    ThreadHelper.SetText(this, richTextBoxInformation, $"There was an issue deleting the Physical Object '{table}', the reported exception is {exception.Message}\r\n");
                }

                // Check if any of the directories need clean-up.
                var schemaDirectory = globalParameters.MetadataPath + globalParameters.PhysicalModelDirectory + database + @"\" + schema + @"\";
                try
                {
                    if (Directory.Exists(schemaDirectory) && Directory.GetFiles(schemaDirectory).Length == 0)
                    {
                        Directory.Delete(schemaDirectory, true);
                    }

                }
                catch (Exception exception)
                {
                    ThreadHelper.SetText(this, richTextBoxInformation, $"There was an issue deleting the physical model '{schemaDirectory}' directory, the reported exception is {exception.Message}\r\n");
                }

                var databaseDirectory = globalParameters.MetadataPath + globalParameters.PhysicalModelDirectory + database + @"\";
                try
                {
                    if (Directory.Exists(databaseDirectory) && Directory.GetFiles(databaseDirectory, "*.*", SearchOption.AllDirectories).Length == 0)
                    {
                        Directory.Delete(databaseDirectory, true);
                    }
                }
                catch (Exception exception)
                {
                    ThreadHelper.SetText(this, richTextBoxInformation, $"There was an issue deleting the physical model '{databaseDirectory}' directory, the reported exception is {exception.Message}\r\n");
                }
            }
        }

        private void openCoreDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            {
                try
                {
                    var psi = new ProcessStartInfo() { FileName = globalParameters.CorePath, UseShellExecute = true };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.Text = $@"An error has occurred while attempting to open the directory. The error message is: {ex.Message}.";
                }
            }
        }

        private void clearEventLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TeamEventLog.Clear();
            TeamEventLog.errorReportedHighWaterMark = 0;
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadMetadata();
        }
    }
}