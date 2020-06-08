using DataWarehouseAutomation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TEAM
{
    public partial class FormManageMetadata : FormBase
    {
        Form_Alert _alert;
        Form_Alert _alertValidation;
        Form_Alert _generatedScripts;
        Form_Alert _generatedJsonInterface;

        //Getting the DataTable to bind to something
        private BindingSource _bindingSourceTableMetadata = new BindingSource();
        private BindingSource _bindingSourceAttributeMetadata = new BindingSource();
        private BindingSource _bindingSourcePhysicalModelMetadata = new BindingSource();

        public FormManageMetadata()
        {
            InitializeComponent();
        }

        internal static class FileConfiguration
        {
            internal static string newFileTableMapping { get; set; }
            internal static string newFileAttributeMapping { get; set; }
            internal static string newFilePhysicalModel { get; set; }
            internal static string jsonVersionExtension { get; set; }
        }

        public FormManageMetadata(FormMain parent) : base(parent)
        {
            InitializeComponent();

            radiobuttonNoVersionChange.Checked = true;
            MetadataParameters.ValidationIssues = 0;
            MetadataParameters.ValidationRunning = false;

            labelHubCount.Text = "0 Hubs";
            labelSatCount.Text = "0 Satellites";
            labelLnkCount.Text = "0 Links";
            labelLsatCount.Text = "0 Link-Satellites";

            radiobuttonNoVersionChange.Checked = true;

            // Retrieve the version from the database
            var connOmd = new SqlConnection {ConnectionString = ConfigurationSettings.ConnectionStringOmd};
            var selectedVersion = GetMaxVersionId(connOmd);
            
            // Set the version in memory
            GlobalParameters.CurrentVersionId = selectedVersion;
            GlobalParameters.HighestVersionId = selectedVersion; // On startup, the highest version is the same as the current version
            FileConfiguration.jsonVersionExtension = @"_v" + selectedVersion + ".json";

            trackBarVersioning.Maximum = selectedVersion;
            trackBarVersioning.TickFrequency = GetVersionCount();

            //Make sure the version is displayed
            var versionMajorMinor = GetVersion(selectedVersion, connOmd);
            var majorVersion = versionMajorMinor.Key;
            var minorVersion = versionMajorMinor.Value;

            trackBarVersioning.Value = selectedVersion;
            labelVersion.Text = majorVersion + "." + minorVersion;

            //  Load the grids from the repository
            richTextBoxInformation.Clear();
            PopulateTableMappingGridWithVersion(selectedVersion);
            PopulateAttributeGridWithVersion(selectedVersion);
            PopulatePhysicalModelGridWithVersion(selectedVersion);

            richTextBoxInformation.Text +="The metadata for version " + majorVersion + "." + minorVersion + " has been loaded.";
            ContentCounter();

            // Make sure the validation information is available in this form
            try
            {
                var validationFile = GlobalParameters.ConfigurationPath + GlobalParameters.ValidationFileName + '_' +
                                     GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension;

                // If the config file does not exist yet, create it by calling the EnvironmentConfiguration Class
                if (!File.Exists(validationFile))
                {
                    EnvironmentConfiguration.CreateDummyValidationFile(validationFile);
                }

                // Load the validation settings file using the paths retrieved from the application root contents (configuration path)
                EnvironmentConfiguration.LoadValidationFile(validationFile);

                richTextBoxInformation.Text += "\r\nThe validation file " + validationFile + " has been loaded.";
            }
            catch (Exception)
            {
                // ignored
            }
            
        }


        /// <summary>
        /// Sets the ToolTip text for cells in the datagrid (hover over).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DataGridViewTableMetadata_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Retrieve the full row for the selected cell
            DataGridViewRow selectedRow = dataGridViewTableMetadata.Rows[e.RowIndex];

            var loadVector = "";
            var tableType = "";
            DataGridViewCell cell = null;

            if (e.Value != null)
            {
                loadVector = ClassMetadataHandling.GetLoadVector(selectedRow.DataGridView.Rows[e.RowIndex].Cells[2].Value.ToString(),
                    selectedRow.DataGridView.Rows[e.RowIndex].Cells[3].Value.ToString());
            }

            // Assert table type for Source column
            if (e.ColumnIndex == dataGridViewTableMetadata.Columns[2].Index && e.Value != null)
            {
                // Retrieve the specific cell value for the hover-over
                cell = dataGridViewTableMetadata.Rows[e.RowIndex].Cells[e.ColumnIndex];

                tableType = ClassMetadataHandling.GetTableType(e.Value.ToString(),"");
            }
            // Assert table type for the Target column
            else if ((e.ColumnIndex == dataGridViewTableMetadata.Columns[3].Index && e.Value != null))
            {
                cell = dataGridViewTableMetadata.Rows[e.RowIndex].Cells[e.ColumnIndex];

                tableType = ClassMetadataHandling.GetTableType(e.Value.ToString(), selectedRow.DataGridView.Rows[e.RowIndex].Cells[5].Value.ToString());
            }
            else
            {
                // Do nothing
            }

            if (cell != null)
            {
                cell.ToolTipText = "The table " + e.Value + " has been evaluated as a " + tableType + " object." +
                                   "\n" + "The direction of loading is " + loadVector + ".";
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
                MessageBox.Show("Pasting into the data grid has failed", "Copy/Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            catch (FormatException)
            {
                MessageBox.Show("There is an issue with the data format for this cell!");
            }
        }

        private void PopulatePhysicalModelGridWithVersion(int versionId)
        {
            var repositoryTarget = ConfigurationSettings.MetadataRepositoryType;

            if (repositoryTarget == "SQLServer") //Queries the tables in SQL Server
            {
                // open latest version
                //LBM: 17/05/2019 moving the code inside the catch for error handling
                var connOmd = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };
                
                try
                {
                    connOmd.Open();
                    var sqlStatementForLatestVersion = new StringBuilder();

                    sqlStatementForLatestVersion.AppendLine("SELECT ");
                    sqlStatementForLatestVersion.AppendLine(" [VERSION_ATTRIBUTE_HASH],");
                    sqlStatementForLatestVersion.AppendLine(" CAST([VERSION_ID] AS VARCHAR(100)) AS VERSION_ID,");
                    sqlStatementForLatestVersion.AppendLine(" [DATABASE_NAME],");
                    sqlStatementForLatestVersion.AppendLine(" [SCHEMA_NAME],");
                    sqlStatementForLatestVersion.AppendLine(" [TABLE_NAME],");
                    sqlStatementForLatestVersion.AppendLine(" [COLUMN_NAME],");
                    sqlStatementForLatestVersion.AppendLine(" [DATA_TYPE],");
                    sqlStatementForLatestVersion.AppendLine(" CAST([CHARACTER_MAXIMUM_LENGTH] AS VARCHAR(100)) AS CHARACTER_MAXIMUM_LENGTH,");
                    sqlStatementForLatestVersion.AppendLine(" CAST([NUMERIC_PRECISION] AS VARCHAR(100)) AS NUMERIC_PRECISION,");
                    sqlStatementForLatestVersion.AppendLine(" CAST([ORDINAL_POSITION] AS VARCHAR(100)) AS ORDINAL_POSITION,");
                    sqlStatementForLatestVersion.AppendLine(" [PRIMARY_KEY_INDICATOR],");
                    sqlStatementForLatestVersion.AppendLine(" [MULTI_ACTIVE_INDICATOR]");
                    sqlStatementForLatestVersion.AppendLine("FROM [MD_VERSION_ATTRIBUTE]");

                    var versionList = Utility.GetDataTable(ref connOmd, sqlStatementForLatestVersion.ToString());
                    _bindingSourcePhysicalModelMetadata.DataSource = versionList;

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
                    dataGridViewPhysicalModelMetadata.Columns[9].HeaderText = "Position";
                    dataGridViewPhysicalModelMetadata.Columns[10].HeaderText = "Primary Key";
                    dataGridViewPhysicalModelMetadata.Columns[11].HeaderText = "Multi-Active";
                }
                catch (Exception exception)
                {
                    richTextBoxInformation.Text += exception.Message;
                    return;
                }
                finally
                {
                    if (connOmd != null)
                    {
                        connOmd.Dispose();
                    }
                }



            }
            else if (repositoryTarget == "JSON") //Update the JSON
            {
                //Check if the file exists, otherwise create a dummy / empty file   
                if (!File.Exists(GlobalParameters.ConfigurationPath + GlobalParameters.JsonModelMetadataFileName + FileConfiguration.jsonVersionExtension))
                {
                    richTextBoxInformation.AppendText("No JSON file was found, so a new empty one was created.\r\n");
                    JsonHandling.CreateDummyJsonFile(GlobalParameters.JsonModelMetadataFileName);
                }

                // Load the file, convert it to a DataTable and bind it to the source
                List<PhysicalModelMetadataJson> jsonArray = JsonConvert.DeserializeObject<List<PhysicalModelMetadataJson>>(File.ReadAllText(GlobalParameters.ConfigurationPath +GlobalParameters.JsonModelMetadataFileName+ FileConfiguration.jsonVersionExtension));

                DataTable dt = Utility.ConvertToDataTable(jsonArray);

                //Make sure the changes are seen as committed, so that changes can be detected later on.
                dt.AcceptChanges();

                SetTeamDataTableMapping.SetPhysicalModelDataTableColumns(dt);

                _bindingSourcePhysicalModelMetadata.DataSource = dt;

                if (jsonArray != null)
                {
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
                    dataGridViewPhysicalModelMetadata.Columns[9].HeaderText = "Position";
                    dataGridViewPhysicalModelMetadata.Columns[10].HeaderText = "Primary Key";
                    dataGridViewPhysicalModelMetadata.Columns[11].HeaderText = "Multi-Active";
                }

                richTextBoxInformation.AppendText("The file " + GlobalParameters.ConfigurationPath + GlobalParameters.JsonModelMetadataFileName+FileConfiguration.jsonVersionExtension + " was loaded.\r\n");
            }
            GridAutoLayoutPhysicalModelMetadata();
        }


        /// <summary>
        /// Populate the Table Mapping DataGrid from a database or existing JSON file.
        /// </summary>
        /// <param name="versionId"></param>
        private void PopulateTableMappingGridWithVersion(int versionId)
        {
            //var selectedVersion = versionId;
            var repositoryTarget = ConfigurationSettings.MetadataRepositoryType;

            if (repositoryTarget == "SQLServer") //Queries the tables in SQL Server
            {
                var connOmd = new SqlConnection {ConnectionString = ConfigurationSettings.ConnectionStringOmd};

                try
                {
                    connOmd.Open();
                    var sqlStatementForLatestVersion = new StringBuilder();
                    sqlStatementForLatestVersion.AppendLine("SELECT ");
                    sqlStatementForLatestVersion.AppendLine(" [TABLE_MAPPING_HASH],");
                    sqlStatementForLatestVersion.AppendLine(" CAST([VERSION_ID] AS VARCHAR(100)) AS VERSION_ID,");
                    sqlStatementForLatestVersion.AppendLine(" [SOURCE_TABLE],");
                    sqlStatementForLatestVersion.AppendLine(" [TARGET_TABLE],");
                    sqlStatementForLatestVersion.AppendLine(" [BUSINESS_KEY_ATTRIBUTE],");
                    sqlStatementForLatestVersion.AppendLine(" [DRIVING_KEY_ATTRIBUTE],");
                    sqlStatementForLatestVersion.AppendLine(" [FILTER_CRITERIA],");
                    sqlStatementForLatestVersion.AppendLine(" [PROCESS_INDICATOR]");
                    sqlStatementForLatestVersion.AppendLine("FROM [MD_TABLE_MAPPING]");
                    sqlStatementForLatestVersion.AppendLine("WHERE [VERSION_ID] = " + versionId);

                    var versionList = Utility.GetDataTable(ref connOmd, sqlStatementForLatestVersion.ToString());

                    // Order by Source Table, Integration_Area table, Business Key Attribute
                    //versionList.DefaultView.Sort = "[SOURCE_TABLE] ASC, [TARGET_TABLE] ASC, [BUSINESS_KEY_ATTRIBUTE] ASC";

                    _bindingSourceTableMetadata.DataSource = versionList;

                    if (versionList != null)
                    {
                        // Set the column header names.
                        dataGridViewTableMetadata.DataSource = _bindingSourceTableMetadata;
                        dataGridViewTableMetadata.ColumnHeadersVisible = true;
                        dataGridViewTableMetadata.Columns[0].Visible = false;
                        dataGridViewTableMetadata.Columns[1].Visible = false;

                        dataGridViewTableMetadata.Columns[0].HeaderText = "Hash Key";
                        dataGridViewTableMetadata.Columns[1].HeaderText = "Version ID";
                        dataGridViewTableMetadata.Columns[2].HeaderText = "Source Table";
                        dataGridViewTableMetadata.Columns[3].HeaderText = "Target Table";
                        dataGridViewTableMetadata.Columns[4].HeaderText = "Business Key Definition";
                        dataGridViewTableMetadata.Columns[5].HeaderText = "Driving Key Definition";
                        dataGridViewTableMetadata.Columns[6].HeaderText = "Filter Criteria";
                        dataGridViewTableMetadata.Columns[7].HeaderText = "Process Indicator";
                    }
                }
                catch (Exception exception)
                {
                    richTextBoxInformation.Text += exception.Message;
                }
                finally
                {
                    connOmd.Close();
                    if (connOmd!=null)
                        connOmd.Dispose();
                }

            }
            else if (repositoryTarget == "JSON") // Retrieve from the JSON file
            {
                // Check if the file exists, otherwise create a dummy / empty file   
                if (!File.Exists(GlobalParameters.ConfigurationPath + GlobalParameters.JsonTableMappingFileName+FileConfiguration.jsonVersionExtension))
                {
                    richTextBoxInformation.AppendText("No JSON file was found, so a new empty one was created.\r\n");
                    JsonHandling.CreateDummyJsonFile(GlobalParameters.JsonTableMappingFileName);
                }

                // Load the file, convert it to a DataTable and bind it to the source
                List<TableMappingJson> jsonArray = JsonConvert.DeserializeObject<List<TableMappingJson>>(File.ReadAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonTableMappingFileName + FileConfiguration.jsonVersionExtension));
                DataTable dt = Utility.ConvertToDataTable(jsonArray);
                // Order by Source Table, Integration_Area table, Business Key Attribute

                dt.AcceptChanges(); //Make sure the changes are seen as committed, so that changes can be detected later on

                SetTeamDataTableMapping.SetTableDataTableColumns(dt);
                SetTeamDataTableMapping.SetTableDataTableSorting(dt);

                _bindingSourceTableMetadata.DataSource = dt;

                if (jsonArray != null)
                {
                    // Set the column header names.
                    dataGridViewTableMetadata.DataSource = _bindingSourceTableMetadata;
                    dataGridViewTableMetadata.ColumnHeadersVisible = true;
                    dataGridViewTableMetadata.Columns[0].Visible = false;
                    dataGridViewTableMetadata.Columns[1].Visible = false;

                    dataGridViewTableMetadata.Columns[0].HeaderText = "Hash Key";
                    dataGridViewTableMetadata.Columns[1].HeaderText = "Version ID";
                    dataGridViewTableMetadata.Columns[2].HeaderText = "Source Table";
                    dataGridViewTableMetadata.Columns[3].HeaderText = "Target Table";
                    dataGridViewTableMetadata.Columns[4].HeaderText = "Business Key Definition";
                    dataGridViewTableMetadata.Columns[5].HeaderText = "Driving Key Definition";
                    dataGridViewTableMetadata.Columns[6].HeaderText = "Filter Criteria";
                    dataGridViewTableMetadata.Columns[7].HeaderText = "Process Indicator";
                }

                richTextBoxInformation.AppendText("The file "+ GlobalParameters.ConfigurationPath + GlobalParameters.JsonTableMappingFileName+FileConfiguration.jsonVersionExtension + " was loaded.\r\n");
            }

            // Resize the grid
            GridAutoLayoutTableMappingMetadata();
        }



        /// <summary>
        /// Populates the data grid directly from a database or an existing JSON file
        /// </summary>
        /// <param name="versionId"></param>
        private void PopulateAttributeGridWithVersion(int versionId)
        {
            var selectedVersion = versionId;
            var repositoryTarget = ConfigurationSettings.MetadataRepositoryType;


            if (repositoryTarget == "SQLServer")
            {
                var connOmd = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };

                try
                {
                    connOmd.Open();
                }
                catch (Exception exception)
                {
                    richTextBoxInformation.Text += exception.Message;
                }

                var sqlStatementForLatestVersion = new StringBuilder();
                sqlStatementForLatestVersion.AppendLine("SELECT ");
                sqlStatementForLatestVersion.AppendLine(" [ATTRIBUTE_MAPPING_HASH],");
                sqlStatementForLatestVersion.AppendLine(" CAST([VERSION_ID] AS VARCHAR(100)) AS VERSION_ID,");
                sqlStatementForLatestVersion.AppendLine(" [SOURCE_TABLE],");
                sqlStatementForLatestVersion.AppendLine(" [SOURCE_COLUMN],");
                sqlStatementForLatestVersion.AppendLine(" [TARGET_TABLE],");
                sqlStatementForLatestVersion.AppendLine(" [TARGET_COLUMN],");
                sqlStatementForLatestVersion.AppendLine(" [TRANSFORMATION_RULE]");
                sqlStatementForLatestVersion.AppendLine("FROM [MD_ATTRIBUTE_MAPPING]");
                sqlStatementForLatestVersion.AppendLine("WHERE [VERSION_ID] = " + selectedVersion);

                var versionList = Utility.GetDataTable(ref connOmd, sqlStatementForLatestVersion.ToString());
                
                _bindingSourceAttributeMetadata.DataSource = versionList;

                if (versionList != null)
                {
                    // Set the column header names.
                    dataGridViewAttributeMetadata.DataSource = _bindingSourceAttributeMetadata;
                    dataGridViewAttributeMetadata.ColumnHeadersVisible = true;
                    dataGridViewAttributeMetadata.Columns[0].Visible = false;
                    dataGridViewAttributeMetadata.Columns[1].Visible = false;
                    dataGridViewAttributeMetadata.Columns[6].ReadOnly = true;
                    //dataGridViewAttributeMetadata.Columns[6].DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;

                    dataGridViewAttributeMetadata.Columns[0].HeaderText = "Hash Key";
                    dataGridViewAttributeMetadata.Columns[1].HeaderText = "Version ID";
                    dataGridViewAttributeMetadata.Columns[2].HeaderText = "Source Table";
                    dataGridViewAttributeMetadata.Columns[3].HeaderText = "Source Column";
                    dataGridViewAttributeMetadata.Columns[4].HeaderText = "Target Table";
                    dataGridViewAttributeMetadata.Columns[5].HeaderText = "Target Column";
                    dataGridViewAttributeMetadata.Columns[6].HeaderText = "Transformation Rule";
                }
            }
            else if (repositoryTarget == "JSON") //Update the JSON
            {
                //Check if the file exists, otherwise create a dummy / empty file   
                if (!File.Exists(GlobalParameters.ConfigurationPath + GlobalParameters.JsonAttributeMappingFileName+FileConfiguration.jsonVersionExtension))
                {
                    richTextBoxInformation.AppendText("No attribute mapping JSON file was found, so a new empty one was created.\r\n");
                    JsonHandling.CreateDummyJsonFile(GlobalParameters.JsonAttributeMappingFileName);
                }

                // Load the file, convert it to a DataTable and bind it to the source
                List<AttributeMappingJson> jsonArray = JsonConvert.DeserializeObject<List<AttributeMappingJson>>(File.ReadAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonAttributeMappingFileName+FileConfiguration.jsonVersionExtension));
                DataTable dt = Utility.ConvertToDataTable(jsonArray);
                dt.AcceptChanges(); //Make sure the changes are seen as committed, so that changes can be detected later on
                SetTeamDataTableMapping.SetAttributeDataTableColumns(dt);

                _bindingSourceAttributeMetadata.DataSource = dt;

                if (jsonArray != null)
                {
                    // Set the column header names.
                    dataGridViewAttributeMetadata.DataSource = _bindingSourceAttributeMetadata;
                    dataGridViewAttributeMetadata.ColumnHeadersVisible = true;
                    dataGridViewAttributeMetadata.Columns[0].Visible = false;
                    dataGridViewAttributeMetadata.Columns[1].Visible = false;
                    dataGridViewAttributeMetadata.Columns[6].ReadOnly = true;

                    dataGridViewAttributeMetadata.Columns[0].HeaderText = "Hash Key";
                    dataGridViewAttributeMetadata.Columns[1].HeaderText = "Version ID";
                    dataGridViewAttributeMetadata.Columns[2].HeaderText = "Source Table";
                    dataGridViewAttributeMetadata.Columns[3].HeaderText = "Source Column";
                    dataGridViewAttributeMetadata.Columns[4].HeaderText = "Target Table";
                    dataGridViewAttributeMetadata.Columns[5].HeaderText = "Target Column";
                    dataGridViewAttributeMetadata.Columns[6].HeaderText = "Transformation Rule";
                }

                richTextBoxInformation.AppendText("The file " + GlobalParameters.ConfigurationPath + GlobalParameters.JsonAttributeMappingFileName+FileConfiguration.jsonVersionExtension + " was loaded.\r\n");
            }

            // Resize the grid
            GridAutoLayoutAttributeMetadata();
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

            //Table Mapping metadata grid - set the autosize based on all cells for each column
            for (var i = 0; i < dataGridViewTableMetadata.Columns.Count - 1; i++)
            {
                dataGridViewTableMetadata.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (dataGridViewTableMetadata.Columns.Count > 0)
            {
                dataGridViewTableMetadata.Columns[dataGridViewTableMetadata.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            // Table Mapping metadata grid - disable the auto size again (to enable manual resizing)
            for (var i = 0; i < dataGridViewTableMetadata.Columns.Count - 1; i++)
            {
                int columnWidth = dataGridViewTableMetadata.Columns[i].Width;
                dataGridViewTableMetadata.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridViewTableMetadata.Columns[i].Width = columnWidth;
            }
        }

        private void GridAutoLayoutAttributeMetadata()
        {
            if (checkBoxResizeDataGrid.Checked == false)
                return;

            //Set the autosize based on all cells for each column
            for (var i = 0; i < dataGridViewAttributeMetadata.Columns.Count - 1; i++)
            {
                dataGridViewAttributeMetadata.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (dataGridViewAttributeMetadata.Columns.Count > 0)
            {
                dataGridViewAttributeMetadata.Columns[dataGridViewAttributeMetadata.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            // Disable the auto size again (to enable manual resizing)
            for (var i = 0; i < dataGridViewAttributeMetadata.Columns.Count - 1; i++)
            {
                int columnWidth = dataGridViewAttributeMetadata.Columns[i].Width;
                dataGridViewAttributeMetadata.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridViewAttributeMetadata.Columns[i].Width = columnWidth;
            }
        }

        private void GridAutoLayoutPhysicalModelMetadata()
        {
            if (checkBoxResizeDataGrid.Checked == false)
                return;

            //Physical model metadata grid - set the autosize based on all cells for each column
            for (var i = 0; i < dataGridViewPhysicalModelMetadata.Columns.Count - 1; i++)
            {
                dataGridViewPhysicalModelMetadata.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (dataGridViewPhysicalModelMetadata.Columns.Count > 0)
            {
                dataGridViewPhysicalModelMetadata.Columns[dataGridViewPhysicalModelMetadata.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            // Disable the auto size again (to enable manual resizing)
            for (var i = 0; i < dataGridViewPhysicalModelMetadata.Columns.Count - 1; i++)
            {
                int columnWidth = dataGridViewPhysicalModelMetadata.Columns[i].Width;
                dataGridViewPhysicalModelMetadata.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridViewPhysicalModelMetadata.Columns[i].Width = columnWidth;
            }
        }

        private List<string> TableMetadataFilter (DataTable inputDataTable)
        {
            var distinctList = new List<string>();
            foreach (DataRow row in inputDataTable.Rows) 
            {
                if (!distinctList.Contains((string)row["SOURCE_TABLE"]))
                {
                    distinctList.Add((string)row["SOURCE_TABLE"]);
                }

                if (!distinctList.Contains((string)row["TARGET_TABLE"]))
                {
                    distinctList.Add((string)row["TARGET_TABLE"]);
                }
            }

            return distinctList;
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
                var integrationTable = row.Cells[3].Value;

                if (gridViewRows != counter + 1 && integrationTable.ToString().Length>3)
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

            labelHubCount.Text = hubSet.Count + " Hubs";
            labelSatCount.Text = satSet.Count + " Satellites";
            labelLnkCount.Text = lnkSet.Count + " Links";
            labelLsatCount.Text = lsatSet.Count + " Link-Satellites";
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
                    _myValidationForm.Invoke((MethodInvoker)delegate { _myValidationForm.Close(); });
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

        private void SaveVersion(int majorVersion, int minorVersion)
        {
            //Insert or create version
            var insertStatement = new StringBuilder();

            insertStatement.AppendLine("INSERT INTO [dbo].[MD_VERSION] ");
            insertStatement.AppendLine("([VERSION_NAME],[VERSION_NOTES],[MAJOR_RELEASE_NUMBER],[MINOR_RELEASE_NUMBER])");
            insertStatement.AppendLine("VALUES ");
            insertStatement.AppendLine("('N/A', 'N/A', " + majorVersion + "," + minorVersion + ")");

            using (var connectionVersion = new SqlConnection(ConfigurationSettings.ConnectionStringOmd))
            {
                var commandVersion = new SqlCommand(insertStatement.ToString(), connectionVersion);

                try
                {
                    connectionVersion.Open();
                    commandVersion.ExecuteNonQuery();
                    richTextBoxInformation.Text += "A new version (" + majorVersion + "." + minorVersion +
                                                    ") was created.\r\n";
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.Text += "An issue has occurred: " + ex;
                }
            }
        }

        private void TruncateMetadata()
        {
            //Truncate tables
            const string commandText = "TRUNCATE TABLE [MD_TABLE_MAPPING]; " +
                                       "TRUNCATE TABLE [MD_ATTRIBUTE_MAPPING]; " +
                                       "TRUNCATE TABLE [MD_VERSION_ATTRIBUTE]; " + //This is the model metadata
                                       "TRUNCATE TABLE [MD_VERSION];";

            using (var connection = new SqlConnection(ConfigurationSettings.ConnectionStringOmd))
            {
                var command = new SqlCommand(commandText, connection);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    richTextBoxInformation.Text += "All metadata tables have been truncated.\r\n";
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.Text += "An issue has occurred: " + ex;
                }
            }
        }

        private void trackBarVersioning_ValueChanged(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();
            FileConfiguration.jsonVersionExtension = @"_v" + trackBarVersioning.Value + ".json";
            GlobalParameters.CurrentVersionId = trackBarVersioning.Value;
            
            PopulateTableMappingGridWithVersion(trackBarVersioning.Value);
            PopulateAttributeGridWithVersion(trackBarVersioning.Value);
            PopulatePhysicalModelGridWithVersion(trackBarVersioning.Value);

            var connOmd = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };
            var versionMajorMinor = GetVersion(trackBarVersioning.Value, connOmd);
            var majorVersion = versionMajorMinor.Key;
            var minorVersion = versionMajorMinor.Value;

            labelVersion.Text = majorVersion + "." + minorVersion;

            //richTextBoxInformation.Text = "The metadata for version " + majorVersion + "." + minorVersion + " has been loaded.";
            ContentCounter();
        }


        /// <summary>
        ///   Clicking the 'activate' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSaveMetadata_Click(object sender, EventArgs e)
        {
            // Clear out the information textbox
            richTextBoxInformation.Clear();

            // Check if the current version is the maximum version. At this stage updates on earlier versions are not supported (and cause a NULLreference exception)
            var highestVersion = GlobalParameters.HighestVersionId;
            var currentVersion = GlobalParameters.CurrentVersionId;

            if (currentVersion < highestVersion)
            {
                richTextBoxInformation.Text += "Cannot save the metadata changes because these are applied to an earlier version. Only updates to the latest or newer version are supported in TEAM.";
            }
            else
            {
                // Create a data table containing the changes, to check if there are changes made to begin with
                var dataTableTableMappingChanges = ((DataTable) _bindingSourceTableMetadata.DataSource).GetChanges();
                var dataTableAttributeMappingChanges =
                    ((DataTable) _bindingSourceAttributeMetadata.DataSource).GetChanges();
                var dataTablePhysicalModelChanges =
                    ((DataTable) _bindingSourcePhysicalModelMetadata.DataSource).GetChanges();

                // Check if there are any rows available in the grid view, and if changes have been detected at all
                if (
                    dataGridViewTableMetadata.RowCount > 0 && dataTableTableMappingChanges != null &&
                    dataTableTableMappingChanges.Rows.Count > 0 ||
                    dataGridViewAttributeMetadata.RowCount > 0 && dataTableAttributeMappingChanges != null &&
                    dataTableAttributeMappingChanges.Rows.Count > 0 ||
                    dataGridViewPhysicalModelMetadata.RowCount > 0 && dataTablePhysicalModelChanges != null &&
                    dataTablePhysicalModelChanges.Rows.Count > 0
                )
                {
                    //Create new version, or retain the old one, depending on selection (version radiobuttons)
                    try
                    {
                        // Capture the 'old ' current version in case the UI needs updating
                        var oldVersionId = trackBarVersioning.Value;

                        //Retrieve the current version, or create a new one
                        int versionId = CreateOrRetrieveVersion();

                        //Commit the save of the metadata, one for each grid
                        if (ConfigurationSettings.MetadataRepositoryType == "SQLServer")
                        {
                            SaveTableMappingMetadataSql(versionId, dataTableTableMappingChanges);
                        }
                        else if (ConfigurationSettings.MetadataRepositoryType == "JSON")
                        {
                            SaveTableMappingMetadataJson(versionId, dataTableTableMappingChanges);
                        }
                        else
                        {
                            richTextBoxInformation.Text =
                                "There was an issue detecting the repository type. The in-use value is: " +
                                ConfigurationSettings.MetadataRepositoryType;
                        }

                        SaveAttributeMappingMetadata(versionId, dataTableAttributeMappingChanges,
                            ConfigurationSettings.MetadataRepositoryType);

                        SaveModelPhysicalModelMetadata(versionId, dataTablePhysicalModelChanges,
                            ConfigurationSettings.MetadataRepositoryType);


                        //Load the grids from the repository after being updated
                        PopulateTableMappingGridWithVersion(versionId);
                        PopulateAttributeGridWithVersion(versionId);
                        PopulatePhysicalModelGridWithVersion(versionId);

                        //Refresh the UI to display the newly created version
                        if (oldVersionId != versionId)
                        {
                            var connOmd = new SqlConnection
                                {ConnectionString = ConfigurationSettings.ConnectionStringOmd};
                            trackBarVersioning.Maximum = GetMaxVersionId(connOmd);
                            trackBarVersioning.TickFrequency = GetVersionCount();
                            trackBarVersioning.Value = GetMaxVersionId(connOmd);
                        }
                    }
                    catch (Exception exception)
                    {
                        richTextBoxInformation.Text +=
                            "The metadata wasn't saved. There are errors saving the metadata version. The reported error is: " +
                            exception;
                    }

                }
                else
                {
                    richTextBoxInformation.Text += "There is no metadata to save!";
                }
            }
        }

        /// <summary>
        /// Verifies the version checkbox (major or minor) and creates new version instance in the metadata repository. If 'no change' is checked this will return the current version Id.
        /// </summary>
        /// <returns></returns>
        private int CreateOrRetrieveVersion()
        {
            var connOmd = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };

            if (!radiobuttonNoVersionChange.Checked)
            {
                //If nothing is checked, just retrieve and return the current version
                var maxVersion = GetMaxVersionId(connOmd);
                var versionKeyValuePair = GetVersion(maxVersion, connOmd);
                var majorVersion = versionKeyValuePair.Key;
                var minorVersion = versionKeyValuePair.Value;

                //Increase the major version, if required
                if (radiobuttonMajorRelease.Checked)
                {
                    try
                    {
                        //Creates a new version
                        majorVersion++;
                        minorVersion = 0;
                        SaveVersion(majorVersion, minorVersion);
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.Text += "An issue occured when saving a new version: " + ex;
                    }
                }

                //Increase the minor version, if required
                if (radioButtonMinorRelease.Checked)
                {
                    try
                    {
                        //Creates a new version
                        minorVersion++;
                        SaveVersion(majorVersion, minorVersion);
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.Text += "An issue occured when saving a new version: " + ex;
                    }
                }
            }

            //Retrieve the current version (again, may have changed)
            var versionId = GetMaxVersionId(connOmd);

            //Make sure the correct version is added to the global parameters
            GlobalParameters.CurrentVersionId = versionId;
            GlobalParameters.HighestVersionId = versionId;
            FileConfiguration.jsonVersionExtension = @"_v" + versionId + ".json";

            return versionId;
        }


        /// <summary>
        /// Creates a new snapshot of the Physical Model metadata to a SQL Server target repository, with the versionId as input parameter
        /// </summary>
        /// <param name="versionId"></param>
        internal void CreateNewPhysicalModelMetadataVersionSqlServer(int versionId)
        {
            // This method creates a new version in the repository for the physical model (MD_VERSION_ATTRIBUTE table)
            var insertQueryTables = new StringBuilder();

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
                        string ordinalPosition = "0";
                        string primaryKeyIndicator = "";
                        string multiActiveIndicator = "";

                        if (row.Cells[2].Value != DBNull.Value)
                        {
                            databaseName = (string)row.Cells[2].Value;
                        }

                        if (row.Cells[3].Value != DBNull.Value)
                        {
                            schemaName = (string)row.Cells[3].Value;
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
                            ordinalPosition = (string) row.Cells[9].Value;
                        }

                        if (row.Cells[10].Value != DBNull.Value)
                        {
                            primaryKeyIndicator = (string) row.Cells[10].Value;
                        }

                        if (row.Cells[11].Value != DBNull.Value)
                        {
                            multiActiveIndicator = (string) row.Cells[11].Value;
                        }

                        insertQueryTables.AppendLine("INSERT INTO MD_VERSION_ATTRIBUTE");
                        insertQueryTables.AppendLine("([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR])");
                        insertQueryTables.AppendLine("VALUES");
                        insertQueryTables.AppendLine("(" + versionId + ", '"+databaseName+"' ,'"+schemaName+"' ,'" + tableName + "','" + columnName + "','" +
                                                     dataType + "','" + maxLength + "','" + numericPrecision + "','" +
                                                     ordinalPosition + "','" + primaryKeyIndicator + "','" +
                                                     multiActiveIndicator + "')");

                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            // Execute the statement, if the repository is SQL Server
            // If the source is JSON this is done in separate calls for now

            if (insertQueryTables.ToString() == "")
            {
                richTextBoxInformation.Text += "No new version was saved.\r\n";
            }
            else
            {
                using (var connection = new SqlConnection(ConfigurationSettings.ConnectionStringOmd))
                {
                    var command = new SqlCommand(insertQueryTables.ToString(), connection);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.Text += "An issue has occurred: " + ex;
                    }
                }

            }
        }

        /// <summary>
        /// Creates a new snapshot of the Physical Model metadata to a Json target repository, with the versionId as input parameter
        /// </summary>
        /// <param name="versionId"></param>
        internal void CreateNewPhysicalModelMetadataVersionJson(int versionId)
        {
            // This method creates a new version in the repository for the physical model (MD_VERSION_ATTRIBUTE table or TEAM_Model.json file)    

            // Create a JArray so segments can be added easily from the datatable
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
                        string ordinalPosition = "0";
                        string primaryKeyIndicator = "";
                        string multiActiveIndicator = "";

                        if (row.Cells[2].Value != DBNull.Value)
                        {
                            databaseName = (string)row.Cells[2].Value;
                        }

                        if (row.Cells[3].Value != DBNull.Value)
                        {
                            schemaName = (string)row.Cells[3].Value;
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
                            ordinalPosition = (string) row.Cells[9].Value;
                        }

                        if (row.Cells[10].Value != DBNull.Value)
                        {
                            primaryKeyIndicator = (string) row.Cells[10].Value;
                        }

                        if (row.Cells[11].Value != DBNull.Value)
                        {
                            multiActiveIndicator = (string) row.Cells[11].Value;
                        }

                        string[] inputHashValue = new string[] { versionId.ToString(), tableName, columnName};
                        var hashKey = Utility.CreateMd5(inputHashValue, GlobalParameters.SandingElement);
                      
                        JObject newJsonSegment = new JObject(
                            new JProperty("versionAttributeHash", hashKey),
                            new JProperty("versionId", versionId),
                            new JProperty("databaseName", databaseName),
                            new JProperty("schemaName", schemaName),
                            new JProperty("tableName", tableName),
                            new JProperty("columnName", columnName),
                            new JProperty("dataType", dataType),
                            new JProperty("characterMaximumLength", maxLength),
                            new JProperty("numericPrecision", numericPrecision),
                            new JProperty("ordinalPosition", ordinalPosition),
                            new JProperty("primaryKeyIndicator", primaryKeyIndicator),
                            new JProperty("multiActiveIndicator", multiActiveIndicator)
                        );

                        jsonModelMappingFull.Add(newJsonSegment);
                    }
                }

            }
            catch (Exception)
            {
                // ignored
            }

            // Execute the statement, if the repository is JSON
            try
            {
                //Generate a unique key using a hash
                string output = JsonConvert.SerializeObject(jsonModelMappingFull, Formatting.Indented);
                File.WriteAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonModelMetadataFileName + FileConfiguration.jsonVersionExtension, output);
            }
            catch (JsonReaderException ex)
            {
                richTextBoxInformation.Text += "There were issues inserting the new JSON version file for the Physical Model.\r\n" + ex;
            }

        }

        /// <summary>
        /// Creates a new snapshot of the Table Mapping metadata to a SQL Server target repository, with the versionId as input parameter
        /// </summary>
        /// <param name="versionId"></param>
        internal void CreateNewTableMappingMetadataVersionSqlServer(int versionId)
        {
            var insertQueryTables = new StringBuilder();

            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (!row.IsNewRow)
                {
                    var stagingTable = "";
                    var integrationTable = "";
                    var businessKeyDefinition = "";
                    var drivingKeyDefinition = "";
                    var filterCriterion = "";
                    var generateIndicator = "";


                    // position 0 and 1 in the row are respectively the hash key and version ID, so the grid starts at [2]
                    if (row.Cells[2].Value != DBNull.Value)
                    {
                        stagingTable = (string)row.Cells[2].Value;
                    }

                    if (row.Cells[3].Value != DBNull.Value)
                    {
                        integrationTable = (string)row.Cells[3].Value;
                    }

                    if (row.Cells[4].Value != DBNull.Value)
                    {
                        businessKeyDefinition = (string)row.Cells[4].Value;
                        businessKeyDefinition = businessKeyDefinition.Replace("'", "''");  //Double quotes for composites
                    }

                    if (row.Cells[5].Value != DBNull.Value)
                    {
                        drivingKeyDefinition = (string)row.Cells[5].Value;
                        drivingKeyDefinition = drivingKeyDefinition.Replace("'", "''"); //Double quotes for composites
                    }

                    if (row.Cells[6].Value != DBNull.Value)
                    {
                        filterCriterion = (string)row.Cells[6].Value;
                        filterCriterion = filterCriterion.Replace("'", "''"); //Double quotes for composites
                    }

                    if (row.Cells[7].Value != DBNull.Value)
                    {
                        generateIndicator = (string)row.Cells[7].Value;
                        generateIndicator = generateIndicator.Replace("'", "''"); //Double quotes for composites
                    }

                    insertQueryTables.AppendLine("INSERT INTO MD_TABLE_MAPPING");
                    insertQueryTables.AppendLine("([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [DRIVING_KEY_ATTRIBUTE], [FILTER_CRITERIA], [PROCESS_INDICATOR])");
                    insertQueryTables.AppendLine("VALUES (" + versionId + ",'" + stagingTable + "','" +
                                                 businessKeyDefinition + "','" + integrationTable + "','" +
                                                 drivingKeyDefinition + "','" + filterCriterion + "','" +
                                                 generateIndicator + "')");
                }
            }

            // Execute the statement, if the repository is SQL Server
            // If the source is JSON this is done in separate calls for now
             if (insertQueryTables.ToString() == "")
            {
                richTextBoxInformation.Text += "No new version was saved.\r\n";
            }
            else
            {
                using (var connection = new SqlConnection(ConfigurationSettings.ConnectionStringOmd))
                {
                    var command = new SqlCommand(insertQueryTables.ToString(), connection);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.Text += "An issue has occurred: " + ex;
                    }
                }
            }
        }


        /// <summary>
        /// Creates a new snapshot of the Table Mapping metadata for a JSON repository, with the versionId as input parameter. A new file will created for the provided version Id.
        /// </summary>
        /// <param name="versionId"></param>
        internal void CreateNewTableMappingMetadataVersionJson(int versionId)
        {
            var JsonVersionExtension = @"_v" + versionId + ".json";

            // Create a JArray so segments can be added easily from the datatable
            var jsonTableMappingFull = new JArray();

            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (!row.IsNewRow)
                {
                    var stagingTable = "";
                    var integrationTable = "";
                    var businessKeyDefinition = "";
                    var drivingKeyDefinition = "";
                    var filterCriterion = "";
                    var generateIndicator = "";


                    // position 0 and 1 in the row are respectively the hash key and version ID, so the grid starts at [2]
                    if (row.Cells[2].Value != DBNull.Value)
                    {
                        stagingTable = (string)row.Cells[2].Value;
                    }

                    if (row.Cells[3].Value != DBNull.Value)
                    {
                        integrationTable = (string)row.Cells[3].Value;
                    }

                    if (row.Cells[4].Value != DBNull.Value)
                    {
                        businessKeyDefinition = (string)row.Cells[4].Value;
                        //businessKeyDefinition = businessKeyDefinition.Replace("'", "''");  //Double quotes for composites
                    }

                    if (row.Cells[5].Value != DBNull.Value)
                    {
                        drivingKeyDefinition = (string)row.Cells[5].Value;
                        //drivingKeyDefinition = drivingKeyDefinition.Replace("'", "''"); //Double quotes for composites
                    }

                    if (row.Cells[6].Value != DBNull.Value)
                    {
                        filterCriterion = (string)row.Cells[6].Value;
                        //filterCriterion = filterCriterion.Replace("'", "''"); //Double quotes for composites
                    }

                    if (row.Cells[7].Value != DBNull.Value)
                    {
                        generateIndicator = (string)row.Cells[7].Value;
                        //generateIndicator = generateIndicator.Replace("'", "''"); //Double quotes for composites
                    }

                    string[] inputHashValue = new string[] { versionId.ToString(), stagingTable, integrationTable, businessKeyDefinition, drivingKeyDefinition, filterCriterion };
                    var hashKey = Utility.CreateMd5(inputHashValue, GlobalParameters.SandingElement);

                    JObject newJsonSegment = new JObject(
                        new JProperty("tableMappingHash", hashKey),
                        new JProperty("versionId", versionId),
                        new JProperty("sourceTable", stagingTable),
                        new JProperty("targetTable", integrationTable),
                        new JProperty("businessKeyDefinition", businessKeyDefinition),
                        new JProperty("drivingKeyDefinition", drivingKeyDefinition),
                        new JProperty("filterCriteria", filterCriterion),
                        new JProperty("processIndicator", generateIndicator)
                    );

                    jsonTableMappingFull.Add(newJsonSegment);

                }
            }

            // Execute the statement, if the repository is JSON
            try
            {
                //Generate a unique key using a hash
                string output = JsonConvert.SerializeObject(jsonTableMappingFull, Formatting.Indented);
                File.WriteAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonTableMappingFileName + JsonVersionExtension, output);
            }
            catch (JsonReaderException ex)
            {
                richTextBoxInformation.Text += "There were issues inserting the new JSON version file for the Table Mapping.\r\n" + ex;
            }
        }

        /// <summary>
        /// Creates a new snapshot of the Attribute Mapping metadata for a Sql Server repository, with the versionId as input parameter. A new file will created for the provided version Id.
        /// </summary>
        /// <param name="versionId"></param>
        internal void CreateNewAttributeMappingMetadataVersionSqlServer(int versionId)
        {
            var repositoryTarget = ConfigurationSettings.MetadataRepositoryType;

            var insertQueryTables = new StringBuilder();

            foreach (DataGridViewRow row in dataGridViewAttributeMetadata.Rows)
            {
                if (!row.IsNewRow)
                {
                    var stagingTable = "";
                    var stagingColumn = "";
                    var integrationTable = "";
                    var integrationColumn = "";
                    var transformationRule = "";

                    if (row.Cells[2].Value != DBNull.Value)
                    {
                        stagingTable = (string)row.Cells[2].Value;
                    }

                    if (row.Cells[3].Value != DBNull.Value)
                    {
                        stagingColumn = (string)row.Cells[3].Value;
                    }

                    if (row.Cells[4].Value != DBNull.Value)
                    {
                        integrationTable = (string)row.Cells[4].Value;
                    }

                    if (row.Cells[5].Value != DBNull.Value)
                    {
                        integrationColumn = (string)row.Cells[5].Value;
                    }

                    if (row.Cells[6].Value != DBNull.Value)
                    {
                        transformationRule = (string)row.Cells[6].Value;
                    }

                    insertQueryTables.AppendLine("INSERT INTO MD_ATTRIBUTE_MAPPING");
                    insertQueryTables.AppendLine("([VERSION_ID],[SOURCE_TABLE],[SOURCE_COLUMN],[TARGET_TABLE],[TARGET_COLUMN],[TRANSFORMATION_RULE])");
                    insertQueryTables.AppendLine("VALUES (" + versionId + ",'" + stagingTable + "','" +
                                                 stagingColumn +
                                                 "','" + integrationTable + "','" + integrationColumn + "','" +
                                                 transformationRule + "')");
                }
            }

            // Execute the statement, if the repository is SQL Server
            // If the source is JSON this is done in separate calls for now
            if (repositoryTarget == "SQLServer")
            {
                if (insertQueryTables.ToString() == "")
                {
                    richTextBoxInformation.Text += "No new version was saved.\r\n";
                }
                else
                {
                    using (var connection = new SqlConnection(ConfigurationSettings.ConnectionStringOmd))
                    {
                        var command = new SqlCommand(insertQueryTables.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            richTextBoxInformation.Text += "An issue has occurred: " + ex;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new snapshot of the Attribute Mapping metadata for a JSON repository, with the versionId as input parameter. A new file will created for the provided version Id.
        /// </summary>
        /// <param name="versionId"></param>
        internal void CreateNewAttributeMappingMetadataVersionJson(int versionId)
        {
            var JsonVersionExtension = @"_v" + versionId + ".json";

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
                    var transformationRule = "";

                    if (row.Cells[2].Value != DBNull.Value)
                    {
                        stagingTable = (string)row.Cells[2].Value;
                    }

                    if (row.Cells[3].Value != DBNull.Value)
                    {
                        stagingColumn = (string)row.Cells[3].Value;
                    }

                    if (row.Cells[4].Value != DBNull.Value)
                    {
                        integrationTable = (string)row.Cells[4].Value;
                    }

                    if (row.Cells[5].Value != DBNull.Value)
                    {
                        integrationColumn = (string)row.Cells[5].Value;
                    }

                    if (row.Cells[6].Value != DBNull.Value)
                    {
                        transformationRule = (string)row.Cells[6].Value;
                    }


                    string[] inputHashValue = new string[] { versionId.ToString(), stagingTable, stagingColumn, integrationTable, integrationColumn, transformationRule };
                    var hashKey = Utility.CreateMd5(inputHashValue, GlobalParameters.SandingElement);

                   
                    JObject newJsonSegment = new JObject(
                        new JProperty("attributeMappingHash", hashKey),
                        new JProperty("versionId", versionId),
                        new JProperty("sourceTable", stagingTable),
                        new JProperty("sourceAttribute", stagingColumn),
                        new JProperty("targetTable", integrationTable),
                        new JProperty("targetAttribute", integrationColumn),
                        new JProperty("transformationRule", transformationRule)
                    );

                    jsonAttributeMappingFull.Add(newJsonSegment);
                }
            }

            // Execute the statement, if the repository is JSON
            try
            {
                //Generate a unique key using a hash
                string output = JsonConvert.SerializeObject(jsonAttributeMappingFull, Formatting.Indented);
                File.WriteAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonAttributeMappingFileName + JsonVersionExtension, output);
            }
            catch (JsonReaderException ex)
            {
                richTextBoxInformation.Text += "There were issues inserting the new JSON version file for the Attribute Mapping.\r\n" + ex;
            }

        }


        private void SaveTableMappingMetadataSql(int versionId, DataTable dataTableChanges)
        {
            if (FileConfiguration.newFileTableMapping == "true")
            {
                // TO BE DEVELOPED FURTHER, needs to clear out existing version from MD_TABLE_MAPPING
                FileConfiguration.newFileTableMapping = "false";
            }

            //If no change radio buttons are selected this means either minor or major version is checked, so a full new snapshot will be created
            if (!radiobuttonNoVersionChange.Checked)
            {
               CreateNewTableMappingMetadataVersionSqlServer(versionId);
            }
            else //... otherwise an in-place update to the existing version is done (insert / update / delete)
            {
                var insertQueryTables = new StringBuilder();

                if (dataTableChanges != null && (dataTableChanges.Rows.Count > 0)) //Double-check if there are any changes made at all
                {
                    foreach (DataRow row in dataTableChanges.Rows) //Start looping through the changes
                    {
                        #region Changed rows 
                        if ((row.RowState & DataRowState.Modified) != 0)
                        {
                            //Grab the attributes into local variables
                            var hashKey = (string)row["TABLE_MAPPING_HASH"];
                            var versionKey = row["VERSION_ID"].ToString();
                            var stagingTable = "";
                            var integrationTable = "";
                            var businessKeyDefinition = "";
                            var drivingKeyDefinition = "";
                            var filterCriterion = "";
                            var generateIndicator = "";

                            if (row["SOURCE_TABLE"] != DBNull.Value)
                            {
                                stagingTable = (string)row["SOURCE_TABLE"];
                            }
                            if (row["TARGET_TABLE"] != DBNull.Value)
                            {
                                integrationTable = (string)row["TARGET_TABLE"];
                            }
                            if (row["BUSINESS_KEY_ATTRIBUTE"] != DBNull.Value)
                            {
                                businessKeyDefinition = (string)row["BUSINESS_KEY_ATTRIBUTE"];
                            }
                            if (row["DRIVING_KEY_ATTRIBUTE"] != DBNull.Value)
                            {
                                drivingKeyDefinition = (string)row["DRIVING_KEY_ATTRIBUTE"];
                            }
                            if (row["FILTER_CRITERIA"] != DBNull.Value)
                            {
                                filterCriterion = (string)row["FILTER_CRITERIA"];
                            }
                            if (row["PROCESS_INDICATOR"] != DBNull.Value)
                            {
                                generateIndicator = (string)row["PROCESS_INDICATOR"];
                            }

                            //Double quotes for composites, but only if things are written to the database otherwise it's already OK
                            businessKeyDefinition = businessKeyDefinition.Replace("'", "''");
                            drivingKeyDefinition = drivingKeyDefinition.Replace("'", "''");
                            filterCriterion = filterCriterion.Replace("'", "''");
                            generateIndicator = generateIndicator.Replace("'", "''");

                            insertQueryTables.AppendLine("UPDATE [MD_TABLE_MAPPING]");
                            insertQueryTables.AppendLine("SET [SOURCE_TABLE] = '" + stagingTable +
                                                            "',[BUSINESS_KEY_ATTRIBUTE] = '" + businessKeyDefinition +
                                                            "',[TARGET_TABLE] = '" + integrationTable +
                                                            "',[DRIVING_KEY_ATTRIBUTE] = '" + drivingKeyDefinition +
                                                            "',[FILTER_CRITERIA] = '" + filterCriterion +
                                                            "',[PROCESS_INDICATOR] = '" + generateIndicator + "'");
                            insertQueryTables.AppendLine("WHERE [TABLE_MAPPING_HASH] = '" + hashKey + "' AND [VERSION_ID] = " + versionKey);
                        }
                        #endregion

                        #region Inserted rows
                        if ((row.RowState & DataRowState.Added) != 0)
                        {
                            var stagingTable = "";
                            var integrationTable = "";
                            var businessKeyDefinition = "";
                            var drivingKeyDefinition = "";
                            var filterCriterion = "";
                            var generateIndicator = "";

                            if (row[2] != DBNull.Value)
                            {
                                stagingTable = (string)row[2];
                            }
                            if (row[3] != DBNull.Value)
                            {
                                integrationTable = (string)row[3];
                            }
                            if (row[4] != DBNull.Value)
                            {
                                businessKeyDefinition = (string)row[4];
                                businessKeyDefinition = businessKeyDefinition.Replace("'", "''");
                                //Double quotes for composites
                            }
                            if (row[5] != DBNull.Value)
                            {
                                drivingKeyDefinition = (string)row[5];
                                drivingKeyDefinition = drivingKeyDefinition.Replace("'", "''");
                            }
                            if (row[6] != DBNull.Value)
                            {
                                filterCriterion = (string)row[6];
                                filterCriterion = filterCriterion.Replace("'", "''");
                            }
                            if (row[7] != DBNull.Value)
                            {
                                generateIndicator = (string)row[7];
                                generateIndicator = generateIndicator.Replace("'", "''");
                            }

                            insertQueryTables.AppendLine("INSERT INTO [MD_TABLE_MAPPING]");
                            insertQueryTables.AppendLine("([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [DRIVING_KEY_ATTRIBUTE], [FILTER_CRITERIA], [PROCESS_INDICATOR])");
                            insertQueryTables.AppendLine("VALUES (" + versionId + ",'" + stagingTable + "','" +
                                                         businessKeyDefinition + "','" + integrationTable + "','" +
                                                         drivingKeyDefinition + "','" + filterCriterion + "','" +
                                                         generateIndicator + "')");

                        }
                        #endregion

                        #region Deleted rows
                        //Deleted rows
                        if ((row.RowState & DataRowState.Deleted) != 0)
                        {
                            var hashKey = row["TABLE_MAPPING_HASH", DataRowVersion.Original].ToString();
                            var versionKey = row["VERSION_ID", DataRowVersion.Original].ToString();

                            insertQueryTables.AppendLine("DELETE FROM MD_TABLE_MAPPING");
                            insertQueryTables.AppendLine("WHERE [TABLE_MAPPING_HASH] = '" + hashKey + "' AND [VERSION_ID] = " + versionKey);
                        }
                        #endregion
                    }

                    #region Statement execution
                    // Execute the statement, if the repository is SQL Server
                    if (insertQueryTables.ToString() == "")
                    {
                        richTextBoxInformation.Text += "No Business Key / Table mapping metadata changes were saved.\r\n";
                    }
                    else
                    {
                        using (var connection = new SqlConnection(ConfigurationSettings.ConnectionStringOmd))
                        {
                            var command = new SqlCommand(insertQueryTables.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                richTextBoxInformation.Text += "An issue has occurred: " + ex;
                            }
                        }
                    }             

                    //Committing the changes to the datatable - making sure new changes can be picked up
                    // AcceptChanges will clear all New, Deleted and/or Modified settings
                    dataTableChanges.AcceptChanges();
                    ((DataTable)_bindingSourceTableMetadata.DataSource).AcceptChanges();
                    #endregion

                    richTextBoxInformation.Text += "The Business Key / Table Mapping metadata has been saved.\r\n";
                }
            } // End of constructing the statements for insert / update / delete
        }

        private void SaveTableMappingMetadataJson(int versionId, DataTable dataTableChanges)
        {
            if (FileConfiguration.newFileTableMapping == "true")
            {
                JsonHandling.RemoveExistingJsonFile(GlobalParameters.JsonTableMappingFileName + @"_v" + GlobalParameters.CurrentVersionId + ".json");
                JsonHandling.CreatePlaceholderJsonFile(GlobalParameters.JsonTableMappingFileName);
                FileConfiguration.newFileTableMapping = "false";
            }

            //If no change radio buttons are selected this means either minor or major version is checked, so a full new snapshot will be created
            if (!radiobuttonNoVersionChange.Checked)
            {
                CreateNewTableMappingMetadataVersionJson(versionId);
            }

            //... otherwise an in-place update to the existing version is done (insert / update / delete)
            else
            {
                var insertQueryTables = new StringBuilder();

                if (dataTableChanges != null && (dataTableChanges.Rows.Count > 0)) //Double-check if there are any changes made at all
                {
                    foreach (DataRow row in dataTableChanges.Rows) //Start looping through the changes
                    {
                        #region Changed rows
                        //Changed rows
                        if ((row.RowState & DataRowState.Modified) != 0)
                        {
                            //Grab the attributes into local variables
                            var hashKey = (string)row["TABLE_MAPPING_HASH"];
                            var versionKey = row["VERSION_ID"].ToString();
                            var stagingTable = "";
                            var integrationTable = "";
                            var businessKeyDefinition = "";
                            var drivingKeyDefinition = "";
                            var filterCriterion = "";
                            var generateIndicator = "";

                            if (row["SOURCE_TABLE"] != DBNull.Value)
                            {
                                stagingTable = (string)row["SOURCE_TABLE"];
                            }
                            if (row["TARGET_TABLE"] != DBNull.Value)
                            {
                                integrationTable = (string)row["TARGET_TABLE"];
                            }
                            if (row["BUSINESS_KEY_ATTRIBUTE"] != DBNull.Value)
                            {
                                businessKeyDefinition = (string)row["BUSINESS_KEY_ATTRIBUTE"];
                            }
                            if (row["DRIVING_KEY_ATTRIBUTE"] != DBNull.Value)
                            {
                                drivingKeyDefinition = (string)row["DRIVING_KEY_ATTRIBUTE"];
                            }
                            if (row["FILTER_CRITERIA"] != DBNull.Value)
                            {
                                filterCriterion = (string)row["FILTER_CRITERIA"];
                            }
                            if (row["PROCESS_INDICATOR"] != DBNull.Value)
                            {
                                generateIndicator = (string)row["PROCESS_INDICATOR"];
                            }

                            //Read the file in memory
                            TableMappingJson[] jsonArray = JsonConvert.DeserializeObject<TableMappingJson[]>(File.ReadAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonTableMappingFileName + FileConfiguration.jsonVersionExtension));

                            //Retrieves the json segment in the file for the given hash returns value or NULL
                            var jsonHash = jsonArray.FirstOrDefault(obj => obj.tableMappingHash == hashKey);

                            if (jsonHash.tableMappingHash == "")
                            {
                                richTextBoxInformation.Text += "The correct segment in the JSON file was not found.\r\n";
                            }
                            else
                            {
                                // Update the values in the JSON segment
                                jsonHash.businessKeyDefinition = businessKeyDefinition;
                                jsonHash.drivingKeyDefinition = drivingKeyDefinition;
                                jsonHash.filterCriteria = filterCriterion;
                                jsonHash.processIndicator = generateIndicator;
                                jsonHash.targetTable = integrationTable;
                                jsonHash.sourceTable = stagingTable;
                            }

                            string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);

                            try
                            {
                                // The below is not really necessary and was added as an attempt to work around limitations in WriteAllText, but turns out to be handy nonetheless
                                //var shortDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                                //var targetFilePathName = GlobalParameters.ConfigurationPath + string.Concat("Backup_" + shortDatetime + "_", GlobalParameters.jsonTableMappingFileName+JsonVersionExtension);

                                //File.Copy(GlobalParameters.ConfigurationPath + GlobalParameters.jsonTableMappingFileName+JsonVersionExtension, targetFilePathName);
                                //File.Delete(GlobalParameters.ConfigurationPath + GlobalParameters.jsonTableMappingFileName+JsonVersionExtension);


                                // Write the updated JSON file to disk. NOTE - DOES NOT ALWAYS WORK WHEN FILE IS OPEN IN NOTEPAD AND DOES NOT RAISE EXCEPTION
                                File.WriteAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonTableMappingFileName + FileConfiguration.jsonVersionExtension, output);

                                // Wait for half a second for I/O operations to complete
                                // Thread.Sleep(500);
                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += "There were issues saving the JSON update to disk.\r\n" + ex;
                            }
                            //var bla2 = jsonArray.Any(obj => obj.tableMappingHash == "1029C102DE45D40066210E426B605885"); // Returns true if any is found

                        }
                        #endregion

                        #region Inserted rows
                        //Inserted rows
                        if ((row.RowState & DataRowState.Added) != 0)
                        {
                            var stagingTable = "";
                            var integrationTable = "";
                            var businessKeyDefinition = "";
                            var drivingKeyDefinition = "";
                            var filterCriterion = "";
                            var generateIndicator = "";

                            if (row[2] != DBNull.Value)
                            {
                                stagingTable = (string)row[2];
                            }

                            if (row[3] != DBNull.Value)
                            {
                                integrationTable = (string)row[3];
                            }

                            if (row[4] != DBNull.Value)
                            {
                                businessKeyDefinition = (string)row[4];
                                //businessKeyDefinition = businessKeyDefinition.Replace("'", "''");
                                //Double quotes for composites
                            }

                            if (row[5] != DBNull.Value)
                            {
                                drivingKeyDefinition = (string)row[5];
                                //drivingKeyDefinition = drivingKeyDefinition.Replace("'", "''");
                            }

                            if (row[6] != DBNull.Value)
                            {
                                filterCriterion = (string)row[6];
                                //filterCriterion = filterCriterion.Replace("'", "''");
                            }

                            if (row[7] != DBNull.Value)
                            {
                                generateIndicator = (string)row[7];
                                //generateIndicator = generateIndicator.Replace("'", "''");
                            }



                            try
                            {
                                var jsonTableMappingFull = new JArray();

                                // Load the file, if existing information needs to be merged
                                TableMappingJson[] jsonArray =
                                    JsonConvert.DeserializeObject<TableMappingJson[]>(
                                        File.ReadAllText(
                                            GlobalParameters.ConfigurationPath +
                                            GlobalParameters.JsonTableMappingFileName + FileConfiguration.jsonVersionExtension));

                                // Convert it into a JArray so segments can be added easily
                                if (jsonArray != null)
                                {
                                    jsonTableMappingFull = JArray.FromObject(jsonArray);
                                }

                                string[] inputHashValue = new string[] { versionId.ToString(), stagingTable, integrationTable, businessKeyDefinition, drivingKeyDefinition, filterCriterion };
                                var hashKey = Utility.CreateMd5(inputHashValue, GlobalParameters.SandingElement);

                                // Convert it into a JArray so segments can be added easily
                                JObject newJsonSegment = new JObject(
                                    new JProperty("tableMappingHash", hashKey),
                                    new JProperty("versionId", versionId),
                                    new JProperty("sourceTable", stagingTable),
                                    new JProperty("targetTable", integrationTable),
                                    new JProperty("businessKeyDefinition", businessKeyDefinition),
                                    new JProperty("drivingKeyDefinition", drivingKeyDefinition),
                                    new JProperty("filterCriteria", filterCriterion),
                                    new JProperty("processIndicator", generateIndicator)
                                    );

                                jsonTableMappingFull.Add(newJsonSegment);

                                string output = JsonConvert.SerializeObject(jsonTableMappingFull, Formatting.Indented);
                                File.WriteAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonTableMappingFileName + FileConfiguration.jsonVersionExtension, output);

                                //Making sure the hash key value is added to the datatable as well
                                row[0] = hashKey;

                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += "There were issues inserting the JSON segment / record.\r\n" + ex;
                            }

                        }
                        #endregion

                        #region Deleted rows
                        //Deleted rows
                        if ((row.RowState & DataRowState.Deleted) != 0)
                        {
                            var hashKey = row["TABLE_MAPPING_HASH", DataRowVersion.Original].ToString();
                            var versionKey = row["VERSION_ID", DataRowVersion.Original].ToString();


                            try
                            {
                                var jsonArray =
                                    JsonConvert.DeserializeObject<TableMappingJson[]>(
                                        File.ReadAllText(GlobalParameters.ConfigurationPath +
                                                         GlobalParameters.JsonTableMappingFileName + FileConfiguration.jsonVersionExtension)).ToList();

                                //Retrieves the json segment in the file for the given hash returns value or NULL
                                var jsonSegment = jsonArray.FirstOrDefault(obj => obj.tableMappingHash == hashKey);

                                jsonArray.Remove(jsonSegment);

                                if (jsonSegment.tableMappingHash == "")
                                {
                                    richTextBoxInformation.Text += "The correct segment in the JSON file was not found.\r\n";
                                }
                                else
                                {
                                    //Remove the segment from the JSON
                                    jsonArray.Remove(jsonSegment);
                                }

                                string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                                File.WriteAllText(
                                    GlobalParameters.ConfigurationPath + GlobalParameters.JsonTableMappingFileName + FileConfiguration.jsonVersionExtension,
                                    output);

                            }
                            catch (JsonReaderException ex)
                            {
                                richTextBoxInformation.Text += "There were issues applying the JSON update.\r\n" + ex;
                            }

                        }
                        #endregion
                    }

                    #region Statement execution
                    // Execute the statement. If the source is JSON this is done in separate calls for now

                    //Committing the changes to the datatable - making sure new changes can be picked up
                    // AcceptChanges will clear all New, Deleted and/or Modified settings
                    dataTableChanges.AcceptChanges();
                    ((DataTable)_bindingSourceTableMetadata.DataSource).AcceptChanges();

                    //The JSON needs to be re-bound to the datatable / datagrid after being updated (accepted) to allow all values to be present including the hash which may have changed

                    BindTableMappingJsonToDataTable();

                    #endregion

                    richTextBoxInformation.Text += "The Business Key / Table Mapping metadata has been saved.\r\n";
                }
            } // End of constructing the statements for insert / update / delete
        }

        private void SaveModelPhysicalModelMetadata(int versionId, DataTable dataTableChanges, string repositoryTarget)
        {
            if (FileConfiguration.newFilePhysicalModel == "true")
            {
                JsonHandling.RemoveExistingJsonFile(GlobalParameters.JsonModelMetadataFileName + @"_v" + GlobalParameters.CurrentVersionId + ".json");
                JsonHandling.CreatePlaceholderJsonFile(GlobalParameters.JsonModelMetadataFileName);
                FileConfiguration.newFilePhysicalModel = "false";
            }

            //If the save version radiobutton is selected it means either minor or major version is checked and a full new snapshot needs to be created first
            if (!radiobuttonNoVersionChange.Checked)
            {
                if (ConfigurationSettings.MetadataRepositoryType == "SQLServer")
                {
                    CreateNewPhysicalModelMetadataVersionSqlServer(versionId);
                }
                else if (ConfigurationSettings.MetadataRepositoryType == "JSON")
                {
                    CreateNewPhysicalModelMetadataVersionJson(versionId);
                }
            }

            //An in-place update (no change) to the existing version is done
            else
            {
                //Grabbing the generic settings from the main forms
                var insertQueryTables = new StringBuilder();

                if ((dataTableChanges != null && (dataTableChanges.Rows.Count > 0))) //Check if there are any changes made at all
                {
                    foreach (DataRow row in dataTableChanges.Rows) //Loop through the detected changes
                    {
                        #region Changed Rows
                        //Changed rows
                        if ((row.RowState & DataRowState.Modified) != 0)
                        {
                            var hashKey = (string)row["VERSION_ATTRIBUTE_HASH"];
                            var databaseName = (string)row["DATABASE_NAME"];
                            var schemaName = (string)row["SCHEMA_NAME"];
                            var tableName = (string)row["TABLE_NAME"];
                            var columnName = (string)row["COLUMN_NAME"];
                            var dataType = (string)row["DATA_TYPE"];
                            var maxLength = (string)row["CHARACTER_MAXIMUM_LENGTH"];
                            var numericPrecision = (string)row["NUMERIC_PRECISION"];
                            var ordinalPosition = (string)row["ORDINAL_POSITION"];
                            var primaryKeyIndicator = (string)row["PRIMARY_KEY_INDICATOR"];
                            var multiActiveIndicator = (string)row["MULTI_ACTIVE_INDICATOR"];
                            var versionKey = row["VERSION_ID"].ToString();

                            if (repositoryTarget == "SQLServer")
                            {
                                insertQueryTables.AppendLine("UPDATE MD_VERSION_ATTRIBUTE");
                                insertQueryTables.AppendLine("SET " +
                                                             "  [DATABASE_NAME] = '" + databaseName +
                                                             "' [SCHEMA_NAME] = '" + schemaName +
                                                             "',[TABLE_NAME] = '" + tableName +
                                                             "',[COLUMN_NAME] = '" + columnName +
                                                             "',[DATA_TYPE] = '" + dataType +
                                                             "',[CHARACTER_MAXIMUM_LENGTH] = '" + maxLength +
                                                             "',[NUMERIC_PRECISION] = '" + numericPrecision +
                                                             "',[ORDINAL_POSITION] = '" + ordinalPosition +
                                                             "',[PRIMARY_KEY_INDICATOR] = '" + primaryKeyIndicator +
                                                             "',[MULTI_ACTIVE_INDICATOR] = '" + multiActiveIndicator +
                                                             "'");
                                insertQueryTables.AppendLine("WHERE [VERSION_ATTRIBUTE_HASH] = '" + hashKey +
                                                             "' AND [VERSION_ID] = " + versionKey);
                            }
                            else if (repositoryTarget == "JSON") //Insert a new segment (row) in the JSON
                            {

                                try
                                {
                                    PhysicalModelMetadataJson[] jsonArray = JsonConvert.DeserializeObject<PhysicalModelMetadataJson[]>(File.ReadAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonModelMetadataFileName + FileConfiguration.jsonVersionExtension));

                                    var jsonHash = jsonArray.FirstOrDefault(obj => obj.versionAttributeHash == hashKey); //Retrieves the json segment in the file for the given hash returns value or NULL

                                    if (jsonHash.versionAttributeHash == "")
                                    {
                                        richTextBoxInformation.Text += "The correct segment in the JSON file was not found.\r\n";
                                    }
                                    else
                                    {
                                        // Update the values in the JSON segment
                                        jsonHash.databaseName = databaseName;
                                        jsonHash.schemaName = schemaName;
                                        jsonHash.tableName = tableName;
                                        jsonHash.columnName = columnName;
                                        jsonHash.dataType = dataType;
                                        jsonHash.characterMaximumLength = maxLength;
                                        jsonHash.numericPrecision = numericPrecision;
                                        jsonHash.ordinalPosition = ordinalPosition;
                                        jsonHash.primaryKeyIndicator = primaryKeyIndicator;
                                        jsonHash.multiActiveIndicator = multiActiveIndicator;
                                    }

                                    string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                                    File.WriteAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonModelMetadataFileName + FileConfiguration.jsonVersionExtension, output);
                                }
                                catch (JsonReaderException ex)
                                {
                                    richTextBoxInformation.Text += "There were issues applying the JSON update.\r\n" + ex;
                                }
                            }
                            else
                            {
                                richTextBoxInformation.Text += "There were issues identifying the repository type to apply changes.\r\n";
                            }
                        }
                        #endregion

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
                            string ordinalPosition = "0";
                            string primaryKeyIndicator = "";
                            string multiActiveIndicator = "";


                            if (row[0] != DBNull.Value)
                            {
                                databaseName = (string)row[2];
                            }

                            if (row[1] != DBNull.Value)
                            {
                                schemaName = (string)row[3];
                            }

                            if (row[2] != DBNull.Value)
                            {
                                tableName = (string)row[4];
                            }

                            if (row[3] != DBNull.Value)
                            {
                                columnName = (string)row[5];
                            }

                            if (row[4] != DBNull.Value)
                            {
                                dataType = (string)row[6];
                            }

                            if (row[5] != DBNull.Value)
                            {
                                maxLength = (string)row[7];
                            }

                            if (row[6] != DBNull.Value)
                            {
                                numericPrecision = (string)row[8];
                            }

                            if (row[7] != DBNull.Value)
                            {
                                ordinalPosition = (string)row[9];
                            }

                            if (row[8] != DBNull.Value)
                            {
                                primaryKeyIndicator = (string)row[10];
                            }

                            if (row[9] != DBNull.Value)
                            {
                                multiActiveIndicator = (string)row[11];
                            }

                            if (repositoryTarget == "SQLServer")
                            {
                                insertQueryTables.AppendLine("IF NOT EXISTS (SELECT * FROM [MD_VERSION_ATTRIBUTE] WHERE [VERSION_ID]= " + versionId + " AND [DATABASE_NAME] = '"+ databaseName+"' AND [SCHEMA_NAME]='" + schemaName + "' AND [TABLE_NAME]='" + tableName + "' AND [COLUMN_NAME]='" + columnName + "')");
                                insertQueryTables.AppendLine("INSERT INTO [MD_VERSION_ATTRIBUTE]");
                                insertQueryTables.AppendLine("([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME],[COLUMN_NAME],[DATA_TYPE],[CHARACTER_MAXIMUM_LENGTH],[NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR])");
                                insertQueryTables.AppendLine("VALUES");
                                insertQueryTables.AppendLine("(" + versionId + ", '"+databaseName+"' ,'"+schemaName+"' ,'" + tableName + "','" + columnName +
                                                             "','" + dataType + "','" + maxLength + "','" +
                                                             numericPrecision + "','" + ordinalPosition + "','" +
                                                             primaryKeyIndicator + "','" + multiActiveIndicator + "')");
                            }
                            else if (repositoryTarget == "JSON") //Update the JSON
                            {
                                try
                                {
                                    var jsonPhysicalModelMappingFull = new JArray();

                                    // Load the file, if existing information needs to be merged
                                    PhysicalModelMetadataJson[] jsonArray =
                                        JsonConvert.DeserializeObject<PhysicalModelMetadataJson[]>(
                                            File.ReadAllText(
                                                GlobalParameters.ConfigurationPath +
                                                GlobalParameters.JsonModelMetadataFileName + FileConfiguration.jsonVersionExtension));

                                    // Convert it into a JArray so segments can be added easily
                                    if (jsonArray != null)
                                    {
                                        jsonPhysicalModelMappingFull = JArray.FromObject(jsonArray);
                                    }
                                    //Generate a unique key using a hash

                                    string[] inputHashValue = new string[] { versionId.ToString(), tableName, columnName };
                                    var hashKey = Utility.CreateMd5(inputHashValue, GlobalParameters.SandingElement);

                                    JObject newJsonSegment = new JObject(
                                            new JProperty("versionAttributeHash", hashKey),
                                            new JProperty("versionId", versionId),
                                            new JProperty("databaseName", databaseName),
                                            new JProperty("schemaName", schemaName),
                                            new JProperty("tableName", tableName),
                                            new JProperty("columnName", columnName),
                                            new JProperty("dataType", dataType),
                                            new JProperty("characterMaximumLength", maxLength),
                                            new JProperty("numericPrecision", numericPrecision),
                                            new JProperty("ordinalPosition", ordinalPosition),
                                            new JProperty("primaryKeyIndicator", primaryKeyIndicator),
                                            new JProperty("multiActiveIndicator", multiActiveIndicator)
                                        );

                                    jsonPhysicalModelMappingFull.Add(newJsonSegment);

                                    string output = JsonConvert.SerializeObject(jsonPhysicalModelMappingFull, Formatting.Indented);
                                    File.WriteAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonModelMetadataFileName + FileConfiguration.jsonVersionExtension, output);

                                    //Making sure the hash key value is added to the datatable as well
                                    row[0] = hashKey;

                                }
                                catch (JsonReaderException ex)
                                {
                                    richTextBoxInformation.Text += "There were issues inserting the JSON segment / record.\r\n" + ex;
                                }
                            }
                            else
                            {
                                richTextBoxInformation.Text += "There were issues identifying the repository type to apply changes.\r\n";
                            }
                        }

                        #region Deleted Rows
                        //Deleted rows
                        if ((row.RowState & DataRowState.Deleted) != 0)
                        {
                            var hashKey = row["VERSION_ATTRIBUTE_HASH", DataRowVersion.Original].ToString();
                            var versionKey = row["VERSION_ID", DataRowVersion.Original].ToString();

                            if (repositoryTarget == "SQLServer")
                            {
                                insertQueryTables.AppendLine("DELETE FROM MD_VERSION_ATTRIBUTE");
                                insertQueryTables.AppendLine("WHERE [VERSION_ATTRIBUTE_HASH] = '" + hashKey + "' AND [VERSION_ID] = " + versionKey);
                            }
                            else if (repositoryTarget == "JSON") //Remove a segment (row) from the JSON
                            {
                                try
                                {
                                    var jsonArray =
                                        JsonConvert.DeserializeObject<PhysicalModelMetadataJson[]>(
                                            File.ReadAllText(GlobalParameters.ConfigurationPath +
                                                             GlobalParameters.JsonModelMetadataFileName + FileConfiguration.jsonVersionExtension)).ToList();

                                    //Retrieves the json segment in the file for the given hash returns value or NULL
                                    var jsonSegment = jsonArray.FirstOrDefault(obj => obj.versionAttributeHash == hashKey);

                                    jsonArray.Remove(jsonSegment);

                                    if (jsonSegment.versionAttributeHash == "")
                                    {
                                        richTextBoxInformation.Text += "The correct segment in the JSON file was not found.\r\n";
                                    }
                                    else
                                    {
                                        //Remove the segment from the JSON
                                        jsonArray.Remove(jsonSegment);
                                    }

                                    string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                                    File.WriteAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonModelMetadataFileName + FileConfiguration.jsonVersionExtension, output);

                                }
                                catch (JsonReaderException ex)
                                {
                                    richTextBoxInformation.Text += "There were issues applying the JSON update.\r\n" + ex;
                                }
                            }
                            else
                            {
                                richTextBoxInformation.Text += "There were issues identifying the repository type to apply changes.\r\n";
                            }
                        }
                        #endregion

                    } // All changes have been processed.

                    #region Statement execution
                    // Execute the statement, if the repository is SQL Server
                    // If the source is JSON this is done in separate calls for now
                    if (repositoryTarget == "SQLServer")
                    {
                        if (insertQueryTables.ToString() == null || insertQueryTables.ToString() == "")
                        {
                            richTextBoxInformation.Text += "No model metadata changes were saved.\r\n";
                        }
                        else
                        {
                            using (var connection = new SqlConnection(ConfigurationSettings.ConnectionStringOmd))
                            {
                                var command = new SqlCommand(insertQueryTables.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                    dataTableChanges.AcceptChanges();
                                    ((DataTable)_bindingSourcePhysicalModelMetadata.DataSource).AcceptChanges();
                                }
                                catch (Exception ex)
                                {
                                    richTextBoxInformation.Text += "An issue has occurred: " + ex;
                                }
                            }
                        }
                    }

                    //Committing the changes to the datatable
                    dataTableChanges.AcceptChanges();
                    ((DataTable)_bindingSourcePhysicalModelMetadata.DataSource).AcceptChanges();

                    //The JSON needs to be re-bound to the datatable / datagrid after being updated to allow all values to be present
                    if (repositoryTarget == "JSON")
                    {
                        BindModelMetadataJsonToDataTable();
                    }

                    richTextBoxInformation.Text += "The (physical) model metadata has been saved.\r\n";
                    #endregion
                }
            }
        }

        private void SaveAttributeMappingMetadata(int versionId, DataTable dataTableChanges, string repositoryTarget)
        {
            if (FileConfiguration.newFileAttributeMapping == "true")
            {
                JsonHandling.RemoveExistingJsonFile(GlobalParameters.JsonAttributeMappingFileName + @"_v" + GlobalParameters.CurrentVersionId + ".json");
                JsonHandling.CreatePlaceholderJsonFile(GlobalParameters.JsonAttributeMappingFileName);
                FileConfiguration.newFileAttributeMapping = "false";
            }

            //If the save version radiobutton is selected it means either minor or major version is checked and a full new snapshot needs to be created first
            if (!radiobuttonNoVersionChange.Checked)
            {
                if (ConfigurationSettings.MetadataRepositoryType == "SQLServer")
                {
                    CreateNewAttributeMappingMetadataVersionSqlServer(versionId);
                }
                else if (ConfigurationSettings.MetadataRepositoryType == "JSON")
                {
                    CreateNewAttributeMappingMetadataVersionJson(versionId);
                }
            }

            #region In-version change
            else //An update (no change) to the existing version is done with regular inserts, updates and deletes
            {

                var insertQueryTables = new StringBuilder();

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
                            var hashKey = (string)row["ATTRIBUTE_MAPPING_HASH"];
                            var versionKey = row["VERSION_ID"].ToString();
                            var stagingTable = "";
                            var stagingColumn = "";
                            var integrationTable = "";
                            var integrationColumn = "";
                            var transformationRule = "";

                            if (row["SOURCE_TABLE"] != DBNull.Value)
                            {
                                stagingTable = (string)row["SOURCE_TABLE"];
                            }

                            if (row["SOURCE_COLUMN"] != DBNull.Value)
                            {
                                stagingColumn = (string)row["SOURCE_COLUMN"];
                            }

                            if (row["TARGET_TABLE"] != DBNull.Value)
                            {
                                integrationTable = (string)row["TARGET_TABLE"];
                            }

                            if (row["TARGET_COLUMN"] != DBNull.Value)
                            {
                                integrationColumn = (string)row["TARGET_COLUMN"];
                            }

                            if (row["TRANSFORMATION_RULE"] != DBNull.Value)
                            {
                                transformationRule = (string)row["TRANSFORMATION_RULE"];
                            }

                            //Double quotes for composites, but only if things are written to the database otherwise it's already OK
                            if (repositoryTarget == "SQLServer") //Update the tables in SQL Server
                            {
                                transformationRule = transformationRule.Replace("'", "''");
                            }



                            if (repositoryTarget == "SQLServer")
                            {
                                insertQueryTables.AppendLine("UPDATE MD_ATTRIBUTE_MAPPING");
                                insertQueryTables.AppendLine("SET [SOURCE_TABLE] = '" + stagingTable +
                                                             "',[SOURCE_COLUMN] = '" + stagingColumn +
                                                             "', [TARGET_TABLE] = '" + integrationTable +
                                                             "', [TARGET_COLUMN] = '" + integrationColumn +
                                                             "',[TRANSFORMATION_RULE] = '" + transformationRule + "'");
                                insertQueryTables.AppendLine("WHERE [ATTRIBUTE_MAPPING_HASH] = '" + hashKey +
                                                             "' AND [VERSION_ID] = " + versionKey);
                            }

                            else if (repositoryTarget == "JSON") //Insert a new segment (row) in the JSON
                            {

                                try
                                {
                                    AttributeMappingJson[] jsonArray =
                                        JsonConvert.DeserializeObject<AttributeMappingJson[]>(
                                            File.ReadAllText(GlobalParameters.ConfigurationPath +
                                                             GlobalParameters.JsonAttributeMappingFileName + FileConfiguration.jsonVersionExtension));

                                    var jsonHash = jsonArray.FirstOrDefault(obj => obj.attributeMappingHash == hashKey);
                                    //Retrieves the json segment in the file for the given hash returns value or NULL

                                    if (jsonHash.attributeMappingHash == "")
                                    {
                                        richTextBoxInformation.Text +=
                                            "The correct segment in the JSON file was not found.\r\n";
                                    }
                                    else
                                    {
                                        // Update the values in the JSON segment
                                        jsonHash.sourceTable = stagingTable;
                                        jsonHash.sourceAttribute = stagingColumn;
                                        jsonHash.targetTable = integrationTable;
                                        jsonHash.targetAttribute = integrationColumn;
                                        jsonHash.transformationRule = transformationRule;
                                    }

                                    string output = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                                    File.WriteAllText(
                                        GlobalParameters.ConfigurationPath +
                                        GlobalParameters.JsonAttributeMappingFileName + FileConfiguration.jsonVersionExtension, output);
                                }
                                catch (JsonReaderException ex)
                                {
                                    richTextBoxInformation.Text += "There were issues applying the JSON update.\r\n" +
                                                                   ex;
                                }
                            }
                            else
                            {
                                richTextBoxInformation.Text +=
                                    "There were issues identifying the repository type to apply changes.\r\n";
                            }
                        }

                        #endregion

                        #region Inserts in Attribute Mapping

                        // Inserts
                        if ((row.RowState & DataRowState.Added) != 0)
                        {
                            var stagingTable = "";
                            var stagingColumn = "";
                            var integrationTable = "";
                            var integrationColumn = "";
                            var transformationRule = "";

                            if (row[2] != DBNull.Value)
                            {
                                stagingTable = (string)row[2];
                            }

                            if (row[3] != DBNull.Value)
                            {
                                stagingColumn = (string)row[3];
                            }

                            if (row[4] != DBNull.Value)
                            {
                                integrationTable = (string)row[4];
                            }

                            if (row[5] != DBNull.Value)
                            {
                                integrationColumn = (string)row[5];
                            }

                            if (row[6] != DBNull.Value)
                            {
                                transformationRule = (string)row[6];
                            }

                            if (repositoryTarget == "SQLServer")
                            {
                                insertQueryTables.AppendLine("INSERT INTO MD_ATTRIBUTE_MAPPING");
                                insertQueryTables.AppendLine(
                                    "([VERSION_ID],[SOURCE_TABLE],[SOURCE_COLUMN],[TARGET_TABLE],[TARGET_COLUMN],[TRANSFORMATION_RULE])");
                                insertQueryTables.AppendLine("VALUES (" + versionId + ",'" + stagingTable + "','" +
                                                             stagingColumn + "','" + integrationTable + "','" +
                                                             integrationColumn + "','" + transformationRule + "')");
                            }
                            else if (repositoryTarget == "JSON") //Update the JSON
                            {
                                try
                                {
                                    var jsonAttributeMappingFull = new JArray();

                                    // Load the file, if existing information needs to be merged
                                    AttributeMappingJson[] jsonArray =
                                        JsonConvert.DeserializeObject<AttributeMappingJson[]>(
                                            File.ReadAllText(
                                                GlobalParameters.ConfigurationPath +
                                                GlobalParameters.JsonAttributeMappingFileName + FileConfiguration.jsonVersionExtension));

                                    // Convert it into a JArray so segments can be added easily
                                    if (jsonArray != null)
                                    {
                                        jsonAttributeMappingFull = JArray.FromObject(jsonArray);
                                    }

                                    string[] inputHashValue = new string[] { versionId.ToString(), stagingTable, stagingColumn, integrationTable, integrationColumn, transformationRule };
                                    var hashKey = Utility.CreateMd5(inputHashValue, GlobalParameters.SandingElement);

                                    JObject newJsonSegment = new JObject(
                                        new JProperty("attributeMappingHash", hashKey),
                                        new JProperty("versionId", versionId),
                                        new JProperty("sourceTable", stagingTable),
                                        new JProperty("sourceAttribute", stagingColumn),
                                        new JProperty("targetTable", integrationTable),
                                        new JProperty("targetAttribute", integrationColumn),
                                        new JProperty("transformationRule", transformationRule)
                                        );

                                    jsonAttributeMappingFull.Add(newJsonSegment);

                                    string output = JsonConvert.SerializeObject(jsonAttributeMappingFull,
                                        Formatting.Indented);
                                    File.WriteAllText(
                                        GlobalParameters.ConfigurationPath +
                                        GlobalParameters.JsonAttributeMappingFileName + FileConfiguration.jsonVersionExtension, output);

                                    //Making sure the hash key value is added to the datatable as well
                                    row[0] = hashKey;

                                }
                                catch (JsonReaderException ex)
                                {
                                    richTextBoxInformation.Text +=
                                        "There were issues inserting the JSON segment / record.\r\n" + ex;
                                }
                            }
                            else
                            {
                                richTextBoxInformation.Text +=
                                    "There were issues identifying the repository type to apply changes.\r\n";
                            }
                        }

                        #endregion

                        #region Deletes in Attribute Mapping

                        // Deletes
                        if ((row.RowState & DataRowState.Deleted) != 0)
                        {
                            var hashKey = row["ATTRIBUTE_MAPPING_HASH", DataRowVersion.Original].ToString();
                            var versionKey = row["VERSION_ID", DataRowVersion.Original].ToString();

                            if (repositoryTarget == "SQLServer")
                            {
                                insertQueryTables.AppendLine("DELETE FROM MD_ATTRIBUTE_MAPPING");
                                insertQueryTables.AppendLine("WHERE [ATTRIBUTE_MAPPING_HASH] = '" + hashKey +
                                                             "' AND [VERSION_ID] = " + versionKey);
                            }
                            else if (repositoryTarget == "JSON") //Insert a new segment (row) in the JSON
                            {
                                try
                                {
                                    var jsonArray =
                                        JsonConvert.DeserializeObject<AttributeMappingJson[]>(
                                            File.ReadAllText(GlobalParameters.ConfigurationPath +
                                                             GlobalParameters.JsonAttributeMappingFileName + FileConfiguration.jsonVersionExtension)).ToList();

                                    //Retrieves the json segment in the file for the given hash returns value or NULL
                                    var jsonSegment =
                                        jsonArray.FirstOrDefault(obj => obj.attributeMappingHash == hashKey);

                                    jsonArray.Remove(jsonSegment);

                                    if (jsonSegment.attributeMappingHash == "")
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
                                    File.WriteAllText(
                                        GlobalParameters.ConfigurationPath +
                                        GlobalParameters.JsonAttributeMappingFileName + FileConfiguration.jsonVersionExtension, output);

                                }
                                catch (JsonReaderException ex)
                                {
                                    richTextBoxInformation.Text += "There were issues applying the JSON update.\r\n" +
                                                                   ex;
                                }
                            }
                            else
                            {
                                richTextBoxInformation.Text +=
                                    "There were issues identifying the repository type to apply changes.\r\n";
                            }
                        }

                        #endregion
                    }

                    #region Statement execution

                    // Execute the statement, if the repository is SQL Server
                    // If the source is JSON this is done in separate calls for now
                    if (repositoryTarget == "SQLServer")
                    {
                        if (insertQueryTables.ToString() == "")
                        {
                            richTextBoxInformation.Text +=
                                "No Business Key / Table mapping metadata changes were saved.\r\n";
                        }
                        else
                        {
                            using (var connection = new SqlConnection(ConfigurationSettings.ConnectionStringOmd))
                            {
                                var command = new SqlCommand(insertQueryTables.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    richTextBoxInformation.Text += "An issue has occurred: " + ex;
                                }
                            }
                        }
                    }

                    //Committing the changes to the datatable
                    dataTableChanges.AcceptChanges();
                    ((DataTable)_bindingSourceAttributeMetadata.DataSource).AcceptChanges();

                    //The JSON needs to be re-bound to the datatable / datagrid after being updated to allow all values to be present
                    if (repositoryTarget == "JSON")
                    {
                        BindAttributeMappingJsonToDataTable();
                    }

                    richTextBoxInformation.Text += "The Attribute Mapping metadata has been saved.\r\n";

                    #endregion
                }

            }
            #endregion

        }

        private void BindTableMappingJsonToDataTable()
        {
            var versionId = CreateOrRetrieveVersion();
            var JsonVersionExtension = @"_v" + versionId + ".json";

            // Load the table mapping file, convert it to a DataTable and bind it to the source
            List<TableMappingJson> jsonArray = JsonConvert.DeserializeObject<List<TableMappingJson>>(File.ReadAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonTableMappingFileName + JsonVersionExtension));
            DataTable dt = Utility.ConvertToDataTable(jsonArray);

            //Make sure the changes are seen as committed, so that changes can be detected later on.
            dt.AcceptChanges(); 

            SetTeamDataTableMapping.SetTableDataTableColumns(dt);
            _bindingSourceTableMetadata.DataSource = dt;
        }

        private void BindAttributeMappingJsonToDataTable()
        {
            var versionId = CreateOrRetrieveVersion();
            var JsonVersionExtension = @"_v" + versionId + ".json";

            // Load the attribute mapping file, convert it to a DataTable and bind it to the source
            List<AttributeMappingJson> jsonArray = JsonConvert.DeserializeObject<List<AttributeMappingJson>>(File.ReadAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonAttributeMappingFileName + JsonVersionExtension));
            DataTable dt = Utility.ConvertToDataTable(jsonArray);

            //Make sure the changes are seen as committed, so that changes can be detected later on.
            dt.AcceptChanges(); 

            SetTeamDataTableMapping.SetAttributeDataTableColumns(dt);
            _bindingSourceAttributeMetadata.DataSource = dt;
        }

        private void BindModelMetadataJsonToDataTable()
        {
            var versionId = CreateOrRetrieveVersion();
            var JsonVersionExtension = @"_v" + versionId + ".json";

            // Load the table mapping file, convert it to a DataTable and bind it to the source
            List<PhysicalModelMetadataJson> jsonArray = JsonConvert.DeserializeObject<List<PhysicalModelMetadataJson>>(File.ReadAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonModelMetadataFileName + JsonVersionExtension));
            DataTable dt = Utility.ConvertToDataTable(jsonArray);

            //Make sure the changes are seen as committed, so that changes can be detected later on.
            dt.AcceptChanges(); 

            SetTeamDataTableMapping.SetPhysicalModelDataTableColumns(dt);
            _bindingSourcePhysicalModelMetadata.DataSource = dt;
        }

        /// <summary>
        ///   Load a Table Mapping Metadata Json or XML file into the datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openMetadataFileToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Business Key Metadata File",
                Filter = @"Business Key files|*.xml;*.json",
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
                                var backupFile = new JsonHandling();
                                var targetFileName = backupFile.BackupJsonFile(GlobalParameters.JsonTableMappingFileName + @"_v" + GlobalParameters.CurrentVersionId +".json", FormBase.GlobalParameters.ConfigurationPath);
                                richTextBoxInformation.Text ="A backup of the in-use JSON file was created as " + targetFileName + ".\r\n\r\n";
                            }
                            catch (Exception exception)
                            {
                                richTextBoxInformation.Text ="An issue occured when trying to make a backup of the in-use JSON file. The error message was " +exception + ".";
                            }
                        }

                        // If the information needs to be merged, a global parameter needs to be set.
                        // This will overwrite existing files for the in-use version.
                        if (!checkBoxMergeFiles.Checked)
                        {
                            FileConfiguration.newFileTableMapping = "true";
                        }

                        // Load the file, convert it to a DataTable and bind it to the source
                        List<TableMappingJson> jsonArray = JsonConvert.DeserializeObject<List<TableMappingJson>>(File.ReadAllText(chosenFile));
                        DataTable dt = Utility.ConvertToDataTable(jsonArray);

                        // Setup the datatable with proper headings.
                        SetTeamDataTableMapping.SetTableDataTableColumns(dt);

                        // Sort the columns
                        SetTeamDataTableMapping.SetTableDataTableSorting(dt);

                        // Clear out the existing data from the grid
                        _bindingSourceTableMetadata.DataSource = null;
                        _bindingSourceTableMetadata.Clear();
                        dataGridViewTableMetadata.DataSource = null;
 
                        // Bind the datatable to the gridview
                        _bindingSourceTableMetadata.DataSource = dt;

                        if (jsonArray != null)
                        {
                            // Set the column header names
                            dataGridViewTableMetadata.DataSource = _bindingSourceTableMetadata;
                            dataGridViewTableMetadata.ColumnHeadersVisible = true;
                            dataGridViewTableMetadata.Columns[0].Visible = false;
                            dataGridViewTableMetadata.Columns[1].Visible = false;

                            dataGridViewTableMetadata.Columns[0].HeaderText = "Hash Key";
                            dataGridViewTableMetadata.Columns[1].HeaderText = "Version ID";
                            dataGridViewTableMetadata.Columns[2].HeaderText = "Source Table";
                            dataGridViewTableMetadata.Columns[3].HeaderText = "Target Table";
                            dataGridViewTableMetadata.Columns[4].HeaderText = "Business Key Definition";
                            dataGridViewTableMetadata.Columns[5].HeaderText = "Driving Key Definition";
                            dataGridViewTableMetadata.Columns[6].HeaderText = "Filter Criteria";
                            dataGridViewTableMetadata.Columns[7].HeaderText = "Process Indicator";
                        }
                    }

                    GridAutoLayoutTableMappingMetadata();
                    ContentCounter();
                    richTextBoxInformation.AppendText("The file " + chosenFile + " was loaded.\r\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error has been encountered! The reported error is: "+ex);
                }
            }
        }

        private void OpenAttributeFileMenuItem_Click(object sender, EventArgs e)
        {
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Attribute Mapping Metadata File",
                Filter = @"Attribute Mapping files|*.xml;*.json",
                InitialDirectory =  GlobalParameters.ConfigurationPath //Application.StartupPath + @"\Configuration\"
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
                                var backupFile = new JsonHandling();
                                var targetFileName = backupFile.BackupJsonFile(GlobalParameters.JsonAttributeMappingFileName + @"_v" + GlobalParameters.CurrentVersionId + ".json", GlobalParameters.ConfigurationPath);
                                richTextBoxInformation.Text = "A backup of the in-use JSON file was created as " + targetFileName + ".\r\n\r\n";
                            }
                            catch (Exception exception)
                            {
                                richTextBoxInformation.Text = "An issue occured when trying to make a backup of the in-use JSON file. The error message was " + exception + ".";
                            }
                        }

                    // If the information needs to be merged, a global parameter needs to be set.
                    // This will overwrite existing files for the in-use version.
                    if (!checkBoxMergeFiles.Checked)
                    {
                        FileConfiguration.newFileAttributeMapping = "true";
                    }                                           


                    // Load the file, convert it to a DataTable and bind it to the source
                    List<AttributeMappingJson> jsonArray = JsonConvert.DeserializeObject<List<AttributeMappingJson>>(File.ReadAllText(chosenFile));
                    DataTable dt = Utility.ConvertToDataTable(jsonArray);

                    // Set the column names in the datatable.
                    SetTeamDataTableMapping.SetAttributeDataTableColumns(dt);
                    // Sort the columns in the datatable.
                    SetTeamDataTableMapping.SetAttributeDatTableSorting(dt);

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
                            dataGridViewAttributeMetadata.Columns[6].ReadOnly = true;

                            dataGridViewAttributeMetadata.Columns[0].HeaderText = "Hash Key";
                            dataGridViewAttributeMetadata.Columns[1].HeaderText = "Version ID";
                            dataGridViewAttributeMetadata.Columns[2].HeaderText = "Source Table";
                            dataGridViewAttributeMetadata.Columns[3].HeaderText = "Source Column";
                            dataGridViewAttributeMetadata.Columns[4].HeaderText = "Target Table";
                            dataGridViewAttributeMetadata.Columns[5].HeaderText = "Target Column";
                            dataGridViewAttributeMetadata.Columns[6].HeaderText = "Transformation Rule";
                        }
                    }

                    GridAutoLayoutAttributeMetadata();
                    richTextBoxInformation.Text = "The metadata has been loaded from file.\r\n";
                    ContentCounter();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void createTemporaryWorkerTable(string connString)
        {
            var inputTableMapping = (DataTable)_bindingSourceTableMetadata.DataSource;
            var inputAttributeMapping = (DataTable)_bindingSourceAttributeMetadata.DataSource;
            var inputPhysicalModel = (DataTable)_bindingSourcePhysicalModelMetadata.DataSource;

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
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[TARGET_COLUMN])),'NA')+'|'+");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[SOURCE_TABLE])),'NA')+'|'+");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[SOURCE_COLUMN])),'NA')+'|' +");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[TRANSFORMATION_RULE])),'NA')+'|'");
            createStatement.AppendLine("			),(2)");
            createStatement.AppendLine("		)");
            createStatement.AppendLine("	) PERSISTED NOT NULL,");
            createStatement.AppendLine("	[VERSION_ID]          integer NOT NULL,");
            createStatement.AppendLine("	[SOURCE_TABLE]        varchar(100)  NULL,");
            createStatement.AppendLine("	[SOURCE_TABLE_TYPE]   varchar(100)  NULL,");
            createStatement.AppendLine("	[SOURCE_COLUMN]       varchar(100)  NULL,");
            createStatement.AppendLine("	[TARGET_TABLE]        varchar(100)  NULL,");
            createStatement.AppendLine("	[TARGET_TABLE_TYPE]   varchar(100)  NULL,");
            createStatement.AppendLine("	[TARGET_COLUMN]       varchar(100)  NULL,");
            createStatement.AppendLine("	[TRANSFORMATION_RULE] varchar(4000)  NULL,");
            createStatement.AppendLine("   CONSTRAINT [PK_TMP_MD_ATTRIBUTE_MAPPING] PRIMARY KEY CLUSTERED ([ATTRIBUTE_MAPPING_HASH] ASC, [VERSION_ID] ASC)");
            createStatement.AppendLine(")");

            executeSqlCommand(createStatement, connString);
            createStatement.Clear();

            foreach (DataRow row in inputAttributeMapping.Rows)
            {
                //LBM 2019-01-31 -- ENSURE NO NULLS ARE INSERTED IN THE TABLE
                string sourceTable       = "";
                string SOURCE_COLUMN      ="";
                string targetTable       ="";
                string TARGET_COLUMN      ="";
                string TRANSFORMATION_RULE="";

                if (row["SOURCE_TABLE"] != DBNull.Value)
                    sourceTable = (string)row["SOURCE_TABLE"];
                if (row["SOURCE_COLUMN"] != DBNull.Value)
                    SOURCE_COLUMN = (string)row["SOURCE_COLUMN"];
                if (row["TARGET_TABLE"] != DBNull.Value)
                    targetTable = (string)row["TARGET_TABLE"];
                if (row["TARGET_COLUMN"] != DBNull.Value)
                    TARGET_COLUMN = (string)row["TARGET_COLUMN"];
                if (row["TRANSFORMATION_RULE"] != DBNull.Value)
                    TRANSFORMATION_RULE = (string)row["TRANSFORMATION_RULE"];

                var fullyQualifiedSourceName = ClassMetadataHandling.GetFullyQualifiedTableName(sourceTable);
                var sourceType = ClassMetadataHandling.GetTableType(sourceTable, "");

                var fullyQualifiedTargetName = ClassMetadataHandling.GetFullyQualifiedTableName(targetTable);
                var targetType = ClassMetadataHandling.GetTableType(targetTable, "");

                createStatement.AppendLine("INSERT[dbo].[TMP_MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_TABLE_TYPE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_TABLE_TYPE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'" + fullyQualifiedSourceName + "', '"+sourceType+"' ,N'" + SOURCE_COLUMN + "', N'" + fullyQualifiedTargetName + "', '"+targetType+"' , N'" + TARGET_COLUMN + "', N'" + TRANSFORMATION_RULE+ "');");
            }

            executeSqlCommand(createStatement, connString);
            createStatement.Clear();


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
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[BUSINESS_KEY_ATTRIBUTE])),'NA')+'|'+");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[DRIVING_KEY_ATTRIBUTE])),'NA')+'|'+");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[FILTER_CRITERIA])),'NA')+'|'");
            createStatement.AppendLine("			),(2)");
            createStatement.AppendLine("			)");
            createStatement.AppendLine("		) PERSISTED NOT NULL ,");
            createStatement.AppendLine("	[VERSION_ID] integer NOT NULL ,");
            createStatement.AppendLine("	[SOURCE_TABLE] varchar(100)  NULL,");
            createStatement.AppendLine("	[SOURCE_TABLE_TYPE] varchar(100)  NULL,");
            createStatement.AppendLine("	[BUSINESS_KEY_ATTRIBUTE] varchar(4000)  NULL,");
            createStatement.AppendLine("	[DRIVING_KEY_ATTRIBUTE] varchar(4000)  NULL,");
            createStatement.AppendLine("	[TARGET_TABLE] varchar(100)  NULL,");
            createStatement.AppendLine("	[TARGET_TABLE_TYPE] varchar(100)  NULL,");
            createStatement.AppendLine("	[FILTER_CRITERIA] varchar(4000)  NULL,");
            createStatement.AppendLine("	[PROCESS_INDICATOR] varchar(1)  NULL,");
            createStatement.AppendLine("    CONSTRAINT [PK_TMP_MD_TABLE_MAPPING] PRIMARY KEY CLUSTERED([TABLE_MAPPING_HASH] ASC, [VERSION_ID] ASC)");
            createStatement.AppendLine(")");

            executeSqlCommand(createStatement, connString);
            createStatement.Clear();

            foreach (DataRow row in inputTableMapping.Rows)
            {
                string sourceTable = "";
                string BUSINESS_KEY_ATTRIBUTE = "";
                string targetTable = "";
                string FILTER_CRITERIA = "";
                string DRIVING_KEY_ATTRIBUTE = "";
                string PROCESS_INDICATOR = "";

                if (row["SOURCE_TABLE"] != DBNull.Value)
                    sourceTable = (string)row["SOURCE_TABLE"];
                if (row["BUSINESS_KEY_ATTRIBUTE"] != DBNull.Value) 
                    BUSINESS_KEY_ATTRIBUTE = (string)row["BUSINESS_KEY_ATTRIBUTE"];
                if (row["TARGET_TABLE"] != DBNull.Value)
                    targetTable = (string)row["TARGET_TABLE"];
                if (row["FILTER_CRITERIA"] != DBNull.Value)
                {
                    FILTER_CRITERIA = (string)row["FILTER_CRITERIA"];
                    FILTER_CRITERIA = FILTER_CRITERIA.Replace("'", "''");
                }
                if (row["DRIVING_KEY_ATTRIBUTE"] != DBNull.Value)
                    DRIVING_KEY_ATTRIBUTE = (string)row["DRIVING_KEY_ATTRIBUTE"];
                if (row["PROCESS_INDICATOR"] != DBNull.Value)
                    PROCESS_INDICATOR = (string)row["PROCESS_INDICATOR"];

                var fullyQualifiedSourceName = ClassMetadataHandling.GetFullyQualifiedTableName(sourceTable);
                var sourceType = ClassMetadataHandling.GetTableType(sourceTable,"");

                var fullyQualifiedTargetName = ClassMetadataHandling.GetFullyQualifiedTableName(targetTable);
                var targetType = ClassMetadataHandling.GetTableType(targetTable, "");

                createStatement.AppendLine("INSERT [dbo].[TMP_MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_TABLE_TYPE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [TARGET_TABLE_TYPE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'" + fullyQualifiedSourceName + "', '"+sourceType+"' , N'" + BUSINESS_KEY_ATTRIBUTE.Replace("'","''") + "', N'" + fullyQualifiedTargetName + "', '"+targetType+"' , N'" + FILTER_CRITERIA + "', '" + DRIVING_KEY_ATTRIBUTE + "', '" + PROCESS_INDICATOR + "');");
            }

            executeSqlCommand(createStatement, connString);
            createStatement.Clear();


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
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[DATABASE_NAME])),'NA')+'|'+");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[SCHEMA_NAME])),'NA')+'|'+");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[TABLE_NAME])),'NA')+'|'+");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[COLUMN_NAME])),'NA')+'|'+");
            createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[VERSION_ID])),'NA')+'|'");
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
            createStatement.AppendLine("    [ORDINAL_POSITION]   integer NULL,");
            createStatement.AppendLine("    [PRIMARY_KEY_INDICATOR] varchar(1)  NULL ,");
            createStatement.AppendLine("	[MULTI_ACTIVE_INDICATOR] varchar(1)  NULL ");
            createStatement.AppendLine(")");
            createStatement.AppendLine("");
            createStatement.AppendLine("ALTER TABLE [TMP_MD_VERSION_ATTRIBUTE]");
            createStatement.AppendLine("    ADD CONSTRAINT[PK_TMP_MD_VERSION_ATTRIBUTE] PRIMARY KEY CLUSTERED([DATABASE_NAME] ASC, [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [VERSION_ID] ASC)");
            createStatement.AppendLine();

            executeSqlCommand(createStatement, connString);
            createStatement.Clear();

            // Load the datatable into the worker table for the physical model 
            foreach (DataRow row in inputPhysicalModel.Rows)
            {
                string databaseName = "";
                string schemaName = "";
                string tableName = "";
                string columnName = "";

                if (row["DATABASE_NAME"] != DBNull.Value)
                    databaseName = (string)row["DATABASE_NAME"];
                if (row["SCHEMA_NAME"] != DBNull.Value)
                    schemaName = (string)row["SCHEMA_NAME"];
                if (row["TABLE_NAME"] != DBNull.Value) 
                    tableName = (string)row["TABLE_NAME"];
                if (row["COLUMN_NAME"] != DBNull.Value)
                    columnName = (string)row["COLUMN_NAME"];

                createStatement.AppendLine("INSERT [dbo].[TMP_MD_VERSION_ATTRIBUTE]" +
                                           " ([VERSION_ID], " +
                                           "[DATABASE_NAME], " +
                                           "[SCHEMA_NAME], " +
                                           "[TABLE_NAME], " +
                                           "[COLUMN_NAME], " +
                                           "[DATA_TYPE], " +
                                           "[CHARACTER_MAXIMUM_LENGTH], " +
                                           "[NUMERIC_PRECISION], " +
                                           "[ORDINAL_POSITION], " +
                                           "[PRIMARY_KEY_INDICATOR], " +
                                           "[MULTI_ACTIVE_INDICATOR]) " +
                                           "VALUES(" +
                                           "0, " +
                                           "N'" + databaseName + "', " +
                                           "N'" + schemaName + "', " +
                                           "N'" + tableName + "', " +
                                           "N'" + columnName + "', " +
                                           "N'" + (string)row["DATA_TYPE"] + "', " +
                                           "N'" + (string)row["CHARACTER_MAXIMUM_LENGTH"] + "', " +
                                           "N'" + (string)row["NUMERIC_PRECISION"] + "', " +
                                           "N'" + (string)row["ORDINAL_POSITION"] + "', " +
                                           "N'" + (string)row["PRIMARY_KEY_INDICATOR"] + "', " +
                                           "N'" + (string)row["MULTI_ACTIVE_INDICATOR"] + "');");
            }

            executeSqlCommand(createStatement, connString);
            createStatement.Clear();

        }

        private void executeSqlCommand(StringBuilder inputString, string connString)
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

        private void droptemporaryWorkerTable(string connString)
        {
            // Attribute mapping
            var createStatement = new StringBuilder();
            createStatement.AppendLine("-- Attribute mapping");
            createStatement.AppendLine("IF OBJECT_ID('[TMP_MD_ATTRIBUTE_MAPPING]', 'U') IS NOT NULL");
            createStatement.AppendLine(" DROP TABLE [TMP_MD_ATTRIBUTE_MAPPING]");

            executeSqlCommand(createStatement, connString);
            createStatement.Clear();

            // Table Mapping
            createStatement.AppendLine("-- Table Mapping");
            createStatement.AppendLine("IF OBJECT_ID('[TMP_MD_TABLE_MAPPING]', 'U') IS NOT NULL");
            createStatement.AppendLine(" DROP TABLE[TMP_MD_TABLE_MAPPING]");

            executeSqlCommand(createStatement, connString);
            createStatement.Clear();        
  
            // Physical Model
            createStatement.AppendLine("-- Version Attribute");
            createStatement.AppendLine("IF OBJECT_ID('[TMP_MD_VERSION_ATTRIBUTE]', 'U') IS NOT NULL");
            createStatement.AppendLine(" DROP TABLE [TMP_MD_VERSION_ATTRIBUTE]");

            executeSqlCommand(createStatement, connString);
            createStatement.Clear();
        }

        # region Background worker
        private void buttonStart_Click(object sender, EventArgs e)
        {
            #region Validation
            // The first thing to happen is to check if the validation needs to be run (and started if the answer to this is yes)
            if (checkBoxValidation.Checked)
            {
                if (radioButtonPhysicalMode.Checked == false && _bindingSourcePhysicalModelMetadata.Count == 0)
                {
                    richTextBoxInformation.Text += "There is no model metadata available, so the metadata can only be validated with the 'Ignore Version' enabled.\r\n ";
                }
                else
                {
                    if (backgroundWorkerValidationOnly.IsBusy) return;
                    // create a new instance of the alert form
                    _alertValidation = new Form_Alert();
                    _alertValidation.SetFormName("Validating the metadata");
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
            if (!checkBoxValidation.Checked || (checkBoxValidation.Checked && MetadataParameters.ValidationIssues == 0))
            {
                // Commence the activation
                var connOmd = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };

                richTextBoxInformation.Clear();

                var versionMajorMinor = GetVersion(trackBarVersioning.Value, connOmd);
                var majorVersion = versionMajorMinor.Key;
                var minorVersion = versionMajorMinor.Value;
                richTextBoxInformation.Text += "Commencing preparation / activation for version " + majorVersion + "." + minorVersion + ".\r\n";

                // Move data from the grids into temp tables
                createTemporaryWorkerTable(ConfigurationSettings.ConnectionStringOmd);

                if (radioButtonPhysicalMode.Checked == false)
                {
                    var versionExistenceCheck = new StringBuilder();

                    versionExistenceCheck.AppendLine("SELECT * FROM TMP_MD_VERSION_ATTRIBUTE WHERE VERSION_ID = " + trackBarVersioning.Value);

                    var versionExistenceCheckDataTable = Utility.GetDataTable(ref connOmd, versionExistenceCheck.ToString());

                    if (versionExistenceCheckDataTable != null && versionExistenceCheckDataTable.Rows.Count > 0)
                    {
                        if (backgroundWorkerMetadata.IsBusy) return;
                        // create a new instance of the alert form
                        _alert = new Form_Alert();
                        // event handler for the Cancel button in AlertForm
                        _alert.Canceled += buttonCancel_Click;
                        _alert.Show();
                        // Start the asynchronous operation.
                        backgroundWorkerMetadata.RunWorkerAsync();
                    }
                    else
                    {
                        richTextBoxInformation.Text += "There is no model metadata available for this version, so the metadata can only be activated with the 'Ignore Version' enabled for this specific version.\r\n ";
                    }
                }
                else
                {
                    if (backgroundWorkerMetadata.IsBusy) return;
                    // create a new instance of the alert form
                    _alert = new Form_Alert();
                    // event handler for the Cancel button in AlertForm
                    _alert.Canceled += buttonCancel_Click;
                    _alert.Show();
                    // Start the asynchronous operation.
                    backgroundWorkerMetadata.RunWorkerAsync();
                }
            }
            else
            {
                richTextBoxInformation.Text = "Validation found issues which should be investigated. If you would like to continue, please uncheck the validation and activate the metadata again.\r\n ";
            }
            #endregion
        }

        /// <summary>
        /// This event handler cancels the backgroundworker, fired from Cancel button in AlertForm.
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
        /// Multithreading for informaing the user when version changes (to other forms)
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
                    // After activation, the Json information can be created if enabled.
                    List<string> dummyFilter = new List<string>();
                    GenerateFromPattern(ConfigurationSettings.patternDefinitionList, dummyFilter);
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

        // This event handler is where the time-consuming work is done.
        private void backgroundWorkerMetadata_DoWorkMetadataActivation(object sender, DoWorkEventArgs e)
        {
            #region Generic
            // Set the stopwatch to be able to report back on process duration.
            Stopwatch totalProcess = new Stopwatch();
            Stopwatch subProcess = new Stopwatch();
            totalProcess.Start();

            BackgroundWorker worker = sender as BackgroundWorker;

            var inputTableMetadata = (DataTable)_bindingSourceTableMetadata.DataSource;
            var inputAttributeMetadata = (DataTable)_bindingSourceAttributeMetadata.DataSource;

            DataRow[] selectionRows;

            // Create an instance of the EventLog to information capture.
            EventLog eventLog = new EventLog();
            string eventMessage = "";

            var errorLog = new StringBuilder();
            var errorCounter = new int();

            var connOmd = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };
            var connStg= new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringStg };
            var connPsa = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringHstg};
            var connInt = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringInt };
            var connPres = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringPres };

            var metaDataConnection = ConfigurationSettings.ConnectionStringOmd;

            // Get everything as local variables to reduce multi-threading issues
            var integrationDatabase = '['+ ConfigurationSettings.IntegrationDatabaseName + ']';

            var linkedServer = ConfigurationSettings.PhysicalModelServerName;
            var metadataServer = ConfigurationSettings.MetadataServerName;
            if (linkedServer != "" && linkedServer != metadataServer)
            {
                linkedServer = '[' + linkedServer + "].";
            }
            else
                linkedServer = "";

            var effectiveDateTimeAttribute = ConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute=="True" ? ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute : ConfigurationSettings.LoadDateTimeAttribute;
            var currentRecordAttribute = ConfigurationSettings.CurrentRowAttribute;
            var eventDateTimeAtttribute = ConfigurationSettings.EventDateTimeAttribute;
            var recordSource = ConfigurationSettings.RecordSourceAttribute;
            var alternativeRecordSource = ConfigurationSettings.AlternativeRecordSourceAttribute;
            var sourceRowId = ConfigurationSettings.RowIdAttribute;
            var recordChecksum = ConfigurationSettings.RecordChecksumAttribute;
            var changeDataCaptureIndicator = ConfigurationSettings.ChangeDataCaptureAttribute;
            var hubAlternativeLdts = ConfigurationSettings.AlternativeLoadDateTimeAttribute;
            var etlProcessId = ConfigurationSettings.EtlProcessAttribute;
            var loadDateTimeStamp = ConfigurationSettings.LoadDateTimeAttribute;

            var stagingPrefix = ConfigurationSettings.StgTablePrefixValue;
            var psaPrefix = ConfigurationSettings.PsaTablePrefixValue;
            var hubTablePrefix = ConfigurationSettings.HubTablePrefixValue;
            var lnkTablePrefix = ConfigurationSettings.LinkTablePrefixValue;
            var satTablePrefix = ConfigurationSettings.SatTablePrefixValue;
            var lsatTablePrefix = ConfigurationSettings.LsatTablePrefixValue;

            if (ConfigurationSettings.TableNamingLocation=="Prefix")
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

            var dwhKeyIdentifier = ConfigurationSettings.DwhKeyIdentifier;

            if (ConfigurationSettings.KeyNamingLocation=="Prefix")
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
                // Determine the version
                var versionId = GetVersionFromTrackBar();

                var versionMajorMinor = GetVersion(versionId, connOmd);
                var majorVersion = versionMajorMinor.Key;
                var minorVersion = versionMajorMinor.Value;

                // Determine the query type (physical or virtual)
                var queryMode = radioButtonPhysicalMode.Checked ? "physical" : "virtual";

                // Event reporting - informing the user that the activation process has started
                eventMessage = "Commencing metadata preparation / activation for version " + majorVersion + "." + minorVersion + ".\r\n\r\n";
                eventLog.Add(Event.CreateNewEvent(EventTypes.Information, eventMessage));
                _alert.SetTextLogging(eventMessage);

                // Event reporting - alerting the user what kind of metadata is prepared
                eventMessage = queryMode == "physical" ? "Physical Mode has been selected as metadata source for activation. This means that the database will be used to query physical model (table and attribute) metadata. In other words, the physical model versioning is ignored.\r\n\r\n" : "Virtual Mode has been selected. This means that the versioned physical model in the data grid will be used as table and attribute metadata.\r\n\r\n";
                eventLog.Add(Event.CreateNewEvent(EventTypes.Information, eventMessage));
                _alert.SetTextLogging(eventMessage);
                #endregion

                #region Delete Metadata - 2%

                // 1. Deleting metadata
                _alert.SetTextLogging("Commencing removal of existing metadata.\r\n");

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
                        _alert.SetTextLogging("Removal of existing metadata completed.\r\n");
                    }
                    catch (Exception ex)
                    {
                        errorCounter++;
                        _alert.SetTextLogging(
                            "An issue has occured during removal of old metadata. Please check the Error Log for more details.\r\n");
                        errorLog.AppendLine("\r\nAn issue has occured during removal of old metadata: \r\n\r\n" + ex);
                        errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + deleteStatement);
                    }
                }

                # endregion


                # region Prepare Version Information - 3%

                // 2. Prepare Version
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the version metadata.\r\n");

                var versionName = string.Concat(majorVersion, '.', minorVersion);

                using (var connection = new SqlConnection(metaDataConnection))
                {
                    _alert.SetTextLogging("-->  Working on committing version " + versionName +
                                          " to the metadata repository.\r\n");

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
                        errorCounter++;
                        _alert.SetTextLogging(
                            "An issue has occured during preparation of the version information. Please check the Error Log for more details.\r\n");
                        errorLog.AppendLine(
                            "\r\nAn issue has occured during preparation of the version information: \r\n\r\n" + ex);
                    }
                }

                if (worker != null) worker.ReportProgress(3);
                _alert.SetTextLogging("Preparation of the version details completed.\r\n");


                #endregion


                # region Prepare Source - 5%

                // Prepare the generic sources
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the source metadata.\r\n");

                // Getting the distinct list of tables to go into the 'source'
                selectionRows = inputTableMetadata.Select("PROCESS_INDICATOR = 'Y'");

                var distinctListSource = new List<string>
                {
                    // Create a dummy row
                    "Not applicable"
                };

                // Create a distinct list of sources from the datagrid
                foreach (DataRow row in selectionRows)
                {
                    string source_table = row["SOURCE_TABLE"].ToString().Trim();
                    if (!distinctListSource.Contains(source_table))
                    {
                        distinctListSource.Add(source_table);
                    }
                }

                // Add the list of sources to the MD_SOURCE table
                foreach (var tableName in distinctListSource)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        if (tableName != "Not applicable")
                        {
                            _alert.SetTextLogging("--> " + tableName + "\r\n");
                        }

                        var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_SOURCE]");
                        insertStatement.AppendLine("([SOURCE_NAME], [SOURCE_NAME_SHORT], [SCHEMA_NAME])");
                        insertStatement.AppendLine("VALUES ('" + tableName + "','" + fullyQualifiedName.Value + "','" + fullyQualifiedName.Key + "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            errorCounter++;
                            _alert.SetTextLogging(
                                "An issue has occured during preparation of the source metadata. Please check the Error Log for more details.\r\n");
                            errorLog.AppendLine(
                                "\r\nAn issue has occured during preparation of the source metadata: \r\n\r\n" + ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                        }
                    }
                }

                worker?.ReportProgress(5);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the source metadata completed, and has taken " + subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Prepare Staging Area - 7%

                //Prepare the Staging Area
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Staging Area metadata.\r\n");

                // Getting the distinct list of tables to go into the MD_STAGING table
                if (ConfigurationSettings.TableNamingLocation == "Prefix")
                {
                    selectionRows = inputTableMetadata.Select("PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '" +
                                                              ConfigurationSettings.StgTablePrefixValue + "%'");
                }
                else
                {
                    selectionRows = inputTableMetadata.Select("PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '%" +
                                                              ConfigurationSettings.StgTablePrefixValue + "'");
                }

                var distinctListStg = new List<string>
                {
                    // Create a dummy row
                    "Not applicable"
                };

                // Create a distinct list of sources from the datagrid
                foreach (DataRow row in selectionRows)
                {
                    string target_table = row["TARGET_TABLE"].ToString().Trim();
                    if (!distinctListStg.Contains(target_table))
                    {
                        distinctListStg.Add(target_table);
                    }
                }

                // Process the unique Staging Area records
                foreach (var tableName in distinctListStg)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        if (tableName != "Not applicable")
                        {
                            _alert.SetTextLogging("--> " + tableName + "\r\n");
                        }

                        var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_STAGING]");
                        insertStatement.AppendLine("([STAGING_NAME], [SCHEMA_NAME])");
                        insertStatement.AppendLine("VALUES ('" + fullyQualifiedName.Value + "','" + fullyQualifiedName.Key + "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            errorCounter++;
                            _alert.SetTextLogging(
                                "An issue has occured during preparation of the Staging Area. Please check the Error Log for more details.\r\n");

                            errorLog.AppendLine(
                                "\r\nAn issue has occured during preparation of the Staging Area: \r\n\r\n" + ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                        }
                    }
                }

                worker?.ReportProgress(7);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the Staging Area metadata completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Prepare Source to Staging Area XREF - 10%

                // Prepare the Source to Staging Area XREF
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the relationship between Source and Staging Area.\r\n");

                // Getting the mapping list from the data table
                if (ConfigurationSettings.TableNamingLocation == "Prefix")
                {
                    selectionRows = inputTableMetadata.Select("PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '" +
                                                              ConfigurationSettings.StgTablePrefixValue + "%'");
                }
                else
                {
                    selectionRows = inputTableMetadata.Select("PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '%" +
                                                              ConfigurationSettings.StgTablePrefixValue + "'");
                }

                // Process the unique Staging Area records
                foreach (var row in selectionRows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        var sourceFullyQualifiedName = ClassMetadataHandling.GetSchema(row["SOURCE_TABLE"].ToString())
                            .FirstOrDefault();
                        var targetFullyQualifiedName = ClassMetadataHandling.GetSchema(row["TARGET_TABLE"].ToString())
                            .FirstOrDefault();

                        _alert.SetTextLogging("--> Processing the " + sourceFullyQualifiedName.Value + " to " +
                                              targetFullyQualifiedName.Value + " relationship.\r\n");

                        var filterCriterion = row["FILTER_CRITERIA"].ToString().Trim();
                        filterCriterion = filterCriterion.Replace("'", "''");

                        var businessKeyDefinition = row["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                        businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_SOURCE_STAGING_XREF]");
                        insertStatement.AppendLine(
                            "([SOURCE_NAME], [STAGING_NAME], [CHANGE_DATETIME_DEFINITION], [CHANGE_DATA_CAPTURE_DEFINITION], [KEY_DEFINITION], [FILTER_CRITERIA])");
                        insertStatement.AppendLine("VALUES (" +
                                                   "'" + sourceFullyQualifiedName.Value + "', " +
                                                   "'" + targetFullyQualifiedName.Value + "', " +
                                                   "NULL, " +
                                                   "NULL, " +
                                                   "'" + businessKeyDefinition + "', " +
                                                   "'" + filterCriterion + "'" +
                                                   ")");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            errorCounter++;
                            _alert.SetTextLogging(
                                "An issue has occured during preparation of the relationship between the Source and the Staging Area. Please check the Error Log for more details.\r\n");

                            errorLog.AppendLine(
                                "\r\nAn issue has occured during preparation of the Source / Staging Area XREF: \r\n\r\n" +
                                ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                        }
                    }
                }

                worker?.ReportProgress(10);
                subProcess.Stop();
                _alert.SetTextLogging(
                    "Preparation of the Source / Staging Area XREF metadata completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Prepare Persistent Staging Area - 13%

                //3. Prepare Persistent Staging Area
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Persistent Staging Area metadata.\r\n");

                // Getting the distinct list of tables to go into the MD_PERSISTENT_STAGING table
                if (ConfigurationSettings.TableNamingLocation == "Prefix")
                {
                    selectionRows = inputTableMetadata.Select("PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '" +
                                                              ConfigurationSettings.PsaTablePrefixValue + "%'");
                }
                else
                {
                    selectionRows = inputTableMetadata.Select("PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '%" +
                                                              ConfigurationSettings.PsaTablePrefixValue + "'");
                }

                var distinctListPsa = new List<string>
                {
                    // Create a dummy row
                    "Not applicable"
                };

                // Create a distinct list of sources from the data grid
                foreach (DataRow row in selectionRows)
                {
                    var target_table = row["TARGET_TABLE"].ToString().Trim();
                    if (!distinctListPsa.Contains(target_table))
                    {
                        distinctListPsa.Add(target_table);
                    }
                }

                // Process the unique Persistent Staging Area records
                foreach (var tableName in distinctListPsa)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        if (tableName != "Not applicable")
                        {
                            _alert.SetTextLogging("--> " + tableName + "\r\n");
                        }

                        var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_PERSISTENT_STAGING]");
                        insertStatement.AppendLine("([PERSISTENT_STAGING_NAME], [PERSISTENT_STAGING_NAME_SHORT], [SCHEMA_NAME])");
                        insertStatement.AppendLine("VALUES ('" + tableName + "','" + fullyQualifiedName.Value + "','" + fullyQualifiedName.Key + "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            errorCounter++;
                            _alert.SetTextLogging("An issue has occured during preparation of the Persistent Staging Area. Please check the Error Log for more details.\r\n");

                            errorLog.AppendLine("\r\nAn issue has occured during preparation of the Persistent Staging Area: \r\n\r\n" + ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                        }
                    }
                }

                if (worker != null) worker.ReportProgress(13);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the Persistent Staging Area metadata completed, and has taken " + subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Prepare Source to Persistent Staging Area XREF - 15%

                // Prepare the Source to Persistent Staging Area XREF
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the relationship between Source and Persistent Staging Area.\r\n");

                // Getting the mapping list from the data table
                if (ConfigurationSettings.TableNamingLocation == "Prefix")
                {
                    selectionRows = inputTableMetadata.Select("PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '" + ConfigurationSettings.PsaTablePrefixValue + "%'");
                }
                else
                {
                    selectionRows = inputTableMetadata.Select("PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '%" + ConfigurationSettings.PsaTablePrefixValue + "'");
                }

                // Process the unique Staging Area records
                foreach (var row in selectionRows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        _alert.SetTextLogging("--> Processing the " + row["SOURCE_TABLE"] + " to " + row["TARGET_TABLE"] + " relationship.\r\n");

                        var filterCriterion = row["FILTER_CRITERIA"].ToString().Trim();
                        filterCriterion = filterCriterion.Replace("'", "''");

                        var businessKeyDefinition = row["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                        businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_SOURCE_PERSISTENT_STAGING_XREF]");
                        insertStatement.AppendLine("([SOURCE_NAME], [PERSISTENT_STAGING_NAME], [CHANGE_DATETIME_DEFINITION], [KEY_DEFINITION], [FILTER_CRITERIA])");
                        insertStatement.AppendLine("VALUES ('" + row["SOURCE_TABLE"] + "','" +
                                                   row["TARGET_TABLE"] + 
                                                   "', NULL, '" +
                                                   businessKeyDefinition + "', '" + 
                                                   filterCriterion + "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            errorCounter++;
                            _alert.SetTextLogging(
                                "An issue has occured during preparation of the relationship between the Source and the Persistent Staging Area. Please check the Error Log for more details.\r\n");

                            errorLog.AppendLine(
                                "\r\nAn issue has occured during preparation of the Source / Persistent Staging Area XREF: \r\n\r\n" +
                                ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                        }
                    }
                }

                worker?.ReportProgress(15);
                subProcess.Stop();
                _alert.SetTextLogging(
                    "Preparation of the Source / Persistent Staging Area XREF metadata completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Prepare Hubs - 17%

                //3. Prepare Hubs
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Hub metadata.\r\n");

                // Getting the distinct list of tables to go into the MD_HUB table
                selectionRows =
                    inputTableMetadata.Select(
                        "PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '%" + hubTablePrefix + "%'");

                var distinctListHub = new List<string>();

                // Create a dummy row
                distinctListHub.Add("Not applicable");

                // Create a distinct list of sources from the datagrid
                foreach (DataRow row in selectionRows)
                {
                    string target_table = row["TARGET_TABLE"].ToString().Trim();
                    if (!distinctListHub.Contains(target_table))
                    {
                        distinctListHub.Add(target_table);
                    }
                }

                // Process the unique Hub records
                foreach (var tableName in distinctListHub)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        if (tableName != "Not applicable")
                        {
                            _alert.SetTextLogging("--> " + tableName + "\r\n");
                        }

                        var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                        // Retrieve the business key
                        var hubBusinessKey = ClassMetadataHandling.GetHubTargetBusinessKeyList(fullyQualifiedName.Key,
                            fullyQualifiedName.Value, versionId, queryMode);
                        string businessKeyString = string.Join(",", hubBusinessKey);
                        string surrogateKey = ClassMetadataHandling.GetSurrogateKey(tableName);

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_HUB]");
                        insertStatement.AppendLine("([HUB_NAME], [SCHEMA_NAME], [BUSINESS_KEY], [SURROGATE_KEY])");
                        insertStatement.AppendLine("VALUES ('" + fullyQualifiedName.Value + "','" +
                                                   fullyQualifiedName.Key + "', '" + businessKeyString + "', '" +
                                                   surrogateKey + "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            errorCounter++;
                            _alert.SetTextLogging(
                                "An issue has occured during preparation of the Hubs. Please check the Error Log for more details.\r\n");

                            errorLog.AppendLine(
                                "\r\nAn issue has occured during preparation of the Hubs: \r\n\r\n" + ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                        }
                    }
                }

                if (worker != null) worker.ReportProgress(17);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the Hub metadata completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Prepare Links - 20%

                //4. Prepare links
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Link metadata.\r\n");

                // Getting the distinct list of tables to go into the MD_LINK table
                selectionRows =
                    inputTableMetadata.Select(
                        "PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '%" + lnkTablePrefix + "%'");

                var distinctListLinks = new List<string>();

                // Create a dummy row
                distinctListLinks.Add("Not applicable");

                // Create a distinct list of sources from the data grid
                foreach (DataRow row in selectionRows)
                {
                    string target_table = row["TARGET_TABLE"].ToString().Trim();
                    if (!distinctListLinks.Contains(target_table))
                    {
                        distinctListLinks.Add(target_table);
                    }
                }

                // Insert the rest of the rows
                foreach (var tableName in distinctListLinks)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        if (tableName != "Not applicable")
                        {
                            _alert.SetTextLogging("--> " + tableName + "\r\n");
                        }

                        var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                        // Retrieve the surrogate key
                        string surrogateKey = ClassMetadataHandling.GetSurrogateKey(tableName);

                        var insertStatement = new StringBuilder();

                        insertStatement.AppendLine("INSERT INTO [MD_LINK]");
                        insertStatement.AppendLine("([LINK_NAME], [SCHEMA_NAME], [SURROGATE_KEY])");
                        insertStatement.AppendLine("VALUES ('" + fullyQualifiedName.Value + "','" +
                                                   fullyQualifiedName.Key + "','" + surrogateKey + "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            errorCounter++;
                            _alert.SetTextLogging(
                                "An issue has occured during preparation of the Links. Please check the Error Log for more details.\r\n");

                            errorLog.AppendLine("\r\nAn issue has occured during preparation of the Links: \r\n\r\n" +
                                                ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                        }
                    }
                }

                if (worker != null) worker.ReportProgress(20);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the Link metadata completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Prepare Satellites - 24%

                // Prepare Satellites
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Satellite metadata.\r\n");

                var prepareSatStatement = new StringBuilder();

                prepareSatStatement.AppendLine("SELECT DISTINCT");
                prepareSatStatement.AppendLine("  spec.TARGET_TABLE AS SATELLITE_NAME,");
                prepareSatStatement.AppendLine("  hubkeysub.HUB_NAME, ");
                prepareSatStatement.AppendLine("  'Normal' AS SATELLITE_TYPE, ");
                prepareSatStatement.AppendLine(
                    "  (SELECT LINK_NAME FROM MD_LINK WHERE LINK_NAME='Not applicable') AS LINK_NAME -- No link for normal Satellites ");
                prepareSatStatement.AppendLine("FROM TMP_MD_TABLE_MAPPING spec ");
                prepareSatStatement.AppendLine("LEFT OUTER JOIN ");
                prepareSatStatement.AppendLine("(");
                prepareSatStatement.AppendLine(
                    "  SELECT DISTINCT TARGET_TABLE, hub.HUB_NAME, SOURCE_TABLE, BUSINESS_KEY_ATTRIBUTE ");
                prepareSatStatement.AppendLine("  FROM TMP_MD_TABLE_MAPPING spec2 ");
                prepareSatStatement.AppendLine("  LEFT OUTER JOIN -- Join in the Hub NAME from the MD table ");
                prepareSatStatement.AppendLine(
                    "  MD_HUB hub ON hub.[SCHEMA_NAME]+'.'+hub.HUB_NAME=spec2.TARGET_TABLE ");
                prepareSatStatement.AppendLine("  WHERE TARGET_TABLE_TYPE = '" +
                                               ClassMetadataHandling.TableTypes.CoreBusinessConcept +
                                               "' AND [PROCESS_INDICATOR] = 'Y'                                                        ");
                prepareSatStatement.AppendLine(") hubkeysub ");
                prepareSatStatement.AppendLine("        ON spec.SOURCE_TABLE=hubkeysub.SOURCE_TABLE ");
                prepareSatStatement.AppendLine(
                    "        AND replace(spec.BUSINESS_KEY_ATTRIBUTE,' ','')=replace(hubkeysub.BUSINESS_KEY_ATTRIBUTE,' ','') ");
                prepareSatStatement.AppendLine("WHERE spec.TARGET_TABLE_TYPE = '" +
                                               ClassMetadataHandling.TableTypes.Context + "' ");
                prepareSatStatement.AppendLine("AND [PROCESS_INDICATOR] = 'Y'");

                var listSat = Utility.GetDataTable(ref connOmd, prepareSatStatement.ToString());

                foreach (DataRow satelliteName in listSat.Rows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        var tableName = satelliteName["SATELLITE_NAME"].ToString().Trim();
                        var tableType = satelliteName["SATELLITE_TYPE"].ToString().Trim();
                        var hubName = satelliteName["HUB_NAME"];
                        var linkName = satelliteName["LINK_NAME"];

                        var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                        if (tableName != "Not applicable")
                        {
                            _alert.SetTextLogging("--> " + fullyQualifiedName.Value + "\r\n");
                        }

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_SATELLITE]");
                        insertStatement.AppendLine(
                            "([SATELLITE_NAME], [SATELLITE_TYPE], [SCHEMA_NAME], [HUB_NAME], [LINK_NAME])");
                        insertStatement.AppendLine("VALUES ('" + fullyQualifiedName.Value + "','" + tableType + "', '" +
                                                   fullyQualifiedName.Key + "', '" + hubName + "','" + linkName + "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            errorCounter++;
                            _alert.SetTextLogging(
                                "An issue has occured during preparation of the Satellites. Please check the Error Log for more details.\r\n");

                            errorLog.AppendLine(
                                "\r\nAn issue has occured during preparation of the Satellites: \r\n\r\n" + ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                        }
                    }
                }

                worker.ReportProgress(24);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the Satellite metadata completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Prepare Link Satellites - 28%

                //Prepare Link Satellites
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Link Satellite metadata.\r\n");

                var prepareLsatStatement = new StringBuilder();
                prepareLsatStatement.AppendLine("SELECT DISTINCT");
                prepareLsatStatement.AppendLine("        spec.TARGET_TABLE AS SATELLITE_NAME, ");
                prepareLsatStatement.AppendLine(
                    "        (SELECT HUB_NAME FROM MD_HUB WHERE HUB_NAME='Not applicable') AS HUB_NAME, -- No Hub for Link Satellites");
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
                    "                MD_LINK lnk ON lnk.[SCHEMA_NAME]+'.'+lnk.LINK_NAME=spec2.TARGET_TABLE");
                prepareLsatStatement.AppendLine("        WHERE TARGET_TABLE_TYPE = '" +
                                                ClassMetadataHandling.TableTypes.NaturalBusinessRelationship + "' ");
                prepareLsatStatement.AppendLine("        AND [PROCESS_INDICATOR] = 'Y'");
                prepareLsatStatement.AppendLine(") lnkkeysub");
                prepareLsatStatement.AppendLine(
                    "    ON spec.SOURCE_TABLE=lnkkeysub.SOURCE_TABLE -- Only the combination of Link table and Business key can belong to the LSAT");
                prepareLsatStatement.AppendLine(
                    "    AND REPLACE(spec.BUSINESS_KEY_ATTRIBUTE,' ','')=REPLACE(lnkkeysub.BUSINESS_KEY_ATTRIBUTE,' ','')");
                prepareLsatStatement.AppendLine(
                    "-- Only select Link Satellites as the base / driving table (spec alias)");
                prepareLsatStatement.AppendLine("WHERE spec.TARGET_TABLE_TYPE = '" +
                                                ClassMetadataHandling.TableTypes.NaturalBusinessRelationshipContext +
                                                "'");
                prepareLsatStatement.AppendLine("AND [PROCESS_INDICATOR] = 'Y'");


                var listLsat = Utility.GetDataTable(ref connOmd, prepareLsatStatement.ToString());

                foreach (DataRow satelliteName in listLsat.Rows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        var tableName = satelliteName["SATELLITE_NAME"].ToString().Trim();
                        var tableType = satelliteName["SATELLITE_TYPE"].ToString().Trim();
                        var hubName = satelliteName["HUB_NAME"];
                        var linkName = satelliteName["LINK_NAME"];

                        var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                        _alert.SetTextLogging("--> " + fullyQualifiedName.Value + "\r\n");

                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_SATELLITE]");
                        insertStatement.AppendLine(
                            "([SATELLITE_NAME], [SATELLITE_TYPE], [SCHEMA_NAME], [HUB_NAME], [LINK_NAME])");
                        insertStatement.AppendLine("VALUES ('" + fullyQualifiedName.Value + "','" + tableType + "', '" +
                                                   fullyQualifiedName.Key + "', '" + hubName + "','" + linkName + "')");

                        var command = new SqlCommand(insertStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            errorCounter++;
                            _alert.SetTextLogging(
                                "An issue has occured during preparation of the Link Satellites. Please check the Error Log for more details.\r\n");

                            errorLog.AppendLine(
                                "\r\nAn issue has occured during preparation of the Link Satellites: \r\n\r\n" + ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);

                        }
                    }
                }

                worker.ReportProgress(28);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the Link Satellite metadata completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Prepare Source / SAT Xref - 28%

                //Prepare Source / Sat XREF
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging(
                    "Commencing preparing the relationship between (Link) Satellites and the Source tables.\r\n");

                var prepareSatXrefStatement = new StringBuilder();
                prepareSatXrefStatement.AppendLine("SELECT");
                prepareSatXrefStatement.AppendLine("        sat.SATELLITE_NAME,");
                prepareSatXrefStatement.AppendLine("        stg.SOURCE_NAME,");
                prepareSatXrefStatement.AppendLine("        spec.BUSINESS_KEY_ATTRIBUTE,");
                prepareSatXrefStatement.AppendLine("        spec.FILTER_CRITERIA");
                prepareSatXrefStatement.AppendLine("FROM TMP_MD_TABLE_MAPPING spec");
                prepareSatXrefStatement.AppendLine("LEFT OUTER JOIN -- Join in the Source_ID from the MD_SOURCE table");
                prepareSatXrefStatement.AppendLine(
                    "        MD_SOURCE stg ON stg.[SCHEMA_NAME]+'.'+stg.SOURCE_NAME=spec.SOURCE_TABLE");
                prepareSatXrefStatement.AppendLine(
                    "LEFT OUTER JOIN -- Join in the Satellite_ID from the MD_SATELLITE table");
                prepareSatXrefStatement.AppendLine(
                    "        MD_SATELLITE sat ON sat.[SCHEMA_NAME]+'.'+sat.SATELLITE_NAME=spec.TARGET_TABLE");
                prepareSatXrefStatement.AppendLine("WHERE spec.TARGET_TABLE_TYPE = '" +
                                                   ClassMetadataHandling.TableTypes.Context + "'");
                prepareSatXrefStatement.AppendLine("AND [PROCESS_INDICATOR] = 'Y'");
                prepareSatXrefStatement.AppendLine("UNION");
                prepareSatXrefStatement.AppendLine("SELECT");
                prepareSatXrefStatement.AppendLine("        sat.SATELLITE_NAME,");
                prepareSatXrefStatement.AppendLine("        stg.SOURCE_NAME,");
                prepareSatXrefStatement.AppendLine("        spec.BUSINESS_KEY_ATTRIBUTE,");
                prepareSatXrefStatement.AppendLine("        spec.FILTER_CRITERIA");
                prepareSatXrefStatement.AppendLine("FROM TMP_MD_TABLE_MAPPING spec");
                prepareSatXrefStatement.AppendLine("LEFT OUTER JOIN -- Join in the Source from the MD_SOURCE table");
                prepareSatXrefStatement.AppendLine(
                    "        MD_SOURCE stg ON stg.[SCHEMA_NAME]+'.'+stg.SOURCE_NAME=spec.SOURCE_TABLE");
                prepareSatXrefStatement.AppendLine(
                    "LEFT OUTER JOIN -- Join in the Satellite_ID from the MD_SATELLITE table");
                prepareSatXrefStatement.AppendLine(
                    "        MD_SATELLITE sat ON sat.[SCHEMA_NAME]+'.'+sat.SATELLITE_NAME=spec.TARGET_TABLE");
                prepareSatXrefStatement.AppendLine("WHERE spec.TARGET_TABLE_TYPE = '" +
                                                   ClassMetadataHandling.TableTypes.NaturalBusinessRelationshipContext +
                                                   "'");
                prepareSatXrefStatement.AppendLine("AND [PROCESS_INDICATOR] = 'Y'");

                var listSatXref = Utility.GetDataTable(ref connOmd, prepareSatXrefStatement.ToString());

                foreach (DataRow tableName in listSatXref.Rows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        _alert.SetTextLogging("--> Processing the " + tableName["SOURCE_NAME"] + " to " +
                                              tableName["SATELLITE_NAME"] + " relationship.\r\n");

                        var insertStatement = new StringBuilder();
                        var filterCriterion = tableName["FILTER_CRITERIA"].ToString().Trim();
                        filterCriterion = filterCriterion.Replace("'", "''");

                        var businessKeyDefinition = tableName["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                        businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                        var loadVector = ClassMetadataHandling.GetLoadVector(tableName["SOURCE_NAME"].ToString(),
                            tableName["SATELLITE_NAME"].ToString());

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
                            errorCounter++;
                            _alert.SetTextLogging(
                                "An issue has occured during preparation of the relationship between the Source and the Satellite. Please check the Error Log for more details.\r\n");

                            errorLog.AppendLine(
                                "\r\nAn issue has occured during preparation of the Source / Satellite XREF: \r\n\r\n" +
                                ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                        }
                    }
                }

                worker.ReportProgress(28);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the Source / Satellite XREF metadata completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Source / Hub relationship - 30%

                //Prepare Source / HUB xref
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the relationship between Source and Hubs.\r\n");

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
                prepareStgHubXrefStatement.AppendLine("    TARGET_TABLE,");
                prepareStgHubXrefStatement.AppendLine("    BUSINESS_KEY_ATTRIBUTE,");
                prepareStgHubXrefStatement.AppendLine("    FILTER_CRITERIA");
                prepareStgHubXrefStatement.AppendLine("    FROM TMP_MD_TABLE_MAPPING");
                prepareStgHubXrefStatement.AppendLine("    WHERE ");
                prepareStgHubXrefStatement.AppendLine("        TARGET_TABLE_TYPE = '" +
                                                      ClassMetadataHandling.TableTypes.CoreBusinessConcept + "'");
                prepareStgHubXrefStatement.AppendLine("    AND [PROCESS_INDICATOR] = 'Y'");
                prepareStgHubXrefStatement.AppendLine(") hub");
                prepareStgHubXrefStatement.AppendLine("LEFT OUTER JOIN");
                prepareStgHubXrefStatement.AppendLine("( ");
                prepareStgHubXrefStatement.AppendLine("    SELECT SOURCE_NAME, [SCHEMA_NAME]");
                prepareStgHubXrefStatement.AppendLine("    FROM MD_SOURCE");
                prepareStgHubXrefStatement.AppendLine(") stgsub");
                prepareStgHubXrefStatement.AppendLine(
                    "ON hub.SOURCE_TABLE=stgsub.[SCHEMA_NAME]+'.'+stgsub.SOURCE_NAME");
                prepareStgHubXrefStatement.AppendLine("LEFT OUTER JOIN");
                prepareStgHubXrefStatement.AppendLine("( ");
                prepareStgHubXrefStatement.AppendLine("    SELECT HUB_NAME, [SCHEMA_NAME]");
                prepareStgHubXrefStatement.AppendLine("    FROM MD_HUB");
                prepareStgHubXrefStatement.AppendLine(") hubsub");
                prepareStgHubXrefStatement.AppendLine("ON hub.TARGET_TABLE=hubsub.[SCHEMA_NAME]+'.'+hubsub.HUB_NAME");

                var listXref = Utility.GetDataTable(ref connOmd, prepareStgHubXrefStatement.ToString());

                foreach (DataRow tableName in listXref.Rows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        _alert.SetTextLogging("--> Processing the " + tableName["SOURCE_NAME"] + " to " +
                                              tableName["HUB_NAME"] + " relationship.\r\n");

                        var filterCriterion = tableName["FILTER_CRITERIA"].ToString().Trim();
                        filterCriterion = filterCriterion.Replace("'", "''");

                        var businessKeyDefinition = tableName["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                        businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                        var loadVector = ClassMetadataHandling.GetLoadVector(tableName["SOURCE_NAME"].ToString(),
                            tableName["HUB_NAME"].ToString());

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
                            errorCounter++;
                            _alert.SetTextLogging(
                                "An issue has occured during preparation of the relationship between the Source and the Hubs. Please check the Error Log for more details.\r\n");

                            errorLog.AppendLine(
                                "\r\nAn issue has occured during preparation of the Staging / Hub XREF: \r\n\r\n" + ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                        }
                    }
                }

                worker.ReportProgress(30);
                subProcess.Stop();
                _alert.SetTextLogging(
                    "Preparation of the relationship between Source and Hubs completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Filter Variables  - 35%

                subProcess.Reset();
                subProcess.Start();

                string tableFilterQuery =
                    @"SELECT DISTINCT [SOURCE_TABLE] AS [TABLE_NAME], [SOURCE_TABLE_TYPE] AS [TABLE_TYPE] FROM [TMP_MD_TABLE_MAPPING]
                                            UNION
                                            SELECT DISTINCT [TARGET_TABLE] AS [TABLE_NAME], [TARGET_TABLE_TYPE] AS [TABLE_TYPE] FROM [TMP_MD_TABLE_MAPPING]";

                // Filters need to be executed against specific databases, so each filter is setup to be used against a specific database connection
                var stgTableFilterObjects = "";
                var psaTableFilterObjects = "";
                var intTableFilterObjects = "";
                var presTableFilterObjects = "";

                var tableDataTable = Utility.GetDataTable(ref connOmd, tableFilterQuery);

                // Creating the filters
                int objectCounter = 1;
                foreach (DataRow tableRow in tableDataTable.Rows)
                {
                    // Get the right database name for the table type (which can be anything including STG, PSA, base- and derived DV and Dimension or Facts)
                    Dictionary<string, string> databaseNameDictionary = ClassMetadataHandling.GetConnectionInformationForTableType(tableRow["TABLE_TYPE"].ToString());
                    
                    string databaseNameKey = databaseNameDictionary.FirstOrDefault().Key;
                   

                    // Regular processing
                  //  if (databaseNameKey == ConfigurationSettings.StagingDatabaseName && (tableRow["TABLE_NAME"].ToString().StartsWith(ConfigurationSettings.StgTablePrefixValue) ||
                   //    tableRow["TABLE_NAME"].ToString().EndsWith(ConfigurationSettings.StgTablePrefixValue)))

                    if (databaseNameKey == ConfigurationSettings.StagingDatabaseName)
                    {
                        // Staging filter
                        stgTableFilterObjects = stgTableFilterObjects + "OBJECT_ID(N'[" + databaseNameKey + "]." + tableRow["TABLE_NAME"] + "') ,";
                    }
                    //else if (databaseNameKey == ConfigurationSettings.PsaDatabaseName && (tableRow["TABLE_NAME"].ToString().StartsWith(ConfigurationSettings.PsaTablePrefixValue) ||
                    //                                                                      tableRow["TABLE_NAME"].ToString().EndsWith(ConfigurationSettings.PsaTablePrefixValue)))
                    else if (databaseNameKey == ConfigurationSettings.PsaDatabaseName)
                    {
                        // Persistent Staging Area filter
                        psaTableFilterObjects = psaTableFilterObjects + "OBJECT_ID(N'[" + databaseNameKey + "]." + tableRow["TABLE_NAME"] + "') ,";
                    }
                    else if (databaseNameKey == ConfigurationSettings.IntegrationDatabaseName)
                    {
                        // Integration Layer filter
                        intTableFilterObjects = intTableFilterObjects + "OBJECT_ID(N'[" + databaseNameKey + "]." + tableRow["TABLE_NAME"] + "') ,";
                    }
                    else if (databaseNameKey == ConfigurationSettings.PresentationDatabaseName)
                    {
                        // Presentation Layer filter
                        presTableFilterObjects = presTableFilterObjects + "OBJECT_ID(N'[" + databaseNameKey + "]." + tableRow["TABLE_NAME"] + "') ,";
                    }

                    objectCounter++;
                }

                // Remove trailing commas
                stgTableFilterObjects = stgTableFilterObjects.TrimEnd(',');
                psaTableFilterObjects = psaTableFilterObjects.TrimEnd(',');
                intTableFilterObjects = intTableFilterObjects.TrimEnd(',');
                presTableFilterObjects = presTableFilterObjects.TrimEnd(',');

                // Making sure a NULL value is returned if the filter doesn't contain any results
                if (stgTableFilterObjects == "")
                {
                    stgTableFilterObjects = "NULL";
                }

                if (psaTableFilterObjects == "")
                {
                    psaTableFilterObjects = "NULL";
                }

                if (intTableFilterObjects == "")
                {
                    intTableFilterObjects = "NULL";
                }

                if (presTableFilterObjects == "")
                {
                    presTableFilterObjects = "NULL";
                }

                worker.ReportProgress(35);
                subProcess.Stop();
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Filter variables created successfully, this took " +
                                      subProcess.Elapsed.TotalSeconds.ToString() + " seconds.\r\n");

                #endregion

                #region Physical Model dump- 40%

                // Creating a point-in-time snapshot of the physical model used for export to the interface schemas
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Creating a snapshot of the physical model.\r\n");

                // First, define the master attribute list for reuse many times later on (assuming ignore version is active and hence the virtual mode is enabled).
                var physicalModelDataTable = new DataTable();

                if (radioButtonPhysicalMode.Checked) // Get the attributes from the physical model / catalog. No virtualisation needed.
                {
                    var physicalModelInstantiation = new AttributeSelection();

                    // Staging / landing
                    var preparePhysicalModelStgStatement = new StringBuilder();
                    preparePhysicalModelStgStatement.AppendLine("SELECT ");
                    preparePhysicalModelStgStatement.AppendLine(" [DATABASE_NAME] ");
                    preparePhysicalModelStgStatement.AppendLine(",[SCHEMA_NAME]");
                    preparePhysicalModelStgStatement.AppendLine(",[TABLE_NAME]");
                    preparePhysicalModelStgStatement.AppendLine(",[COLUMN_NAME]");
                    preparePhysicalModelStgStatement.AppendLine(",[DATA_TYPE]");
                    preparePhysicalModelStgStatement.AppendLine(",[CHARACTER_MAXIMUM_LENGTH]");
                    preparePhysicalModelStgStatement.AppendLine(",[NUMERIC_PRECISION]");
                    preparePhysicalModelStgStatement.AppendLine(",[ORDINAL_POSITION]");
                    preparePhysicalModelStgStatement.AppendLine(",[PRIMARY_KEY_INDICATOR]");
                    preparePhysicalModelStgStatement.AppendLine("FROM");
                    preparePhysicalModelStgStatement.AppendLine("(");
                    preparePhysicalModelStgStatement.AppendLine(physicalModelInstantiation.CreatePhysicalModelSet(ConfigurationSettings.StagingDatabaseName, stgTableFilterObjects).ToString());
                    preparePhysicalModelStgStatement.AppendLine(") sub");

                    var physicalModelDataTableStg = Utility.GetDataTable(ref connStg, preparePhysicalModelStgStatement.ToString());

                    // PSA
                    var preparePhysicalModelPsaStatement = new StringBuilder();
                    preparePhysicalModelPsaStatement.AppendLine("SELECT ");
                    preparePhysicalModelPsaStatement.AppendLine(" [DATABASE_NAME] ");
                    preparePhysicalModelPsaStatement.AppendLine(",[SCHEMA_NAME]");
                    preparePhysicalModelPsaStatement.AppendLine(",[TABLE_NAME]");
                    preparePhysicalModelPsaStatement.AppendLine(",[COLUMN_NAME]");
                    preparePhysicalModelPsaStatement.AppendLine(",[DATA_TYPE]");
                    preparePhysicalModelPsaStatement.AppendLine(",[CHARACTER_MAXIMUM_LENGTH]");
                    preparePhysicalModelPsaStatement.AppendLine(",[NUMERIC_PRECISION]");
                    preparePhysicalModelPsaStatement.AppendLine(",[ORDINAL_POSITION]");
                    preparePhysicalModelPsaStatement.AppendLine(",[PRIMARY_KEY_INDICATOR]");
                    preparePhysicalModelPsaStatement.AppendLine("FROM");
                    preparePhysicalModelPsaStatement.AppendLine("(");
                    preparePhysicalModelPsaStatement.AppendLine(physicalModelInstantiation.CreatePhysicalModelSet(ConfigurationSettings.PsaDatabaseName, psaTableFilterObjects).ToString());
                    preparePhysicalModelPsaStatement.AppendLine(") sub");

                    var physicalModelDataTablePsa = Utility.GetDataTable(ref connPsa, preparePhysicalModelPsaStatement.ToString());

                    // INT
                    var preparePhysicalModelIntStatement = new StringBuilder();
                    preparePhysicalModelIntStatement.AppendLine("SELECT ");
                    preparePhysicalModelIntStatement.AppendLine(" [DATABASE_NAME] ");
                    preparePhysicalModelIntStatement.AppendLine(",[SCHEMA_NAME]");
                    preparePhysicalModelIntStatement.AppendLine(",[TABLE_NAME]");
                    preparePhysicalModelIntStatement.AppendLine(",[COLUMN_NAME]");
                    preparePhysicalModelIntStatement.AppendLine(",[DATA_TYPE]");
                    preparePhysicalModelIntStatement.AppendLine(",[CHARACTER_MAXIMUM_LENGTH]");
                    preparePhysicalModelIntStatement.AppendLine(",[NUMERIC_PRECISION]");
                    preparePhysicalModelIntStatement.AppendLine(",[ORDINAL_POSITION]");
                    preparePhysicalModelIntStatement.AppendLine(",[PRIMARY_KEY_INDICATOR]");
                    preparePhysicalModelIntStatement.AppendLine("FROM");
                    preparePhysicalModelIntStatement.AppendLine("(");
                    preparePhysicalModelIntStatement.AppendLine(physicalModelInstantiation.CreatePhysicalModelSet(ConfigurationSettings.IntegrationDatabaseName, intTableFilterObjects).ToString());
                    preparePhysicalModelIntStatement.AppendLine(") sub");

                    var physicalModelDataTableInt = Utility.GetDataTable(ref connInt, preparePhysicalModelIntStatement.ToString());

                    // PRES
                    var preparePhysicalModelPresStatement = new StringBuilder();
                    preparePhysicalModelPresStatement.AppendLine("SELECT ");
                    preparePhysicalModelPresStatement.AppendLine(" [DATABASE_NAME] ");
                    preparePhysicalModelPresStatement.AppendLine(",[SCHEMA_NAME]");
                    preparePhysicalModelPresStatement.AppendLine(",[TABLE_NAME]");
                    preparePhysicalModelPresStatement.AppendLine(",[COLUMN_NAME]");
                    preparePhysicalModelPresStatement.AppendLine(",[DATA_TYPE]");
                    preparePhysicalModelPresStatement.AppendLine(",[CHARACTER_MAXIMUM_LENGTH]");
                    preparePhysicalModelPresStatement.AppendLine(",[NUMERIC_PRECISION]");
                    preparePhysicalModelPresStatement.AppendLine(",[ORDINAL_POSITION]");
                    preparePhysicalModelPresStatement.AppendLine(",[PRIMARY_KEY_INDICATOR]");
                    preparePhysicalModelPresStatement.AppendLine("FROM");
                    preparePhysicalModelPresStatement.AppendLine("(");
                    preparePhysicalModelPresStatement.AppendLine(physicalModelInstantiation.CreatePhysicalModelSet(ConfigurationSettings.PresentationDatabaseName, presTableFilterObjects).ToString());
                    preparePhysicalModelPresStatement.AppendLine(") sub");

                    var physicalModelDataTablePres = Utility.GetDataTable(ref connPres, preparePhysicalModelPresStatement.ToString());


                    if (physicalModelDataTableStg != null)
                    {
                        physicalModelDataTable.Merge(physicalModelDataTableStg);
                    }

                    if (physicalModelDataTablePsa != null)
                    {
                        physicalModelDataTable.Merge(physicalModelDataTablePsa);
                    }

                    if (physicalModelDataTableInt != null)
                    {
                        physicalModelDataTable.Merge(physicalModelDataTableInt);
                    }

                    if (physicalModelDataTablePres != null)
                    {
                        physicalModelDataTable.Merge(physicalModelDataTablePres);
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
                    allVirtualDatabaseAttributes.AppendLine(" ,[ORDINAL_POSITION]");
                    allVirtualDatabaseAttributes.AppendLine(" ,[PRIMARY_KEY_INDICATOR]");
                    allVirtualDatabaseAttributes.AppendLine("FROM [TMP_MD_VERSION_ATTRIBUTE] mapping");

                    physicalModelDataTable = Utility.GetDataTable(ref connOmd, allVirtualDatabaseAttributes.ToString());
                }

                try
                {
                    if (physicalModelDataTable.Rows.Count == 0)
                    {
                        _alert.SetTextLogging("--> No model information was found in the metadata.\r\n");
                    }
                    else
                    {
                        foreach (DataRow tableName in physicalModelDataTable.Rows)
                        {
                            using (var connection = new SqlConnection(metaDataConnection))
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
                                                              "','" + tableName["ORDINAL_POSITION"].ToString().Trim() +
                                                              "','" + tableName["PRIMARY_KEY_INDICATOR"].ToString()
                                                                  .Trim() + "')");

                                var command = new SqlCommand(insertKeyStatement.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    errorCounter++;
                                    _alert.SetTextLogging(
                                        "An issue has occured during preparation of the physical model extract metadata. Please check the Error Log for more details.\r\n");
                                    errorLog.AppendLine(
                                        "\r\nAn issue has occured during preparation of physical model metadata: \r\n\r\n" +
                                        ex);
                                }
                            }
                        }
                    }

                    worker.ReportProgress(40);
                    subProcess.Stop();
                    _alert.SetTextLogging("Preparation of the physical model extract completed, and has taken " +
                                          subProcess.Elapsed.TotalSeconds + " seconds.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging(
                        "An issue has occured during preparation of the physical model metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine(
                        "\r\nAn issue has occured during preparation of physical model metadata: \r\n\r\n" + ex);
                }

                #endregion


                #region Prepare attributes - 45%

                //Prepare Attributes
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
                        errorCounter++;
                        _alert.SetTextLogging("An issue has occured during preparation of the attribute metadata. Please check the Error Log for more details.\r\n");

                        errorLog.AppendLine("\r\nAn issue has occured during preparation of attribute metadata: \r\n\r\n" + ex);
                        errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertNAStatement);
                    }
                }

                /* Regular processing
                    RV: there is an issue below where not all SQL version (i.e. SQL Server) are supporting cross database SQL.
                    i.e. Azure. long term fix is to create individual queries to database without cross-db sql and add to single data table in the application
                */
                if (radioButtonPhysicalMode.Checked) // Read from live database
                {
                    _alert.SetTextLogging("Commencing preparing the attributes directly from the database.\r\n");
                }
                else // Virtual processing
                {
                    _alert.SetTextLogging("Commencing preparing the attributes from the metadata.\r\n");
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
                prepareAttStatement.AppendLine("    '" + eventDateTimeAtttribute + "',");
                prepareAttStatement.AppendLine("    '" + effectiveDateTimeAttribute + "',");
                prepareAttStatement.AppendLine("    '" + etlProcessId + "',");
                prepareAttStatement.AppendLine("    '" + loadDateTimeStamp + "',");
                prepareAttStatement.AppendLine("    '" + currentRecordAttribute + "'");
                prepareAttStatement.AppendLine("  ) ");

                // Load the data table, get the attributes
                var listAtt = Utility.GetDataTable(ref connOmd, prepareAttStatement.ToString());

                // Check if there are any attributes found, otherwise insert into the repository
                if (listAtt.Rows.Count == 0)
                {
                    _alert.SetTextLogging(
                        "--> No attributes were found in the metadata, did you reverse-engineer the model?\r\n");
                }
                else
                {
                    foreach (DataRow tableName in listAtt.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            //_alert.SetTextLogging("--> Processing " + tableName["COLUMN_NAME"] + ".\r\n");

                            var insertStatement = new StringBuilder();

                            insertStatement.AppendLine("INSERT INTO [MD_ATTRIBUTE]");
                            insertStatement.AppendLine("([ATTRIBUTE_NAME])");
                            insertStatement.AppendLine("VALUES ('" + tableName["COLUMN_NAME"].ToString().Trim() + "')");

                            var command = new SqlCommand(insertStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                attCounter++;
                            }
                            catch (Exception ex)
                            {
                                errorCounter++;
                                _alert.SetTextLogging(
                                    "An issue has occured during preparation of the attribute metadata. Please check the Error Log for more details.\r\n");
                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of attribute metadata: \r\n\r\n" + ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                            }
                        }
                    }

                    _alert.SetTextLogging("--> Processing " + attCounter + " attributes.\r\n");
                }

                worker.ReportProgress(45);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the attributes completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds + " seconds.\r\n");


                #endregion


                #region Business Key - 50%

                //Understanding the Business Key (MD_BUSINESS_KEY_COMPONENT)
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing the definition of the Business Key.\r\n");

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
                prepareKeyStatement.AppendLine("        A.BUSINESS_KEY_ATTRIBUTE,");
                prepareKeyStatement.AppendLine("        A.TARGET_TABLE,");
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
                prepareKeyStatement.AppendLine("          TARGET_TABLE, ");
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
                    "            SELECT DISTINCT SOURCE_TABLE, TARGET_TABLE, LTRIM(RTRIM(BUSINESS_KEY_ATTRIBUTE)) AS BUSINESS_KEY_ATTRIBUTE");
                prepareKeyStatement.AppendLine("            FROM TMP_MD_TABLE_MAPPING");
                prepareKeyStatement.AppendLine("            WHERE TARGET_TABLE_TYPE = '" +
                                               ClassMetadataHandling.TableTypes.CoreBusinessConcept + "'");
                prepareKeyStatement.AppendLine("              AND [PROCESS_INDICATOR] = 'Y'");
                prepareKeyStatement.AppendLine("        ) TableName");
                prepareKeyStatement.AppendLine(
                    "    ) AS A CROSS APPLY BUSINESS_KEY_ATTRIBUTE_XML.nodes('/M') AS Split(a)");
                prepareKeyStatement.AppendLine(
                    "    WHERE BUSINESS_KEY_ATTRIBUTE <> 'N/A' AND A.BUSINESS_KEY_ATTRIBUTE != ''");
                prepareKeyStatement.AppendLine(") pivotsub");
                prepareKeyStatement.AppendLine("LEFT OUTER JOIN");
                prepareKeyStatement.AppendLine("       (");
                prepareKeyStatement.AppendLine("              SELECT SOURCE_NAME, [SCHEMA_NAME]");
                prepareKeyStatement.AppendLine("              FROM MD_SOURCE");
                prepareKeyStatement.AppendLine("       ) stgsub");
                prepareKeyStatement.AppendLine(
                    "ON pivotsub.SOURCE_TABLE = stgsub.[SCHEMA_NAME]+'.'+stgsub.SOURCE_NAME");
                prepareKeyStatement.AppendLine("LEFT OUTER JOIN");
                prepareKeyStatement.AppendLine("       (");
                prepareKeyStatement.AppendLine("              SELECT HUB_NAME AS TARGET_NAME, [SCHEMA_NAME]");
                prepareKeyStatement.AppendLine("              FROM MD_HUB");
                prepareKeyStatement.AppendLine("       ) hubsub");
                prepareKeyStatement.AppendLine(
                    "ON pivotsub.TARGET_TABLE = hubsub.[SCHEMA_NAME]+'.'+hubsub.TARGET_NAME");
                prepareKeyStatement.AppendLine("ORDER BY stgsub.SOURCE_NAME, hubsub.TARGET_NAME, COMPONENT_ORDER");

                var listKeys = Utility.GetDataTable(ref connOmd, prepareKeyStatement.ToString());

                if (listKeys.Rows.Count == 0)
                {
                    _alert.SetTextLogging(
                        "-- >  No attributes were found in the metadata, did you reverse-engineer the model?\r\n");
                }
                else
                {
                    foreach (DataRow tableName in listKeys.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {

                            var keyComponent = tableName["COMPONENT_VALUE"]; //Handle quotes between SQL and C%
                            keyComponent = keyComponent.ToString().Replace("'", "''");

                            _alert.SetTextLogging("--> Processing the Business Key " +
                                                  tableName["BUSINESS_KEY_ATTRIBUTE"] + " (for component " +
                                                  keyComponent + ") from " + tableName["SOURCE_NAME"] + " to " +
                                                  tableName["TARGET_NAME"] + "\r\n");

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
                                errorCounter++;
                                _alert.SetTextLogging(
                                    "An issue has occured during preparation of the Business Key metadata. Please check the Error Log for more details.\r\n");

                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of Business Key metadata: \r\n\r\n" +
                                    ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                            }
                        }
                    }
                }

                worker.ReportProgress(50);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the Business Key definition completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Business Key components - 60%

                //Understanding the Business Key component parts
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing the Business Key component analysis.\r\n");

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
                    _alert.SetTextLogging(
                        "--> No attributes were found in the metadata, did you reverse-engineer the model?\r\n");
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
                                errorCounter++;
                                _alert.SetTextLogging(
                                    "An issue has occured during preparation of the Business Key component metadata. Please check the Error Log for more details.\r\n");

                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of Business Key component metadata: \r\n\r\n" +
                                    ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                            }
                        }
                    }
                }

                worker.ReportProgress(60);
                subProcess.Stop();
                _alert.SetTextLogging("--> Processing " + keyPartCounter + " Business Key component attributes.\r\n");
                _alert.SetTextLogging("Preparation of the Business Key components completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds + " seconds.\r\n");


                #endregion


                #region Hub / Link relationship - 75%

                //Prepare HUB / LNK xref
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the relationship between Hubs and Links.\r\n");

                var virtualisationSnippet = new StringBuilder();
                if (radioButtonPhysicalMode.Checked)
                {
                    virtualisationSnippet.AppendLine(" SELECT ");
                    virtualisationSnippet.AppendLine("     OBJECT_SCHEMA_NAME(OBJECT_ID, DB_ID('" +
                                                     ConfigurationSettings.IntegrationDatabaseName +
                                                     "')) AS LINK_SCHEMA,");
                    virtualisationSnippet.AppendLine("     OBJECT_NAME(OBJECT_ID,DB_ID('" +
                                                     ConfigurationSettings.IntegrationDatabaseName +
                                                     "'))  AS LINK_NAME,");
                    virtualisationSnippet.AppendLine("     [name] AS HUB_TARGET_KEY_NAME_IN_LINK,");
                    virtualisationSnippet.AppendLine(
                        "     ROW_NUMBER() OVER(PARTITION BY OBJECT_NAME(OBJECT_ID,DB_ID('" +
                        ConfigurationSettings.IntegrationDatabaseName + "')) ORDER BY column_id) AS LINK_ORDER");
                    virtualisationSnippet.AppendLine(" FROM " + linkedServer + integrationDatabase + @".sys.columns");
                    virtualisationSnippet.AppendLine(" WHERE [column_id] > 1");
                    virtualisationSnippet.AppendLine("   AND OBJECT_NAME(OBJECT_ID,DB_ID('" +
                                                     ConfigurationSettings.IntegrationDatabaseName + "')) LIKE '" +
                                                     lnkTablePrefix + @"'");
                    virtualisationSnippet.AppendLine("   AND [name] NOT IN ('" +
                                                     ConfigurationSettings.RecordSourceAttribute + "','" +
                                                     ConfigurationSettings.AlternativeRecordSourceAttribute + "','" +
                                                     ConfigurationSettings.AlternativeLoadDateTimeAttribute + "','" +
                                                     ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute +
                                                     "','" + ConfigurationSettings.EtlProcessAttribute + "','" +
                                                     FormBase.ConfigurationSettings.LoadDateTimeAttribute + "')");
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
                    virtualisationSnippet.AppendLine("  AND COLUMN_NAME NOT IN ('" +
                                                     ConfigurationSettings.RecordSourceAttribute + "','" +
                                                     ConfigurationSettings.AlternativeRecordSourceAttribute + "','" +
                                                     ConfigurationSettings.AlternativeLoadDateTimeAttribute + "','" +
                                                     ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute +
                                                     "','" + FormBase.ConfigurationSettings.EtlProcessAttribute +
                                                     "','" + FormBase.ConfigurationSettings.LoadDateTimeAttribute +
                                                     "')");

                }


                var prepareHubLnkXrefStatement = new StringBuilder();

                prepareHubLnkXrefStatement.AppendLine("SELECT");
                prepareHubLnkXrefStatement.AppendLine("       hub_tbl.HUB_NAME,");
                prepareHubLnkXrefStatement.AppendLine("       lnk_tbl.LINK_NAME,");
                prepareHubLnkXrefStatement.AppendLine("       lnk_hubkey_order.HUB_KEY_ORDER AS HUB_ORDER,");
                prepareHubLnkXrefStatement.AppendLine("       lnk_target_model.HUB_TARGET_KEY_NAME_IN_LINK");
                prepareHubLnkXrefStatement.AppendLine("   FROM");
                prepareHubLnkXrefStatement.AppendLine(
                    "   -- This base query adds the Link and its Hubs and their order by pivoting on the full business key");
                prepareHubLnkXrefStatement.AppendLine("   (");
                prepareHubLnkXrefStatement.AppendLine("       SELECT");
                prepareHubLnkXrefStatement.AppendLine("       TARGET_TABLE,");
                prepareHubLnkXrefStatement.AppendLine("       SOURCE_TABLE,");
                prepareHubLnkXrefStatement.AppendLine("       BUSINESS_KEY_ATTRIBUTE,");
                prepareHubLnkXrefStatement.AppendLine(
                    "       LTRIM(Split.a.value('.', 'VARCHAR(4000)')) AS BUSINESS_KEY_PART,");
                prepareHubLnkXrefStatement.AppendLine(
                    "       ROW_NUMBER() OVER(PARTITION BY TARGET_TABLE ORDER BY TARGET_TABLE) AS HUB_KEY_ORDER");
                prepareHubLnkXrefStatement.AppendLine("       FROM");
                prepareHubLnkXrefStatement.AppendLine("       (");
                prepareHubLnkXrefStatement.AppendLine("       SELECT");
                prepareHubLnkXrefStatement.AppendLine("           TARGET_TABLE,");
                prepareHubLnkXrefStatement.AppendLine("           SOURCE_TABLE,");
                prepareHubLnkXrefStatement.AppendLine(
                    "           ROW_NUMBER() OVER(PARTITION BY TARGET_TABLE ORDER BY TARGET_TABLE) AS LINK_ORDER,");
                prepareHubLnkXrefStatement.AppendLine(
                    "           BUSINESS_KEY_ATTRIBUTE, CAST('<M>' + REPLACE(BUSINESS_KEY_ATTRIBUTE, ',', '</M><M>') + '</M>' AS XML) AS BUSINESS_KEY_SOURCE_XML");
                prepareHubLnkXrefStatement.AppendLine("       FROM  TMP_MD_TABLE_MAPPING");
                prepareHubLnkXrefStatement.AppendLine("       WHERE [TARGET_TABLE_TYPE] = '" +
                                                      ClassMetadataHandling.TableTypes.NaturalBusinessRelationship +
                                                      "'");
                prepareHubLnkXrefStatement.AppendLine("           AND [PROCESS_INDICATOR] = 'Y'");
                prepareHubLnkXrefStatement.AppendLine(
                    "     ) AS A CROSS APPLY BUSINESS_KEY_SOURCE_XML.nodes('/M') AS Split(a)");
                prepareHubLnkXrefStatement.AppendLine(
                    "     WHERE LINK_ORDER=1 --Any link will do, the order of the Hub keys in the Link will always be the same");
                prepareHubLnkXrefStatement.AppendLine(" ) lnk_hubkey_order");
                prepareHubLnkXrefStatement.AppendLine(
                    " -- Adding the information required for the target model in the query");
                prepareHubLnkXrefStatement.AppendLine(" JOIN ");
                prepareHubLnkXrefStatement.AppendLine(" (");
                prepareHubLnkXrefStatement.AppendLine(virtualisationSnippet.ToString());
                prepareHubLnkXrefStatement.AppendLine(" ) lnk_target_model");
                prepareHubLnkXrefStatement.AppendLine(
                    " ON lnk_hubkey_order.TARGET_TABLE = lnk_target_model.LINK_SCHEMA+'.'+lnk_target_model.LINK_NAME COLLATE DATABASE_DEFAULT");
                prepareHubLnkXrefStatement.AppendLine(
                    " AND lnk_hubkey_order.HUB_KEY_ORDER = lnk_target_model.LINK_ORDER");
                prepareHubLnkXrefStatement.AppendLine(" --Adding the Hub mapping data to get the business keys");
                prepareHubLnkXrefStatement.AppendLine(" JOIN TMP_MD_TABLE_MAPPING hub");
                prepareHubLnkXrefStatement.AppendLine("     ON lnk_hubkey_order.[SOURCE_TABLE] = hub.SOURCE_TABLE");
                prepareHubLnkXrefStatement.AppendLine(
                    "     AND lnk_hubkey_order.[BUSINESS_KEY_PART] = hub.BUSINESS_KEY_ATTRIBUTE-- This condition is required to remove the redundant rows caused by the Link key pivoting");
                prepareHubLnkXrefStatement.AppendLine("     AND hub.[TARGET_TABLE_TYPE] = '" +
                                                      ClassMetadataHandling.TableTypes.CoreBusinessConcept + "'");
                prepareHubLnkXrefStatement.AppendLine("     AND hub.[PROCESS_INDICATOR] = 'Y'");
                prepareHubLnkXrefStatement.AppendLine(" --Lastly adding the IDs for the Hubs and Links");
                prepareHubLnkXrefStatement.AppendLine(" JOIN dbo.MD_HUB hub_tbl");
                prepareHubLnkXrefStatement.AppendLine(
                    "     ON hub.TARGET_TABLE = hub_tbl.[SCHEMA_NAME]+'.'+hub_tbl.HUB_NAME");
                prepareHubLnkXrefStatement.AppendLine(" JOIN dbo.MD_LINK lnk_tbl");
                prepareHubLnkXrefStatement.AppendLine(
                    "     ON lnk_hubkey_order.TARGET_TABLE = lnk_tbl.[SCHEMA_NAME]+'.'+lnk_tbl.LINK_NAME");

                var listHlXref = Utility.GetDataTable(ref connOmd, prepareHubLnkXrefStatement.ToString());

                if (listHlXref != null)
                {
                    foreach (DataRow tableName in listHlXref.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            _alert.SetTextLogging("--> Processing the " + tableName["HUB_NAME"] + " to " +
                                                  tableName["LINK_NAME"] + " relationship.\r\n");

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
                                errorCounter++;
                                _alert.SetTextLogging(
                                    "An issue has occured during preparation of the Hub / Link XREF metadata. Please check the Error Log for more details.\r\n");

                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of the Hub / Link XREF metadata: \r\n\r\n" +
                                    ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                            }
                        }
                    }
                }

                worker.ReportProgress(75);
                subProcess.Stop();
                _alert.SetTextLogging(
                    "Preparation of the relationship between Hubs and Links completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Link Business Key - 78%

                // Prepare links business key backfill

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Link Business key metadata.\r\n");

                // Getting the distinct list of tables to go into the MD_LINK table
                selectionRows =
                    inputTableMetadata.Select(
                        "PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '%" + lnkTablePrefix + "%'");

                var distincLinksForBusinessKey = new List<string>();

                // Create a distinct list of sources from the data grid
                foreach (DataRow row in selectionRows)
                {
                    string target_table = row["TARGET_TABLE"].ToString().Trim();
                    if (!distincLinksForBusinessKey.Contains(target_table))
                    {
                        distincLinksForBusinessKey.Add(target_table);
                    }
                }

                // Insert the rest of the rows
                foreach (var tableName in distincLinksForBusinessKey)
                {
                    var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                    var businessKeyList = ClassMetadataHandling.GetLinkTargetBusinessKeyList(fullyQualifiedName.Key,
                        fullyQualifiedName.Value, versionId);
                    string businessKey = string.Join(",", businessKeyList);

                    var updateStatement = new StringBuilder();

                    updateStatement.AppendLine("UPDATE [MD_LINK]");
                    updateStatement.AppendLine("SET [BUSINESS_KEY] = '" + businessKey + "'");
                    updateStatement.AppendLine("WHERE [SCHEMA_NAME] =  '" + fullyQualifiedName.Key + "'");
                    updateStatement.AppendLine("AND [LINK_NAME] =  '" + fullyQualifiedName.Value + "'");

                    var connection = new SqlConnection(metaDataConnection);
                    var command = new SqlCommand(updateStatement.ToString(), connection);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        errorCounter++;
                        _alert.SetTextLogging(
                            "An issue has occured during preparation of the Link Business Key. Please check the Error Log for more details.\r\n");

                        errorLog.AppendLine(
                            "\r\nAn issue has occured during preparation of the Link Business Key: \r\n\r\n" + ex);
                        errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + updateStatement);
                    }
                }

                #endregion


                #region Stg / Link relationship - 80%

                //Prepare STG / LNK xref
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the relationship between Source and Link tables.\r\n");

                var preparestgLnkXrefStatement = new StringBuilder();
                preparestgLnkXrefStatement.AppendLine("SELECT");
                preparestgLnkXrefStatement.AppendLine("  lnk_tbl.LINK_NAME,");
                preparestgLnkXrefStatement.AppendLine("  stg_tbl.SOURCE_NAME,");
                preparestgLnkXrefStatement.AppendLine("  lnk.FILTER_CRITERIA,");
                preparestgLnkXrefStatement.AppendLine("  lnk.BUSINESS_KEY_ATTRIBUTE");
                preparestgLnkXrefStatement.AppendLine("FROM [dbo].[TMP_MD_TABLE_MAPPING] lnk");
                preparestgLnkXrefStatement.AppendLine(
                    "JOIN [dbo].[MD_LINK] lnk_tbl ON lnk.TARGET_TABLE = lnk_tbl.[SCHEMA_NAME]+'.'+lnk_tbl.LINK_NAME");
                preparestgLnkXrefStatement.AppendLine(
                    "JOIN [dbo].[MD_SOURCE] stg_tbl ON lnk.SOURCE_TABLE = stg_tbl.[SCHEMA_NAME]+'.'+stg_tbl.SOURCE_NAME");
                preparestgLnkXrefStatement.AppendLine("WHERE lnk.TARGET_TABLE_TYPE = '" +
                                                      ClassMetadataHandling.TableTypes.NaturalBusinessRelationship +
                                                      "'");
                preparestgLnkXrefStatement.AppendLine("AND[PROCESS_INDICATOR] = 'Y'");

                var listStgLinkXref = Utility.GetDataTable(ref connOmd, preparestgLnkXrefStatement.ToString());

                foreach (DataRow tableName in listStgLinkXref.Rows)
                {
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        _alert.SetTextLogging("--> Processing the " + tableName["SOURCE_NAME"] + " to " +
                                              tableName["LINK_NAME"] + " relationship.\r\n");


                        var filterCriterion = tableName["FILTER_CRITERIA"].ToString().Trim();
                        filterCriterion = filterCriterion.Replace("'", "''");

                        var businessKeyDefinition = tableName["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                        businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                        var loadVector = ClassMetadataHandling.GetLoadVector(tableName["SOURCE_NAME"].ToString(),
                            tableName["LINK_NAME"].ToString());


                        var insertStatement = new StringBuilder();
                        insertStatement.AppendLine("INSERT INTO [MD_SOURCE_LINK_XREF]");
                        insertStatement.AppendLine(
                            "([SOURCE_NAME], [LINK_NAME], [FILTER_CRITERIA], [BUSINESS_KEY_DEFINITION], [LOAD_VECTOR])");
                        insertStatement.AppendLine("VALUES ('" + tableName["SOURCE_NAME"] +
                                                   "','" + tableName["LINK_NAME"] +
                                                   "','" + filterCriterion +
                                                   "','" + businessKeyDefinition +
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
                            errorCounter++;
                            _alert.SetTextLogging(
                                "An issue has occured during preparation of the Hub / Link XREF metadata. Please check the Error Log for more details.\r\n");

                            errorLog.AppendLine(
                                "\r\nAn issue has occured during preparation of the Hub / Link XREF metadata: \r\n\r\n" +
                                ex);
                            errorLog.AppendLine("\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                        }
                    }
                }

                worker.ReportProgress(80);
                subProcess.Stop();
                _alert.SetTextLogging(
                    "Preparation of the relationship between Source and the Links completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Manually mapped Source to Staging Area Attribute XREF - 81%

                // Prepare the Source to Staging Area XREF
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging(
                    "Commencing preparing the Source to Staging column-to-column mapping metadata based on the manual mappings.\r\n");

                // Getting the distinct list of row from the data table
                selectionRows = inputAttributeMetadata.Select("TARGET_TABLE LIKE '%" + stagingPrefix + "%'");

                if (selectionRows.Length == 0)
                {
                    _alert.SetTextLogging(
                        "No manual column-to-column mappings for Source-to-Staging were detected.\r\n");
                }
                else
                {
                    // Process the unique Staging Area records
                    foreach (var row in selectionRows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            _alert.SetTextLogging("--> Processing the mapping from " + row["SOURCE_TABLE"] + " - " +
                                                  (string) row["SOURCE_COLUMN"] + " to " + row["TARGET_TABLE"] + " - " +
                                                  (string) row["TARGET_COLUMN"] + ".\r\n");

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_SOURCE_STAGING_ATTRIBUTE_XREF]");
                            insertStatement.AppendLine("([SOURCE_NAME], [STAGING_NAME], [ATTRIBUTE_NAME_FROM], [ATTRIBUTE_NAME_TO], [MAPPING_TYPE])");
                            insertStatement.AppendLine("VALUES (" +
                                                       "'" + row["SOURCE_TABLE"] + "'," +
                                                       "'" + row["TARGET_TABLE"] + "', " +
                                                       "'" + (string) row["SOURCE_COLUMN"] + "', " +
                                                       "'" + (string) row["TARGET_COLUMN"] + "', " +
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
                                errorCounter++;
                                _alert.SetTextLogging(
                                    "An issue has occured during preparation of the attribute mapping between the Source and the Staging Area. Please check the Error Log for more details.\r\n");

                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of the Source to Staging attribute mapping: \r\n\r\n" +
                                    ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                            }
                        }
                    }
                }

                worker?.ReportProgress(87);
                subProcess.Stop();
                _alert.SetTextLogging(
                    "Preparation of the manual column-to-column mappings for Source-to-Staging completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Automatically mapped Source to Staging Area Attribute XREF 93%

                //Prepare automatic attribute mapping
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");

                int automaticMappingCounter = 0;

                if (radioButtonPhysicalMode.Checked)
                {
                    _alert.SetTextLogging(
                        "Commencing preparing the (automatic) column-to-column mapping metadata for Source to Staging, based on what's available in the database.\r\n");
                }
                else
                {
                    _alert.SetTextLogging(
                        "Commencing preparing the (automatic) column-to-column mapping metadata for Source to Staging, based on what's available in the physical model metadata.\r\n");
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
                prepareMappingStagingStatement.AppendLine(" ,[ORDINAL_POSITION]");
                prepareMappingStagingStatement.AppendLine(" ,[PRIMARY_KEY_INDICATOR]");
                prepareMappingStagingStatement.AppendLine("FROM [MD_PHYSICAL_MODEL]");

                prepareMappingStagingStatement.AppendLine("),");
                prepareMappingStagingStatement.AppendLine("XREF AS");
                prepareMappingStagingStatement.AppendLine("(");
                prepareMappingStagingStatement.AppendLine("  SELECT");
                prepareMappingStagingStatement.AppendLine("    xref.*,");
                prepareMappingStagingStatement.AppendLine("    src.[SCHEMA_NAME] AS SOURCE_SCHEMA_NAME,");
                prepareMappingStagingStatement.AppendLine("    tgt.[SCHEMA_NAME] AS TARGET_SCHEMA_NAME");
                prepareMappingStagingStatement.AppendLine("  FROM MD_SOURCE_STAGING_XREF xref");
                prepareMappingStagingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_SOURCE src ON xref.SOURCE_NAME = src.SOURCE_NAME");
                prepareMappingStagingStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_STAGING tgt ON xref.STAGING_NAME = tgt.STAGING_NAME");
                prepareMappingStagingStatement.AppendLine(") ");
                prepareMappingStagingStatement.AppendLine("SELECT");
                prepareMappingStagingStatement.AppendLine("  XREF.SOURCE_NAME, ");
                prepareMappingStagingStatement.AppendLine("  XREF.STAGING_NAME,");
                prepareMappingStagingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_NAME_FROM,");
                prepareMappingStagingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_NAME_TO,");
                prepareMappingStagingStatement.AppendLine("  'automatically mapped' as VERIFICATION");
                prepareMappingStagingStatement.AppendLine("FROM XREF");
                prepareMappingStagingStatement.AppendLine(
                    "JOIN ALL_DATABASE_COLUMNS ADC_TARGET ON XREF.TARGET_SCHEMA_NAME = ADC_TARGET.[SCHEMA_NAME] AND XREF.STAGING_NAME = ADC_TARGET.TABLE_NAME");
                prepareMappingStagingStatement.AppendLine(
                    "JOIN dbo.MD_ATTRIBUTE tgt_attr ON ADC_TARGET.COLUMN_NAME = tgt_attr.ATTRIBUTE_NAME COLLATE DATABASE_DEFAULT");
                prepareMappingStagingStatement.AppendLine("WHERE NOT EXISTS (");
                prepareMappingStagingStatement.AppendLine("  SELECT SOURCE_NAME, STAGING_NAME, ATTRIBUTE_NAME_FROM");
                prepareMappingStagingStatement.AppendLine("  FROM MD_SOURCE_STAGING_ATTRIBUTE_XREF manualmapping");
                prepareMappingStagingStatement.AppendLine("WHERE");
                prepareMappingStagingStatement.AppendLine("      manualmapping.SOURCE_NAME = XREF.SOURCE_NAME");
                prepareMappingStagingStatement.AppendLine("  AND manualmapping.STAGING_NAME = XREF.STAGING_NAME");
                prepareMappingStagingStatement.AppendLine(
                    "  AND manualmapping.ATTRIBUTE_NAME_FROM = ADC_TARGET.COLUMN_NAME");
                prepareMappingStagingStatement.AppendLine(")");
                prepareMappingStagingStatement.AppendLine("ORDER BY SOURCE_NAME");


                var automaticAttributeMappings = Utility.GetDataTable(ref connOmd, prepareMappingStagingStatement.ToString());

                if (automaticAttributeMappings.Rows.Count == 0)
                {
                    _alert.SetTextLogging("--> No automatic column-to-column mappings were detected.\r\n");
                }
                else
                {
                    // Process the unique attribute mappings
                    foreach (DataRow row in automaticAttributeMappings.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            _alert.SetTextLogging("--> Processing the mapping from " + (string) row["SOURCE_NAME"] +
                                                  " - " + (string) row["ATTRIBUTE_NAME_FROM"] + " to " +
                                                  (string) row["STAGING_NAME"] + " - " +
                                                  (string) row["ATTRIBUTE_NAME_TO"] + ".\r\n");

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_SOURCE_STAGING_ATTRIBUTE_XREF]");
                            insertStatement.AppendLine("([SOURCE_NAME], [STAGING_NAME], [ATTRIBUTE_NAME_FROM], [ATTRIBUTE_NAME_TO], [MAPPING_TYPE])");
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
                                errorCounter++;
                                _alert.SetTextLogging(
                                    "An issue has occured during preparation of the attribute mapping between the Source and the Staging Area. Please check the Error Log for more details.\r\n");

                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of the Source to Staging attribute mapping: \r\n\r\n" +
                                    ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                            }
                        }
                    }
                }

                worker.ReportProgress(90);
                subProcess.Stop();
                _alert.SetTextLogging("--> Processing " + automaticMappingCounter +
                                      " automatically added attribute mappings.\r\n");
                _alert.SetTextLogging(
                    "Preparation of the automatically mapped column-to-column metadata completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.\r\n");
                #endregion


                #region Manually mapped Source to Persistent Staging Area Attribute XREF - 81%

                // Prepare the Source to Persistent Staging Area XREF
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Source to Persistent Staging column-to-column mapping metadata based on the manual mappings.\r\n");

                // Getting the distinct list of row from the data table
                selectionRows = inputAttributeMetadata.Select("TARGET_TABLE LIKE '%" + psaPrefix + "%'");

                if (selectionRows.Length == 0)
                {
                    _alert.SetTextLogging("No manual column-to-column mappings for Source to Persistent Staging were detected.\r\n");
                }
                else
                {
                    // Process the unique Persistent Staging Area records
                    foreach (var row in selectionRows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            _alert.SetTextLogging("--> Processing the mapping from " + row["SOURCE_TABLE"] + " - " +
                                                  (string) row["SOURCE_COLUMN"] + " to " + row["TARGET_TABLE"] + " - " +
                                                  (string) row["TARGET_COLUMN"] + ".\r\n");

                            //var localTableName = ClassMetadataHandling.GetNonQualifiedTableName(row["TARGET_TABLE"].ToString());

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF]");
                            insertStatement.AppendLine("([SOURCE_NAME], [PERSISTENT_STAGING_NAME], [ATTRIBUTE_NAME_FROM], [ATTRIBUTE_NAME_TO], [MAPPING_TYPE])");
                            insertStatement.AppendLine("VALUES (" + 
                                                       "'" + row["SOURCE_TABLE"] + "', " +
                                                       "'" + row["TARGET_TABLE"] + "', " +
                                                       "'" + (string) row["SOURCE_COLUMN"] + "', " +
                                                       "'" + (string) row["TARGET_COLUMN"] + "', " +
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
                                errorCounter++;
                                _alert.SetTextLogging(
                                    "An issue has occured during preparation of the attribute mapping between the Source and the Persistent Staging Area. Please check the Error Log for more details.\r\n");

                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of the Source to Persistent Staging attribute mapping: \r\n\r\n" +
                                    ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                            }
                        }
                    }
                }

                worker?.ReportProgress(87);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the manual column-to-column mappings for Source-to-Staging completed, and has taken " + subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Automatically mapped Source to Persistent Staging Area Attribute XREF 93%

                // Prepare automatic attribute mapping
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");

                var prepareMappingPersistentStagingStatement = new StringBuilder();

                automaticMappingCounter = 0;

                _alert.SetTextLogging(
                    radioButtonPhysicalMode.Checked
                        ? "Commencing preparing the (automatic) column-to-column mapping metadata for Source to Persistent Staging, based on what's available in the database.\r\n"
                        : "Commencing preparing the (automatic) column-to-column mapping metadata for Source to Persistent Staging, based on what's available in the physical model metadata.\r\n");

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
                prepareMappingPersistentStagingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_SOURCE src ON xref.SOURCE_NAME = src.SOURCE_NAME");
                prepareMappingPersistentStagingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_PERSISTENT_STAGING tgt ON xref.PERSISTENT_STAGING_NAME = tgt.PERSISTENT_STAGING_NAME");
                prepareMappingPersistentStagingStatement.AppendLine(") ");
                prepareMappingPersistentStagingStatement.AppendLine("SELECT");
                prepareMappingPersistentStagingStatement.AppendLine("  XREF.SOURCE_NAME, ");
                prepareMappingPersistentStagingStatement.AppendLine("  XREF.PERSISTENT_STAGING_NAME,");
                prepareMappingPersistentStagingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_NAME_FROM,");
                prepareMappingPersistentStagingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_NAME_TO,");
                prepareMappingPersistentStagingStatement.AppendLine("  'automatically mapped' as VERIFICATION");
                prepareMappingPersistentStagingStatement.AppendLine("FROM XREF");
                prepareMappingPersistentStagingStatement.AppendLine("JOIN ALL_DATABASE_COLUMNS ADC_TARGET ON XREF.TARGET_SCHEMA_NAME = ADC_TARGET.[SCHEMA_NAME] AND XREF.PERSISTENT_STAGING_NAME_SHORT = ADC_TARGET.TABLE_NAME");
                prepareMappingPersistentStagingStatement.AppendLine("JOIN dbo.MD_ATTRIBUTE tgt_attr ON ADC_TARGET.COLUMN_NAME = tgt_attr.ATTRIBUTE_NAME COLLATE DATABASE_DEFAULT");
                prepareMappingPersistentStagingStatement.AppendLine("WHERE NOT EXISTS (");
                prepareMappingPersistentStagingStatement.AppendLine("  SELECT SOURCE_NAME, PERSISTENT_STAGING_NAME, ATTRIBUTE_NAME_FROM");
                prepareMappingPersistentStagingStatement.AppendLine("  FROM MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF manualmapping");
                prepareMappingPersistentStagingStatement.AppendLine("  WHERE");
                prepareMappingPersistentStagingStatement.AppendLine("      manualmapping.SOURCE_NAME = XREF.SOURCE_NAME");
                prepareMappingPersistentStagingStatement.AppendLine("  AND manualmapping.PERSISTENT_STAGING_NAME = XREF.PERSISTENT_STAGING_NAME");
                prepareMappingPersistentStagingStatement.AppendLine("  AND manualmapping.ATTRIBUTE_NAME_TO = ADC_TARGET.COLUMN_NAME");
                prepareMappingPersistentStagingStatement.AppendLine("  AND manualmapping.MAPPING_TYPE = 'Manual mapping'");
                prepareMappingPersistentStagingStatement.AppendLine(")");
                prepareMappingPersistentStagingStatement.AppendLine("ORDER BY SOURCE_NAME");

                var automaticAttributeMappingsPsa = Utility.GetDataTable(ref connOmd, prepareMappingPersistentStagingStatement.ToString());

                if (automaticAttributeMappingsPsa.Rows.Count == 0)
                {
                    _alert.SetTextLogging("--> No automatic column-to-column mappings were detected.\r\n");
                }
                else
                {
                    // Process the unique attribute mappings
                    foreach (DataRow row in automaticAttributeMappingsPsa.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            _alert.SetTextLogging("--> Processing the mapping from " + (string) row["SOURCE_NAME"] +
                                                  " - " + (string) row["ATTRIBUTE_NAME_FROM"] + " to " +
                                                  (string) row["PERSISTENT_STAGING_NAME"] + " - " +
                                                  (string) row["ATTRIBUTE_NAME_TO"] + ".\r\n");

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF]");
                            insertStatement.AppendLine("([SOURCE_NAME], [PERSISTENT_STAGING_NAME], [ATTRIBUTE_NAME_FROM], [ATTRIBUTE_NAME_TO], [MAPPING_TYPE])");
                            insertStatement.AppendLine("VALUES ("  +
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
                                errorCounter++;
                                _alert.SetTextLogging(
                                    "An issue has occured during preparation of the attribute mapping between the Source and the Persistent Staging Area. Please check the Error Log for more details.\r\n");

                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of the Source to Persistent Staging attribute mapping: \r\n\r\n" +
                                    ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                            }
                        }
                    }
                }

                worker.ReportProgress(90);
                subProcess.Stop();
                _alert.SetTextLogging("--> Processing " + automaticMappingCounter +
                                      " automatically added attribute mappings.\r\n");
                _alert.SetTextLogging(
                    "Preparation of the automatically mapped column-to-column metadata completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.\r\n");


                #endregion


                #region Manually mapped attributes for SAT and LSAT 90%

                //Prepare Manual Attribute mapping for Satellites and Link Satellites
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging(
                    "Commencing preparing the Satellite and Link-Satellite column-to-column mapping metadata based on the manual mappings.\r\n");

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
                prepareMappingStatementManual.AppendLine("LEFT OUTER JOIN dbo.MD_SATELLITE sat on sat.[SCHEMA_NAME]+'.'+sat.SATELLITE_NAME=mapping.TARGET_TABLE");
                prepareMappingStatementManual.AppendLine("LEFT OUTER JOIN dbo.MD_ATTRIBUTE target_attr on mapping.TARGET_COLUMN = target_attr.ATTRIBUTE_NAME");
                prepareMappingStatementManual.AppendLine("LEFT OUTER JOIN dbo.MD_SOURCE stg on stg.[SCHEMA_NAME]+'.'+stg.SOURCE_NAME = mapping.SOURCE_TABLE");
                prepareMappingStatementManual.AppendLine("LEFT OUTER JOIN dbo.MD_ATTRIBUTE stg_attr on mapping.SOURCE_COLUMN = stg_attr.ATTRIBUTE_NAME");
                prepareMappingStatementManual.AppendLine("LEFT OUTER JOIN dbo.TMP_MD_TABLE_MAPPING table_mapping");
                prepareMappingStatementManual.AppendLine("    ON mapping.TARGET_TABLE = table_mapping.TARGET_TABLE");
                prepareMappingStatementManual.AppendLine("AND mapping.SOURCE_TABLE = table_mapping.SOURCE_TABLE");
                prepareMappingStatementManual.AppendLine("WHERE mapping.TARGET_TABLE_TYPE IN ('" +
                                                         ClassMetadataHandling.TableTypes.Context + "', '" +
                                                         ClassMetadataHandling.TableTypes
                                                             .NaturalBusinessRelationshipContext + "')");
                prepareMappingStatementManual.AppendLine("   AND table_mapping.PROCESS_INDICATOR = 'Y' ");


                var attributeMappingsSatellites = Utility.GetDataTable(ref connOmd, prepareMappingStatementManual.ToString());

                if (attributeMappingsSatellites.Rows.Count == 0)
                {
                    _alert.SetTextLogging("--> No manual column-to-column mappings were detected.\r\n");
                }
                else
                {
                    foreach (DataRow row in attributeMappingsSatellites.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]");
                            insertStatement.AppendLine("( [SOURCE_NAME],[SATELLITE_NAME],[ATTRIBUTE_NAME_FROM],[ATTRIBUTE_NAME_TO],[MULTI_ACTIVE_KEY_INDICATOR], [MAPPING_TYPE])");
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
                                _alert.SetTextLogging("--> Processing the mapping from " + (string) row["SOURCE_NAME"] +
                                                      " - " + (string) row["ATTRIBUTE_NAME_FROM"] + " to " +
                                                      (string) row["SATELLITE_NAME"] + " - " +
                                                      (string) row["ATTRIBUTE_NAME_TO"] + ".\r\n");

                                manualSatMappingCounter++;

                            }
                            catch (Exception ex)
                            {
                                _alert.SetTextLogging("-----> An issue has occurred mapping columns from table " +
                                                      row["SOURCE_NAME"] + " to " + row["SATELLITE_NAME"] +
                                                      ". Please check the Error Log for more details.\r\n");
                                errorCounter++;
                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of the Source to Satellite attribute mapping: \r\n\r\n" +
                                    ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);

                                if (row["ATTRIBUTE_NAME_FROM"].ToString() == "")
                                {
                                    _alert.SetTextLogging("Both attributes are NULL.");
                                }
                            }
                        }
                    }
                }

                worker.ReportProgress(90);
                subProcess.Stop();
                _alert.SetTextLogging("--> Processing " + manualSatMappingCounter + " manual attribute mappings.\r\n");
                _alert.SetTextLogging(
                    "Preparation of the manual column-to-column mapping for Satellites and Link-Satellites completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Automatically mapped attributes for SAT and LSAT 93%

                //Prepare automatic attribute mapping
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");

                var prepareMappingStatement = new StringBuilder();

                if (radioButtonPhysicalMode.Checked)
                {
                    _alert.SetTextLogging(
                        "Commencing preparing the (automatic) column-to-column mapping metadata for Satellites and Link-Satellites, based on what's available in the database.\r\n");
                }
                else
                {
                    _alert.SetTextLogging(
                        "Commencing preparing the (automatic) column-to-column mapping metadata for Satellites and Link-Satellites, based on what's available in the physical model metadata.\r\n");
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
                    "JOIN dbo.MD_ATTRIBUTE stg_attr ON ADC_SOURCE.COLUMN_NAME = stg_attr.ATTRIBUTE_NAME COLLATE DATABASE_DEFAULT");
                prepareMappingStatement.AppendLine(
                    "JOIN dbo.MD_ATTRIBUTE tgt_attr ON ADC_TARGET.COLUMN_NAME = tgt_attr.ATTRIBUTE_NAME COLLATE DATABASE_DEFAULT");
                prepareMappingStatement.AppendLine(
                    "WHERE UPPER(stg_attr.ATTRIBUTE_NAME) = UPPER(tgt_attr.ATTRIBUTE_NAME)");
                prepareMappingStatement.AppendLine("AND NOT EXISTS (");
                prepareMappingStatement.AppendLine("  SELECT SOURCE_NAME, SATELLITE_NAME, ATTRIBUTE_NAME_FROM");
                prepareMappingStatement.AppendLine("  FROM MD_SOURCE_SATELLITE_ATTRIBUTE_XREF manualmapping");
                prepareMappingStatement.AppendLine("  WHERE");
                prepareMappingStatement.AppendLine("      manualmapping.SOURCE_NAME = XREF.SOURCE_NAME");
                prepareMappingStatement.AppendLine("  AND manualmapping.SATELLITE_NAME = XREF.TARGET_NAME");
                prepareMappingStatement.AppendLine("  AND manualmapping.ATTRIBUTE_NAME_FROM = ADC_SOURCE.COLUMN_NAME");
                prepareMappingStatement.AppendLine(")");
                prepareMappingStatement.AppendLine("ORDER BY SOURCE_NAME");

                var automaticAttributeMappingsSatellites =
                    Utility.GetDataTable(ref connOmd, prepareMappingStatement.ToString());

                if (automaticAttributeMappingsSatellites.Rows.Count == 0)
                {
                    _alert.SetTextLogging("--> No automatic column-to-column mappings were detected.\r\n");
                }
                else
                {
                    foreach (DataRow row in automaticAttributeMappingsSatellites.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            _alert.SetTextLogging("--> Processing the mapping from " + (string) row["SOURCE_NAME"] +
                                                  " - " + (string) row["ATTRIBUTE_NAME_FROM"] + " to " +
                                                  (string) row["SATELLITE_NAME"] + " - " +
                                                  (string) row["ATTRIBUTE_NAME_TO"] + ".\r\n");

                            var insertStatement = new StringBuilder();
                            insertStatement.AppendLine("INSERT INTO [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]");
                            insertStatement.AppendLine("( [SOURCE_NAME],[SATELLITE_NAME],[ATTRIBUTE_NAME_FROM],[ATTRIBUTE_NAME_TO],[MULTI_ACTIVE_KEY_INDICATOR], [MAPPING_TYPE])");
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
                                _alert.SetTextLogging("-----> An issue has occurred mapping columns from table " +
                                                      row["SOURCE_NAME"] + " to " + row["SATELLITE_NAME"] +
                                                      ". Please check the Error Log for more details.\r\n");
                                errorCounter++;
                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of the Source to Satellite attribute mapping: \r\n\r\n" +
                                    ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);

                                if (row["ATTRIBUTE_NAME_FROM"].ToString() == "")
                                {
                                    _alert.SetTextLogging("Both attributes are NULL.");
                                }
                            }
                        }
                    }
                }

                worker.ReportProgress(90);
                subProcess.Stop();
                _alert.SetTextLogging("--> Processing " + automaticAttributeMappingsSatellites.Rows.Count +
                                      " automatically added attribute mappings.\r\n");
                _alert.SetTextLogging(
                    "Preparation of the automatically mapped column-to-column metadata completed, and has taken " +
                    subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Manually mapped degenerate attributes for Links 95%

                //12. Prepare Manual Attribute mapping for Link degenerate fields
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging(
                    "Commencing preparing the column-to-column mapping metadata based on the manual mappings for degenerate attributes.\r\n");

                var prepareMappingStatementLink = new StringBuilder();

                prepareMappingStatementLink.AppendLine("SELECT");
                prepareMappingStatementLink.AppendLine("  stg.SOURCE_NAME");
                prepareMappingStatementLink.AppendLine(" ,lnk.LINK_NAME");
                prepareMappingStatementLink.AppendLine(" ,stg_attr.ATTRIBUTE_NAME AS ATTRIBUTE_NAME_FROM");
                prepareMappingStatementLink.AppendLine(" ,target_attr.ATTRIBUTE_NAME AS ATTRIBUTE_NAME_TO");
                prepareMappingStatementLink.AppendLine(" ,'Manual mapping' as MAPPING_TYPE");
                prepareMappingStatementLink.AppendLine("FROM dbo.TMP_MD_ATTRIBUTE_MAPPING mapping");
                prepareMappingStatementLink.AppendLine("LEFT OUTER JOIN dbo.MD_LINK lnk on lnk.[SCHEMA_NAME]+'.'+lnk.LINK_NAME=mapping.TARGET_TABLE");
                prepareMappingStatementLink.AppendLine("LEFT OUTER JOIN dbo.MD_ATTRIBUTE target_attr on mapping.TARGET_COLUMN = target_attr.ATTRIBUTE_NAME");
                prepareMappingStatementLink.AppendLine("LEFT OUTER JOIN dbo.MD_SOURCE stg on stg.[SCHEMA_NAME]+'.'+stg.SOURCE_NAME = mapping.SOURCE_TABLE");
                prepareMappingStatementLink.AppendLine("LEFT OUTER JOIN dbo.MD_ATTRIBUTE stg_attr on mapping.SOURCE_COLUMN = stg_attr.ATTRIBUTE_NAME");
                prepareMappingStatementLink.AppendLine("LEFT OUTER JOIN dbo.TMP_MD_TABLE_MAPPING table_mapping");
                prepareMappingStatementLink.AppendLine("  ON mapping.TARGET_TABLE = table_mapping.TARGET_TABLE");
                prepareMappingStatementLink.AppendLine(" AND mapping.SOURCE_TABLE = table_mapping.SOURCE_TABLE");
                prepareMappingStatementLink.AppendLine("WHERE mapping.TARGET_TABLE_TYPE = ('" +
                                                       ClassMetadataHandling.TableTypes.NaturalBusinessRelationship +
                                                       "')");
                prepareMappingStatementLink.AppendLine("      AND table_mapping.PROCESS_INDICATOR = 'Y'");

                var degenerateMappings = Utility.GetDataTable(ref connOmd, prepareMappingStatementLink.ToString());

                if (degenerateMappings.Rows.Count == 0)
                {
                    _alert.SetTextLogging("--> No manually mapped degenerate columns were detected.\r\n");
                }

                worker.ReportProgress(95);
                subProcess.Stop();
                _alert.SetTextLogging("--> Processing " + degenerateMappings.Rows.Count +
                                      " manual degenerate attribute mappings.\r\n");
                _alert.SetTextLogging("Preparation of the degenerate column metadata completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds + " seconds.\r\n");

                #endregion


                #region Automatically mapped degenerate attributes for Links 95%

                //13. Prepare the automatic degenerate attribute mapping
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
                prepareDegenerateMappingStatement.AppendLine(" ,[ORDINAL_POSITION]");
                prepareDegenerateMappingStatement.AppendLine(" ,[PRIMARY_KEY_INDICATOR]");
                prepareDegenerateMappingStatement.AppendLine("FROM [MD_PHYSICAL_MODEL]");

                prepareDegenerateMappingStatement.AppendLine("),");
                prepareDegenerateMappingStatement.AppendLine("XREF AS");
                prepareDegenerateMappingStatement.AppendLine("(");
                prepareDegenerateMappingStatement.AppendLine("  SELECT");
                prepareDegenerateMappingStatement.AppendLine("    src.[SCHEMA_NAME] AS SOURCE_SCHEMA_NAME,");
                prepareDegenerateMappingStatement.AppendLine("    xref.SOURCE_NAME AS SOURCE_NAME,");
                prepareDegenerateMappingStatement.AppendLine("    lnk.[SCHEMA_NAME] AS TARGET_SCHEMA_NAME,");
                prepareDegenerateMappingStatement.AppendLine("    xref.LINK_NAME AS TARGET_NAME");
                prepareDegenerateMappingStatement.AppendLine("  FROM MD_SOURCE_LINK_XREF xref");
                prepareDegenerateMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_SOURCE src ON xref.SOURCE_NAME = src.SOURCE_NAME");
                prepareDegenerateMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_LINK lnk ON xref.LINK_NAME = lnk.LINK_NAME");
                prepareDegenerateMappingStatement.AppendLine(") ");
                prepareDegenerateMappingStatement.AppendLine("SELECT");
                prepareDegenerateMappingStatement.AppendLine("  XREF.SOURCE_NAME, ");
                prepareDegenerateMappingStatement.AppendLine("  XREF.TARGET_NAME AS LINK_NAME,");
                prepareDegenerateMappingStatement.AppendLine("  ADC_SOURCE.COLUMN_NAME AS ATTRIBUTE_NAME_FROM,");
                prepareDegenerateMappingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_NAME_TO,");
                prepareDegenerateMappingStatement.AppendLine("  'N' AS MULTI_ACTIVE_INDICATOR,");
                prepareDegenerateMappingStatement.AppendLine("  'Automatic mapping' as MAPPING_TYPE");
                prepareDegenerateMappingStatement.AppendLine("FROM XREF");
                prepareDegenerateMappingStatement.AppendLine("JOIN ALL_DATABASE_COLUMNS ADC_SOURCE ON XREF.SOURCE_SCHEMA_NAME = ADC_SOURCE.[SCHEMA_NAME] AND XREF.SOURCE_NAME = ADC_SOURCE.TABLE_NAME");
                prepareDegenerateMappingStatement.AppendLine("JOIN ALL_DATABASE_COLUMNS ADC_TARGET ON XREF.TARGET_SCHEMA_NAME = ADC_TARGET.[SCHEMA_NAME] AND XREF.TARGET_NAME = ADC_TARGET.TABLE_NAME");
                prepareDegenerateMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_ATTRIBUTE stg_attr ON UPPER(ADC_SOURCE.COLUMN_NAME) = UPPER(stg_attr.ATTRIBUTE_NAME) COLLATE DATABASE_DEFAULT");
                prepareDegenerateMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_ATTRIBUTE tgt_attr ON UPPER(ADC_TARGET.COLUMN_NAME) = UPPER(tgt_attr.ATTRIBUTE_NAME) COLLATE DATABASE_DEFAULT");
                prepareDegenerateMappingStatement.AppendLine("WHERE stg_attr.ATTRIBUTE_NAME = tgt_attr.ATTRIBUTE_NAME");


                if (radioButtonPhysicalMode.Checked)
                {
                    _alert.SetTextLogging(
                        "Commencing preparing the (automatic) column-to-column mapping metadata for degenerate attributes, based on what's available in the database.\r\n");
                }
                else
                {
                    _alert.SetTextLogging(
                        "Commencing preparing the degenerate column metadata using the physical model metadata.\r\n");
                }

                var automaticDegenerateMappings =
                    Utility.GetDataTable(ref connOmd, prepareDegenerateMappingStatement.ToString());

                if (automaticDegenerateMappings.Rows.Count == 0)
                {
                    _alert.SetTextLogging("--> No automatic degenerate columns were detected.\r\n");
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
                            insertStatement.AppendLine("( [SOURCE_NAME],[LINK_NAME],[ATTRIBUTE_NAME_FROM],[ATTRIBUTE_NAME_TO], [MAPPING_TYPE])");
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
                            catch (Exception)
                            {
                                _alert.SetTextLogging(
                                    "-----> An issue has occurred mapping degenerate columns from table " +
                                    tableName["SOURCE_NAME"] + " to " + tableName["LINK_NAME"] +
                                    ". Please check the Error Log for more details.\r\n");
                                errorCounter++;
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                                if (tableName["ATTRIBUTE_NAME_FROM"].ToString() == "")
                                {
                                    _alert.SetTextLogging("Both attributes are NULL.");
                                }
                            }
                        }
                    }
                }

                worker.ReportProgress(95);
                subProcess.Stop();
                _alert.SetTextLogging("--> Processing " + automaticDegenerateMappingCounter +
                                      " automatically added degenerate attribute mappings.\r\n");
                _alert.SetTextLogging("Preparation of the degenerate column metadata completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds.ToString() + " seconds.\r\n");

                #endregion


                #region Multi-Active Key - 97%

                //Handle the Multi-Active Key
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");


                var prepareMultiKeyStatement = new StringBuilder();

                if (radioButtonPhysicalMode.Checked)
                {
                    _alert.SetTextLogging("Commencing Multi-Active Key handling using database.\r\n");

                    prepareMultiKeyStatement.AppendLine("SELECT");
                    prepareMultiKeyStatement.AppendLine("   xref.SOURCE_NAME");
                    prepareMultiKeyStatement.AppendLine("  ,xref.SATELLITE_NAME");
                    prepareMultiKeyStatement.AppendLine("  ,xref.ATTRIBUTE_NAME_FROM");
                    prepareMultiKeyStatement.AppendLine("  ,xref.ATTRIBUTE_NAME_TO");
                    prepareMultiKeyStatement.AppendLine("FROM MD_SOURCE_SATELLITE_ATTRIBUTE_XREF xref");
                    prepareMultiKeyStatement.AppendLine("INNER JOIN ");
                    prepareMultiKeyStatement.AppendLine("(");
                    prepareMultiKeyStatement.AppendLine("  SELECT ");
                    prepareMultiKeyStatement.AppendLine("  	sc.name AS SATELLITE_NAME,");
                    prepareMultiKeyStatement.AppendLine("  	C.name AS ATTRIBUTE_NAME");
                    prepareMultiKeyStatement.AppendLine("  FROM " + linkedServer + integrationDatabase +
                                                        ".sys.index_columns A");
                    prepareMultiKeyStatement.AppendLine("  JOIN " + linkedServer + integrationDatabase +
                                                        ".sys.indexes B");
                    prepareMultiKeyStatement.AppendLine("    ON A.OBJECT_ID=B.OBJECT_ID AND A.index_id=B.index_id");
                    prepareMultiKeyStatement.AppendLine("  JOIN " + linkedServer + integrationDatabase +
                                                        ".sys.columns C");
                    prepareMultiKeyStatement.AppendLine("    ON A.column_id=C.column_id AND A.OBJECT_ID=C.OBJECT_ID");
                    prepareMultiKeyStatement.AppendLine("  JOIN " + linkedServer + integrationDatabase +
                                                        ".sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
                    prepareMultiKeyStatement.AppendLine("    WHERE is_primary_key=1");
                    prepareMultiKeyStatement.AppendLine("  AND C.name!='" + effectiveDateTimeAttribute +
                                                        "' AND C.name!='" + currentRecordAttribute + "' AND C.name!='" +
                                                        eventDateTimeAtttribute + "'");
                    prepareMultiKeyStatement.AppendLine("  AND C.name NOT LIKE '" + dwhKeyIdentifier + "'");
                    prepareMultiKeyStatement.AppendLine(") ddsub");
                    prepareMultiKeyStatement.AppendLine(
                        "ON xref.SATELLITE_NAME = ddsub.SATELLITE_NAME COLLATE DATABASE_DEFAULT");
                    prepareMultiKeyStatement.AppendLine(
                        "AND xref.ATTRIBUTE_NAME_TO = ddsub.ATTRIBUTE_NAME COLLATE DATABASE_DEFAULT");
                    prepareMultiKeyStatement.AppendLine("  WHERE ddsub.SATELLITE_NAME LIKE '" + satTablePrefix +
                                                        "' OR ddsub.SATELLITE_NAME LIKE '" + lsatTablePrefix + "'");
                }
                else
                {
                    _alert.SetTextLogging("Commencing Multi-Active Key handling using model metadata.\r\n");

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
                    _alert.SetTextLogging("--> No Multi-Active Keys were detected.\r\n");
                }
                else
                {
                    foreach (DataRow tableName in listMultiKeys.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            _alert.SetTextLogging("--> Processing the Multi-Active Key attribute " +
                                                  tableName["ATTRIBUTE_NAME_TO"] + " for " +
                                                  tableName["SATELLITE_NAME"] + ".\r\n");

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
                                errorCounter++;
                                _alert.SetTextLogging(
                                    "An issue has occured during preparation of the Multi-Active key metadata. Please check the Error Log for more details.\r\n");

                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of the Multi-Active key metadata: \r\n\r\n" +
                                    ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                            }
                        }
                    }
                }

                worker.ReportProgress(97);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the Multi-Active Keys completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds.ToString() + " seconds.\r\n");


                #endregion


                #region Driving Key preparation

                //Prepare driving keys
                subProcess.Reset();
                subProcess.Start();

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Driving Key metadata.\r\n");


                var prepareDrivingKeyStatement = new StringBuilder();
                prepareDrivingKeyStatement.AppendLine("SELECT DISTINCT");
                prepareDrivingKeyStatement.AppendLine("         sat.SATELLITE_NAME");
                prepareDrivingKeyStatement.AppendLine(
                    "         ,COALESCE(hubkey.HUB_NAME, (SELECT HUB_NAME FROM MD_HUB WHERE HUB_NAME = 'Not applicable')) AS HUB_NAME");
                prepareDrivingKeyStatement.AppendLine(" FROM");
                prepareDrivingKeyStatement.AppendLine(" (");
                prepareDrivingKeyStatement.AppendLine("         SELECT");
                prepareDrivingKeyStatement.AppendLine("                 SOURCE_TABLE,");
                prepareDrivingKeyStatement.AppendLine("                 TARGET_TABLE,");
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
                    "                 SELECT SOURCE_TABLE, TARGET_TABLE, DRIVING_KEY_ATTRIBUTE, VERSION_ID, CONVERT(XML, '<M>' + REPLACE(DRIVING_KEY_ATTRIBUTE, ',', '</M><M>') + '</M>') AS DRIVING_KEY_ATTRIBUTE_XML");
                prepareDrivingKeyStatement.AppendLine("                 FROM");
                prepareDrivingKeyStatement.AppendLine("                 (");
                prepareDrivingKeyStatement.AppendLine(
                    "                         SELECT DISTINCT SOURCE_TABLE, TARGET_TABLE, VERSION_ID, LTRIM(RTRIM(DRIVING_KEY_ATTRIBUTE)) AS DRIVING_KEY_ATTRIBUTE");
                prepareDrivingKeyStatement.AppendLine("                         FROM TMP_MD_TABLE_MAPPING");
                prepareDrivingKeyStatement.AppendLine("                         WHERE TARGET_TABLE_TYPE IN ('" +
                                                      ClassMetadataHandling.TableTypes
                                                          .NaturalBusinessRelationshipContext +
                                                      "') AND DRIVING_KEY_ATTRIBUTE IS NOT NULL AND DRIVING_KEY_ATTRIBUTE != ''");
                prepareDrivingKeyStatement.AppendLine("                         AND [PROCESS_INDICATOR] = 'Y'");
                prepareDrivingKeyStatement.AppendLine("                 ) TableName");
                prepareDrivingKeyStatement.AppendLine(
                    "         ) AS A CROSS APPLY DRIVING_KEY_ATTRIBUTE_XML.nodes('/M') AS Split(a)");
                prepareDrivingKeyStatement.AppendLine(" )  base");
                prepareDrivingKeyStatement.AppendLine(" LEFT JOIN [dbo].[TMP_MD_TABLE_MAPPING]");
                prepareDrivingKeyStatement.AppendLine("         hub");
                prepareDrivingKeyStatement.AppendLine("     ON  base.SOURCE_TABLE=hub.SOURCE_TABLE");
                prepareDrivingKeyStatement.AppendLine("     AND hub.TARGET_TABLE_TYPE IN ('" +
                                                      ClassMetadataHandling.TableTypes.CoreBusinessConcept + "')");
                prepareDrivingKeyStatement.AppendLine(
                    "     AND base.BUSINESS_KEY_ATTRIBUTE=hub.BUSINESS_KEY_ATTRIBUTE");
                prepareDrivingKeyStatement.AppendLine(" LEFT JOIN MD_SATELLITE sat");
                prepareDrivingKeyStatement.AppendLine(
                    "     ON base.TARGET_TABLE = sat.[SCHEMA_NAME]+'.'+sat.SATELLITE_NAME");
                prepareDrivingKeyStatement.AppendLine(" LEFT JOIN MD_HUB hubkey");
                prepareDrivingKeyStatement.AppendLine(
                    "     ON hub.TARGET_TABLE = hubkey.[SCHEMA_NAME]+'.'+hubkey.HUB_NAME");
                prepareDrivingKeyStatement.AppendLine(" WHERE 1=1");
                prepareDrivingKeyStatement.AppendLine(" AND base.BUSINESS_KEY_ATTRIBUTE IS NOT NULL");
                prepareDrivingKeyStatement.AppendLine(" AND base.BUSINESS_KEY_ATTRIBUTE!=''");
                prepareDrivingKeyStatement.AppendLine(" AND [PROCESS_INDICATOR] = 'Y'");


                var listDrivingKeys = Utility.GetDataTable(ref connOmd, prepareDrivingKeyStatement.ToString());

                if (listDrivingKeys.Rows.Count == 0)
                {
                    _alert.SetTextLogging("--> No Driving Key based Link-Satellites were detected.\r\n");
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
                                errorCounter++;
                                _alert.SetTextLogging(
                                    "An issue has occured during preparation of the Driving Key metadata. Please check the Error Log for more details.\r\n");

                                errorLog.AppendLine(
                                    "\r\nAn issue has occured during preparation of the Driving Key metadata: \r\n\r\n" +
                                    ex);
                                errorLog.AppendLine(
                                    "\r\nThe query that caused the issue is: \r\n\r\n" + insertStatement);
                            }
                        }
                    }
                }

                worker.ReportProgress(98);
                subProcess.Stop();
                _alert.SetTextLogging("Preparation of the Driving Key column metadata completed, and has taken " +
                                      subProcess.Elapsed.TotalSeconds.ToString() + " seconds.\r\n");

                #endregion

                //
                // Activation completed!
                //

                // Report the events (including errors) back to the user
                // Clear out the existing error log, or create an empty new file
                using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + @"\Event_Log.txt"))
                {
                    outfile.Write(String.Empty);
                    outfile.Close();
                }

                int eventErrorCounter = 0;
                StringBuilder logOutput = new StringBuilder();
                foreach (Event individualEvent in eventLog)
                {
                    if (individualEvent.eventCode == (int)EventTypes.Error)
                    {
                        eventErrorCounter++;
                    }

                    logOutput.AppendLine((EventTypes)individualEvent.eventCode + ": " + individualEvent.eventDescription);
                }

                using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + @"\Event_Log.txt"))
                {
                    outfile.Write(logOutput.ToString());
                    outfile.Close();
                }

                // Error handling
                // Clear out the existing error log, or create an empty new file
                using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + @"\Error_Log.txt"))
                {
                    outfile.Write(String.Empty);
                    outfile.Close();
                }

                // Write any errors
                if (errorCounter > 0)
                {
                    _alert.SetTextLogging("\r\nWarning! There were " + errorCounter +
                                          " error(s) found while processing the metadata.\r\n");
                    _alert.SetTextLogging("Please check the Error Log for details \r\n");
                    _alert.SetTextLogging("\r\n");

                    using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + @"\Error_Log.txt"))
                    {
                        outfile.Write(errorLog.ToString());
                        outfile.Close();
                    }
                }
                else
                {
                    _alert.SetTextLogging("\r\nNo errors were detected.\r\n");
                }

                // Remove the temporary tables that have been used
                droptemporaryWorkerTable(ConfigurationSettings.ConnectionStringOmd);

                // Report completion
                totalProcess.Stop();
                _alert.SetTextLogging("\r\n\r\nThe full activation process has taken "+totalProcess.Elapsed.TotalSeconds+" seconds.");
                worker.ReportProgress(100);
            }
        }




        private void DataGridViewTableMetadataKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch(e.KeyCode)
                    {
                        case Keys.V:
                            PasteClipboardTableMetadata();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Pasting into the data grid has failed", "Copy/Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

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
                MessageBox.Show("Pasting into the data grid has failed", "Copy/Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            catch (FormatException)
            {
                MessageBox.Show("There is an issue with the data formate for this cell!");
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
            catch (FormatException)
            {
                MessageBox.Show("There is an issue with the data format for this cell!");
            }
        }

        private void FormManageMetadata_SizeChanged(object sender, EventArgs e)
        {
            GridAutoLayout();
        }

        private void dataGridViewTableMetadata_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Validate the data entry on the Table Mapping datagrid
            var valueLength = e.FormattedValue.ToString().Length;

            // Source Table (Source)
            if (e.ColumnIndex == 2)
            {
                dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "";

                if (e.FormattedValue == DBNull.Value || valueLength == 0)
                {
                    e.Cancel = true;
                    dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "The Source (Source) table cannot be empty!";
                }
            }

            // Target Table
            if (e.ColumnIndex == 3)
            {
                dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "";

                if (e.FormattedValue == DBNull.Value || valueLength == 0)
                {
                    e.Cancel = true;
                    dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "The Target (Integration Layer) table cannot be empty!";
                }
            }

            // Business Key
            if (e.ColumnIndex == 4)
            {
                dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "";

                if (e.FormattedValue==DBNull.Value || valueLength == 0)
                {
                    e.Cancel = true;
                    dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "The Business Key cannot be empty!";
                }
            }

            // Filter criteria
            if (e.ColumnIndex == 6) 
            {
                dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "";
                //int newInteger;
                var equalSignIndex = e.FormattedValue.ToString().IndexOf('=')+1;

                if (valueLength>0 && valueLength < 3)
                {
                    e.Cancel = true;
                    dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "The filter criterion cannot only be just one or two characters as it translates into a WHERE clause.";
                }

                if (valueLength > 0)
                {
                    //Check if an '=' is there
                    if (e.FormattedValue.ToString()=="=")
                    {
                        e.Cancel = true;
                        dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "The filter criterion cannot only be '=' as it translates into a WHERE clause.";
                    }

                    // If there are value in the filter, and the filter contains an equal sign but it's the last then cancel
                    if (valueLength > 2 && (e.FormattedValue.ToString().Contains("=") && !(equalSignIndex < valueLength)))
                    {
                        e.Cancel = true;
                        dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "The filter criterion include values either side of the '=' sign as it is expressed as a WHERE clause.";
                    }
                }
            }
        }

        public DateTime ActivationMetadata()
        {
            DateTime mostRecentActivationDateTime = DateTime.MinValue; 

            var connOmd = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };

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
                richTextBoxInformation.Text = $"DGML will be generated following the most recent activation metadata, as per ({activationDateTime}).";
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

                var errorLog = new StringBuilder();
                var errorCounter = 0;

                if (dataGridViewTableMetadata != null) // There needs to be metadata available
                {
                    var connOmd = new SqlConnection {ConnectionString = ConfigurationSettings.ConnectionStringOmd};

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
                        string sourceNode = row.Cells[2].Value.ToString();
                        string targetNode = row.Cells[3].Value.ToString();

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
                    }

                    dgmlExtract.AppendLine("  <Nodes>");

                    var edgeBuilder = new StringBuilder(); // Also create the links while iterating through the below set
                    
                    foreach (string node in nodeList)
                    {
                        if (node.Contains(ConfigurationSettings.StgTablePrefixValue))
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Landing Area\" Group=\"Collapsed\" Label=\"" + node + "\" />");
                            edgeBuilder.AppendLine("     <Link Source=\"Staging Layer\" Target=\"" + node + "\" Category=\"Contains\" />");
                        }
                        else if (node.Contains(ConfigurationSettings.PsaTablePrefixValue))
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Persistent Staging Area\" Group=\"Collapsed\" Label=\"" + node + "\" />");
                            edgeBuilder.AppendLine("     <Link Source=\"Staging Layer\" Target=\"" + node + "\" Category=\"Contains\" />");
                        }
                        else if (node.Contains(ConfigurationSettings.HubTablePrefixValue))
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Hub\"  Label=\"" + node + "\" />");
                            //edgeBuilder.AppendLine("     <Link Source=\"Data Vault\" Target=\"" + node + "\" Category=\"Contains\" />");
                        }
                        else if (node.Contains(ConfigurationSettings.LinkTablePrefixValue))
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Link\" Label=\"" +node + "\" />");
                            //edgeBuilder.AppendLine("     <Link Source=\"Data Vault\" Target=\"" + node + "\" Category=\"Contains\" />");
                        }
                        else if (node.Contains(ConfigurationSettings.SatTablePrefixValue) || node.Contains(ConfigurationSettings.LsatTablePrefixValue))
                        {
                            dgmlExtract.AppendLine("     <Node Id=\"" + node +"\"  Category=\"Satellite\" Group=\"Collapsed\" Label=\"" +node + "\" />");
                            //edgeBuilder.AppendLine("     <Link Source=\"Data Vault\" Target=\"" + node + "\" Category=\"Contains\" />");
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
                    sqlStatementForSatelliteAttributes.AppendLine("SELECT *");
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

                        var modelRelationshipsLinksDataTable = Utility.GetDataTable(ref connOmd, sqlStatementForSubjectAreas.ToString());

                        foreach (DataRow row in modelRelationshipsLinksDataTable.Rows)
                        {
                            //dgmlExtract.AppendLine("     <Link Source=\"" + (string)row["BUSINESS_CONCEPT"] + "\" Target=\"" + (string)row["CONTEXT_TABLE"] + "\" />");
                            dgmlExtract.AppendLine("     <Node Id=\"SubjectArea_" + (string)row["SUBJECT_AREA"] + "\"  Group=\"Collapsed\" Category=\"Subject Area\" Label=\"" + (string)row["SUBJECT_AREA"] + "\" />");
                            edgeBuilder.AppendLine("     <Link Source=\"Data Vault\" Target=\"SubjectArea_" + (string)row["SUBJECT_AREA"] + "\" Category=\"Contains\" />");
                        }
                    }
                    catch (Exception)
                    {
                        errorCounter++;
                        errorLog.AppendLine("The following query caused an issue when generating the DGML file: " + sqlStatementForSubjectAreas);
                    }
                    #endregion

                    dgmlExtract.AppendLine("  </Nodes>");
                    //End of Nodes


                    //Edges and containers
                    dgmlExtract.AppendLine("  <Links>");
                    dgmlExtract.AppendLine("     <!-- Place regular nodes in layer containers ('contains') -->");
                    dgmlExtract.Append(edgeBuilder); // Add the containers (e.g. STG and PSA to Staging Layer, Hubs, Links and Satellites to Data Vault


                    // Separate routine to create table / attribute relationships
                    dgmlExtract.AppendLine("     <!-- Table / Attribute relationships -->");
                    foreach (DataRow row in satelliteAttributes.Rows)
                    {
                        var sourceNodeSat = (string) row["TARGET_NAME"];
                        var targetNodeSat = "dwh_" + (string) row["TARGET_ATTRIBUTE_NAME"];
                        var sourceNodeStg = (string) row["SOURCE_NAME"];
                        var targetNodeStg = "staging_" + (string) row["SOURCE_ATTRIBUTE_NAME"];

                        // This is adding the attributes to the tables
                        dgmlExtract.AppendLine("     <Link Source=\"" + sourceNodeSat + "\" Target=\"" +targetNodeSat + "\" Category=\"Contains\" />");
                        dgmlExtract.AppendLine("     <Link Source=\"" + sourceNodeStg + "\" Target=\"" +targetNodeStg + "\" Category=\"Contains\" />");

                        // This is adding the edge between the attributes
                        dgmlExtract.AppendLine("     <Link Source=\"" + targetNodeStg + "\" Target=\"" +targetNodeSat + "\" />");
                    }

                    // Get the source / target model relationships for Hubs and Satellites
                    List<string> segmentNodeList = new List<string>();
                    var modelRelationshipsHubDataTable = new DataTable();
                    var sqlStatementForHubCategories = new StringBuilder();
                    try
                    {

                        sqlStatementForHubCategories.AppendLine("SELECT *");
                        sqlStatementForHubCategories.AppendLine("FROM [interface].[INTERFACE_SOURCE_SATELLITE_XREF]");
                        sqlStatementForHubCategories.AppendLine("WHERE TARGET_TYPE = 'Normal'");

                        modelRelationshipsHubDataTable = Utility.GetDataTable(ref connOmd, sqlStatementForHubCategories.ToString());
                    }
                    catch
                    {
                        errorCounter++;
                        errorLog.AppendLine("The following query caused an issue when generating the DGML file: " + sqlStatementForHubCategories);
                    }

                    foreach (DataRow row in modelRelationshipsHubDataTable.Rows)
                    {
                        var modelRelationshipsHub = (string)row["TARGET_NAME"];

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
                        sqlStatementForRelationships.AppendLine("SELECT DISTINCT [HUB_NAME], [TARGET_NAME]");
                        sqlStatementForRelationships.AppendLine("FROM [interface].[INTERFACE_HUB_LINK_XREF]");
                        sqlStatementForRelationships.AppendLine("WHERE HUB_NAME NOT IN ('N/A')");

                        var businessConceptsRelationships = Utility.GetDataTable(ref connOmd, sqlStatementForRelationships.ToString());

                        foreach (DataRow row in businessConceptsRelationships.Rows)
                        {
                            dgmlExtract.AppendLine("     <Link Source=\"" + (string)row["HUB_NAME"] + "\" Target=\"" + (string)row["TARGET_NAME"] + "\" />");
                        }
                    }
                    catch
                    {
                        errorCounter++;
                        errorLog.AppendLine("The following query caused an issue when generating the DGML file: " + sqlStatementForRelationships);
                    }


                    // Add the relationships to the context tables
                    dgmlExtract.AppendLine("     <!-- Relationships between Hubs/Links to context and their subject area -->");
                    var sqlStatementForLinkCategories = new StringBuilder();
                    try
                    {
                        sqlStatementForLinkCategories.AppendLine("SELECT *");
                        sqlStatementForLinkCategories.AppendLine("FROM [interface].[INTERFACE_SUBJECT_AREA]");

                        var modelRelationshipsLinksDataTable = Utility.GetDataTable(ref connOmd, sqlStatementForLinkCategories.ToString());

                        foreach (DataRow row in modelRelationshipsLinksDataTable.Rows)
                        {
                            var businessConcept = (string) row["BUSINESS_CONCEPT"];

                            var contextTable = Utility.ConvertFromDBVal<string>(row["CONTEXT_TABLE"]);

                            dgmlExtract.AppendLine("     <Link Source=\"" + businessConcept + "\" Target=\"" + contextTable + "\" />");

                            dgmlExtract.AppendLine("     <Link Source=\"SubjectArea_" + (string)row["SUBJECT_AREA"] + "\" Target=\"" + businessConcept + "\" Category=\"Contains\" />");

                            if (contextTable != null)
                            {
                                dgmlExtract.AppendLine("     <Link Source=\"SubjectArea_" + (string) row["SUBJECT_AREA"] + "\" Target=\"" + contextTable + "\" Category=\"Contains\" />");
                            }
                        }

                    }
                    catch (Exception)
                    {
                        errorCounter++;
                        errorLog.AppendLine("The following query caused an issue when generating the DGML file: " + sqlStatementForLinkCategories);
                    }


                    // Add the regular source-to-target mappings as edges using the datagrid
                    dgmlExtract.AppendLine("     <!-- Regular source-to-target mappings -->");
                    for (var i = 0; i < dataGridViewTableMetadata.Rows.Count - 1; i++)
                    {
                        var row = dataGridViewTableMetadata.Rows[i];
                        var sourceNode = row.Cells[2].Value.ToString();
                        var targetNode = row.Cells[3].Value.ToString();
                        var businessKey = row.Cells[4].Value.ToString();

                        dgmlExtract.AppendLine("     <Link Source=\"" + sourceNode + "\" Target=\"" + targetNode + "\" BusinessKeyDefinition=\"" + businessKey + "\"/>");
                    }

                    dgmlExtract.AppendLine("  </Links>");
                    // End of edges and containers


                    //Add categories
                    dgmlExtract.AppendLine("  <Categories>");
                    dgmlExtract.AppendLine("    <Category Id = \"Sources\" Label = \"Sources\" Background = \"#FFE51400\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine("    <Category Id = \"Landing Area\" Label = \"Landing Area\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine("    <Category Id = \"Persistent Staging Area\" Label = \"Persistent Staging Area\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine("    <Category Id = \"Hub\" Label = \"Hub\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine("    <Category Id = \"Link\" Label = \"Link\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine("    <Category Id = \"Satellite\" Label = \"Satellite\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine("    <Category Id = \"Subject Area\" Label = \"Subject Area\" IsTag = \"True\" /> ");
                    dgmlExtract.AppendLine("  </Categories>");

                    //Add category styles 
                    dgmlExtract.AppendLine("  <Styles >");

                    dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Sources\" ValueLabel = \"Has category\" >");
                    dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Sources')\" />");
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

                    dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Hub\" ValueLabel = \"Has category\" >");
                    dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Hub')\" />");
                    dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                    dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FF6495ED\" />");
                    dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                    dgmlExtract.AppendLine("    </Style >");

                    dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Link\" ValueLabel = \"Has category\" >");
                    dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Link')\" />");
                    dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                    dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FFB22222\" />");
                    dgmlExtract.AppendLine("      <Setter Property = \"Icon\" Value = \"pack://application:,,,/Microsoft.VisualStudio.Progression.GraphControl;component/Icons/Table.png\" />");
                    dgmlExtract.AppendLine("    </Style >");

                    dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Satellite\" ValueLabel = \"Has category\" >");
                    dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Satellite')\" />");
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
                    // End of graph file creation


                    // Error handling
                    if (errorCounter > 0)
                    {
                        richTextBoxInformation.AppendText("\r\nWarning! There were " + errorCounter +
                                                          " error(s) found while generating the DGML file.\r\n");
                        richTextBoxInformation.AppendText("Please check the Error Log for details \r\n");
                        richTextBoxInformation.AppendText("\r\n");

                        using (var outfile =
                            new StreamWriter(GlobalParameters.ConfigurationPath + @"\Error_Log.txt"))
                        {
                            outfile.Write(errorLog.ToString());
                            outfile.Close();
                        }
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
                if (dr.Cells[3].Value != null)
                {
                    if (!dr.Cells[3].Value.ToString().Contains(textBoxFilterCriterion.Text) && !dr.Cells[2].Value.ToString().Contains(textBoxFilterCriterion.Text))
                    {
                        CurrencyManager currencyManager1 = (CurrencyManager)BindingContext[dataGridViewTableMetadata.DataSource];
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
                    Title = @"Save Business Key Metadata File",
                    Filter = @"JSON files|*.json",
                    InitialDirectory =  GlobalParameters.ConfigurationPath //Application.StartupPath + @"\Configuration\"
                };

                var ret = STAShowDialog(theDialog);

                if (ret == DialogResult.OK)
                {
                    try
                    {
                        var chosenFile = theDialog.FileName;

                        DataTable gridDataTable = (DataTable)_bindingSourceTableMetadata.DataSource;

                        // Make sure the output is sorted
                        gridDataTable.DefaultView.Sort = "[SOURCE_TABLE] ASC, [TARGET_TABLE] ASC, [BUSINESS_KEY_ATTRIBUTE] ASC";

                        gridDataTable.TableName = "TableMappingMetadata";

                        JArray outputFileArray = new JArray();
                        foreach (DataRow singleRow in gridDataTable.DefaultView.ToTable().Rows)
                        {
                            JObject individualRow = JObject.FromObject(new
                            {
                                tableMappingHash = singleRow[0].ToString(),
                                versionId = singleRow[1].ToString(),
                                sourceTable = singleRow[2].ToString(),
                                targetTable = singleRow[3].ToString(),
                                businessKeyDefinition = singleRow[4].ToString(),
                                drivingKeyDefinition = singleRow[5].ToString(),
                                filterCriteria = singleRow[6].ToString(),
                                processIndicator = singleRow[7].ToString()
                            });
                            outputFileArray.Add(individualRow);
                        }

                        string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

                        File.WriteAllText(chosenFile, json);

                        richTextBoxInformation.Text = "The Business Key metadata file " + chosenFile + " saved successfully.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A problem occured when attempting to save the file to disk. The detail error message is: " + ex.Message);
            }
        }

        /// <summary>
        /// Run the validation based on the validation settings (in the validation form / file)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonValidation_Click(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();

            if (radioButtonPhysicalMode.Checked == false && _bindingSourcePhysicalModelMetadata.Count == 0)
            {
                richTextBoxInformation.Text += "There is no physical model metadata available, so the metadata can only be validated with the 'Ignore Version' enabled.\r\n ";
            }
            else
            {
                if (backgroundWorkerValidationOnly.IsBusy) return;
                // create a new instance of the alert form
                _alertValidation = new Form_Alert();
                // event handler for the Cancel button in AlertForm
                _alertValidation.Canceled += buttonCancel_Click;
                _alertValidation.Show();
                // Start the asynchronous operation.
                backgroundWorkerValidationOnly.RunWorkerAsync();
            }
        }

        private void openOutputDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(GlobalParameters.OutputPath);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occured while attempting to open the output directory. The error message is: " + ex;
            }
        }

        private void saveAttributeMappingAsJSONToolStripMenuItem_Click(object sender, EventArgs e)
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

                        DataTable gridDataTable = (DataTable)_bindingSourceAttributeMetadata.DataSource;

                        // Make sure the output is sorted
                        gridDataTable.DefaultView.Sort = "[SOURCE_TABLE] ASC, [SOURCE_COLUMN] ASC, [TARGET_TABLE] ASC, [TARGET_COLUMN] ASC";

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
                                transformationRule = singleRow[6].ToString()
                            });
                            outputFileArray.Add(individualRow);
                        }

                        string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

                        File.WriteAllText(chosenFile, json);

                        richTextBoxInformation.Text = "The Attribute Mapping metadata file " + chosenFile + " saved successfully.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A problem occured when attempting to save the file to disk. The detail error message is: " + ex.Message);
            }
        }

        private void saveModelMetadataFileAsJSONToolStripMenuItem_Click(object sender, EventArgs e)
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

                        DataTable gridDataTable = (DataTable)_bindingSourcePhysicalModelMetadata.DataSource;

                        gridDataTable.DefaultView.Sort = "[DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME] ASC, [ORDINAL_POSITION] ASC";

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
                                characterMaximumLength = singleRow[7].ToString(),
                                numericPrecision = singleRow[8].ToString(),
                                ordinalPosition = singleRow[9].ToString(),
                                primaryKeyIndicator = singleRow[10].ToString(),
                                multiActiveIndicator = singleRow[11].ToString()
                            });
                            outputFileArray.Add(individualRow);
                        }

                        string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

                        File.WriteAllText(chosenFile, json);

                        richTextBoxInformation.Text = "The model metadata file " + chosenFile + " saved successfully.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A problem occured when attempting to save the file to disk. The detail error message is: " + ex.Message);
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

            // Populate table / attribute version table
            var intDatabase = ConfigurationSettings.IntegrationDatabaseName;
            var stgDatabase = ConfigurationSettings.StagingDatabaseName;
            var psaDatabase = ConfigurationSettings.PsaDatabaseName;
            var presDatabase = ConfigurationSettings.PresentationDatabaseName;

            var connStg = new SqlConnection {ConnectionString = ConfigurationSettings.ConnectionStringStg};
            var connPsa = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringHstg };
            var connInt = new SqlConnection {ConnectionString = ConfigurationSettings.ConnectionStringInt};
            var connPres = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringPres };

            var stgPrefix = ConfigurationSettings.StgTablePrefixValue;
            var psaPrefix = ConfigurationSettings.PsaTablePrefixValue;

            // Process changes
            var stagingReverseEngineerResults = new DataTable();
            if (checkBoxStagingArea.Checked)
            {
                stagingReverseEngineerResults = ReverseEngineerModelMetadata(connStg, stgPrefix, stgDatabase); 
            }

            var psaReverseEngineerResults = new DataTable();
            if (checkBoxPsa.Checked)
            {
                psaReverseEngineerResults = ReverseEngineerModelMetadata(connPsa, psaPrefix, psaDatabase);
            }

            var integrationReverseEngineerResults = new DataTable();
            if (checkBoxIntegrationLayer.Checked)
            {
                integrationReverseEngineerResults = ReverseEngineerModelMetadata(connInt, @"", intDatabase);
            }

            var presentationReverseEngineerResults = new DataTable();
            if (checkBoxPresentationLayer.Checked)
            {
                presentationReverseEngineerResults = ReverseEngineerModelMetadata(connPres, @"", presDatabase);
            }

            // Merge the data tables
            var completeDataTable = new DataTable();

            if (stagingReverseEngineerResults != null)
            {
                completeDataTable.Merge(stagingReverseEngineerResults);
            }

            if (integrationReverseEngineerResults != null)
            {
                completeDataTable.Merge(integrationReverseEngineerResults);
            }

            if (psaReverseEngineerResults != null)
            {
                completeDataTable.Merge(psaReverseEngineerResults);
            }

            if (presentationReverseEngineerResults != null)
            {
                completeDataTable.Merge(presentationReverseEngineerResults);
            }

            DataTable distinctTable = completeDataTable.DefaultView.ToTable( /*distinct*/ true);

            distinctTable.DefaultView.Sort = "[DATABASE_NAME] ASC, [SCHEMA_NAME] ASC, [TABLE_NAME] ASC, [ORDINAL_POSITION] ASC";

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
            dataGridViewPhysicalModelMetadata.Columns[9].HeaderText = "Position";
            dataGridViewPhysicalModelMetadata.Columns[10].HeaderText = "Primary Key";
            dataGridViewPhysicalModelMetadata.Columns[11].HeaderText = "Multi-Active";

            foreach (DataRow row in completeDataTable.Rows) //Flag as new row so it's detected by the save button
            {
                row.SetAdded();
            }
        }


        /// <summary>
        ///   Connect to a given database and return the data dictionary (catalog) information in the datagrid.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="prefix"></param>
        /// <param name="databaseName"></param>
        private DataTable ReverseEngineerModelMetadata(SqlConnection conn, string prefix, string databaseName)
        {
            try
            {
                conn.Open();
            }
            catch (Exception exception)
            {
                richTextBoxInformation.Text += "An error has occurred uploading the model for the new version because the database could not be connected to. The error message is: " + exception.Message + ".\r\n";
            }

            // Get everything as local variables to reduce multi-threading issues
            var effectiveDateTimeAttribute = ConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute == "True" ? ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute : ConfigurationSettings.LoadDateTimeAttribute;
            var dwhKeyIdentifier = ConfigurationSettings.DwhKeyIdentifier; //Indicates _HSH, _SK etc.
            var keyIdentifierLocation = ConfigurationSettings.KeyNamingLocation;

            // Create the attribute selection statement for the array
            var sqlStatementForAttributeVersion = new StringBuilder();

            sqlStatementForAttributeVersion.AppendLine("SELECT ");
            sqlStatementForAttributeVersion.AppendLine("  CONVERT(CHAR(32),HASHBYTES('MD5',CONVERT(NVARCHAR(100), " + GlobalParameters.CurrentVersionId + ") + '|' + OBJECT_NAME(main.OBJECT_ID) + '|' + main.[name]),2) AS ROW_CHECKSUM,");
            sqlStatementForAttributeVersion.AppendLine("  " + GlobalParameters.CurrentVersionId + " AS [VERSION_ID],");
            sqlStatementForAttributeVersion.AppendLine("  DB_NAME(DB_ID('"+databaseName+"')) AS [DATABASE_NAME],");
            sqlStatementForAttributeVersion.AppendLine("  OBJECT_SCHEMA_NAME(main.OBJECT_ID) AS [SCHEMA_NAME],");
            sqlStatementForAttributeVersion.AppendLine("  OBJECT_NAME(main.OBJECT_ID) AS [TABLE_NAME], ");
            sqlStatementForAttributeVersion.AppendLine("  main.[name] AS [COLUMN_NAME], ");
            sqlStatementForAttributeVersion.AppendLine("  t.[name] AS [DATA_TYPE], ");
            sqlStatementForAttributeVersion.AppendLine("  CAST(COALESCE(");
            sqlStatementForAttributeVersion.AppendLine("    CASE WHEN UPPER(t.[name]) = 'NVARCHAR' THEN main.[max_length]/2"); //Exception for unicode
            sqlStatementForAttributeVersion.AppendLine("    ELSE main.[max_length]");
            sqlStatementForAttributeVersion.AppendLine("    END");
            sqlStatementForAttributeVersion.AppendLine("     ,0) AS VARCHAR(100)) AS [CHARACTER_MAXIMUM_LENGTH],");
            sqlStatementForAttributeVersion.AppendLine("  CAST(COALESCE(main.[precision],0) AS VARCHAR(100)) AS [NUMERIC_PRECISION], ");
            sqlStatementForAttributeVersion.AppendLine("  CAST(main.[column_id] AS VARCHAR(100)) AS [ORDINAL_POSITION], ");

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
            sqlStatementForAttributeVersion.AppendLine("	JOIN [" + databaseName + "].sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
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
            sqlStatementForAttributeVersion.AppendLine("	JOIN [" + databaseName + "].sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
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

            var filterList = TableMetadataFilter((DataTable)_bindingSourceTableMetadata.DataSource);
            foreach (var filter in filterList)
            {
                var fullyQualifiedName = ClassMetadataHandling.GetSchema(filter).FirstOrDefault();
                // Always add the 'regular' mapping.
                sqlStatementForAttributeVersion.AppendLine("  (OBJECT_NAME(main.OBJECT_ID) = '"+ fullyQualifiedName.Value+ "' AND OBJECT_SCHEMA_NAME(main.OBJECT_ID) = '"+fullyQualifiedName.Key+"')");
                sqlStatementForAttributeVersion.AppendLine("  OR");

                //// Workaround to allow PSA tables to be reverse-engineered automatically by replacing the STG prefix/suffix
                //if (filter.StartsWith(ConfigurationSettings.StgTablePrefixValue+"_") || filter.EndsWith("_"+ConfigurationSettings.StgTablePrefixValue))
                //{
                //    var tempFilter = filter.Replace(ConfigurationSettings.StgTablePrefixValue,ConfigurationSettings.PsaTablePrefixValue);
                //    sqlStatementForAttributeVersion.AppendLine("  '" + tempFilter + "',");
                //}
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
                dataGridViewTableMetadata.ClearSelection();
                dataGridViewTableMetadata.Rows[hti.RowIndex].Selected = true;
            }
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
        /// This method is called from the context menu on the data grid. It exports the selected row to JSON.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ExportThisRowAsSourceToTargetInterfaceJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();

            //Check if any cells were clicked / selected
            Int32 selectedRow = dataGridViewTableMetadata.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            List<string> selectionFilter = new List<string>();            
            selectionFilter.Add(dataGridViewTableMetadata.Rows[selectedRow].Cells[3].Value.ToString());

            string additionalInfoForDrivingKey = dataGridViewTableMetadata.Rows[selectedRow].Cells[5].Value.ToString();

            List<LoadPatternDefinition> patternlist = new List<LoadPatternDefinition>();
            var tableType = ClassMetadataHandling.GetTableType(dataGridViewTableMetadata.Rows[selectedRow].Cells[3].Value.ToString(), additionalInfoForDrivingKey);

            foreach (LoadPatternDefinition pattern in ConfigurationSettings.patternDefinitionList)
            {
                if (pattern.LoadPatternType==tableType)
                {
                    patternlist.Add(pattern);
                }
            }

            GenerateFromPattern(patternlist, selectionFilter);
        }

        /// <summary>
        /// This method is called from the context menu on the data grid. It deletes the row from the grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteThisRowFromTableDataGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Int32 rowToDelete = dataGridViewTableMetadata.Rows.GetFirstRow(DataGridViewElementStates.Selected);
            dataGridViewTableMetadata.Rows.RemoveAt(rowToDelete);
        }
        
        /// <summary>
        /// Write out a row in the DataGrid to file
        /// </summary>
        /// <param name="sourceTableName"></param>
        /// <param name="targetTableName"></param>
        /// <param name="businessKeyDefinition"></param>
        public void CreateJsonSourceToTargetMapping(string sourceTableName, string targetTableName,string businessKeyDefinition)
        {
            try
            {
                var jsonTableMappingFull = new JArray();
                var outputFileName = sourceTableName+"_"+targetTableName+".json";

                //Create a segment to work with
                JObject newJsonSegment = new JObject(
                    new JProperty("sourceTableName", sourceTableName),
                    //new JProperty("versionId", versionId),
                    new JProperty("targetTableName", targetTableName),
                    new JProperty("businessKeyDefinition", businessKeyDefinition)
                    );

                //Add the segment to the array
                jsonTableMappingFull.Add(newJsonSegment);

                //Spool to disk
                string output = JsonConvert.SerializeObject(jsonTableMappingFull, Formatting.Indented);
                File.WriteAllText(GlobalParameters.OutputPath + outputFileName, output);

                richTextBoxInformation.Text = "File "+ outputFileName + " has been saved to "+ GlobalParameters.OutputPath + ".";
            }
            catch (JsonReaderException ex)
            {
                richTextBoxInformation.Text += "There were issues saving the JSON file.\r\n" + ex;
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
                if (ValidationSettings.SourceObjectExistence == "True")
                {
                    ValidateObjectExistence("source");
                }

                if (worker != null) worker.ReportProgress(15);

                if (ValidationSettings.TargetObjectExistence == "True")
                {
                    ValidateObjectExistence("target");
                }

                if (worker != null) worker.ReportProgress(30);

                if (ValidationSettings.SourceBusinessKeyExistence == "True")
                {
                    ValidateBusinessKeyObject();
                }

                if (ValidationSettings.SourceAttributeExistence == "True")
                {
                    ValidateAttributeExistence("source");
                }

                if (ValidationSettings.TargetAttributeExistence == "True")
                {
                    ValidateAttributeExistence("target");
                }

                if (worker != null) worker.ReportProgress(60);

                if (ValidationSettings.LogicalGroup == "True")
                {
                    ValidateLogicalGroup();
                }

                if (worker != null) worker.ReportProgress(75);

                if (ValidationSettings.LinkKeyOrder == "True")
                {
                    ValidateLinkKeyOrder();
                }

                if (worker != null) worker.ReportProgress(100);

                // Informing the user.
                _alertValidation.SetTextLogging("\r\nIn total " + MetadataParameters.ValidationIssues + " validation issues have been found.");
            }
        }

        internal static class MetadataParameters
        {
            // TEAM core path parameters
            public static int ValidationIssues { get; set; }
            public static bool ValidationRunning {get; set;}
        }

        /// <summary>
        /// This method runs a check against the DataGrid to assert if model metadata is available for the attributes. The attribute needs to exist somewhere, either in the physical model or in the model metadata in order for activation to run successfully.
        /// </summary>
        /// <param name="area"></param>
        private void ValidateAttributeExistence(string area)
        {
            string evaluationMode = radioButtonPhysicalMode.Checked ? "physical" : "virtual";

            // Map the area to the column in the datagrid (e.g. source or target)
            int areaColumnIndex = 0;
            int areaAttributeColumnIndex = 0;

            switch (area)
            {
                case "source":
                    areaColumnIndex = 2;
                    areaAttributeColumnIndex = 3;
                    break;
                case "target":
                    areaColumnIndex = 4;
                    areaAttributeColumnIndex = 5;
                    break;
                default:
                    areaColumnIndex = 0;
                    areaAttributeColumnIndex = 0;
                    break;
            }

            // Informing the user.
            _alertValidation.SetTextLogging($"--> Commencing the validation to determine if the attributes in the {area} metadata exists in the model.\r\n");

            var resultList = new Dictionary<string, string>();

            foreach (DataGridViewRow row in dataGridViewAttributeMetadata.Rows)
            {
                if (!row.IsNewRow)
                {
                    string objectValidated;
                    var validationObject = row.Cells[areaColumnIndex].Value.ToString();
                    var validationAttribute = row.Cells[areaAttributeColumnIndex].Value.ToString();

                    if (evaluationMode == "physical" && ClassMetadataHandling.GetTableType(validationObject, "") != ClassMetadataHandling.TableTypes.Source.ToString()) // No need to evaluate the operational system (real sources)
                    {
                        Dictionary<string, string> connectionInformation = ClassMetadataHandling.GetConnectionInformationForTableType(ClassMetadataHandling.GetTableType(validationObject,""));
                        string connectionValue = connectionInformation.FirstOrDefault().Value;

                        objectValidated = ClassMetadataValidation.ValidateAttributeExistencePhysical(validationObject, validationAttribute, connectionValue);
                    }
                    else if (evaluationMode == "virtual")
                    {
                        objectValidated = "";
                        // Exclude a lookup to the source
                        if (ClassMetadataHandling.GetTableType(validationObject, "") != ClassMetadataHandling.TableTypes.Source.ToString())
                        {
                            objectValidated = ClassMetadataValidation.ValidateAttributeExistenceVirtual(validationObject, validationAttribute, (DataTable)_bindingSourcePhysicalModelMetadata.DataSource);
                        }
                    }
                    else
                    {
                        objectValidated = "     The validation approach (physical/virtual) could not be asserted.";
                    }

                    // Add negative results to dictionary
                    if (objectValidated == "False" && !resultList.ContainsKey((validationAttribute)))
                    {
                        resultList.Add(validationAttribute, objectValidated); // Add objects that did not pass the test
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
                _alertValidation.SetTextLogging($"     There were no validation issues related to the existence of the {area} attribute.\r\n\r\n");
            }
        }

        /// <summary>
        /// This method runs a check against the DataGrid to assert if model metadata is available for the object. The object needs to exist somewhere, either in the physical model or in the model metadata in order for activation to run succesfully.
        /// </summary>
        /// <param name="area"></param>
        private void ValidateObjectExistence(string area)
        {
            string evaluationMode = radioButtonPhysicalMode.Checked ? "physical" : "virtual";

            // Map the area to the column in the datagrid (e.g. source or target)
            int areaColumnIndex = 0;
            switch (area)
            {
                case "source":
                    areaColumnIndex = 2;
                    break;
                case "target":
                    areaColumnIndex = 3;
                    break;
                default:
                    // Do nothing
                    break;
            }

            // Informing the user.
            _alertValidation.SetTextLogging($"--> Commencing the validation to determine if the objects in the {area} metadata exists in the model.\r\n");

            var resultList = new Dictionary<string, string>();

            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (!row.IsNewRow)
                {
                    string objectValidated;
                    var validationObject = row.Cells[areaColumnIndex].Value.ToString();
                    if (evaluationMode == "physical" && ClassMetadataHandling.GetTableType(validationObject, "") != ClassMetadataHandling.TableTypes.Source.ToString()) // No need to evaluate the operational system (real sources)
                    {
                        Dictionary<string, string> connectionInformation = ClassMetadataHandling.GetConnectionInformationForTableType(ClassMetadataHandling.GetTableType(validationObject, ""));
                        string connectionValue = connectionInformation.FirstOrDefault().Value;

                        try
                        {
                            objectValidated =
                                ClassMetadataValidation.ValidateObjectExistencePhysical(validationObject,
                                    connectionValue);
                        }
                        catch
                        {
                            objectValidated = "     An issue occurred connecting to the database.";
                        }
                    }
                    else if (evaluationMode == "virtual")
                    {
                        objectValidated = "";
                        // Exclude a lookup to the source
                        if (ClassMetadataHandling.GetTableType(validationObject,"") != ClassMetadataHandling.TableTypes.Source.ToString())
                        {
                            objectValidated = ClassMetadataValidation.ValidateObjectExistenceVirtual(validationObject,
                                (DataTable) _bindingSourcePhysicalModelMetadata.DataSource);
                        }
                    }
                    else
                    {
                        objectValidated = "     The validation approach (physical/virtual) could not be asserted.";
                    }

                    // Add negative results to dictionary
                    if (objectValidated == "False" && !resultList.ContainsKey(validationObject))
                    {
                        resultList.Add(validationObject, objectValidated); // Add objects that did not pass the test
                    }
                }
            }

            // Return the results back to the user
            if (resultList.Count > 0)
            {
                foreach (var objectValidationResult in resultList)
                {
                    _alertValidation.SetTextLogging("     " + objectValidationResult.Key + " is tested with this outcome: " + objectValidationResult.Value + "\r\n");
                }

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count;
                _alertValidation.SetTextLogging("\r\n");
            }
            else
            {
                _alertValidation.SetTextLogging($"     There were no validation issues related to the existence of the {area} table / object.\r\n\r\n");
            }

        }
 
        /// <summary>
        /// This method will check if the order of the keys in the Link is consistent with the physical table structures.
        /// </summary>
        internal void ValidateLinkKeyOrder()
        {
            string evaluationMode = radioButtonPhysicalMode.Checked ? "physical" : "virtual";

            #region Retrieving the Links
            // Informing the user.
            _alertValidation.SetTextLogging("--> Commencing the validation to ensure the order of Business Keys in the Link metadata corresponds with the physical model.\r\n");


            // Creating a list of unique Link business key combinations from the data grid / data table
            var objectList = new List<Tuple<string, string, string>>();
            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (!row.IsNewRow && row.Cells[3].Value.ToString().StartsWith(ConfigurationSettings.LinkTablePrefixValue)) // Only select the lines that relate to a Link target
                {
                    var businessKey = row.Cells[4].Value.ToString().Replace("''''", "'");
                    if (!objectList.Contains(new Tuple<string, string, string>(row.Cells[2].Value.ToString(), row.Cells[3].Value.ToString(), businessKey)))
                    {
                        objectList.Add(new Tuple<string, string, string>(row.Cells[2].Value.ToString(), row.Cells[3].Value.ToString(), businessKey));
                    }
                }
            } 

            // Execute the validation check using the list of unique objects
            var resultList = new Dictionary<string, bool>();

            foreach (var sourceObject in objectList)
            {
                // The validation check returns a Dictionary
                var sourceObjectValidated = ClassMetadataValidation.ValidateLinkKeyOrder(sourceObject, ConfigurationSettings.ConnectionStringOmd, GlobalParameters.CurrentVersionId, (DataTable)_bindingSourceTableMetadata.DataSource, (DataTable)_bindingSourcePhysicalModelMetadata.DataSource,evaluationMode);

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
                    _alertValidation.SetTextLogging("     "+sourceObjectResult.Key + " is tested with this outcome: " + sourceObjectResult.Value + "\r\n");
                }

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count();
                _alertValidation.SetTextLogging("\r\n");
            }
            else
            {
                _alertValidation.SetTextLogging("     There were no validation issues related to order of business keys in the Link tables.\r\n\r\n");
            }
        }

        /// <summary>
        /// Checks if all the supporting mappings are available (e.g. a Context table also needs a Core Business Concept present.
        /// </summary>
        internal void ValidateLogicalGroup()
        {
            string evaluationMode = radioButtonPhysicalMode.Checked ? "physical" : "virtual";

            #region Retrieving the Integration Layer tables
            // Informing the user.
            _alertValidation.SetTextLogging("--> Commencing the validation to check if the functional dependencies (logical group / unit of work) are present.\r\n");

            // Creating a list of tables which are dependent on other tables being present
            var objectList = new List<Tuple<string, string, string>>();
            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (!row.IsNewRow && (row.Cells[3].Value.ToString().StartsWith(ConfigurationSettings.LinkTablePrefixValue) || row.Cells[3].Value.ToString().StartsWith(ConfigurationSettings.SatTablePrefixValue) || row.Cells[3].Value.ToString().StartsWith(ConfigurationSettings.LsatTablePrefixValue))  )
                {
                    var businessKey = row.Cells[4].Value.ToString().Replace("''''", "'");
                    if (!objectList.Contains(new Tuple<string, string, string>(row.Cells[2].Value.ToString(), row.Cells[3].Value.ToString(), businessKey)))
                    {
                        objectList.Add(new Tuple<string, string, string>(row.Cells[2].Value.ToString(), row.Cells[3].Value.ToString(), businessKey));
                    }
                }
            }

            // Execute the validation check using the list of unique objects
            var resultList = new Dictionary<string, bool>();

            foreach (var sourceObject in objectList)
            {
                // The validation check returns a Dictionary
                var sourceObjectValidated = ClassMetadataValidation.ValidateLogicalGroup(sourceObject, ConfigurationSettings.ConnectionStringOmd, GlobalParameters.CurrentVersionId, (DataTable)_bindingSourceTableMetadata.DataSource);

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
                    _alertValidation.SetTextLogging("     "+sourceObjectResult.Key + " is tested with this outcome: " + sourceObjectResult.Value + "\r\n");
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
            string evaluationMode = radioButtonPhysicalMode.Checked ? "physical" : "virtual";

            // Informing the user.
            _alertValidation.SetTextLogging("--> Commencing the validation to determine if the Business Key metadata attributes exist in the physical model.\r\n");

            var resultList = new Dictionary<Tuple<string, string>, bool>();
            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (!row.IsNewRow)
                {
                    Dictionary<Tuple<string, string>, bool> objectValidated = new Dictionary<Tuple<string, string>, bool>();
                    Tuple<string, string> validationObject = new Tuple<string, string>(row.Cells[2].Value.ToString(), row.Cells[4].Value.ToString());

                    //row.Cells[areaColumnIndex].Value.ToString();
                    if (evaluationMode == "physical" && ClassMetadataHandling.GetTableType(validationObject.Item1,"") != ClassMetadataHandling.TableTypes.Source.ToString()) // No need to evaluate the operational system (real sources)
                    {
                        Dictionary<string, string> connectionInformation = ClassMetadataHandling.GetConnectionInformationForTableType(ClassMetadataHandling.GetTableType(validationObject.Item1,""));
                        string connectionValue = connectionInformation.FirstOrDefault().Value;

                        try
                        {
                            objectValidated = ClassMetadataValidation.ValidateSourceBusinessKeyExistencePhysical(validationObject, connectionValue);
                        }
                        catch
                        {
                            _alertValidation.SetTextLogging("     An issue occurred connecting to the database while looking up physical model references.\r\n");
                        }
                    }
                    else if (evaluationMode == "virtual")
                    {
                        // Exclude a lookup to the source
                        if (ClassMetadataHandling.GetTableType(validationObject.Item1,"") != ClassMetadataHandling.TableTypes.Source.ToString())
                        { 
                            objectValidated = ClassMetadataValidation.ValidateSourceBusinessKeyExistenceVirtual(validationObject, (DataTable)_bindingSourcePhysicalModelMetadata.DataSource);
                        }
                    }
                    else
                    {
                        if (ClassMetadataHandling.GetTableType(validationObject.Item1,"") !=
                            ClassMetadataHandling.TableTypes.Source.ToString())
                        {
                            _alertValidation.SetTextLogging("     The validation approach (physical/virtual) could not be asserted.\r\n");
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
                    _alertValidation.SetTextLogging("     Table " + sourceObjectResult.Key.Item1 + " does not contain Business Key attribute " + sourceObjectResult.Key.Item2 + ".\r\n");
                }

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count();
            }
            else
            {
                _alertValidation.SetTextLogging("     There were no validation issues related to the existence of the business keys in the Source tables.\r\n");
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

        private void openModelMetadataFileToolStripMenuItem_Click(object sender, EventArgs e)
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
                                var backupFile = new JsonHandling();
                                var targetFileName = backupFile.BackupJsonFile(GlobalParameters.JsonModelMetadataFileName + @"_v" + GlobalParameters.CurrentVersionId + ".json", FormBase.GlobalParameters.ConfigurationPath);
                                richTextBoxInformation.Text = "A backup of the in-use JSON file was created as " + targetFileName + ".\r\n\r\n";
                            }
                            catch (Exception exception)
                            {
                                richTextBoxInformation.Text = "An issue occured when trying to make a backup of the in-use JSON file. The error message was " + exception + ".";
                            }
                        }

                        // If the information needs to be merged, a global parameter needs to be set.
                        // This will overwrite existing files for the in-use version.
                        if (!checkBoxMergeFiles.Checked)
                        {
                            FileConfiguration.newFilePhysicalModel = "true";
                        }

                        // Load the file, convert it to a DataTable and bind it to the source
                        List<PhysicalModelMetadataJson> jsonArray = JsonConvert.DeserializeObject<List<PhysicalModelMetadataJson>>(File.ReadAllText(chosenFile));
                        DataTable dt = Utility.ConvertToDataTable(jsonArray);

                        // Setup the datatable with proper column headings.
                        SetTeamDataTableMapping.SetPhysicalModelDataTableColumns(dt);
                        // Sort the columns.
                        SetTeamDataTableMapping.SetPhysicalModelDataTableSorting(dt);

                        // Clear out the existing data from the grid
                        _bindingSourcePhysicalModelMetadata.DataSource = null;
                        _bindingSourcePhysicalModelMetadata.Clear();
                        dataGridViewPhysicalModelMetadata.DataSource = null;

                        // Bind the datatable to the gridview
                        _bindingSourcePhysicalModelMetadata.DataSource = dt;

                        if (jsonArray != null)
                        {
                            // Set the column header names
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
                            dataGridViewPhysicalModelMetadata.Columns[9].HeaderText = "Position";
                            dataGridViewPhysicalModelMetadata.Columns[10].HeaderText = "Primary Key";
                            dataGridViewPhysicalModelMetadata.Columns[11].HeaderText = "Multi-Active";
                        }
                    }

                    GridAutoLayoutPhysicalModelMetadata();
                    ContentCounter();
                    richTextBoxInformation.AppendText("The file " + chosenFile + " was loaded.\r\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error has been encountered! The reported error is: " + ex);
                }
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

            DataTable gridDataTable = (DataTable)_bindingSourcePhysicalModelMetadata.DataSource;
            DataTable dt2 = gridDataTable.Clone();
            dt2.Columns["ORDINAL_POSITION"].DataType = Type.GetType("System.Int32");

            foreach (DataRow dr in gridDataTable.Rows)
            {
                dt2.ImportRow(dr);
            }
            dt2.AcceptChanges();

            // Make sure the output is sorted
            dt2.DefaultView.Sort = "[TABLE_NAME] ASC, [ORDINAL_POSITION] ASC";

            // Retrieve all rows relative to the selected row (e.g. all attributes for the table)
            IEnumerable<DataRow> rows = dt2.DefaultView.ToTable().AsEnumerable().Where(r =>
                r.Field<string>("TABLE_NAME") ==
                dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[4].Value.ToString()
                && r.Field<string>("SCHEMA_NAME") ==
                dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[3].Value.ToString()
                && r.Field<string>("DATABASE_NAME") ==
                dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[2].Value.ToString()
                );

            // Create a form and display the results
            var results = new StringBuilder();

            _generatedScripts = new Form_Alert();
            _generatedScripts.SetFormName("Display model metadata");
            _generatedScripts.Canceled += buttonCancel_Click;
            _generatedScripts.Show();

            results.AppendLine("IF OBJECT_ID('["+ dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[4].Value + "]', 'U') IS NOT NULL");
            results.AppendLine("DROP TABLE [" + dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[4].Value + "]");
            results.AppendLine();
            results.AppendLine("CREATE TABLE [" + dataGridViewPhysicalModelMetadata.Rows[selectedRow].Cells[4].Value + "]");
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
                results.AppendLine(commaSnippet + row["COLUMN_NAME"] + " -- with ordinal position of "+ row["ORDINAL_POSITION"]);
            }
            results.AppendLine(")");

            _generatedScripts.SetTextLogging(results.ToString());
            _generatedScripts.ProgressValue = 100;
            _generatedScripts.Message = "Done";
        }

        private void ButtonClickExportToJson(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();

            List<string> dummyFilter = new List<string>();
            GenerateFromPattern(ConfigurationSettings.patternDefinitionList, dummyFilter);
        }

        /// <summary>
        /// Creates a Json schema based on the Data Warehouse Automation interface definition using a pattern and filter as input. 
        /// </summary>
        /// <param name="LoadPatternDefinitionList"></param>
        /// <param name="filter"></param>
        private void GenerateFromPattern(List<LoadPatternDefinition> LoadPatternDefinitionList, List<string> filter)
        {
            // Set up the form in case the show Json output checkbox has been selected
            if (checkBoxShowJsonOutput.Checked)
            {
                _generatedJsonInterface = new Form_Alert();
                _generatedJsonInterface.SetFormName("Exporting the metadata");
                _generatedJsonInterface.ShowProgressBar(false);
                _generatedJsonInterface.ShowCancelButton(false);
                _generatedJsonInterface.ShowLogButton(false);
                _generatedJsonInterface.ShowProgressLabel(false);
                _generatedJsonInterface.Show();
            }

            int fileCounter = 0;

            EventLog eventLog = new EventLog();
            SqlConnection conn = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };


            foreach (LoadPatternDefinition pattern in LoadPatternDefinitionList)
            {
                List<string> itemList = new List<string>();

                // Retrieve the items related to the pattern (i.e. all the Hubs, or all the Links) - selectionQuery. Or replace this with an input list.            
                try
                {
                    if (filter.Count == 0)
                    {
                        itemList = DatabaseHandling.GetItemList(pattern.LoadPatternSelectionQuery, conn);
                    }
                    else
                    {
                        itemList = filter;
                    }
                }
                catch (Exception ex)
                {
                    eventLog.Add(Event.CreateNewEvent(EventTypes.Error, "The item list could not be retrieved (selectionQuery in PatternDefinition file). The error message is " + ex + ".\r\n"));
                }

                // Retrieve the source-to-target mappings (base query)
                DataTable metadataDataTable = new DataTable();
                try
                {
                    var metadataQuery = pattern.LoadPatternBaseQuery;
                    metadataDataTable = Utility.GetDataTable(ref conn, metadataQuery);
                }
                catch (Exception ex)
                {
                    eventLog.Add(Event.CreateNewEvent(EventTypes.Error, "The source-to-target mapping list could not be retrieved (baseQuery in PatternDefinition file). The error message is " + ex + ".\r\n"));
                }

                // Populate the attribute mappings
                // Create the column-to-column mapping
                var columnMetadataQuery = pattern.LoadPatternAttributeQuery;
                var columnMetadataDataTable = Utility.GetDataTable(ref conn, columnMetadataQuery);

                // Populate the additional business key information (i.e. links)
                var additionalBusinessKeyQuery = pattern.LoadPatternAdditionalBusinessKeyQuery;
                var additionalBusinessKeyDataTable = Utility.GetDataTable(ref conn, additionalBusinessKeyQuery);

                // Loop through the available items, select the right mapping and map the metadata to the DWH automation schema
                foreach (string item in itemList)
                {
                    var targetTableName = item;
                    richTextBoxInformation.AppendText(@"Processing generation for " + targetTableName + ".\r\n");

                    DataRow[] mappingRows = null;
                    try
                    {
                        mappingRows = metadataDataTable.Select("[TARGET_NAME] = '" + targetTableName + "'");
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.AppendText(
                            "There was an error generating the output, this happened when interpreting the source-to-mapping rows. " +
                            "\r\n\r\nThe query used was:" + pattern.LoadPatternBaseQuery +
                            ".\r\n\r\nThe error message was:" + ex);
                    }

                    // Move the data table to the class instance
                    List<DataObjectMapping> sourceToTargetMappingList = new List<DataObjectMapping>();

                    if (mappingRows != null)
                    {
                        foreach (DataRow row in mappingRows)
                        {
                            #region Business Key

                            // Creating the Business Key definition, using the available components (see above)
                            List<BusinessKey> businessKeyList = new List<BusinessKey>();
                            BusinessKey businessKey =
                                new BusinessKey
                                {
                                    businessKeyComponentMapping =
                                        InterfaceHandling.BusinessKeyComponentMappingList(
                                            (string)row["SOURCE_BUSINESS_KEY_DEFINITION"],
                                            (string)row["TARGET_BUSINESS_KEY_DEFINITION"]),
                                    surrogateKey = (string)row["SURROGATE_KEY"]
                                };


                            // Create the classifications at Data Item (target) level, to capture if this attribute is a Multi-Active attribute.
                            if (row.Table.Columns.Contains("DRIVING_KEY_SOURCE"))
                            {
                                if (row["DRIVING_KEY_SOURCE"].ToString().Length>0)
                                {
                                    // Update the existing Business Key with a classification if a Driving Key exists.

                                    foreach (var localDataItemMapping in businessKey.businessKeyComponentMapping)
                                    {
                                        if (localDataItemMapping.sourceDataItem.name ==
                                            (string) row["DRIVING_KEY_SOURCE"])
                                        {
                                            
                                            List<Classification> dataItemClassificationList =
                                                new List<Classification>();
                                            var dataItemClassification = new Classification();
                                            dataItemClassification.classification = "DrivingKey";
                                            dataItemClassification.notes =
                                                "The attribute that triggers (drives) closing of a relationship.";
                                            dataItemClassificationList.Add(dataItemClassification);

                                            localDataItemMapping.sourceDataItem.dataItemClassification =
                                                dataItemClassificationList;
                                        }
                                    }


                                }
                            }

                            businessKeyList.Add(businessKey);

                            #endregion

                            #region Data Item Mapping (column to column)

                            // Create the column-to-column mapping.
                            List<DataItemMapping> dataItemMappingList = new List<DataItemMapping>();
                            if (columnMetadataDataTable != null && columnMetadataDataTable.Rows.Count > 0)
                            {
                                DataRow[] columnRows = columnMetadataDataTable.Select("[TARGET_NAME] = '" + targetTableName + "' AND [SOURCE_NAME] = '" + (string)row["SOURCE_NAME"] + "'");

                                foreach (DataRow column in columnRows)
                                {
                                    DataItemMapping columnMapping = new DataItemMapping();
                                    DataItem sourceColumn = new DataItem();
                                    DataItem targetColumn = new DataItem();

                                    sourceColumn.name = (string)column["SOURCE_ATTRIBUTE_NAME"];
                                    targetColumn.name = (string)column["TARGET_ATTRIBUTE_NAME"];

                                    columnMapping.sourceDataItem = sourceColumn;
                                    columnMapping.targetDataItem = targetColumn;

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
                                            columnMapping.targetDataItem.dataItemClassification = dataItemClassificationList;
                                        }
                                    }

                                    // Adding NULL classification
                                    if ((string) column["SOURCE_ATTRIBUTE_NAME"] == "NULL")
                                    {
                                        // Create the classifications at Data Item (target) level, to capture if this attribute is a NULL.
                                        List<Classification> dataItemClassificationList = new List<Classification>();
                                        var dataItemClassification = new Classification();
                                        dataItemClassification.classification = "NULL value";
                                        dataItemClassificationList.Add(dataItemClassification);

                                        // Add the classification to the target Data Item
                                        columnMapping.sourceDataItem.dataItemClassification = dataItemClassificationList;
                                    }

                                    dataItemMappingList.Add(columnMapping);
                                }
                            }

                            #endregion

                            #region Additional Business Keys

                            if (additionalBusinessKeyDataTable != null && additionalBusinessKeyDataTable.Rows.Count > 0)
                            {
                                DataRow[] additionalBusinessKeyRows =
                                    additionalBusinessKeyDataTable.Select("[TARGET_NAME] = '" + targetTableName + "'");

                                foreach (DataRow additionalKeyRow in additionalBusinessKeyRows)
                                {
                                    var hubBusinessKey = new BusinessKey();

                                    hubBusinessKey.businessKeyComponentMapping =
                                        InterfaceHandling.BusinessKeyComponentMappingList(
                                            (string)additionalKeyRow["SOURCE_BUSINESS_KEY_DEFINITION"],
                                            (string)additionalKeyRow["TARGET_BUSINESS_KEY_DEFINITION"]);
                                    hubBusinessKey.surrogateKey = (string)additionalKeyRow["TARGET_KEY_NAME"];

                                    if ((string) additionalKeyRow["HUB_NAME"] == "N/A")
                                    {
                                        // Classification (degenerate field)
                                        List<Classification> businesskeyClassificationList = new List<Classification>();
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

                            #endregion

                            #region Lookup Table
                            // Define a lookup table, in case there is a desire to do key lookups.
                            var lookupTable = (string)row["TARGET_NAME"];

                            if (ConfigurationSettings.TableNamingLocation == "Prefix")
                            {
                                int prefixLocation = lookupTable.IndexOf(ConfigurationSettings.StgTablePrefixValue);
                                if (prefixLocation != -1)
                                {
                                    lookupTable = lookupTable
                                        .Remove(prefixLocation, ConfigurationSettings.StgTablePrefixValue.Length)
                                        .Insert(prefixLocation, ConfigurationSettings.PsaTablePrefixValue);
                                }
                            }
                            else
                            {
                                int prefixLocation = lookupTable.LastIndexOf(ConfigurationSettings.StgTablePrefixValue);
                                if (prefixLocation != -1)
                                {
                                    lookupTable = lookupTable
                                        .Remove(prefixLocation, ConfigurationSettings.StgTablePrefixValue.Length)
                                        .Insert(prefixLocation, ConfigurationSettings.PsaTablePrefixValue);
                                }
                            }
                            #endregion

                            // Add the created Business Key to the source-to-target mapping.
                            var sourceToTargetMapping = new DataObjectMapping();

                            var sourceDataObject = new DataWarehouseAutomation.DataObject();
                            var targetDataObject = new DataWarehouseAutomation.DataObject();

                            sourceDataObject.name = (string)row["SOURCE_NAME"];
                            targetDataObject.name = (string)row["TARGET_NAME"];

                            var targetConnectionKey = new DataConnection();
                            targetConnectionKey.dataConnectionString = pattern.LoadPatternConnectionKey;

                            targetDataObject.dataObjectConnection = targetConnectionKey;

                            sourceToTargetMapping.sourceDataObject = sourceDataObject;
                            sourceToTargetMapping.targetDataObject = targetDataObject;
                            sourceToTargetMapping.enabled = true;

                            // Create a related data object to capture the lookup information.
                            // This needs to be put in a collection because the relatedDataObject is a List of Data Objects.
                            List<DataWarehouseAutomation.DataObject> relatedDataObject = new List<DataWarehouseAutomation.DataObject>();
                            var lookupTableDataObject = new DataWarehouseAutomation.DataObject();
                            lookupTableDataObject.name = lookupTable;

                            // Create the classifications at Data Object level, to capture this is a Lookup relationship.
                            List<Classification> dataObjectClassificationList = new List<Classification>();
                            var dataObjectClassification = new Classification();
                            dataObjectClassification.classification = "Lookup";
                            dataObjectClassification.notes = "Lookup table related to the source-to-target mapping";
                            dataObjectClassificationList.Add(dataObjectClassification);

                            lookupTableDataObject.dataObjectClassification = dataObjectClassificationList;

                            relatedDataObject.Add(lookupTableDataObject);

                            sourceToTargetMapping.relatedDataObject = relatedDataObject;


                            //sourceToTargetMapping.lookupTable = lookupTable; // Lookup Table
                            sourceToTargetMapping.mappingName = (string)row["TARGET_NAME"]; // Source-to-target mapping name
                            sourceToTargetMapping.businessKey = businessKeyList; // Business Key]

                            // Create the classifications at Data Object Mapping level.
                            List<Classification> dataObjectMappingClassificationList= new List<Classification>();
                            var dataObjectMappingClassification = new Classification();
                            dataObjectMappingClassification.id = pattern.LoadPatternKey;
                            dataObjectMappingClassification.classification = pattern.LoadPatternType;
                            dataObjectMappingClassification.notes = pattern.LoadPatternNotes;
                            dataObjectMappingClassificationList.Add(dataObjectMappingClassification);

                            sourceToTargetMapping.mappingClassification = dataObjectMappingClassificationList;
                            //sourceToTargetMapping.classification = ClassMetadataHandling.GetTableType((string)row["TARGET_NAME"], "").Split(',').ToList();
                            //sourceToTargetMapping.classification = pattern.LoadPatternType.Split(',').ToList(); ;
                            
                            sourceToTargetMapping.filterCriterion = (string)row["FILTER_CRITERIA"]; // Filter criterion

                            if (dataItemMappingList.Count == 0)
                            {
                                dataItemMappingList = null;
                            }

                            sourceToTargetMapping.dataItemMapping = dataItemMappingList; // Column to column mapping

                            // Add the source-to-target mapping to the mapping list
                            sourceToTargetMappingList.Add(sourceToTargetMapping);
                        }
                    }

                    // Create an instance of the non-generic information i.e. VEDW specific. For example the generation date/time.
                    GenerationSpecificMetadata vedwMetadata = new GenerationSpecificMetadata();
                    vedwMetadata.selectedDataObject = targetTableName;

                    // Create an instance of the 'MappingList' class / object model 
                    VEDW_DataObjectMappingList sourceTargetMappingList = new VEDW_DataObjectMappingList();
                    sourceTargetMappingList.dataObjectMappingList = sourceToTargetMappingList;

                    sourceTargetMappingList.metadataConfiguration = new MetadataConfiguration();
                    sourceTargetMappingList.generationSpecificMetadata = vedwMetadata;

                    // Check if the metadata needs to be displayed
                    try
                    {
                        var json = JsonConvert.SerializeObject(sourceTargetMappingList, Formatting.Indented);

                        if (checkBoxShowJsonOutput.Checked)
                        {
                            _generatedJsonInterface.SetTextLogging("Creating " + targetTableName + "\r\n\r\n" + json + "\r\n\r\n");
                        }

                        // Spool the output to disk
                        if (checkBoxSaveInterfaceToJson.Checked)
                        {
                            Event fileSaveEventLog = TeamUtility.SaveTextToFile(GlobalParameters.OutputPath + targetTableName + ".json", json);
                            eventLog.Add(fileSaveEventLog);
                            fileCounter++;
                        }
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.AppendText("An error was encountered while generating the JSON metadata. The error message is: " + ex);
                    }
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

            richTextBoxInformation.AppendText($"\r\n{errorCounter} errors have been found.\r\n");

            richTextBoxInformation.AppendText($"\r\n{fileCounter} json schemas (files) have been prepared.\r\n");

            // Spool the output to disk
            if (checkBoxSaveInterfaceToJson.Checked)
            {
                richTextBoxInformation.AppendText($"Associated scripts have been saved in {GlobalParameters.OutputPath}.\r\n");
            }

            richTextBoxInformation.ScrollToCaret();
            //_generatedJsonInterface.Focus();

            conn.Close();
            conn.Dispose();
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
                richTextBoxInformation.Text = "An error has occured while attempting to open the configuration directory. The error message is: " + ex;
            }
        }

        private void checkBoxShowJsonOutput_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}