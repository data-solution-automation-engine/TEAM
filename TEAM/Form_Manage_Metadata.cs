using DataWarehouseAutomation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TEAM_Library;
using EventLog = TEAM_Library.EventLog;

namespace TEAM
{
    public partial class FormManageMetadata : FormBase
    {
        // Initialise various instances of the status/alert form.
        Form_Alert _alert;
        Form_Alert _alertValidation;
        Form_Alert _generatedScripts;
        Form_Alert _generatedJsonInterface;
        Form_Alert _alertEventLog;

        // Getting the DataTable to bind to something
        private BindingSource _bindingSourceTableMetadata = new BindingSource();
        private BindingSource _bindingSourceAttributeMetadata = new BindingSource();
        private BindingSource _bindingSourcePhysicalModelMetadata = new BindingSource();

        public FormManageMetadata()
        {
            InitializeComponent();
        }

        public FormManageMetadata(FormMain parent) : base(parent)
        {
            InitializeComponent();

            // Hide the physical model tab if in physical mode
            if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode)
            {
                tabControlDataMappings.TabPages.Remove(tabPagePhysicalModel);
            }

            // Default setting and initialisation of counters etc.
            radiobuttonNoVersionChange.Checked = true;
            MetadataParameters.ValidationIssues = 0;
            MetadataParameters.ValidationRunning = false;

            labelHubCount.Text = @"0 Core Business Concepts";
            labelSatCount.Text = @"0 Context entities";
            labelLnkCount.Text = @"0 Relationships";
            labelLsatCount.Text = @"0 Relationship context entities";

            radiobuttonNoVersionChange.Checked = true;

            // Check if the version file exists and create a new file containing the list of versions.
            if (!File.Exists(GlobalParameters.CorePath + GlobalParameters.VersionFileName +
                             GlobalParameters.JsonExtension))
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information,
                    $"Creating a new version list file {GlobalParameters.CorePath + GlobalParameters.VersionFileName + GlobalParameters.JsonExtension}."));
                TeamVersionList.CreateNewVersionListFile(
                    GlobalParameters.CorePath + GlobalParameters.VersionFileName + GlobalParameters.JsonExtension,
                    GlobalParameters.WorkingEnvironment);
            }

            var selectedVersion = TeamVersionList.GetMaxVersionForEnvironment(GlobalParameters.WorkingEnvironment);

            if (selectedVersion != null)
            {
                // Set the version in memory
                GlobalParameters.CurrentVersionId = selectedVersion.Item1;
                GlobalParameters.HighestVersionId = selectedVersion.Item1; // On startup, the highest version is the same as the current version
                TeamJsonHandling.JsonFileConfiguration.jsonVersionExtension = @"_v" + selectedVersion.Item1 + ".json";

                trackBarVersioning.Maximum = selectedVersion.Item1;
                trackBarVersioning.TickFrequency = TeamVersionList.GetTotalVersionCount(GlobalParameters.WorkingEnvironment);

                //Make sure the version is displayed
                var versionMajorMinor = TeamVersionList.GetMajorMinorForVersionId(GlobalParameters.WorkingEnvironment, selectedVersion.Item1);
                var majorVersion = versionMajorMinor.Item2;
                var minorVersion = versionMajorMinor.Item3;

                trackBarVersioning.Value = selectedVersion.Item1;
                labelVersion.Text = majorVersion + "." + minorVersion;

                //  Load the grids from the repository
                richTextBoxInformation.Clear();

                // Load the data grids
                PopulateTableMappingGridWithVersion();
                PopulateAttributeGridWithVersion();
                PopulatePhysicalModelGridWithVersion();

                // Inform the user
                string userFeedback = $"The metadata for version {majorVersion}.{minorVersion} has been loaded.";
                richTextBoxInformation.AppendText($"{userFeedback}\r\n");
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"{userFeedback}"));
            }
            else
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"No version information was loaded, and therefore the metadata could not be loaded either."));
            }

            ContentCounter();

            // Make sure the validation information is available for this form.
            try
            {
                var validationFile = GlobalParameters.ConfigurationPath + GlobalParameters.ValidationFileName + '_' +
                                     GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension;

                // If the config file does not exist yet, create it by calling the EnvironmentConfiguration Class
                if (!File.Exists(validationFile))
                {
                    ValidationSetting.CreateDummyValidationFile(validationFile);
                }

                // Load the validation settings file using the paths retrieved from the application root contents (configuration path)
                ValidationSetting.LoadValidationFile(validationFile);

                richTextBoxInformation.AppendText($"The configuration file {validationFile} has been loaded.\r\n");
            }
            catch (Exception ex)
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning,
                    $"A validation file could not be loaded, so default (all) validation will be used. The exception message is {ex}."));
            }


            // Make sure the json configuration information is available for this form.
            try
            {
                var jsonConfigurationFile = GlobalParameters.ConfigurationPath +
                                            GlobalParameters.JsonExportConfigurationFileName + '_' +
                                            GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension;

                // If the config file does not exist yet, create it.
                if (!File.Exists(jsonConfigurationFile))
                {
                    JsonExportSetting.CreateDummyJsonConfigurationFile(jsonConfigurationFile);
                }

                // Load the validation settings file using the paths retrieved from the application root contents (configuration path)
                JsonExportSetting.LoadJsonConfigurationFile(jsonConfigurationFile);

                richTextBoxInformation.AppendText(
                    $"The configuration file {jsonConfigurationFile} has been loaded.\r\n");
            }
            catch (Exception ex)
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning,
                    $"The Json export configuration file could not be loaded, so default (all) validation will be used. The exception message is {ex}."));
            }

            #region CheckedListBox for reverse engineering

            checkedListBoxReverseEngineeringAreas.CheckOnClick = true;
            checkedListBoxReverseEngineeringAreas.ValueMember = "Key";
            checkedListBoxReverseEngineeringAreas.DisplayMember = "Value";

            // Load the checkboxes for the reverse-engineering tab
            foreach (var connection in TeamConfiguration.ConnectionDictionary)
            {
                checkedListBoxReverseEngineeringAreas.Items.Add(
                    new KeyValuePair<TeamConnection, string>(connection.Value, connection.Value.ConnectionKey));
            }

            #endregion
        }

        #region Grid View Formatting & Handling

        /// <summary>
        /// Sets the ToolTip text for cells in the DataGridView (hover over).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DataGridViewTableMetadata_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Retrieve the full row for the selected cell
            DataGridViewRow selectedRow = dataGridViewTableMetadata.Rows[e.RowIndex];

            if (selectedRow.IsNewRow == false)
            {
                // Source info
                var sourceDataObjectName = selectedRow.DataGridView.Rows[e.RowIndex]
                    .Cells[(int) TableMappingMetadataColumns.SourceTable].Value.ToString();
                var sourceConnectionId = selectedRow.DataGridView.Rows[e.RowIndex]
                    .Cells[(int) TableMappingMetadataColumns.SourceConnection].Value.ToString();
                TeamConnection sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionId);
                var sourceDataObjectFullyQualifiedKeyValuePair = MetadataHandling
                    .GetFullyQualifiedDataObjectName(sourceDataObjectName, sourceConnection).FirstOrDefault();
                string sourceDataObjectFullyQualifiedName =
                    $"{sourceDataObjectFullyQualifiedKeyValuePair.Key}.{sourceDataObjectFullyQualifiedKeyValuePair.Value}";

                // Target info
                var targetDataObjectName = selectedRow.DataGridView.Rows[e.RowIndex]
                    .Cells[(int) TableMappingMetadataColumns.TargetTable].Value.ToString();
                var targetConnectionId = selectedRow.DataGridView.Rows[e.RowIndex]
                    .Cells[(int) TableMappingMetadataColumns.TargetConnection].Value.ToString();
                TeamConnection targetConnection = GetTeamConnectionByConnectionId(targetConnectionId);
                var targetDataObjectFullyQualifiedKeyValuePair = MetadataHandling
                    .GetFullyQualifiedDataObjectName(targetDataObjectName, targetConnection).FirstOrDefault();
                string targetDataObjectFullyQualifiedName =
                    $"{targetDataObjectFullyQualifiedKeyValuePair.Key}.{targetDataObjectFullyQualifiedKeyValuePair.Value}";


                var loadVector = "";
                MetadataHandling.TableTypes tableType = MetadataHandling.TableTypes.Unknown;
                DataGridViewCell cell = null;

                if (e.Value != null && sourceDataObjectName != null && targetDataObjectName != null)
                {
                    loadVector = MetadataHandling.GetDataObjectMappingLoadVector(sourceDataObjectFullyQualifiedName,
                        targetDataObjectFullyQualifiedName, TeamConfiguration);
                }

                // Assert table type for Source column, retrieve the specific cell value for the hover-over.
                if (e.ColumnIndex == dataGridViewTableMetadata.Columns[(int) TableMappingMetadataColumns.SourceTable]
                    .Index && e.Value != null)
                {
                    cell = dataGridViewTableMetadata.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    tableType = MetadataHandling.GetDataObjectType(e.Value.ToString(), "", TeamConfiguration);
                }
                // Assert table type for the Target column, , retrieve the specific cell value for the hover-over.
                else if ((e.ColumnIndex == dataGridViewTableMetadata
                    .Columns[(int) TableMappingMetadataColumns.TargetTable].Index && e.Value != null))
                {
                    cell = dataGridViewTableMetadata.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    tableType = MetadataHandling.GetDataObjectType(e.Value.ToString(),
                        selectedRow.DataGridView.Rows[e.RowIndex]
                            .Cells[(int) TableMappingMetadataColumns.BusinessKeyDefinition].Value.ToString(),
                        TeamConfiguration);
                }
                else
                {
                    // Do nothing, this hits like a billion times
                    // GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The load vector and table types between {sourceDataObjectFullyQualifiedName} and {targetDataObjectFullyQualifiedName} could not be asserted."));
                }

                if (cell != null)
                {
                    cell.ToolTipText = "The table " + e.Value + " has been evaluated as a " + tableType + " object." +
                                       "\n" + "The direction of loading is " + loadVector + ".";
                }
            }
        }

        private void DataGridViewPhysicalModelMetadataKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.V:
                            PasteClipboardPhysicalModelMetadata();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Pasting into the data grid has failed", "Copy/Paste", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void PasteClipboardPhysicalModelMetadata()
        {
            try
            {
                string s = Clipboard.GetText();
                string[] lines = s.Split('\n');

                int iRow = dataGridViewPhysicalModelMetadata.CurrentCell.RowIndex;
                int iCol = dataGridViewPhysicalModelMetadata.CurrentCell.ColumnIndex;
                DataGridViewCell oCell;
                if (iRow + lines.Length > dataGridViewPhysicalModelMetadata.Rows.Count - 1)
                {
                    bool bFlag = false;
                    foreach (string sEmpty in lines)
                    {
                        if (sEmpty == "")
                        {
                            bFlag = true;
                        }
                    }

                    int iNewRows = iRow + lines.Length - dataGridViewPhysicalModelMetadata.Rows.Count;
                    if (iNewRows > 0)
                    {
                        if (bFlag)
                            dataGridViewPhysicalModelMetadata.Rows.Add(iNewRows);
                        else
                            dataGridViewPhysicalModelMetadata.Rows.Add(iNewRows + 1);
                    }
                    else
                        dataGridViewPhysicalModelMetadata.Rows.Add(iNewRows + 1);
                }

                foreach (string line in lines)
                {
                    if (iRow < dataGridViewPhysicalModelMetadata.RowCount && line.Length > 0)
                    {
                        string[] sCells = line.Split('\t');
                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            if (iCol + i < dataGridViewPhysicalModelMetadata.ColumnCount)
                            {
                                oCell = dataGridViewPhysicalModelMetadata[iCol + i, iRow];
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
                richTextBoxInformation.AppendText(
                    "There is an issue formatting this cell.Please check the Event Log for more details.\r\n");
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    $"An exception has been encountered: {ex.Message}."));
            }
        }

        #endregion

        /// <summary>
        /// Populate the Table Mapping DataGrid from an existing Json file.
        /// </summary>
        private void PopulateTableMappingGridWithVersion()
        {
            // Create a new dummy / starter file, if it doesn't exist.
            if (!File.Exists(TeamJsonHandling.JsonFileConfiguration.TableMappingJsonFileName()))
            {
                richTextBoxInformation.AppendText(
                    $"No Json file was found, so a new empty one was created: {TeamJsonHandling.JsonFileConfiguration.TableMappingJsonFileName()}.\r\n");
                TeamJsonHandling.CreateDummyJsonFile(GlobalParameters.JsonTableMappingFileName);
            }

            // Load the file into memory (data table and json list)
            TableMapping.GetMetadata(TeamJsonHandling.JsonFileConfiguration.TableMappingJsonFileName());

            // Handle unknown combobox values, by setting them to empty.
            var localConnectionKeyList =
                LocalTeamConnection.TeamConnectionKeyList(TeamConfiguration.ConnectionDictionary);
            List<string> userFeedbackList = new List<string>();
            foreach (DataRow row in TableMapping.DataTable.Rows)
            {
                var comboBoxValueSource = row[(int) TableMappingMetadataColumns.SourceConnection].ToString();
                var comboBoxValueTarget = row[(int) TableMappingMetadataColumns.TargetConnection].ToString();


                if (!localConnectionKeyList.Contains(comboBoxValueSource))
                {
                    if (!userFeedbackList.Contains(comboBoxValueSource))
                    {
                        userFeedbackList.Add(comboBoxValueSource);
                    }

                    row[(int) TableMappingMetadataColumns.SourceConnection] = DBNull.Value;
                }

                if (!localConnectionKeyList.Contains(comboBoxValueTarget))
                {
                    if (!userFeedbackList.Contains(comboBoxValueTarget))
                    {
                        userFeedbackList.Add(comboBoxValueTarget);
                    }

                    row[(int) TableMappingMetadataColumns.TargetConnection] = DBNull.Value;
                }
            }

            // Provide user feedback is any of the connections have been invalidated.
            if (userFeedbackList.Count > 0)
            {
                foreach (string issue in userFeedbackList)
                {
                    richTextBoxInformation.AppendText(
                        $"The connection '{issue}' found in the metadata file does not seem to exist in TEAM. The value has been defaulted in the grid, but not saved yet.\r\n");
                }
            }

            //Make sure the changes are seen as committed, so that changes can be detected later on.
            TableMapping.DataTable.AcceptChanges();

            // Order by Source Table, Integration_Area table, Business Key Attribute.
            TableMapping.SetDataTableColumns();
            TableMapping.SetDataTableSorting();

            _bindingSourceTableMetadata.DataSource = TableMapping.DataTable;

            // Set the column header names etc. for the data grid view.
            dataGridViewTableMetadata.DataSource = _bindingSourceTableMetadata;

            richTextBoxInformation.AppendText(
                $"The file {TeamJsonHandling.JsonFileConfiguration.TableMappingJsonFileName()} was loaded.\r\n");

            // Resize the grid
            GridAutoLayoutTableMappingMetadata();
        }

        /// <summary>
        /// Populates the Attribute Mapping DataGrid directly from an existing Json file.
        /// </summary>
        private void PopulateAttributeGridWithVersion()
        {
            //Check if the file exists, otherwise create a dummy / empty file   
            if (!File.Exists(TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName()))
            {
                richTextBoxInformation.AppendText(
                    $"No attribute mapping Json file was found, so a new empty one was created: {TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName()}.\r\n");
                TeamJsonHandling.CreateDummyJsonFile(GlobalParameters.JsonAttributeMappingFileName);
            }

            // Load the file into memory (data table and json list)
            AttributeMapping.GetMetadata(TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName());

            //Make sure the changes are seen as committed, so that changes can be detected later on.
            AttributeMapping.DataTable.AcceptChanges();

            AttributeMapping.SetDataTableColumns();
            AttributeMapping.SetDataTableSorting();

            _bindingSourceAttributeMetadata.DataSource = AttributeMapping.DataTable;

            // Set the column header names.
            dataGridViewAttributeMetadata.DataSource = _bindingSourceAttributeMetadata;
            dataGridViewAttributeMetadata.ColumnHeadersVisible = true;
            dataGridViewAttributeMetadata.Columns[0].Visible = false;
            dataGridViewAttributeMetadata.Columns[1].Visible = false;
            dataGridViewAttributeMetadata.Columns[6].ReadOnly = false;

            dataGridViewAttributeMetadata.Columns[0].HeaderText = "Hash Key";
            dataGridViewAttributeMetadata.Columns[1].HeaderText = "Version ID";
            dataGridViewAttributeMetadata.Columns[2].HeaderText = "Source Table";
            dataGridViewAttributeMetadata.Columns[3].HeaderText = "Source Column";
            dataGridViewAttributeMetadata.Columns[4].HeaderText = "Target Table";
            dataGridViewAttributeMetadata.Columns[5].HeaderText = "Target Column";
            dataGridViewAttributeMetadata.Columns[6].HeaderText = "Notes";

            richTextBoxInformation.AppendText(
                $"The file {TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName()} was loaded.\r\n");

            // Resize the grid
            GridAutoLayoutAttributeMetadata();
        }

        /// <summary>
        /// Populates the Physical Model DataGrid from an existing Json file.
        /// </summary>
        private void PopulatePhysicalModelGridWithVersion()
        {
            if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode)
            {
                //Check if the file exists, otherwise create a dummy / empty file   
                if (!File.Exists(TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName()))
                {
                    richTextBoxInformation.AppendText(
                        $"No Json file was found, so a new empty one was created: {TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName()}.\r\n");
                    TeamJsonHandling.CreateDummyJsonFile(GlobalParameters.JsonModelMetadataFileName);
                }

                // Load the file into memory (data table and json list)
                PhysicalModel.GetMetadata(TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName());

                //Make sure the changes are seen as committed, so that changes can be detected later on.
                PhysicalModel.DataTable.AcceptChanges();

                // Order by Source Table, Integration_Area table, Business Key Attribute.
                PhysicalModel.SetDataTableColumns();
                PhysicalModel.SetDataTableSorting();

                _bindingSourcePhysicalModelMetadata.DataSource = PhysicalModel.DataTable;

                // Data Grid View - set the column header names etc. for the data grid view.
                dataGridViewPhysicalModelMetadata.DataSource = _bindingSourcePhysicalModelMetadata;

                dataGridViewPhysicalModelMetadata.ColumnHeadersVisible = true;
                dataGridViewPhysicalModelMetadata.Columns[0].Visible = false;
                dataGridViewPhysicalModelMetadata.Columns[1].Visible = false;

                dataGridViewPhysicalModelMetadata.Columns[0].HeaderText = "Hash Key";
                dataGridViewPhysicalModelMetadata.Columns[1].HeaderText = "Version ID";
                dataGridViewPhysicalModelMetadata.Columns[2].HeaderText = "Database Name";
                dataGridViewPhysicalModelMetadata.Columns[3].HeaderText = "Schema Name";
                dataGridViewPhysicalModelMetadata.Columns[4].HeaderText = "Table Name";
                dataGridViewPhysicalModelMetadata.Columns[5].HeaderText = "Column Name";
                dataGridViewPhysicalModelMetadata.Columns[6].HeaderText = "Data Type";
                dataGridViewPhysicalModelMetadata.Columns[7].HeaderText = "Character Length";
                dataGridViewPhysicalModelMetadata.Columns[8].HeaderText = "Numeric Precision";
                dataGridViewPhysicalModelMetadata.Columns[9].HeaderText = "Numeric Scale";
                dataGridViewPhysicalModelMetadata.Columns[10].HeaderText = "Ordinal Position";
                dataGridViewPhysicalModelMetadata.Columns[11].HeaderText = "Primary Key Indicator";
                dataGridViewPhysicalModelMetadata.Columns[12].HeaderText = "Multi Active Indicator";

                richTextBoxInformation.AppendText(
                    $"The file {TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName()} was loaded.\r\n");

                // Resize the grid
                GridAutoLayoutPhysicalModelMetadata();
            }
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
            if (checkBoxResizeDataGrid.Checked == false)
                return;

            GridAutoLayoutTableMappingMetadata();
            GridAutoLayoutAttributeMetadata();
            GridAutoLayoutPhysicalModelMetadata();
        }

        private void GridAutoLayoutTableMappingMetadata()
        {
            if (checkBoxResizeDataGrid.Checked == false)
                return;

            dataGridViewTableMetadata.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewTableMetadata.Columns[dataGridViewTableMetadata.ColumnCount - 1].AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;

            // Disable the auto size again (to enable manual resizing).
            for (var i = 0; i < dataGridViewTableMetadata.Columns.Count - 1; i++)
            {

                dataGridViewTableMetadata.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridViewTableMetadata.Columns[i].Width = dataGridViewTableMetadata.Columns[i]
                    .GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            }

        }

        private void GridAutoLayoutAttributeMetadata()
        {
            if (checkBoxResizeDataGrid.Checked == false)
                return;

            dataGridViewAttributeMetadata.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            //dataGridViewLoadPatternCollection.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewAttributeMetadata.Columns[dataGridViewAttributeMetadata.ColumnCount - 1].AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;

            // Disable the auto size again (to enable manual resizing).
            for (var i = 0; i < dataGridViewAttributeMetadata.Columns.Count - 1; i++)
            {

                dataGridViewAttributeMetadata.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridViewAttributeMetadata.Columns[i].Width = dataGridViewAttributeMetadata.Columns[i]
                    .GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            }
        }

        private void GridAutoLayoutPhysicalModelMetadata()
        {
            if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode)
            {
                if (checkBoxResizeDataGrid.Checked == false)
                    return;

                dataGridViewPhysicalModelMetadata.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                //dataGridViewLoadPatternCollection.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridViewPhysicalModelMetadata.Columns[dataGridViewPhysicalModelMetadata.ColumnCount - 1]
                    .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                // Disable the auto size again (to enable manual resizing).
                for (var i = 0; i < dataGridViewPhysicalModelMetadata.Columns.Count - 1; i++)
                {

                    dataGridViewPhysicalModelMetadata.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dataGridViewPhysicalModelMetadata.Columns[i].Width = dataGridViewPhysicalModelMetadata.Columns[i]
                        .GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
                }
            }
        }

        private void ContentCounter()
        {
            int gridViewRows = dataGridViewTableMetadata.RowCount;
            var counter = 0;

            var hubSet = new HashSet<string>();
            var satSet = new HashSet<string>();
            var lnkSet = new HashSet<string>();
            var lsatSet = new HashSet<string>();

            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                var integrationTable = row.Cells[(int) TableMappingMetadataColumns.TargetTable].Value;

                if (gridViewRows != counter + 1 && integrationTable.ToString().Length > 3)
                {
                    if (integrationTable.ToString().Substring(0, 4) == "HUB_")
                    {
                        if (!hubSet.Contains(integrationTable.ToString()))
                        {
                            hubSet.Add(integrationTable.ToString());
                        }
                    }
                    else if (integrationTable.ToString().Substring(0, 4) == "SAT_")
                    {
                        if (!satSet.Contains(integrationTable.ToString()))
                        {
                            satSet.Add(integrationTable.ToString());
                        }
                    }
                    else if (integrationTable.ToString().Substring(0, 5) == "LSAT_")
                    {
                        if (!lsatSet.Contains(integrationTable.ToString()))
                        {
                            lsatSet.Add(integrationTable.ToString());
                        }
                    }
                    else if (integrationTable.ToString().Substring(0, 4) == "LNK_")
                    {
                        if (!lnkSet.Contains(integrationTable.ToString()))
                        {
                            lnkSet.Add(integrationTable.ToString());
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



        private void trackBarVersioning_ValueChanged(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();
            TeamJsonHandling.JsonFileConfiguration.jsonVersionExtension = @"_v" + trackBarVersioning.Value + ".json";
            GlobalParameters.CurrentVersionId = trackBarVersioning.Value;

            PopulateTableMappingGridWithVersion();
            PopulateAttributeGridWithVersion();
            PopulatePhysicalModelGridWithVersion();

            var versionMajorMinor =
                TeamVersionList.GetMajorMinorForVersionId(GlobalParameters.WorkingEnvironment,
                    trackBarVersioning.Value);

            labelVersion.Text = versionMajorMinor.Item2 + "." + versionMajorMinor.Item3;

            ContentCounter();
        }


        /// <summary>
        ///   Clicking the 'save' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSaveMetadata_Click(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();

            // Check if the current version is the maximum version. At this stage updates on earlier versions are not supported (and cause a NULL reference exception)
            var highestVersion = GlobalParameters.HighestVersionId;
            var currentVersion = GlobalParameters.CurrentVersionId;

            if (currentVersion < highestVersion)
            {
                richTextBoxInformation.Text +=
                    "Cannot save the metadata changes because these are applied to an earlier version. Only updates to the latest or newer version are supported in TEAM.";
            }
            else
            {
                // Create a data table containing the changes, to check if there are changes made to begin with
                var dataTableTableMappingChanges = ((DataTable) _bindingSourceTableMetadata.DataSource).GetChanges();
                var dataTableAttributeMappingChanges =
                    ((DataTable) _bindingSourceAttributeMetadata.DataSource).GetChanges();

                // Check if there are any rows available in the grid view, and if changes have been detected at all
                if (
                    dataGridViewTableMetadata.RowCount > 0 && dataTableTableMappingChanges != null &&
                    dataTableTableMappingChanges.Rows.Count > 0 ||
                    dataGridViewAttributeMetadata.RowCount > 0 && dataTableAttributeMappingChanges != null &&
                    dataTableAttributeMappingChanges.Rows.Count > 0
                )
                {
                    //Create new version, or retain the old one, depending on selection (version radiobuttons)

                    // Capture the 'old ' current version in case the UI needs updating
                    var oldVersionId = trackBarVersioning.Value;

                    //Retrieve the current version, or create a new one
                    int versionId = CreateOrRetrieveVersion();

                    //Commit the save of the metadata, one for each grid
                    try
                    {
                        SaveTableMappingMetadataJson(versionId, dataTableTableMappingChanges);
                    }
                    catch (Exception exception)
                    {
                        richTextBoxInformation.Text +=
                            "The Data Object Mapping metadata wasn't saved. There are errors saving the metadata version. The reported error is: " +
                            exception;
                    }

                    try
                    {
                        SaveAttributeMappingMetadata(versionId, dataTableAttributeMappingChanges);
                    }
                    catch (Exception exception)
                    {
                        richTextBoxInformation.Text +=
                            "The Data Item Mapping metadata wasn't saved. There are errors saving the metadata version. The reported error is: " +
                            exception;
                    }

                    if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode)
                    {
                        var dataTablePhysicalModelChanges =
                            ((DataTable) _bindingSourcePhysicalModelMetadata.DataSource).GetChanges();

                        if (dataGridViewPhysicalModelMetadata.RowCount > 0 && dataTablePhysicalModelChanges != null &&
                            dataTablePhysicalModelChanges.Rows.Count > 0)
                        {
                            try
                            {
                                SaveModelPhysicalModelMetadata(versionId, dataTablePhysicalModelChanges);
                            }
                            catch (Exception exception)
                            {
                                richTextBoxInformation.Text +=
                                    "The Physical Model metadata wasn't saved. There are errors saving the metadata version. The reported error is: " +
                                    exception;
                            }

                            PopulatePhysicalModelGridWithVersion();
                        }
                    }


                    //Load the grids from the repository after being updated
                    PopulateTableMappingGridWithVersion();
                    PopulateAttributeGridWithVersion();


                    //Refresh the UI to display the newly created version
                    if (oldVersionId != versionId)
                    {
                        var maxVersion =
                            TeamVersionList.GetMaxVersionForEnvironment(GlobalParameters.WorkingEnvironment);

                        trackBarVersioning.Maximum = maxVersion.Item1;
                        trackBarVersioning.TickFrequency =
                            TeamVersionList.GetTotalVersionCount(GlobalParameters.WorkingEnvironment);
                        trackBarVersioning.Value = maxVersion.Item1;
                    }
                }
                else
                {
                    richTextBoxInformation.Text += "There is no metadata to save!";
                }
            }
        }

        /// <summary>
        /// Verifies the version checkbox (major or minor) and creates new version instance. If 'no change' is checked this will return the current version Id.
        /// </summary>
        /// <returns></returns>
        private int CreateOrRetrieveVersion()
        {
            if (!radiobuttonNoVersionChange.Checked)
            {
                //If nothing is checked, just retrieve and return the current version
                var versionKeyValuePair =
                    TeamVersionList.GetMaxVersionForEnvironment(GlobalParameters.WorkingEnvironment);
                var majorVersion = versionKeyValuePair.Item2;
                var minorVersion = versionKeyValuePair.Item3;

                //Increase the major version, if required
                if (radiobuttonMajorRelease.Checked)
                {
                    try
                    {
                        //Creates a new version
                        majorVersion++;
                        minorVersion = 0;
                        TeamVersionList.AddNewVersionToList(GlobalParameters.WorkingEnvironment, majorVersion, 0);
                        TeamVersionList.SaveVersionList(GlobalParameters.CorePath +
                                                           GlobalParameters.VersionFileName +
                                                           GlobalParameters.JsonExtension);
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.Text += "An issue occurred when saving a new version: " + ex;
                    }
                }

                //Increase the minor version, if required
                if (radioButtonMinorRelease.Checked)
                {
                    try
                    {
                        //Creates a new version
                        minorVersion++;
                        TeamVersionList.AddNewVersionToList(GlobalParameters.WorkingEnvironment, majorVersion,
                            minorVersion);
                        TeamVersionList.SaveVersionList(GlobalParameters.CorePath +
                                                           GlobalParameters.VersionFileName +
                                                           GlobalParameters.JsonExtension);
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.Text += "An issue occurred when saving a new version: " + ex;
                    }
                }
            }

            //Retrieve the current version (again, may have changed).
            var newVersionKeyValuePair =
                TeamVersionList.GetMaxVersionForEnvironment(GlobalParameters.WorkingEnvironment);

            //Make sure the correct version is added to the global parameters
            GlobalParameters.CurrentVersionId = newVersionKeyValuePair.Item1;
            GlobalParameters.HighestVersionId = newVersionKeyValuePair.Item1;
            TeamJsonHandling.JsonFileConfiguration.jsonVersionExtension =
                @"_v" + newVersionKeyValuePair.Item1 + ".json";

            labelVersion.Text = newVersionKeyValuePair.Item2 + "." + newVersionKeyValuePair.Item3;

            return newVersionKeyValuePair.Item1;
        }

        /// <summary>
        /// Creates a new snapshot of the Physical Model metadata to a Json target repository, with the versionId as input parameter.
        /// This method creates a new version in the repository for the physical model (TEAM_Model.json file).
        /// </summary>
        /// <param name="versionId"></param>
        internal void CreateNewPhysicalModelMetadataVersionJson(int versionId)
        {
            // Update the version extension for the file.
            TeamJsonHandling.JsonFileConfiguration.jsonVersionExtension = @"_v" + versionId + ".json";

            // Create a JArray so segments can be added easily from the data table.
            var jsonModelMappingFull = new JArray();

            try
            {
                foreach (DataGridViewRow row in dataGridViewPhysicalModelMetadata.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        string databaseName = "";
                        string schemaName = "";
                        string tableName = "";
                        string columnName = "";
                        string dataType = "";
                        string maxLength = "0";
                        string numericPrecision = "0";
                        string numericScale = "0";
                        string ordinalPosition = "0";
                        string primaryKeyIndicator = "";
                        string multiActiveIndicator = "";

                        if (row.Cells[2].Value != DBNull.Value)
                        {
                            databaseName = (string) row.Cells[2].Value;
                        }

                        if (row.Cells[3].Value != DBNull.Value)
                        {
                            schemaName = (string) row.Cells[3].Value;
                        }

                        if (row.Cells[4].Value != DBNull.Value)
                        {
                            tableName = (string) row.Cells[4].Value;
                        }

                        if (row.Cells[5].Value != DBNull.Value)
                        {
                            columnName = (string) row.Cells[5].Value;
                        }

                        if (row.Cells[6].Value != DBNull.Value)
                        {
                            dataType = (string) row.Cells[6].Value;
                        }

                        if (row.Cells[7].Value != DBNull.Value)
                        {
                            maxLength = (string) row.Cells[7].Value;
                        }

                        if (row.Cells[8].Value != DBNull.Value)
                        {
                            numericPrecision = (string) row.Cells[8].Value;
                        }

                        if (row.Cells[9].Value != DBNull.Value)
                        {
                            numericScale = (string) row.Cells[9].Value;
                        }

                        if (row.Cells[10].Value != DBNull.Value)
                        {
                            ordinalPosition = (string) row.Cells[10].Value;
                        }

                        if (row.Cells[11].Value != DBNull.Value)
                        {
                            primaryKeyIndicator = (string) row.Cells[11].Value;
                        }

                        if (row.Cells[12].Value != DBNull.Value)
                        {
                            multiActiveIndicator = (string) row.Cells[12].Value;
                        }

                        string[] inputHashValue = new string[] {versionId.ToString(), tableName, columnName};
                        var hashKey = Utility.CreateMd5(inputHashValue, Utility.SandingElement);

                        JObject newJsonSegment = new JObject(
                            new JProperty("versionAttributeHash", hashKey),
                            new JProperty("versionId", versionId),
                            new JProperty("databaseName", databaseName),
                            new JProperty("schemaName", schemaName),
                            new JProperty("tableName", tableName),
                            new JProperty("columnName", columnName),
                            new JProperty("dataType", dataType),
                            new JProperty("characterLength", maxLength),
                            new JProperty("numericPrecision", numericPrecision),
                            new JProperty("numericScale", numericScale),
                            new JProperty("ordinalPosition", ordinalPosition),
                            new JProperty("primaryKeyIndicator", primaryKeyIndicator),
                            new JProperty("multiActiveIndicator", multiActiveIndicator)
                        );

                        jsonModelMappingFull.Add(newJsonSegment);
                    }
                }

            }
            catch (Exception ex)
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning,
                    $"A snapshot of the physical model was attempted to be created as a Json array, but this did not succeed. The message is {ex}."));
            }

            try
            {
                //Generate a unique key using a hash
                string output = JsonConvert.SerializeObject(jsonModelMappingFull, Formatting.Indented);
                string outputFileName = TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName();
                File.WriteAllText(outputFileName, output);
            }
            catch (JsonReaderException ex)
            {
                richTextBoxInformation.Text +=
                    "There were issues inserting the new Json version file for the Physical Model.\r\n" + ex;
            }

        }

        /// <summary>
        /// Creates a new snapshot of the Table Mapping metadata for a JSON repository, with the versionId as input parameter. A new file will created for the provided version Id.
        /// </summary>
        /// <param name="versionId"></param>
        internal void CreateNewTableMappingMetadataVersionJson(int versionId)
        {
            TeamJsonHandling.JsonFileConfiguration.jsonVersionExtension = @"_v" + versionId + ".json";

            // Create a JArray so segments can be added easily from the data table
            var jsonTableMappingFull = new JArray();

            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (!row.IsNewRow)
                {
                    try
                    {
                        var sourceTable = "";
                        var sourceConnectionKey = "";
                        var targetTable = "";
                        var targetConnectionKey = "";
                        var businessKeyDefinition = "";
                        var drivingKeyDefinition = "";
                        var filterCriterion = "";
                        bool generateIndicator = true;

                        if (row.Cells[TableMappingMetadataColumns.SourceTable.ToString()].Value != DBNull.Value)
                        {
                            sourceTable = (string) row.Cells[TableMappingMetadataColumns.SourceTable.ToString()].Value;
                        }

                        // Source Connection
                        if (row.Cells[TableMappingMetadataColumns.SourceConnection.ToString()].Value != DBNull.Value)
                        {
                            sourceConnectionKey =
                                (string) row.Cells[TableMappingMetadataColumns.SourceConnection.ToString()].Value;
                        }

                        if (row.Cells[TableMappingMetadataColumns.TargetTable.ToString()].Value != DBNull.Value)
                        {
                            targetTable = (string) row.Cells[TableMappingMetadataColumns.TargetTable.ToString()].Value;
                        }

                        // Target Connection
                        if (row.Cells[TableMappingMetadataColumns.TargetConnection.ToString()].Value != DBNull.Value)
                        {
                            targetConnectionKey =
                                (string) row.Cells[TableMappingMetadataColumns.TargetConnection.ToString()].Value;
                        }

                        if (row.Cells[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()].Value !=
                            DBNull.Value)
                        {
                            businessKeyDefinition =
                                (string) row.Cells[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()].Value;
                            //businessKeyDefinition = businessKeyDefinition.Replace("'", "''");  //Double quotes for composites
                        }

                        if (row.Cells[TableMappingMetadataColumns.DrivingKeyDefinition.ToString()].Value !=
                            DBNull.Value)
                        {
                            drivingKeyDefinition =
                                (string) row.Cells[TableMappingMetadataColumns.DrivingKeyDefinition.ToString()].Value;
                            //drivingKeyDefinition = drivingKeyDefinition.Replace("'", "''"); //Double quotes for composites
                        }

                        if (row.Cells[TableMappingMetadataColumns.FilterCriterion.ToString()].Value != DBNull.Value)
                        {
                            filterCriterion = (string) row.Cells[TableMappingMetadataColumns.FilterCriterion.ToString()]
                                .Value;
                            //filterCriterion = filterCriterion.Replace("'", "''"); //Double quotes for composites
                        }

                        if (row.Cells[TableMappingMetadataColumns.Enabled.ToString()].Value != DBNull.Value)
                        {
                            generateIndicator = (bool) row.Cells[TableMappingMetadataColumns.Enabled.ToString()].Value;
                            //generateIndicator = generateIndicator.Replace("'", "''"); //Double quotes for composites
                        }

                        string[] inputHashValue = new string[]
                        {
                            versionId.ToString(), sourceTable, targetTable, businessKeyDefinition, drivingKeyDefinition,
                            filterCriterion
                        };
                        var hashKey = Utility.CreateMd5(inputHashValue, Utility.SandingElement);

                        // Convert it into a JArray so segments can be added easily
                        JObject newJsonSegment = new JObject(
                            new JProperty("enabledIndicator", generateIndicator),
                            new JProperty("tableMappingHash", hashKey),
                            new JProperty("versionId", versionId),
                            new JProperty("sourceTable", sourceTable),
                            new JProperty("sourceConnectionKey", sourceConnectionKey),
                            new JProperty("targetTable", targetTable),
                            new JProperty("targetConnectionKey", targetConnectionKey),
                            new JProperty("businessKeyDefinition", businessKeyDefinition),
                            new JProperty("drivingKeyDefinition", drivingKeyDefinition),
                            new JProperty("filterCriteria", filterCriterion)
                        );

                        jsonTableMappingFull.Add(newJsonSegment);
                    }
                    catch
                    {
                        // TBD
                    }
                }
            }

            try
            {
                //Generate a unique key using a hash
                string output = JsonConvert.SerializeObject(jsonTableMappingFull, Formatting.Indented);
                string outputFileName = TeamJsonHandling.JsonFileConfiguration.TableMappingJsonFileName();

                File.WriteAllText(outputFileName, output);
            }
            catch (JsonReaderException ex)
            {
                richTextBoxInformation.Text +=
                    "There were issues inserting the new Json version file for the Table Mapping.\r\n" + ex;
            }
        }

        /// <summary>
        /// Creates a new snapshot of the Attribute Mapping metadata for a JSON repository, with the versionId as input parameter. A new file will created for the provided version Id.
        /// </summary>
        /// <param name="versionId"></param>
        internal void CreateNewAttributeMappingMetadataVersionJson(int versionId)
        {
            TeamJsonHandling.JsonFileConfiguration.jsonVersionExtension = @"_v" + versionId + ".json";

            // Create a JArray so segments can be added easily from the datatable
            var jsonAttributeMappingFull = new JArray();

            foreach (DataGridViewRow row in dataGridViewAttributeMetadata.Rows)
            {
                if (!row.IsNewRow)
                {
                    var stagingTable = "";
                    var stagingColumn = "";
                    var integrationTable = "";
                    var integrationColumn = "";
                    var notes = "";

                    if (row.Cells[2].Value != DBNull.Value)
                    {
                        stagingTable = (string) row.Cells[2].Value;
                    }

                    if (row.Cells[3].Value != DBNull.Value)
                    {
                        stagingColumn = (string) row.Cells[3].Value;
                    }

                    if (row.Cells[4].Value != DBNull.Value)
                    {
                        integrationTable = (string) row.Cells[4].Value;
                    }

                    if (row.Cells[5].Value != DBNull.Value)
                    {
                        integrationColumn = (string) row.Cells[5].Value;
                    }

                    if (row.Cells[6].Value != DBNull.Value)
                    {
                        notes = (string) row.Cells[6].Value;
                    }


                    string[] inputHashValue = new string[]
                        {versionId.ToString(), stagingTable, stagingColumn, integrationTable, integrationColumn, notes};
                    var hashKey = Utility.CreateMd5(inputHashValue, Utility.SandingElement);


                    JObject newJsonSegment = new JObject(
                        new JProperty("attributeMappingHash", hashKey),
                        new JProperty("versionId", versionId),
                        new JProperty("sourceTable", stagingTable),
                        new JProperty("sourceAttribute", stagingColumn),
                        new JProperty("targetTable", integrationTable),
                        new JProperty("targetAttribute", integrationColumn),
                        new JProperty("notes", notes)
                    );

                    jsonAttributeMappingFull.Add(newJsonSegment);
                }
            }

            // Execute the statement, if the repository is JSON
            try
            {
                //Generate a unique key using a hash
                string output = JsonConvert.SerializeObject(jsonAttributeMappingFull, Formatting.Indented);
                string outputFileName = TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                File.WriteAllText(outputFileName, output);
            }
            catch (JsonReaderException ex)
            {
                richTextBoxInformation.Text +=
                    "There were issues inserting the new JSON version file for the Attribute Mapping.\r\n" + ex;
            }

        }

        private void SaveTableMappingMetadataJson(int versionId, DataTable dataTableChanges)
        {
            if (TeamJsonHandling.JsonFileConfiguration.newFileTableMapping == "true")
            {
                TeamJsonHandling.RemoveExistingJsonFile(GlobalParameters.JsonTableMappingFileName + @"_v" +
                                                        GlobalParameters.CurrentVersionId + ".json");
                TeamJsonHandling.CreatePlaceholderJsonFile(GlobalParameters.JsonTableMappingFileName);
                TeamJsonHandling.JsonFileConfiguration.newFileTableMapping = "false";
            }

            //If no change radio buttons are selected this means either minor or major version is checked, so a full new snapshot will be created of everything.
            if (!radiobuttonNoVersionChange.Checked)
            {
                CreateNewTableMappingMetadataVersionJson(versionId);
            }

            //... otherwise an in-place update to the existing version is done (insert / update / delete)
            else
            {
                if (dataTableChanges != null && (dataTableChanges.Rows.Count > 0)
                ) //Double-check if there are any changes made at all
                {
                    foreach (DataRow row in dataTableChanges.Rows) //Start looping through the changes
                    {
                        #region Changed rows

                        //Changed rows
                        if ((row.RowState & DataRowState.Modified) != 0)
                        {
                            //Grab the attributes into local variables
                            string hashKey = (string) row[TableMappingMetadataColumns.HashKey.ToString()];
                            int versionKey = (int) row[TableMappingMetadataColumns.VersionId.ToString()];
                            var stagingTable = "";
                            var sourceConnectionKey = "";
                            var targetConnectionKey = "";
                            var integrationTable = "";
                            var businessKeyDefinition = "";
                            var drivingKeyDefinition = "";
                            var filterCriterion = "";
                            bool generateIndicator = true;

                            if (row[TableMappingMetadataColumns.Enabled.ToString()] != DBNull.Value)
                            {
                                generateIndicator = (bool) row[TableMappingMetadataColumns.Enabled.ToString()];
                            }

                            if (row[TableMappingMetadataColumns.SourceTable.ToString()] != DBNull.Value)
                            {
                                stagingTable = (string) row[TableMappingMetadataColumns.SourceTable.ToString()];
                            }

                            if (row[TableMappingMetadataColumns.SourceConnection.ToString()] != DBNull.Value)
                            {
                                sourceConnectionKey =
                                    (string) row[TableMappingMetadataColumns.SourceConnection.ToString()];
                            }

                            if (row[TableMappingMetadataColumns.TargetTable.ToString()] != DBNull.Value)
                            {
                                integrationTable = (string) row[TableMappingMetadataColumns.TargetTable.ToString()];
                            }

                            if (row[TableMappingMetadataColumns.TargetConnection.ToString()] != DBNull.Value)
                            {
                                targetConnectionKey =
                                    (string) row[TableMappingMetadataColumns.TargetConnection.ToString()];
                            }

                            if (row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()] != DBNull.Value)
                            {
                                businessKeyDefinition =
                                    (string) row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()];
                            }

                            if (row[TableMappingMetadataColumns.DrivingKeyDefinition.ToString()] != DBNull.Value)
                            {
                                drivingKeyDefinition =
                                    (string) row[TableMappingMetadataColumns.DrivingKeyDefinition.ToString()];
                            }

                            if (row[TableMappingMetadataColumns.FilterCriterion.ToString()] != DBNull.Value)
                            {
                                filterCriterion = (string) row[TableMappingMetadataColumns.FilterCriterion.ToString()];
                            }

                            //Read the file in memory
                            string inputFileName = TeamJsonHandling.JsonFileConfiguration.TableMappingJsonFileName();
                            TableMappingJson[] jsonArray =
                                JsonConvert.DeserializeObject<TableMappingJson[]>(File.ReadAllText(inputFileName));

                            //Retrieves the json segment in the file for the given hash returns value or NULL
                            var jsonHash = jsonArray.FirstOrDefault(obj => obj.tableMappingHash == hashKey);

                            if (jsonHash.tableMappingHash == "")
                            {
                                richTextBoxInformation.Text +=
                                    "The correct segment in the Json file was not found.\r\n";
                            }
                            else
                            {
                                // Update the values in the JSON segment
                                jsonHash.enabledIndicator = generateIndicator;
                                jsonHash.sourceTable = stagingTable;
                                jsonHash.sourceConnectionKey = sourceConnectionKey;
                                jsonHash.targetTable = integrationTable;
                                jsonHash.targetConnectionKey = targetConnectionKey;
                                jsonHash.businessKeyDefinition = businessKeyDefinition;
                                jsonHash.drivingKeyDefinition = drivingKeyDefinition;
                                jsonHash.filterCriteria = filterCriterion;
                            }

                            string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);

                            try
                            {
                                // Write the updated JSON file to disk. NOTE - DOES NOT ALWAYS WORK WHEN FILE IS OPEN IN NOTEPAD AND DOES NOT RAISE EXCEPTION
                                string outputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.TableMappingJsonFileName();
                                File.WriteAllText(outputFileName, output);
                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text +=
                                    "There were issues saving the Json update to disk.\r\n" + ex;
                            }
                        }

                        #endregion

                        #region Inserted rows

                        //Inserted rows
                        if ((row.RowState & DataRowState.Added) != 0)
                        {
                            var sourceTable = "";
                            var sourceConnectionKey = "";
                            var targetTable = "";
                            var targetConnectionKey = "";
                            var businessKeyDefinition = "";
                            var drivingKeyDefinition = "";
                            var filterCriterion = "";
                            bool generateIndicator = true;

                            if (row[(int) TableMappingMetadataColumns.Enabled] != DBNull.Value)
                            {
                                generateIndicator = (bool) row[(int) TableMappingMetadataColumns.Enabled];
                                //generateIndicator = generateIndicator.Replace("'", "''");
                            }

                            // Source
                            if (row[(int) TableMappingMetadataColumns.SourceTable] != DBNull.Value)
                            {
                                sourceTable = (string) row[(int) TableMappingMetadataColumns.SourceTable];
                            }

                            // Source Connection
                            if (row[(int) TableMappingMetadataColumns.SourceConnection] != DBNull.Value)
                            {
                                sourceConnectionKey =
                                    (string) row[TableMappingMetadataColumns.SourceConnection.ToString()];
                            }

                            // Target
                            if (row[(int) TableMappingMetadataColumns.TargetTable] != DBNull.Value)
                            {
                                targetTable = (string) row[(int) TableMappingMetadataColumns.TargetTable];
                            }

                            // Target Connection
                            if (row[(int) TableMappingMetadataColumns.TargetConnection] != DBNull.Value)
                            {
                                targetConnectionKey =
                                    (string) row[TableMappingMetadataColumns.TargetConnection.ToString()];
                            }

                            if (row[(int) TableMappingMetadataColumns.BusinessKeyDefinition] != DBNull.Value)
                            {
                                businessKeyDefinition =
                                    (string) row[(int) TableMappingMetadataColumns.BusinessKeyDefinition];
                                //businessKeyDefinition = businessKeyDefinition.Replace("'", "''");
                                //Double quotes for composites
                            }

                            if (row[(int) TableMappingMetadataColumns.DrivingKeyDefinition] != DBNull.Value)
                            {
                                drivingKeyDefinition =
                                    (string) row[(int) TableMappingMetadataColumns.DrivingKeyDefinition];
                                //drivingKeyDefinition = drivingKeyDefinition.Replace("'", "''");
                            }

                            if (row[(int) TableMappingMetadataColumns.FilterCriterion] != DBNull.Value)
                            {
                                filterCriterion = (string) row[(int) TableMappingMetadataColumns.FilterCriterion];
                                //filterCriterion = filterCriterion.Replace("'", "''");
                            }

                            try
                            {
                                var jsonTableMappingFull = new JArray();

                                // Load the file, if existing information needs to be merged
                                var mappingFileName = TeamJsonHandling.JsonFileConfiguration.TableMappingJsonFileName();
                                TableMappingJson[] jsonArray =
                                    JsonConvert.DeserializeObject<TableMappingJson[]>(
                                        File.ReadAllText(mappingFileName));

                                // Convert it into a JArray so segments can be added easily
                                if (jsonArray != null)
                                {
                                    jsonTableMappingFull = JArray.FromObject(jsonArray);
                                }

                                string[] inputHashValue = new string[]
                                {
                                    versionId.ToString(), sourceTable, targetTable, businessKeyDefinition,
                                    drivingKeyDefinition, filterCriterion
                                };
                                var hashKey = Utility.CreateMd5(inputHashValue, Utility.SandingElement);

                                // Convert it into a JArray so segments can be added easily
                                JObject newJsonSegment = new JObject(
                                    new JProperty("enabledIndicator", generateIndicator),
                                    new JProperty("tableMappingHash", hashKey),
                                    new JProperty("versionId", versionId),
                                    new JProperty("sourceTable", sourceTable),
                                    new JProperty("sourceConnectionKey", sourceConnectionKey),
                                    new JProperty("targetTable", targetTable),
                                    new JProperty("targetConnectionKey", targetConnectionKey),
                                    new JProperty("businessKeyDefinition", businessKeyDefinition),
                                    new JProperty("drivingKeyDefinition", drivingKeyDefinition),
                                    new JProperty("filterCriteria", filterCriterion)
                                );

                                jsonTableMappingFull.Add(newJsonSegment);

                                string output = JsonConvert.SerializeObject(jsonTableMappingFull, Formatting.Indented);
                                var outputFileName = TeamJsonHandling.JsonFileConfiguration.TableMappingJsonFileName();
                                File.WriteAllText(outputFileName, output);

                                //Making sure the hash key value is added to the data table as well
                                row[(int) TableMappingMetadataColumns.HashKey] = hashKey;

                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text +=
                                    "There were issues inserting the Json segment / record.\r\n" + ex;
                            }

                        }

                        #endregion

                        #region Deleted rows

                        //Deleted rows
                        if ((row.RowState & DataRowState.Deleted) != 0)
                        {
                            var hashKey = row[TableMappingMetadataColumns.HashKey.ToString(), DataRowVersion.Original]
                                .ToString();
                            var versionKey = row[TableMappingMetadataColumns.VersionId.ToString(),
                                DataRowVersion.Original].ToString();

                            try
                            {
                                string inputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.TableMappingJsonFileName();
                                var jsonArray = JsonConvert
                                    .DeserializeObject<TableMappingJson[]>(File.ReadAllText(inputFileName)).ToList();

                                //Retrieves the json segment in the file for the given hash returns value or NULL
                                var jsonSegment = jsonArray.FirstOrDefault(obj => obj.tableMappingHash == hashKey);

                                jsonArray.Remove(jsonSegment);

                                if (jsonSegment.tableMappingHash == "")
                                {
                                    richTextBoxInformation.Text +=
                                        "The correct segment in the Json file was not found.\r\n";
                                }
                                else
                                {
                                    //Remove the segment from the JSON
                                    jsonArray.Remove(jsonSegment);
                                }

                                string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                                string outputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.TableMappingJsonFileName();
                                File.WriteAllText(outputFileName, output);

                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += "There were issues applying the Json update.\r\n" + ex;
                            }

                        }

                        #endregion
                    }

                    #region Statement execution

                    // Execute the statement. If the source is JSON this is done in separate calls for now

                    // Committing the changes to the data table - making sure new changes can be picked up
                    // AcceptChanges will clear all New, Deleted and/or Modified settings
                    dataTableChanges.AcceptChanges();
                    ((DataTable) _bindingSourceTableMetadata.DataSource).AcceptChanges();

                    #endregion

                    richTextBoxInformation.Text += "The Data Object Mapping metadata has been saved.\r\n";
                }
            } // End of constructing the statements for insert / update / delete
        }

        private void SaveModelPhysicalModelMetadata(int versionId, DataTable dataTableChanges)
        {
            if (TeamJsonHandling.JsonFileConfiguration.newFilePhysicalModel == "true")
            {
                TeamJsonHandling.RemoveExistingJsonFile(GlobalParameters.JsonModelMetadataFileName + @"_v" +
                                                        GlobalParameters.CurrentVersionId + ".json");
                TeamJsonHandling.CreatePlaceholderJsonFile(GlobalParameters.JsonModelMetadataFileName);
                TeamJsonHandling.JsonFileConfiguration.newFilePhysicalModel = "false";
            }

            //If the save version radiobutton is selected it means either minor or major version is checked and a full new snapshot needs to be created first
            if (!radiobuttonNoVersionChange.Checked)
            {
                CreateNewPhysicalModelMetadataVersionJson(versionId);
            }
            //An in-place update (no change) to the existing version is done
            else
            {
                //Grabbing the generic settings from the main forms
                if (dataTableChanges != null && dataTableChanges.Rows.Count > 0
                ) //Check if there are any changes made at all
                {
                    foreach (DataRow row in dataTableChanges.Rows) //Loop through the detected changes
                    {
                        #region Changed rows

                        //Changed rows
                        if ((row.RowState & DataRowState.Modified) != 0)
                        {
                            //Grab the attributes into local variables
                            string hashKey = (string) row[PhysicalModelMappingMetadataColumns.HashKey.ToString()];
                            int versionKey = (int) row[PhysicalModelMappingMetadataColumns.VersionId.ToString()];
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

                            if (row[PhysicalModelMappingMetadataColumns.DatabaseName.ToString()] != DBNull.Value)
                            {
                                databaseName =
                                    (string) row[PhysicalModelMappingMetadataColumns.DatabaseName.ToString()];
                            }

                            if (row[PhysicalModelMappingMetadataColumns.SchemaName.ToString()] != DBNull.Value)
                            {
                                schemaName = (string) row[PhysicalModelMappingMetadataColumns.SchemaName.ToString()];
                            }

                            if (row[PhysicalModelMappingMetadataColumns.TableName.ToString()] != DBNull.Value)
                            {
                                tableName = (string) row[PhysicalModelMappingMetadataColumns.TableName.ToString()];
                            }

                            if (row[PhysicalModelMappingMetadataColumns.ColumnName.ToString()] != DBNull.Value)
                            {
                                columnName = (string) row[PhysicalModelMappingMetadataColumns.ColumnName.ToString()];
                            }

                            if (row[PhysicalModelMappingMetadataColumns.DataType.ToString()] != DBNull.Value)
                            {
                                dataType = (string) row[PhysicalModelMappingMetadataColumns.DataType.ToString()];
                            }

                            if (row[PhysicalModelMappingMetadataColumns.CharacterLength.ToString()] != DBNull.Value)
                            {
                                characterLength =
                                    (string) row[PhysicalModelMappingMetadataColumns.CharacterLength.ToString()];
                            }

                            if (row[PhysicalModelMappingMetadataColumns.NumericPrecision.ToString()] != DBNull.Value)
                            {
                                numericPrecision =
                                    (string) row[PhysicalModelMappingMetadataColumns.NumericPrecision.ToString()];
                            }

                            if (row[PhysicalModelMappingMetadataColumns.NumericScale.ToString()] != DBNull.Value)
                            {
                                numericScale =
                                    (string) row[PhysicalModelMappingMetadataColumns.NumericScale.ToString()];
                            }

                            if (row[PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString()] != DBNull.Value)
                            {
                                ordinalPosition =
                                    (string) row[PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString()];
                            }

                            if (row[PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator.ToString()] != DBNull.Value)
                            {
                                primaryKeyIndicator =
                                    (string) row[PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator.ToString()];
                            }

                            if (row[PhysicalModelMappingMetadataColumns.MultiActiveIndicator.ToString()] !=
                                DBNull.Value)
                            {
                                multiActiveIndicator =
                                    (string) row[PhysicalModelMappingMetadataColumns.MultiActiveIndicator.ToString()];
                            }

                            try
                            {
                                var inputFileName = TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName();
                                PhysicalModelMetadataJson[] jsonArray =
                                    JsonConvert.DeserializeObject<PhysicalModelMetadataJson[]>(
                                        File.ReadAllText(inputFileName));

                                var jsonHash =
                                    jsonArray.FirstOrDefault(obj =>
                                        obj.versionAttributeHash ==
                                        hashKey); //Retrieves the json segment in the file for the given hash returns value or NULL

                                if (jsonHash.versionAttributeHash == "")
                                {
                                    richTextBoxInformation.Text +=
                                        "The correct segment in the Json file was not found.\r\n";
                                }
                                else
                                {
                                    // Update the values in the Json segment
                                    jsonHash.databaseName = databaseName;
                                    jsonHash.schemaName = schemaName;
                                    jsonHash.tableName = tableName;
                                    jsonHash.columnName = columnName;
                                    jsonHash.dataType = dataType;
                                    jsonHash.characterLength = characterLength;
                                    jsonHash.numericPrecision = numericPrecision;
                                    jsonHash.numericScale = numericScale;
                                    jsonHash.ordinalPosition = ordinalPosition;
                                    jsonHash.primaryKeyIndicator = primaryKeyIndicator;
                                    jsonHash.multiActiveIndicator = multiActiveIndicator;
                                }

                                string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                                string outputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName();

                                File.WriteAllText(outputFileName, output);
                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += "There were issues applying the Json update.\r\n" + ex;
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
                            string maxLength = "0";
                            string numericPrecision = "0";
                            string numericScale = "0";
                            string ordinalPosition = "0";
                            string primaryKeyIndicator = "";
                            string multiActiveIndicator = "";

                            if (row[(int) PhysicalModelMappingMetadataColumns.DatabaseName] != DBNull.Value)
                            {
                                databaseName = (string) row[(int) PhysicalModelMappingMetadataColumns.DatabaseName];
                            }

                            if (row[(int) PhysicalModelMappingMetadataColumns.SchemaName] != DBNull.Value)
                            {
                                schemaName = (string) row[(int) PhysicalModelMappingMetadataColumns.SchemaName];
                            }

                            if (row[(int) PhysicalModelMappingMetadataColumns.TableName] != DBNull.Value)
                            {
                                tableName = (string) row[(int) PhysicalModelMappingMetadataColumns.TableName];
                            }

                            if (row[(int) PhysicalModelMappingMetadataColumns.ColumnName] != DBNull.Value)
                            {
                                columnName = (string) row[(int) PhysicalModelMappingMetadataColumns.ColumnName];
                            }

                            if (row[(int) PhysicalModelMappingMetadataColumns.DataType] != DBNull.Value)
                            {
                                dataType = (string) row[(int) PhysicalModelMappingMetadataColumns.DataType];
                            }

                            if (row[(int) PhysicalModelMappingMetadataColumns.CharacterLength] != DBNull.Value)
                            {
                                maxLength = (string) row[(int) PhysicalModelMappingMetadataColumns.CharacterLength];
                            }

                            if (row[(int) PhysicalModelMappingMetadataColumns.NumericPrecision] != DBNull.Value)
                            {
                                numericPrecision =
                                    (string) row[(int) PhysicalModelMappingMetadataColumns.NumericPrecision];
                            }

                            if (row[(int) PhysicalModelMappingMetadataColumns.NumericScale] != DBNull.Value)
                            {
                                numericScale = (string) row[(int) PhysicalModelMappingMetadataColumns.NumericScale];
                            }

                            if (row[(int) PhysicalModelMappingMetadataColumns.OrdinalPosition] != DBNull.Value)
                            {
                                ordinalPosition =
                                    (string) row[(int) PhysicalModelMappingMetadataColumns.OrdinalPosition];
                            }

                            if (row[(int) PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator] != DBNull.Value)
                            {
                                primaryKeyIndicator =
                                    (string) row[(int) PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator];
                            }

                            if (row[(int) PhysicalModelMappingMetadataColumns.MultiActiveIndicator] != DBNull.Value)
                            {
                                multiActiveIndicator =
                                    (string) row[(int) PhysicalModelMappingMetadataColumns.MultiActiveIndicator];
                            }

                            try
                            {
                                var jsonPhysicalModelMappingFull = new JArray();

                                // Load the file, if existing information needs to be merged
                                string inputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName();
                                PhysicalModelMetadataJson[] jsonArray =
                                    JsonConvert.DeserializeObject<PhysicalModelMetadataJson[]>(
                                        File.ReadAllText(inputFileName));

                                // Convert it into a JArray so segments can be added easily
                                if (jsonArray != null)
                                {
                                    jsonPhysicalModelMappingFull = JArray.FromObject(jsonArray);
                                }
                                //Generate a unique key using a hash

                                string[] inputHashValue = new string[] {versionId.ToString(), tableName, columnName};
                                var hashKey = Utility.CreateMd5(inputHashValue, Utility.SandingElement);

                                JObject newJsonSegment = new JObject(
                                    new JProperty("versionAttributeHash", hashKey),
                                    new JProperty("versionId", versionId),
                                    new JProperty("databaseName", databaseName),
                                    new JProperty("schemaName", schemaName),
                                    new JProperty("tableName", tableName),
                                    new JProperty("columnName", columnName),
                                    new JProperty("dataType", dataType),
                                    new JProperty("characterLength", maxLength),
                                    new JProperty("numericPrecision", numericPrecision),
                                    new JProperty("numericScale", numericScale),
                                    new JProperty("ordinalPosition", ordinalPosition),
                                    new JProperty("primaryKeyIndicator", primaryKeyIndicator),
                                    new JProperty("multiActiveIndicator", multiActiveIndicator)
                                );

                                jsonPhysicalModelMappingFull.Add(newJsonSegment);

                                string output = JsonConvert.SerializeObject(jsonPhysicalModelMappingFull,
                                    Formatting.Indented);
                                string outputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName();
                                File.WriteAllText(outputFileName, output);

                                //Making sure the hash key value is added to the datatable as well
                                row[(int) PhysicalModelMappingMetadataColumns.HashKey] = hashKey;

                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text +=
                                    "There were issues inserting the JSON segment / record.\r\n" + ex;
                            }

                        }

                        #endregion

                        #region Deleted rows

                        //Deleted rows
                        if ((row.RowState & DataRowState.Deleted) != 0)
                        {
                            var hashKey = row[PhysicalModelMappingMetadataColumns.HashKey.ToString(),
                                DataRowVersion.Original].ToString();
                            var versionKey = row[PhysicalModelMappingMetadataColumns.VersionId.ToString(),
                                DataRowVersion.Original].ToString();

                            try
                            {
                                string inputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName();
                                var jsonArray = JsonConvert
                                    .DeserializeObject<PhysicalModelMetadataJson[]>(File.ReadAllText(inputFileName))
                                    .ToList();

                                //Retrieves the json segment in the file for the given hash returns value or NULL
                                var jsonSegment = jsonArray.FirstOrDefault(obj => obj.versionAttributeHash == hashKey);

                                jsonArray.Remove(jsonSegment);

                                if (jsonSegment.versionAttributeHash == "")
                                {
                                    richTextBoxInformation.Text +=
                                        "The correct segment in the JSON file was not found.\r\n";
                                }
                                else
                                {
                                    //Remove the segment from the JSON
                                    jsonArray.Remove(jsonSegment);
                                }

                                string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                                string outputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.PhysicalModelJsonFileName();
                                File.WriteAllText(outputFileName, output);

                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += "There were issues applying the JSON update.\r\n" + ex;
                            }

                        }

                        #endregion

                    } // All changes have been processed.

                    #region Statement execution

                    //Committing the changes to the data table
                    dataTableChanges.AcceptChanges();
                    ((DataTable) _bindingSourcePhysicalModelMetadata.DataSource).AcceptChanges();

                    richTextBoxInformation.AppendText("The (physical) model metadata has been saved.\r\n");

                    #endregion
                }
            }
        }

        private void SaveAttributeMappingMetadata(int versionId, DataTable dataTableChanges)
        {
            if (TeamJsonHandling.JsonFileConfiguration.newFileAttributeMapping == "true")
            {
                TeamJsonHandling.RemoveExistingJsonFile(GlobalParameters.JsonAttributeMappingFileName + @"_v" +
                                                        GlobalParameters.CurrentVersionId + ".json");
                TeamJsonHandling.CreatePlaceholderJsonFile(GlobalParameters.JsonAttributeMappingFileName);
                TeamJsonHandling.JsonFileConfiguration.newFileAttributeMapping = "false";
            }

            //If the save version radiobutton is selected it means either minor or major version is checked and a full new snapshot needs to be created first
            if (!radiobuttonNoVersionChange.Checked)
            {
                CreateNewAttributeMappingMetadataVersionJson(versionId);
            }
            // An update (no change) to the existing version is done with regular inserts, updates and deletes
            else
            {
                if (dataTableChanges != null && (dataTableChanges.Rows.Count > 0))
                    //Check if there are any changes made at all
                {
                    // Loop through the changes captured in the data table
                    foreach (DataRow row in dataTableChanges.Rows)
                    {
                        #region Updates in Attribute Mapping

                        // Updates
                        if ((row.RowState & DataRowState.Modified) != 0)
                        {
                            //Grab the attributes into local variables
                            var hashKey = (string) row[AttributeMappingMetadataColumns.HashKey.ToString()];
                            var versionKey = row[AttributeMappingMetadataColumns.VersionId.ToString()].ToString();
                            var stagingTable = "";
                            var stagingColumn = "";
                            var integrationTable = "";
                            var integrationColumn = "";
                            var notes = "";

                            if (row[AttributeMappingMetadataColumns.SourceTable.ToString()] != DBNull.Value)
                            {
                                stagingTable = (string) row[AttributeMappingMetadataColumns.SourceTable.ToString()];
                            }

                            if (row[AttributeMappingMetadataColumns.SourceColumn.ToString()] != DBNull.Value)
                            {
                                stagingColumn = (string) row[AttributeMappingMetadataColumns.SourceColumn.ToString()];
                            }

                            if (row[AttributeMappingMetadataColumns.TargetTable.ToString()] != DBNull.Value)
                            {
                                integrationTable = (string) row[AttributeMappingMetadataColumns.TargetTable.ToString()];
                            }

                            if (row[AttributeMappingMetadataColumns.TargetColumn.ToString()] != DBNull.Value)
                            {
                                integrationColumn =
                                    (string) row[AttributeMappingMetadataColumns.TargetColumn.ToString()];
                            }

                            if (row[AttributeMappingMetadataColumns.Notes.ToString()] != DBNull.Value)
                            {
                                notes = (string) row[AttributeMappingMetadataColumns.Notes.ToString()];
                            }

                            try
                            {
                                var inputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                                AttributeMappingJson[] jsonArray =
                                    JsonConvert.DeserializeObject<AttributeMappingJson[]>(
                                        File.ReadAllText(inputFileName));

                                var jsonHash = jsonArray.FirstOrDefault(obj => obj.attributeMappingHash == hashKey);
                                //Retrieves the json segment in the file for the given hash returns value or NULL

                                if (jsonHash.attributeMappingHash == "")
                                {
                                    richTextBoxInformation.Text +=
                                        "The correct segment in the Json file was not found.\r\n";
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
                                string outputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                                File.WriteAllText(outputFileName, output);
                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += "There were issues applying the Json update.\r\n" + ex;
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

                            if (row[(int) AttributeMappingMetadataColumns.SourceTable] != DBNull.Value)
                            {
                                sourceTable = (string) row[(int) AttributeMappingMetadataColumns.SourceTable];
                            }

                            if (row[(int) AttributeMappingMetadataColumns.SourceColumn] != DBNull.Value)
                            {
                                sourceColumn = (string) row[(int) AttributeMappingMetadataColumns.SourceColumn];
                            }

                            if (row[(int) AttributeMappingMetadataColumns.TargetTable] != DBNull.Value)
                            {
                                targetTable = (string) row[(int) AttributeMappingMetadataColumns.TargetTable];
                            }

                            if (row[(int) AttributeMappingMetadataColumns.TargetColumn] != DBNull.Value)
                            {
                                targetColumn = (string) row[(int) AttributeMappingMetadataColumns.TargetColumn];
                            }

                            if (row[(int) AttributeMappingMetadataColumns.Notes] != DBNull.Value)
                            {
                                notes = (string) row[(int) AttributeMappingMetadataColumns.Notes];
                            }

                            try
                            {
                                var jsonAttributeMappingFull = new JArray();

                                // Load the file, if existing information needs to be merged
                                string inputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                                AttributeMappingJson[] jsonArray =
                                    JsonConvert.DeserializeObject<AttributeMappingJson[]>(
                                        File.ReadAllText(inputFileName));

                                // Convert it into a JArray so segments can be added easily
                                if (jsonArray != null)
                                {
                                    jsonAttributeMappingFull = JArray.FromObject(jsonArray);
                                }

                                string[] inputHashValue = new string[]
                                    {versionId.ToString(), sourceTable, sourceColumn, targetTable, targetColumn, notes};
                                var hashKey = Utility.CreateMd5(inputHashValue, Utility.SandingElement);

                                JObject newJsonSegment = new JObject(
                                    new JProperty("attributeMappingHash", hashKey),
                                    new JProperty("versionId", versionId),
                                    new JProperty("sourceTable", sourceTable),
                                    new JProperty("sourceAttribute", sourceColumn),
                                    new JProperty("targetTable", targetTable),
                                    new JProperty("targetAttribute", targetColumn),
                                    new JProperty("notes", notes)
                                );

                                jsonAttributeMappingFull.Add(newJsonSegment);

                                string output =
                                    JsonConvert.SerializeObject(jsonAttributeMappingFull, Formatting.Indented);
                                string outputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                                File.WriteAllText(outputFileName, output);

                                //Making sure the hash key value is added to the data table as well
                                row[(int) AttributeMappingMetadataColumns.HashKey] = hashKey;

                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text +=
                                    "There were issues inserting the Json segment / record.\r\n" + ex;
                            }


                        }

                        #endregion

                        #region Deletes in Attribute Mapping

                        // Deletes
                        if ((row.RowState & DataRowState.Deleted) != 0)
                        {
                            var hashKey = row[AttributeMappingMetadataColumns.HashKey.ToString(),
                                DataRowVersion.Original].ToString();
                            var versionKey = row[AttributeMappingMetadataColumns.VersionId.ToString(),
                                DataRowVersion.Original].ToString();

                            try
                            {
                                string inputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                                var jsonArray =
                                    JsonConvert.DeserializeObject<AttributeMappingJson[]>(
                                        File.ReadAllText(inputFileName)).ToList();

                                //Retrieves the json segment in the file for the given hash returns value or NULL
                                var jsonSegment =
                                    jsonArray.FirstOrDefault(obj => obj.attributeMappingHash == hashKey);

                                jsonArray.Remove(jsonSegment);

                                if (jsonSegment.attributeMappingHash == "")
                                {
                                    richTextBoxInformation.Text +=
                                        "The correct segment in the Json file was not found.\r\n";
                                }
                                else
                                {
                                    //Remove the segment from the JSON
                                    jsonArray.Remove(jsonSegment);
                                }

                                string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                                string outputFileName =
                                    TeamJsonHandling.JsonFileConfiguration.AttributeMappingJsonFileName();
                                File.WriteAllText(outputFileName, output);

                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += "There were issues applying the Json update.\r\n" + ex;
                            }

                        }

                        #endregion
                    }

                    #region Statement execution

                    //Committing the changes to the data table
                    dataTableChanges.AcceptChanges();
                    ((DataTable) _bindingSourceAttributeMetadata.DataSource).AcceptChanges();

                    richTextBoxInformation.AppendText($"The Attribute Mapping metadata has been saved.\r\n");

                    #endregion
                }
            }

        }


        private void CreateTemporaryWorkerTables(string connString)
        {
            var inputTableMapping = (DataTable) _bindingSourceTableMetadata.DataSource;
            var inputAttributeMapping = (DataTable) _bindingSourceAttributeMetadata.DataSource;

            #region Attribute Mapping

            // Attribute mapping
            var createStatement = new StringBuilder();
            createStatement.AppendLine();
            createStatement.AppendLine("-- Attribute mapping");
            createStatement.AppendLine("IF OBJECT_ID('[TMP_MD_ATTRIBUTE_MAPPING]', 'U') IS NOT NULL");
            createStatement.AppendLine(" DROP TABLE [TMP_MD_ATTRIBUTE_MAPPING]");
            createStatement.AppendLine("");
            createStatement.AppendLine("CREATE TABLE [TMP_MD_ATTRIBUTE_MAPPING]");
            createStatement.AppendLine("( ");
            createStatement.AppendLine("    [ATTRIBUTE_MAPPING_HASH] AS(");
            createStatement.AppendLine("                CONVERT([CHAR](32),HASHBYTES('MD5',");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[TARGET_TABLE])),'NA')+'|'+");
            createStatement.AppendLine(
                "                ISNULL(RTRIM(CONVERT(VARCHAR(100),[TARGET_COLUMN])),'NA')+'|'+");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[SOURCE_TABLE])),'NA')+'|'+");
            createStatement.AppendLine(
                "                ISNULL(RTRIM(CONVERT(VARCHAR(100),[SOURCE_COLUMN])),'NA')+'|' +");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[NOTES])),'NA')+'|'");
            createStatement.AppendLine("			),(2)");
            createStatement.AppendLine("		)");
            createStatement.AppendLine("	) PERSISTED NOT NULL,");
            createStatement.AppendLine("	[VERSION_ID]          integer NOT NULL,");
            createStatement.AppendLine("	[SOURCE_TABLE]        varchar(100)  NULL,");
            createStatement.AppendLine("	[SOURCE_TABLE_SCHEMA] varchar(100)  NULL,");
            createStatement.AppendLine("	[SOURCE_TABLE_TYPE]   varchar(100)  NULL,");
            createStatement.AppendLine("	[SOURCE_COLUMN]       varchar(100)  NULL,");
            createStatement.AppendLine("	[TARGET_TABLE]        varchar(100)  NULL,");
            createStatement.AppendLine("	[TARGET_TABLE_SCHEMA] varchar(100)  NULL,");
            createStatement.AppendLine("	[TARGET_TABLE_TYPE]   varchar(100)  NULL,");
            createStatement.AppendLine("	[TARGET_COLUMN]       varchar(100)  NULL,");
            createStatement.AppendLine("	[NOTES] varchar(4000)  NULL,");
            createStatement.AppendLine(
                "   CONSTRAINT [PK_TMP_MD_ATTRIBUTE_MAPPING] PRIMARY KEY CLUSTERED ([ATTRIBUTE_MAPPING_HASH] ASC, [VERSION_ID] ASC)");
            createStatement.AppendLine(")");

            ExecuteSqlCommand(createStatement, connString);
            createStatement.Clear();

            foreach (DataRow row in inputAttributeMapping.Rows)
            {
                // Evaluate normal elements of Data Item Mapping grid
                string sourceTable = "";
                string sourceColumn = "";
                string targetTable = "";
                string targetColumn = "";
                string mappingNotes = "";

                if (row[AttributeMappingMetadataColumns.SourceTable.ToString()] != DBNull.Value)
                    sourceTable = (string) row[AttributeMappingMetadataColumns.SourceTable.ToString()];

                if (row[AttributeMappingMetadataColumns.SourceColumn.ToString()] != DBNull.Value)
                    sourceColumn = (string) row[AttributeMappingMetadataColumns.SourceColumn.ToString()];

                if (row[AttributeMappingMetadataColumns.TargetTable.ToString()] != DBNull.Value)
                    targetTable = (string) row[AttributeMappingMetadataColumns.TargetTable.ToString()];

                if (row[AttributeMappingMetadataColumns.TargetColumn.ToString()] != DBNull.Value)
                    targetColumn = (string) row[AttributeMappingMetadataColumns.TargetColumn.ToString()];

                if (row[AttributeMappingMetadataColumns.Notes.ToString()] != DBNull.Value)
                    mappingNotes = (string) row[AttributeMappingMetadataColumns.Notes.ToString()];


                // Get the internal Ids for the source and target objects.
                var connectionInternalIdTuple =
                    GetDataObjectMappingFromDataItemMapping(inputTableMapping, sourceTable, targetTable);

                var fullyQualifiedSourceName = MetadataHandling
                    .GetFullyQualifiedDataObjectName(sourceTable, connectionInternalIdTuple.Item3).FirstOrDefault();
                var sourceType = MetadataHandling.GetDataObjectType(sourceTable, "", TeamConfiguration);

                var fullyQualifiedTargetName = MetadataHandling
                    .GetFullyQualifiedDataObjectName(targetTable, connectionInternalIdTuple.Item5).FirstOrDefault();
                var targetType = MetadataHandling.GetDataObjectType(targetTable, "", TeamConfiguration);

                createStatement.AppendLine("INSERT[dbo].[TMP_MD_ATTRIBUTE_MAPPING] (" +
                                           "[VERSION_ID], " +
                                           "[SOURCE_TABLE], " +
                                           "[SOURCE_TABLE_SCHEMA], " +
                                           "[SOURCE_TABLE_TYPE], " +
                                           "[SOURCE_COLUMN], " +
                                           "[TARGET_TABLE], " +
                                           "[TARGET_TABLE_SCHEMA], " +
                                           "[TARGET_TABLE_TYPE], " +
                                           "[TARGET_COLUMN], " +
                                           "[NOTES]" +
                                           ") " +
                                           "VALUES (" +
                                           "0, " +
                                           "N'" + fullyQualifiedSourceName.Value + "', " +
                                           "N'" + fullyQualifiedSourceName.Key + "', " +
                                           "'" + sourceType + "' ," +
                                           "N'" + sourceColumn + "', " +
                                           "N'" + fullyQualifiedTargetName.Value + "', " +
                                           "N'" + fullyQualifiedTargetName.Key + "', " +
                                           "'" + targetType + "' , " +
                                           "N'" + targetColumn + "', " +
                                           "N'" + mappingNotes + "'" +
                                           ");");
            }

            ExecuteSqlCommand(createStatement, connString);
            createStatement.Clear();

            #endregion


            // Table Mapping
            createStatement.AppendLine();
            createStatement.AppendLine("-- Table Mapping");
            createStatement.AppendLine("IF OBJECT_ID('[TMP_MD_TABLE_MAPPING]', 'U') IS NOT NULL");
            createStatement.AppendLine(" DROP TABLE[TMP_MD_TABLE_MAPPING]");
            createStatement.AppendLine("");
            createStatement.AppendLine("CREATE TABLE[TMP_MD_TABLE_MAPPING]");
            createStatement.AppendLine("( ");
            createStatement.AppendLine("    [TABLE_MAPPING_HASH] AS(");
            createStatement.AppendLine("                CONVERT([CHAR](32),HASHBYTES('MD5',");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[TARGET_TABLE])),'NA')+'|'+");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[SOURCE_TABLE])),'NA')+'|'+");
            createStatement.AppendLine(
                "                ISNULL(RTRIM(CONVERT(VARCHAR(100),[BUSINESS_KEY_ATTRIBUTE])),'NA')+'|'+");
            createStatement.AppendLine(
                "                ISNULL(RTRIM(CONVERT(VARCHAR(100),[DRIVING_KEY_ATTRIBUTE])),'NA')+'|'+");
            createStatement.AppendLine(
                "                ISNULL(RTRIM(CONVERT(VARCHAR(100),[FILTER_CRITERIA])),'NA')+'|'");
            createStatement.AppendLine("			),(2)");
            createStatement.AppendLine("			)");
            createStatement.AppendLine("		) PERSISTED NOT NULL ,");
            createStatement.AppendLine("	[VERSION_ID] integer NOT NULL ,");
            createStatement.AppendLine("	[SOURCE_TABLE] varchar(100)  NULL,");
            createStatement.AppendLine("	[SOURCE_TABLE_SCHEMA] varchar(100)  NULL,");
            createStatement.AppendLine("	[SOURCE_TABLE_TYPE] varchar(100)  NULL,");
            createStatement.AppendLine("	[BUSINESS_KEY_ATTRIBUTE] varchar(4000)  NULL,");
            createStatement.AppendLine("	[DRIVING_KEY_ATTRIBUTE] varchar(4000)  NULL,");
            createStatement.AppendLine("	[TARGET_TABLE] varchar(100)  NULL,");
            createStatement.AppendLine("	[TARGET_TABLE_SCHEMA] varchar(100)  NULL,");
            createStatement.AppendLine("	[TARGET_TABLE_TYPE] varchar(100)  NULL,");
            createStatement.AppendLine("	[FILTER_CRITERIA] varchar(4000)  NULL,");
            createStatement.AppendLine("	[ENABLED_INDICATOR] varchar(5)  NULL,");
            createStatement.AppendLine(
                "    CONSTRAINT [PK_TMP_MD_TABLE_MAPPING] PRIMARY KEY CLUSTERED([TABLE_MAPPING_HASH] ASC, [VERSION_ID] ASC)");
            createStatement.AppendLine(")");

            ExecuteSqlCommand(createStatement, connString);
            createStatement.Clear();

            foreach (DataRow row in inputTableMapping.Rows)
            {
                string sourceTable = "";
                string BUSINESS_KEY_ATTRIBUTE = "";
                string targetTable = "";
                string FILTER_CRITERIA = "";
                string DRIVING_KEY_ATTRIBUTE = "";
                string ENABLED_INDICATOR = "";

                if (row[TableMappingMetadataColumns.SourceTable.ToString()] != DBNull.Value)
                    sourceTable = (string) row[TableMappingMetadataColumns.SourceTable.ToString()];

                if (row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()] != DBNull.Value)
                    BUSINESS_KEY_ATTRIBUTE = (string) row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()];

                if (row[TableMappingMetadataColumns.TargetTable.ToString()] != DBNull.Value)
                    targetTable = (string) row[TableMappingMetadataColumns.TargetTable.ToString()];

                if (row[TableMappingMetadataColumns.FilterCriterion.ToString()] != DBNull.Value)
                {
                    FILTER_CRITERIA = (string) row[TableMappingMetadataColumns.FilterCriterion.ToString()];
                    FILTER_CRITERIA = FILTER_CRITERIA.Replace("'", "''");
                }

                if (row[TableMappingMetadataColumns.DrivingKeyDefinition.ToString()] != DBNull.Value)
                    DRIVING_KEY_ATTRIBUTE = (string) row[TableMappingMetadataColumns.DrivingKeyDefinition.ToString()];

                if (row[TableMappingMetadataColumns.Enabled.ToString()] != DBNull.Value)
                    ENABLED_INDICATOR = (string) row[TableMappingMetadataColumns.Enabled.ToString()].ToString();


                string localInternalSourceConnectionId =
                    row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                var sourceConnection = GetTeamConnectionByConnectionId(localInternalSourceConnectionId);
                var fullyQualifiedSourceName = MetadataHandling
                    .GetFullyQualifiedDataObjectName(sourceTable, sourceConnection).FirstOrDefault();
                var sourceType = MetadataHandling.GetDataObjectType(sourceTable, "", TeamConfiguration);

                string localInternalTargetConnectionId =
                    row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                var targetConnection = GetTeamConnectionByConnectionId(localInternalTargetConnectionId);
                var fullyQualifiedTargetName = MetadataHandling
                    .GetFullyQualifiedDataObjectName(targetTable, targetConnection).FirstOrDefault();
                var targetType = MetadataHandling.GetDataObjectType(targetTable, "", TeamConfiguration);

                createStatement.AppendLine("INSERT [dbo].[TMP_MD_TABLE_MAPPING] (" +
                                           "[VERSION_ID], " +
                                           "[SOURCE_TABLE], " +
                                           "[SOURCE_TABLE_SCHEMA], " +
                                           "[SOURCE_TABLE_TYPE], " +
                                           "[BUSINESS_KEY_ATTRIBUTE], " +
                                           "[TARGET_TABLE], " +
                                           "[TARGET_TABLE_SCHEMA], " +
                                           "[TARGET_TABLE_TYPE], " +
                                           "[FILTER_CRITERIA], " +
                                           "[DRIVING_KEY_ATTRIBUTE], " +
                                           "[ENABLED_INDICATOR]" +
                                           ") " +
                                           "VALUES (" +
                                           "0, " +
                                           "N'" + fullyQualifiedSourceName.Value + "', " +
                                           "N'" + fullyQualifiedSourceName.Key + "', " +
                                           "'" + sourceType + "' , " +
                                           "N'" + BUSINESS_KEY_ATTRIBUTE.Replace("'", "''") + "', " +
                                           "N'" + fullyQualifiedTargetName.Value + "', " +
                                           "N'" + fullyQualifiedTargetName.Key + "', " +
                                           "'" + targetType + "' , " +
                                           "N'" + FILTER_CRITERIA + "', " +
                                           "'" + DRIVING_KEY_ATTRIBUTE + "', " +
                                           "'" + ENABLED_INDICATOR + "'" +
                                           ");");
            }

            try
            {
                ExecuteSqlCommand(createStatement, connString);
            }
            catch (Exception ex)
            {
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning,
                    $"A row could not be inserted into the temporary worker table TMP_MD_TABLE_MAPPING. The message is {ex} for the statement {createStatement}."));
            }

            createStatement.Clear();


            if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode)
            {
                var inputPhysicalModel = (DataTable) _bindingSourcePhysicalModelMetadata.DataSource;
                // Physical Model
                createStatement.AppendLine();
                createStatement.AppendLine("-- Version Attribute");
                createStatement.AppendLine("IF OBJECT_ID('[TMP_MD_VERSION_ATTRIBUTE]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE[TMP_MD_VERSION_ATTRIBUTE]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE[TMP_MD_VERSION_ATTRIBUTE]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("");
                createStatement.AppendLine("    [VERSION_ATTRIBUTE_HASH] AS(");
                createStatement.AppendLine("                CONVERT([CHAR](32),HASHBYTES('MD5',");
                createStatement.AppendLine(
                    "                ISNULL(RTRIM(CONVERT(VARCHAR(100),[DATABASE_NAME])),'NA')+'|'+");
                createStatement.AppendLine(
                    "                ISNULL(RTRIM(CONVERT(VARCHAR(100),[SCHEMA_NAME])),'NA')+'|'+");
                createStatement.AppendLine(
                    "                ISNULL(RTRIM(CONVERT(VARCHAR(100),[TABLE_NAME])),'NA')+'|'+");
                createStatement.AppendLine(
                    "                ISNULL(RTRIM(CONVERT(VARCHAR(100),[COLUMN_NAME])),'NA')+'|'+");
                createStatement.AppendLine(
                    "                ISNULL(RTRIM(CONVERT(VARCHAR(100),[VERSION_ID])),'NA')+'|'");
                createStatement.AppendLine("			),(2)");
                createStatement.AppendLine("			)");
                createStatement.AppendLine("		) PERSISTED NOT NULL ,");
                createStatement.AppendLine("	[VERSION_ID] integer NOT NULL ,");
                createStatement.AppendLine("	[DATABASE_NAME]      varchar(100)  NOT NULL ,");
                createStatement.AppendLine("	[SCHEMA_NAME]        varchar(100)  NOT NULL ,");
                createStatement.AppendLine("	[TABLE_NAME]         varchar(100)  NOT NULL ,");
                createStatement.AppendLine("	[COLUMN_NAME]        varchar(100)  NOT NULL,");
                createStatement.AppendLine("    [DATA_TYPE]          varchar(100)  NOT NULL ,");
                createStatement.AppendLine("	[CHARACTER_MAXIMUM_LENGTH] integer NULL,");
                createStatement.AppendLine("    [NUMERIC_PRECISION]  integer NULL,");
                createStatement.AppendLine("    [NUMERIC_SCALE]  integer NULL,");
                createStatement.AppendLine("    [ORDINAL_POSITION]   integer NULL,");
                createStatement.AppendLine("    [PRIMARY_KEY_INDICATOR] varchar(1)  NULL ,");
                createStatement.AppendLine("	[MULTI_ACTIVE_INDICATOR] varchar(1)  NULL ");
                createStatement.AppendLine(")");
                createStatement.AppendLine("");
                createStatement.AppendLine("ALTER TABLE [TMP_MD_VERSION_ATTRIBUTE]");
                createStatement.AppendLine(
                    "    ADD CONSTRAINT[PK_TMP_MD_VERSION_ATTRIBUTE] PRIMARY KEY CLUSTERED([DATABASE_NAME] ASC, [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [VERSION_ID] ASC)");
                createStatement.AppendLine();

                ExecuteSqlCommand(createStatement, connString);
                createStatement.Clear();

                // Load the data table into the worker table for the physical model 
                foreach (DataRow row in inputPhysicalModel.Rows)
                {
                    string databaseName = "";
                    string schemaName = "";
                    string tableName = "";
                    string columnName = "";
                    string numericScale = "";
                    string numericPrecision = "";
                    string ordinalPosition = "";
                    string characterLength = "";
                    string dataType = "";
                    string primaryKeyIndicator = "";
                    string multiActiveIndicator = "";

                    if (row[PhysicalModelMappingMetadataColumns.DatabaseName.ToString()] != DBNull.Value)
                        databaseName = (string) row[PhysicalModelMappingMetadataColumns.DatabaseName.ToString()];

                    if (row[PhysicalModelMappingMetadataColumns.SchemaName.ToString()] != DBNull.Value)
                        schemaName = (string) row[PhysicalModelMappingMetadataColumns.SchemaName.ToString()];

                    if (row[PhysicalModelMappingMetadataColumns.TableName.ToString()] != DBNull.Value)
                        tableName = (string) row[PhysicalModelMappingMetadataColumns.TableName.ToString()];

                    if (row[PhysicalModelMappingMetadataColumns.ColumnName.ToString()] != DBNull.Value)
                        columnName = (string) row[PhysicalModelMappingMetadataColumns.ColumnName.ToString()];

                    if (row[PhysicalModelMappingMetadataColumns.NumericPrecision.ToString()] != DBNull.Value)
                        numericPrecision =
                            (string) row[PhysicalModelMappingMetadataColumns.NumericPrecision.ToString()];

                    if (row[PhysicalModelMappingMetadataColumns.NumericScale.ToString()] != DBNull.Value)
                        numericScale = (string) row[PhysicalModelMappingMetadataColumns.NumericScale.ToString()];

                    if (row[PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString()] != DBNull.Value)
                        ordinalPosition = (string) row[PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString()];

                    if (row[PhysicalModelMappingMetadataColumns.DataType.ToString()] != DBNull.Value)
                        dataType = (string) row[PhysicalModelMappingMetadataColumns.DataType.ToString()];

                    if (row[PhysicalModelMappingMetadataColumns.CharacterLength.ToString()] != DBNull.Value)
                        characterLength = (string) row[PhysicalModelMappingMetadataColumns.CharacterLength.ToString()];

                    if (row[PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator.ToString()] != DBNull.Value)
                        primaryKeyIndicator =
                            (string) row[PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator.ToString()];

                    if (row[PhysicalModelMappingMetadataColumns.MultiActiveIndicator.ToString()] != DBNull.Value)
                        multiActiveIndicator =
                            (string) row[PhysicalModelMappingMetadataColumns.MultiActiveIndicator.ToString()];


                    createStatement.AppendLine("INSERT [dbo].[TMP_MD_VERSION_ATTRIBUTE]" +
                                               " ([VERSION_ID], " +
                                               "[DATABASE_NAME], " +
                                               "[SCHEMA_NAME], " +
                                               "[TABLE_NAME], " +
                                               "[COLUMN_NAME], " +
                                               "[DATA_TYPE], " +
                                               "[CHARACTER_MAXIMUM_LENGTH], " +
                                               "[NUMERIC_PRECISION], " +
                                               "[NUMERIC_SCALE], " +
                                               "[ORDINAL_POSITION], " +
                                               "[PRIMARY_KEY_INDICATOR], " +
                                               "[MULTI_ACTIVE_INDICATOR]) " +
                                               "VALUES(" +
                                               "0, " +
                                               "N'" + databaseName + "', " +
                                               "N'" + schemaName + "', " +
                                               "N'" + tableName + "', " +
                                               "N'" + columnName + "', " +
                                               "N'" + dataType + "', " +
                                               "N'" + characterLength + "', " +
                                               "N'" + numericPrecision + "', " +
                                               "N'" + numericScale + "', " +
                                               "N'" + ordinalPosition + "', " +
                                               "N'" + primaryKeyIndicator + "', " +
                                               "N'" + multiActiveIndicator + "'" +
                                               ");");
                }

                ExecuteSqlCommand(createStatement, connString);
                createStatement.Clear();
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
        private static Tuple<bool, string, TeamConnection, string, TeamConnection>
            GetDataObjectMappingFromDataItemMapping(DataTable tableMappingDataTable, string sourceTable,
                string targetTable)
        {
            // Default return value
            Tuple<bool, string, TeamConnection, string, TeamConnection> returnTuple =
                new Tuple<bool, string, TeamConnection, string, TeamConnection>
                (
                    false,
                    sourceTable,
                    null,
                    targetTable,
                    null
                );

            // Find the corresponding row in the Data Object Mapping grid
            DataRow[] DataObjectMappings = tableMappingDataTable.Select("[" + TableMappingMetadataColumns.SourceTable +
                                                                        "] = '" + sourceTable + "' AND" +
                                                                        "[" + TableMappingMetadataColumns.TargetTable +
                                                                        "] = '" + targetTable + "'");

            if (DataObjectMappings is null || DataObjectMappings.Length == 0)
            {
                // There is no matching row found in the Data Object Mapping grid. Validation should pick this up!
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    $"While processing the Data Item mappings, no matching Data Object mapping was found."));

            }
            else if (DataObjectMappings.Length > 1)
            {
                // There are too many entries! There should be only a single mapping from source to target
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    $"While processing the Data Item mappings, to many (more than 1) matching Data Object mapping were found."));
            }
            else
            {
                var connectionInternalIdSource =
                    DataObjectMappings[0][TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                var connectionInternalIdTarget =
                    DataObjectMappings[0][TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                TeamConnection sourceConnection = GetTeamConnectionByConnectionId(connectionInternalIdSource);
                TeamConnection targetConnection = GetTeamConnectionByConnectionId(connectionInternalIdTarget);

                // Set the right values
                returnTuple = new Tuple<bool, string, TeamConnection, string, TeamConnection>
                (
                    (bool) DataObjectMappings[0][TableMappingMetadataColumns.Enabled.ToString()],
                    sourceTable,
                    sourceConnection,
                    targetTable,
                    targetConnection
                );

            }



            return returnTuple;
        }

        private void ExecuteSqlCommand(StringBuilder inputString, string connString)
        {
            using (var connectionVersion = new SqlConnection(connString))
            {
                var commandVersion = new SqlCommand(inputString.ToString(), connectionVersion);

                try
                {
                    connectionVersion.Open();
                    commandVersion.ExecuteNonQuery();

                }
                catch (Exception)
                {
                    // IGNORE FOR NOW
                }
            }
        }

        private void DropTemporaryWorkerTable(string connString)
        {
            // Attribute mapping
            var createStatement = new StringBuilder();
            createStatement.AppendLine("-- Attribute mapping");
            createStatement.AppendLine("IF OBJECT_ID('[TMP_MD_ATTRIBUTE_MAPPING]', 'U') IS NOT NULL");
            createStatement.AppendLine(" DROP TABLE [TMP_MD_ATTRIBUTE_MAPPING]");

            ExecuteSqlCommand(createStatement, connString);
            createStatement.Clear();

            // Table Mapping
            createStatement.AppendLine("-- Table Mapping");
            createStatement.AppendLine("IF OBJECT_ID('[TMP_MD_TABLE_MAPPING]', 'U') IS NOT NULL");
            createStatement.AppendLine(" DROP TABLE[TMP_MD_TABLE_MAPPING]");

            ExecuteSqlCommand(createStatement, connString);
            createStatement.Clear();

            // Physical Model
            createStatement.AppendLine("-- Version Attribute");
            createStatement.AppendLine("IF OBJECT_ID('[TMP_MD_VERSION_ATTRIBUTE]', 'U') IS NOT NULL");
            createStatement.AppendLine(" DROP TABLE [TMP_MD_VERSION_ATTRIBUTE]");

            ExecuteSqlCommand(createStatement, connString);
            createStatement.Clear();
        }

        # region Background worker

        private void ButtonActivate_Click(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();

            // Local boolean to manage whether activation is OK to go ahead.
            bool activationContinue = true;

            // Check if there are any outstanding saves / commits in the data grid
            var dataTableTableMappingChanges = ((DataTable) _bindingSourceTableMetadata.DataSource).GetChanges();
            var dataTableAttributeMappingChanges =
                ((DataTable) _bindingSourceAttributeMetadata.DataSource).GetChanges();
            DataTable dataTablePhysicalModelChanges = new DataTable();

            if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode)
            {
                dataTablePhysicalModelChanges =
                    ((DataTable) _bindingSourcePhysicalModelMetadata.DataSource).GetChanges();
            }

            if (
                (dataTableTableMappingChanges != null && dataTableTableMappingChanges.Rows.Count > 0) ||
                (dataTableAttributeMappingChanges != null && dataTableAttributeMappingChanges.Rows.Count > 0) ||
                (dataTablePhysicalModelChanges != null && dataTablePhysicalModelChanges.Rows.Count > 0)
            )
            {
                string localMessage = "You have unsaved edits, please save your work before running the activation.";
                MessageBox.Show(localMessage);
                richTextBoxInformation.AppendText(localMessage);
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, localMessage));
                activationContinue = false;
            }

            #region Validation

            // The first thing to happen is to check if the validation needs to be run (and started if the answer to this is yes)
            if (checkBoxValidation.Checked && activationContinue)
            {
                if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode &&
                    _bindingSourcePhysicalModelMetadata.Count == 0)
                {
                    richTextBoxInformation.Text +=
                        "There is no model metadata available, so the metadata can only be validated with the 'Ignore Version' enabled.\r\n ";
                }
                else
                {
                    if (backgroundWorkerValidationOnly.IsBusy) return;
                    // create a new instance of the alert form
                    _alertValidation = new Form_Alert();
                    _alertValidation.SetFormName("Validating the metadata");
                    _alertValidation.ShowLogButton(false);
                    _alertValidation.ShowCancelButton(false);
                    // event handler for the Cancel button in AlertForm
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

            if (!checkBoxValidation.Checked ||
                (checkBoxValidation.Checked && MetadataParameters.ValidationIssues == 0) && activationContinue)
            {
                // Commence the activation
                var conn = new SqlConnection
                {
                    ConnectionString =
                        TeamConfiguration.MetadataConnection.CreateSqlServerConnectionString(false)
                };

                richTextBoxInformation.Clear();

                // var versionMajorMinor = GetVersion(trackBarVersioning.Value, conn);
                var versionMajorMinor =
                    TeamVersionList.GetMajorMinorForVersionId(GlobalParameters.WorkingEnvironment,
                        trackBarVersioning.Value);

                var majorVersion = versionMajorMinor.Item2;
                var minorVersion = versionMajorMinor.Item3;
                richTextBoxInformation.AppendText("Commencing preparation / activation for version " + majorVersion +
                                                  "." + minorVersion + ".\r\n");

                // Move data from the grids into temp tables
                CreateTemporaryWorkerTables(
                    TeamConfiguration.MetadataConnection.CreateSqlServerConnectionString(false));

                if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode)
                {
                    var versionExistenceCheck = new StringBuilder();

                    versionExistenceCheck.AppendLine("SELECT * FROM TMP_MD_VERSION_ATTRIBUTE WHERE VERSION_ID = " +
                                                     trackBarVersioning.Value);

                    var versionExistenceCheckDataTable =
                        Utility.GetDataTable(ref conn, versionExistenceCheck.ToString());

                    if (versionExistenceCheckDataTable != null && versionExistenceCheckDataTable.Rows.Count > 0)
                    {
                        if (backgroundWorkerMetadata.IsBusy) return;
                        // create a new instance of the alert form
                        _alert = new Form_Alert();
                        // event handler for the Cancel button in AlertForm
                        _alert.Canceled += buttonCancel_Click;
                        _alert.ShowLogButton(false);
                        _alert.ShowCancelButton(false);
                        _alert.Show();
                        // Start the asynchronous operation.
                        _bindingSourcePhysicalModelMetadata.SuspendBinding();
                        backgroundWorkerMetadata.RunWorkerAsync();
                        _bindingSourcePhysicalModelMetadata.ResumeBinding();
                    }
                    else
                    {
                        richTextBoxInformation.AppendText(
                            "There is no model metadata available for this version, so the metadata can only be activated with the 'Ignore Version' enabled for this specific version.\r\n");
                    }
                }
                else
                {
                    if (backgroundWorkerMetadata.IsBusy) return;
                    // create a new instance of the alert form
                    _alert = new Form_Alert();
                    // event handler for the Cancel button in AlertForm
                    _alert.Canceled += buttonCancel_Click;
                    _alert.ShowLogButton(false);
                    _alert.ShowCancelButton(false);
                    _alert.Show();
                    // Start the asynchronous operation.
                    backgroundWorkerMetadata.RunWorkerAsync();
                }
            }
            else
            {
                richTextBoxInformation.AppendText(
                    "Validation found issues which should be investigated. If you would like to continue, please uncheck the validation and activate the metadata again.\r\n");
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

        /// <summary>
        /// Multi-threading for informing the user when version changes (to other forms).
        /// </summary>
        /// <returns></returns>
        delegate int GetVersionFromTrackBarCallBack();

        private int GetVersionFromTrackBar()
        {
            if (trackBarVersioning.InvokeRequired)
            {
                var d = new GetVersionFromTrackBarCallBack(GetVersionFromTrackBar);
                return Int32.Parse(Invoke(d).ToString());
            }
            else
            {
                return trackBarVersioning.Value;
            }
        }

        // This event handler deals with the results of the background operation.
        private void backgroundWorkerMetadata_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                richTextBoxInformation.Text += "The metadata was processed successfully!\r\n";

                #region Save the JSON interface files

                // Saving the interfaces to Json
                if (checkBoxSaveInterfaceToJson.Checked)
                {
                    // Take all the rows from the grid
                    var localDataTable = (DataTable) _bindingSourceTableMetadata.DataSource;
                    List<DataRow> rowList = new List<DataRow>();
                    foreach (DataRow row in localDataTable.Rows)
                    {
                        if ((bool) row[TableMappingMetadataColumns.Enabled.ToString()])
                        {
                            rowList.Add(row); //add the row to the list
                        }
                    }

                    ManageFormOverallJsonExport();
                }

                #endregion
            }

            // Close the AlertForm
            //alert.Close();
        }

        // This event handler updates the progress.
        private void backgroundWorkerMetadata_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Show the progress in main form (GUI)
            labelResult.Text = (e.ProgressPercentage + "%");

            // Pass the progress to AlertForm label and progressbar
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
            #region Generic

            // Set the stopwatch to be able to report back on process duration.
            Stopwatch totalProcess = new Stopwatch();
            totalProcess.Start();
            Stopwatch subProcess = new Stopwatch();

            // Used to retrieve any error messages from the log related to this activation run.
            DateTime activationStartDateTime = DateTime.Now;

            BackgroundWorker worker = sender as BackgroundWorker;

            var inputTableMetadata = (DataTable) _bindingSourceTableMetadata.DataSource;
            var inputAttributeMetadata = (DataTable) _bindingSourceAttributeMetadata.DataSource;

            DataRow[] selectionRows;

            var connOmd = new SqlConnection
            {
                ConnectionString = TeamConfiguration.MetadataConnection.CreateSqlServerConnectionString(false)
            };
            var metaDataConnection =
                TeamConfiguration.MetadataConnection.CreateSqlServerConnectionString(false);


            var effectiveDateTimeAttribute =
                TeamConfiguration.EnableAlternativeSatelliteLoadDateTimeAttribute == "True"
                    ? TeamConfiguration.AlternativeSatelliteLoadDateTimeAttribute
                    : TeamConfiguration.LoadDateTimeAttribute;
            var currentRecordAttribute = TeamConfiguration.CurrentRowAttribute;
            var eventDateTimeAttribute = TeamConfiguration.EventDateTimeAttribute;
            var recordSource = TeamConfiguration.RecordSourceAttribute;
            var alternativeRecordSource = TeamConfiguration.AlternativeRecordSourceAttribute;
            var sourceRowId = TeamConfiguration.RowIdAttribute;
            var recordChecksum = TeamConfiguration.RecordChecksumAttribute;
            var changeDataCaptureIndicator = TeamConfiguration.ChangeDataCaptureAttribute;
            var hubAlternativeLdts = TeamConfiguration.AlternativeLoadDateTimeAttribute;
            var etlProcessId = TeamConfiguration.EtlProcessAttribute;
            var loadDateTimeStamp = TeamConfiguration.LoadDateTimeAttribute;

            var stagingPrefix = TeamConfiguration.StgTablePrefixValue;
            var psaPrefix = TeamConfiguration.PsaTablePrefixValue;
            var hubTablePrefix = TeamConfiguration.HubTablePrefixValue;
            var lnkTablePrefix = TeamConfiguration.LinkTablePrefixValue;
            var satTablePrefix = TeamConfiguration.SatTablePrefixValue;
            var lsatTablePrefix = TeamConfiguration.LsatTablePrefixValue;

            if (TeamConfiguration.TableNamingLocation == "Prefix")
            {
                stagingPrefix = stagingPrefix + "%";
                psaPrefix = psaPrefix + "%";
                hubTablePrefix = hubTablePrefix + "%";
                lnkTablePrefix = lnkTablePrefix + "%";
                satTablePrefix = satTablePrefix + "%";
                lsatTablePrefix = lsatTablePrefix + "%";
            }
            else
            {
                stagingPrefix = "%" + stagingPrefix;
                psaPrefix = "%" + psaPrefix;
                hubTablePrefix = "%" + hubTablePrefix;
                lnkTablePrefix = "%" + lnkTablePrefix;
                satTablePrefix = "%" + satTablePrefix;
                lsatTablePrefix = "%" + lsatTablePrefix;
            }

            var dwhKeyIdentifier = TeamConfiguration.DwhKeyIdentifier;

            if (TeamConfiguration.KeyNamingLocation == "Prefix")
            {
                dwhKeyIdentifier = dwhKeyIdentifier + '%';
            }
            else
            {
                dwhKeyIdentifier = '%' + dwhKeyIdentifier;
            }

            // Handling multi-threading
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                // Determine the version.
                var versionId = GetVersionFromTrackBar();

                var versionMajorMinor =
                    TeamVersionList.GetMajorMinorForVersionId(GlobalParameters.WorkingEnvironment, versionId);
                var majorVersion = versionMajorMinor.Item2;
                var minorVersion = versionMajorMinor.Item3;

                // Get the full dictionary of objects and connections.
                //var localTableMappingConnectionDictionary = GetTableMappingConnections();

                // Get the dictionary of target data objects and their enabled / disabled flag.
                var localTableEnabledDictionary = GetEnabledForDataObject();

                // Event reporting - informing the user that the activation process has started.
                LogMetadataEvent($"Commencing metadata preparation / activation for version {majorVersion}.{minorVersion} at {activationStartDateTime}.",
                    EventTypes.Information);

                // Event reporting - alerting the user what kind of metadata is prepared.
                LogMetadataEvent($"The {GlobalParameters.EnvironmentMode} has been selected for activation.",
                    EventTypes.Information);

                #endregion


                #region Delete Metadata - 2%

                // 1. Deleting metadata
                LogMetadataEvent($"Commencing removal of existing metadata.", EventTypes.Information);

                var deleteStatement = new StringBuilder();
                deleteStatement.AppendLine(@"
                                        DELETE FROM dbo.[MD_SOURCE_STAGING_XREF];
                                        DELETE FROM dbo.[MD_SOURCE_STAGING_ATTRIBUTE_XREF];
                                        DELETE FROM dbo.[MD_SOURCE_PERSISTENT_STAGING_XREF];
                                        DELETE FROM dbo.[MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF];
                                        DELETE FROM dbo.[MD_STAGING];
                                        DELETE FROM dbo.[MD_PERSISTENT_STAGING];
                                        DELETE FROM dbo.[MD_SOURCE_LINK_ATTRIBUTE_XREF];
                                        DELETE FROM dbo.[MD_SOURCE_SATELLITE_ATTRIBUTE_XREF];
                                        DELETE FROM dbo.[MD_SOURCE_LINK_XREF];
                                        DELETE FROM dbo.[MD_SOURCE_SATELLITE_XREF];
                                        DELETE FROM dbo.[MD_DRIVING_KEY_XREF];
                                        DELETE FROM dbo.[MD_HUB_LINK_XREF];
                                        DELETE FROM dbo.[MD_SATELLITE];
                                        DELETE FROM dbo.[MD_BUSINESS_KEY_COMPONENT_PART];
                                        DELETE FROM dbo.[MD_BUSINESS_KEY_COMPONENT];
                                        DELETE FROM dbo.[MD_SOURCE_HUB_XREF];
                                        DELETE FROM dbo.[MD_ATTRIBUTE];
                                        DELETE FROM dbo.[MD_SOURCE];
                                        DELETE FROM dbo.[MD_HUB];
                                        DELETE FROM dbo.[MD_LINK];
                                        DELETE FROM dbo.[MD_MODEL_METADATA];
                                        DELETE FROM dbo.[MD_PHYSICAL_MODEL];
                                        ");

                using (var connectionVersion = new SqlConnection(metaDataConnection))
                {
                    var commandVersion = new SqlCommand(deleteStatement.ToString(), connectionVersion);

                    try
                    {
                        connectionVersion.Open();
                        commandVersion.ExecuteNonQuery();

                        if (worker != null) worker.ReportProgress(2);
                        LogMetadataEvent($"Removal of existing metadata completed.", EventTypes.Information);
                    }
                    catch (Exception ex)
                    {
                        LogMetadataEvent(
                            $"An issue has occurred during removal of old metadata. The query that caused the issue is \r\n\r\n {deleteStatement}, and the message is {ex}.",
                            EventTypes.Error);
                    }
                }

                #endregion

                #region Deploy repository / workspace

                LogMetadataEvent($"Deploying repository / temporary workspace against metadata connection.",
                    EventTypes.Information);
                try
                {
                    using (StreamReader sr = new StreamReader(GlobalParameters.ScriptPath + "generateRepository.sql"))
                    {
                        var sqlCommands = sr.ReadToEnd().Split(new string[] {Environment.NewLine + Environment.NewLine},
                            StringSplitOptions.RemoveEmptyEntries);

                        foreach (var command in sqlCommands)
                        {
                            using (var connectionVersion = new SqlConnection(metaDataConnection))
                            {
                                var commandVersion = new SqlCommand(command, connectionVersion);

                                try
                                {
                                    connectionVersion.Open();
                                    commandVersion.ExecuteNonQuery();
                                    LogMetadataEvent($"Executing statement: \r\n\r\n {command}",
                                        EventTypes.Information);
                                }
                                catch (Exception ex)
                                {
                                    LogMetadataEvent(
                                        $"An issue has occurred during removal of old metadata. The query that caused the issue is \r\n\r\n {command} with exception {ex}.",
                                        EventTypes.Error);
                                }
                            }

                        }

                        worker.ReportProgress(2);
                    }
                }
                catch (Exception ex)
                {
                    LogMetadataEvent(
                        $"An issue has occurred executing the repository creation logic. The reported error was: \r\n\r\n {ex}",
                        EventTypes.Error);
                }

                LogMetadataEvent($"Preparation of the repository completed.", EventTypes.Information);

                #endregion


                #region Prepare Version Information

                // Prepare Version
                LogMetadataEvent($"Commencing preparing the version metadata.", EventTypes.Information);

                var versionName = string.Concat(majorVersion, '.', minorVersion);

                using (var connection = new SqlConnection(metaDataConnection))
                {
                    LogMetadataEvent("Working on committing version " + versionName + " to the metadata repository.",
                        EventTypes.Information);

                    var insertVersionStatement = new StringBuilder();
                    insertVersionStatement.AppendLine("INSERT INTO [MD_MODEL_METADATA]");
                    insertVersionStatement.AppendLine("([VERSION_NAME],[ACTIVATION_DATETIME])");
                    insertVersionStatement.AppendLine("VALUES ('" + versionName + "','" +
                                                      DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz") + "')");

                    var command = new SqlCommand(insertVersionStatement.ToString(), connection);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        LogMetadataEvent(
                            $"An issue has occurred during preparation of the version information: \r\n\r\n {ex}.",
                            EventTypes.Error);
                    }
                }

                if (worker != null) worker.ReportProgress(3);
                LogMetadataEvent($"Preparation of the version details completed.", EventTypes.Information);

                #endregion


                #region Physical Model dump - 5%

                // Creating a point-in-time snapshot of the physical model used for export to the interface schemas
                subProcess.Reset();
                subProcess.Start();
                LogMetadataEvent($"Creating a snapshot of the physical model.", EventTypes.Information);


                // First, define the master attribute list for reuse many times later on (assuming ignore version is active and hence the virtual mode is enabled).
                var physicalModelDataTable = new DataTable();

                if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode
                ) // Get the attributes from the physical model / catalog. No virtualisation needed.
                {
                    var physicalModelInstantiation = new AttributeSelection();

                    foreach (var connection in TeamConfiguration.ConnectionDictionary)
                    {
                        if (connection.Value.ConnectionKey != "Metadata")
                        {
                            var localConnectionObject = (TeamConnection) connection.Value;
                            var localSqlConnection = new SqlConnection
                                {ConnectionString = localConnectionObject.CreateSqlServerConnectionString(false)};

                            // Build up the filter criteria to only select information for tables that are associated with the connection
                            var tableFilterObjects = "";
                            foreach (DataRow row in inputTableMetadata.Rows)
                            {
                                var localInternalConnectionIdSource =
                                    row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                                var sourceConnection = GetTeamConnectionByConnectionId(localInternalConnectionIdSource);

                                var localInternalConnectionIdTarget =
                                    row[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                                var targetConnection = GetTeamConnectionByConnectionId(localInternalConnectionIdTarget);

                                if (localInternalConnectionIdSource == connection.Value.ConnectionInternalId)
                                {
                                    var localTable = row[TableMappingMetadataColumns.SourceTable.ToString()].ToString();

                                    // Schema and Table Name
                                    var localFullyQualifiedTableName = MetadataHandling
                                        .GetFullyQualifiedDataObjectName(localTable, sourceConnection).FirstOrDefault();

                                    tableFilterObjects = tableFilterObjects + "OBJECT_ID(N'[" +
                                                         connection.Value.DatabaseServer.DatabaseName + "]." +
                                                         localFullyQualifiedTableName.Key + "." +
                                                         localFullyQualifiedTableName.Value + "') ,";
                                }

                                if (row[TableMappingMetadataColumns.TargetConnection.ToString()].ToString() ==
                                    connection.Value.ConnectionInternalId)
                                {
                                    var localTable = row[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                                    var localInternalConnectionId =
                                        row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();

                                    // Schema and Table Name
                                    var localFullyQualifiedTableName = MetadataHandling
                                        .GetFullyQualifiedDataObjectName(localTable, targetConnection).FirstOrDefault();
                                    tableFilterObjects = tableFilterObjects + "OBJECT_ID(N'[" +
                                                         connection.Value.DatabaseServer.DatabaseName + "]." +
                                                         localFullyQualifiedTableName.Key + "." +
                                                         localFullyQualifiedTableName.Value + "') ,";
                                }

                            }

                            tableFilterObjects = tableFilterObjects.TrimEnd(',');


                            //var physicalModelStatement = new StringBuilder();
                            //physicalModelStatement.AppendLine("SELECT ");
                            //physicalModelStatement.AppendLine(" [DATABASE_NAME] ");
                            //physicalModelStatement.AppendLine(",[SCHEMA_NAME]");
                            //physicalModelStatement.AppendLine(",[TABLE_NAME]");
                            //physicalModelStatement.AppendLine(",[COLUMN_NAME]");
                            //physicalModelStatement.AppendLine(",[DATA_TYPE]");
                            //physicalModelStatement.AppendLine(",[CHARACTER_MAXIMUM_LENGTH]");
                            //physicalModelStatement.AppendLine(",[NUMERIC_PRECISION]");
                            //physicalModelStatement.AppendLine(",[NUMERIC_SCALE]");
                            //physicalModelStatement.AppendLine(",[ORDINAL_POSITION]");
                            //physicalModelStatement.AppendLine(",[PRIMARY_KEY_INDICATOR]");
                            //physicalModelStatement.AppendLine("FROM");
                            //physicalModelStatement.AppendLine("(");
                            //physicalModelStatement.AppendLine();
                            //physicalModelStatement.AppendLine(") sub");

                            var localPhysicalModelDataTable = Utility.GetDataTable(ref localSqlConnection,
                                physicalModelInstantiation
                                    .CreatePhysicalModelSet(localConnectionObject.DatabaseServer.DatabaseName,
                                        tableFilterObjects).ToString());

                            if (localPhysicalModelDataTable != null)
                            {
                                physicalModelDataTable.Merge(localPhysicalModelDataTable);
                            }
                        }
                    }
                }
                else // Get the values from the data grid or worker table (virtual mode)
                {
                    StringBuilder allVirtualDatabaseAttributes = new StringBuilder();

                    allVirtualDatabaseAttributes.AppendLine("SELECT ");
                    allVirtualDatabaseAttributes.AppendLine("  [DATABASE_NAME] ");
                    allVirtualDatabaseAttributes.AppendLine(" ,[SCHEMA_NAME]");
                    allVirtualDatabaseAttributes.AppendLine(" ,[TABLE_NAME]");
                    allVirtualDatabaseAttributes.AppendLine(" ,[COLUMN_NAME]");
                    allVirtualDatabaseAttributes.AppendLine(" ,[DATA_TYPE]");
                    allVirtualDatabaseAttributes.AppendLine(" ,[CHARACTER_MAXIMUM_LENGTH]");
                    allVirtualDatabaseAttributes.AppendLine(" ,[NUMERIC_PRECISION]");
                    allVirtualDatabaseAttributes.AppendLine(" ,[NUMERIC_SCALE]");
                    allVirtualDatabaseAttributes.AppendLine(" ,[ORDINAL_POSITION]");
                    allVirtualDatabaseAttributes.AppendLine(" ,[PRIMARY_KEY_INDICATOR]");
                    allVirtualDatabaseAttributes.AppendLine("FROM [TMP_MD_VERSION_ATTRIBUTE] mapping");

                    physicalModelDataTable = Utility.GetDataTable(ref connOmd, allVirtualDatabaseAttributes.ToString());
                }

                try
                {
                    if (physicalModelDataTable.Rows.Count == 0)
                    {
                        LogMetadataEvent("No model information was found in the metadata.", EventTypes.Warning);
                    }
                    else
                    {
                        // Create a large insert string to save per-row database connection.
                        var createStatement = new StringBuilder();

                        foreach (DataRow tableName in physicalModelDataTable.Rows)
                        {
                            var insertKeyStatement = new StringBuilder();

                            insertKeyStatement.AppendLine("INSERT INTO [MD_PHYSICAL_MODEL]");
                            insertKeyStatement.AppendLine("([DATABASE_NAME], " +
                                                          "[SCHEMA_NAME], " +
                                                          "[TABLE_NAME], " +
                                                          "[COLUMN_NAME], " +
                                                          "[DATA_TYPE], " +
                                                          "[CHARACTER_MAXIMUM_LENGTH], " +
                                                          "[NUMERIC_PRECISION], " +
                                                          "[NUMERIC_SCALE], " +
                                                          "[ORDINAL_POSITION], " +
                                                          "[PRIMARY_KEY_INDICATOR])");
                            insertKeyStatement.AppendLine("VALUES ('" +
                                                          tableName["DATABASE_NAME"].ToString().Trim() +
                                                          "','" + tableName["SCHEMA_NAME"].ToString().Trim() +
                                                          "','" + tableName["TABLE_NAME"].ToString().Trim() +
                                                          "','" + tableName["COLUMN_NAME"].ToString().Trim() +
                                                          "','" + tableName["DATA_TYPE"].ToString().Trim() +
                                                          "','" + tableName["CHARACTER_MAXIMUM_LENGTH"].ToString()
                                                              .Trim() +
                                                          "','" + tableName["NUMERIC_PRECISION"].ToString().Trim() +
                                                          "','" + tableName["NUMERIC_SCALE"].ToString().Trim() +
                                                          "','" + tableName["ORDINAL_POSITION"].ToString().Trim() +
                                                          "','" + tableName["PRIMARY_KEY_INDICATOR"].ToString().Trim() +
                                                          "')");

                            createStatement.AppendLine(insertKeyStatement.ToString());
                        }

                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            // Execute the statement
                            var command = new SqlCommand(createStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An issue has occurred during preparation of the physical model metadata: \r\n\r\n {ex}.",
                                    EventTypes.Error);
                            }
                        }
                    }

                    worker.ReportProgress(5);
                    subProcess.Stop();
                    LogMetadataEvent(
                        "Preparation of the physical model extract completed, and has taken " +
                        subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);
                }
                catch (Exception ex)
                {
                    LogMetadataEvent(
                        $"An issue has occurred during preparation of the physical model metadata: \r\n\r\n {ex}.",
                        EventTypes.Error);
                }

                #endregion


                # region Prepare Source

                // Prepare the generic sources
                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent($"Commencing preparing the source metadata.", EventTypes.Information);

                // Getting the distinct list of tables to go into the 'source'
                selectionRows = inputTableMetadata.Select(TableMappingMetadataColumns.Enabled + " = 'true'");

                var distinctSourceList = new List<Tuple<string, TeamConnection>>();
                distinctSourceList.Add(new Tuple<string, TeamConnection>("Not applicable", null));

                // Create a distinct list of sources from the data grid
                foreach (DataRow row in selectionRows)
                {
                    string sourceTable = row[TableMappingMetadataColumns.SourceTable.ToString()].ToString().Trim();
                    string sourceInternalConnectionId =
                        row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                    var sourceTableConnection = GetTeamConnectionByConnectionId(sourceInternalConnectionId);

                    var localTuple = new Tuple<string, TeamConnection>(sourceTable, sourceTableConnection);

                    if (!distinctSourceList.Contains(localTuple))
                    {
                        distinctSourceList.Add(localTuple);
                    }
                }

                // Add the list of sources to the MD_SOURCE table
                foreach (var sourceTuple in distinctSourceList)
                {
                    string localTableName = sourceTuple.Item1;
                    TeamConnection localTeamConnection = sourceTuple.Item2;

                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        if (localTableName != "Not applicable")
                        {
                            LogMetadataEvent($"Adding {localTableName}.", EventTypes.Information);
                        }

                        var fullyQualifiedName = MetadataHandling
                            .GetFullyQualifiedDataObjectName(localTableName, localTeamConnection).FirstOrDefault();

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_SOURCE]");
                        insertStatement.AppendLine("([SOURCE_NAME], [SOURCE_NAME_SHORT], [SCHEMA_NAME])");
                        insertStatement.AppendLine("VALUES ('" + fullyQualifiedName.Key + "." +
                                                   fullyQualifiedName.Value + "','" + fullyQualifiedName.Value + "','" +
                                                   fullyQualifiedName.Key + "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            LogMetadataEvent(
                                $"An issue has occurred during preparation of the source metadata: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                EventTypes.Error);
                        }
                    }
                }

                worker?.ReportProgress(5);
                subProcess.Stop();
                LogMetadataEvent(
                    $"Preparation of the source metadata completed, and has taken {subProcess.Elapsed.TotalSeconds} seconds.",
                    EventTypes.Information);

                #endregion


                #region Prepare Source and Target Data Objects

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent($"Commencing the preparation of the Data Object (source and target) metadata.",
                    EventTypes.Information);

                // Getting the distinct list of source- and target Data Objects, so that these can be evaluated for their types and loaded into separate metadata tables.
                // Also, creating a dummy row for each type to support referential integrity in the metadata model.
                // <name> , <type>, <connection>
                List<Tuple<string, MetadataHandling.TableTypes, TeamConnection>> dataObjectList =
                    new List<Tuple<string, MetadataHandling.TableTypes, TeamConnection>>();
                foreach (MetadataHandling.TableTypes tableType in Enum.GetValues(typeof(MetadataHandling.TableTypes)))
                {
                    dataObjectList.Add(
                        new Tuple<string, MetadataHandling.TableTypes, TeamConnection>("Not applicable", tableType,
                            null));
                }

                // Also capture the source/staging XREF relationships while we're evaluating STG table types.
                // source/target/business key/filter/type (e.g. stg, psa, core business concept etc.)
                List<Tuple<string, string, string, string, MetadataHandling.TableTypes>> sourceTargetXrefList =
                    new List<Tuple<string, string, string, string, MetadataHandling.TableTypes>>();

                // Iterate over enabled row to see if there are any sources and targets to add to the list.
                foreach (DataRow row in inputTableMetadata.Rows)
                {
                    if ((bool) row[TableMappingMetadataColumns.Enabled.ToString()])
                    {
                        // Need to evaluate the non qualified name for the table type, i.e. disregarding schemas etc.
                        // The Data Object either has a Staging prefix defined and starts with it, or an suffix and ends with it.

                        /* SOURCES */
                        string localTableSourceName =
                            row[TableMappingMetadataColumns.SourceTable.ToString()]
                                .ToString(); // The full table as visible in the data grid view.
                        string localTableSourceConnectionId =
                            row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                        TeamConnection localTableConnectionSource =
                            GetTeamConnectionByConnectionId(localTableSourceConnectionId);
                        var localFullyQualifiedKeyValuePairSource = MetadataHandling
                            .GetFullyQualifiedDataObjectName(localTableSourceName, localTableConnectionSource)
                            .FirstOrDefault();
                        string localTableSourceFullyQualifiedNameSource =
                            localFullyQualifiedKeyValuePairSource.Key + '.' +
                            localFullyQualifiedKeyValuePairSource.Value;

                        /* TARGETS and Xref  */
                        var localBusinessKey = row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()]
                            .ToString();
                        var localFilterCriterion =
                            row[TableMappingMetadataColumns.FilterCriterion.ToString()].ToString();

                        string localTableTargetName =
                            row[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                        string localTableTargetConnectionId =
                            row[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                        TeamConnection localTableConnectionTarget =
                            GetTeamConnectionByConnectionId(localTableTargetConnectionId);
                        var localFullyQualifiedKeyValuePairTarget = MetadataHandling
                            .GetFullyQualifiedDataObjectName(localTableTargetName, localTableConnectionTarget)
                            .FirstOrDefault();
                        string localTableSourceFullyQualifiedNameTarget =
                            localFullyQualifiedKeyValuePairTarget.Key + '.' +
                            localFullyQualifiedKeyValuePairTarget.Value;


                        // Process source data objects
                        // Only the first 3 elements count for this step
                        EvaluateDataObjectsToList(localTableSourceFullyQualifiedNameSource, localTableConnectionSource,
                            dataObjectList, false, sourceTargetXrefList, localBusinessKey, localFilterCriterion,
                            localTableSourceFullyQualifiedNameSource, localTableSourceFullyQualifiedNameTarget);

                        // And for the targets, this is where the XREF matters
                        EvaluateDataObjectsToList(localTableSourceFullyQualifiedNameTarget, localTableConnectionTarget,
                            dataObjectList, true, sourceTargetXrefList, localBusinessKey, localFilterCriterion,
                            localTableSourceFullyQualifiedNameSource, localTableSourceFullyQualifiedNameTarget);
                    }
                }

                // Process each of the Data Objects into their own separate tables.
                using (var connection = new SqlConnection(metaDataConnection))
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex)
                    {
                        LogMetadataEvent($"An issue has occurred connecting to the database: \r\n\r\n {ex}.",
                            EventTypes.Error);
                    }

                    foreach (var dataObjectTuple in dataObjectList)
                    {
                        if (dataObjectTuple.Item2 == MetadataHandling.TableTypes.StagingArea ||
                            dataObjectTuple.Item2 == MetadataHandling.TableTypes.PersistentStagingArea ||
                            dataObjectTuple.Item2 == MetadataHandling.TableTypes.CoreBusinessConcept ||
                            dataObjectTuple.Item2 == MetadataHandling.TableTypes.NaturalBusinessRelationship)
                        {
                            LogMetadataEvent($"Adding {dataObjectTuple.Item1} as {dataObjectTuple.Item2}.",
                                EventTypes.Information);

                            var localTableTypeEvaluation = "";
                            var localAttributeEvaluation = "";
                            var localAttributeEvaluationShort = "";

                            switch (dataObjectTuple.Item2)
                            {
                                case MetadataHandling.TableTypes.StagingArea:
                                    localTableTypeEvaluation = "MD_STAGING";
                                    localAttributeEvaluation = "STAGING_NAME";
                                    localAttributeEvaluationShort = "STAGING_NAME_SHORT";
                                    break;
                                case MetadataHandling.TableTypes.PersistentStagingArea:
                                    localTableTypeEvaluation = "MD_PERSISTENT_STAGING";
                                    localAttributeEvaluation = "PERSISTENT_STAGING_NAME";
                                    localAttributeEvaluationShort = "PERSISTENT_STAGING_NAME_SHORT";
                                    break;
                                case MetadataHandling.TableTypes.CoreBusinessConcept:
                                    localTableTypeEvaluation = "MD_HUB";
                                    localAttributeEvaluation = "HUB_NAME";
                                    localAttributeEvaluationShort = "HUB_NAME_SHORT";
                                    break;
                                case MetadataHandling.TableTypes.NaturalBusinessRelationship:
                                    localTableTypeEvaluation = "MD_LINK";
                                    localAttributeEvaluation = "LINK_NAME";
                                    localAttributeEvaluationShort = "LINK_NAME_SHORT";
                                    break;
                                default:
                                    localTableTypeEvaluation = "UNKNOWN";
                                    break;
                            }


                            // Retrieve the business key
                            var businessKey = new List<string>();
                            var fullyQualifiedName = MetadataHandling
                                .GetFullyQualifiedDataObjectName(dataObjectTuple.Item1, dataObjectTuple.Item3)
                                .FirstOrDefault();

                            if (dataObjectTuple.Item1 != "Not applicable")
                            {
                                if (dataObjectTuple.Item2 == MetadataHandling.TableTypes.CoreBusinessConcept)
                                {
                                    if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode)
                                    {
                                        businessKey = MetadataHandling.GetHubTargetBusinessKeyListPhysical(dataObjectTuple.Item1, dataObjectTuple.Item3, TeamConfiguration);
                                    }
                                    else
                                    {
                                        businessKey = MetadataHandling.GetHubTargetBusinessKeyListVirtual(dataObjectTuple.Item1, dataObjectTuple.Item3, versionId, TeamConfiguration);
                                    }
                                }
                            }

                            string businessKeyString = string.Join(",", businessKey);
                            string surrogateKey = MetadataHandling.GetSurrogateKey(dataObjectTuple.Item1, dataObjectTuple.Item3, TeamConfiguration);

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine($"INSERT INTO [{localTableTypeEvaluation}]");
                            insertStatement.AppendLine($"([{localAttributeEvaluation}], [{localAttributeEvaluationShort}],[SCHEMA_NAME], [BUSINESS_KEY], [SURROGATE_KEY])");
                            insertStatement.AppendLine($"VALUES ('{fullyQualifiedName.Key}.{fullyQualifiedName.Value}','{fullyQualifiedName.Value}','{fullyQualifiedName.Key}', '{businessKeyString}', '{surrogateKey}')");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An issue has occurred during preparation of the {dataObjectTuple.Item2} metadata: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);
                            }
                        }
                    }
                }

                worker?.ReportProgress(10);
                subProcess.Stop();
                LogMetadataEvent(
                    $"Preparation of the Data Object metadata completed, and has taken {subProcess.Elapsed.TotalSeconds} seconds.",
                    EventTypes.Information);

                #endregion


                #region Prepare Source to Target relationships

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent($"Commencing preparing the relationship between Sources and Targets.",
                    EventTypes.Information);

                // Process the relationship records
                using (var connection = new SqlConnection(metaDataConnection))
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex)
                    {
                        LogMetadataEvent($"An issue has occurred connecting to the database: \r\n\r\n {ex}.",
                            EventTypes.Error);
                    }

                    foreach (var row in sourceTargetXrefList)
                    {
                        if (row.Item5 == MetadataHandling.TableTypes.StagingArea ||
                            row.Item5 == MetadataHandling.TableTypes.PersistentStagingArea)
                        {
                            var localTableTypeEvaluation = "";
                            var localTableTarget = "";
                            switch (row.Item5)
                            {
                                case MetadataHandling.TableTypes.StagingArea:
                                    localTableTypeEvaluation = "MD_SOURCE_STAGING_XREF";
                                    localTableTarget = "STAGING_NAME";
                                    break;
                                case MetadataHandling.TableTypes.PersistentStagingArea:
                                    localTableTypeEvaluation = "MD_SOURCE_PERSISTENT_STAGING_XREF";
                                    localTableTarget = "PERSISTENT_STAGING_NAME";
                                    break;
                                //case MetadataHandling.TableTypes.CoreBusinessConcept:
                                //    localTableTypeEvaluation = "MD_SOURCE_HUB_XREF";
                                //    localTableTarget = "HUB_NAME_NAME";
                                //    break;
                                //case MetadataHandling.TableTypes.NaturalBusinessRelationship:
                                //    localTableTypeEvaluation = "MD_SOURCE_LINK_XREF";
                                //    localTableTarget = "LINK_NAME";
                                //    break;
                                default:
                                    localTableTypeEvaluation = "UNKNOWN";
                                    break;
                            }

                            LogMetadataEvent($"Processing the {row.Item1} to {row.Item2} relationship.",
                                EventTypes.Information);

                            var businessKeyDefinition = row.Item3.Trim();
                            businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                            var filterCriterion = row.Item4.Trim();
                            filterCriterion = filterCriterion.Replace("'", "''");

                            var loadVector =
                                MetadataHandling.GetDataObjectMappingLoadVector(row.Item1, row.Item2,
                                    TeamConfiguration);

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine($"INSERT INTO [{localTableTypeEvaluation}]");
                            insertStatement.AppendLine(
                                $"([SOURCE_NAME], [{localTableTarget}], [BUSINESS_KEY_DEFINITION], [FILTER_CRITERIA], [LOAD_VECTOR])");
                            insertStatement.AppendLine("VALUES (" +
                                                       "'" + row.Item1 + "', " +
                                                       "'" + row.Item2 + "', " +
                                                       "'" + businessKeyDefinition + "', " +
                                                       "'" + filterCriterion + "', " +
                                                       "'" + loadVector + "'" +
                                                       ")");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An issue has occurred during preparation of the relationship between the Source and Target Data Object: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);
                            }

                        }
                    }
                }

                worker?.ReportProgress(10);
                subProcess.Stop();
                LogMetadataEvent(
                    $"Preparation of the Source to Staging Area XREF metadata has been completed, and has taken {subProcess.Elapsed.TotalSeconds} seconds.",
                    EventTypes.Information);

                #endregion


                #region Prepare Satellites

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent("Commencing preparing the Satellite metadata.", EventTypes.Information);

                var prepareSatStatement = new StringBuilder();

                prepareSatStatement.AppendLine("SELECT DISTINCT");
                prepareSatStatement.AppendLine("  spec.TARGET_TABLE_SCHEMA+'.'+spec.TARGET_TABLE AS SATELLITE_NAME,");
                prepareSatStatement.AppendLine("  hubkeysub.HUB_NAME, ");
                prepareSatStatement.AppendLine("  'Normal' AS SATELLITE_TYPE, ");
                prepareSatStatement.AppendLine(
                    "  (SELECT LINK_NAME FROM MD_LINK WHERE LINK_NAME_SHORT = 'Not applicable') AS LINK_NAME -- No link for normal Satellites ");
                prepareSatStatement.AppendLine("FROM TMP_MD_TABLE_MAPPING spec ");
                prepareSatStatement.AppendLine("LEFT OUTER JOIN ");
                prepareSatStatement.AppendLine("(");
                prepareSatStatement.AppendLine(
                    "  SELECT DISTINCT TARGET_TABLE, hub.HUB_NAME, SOURCE_TABLE, BUSINESS_KEY_ATTRIBUTE ");
                prepareSatStatement.AppendLine("  FROM TMP_MD_TABLE_MAPPING spec2 ");
                prepareSatStatement.AppendLine("  LEFT OUTER JOIN -- Join in the Hub NAME from the MD table ");
                prepareSatStatement.AppendLine(
                    "  MD_HUB hub ON hub.HUB_NAME_SHORT=spec2.TARGET_TABLE and hub.SCHEMA_NAME = spec2.TARGET_TABLE_SCHEMA ");
                prepareSatStatement.AppendLine("  WHERE TARGET_TABLE_TYPE = '" +
                                               MetadataHandling.TableTypes.CoreBusinessConcept +
                                               "' AND [ENABLED_INDICATOR] = 'True'                                                        ");
                prepareSatStatement.AppendLine(") hubkeysub ");
                prepareSatStatement.AppendLine("        ON spec.SOURCE_TABLE = hubkeysub.SOURCE_TABLE ");
                prepareSatStatement.AppendLine(
                    "        AND replace(spec.BUSINESS_KEY_ATTRIBUTE,' ','')=replace(hubkeysub.BUSINESS_KEY_ATTRIBUTE,' ','') ");
                prepareSatStatement.AppendLine("WHERE spec.TARGET_TABLE_TYPE = '" +
                                               MetadataHandling.TableTypes.Context + "' ");
                prepareSatStatement.AppendLine("AND [ENABLED_INDICATOR] = 'True'");

                var listSat = Utility.GetDataTable(ref connOmd, prepareSatStatement.ToString());

                foreach (DataRow satelliteName in listSat.Rows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        var tableName = satelliteName["SATELLITE_NAME"].ToString().Trim();
                        var tableType = satelliteName["SATELLITE_TYPE"].ToString().Trim();
                        var hubName = satelliteName["HUB_NAME"];
                        var linkName = satelliteName["LINK_NAME"];

                        //var fullyQualifiedName = MetadataHandling.GetTableAndSchema(tableName).FirstOrDefault();


                        string[] fullyQualifiedName = tableName.Split('.');

                        //Separate out schema and table


                        if (tableName != "Not applicable")
                        {
                            LogMetadataEvent("Processing " + tableName + ".", EventTypes.Information);
                        }

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_SATELLITE]");
                        insertStatement.AppendLine(
                            "([SATELLITE_NAME], [SATELLITE_NAME_SHORT], [SATELLITE_TYPE], [SCHEMA_NAME], [HUB_NAME], [LINK_NAME])");
                        insertStatement.AppendLine("VALUES (" +
                                                   "'" + tableName + "'," +
                                                   "'" + fullyQualifiedName[1] + "'," +
                                                   "'" + tableType + "'," +
                                                   "'" + fullyQualifiedName[0] + "','" +
                                                   "" + hubName + "','" +
                                                   "" + linkName + "'" +
                                                   ")");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            LogMetadataEvent(
                                $"An issue has occurred during preparation of the Satellites: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                EventTypes.Error);
                        }
                    }
                }

                worker.ReportProgress(24);
                subProcess.Stop();
                LogMetadataEvent(
                    $"Preparation of the Satellite metadata completed, and has taken {subProcess.Elapsed.TotalSeconds} seconds.",
                    EventTypes.Information);

                #endregion


                #region Prepare Link Satellites

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent("Commencing preparing the Link Satellite metadata.", EventTypes.Information);

                var prepareLsatStatement = new StringBuilder();
                prepareLsatStatement.AppendLine("SELECT DISTINCT");
                prepareLsatStatement.AppendLine(
                    "        spec.TARGET_TABLE_SCHEMA+'.'+spec.TARGET_TABLE AS SATELLITE_NAME, ");
                prepareLsatStatement.AppendLine(
                    "        (SELECT HUB_NAME FROM MD_HUB WHERE HUB_NAME_SHORT = 'Not applicable') AS HUB_NAME, -- No Hub for Link Satellites");
                prepareLsatStatement.AppendLine("        'Link Satellite' AS SATELLITE_TYPE,");
                prepareLsatStatement.AppendLine("        lnkkeysub.LINK_NAME");
                prepareLsatStatement.AppendLine("FROM TMP_MD_TABLE_MAPPING spec");
                prepareLsatStatement.AppendLine("LEFT OUTER JOIN  -- Get the Link ID that belongs to this LSAT");
                prepareLsatStatement.AppendLine("(");
                prepareLsatStatement.AppendLine("        SELECT DISTINCT ");
                prepareLsatStatement.AppendLine("                lnk.LINK_NAME AS LINK_NAME,");
                prepareLsatStatement.AppendLine("                SOURCE_TABLE,");
                prepareLsatStatement.AppendLine("                BUSINESS_KEY_ATTRIBUTE");
                prepareLsatStatement.AppendLine("        FROM TMP_MD_TABLE_MAPPING spec2");
                prepareLsatStatement.AppendLine("        LEFT OUTER JOIN -- Join in the Link Name from the MD table");
                prepareLsatStatement.AppendLine(
                    "                MD_LINK lnk ON lnk.LINK_NAME_SHORT = spec2.TARGET_TABLE AND lnk.SCHEMA_NAME = spec2.TARGET_TABLE_SCHEMA");
                prepareLsatStatement.AppendLine("        WHERE TARGET_TABLE_TYPE = '" +
                                                MetadataHandling.TableTypes.NaturalBusinessRelationship + "' ");
                prepareLsatStatement.AppendLine("        AND [ENABLED_INDICATOR] = 'True'");
                prepareLsatStatement.AppendLine(") lnkkeysub");
                prepareLsatStatement.AppendLine(
                    "    ON spec.SOURCE_TABLE=lnkkeysub.SOURCE_TABLE -- Only the combination of Link table and Business key can belong to the LSAT");
                prepareLsatStatement.AppendLine(
                    "    AND REPLACE(spec.BUSINESS_KEY_ATTRIBUTE,' ','')=REPLACE(lnkkeysub.BUSINESS_KEY_ATTRIBUTE,' ','')");
                prepareLsatStatement.AppendLine(
                    "-- Only select Link Satellites as the base / driving table (spec alias)");
                prepareLsatStatement.AppendLine("WHERE spec.TARGET_TABLE_TYPE = '" +
                                                MetadataHandling.TableTypes.NaturalBusinessRelationshipContext + "'");
                prepareLsatStatement.AppendLine("AND [ENABLED_INDICATOR] = 'True'");


                var listLsat = Utility.GetDataTable(ref connOmd, prepareLsatStatement.ToString());

                foreach (DataRow satelliteName in listLsat.Rows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        var tableName = satelliteName["SATELLITE_NAME"].ToString().Trim();
                        var tableType = satelliteName["SATELLITE_TYPE"].ToString().Trim();
                        var hubName = satelliteName["HUB_NAME"];
                        var linkName = satelliteName["LINK_NAME"];

                        //var fullyQualifiedName = MetadataHandling.GetTableAndSchema(tableName).FirstOrDefault();

                        string[] fullyQualifiedName = tableName.Split('.');

                        LogMetadataEvent("Processing " + tableName + ".", EventTypes.Information);

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_SATELLITE]");
                        insertStatement.AppendLine(
                            "([SATELLITE_NAME], [SATELLITE_NAME_SHORT], [SATELLITE_TYPE], [SCHEMA_NAME], [HUB_NAME], [LINK_NAME])");
                        insertStatement.AppendLine("VALUES (" +
                                                   "'" + tableName + "'," +
                                                   "'" + fullyQualifiedName[1] + "'," +
                                                   "'" + tableType + "'," +
                                                   "'" + fullyQualifiedName[0] + "'," +
                                                   "'" + hubName + "'," +
                                                   "'" + linkName + "'" +
                                                   ")");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            LogMetadataEvent(
                                $"An issue has occurred during preparation of the Link-Satellites: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                EventTypes.Error);
                        }
                    }
                }

                worker.ReportProgress(28);
                subProcess.Stop();
                LogMetadataEvent(
                    "Preparation of the Link Satellite metadata completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);

                #endregion


                #region Prepare Source / SAT Xref

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent(
                    "Commencing preparing the relationship between (Link) Satellites and the Source tables.",
                    EventTypes.Information);

                var prepareSatXrefStatement = new StringBuilder();
                prepareSatXrefStatement.AppendLine("SELECT");
                prepareSatXrefStatement.AppendLine("        sat.SATELLITE_NAME AS SATELLITE_NAME,");
                prepareSatXrefStatement.AppendLine("        stg.SOURCE_NAME AS SOURCE_NAME,");
                prepareSatXrefStatement.AppendLine("        spec.BUSINESS_KEY_ATTRIBUTE,");
                prepareSatXrefStatement.AppendLine("        spec.FILTER_CRITERIA");
                prepareSatXrefStatement.AppendLine("FROM TMP_MD_TABLE_MAPPING spec");
                prepareSatXrefStatement.AppendLine("LEFT OUTER JOIN -- Join in the Source_ID from the MD_SOURCE table");
                prepareSatXrefStatement.AppendLine(
                    "        MD_SOURCE stg ON stg.SOURCE_NAME_SHORT = spec.SOURCE_TABLE and stg.SCHEMA_NAME = spec.SOURCE_TABLE_SCHEMA");
                prepareSatXrefStatement.AppendLine(
                    "LEFT OUTER JOIN -- Join in the Satellite_ID from the MD_SATELLITE table");
                prepareSatXrefStatement.AppendLine(
                    "        MD_SATELLITE sat ON sat.SATELLITE_NAME_SHORT = spec.TARGET_TABLE AND sat.SCHEMA_NAME = spec.TARGET_TABLE_SCHEMA");
                prepareSatXrefStatement.AppendLine("WHERE spec.TARGET_TABLE_TYPE = '" +
                                                   MetadataHandling.TableTypes.Context + "'");
                prepareSatXrefStatement.AppendLine("AND [ENABLED_INDICATOR] = 'True'");
                prepareSatXrefStatement.AppendLine("UNION");
                prepareSatXrefStatement.AppendLine("SELECT");
                prepareSatXrefStatement.AppendLine("        sat.SATELLITE_NAME,");
                prepareSatXrefStatement.AppendLine("        stg.SOURCE_NAME,");
                prepareSatXrefStatement.AppendLine("        spec.BUSINESS_KEY_ATTRIBUTE,");
                prepareSatXrefStatement.AppendLine("        spec.FILTER_CRITERIA");
                prepareSatXrefStatement.AppendLine("FROM TMP_MD_TABLE_MAPPING spec");
                prepareSatXrefStatement.AppendLine("LEFT OUTER JOIN -- Join in the Source from the MD_SOURCE table");
                prepareSatXrefStatement.AppendLine(
                    "        MD_SOURCE stg ON stg.SOURCE_NAME_SHORT = spec.SOURCE_TABLE and stg.SCHEMA_NAME = spec.SOURCE_TABLE_SCHEMA");
                prepareSatXrefStatement.AppendLine(
                    "LEFT OUTER JOIN -- Join in the Satellite_ID from the MD_SATELLITE table");
                prepareSatXrefStatement.AppendLine(
                    "        MD_SATELLITE sat ON sat.SATELLITE_NAME_SHORT = spec.TARGET_TABLE AND sat.SCHEMA_NAME = spec.TARGET_TABLE_SCHEMA");
                prepareSatXrefStatement.AppendLine("WHERE spec.TARGET_TABLE_TYPE = '" +
                                                   MetadataHandling.TableTypes.NaturalBusinessRelationshipContext +
                                                   "'");
                prepareSatXrefStatement.AppendLine("AND [ENABLED_INDICATOR] = 'True'");

                var listSatXref = Utility.GetDataTable(ref connOmd, prepareSatXrefStatement.ToString());

                foreach (DataRow tableName in listSatXref.Rows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        LogMetadataEvent(
                            "Processing the " + tableName["SOURCE_NAME"] + " to " + tableName["SATELLITE_NAME"] +
                            " relationship.", EventTypes.Information);

                        var insertStatement = new StringBuilder();
                        var filterCriterion = tableName["FILTER_CRITERIA"].ToString().Trim();
                        filterCriterion = filterCriterion.Replace("'", "''");

                        var businessKeyDefinition = tableName["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                        businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                        var loadVector = MetadataHandling.GetDataObjectMappingLoadVector(
                            tableName["SOURCE_NAME"].ToString(),
                            tableName["SATELLITE_NAME"].ToString(), TeamConfiguration);

                        insertStatement.AppendLine("INSERT INTO [MD_SOURCE_SATELLITE_XREF]");
                        insertStatement.AppendLine(
                            "([SATELLITE_NAME], [SOURCE_NAME], [BUSINESS_KEY_DEFINITION], [FILTER_CRITERIA], [LOAD_VECTOR])");
                        insertStatement.AppendLine("VALUES ('" +
                                                   tableName["SATELLITE_NAME"] + "','" +
                                                   tableName["SOURCE_NAME"] + "','" +
                                                   businessKeyDefinition + "','" +
                                                   filterCriterion + "','" +
                                                   loadVector + "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            LogMetadataEvent(
                                $"An issue has occurred during preparation of the relationship between the Source and the Satellites: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                EventTypes.Error);
                        }
                    }
                }

                worker.ReportProgress(28);
                subProcess.Stop();
                LogMetadataEvent(
                    "Preparation of the Source / Satellite XREF metadata completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);

                #endregion


                #region Source / Hub relationship - 30%

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent("Commencing preparing the relationship between Source and Hubs.",
                    EventTypes.Information);

                var prepareStgHubXrefStatement = new StringBuilder();
                prepareStgHubXrefStatement.AppendLine("SELECT");
                prepareStgHubXrefStatement.AppendLine("    HUB_NAME,");
                prepareStgHubXrefStatement.AppendLine("    SOURCE_NAME,");
                prepareStgHubXrefStatement.AppendLine("    BUSINESS_KEY_ATTRIBUTE,");
                prepareStgHubXrefStatement.AppendLine("    FILTER_CRITERIA");
                prepareStgHubXrefStatement.AppendLine("FROM");
                prepareStgHubXrefStatement.AppendLine("(      ");
                prepareStgHubXrefStatement.AppendLine("    SELECT DISTINCT ");
                prepareStgHubXrefStatement.AppendLine("    SOURCE_TABLE,");
                prepareStgHubXrefStatement.AppendLine("    SOURCE_TABLE_SCHEMA,");
                prepareStgHubXrefStatement.AppendLine("    TARGET_TABLE,");
                prepareStgHubXrefStatement.AppendLine("    TARGET_TABLE_SCHEMA,");
                prepareStgHubXrefStatement.AppendLine("    BUSINESS_KEY_ATTRIBUTE,");
                prepareStgHubXrefStatement.AppendLine("    FILTER_CRITERIA");
                prepareStgHubXrefStatement.AppendLine("    FROM TMP_MD_TABLE_MAPPING");
                prepareStgHubXrefStatement.AppendLine("    WHERE ");
                prepareStgHubXrefStatement.AppendLine("        TARGET_TABLE_TYPE = '" +
                                                      MetadataHandling.TableTypes.CoreBusinessConcept + "'");
                prepareStgHubXrefStatement.AppendLine("    AND [ENABLED_INDICATOR] = 'True'");
                prepareStgHubXrefStatement.AppendLine(") hub");
                prepareStgHubXrefStatement.AppendLine("LEFT OUTER JOIN");
                prepareStgHubXrefStatement.AppendLine("( ");
                prepareStgHubXrefStatement.AppendLine("    SELECT SOURCE_NAME, SOURCE_NAME_SHORT, [SCHEMA_NAME]");
                prepareStgHubXrefStatement.AppendLine("    FROM MD_SOURCE");
                prepareStgHubXrefStatement.AppendLine(") stgsub");
                prepareStgHubXrefStatement.AppendLine(
                    "ON hub.SOURCE_TABLE = stgsub.SOURCE_NAME_SHORT and hub.SOURCE_TABLE_SCHEMA = stgsub.SCHEMA_NAME");
                prepareStgHubXrefStatement.AppendLine("LEFT OUTER JOIN");
                prepareStgHubXrefStatement.AppendLine("( ");
                prepareStgHubXrefStatement.AppendLine("    SELECT HUB_NAME, HUB_NAME_SHORT, [SCHEMA_NAME]");
                prepareStgHubXrefStatement.AppendLine("    FROM MD_HUB");
                prepareStgHubXrefStatement.AppendLine(") hubsub");
                prepareStgHubXrefStatement.AppendLine(
                    "ON hub.TARGET_TABLE = hubsub.HUB_NAME_SHORT and hub.TARGET_TABLE_SCHEMA = hubsub.SCHEMA_NAME");

                var listXref = Utility.GetDataTable(ref connOmd, prepareStgHubXrefStatement.ToString());

                foreach (DataRow tableName in listXref.Rows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        LogMetadataEvent(
                            "Processing the " + tableName["SOURCE_NAME"] + " to " + tableName["HUB_NAME"] +
                            " relationship.", EventTypes.Information);

                        var filterCriterion = tableName["FILTER_CRITERIA"].ToString().Trim();
                        filterCriterion = filterCriterion.Replace("'", "''");

                        var businessKeyDefinition = tableName["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                        businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                        var loadVector = MetadataHandling.GetDataObjectMappingLoadVector(
                            tableName["SOURCE_NAME"].ToString(),
                            tableName["HUB_NAME"].ToString(), TeamConfiguration);

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_SOURCE_HUB_XREF]");
                        insertStatement.AppendLine(
                            "([HUB_NAME], [SOURCE_NAME], [BUSINESS_KEY_DEFINITION], [FILTER_CRITERIA], [LOAD_VECTOR])");
                        insertStatement.AppendLine("VALUES ('" + tableName["HUB_NAME"] +
                                                   "','" + tableName["SOURCE_NAME"] +
                                                   "','" + businessKeyDefinition +
                                                   "','" + filterCriterion +
                                                   "','" + loadVector +
                                                   "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            LogMetadataEvent(
                                $"An issue has occurred during preparation of the relationship between the Source and the Hubs: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                EventTypes.Error);
                        }
                    }
                }

                worker.ReportProgress(30);
                subProcess.Stop();
                LogMetadataEvent(
                    $"Preparation of relationship between Source and Hubs has been completed, and has taken {subProcess.Elapsed.TotalSeconds} seconds.",
                    EventTypes.Information);

                #endregion


                #region Prepare attributes - 45%

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                var attCounter = 1;

                // Dummy row - insert 'Not Applicable' attribute to satisfy RI
                using (var connection = new SqlConnection(metaDataConnection))
                {
                    var insertNAStatement = new StringBuilder();

                    insertNAStatement.AppendLine("INSERT INTO [MD_ATTRIBUTE]");
                    insertNAStatement.AppendLine("([ATTRIBUTE_NAME])");
                    insertNAStatement.AppendLine("VALUES ('Not applicable')");

                    var commandNA = new SqlCommand(insertNAStatement.ToString(), connection);

                    var insertNULLStatement = new StringBuilder();

                    insertNULLStatement.AppendLine("INSERT INTO [MD_ATTRIBUTE]");
                    insertNULLStatement.AppendLine("([ATTRIBUTE_NAME])");
                    insertNULLStatement.AppendLine("VALUES ('NULL')");

                    var commandNULL = new SqlCommand(insertNULLStatement.ToString(), connection);

                    try
                    {
                        connection.Open();
                        commandNA.ExecuteNonQuery();
                        attCounter++;
                        commandNULL.ExecuteNonQuery();
                        attCounter++;
                    }
                    catch (Exception ex)
                    {
                        LogMetadataEvent(
                            $"An issue has occurred during preparation of the attribute metadata: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertNAStatement}",
                            EventTypes.Error);
                    }
                }

                /* Regular processing
                    RV: there is an issue below where not all SQL version (i.e. SQL Server) are supporting cross database SQL.
                    i.e. Azure. long term fix is to create individual queries to database without cross-db sql and add to single data table in the application
                */
                if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode) // Read from live database
                {
                    LogMetadataEvent("Commencing preparing the attributes directly from the database.",
                        EventTypes.Information);
                }
                else // Virtual processing
                {
                    LogMetadataEvent("Commencing preparing the attributes from the metadata.", EventTypes.Information);
                }

                var prepareAttStatement = new StringBuilder();
                prepareAttStatement.AppendLine("SELECT DISTINCT(COLUMN_NAME) AS COLUMN_NAME FROM (");

                prepareAttStatement.AppendLine("SELECT");
                prepareAttStatement.AppendLine("  [DATABASE_NAME]");
                prepareAttStatement.AppendLine(" ,[SCHEMA_NAME]");
                prepareAttStatement.AppendLine(" ,[TABLE_NAME]");
                prepareAttStatement.AppendLine(" ,[COLUMN_NAME]");
                prepareAttStatement.AppendLine(" ,[DATA_TYPE]");
                prepareAttStatement.AppendLine(" ,[CHARACTER_MAXIMUM_LENGTH]");
                prepareAttStatement.AppendLine(" ,[NUMERIC_PRECISION]");
                prepareAttStatement.AppendLine(" ,[NUMERIC_SCALE]");
                prepareAttStatement.AppendLine(" ,[ORDINAL_POSITION]");
                prepareAttStatement.AppendLine(" ,[PRIMARY_KEY_INDICATOR]");
                prepareAttStatement.AppendLine("FROM [MD_PHYSICAL_MODEL]");

                prepareAttStatement.AppendLine(") sub");
                prepareAttStatement.AppendLine("WHERE sub.COLUMN_NAME NOT IN");
                prepareAttStatement.AppendLine("  ( ");
                prepareAttStatement.AppendLine("    '" + recordSource + "',");
                prepareAttStatement.AppendLine("    '" + alternativeRecordSource + "',");
                prepareAttStatement.AppendLine("    '" + sourceRowId + "',");
                prepareAttStatement.AppendLine("    '" + recordChecksum + "',");
                prepareAttStatement.AppendLine("    '" + changeDataCaptureIndicator + "',");
                prepareAttStatement.AppendLine("    '" + hubAlternativeLdts + "',");
                prepareAttStatement.AppendLine("    '" + eventDateTimeAttribute + "',");
                prepareAttStatement.AppendLine("    '" + effectiveDateTimeAttribute + "',");
                prepareAttStatement.AppendLine("    '" + etlProcessId + "',");
                prepareAttStatement.AppendLine("    '" + loadDateTimeStamp + "',");
                prepareAttStatement.AppendLine("    '" + currentRecordAttribute + "'");
                prepareAttStatement.AppendLine("  ) ");

                // Load the data table, get the attributes
                var listAtt = Utility.GetDataTable(ref connOmd, prepareAttStatement.ToString());

                // Convert to a List object
                List<string> attributeList = new List<string>();
                if (listAtt != null)
                {
                    foreach (DataRow row in listAtt.Rows)
                    {
                        if (!attributeList.Contains(row["COLUMN_NAME"].ToString()))
                        {
                            attributeList.Add(row["COLUMN_NAME"].ToString());
                        }
                    }
                }


                //Also get attributes from the data grid, just in case there are a few hardcoded ones or formulas.
                foreach (DataGridViewRow row in dataGridViewAttributeMetadata.Rows)
                {
                    string localAttributeSource =
                        (string) row.Cells[AttributeMappingMetadataColumns.SourceColumn.ToString()].Value;
                    string localAttributeTarget =
                        (string) row.Cells[AttributeMappingMetadataColumns.TargetColumn.ToString()].Value;

                    localAttributeSource = MetadataHandling.QuoteStringValuesForAttributes(localAttributeSource);
                    localAttributeTarget = MetadataHandling.QuoteStringValuesForAttributes(localAttributeTarget);

                    if (!attributeList.Contains(localAttributeSource) && localAttributeSource != null)
                    {
                        attributeList.Add(localAttributeSource);
                    }

                    if (!attributeList.Contains(localAttributeTarget) && localAttributeTarget != null)
                    {
                        attributeList.Add(localAttributeTarget);
                    }
                }


                // Check if there are any attributes found, otherwise insert into the repository.
                if (attributeList == null || attributeList.Count == 0)
                {
                    LogMetadataEvent($"No attributes were found in the metadata, did you reverse-engineer the model?",
                        EventTypes.Warning);
                }
                else
                {
                    foreach (string attributeName in attributeList)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            var insertStatement = new StringBuilder();

                            insertStatement.AppendLine("INSERT INTO [MD_ATTRIBUTE]");
                            insertStatement.AppendLine("([ATTRIBUTE_NAME])");
                            insertStatement.AppendLine("VALUES ('" + attributeName.Trim() + "')");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                attCounter++;
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An issue has occurred during preparation of the attribute metadata: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);
                            }
                        }
                    }

                    LogMetadataEvent($"Processing {attCounter} attributes.", EventTypes.Information);
                }

                worker.ReportProgress(45);
                subProcess.Stop();
                LogMetadataEvent(
                    $"Preparation of the attributes has been completed, and has taken {subProcess.Elapsed.TotalSeconds} seconds.",
                    EventTypes.Information);

                #endregion


                #region Business Key - 50%

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent("Commencing the definition of the Business Key.", EventTypes.Information);

                var prepareKeyStatement = new StringBuilder();

                prepareKeyStatement.AppendLine("SELECT");
                prepareKeyStatement.AppendLine("  SOURCE_NAME,");
                prepareKeyStatement.AppendLine("  TARGET_NAME,");
                prepareKeyStatement.AppendLine("  BUSINESS_KEY_ATTRIBUTE,");
                prepareKeyStatement.AppendLine(
                    "  ROW_NUMBER() OVER(PARTITION BY SOURCE_NAME, TARGET_NAME, BUSINESS_KEY_ATTRIBUTE ORDER BY SOURCE_NAME, TARGET_NAME, COMPONENT_ORDER ASC) AS COMPONENT_ID,");
                prepareKeyStatement.AppendLine("  COMPONENT_ORDER,");
                prepareKeyStatement.AppendLine("  REPLACE(COMPONENT_VALUE,'COMPOSITE(', '') AS COMPONENT_VALUE,");
                prepareKeyStatement.AppendLine("    CASE");
                prepareKeyStatement.AppendLine(
                    "            WHEN SUBSTRING(BUSINESS_KEY_ATTRIBUTE,1, 11)= 'CONCATENATE' THEN 'CONCATENATE()'");
                prepareKeyStatement.AppendLine(
                    "            WHEN SUBSTRING(BUSINESS_KEY_ATTRIBUTE,1, 6)= 'PIVOT' THEN 'PIVOT()'");
                prepareKeyStatement.AppendLine(
                    "            WHEN SUBSTRING(BUSINESS_KEY_ATTRIBUTE,1, 9)= 'COMPOSITE' THEN 'COMPOSITE()'");
                prepareKeyStatement.AppendLine("            ELSE 'NORMAL'");
                prepareKeyStatement.AppendLine("    END AS COMPONENT_TYPE");
                prepareKeyStatement.AppendLine("FROM");
                prepareKeyStatement.AppendLine("(");
                prepareKeyStatement.AppendLine("    SELECT DISTINCT");
                prepareKeyStatement.AppendLine("        A.SOURCE_TABLE,");
                prepareKeyStatement.AppendLine("        A.SOURCE_TABLE_SCHEMA,");
                prepareKeyStatement.AppendLine("        A.BUSINESS_KEY_ATTRIBUTE,");
                prepareKeyStatement.AppendLine("        A.TARGET_TABLE,");
                prepareKeyStatement.AppendLine("        A.TARGET_TABLE_SCHEMA,");
                prepareKeyStatement.AppendLine("        CASE");
                prepareKeyStatement.AppendLine(
                    "            WHEN CHARINDEX('(', RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)')))) > 0");
                prepareKeyStatement.AppendLine("            THEN RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)')))");
                prepareKeyStatement.AppendLine(
                    "            ELSE REPLACE(RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)'))), ')', '')");
                prepareKeyStatement.AppendLine("        END AS COMPONENT_VALUE,");
                prepareKeyStatement.AppendLine(
                    "        ROW_NUMBER() OVER(PARTITION BY SOURCE_TABLE, TARGET_TABLE, BUSINESS_KEY_ATTRIBUTE ORDER BY SOURCE_TABLE, TARGET_TABLE, BUSINESS_KEY_ATTRIBUTE ASC) AS COMPONENT_ORDER");
                prepareKeyStatement.AppendLine("    FROM");
                prepareKeyStatement.AppendLine("    (");
                prepareKeyStatement.AppendLine("      SELECT");
                prepareKeyStatement.AppendLine("          SOURCE_TABLE, ");
                prepareKeyStatement.AppendLine("          SOURCE_TABLE_SCHEMA, ");
                prepareKeyStatement.AppendLine("          TARGET_TABLE, ");
                prepareKeyStatement.AppendLine("          TARGET_TABLE_SCHEMA, ");
                prepareKeyStatement.AppendLine("          BUSINESS_KEY_ATTRIBUTE,");
                prepareKeyStatement.AppendLine(
                    "          CASE SUBSTRING(BUSINESS_KEY_ATTRIBUTE, 0, CHARINDEX('(', BUSINESS_KEY_ATTRIBUTE))");
                prepareKeyStatement.AppendLine(
                    "             WHEN 'COMPOSITE' THEN CONVERT(XML, '<M>' + REPLACE(BUSINESS_KEY_ATTRIBUTE, ';', '</M><M>') + '</M>') ");
                prepareKeyStatement.AppendLine(
                    "             ELSE CONVERT(XML, '<M>' + REPLACE(BUSINESS_KEY_ATTRIBUTE, ',', '</M><M>') + '</M>') ");
                prepareKeyStatement.AppendLine("          END AS BUSINESS_KEY_ATTRIBUTE_XML");
                prepareKeyStatement.AppendLine("        FROM");
                prepareKeyStatement.AppendLine("        (");
                prepareKeyStatement.AppendLine(
                    "            SELECT DISTINCT SOURCE_TABLE, SOURCE_TABLE_SCHEMA, TARGET_TABLE, TARGET_TABLE_SCHEMA, LTRIM(RTRIM(BUSINESS_KEY_ATTRIBUTE)) AS BUSINESS_KEY_ATTRIBUTE");
                prepareKeyStatement.AppendLine("            FROM TMP_MD_TABLE_MAPPING");
                prepareKeyStatement.AppendLine("            WHERE TARGET_TABLE_TYPE = '" +
                                               MetadataHandling.TableTypes.CoreBusinessConcept + "'");
                prepareKeyStatement.AppendLine("              AND [ENABLED_INDICATOR] = 'True'");
                prepareKeyStatement.AppendLine("        ) TableName");
                prepareKeyStatement.AppendLine(
                    "    ) AS A CROSS APPLY BUSINESS_KEY_ATTRIBUTE_XML.nodes('/M') AS Split(a)");
                prepareKeyStatement.AppendLine(
                    "    WHERE BUSINESS_KEY_ATTRIBUTE <> 'N/A' AND A.BUSINESS_KEY_ATTRIBUTE != ''");
                prepareKeyStatement.AppendLine(") pivotsub");
                prepareKeyStatement.AppendLine("LEFT OUTER JOIN");
                prepareKeyStatement.AppendLine("       (");
                prepareKeyStatement.AppendLine("              SELECT SOURCE_NAME, SOURCE_NAME_SHORT, [SCHEMA_NAME]");
                prepareKeyStatement.AppendLine("              FROM MD_SOURCE");
                prepareKeyStatement.AppendLine("       ) stgsub");
                prepareKeyStatement.AppendLine(
                    "ON pivotsub.SOURCE_TABLE = stgsub.SOURCE_NAME_SHORT AND pivotsub.SOURCE_TABLE_SCHEMA = stgsub.SCHEMA_NAME");
                prepareKeyStatement.AppendLine("LEFT OUTER JOIN");
                prepareKeyStatement.AppendLine("       (");
                prepareKeyStatement.AppendLine(
                    "              SELECT HUB_NAME AS TARGET_NAME, HUB_NAME_SHORT, [SCHEMA_NAME]");
                prepareKeyStatement.AppendLine("              FROM MD_HUB");
                prepareKeyStatement.AppendLine("       ) hubsub");
                prepareKeyStatement.AppendLine(
                    "ON pivotsub.TARGET_TABLE = hubsub.HUB_NAME_SHORT AND pivotsub.TARGET_TABLE_SCHEMA = hubsub.SCHEMA_NAME");
                prepareKeyStatement.AppendLine("ORDER BY stgsub.SOURCE_NAME, hubsub.TARGET_NAME, COMPONENT_ORDER");

                var listKeys = Utility.GetDataTable(ref connOmd, prepareKeyStatement.ToString());

                if (listKeys.Rows.Count == 0)
                {
                    LogMetadataEvent("No attributes were found in the metadata, did you reverse-engineer the model?",
                        EventTypes.Warning);
                }
                else
                {
                    foreach (DataRow tableName in listKeys.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {

                            var keyComponent = tableName["COMPONENT_VALUE"]; //Handle quotes between SQL and C%
                            keyComponent = keyComponent.ToString().Replace("'", "''");

                            LogMetadataEvent("Processing the Business Key " +
                                             tableName["BUSINESS_KEY_ATTRIBUTE"] + " (for component " +
                                             keyComponent + ") from " + tableName["SOURCE_NAME"] + " to " +
                                             tableName["TARGET_NAME"] + ".", EventTypes.Information);

                            var businessKeyDefinition = tableName["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                            businessKeyDefinition = businessKeyDefinition.Replace("'", "''");


                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_BUSINESS_KEY_COMPONENT]");
                            insertStatement.AppendLine(
                                "(SOURCE_NAME, HUB_NAME, BUSINESS_KEY_DEFINITION, COMPONENT_ID, COMPONENT_ORDER, COMPONENT_VALUE, COMPONENT_TYPE)");
                            insertStatement.AppendLine("VALUES ('" + tableName["SOURCE_NAME"] + "','" +
                                                       tableName["TARGET_NAME"] + "','" + businessKeyDefinition +
                                                       "','" + tableName["COMPONENT_ID"] + "','" +
                                                       tableName["COMPONENT_ORDER"] + "','" + keyComponent + "','" +
                                                       tableName["COMPONENT_TYPE"] + "')");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An issue has occurred during preparation of the business key metadata: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);
                            }
                        }
                    }
                }

                worker.ReportProgress(50);
                subProcess.Stop();
                LogMetadataEvent(
                    "Preparation of the Business Key definition completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);

                #endregion


                #region Business Key components - 60%

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent("Commencing the Business Key component analysis.", EventTypes.Information);

                var prepareKeyComponentStatement = new StringBuilder();
                var keyPartCounter = 1;
                /*LBM 2019/01/10: Changing to use @ String*/
                prepareKeyComponentStatement.AppendLine(@"
                                                            SELECT DISTINCT
                                                              SOURCE_NAME,
                                                              HUB_NAME,
                                                              BUSINESS_KEY_DEFINITION,
                                                              COMPONENT_ID,
                                                              ROW_NUMBER() over(partition by SOURCE_NAME, HUB_NAME, BUSINESS_KEY_DEFINITION, COMPONENT_ID order by nullif(0 * Split.a.value('count(.)', 'int'), 0)) AS COMPONENT_ELEMENT_ID,
                                                              ROW_NUMBER() over(partition by SOURCE_NAME, HUB_NAME, BUSINESS_KEY_DEFINITION, COMPONENT_ID order by nullif(0 * Split.a.value('count(.)', 'int'), 0)) AS COMPONENT_ELEMENT_ORDER,
                                                              REPLACE(REPLACE(REPLACE(RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)'))), 'CONCATENATE(', ''), ')', ''), 'COMPOSITE(', '') AS COMPONENT_ELEMENT_VALUE,
                                                              CASE
                                                                 WHEN charindex(CHAR(39), REPLACE(REPLACE(RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)'))), 'CONCATENATE(', ''), ')', '')) = 1 THEN 'User Defined Value'
                                                                ELSE 'Attribute'
                                                              END AS COMPONENT_ELEMENT_TYPE,
                                                              COALESCE(att.ATTRIBUTE_NAME, 'Not applicable') AS ATTRIBUTE_NAME
                                                            FROM
                                                            (
                                                                SELECT
                                                                    SOURCE_NAME,
                                                                    HUB_NAME,
                                                                    BUSINESS_KEY_DEFINITION,
                                                                    COMPONENT_ID,
                                                                    COMPONENT_VALUE,
                                                                    CONVERT(XML, '<M>' + REPLACE(COMPONENT_VALUE, ';', '</M><M>') + '</M>') AS COMPONENT_VALUE_XML
														        FROM MD_BUSINESS_KEY_COMPONENT
                                                            ) AS A CROSS APPLY COMPONENT_VALUE_XML.nodes('/M') AS Split(a)
                                                            LEFT OUTER JOIN MD_ATTRIBUTE att ON
                                                                REPLACE(REPLACE(RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)'))), 'CONCATENATE(', ''), ')', '') = att.ATTRIBUTE_NAME
                                                            WHERE COMPONENT_VALUE <> 'N/A' AND A.COMPONENT_VALUE != ''
                                                            ORDER BY A.SOURCE_NAME, A.HUB_NAME, BUSINESS_KEY_DEFINITION, A.COMPONENT_ID, COMPONENT_ELEMENT_ORDER
                                                        ");
                var listKeyParts = Utility.GetDataTable(ref connOmd, prepareKeyComponentStatement.ToString());

                if (listKeyParts.Rows.Count == 0)
                {
                    LogMetadataEvent("No attributes were found in the metadata, did you reverse-engineer the model?",
                        EventTypes.Information);
                }
                else
                {
                    foreach (DataRow tableName in listKeyParts.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {


                            var keyComponent = tableName["COMPONENT_ELEMENT_VALUE"]; //Handle quotes between SQL and C#
                            keyComponent = keyComponent.ToString().Trim().Replace("'", "''");

                            var businessKeyDefinition = tableName["BUSINESS_KEY_DEFINITION"];
                            businessKeyDefinition = businessKeyDefinition.ToString().Trim().Replace("'", "''");

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_BUSINESS_KEY_COMPONENT_PART]");
                            insertStatement.AppendLine(
                                "(SOURCE_NAME, HUB_NAME, BUSINESS_KEY_DEFINITION, COMPONENT_ID,COMPONENT_ELEMENT_ID,COMPONENT_ELEMENT_ORDER,COMPONENT_ELEMENT_VALUE,COMPONENT_ELEMENT_TYPE,ATTRIBUTE_NAME)");
                            insertStatement.AppendLine("VALUES ('" + tableName["SOURCE_NAME"] + "','" +
                                                       tableName["HUB_NAME"] + "','" + businessKeyDefinition + "','" +
                                                       tableName["COMPONENT_ID"] + "','" +
                                                       tableName["COMPONENT_ELEMENT_ID"] + "','" +
                                                       tableName["COMPONENT_ELEMENT_ORDER"] + "','" + keyComponent +
                                                       "','" + tableName["COMPONENT_ELEMENT_TYPE"] + "','" +
                                                       tableName["ATTRIBUTE_NAME"] + "')");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                keyPartCounter++;
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An issue has occurred during preparation of the business key component metadata: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);
                            }
                        }
                    }
                }

                worker.ReportProgress(60);
                subProcess.Stop();
                LogMetadataEvent("Processing " + keyPartCounter + " Business Key component attributes.",
                    EventTypes.Information);
                LogMetadataEvent(
                    "Preparation of the Business Key components completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);


                #endregion


                #region Hub / Link relationship - 75%

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent("Commencing preparing the relationship between Hubs and Links.",
                    EventTypes.Information);

                var virtualisationSnippet = new StringBuilder();
                if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode)
                {
                    //virtualisationSnippet.AppendLine("SELECT ");
                    //virtualisationSnippet.AppendLine("  OBJECT_SCHEMA_NAME(OBJECT_ID, DB_ID('" + TeamConfigurationSettings.IntegrationDatabaseName + "')) AS LINK_SCHEMA,");
                    //virtualisationSnippet.AppendLine("  OBJECT_NAME(OBJECT_ID,DB_ID('" + TeamConfigurationSettings.IntegrationDatabaseName + "'))  AS LINK_NAME,");
                    //virtualisationSnippet.AppendLine("  [name] AS HUB_TARGET_KEY_NAME_IN_LINK,");
                    //virtualisationSnippet.AppendLine("  ROW_NUMBER() OVER(PARTITION BY OBJECT_NAME(OBJECT_ID,DB_ID('" + TeamConfigurationSettings.IntegrationDatabaseName + "')) ORDER BY column_id) AS LINK_ORDER");
                    //virtualisationSnippet.AppendLine("FROM " + linkedServer + integrationDatabase + @".sys.columns");
                    //virtualisationSnippet.AppendLine("WHERE [column_id] > 1");
                    //virtualisationSnippet.AppendLine("AND OBJECT_NAME(OBJECT_ID,DB_ID('" + TeamConfigurationSettings.IntegrationDatabaseName + "')) LIKE '" + lnkTablePrefix + @"'");
                    //virtualisationSnippet.AppendLine("   AND [name] NOT IN ('" +
                    //                                 TeamConfigurationSettings.RecordSourceAttribute + "','" +
                    //                                 TeamConfigurationSettings.AlternativeRecordSourceAttribute + "','" +
                    //                                 TeamConfigurationSettings.AlternativeLoadDateTimeAttribute + "','" +
                    //                                 TeamConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute + "','" +
                    //                                 TeamConfigurationSettings.EtlProcessAttribute + "','" +
                    //                                 TeamConfigurationSettings.LoadDateTimeAttribute + "" +
                    //                                 "')");

                    // Use the physical model snapshot.
                    virtualisationSnippet.AppendLine("SELECT");
                    virtualisationSnippet.AppendLine("  [SCHEMA_NAME] AS LINK_SCHEMA,");
                    virtualisationSnippet.AppendLine("  [TABLE_NAME]  AS LINK_NAME,");
                    virtualisationSnippet.AppendLine("  [COLUMN_NAME] AS HUB_TARGET_KEY_NAME_IN_LINK,");
                    virtualisationSnippet.AppendLine(
                        "  ROW_NUMBER() OVER(PARTITION BY [TABLE_NAME] ORDER BY ORDINAL_POSITION) AS LINK_ORDER");
                    virtualisationSnippet.AppendLine("FROM MD_PHYSICAL_MODEL");
                    virtualisationSnippet.AppendLine("WHERE [ORDINAL_POSITION] > 1");
                    virtualisationSnippet.AppendLine(" AND TABLE_NAME LIKE '" + lnkTablePrefix + @"'");
                    virtualisationSnippet.AppendLine(" AND COLUMN_NAME NOT IN ('" +
                                                     TeamConfiguration.RecordSourceAttribute + "','" +
                                                     TeamConfiguration.AlternativeRecordSourceAttribute +
                                                     "','" +
                                                     TeamConfiguration.AlternativeLoadDateTimeAttribute +
                                                     "','" +
                                                     TeamConfiguration
                                                         .AlternativeSatelliteLoadDateTimeAttribute + "','" +
                                                     TeamConfiguration.EtlProcessAttribute + "','" +
                                                     TeamConfiguration.LoadDateTimeAttribute +
                                                     "')");
                }
                else
                {
                    virtualisationSnippet.AppendLine("SELECT");
                    virtualisationSnippet.AppendLine("  [SCHEMA_NAME] AS LINK_SCHEMA,");
                    virtualisationSnippet.AppendLine("  [TABLE_NAME]  AS LINK_NAME,");
                    virtualisationSnippet.AppendLine("  [COLUMN_NAME] AS HUB_TARGET_KEY_NAME_IN_LINK,");
                    virtualisationSnippet.AppendLine(
                        "  ROW_NUMBER() OVER(PARTITION BY[TABLE_NAME] ORDER BY ORDINAL_POSITION) AS LINK_ORDER");
                    virtualisationSnippet.AppendLine("FROM TMP_MD_VERSION_ATTRIBUTE");
                    virtualisationSnippet.AppendLine("WHERE [ORDINAL_POSITION] > 1");
                    virtualisationSnippet.AppendLine(" AND TABLE_NAME LIKE '" + lnkTablePrefix + @"'");
                    virtualisationSnippet.AppendLine(" AND COLUMN_NAME NOT IN ('" +
                                                     TeamConfiguration.RecordSourceAttribute + "','" +
                                                     TeamConfiguration.AlternativeRecordSourceAttribute +
                                                     "','" +
                                                     TeamConfiguration.AlternativeLoadDateTimeAttribute +
                                                     "','" +
                                                     TeamConfiguration
                                                         .AlternativeSatelliteLoadDateTimeAttribute + "','" +
                                                     TeamConfiguration.EtlProcessAttribute + "','" +
                                                     TeamConfiguration.LoadDateTimeAttribute +
                                                     "')");
                }



                var prepareHubLnkXrefStatement = new StringBuilder();

                prepareHubLnkXrefStatement.AppendLine("SELECT");
                prepareHubLnkXrefStatement.AppendLine("  hub_tbl.HUB_NAME AS HUB_NAME,");
                prepareHubLnkXrefStatement.AppendLine("  --hub_tbl.[SCHEMA_NAME] AS HUB_SCHEMA,");
                prepareHubLnkXrefStatement.AppendLine("  lnk_tbl.LINK_NAME AS LINK_NAME,");
                prepareHubLnkXrefStatement.AppendLine("  --lnk_tbl.[SCHEMA_NAME] AS LINK_SCHEMA,");
                prepareHubLnkXrefStatement.AppendLine("  lnk_hubkey_order.HUB_KEY_ORDER AS HUB_ORDER,");
                prepareHubLnkXrefStatement.AppendLine("  lnk_target_model.HUB_TARGET_KEY_NAME_IN_LINK");
                prepareHubLnkXrefStatement.AppendLine("FROM");
                prepareHubLnkXrefStatement.AppendLine(
                    "-- This base query adds the Link and its Hubs and their order by pivoting on the full business key");
                prepareHubLnkXrefStatement.AppendLine("(");
                prepareHubLnkXrefStatement.AppendLine("  SELECT");
                prepareHubLnkXrefStatement.AppendLine("    SOURCE_TABLE,");
                prepareHubLnkXrefStatement.AppendLine("    SOURCE_TABLE_SCHEMA,");
                prepareHubLnkXrefStatement.AppendLine("    TARGET_TABLE,");
                prepareHubLnkXrefStatement.AppendLine("    TARGET_TABLE_SCHEMA,");
                prepareHubLnkXrefStatement.AppendLine("    BUSINESS_KEY_ATTRIBUTE,");
                prepareHubLnkXrefStatement.AppendLine(
                    "    LTRIM(Split.a.value('.', 'VARCHAR(4000)')) AS BUSINESS_KEY_PART,");
                prepareHubLnkXrefStatement.AppendLine(
                    "    ROW_NUMBER() OVER(PARTITION BY TARGET_TABLE ORDER BY TARGET_TABLE) AS HUB_KEY_ORDER");
                prepareHubLnkXrefStatement.AppendLine("  FROM");
                prepareHubLnkXrefStatement.AppendLine("  (");
                prepareHubLnkXrefStatement.AppendLine("    SELECT");
                prepareHubLnkXrefStatement.AppendLine("      SOURCE_TABLE,");
                prepareHubLnkXrefStatement.AppendLine("      SOURCE_TABLE_SCHEMA,");
                prepareHubLnkXrefStatement.AppendLine("      TARGET_TABLE,");
                prepareHubLnkXrefStatement.AppendLine("      TARGET_TABLE_SCHEMA,");
                prepareHubLnkXrefStatement.AppendLine(
                    "      ROW_NUMBER() OVER(PARTITION BY TARGET_TABLE, TARGET_TABLE_SCHEMA ORDER BY TARGET_TABLE, TARGET_TABLE_SCHEMA) AS LINK_ORDER,");
                prepareHubLnkXrefStatement.AppendLine(
                    "      BUSINESS_KEY_ATTRIBUTE, CAST('<M>' + REPLACE(BUSINESS_KEY_ATTRIBUTE, ',', '</M><M>') + '</M>' AS XML) AS BUSINESS_KEY_SOURCE_XML");
                prepareHubLnkXrefStatement.AppendLine("    FROM  TMP_MD_TABLE_MAPPING");
                prepareHubLnkXrefStatement.AppendLine("    WHERE [TARGET_TABLE_TYPE] = '" +
                                                      MetadataHandling.TableTypes.NaturalBusinessRelationship + "'");
                prepareHubLnkXrefStatement.AppendLine("    AND [ENABLED_INDICATOR] = 'True'");
                prepareHubLnkXrefStatement.AppendLine(
                    "  ) AS A CROSS APPLY BUSINESS_KEY_SOURCE_XML.nodes('/M') AS Split(a)");
                prepareHubLnkXrefStatement.AppendLine(
                    "  WHERE LINK_ORDER=1 --Any link will do, the order of the Hub keys in the Link will always be the same");
                prepareHubLnkXrefStatement.AppendLine(") lnk_hubkey_order");
                prepareHubLnkXrefStatement.AppendLine(
                    "-- Adding the information required for the target model in the query");
                prepareHubLnkXrefStatement.AppendLine(" JOIN ");
                prepareHubLnkXrefStatement.AppendLine(" (");
                prepareHubLnkXrefStatement.AppendLine(virtualisationSnippet.ToString());
                prepareHubLnkXrefStatement.AppendLine(" ) lnk_target_model");
                prepareHubLnkXrefStatement.AppendLine(
                    " ON lnk_hubkey_order.TARGET_TABLE = lnk_target_model.LINK_NAME AND lnk_hubKey_order.TARGET_TABLE_SCHEMA = lnk_target_model.LINK_SCHEMA COLLATE DATABASE_DEFAULT");
                prepareHubLnkXrefStatement.AppendLine(
                    " AND lnk_hubkey_order.HUB_KEY_ORDER = lnk_target_model.LINK_ORDER");
                prepareHubLnkXrefStatement.AppendLine(" --Adding the Hub mapping data to get the business keys");
                prepareHubLnkXrefStatement.AppendLine(" JOIN TMP_MD_TABLE_MAPPING hub");
                prepareHubLnkXrefStatement.AppendLine(
                    "     ON lnk_hubkey_order.[SOURCE_TABLE] = hub.SOURCE_TABLE AND lnk_hubkey_order.SOURCE_TABLE_SCHEMA = hub.[SOURCE_TABLE_SCHEMA]");
                prepareHubLnkXrefStatement.AppendLine(
                    "     AND lnk_hubkey_order.[BUSINESS_KEY_PART] = hub.BUSINESS_KEY_ATTRIBUTE-- This condition is required to remove the redundant rows caused by the Link key pivoting");
                prepareHubLnkXrefStatement.AppendLine("     AND hub.[TARGET_TABLE_TYPE] = '" +
                                                      MetadataHandling.TableTypes.CoreBusinessConcept + "'");
                prepareHubLnkXrefStatement.AppendLine("     AND hub.[ENABLED_INDICATOR] = 'True'");
                prepareHubLnkXrefStatement.AppendLine(" --Lastly adding the IDs for the Hubs and Links");
                prepareHubLnkXrefStatement.AppendLine(" JOIN dbo.MD_HUB hub_tbl");
                prepareHubLnkXrefStatement.AppendLine(
                    "     ON hub.TARGET_TABLE = hub_tbl.HUB_NAME_SHORT AND hub.TARGET_TABLE_SCHEMA = hub_tbl.[SCHEMA_NAME]");
                prepareHubLnkXrefStatement.AppendLine(" JOIN dbo.MD_LINK lnk_tbl");
                prepareHubLnkXrefStatement.AppendLine(
                    "     ON lnk_hubkey_order.TARGET_TABLE = lnk_tbl.LINK_NAME_SHORT AND lnk_hubkey_order.TARGET_TABLE_SCHEMA = lnk_tbl.[SCHEMA_NAME]");

                var listHlXref = Utility.GetDataTable(ref connOmd, prepareHubLnkXrefStatement.ToString());

                if (listHlXref != null)
                {
                    foreach (DataRow tableName in listHlXref.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            LogMetadataEvent(
                                "Processing the " + tableName["HUB_NAME"] + " to " + tableName["LINK_NAME"] +
                                " relationship.", EventTypes.Information);

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_HUB_LINK_XREF]");
                            insertStatement.AppendLine(
                                "([HUB_NAME], [LINK_NAME], [HUB_ORDER], [HUB_TARGET_KEY_NAME_IN_LINK])");
                            insertStatement.AppendLine("VALUES ('" + tableName["HUB_NAME"] + "','" +
                                                       tableName["LINK_NAME"] + "','" + tableName["HUB_ORDER"] + "','" +
                                                       tableName["HUB_TARGET_KEY_NAME_IN_LINK"] + "')");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An issue has occurred during preparation of the relationship between the Hubs and Links: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);
                            }
                        }
                    }
                }

                worker.ReportProgress(75);
                subProcess.Stop();
                LogMetadataEvent(
                    "Preparation of the relationship between Hubs and Links completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);

                #endregion


                #region Link Business Key

                _alert.SetTextLogging("\r\n");
                LogMetadataEvent("Commencing preparing the Link Business key metadata.", EventTypes.Information);

                // Insert the rest of the rows
                using (var connection = new SqlConnection(metaDataConnection))
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex)
                    {
                        LogMetadataEvent($"An issue has occurred connecting to the database: \r\n\r\n {ex}.",
                            EventTypes.Error);
                    }

                    foreach (var dataObjectTuple in dataObjectList)
                    {
                        if (dataObjectTuple.Item2 == MetadataHandling.TableTypes.NaturalBusinessRelationship)
                        {
                            var fullyQualifiedName = MetadataHandling
                                .GetFullyQualifiedDataObjectName(dataObjectTuple.Item1, dataObjectTuple.Item3)
                                .FirstOrDefault();

                            var businessKeyList = MetadataHandling.GetLinkTargetBusinessKeyList(fullyQualifiedName.Key,
                                fullyQualifiedName.Value, versionId,
                                TeamConfiguration.MetadataConnection.CreateSqlServerConnectionString(false));
                            string businessKey = string.Join(",", businessKeyList);

                            var updateStatement = new StringBuilder();

                            updateStatement.AppendLine("UPDATE [MD_LINK]");
                            updateStatement.AppendLine("SET [BUSINESS_KEY] = '" + businessKey + "'");
                            updateStatement.AppendLine("WHERE [SCHEMA_NAME] =  '" + fullyQualifiedName.Key + "'");
                            updateStatement.AppendLine("AND [LINK_NAME_SHORT] =  '" + fullyQualifiedName.Value + "'");

                            var command = new SqlCommand(updateStatement.ToString(), connection);

                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An issue has occurred during preparation of the Link Business Key: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{updateStatement}",
                                    EventTypes.Error);
                            }
                        }
                    }
                }

                #endregion


                #region Source / Link relationship

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent("Commencing preparing the relationship between Source and Link tables.",
                    EventTypes.Information);

                var preparestgLnkXrefStatement = new StringBuilder();
                preparestgLnkXrefStatement.AppendLine("SELECT");
                preparestgLnkXrefStatement.AppendLine("  lnk_tbl.LINK_NAME,");
                preparestgLnkXrefStatement.AppendLine("  lnk_tbl.[SCHEMA_NAME] AS LINK_SCHEMA,");
                preparestgLnkXrefStatement.AppendLine("  stg_tbl.SOURCE_NAME,");
                preparestgLnkXrefStatement.AppendLine("  stg_tbl.[SCHEMA_NAME] AS SOURCE_SCHEMA,");
                preparestgLnkXrefStatement.AppendLine("  lnk.FILTER_CRITERIA,");
                preparestgLnkXrefStatement.AppendLine("  lnk.BUSINESS_KEY_ATTRIBUTE");
                preparestgLnkXrefStatement.AppendLine("FROM [dbo].[TMP_MD_TABLE_MAPPING] lnk");
                preparestgLnkXrefStatement.AppendLine(
                    "JOIN [dbo].[MD_LINK] lnk_tbl ON lnk.TARGET_TABLE = lnk_tbl.LINK_NAME_SHORT AND lnk.TARGET_TABLE_SCHEMA = lnk_tbl.[SCHEMA_NAME]");
                preparestgLnkXrefStatement.AppendLine(
                    "JOIN [dbo].[MD_SOURCE] stg_tbl ON lnk.SOURCE_TABLE = stg_tbl.SOURCE_NAME_SHORT AND lnk.SOURCE_TABLE_SCHEMA = stg_tbl.[SCHEMA_NAME]");
                preparestgLnkXrefStatement.AppendLine("WHERE lnk.TARGET_TABLE_TYPE = '" +
                                                      MetadataHandling.TableTypes.NaturalBusinessRelationship + "'");
                preparestgLnkXrefStatement.AppendLine("AND[ENABLED_INDICATOR] = 'True'");

                var listStgLinkXref = Utility.GetDataTable(ref connOmd, preparestgLnkXrefStatement.ToString());

                foreach (DataRow tableName in listStgLinkXref.Rows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        LogMetadataEvent(
                            "Processing the " + tableName["SOURCE_NAME"] + " to " + tableName["LINK_NAME"] +
                            " relationship.", EventTypes.Information);

                        var filterCriterion = tableName["FILTER_CRITERIA"].ToString().Trim();
                        filterCriterion = filterCriterion.Replace("'", "''");

                        var businessKeyDefinition = tableName["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                        businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                        var loadVector = MetadataHandling.GetDataObjectMappingLoadVector(
                            tableName["SOURCE_NAME"].ToString(),
                            tableName["LINK_NAME"].ToString(), TeamConfiguration);


                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_SOURCE_LINK_XREF]");
                        insertStatement.AppendLine(
                            "([SOURCE_NAME], [LINK_NAME], [FILTER_CRITERIA], [BUSINESS_KEY_DEFINITION], [LOAD_VECTOR])");
                        insertStatement.AppendLine("VALUES (" +
                                                   "'" + tableName["SOURCE_NAME"] + "'," +
                                                   "'" + tableName["LINK_NAME"] + "'," +
                                                   "'" + filterCriterion + "'," +
                                                   "'" + businessKeyDefinition + "'," +
                                                   "'" + loadVector +
                                                   "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            LogMetadataEvent(
                                $"An issue has occurred during preparation of the relationship between the Hubs and Links: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                EventTypes.Error);
                        }
                    }
                }

                worker.ReportProgress(80);
                subProcess.Stop();
                LogMetadataEvent(
                    $"Preparation of the relationship between Source and the Links has been completed, and has taken {subProcess.Elapsed.TotalSeconds} seconds.",
                    EventTypes.Information);

                #endregion


                #region Manually mapped Source to Staging Area Attribute XREF

                // Prepare the Source to Staging Area XREF
                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent($"Commencing preparing the Source to Staging column-to-column mapping metadata based on the manual mappings.",
                    EventTypes.Information);

                // Getting the distinct list of row from the data table
                selectionRows = inputAttributeMetadata.Select("" + AttributeMappingMetadataColumns.TargetTable + " LIKE '%" + stagingPrefix + "%'");

                if (selectionRows.Length == 0)
                {
                    LogMetadataEvent($"No manual column-to-column mappings for Source-to-Staging were detected.", EventTypes.Information);
                }
                else
                {
                    // Process the unique Staging Area records
                    foreach (var row in selectionRows)
                    {
                        if (localTableEnabledDictionary.TryGetValue(row[AttributeMappingMetadataColumns.TargetTable.ToString()].ToString(), out var enabledValue))
                        {
                            // The key isn't in the dictionary.

                            var fromAttribute = MetadataHandling.QuoteStringValuesForAttributes((string) row[AttributeMappingMetadataColumns.SourceColumn.ToString()]);
                            var toAttribute = MetadataHandling.QuoteStringValuesForAttributes((string) row[AttributeMappingMetadataColumns.TargetColumn.ToString()]);

                            using (var connection = new SqlConnection(metaDataConnection))
                            {
                                var localSourceTable = row[AttributeMappingMetadataColumns.SourceTable.ToString()].ToString();
                                var localTargetTable = row[AttributeMappingMetadataColumns.TargetTable.ToString()].ToString();

                                LogMetadataEvent($"Processing the mapping from  {row[AttributeMappingMetadataColumns.SourceTable.ToString()]} - {fromAttribute} to {row[AttributeMappingMetadataColumns.TargetTable.ToString()]} - {toAttribute}.", EventTypes.Information);

                                // Get the corresponding Data Object.
                                var dataObjectRow = inputTableMetadata.Select($"[{TableMappingMetadataColumns.SourceTable}] = '{localSourceTable}' AND [{TableMappingMetadataColumns.TargetTable}] = '{localTargetTable}'").FirstOrDefault();

                                var sourceConnection = GetTeamConnectionByConnectionId(dataObjectRow[TableMappingMetadataColumns.SourceConnection.ToString()].ToString());
                                var targetConnection = GetTeamConnectionByConnectionId(dataObjectRow[TableMappingMetadataColumns.TargetConnection.ToString()].ToString());

                                var sourceDataObjectFullyQualified = MetadataHandling.GetFullyQualifiedDataObjectName(localSourceTable, sourceConnection).FirstOrDefault();
                                var targetDataObjectFullyQualified = MetadataHandling.GetFullyQualifiedDataObjectName(localTargetTable, targetConnection).FirstOrDefault();



                                var insertStatement = new StringBuilder();
                                insertStatement.AppendLine("INSERT INTO [MD_SOURCE_STAGING_ATTRIBUTE_XREF]");
                                insertStatement.AppendLine("([SOURCE_NAME], [STAGING_NAME], [ATTRIBUTE_NAME_FROM], [ATTRIBUTE_NAME_TO], [MAPPING_TYPE])");
                                insertStatement.AppendLine("VALUES " +
                                                           "(" + 
                                                           $"'{sourceDataObjectFullyQualified.Key}.{sourceDataObjectFullyQualified.Value}'," +
                                                           $"'{targetDataObjectFullyQualified.Key}.{targetDataObjectFullyQualified.Value}'," +
                                                           "'" + fromAttribute + "', " +
                                                           "'" + toAttribute + "', " +
                                                           "'Manual mapping'" +
                                                           ")");

                                var command = new SqlCommand(insertStatement.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    LogMetadataEvent(
                                        $"An issue has occurred during preparation of the Source to Staging Area attribute mapping: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                        EventTypes.Error);
                                }
                            }
                        }
                        else
                        {

                            LogMetadataEvent($"The enabled / disabled state for {row[AttributeMappingMetadataColumns.TargetTable.ToString()]} could not be asserted.", EventTypes.Error);
                        }
                    }
                }

                worker?.ReportProgress(87);
                subProcess.Stop();
                LogMetadataEvent(
                    $"Preparation of the manual column-to-column mappings from Source to Staging has been completed, and has taken {subProcess.Elapsed.TotalSeconds} seconds.",
                    EventTypes.Information);

                #endregion


                #region Automatically mapped Source to Staging Area Attribute XREF

                //Prepare automatic attribute mapping
                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");

                int automaticMappingCounter = 0;

                if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode)
                {
                    LogMetadataEvent(
                        $"Commencing preparing the (automatic) column-to-column mapping metadata for Source to Staging, based on what's available in the database.",
                        EventTypes.Information);

                }
                else
                {
                    LogMetadataEvent(
                        $"Commencing preparing the (automatic) column-to-column mapping metadata for Source to Staging, based on what's available in the physical model metadata.",
                        EventTypes.Information);
                }

                // Run the statement, the virtual vs. physical lookups are embedded in MD_PHYSICAL_MODEL
                var prepareMappingStagingStatement = new StringBuilder();
                prepareMappingStagingStatement.AppendLine("WITH ALL_DATABASE_COLUMNS AS");
                prepareMappingStagingStatement.AppendLine("(");

                prepareMappingStagingStatement.AppendLine("SELECT");
                prepareMappingStagingStatement.AppendLine("  [DATABASE_NAME]");
                prepareMappingStagingStatement.AppendLine(" ,[SCHEMA_NAME]");
                prepareMappingStagingStatement.AppendLine(" ,[TABLE_NAME]");
                prepareMappingStagingStatement.AppendLine(" ,[COLUMN_NAME]");
                prepareMappingStagingStatement.AppendLine(" ,[DATA_TYPE]");
                prepareMappingStagingStatement.AppendLine(" ,[CHARACTER_MAXIMUM_LENGTH]");
                prepareMappingStagingStatement.AppendLine(" ,[NUMERIC_PRECISION]");
                prepareMappingStagingStatement.AppendLine(" ,[NUMERIC_SCALE]");
                prepareMappingStagingStatement.AppendLine(" ,[ORDINAL_POSITION]");
                prepareMappingStagingStatement.AppendLine(" ,[PRIMARY_KEY_INDICATOR]");
                prepareMappingStagingStatement.AppendLine("FROM [MD_PHYSICAL_MODEL]");

                prepareMappingStagingStatement.AppendLine("),");
                prepareMappingStagingStatement.AppendLine("XREF AS");
                prepareMappingStagingStatement.AppendLine("(");
                prepareMappingStagingStatement.AppendLine("  SELECT");
                prepareMappingStagingStatement.AppendLine("    src.[SCHEMA_NAME] AS SOURCE_SCHEMA_NAME,");
                prepareMappingStagingStatement.AppendLine("    src.SOURCE_NAME AS SOURCE_NAME_FULL,");
                prepareMappingStagingStatement.AppendLine("    src.SOURCE_NAME_SHORT AS SOURCE_NAME,");
                prepareMappingStagingStatement.AppendLine("    tgt.[SCHEMA_NAME] AS TARGET_SCHEMA_NAME,");
                prepareMappingStagingStatement.AppendLine("    tgt.STAGING_NAME AS STAGING_NAME_FULL,");
                prepareMappingStagingStatement.AppendLine("    tgt.STAGING_NAME_SHORT AS STAGING_NAME");
                prepareMappingStagingStatement.AppendLine("  FROM MD_SOURCE_STAGING_XREF xref");
                prepareMappingStagingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_SOURCE src ON xref.SOURCE_NAME = src.SOURCE_NAME");
                prepareMappingStagingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_STAGING tgt ON xref.STAGING_NAME = tgt.STAGING_NAME");
                prepareMappingStagingStatement.AppendLine(") ");
                prepareMappingStagingStatement.AppendLine("SELECT");
                prepareMappingStagingStatement.AppendLine("  XREF.SOURCE_NAME_FULL AS SOURCE_NAME, ");
                prepareMappingStagingStatement.AppendLine("  XREF.STAGING_NAME_FULL AS STAGING_NAME,");
                prepareMappingStagingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_NAME_FROM,");
                prepareMappingStagingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_NAME_TO,");
                prepareMappingStagingStatement.AppendLine("  'automatically mapped' as VERIFICATION");
                prepareMappingStagingStatement.AppendLine("FROM XREF");
                prepareMappingStagingStatement.AppendLine(
                    "JOIN ALL_DATABASE_COLUMNS ADC_TARGET ON XREF.TARGET_SCHEMA_NAME = ADC_TARGET.[SCHEMA_NAME] AND XREF.STAGING_NAME = ADC_TARGET.TABLE_NAME");
                prepareMappingStagingStatement.AppendLine(
                    "JOIN dbo.MD_ATTRIBUTE tgt_attr ON ADC_TARGET.COLUMN_NAME = tgt_attr.ATTRIBUTE_NAME COLLATE Latin1_General_CS_AS");
                prepareMappingStagingStatement.AppendLine("WHERE NOT EXISTS (");
                prepareMappingStagingStatement.AppendLine("  SELECT SOURCE_NAME, STAGING_NAME, ATTRIBUTE_NAME_TO");
                prepareMappingStagingStatement.AppendLine("  FROM MD_SOURCE_STAGING_ATTRIBUTE_XREF manualmapping");
                prepareMappingStagingStatement.AppendLine("WHERE");
                prepareMappingStagingStatement.AppendLine("      manualmapping.SOURCE_NAME = XREF.SOURCE_NAME_FULL");
                prepareMappingStagingStatement.AppendLine("  AND manualmapping.STAGING_NAME = XREF.STAGING_NAME_FULL");
                prepareMappingStagingStatement.AppendLine(
                    "  AND manualmapping.ATTRIBUTE_NAME_TO = ADC_TARGET.COLUMN_NAME COLLATE Latin1_General_CS_AS");
                prepareMappingStagingStatement.AppendLine(")");
                prepareMappingStagingStatement.AppendLine("ORDER BY SOURCE_NAME");


                var automaticAttributeMappings =
                    Utility.GetDataTable(ref connOmd, prepareMappingStagingStatement.ToString());

                if (automaticAttributeMappings.Rows.Count == 0)
                {
                    LogMetadataEvent(
                        $"No automatic column-to-column mappings between source and staging were detected..",
                        EventTypes.Information);
                }
                else
                {
                    // Process the unique attribute mappings
                    foreach (DataRow row in automaticAttributeMappings.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            LogMetadataEvent("Processing the mapping from " + (string) row["SOURCE_NAME"] +
                                             " - " + (string) row["ATTRIBUTE_NAME_FROM"] + " to " +
                                             (string) row["STAGING_NAME"] + " - " +
                                             (string) row["ATTRIBUTE_NAME_TO"] + ".", EventTypes.Information);

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_SOURCE_STAGING_ATTRIBUTE_XREF]");
                            insertStatement.AppendLine(
                                "([SOURCE_NAME], [STAGING_NAME], [ATTRIBUTE_NAME_FROM], [ATTRIBUTE_NAME_TO], [MAPPING_TYPE])");
                            insertStatement.AppendLine("VALUES (" +
                                                       "'" + (string) row["SOURCE_NAME"] + "', " +
                                                       "'" + (string) row["STAGING_NAME"] + "', " +
                                                       "'" + (string) row["ATTRIBUTE_NAME_FROM"] + "', " +
                                                       "'" + (string) row["ATTRIBUTE_NAME_TO"] + "', " +
                                                       "'Automatic mapping'" +
                                                       ")");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                automaticMappingCounter++;
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An issue has occurred during preparation of the Source to Staging Area attribute mapping: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);
                            }
                        }
                    }
                }

                worker.ReportProgress(90);
                subProcess.Stop();
                LogMetadataEvent($"Processed {automaticMappingCounter} automatically added attribute mappings.",
                    EventTypes.Information);
                LogMetadataEvent(
                    $"Preparation of the automatically mapped column-to-column metadata completed, and has taken {subProcess.Elapsed.TotalSeconds} seconds.",
                    EventTypes.Information);

                #endregion


                #region Manually mapped Source to Persistent Staging Area Attribute XREF

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent("Commencing preparing the Source to Persistent Staging column-to-column mapping metadata based on the manual mappings.", EventTypes.Information);

                // Getting the distinct list of row from the data table
                selectionRows = inputAttributeMetadata.Select("" + AttributeMappingMetadataColumns.TargetTable + " LIKE '%" + psaPrefix + "%'");

                if (selectionRows.Length == 0)
                {
                    LogMetadataEvent("No manual column-to-column mappings for Source to Persistent Staging were detected.", EventTypes.Information);
                }
                else
                {
                    // Process the unique Persistent Staging Area records
                    foreach (var row in selectionRows)
                    {
                        // Only process rows whose parent is enabled
                        if (localTableEnabledDictionary.TryGetValue(row[AttributeMappingMetadataColumns.TargetTable.ToString()].ToString(), out var enabledValue) == true)
                        {

                            using (var connection = new SqlConnection(metaDataConnection))
                            {
                                var localSourceTable = row[AttributeMappingMetadataColumns.SourceTable.ToString()].ToString();
                                var localTargetTable = row[AttributeMappingMetadataColumns.TargetTable.ToString()].ToString();

                                LogMetadataEvent($"Processing the mapping from {row[AttributeMappingMetadataColumns.SourceTable.ToString()]} - {(string) row[AttributeMappingMetadataColumns.SourceColumn.ToString()]} to {row[AttributeMappingMetadataColumns.TargetTable.ToString()]} - {(string) row[AttributeMappingMetadataColumns.TargetColumn.ToString()]}.", EventTypes.Information);

                                //var localTableName = MetadataHandling.GetNonQualifiedTableName(row[TableMetadataColumns.TargetTable.ToString()].ToString());

                                // selectionRows = inputTableMetadata.Select("" + AttributeMappingMetadataColumns.TargetTable + " LIKE '%" + psaPrefix + "%'");
                                
                                // Get the corresponding Data Object.
                                var dataObjectRow = inputTableMetadata.Select($"[{TableMappingMetadataColumns.SourceTable}] = '{localSourceTable}' AND [{TableMappingMetadataColumns.TargetTable}] = '{localTargetTable}'").FirstOrDefault();

                                var sourceConnection = GetTeamConnectionByConnectionId(dataObjectRow[TableMappingMetadataColumns.SourceConnection.ToString()].ToString());
                                var targetConnection = GetTeamConnectionByConnectionId(dataObjectRow[TableMappingMetadataColumns.TargetConnection.ToString()].ToString());

                                var sourceDataObjectFullyQualified = MetadataHandling.GetFullyQualifiedDataObjectName(localSourceTable, sourceConnection).FirstOrDefault();
                                var targetDataObjectFullyQualified = MetadataHandling.GetFullyQualifiedDataObjectName(localTargetTable, targetConnection).FirstOrDefault();


                                var insertStatement = new StringBuilder();
                                insertStatement.AppendLine("INSERT INTO [MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF]");
                                insertStatement.AppendLine("([SOURCE_NAME], [PERSISTENT_STAGING_NAME], [ATTRIBUTE_NAME_FROM], [ATTRIBUTE_NAME_TO], [MAPPING_TYPE])");
                                insertStatement.AppendLine($"VALUES " +
                                                           $"(" +
                                                           $" '{sourceDataObjectFullyQualified.Key}.{sourceDataObjectFullyQualified.Value}'," +
                                                           $" '{targetDataObjectFullyQualified.Key}.{targetDataObjectFullyQualified.Value}'," +
                                                           $" '{row[AttributeMappingMetadataColumns.SourceColumn.ToString()]}'," +
                                                           $" '{row[AttributeMappingMetadataColumns.TargetColumn.ToString()]}', " +
                                                           $" 'Manual mapping'" +
                                                           $")");

                                var command = new SqlCommand(insertStatement.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    LogMetadataEvent(
                                        $"An issue has occurred during preparation of the Source to Persistent Staging Area attribute mapping: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                        EventTypes.Error);
                                }
                            }
                        }
                        else
                        {
                            var test = localTableEnabledDictionary;
                            var test2 = row[AttributeMappingMetadataColumns.TargetTable.ToString()];
                            LogMetadataEvent($"The enabled / disabled state for {row[AttributeMappingMetadataColumns.TargetTable.ToString()]} could not be asserted.", EventTypes.Error);
                        }
                    }
                }

                worker?.ReportProgress(87);
                subProcess.Stop();
                LogMetadataEvent(
                    "Preparation of the manual column-to-column mappings for Source-to-Staging completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);

                #endregion


                #region Automatically mapped Source to Persistent Staging Area Attribute XREF

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                var prepareMappingPersistentStagingStatement = new StringBuilder();

                automaticMappingCounter = 0;

                LogMetadataEvent(GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode
                        ? "Commencing preparing the (automatic) column-to-column mapping metadata for Source to Persistent Staging, based on what's available in the database."
                        : "Commencing preparing the (automatic) column-to-column mapping metadata for Source to Persistent Staging, based on what's available in the physical model metadata.",
                    EventTypes.Information);

                prepareMappingPersistentStagingStatement.AppendLine("WITH ALL_DATABASE_COLUMNS AS");
                prepareMappingPersistentStagingStatement.AppendLine("(");
                prepareMappingPersistentStagingStatement.AppendLine("SELECT");
                prepareMappingPersistentStagingStatement.AppendLine("  [DATABASE_NAME]");
                prepareMappingPersistentStagingStatement.AppendLine(" ,[SCHEMA_NAME]");
                prepareMappingPersistentStagingStatement.AppendLine(" ,[TABLE_NAME]");
                prepareMappingPersistentStagingStatement.AppendLine(" ,[COLUMN_NAME]");
                prepareMappingPersistentStagingStatement.AppendLine(" ,[DATA_TYPE]");
                prepareMappingPersistentStagingStatement.AppendLine(" ,[CHARACTER_MAXIMUM_LENGTH]");
                prepareMappingPersistentStagingStatement.AppendLine(" ,[NUMERIC_PRECISION]");
                prepareMappingPersistentStagingStatement.AppendLine(" ,[NUMERIC_SCALE]");
                prepareMappingPersistentStagingStatement.AppendLine(" ,[ORDINAL_POSITION]");
                prepareMappingPersistentStagingStatement.AppendLine(" ,[PRIMARY_KEY_INDICATOR]");
                prepareMappingPersistentStagingStatement.AppendLine("FROM [MD_PHYSICAL_MODEL]");
                prepareMappingPersistentStagingStatement.AppendLine("),");
                prepareMappingPersistentStagingStatement.AppendLine("XREF AS");
                prepareMappingPersistentStagingStatement.AppendLine("(");
                prepareMappingPersistentStagingStatement.AppendLine("  SELECT");
                prepareMappingPersistentStagingStatement.AppendLine("    xref.*,");
                prepareMappingPersistentStagingStatement.AppendLine("    tgt.PERSISTENT_STAGING_NAME_SHORT,");
                prepareMappingPersistentStagingStatement.AppendLine("    src.[SCHEMA_NAME] AS SOURCE_SCHEMA_NAME,");
                prepareMappingPersistentStagingStatement.AppendLine("    tgt.[SCHEMA_NAME] AS TARGET_SCHEMA_NAME");
                prepareMappingPersistentStagingStatement.AppendLine("  FROM MD_SOURCE_PERSISTENT_STAGING_XREF xref");
                prepareMappingPersistentStagingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_SOURCE src ON xref.SOURCE_NAME = src.SOURCE_NAME");
                prepareMappingPersistentStagingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_PERSISTENT_STAGING tgt ON xref.PERSISTENT_STAGING_NAME = tgt.PERSISTENT_STAGING_NAME");
                prepareMappingPersistentStagingStatement.AppendLine(") ");
                prepareMappingPersistentStagingStatement.AppendLine("SELECT");
                prepareMappingPersistentStagingStatement.AppendLine("  XREF.SOURCE_NAME, ");
                prepareMappingPersistentStagingStatement.AppendLine("  XREF.PERSISTENT_STAGING_NAME,");
                prepareMappingPersistentStagingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_NAME_FROM,");
                prepareMappingPersistentStagingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_NAME_TO,");
                prepareMappingPersistentStagingStatement.AppendLine("  'automatically mapped' as VERIFICATION");
                prepareMappingPersistentStagingStatement.AppendLine("FROM XREF");
                prepareMappingPersistentStagingStatement.AppendLine(
                    "JOIN ALL_DATABASE_COLUMNS ADC_TARGET ON XREF.TARGET_SCHEMA_NAME = ADC_TARGET.[SCHEMA_NAME] AND XREF.PERSISTENT_STAGING_NAME_SHORT = ADC_TARGET.TABLE_NAME");
                prepareMappingPersistentStagingStatement.AppendLine(
                    "JOIN dbo.MD_ATTRIBUTE tgt_attr ON ADC_TARGET.COLUMN_NAME = tgt_attr.ATTRIBUTE_NAME COLLATE Latin1_General_CS_AS");
                prepareMappingPersistentStagingStatement.AppendLine("WHERE NOT EXISTS (");
                prepareMappingPersistentStagingStatement.AppendLine(
                    "  SELECT SOURCE_NAME, PERSISTENT_STAGING_NAME, ATTRIBUTE_NAME_TO");
                prepareMappingPersistentStagingStatement.AppendLine(
                    "  FROM MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF manualmapping");
                prepareMappingPersistentStagingStatement.AppendLine("  WHERE");
                prepareMappingPersistentStagingStatement.AppendLine(
                    "      manualmapping.SOURCE_NAME = XREF.SOURCE_NAME");
                prepareMappingPersistentStagingStatement.AppendLine(
                    "  AND manualmapping.PERSISTENT_STAGING_NAME = XREF.PERSISTENT_STAGING_NAME");
                prepareMappingPersistentStagingStatement.AppendLine(
                    "  AND manualmapping.ATTRIBUTE_NAME_TO = ADC_TARGET.COLUMN_NAME COLLATE Latin1_General_CS_AS");
                prepareMappingPersistentStagingStatement.AppendLine(
                    "  AND manualmapping.MAPPING_TYPE = 'Manual mapping'");
                prepareMappingPersistentStagingStatement.AppendLine(")");
                prepareMappingPersistentStagingStatement.AppendLine("ORDER BY SOURCE_NAME");

                var automaticAttributeMappingsPsa =
                    Utility.GetDataTable(ref connOmd, prepareMappingPersistentStagingStatement.ToString());

                if (automaticAttributeMappingsPsa.Rows.Count == 0)
                {
                    LogMetadataEvent("--> No automatic column-to-column mappings were detected.",
                        EventTypes.Information);
                }
                else
                {
                    // Process the unique attribute mappings
                    foreach (DataRow row in automaticAttributeMappingsPsa.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            LogMetadataEvent("Processing the mapping from " + (string) row["SOURCE_NAME"] +
                                             " - " + (string) row["ATTRIBUTE_NAME_FROM"] + " to " +
                                             (string) row["PERSISTENT_STAGING_NAME"] + " - " +
                                             (string) row["ATTRIBUTE_NAME_TO"] + ".", EventTypes.Information);

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF]");
                            insertStatement.AppendLine(
                                "([SOURCE_NAME], [PERSISTENT_STAGING_NAME], [ATTRIBUTE_NAME_FROM], [ATTRIBUTE_NAME_TO], [MAPPING_TYPE])");
                            insertStatement.AppendLine("VALUES (" +
                                                       "'" + (string) row["SOURCE_NAME"] + "'," +
                                                       "'" + (string) row["PERSISTENT_STAGING_NAME"] + "', " +
                                                       "'" + (string) row["ATTRIBUTE_NAME_FROM"] + "', " +
                                                       "'" + (string) row["ATTRIBUTE_NAME_TO"] + "', " +
                                                       "'Automatic mapping'" +
                                                       ")");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                automaticMappingCounter++;
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An issue has occurred during preparation of the Source to Persistent Staging Area attribute mapping: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);
                            }
                        }
                    }
                }

                worker.ReportProgress(90);
                subProcess.Stop();
                LogMetadataEvent("Processing " + automaticMappingCounter + " automatically added attribute mappings.",
                    EventTypes.Information);
                LogMetadataEvent(
                    "Preparation of the automatically mapped column-to-column metadata completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);


                #endregion


                #region Manually mapped attributes for SAT and LSAT

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent(
                    "Commencing preparing the Satellite and Link-Satellite column-to-column mapping metadata based on the manual mappings.",
                    EventTypes.Information);

                int manualSatMappingCounter = 0;

                var prepareMappingStatementManual = new StringBuilder();
                prepareMappingStatementManual.AppendLine("SELECT");
                prepareMappingStatementManual.AppendLine("   stg.SOURCE_NAME");
                prepareMappingStatementManual.AppendLine("  ,sat.SATELLITE_NAME");
                prepareMappingStatementManual.AppendLine("  ,stg_attr.ATTRIBUTE_NAME AS ATTRIBUTE_NAME_FROM");
                prepareMappingStatementManual.AppendLine("  ,target_attr.ATTRIBUTE_NAME AS ATTRIBUTE_NAME_TO");
                prepareMappingStatementManual.AppendLine("  ,'N' as MULTI_ACTIVE_KEY_INDICATOR");
                prepareMappingStatementManual.AppendLine("  ,'manually_mapped' as VERIFICATION");
                prepareMappingStatementManual.AppendLine("FROM dbo.TMP_MD_ATTRIBUTE_MAPPING mapping");
                prepareMappingStatementManual.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_SATELLITE sat on sat.SATELLITE_NAME_SHORT = mapping.TARGET_TABLE AND sat.[SCHEMA_NAME] = mapping.TARGET_TABLE_SCHEMA");
                prepareMappingStatementManual.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_ATTRIBUTE target_attr on mapping.TARGET_COLUMN = target_attr.ATTRIBUTE_NAME COLLATE Latin1_General_CS_AS");
                prepareMappingStatementManual.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_SOURCE stg on stg.SOURCE_NAME_SHORT = mapping.SOURCE_TABLE AND stg.[SCHEMA_NAME] = mapping.SOURCE_TABLE_SCHEMA");
                prepareMappingStatementManual.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_ATTRIBUTE stg_attr on mapping.SOURCE_COLUMN = stg_attr.ATTRIBUTE_NAME COLLATE Latin1_General_CS_AS");
                prepareMappingStatementManual.AppendLine("LEFT OUTER JOIN dbo.TMP_MD_TABLE_MAPPING table_mapping");
                prepareMappingStatementManual.AppendLine("    ON mapping.TARGET_TABLE = table_mapping.TARGET_TABLE");
                prepareMappingStatementManual.AppendLine("AND mapping.SOURCE_TABLE = table_mapping.SOURCE_TABLE");
                prepareMappingStatementManual.AppendLine("WHERE mapping.TARGET_TABLE_TYPE IN ('" +
                                                         MetadataHandling.TableTypes.Context + "', '" +
                                                         MetadataHandling.TableTypes
                                                             .NaturalBusinessRelationshipContext + "')");
                prepareMappingStatementManual.AppendLine("   AND table_mapping.[ENABLED_INDICATOR] = 'True' ");


                var attributeMappingsSatellites =
                    Utility.GetDataTable(ref connOmd, prepareMappingStatementManual.ToString());

                if (attributeMappingsSatellites is null)
                {
                    LogMetadataEvent(
                        "There was an issue retrieving automatic attribute metadata from the repository. The query is: \r\n\r\n" +
                        prepareMappingStatementManual, EventTypes.Error);
                }

                try
                {
                    if (attributeMappingsSatellites is null || attributeMappingsSatellites.Rows.Count == 0)
                    {
                        LogMetadataEvent(
                            "No information on Satellite attribute mappings was retrieved from the repository.",
                            EventTypes.Warning);
                    }
                    else
                    {
                        foreach (DataRow row in attributeMappingsSatellites.Rows)
                        {
                            using (var connection = new SqlConnection(metaDataConnection))
                            {
                                var insertStatement = new StringBuilder();
                                insertStatement.AppendLine("INSERT INTO [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]");
                                insertStatement.AppendLine(
                                    "( [SOURCE_NAME],[SATELLITE_NAME],[ATTRIBUTE_NAME_FROM],[ATTRIBUTE_NAME_TO],[MULTI_ACTIVE_KEY_INDICATOR], [MAPPING_TYPE])");
                                insertStatement.AppendLine("VALUES (" +
                                                           "'" + row["SOURCE_NAME"] + "', " +
                                                           "'" + row["SATELLITE_NAME"] + "', " +
                                                           "'" + row["ATTRIBUTE_NAME_FROM"] + "', " +
                                                           "'" + row["ATTRIBUTE_NAME_TO"] + "', " +
                                                           "'" + row["MULTI_ACTIVE_KEY_INDICATOR"] + "'," +
                                                           "'Manual mapping'" +
                                                           ")");

                                try
                                {

                                    var command = new SqlCommand(insertStatement.ToString(), connection);
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                    LogMetadataEvent("Processing the mapping from " + (string) row["SOURCE_NAME"] +
                                                     " - " + (string) row["ATTRIBUTE_NAME_FROM"] + " to " +
                                                     (string) row["SATELLITE_NAME"] + " - " +
                                                     (string) row["ATTRIBUTE_NAME_TO"] + ".", EventTypes.Information);

                                    manualSatMappingCounter++;

                                }
                                catch (Exception ex)
                                {
                                    LogMetadataEvent(
                                        $"An occurred during the preparation of Source to Satellite attribute mappings for {row["SOURCE_NAME"]} to {row["SATELLITE_NAME"]}. The error is: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                        EventTypes.Error);

                                    if (row["ATTRIBUTE_NAME_FROM"].ToString() == "")
                                    {
                                        LogMetadataEvent("Both attributes are NULL.", EventTypes.Error);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMetadataEvent($"An exception was encountered: {ex}.", EventTypes.Error);
                }

                worker.ReportProgress(90);
                subProcess.Stop();
                LogMetadataEvent($"Processing {manualSatMappingCounter}  manual attribute mappings.",
                    EventTypes.Information);
                LogMetadataEvent(
                    "Preparation of the manual column-to-column mapping for Satellites and Link-Satellites completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);

                #endregion


                #region Automatically mapped attributes for SAT and LSAT

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");

                var prepareMappingStatement = new StringBuilder();

                if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode)
                {
                    LogMetadataEvent(
                        "Commencing preparing the (automatic) column-to-column mapping metadata for Satellites and Link-Satellites, based on what's available in the database.",
                        EventTypes.Information);
                }
                else
                {
                    LogMetadataEvent(
                        "Commencing preparing the (automatic) column-to-column mapping metadata for Satellites and Link-Satellites, based on what's available in the physical model metadata.",
                        EventTypes.Information);
                }

                // Run the statement, the virtual vs. physical lookups are embedded in allDatabaseAttributes
                prepareMappingStatement.AppendLine("WITH ALL_DATABASE_COLUMNS AS");
                prepareMappingStatement.AppendLine("(");

                prepareMappingStatement.AppendLine("SELECT");
                prepareMappingStatement.AppendLine("  [DATABASE_NAME]");
                prepareMappingStatement.AppendLine(" ,[SCHEMA_NAME]");
                prepareMappingStatement.AppendLine(" ,[TABLE_NAME]");
                prepareMappingStatement.AppendLine(" ,[COLUMN_NAME]");
                prepareMappingStatement.AppendLine(" ,[DATA_TYPE]");
                prepareMappingStatement.AppendLine(" ,[CHARACTER_MAXIMUM_LENGTH]");
                prepareMappingStatement.AppendLine(" ,[NUMERIC_PRECISION]");
                prepareMappingStatement.AppendLine(" ,[NUMERIC_SCALE]");
                prepareMappingStatement.AppendLine(" ,[ORDINAL_POSITION]");
                prepareMappingStatement.AppendLine(" ,[PRIMARY_KEY_INDICATOR]");
                prepareMappingStatement.AppendLine("FROM [MD_PHYSICAL_MODEL]");

                prepareMappingStatement.AppendLine("),");
                prepareMappingStatement.AppendLine("XREF AS");
                prepareMappingStatement.AppendLine("(");
                prepareMappingStatement.AppendLine("  SELECT");
                prepareMappingStatement.AppendLine("    src.[SCHEMA_NAME] AS SOURCE_SCHEMA_NAME,");
                prepareMappingStatement.AppendLine("    src.[SOURCE_NAME] AS SOURCE_NAME,");
                prepareMappingStatement.AppendLine("    tgt.[SCHEMA_NAME] AS TARGET_SCHEMA_NAME,");
                prepareMappingStatement.AppendLine("    tgt.SATELLITE_NAME AS TARGET_NAME");
                prepareMappingStatement.AppendLine("  FROM MD_SOURCE_SATELLITE_XREF xref");
                prepareMappingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_SOURCE src ON xref.SOURCE_NAME = src.SOURCE_NAME");
                prepareMappingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_SATELLITE tgt ON xref.SATELLITE_NAME = tgt.SATELLITE_NAME");
                prepareMappingStatement.AppendLine(")");
                prepareMappingStatement.AppendLine("SELECT");
                prepareMappingStatement.AppendLine("  XREF.SOURCE_NAME, ");
                prepareMappingStatement.AppendLine("  XREF.TARGET_NAME AS SATELLITE_NAME,");
                prepareMappingStatement.AppendLine("  ADC_SOURCE.COLUMN_NAME AS ATTRIBUTE_NAME_FROM,");
                prepareMappingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_NAME_TO,");
                prepareMappingStatement.AppendLine("  'N' AS MULTI_ACTIVE_KEY_INDICATOR,");
                prepareMappingStatement.AppendLine("  'automatically mapped' as VERIFICATION");
                prepareMappingStatement.AppendLine("FROM XREF");
                prepareMappingStatement.AppendLine(
                    "JOIN ALL_DATABASE_COLUMNS ADC_SOURCE ON XREF.SOURCE_SCHEMA_NAME = ADC_SOURCE.[SCHEMA_NAME] AND XREF.SOURCE_NAME = ADC_SOURCE.TABLE_NAME");
                prepareMappingStatement.AppendLine(
                    "JOIN ALL_DATABASE_COLUMNS ADC_TARGET ON XREF.TARGET_SCHEMA_NAME = ADC_TARGET.[SCHEMA_NAME] AND XREF.TARGET_NAME = ADC_TARGET.TABLE_NAME");
                prepareMappingStatement.AppendLine(
                    "JOIN dbo.MD_ATTRIBUTE stg_attr ON ADC_SOURCE.COLUMN_NAME = stg_attr.ATTRIBUTE_NAME COLLATE Latin1_General_CS_AS");
                prepareMappingStatement.AppendLine(
                    "JOIN dbo.MD_ATTRIBUTE tgt_attr ON ADC_TARGET.COLUMN_NAME = tgt_attr.ATTRIBUTE_NAME COLLATE Latin1_General_CS_AS");
                prepareMappingStatement.AppendLine(
                    "WHERE UPPER(stg_attr.ATTRIBUTE_NAME) = UPPER(tgt_attr.ATTRIBUTE_NAME)");
                prepareMappingStatement.AppendLine("AND NOT EXISTS (");
                prepareMappingStatement.AppendLine("  SELECT SOURCE_NAME, SATELLITE_NAME, ATTRIBUTE_NAME_TO");
                prepareMappingStatement.AppendLine("  FROM MD_SOURCE_SATELLITE_ATTRIBUTE_XREF manualmapping");
                prepareMappingStatement.AppendLine("  WHERE");
                prepareMappingStatement.AppendLine("      manualmapping.SOURCE_NAME = XREF.SOURCE_NAME");
                prepareMappingStatement.AppendLine("  AND manualmapping.SATELLITE_NAME = XREF.TARGET_NAME");
                prepareMappingStatement.AppendLine(
                    "  AND manualmapping.ATTRIBUTE_NAME_TO = ADC_TARGET.COLUMN_NAME COLLATE Latin1_General_CS_AS");
                prepareMappingStatement.AppendLine(")");
                prepareMappingStatement.AppendLine("ORDER BY SOURCE_NAME");

                var automaticAttributeMappingsSatellites =
                    Utility.GetDataTable(ref connOmd, prepareMappingStatement.ToString());
                int attCounterSatellite = 0;

                if (automaticAttributeMappingsSatellites is null)
                {
                    LogMetadataEvent(
                        "There was an issue retrieving automatic attribute metadata from the repository. The query is: \r\n\r\n" +
                        prepareMappingStatement, EventTypes.Error);
                }

                if (automaticAttributeMappingsSatellites is null ||
                    automaticAttributeMappingsSatellites.Rows.Count == 0)
                {
                    LogMetadataEvent(
                        "No automatic column-to-column mappings were able to be retrieved from the repository.",
                        EventTypes.Warning);
                }
                else
                {

                    foreach (DataRow row in automaticAttributeMappingsSatellites.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            LogMetadataEvent("Processing the mapping from " + (string) row["SOURCE_NAME"] +
                                             " - " + (string) row["ATTRIBUTE_NAME_FROM"] + " to " +
                                             (string) row["SATELLITE_NAME"] + " - " +
                                             (string) row["ATTRIBUTE_NAME_TO"] + ".", EventTypes.Information);

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]");
                            insertStatement.AppendLine(
                                "( [SOURCE_NAME],[SATELLITE_NAME],[ATTRIBUTE_NAME_FROM],[ATTRIBUTE_NAME_TO],[MULTI_ACTIVE_KEY_INDICATOR], [MAPPING_TYPE])");
                            insertStatement.AppendLine("VALUES (" +
                                                       "'" + row["SOURCE_NAME"] + "', " +
                                                       "'" + row["SATELLITE_NAME"] + "', " +
                                                       "'" + row["ATTRIBUTE_NAME_FROM"] + "', " +
                                                       "'" + row["ATTRIBUTE_NAME_TO"] + "', " +
                                                       "'" + row["MULTI_ACTIVE_KEY_INDICATOR"] + "'," +
                                                       "'Automatic mapping'" +
                                                       ")");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                automaticMappingCounter++;

                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An occurred during the preparation of Source to Satellite attribute mappings for {row["SOURCE_NAME"]} to {row["SATELLITE_NAME"]}. The error is: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);

                                if (row["ATTRIBUTE_NAME_FROM"].ToString() == "")
                                {
                                    LogMetadataEvent("Both attributes are NULL.", EventTypes.Error);
                                }
                            }
                        }

                        attCounterSatellite++;
                    }
                }

                worker.ReportProgress(90);
                subProcess.Stop();
                LogMetadataEvent("Processing " + attCounterSatellite + " automatically added attribute mappings.",
                    EventTypes.Information);
                LogMetadataEvent(
                    "Preparation of the automatically mapped column-to-column metadata completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);

                #endregion


                #region Manually mapped degenerate attributes for Links

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent(
                    "Commencing preparing the column-to-column mapping metadata based on the manual mappings for degenerate attributes.",
                    EventTypes.Information);

                var prepareMappingStatementLink = new StringBuilder();

                prepareMappingStatementLink.AppendLine("SELECT");
                prepareMappingStatementLink.AppendLine("  stg.SOURCE_NAME");
                prepareMappingStatementLink.AppendLine(" ,lnk.LINK_NAME");
                prepareMappingStatementLink.AppendLine(" ,stg_attr.ATTRIBUTE_NAME AS ATTRIBUTE_NAME_FROM");
                prepareMappingStatementLink.AppendLine(" ,target_attr.ATTRIBUTE_NAME AS ATTRIBUTE_NAME_TO");
                prepareMappingStatementLink.AppendLine(" ,'Manual mapping' as MAPPING_TYPE");
                prepareMappingStatementLink.AppendLine("FROM dbo.TMP_MD_ATTRIBUTE_MAPPING mapping");
                prepareMappingStatementLink.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_LINK lnk on lnk.LINK_NAME_SHORT = mapping.TARGET_TABLE AND lnk.SCHEMA_NAME = mapping.TARGET_TABLE_SCHEMA");
                prepareMappingStatementLink.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_ATTRIBUTE target_attr on mapping.TARGET_COLUMN = target_attr.ATTRIBUTE_NAME COLLATE Latin1_General_CS_AS");
                prepareMappingStatementLink.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_SOURCE stg on stg.SOURCE_NAME_SHORT = mapping.SOURCE_TABLE AND stg.SCHEMA_NAME = mapping.SOURCE_TABLE_SCHEMA");
                prepareMappingStatementLink.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_ATTRIBUTE stg_attr on mapping.SOURCE_COLUMN = stg_attr.ATTRIBUTE_NAME COLLATE Latin1_General_CS_AS");
                prepareMappingStatementLink.AppendLine("LEFT OUTER JOIN dbo.TMP_MD_TABLE_MAPPING table_mapping");
                prepareMappingStatementLink.AppendLine("  ON mapping.TARGET_TABLE = table_mapping.TARGET_TABLE");
                prepareMappingStatementLink.AppendLine(" AND mapping.SOURCE_TABLE = table_mapping.SOURCE_TABLE");
                prepareMappingStatementLink.AppendLine("WHERE mapping.TARGET_TABLE_TYPE = ('" +
                                                       MetadataHandling.TableTypes.NaturalBusinessRelationship + "')");
                prepareMappingStatementLink.AppendLine("      AND table_mapping.[ENABLED_INDICATOR] = 'True'");

                var degenerateMappings = Utility.GetDataTable(ref connOmd, prepareMappingStatementLink.ToString());

                if (degenerateMappings.Rows.Count == 0)
                {
                    LogMetadataEvent("--> No manually mapped degenerate columns were detected.",
                        EventTypes.Information);
                }

                worker.ReportProgress(95);
                subProcess.Stop();
                LogMetadataEvent("Processing " + degenerateMappings.Rows.Count +
                                 " manual degenerate attribute mappings.", EventTypes.Information);
                LogMetadataEvent("Preparation of the degenerate column metadata completed, and has taken " +
                                 subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);

                #endregion


                #region Automatically mapped degenerate attributes for Links

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");

                int automaticDegenerateMappingCounter = 0;
                var prepareDegenerateMappingStatement = new StringBuilder();

                prepareDegenerateMappingStatement.AppendLine("WITH ALL_DATABASE_COLUMNS AS");
                prepareDegenerateMappingStatement.AppendLine("(");

                prepareDegenerateMappingStatement.AppendLine("SELECT");
                prepareDegenerateMappingStatement.AppendLine("  [DATABASE_NAME]");
                prepareDegenerateMappingStatement.AppendLine(" ,[SCHEMA_NAME]");
                prepareDegenerateMappingStatement.AppendLine(" ,[TABLE_NAME]");
                prepareDegenerateMappingStatement.AppendLine(" ,[COLUMN_NAME]");
                prepareDegenerateMappingStatement.AppendLine(" ,[DATA_TYPE]");
                prepareDegenerateMappingStatement.AppendLine(" ,[CHARACTER_MAXIMUM_LENGTH]");
                prepareDegenerateMappingStatement.AppendLine(" ,[NUMERIC_PRECISION]");
                prepareDegenerateMappingStatement.AppendLine(" ,[NUMERIC_SCALE]");
                prepareDegenerateMappingStatement.AppendLine(" ,[ORDINAL_POSITION]");
                prepareDegenerateMappingStatement.AppendLine(" ,[PRIMARY_KEY_INDICATOR]");
                prepareDegenerateMappingStatement.AppendLine("FROM [MD_PHYSICAL_MODEL]");

                prepareDegenerateMappingStatement.AppendLine("),");
                prepareDegenerateMappingStatement.AppendLine("XREF AS");
                prepareDegenerateMappingStatement.AppendLine("(");
                prepareDegenerateMappingStatement.AppendLine("  SELECT");
                prepareDegenerateMappingStatement.AppendLine("    src.[SCHEMA_NAME] AS SOURCE_SCHEMA_NAME,");
                prepareDegenerateMappingStatement.AppendLine("    src.SOURCE_NAME_SHORT AS SOURCE_NAME,");
                prepareDegenerateMappingStatement.AppendLine("    lnk.[SCHEMA_NAME] AS TARGET_SCHEMA_NAME,");
                prepareDegenerateMappingStatement.AppendLine("    lnk.LINK_NAME_SHORT AS TARGET_NAME");
                prepareDegenerateMappingStatement.AppendLine("  FROM MD_SOURCE_LINK_XREF xref");
                prepareDegenerateMappingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_SOURCE src ON xref.SOURCE_NAME = src.SOURCE_NAME");
                prepareDegenerateMappingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_LINK lnk ON xref.LINK_NAME = lnk.LINK_NAME");
                prepareDegenerateMappingStatement.AppendLine(") ");
                prepareDegenerateMappingStatement.AppendLine("SELECT");
                prepareDegenerateMappingStatement.AppendLine("  XREF.SOURCE_NAME, ");
                prepareDegenerateMappingStatement.AppendLine("  XREF.TARGET_NAME AS LINK_NAME,");
                prepareDegenerateMappingStatement.AppendLine("  ADC_SOURCE.COLUMN_NAME AS ATTRIBUTE_NAME_FROM,");
                prepareDegenerateMappingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_NAME_TO,");
                prepareDegenerateMappingStatement.AppendLine("  'N' AS MULTI_ACTIVE_INDICATOR,");
                prepareDegenerateMappingStatement.AppendLine("  'Automatic mapping' as MAPPING_TYPE");
                prepareDegenerateMappingStatement.AppendLine("FROM XREF");
                prepareDegenerateMappingStatement.AppendLine(
                    "JOIN ALL_DATABASE_COLUMNS ADC_SOURCE ON XREF.SOURCE_SCHEMA_NAME = ADC_SOURCE.[SCHEMA_NAME] AND XREF.SOURCE_NAME = ADC_SOURCE.TABLE_NAME");
                prepareDegenerateMappingStatement.AppendLine(
                    "JOIN ALL_DATABASE_COLUMNS ADC_TARGET ON XREF.TARGET_SCHEMA_NAME = ADC_TARGET.[SCHEMA_NAME] AND XREF.TARGET_NAME = ADC_TARGET.TABLE_NAME");
                prepareDegenerateMappingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_ATTRIBUTE stg_attr ON UPPER(ADC_SOURCE.COLUMN_NAME) = UPPER(stg_attr.ATTRIBUTE_NAME) COLLATE Latin1_General_CS_AS");
                prepareDegenerateMappingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_ATTRIBUTE tgt_attr ON UPPER(ADC_TARGET.COLUMN_NAME) = UPPER(tgt_attr.ATTRIBUTE_NAME) COLLATE Latin1_General_CS_AS");
                prepareDegenerateMappingStatement.AppendLine(
                    "WHERE stg_attr.ATTRIBUTE_NAME = tgt_attr.ATTRIBUTE_NAME COLLATE Latin1_General_CS_AS");


                if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode)
                {
                    LogMetadataEvent(
                        "Commencing preparing the (automatic) column-to-column mapping metadata for degenerate attributes, based on what's available in the database.",
                        EventTypes.Information);
                }
                else
                {
                    LogMetadataEvent(
                        "Commencing preparing the degenerate column metadata using the physical model metadata.",
                        EventTypes.Information);
                }

                var automaticDegenerateMappings =
                    Utility.GetDataTable(ref connOmd, prepareDegenerateMappingStatement.ToString());

                if (automaticDegenerateMappings is null)
                {
                    LogMetadataEvent(
                        "There was an issue retrieving automatic attribute metadata from the repository. The query is: \r\n\r\n" +
                        prepareDegenerateMappingStatement, EventTypes.Error);
                }

                if (automaticDegenerateMappings is null || automaticDegenerateMappings.Rows.Count == 0)
                {
                    LogMetadataEvent("No automatic degenerate columns were detected.", EventTypes.Information);
                }
                else
                {
                    // Prevent duplicates to be inserted into the datatable, by only inserting new ones
                    // Entries found in the automatic check which are not already in the manual datatable will be added
                    foreach (DataRow automaticMapping in automaticDegenerateMappings.Rows)
                    {
                        DataRow[] foundRow = degenerateMappings.Select(
                            "SOURCE_NAME = '" + automaticMapping["SOURCE_NAME"] + "' AND LINK_NAME = '" +
                            automaticMapping["LINK_NAME"] + "' AND ATTRIBUTE_NAME_FROM = '" +
                            automaticMapping["ATTRIBUTE_NAME_FROM"] + "'AND ATTRIBUTE_NAME_TO = '" +
                            automaticMapping["ATTRIBUTE_NAME_TO"] + "'");
                        if (foundRow.Length == 0)
                        {
                            // If nothing is found, add to the overall data table that is inserted into SOURCE_SATELLITE_ATTRIBUTE_XREF
                            degenerateMappings.Rows.Add(
                                automaticMapping["SOURCE_NAME"],
                                automaticMapping["LINK_NAME"],
                                automaticMapping["ATTRIBUTE_NAME_FROM"],
                                automaticMapping["ATTRIBUTE_NAME_TO"],
                                automaticMapping["MAPPING_TYPE"]);

                            automaticDegenerateMappingCounter++;
                        }
                    }
                }

                // Now the full data table can be processed
                if (degenerateMappings.Rows.Count > 0)
                {
                    foreach (DataRow tableName in degenerateMappings.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {

                            var insertStatement = new StringBuilder();

                            insertStatement.AppendLine("INSERT INTO [MD_SOURCE_LINK_ATTRIBUTE_XREF]");
                            insertStatement.AppendLine(
                                "( [SOURCE_NAME],[LINK_NAME],[ATTRIBUTE_NAME_FROM],[ATTRIBUTE_NAME_TO], [MAPPING_TYPE])");
                            insertStatement.AppendLine("VALUES (" +
                                                       "'" + tableName["SOURCE_NAME"] + "', " +
                                                       "'" + tableName["LINK_NAME"] + "', " +
                                                       "'" + tableName["ATTRIBUTE_NAME_FROM"] + "', " +
                                                       "'" + tableName["ATTRIBUTE_NAME_TO"] + "', " +
                                                       "'" + tableName["MAPPING_TYPE"] + "'" +
                                                       ")");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();

                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An occurred during the preparation of degenerate columns between {tableName["SOURCE_NAME"]} to {tableName["LINK_NAME"]}. The error is: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);

                                if (tableName["ATTRIBUTE_NAME_FROM"].ToString() == "")
                                {
                                    LogMetadataEvent("Both attributes are NULL.", EventTypes.Information);
                                }
                            }
                        }
                    }
                }

                worker.ReportProgress(95);
                subProcess.Stop();
                LogMetadataEvent("Processing " + automaticDegenerateMappingCounter +
                                 " automatically added degenerate attribute mappings.", EventTypes.Information);
                LogMetadataEvent("Preparation of the degenerate column metadata completed, and has taken " +
                                 subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);

                #endregion


                #region Multi-Active Key

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                var prepareMultiKeyStatement = new StringBuilder();

                if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode)
                {
                    LogMetadataEvent("Commencing Multi-Active Key handling using database.", EventTypes.Information);

                    prepareMultiKeyStatement.AppendLine("SELECT");
                    prepareMultiKeyStatement.AppendLine("   xref.SOURCE_NAME");
                    prepareMultiKeyStatement.AppendLine("  ,xref.SATELLITE_NAME");
                    prepareMultiKeyStatement.AppendLine("  ,xref.ATTRIBUTE_NAME_FROM");
                    prepareMultiKeyStatement.AppendLine("  ,xref.ATTRIBUTE_NAME_TO");
                    prepareMultiKeyStatement.AppendLine("FROM MD_SOURCE_SATELLITE_ATTRIBUTE_XREF xref");
                    prepareMultiKeyStatement.AppendLine("INNER JOIN ");
                    prepareMultiKeyStatement.AppendLine("(");

                    prepareMultiKeyStatement.AppendLine("  SELECT");
                    prepareMultiKeyStatement.AppendLine("    [SCHEMA_NAME] AS LINK_SCHEMA,");
                    prepareMultiKeyStatement.AppendLine("    [TABLE_NAME]  AS SATELLITE_NAME,");
                    prepareMultiKeyStatement.AppendLine("    [COLUMN_NAME] AS ATTRIBUTE_NAME");
                    prepareMultiKeyStatement.AppendLine("  FROM MD_PHYSICAL_MODEL");
                    prepareMultiKeyStatement.AppendLine("  WHERE ");
                    prepareMultiKeyStatement.AppendLine("        COLUMN_NAME != '" + effectiveDateTimeAttribute +
                                                        "' AND COLUMN_NAME != '" + currentRecordAttribute +
                                                        "' AND COLUMN_NAME != '" + eventDateTimeAttribute + "'");
                    prepareMultiKeyStatement.AppendLine("    AND COLUMN_NAME NOT LIKE '" + dwhKeyIdentifier + "'");
                    prepareMultiKeyStatement.AppendLine("    AND (TABLE_NAME LIKE '" + satTablePrefix +
                                                        "' OR TABLE_NAME LIKE '" + lsatTablePrefix + "')");
                    prepareMultiKeyStatement.AppendLine("    AND PRIMARY_KEY_INDICATOR='Y'");

                    prepareMultiKeyStatement.AppendLine(") ddsub");
                    prepareMultiKeyStatement.AppendLine("ON xref.SATELLITE_NAME = ddsub.SATELLITE_NAME");
                    prepareMultiKeyStatement.AppendLine("AND xref.ATTRIBUTE_NAME_TO = ddsub.ATTRIBUTE_NAME");
                    prepareMultiKeyStatement.AppendLine("  WHERE ddsub.SATELLITE_NAME LIKE '" + satTablePrefix +
                                                        "' OR ddsub.SATELLITE_NAME LIKE '" + lsatTablePrefix + "'");

                }
                else
                {
                    LogMetadataEvent("Commencing Multi-Active Key handling using model metadata.",
                        EventTypes.Information);

                    prepareMultiKeyStatement.AppendLine("SELECT ");
                    prepareMultiKeyStatement.AppendLine("   xref.SOURCE_NAME");
                    prepareMultiKeyStatement.AppendLine("  ,xref.SATELLITE_NAME");
                    prepareMultiKeyStatement.AppendLine("  ,xref.ATTRIBUTE_NAME_FROM");
                    prepareMultiKeyStatement.AppendLine("  ,xref.ATTRIBUTE_NAME_TO");
                    prepareMultiKeyStatement.AppendLine("FROM MD_SOURCE_SATELLITE_ATTRIBUTE_XREF xref");
                    prepareMultiKeyStatement.AppendLine("INNER JOIN ");
                    prepareMultiKeyStatement.AppendLine("(");
                    prepareMultiKeyStatement.AppendLine("	SELECT");
                    prepareMultiKeyStatement.AppendLine("		TABLE_NAME AS SATELLITE_NAME,");
                    prepareMultiKeyStatement.AppendLine("		COLUMN_NAME AS ATTRIBUTE_NAME");
                    prepareMultiKeyStatement.AppendLine("	FROM TMP_MD_VERSION_ATTRIBUTE");
                    prepareMultiKeyStatement.AppendLine("	WHERE MULTI_ACTIVE_INDICATOR='Y'");
                    prepareMultiKeyStatement.AppendLine(") sub");
                    prepareMultiKeyStatement.AppendLine("ON xref.SATELLITE_NAME = sub.SATELLITE_NAME");
                    prepareMultiKeyStatement.AppendLine("AND xref.ATTRIBUTE_NAME_TO = sub.ATTRIBUTE_NAME");
                }

                var listMultiKeys = Utility.GetDataTable(ref connOmd, prepareMultiKeyStatement.ToString());

                if (listMultiKeys == null || listMultiKeys.Rows.Count == 0)
                {
                    LogMetadataEvent("No Multi-Active Keys were detected.", EventTypes.Information);
                }
                else
                {
                    foreach (DataRow tableName in listMultiKeys.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            LogMetadataEvent("Processing the Multi-Active Key attribute " +
                                             tableName["ATTRIBUTE_NAME_TO"] + " for " +
                                             tableName["SATELLITE_NAME"] + ".", EventTypes.Information);

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("UPDATE [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]");
                            insertStatement.AppendLine("SET MULTI_ACTIVE_KEY_INDICATOR='Y'");
                            insertStatement.AppendLine("WHERE SOURCE_NAME = '" + tableName["SOURCE_NAME"] + "'");
                            insertStatement.AppendLine("AND SATELLITE_NAME = '" + tableName["SATELLITE_NAME"] + "'");
                            insertStatement.AppendLine("AND ATTRIBUTE_NAME_FROM = '" +
                                                       tableName["ATTRIBUTE_NAME_FROM"] + "'");
                            insertStatement.AppendLine("AND ATTRIBUTE_NAME_TO = '" + tableName["ATTRIBUTE_NAME_TO"] +
                                                       "'");


                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An error occurred during the preparation of multi-active key metadata: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);
                            }
                        }
                    }
                }

                worker.ReportProgress(97);
                subProcess.Stop();
                LogMetadataEvent(
                    $"Preparation of the Multi-Active Keys completed, and has taken {subProcess.Elapsed.TotalSeconds} seconds.",
                    EventTypes.Information);

                #endregion


                #region Driving Key preparation

                subProcess.Reset();
                subProcess.Start();
                _alert.SetTextLogging("\r\n");
                LogMetadataEvent("Commencing preparing the Driving Key metadata.", EventTypes.Information);


                var prepareDrivingKeyStatement = new StringBuilder();
                prepareDrivingKeyStatement.AppendLine("SELECT DISTINCT");
                prepareDrivingKeyStatement.AppendLine("         sat.SATELLITE_NAME");
                prepareDrivingKeyStatement.AppendLine(
                    "         ,COALESCE(hubkey.HUB_NAME, (SELECT HUB_NAME FROM MD_HUB WHERE HUB_NAME_SHORT = 'Not applicable')) AS HUB_NAME");
                prepareDrivingKeyStatement.AppendLine(" FROM");
                prepareDrivingKeyStatement.AppendLine(" (");
                prepareDrivingKeyStatement.AppendLine("         SELECT");
                prepareDrivingKeyStatement.AppendLine("                 SOURCE_TABLE,");
                prepareDrivingKeyStatement.AppendLine("                 SOURCE_TABLE_SCHEMA,");
                prepareDrivingKeyStatement.AppendLine("                 TARGET_TABLE,");
                prepareDrivingKeyStatement.AppendLine("                 TARGET_TABLE_SCHEMA,");
                prepareDrivingKeyStatement.AppendLine("                 VERSION_ID,");
                prepareDrivingKeyStatement.AppendLine("                 CASE");
                prepareDrivingKeyStatement.AppendLine(
                    "                         WHEN CHARINDEX('(', RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)')))) > 0");
                prepareDrivingKeyStatement.AppendLine(
                    "                         THEN RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)')))");
                prepareDrivingKeyStatement.AppendLine(
                    "                         ELSE REPLACE(RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)'))), ')', '')");
                prepareDrivingKeyStatement.AppendLine(
                    "                 END AS BUSINESS_KEY_ATTRIBUTE--For Driving Key");
                prepareDrivingKeyStatement.AppendLine("         FROM");
                prepareDrivingKeyStatement.AppendLine("         (");
                prepareDrivingKeyStatement.AppendLine(
                    "                 SELECT SOURCE_TABLE, SOURCE_TABLE_SCHEMA, TARGET_TABLE, TARGET_TABLE_SCHEMA, DRIVING_KEY_ATTRIBUTE, VERSION_ID, CONVERT(XML, '<M>' + REPLACE(DRIVING_KEY_ATTRIBUTE, ',', '</M><M>') + '</M>') AS DRIVING_KEY_ATTRIBUTE_XML");
                prepareDrivingKeyStatement.AppendLine("                 FROM");
                prepareDrivingKeyStatement.AppendLine("                 (");
                prepareDrivingKeyStatement.AppendLine(
                    "                         SELECT DISTINCT SOURCE_TABLE, SOURCE_TABLE_SCHEMA, TARGET_TABLE, TARGET_TABLE_SCHEMA, VERSION_ID, LTRIM(RTRIM(DRIVING_KEY_ATTRIBUTE)) AS DRIVING_KEY_ATTRIBUTE");
                prepareDrivingKeyStatement.AppendLine("                         FROM TMP_MD_TABLE_MAPPING");
                prepareDrivingKeyStatement.AppendLine("                         WHERE TARGET_TABLE_TYPE IN ('" +
                                                      MetadataHandling.TableTypes.NaturalBusinessRelationshipContext +
                                                      "') AND DRIVING_KEY_ATTRIBUTE IS NOT NULL AND DRIVING_KEY_ATTRIBUTE != ''");
                prepareDrivingKeyStatement.AppendLine("                         AND [ENABLED_INDICATOR] = 'True'");
                prepareDrivingKeyStatement.AppendLine("                 ) TableName");
                prepareDrivingKeyStatement.AppendLine(
                    "         ) AS A CROSS APPLY DRIVING_KEY_ATTRIBUTE_XML.nodes('/M') AS Split(a)");
                prepareDrivingKeyStatement.AppendLine(" )  base");
                prepareDrivingKeyStatement.AppendLine(" LEFT JOIN [dbo].[TMP_MD_TABLE_MAPPING] hub");
                prepareDrivingKeyStatement.AppendLine("     ON  base.SOURCE_TABLE = hub.SOURCE_TABLE");
                prepareDrivingKeyStatement.AppendLine("     AND  base.SOURCE_TABLE_SCHEMA = hub.SOURCE_TABLE_SCHEMA");
                prepareDrivingKeyStatement.AppendLine("     AND hub.TARGET_TABLE_TYPE IN ('" +
                                                      MetadataHandling.TableTypes.CoreBusinessConcept + "')");
                prepareDrivingKeyStatement.AppendLine(
                    "     AND base.BUSINESS_KEY_ATTRIBUTE=hub.BUSINESS_KEY_ATTRIBUTE");
                prepareDrivingKeyStatement.AppendLine(" LEFT JOIN MD_SATELLITE sat");
                prepareDrivingKeyStatement.AppendLine(
                    "     ON base.TARGET_TABLE = sat.SATELLITE_NAME_SHORT AND base.TARGET_TABLE_SCHEMA = sat.SCHEMA_NAME");
                prepareDrivingKeyStatement.AppendLine(" LEFT JOIN MD_HUB hubkey");
                prepareDrivingKeyStatement.AppendLine(
                    "     ON hub.TARGET_TABLE = hubkey.HUB_NAME_SHORT AND hub.TARGET_TABLE_SCHEMA = hubkey.SCHEMA_NAME");
                prepareDrivingKeyStatement.AppendLine(" WHERE 1=1");
                prepareDrivingKeyStatement.AppendLine(" AND base.BUSINESS_KEY_ATTRIBUTE IS NOT NULL");
                prepareDrivingKeyStatement.AppendLine(" AND base.BUSINESS_KEY_ATTRIBUTE!=''");
                prepareDrivingKeyStatement.AppendLine(" AND [ENABLED_INDICATOR] = 'True'");


                var listDrivingKeys = Utility.GetDataTable(ref connOmd, prepareDrivingKeyStatement.ToString());

                if (listDrivingKeys.Rows.Count == 0)
                {
                    LogMetadataEvent("No Driving Key based Link-Satellites were detected.", EventTypes.Information);
                }
                else
                {
                    foreach (DataRow tableName in listDrivingKeys.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            var insertStatement = new StringBuilder();

                            insertStatement.AppendLine("INSERT INTO [MD_DRIVING_KEY_XREF]");
                            insertStatement.AppendLine("( [SATELLITE_NAME] ,[HUB_NAME] )");
                            insertStatement.AppendLine("VALUES ");
                            insertStatement.AppendLine("(");
                            insertStatement.AppendLine("  '" + tableName["SATELLITE_NAME"] + "',");
                            insertStatement.AppendLine("  '" + tableName["HUB_NAME"] + "'");
                            insertStatement.AppendLine(")");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                LogMetadataEvent(
                                    $"An error occurred during the preparation of driving-active key metadata: \r\n\r\n {ex}. \r\nThe query that caused the issue is: \r\n\r\n{insertStatement}",
                                    EventTypes.Error);
                            }
                        }
                    }
                }

                worker.ReportProgress(98);
                subProcess.Stop();
                LogMetadataEvent(
                    "Preparation of the Driving Key column metadata completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.", EventTypes.Information);

                #endregion

                //
                // Activation completed!
                //

                // Error handling
                // Clear out the existing error log, or create an empty new file
                using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + @"\Error_Log.txt"))
                {
                    outfile.Write(String.Empty);
                    outfile.Close();
                }

                // Write any errors

                int errorCounter = 0;
                foreach (var individualEvent in GlobalParameters.TeamEventLog)
                {
                    if (individualEvent.eventTime >= activationStartDateTime &&
                        individualEvent.eventCode == (int) EventTypes.Error)
                    {
                        errorCounter++;
                    }
                }

                if (errorCounter > 0)
                {
                    _alert.SetTextLogging("\r\n");
                    LogMetadataEvent("There were " + errorCounter + " error(s) found while processing the metadata.",
                        EventTypes.Warning);
                    LogMetadataEvent("Please check the TEAM Event Log for details.", EventTypes.Information);
                }
                else
                {
                    _alert.SetTextLogging("\r\n");
                    _alert.SetTextLogging("\r\n");
                    _alert.SetTextLogging("\r\n");
                    LogMetadataEvent($"No errors were detected in the activation process.", EventTypes.Information);
                }

                // Remove the temporary tables that have been used
                DropTemporaryWorkerTable(
                    TeamConfiguration.MetadataConnection.CreateSqlServerConnectionString(false));

                // Report completion
                totalProcess.Stop();
                LogMetadataEvent(
                    "The full activation process has taken " + totalProcess.Elapsed.TotalSeconds + " seconds.",
                    EventTypes.Information);
                worker.ReportProgress(100);

            }
        }


        /// <summary>
        /// Evaluates the type of a Data Object, and adds it to a list of objects and cross-references to process further.
        /// </summary>
        /// <param name="localTableNonQualified"></param>
        /// <param name="localTableFull"></param>
        /// <param name="dataObjectList"></param>
        /// <param name="addToXref"></param>
        /// <param name="xrefList"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="filterCriterion"></param>
        /// <param name="sourceTableFull"></param>
        /// <param name="targetTableFull"></param>
        private static void EvaluateDataObjectsToList(string localTableNameFullyQualified,
            TeamConnection teamConnection,
            List<Tuple<string, MetadataHandling.TableTypes, TeamConnection>> dataObjectList, bool addToXref,
            List<Tuple<string, string, string, string, MetadataHandling.TableTypes>> xrefList,
            string businessKeyDefinition, string filterCriterion, string sourceTableFull, string targetTableFull)
        {

            var localTableNonQualified = MetadataHandling.GetNonQualifiedTableName(localTableNameFullyQualified);

            if // Evaluate STG
            (
                (TeamConfiguration.TableNamingLocation == "Prefix" &&
                 localTableNonQualified.StartsWith(TeamConfiguration.StgTablePrefixValue))
                ||
                (TeamConfiguration.TableNamingLocation == "Suffix" &&
                 localTableNonQualified.EndsWith(TeamConfiguration.StgTablePrefixValue))
            )
            {
                var localDataObjectListEntry =
                    new Tuple<string, MetadataHandling.TableTypes, TeamConnection>(localTableNameFullyQualified,
                        MetadataHandling.TableTypes.StagingArea, teamConnection);
                if (!dataObjectList.Contains(localDataObjectListEntry))
                {
                    dataObjectList.Add(localDataObjectListEntry);
                }

                if (addToXref
                ) // Only add xref entries if the target is evaluated. addToXref = true only applies to targets
                {
                    // Source/target/businessKey/filter/type
                    xrefList.Add(new Tuple<string, string, string, string, MetadataHandling.TableTypes>
                        (
                            sourceTableFull,
                            targetTableFull,
                            businessKeyDefinition,
                            filterCriterion,
                            MetadataHandling.TableTypes.StagingArea)
                    );
                }
            }
            else if // Evaluate PSA
            (
                (TeamConfiguration.TableNamingLocation == "Prefix" &&
                 localTableNonQualified.StartsWith(TeamConfiguration.PsaTablePrefixValue))
                ||
                (TeamConfiguration.TableNamingLocation == "Suffix" &&
                 localTableNonQualified.EndsWith(TeamConfiguration.PsaTablePrefixValue))
            )
            {
                var localDataObjectListEntry = new Tuple<string, MetadataHandling.TableTypes, TeamConnection>(
                    localTableNameFullyQualified, MetadataHandling.TableTypes.PersistentStagingArea, teamConnection);
                if (!dataObjectList.Contains(localDataObjectListEntry))
                {
                    dataObjectList.Add(localDataObjectListEntry);
                }

                if (addToXref
                ) // Only add xref entries if the target is evaluated. addToXref = true only applies to targets
                {
                    // Source/target/businessKey/filter/type
                    xrefList.Add(new Tuple<string, string, string, string, MetadataHandling.TableTypes>
                        (
                            sourceTableFull,
                            targetTableFull,
                            businessKeyDefinition,
                            filterCriterion,
                            MetadataHandling.TableTypes.PersistentStagingArea)
                    );
                }
            }
            else if ( // Evaluate Core Business Concepts
                (TeamConfiguration.TableNamingLocation == "Prefix" &&
                 localTableNonQualified.StartsWith(TeamConfiguration.HubTablePrefixValue))
                ||
                (TeamConfiguration.TableNamingLocation == "Suffix" &&
                 localTableNonQualified.EndsWith(TeamConfiguration.HubTablePrefixValue))
            )
            {
                var localDataObjectListEntry = new Tuple<string, MetadataHandling.TableTypes, TeamConnection>(
                    localTableNameFullyQualified, MetadataHandling.TableTypes.CoreBusinessConcept, teamConnection);
                if (!dataObjectList.Contains(localDataObjectListEntry))
                {
                    dataObjectList.Add(localDataObjectListEntry);
                }

                if (addToXref
                ) // Only add xref entries if the target is evaluated. addToXref = true only applies to targets
                {
                    // Source/target/businessKey/filter/type
                    xrefList.Add(new Tuple<string, string, string, string, MetadataHandling.TableTypes>
                        (
                            sourceTableFull,
                            targetTableFull,
                            businessKeyDefinition,
                            filterCriterion,
                            MetadataHandling.TableTypes.CoreBusinessConcept)
                    );
                }
            }
            else if ( // Evaluate Context objects
                (TeamConfiguration.TableNamingLocation == "Prefix" &&
                 localTableNonQualified.StartsWith(TeamConfiguration.SatTablePrefixValue))
                ||
                (TeamConfiguration.TableNamingLocation == "Suffix" &&
                 localTableNonQualified.EndsWith(TeamConfiguration.SatTablePrefixValue))
            )
            {
                var localDataObjectListEntry =
                    new Tuple<string, MetadataHandling.TableTypes, TeamConnection>(localTableNameFullyQualified,
                        MetadataHandling.TableTypes.Context, teamConnection);
                if (!dataObjectList.Contains(localDataObjectListEntry))
                {
                    dataObjectList.Add(localDataObjectListEntry);
                }

                if (addToXref
                ) // Only add xref entries if the target is evaluated. addToXref = true only applies to targets
                {
                    // Source/target/businessKey/filter/type
                    xrefList.Add(new Tuple<string, string, string, string, MetadataHandling.TableTypes>
                        (
                            sourceTableFull,
                            targetTableFull,
                            businessKeyDefinition,
                            filterCriterion,
                            MetadataHandling.TableTypes.Context)
                    );
                }
            }
            else if ( // Evaluate Relationship objects
                (TeamConfiguration.TableNamingLocation == "Prefix" &&
                 localTableNonQualified.StartsWith(TeamConfiguration.LinkTablePrefixValue))
                ||
                (TeamConfiguration.TableNamingLocation == "Suffix" &&
                 localTableNonQualified.EndsWith(TeamConfiguration.LinkTablePrefixValue))
            )
            {
                var localDataObjectListEntry = new Tuple<string, MetadataHandling.TableTypes, TeamConnection>(
                    localTableNameFullyQualified,
                    MetadataHandling.TableTypes.NaturalBusinessRelationship, teamConnection);
                if (!dataObjectList.Contains(localDataObjectListEntry))
                {
                    dataObjectList.Add(localDataObjectListEntry);
                }

                if (addToXref
                ) // Only add xref entries if the target is evaluated. addToXref = true only applies to targets
                {
                    // Source/target/businessKey/filter/type
                    xrefList.Add(new Tuple<string, string, string, string, MetadataHandling.TableTypes>
                        (
                            sourceTableFull,
                            targetTableFull,
                            businessKeyDefinition,
                            filterCriterion,
                            MetadataHandling.TableTypes.NaturalBusinessRelationship)
                    );
                }
            }
            else if ( // Evaluate Relationship Context objects
                (TeamConfiguration.TableNamingLocation == "Prefix" &&
                 localTableNonQualified.StartsWith(TeamConfiguration.LsatTablePrefixValue))
                ||
                (TeamConfiguration.TableNamingLocation == "Suffix" &&
                 localTableNonQualified.EndsWith(TeamConfiguration.LsatTablePrefixValue))
            )
            {
                var localDataObjectListEntry = new Tuple<string, MetadataHandling.TableTypes, TeamConnection>(
                    localTableNameFullyQualified,
                    MetadataHandling.TableTypes.NaturalBusinessRelationshipContext, teamConnection);
                if (!dataObjectList.Contains(localDataObjectListEntry))
                {
                    dataObjectList.Add(localDataObjectListEntry);
                }

                if (addToXref
                ) // Only add xref entries if the target is evaluated. addToXref = true only applies to targets
                {
                    // Source/target/businessKey/filter/type
                    xrefList.Add(new Tuple<string, string, string, string, MetadataHandling.TableTypes>
                        (
                            sourceTableFull,
                            targetTableFull,
                            businessKeyDefinition,
                            filterCriterion,
                            MetadataHandling.TableTypes.NaturalBusinessRelationshipContext)
                    );
                }
            }
            else // Other - Unknown
            {
                var localDataObjectListEntry =
                    new Tuple<string, MetadataHandling.TableTypes, TeamConnection>(localTableNameFullyQualified,
                        MetadataHandling.TableTypes.Unknown, teamConnection);
                if (!dataObjectList.Contains(localDataObjectListEntry))
                {
                    dataObjectList.Add(localDataObjectListEntry);
                }

                if (addToXref
                ) // Only add xref entries if the target is evaluated. addToXref = true only applies to targets
                {
                    // Source/target/businessKey/filter/type
                    xrefList.Add(new Tuple<string, string, string, string, MetadataHandling.TableTypes>
                        (
                            sourceTableFull,
                            targetTableFull,
                            businessKeyDefinition,
                            filterCriterion,
                            MetadataHandling.TableTypes.Unknown)
                    );
                }
            }
        }

        private void LogMetadataEvent(string eventMessage, EventTypes eventType)
        {
            GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(eventType, eventMessage));
            _alert.SetTextLogging("\r\n" + eventMessage);
        }

        private void DataGridViewTableMetadataKeyDown(object sender, KeyEventArgs e)
        {
            // Only works when not in edit mode.
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.V:
                            PasteClipboardTableMetadata();
                            break;
                        case Keys.C:
                            if (sender.GetType() == typeof(DataGridViewComboBoxEditingControl))
                            {
                                var temp = (DataGridViewComboBoxEditingControl) sender;
                                Clipboard.SetText(temp.SelectedValue.ToString());
                            }

                            break;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Pasting into the data grid has failed", "Copy/Paste", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// DataGridView OnKeyDown event for DataGridViewAttributeMetadata
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewAttributeMetadataKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.V:
                            PasteClipboardAttributeMetadata();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Pasting into the data grid has failed", "Copy/Paste", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void PasteClipboardTableMetadata()
        {
            try
            {
                string s = Clipboard.GetText();
                string[] lines = s.Split('\n');

                int iRow = dataGridViewTableMetadata.CurrentCell.RowIndex;
                int iCol = dataGridViewTableMetadata.CurrentCell.ColumnIndex;
                DataGridViewCell oCell;
                if (iRow + lines.Length > dataGridViewTableMetadata.Rows.Count - 1)
                {
                    bool bFlag = false;
                    foreach (string sEmpty in lines)
                    {
                        if (sEmpty == "")
                        {
                            bFlag = true;
                        }
                    }

                    int iNewRows = iRow + lines.Length - dataGridViewTableMetadata.Rows.Count;
                    if (iNewRows > 0)
                    {
                        if (bFlag)
                            dataGridViewTableMetadata.Rows.Add(iNewRows);
                        else
                            dataGridViewTableMetadata.Rows.Add(iNewRows + 1);
                    }
                    else
                        dataGridViewTableMetadata.Rows.Add(iNewRows + 1);
                }

                foreach (string line in lines)
                {
                    if (iRow < dataGridViewTableMetadata.RowCount && line.Length > 0)
                    {
                        string[] sCells = line.Split('\t');
                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            if (iCol + i < dataGridViewTableMetadata.ColumnCount)
                            {
                                oCell = dataGridViewTableMetadata[iCol + i, iRow];
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
                richTextBoxInformation.AppendText(
                    "An error has been encountered formatting this cell. Please check the Event Log for more details.\r\n");
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    $"An exception has been encountered: {ex.Message}."));
            }
        }

        private void PasteClipboardAttributeMetadata()
        {
            try
            {
                string s = Clipboard.GetText();
                string[] lines = s.Split('\n');

                int iRow = dataGridViewAttributeMetadata.CurrentCell.RowIndex;
                int iCol = dataGridViewAttributeMetadata.CurrentCell.ColumnIndex;
                DataGridViewCell oCell;
                if (iRow + lines.Length > dataGridViewAttributeMetadata.Rows.Count - 1)
                {
                    bool bFlag = false;
                    foreach (string sEmpty in lines)
                    {
                        if (sEmpty == "")
                        {
                            bFlag = true;
                        }
                    }

                    int iNewRows = iRow + lines.Length - dataGridViewAttributeMetadata.Rows.Count;
                    if (iNewRows > 0)
                    {
                        if (bFlag)
                            dataGridViewAttributeMetadata.Rows.Add(iNewRows);
                        else
                            dataGridViewAttributeMetadata.Rows.Add(iNewRows + 1);
                    }
                    else
                        dataGridViewAttributeMetadata.Rows.Add(iNewRows + 1);
                }

                foreach (string line in lines)
                {
                    if (iRow < dataGridViewAttributeMetadata.RowCount && line.Length > 0)
                    {
                        string[] sCells = line.Split('\t');
                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            if (iCol + i < dataGridViewAttributeMetadata.ColumnCount)
                            {
                                oCell = dataGridViewAttributeMetadata[iCol + i, iRow];
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
                richTextBoxInformation.AppendText(
                    "An error has been encountered formatting this cell. Please check the Event Log for more details.\r\n");
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    $"An exception has been encountered: {ex.Message}."));
            }
        }

        private void FormManageMetadata_SizeChanged(object sender, EventArgs e)
        {
            GridAutoLayout();
        }

        /// <summary>
        /// Validation event on Table Metadata datagridview.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewTableMetadata_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Validate the data entry on the Table Mapping datagridview
            var valueLength = e.FormattedValue.ToString().Length;

            // Source Table (Source)
            if (e.ColumnIndex == (int) TableMappingMetadataColumns.SourceTable)
            {
                dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "";

                if (e.FormattedValue == DBNull.Value || valueLength == 0)
                {
                    e.Cancel = true;
                    dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "The Source (Source) table cannot be empty!";
                }
            }

            // Target Table
            if (e.ColumnIndex == (int) TableMappingMetadataColumns.TargetTable)
            {
                dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "";

                if (e.FormattedValue == DBNull.Value || valueLength == 0)
                {
                    e.Cancel = true;
                    dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText =
                        "The Target (Integration Layer) table cannot be empty!";
                }
            }

            // Business Key
            if (e.ColumnIndex == (int) TableMappingMetadataColumns.BusinessKeyDefinition)
            {
                dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "";

                if (e.FormattedValue == DBNull.Value || valueLength == 0)
                {
                    e.Cancel = true;
                    dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "The Business Key cannot be empty!";
                }
            }

            // Filter criteria
            if (e.ColumnIndex == (int) TableMappingMetadataColumns.FilterCriterion)
            {
                dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "";
                //int newInteger;
                var equalSignIndex = e.FormattedValue.ToString().IndexOf('=') + 1;

                if (valueLength > 0 && valueLength < 3)
                {
                    e.Cancel = true;
                    dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText =
                        "The filter criterion cannot only be just one or two characters as it translates into a WHERE clause.";
                }

                if (valueLength > 0)
                {
                    //Check if an '=' is there
                    if (e.FormattedValue.ToString() == "=")
                    {
                        e.Cancel = true;
                        dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText =
                            "The filter criterion cannot only be '=' as it translates into a WHERE clause.";
                    }

                    // If there are value in the filter, and the filter contains an equal sign but it's the last then cancel
                    if (valueLength > 2 &&
                        (e.FormattedValue.ToString().Contains("=") && !(equalSignIndex < valueLength)))
                    {
                        e.Cancel = true;
                        dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText =
                            "The filter criterion include values either side of the '=' sign as it is expressed as a WHERE clause.";
                    }
                }
            }
        }

        public DateTime ActivationMetadata()
        {
            DateTime mostRecentActivationDateTime = DateTime.MinValue;

            var connOmd = new SqlConnection
            {
                ConnectionString = TeamConfiguration.MetadataConnection.CreateSqlServerConnectionString(false)
            };

            var sqlStatementForActivationMetadata = new StringBuilder();
            sqlStatementForActivationMetadata.AppendLine(
                "SELECT [VERSION_NAME], MAX([ACTIVATION_DATETIME]) AS [ACTIVATION_DATETIME]");
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

                if (dataGridViewTableMetadata != null) // There needs to be metadata available
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

                    for (int i = 0; i < dataGridViewTableMetadata.Rows.Count - 1; i++)
                    {
                        DataGridViewRow row = dataGridViewTableMetadata.Rows[i];
                        string sourceNode = row.Cells[(int) TableMappingMetadataColumns.SourceTable].Value.ToString();
                        var sourceConnectionId = row.Cells[(int)TableMappingMetadataColumns.SourceConnection].ToString();
                        var sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionId);
                        KeyValuePair<string, string> fullyQualifiedObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(sourceNode, sourceConnection).FirstOrDefault();


                        string targetNode = row.Cells[(int)TableMappingMetadataColumns.TargetTable].Value.ToString();
                        var targetConnectionId = row.Cells[(int)TableMappingMetadataColumns.TargetConnection].ToString();
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
                        GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The following query caused an issue when generating the DGML file: \r\n\r\n{sqlStatementForSubjectAreas}."));
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
                        GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The following query caused an issue when generating the DGML file: \r\n\r\n{sqlStatementForHubCategories}."));
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
                        sqlStatementForRelationships.AppendLine("SELECT DISTINCT [HUB_NAME], TARGET_SCHEMA_NAME+'.'+[TARGET_NAME]");
                        sqlStatementForRelationships.AppendLine("FROM [interface].[INTERFACE_HUB_LINK_XREF]");
                        sqlStatementForRelationships.AppendLine("WHERE HUB_NAME NOT IN ('N/A')");

                        var businessConceptsRelationships = Utility.GetDataTable(ref connOmd, sqlStatementForRelationships.ToString());

                        foreach (DataRow row in businessConceptsRelationships.Rows)
                        {
                            dgmlExtract.AppendLine("     <Link Source=\"" + (string) row["HUB_NAME"] + "\" Target=\"" +
                                                   (string) row["TARGET_NAME"] + "\" />");
                        }
                    }
                    catch
                    {
                        GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The following query caused an issue when generating the DGML file: \r\n\r\n{sqlStatementForRelationships}."));
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
                        GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The following query caused an issue when generating the DGML file: \r\n\r\n{sqlStatementForLinkCategories}."));
                        errorCounter++;
                    }


                    // Add the regular source-to-target mappings as edges using the data grid
                    dgmlExtract.AppendLine("     <!-- Regular source-to-target mappings -->");
                    for (var i = 0; i < dataGridViewTableMetadata.Rows.Count - 1; i++)
                    {
                        var row = dataGridViewTableMetadata.Rows[i];
                        
                        string sourceNode = row.Cells[(int)TableMappingMetadataColumns.SourceTable].Value.ToString();
                        var sourceConnectionId = row.Cells[(int)TableMappingMetadataColumns.SourceConnection].ToString();
                        var sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionId);
                        KeyValuePair<string, string> fullyQualifiedObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(sourceNode, sourceConnection).FirstOrDefault();

                        string targetNode = row.Cells[(int)TableMappingMetadataColumns.TargetTable].Value.ToString();
                        var targetConnectionId = row.Cells[(int)TableMappingMetadataColumns.TargetConnection].ToString();
                        var targetConnection = GetTeamConnectionByConnectionId(targetConnectionId);
                        KeyValuePair<string, string> fullyQualifiedObjectTarget = MetadataHandling.GetFullyQualifiedDataObjectName(targetNode, targetConnection).FirstOrDefault();

                        var businessKey = row.Cells[(int) TableMappingMetadataColumns.BusinessKeyDefinition].Value.ToString();


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

        private void textBoxFilterCriterion_OnDelayedTextChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dr in dataGridViewTableMetadata.Rows)
            {
                dr.Visible = true;
            }

            foreach (DataGridViewRow dr in dataGridViewTableMetadata.Rows)
            {
                if (dr.Cells[(int) TableMappingMetadataColumns.TargetTable].Value != null)
                {
                    if (!dr.Cells[(int) TableMappingMetadataColumns.TargetTable].Value.ToString()
                        .Contains(textBoxFilterCriterion.Text) && !dr
                        .Cells[(int) TableMappingMetadataColumns.SourceTable].Value.ToString()
                        .Contains(textBoxFilterCriterion.Text))
                    {
                        CurrencyManager currencyManager1 =
                            (CurrencyManager) BindingContext[dataGridViewTableMetadata.DataSource];
                        currencyManager1.SuspendBinding();
                        dr.Visible = false;
                        currencyManager1.ResumeBinding();
                    }
                }
            }
        }

        private void saveTableMappingAsJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var theDialog = new SaveFileDialog
                {
                    Title = @"Save Table Mapping Metadata File",
                    Filter = @"JSON files|*.json",
                    InitialDirectory = GlobalParameters.ConfigurationPath //Application.StartupPath + @"\Configuration\"
                };

                var ret = STAShowDialog(theDialog);

                if (ret == DialogResult.OK)
                {
                    try
                    {
                        var chosenFile = theDialog.FileName;

                        DataTable gridDataTable = (DataTable) _bindingSourceTableMetadata.DataSource;

                        // Make sure the output is sorted
                        TableMapping.SetDataTableSorting();

                        gridDataTable.TableName = "TableMappingMetadata";

                        JArray outputFileArray = new JArray();
                        foreach (DataRow singleRow in gridDataTable.DefaultView.ToTable().Rows)
                        {
                            JObject individualRow = JObject.FromObject(new
                            {
                                enabledIndicator = singleRow[(int) TableMappingMetadataColumns.Enabled].ToString(),
                                tableMappingHash = singleRow[(int) TableMappingMetadataColumns.HashKey].ToString(),
                                versionId = singleRow[(int) TableMappingMetadataColumns.VersionId].ToString(),
                                sourceTable = singleRow[(int) TableMappingMetadataColumns.SourceTable].ToString(),
                                sourceConnection = singleRow[(int) TableMappingMetadataColumns.SourceConnection]
                                    .ToString(),
                                targetTable = singleRow[(int) TableMappingMetadataColumns.TargetTable].ToString(),
                                targetConnection = singleRow[(int) TableMappingMetadataColumns.TargetConnection]
                                    .ToString(),
                                businessKeyDefinition =
                                    singleRow[(int) TableMappingMetadataColumns.BusinessKeyDefinition].ToString(),
                                drivingKeyDefinition = singleRow[(int) TableMappingMetadataColumns.DrivingKeyDefinition]
                                    .ToString(),
                                filterCriteria = singleRow[(int) TableMappingMetadataColumns.FilterCriterion].ToString()
                            });
                            outputFileArray.Add(individualRow);
                        }

                        string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

                        File.WriteAllText(chosenFile, json);

                        richTextBoxInformation.Text =
                            "The Table Mapping metadata file " + chosenFile + " saved successfully.";
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.AppendText(
                            "An error has been encountered when attempting to save the file to disk. Please check the Event Log for more details.\r\n");
                        GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                            $"An exception has been encountered: {ex.Message}."));
                    }
                }
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText(
                    "An error has been encountered when attempting to save the file to disk. Please check the Event Log for more details.\r\n");
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    $"An exception has been encountered: {ex.Message}."));
            }
        }



        private void openOutputDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(GlobalParameters.OutputPath);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text =
                    "An error has occurred while attempting to open the output directory. The error message is: " + ex;
            }
        }


        /// <summary>
        ///   Method called when clicking the Reverse Engineer button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReverseEngineerMetadataButtonClick(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();
            richTextBoxInformation.Text += "Commencing reverse-engineering the model metadata from the database.\r\n";

            var completeDataTable = new DataTable();

            foreach (var item in checkedListBoxReverseEngineeringAreas.CheckedItems)
            {
                var localConnectionObject = (KeyValuePair<TeamConnection, string>) item;

                var localSqlConnection = new SqlConnection
                    {ConnectionString = localConnectionObject.Key.CreateSqlServerConnectionString(false)};
                var reverseEngineerResults = ReverseEngineerModelMetadata(localSqlConnection,
                    localConnectionObject.Key.DatabaseServer.DatabaseName);

                if (reverseEngineerResults != null)
                {
                    completeDataTable.Merge(reverseEngineerResults);
                }
            }

            DataTable distinctTable = completeDataTable.DefaultView.ToTable( /*distinct*/ true);

            distinctTable.DefaultView.Sort =
                "[DATABASE_NAME] ASC, [SCHEMA_NAME] ASC, [TABLE_NAME] ASC, [ORDINAL_POSITION] ASC";

            // Display the results on the datagrid
            _bindingSourcePhysicalModelMetadata.DataSource = distinctTable;

            // Set the column header names.
            dataGridViewPhysicalModelMetadata.DataSource = _bindingSourcePhysicalModelMetadata;
            dataGridViewPhysicalModelMetadata.ColumnHeadersVisible = true;
            dataGridViewPhysicalModelMetadata.Columns[0].Visible = false;
            dataGridViewPhysicalModelMetadata.Columns[1].Visible = false;

            dataGridViewPhysicalModelMetadata.Columns[0].HeaderText = "Hash Key"; //Key column
            dataGridViewPhysicalModelMetadata.Columns[1].HeaderText = "Version ID"; //Key column
            dataGridViewPhysicalModelMetadata.Columns[2].HeaderText = "Database Name"; //Key column
            dataGridViewPhysicalModelMetadata.Columns[3].HeaderText = "Schema Name"; //Key column
            dataGridViewPhysicalModelMetadata.Columns[4].HeaderText = "Table Name"; //Key column
            dataGridViewPhysicalModelMetadata.Columns[5].HeaderText = "Column Name"; //Key column
            dataGridViewPhysicalModelMetadata.Columns[6].HeaderText = "Data Type";
            dataGridViewPhysicalModelMetadata.Columns[7].HeaderText = "Length";
            dataGridViewPhysicalModelMetadata.Columns[8].HeaderText = "Precision";
            dataGridViewPhysicalModelMetadata.Columns[9].HeaderText = "Scale";
            dataGridViewPhysicalModelMetadata.Columns[10].HeaderText = "Position";
            dataGridViewPhysicalModelMetadata.Columns[11].HeaderText = "Primary Key";
            dataGridViewPhysicalModelMetadata.Columns[12].HeaderText = "Multi-Active";

            foreach (DataRow row in completeDataTable.Rows) //Flag as new row so it's detected by the save button
            {
                row.SetAdded();
            }

            // Resize the grid
            GridAutoLayoutPhysicalModelMetadata();
        }


        /// <summary>
        ///   Connect to a given database and return the data dictionary (catalog) information in the datagrid.
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
                richTextBoxInformation.Text +=
                    "An error has occurred uploading the model for the new version because the database could not be connected to. The error message is: " +
                    exception.Message + ".\r\n";
            }

            // Get everything as local variables to reduce multi-threading issues
            var effectiveDateTimeAttribute =
                TeamConfiguration.EnableAlternativeSatelliteLoadDateTimeAttribute == "True"
                    ? TeamConfiguration.AlternativeSatelliteLoadDateTimeAttribute
                    : TeamConfiguration.LoadDateTimeAttribute;
            var dwhKeyIdentifier = TeamConfiguration.DwhKeyIdentifier; //Indicates _HSH, _SK etc.
            var keyIdentifierLocation = TeamConfiguration.KeyNamingLocation;

            // Create the attribute selection statement for the array
            var sqlStatementForAttributeVersion = new StringBuilder();

            sqlStatementForAttributeVersion.AppendLine("SELECT ");
            sqlStatementForAttributeVersion.AppendLine("  CONVERT(CHAR(32),HASHBYTES('MD5',CONVERT(NVARCHAR(100), " +
                                                       GlobalParameters.CurrentVersionId +
                                                       ") + '|' + OBJECT_NAME(main.OBJECT_ID) + '|' + main.[name]),2) AS ROW_CHECKSUM,");
            sqlStatementForAttributeVersion.AppendLine("  " + GlobalParameters.CurrentVersionId + " AS [VERSION_ID],");
            sqlStatementForAttributeVersion.AppendLine("  DB_NAME(DB_ID('" + databaseName + "')) AS [DATABASE_NAME],");
            sqlStatementForAttributeVersion.AppendLine("  OBJECT_SCHEMA_NAME(main.OBJECT_ID) AS [SCHEMA_NAME],");
            sqlStatementForAttributeVersion.AppendLine("  OBJECT_NAME(main.OBJECT_ID) AS [TABLE_NAME], ");
            sqlStatementForAttributeVersion.AppendLine("  main.[name] AS [COLUMN_NAME], ");
            sqlStatementForAttributeVersion.AppendLine("  t.[name] AS [DATA_TYPE], ");
            sqlStatementForAttributeVersion.AppendLine("  CAST(COALESCE(");
            sqlStatementForAttributeVersion.AppendLine(
                "    CASE WHEN UPPER(t.[name]) = 'NVARCHAR' THEN main.[max_length]/2"); //Exception for unicode
            sqlStatementForAttributeVersion.AppendLine("    ELSE main.[max_length]");
            sqlStatementForAttributeVersion.AppendLine("    END");
            sqlStatementForAttributeVersion.AppendLine("     ,0) AS VARCHAR(100)) AS [CHARACTER_MAXIMUM_LENGTH],");
            sqlStatementForAttributeVersion.AppendLine(
                "  CAST(COALESCE(main.[precision],0) AS VARCHAR(100)) AS [NUMERIC_PRECISION], ");
            sqlStatementForAttributeVersion.AppendLine(
                "  CAST(COALESCE(main.[scale], 0) AS VARCHAR(100)) AS[NUMERIC_SCALE], ");

            sqlStatementForAttributeVersion.AppendLine(
                "  CAST(main.[column_id] AS VARCHAR(100)) AS [ORDINAL_POSITION], ");

            sqlStatementForAttributeVersion.AppendLine("  CASE ");
            sqlStatementForAttributeVersion.AppendLine("    WHEN keysub.COLUMN_NAME IS NULL ");
            sqlStatementForAttributeVersion.AppendLine("    THEN 'N' ");
            sqlStatementForAttributeVersion.AppendLine("    ELSE 'Y' ");
            sqlStatementForAttributeVersion.AppendLine("  END AS PRIMARY_KEY_INDICATOR, ");

            sqlStatementForAttributeVersion.AppendLine("  CASE ");
            sqlStatementForAttributeVersion.AppendLine("    WHEN ma.COLUMN_NAME IS NULL ");
            sqlStatementForAttributeVersion.AppendLine("    THEN 'N' ");
            sqlStatementForAttributeVersion.AppendLine("    ELSE 'Y' ");
            sqlStatementForAttributeVersion.AppendLine("  END AS MULTI_ACTIVE_INDICATOR ");

            sqlStatementForAttributeVersion.AppendLine("FROM [" + databaseName + "].sys.columns main");
            sqlStatementForAttributeVersion.AppendLine("JOIN sys.types t ON main.user_type_id=t.user_type_id");
            sqlStatementForAttributeVersion.AppendLine("-- Primary Key");
            sqlStatementForAttributeVersion.AppendLine("LEFT OUTER JOIN (");
            sqlStatementForAttributeVersion.AppendLine("	SELECT ");
            sqlStatementForAttributeVersion.AppendLine("	  sc.name AS TABLE_NAME,");
            sqlStatementForAttributeVersion.AppendLine("	  C.name AS COLUMN_NAME");
            sqlStatementForAttributeVersion.AppendLine("	FROM [" + databaseName + "].sys.index_columns A");
            sqlStatementForAttributeVersion.AppendLine("	JOIN [" + databaseName + "].sys.indexes B");
            sqlStatementForAttributeVersion.AppendLine("	ON A.OBJECT_ID=B.OBJECT_ID AND A.index_id=B.index_id");
            sqlStatementForAttributeVersion.AppendLine("	JOIN [" + databaseName + "].sys.columns C");
            sqlStatementForAttributeVersion.AppendLine("	ON A.column_id=C.column_id AND A.OBJECT_ID=C.OBJECT_ID");
            sqlStatementForAttributeVersion.AppendLine("	JOIN [" + databaseName +
                                                       "].sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
            sqlStatementForAttributeVersion.AppendLine("	WHERE is_primary_key=1 ");
            sqlStatementForAttributeVersion.AppendLine(") keysub");
            sqlStatementForAttributeVersion.AppendLine("   ON OBJECT_NAME(main.OBJECT_ID) = keysub.TABLE_NAME");
            sqlStatementForAttributeVersion.AppendLine("  AND main.[name] = keysub.COLUMN_NAME");

            //Multi-active
            sqlStatementForAttributeVersion.AppendLine("-- Multi-Active");
            sqlStatementForAttributeVersion.AppendLine("LEFT OUTER JOIN (");
            sqlStatementForAttributeVersion.AppendLine("	SELECT ");
            sqlStatementForAttributeVersion.AppendLine("		sc.name AS TABLE_NAME,");
            sqlStatementForAttributeVersion.AppendLine("		C.name AS COLUMN_NAME");
            sqlStatementForAttributeVersion.AppendLine("	FROM [" + databaseName + "].sys.index_columns A");
            sqlStatementForAttributeVersion.AppendLine("	JOIN [" + databaseName + "].sys.indexes B");
            sqlStatementForAttributeVersion.AppendLine("	ON A.OBJECT_ID=B.OBJECT_ID AND A.index_id=B.index_id");
            sqlStatementForAttributeVersion.AppendLine("	JOIN [" + databaseName + "].sys.columns C");
            sqlStatementForAttributeVersion.AppendLine("	ON A.column_id=C.column_id AND A.OBJECT_ID=C.OBJECT_ID");
            sqlStatementForAttributeVersion.AppendLine("	JOIN [" + databaseName +
                                                       "].sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
            sqlStatementForAttributeVersion.AppendLine("	WHERE is_primary_key=1");
            sqlStatementForAttributeVersion.AppendLine("	AND C.name NOT IN ('" + effectiveDateTimeAttribute + "')");

            if (keyIdentifierLocation == "Prefix")
            {
                sqlStatementForAttributeVersion.AppendLine("	AND C.name NOT LIKE '" + dwhKeyIdentifier + "_%'");
            }
            else
            {
                sqlStatementForAttributeVersion.AppendLine("	AND C.name NOT LIKE '%_" + dwhKeyIdentifier + "'");
            }

            sqlStatementForAttributeVersion.AppendLine("	) ma");
            sqlStatementForAttributeVersion.AppendLine("	ON OBJECT_NAME(main.OBJECT_ID) = ma.TABLE_NAME");
            sqlStatementForAttributeVersion.AppendLine("	AND main.[name] = ma.COLUMN_NAME");


            //sqlStatementForAttributeVersion.AppendLine("WHERE OBJECT_NAME(main.OBJECT_ID) LIKE '" + prefix + "_%'");
            sqlStatementForAttributeVersion.AppendLine("WHERE 1=1");

            // Retrieve (and apply) the list of tables to filter from the Table Mapping datagrid
            sqlStatementForAttributeVersion.AppendLine("  AND (");


            var filterList = new List<Tuple<string, TeamConnection>>();
            foreach (DataRow row in ((DataTable) _bindingSourceTableMetadata.DataSource).Rows)
            {
                string localInternalConnectionIdSource =
                    row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                TeamConnection localConnectionSource = GetTeamConnectionByConnectionId(localInternalConnectionIdSource);

                string localInternalConnectionIdTarget =
                    row[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                TeamConnection localConnectionTarget = GetTeamConnectionByConnectionId(localInternalConnectionIdTarget);

                var localTupleSource = new Tuple<string, TeamConnection>(
                    (string) row[TableMappingMetadataColumns.SourceTable.ToString()], localConnectionSource);

                var localTupleTarget = new Tuple<string, TeamConnection>(
                    (string) row[TableMappingMetadataColumns.TargetTable.ToString()], localConnectionTarget);

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
                var fullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(filter.Item1, filter.Item2)
                    .FirstOrDefault();

                // Always add the 'regular' mapping.
                sqlStatementForAttributeVersion.AppendLine("  (OBJECT_NAME(main.OBJECT_ID) = '" +
                                                           fullyQualifiedName.Value +
                                                           "' AND OBJECT_SCHEMA_NAME(main.OBJECT_ID) = '" +
                                                           fullyQualifiedName.Key + "')");
                sqlStatementForAttributeVersion.AppendLine("  OR");


            }

            sqlStatementForAttributeVersion.Remove(sqlStatementForAttributeVersion.Length - 6, 6);
            sqlStatementForAttributeVersion.AppendLine();
            sqlStatementForAttributeVersion.AppendLine("  )");
            sqlStatementForAttributeVersion.AppendLine("ORDER BY main.column_id");

            var reverseEngineerResults = Utility.GetDataTable(ref conn, sqlStatementForAttributeVersion.ToString());
            conn.Close();
            return reverseEngineerResults;
        }

        #region ContextMenu

        private void dataGridViewTableMetadata_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hti = dataGridViewTableMetadata.HitTest(e.X, e.Y);

                // If the column in the selected cell is a Combobox and there are multiple cells selected then c\setup a separate context menu
                //var currentCell = dataGridViewTableMetadata.Rows[hti.RowIndex].Cells[hti.ColumnIndex];
                //var currentColumn = dataGridViewTableMetadata.Columns[hti.ColumnIndex];

                //if (currentColumn.GetType() == typeof(DataGridViewComboBoxColumn))
                //{
                //    var selectedCellCollection = dataGridViewTableMetadata.SelectedCells;


                //    //if (currentCell.Value != DBNull.Value)
                //    //{
                //    //    string currentValue = (string) currentCell.Value;
                //    //}
                //}
                //else
                //{
                // Normal selection
                dataGridViewTableMetadata.ClearSelection();
                dataGridViewTableMetadata.Rows[hti.RowIndex].Selected = true;
                //}
            }
            //if (e.Button == MouseButtons.Left)
            //{
            //    var hti = dataGridViewTableMetadata.HitTest(e.X, e.Y);
            //    var currentCell = dataGridViewTableMetadata.Rows[hti.RowIndex].Cells[hti.ColumnIndex];

            //    if (currentCell.GetType() == typeof(DataGridViewComboBoxCell))
            //    {

            //       if (currentCell.Value != DBNull.Value)
            //       {
            //           string currentValue = (string)currentCell.Value;
            //        }
            //    }
            //}
        }

        private void dataGridViewAttributeMetadata_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hti = dataGridViewAttributeMetadata.HitTest(e.X, e.Y);
                dataGridViewAttributeMetadata.ClearSelection();
                dataGridViewAttributeMetadata.Rows[hti.RowIndex].Selected = true;
            }
        }

        private void dataGridViewModelMetadata_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hti = dataGridViewPhysicalModelMetadata.HitTest(e.X, e.Y);
                dataGridViewPhysicalModelMetadata.ClearSelection();
                dataGridViewPhysicalModelMetadata.Rows[hti.RowIndex].Selected = true;
            }
        }

        /// <summary>
        /// This method is called from the context menu on the data grid. It exports the selected row to Json.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ExportThisRowAsSourceToTargetInterfaceJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();

            // Check if any cells were clicked / selected.
            Int32 selectedRow = dataGridViewTableMetadata.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            var generationMetadataRow = ((DataRowView) dataGridViewTableMetadata.Rows[selectedRow].DataBoundItem).Row;

            var targetDataObjectName =
                generationMetadataRow[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
            var targetConnectionInternalId =
                generationMetadataRow[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
            var targetConnection = GetTeamConnectionByConnectionId(targetConnectionInternalId);
            var targetFullyQualifiedName = MetadataHandling
                .GetFullyQualifiedDataObjectName(targetDataObjectName, targetConnection).FirstOrDefault();
            var tableType = MetadataHandling.GetDataObjectType(targetDataObjectName, "", TeamConfiguration);

            if (tableType != MetadataHandling.TableTypes.Presentation)
            {

                List<DataRow> generationMetadataList = new List<DataRow>();
                generationMetadataList.Add(generationMetadataRow);
                GenerateJsonFromPattern(generationMetadataList);
            }
            else
            {
                ManageFormJsonInteraction(targetDataObjectName, JsonExportSetting);
            }
        }

        /// <summary>
        /// This method is called from the context menu on the data grid. It deletes the row from the grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteThisRowFromTableDataGridToolStripMenuItem_Click(object sender, EventArgs e)
        {


            var selectedRows = dataGridViewTableMetadata.SelectedRows;

            foreach (DataGridViewRow bla in selectedRows)
            {
                if (bla.IsNewRow)
                {

                }
                else
                {
                    Int32 rowToDelete = dataGridViewTableMetadata.Rows.GetFirstRow(DataGridViewElementStates.Selected);
                    dataGridViewTableMetadata.Rows.RemoveAt(rowToDelete);
                }
            }


        }

        #endregion

        /// <summary>
        ///   Run the validation checks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerValidation_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // Handling multi-threading
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                _alertValidation.SetTextLogging("Commencing validation on available metadata according to settings in in the validation screen.\r\n\r\n");
                MetadataParameters.ValidationIssues = 0;

                if (ValidationSetting.DataObjectExistence == "True")
                {
                    ValidateObjectExistence();
                }

                worker?.ReportProgress(10);


                if (ValidationSetting.SourceBusinessKeyExistence == "True")
                {
                    ValidateBusinessKeyObject();
                }

                worker?.ReportProgress(20);


                if (ValidationSetting.DataItemExistence == "True")
                {
                    ValidateAttributeExistence();
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
                _alertValidation.SetTextLogging("\r\n\r\nIn total " + MetadataParameters.ValidationIssues + " validation issues have been found.");
            }
        }

        internal static class MetadataParameters
        {
            // TEAM core path parameters
            public static int ValidationIssues { get; set; }
            public static bool ValidationRunning { get; set; }
        }


        /// <summary>
        /// This method runs a check against the Attribute Mappings DataGrid to assert if model metadata is available for the attributes. The attribute needs to exist somewhere, either in the physical model or in the model metadata in order for activation to run successfully.
        /// </summary>
        /// <param name="area"></param>
        private void ValidateSchemaConfiguration()
        {
            var localDataTable = (DataTable) _bindingSourceTableMetadata.DataSource;

            // Informing the user.
            _alertValidation.SetTextLogging($"--> Commencing the validation to check if connection settings align with schemas entered in the Data Object mapping grid.\r\n");

            int resultCounter = 0;
            
            foreach (DataRow row in localDataTable.Rows)
            {
                if ((bool) row[TableMappingMetadataColumns.Enabled.ToString()]) // If row is enabled
                {
                    string localSourceConnectionInternalId = row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                    string localTargetConnectionInternalId = row[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();

                    TeamConnection sourceConnection = GetTeamConnectionByConnectionId(localSourceConnectionInternalId);
                    TeamConnection targetConnection = GetTeamConnectionByConnectionId(localTargetConnectionInternalId);

                    // The values in the data grid, fully qualified. This means the default schema is added if necessary.
                    var sourceDataObject = MetadataHandling
                        .GetFullyQualifiedDataObjectName(row[TableMappingMetadataColumns.SourceTable.ToString()].ToString(), sourceConnection)
                        .FirstOrDefault();
                    var targetDataObject = MetadataHandling
                        .GetFullyQualifiedDataObjectName(
                            row[TableMappingMetadataColumns.TargetTable.ToString()].ToString(), targetConnection)
                        .FirstOrDefault();

                    // The values as defined in the associated connections
                    var sourceSchemaNameForConnection = TeamConfiguration
                        .GetTeamConnectionByInternalId(localSourceConnectionInternalId,
                            TeamConfiguration.ConnectionDictionary).DatabaseServer.SchemaName.Replace("[", "")
                        .Replace("]", "");
                    ;
                    var targetSchemaNameForConnection = TeamConfiguration
                        .GetTeamConnectionByInternalId(localTargetConnectionInternalId,
                            TeamConfiguration.ConnectionDictionary).DatabaseServer.SchemaName.Replace("[", "")
                        .Replace("]", "");
                    ;


                    if (sourceDataObject.Key.Replace("[", "").Replace("]", "") != sourceSchemaNameForConnection)
                    {
                        _alertValidation.SetTextLogging($"--> Inconsistency detected for {sourceDataObject.Value} between the schema definition in the table grid {sourceDataObject.Key} and its assigned connection {sourceSchemaNameForConnection}.\r\n");
                        resultCounter++;
                    }

                    if (targetDataObject.Key.Replace("[", "").Replace("]", "") != targetSchemaNameForConnection)
                    {
                        _alertValidation.SetTextLogging($"--> Inconsistency for {targetDataObject.Value} detected between the schema definition in the table grid {targetDataObject.Key} and its assigned connection {targetSchemaNameForConnection}.\r\n");
                        resultCounter++;
                    }

                }

            }

            if (resultCounter == 0)
            {
                _alertValidation.SetTextLogging("     There were no validation issues related to schema configuration.\r\n\r\n");
            }

        }


        /// <summary>
        /// This method runs a check against the Attribute Mappings DataGrid to assert if model metadata is available for the attributes. The attribute needs to exist somewhere, either in the physical model or in the model metadata in order for activation to run successfully.
        /// </summary>
        /// <param name="area"></param>
        private void ValidateAttributeExistence()
        {
            // Informing the user.
            _alertValidation.SetTextLogging($"--> Commencing the validation to determine if the attributes in the metadata exists in the model.\r\n");

            var resultList = new Dictionary<string, string>();

            var localDataItemTable = (DataTable) _bindingSourceAttributeMetadata.DataSource;
            var localDataObjectTable = (DataTable) _bindingSourceTableMetadata.DataSource;

            foreach (DataRow row in localDataItemTable.Rows)
            {
                // Look for the corresponding Data Object Mapping row.
                var dataObjectRow = GetDataObjectMappingFromDataItemMapping(localDataObjectTable,
                    row[AttributeMappingMetadataColumns.SourceTable.ToString()].ToString(),
                    row[AttributeMappingMetadataColumns.TargetTable.ToString()].ToString());


                if (dataObjectRow.Item1) //If the corresponding Data Object is enabled
                {
                    string objectValidated = "";
                    var validationObjectSource =
                        row[AttributeMappingMetadataColumns.SourceColumn.ToString()].ToString();
                    TeamConnection sourceConnection = dataObjectRow.Item3;
                    var validationAttributeSource =
                        row[AttributeMappingMetadataColumns.SourceColumn.ToString()].ToString();

                    var validationObjectTarget = row[AttributeMappingMetadataColumns.TargetTable.ToString()].ToString();
                    TeamConnection targetConnection = dataObjectRow.Item5;
                    var validationAttributeTarget =
                        row[AttributeMappingMetadataColumns.TargetColumn.ToString()].ToString();

                    if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode &&
                        MetadataHandling.GetDataObjectType(validationObjectSource, "", TeamConfiguration)
                            .ToString() !=
                        MetadataHandling.TableTypes.Source.ToString()
                    ) // No need to evaluate the operational system (real sources)
                    {
                        // Check the source
                        try
                        {
                            objectValidated =
                                MetadataValidation.ValidateAttributeExistencePhysical(validationObjectSource,
                                    validationAttributeSource, sourceConnection);

                            // Add negative results to dictionary
                            if (objectValidated == "False")
                            {
                                resultList.Add(validationAttributeSource,
                                    objectValidated); // Add objects that did not pass the test
                            }
                        }
                        catch (Exception ex)
                        {
                            _alertValidation.SetTextLogging(
                                $"An issue was encountered running the validation check. The message is:\r\n\r\n{ex}.\r\n");
                        }

                        // Check the target
                        try
                        {
                            objectValidated =
                                MetadataValidation.ValidateAttributeExistencePhysical(validationObjectTarget,
                                    validationAttributeTarget, targetConnection);

                            // Add negative results to dictionary
                            if (objectValidated == "False")
                            {
                                resultList.Add(validationAttributeTarget,
                                    objectValidated); // Add objects that did not pass the test
                            }
                        }
                        catch (Exception ex)
                        {
                            _alertValidation.SetTextLogging(
                                $"An issue was encountered running the validation check. The message is:\r\n\r\n{ex}.\r\n");
                        }

                    }
                    else if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode &&
                             MetadataHandling.GetDataObjectType(validationObjectSource, "", TeamConfiguration)
                                 .ToString() !=
                             MetadataHandling.TableTypes.Source.ToString()
                    ) // No need to evaluate the operational system (real sources)
                    {
                        objectValidated = "";

                        objectValidated = MetadataValidation.ValidateAttributeExistenceVirtual(validationObjectSource,
                            validationAttributeSource, sourceConnection,
                            (DataTable) _bindingSourcePhysicalModelMetadata.DataSource);
                        // Add negative results to dictionary
                        if (objectValidated == "False")
                        {
                            resultList.Add(validationAttributeSource,
                                objectValidated); // Add objects that did not pass the test
                        }

                        objectValidated = MetadataValidation.ValidateAttributeExistenceVirtual(validationObjectTarget,
                            validationAttributeTarget, targetConnection,
                            (DataTable) _bindingSourcePhysicalModelMetadata.DataSource);

                        // Add negative results to dictionary
                        if (objectValidated == "False")
                        {
                            resultList.Add(validationAttributeTarget,
                                objectValidated); // Add objects that did not pass the test
                        }
                    }
                    else
                    {
                        objectValidated = "     The validation approach (physical/virtual) could not be asserted.";
                    }
                }
            }

            // Return the results back to the user
            if (resultList.Count > 0)
            {
                foreach (var objectValidationResult in resultList)
                {
                    _alertValidation.SetTextLogging("     " + objectValidationResult.Key +
                                                    " is tested with this outcome: " + objectValidationResult.Value +
                                                    "\r\n");
                }

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count;

                _alertValidation.SetTextLogging("\r\n");
            }
            else
            {
                _alertValidation.SetTextLogging(
                    $"     There were no validation issues related to the existence of the attribute(s).\r\n\r\n");
            }
        }

        /// <summary>
        /// Create a dictionary of all target data objects and whether they are enabled in metadata or not (bool).
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, bool> GetEnabledForDataObject()
        {
            Dictionary<string, bool> returnDictionary = new Dictionary<string, bool>();

            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (row.IsNewRow == false)
                {
                    string targetDataObject = row.Cells[TableMappingMetadataColumns.TargetTable.ToString()].Value.ToString();
                    bool rowEnabled = (bool) row.Cells[TableMappingMetadataColumns.Enabled.ToString()].Value;

                    if (rowEnabled)
                    {
                        returnDictionary[targetDataObject] = rowEnabled;
                    }
                }
            }

            return returnDictionary;
        }


        /// <summary>
        /// This method runs a check against the DataGrid to assert if model metadata is available for the object. The object needs to exist somewhere, either in the physical model or in the model metadata in order for activation to run succesfully.
        /// </summary>
        private void ValidateObjectExistence()
        {
            // Informing the user.
            _alertValidation.SetTextLogging(
                $"--> Commencing the validation to determine if the defined Data Objects exists in the model in {GlobalParameters.EnvironmentMode} mode.\r\n");

            var resultList = new Dictionary<string, string>();

            // Iterating over the grid
            var localDataTable = (DataTable) _bindingSourceTableMetadata.DataSource;
            foreach (DataRow row in localDataTable.Rows)
            {
                if ((bool) row[TableMappingMetadataColumns.Enabled.ToString()])
                {
                    // Sources
                    var validationObjectSource = row[TableMappingMetadataColumns.SourceTable.ToString()].ToString();
                    var validationObjectSourceConnectionId = row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                    var sourceConnection = GetTeamConnectionByConnectionId(validationObjectSourceConnectionId);
                    KeyValuePair<string, string> fullyQualifiedValidationObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(validationObjectSource, sourceConnection).FirstOrDefault();

                    // Targets
                    var validationObjectTarget = row[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                    var validationObjectTargetConnectionId =
                        row[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                    var targetConnection = GetTeamConnectionByConnectionId(validationObjectTargetConnectionId);
                    KeyValuePair<string, string> fullyQualifiedValidationObjectTarget = MetadataHandling
                        .GetFullyQualifiedDataObjectName(validationObjectTarget, targetConnection).FirstOrDefault();

                    // No need to evaluate the operational system (real sources))
                    if (MetadataHandling.GetDataObjectType(validationObjectSource, "", TeamConfiguration)
                        .ToString() != MetadataHandling.TableTypes.Source.ToString())
                    {
                        string objectValidated;
                        if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode)
                        {
                            try
                            {
                                objectValidated =
                                    MetadataValidation.ValidateObjectExistencePhysical(
                                        fullyQualifiedValidationObjectSource, sourceConnection);
                                // Add negative results to dictionary
                                if (objectValidated == "False" && !resultList.ContainsKey(
                                    fullyQualifiedValidationObjectSource.Key + '.' +
                                    fullyQualifiedValidationObjectSource.Value))
                                {
                                    resultList.Add(
                                        fullyQualifiedValidationObjectSource.Key + '.' +
                                        fullyQualifiedValidationObjectSource.Value,
                                        objectValidated); // Add objects that did not pass the test
                                }

                                objectValidated =
                                    MetadataValidation.ValidateObjectExistencePhysical(
                                        fullyQualifiedValidationObjectTarget, targetConnection);
                                // Add negative results to dictionary
                                if (objectValidated == "False" && !resultList.ContainsKey(
                                    fullyQualifiedValidationObjectTarget.Key + '.' +
                                    fullyQualifiedValidationObjectTarget.Value))
                                {
                                    resultList.Add(
                                        fullyQualifiedValidationObjectTarget.Key + '.' +
                                        fullyQualifiedValidationObjectTarget.Value,
                                        objectValidated); // Add objects that did not pass the test
                                }
                            }
                            catch (Exception ex)
                            {
                                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                                    $"An issue occurred connecting to the database: \r\n\r\n {ex}."));
                            }
                        }
                        else if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode)
                        {

                            objectValidated = MetadataValidation.ValidateObjectExistenceVirtual(validationObjectSource,
                                sourceConnection, (DataTable) _bindingSourcePhysicalModelMetadata.DataSource);
                            if (objectValidated == "False" && !resultList.ContainsKey(
                                fullyQualifiedValidationObjectSource.Key + '.' +
                                fullyQualifiedValidationObjectSource.Value))
                            {
                                resultList.Add(
                                    fullyQualifiedValidationObjectSource.Key + '.' +
                                    fullyQualifiedValidationObjectSource.Value,
                                    objectValidated); // Add objects that did not pass the test
                            }

                            objectValidated = MetadataValidation.ValidateObjectExistenceVirtual(validationObjectTarget,
                                targetConnection, (DataTable) _bindingSourcePhysicalModelMetadata.DataSource);
                            if (objectValidated == "False" && !resultList.ContainsKey(
                                fullyQualifiedValidationObjectTarget.Key + '.' +
                                fullyQualifiedValidationObjectTarget.Value))
                            {
                                resultList.Add(
                                    fullyQualifiedValidationObjectTarget.Key + '.' +
                                    fullyQualifiedValidationObjectTarget.Value,
                                    objectValidated); // Add objects that did not pass the test
                            }
                        }
                        else
                        {
                            GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning,
                                $"The validation approach (physical/virtual) could not be asserted."));
                        }
                    }
                }
            }

            // Return the results back to the user
            if (resultList.Count > 0)
            {
                foreach (var objectValidationResult in resultList)
                {
                    _alertValidation.SetTextLogging(
                        $"     {objectValidationResult.Key} is tested with outcome {objectValidationResult.Value}. This may be because the schema is defined differently in the connection, or because it simply does not exist.\r\n");
                }

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count;
                _alertValidation.SetTextLogging("\r\n");
            }
            else
            {
                _alertValidation.SetTextLogging(
                    $"     There were no validation issues related to the (physical) existence of the defined Data Object in the model using {GlobalParameters.EnvironmentMode} mode.\r\n\r\n");
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
            var localDataTable = (DataTable) _bindingSourceTableMetadata.DataSource;
            foreach (DataRow row in localDataTable.Rows)
            {
                // If enabled and is a Staging Layer object
                if ((bool) row[TableMappingMetadataColumns.Enabled.ToString()] && MetadataHandling
                    .GetDataObjectType((string) row[TableMappingMetadataColumns.TargetTable.ToString()], "",
                        TeamConfiguration).In(MetadataHandling.TableTypes.StagingArea,
                        MetadataHandling.TableTypes.PersistentStagingArea))
                {
                    if (row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()].ToString().Contains("'"))
                    {
                        issueCounter++;
                        _alertValidation.SetTextLogging(
                            $"     Data Object {(string) row[TableMappingMetadataColumns.TargetTable.ToString()]} should not contain hard-coded values in the Business Key definition. This can not be supported in the Staging Layer (Staging Area and Persistent Staging Area)");
                    }
                }
            }

            if (issueCounter == 0)
            {
                _alertValidation.SetTextLogging(
                    $"     There were no validation issues related to the definition of hard-coded Business Key components.\r\n\r\n");
            }

            MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + issueCounter;
        }

        internal void ValidateAttributeDataObjectsForTableMappings()
        {
            _alertValidation.SetTextLogging($"--> Commencing the validation to see if all data item (attribute) mappings exist as data object (table) mapping also (if enabled in the grid).\r\n");
            int issueCounter = 0;

            var localDataTableTableMappings = (DataTable) _bindingSourceTableMetadata.DataSource;
            var localDataTableAttributeMappings = (DataTable) _bindingSourceAttributeMetadata.DataSource;

            // Create a list of all sources and targets for the Data Object mappings
            List<Tuple<string,bool>> sourceDataObjectListTableMapping = new List<Tuple<string, bool>>();
            List<Tuple<string, bool>> targetDataObjectListTableMapping = new List<Tuple<string, bool>>();

            foreach (DataRow row in localDataTableTableMappings.Rows)
            {
                var sourceDataObjectTuple = new Tuple<string, bool>((string)row[TableMappingMetadataColumns.SourceTable.ToString()], (bool) row[TableMappingMetadataColumns.Enabled.ToString()]);
                var targetDataObjectTuple = new Tuple<string, bool>((string)row[TableMappingMetadataColumns.TargetTable.ToString()], (bool)row[TableMappingMetadataColumns.Enabled.ToString()]);

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
                var localSource = (string) row[AttributeMappingMetadataColumns.SourceTable.ToString()];
                var localTarget = (string) row[AttributeMappingMetadataColumns.TargetTable.ToString()];

                // If the value exists, but is disabled just a warning is sufficient.
                // If the value does not exist for an enabled mapping or at all, then it's an error.
                
                if (sourceDataObjectListTableMapping.Contains(new Tuple<string, bool>(localSource, false)))
                {
                    //_alertValidation.SetTextLogging($"     Data Object {localSource} in the attribute mappings exists in the table mappings for an disabled mapping. This can be disregarded.\r\n");
                }
                else if (sourceDataObjectListTableMapping.Contains(new Tuple<string, bool>(localSource, true)))
                {
                    // No problem, it's found
                }
                else 
                {
                    _alertValidation.SetTextLogging($"     Data Object {localSource} in the attribute mappings (source) does not seem to exist in the table mappings for an enabled mapping. Please check if this name is mapped at table level in the grid also.\r\n");
                    issueCounter++;
                }

                if (targetDataObjectListTableMapping.Contains(new Tuple<string, bool>(localTarget, false)))
                {
                    //_alertValidation.SetTextLogging($"     Data Object {localTarget} in the attribute mappings exists in the table mappings for an disabled mapping. This can be disregarded.\r\n");
                }
                else if (targetDataObjectListTableMapping.Contains(new Tuple<string, bool>(localTarget, true)))
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

            MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + issueCounter;
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
            var localDataTableTableMappings = (DataTable)_bindingSourceTableMetadata.DataSource;
            var objectList = new List<Tuple<string, string, string, string>>();
            
            foreach (DataRow row in localDataTableTableMappings.Rows)
            {
                if ((bool) row[TableMappingMetadataColumns.Enabled.ToString()])
                {
                    // Only select the lines that relate to a Link target.
                    if (row[TableMappingMetadataColumns.TargetTable.ToString()].ToString().StartsWith(TeamConfiguration.LinkTablePrefixValue))
                    {
                        // Derive the business key.
                        var businessKey = row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()].ToString().Replace("''''", "'");

                        // Derive the connection
                        localConnectionDictionary.TryGetValue(row[TableMappingMetadataColumns.TargetConnection.ToString()].ToString(), out var connectionValue);
                        
                        var newValidationObject = new Tuple<string, string, string, string>
                        (
                            row[TableMappingMetadataColumns.SourceTable.ToString()].ToString(),
                            row[TableMappingMetadataColumns.TargetTable.ToString()].ToString(),
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
                var sourceObjectValidated = MetadataValidation.ValidateLinkKeyOrder(sourceObject,
                    (DataTable) _bindingSourceTableMetadata.DataSource,
                    (DataTable) _bindingSourcePhysicalModelMetadata.DataSource, GlobalParameters.EnvironmentMode);

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

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count();
                _alertValidation.SetTextLogging("\r\n");
            }
            else
            {
                _alertValidation.SetTextLogging(
                    "     There were no validation issues related to order of business keys in the Link tables.\r\n\r\n");
            }
        }


        internal void ValidateBasicDataVaultAttributeExistence()
        {
            // Informing the user.
            _alertValidation.SetTextLogging("--> Commencing the validation to check if basic Data Vault attributes are present.\r\n");

            List<Tuple<string,string,bool>> masterResultList = new List<Tuple<string, string, bool>>();
            
            var localDataTable = (DataTable)_bindingSourceTableMetadata.DataSource;
            foreach (DataRow row in localDataTable.Rows)
            {
                if ((bool)row[TableMappingMetadataColumns.Enabled.ToString()])
                {
                    var localDataObjectSourceName = row[TableMappingMetadataColumns.SourceTable.ToString()].ToString();
                    var localDataObjectSourceConnectionId = row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                    var localDataObjectSourceConnection = GetTeamConnectionByConnectionId(localDataObjectSourceConnectionId);
                    var localDataObjectSourceTableType = MetadataHandling.GetDataObjectType(localDataObjectSourceName, "", TeamConfiguration);
                    
                    var localDataObjectTargetName = row[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                    var localDataObjectTargetConnectionId = row[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                    var localDataObjectTargetConnection = GetTeamConnectionByConnectionId(localDataObjectTargetConnectionId);
                    var localDataObjectTargetTableType = MetadataHandling.GetDataObjectType(localDataObjectTargetName, "", TeamConfiguration);

                    // Source
                    if (localDataObjectSourceTableType == MetadataHandling.TableTypes.CoreBusinessConcept ||
                        localDataObjectSourceTableType == MetadataHandling.TableTypes.Context ||
                        localDataObjectSourceTableType == MetadataHandling.TableTypes.NaturalBusinessRelationship ||
                        localDataObjectSourceTableType == MetadataHandling.TableTypes.NaturalBusinessRelationshipContext ||
                        localDataObjectSourceTableType == MetadataHandling.TableTypes.NaturalBusinessRelationshipContextDrivingKey)
                    {
                        var result = MetadataValidation.BasicDataVaultValidation(localDataObjectSourceName, localDataObjectSourceConnection, localDataObjectSourceTableType);
                        masterResultList.AddRange(result);
                        
                    }

                    // Target
                    if (localDataObjectTargetTableType == MetadataHandling.TableTypes.CoreBusinessConcept ||
                        localDataObjectTargetTableType == MetadataHandling.TableTypes.Context ||
                        localDataObjectTargetTableType == MetadataHandling.TableTypes.NaturalBusinessRelationship ||
                        localDataObjectTargetTableType == MetadataHandling.TableTypes.NaturalBusinessRelationshipContext ||
                        localDataObjectTargetTableType == MetadataHandling.TableTypes.NaturalBusinessRelationshipContextDrivingKey)
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
        /// Checks if all the supporting mappings are available (e.g. a Context table also needs a Core Business Concept present.
        /// </summary>
        internal void ValidateLogicalGroup()
        {
            #region Retrieving the Integration Layer tables

            // Informing the user.
            _alertValidation.SetTextLogging("--> Commencing the validation to check if the functional dependencies (logical group / unit of work) are present.\r\n");

            // Creating a list of tables which are dependent on other tables being present
            var objectList = new List<Tuple<string, string, string>>();


            var localDataTable = (DataTable) _bindingSourceTableMetadata.DataSource;
            foreach (DataRow row in localDataTable.Rows)
            {
                if ((bool) row[TableMappingMetadataColumns.Enabled.ToString()])
                {
                    if ((row[TableMappingMetadataColumns.TargetTable.ToString()].ToString()
                             .StartsWith(TeamConfiguration.LinkTablePrefixValue) ||
                         row[TableMappingMetadataColumns.TargetTable.ToString()].ToString()
                             .StartsWith(TeamConfiguration.SatTablePrefixValue) ||
                         row[TableMappingMetadataColumns.TargetTable.ToString()].ToString()
                             .StartsWith(TeamConfiguration.LsatTablePrefixValue)))
                    {
                        var businessKey = row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()].ToString()
                            .Replace("''''", "'");
                        if (!objectList.Contains(new Tuple<string, string, string>(
                            row[TableMappingMetadataColumns.SourceTable.ToString()].ToString(),
                            row[TableMappingMetadataColumns.TargetTable.ToString()].ToString(), businessKey)))
                        {
                            objectList.Add(new Tuple<string, string, string>(
                                row[TableMappingMetadataColumns.SourceTable.ToString()].ToString(),
                                row[TableMappingMetadataColumns.TargetTable.ToString()].ToString(), businessKey));
                        }
                    }
                }
            }

            // Execute the validation check using the list of unique objects
            var resultList = new Dictionary<string, bool>();

            foreach (var sourceObject in objectList)
            {
                // The validation check returns a Dictionary
                var sourceObjectValidated = MetadataValidation.ValidateLogicalGroup(sourceObject,
                    (DataTable) _bindingSourceTableMetadata.DataSource);

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

                _alertValidation.SetTextLogging("\r\n");
                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count();
            }
            else
            {
                _alertValidation.SetTextLogging("     There were no validation issues related to order of business keys in the Link tables.\r\n\r\n");
            }
        }

        /// <summary>
        ///   A validation check to make sure the Business Key is available in the source model.
        /// </summary>
        private void ValidateBusinessKeyObject()
        {
            var resultList = new Dictionary<Tuple<string, string>, bool>();

            // Informing the user.
            _alertValidation.SetTextLogging(
                "--> Commencing the validation to determine if the Business Key metadata attributes exist in the physical model.\r\n");


            var localDataTable = (DataTable) _bindingSourceTableMetadata.DataSource;
            foreach (DataRow row in localDataTable.Rows)
            {
                if ((bool) row[TableMappingMetadataColumns.Enabled.ToString()]) // If row is enabled
                {
                    Dictionary<Tuple<string, string>, bool> objectValidated =
                        new Dictionary<Tuple<string, string>, bool>();

                    // Source table and business key definitions.
                    string validationObject = row[TableMappingMetadataColumns.SourceTable.ToString()].ToString();
                    string validationConnectionId =
                        row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                    TeamConnection validationConnection = GetTeamConnectionByConnectionId(validationConnectionId);
                    string businessKeyDefinition =
                        row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()].ToString();


                    if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode &&
                        MetadataHandling.GetDataObjectType(validationObject, "", TeamConfiguration)
                            .ToString() !=
                        MetadataHandling.TableTypes.Source.ToString()
                    ) // No need to evaluate the operational system (real sources)
                    {
                        try
                        {
                            objectValidated =
                                MetadataValidation.ValidateSourceBusinessKeyExistencePhysical(validationObject,
                                    businessKeyDefinition, validationConnection);
                        }
                        catch
                        {
                            _alertValidation.SetTextLogging(
                                "     An issue occurred connecting to the database while looking up physical model references.\r\n");
                        }
                    }
                    else if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode)
                    {
                        // Exclude a lookup to the source
                        if (MetadataHandling.GetDataObjectType(validationObject, "", TeamConfiguration)
                            .ToString() != MetadataHandling.TableTypes.Source.ToString())
                        {
                            objectValidated = MetadataValidation.ValidateSourceBusinessKeyExistenceVirtual(
                                validationObject, businessKeyDefinition, validationConnection,
                                (DataTable) _bindingSourcePhysicalModelMetadata.DataSource);
                        }
                    }
                    else
                    {
                        if (MetadataHandling.GetDataObjectType(validationObject, "", TeamConfiguration)
                                .ToString() !=
                            MetadataHandling.TableTypes.Source.ToString())
                        {
                            _alertValidation.SetTextLogging(
                                "     The validation approach (physical/virtual) could not be asserted.\r\n");
                        }
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

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count();
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

        private void deleteThisRowFromTheGridToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Int32 rowToDelete = dataGridViewAttributeMetadata.Rows.GetFirstRow(DataGridViewElementStates.Selected);
            dataGridViewAttributeMetadata.Rows.RemoveAt(rowToDelete);
        }

        private void deleteThisRowFromTheGridToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Int32 rowToDelete = dataGridViewPhysicalModelMetadata.Rows.GetFirstRow(DataGridViewElementStates.Selected);
            dataGridViewPhysicalModelMetadata.Rows.RemoveAt(rowToDelete);
        }

        private void displayTableScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Retrieve the index of the selected row
            Int32 selectedRow = dataGridViewPhysicalModelMetadata.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            DataTable gridDataTable = (DataTable) _bindingSourcePhysicalModelMetadata.DataSource;
            DataTable dt2 = gridDataTable.Clone();
            dt2.Columns[PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString()].DataType =
                Type.GetType("System.Int32");

            foreach (DataRow dr in gridDataTable.Rows)
            {
                dt2.ImportRow(dr);
            }

            dt2.AcceptChanges();

            // Make sure the output is sorted
            dt2.DefaultView.Sort =
                $"{PhysicalModelMappingMetadataColumns.TableName.ToString()} ASC, {PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString()} ASC";

            // Retrieve all rows relative to the selected row (e.g. all attributes for the table)
            IEnumerable<DataRow> rows = dt2.DefaultView.ToTable().AsEnumerable().Where(r =>
                r.Field<string>(PhysicalModelMappingMetadataColumns.TableName.ToString()) ==
                dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[4].Value.ToString()
                && r.Field<string>(PhysicalModelMappingMetadataColumns.SchemaName.ToString()) ==
                dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[3].Value.ToString()
                && r.Field<string>(PhysicalModelMappingMetadataColumns.DatabaseName.ToString()) ==
                dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[2].Value.ToString()
            );

            // Create a form and display the results
            var results = new StringBuilder();

            _generatedScripts = new Form_Alert();
            _generatedScripts.SetFormName("Display model metadata");
            _generatedScripts.Canceled += buttonCancel_Click;
            _generatedScripts.Show();

            results.AppendLine("IF OBJECT_ID('[" + dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[4].Value +
                               "]', 'U') IS NOT NULL");
            results.AppendLine(
                "DROP TABLE [" + dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[4].Value + "]");
            results.AppendLine();
            results.AppendLine("CREATE TABLE [" + dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[4].Value +
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
                results.AppendLine(commaSnippet + row[PhysicalModelMappingMetadataColumns.ColumnName.ToString()] +
                                   " -- with ordinal position of " +
                                   row[PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString()]);
            }

            results.AppendLine(")");

            _generatedScripts.SetTextLogging(results.ToString());
            _generatedScripts.ProgressValue = 100;
            _generatedScripts.Message = "Done";
        }



        /// <summary>
        /// Convenience method to encapsulate all UI interactions. Needs to be merged further (interim solution).
        /// </summary>
        private void ManageFormOverallJsonExport()
        {
            richTextBoxInformation.Clear();

            // Take all the rows from the grid
            List<DataRow> rowList = new List<DataRow>();

            // Exclude presentation layer for now, working on a new approach for this.
            var localDataTable = (DataTable) _bindingSourceTableMetadata.DataSource;
            foreach (DataRow row in localDataTable.Rows)
            {
                bool enabled = (bool) row[TableMappingMetadataColumns.Enabled.ToString()];
                var targetDataObjectName = row[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                var tableType = MetadataHandling.GetDataObjectType(targetDataObjectName, "", TeamConfiguration);
                if (enabled && tableType != MetadataHandling.TableTypes.Presentation)
                {
                    rowList.Add(row); //add the row to the list
                }
                else if (enabled && tableType == MetadataHandling.TableTypes.Presentation)
                {
                    ManageFormJsonInteraction(targetDataObjectName, JsonExportSetting);
                }
            }

            // Do all the regular stuff
            GenerateJsonFromPattern(rowList);
        }

        /// <summary>
        /// Convenience method to do all the form stuff related to Json generation, such as saving and showing the status form, in one go.
        /// </summary>
        /// <param name="targetDataObjectName"></param>
        /// <param name="jsonExportSettings"></param>
        private void ManageFormJsonInteraction(string targetDataObjectName, JsonExportSetting jsonExportSettings)
        {
            var dataObjectMappings = GenerateJson(targetDataObjectName, jsonExportSettings);

            if (checkBoxShowJsonOutput.Checked)
            {
                _generatedJsonInterface = new Form_Alert();
                _generatedJsonInterface.SetFormName("Exporting the metadata as Json files");
                _generatedJsonInterface.ShowProgressBar(false);
                _generatedJsonInterface.ShowCancelButton(false);
                _generatedJsonInterface.ShowLogButton(false);
                _generatedJsonInterface.ShowProgressLabel(false);
                _generatedJsonInterface.Show();

                _generatedJsonInterface.SetTextLogging(dataObjectMappings + "\r\n\r\n");
            }

            // Spool the output to disk
            if (checkBoxSaveInterfaceToJson.Checked)
            {
                TeamUtility.SaveTextToFile(GlobalParameters.OutputPath + targetDataObjectName + ".json",
                    dataObjectMappings);
                richTextBoxInformation.AppendText(
                    $"The file {GlobalParameters.OutputPath}{targetDataObjectName}.json has been created.");
            }
        }

        /// <summary>
        /// New, WIP method to generate Data Warehouse Automation Json files, based on the name of the target Data Object.
        /// </summary>
        /// <param name="targetDataObjectNameFromDataGrid"></param>
        /// <param name="jsonExportSetting"></param>
        private string GenerateJson(string targetDataObjectNameFromDataGrid, JsonExportSetting jsonExportSetting)
        {
            var localDataObjectMappingDataTable = (DataTable) _bindingSourceTableMetadata.DataSource;
            var localDataItemMappingDataTable = (DataTable) _bindingSourceAttributeMetadata.DataSource;

            var mappingRows = localDataObjectMappingDataTable.Select($"[{TableMappingMetadataColumns.TargetTable}] = '{targetDataObjectNameFromDataGrid}'");

            List<DataObjectMapping> dataObjectMappings = new List<DataObjectMapping>();
            DataObjectMapping dataObjectMapping = new DataObjectMapping();
            dataObjectMapping.enabled = true;
            dataObjectMapping.mappingName = targetDataObjectNameFromDataGrid;

            List<dynamic> sourceDataObjects = new List<dynamic>();
            List<DataItemMapping> dataItemMappings = new List<DataItemMapping>();

            int counter = 0;
            foreach (DataRow row in mappingRows)
            {
                // Get the full details of the target first, and only once
                if (counter == 0)
                {
                    var targetDataObjectName = row[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                    var targetConnectionInternalId = row[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                    var targetConnection = GetTeamConnectionByConnectionId(targetConnectionInternalId);
                    var targetFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObjectName, targetConnection).FirstOrDefault();

                    // Create and set target Data Object.
                    dataObjectMapping.targetDataObject = JsonOutputHandling.CreateDataObject(targetDataObjectName, targetConnection, JsonExportSetting);

                    // Also add the classification at Data Object Mapping level, as this is derived from the target Data Object.
                    var tableType = MetadataHandling.GetDataObjectType(targetDataObjectName, "", TeamConfiguration);

                    List<Classification> dataObjectsMappingClassifications = new List<Classification>();
                    var dataObjectMappingClassification = new Classification();
                    dataObjectMappingClassification.classification = tableType.ToString();
                    dataObjectsMappingClassifications.Add(dataObjectMappingClassification);

                    dataObjectMapping.mappingClassifications = dataObjectsMappingClassifications;

                    // Business key, also only needs to be set once
                    List<BusinessKey> businessKeys = new List<BusinessKey>();
                    BusinessKey businessKey = new BusinessKey();

                    var businessKeyDefinition = row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()].ToString();
                    var businessKeyDataItemMappings = JsonOutputHandling.GetBusinessKeyComponentDataItemMappings(businessKeyDefinition, businessKeyDefinition);
                    businessKey.businessKeyComponentMapping = businessKeyDataItemMappings;

                    // Derive the surrogate key using conventions.
                    var targetDataObjectSurrogateKey = MetadataHandling.GetSurrogateKey(targetDataObjectName, targetConnection, TeamConfiguration);
                    businessKey.surrogateKey = targetDataObjectSurrogateKey;

                    businessKeys.Add(businessKey);
                    dataObjectMapping.businessKeys = businessKeys;
                }

                // Get the source info
                var sourceDataObjectName = row[TableMappingMetadataColumns.SourceTable.ToString()].ToString();
                var sourceConnectionInternalId = row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                var sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionInternalId);
                var sourceFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(sourceDataObjectName, sourceConnection).FirstOrDefault();

                var sourceDataObject = JsonOutputHandling.CreateDataObject(sourceFullyQualifiedName.Value, sourceConnection, jsonExportSetting);

                //// Create the classifications for the source Data Objects
                //List<Classification> sourceDataObjectClassifications = new List<Classification>();
                //var sourceDataObjectClassification = new Classification();
                //sourceDataObjectClassification.classification = MetadataHandling.GetDataObjectType(sourceDataObjectName, "", TeamConfigurationSettings).ToString();
                //sourceDataObjectClassifications.Add(sourceDataObjectClassification);
                //sourceDataObject.dataObjectClassification = sourceDataObjectClassifications;

                // Generate data items for each source, and for the mapping overall
                List<dynamic> sourceDataItems = new List<dynamic>();

                var dataItemRows = localDataItemMappingDataTable.Select($"[{AttributeMappingMetadataColumns.SourceTable}] = '{sourceDataObjectName}' AND [{AttributeMappingMetadataColumns.TargetTable}] = '{targetDataObjectNameFromDataGrid}'");
                foreach (DataRow dataItemRow in dataItemRows)
                {
                    var sourceDataItem = new DataItem();
                    List<dynamic> sourceDataItemLocalList = new List<dynamic>(); // Creating a single source-to-target Data Item mapping
                    var targetDataItem = new DataItem();

                    var localSourceDataItemFromGrid = dataItemRow[AttributeMappingMetadataColumns.SourceColumn.ToString()].ToString();
                    var localTargetDataItemFromGrid = dataItemRow[AttributeMappingMetadataColumns.TargetColumn.ToString()].ToString();

                    sourceDataItem.name = localSourceDataItemFromGrid;
                    sourceDataItems.Add(sourceDataItem);
                    sourceDataItemLocalList.Add(sourceDataItem);

                    targetDataItem.name = localTargetDataItemFromGrid;

                    DataItemMapping dataItemMapping = new DataItemMapping();
                    dataItemMapping.sourceDataItems = sourceDataItemLocalList;
                    dataItemMapping.targetDataItem = targetDataItem;

                    dataItemMappings.Add(dataItemMapping);
                }

                if (sourceDataItems.Count != 0)
                {
                    sourceDataObject.dataItems = sourceDataItems;
                }

                // Add extensions (database and schema)
                //sourceDataObject = JsonOutputHandling.SetDataObjectDatabaseExtension(sourceDataObject, sourceConnection, jsonExportSetting);
                sourceDataObjects.Add(sourceDataObject);

                counter++;
            }

            // Add the source data objects
            dataObjectMapping.sourceDataObjects = sourceDataObjects;
            
            // Related Data Objects
            List<DataWarehouseAutomation.DataObject> relatedDataObjects = new List<DataWarehouseAutomation.DataObject>();

            // Add the metadata connection as related data object (assuming this is set in the json export settings).
            var metaDataObject = JsonOutputHandling.CreateMetadataDataObject(TeamConfiguration.MetadataConnection, JsonExportSetting);
            if (metaDataObject.name != null)
            {
                relatedDataObjects.Add(metaDataObject);
            }

            dataObjectMapping.relatedDataObjects = relatedDataObjects;
            
            

            // Add the data item mappings
            dataObjectMapping.dataItemMappings = dataItemMappings;

            // Adding the Data Object Mapping to the list of Data Object Mappings (the top level object)
            dataObjectMappings.Add(dataObjectMapping);

            
            
            // Create an instance of the non-generic information i.e. VDW specific. For example the generation date/time.
            GenerationSpecificMetadata vedwMetadata = new GenerationSpecificMetadata(targetDataObjectNameFromDataGrid);
            MetadataConfiguration metadataConfiguration = new MetadataConfiguration(TeamConfiguration);

            VDW_DataObjectMappingList sourceTargetMappingList = new VDW_DataObjectMappingList();
            sourceTargetMappingList.dataObjectMappings = dataObjectMappings;
            sourceTargetMappingList.generationSpecificMetadata = vedwMetadata;
            sourceTargetMappingList.metadataConfiguration = metadataConfiguration;


            var jsonOutputAsString = JsonConvert.SerializeObject(sourceTargetMappingList, Formatting.Indented);

            return jsonOutputAsString;
        }

        /// <summary>
        /// Creates a Json schema based on the Data Warehouse Automation interface definition.
        /// </summary>
        /// <param name="generationMetadataList"></param>
        private void GenerateJsonFromPattern(List<DataRow> generationMetadataList)
        {
            // Set up the form in case the show Json output checkbox has been selected
            if (checkBoxShowJsonOutput.Checked)
            {
                _generatedJsonInterface = new Form_Alert();
                _generatedJsonInterface.SetFormName("Exporting the metadata as Json files");
                _generatedJsonInterface.ShowProgressBar(false);
                _generatedJsonInterface.ShowCancelButton(false);
                _generatedJsonInterface.ShowLogButton(false);
                _generatedJsonInterface.ShowProgressLabel(false);
                _generatedJsonInterface.Show();
            }

            int mappingCounter = 0;

            EventLog eventLog = new EventLog();
            SqlConnection conn = new SqlConnection
            {
                ConnectionString = TeamConfiguration.MetadataConnection.CreateSqlServerConnectionString(false)
            };

            foreach (DataRow metadataRow in generationMetadataList)
            {
                #region Preparation

                var sourceDataObjectName = metadataRow[TableMappingMetadataColumns.SourceTable.ToString()].ToString();
                var targetDataObjectName = metadataRow[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                var sourceConnectionInternalId = metadataRow[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                var targetConnectionInternalId = metadataRow[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                var drivingKeyDefinition = metadataRow[TableMappingMetadataColumns.DrivingKeyDefinition.ToString()].ToString();
                

                // Find out what the correct patterns is.
                var tableType = MetadataHandling.GetDataObjectType(targetDataObjectName, drivingKeyDefinition, TeamConfiguration);
                LoadPatternDefinition loadPatternDefinition = GlobalParameters.PatternDefinitionList.FirstOrDefault(item => item.LoadPatternType == tableType.ToString());

                // Exception handling, if null then break
                if (loadPatternDefinition == null)
                {
                    var outputMessage = $"No Json interface file was created for the mapping from '{sourceDataObjectName}' to '{targetDataObjectName}' because its type could not be asserted.";
                    GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, outputMessage));
                    richTextBoxInformation.AppendText(outputMessage + "\r\n");
                }

                // Retrieve the source-to-target mappings (base query).
                if (loadPatternDefinition != null)
                {
                    DataTable metadataDataTable = new DataTable();
                    try
                    {
                        var metadataQuery = loadPatternDefinition.LoadPatternBaseQuery;
                        metadataDataTable = Utility.GetDataTable(ref conn, metadataQuery);
                    }
                    catch (Exception ex)
                    {
                        eventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                            "The source-to-target mapping list could not be retrieved (baseQuery in PatternDefinition file). The error message is " +
                            ex + ".\r\n"));
                    }


                    // Can contain multiple rows in metadata, because multiple sources can be mapped to a target.
                    DataRow[] mappingRows = null;

                    var sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionInternalId);
                    var targetConnection = GetTeamConnectionByConnectionId(targetConnectionInternalId);

                    var fullyQualifiedNameSource = MetadataHandling.GetFullyQualifiedDataObjectName(sourceDataObjectName, sourceConnection).FirstOrDefault();
                    var fullyQualifiedNameTarget = MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObjectName, targetConnection).FirstOrDefault();

                    mappingRows = metadataDataTable.Select("[TARGET_NAME] = '" + fullyQualifiedNameTarget.Value + "' AND [TARGET_SCHEMA_NAME]='" + fullyQualifiedNameTarget.Key + "' ");

                    // Populate the additional business key information (i.e. links)
                    var additionalBusinessKeyQuery = loadPatternDefinition.LoadPatternAdditionalBusinessKeyQuery;
                    var additionalBusinessKeyDataTable = Utility.GetDataTable(ref conn, additionalBusinessKeyQuery);

                    // Select the right mapping and map the metadata to the DWH automation schema
                    richTextBoxInformation.AppendText(@"Processing generation for " + targetDataObjectName + ".\r\n");

                    // Create the list of Data Object Mappings, the top array of the file that needs to be created.
                    List<DataObjectMapping> dataObjectMappingList = new List<DataObjectMapping>();

                    // Retrieve the physical model.
                    // Add data type details, if requested.
                    var physicalModelQuery = @"SELECT
                                                           [DATABASE_NAME]
                                                          ,[SCHEMA_NAME]
                                                          ,[TABLE_NAME]
                                                          ,[COLUMN_NAME]
                                                          ,[DATA_TYPE]
                                                          ,[CHARACTER_MAXIMUM_LENGTH]
                                                          ,[NUMERIC_PRECISION]
                                                          ,[NUMERIC_SCALE]
                                                          ,[ORDINAL_POSITION]
                                                          ,[PRIMARY_KEY_INDICATOR]
                                                      FROM [interface].[INTERFACE_PHYSICAL_MODEL]";

                    var physicalModelDataTable = Utility.GetDataTable(ref conn, physicalModelQuery);

                    #endregion

                    // Now, it's time to iterate over the right scope of mappings.
                    if (mappingRows != null)
                    {
                        foreach (DataRow row in mappingRows)
                        {
                            // Create the individual Data Object Mapping
                            var sourceToTargetMapping = new DataObjectMapping();

                            // Enabled flag
                            sourceToTargetMapping.enabled = true;

                            // Data Object Mapping name
                            sourceToTargetMapping.mappingName = (string) row["TARGET_NAME"]; // Source-to-target mapping name

                            #region Data Objects

                            var sourceDataObjects = new List<dynamic>();
                            var sourceDataObject = JsonOutputHandling.CreateDataObject((string) row["SOURCE_NAME"], sourceConnection, JsonExportSetting);
                            var targetDataObject = JsonOutputHandling.CreateDataObject((string) row["TARGET_NAME"], targetConnection, JsonExportSetting);

                            sourceDataObjects.Add(sourceDataObject);
                            sourceToTargetMapping.sourceDataObjects = sourceDataObjects;
                            sourceToTargetMapping.targetDataObject = targetDataObject;

                            #endregion

                            #region Related Data Objects (e.g. lookup tables, references)
                            List<DataWarehouseAutomation.DataObject> relatedDataObjects = new List<DataWarehouseAutomation.DataObject>();

                            var dependentRows = TableMapping.GetPeerDataRows((string) row["SOURCE_NAME"], (string) row["TARGET_NAME"], (string) row["SOURCE_BUSINESS_KEY_DEFINITION"], TableMapping.DataTable, TeamTableMapping.BusinessKeyEvaluationMode.Partial);

                            foreach (var dependentRow in dependentRows)
                            {
                                var localRelatedDataObjectName = dependentRow[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                                var localRelatedDataObjectConnectionId = dependentRow[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                                var localRelatedDataObjectConnection = GetTeamConnectionByConnectionId(localRelatedDataObjectConnectionId);

                                var relatedDataObject = JsonOutputHandling.CreateDataObject(localRelatedDataObjectName, localRelatedDataObjectConnection, JsonExportSetting);

                                relatedDataObjects.Add(relatedDataObject);
                            }
                            

                            // Add the metadata connection as related data object (assuming this is set in the json export settings).
                            var metaDataObject = JsonOutputHandling.CreateMetadataDataObject(TeamConfiguration.MetadataConnection, JsonExportSetting);
                            if (metaDataObject.name != null)
                            {
                                relatedDataObjects.Add(metaDataObject);
                            }

                            // Add upstream related Data Objects (assuming this is set in the json export settings).
                            relatedDataObjects.AddRange(JsonOutputHandling.SetLineageRelatedDataObjectList((DataTable)_bindingSourceTableMetadata.DataSource, targetDataObjectName, JsonExportSetting));



                            // If the list contains entries, add it to the mapping.
                            if (relatedDataObjects.Count > 0)
                            {
                                sourceToTargetMapping.relatedDataObjects = relatedDataObjects;
                            }

                            #endregion

                            #region Business Keys

                            // Creating the Business Key definition, using the available components (see above).
                            List<BusinessKey> businessKeyList = new List<BusinessKey>();
                            BusinessKey businessKey = new BusinessKey
                            {
                                businessKeyComponentMapping = JsonOutputHandling.GetBusinessKeyComponentDataItemMappings
                                (
                                    (string) row["SOURCE_BUSINESS_KEY_DEFINITION"],
                                    (string) row["TARGET_BUSINESS_KEY_DEFINITION"]
                                ),
                                surrogateKey = (string) row["SURROGATE_KEY"]
                            };

                            // Create the classifications at Data Item (target) level, to capture if this attribute is a Multi-Active attribute.
                            if (row.Table.Columns.Contains("DRIVING_KEY_SOURCE"))
                            {
                                if (row["DRIVING_KEY_SOURCE"].ToString().Length > 0)
                                {
                                    // Update the existing Business Key with a classification if a Driving Key exists.
                                    foreach (var localDataItemMapping in businessKey.businessKeyComponentMapping)
                                    {
                                        if (localDataItemMapping.sourceDataItems[0].name ==
                                            (string) row["DRIVING_KEY_SOURCE"])
                                        {
                                            List<Classification> dataItemClassificationList =
                                                new List<Classification>();
                                            var dataItemClassification = new Classification();
                                            dataItemClassification.classification = "DrivingKey";
                                            dataItemClassification.notes =
                                                "The attribute that triggers (drives) the closing of a relationship.";
                                            dataItemClassificationList.Add(dataItemClassification);
                                            localDataItemMapping.sourceDataItems[0].dataItemClassification =
                                                dataItemClassificationList;
                                        }
                                    }
                                }
                            }

                            businessKeyList.Add(businessKey);

                            #endregion

                            #region Additional Business Keys

                            if (additionalBusinessKeyDataTable != null &&
                                additionalBusinessKeyDataTable.Rows.Count > 0)
                            {
                                DataRow[] additionalBusinessKeyRows =
                                    additionalBusinessKeyDataTable.Select("[TARGET_NAME] = '" +
                                                                          targetDataObjectName + "'");

                                foreach (DataRow additionalKeyRow in additionalBusinessKeyRows)
                                {
                                    var hubBusinessKey = new BusinessKey();

                                    hubBusinessKey.businessKeyComponentMapping =
                                        JsonOutputHandling.GetBusinessKeyComponentDataItemMappings(
                                            (string) additionalKeyRow["SOURCE_BUSINESS_KEY_DEFINITION"],
                                            (string) additionalKeyRow["TARGET_BUSINESS_KEY_DEFINITION"]);
                                    hubBusinessKey.surrogateKey = (string) additionalKeyRow["TARGET_KEY_NAME"];

                                    if ((string) additionalKeyRow["HUB_NAME"] == "N/A")
                                    {
                                        // Classification (degenerate field)
                                        List<Classification> businesskeyClassificationList =
                                            new List<Classification>();
                                        var businesskeyClassification = new Classification();
                                        businesskeyClassification.classification = "DegenerateAttribute";
                                        businesskeyClassification.notes =
                                            "Non Core Business Concept attribute, though part of the Relationship Key.";
                                        businesskeyClassificationList.Add(businesskeyClassification);

                                        hubBusinessKey.businessKeyClassification = businesskeyClassificationList;
                                    }

                                    businessKeyList.Add(hubBusinessKey); // Adding the Link Business Key
                                }
                            }


                            sourceToTargetMapping.businessKeys = businessKeyList; // Business Key


                            // Set the data types, if required
                            foreach (var localBusinessKey in sourceToTargetMapping.businessKeys)
                            {
                                foreach (var component in businessKey.businessKeyComponentMapping)
                                {

                                    if (JsonExportSetting.GenerateSourceDataItemTypes == "True")
                                    {
                                        Dictionary<string, string> tableSchema =
                                            MetadataHandling.GetFullyQualifiedDataObjectName(
                                                sourceToTargetMapping.sourceDataObjects[0].name, sourceConnection);

                                        DataRow[] physicalModelRow = physicalModelDataTable.Select(
                                            "[TABLE_NAME] = '" + tableSchema.Values.FirstOrDefault() +
                                            "' AND [SCHEMA_NAME] = '" + tableSchema.Keys.FirstOrDefault() +
                                            "' AND " +
                                            "[COLUMN_NAME] = '" +
                                            component.sourceDataItems[0].name.Replace("'", "") +
                                            "'"
                                        );
                                        if (physicalModelRow.Length > 0)
                                        {
                                            PrepareDataTypes(component.sourceDataItems[0], physicalModelRow);
                                        }

                                    }

                                    try
                                    {
                                        if (JsonExportSetting.GenerateTargetDataItemTypes == "True")
                                        {
                                            var tableSchema =
                                                MetadataHandling.GetFullyQualifiedDataObjectName(
                                                    sourceToTargetMapping.targetDataObject.name, targetConnection);

                                            DataRow[] physicalModelRow = physicalModelDataTable.Select(
                                                "[TABLE_NAME] = '" + tableSchema.Values.FirstOrDefault() +
                                                "' AND [SCHEMA_NAME] = '" + tableSchema.Keys.FirstOrDefault() +
                                                "' AND " +
                                                "[COLUMN_NAME] = '" + component.targetDataItem.name + "'"
                                            );
                                            if (physicalModelRow.Length > 0)
                                            {
                                                PrepareDataTypes(component.targetDataItem, physicalModelRow);
                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogMetadataEvent(
                                            $"An issue has occurred during generation of the Json files: \r\n\r\n {ex}.",
                                            EventTypes.Error);
                                    }

                                }
                            }



                            #endregion

                            #region Data Item Mapping (column to column mappings)

                            // Create the column-to-column mapping.

                            // Populate the attribute mappings
                            // Create the column-to-column mapping
                            var columnMetadataQuery = loadPatternDefinition.LoadPatternAttributeQuery;
                            var columnMetadataDataTable = Utility.GetDataTable(ref conn, columnMetadataQuery);

                            List<DataItemMapping> dataItemMappingList = new List<DataItemMapping>();

                            if (columnMetadataDataTable != null && columnMetadataDataTable.Rows.Count > 0)
                            {
                                DataRow[] columnRows = columnMetadataDataTable.Select(
                                    "[TARGET_NAME] = '" + fullyQualifiedNameTarget.Value + "' " +
                                    "AND [TARGET_SCHEMA_NAME] = '" + fullyQualifiedNameTarget.Key + "' " +
                                    "AND [SOURCE_NAME] = '" + fullyQualifiedNameSource.Value + "' " +
                                    "AND [SOURCE_SCHEMA_NAME] = '" + fullyQualifiedNameSource.Key + "'");

                                foreach (DataRow column in columnRows)
                                {
                                    DataItemMapping dataItemMapping = new DataItemMapping();

                                    List<dynamic> sourceDataItems = new List<dynamic>();

                                    DataItem sourceDataItem = new DataItem();
                                    DataItem targetDataItem = new DataItem();

                                    sourceDataItem.name = (string) column["SOURCE_ATTRIBUTE_NAME"];
                                    sourceDataItem.isHardCodedValue = sourceDataItem.name.StartsWith("'") &&
                                                                      sourceDataItem.name.EndsWith("'");
                                    targetDataItem.name = (string) column["TARGET_ATTRIBUTE_NAME"];

                                    if (JsonExportSetting.GenerateSourceDataItemTypes == "True")
                                    {
                                        var tableSchema =
                                            MetadataHandling.GetFullyQualifiedDataObjectName(sourceDataObjectName,
                                                sourceConnection);

                                        try
                                        {
                                            DataRow[] physicalModelRow = physicalModelDataTable.Select(
                                                "[TABLE_NAME] = '" + tableSchema.Values.FirstOrDefault() +
                                                "' AND [SCHEMA_NAME] = '" + tableSchema.Keys.FirstOrDefault() +
                                                "' AND " +
                                                "[COLUMN_NAME] = '" +
                                                MetadataHandling.QuoteStringValuesForAttributes(
                                                    (string) column["SOURCE_ATTRIBUTE_NAME"]) + "'"
                                            );
                                            if (physicalModelRow.Length > 0)
                                            {
                                                PrepareDataTypes(sourceDataItem, physicalModelRow);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            LogMetadataEvent(
                                                $"An issue has occurred during generation of the Json files: \r\n\r\n {ex}.",
                                                EventTypes.Error);
                                        }

                                    }

                                    if (JsonExportSetting.GenerateTargetDataItemTypes == "True")
                                    {
                                        var tableSchema =
                                            MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObjectName,
                                                targetConnection);

                                        DataRow[] physicalModelRow = physicalModelDataTable.Select(
                                            "[TABLE_NAME] = '" + tableSchema.Values.FirstOrDefault() +
                                            "' AND [SCHEMA_NAME] = '" + tableSchema.Keys.FirstOrDefault() +
                                            "' AND " +
                                            "[COLUMN_NAME] = '" +
                                            MetadataHandling.QuoteStringValuesForAttributes(
                                                (string) column["TARGET_ATTRIBUTE_NAME"]) + "'"
                                        );
                                        if (physicalModelRow.Length > 0)
                                        {
                                            PrepareDataTypes(targetDataItem, physicalModelRow);
                                        }
                                    }

                                    sourceDataItems.Add(sourceDataItem);

                                    dataItemMapping.sourceDataItems = sourceDataItems;
                                    dataItemMapping.targetDataItem = targetDataItem;

                                    // Adding Multi-Active Key classification
                                    if (column.Table.Columns.Contains("MULTI_ACTIVE_KEY_INDICATOR"))
                                    {
                                        if ((string) column["MULTI_ACTIVE_KEY_INDICATOR"] == "Y")
                                        {
                                            // Create the classifications at Data Item (target) level, to capture if this attribute is a Multi-Active attribute.
                                            List<Classification> dataItemClassificationList =
                                                new List<Classification>();
                                            var dataItemClassification = new Classification();
                                            dataItemClassification.classification = "MultiActive";
                                            dataItemClassification.notes =
                                                "A multi-active attribute is part of the target table key.";
                                            dataItemClassificationList.Add(dataItemClassification);

                                            // Add the classification to the target Data Item
                                            dataItemMapping.targetDataItem.dataItemClassification =
                                                dataItemClassificationList;
                                        }
                                    }

                                    // Adding NULL classification
                                    if ((string) column["SOURCE_ATTRIBUTE_NAME"] == "NULL")
                                    {
                                        // Create the classifications at Data Item (target) level, to capture if this attribute is a NULL.
                                        List<Classification> dataItemClassificationList =
                                            new List<Classification>();
                                        var dataItemClassification = new Classification();
                                        dataItemClassification.classification = "NULL value";
                                        dataItemClassificationList.Add(dataItemClassification);

                                        // Add the classification to the target Data Item
                                        dataItemMapping.sourceDataItems[0].dataItemClassification =
                                            dataItemClassificationList;
                                    }

                                    dataItemMappingList.Add(dataItemMapping);
                                }
                            }

                            #endregion

                            // Create the classifications at Data Object Mapping level.
                            List<Classification> dataObjectMappingClassificationList = new List<Classification>();
                            var dataObjectMappingClassification = new Classification();
                            dataObjectMappingClassification.id = loadPatternDefinition.LoadPatternKey;
                            dataObjectMappingClassification.classification = loadPatternDefinition.LoadPatternType;
                            dataObjectMappingClassification.notes = loadPatternDefinition.LoadPatternNotes;
                            dataObjectMappingClassificationList.Add(dataObjectMappingClassification);
                            
                            sourceToTargetMapping.mappingClassifications = dataObjectMappingClassificationList;


                            sourceToTargetMapping.filterCriterion =
                                (string) row["FILTER_CRITERIA"]; // Filter criterion

                            if (dataItemMappingList.Count == 0)
                            {
                                dataItemMappingList = null;
                            }

                            sourceToTargetMapping.dataItemMappings =
                                dataItemMappingList; // Column to column mapping

                            // Add the source-to-target mapping to the mapping list
                            dataObjectMappingList.Add(sourceToTargetMapping);
                        }
                    }
                    
                    #region Wrap-up

                    // Create an instance of the non-generic information i.e. VEDW specific. For example the generation date/time.
                    GenerationSpecificMetadata vedwMetadata = new GenerationSpecificMetadata(targetDataObjectName);
                    MetadataConfiguration metadataConfiguration = new MetadataConfiguration(TeamConfiguration);

                    VDW_DataObjectMappingList sourceTargetMappingList = new VDW_DataObjectMappingList();
                    sourceTargetMappingList.dataObjectMappings = dataObjectMappingList;
                    sourceTargetMappingList.generationSpecificMetadata = vedwMetadata;
                    sourceTargetMappingList.metadataConfiguration = metadataConfiguration;

                    // Check if the metadata needs to be displayed
                    try
                    {
                        var json = JsonConvert.SerializeObject(sourceTargetMappingList, Formatting.Indented);

                        if (checkBoxShowJsonOutput.Checked)
                        {
                            _generatedJsonInterface.SetTextLogging(json + "\r\n\r\n");
                        }

                        // Spool the output to disk
                        if (checkBoxSaveInterfaceToJson.Checked)
                        {
                            Event fileSaveEventLog = TeamUtility.SaveTextToFile(GlobalParameters.OutputPath + targetDataObjectName + ".json", json);
                            eventLog.Add(fileSaveEventLog);
                            mappingCounter++;
                        }
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.AppendText("An error was encountered while generating the Json metadata. The error message is: " + ex);
                    }

                    #endregion

                }
                else
                {
                    eventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The load pattern was not found for {targetDataObjectName}."));
                }

            }

            // Report back to the user
            int errorCounter = 0;
            foreach (Event individualEvent in eventLog)
            {
                // Only report errors at this stage, can be extended with debug checkbox.
                if (individualEvent.eventCode == 1)
                {
                    errorCounter++;
                    richTextBoxInformation.AppendText(individualEvent.eventDescription);
                }
            }

            // Report back to the user
            richTextBoxInformation.AppendText($"\r\n{errorCounter} errors have been found.\r\n");
            richTextBoxInformation.AppendText($"\r\n{mappingCounter} mapping(s) have been prepared.\r\n");

            // Spool the output to disk
            if (checkBoxSaveInterfaceToJson.Checked)
            {
                richTextBoxInformation.AppendText(
                    $"Associated scripts have been saved in {GlobalParameters.OutputPath}.\r\n");
            }

            richTextBoxInformation.ScrollToCaret();

            conn.Close();
            conn.Dispose();
        }

        private static void PrepareDataTypes(DataItem dataItem, DataRow[] physicalModelRow)
        {
            var dataType = physicalModelRow[0].ItemArray[4].ToString();
            dataItem.dataType = dataType;

            switch (dataType)
            {
                case "varchar":
                case "nvarchar":
                case "binary":
                    dataItem.characterLength = (int) physicalModelRow[0].ItemArray[5];
                    break;
                case "numeric":
                    dataItem.numericPrecision = (int) physicalModelRow[0].ItemArray[6];
                    dataItem.numericScale = (int) physicalModelRow[0].ItemArray[7];
                    break;
                case "int":
                    // No length etc.
                    break;
                case "datetime":
                case "datetime2":
                case "date":
                    dataItem.numericScale = (int) physicalModelRow[0].ItemArray[7];
                    break;
            }

            dataItem.ordinalPosition = (int) physicalModelRow[0].ItemArray[8];
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
                    richTextBoxInformation.Text =
                        "There is no value given for the Configuration Path. Please enter a valid path name.";
                }
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text =
                    "An error has occurred while attempting to open the configuration directory. The error message is: " +
                    ex;
            }
        }

        private void dataGridViewTableMetadata_EditingControlShowing(object sender,
            DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewComboBoxEditingControl tb)
            {
                tb.KeyDown -= DataGridViewTableMetadataKeyDown;
                tb.KeyDown += DataGridViewTableMetadataKeyDown;
            }
        }

        private void openAttributeMappingFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Attribute Mapping Metadata File",
                Filter = @"Attribute Mapping files|*.xml;*.json",
                InitialDirectory = GlobalParameters.ConfigurationPath //Application.StartupPath + @"\Configuration\"
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

                        dataGridViewAttributeMetadata.DataSource = dataSet.Tables[0];
                        _bindingSourceAttributeMetadata.DataSource = dataGridViewAttributeMetadata.DataSource;
                    }
                    else if (fileExtension == ".json")
                    {
                        // Create a backup file, if enabled
                        if (checkBoxBackupFiles.Checked)
                        {
                            try
                            {
                                var backupFile = new TeamJsonHandling();
                                var targetFileName = backupFile.BackupJsonFile(
                                    GlobalParameters.JsonAttributeMappingFileName + @"_v" +
                                    GlobalParameters.CurrentVersionId + ".json", GlobalParameters.ConfigurationPath);
                                richTextBoxInformation.Text = "A backup of the in-use JSON file was created as " +
                                                              targetFileName + ".\r\n\r\n";
                            }
                            catch (Exception exception)
                            {
                                richTextBoxInformation.Text =
                                    "An issue occurred when trying to make a backup of the in-use JSON file. The error message was " +
                                    exception + ".";
                            }
                        }

                        // If the information needs to be merged, a global parameter needs to be set.
                        // This will overwrite existing files for the in-use version.
                        if (!checkBoxMergeFiles.Checked)
                        {
                            TeamJsonHandling.JsonFileConfiguration.newFileAttributeMapping = "true";
                        }


                        // Load the file, convert it to a DataTable and bind it to the source
                        List<AttributeMappingJson> jsonArray =
                            JsonConvert.DeserializeObject<List<AttributeMappingJson>>(File.ReadAllText(chosenFile));
                        DataTable dt = Utility.ConvertToDataTable(jsonArray);

                        // Set the column names in the datatable.
                        //SetTeamDataTableProperties.SetAttributeDataTableColumns(dt);
                        // Sort the columns in the datatable.
                        //SetTeamDataTableProperties.SetAttributeDatTableSorting(dt);

                        // Clear out the existing data from the grid
                        _bindingSourceAttributeMetadata.DataSource = null;
                        _bindingSourceAttributeMetadata.Clear();
                        dataGridViewAttributeMetadata.DataSource = null;

                        // Bind the datatable to the gridview
                        _bindingSourceAttributeMetadata.DataSource = dt;

                        if (jsonArray != null)
                        {
                            // Set the column header names.
                            dataGridViewAttributeMetadata.DataSource = _bindingSourceAttributeMetadata;
                            dataGridViewAttributeMetadata.ColumnHeadersVisible = true;
                            dataGridViewAttributeMetadata.Columns[0].Visible = false;
                            dataGridViewAttributeMetadata.Columns[1].Visible = false;
                            dataGridViewAttributeMetadata.Columns[6].ReadOnly = false;

                            dataGridViewAttributeMetadata.Columns[0].HeaderText = "Hash Key";
                            dataGridViewAttributeMetadata.Columns[1].HeaderText = "Version ID";
                            dataGridViewAttributeMetadata.Columns[2].HeaderText = "Source Table";
                            dataGridViewAttributeMetadata.Columns[3].HeaderText = "Source Column";
                            dataGridViewAttributeMetadata.Columns[4].HeaderText = "Target Table";
                            dataGridViewAttributeMetadata.Columns[5].HeaderText = "Target Column";
                            dataGridViewAttributeMetadata.Columns[6].HeaderText = "Notes";
                        }
                    }

                    GridAutoLayoutAttributeMetadata();
                    richTextBoxInformation.AppendText("The metadata has been loaded from file.\r\n");
                    ContentCounter();
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.AppendText(
                        "An error has been encountered. Please check the Event Log for more details.\r\n");
                    GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                        $"An exception has been encountered: {ex}."));
                }
            }
        }

        private void saveAttributeMappingAsJSONToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var theDialog = new SaveFileDialog
                {
                    Title = @"Save Attribute Mapping Metadata File",
                    Filter = @"JSON files|*.json",
                    InitialDirectory = GlobalParameters.ConfigurationPath //Application.StartupPath + @"\Configuration\"
                };

                var ret = STAShowDialog(theDialog);

                if (ret == DialogResult.OK)
                {
                    try
                    {
                        var chosenFile = theDialog.FileName;

                        DataTable gridDataTable = (DataTable) _bindingSourceAttributeMetadata.DataSource;

                        // Make sure the output is sorted
                        gridDataTable.DefaultView.Sort =
                            "[SOURCE_TABLE] ASC, [SOURCE_COLUMN] ASC, [TARGET_TABLE] ASC, [TARGET_COLUMN] ASC";

                        gridDataTable.TableName = "AttributeMappingMetadata";

                        JArray outputFileArray = new JArray();
                        foreach (DataRow singleRow in gridDataTable.DefaultView.ToTable().Rows)
                        {
                            JObject individualRow = JObject.FromObject(new
                            {
                                attributeMappingHash = singleRow[0].ToString(),
                                versionId = singleRow[1].ToString(),
                                sourceTable = singleRow[2].ToString(),
                                sourceAttribute = singleRow[3].ToString(),
                                targetTable = singleRow[4].ToString(),
                                targetAttribute = singleRow[5].ToString(),
                                Notes = singleRow[6].ToString()
                            });
                            outputFileArray.Add(individualRow);
                        }

                        string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

                        File.WriteAllText(chosenFile, json);

                        richTextBoxInformation.Text = "The Attribute Mapping metadata file " + chosenFile +
                                                      " saved successfully.";
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.AppendText(
                            "An error has been encountered. Please check the Event Log for more details.\r\n");
                        GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                            $"An exception has been encountered: {ex}."));
                    }
                }
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText(
                    "An error has been encountered when attempting to save the file to disk. Please check the Event Log for more details.\r\n");
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    $"An exception has been encountered: {ex.Message}."));
            }
        }

        /// <summary>
        ///   Load a Table Mapping Metadata Json or XML file into the data grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openMetadataFileToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Data Object Mapping Metadata File",
                Filter = @"Data Object Mapping files|*.xml;*.json",
                InitialDirectory = GlobalParameters.ConfigurationPath //Application.StartupPath + @"\Configuration\"
            };

            var ret = STAShowDialog(theDialog);

            if (ret == DialogResult.OK)
            {
                richTextBoxInformation.Clear();
                try
                {
                    var chosenFile = theDialog.FileName;
                    var dataSet = new DataSet();

                    string fileExtension = Path.GetExtension(theDialog.FileName);

                    if (fileExtension == ".xml" || fileExtension == ".XML")
                    {
                        dataSet.ReadXml(chosenFile);

                        dataGridViewTableMetadata.DataSource = dataSet.Tables[0];
                        _bindingSourceTableMetadata.DataSource = dataGridViewTableMetadata.DataSource;

                    }
                    else if (fileExtension == ".json" || fileExtension == ".JSON")
                    {
                        // Create a backup file, if enabled
                        if (checkBoxBackupFiles.Checked)
                        {
                            try
                            {
                                var backupFile = new TeamJsonHandling();
                                var targetFileName = backupFile.BackupJsonFile(
                                    GlobalParameters.JsonTableMappingFileName + @"_v" +
                                    GlobalParameters.CurrentVersionId + ".json",
                                    FormBase.GlobalParameters.ConfigurationPath);
                                richTextBoxInformation.Text = "A backup of the in-use JSON file was created as " +
                                                              targetFileName + ".\r\n\r\n";
                            }
                            catch (Exception exception)
                            {
                                richTextBoxInformation.Text =
                                    "An issue occurred when trying to make a backup of the in-use JSON file. The error message was " +
                                    exception + ".";
                            }
                        }

                        // If the information needs to be merged, a global parameter needs to be set.
                        // This will overwrite existing files for the in-use version.
                        if (!checkBoxMergeFiles.Checked)
                        {
                            TeamJsonHandling.JsonFileConfiguration.newFileTableMapping = "true";
                        }

                        TableMapping.GetMetadata(chosenFile);

                        // Setup the datatable with proper headings.
                        TableMapping.SetDataTableColumns();

                        // Sort the columns
                        TableMapping.SetDataTableSorting();

                        // Clear out the existing data from the grid
                        _bindingSourceTableMetadata.DataSource = null;
                        _bindingSourceTableMetadata.Clear();

                        dataGridViewTableMetadata.DataSource = null;

                        // Bind the datatable to the gridview
                        _bindingSourceTableMetadata.DataSource = TableMapping.DataTable;

                        // Set the column header names
                        dataGridViewTableMetadata.DataSource = _bindingSourceTableMetadata;
                    }

                    GridAutoLayoutTableMappingMetadata();
                    ContentCounter();
                    richTextBoxInformation.AppendText("The file " + chosenFile + " was loaded.\r\n");
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.AppendText(
                        "An error has been encountered when attempting to save the file to disk. Please check the Event Log for more details.\r\n");
                    GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                        $"An exception has been encountered: {ex.Message}."));
                }
            }
        }

        private void openPhysicalModelFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Physical Model Metadata File",
                Filter = @"Physical Model files|*.xml;*.json",
                InitialDirectory = GlobalParameters.ConfigurationPath
            };

            var ret = STAShowDialog(theDialog);

            if (ret == DialogResult.OK)
            {
                richTextBoxInformation.Clear();
                try
                {
                    var chosenFile = theDialog.FileName;
                    var dataSet = new DataSet();

                    string fileExtension = Path.GetExtension(theDialog.FileName);

                    if (fileExtension == ".xml")
                    {
                        dataSet.ReadXml(chosenFile);

                        dataGridViewPhysicalModelMetadata.DataSource = dataSet.Tables[0];
                        _bindingSourcePhysicalModelMetadata.DataSource = dataGridViewPhysicalModelMetadata.DataSource;

                    }
                    else if (fileExtension == ".json")
                    {
                        // Create a backup file, if enabled
                        if (checkBoxBackupFiles.Checked)
                        {
                            try
                            {
                                var backupFile = new TeamJsonHandling();
                                var targetFileName = backupFile.BackupJsonFile(
                                    GlobalParameters.JsonModelMetadataFileName + @"_v" +
                                    GlobalParameters.CurrentVersionId + ".json",
                                    FormBase.GlobalParameters.ConfigurationPath);
                                richTextBoxInformation.Text = "A backup of the in-use Json file was created as " +
                                                              targetFileName + ".\r\n\r\n";
                            }
                            catch (Exception exception)
                            {
                                richTextBoxInformation.Text =
                                    "An issue occurred when trying to make a backup of the in-use Json file. The error message was " +
                                    exception + ".";
                            }
                        }

                        // If the information needs to be merged, a global parameter needs to be set.
                        // This will overwrite existing files for the in-use version.
                        if (!checkBoxMergeFiles.Checked)
                        {
                            TeamJsonHandling.JsonFileConfiguration.newFilePhysicalModel = "true";
                        }

                        PhysicalModel.GetMetadata(chosenFile);

                        // Setup the data table with proper headings.
                        PhysicalModel.SetDataTableColumns();

                        // Sort the columns
                        PhysicalModel.SetDataTableSorting();

                        // Clear out the existing data from the grid
                        _bindingSourcePhysicalModelMetadata.DataSource = null;
                        _bindingSourcePhysicalModelMetadata.Clear();

                        dataGridViewPhysicalModelMetadata.DataSource = null;

                        // Bind the data table to the grid view
                        _bindingSourcePhysicalModelMetadata.DataSource = PhysicalModel.DataTable;

                        // Set the column header names
                        dataGridViewPhysicalModelMetadata.DataSource = _bindingSourcePhysicalModelMetadata;
                    }

                    GridAutoLayoutPhysicalModelMetadata();
                    ContentCounter();

                    richTextBoxInformation.AppendText("The file " + chosenFile + " was loaded.\r\n");
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.AppendText(
                        "An error has been encountered when attempting to save the file to disk. Please check the Event Log for more details.\r\n");
                    GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                        $"An exception has been encountered: {ex.Message}."));
                }
            }
        }

        private void exportPhysicalModelFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var theDialog = new SaveFileDialog
                {
                    Title = @"Save Model Metadata File",
                    Filter = @"JSON files|*.json",
                    InitialDirectory = GlobalParameters.ConfigurationPath //Application.StartupPath + @"\Configuration\"
                };

                var ret = STAShowDialog(theDialog);

                if (ret == DialogResult.OK)
                {
                    try
                    {
                        var chosenFile = theDialog.FileName;

                        DataTable gridDataTable = (DataTable) _bindingSourcePhysicalModelMetadata.DataSource;

                        gridDataTable.DefaultView.Sort =
                            "[DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME] ASC, [ORDINAL_POSITION] ASC";

                        gridDataTable.TableName = "ModelMetadata";

                        JArray outputFileArray = new JArray();
                        foreach (DataRow singleRow in gridDataTable.DefaultView.ToTable().Rows)
                        {
                            JObject individualRow = JObject.FromObject(new
                            {
                                versionAttributeHash = singleRow[0].ToString(),
                                versionId = singleRow[1].ToString(),
                                databaseName = singleRow[2].ToString(),
                                schemaName = singleRow[3].ToString(),
                                tableName = singleRow[4].ToString(),
                                columnName = singleRow[5].ToString(),
                                dataType = singleRow[6].ToString(),
                                characterLength = singleRow[7].ToString(),
                                numericPrecision = singleRow[8].ToString(),
                                numericScale = singleRow[9].ToString(),
                                ordinalPosition = singleRow[10].ToString(),
                                primaryKeyIndicator = singleRow[11].ToString(),
                                multiActiveIndicator = singleRow[12].ToString()
                            });
                            outputFileArray.Add(individualRow);
                        }

                        string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

                        File.WriteAllText(chosenFile, json);

                        richTextBoxInformation.Text = "The model metadata file " + chosenFile + " saved successfully.";
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.AppendText(
                            "An error has been encountered when attempting to save the file to disk. Please check the Event Log for more details.\r\n");
                        GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                            $"An exception has been encountered: {ex.Message}."));
                    }
                }
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText(
                    "An error has been encountered when attempting to save the file to disk. Please check the Event Log for more details.\r\n");
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    $"An exception has been encountered: {ex.Message}."));
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

            var localEventLog = GlobalParameters.TeamEventLog;

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
                        _alertEventLog.SetTextLogging(
                            $"{individualEvent.eventTime} - {(EventTypes) individualEvent.eventCode}: {individualEvent.eventDescription}\r\n");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An issue occurred displaying the event log. The error message is: " + ex,
                        "An issue has occurred", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            if (GlobalParameters.EnvironmentMode == EnvironmentModes.VirtualMode &&
                _bindingSourcePhysicalModelMetadata.Count == 0)
            {
                richTextBoxInformation.Text +=
                    "There is no physical model metadata available, so the metadata can only be validated with the 'Ignore Version' enabled.\r\n ";
            }
            else
            {
                if (backgroundWorkerValidationOnly.IsBusy) return;
                // create a new instance of the alert form
                _alertValidation = new Form_Alert();
                // event handler for the Cancel button in AlertForm
                _alertValidation.Canceled += buttonCancel_Click;
                _alertValidation.Show();
                _alertValidation.ShowLogButton(false);
                _alertValidation.ShowCancelButton(false);
                // Start the asynchronous operation.
                backgroundWorkerValidationOnly.RunWorkerAsync();
            }
        }

        private void generateJsonInterfaceFilesOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ManageFormOverallJsonExport();
        }

        private void AutoMapDataItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControlDataMappings.SelectedTab = tabPageDataItemMapping;
            
            var dataTableAttributeMappingChanges = ((DataTable) _bindingSourceAttributeMetadata.DataSource).GetChanges();
            if (dataTableAttributeMappingChanges != null && dataTableAttributeMappingChanges.Rows.Count > 0)
            {
                string localMessage = "You have unsaved edits in the Data Item (attribute mapping) grid, please save your work before running the automap.";
                MessageBox.Show(localMessage);
                richTextBoxInformation.AppendText(localMessage);
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, localMessage));
            }
            else
            {
                // Get a stable version of the Data Objects from the grid.
                DataTable localDataObjectDataTable = (DataTable) _bindingSourceTableMetadata.DataSource;

                //dataTableChanges.AcceptChanges();
                ((DataTable) _bindingSourceAttributeMetadata.DataSource).AcceptChanges();

                // Iterate across all Data Object Mappings, to see if there are corresponding Data Item Mappings.
                foreach (DataRow dataObjectRow in localDataObjectDataTable.Rows)
                {
                    // Generic
                    var localVersionId = dataObjectRow[TableMappingMetadataColumns.VersionId.ToString()].ToString();

                    // Source Data Object details
                    var sourceDataObjectName = dataObjectRow[TableMappingMetadataColumns.SourceTable.ToString()].ToString();
                    var sourceConnectionId = dataObjectRow[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                    TeamConnection sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionId);
                    var sourceDataObjectFullyQualifiedKeyValuePair = MetadataHandling
                        .GetFullyQualifiedDataObjectName(sourceDataObjectName, sourceConnection).FirstOrDefault();

                    // Get the source details from the database
                    string tableFilterObjectsSource = $"OBJECT_ID(N'[{sourceConnection.DatabaseServer.DatabaseName}].{sourceDataObjectFullyQualifiedKeyValuePair.Key}.{sourceDataObjectFullyQualifiedKeyValuePair.Value}')";

                    var physicalModelInstantiationSource = new AttributeSelection();
                    var localSourceSqlConnection = new SqlConnection {ConnectionString = sourceConnection.CreateSqlServerConnectionString(false)};
                    var localSourceQuery = physicalModelInstantiationSource.CreatePhysicalModelSet(sourceConnection.DatabaseServer.DatabaseName, tableFilterObjectsSource).ToString();

                    DataTable localSourceDatabaseDataTable = Utility.GetDataTable(ref localSourceSqlConnection, localSourceQuery);


                    // Target Data Object details
                    var targetDataObjectName =
                        dataObjectRow[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                    var targetConnectionId =
                        dataObjectRow[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                    TeamConnection targetConnection = GetTeamConnectionByConnectionId(targetConnectionId);
                    var targetDataObjectFullyQualifiedKeyValuePair = MetadataHandling
                        .GetFullyQualifiedDataObjectName(targetDataObjectName, targetConnection).FirstOrDefault();

                    // Get the target details from the database
                    string tableFilterObjectsTarget =
                        $"OBJECT_ID(N'[{targetConnection.DatabaseServer.DatabaseName}].{targetDataObjectFullyQualifiedKeyValuePair.Key}.{targetDataObjectFullyQualifiedKeyValuePair.Value}')";

                    var physicalModelInstantiationTarget = new AttributeSelection();
                    var localTargetSqlConnection = new SqlConnection
                        {ConnectionString = targetConnection.CreateSqlServerConnectionString(false)};
                    var localTargetQuery = physicalModelInstantiationTarget
                        .CreatePhysicalModelSet(targetConnection.DatabaseServer.DatabaseName, tableFilterObjectsTarget)
                        .ToString();

                    DataTable localTargetDatabaseDataTable = Utility.GetDataTable(ref localTargetSqlConnection, localTargetQuery);


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
                            // There is a match and it's not a standard attribute

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
                                TeamConfiguration.AlternativeSatelliteLoadDateTimeAttribute
                            };

                            if (!exclusionAttribute.Contains(sourceDataObjectRow["COLUMN_NAME"].ToString()))
                            {
                                var localMapping = new TeamDataItemMappingRow
                                {
                                    sourceDataObjectName = sourceDataObjectName,
                                    sourceDataObjectConnectionId = sourceConnectionId,
                                    sourceDataItemName = sourceDataObjectRow["COLUMN_NAME"].ToString(),
                                    targetDataObjectName = targetDataObjectName,
                                    targetDataObjectConnectionId = targetConnectionId,
                                    targetDataItemName =
                                        sourceDataObjectRow["COLUMN_NAME"]
                                            .ToString() // Same as source, as it's a direct match on this value.
                                };

                                localDataItemMappings.Add(localMapping);
                            }
                        }
                    }

                    // Now, for each item in the matched list check if there is a corresponding Data Item Mapping in the grid already.
                    DataTable localDataItemDataTable = (DataTable) _bindingSourceAttributeMetadata.DataSource;

                    foreach (var matchedDataItemMappingFromDatabase in localDataItemMappings)
                    {
                        // Do the lookup in the target data item grid
                        var results =
                            from localRow in localDataItemDataTable.AsEnumerable()
                            where
                                localRow.Field<string>(AttributeMappingMetadataColumns.SourceTable.ToString()) ==
                                matchedDataItemMappingFromDatabase.sourceDataObjectName &&
                                localRow.Field<string>(AttributeMappingMetadataColumns.TargetTable.ToString()) ==
                                matchedDataItemMappingFromDatabase.targetDataObjectName &&
                                localRow.Field<string>(AttributeMappingMetadataColumns.SourceColumn.ToString()) ==
                                matchedDataItemMappingFromDatabase.sourceDataItemName &&
                                localRow.Field<string>(AttributeMappingMetadataColumns.TargetColumn.ToString()) ==
                                matchedDataItemMappingFromDatabase.targetDataItemName
                            select localRow;

                        if (results.FirstOrDefault() == null)
                        {
                            // There is NO match...
                            // Add the row as Data Item Mapping in the grid.



                            DataRow newRow = localDataItemDataTable.NewRow();

                            newRow[AttributeMappingMetadataColumns.HashKey.ToString()] =
                                Utility.CreateMd5(new string[] {Utility.GetRandomString(100)}, "#");
                            newRow[AttributeMappingMetadataColumns.VersionId.ToString()] = localVersionId;
                            newRow[AttributeMappingMetadataColumns.SourceTable.ToString()] =
                                matchedDataItemMappingFromDatabase.sourceDataObjectName;
                            newRow[AttributeMappingMetadataColumns.SourceColumn.ToString()] =
                                matchedDataItemMappingFromDatabase.sourceDataItemName;
                            newRow[AttributeMappingMetadataColumns.TargetTable.ToString()] =
                                matchedDataItemMappingFromDatabase.targetDataObjectName;
                            newRow[AttributeMappingMetadataColumns.TargetColumn.ToString()] =
                                matchedDataItemMappingFromDatabase.targetDataItemName;
                            newRow[AttributeMappingMetadataColumns.Notes.ToString()] = "Automatically matched";

                            localDataItemDataTable.Rows.Add(newRow);
                            //localDataItemDataTable.AcceptChanges();
                        }


                    }
                }
            }
        }
    }
}