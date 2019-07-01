using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TEAM
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class FormManageMetadata : FormBase
    {
        FormAlert _alert;
        FormAlert _alertValidation;

        //Getting the datatable to bind to something
        private BindingSource _bindingSourceTableMetadata = new BindingSource();
        private BindingSource _bindingSourceAttributeMetadata = new BindingSource();
        private BindingSource _bindingSourcePhysicalModelMetadata = new BindingSource();

        public FormManageMetadata()
        {
            InitializeComponent();
        }

        #region JSON classes
        public class PhysicalModelMetadataJson
        {
            //JSON representation of the physical model metadata
            public string versionAttributeHash { get; set; }
            public string versionId { get; set; }
            public string databaseName { get; set; }
            public string schemaName { get; set; }
            public string tableName { get; set; }
            public string columnName { get; set; }
            public string dataType { get; set; }
            public string characterMaximumLength { get; set; }
            public string numericPrecision { get; set; }
            public string ordinalPosition { get; set; }
            public string primaryKeyIndicator { get; set; }
            public string multiActiveIndicator { get; set; }
        }

        public class TableMappingJson
        {
            //JSON represenation of the table mapping metadata
            public string tableMappingHash { get; set; }
            public string versionId { get; set; }
            public string sourceTable { get; set; }
            public string targetTable { get; set; }
            public string businessKeyDefinition { get; set; }
            public string drivingKeyDefinition { get; set; }
            public string filterCriteria { get; set; }
            public string processIndicator { get; set; }
        }

        public class AttributeMappingJson
        {
            //JSON represenation of the attribute mapping metadata
            public string attributeMappingHash { get; set; }
            public string versionId { get; set; }
            public string sourceTable { get; set; }
            public string sourceAttribute { get; set; }
            public string targetTable { get; set; }
            public string targetAttribute { get; set; }
            public string transformationRule { get; set; }
        }
        #endregion

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
            GlobalParameters.currentVersionId = selectedVersion;
            GlobalParameters.highestVersionId = selectedVersion; // On startup, the highest version is the same as the current version
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
                    var newEnvironmentConfiguration = new ClassEnvironmentConfiguration();
                    newEnvironmentConfiguration.CreateDummyValidationConfiguration(validationFile);
                }

                // Load the validation settings file using the paths retrieved from the application root contents (configuration path)
                ClassEnvironmentConfiguration.LoadValidationFile(validationFile);

                richTextBoxInformation.Text += "\r\nThe validation file " + validationFile + " has been loaded.";
            }
            catch (Exception)
            {
                // ignored
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
                            // MessageBox.Show("!");
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

                    var versionList = GetDataTable(ref connOmd, sqlStatementForLatestVersion.ToString());
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
                    ClassJsonHandling.CreateDummyJsonFile(GlobalParameters.JsonModelMetadataFileName);
                }

                // Load the file, convert it to a DataTable and bind it to the source
                List<PhysicalModelMetadataJson> jsonArray = JsonConvert.DeserializeObject<List<PhysicalModelMetadataJson>>(File.ReadAllText(GlobalParameters.ConfigurationPath +GlobalParameters.JsonModelMetadataFileName+ FileConfiguration.jsonVersionExtension));

                DataTable dt = ConvertToDataTable(jsonArray);
                dt.AcceptChanges();
                //Make sure the changes are seen as committed, so that changes can be detected later on
                dt.Columns[0].ColumnName = "VERSION_ATTRIBUTE_HASH";
                dt.Columns[1].ColumnName = "VERSION_ID";
                dt.Columns[2].ColumnName = "DATABASE_NAME";
                dt.Columns[3].ColumnName = "SCHEMA_NAME";
                dt.Columns[4].ColumnName = "TABLE_NAME";
                dt.Columns[5].ColumnName = "COLUMN_NAME";
                dt.Columns[6].ColumnName = "DATA_TYPE";
                dt.Columns[7].ColumnName = "CHARACTER_MAXIMUM_LENGTH";
                dt.Columns[8].ColumnName = "NUMERIC_PRECISION";
                dt.Columns[9].ColumnName = "ORDINAL_POSITION";
                dt.Columns[10].ColumnName = "PRIMARY_KEY_INDICATOR";
                dt.Columns[11].ColumnName = "MULTI_ACTIVE_INDICATOR";

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
        /// Populate the Table Mapping datagrid from a database or existing JSON file.
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

                    var versionList = GetDataTable(ref connOmd, sqlStatementForLatestVersion.ToString());

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
                    ClassJsonHandling.CreateDummyJsonFile(GlobalParameters.JsonTableMappingFileName);
                }

                // Load the file, convert it to a DataTable and bind it to the source
                List<TableMappingJson> jsonArray = JsonConvert.DeserializeObject<List<TableMappingJson>>(File.ReadAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonTableMappingFileName+FileConfiguration.jsonVersionExtension));
                DataTable dt = ConvertToDataTable(jsonArray);
                // Order by Source Table, Integration_Area table, Business Key Attribute

                dt.AcceptChanges(); //Make sure the changes are seen as committed, so that changes can be detected later on
                dt.Columns[0].ColumnName = "TABLE_MAPPING_HASH";
                dt.Columns[1].ColumnName = "VERSION_ID";
                dt.Columns[2].ColumnName = "SOURCE_TABLE";
                dt.Columns[3].ColumnName = "TARGET_TABLE";
                dt.Columns[4].ColumnName = "BUSINESS_KEY_ATTRIBUTE";
                dt.Columns[5].ColumnName = "DRIVING_KEY_ATTRIBUTE";
                dt.Columns[6].ColumnName = "FILTER_CRITERIA";
                dt.Columns[7].ColumnName = "PROCESS_INDICATOR";

                dt.DefaultView.Sort = "[SOURCE_TABLE] ASC, [TARGET_TABLE] ASC, [BUSINESS_KEY_ATTRIBUTE] ASC";

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
        /// Populates the datagrid directly from a database or an existing JSON file
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

                var versionList = GetDataTable(ref connOmd, sqlStatementForLatestVersion.ToString());
                
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
                    ClassJsonHandling.CreateDummyJsonFile(GlobalParameters.JsonAttributeMappingFileName);
                }

                // Load the file, convert it to a DataTable and bind it to the source
                List<AttributeMappingJson> jsonArray = JsonConvert.DeserializeObject<List<AttributeMappingJson>>(File.ReadAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonAttributeMappingFileName+FileConfiguration.jsonVersionExtension));
                DataTable dt = ConvertToDataTable(jsonArray);
                dt.AcceptChanges(); //Make sure the changes are seen as committed, so that changes can be detected later on
                dt.Columns[0].ColumnName = "ATTRIBUTE_MAPPING_HASH";
                dt.Columns[1].ColumnName = "VERSION_ID";
                dt.Columns[2].ColumnName = "SOURCE_TABLE";
                dt.Columns[3].ColumnName = "SOURCE_COLUMN";
                dt.Columns[4].ColumnName = "TARGET_TABLE";
                dt.Columns[5].ColumnName = "TARGET_COLUMN";
                dt.Columns[6].ColumnName = "TRANSFORMATION_RULE";

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
            GlobalParameters.currentVersionId = trackBarVersioning.Value;
            
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

            // Remove all metadata from repository
            if (checkBoxClearMetadata.Checked)
            {
                TruncateMetadata();
            }


            // Check if the current version is the maximum version. At this stage updates on earlier versions are not supported (and cause a NULLreference exception)
            var highestVersion = GlobalParameters.highestVersionId;
            var currentVersion = GlobalParameters.currentVersionId;

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
            GlobalParameters.currentVersionId = versionId;
            GlobalParameters.highestVersionId = versionId;
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

                        var hashKey = CreateMd5(versionId + '|' + tableName + '|' + columnName);
                      
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

                    var hashKey =
                        CreateMd5(versionId + '|' + stagingTable + '|' + integrationTable + '|' + businessKeyDefinition + '|' + drivingKeyDefinition + '|' + filterCriterion);

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

                    //Generate a unique key using a hash
                    var hashKey = CreateMd5(versionId + '|' + stagingTable + '|' + stagingColumn + '|' + integrationTable + '|' + integrationColumn + '|' + transformationRule);
                    
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
                ClassJsonHandling.RemoveExistingJsonFile(GlobalParameters.JsonTableMappingFileName + @"_v" + GlobalParameters.currentVersionId + ".json");
                ClassJsonHandling.CreatePlaceholderJsonFile(GlobalParameters.JsonTableMappingFileName);
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

                                //Generate a unique key using a hash
                                var hashKey = CreateMd5(versionId + '|' + stagingTable + '|' + integrationTable + '|' + businessKeyDefinition + '|' + drivingKeyDefinition + '|' + filterCriterion);

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
                ClassJsonHandling.RemoveExistingJsonFile(GlobalParameters.JsonModelMetadataFileName + @"_v" + GlobalParameters.currentVersionId + ".json");
                ClassJsonHandling.CreatePlaceholderJsonFile(GlobalParameters.JsonModelMetadataFileName);
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
                                    var hashKey = CreateMd5(versionId +'|' + tableName + '|' + columnName);


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
                ClassJsonHandling.RemoveExistingJsonFile(GlobalParameters.JsonAttributeMappingFileName + @"_v" + GlobalParameters.currentVersionId + ".json");
                ClassJsonHandling.CreatePlaceholderJsonFile(GlobalParameters.JsonAttributeMappingFileName);
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

                                    //Generate a unique key using a hash
                                    var hashKey = CreateMd5(versionId +'|' + stagingTable + '|' + stagingColumn + '|' + integrationTable + '|' +integrationColumn + '|' + transformationRule);

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
            DataTable dt = ConvertToDataTable(jsonArray);
            dt.AcceptChanges(); //Make sure the changes are seen as committed, so that changes can be detected later on
            dt.Columns[0].ColumnName = "TABLE_MAPPING_HASH";
            dt.Columns[1].ColumnName = "VERSION_ID";
            dt.Columns[2].ColumnName = "SOURCE_TABLE";
            dt.Columns[3].ColumnName = "TARGET_TABLE";
            dt.Columns[4].ColumnName = "BUSINESS_KEY_ATTRIBUTE";
            dt.Columns[5].ColumnName = "DRIVING_KEY_ATTRIBUTE";
            dt.Columns[6].ColumnName = "FILTER_CRITERIA";
            dt.Columns[7].ColumnName = "PROCESS_INDICATOR";
            _bindingSourceTableMetadata.DataSource = dt;
        }

        private void BindAttributeMappingJsonToDataTable()
        {
            var versionId = CreateOrRetrieveVersion();
            var JsonVersionExtension = @"_v" + versionId + ".json";

            // Load the attribute mapping file, convert it to a DataTable and bind it to the source
            List<AttributeMappingJson> jsonArray = JsonConvert.DeserializeObject<List<AttributeMappingJson>>(File.ReadAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonAttributeMappingFileName + JsonVersionExtension));
            DataTable dt = ConvertToDataTable(jsonArray);
            dt.AcceptChanges(); //Make sure the changes are seen as committed, so that changes can be detected later on
            dt.Columns[0].ColumnName = "ATTRIBUTE_MAPPING_HASH";
            dt.Columns[1].ColumnName = "VERSION_ID";
            dt.Columns[2].ColumnName = "SOURCE_TABLE";
            dt.Columns[3].ColumnName = "SOURCE_COLUMN";
            dt.Columns[4].ColumnName = "TARGET_TABLE";
            dt.Columns[5].ColumnName = "TARGET_COLUMN";
            dt.Columns[6].ColumnName = "TRANSFORMATION_RULE";
            _bindingSourceAttributeMetadata.DataSource = dt;
        }

        private void BindModelMetadataJsonToDataTable()
        {
            var versionId = CreateOrRetrieveVersion();
            var JsonVersionExtension = @"_v" + versionId + ".json";

            // Load the table mapping file, convert it to a DataTable and bind it to the source
            List<PhysicalModelMetadataJson> jsonArray = JsonConvert.DeserializeObject<List<PhysicalModelMetadataJson>>(File.ReadAllText(GlobalParameters.ConfigurationPath + GlobalParameters.JsonModelMetadataFileName + JsonVersionExtension));
            DataTable dt = ConvertToDataTable(jsonArray);
            dt.AcceptChanges(); //Make sure the changes are seen as committed, so that changes can be detected later on
            dt.Columns[0].ColumnName = "VERSION_ATTRIBUTE_HASH";
            dt.Columns[1].ColumnName = "VERSION_ID";
            dt.Columns[2].ColumnName = "DATABASE_NAME";
            dt.Columns[3].ColumnName = "SCHEMA_NAME";
            dt.Columns[4].ColumnName = "TABLE_NAME";
            dt.Columns[5].ColumnName = "COLUMN_NAME";
            dt.Columns[6].ColumnName = "DATA_TYPE";
            dt.Columns[7].ColumnName = "CHARACTER_MAXIMUM_LENGTH";
            dt.Columns[8].ColumnName = "NUMERIC_PRECISION";
            dt.Columns[9].ColumnName = "ORDINAL_POSITION";
            dt.Columns[10].ColumnName = "PRIMARY_KEY_INDICATOR";
            dt.Columns[11].ColumnName = "MULTI_ACTIVE_INDICATOR";
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
                                var backupFile = new ClassJsonHandling();
                                var targetFileName = backupFile.BackupJsonFile(GlobalParameters.JsonTableMappingFileName + @"_v" + GlobalParameters.currentVersionId +".json");
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
                        DataTable dt = ConvertToDataTable(jsonArray);

                        // Setup the datatable with proper headings
                        dt.Columns[0].ColumnName = "TABLE_MAPPING_HASH";
                        dt.Columns[1].ColumnName = "VERSION_ID";
                        dt.Columns[2].ColumnName = "SOURCE_TABLE";
                        dt.Columns[3].ColumnName = "TARGET_TABLE";
                        dt.Columns[4].ColumnName = "BUSINESS_KEY_ATTRIBUTE";
                        dt.Columns[5].ColumnName = "DRIVING_KEY_ATTRIBUTE";
                        dt.Columns[6].ColumnName = "FILTER_CRITERIA";
                        dt.Columns[7].ColumnName = "PROCESS_INDICATOR";

                        // Sort the columns
                        dt.DefaultView.Sort = "[SOURCE_TABLE] ASC, [TARGET_TABLE] ASC, [BUSINESS_KEY_ATTRIBUTE] ASC";

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

        private void saveBusinessKeyMetadataFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var theDialog = new SaveFileDialog
                {
                    Title = @"Save Business Key Metadata File",
                    Filter = @"XML files|*.xml",
                    InitialDirectory =  GlobalParameters.ConfigurationPath //Application.StartupPath + @"\Configuration\"
                };

                var ret = STAShowDialog(theDialog);

                if (ret == DialogResult.OK)
                {
                    try
                    {
                        var chosenFile = theDialog.FileName;
                      
                        DataTable gridDataTable = (DataTable) _bindingSourceTableMetadata.DataSource;

                        gridDataTable.TableName = "TableMappingMetadata";

                        gridDataTable.WriteXml(chosenFile);
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
                MessageBox.Show("A problem occure when attempting to save the file to disk. The detail error message is: " + ex.Message);
            }   
        }

        private void saveAttributeMetadataMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var theDialog = new SaveFileDialog
                {
                    Title = @"Save Attribute Mapping Metadata File",
                    Filter = @"XML files|*.xml",
                    InitialDirectory = GlobalParameters.ConfigurationPath //Application.StartupPath + @"\Configuration\"
                };


                var ret = STAShowDialog(theDialog);

                if (ret == DialogResult.OK)
                {
                    try
                    {
                        var chosenFile = theDialog.FileName;

                        DataTable gridDataTable = (DataTable)_bindingSourceAttributeMetadata.DataSource;

                        gridDataTable.TableName = "AttributeMappingMetadata";

                        gridDataTable.WriteXml(chosenFile);
                        richTextBoxInformation.Text = "The attribute mapping file " + chosenFile + " saved successfully.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A problem occure when attempting to save the file to disk. The detail error message is: " + ex.Message);
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
                                var backupFile = new ClassJsonHandling();
                                var targetFileName = backupFile.BackupJsonFile(GlobalParameters.JsonAttributeMappingFileName + @"_v" + GlobalParameters.currentVersionId + ".json");
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
                    DataTable dt = ConvertToDataTable(jsonArray);

                    dt.Columns[0].ColumnName = "ATTRIBUTE_MAPPING_HASH";
                    dt.Columns[1].ColumnName = "VERSION_ID";
                    dt.Columns[2].ColumnName = "SOURCE_TABLE";
                    dt.Columns[3].ColumnName = "SOURCE_COLUMN";
                    dt.Columns[4].ColumnName = "TARGET_TABLE";
                    dt.Columns[5].ColumnName = "TARGET_COLUMN";
                    dt.Columns[6].ColumnName = "TRANSFORMATION_RULE";

                    // Sort the columns
                    dt.DefaultView.Sort = "[SOURCE_TABLE] ASC, [SOURCE_COLUMN] ASC, [TARGET_TABLE] ASC, [TARGET_COLUMN] ASC";

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

        private void checkBoxClearMetadata_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxClearMetadata.Checked)
            {
                MessageBox.Show("Selection this option will mean that all metadata will be truncated.", "Clear metadata", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                if (row["SOURCE_COLUMN"] != DBNull.Value);
                    SOURCE_COLUMN = (string)row["SOURCE_COLUMN"];
                if (row["TARGET_TABLE"] != DBNull.Value)
                    targetTable = (string)row["TARGET_TABLE"];
                if (row["TARGET_COLUMN"] != DBNull.Value)
                    TARGET_COLUMN = (string)row["TARGET_COLUMN"];
                if (row["TRANSFORMATION_RULE"] != DBNull.Value)
                    TRANSFORMATION_RULE = (string)row["TRANSFORMATION_RULE"];

                var fullyQualifiedSourceName = ClassMetadataHandling.getFullSchemaTable(sourceTable);
                var sourceType = ClassMetadataHandling.GetTableType(sourceTable);

                var fullyQualifiedTargetName = ClassMetadataHandling.getFullSchemaTable(targetTable);
                var targetType = ClassMetadataHandling.GetTableType(targetTable);

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
                if (row["BUSINESS_KEY_ATTRIBUTE"] != DBNull.Value) ;
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

                var fullyQualifiedSourceName = ClassMetadataHandling.getFullSchemaTable(sourceTable);
                var sourceType = ClassMetadataHandling.GetTableType(sourceTable);

                var fullyQualifiedTargetName = ClassMetadataHandling.getFullSchemaTable(targetTable);
                var targetType = ClassMetadataHandling.GetTableType(targetTable);

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
                catch (Exception ex)
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
                if (checkBoxIgnoreVersion.Checked == false && _bindingSourcePhysicalModelMetadata.Count == 0)
                {
                    richTextBoxInformation.Text += "There is no physical model metadata available, so the metadata can only be validated with the 'Ignore Version' enabled.\r\n ";
                }
                else
                {
                    if (backgroundWorkerValidationOnly.IsBusy) return;
                    // create a new instance of the alert form
                    _alertValidation = new FormAlert();
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

            // After validation finishes, the activtation thread / process should start.
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

                if (checkBoxIgnoreVersion.Checked == false)
                {
                    var versionExistenceCheck = new StringBuilder();

                    versionExistenceCheck.AppendLine("SELECT * FROM TMP_MD_VERSION_ATTRIBUTE WHERE VERSION_ID = " + trackBarVersioning.Value);

                    var versionExistenceCheckDataTable = GetDataTable(ref connOmd, versionExistenceCheck.ToString());

                    if (versionExistenceCheckDataTable != null && versionExistenceCheckDataTable.Rows.Count > 0)
                    {
                        if (backgroundWorkerMetadata.IsBusy) return;
                        // create a new instance of the alert form
                        _alert = new FormAlert();
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
                    _alert = new FormAlert();
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
                _alert.Close();
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
                richTextBoxInformation.Text += "The metadata was processed succesfully!\r\n";
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
            BackgroundWorker worker = sender as BackgroundWorker;

            var inputTable = (DataTable)_bindingSourceTableMetadata.DataSource;

            var errorLog = new StringBuilder();
            var errorCounter = new int();

            var connOmd = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };
            var metaDataConnection = ConfigurationSettings.ConnectionStringOmd;

            // Get everything as local variables to reduce multithreading issues
            var stagingDatabase = '[' + ConfigurationSettings.StagingDatabaseName + ']';
            var psaDatabase = '[' + ConfigurationSettings.PsaDatabaseName+ ']';
            var integrationDatabase = '['+ ConfigurationSettings.IntegrationDatabaseName + ']';
            var presentationDatabase = '[' + ConfigurationSettings.PresentationDatabaseName + ']';

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
            var lsatTablePrefix = ConfigurationSettings.LsatPrefixValue;

            if (ConfigurationSettings.TableNamingLocation=="Prefix")
            {
                stagingPrefix = stagingPrefix + "_%";
                psaPrefix = psaPrefix + "_%";
                hubTablePrefix = hubTablePrefix + "_%";
                lnkTablePrefix = lnkTablePrefix + "_%";
                satTablePrefix = satTablePrefix + "_%";
                lsatTablePrefix = lsatTablePrefix + "_%";
            }
            else
            {
                stagingPrefix = "%_" + stagingPrefix;
                psaPrefix = "%_" + psaPrefix;
                hubTablePrefix = "%_" + hubTablePrefix;
                lnkTablePrefix = "%_" + lnkTablePrefix;
                satTablePrefix = "%_" + satTablePrefix;
                lsatTablePrefix = "%_" + lsatTablePrefix;
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

            // Handling multithreading
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
                var queryMode = "";
                if (checkBoxIgnoreVersion.Checked)
                {
                    queryMode = "physical";
                }
                else
                {
                    queryMode = "virtual";
                }

                _alert.SetTextLogging("Commencing metadata preparation / activation for version " + majorVersion + "." + minorVersion + ".\r\n\r\n");

                // Alerting the user what kind of metadata is prepared
                _alert.SetTextLogging(queryMode == "physical"
                    ? "The 'ignore model version' option is selected. This means when possible the live database (tables and attributes) will be used in conjunction with the Data Vault metadata. In other words, the model versioning is ignored.\r\n\r\n"
                    : "Metadata is prepared using the selected version for both the Data Vault metadata as well as the model metadata.\r\n\r\n");
                #endregion

                #region Delete Metadata - 5%
                // 1. Deleting metadata
                _alert.SetTextLogging("Commencing removal of existing metadata.\r\n");

                var deleteStatement = new StringBuilder();
                deleteStatement.AppendLine( @"
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

                        if (worker != null) worker.ReportProgress(5);
                        _alert.SetTextLogging("Removal of existing metadata completed.\r\n");
                    }
                    catch (Exception ex)
                    {
                        errorCounter++;
                        _alert.SetTextLogging("An issue has occured during removal of old metadata. Please check the Error Log for more details.\r\n");
                        errorLog.AppendLine("\r\nAn issue has occured during removal of old metadata: \r\n\r\n" + ex);
                    }
                }
                # endregion

                # region Prepare Version Information - 7%
                // 2. Prepare Version
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the version metadata.\r\n");

                try
                {
                    var versionName = string.Concat(majorVersion, '.', minorVersion);

                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        _alert.SetTextLogging("-->  Working on committing version " + versionName + " to the metadata repository.\r\n");

                        var insertVersionStatement = new StringBuilder();
                        insertVersionStatement.AppendLine("INSERT INTO [MD_MODEL_METADATA]");
                        insertVersionStatement.AppendLine("([VERSION_NAME],[ACTIVATION_DATETIME])");
                        insertVersionStatement.AppendLine("VALUES ('" + versionName + "','" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz") + "')");

                        var command = new SqlCommand(insertVersionStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            errorCounter++;
                            _alert.SetTextLogging("An issue has occured during preparation of the version information. Please check the Error Log for more details.\r\n");
                            errorLog.AppendLine("\r\nAn issue has occured during preparation of the version information: \r\n\r\n" + ex);
                        }
                    }

                    if (worker != null) worker.ReportProgress(10);
                    _alert.SetTextLogging("Preparation of the version details completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the version details. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the version: \r\n\r\n" + ex);
                }

                #endregion

                # region Prepare Source - 10%
                // 2. Prepare STG
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the source metadata.\r\n");

                try
                {
                    var stgCounter = 1;

                    // Getting the distinct list of tables to go into the 'source'
                    //DataRow[] selectionRows = inputTable.Select("PROCESS_INDICATOR = 'Y' AND (SOURCE_TABLE LIKE '" + stagingPrefix + "' OR SOURCE_TABLE LIKE '" + psaPrefix + "')");
                    DataRow[] selectionRows = inputTable.Select("PROCESS_INDICATOR = 'Y'");

                    var distinctList = new List<string>();

                    // Create a dummy row
                    distinctList.Add("Not applicable");

                    // Create a distinct list of sources from the datagrid
                    foreach (DataRow row in selectionRows)
                    {
                        string source_table = row["SOURCE_TABLE"].ToString().Trim();
                        if (!distinctList.Contains(source_table))
                        {
                            distinctList.Add(source_table);
                        }
                    }

                    // Add the list of sources to the MD_SOURCE table
                    foreach (var tableName in distinctList)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            if (tableName != "Not applicable")
                            {
                                _alert.SetTextLogging("--> " + tableName + "\r\n");
                            }

                            var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                            var insertStgStatement = new StringBuilder();
                            insertStgStatement.AppendLine("INSERT INTO [MD_SOURCE]");
                            insertStgStatement.AppendLine("([SOURCE_NAME],[SOURCE_ID], [SCHEMA_NAME])");
                            insertStgStatement.AppendLine("VALUES ('" + fullyQualifiedName.Value + "'," + stgCounter + ", '"+ fullyQualifiedName.Key + "')");

                            var command = new SqlCommand(insertStgStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                stgCounter++;
                            }
                            catch (Exception ex)
                            {
                                errorCounter++;
                                _alert.SetTextLogging("An issue has occured during preparation of the source metadata. Please check the Error Log for more details.\r\n");
                                errorLog.AppendLine("\r\nAn issue has occured during preparation of the source metadata: \r\n\r\n" + ex);
                            }
                        }
                    }

                    if (worker != null) worker.ReportProgress(10);
                    _alert.SetTextLogging("Preparation of the source metadata completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the source metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the source metadata: \r\n\r\n" + ex);
                }

                #endregion

                # region Prepare Hubs - 15%
                //3. Prepare Hubs
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Hub metadata.\r\n");

                try
                {
                    var hubCounter = 1; 

                    // Getting the distinct list of tables to go into the MD_HUB table
                    DataRow[] selectionRows = inputTable.Select("PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '%" + hubTablePrefix + "%'");

                    var distinctList = new List<string>();

                    // Create a dummy row
                    distinctList.Add("Not applicable");

                    // Create a distinct list of sources from the datagrid
                    foreach (DataRow row in selectionRows)
                    {
                        string target_table = (string)row["TARGET_TABLE"].ToString().Trim();
                        if (!distinctList.Contains(target_table))
                        {
                            distinctList.Add(target_table);
                        }
                    }

                    // Process the unique Hub records
                    foreach (var tableName in distinctList)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            if (tableName != "Not applicable")
                            {
                                _alert.SetTextLogging("--> " + tableName + "\r\n");
                            }

                            var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                            // Retrieve the business key
                            var hubBusinessKey = ClassMetadataHandling.GetHubTargetBusinessKeyList(fullyQualifiedName.Key, fullyQualifiedName.Value, versionId, queryMode);
                            string businessKeyString = string.Join(",", hubBusinessKey);

                            var insertHubStatement = new StringBuilder();
                            insertHubStatement.AppendLine("INSERT INTO [MD_HUB]");
                            insertHubStatement.AppendLine("([HUB_NAME],[HUB_ID], [SCHEMA_NAME], [BUSINESS_KEY])");
                            insertHubStatement.AppendLine("VALUES ('" + fullyQualifiedName.Value + "'," + hubCounter + ",'"+fullyQualifiedName.Key+"', '"+ businessKeyString + "')");

                            var command = new SqlCommand(insertHubStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                hubCounter++;
                            }
                            catch (Exception ex)
                            {
                                errorCounter++;
                                _alert.SetTextLogging("An issue has occured during preparation of the Hubs. Please check the Error Log for more details.\r\n");
                                errorLog.AppendLine("\r\nAn issue has occured during preparation of the Hubs: \r\n\r\n" + ex);
                            }
                        }
                    }

                    if (worker != null) worker.ReportProgress(15);
                    _alert.SetTextLogging("Preparation of the Hub metadata completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the Hubs. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the Hubs: \r\n\r\n" + ex);
                }
                #endregion

                #region Prepare Links - 20%
                //4. Prepare links
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Link metadata.\r\n");

                try
                {
                    var linkCounter = 1;

                    // Getting the distinct list of tables to go into the MD_LINK table
                    DataRow[] selectionRows = inputTable.Select("PROCESS_INDICATOR = 'Y' AND TARGET_TABLE LIKE '%" + lnkTablePrefix + "%'");

                    var distinctList = new List<string>();

                    // Create a dummy row
                    distinctList.Add("Not applicable");

                    // Create a distinct list of sources from the datagrid
                    foreach (DataRow row in selectionRows)
                    {
                        string target_table = row["TARGET_TABLE"].ToString().Trim();
                        if (!distinctList.Contains(target_table))
                        {
                            distinctList.Add(target_table);
                        }
                    }

                    // Insert the rest of the rows
                    foreach (var tableName in distinctList)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            if (tableName != "Not applicable")
                            {
                                _alert.SetTextLogging("--> " + tableName + "\r\n");
                            }

                            var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                            var insertLinkStatement = new StringBuilder();

                            insertLinkStatement.AppendLine("INSERT INTO [MD_LINK]");
                            insertLinkStatement.AppendLine("([LINK_NAME],[LINK_ID], [SCHEMA_NAME])");
                            insertLinkStatement.AppendLine("VALUES ('" + fullyQualifiedName.Value + "'," + linkCounter + ",'"+fullyQualifiedName.Key+"')");

                            var command = new SqlCommand(insertLinkStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                linkCounter++;
                            }
                            catch (Exception ex)
                            {
                                errorCounter++;
                                _alert.SetTextLogging("An issue has occured during preparation of the Links. Please check the Error Log for more details.\r\n");
                                errorLog.AppendLine("\r\nAn issue has occured during preparation of the Links: \r\n\r\n" + ex);
                            }
                        }
                    }

                    if (worker != null) worker.ReportProgress(20);
                    _alert.SetTextLogging("Preparation of the Link metadata completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the Links. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the Links: \r\n\r\n" + ex);
                }
                #endregion

                #region Prepare Satellites - 24%
                //5.1 Prepare Satellites
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Satellite metadata.\r\n");

                var satCounter = 1;

                try
                {
                    var prepareSatStatement = new StringBuilder();

                    // STILL NOT USING DATAGRID
                    prepareSatStatement.AppendLine(@"
                                                    SELECT DISTINCT
                                                           spec.TARGET_TABLE AS SATELLITE_NAME,
                                                           hubkeysub.HUB_ID,
                                                           'Normal' AS SATELLITE_TYPE,
                                                           (SELECT LINK_ID FROM MD_LINK WHERE LINK_NAME='Not applicable') AS LINK_ID -- No link for normal Satellites 
                                                    FROM TMP_MD_TABLE_MAPPING spec 
                                                    LEFT OUTER JOIN 
                                                    ( 
                                                           SELECT DISTINCT TARGET_TABLE, hub.HUB_ID, SOURCE_TABLE, BUSINESS_KEY_ATTRIBUTE 
                                                           FROM TMP_MD_TABLE_MAPPING spec2 
                                                           LEFT OUTER JOIN -- Join in the Hub ID from the MD table 
                                                                 MD_HUB hub ON hub.[SCHEMA_NAME]+'.'+hub.HUB_NAME=spec2.TARGET_TABLE 
                                                        WHERE TARGET_TABLE_TYPE = 'Hub'
                                                        AND [PROCESS_INDICATOR] = 'Y'                                                        
                                                    ) hubkeysub 
                                                           ON spec.SOURCE_TABLE=hubkeysub.SOURCE_TABLE 
                                                           AND replace(spec.BUSINESS_KEY_ATTRIBUTE,' ','')=replace(hubkeysub.BUSINESS_KEY_ATTRIBUTE,' ','') 
                                                    WHERE spec.TARGET_TABLE_TYPE = 'Satellite'
                                                    AND [PROCESS_INDICATOR] = 'Y'
                                                    ");

                    var listSat = GetDataTable(ref connOmd, prepareSatStatement.ToString());

                    foreach (DataRow satelliteName in listSat.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            var tableName = satelliteName["SATELLITE_NAME"].ToString().Trim();
                            var tableType = satelliteName["SATELLITE_TYPE"].ToString().Trim();
                            var hubId = satelliteName["HUB_ID"];
                            var linkId = satelliteName["LINK_ID"];

                            if (tableName != "Not applicable")
                            {
                                _alert.SetTextLogging("--> " + tableName + "\r\n");
                            }

                            var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                            var insertSatStatement = new StringBuilder();
                            insertSatStatement.AppendLine("INSERT INTO [MD_SATELLITE]");
                            insertSatStatement.AppendLine("([SATELLITE_NAME],[SATELLITE_ID], [SATELLITE_TYPE], [SCHEMA_NAME], [HUB_ID], [LINK_ID])");
                            insertSatStatement.AppendLine("VALUES ('" + fullyQualifiedName.Value+ "'," + satCounter + ",'" + tableType + "', '"+fullyQualifiedName.Key+"', " + hubId + "," + linkId + ")");

                            var command = new SqlCommand(insertSatStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                satCounter++;
                            }
                            catch (Exception ex)
                            {
                                errorCounter++;
                                _alert.SetTextLogging("An issue has occured during preparation of the Satellites. Please check the Error Log for more details.\r\n");
                                errorLog.AppendLine("\r\nAn issue has occured during preparation of the Satellites: \r\n\r\n" + ex);
                            }
                        }
                    }

                    worker.ReportProgress(24);
                    _alert.SetTextLogging("Preparation of the Satellite metadata completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the Satellites. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the Satellites: \r\n\r\n" + ex);
                }
                #endregion

                #region Prepare Link Satellites - 28%
                //5.2 Prepare Link Satellites
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Link Satellite metadata.\r\n");
                //satCounter = 1;

                try
                {
                    var prepareSatStatement = new StringBuilder();
                    /*LBM 2019/01/10 - Changing to use @ Strings*/
                    prepareSatStatement.AppendLine(@"
                                                    SELECT DISTINCT
                                                           spec.TARGET_TABLE AS SATELLITE_NAME, 
                                                           (SELECT HUB_ID FROM MD_HUB WHERE HUB_NAME='Not applicable') AS HUB_ID, -- No Hub for Link Satellites
                                                           'Link Satellite' AS SATELLITE_TYPE,
                                                           lnkkeysub.LINK_ID
                                                    FROM TMP_MD_TABLE_MAPPING spec
                                                    LEFT OUTER JOIN  -- Get the Link ID that belongs to this LSAT
                                                    (
                                                           SELECT DISTINCT 
                                                                 TARGET_TABLE AS LINK_NAME,
                                                                 SOURCE_TABLE,
                                                                 BUSINESS_KEY_ATTRIBUTE,
                                                           lnk.LINK_ID
                                                           FROM TMP_MD_TABLE_MAPPING spec2
                                                           LEFT OUTER JOIN -- Join in the Link ID from the MD table
                                                                 MD_LINK lnk ON lnk.[SCHEMA_NAME]+'.'+lnk.LINK_NAME=spec2.TARGET_TABLE
                                                           WHERE TARGET_TABLE_TYPE = 'Link'
                                                           AND [PROCESS_INDICATOR] = 'Y'
                                                    ) lnkkeysub
                                                        ON spec.SOURCE_TABLE=lnkkeysub.SOURCE_TABLE -- Only the combination of Link table and Business key can belong to the LSAT
                                                       AND REPLACE(spec.BUSINESS_KEY_ATTRIBUTE,' ','')=REPLACE(lnkkeysub.BUSINESS_KEY_ATTRIBUTE,' ','')

                                                    -- Only select Link Satellites as the base / driving table (spec alias)
                                                    WHERE spec.TARGET_TABLE_TYPE LIKE 'Link-Satellite'
                                                    AND [PROCESS_INDICATOR] = 'Y'
                                                    ");
 

                     var listSat = GetDataTable(ref connOmd, prepareSatStatement.ToString());

                    foreach (DataRow satelliteName in listSat.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            var tableName = satelliteName["SATELLITE_NAME"].ToString().Trim();
                            var tableType = satelliteName["SATELLITE_TYPE"].ToString().Trim();
                            var hubId = satelliteName["HUB_ID"];
                            var linkId = satelliteName["LINK_ID"];

                            _alert.SetTextLogging("--> " + tableName + "\r\n");

                            var fullyQualifiedName = ClassMetadataHandling.GetSchema(tableName).FirstOrDefault();

                            var insertSatStatement = new StringBuilder();
                            insertSatStatement.AppendLine("INSERT INTO [MD_SATELLITE]");
                            insertSatStatement.AppendLine("([SATELLITE_NAME],[SATELLITE_ID], [SATELLITE_TYPE], [SCHEMA_NAME], [HUB_ID], [LINK_ID])");
                            insertSatStatement.AppendLine("VALUES ('" + fullyQualifiedName.Value + "'," + satCounter + ",'" + tableType + "', '" + fullyQualifiedName.Key + "', " + hubId + "," + linkId + ")");

                            var command = new SqlCommand(insertSatStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                satCounter++;
                            }
                            catch (Exception ex)
                            {
                                errorCounter++;
                                _alert.SetTextLogging("An issue has occured during preparation of the Link Satellites. Please check the Error Log for more details.\r\n");
                                errorLog.AppendLine("\r\nAn issue has occured during preparation of the Link Satellites: \r\n\r\n" + ex);
                            }
                        }
                    }

                    worker.ReportProgress(28);
                    _alert.SetTextLogging("Preparation of the Link Satellite metadata completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the Link Satellites. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the Link Satellites: \r\n\r\n" + ex);
                }
                #endregion

                #region Prepare STG / SAT Xref - 28%
                //5.3 Prepare STG / Sat XREF
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the relationship between (Link) Satellites and the Source tables.\r\n");
                var srcSatXrefCounter = 1;

                try
                {
                    var prepareSatXrefStatement = new StringBuilder();
                    /*LBM 2019/01/10 - Changing to use @ Strings*/
                    prepareSatXrefStatement.AppendLine(@"
                                                        SELECT
                                                               sat.SATELLITE_ID,
	                                                           sat.SATELLITE_NAME,
                                                               stg.SOURCE_ID, 
	                                                           stg.SOURCE_NAME,
	                                                           spec.BUSINESS_KEY_ATTRIBUTE,
                                                               spec.FILTER_CRITERIA
                                                        FROM TMP_MD_TABLE_MAPPING spec
                                                        LEFT OUTER JOIN -- Join in the Source_ID from the MD_SOURCE table
                                                               MD_SOURCE stg ON stg.[SCHEMA_NAME]+'.'+stg.SOURCE_NAME=spec.SOURCE_TABLE
                                                        LEFT OUTER JOIN -- Join in the Satellite_ID from the MD_SATELLITE table
                                                               MD_SATELLITE sat ON sat.[SCHEMA_NAME]+'.'+sat.SATELLITE_NAME=spec.TARGET_TABLE
                                                        WHERE spec.TARGET_TABLE_TYPE = 'Satellite'
                                                        AND [PROCESS_INDICATOR] = 'Y'
                                                        UNION
                                                        SELECT
                                                               sat.SATELLITE_ID,
	                                                           sat.SATELLITE_NAME,
                                                               stg.SOURCE_ID, 
	                                                           stg.SOURCE_NAME,
	                                                           spec.BUSINESS_KEY_ATTRIBUTE,
                                                               spec.FILTER_CRITERIA
                                                        FROM TMP_MD_TABLE_MAPPING spec
                                                        LEFT OUTER JOIN -- Join in the Source_ID from the MD_SOURCE table
                                                               MD_SOURCE stg ON stg.[SCHEMA_NAME]+'.'+stg.SOURCE_NAME=spec.SOURCE_TABLE
                                                        LEFT OUTER JOIN -- Join in the Satellite_ID from the MD_SATELLITE table
                                                               MD_SATELLITE sat ON sat.[SCHEMA_NAME]+'.'+sat.SATELLITE_NAME=spec.TARGET_TABLE
                                                        WHERE spec.TARGET_TABLE_TYPE = 'Link-Satellite'
                                                        AND [PROCESS_INDICATOR] = 'Y'
                                                        ");


                    var listSat = GetDataTable(ref connOmd, prepareSatXrefStatement.ToString());

                    foreach (DataRow tableName in listSat.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            _alert.SetTextLogging("-->  Processing the " + tableName["SOURCE_NAME"] + " / " + tableName["SATELLITE_NAME"] + " relationship\r\n");

                            var insertSatStatement = new StringBuilder();
                            var filterCriterion = tableName["FILTER_CRITERIA"].ToString().Trim();
                            filterCriterion = filterCriterion.Replace("'", "''");

                            var businessKeyDefinition = tableName["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                            businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                            var loadVector = ClassMetadataHandling.GetLoadVector(tableName["SOURCE_NAME"].ToString(), tableName["SATELLITE_NAME"].ToString());
                            
                            insertSatStatement.AppendLine("INSERT INTO [MD_SOURCE_SATELLITE_XREF]");
                            insertSatStatement.AppendLine("([SATELLITE_ID], [SOURCE_ID], [BUSINESS_KEY_DEFINITION], [FILTER_CRITERIA], [LOAD_VECTOR])");
                            insertSatStatement.AppendLine("VALUES ('" + 
                                                          tableName["SATELLITE_ID"] + "','" + 
                                                          tableName["SOURCE_ID"] + "','" + 
                                                          businessKeyDefinition + "','" +
                                                          filterCriterion + "','" +
                                                          loadVector + "')");

                            var command = new SqlCommand(insertSatStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                srcSatXrefCounter++;
                            }
                            catch (Exception ex)
                            {
                                errorCounter++;
                                _alert.SetTextLogging("An issue has occured during preparation of the relationship between the Source and the Satellite. Please check the Error Log for more details.\r\n");
                                errorLog.AppendLine("\r\nAn issue has occured during preparation of the Source / Satellite XREF: \r\n\r\n" + ex);
                            }
                        }
                    }

                    worker.ReportProgress(28);
                    _alert.SetTextLogging("Preparation of the Source / Satellite XREF metadata completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the relationship between the Source and the Satellite. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the Source / Satellite XREF: \r\n\r\n" + ex);
                }

                #endregion

                #region Staging / Hub relationship - 30%
                //6. Prepare STG / HUB xref
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the relationship between Source and Hubs.\r\n");

                try
                {
                    var prepareStgHubXrefStatement = new StringBuilder();
                    /*LBM 2019/01/10 - Changing to use @ Strings*/
                    prepareStgHubXrefStatement.AppendLine(@"
                    SELECT
                      HUB_ID,
                      HUB_NAME,
                      SOURCE_ID,
	                  SOURCE_NAME,
	                  BUSINESS_KEY_ATTRIBUTE,
                      FILTER_CRITERIA
                    FROM
                    (      
                      SELECT DISTINCT 
                        SOURCE_TABLE,
                        TARGET_TABLE,
					    BUSINESS_KEY_ATTRIBUTE,
                        FILTER_CRITERIA
                      FROM TMP_MD_TABLE_MAPPING
                      WHERE 
                           TARGET_TABLE_TYPE = 'Hub'
                       AND [PROCESS_INDICATOR] = 'Y'
                    ) hub
                    LEFT OUTER JOIN
                    ( 
                      SELECT SOURCE_ID, [SCHEMA_NAME]+'.'+SOURCE_NAME AS SOURCE_NAME
                      FROM MD_SOURCE
                    ) stgsub
                    ON hub.SOURCE_TABLE=stgsub.SOURCE_NAME
                    LEFT OUTER JOIN
                    ( 
                      SELECT HUB_ID, [SCHEMA_NAME]+'.'+HUB_NAME AS HUB_NAME
                      FROM MD_HUB
                    ) hubsub
                    ON hub.TARGET_TABLE=hubsub.HUB_NAME
                    ");


                    var listXref = GetDataTable(ref connOmd, prepareStgHubXrefStatement.ToString());

                    foreach (DataRow tableName in listXref.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            _alert.SetTextLogging("-->  Processing the " + tableName["SOURCE_NAME"] + " / " + tableName["HUB_NAME"] + " relationship\r\n");

                            var insertXrefStatement = new StringBuilder();
                            var filterCriterion = tableName["FILTER_CRITERIA"].ToString().Trim();
                            filterCriterion = filterCriterion.Replace("'", "''");

                            var businessKeyDefinition = tableName["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                            businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                            var loadVector = ClassMetadataHandling.GetLoadVector(tableName["SOURCE_NAME"].ToString(), tableName["HUB_NAME"].ToString());

                            insertXrefStatement.AppendLine("INSERT INTO [MD_SOURCE_HUB_XREF]");
                            insertXrefStatement.AppendLine("([HUB_ID], [SOURCE_ID], [BUSINESS_KEY_DEFINITION], [FILTER_CRITERIA], [LOAD_VECTOR])");
                            insertXrefStatement.AppendLine("VALUES ('" + tableName["HUB_ID"] + 
                                                           "','" + tableName["SOURCE_ID"] + 
                                                           "','" + businessKeyDefinition + 
                                                           "','" + filterCriterion +
                                                           "','" + loadVector +
                                                           "')");

                            var command = new SqlCommand(insertXrefStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                errorCounter++;
                                _alert.SetTextLogging("An issue has occured during preparation of the relationship between the Source and the Hubs. Please check the Error Log for more details.\r\n");
                                errorLog.AppendLine("\r\nAn issue has occured during preparation of the Staging / Hub XREF: \r\n\r\n" + ex);
                            }
                        }
                    }

                    worker.ReportProgress(30);
                    _alert.SetTextLogging("Preparation of the relationship between Source and Hubs completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the relationship between the Source and the Hubs. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the Staging / Hub XREF: \r\n\r\n" + ex);
                }
                #endregion

                #region Filter Variables  - 35%
                string tableFilterQuery = @"SELECT DISTINCT [SOURCE_TABLE] AS [TABLE_NAME], [SOURCE_TABLE_TYPE] AS [TABLE_TYPE] FROM [TMP_MD_TABLE_MAPPING]
                                            UNION
                                            SELECT DISTINCT [TARGET_TABLE] AS [TABLE_NAME], [TARGET_TABLE_TYPE] AS [TABLE_TYPE] FROM [TMP_MD_TABLE_MAPPING]";

                // Filters need to be executed against specific databases, so each filter is setup to be used against a specific database connection
                var stgTableFilterObjects = "";
                var psaTableFilterObjects = "";
                var intTableFilterObjects = "";
                var presTableFilterObjects = "";
   
                var tableDataTable = GetDataTable(ref connOmd, tableFilterQuery);

                // Creating the filters
                int objectCounter = 1;
                foreach (DataRow tableRow in tableDataTable.Rows)
                {
                    // Get the right database for the table type (which can be anything including STG, PSA, base- and derived DV and Dimension or Facts)
                    string databaseName = ClassMetadataHandling.GetDatabaseForArea(tableRow["TABLE_TYPE"].ToString());

                    // Workaround to allow PSA tables to be reverse-engineered automatically by replacing the STG prefix/suffix
                    // I.e. when there are no PSA tables defined, they will be derived from the STG
                    var workingTableName = ClassMetadataHandling.nonQualifiedTableName(tableRow["TABLE_NAME"].ToString());
                    if (workingTableName.StartsWith(ConfigurationSettings.StgTablePrefixValue + "_") || workingTableName.EndsWith("_" + ConfigurationSettings.StgTablePrefixValue))
                    {
                        var tempTableName = tableRow["TABLE_NAME"].ToString().Replace(ConfigurationSettings.StgTablePrefixValue, ConfigurationSettings.PsaTablePrefixValue);
                        var tempTableType = "Persistent Staging Area";
                        string tempDatabaseName = ClassMetadataHandling.GetDatabaseForArea(tempTableType);
                        psaTableFilterObjects = psaTableFilterObjects + "OBJECT_ID(N'[" + tempDatabaseName + "]." + tempTableName + "') ,";
                    }

                    // Regular processing
                    if (databaseName == ConfigurationSettings.StagingDatabaseName)
                    { // Staging filter
                        stgTableFilterObjects = stgTableFilterObjects + "OBJECT_ID(N'[" + databaseName + "]." + tableRow["TABLE_NAME"] + "') ,";
                    }
                    else if (databaseName == ConfigurationSettings.PsaDatabaseName)
                    { // Persistent Staging Area filter
                        psaTableFilterObjects = psaTableFilterObjects + "OBJECT_ID(N'[" + databaseName + "]." + tableRow["TABLE_NAME"] + "') ,";
                    }
                    else if (databaseName == ConfigurationSettings.IntegrationDatabaseName)
                    { // Integration Layer filter
                        intTableFilterObjects = intTableFilterObjects + "OBJECT_ID(N'[" + databaseName + "]." + tableRow["TABLE_NAME"] + "') ,";
                    }
                    else if (databaseName == ConfigurationSettings.PresentationDatabaseName)
                    { // Presentation Layer filter
                        presTableFilterObjects = presTableFilterObjects + "OBJECT_ID(N'[" + databaseName + "]." + tableRow["TABLE_NAME"] + "') ,";
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
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Filter variables created successfully.\r\n");
                
                #endregion

                #region Prepare attributes - 40%
                //7. Prepare Attributes
                _alert.SetTextLogging("\r\n");

                // Define the master attribute list for reuse many times later on (assuming ignore version is active and hence the virtual mode is enabled)
                var allDatabaseAttributes = new StringBuilder();
                if (checkBoxIgnoreVersion.Checked
                ) // Get the attributes from the physical model / catalog. No virtualisation needed.
                {
                    var physicalModelInstantiation = new AttributeSelection();

                    allDatabaseAttributes.AppendLine("  SELECT * FROM");
                    allDatabaseAttributes.AppendLine("  (");

                    allDatabaseAttributes.AppendLine(physicalModelInstantiation.CreatePhysicalModelSet(ConfigurationSettings.StagingDatabaseName, stgTableFilterObjects).ToString());
                    allDatabaseAttributes.AppendLine("    UNION ALL");
                    allDatabaseAttributes.AppendLine(physicalModelInstantiation.CreatePhysicalModelSet(ConfigurationSettings.PsaDatabaseName, psaTableFilterObjects).ToString());
                    allDatabaseAttributes.AppendLine("    UNION ALL");
                    allDatabaseAttributes.AppendLine(physicalModelInstantiation.CreatePhysicalModelSet(ConfigurationSettings.IntegrationDatabaseName, intTableFilterObjects).ToString());
                    allDatabaseAttributes.AppendLine("    UNION ALL");
                    allDatabaseAttributes.AppendLine(physicalModelInstantiation.CreatePhysicalModelSet(ConfigurationSettings.PresentationDatabaseName, presTableFilterObjects).ToString());

                    allDatabaseAttributes.AppendLine("  ) mapping");
                }
                else // Get the values from the data grid or worker table (virtual mode)
                {
                    allDatabaseAttributes.AppendLine("SELECT ");
                    allDatabaseAttributes.AppendLine("  [DATABASE_NAME] ");
                    allDatabaseAttributes.AppendLine(" ,[SCHEMA_NAME]");
                    allDatabaseAttributes.AppendLine(" ,[TABLE_NAME]");
                    allDatabaseAttributes.AppendLine(" ,[COLUMN_NAME]");
                    allDatabaseAttributes.AppendLine(" ,[DATA_TYPE]");
                    allDatabaseAttributes.AppendLine(" ,[CHARACTER_MAXIMUM_LENGTH]");
                    allDatabaseAttributes.AppendLine(" ,[NUMERIC_PRECISION]");
                    allDatabaseAttributes.AppendLine(" ,[ORDINAL_POSITION]");
                    allDatabaseAttributes.AppendLine(" ,[PRIMARY_KEY_INDICATOR]");
                    allDatabaseAttributes.AppendLine("FROM [TMP_MD_VERSION_ATTRIBUTE] mapping");
                }

                try
                {
                    var prepareAttStatement = new StringBuilder();
                    var attCounter = 1;

                    // Dummy row - insert 'Not Applicable' attribute to satisfy RI
                    using (var connection = new SqlConnection(metaDataConnection))
                    {
                        var insertAttDummyStatement = new StringBuilder();

                        insertAttDummyStatement.AppendLine("INSERT INTO [MD_ATTRIBUTE]");
                        insertAttDummyStatement.AppendLine("([ATTRIBUTE_ID], [ATTRIBUTE_NAME])");
                        insertAttDummyStatement.AppendLine("VALUES ("+attCounter+",'Not applicable')");

                        var command = new SqlCommand(insertAttDummyStatement.ToString(), connection);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            attCounter++;
                        }
                        catch (Exception ex)
                        {
                            errorCounter++;
                            _alert.SetTextLogging("An issue has occured during preparation of the attribute metadata. Please check the Error Log for more details.\r\n");
                            errorLog.AppendLine("\r\nAn issue has occured during preparation of attribute metadata: \r\n\r\n" + ex);
                        }
                    }

                    /* Regular processing
                       RV: there is an issue below where not all SQL version (i.e. SQL Server) are supporting cross database SQL.
                       i.e. Azure. long term fix is to create individual queries to database without cross-db sql and add to single data table in the application
                    */
                    if (checkBoxIgnoreVersion.Checked) // Read from live database
                    {
                        _alert.SetTextLogging("Commencing preparing the attributes directly from the database.\r\n");
                    }
                    else // Virtual processing
                    {
                        _alert.SetTextLogging("Commencing preparing the attributes from the metadata.\r\n");
                    }

                    prepareAttStatement.AppendLine("SELECT DISTINCT(COLUMN_NAME) AS COLUMN_NAME FROM (");
                    prepareAttStatement.Append(allDatabaseAttributes); // The master list of all database columns as defined earlier.
                    prepareAttStatement.AppendLine(") sub");
                    prepareAttStatement.AppendLine("  WHERE sub.COLUMN_NAME NOT IN");
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
                    var listAtt = GetDataTable(ref connOmd, prepareAttStatement.ToString());

                    // Check if there are any attributes found, otherwise insert into the repository
                    if (listAtt.Rows.Count == 0)
                    {
                        _alert.SetTextLogging("-->  No attributes were found in the metadata, did you reverse-engineer the model?\r\n");
                    }
                    else
                    {
                        foreach (DataRow tableName in listAtt.Rows)
                        {
                            using (var connection = new SqlConnection(metaDataConnection))
                            {
                                //_alert.SetTextLogging("-->  Processing " + tableName["COLUMN_NAME"] + ".\r\n");

                                var insertAttStatement = new StringBuilder();

                                insertAttStatement.AppendLine("INSERT INTO [MD_ATTRIBUTE]");
                                insertAttStatement.AppendLine("([ATTRIBUTE_ID], [ATTRIBUTE_NAME])");
                                insertAttStatement.AppendLine("VALUES (" + attCounter + ",'" + tableName["COLUMN_NAME"].ToString().Trim() + "')");

                                var command = new SqlCommand(insertAttStatement.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                    attCounter++;
                                }
                                catch (Exception ex)
                                {
                                    errorCounter++;
                                    _alert.SetTextLogging("An issue has occured during preparation of the attribute metadata. Please check the Error Log for more details.\r\n");
                                    errorLog.AppendLine("\r\nAn issue has occured during preparation of attribute metadata: \r\n\r\n" + ex);
                                }
                            }
                        }
                        _alert.SetTextLogging("-->  Processing " + attCounter + " attributes.\r\n");
                    }
                    worker.ReportProgress(40);
                    _alert.SetTextLogging("Preparation of the attributes completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the attribute metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of attribute metadata: \r\n\r\n" + ex);
                }

                #endregion

                #region Physical Model dump- 50%
                //7b - Creating a point-in-time snapshot of the physical model used for export to the interface schemas

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Creating a snapshot of the physical model.\r\n");

                try
                {
                    var preparePhysicalModelStatement = new StringBuilder();

                    preparePhysicalModelStatement.AppendLine("SELECT ");
                    preparePhysicalModelStatement.AppendLine(" [DATABASE_NAME] ");
                    preparePhysicalModelStatement.AppendLine(",[SCHEMA_NAME]");
                    preparePhysicalModelStatement.AppendLine(",[TABLE_NAME]");
                    preparePhysicalModelStatement.AppendLine(",[COLUMN_NAME]");
                    preparePhysicalModelStatement.AppendLine(",[DATA_TYPE]");
                    preparePhysicalModelStatement.AppendLine(",[CHARACTER_MAXIMUM_LENGTH]");
                    preparePhysicalModelStatement.AppendLine(",[NUMERIC_PRECISION]");
                    preparePhysicalModelStatement.AppendLine(",[ORDINAL_POSITION]");
                    preparePhysicalModelStatement.AppendLine(",[PRIMARY_KEY_INDICATOR]");
                    preparePhysicalModelStatement.AppendLine("FROM");
                    preparePhysicalModelStatement.AppendLine("(");
                    preparePhysicalModelStatement.Append(allDatabaseAttributes); // The master list of all database columns as defined earlier.
                    preparePhysicalModelStatement.AppendLine(") sub");

                    var physicalModelDataTable = GetDataTable(ref connOmd, preparePhysicalModelStatement.ToString());

                    if (physicalModelDataTable.Rows.Count == 0)
                    {
                        _alert.SetTextLogging("-->  No model information was found in the metadata.\r\n");
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
                                insertKeyStatement.AppendLine("VALUES ('" + tableName["DATABASE_NAME"].ToString().Trim() + 
                                                              "','" + tableName["SCHEMA_NAME"].ToString().Trim() + 
                                                              "','" + tableName["TABLE_NAME"].ToString().Trim() + 
                                                              "','" + tableName["COLUMN_NAME"].ToString().Trim() + 
                                                              "','" + tableName["DATA_TYPE"].ToString().Trim() +
                                                              "','" + tableName["CHARACTER_MAXIMUM_LENGTH"].ToString().Trim() +
                                                              "','" + tableName["NUMERIC_PRECISION"].ToString().Trim() +
                                                              "','" + tableName["ORDINAL_POSITION"].ToString().Trim() +
                                                              "','" + tableName["PRIMARY_KEY_INDICATOR"].ToString().Trim() + "')");

                                var command = new SqlCommand(insertKeyStatement.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    errorCounter++;
                                    _alert.SetTextLogging("An issue has occured during preparation of the physical model extract metadata. Please check the Error Log for more details.\r\n");
                                    errorLog.AppendLine("\r\nAn issue has occured during preparation of physical model metadata: \r\n\r\n" + ex);
                                }
                            }
                        }
                    }
                    worker.ReportProgress(50);
                    _alert.SetTextLogging("Preparation of the physical model extract completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the physical model metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of physical model metadata: \r\n\r\n" + ex);
                }

                #endregion

                #region Business Key - 50%
                //8. Understanding the Business Key (MD_BUSINESS_KEY_COMPONENT)

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing the definition of the Business Key.\r\n");

                try
                {
                    var prepareKeyStatement = new StringBuilder();

                    prepareKeyStatement.AppendLine("SELECT ");
                    prepareKeyStatement.AppendLine("  SOURCE_ID,");
                    prepareKeyStatement.AppendLine("  SOURCE_NAME,");
                    prepareKeyStatement.AppendLine("  HUB_ID,");
                    prepareKeyStatement.AppendLine("  HUB_NAME,");
                    prepareKeyStatement.AppendLine("  BUSINESS_KEY_ATTRIBUTE,");
                    prepareKeyStatement.AppendLine("  ROW_NUMBER() OVER(PARTITION BY SOURCE_ID, HUB_ID, BUSINESS_KEY_ATTRIBUTE ORDER BY SOURCE_ID, HUB_ID, COMPONENT_ORDER ASC) AS COMPONENT_ID,");
                    prepareKeyStatement.AppendLine("  COMPONENT_ORDER,");
                    prepareKeyStatement.AppendLine("  REPLACE(COMPONENT_VALUE,'COMPOSITE(', '') AS COMPONENT_VALUE,");
                    prepareKeyStatement.AppendLine("    CASE");
                    prepareKeyStatement.AppendLine("            WHEN SUBSTRING(BUSINESS_KEY_ATTRIBUTE,1, 11)= 'CONCATENATE' THEN 'CONCATENATE()'");
                    prepareKeyStatement.AppendLine("            WHEN SUBSTRING(BUSINESS_KEY_ATTRIBUTE,1, 6)= 'PIVOT' THEN 'PIVOT()'");
                    prepareKeyStatement.AppendLine("            WHEN SUBSTRING(BUSINESS_KEY_ATTRIBUTE,1, 9)= 'COMPOSITE' THEN 'COMPOSITE()'");
                    prepareKeyStatement.AppendLine("            ELSE 'NORMAL'");
                    prepareKeyStatement.AppendLine("    END AS COMPONENT_TYPE");
                    prepareKeyStatement.AppendLine("FROM");
                    prepareKeyStatement.AppendLine("(");
                    prepareKeyStatement.AppendLine("    SELECT DISTINCT");
                    prepareKeyStatement.AppendLine("        A.SOURCE_TABLE,");
                    prepareKeyStatement.AppendLine("        A.BUSINESS_KEY_ATTRIBUTE,");
                    prepareKeyStatement.AppendLine("        A.TARGET_TABLE,");
                    prepareKeyStatement.AppendLine("        CASE");
                    prepareKeyStatement.AppendLine("            WHEN CHARINDEX('(', RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)')))) > 0");
                    prepareKeyStatement.AppendLine("            THEN RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)')))");
                    prepareKeyStatement.AppendLine("            ELSE REPLACE(RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)'))), ')', '')");
                    prepareKeyStatement.AppendLine("        END AS COMPONENT_VALUE,");
                    prepareKeyStatement.AppendLine("        ROW_NUMBER() OVER(PARTITION BY SOURCE_TABLE, TARGET_TABLE, BUSINESS_KEY_ATTRIBUTE ORDER BY SOURCE_TABLE, TARGET_TABLE, BUSINESS_KEY_ATTRIBUTE ASC) AS COMPONENT_ORDER");
                    prepareKeyStatement.AppendLine("    FROM");
                    prepareKeyStatement.AppendLine("    (");

                    // Change to move from comma separate to semicolon separation for composite keys
                    prepareKeyStatement.AppendLine("      SELECT");
                    prepareKeyStatement.AppendLine("          SOURCE_TABLE, ");
                    prepareKeyStatement.AppendLine("          TARGET_TABLE, ");
                    prepareKeyStatement.AppendLine("          BUSINESS_KEY_ATTRIBUTE,");
                    prepareKeyStatement.AppendLine("          CASE SUBSTRING(BUSINESS_KEY_ATTRIBUTE, 0, CHARINDEX('(', BUSINESS_KEY_ATTRIBUTE))");
                    prepareKeyStatement.AppendLine("        	 WHEN 'COMPOSITE' THEN CONVERT(XML, '<M>' + REPLACE(BUSINESS_KEY_ATTRIBUTE, ';', '</M><M>') + '</M>') ");
                    prepareKeyStatement.AppendLine("        	 ELSE CONVERT(XML, '<M>' + REPLACE(BUSINESS_KEY_ATTRIBUTE, ',', '</M><M>') + '</M>') ");
                    prepareKeyStatement.AppendLine("          END AS BUSINESS_KEY_ATTRIBUTE_XML");
                    prepareKeyStatement.AppendLine("        FROM");
                    prepareKeyStatement.AppendLine("        (");
                    prepareKeyStatement.AppendLine("            SELECT DISTINCT SOURCE_TABLE, TARGET_TABLE, LTRIM(RTRIM(BUSINESS_KEY_ATTRIBUTE)) AS BUSINESS_KEY_ATTRIBUTE");
                    prepareKeyStatement.AppendLine("            FROM TMP_MD_TABLE_MAPPING");
                    prepareKeyStatement.AppendLine("            WHERE TARGET_TABLE_TYPE = 'Hub'");
                    prepareKeyStatement.AppendLine("              AND [PROCESS_INDICATOR] = 'Y'");
                    prepareKeyStatement.AppendLine("        ) TableName");
                    prepareKeyStatement.AppendLine("    ) AS A CROSS APPLY BUSINESS_KEY_ATTRIBUTE_XML.nodes('/M') AS Split(a)");
                    prepareKeyStatement.AppendLine("    WHERE BUSINESS_KEY_ATTRIBUTE <> 'N/A' AND A.BUSINESS_KEY_ATTRIBUTE != ''");
                    prepareKeyStatement.AppendLine(") pivotsub");
                    prepareKeyStatement.AppendLine("LEFT OUTER JOIN");
                    prepareKeyStatement.AppendLine("       (");
                    prepareKeyStatement.AppendLine("              SELECT SOURCE_ID, [SCHEMA_NAME]+'.'+SOURCE_NAME AS SOURCE_NAME");
                    prepareKeyStatement.AppendLine("              FROM MD_SOURCE");
                    prepareKeyStatement.AppendLine("       ) stgsub");
                    prepareKeyStatement.AppendLine("ON pivotsub.SOURCE_TABLE = stgsub.SOURCE_NAME");
                    prepareKeyStatement.AppendLine("LEFT OUTER JOIN");
                    prepareKeyStatement.AppendLine("       (");
                    prepareKeyStatement.AppendLine("              SELECT HUB_ID, [SCHEMA_NAME]+'.'+HUB_NAME AS HUB_NAME");
                    prepareKeyStatement.AppendLine("              FROM MD_HUB");
                    prepareKeyStatement.AppendLine("       ) hubsub");
                    prepareKeyStatement.AppendLine("ON pivotsub.TARGET_TABLE = hubsub.HUB_NAME");
                    prepareKeyStatement.AppendLine("ORDER BY stgsub.SOURCE_ID, hubsub.HUB_ID, COMPONENT_ORDER");

                    var listKeys = GetDataTable(ref connOmd, prepareKeyStatement.ToString());

                    if (listKeys.Rows.Count == 0)
                    {
                        _alert.SetTextLogging("-->  No attributes were found in the metadata, did you reverse-engineer the model?\r\n");
                    }
                    else
                    {
                        foreach (DataRow tableName in listKeys.Rows)
                        {
                            using (var connection = new SqlConnection(metaDataConnection))
                            {
                                var insertKeyStatement = new StringBuilder();
                                var keyComponent = tableName["COMPONENT_VALUE"]; //Handle quotes between SQL and C%
                                keyComponent = keyComponent.ToString().Replace("'", "''");

                                _alert.SetTextLogging("-->  Processing the Business Key "+ tableName["BUSINESS_KEY_ATTRIBUTE"] + " (for component "+ keyComponent + ") from " + tableName["SOURCE_NAME"] + " to " + tableName["HUB_NAME"] + "\r\n");



                                var businessKeyDefinition = tableName["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                                businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                                insertKeyStatement.AppendLine("INSERT INTO [MD_BUSINESS_KEY_COMPONENT]");
                                insertKeyStatement.AppendLine("(SOURCE_ID, HUB_ID, BUSINESS_KEY_DEFINITION, COMPONENT_ID, COMPONENT_ORDER, COMPONENT_VALUE, COMPONENT_TYPE)");
                                insertKeyStatement.AppendLine("VALUES ('" + tableName["SOURCE_ID"] + "','" + tableName["HUB_ID"] + "','" + businessKeyDefinition + "','" + tableName["COMPONENT_ID"] + "','" + tableName["COMPONENT_ORDER"] + "','" + keyComponent + "','" + tableName["COMPONENT_TYPE"] + "')");

                                var command = new SqlCommand(insertKeyStatement.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    errorCounter++;
                                    _alert.SetTextLogging("An issue has occured during preparation of the Business Key metadata. Please check the Error Log for more details.\r\n");
                                    errorLog.AppendLine("\r\nAn issue has occured during preparation of Business Key metadata: \r\n\r\n" + ex);
                                }
                            }
                        }
                    }
                    worker.ReportProgress(50);
                    // _alert.SetTextLogging("-->  Processing " + keyCounter + " attributes.\r\n");
                    _alert.SetTextLogging("Preparation of the Business Key definition completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the Business Key metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of Business Key metadata: \r\n\r\n" + ex);
                }

                #endregion

                #region Business Key components - 60%
                //9. Understanding the Business Key component parts

                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing the Business Key component analysis.\r\n");

                try
                {
                    var prepareKeyComponentStatement = new StringBuilder();
                    var keyPartCounter = 1;
                    /*LBM 2019/01/10: Changing to use @ String*/
                    prepareKeyComponentStatement.AppendLine(@"
                                                                SELECT DISTINCT
                                                                  SOURCE_ID,
                                                                  HUB_ID,
                                                                  BUSINESS_KEY_DEFINITION,
                                                                  COMPONENT_ID,
                                                                  ROW_NUMBER() over(partition by SOURCE_ID, HUB_ID, BUSINESS_KEY_DEFINITION, COMPONENT_ID order by nullif(0 * Split.a.value('count(.)', 'int'), 0)) AS COMPONENT_ELEMENT_ID,
                                                                  ROW_NUMBER() over(partition by SOURCE_ID, HUB_ID, BUSINESS_KEY_DEFINITION, COMPONENT_ID order by nullif(0 * Split.a.value('count(.)', 'int'), 0)) AS COMPONENT_ELEMENT_ORDER,
                                                                  REPLACE(REPLACE(REPLACE(RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)'))), 'CONCATENATE(', ''), ')', ''), 'COMPOSITE(', '') AS COMPONENT_ELEMENT_VALUE,
                                                                  CASE
                                                                     WHEN charindex(CHAR(39), REPLACE(REPLACE(RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)'))), 'CONCATENATE(', ''), ')', '')) = 1 THEN 'User Defined Value'
                                                                    ELSE 'Attribute'
                                                                  END AS COMPONENT_ELEMENT_TYPE,
                                                                  COALESCE(att.ATTRIBUTE_ID, 1) AS ATTRIBUTE_ID
                                                                FROM
                                                                (
                                                                    SELECT
                                                                        SOURCE_ID,
                                                                        HUB_ID,
                                                                        BUSINESS_KEY_DEFINITION,
                                                                        COMPONENT_ID,
                                                                        COMPONENT_VALUE,
                                                                        CONVERT(XML, '<M>' + REPLACE(COMPONENT_VALUE, ';', '</M><M>') + '</M>') AS COMPONENT_VALUE_XML
                                                                    FROM MD_BUSINESS_KEY_COMPONENT
                                                                ) AS A CROSS APPLY COMPONENT_VALUE_XML.nodes('/M') AS Split(a)
                                                                LEFT OUTER JOIN MD_ATTRIBUTE att ON
                                                                    REPLACE(REPLACE(RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)'))), 'CONCATENATE(', ''), ')', '') = att.ATTRIBUTE_NAME
                                                                WHERE COMPONENT_VALUE <> 'N/A' AND A.COMPONENT_VALUE != ''
                                                                ORDER BY A.SOURCE_ID, A.HUB_ID, BUSINESS_KEY_DEFINITION, A.COMPONENT_ID, COMPONENT_ELEMENT_ORDER
                                                                ");
                    var listKeyParts = GetDataTable(ref connOmd, prepareKeyComponentStatement.ToString());

                    if (listKeyParts.Rows.Count == 0)
                    {
                        _alert.SetTextLogging("-->  No attributes were found in the metadata, did you reverse-engineer the model?\r\n");
                    }
                    else
                    {
                        foreach (DataRow tableName in listKeyParts.Rows)
                        {
                            using (var connection = new SqlConnection(metaDataConnection))
                            {
                                var insertKeyPartStatement = new StringBuilder();

                                var keyComponent = tableName["COMPONENT_ELEMENT_VALUE"]; //Handle quotes between SQL and C#
                                keyComponent = keyComponent.ToString().Trim().Replace("'", "''");

                                var businessKeyDefinition = tableName["BUSINESS_KEY_DEFINITION"];
                                businessKeyDefinition = businessKeyDefinition.ToString().Trim().Replace("'", "''");

                                insertKeyPartStatement.AppendLine("INSERT INTO [MD_BUSINESS_KEY_COMPONENT_PART]");
                                insertKeyPartStatement.AppendLine("(SOURCE_ID, HUB_ID, BUSINESS_KEY_DEFINITION, COMPONENT_ID,COMPONENT_ELEMENT_ID,COMPONENT_ELEMENT_ORDER,COMPONENT_ELEMENT_VALUE,COMPONENT_ELEMENT_TYPE,ATTRIBUTE_ID)");
                                insertKeyPartStatement.AppendLine("VALUES ('" + tableName["SOURCE_ID"] + "','" + tableName["HUB_ID"] + "','" + businessKeyDefinition + "','" + tableName["COMPONENT_ID"] + "','" + tableName["COMPONENT_ELEMENT_ID"] + "','" + tableName["COMPONENT_ELEMENT_ORDER"] + "','" + keyComponent + "','" + tableName["COMPONENT_ELEMENT_TYPE"] + "','" + tableName["ATTRIBUTE_ID"] + "')");

                                var command = new SqlCommand(insertKeyPartStatement.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                    keyPartCounter++;
                                }
                                catch (Exception ex)
                                {
                                    errorCounter++;
                                    _alert.SetTextLogging("An issue has occured during preparation of the Business Key component metadata. Please check the Error Log for more details.\r\n");
                                    errorLog.AppendLine("\r\nAn issue has occured during preparation of Business Key component metadata: \r\n\r\n" + ex);
                                    errorLog.AppendLine("The query that caused a problem was:\r\n");
                                    errorLog.AppendLine(insertKeyPartStatement.ToString());
                                }
                            }
                        }
                    }
                    worker.ReportProgress(60);
                    _alert.SetTextLogging("-->  Processing " + keyPartCounter + " Business Key component attributes\r\n");
                    _alert.SetTextLogging("Preparation of the Business Key components completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the Business Key component metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of Business Key component metadata: \r\n\r\n" + ex);
                }

                #endregion

                #region Hub / Link relationship - 75%

                //10. Prepare HUB / LNK xref
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the relationship between Hubs and Links.\r\n");

                try
                {
                    var virtualisationSnippet = new StringBuilder();
                    if (checkBoxIgnoreVersion.Checked)
                    {
                        virtualisationSnippet.AppendLine(" SELECT ");
                        virtualisationSnippet.AppendLine("     OBJECT_SCHEMA_NAME(OBJECT_ID, DB_ID('" + ConfigurationSettings.IntegrationDatabaseName + "')) AS LINK_SCHEMA,");
                        virtualisationSnippet.AppendLine("     OBJECT_NAME(OBJECT_ID,DB_ID('" + ConfigurationSettings.IntegrationDatabaseName + "'))  AS LINK_NAME,");
                        virtualisationSnippet.AppendLine("     [name] AS HUB_TARGET_KEY_NAME_IN_LINK,");
                        virtualisationSnippet.AppendLine("     ROW_NUMBER() OVER(PARTITION BY OBJECT_NAME(OBJECT_ID,DB_ID('" + ConfigurationSettings.IntegrationDatabaseName + "')) ORDER BY column_id) AS LINK_ORDER");
                        virtualisationSnippet.AppendLine(" FROM " + linkedServer + integrationDatabase + @".sys.columns");
                        virtualisationSnippet.AppendLine(" WHERE [column_id]>4");
                        virtualisationSnippet.AppendLine(" AND OBJECT_NAME(OBJECT_ID,DB_ID('" + ConfigurationSettings.IntegrationDatabaseName + "')) LIKE '" + lnkTablePrefix + @"'");
                    }
                    else
                    {
                        virtualisationSnippet.AppendLine("SELECT");
                        virtualisationSnippet.AppendLine("  [SCHEMA_NAME] AS LINK_SCHEMA,");
                        virtualisationSnippet.AppendLine("  [TABLE_NAME]  AS LINK_NAME,");
                        virtualisationSnippet.AppendLine("  [COLUMN_NAME] AS HUB_TARGET_KEY_NAME_IN_LINK,");
                        virtualisationSnippet.AppendLine("  ROW_NUMBER() OVER(PARTITION BY[TABLE_NAME] ORDER BY ORDINAL_POSITION) AS LINK_ORDER");
                        virtualisationSnippet.AppendLine("FROM TMP_MD_VERSION_ATTRIBUTE");
                        virtualisationSnippet.AppendLine("WHERE[ORDINAL_POSITION] > 4");
                        virtualisationSnippet.AppendLine("AND TABLE_NAME LIKE '" + lnkTablePrefix + @"'");
                    }

                    var prepareHubLnkXrefStatement = new StringBuilder();

                    prepareHubLnkXrefStatement.AppendLine("SELECT");
                    prepareHubLnkXrefStatement.AppendLine("       hub_tbl.HUB_ID,");
                    prepareHubLnkXrefStatement.AppendLine("       hub_tbl.HUB_NAME,");
                    prepareHubLnkXrefStatement.AppendLine("       lnk_tbl.LINK_ID,");
                    prepareHubLnkXrefStatement.AppendLine("       lnk_tbl.LINK_NAME,");
                    prepareHubLnkXrefStatement.AppendLine("       lnk_hubkey_order.HUB_KEY_ORDER AS HUB_ORDER,");
                    prepareHubLnkXrefStatement.AppendLine("       lnk_target_model.HUB_TARGET_KEY_NAME_IN_LINK");
                    prepareHubLnkXrefStatement.AppendLine("   FROM");
                    prepareHubLnkXrefStatement.AppendLine("   -- This base query adds the Link and its Hubs and their order by pivoting on the full business key");
                    prepareHubLnkXrefStatement.AppendLine("   (");
                    prepareHubLnkXrefStatement.AppendLine("       SELECT");
                    prepareHubLnkXrefStatement.AppendLine("       TARGET_TABLE,");
                    prepareHubLnkXrefStatement.AppendLine("       SOURCE_TABLE,");
                    prepareHubLnkXrefStatement.AppendLine("       BUSINESS_KEY_ATTRIBUTE,");
                    prepareHubLnkXrefStatement.AppendLine("       LTRIM(Split.a.value('.', 'VARCHAR(4000)')) AS BUSINESS_KEY_PART,");
                    prepareHubLnkXrefStatement.AppendLine("       ROW_NUMBER() OVER(PARTITION BY TARGET_TABLE ORDER BY TARGET_TABLE) AS HUB_KEY_ORDER");
                    prepareHubLnkXrefStatement.AppendLine("       FROM");
                    prepareHubLnkXrefStatement.AppendLine("       (");
                    prepareHubLnkXrefStatement.AppendLine("       SELECT");
                    prepareHubLnkXrefStatement.AppendLine("           TARGET_TABLE,");
                    prepareHubLnkXrefStatement.AppendLine("           SOURCE_TABLE,");
                    prepareHubLnkXrefStatement.AppendLine("           ROW_NUMBER() OVER(PARTITION BY TARGET_TABLE ORDER BY TARGET_TABLE) AS LINK_ORDER,");
                    prepareHubLnkXrefStatement.AppendLine("           BUSINESS_KEY_ATTRIBUTE, CAST('<M>' + REPLACE(BUSINESS_KEY_ATTRIBUTE, ',', '</M><M>') + '</M>' AS XML) AS BUSINESS_KEY_SOURCE_XML");
                    prepareHubLnkXrefStatement.AppendLine("       FROM  TMP_MD_TABLE_MAPPING");
                    prepareHubLnkXrefStatement.AppendLine("       WHERE [TARGET_TABLE_TYPE] = 'Link'");
                    prepareHubLnkXrefStatement.AppendLine("           AND [PROCESS_INDICATOR] = 'Y'");
                    prepareHubLnkXrefStatement.AppendLine("     ) AS A CROSS APPLY BUSINESS_KEY_SOURCE_XML.nodes('/M') AS Split(a)");
                    prepareHubLnkXrefStatement.AppendLine("     WHERE LINK_ORDER=1 --Any link will do, the order of the Hub keys in the Link will always be the same");
                    prepareHubLnkXrefStatement.AppendLine(" ) lnk_hubkey_order");
                    prepareHubLnkXrefStatement.AppendLine(" -- Adding the information required for the target model in the query");
                    prepareHubLnkXrefStatement.AppendLine(" JOIN ");
                    prepareHubLnkXrefStatement.AppendLine(" (");
                    prepareHubLnkXrefStatement.AppendLine(virtualisationSnippet.ToString());
                    prepareHubLnkXrefStatement.AppendLine(" ) lnk_target_model");
                    prepareHubLnkXrefStatement.AppendLine(" ON lnk_hubkey_order.TARGET_TABLE = lnk_target_model.LINK_SCHEMA+'.'+lnk_target_model.LINK_NAME COLLATE DATABASE_DEFAULT");
                    prepareHubLnkXrefStatement.AppendLine(" AND lnk_hubkey_order.HUB_KEY_ORDER = lnk_target_model.LINK_ORDER");
                    prepareHubLnkXrefStatement.AppendLine(" --Adding the Hub mapping data to get the business keys");
                    prepareHubLnkXrefStatement.AppendLine(" JOIN TMP_MD_TABLE_MAPPING hub");
                    prepareHubLnkXrefStatement.AppendLine("     ON lnk_hubkey_order.[SOURCE_TABLE] = hub.SOURCE_TABLE");
                    prepareHubLnkXrefStatement.AppendLine("     AND lnk_hubkey_order.[BUSINESS_KEY_PART] = hub.BUSINESS_KEY_ATTRIBUTE-- This condition is required to remove the redundant rows caused by the Link key pivoting");
                    prepareHubLnkXrefStatement.AppendLine("     AND hub.[TARGET_TABLE_TYPE] = 'Hub'");
                    prepareHubLnkXrefStatement.AppendLine("     AND hub.[PROCESS_INDICATOR] = 'Y'");
                    prepareHubLnkXrefStatement.AppendLine(" --Lastly adding the IDs for the Hubs and Links");
                    prepareHubLnkXrefStatement.AppendLine(" JOIN dbo.MD_HUB hub_tbl");
                    prepareHubLnkXrefStatement.AppendLine("     ON hub.TARGET_TABLE = hub_tbl.[SCHEMA_NAME]+'.'+hub_tbl.HUB_NAME");
                    prepareHubLnkXrefStatement.AppendLine(" JOIN dbo.MD_LINK lnk_tbl");
                    prepareHubLnkXrefStatement.AppendLine("     ON lnk_hubkey_order.TARGET_TABLE = lnk_tbl.[SCHEMA_NAME]+'.'+lnk_tbl.LINK_NAME");

                   var listHlXref = GetDataTable(ref connOmd, prepareHubLnkXrefStatement.ToString());

                    foreach (DataRow tableName in listHlXref.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            _alert.SetTextLogging("-->  Processing the " + tableName["HUB_NAME"] + " / " + tableName["LINK_NAME"] + " relationship\r\n");

                            var insertHlXrefStatement = new StringBuilder();

                            insertHlXrefStatement.AppendLine("INSERT INTO [MD_HUB_LINK_XREF]");
                            insertHlXrefStatement.AppendLine("([HUB_ID], [LINK_ID], [HUB_ORDER], [HUB_TARGET_KEY_NAME_IN_LINK])");
                            insertHlXrefStatement.AppendLine("VALUES ('" + tableName["HUB_ID"] + "','" + tableName["LINK_ID"] + "','" + tableName["HUB_ORDER"] + "','" + tableName["HUB_TARGET_KEY_NAME_IN_LINK"] + "')");

                            var command = new SqlCommand(insertHlXrefStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                errorCounter++;
                                _alert.SetTextLogging("An issue has occured during preparation of the Hub / Link XREF metadata. Please check the Error Log for more details.\r\n");
                                errorLog.AppendLine("\r\nAn issue has occured during preparation of the Hub / Link XREF metadata: \r\n\r\n" +ex);
                            }
                        }
                    }

                    worker.ReportProgress(75);
                    _alert.SetTextLogging("Preparation of the relationship between Hubs and Links completed.\r\n");
                }
                catch (Exception ex)
                {
                    {
                        errorCounter++;
                        _alert.SetTextLogging("An issue has occured during preparation of the Hub / Link XREF metadata. Please check the Error Log for more details.\r\n");
                        errorLog.AppendLine("\r\nAn issue has occured during preparation of the Hub / Link XREF metadata: \r\n\r\n" + ex);
                    }
                }

                #endregion

                #region Stg / Link relationship - 80%

                //10. Prepare STG / LNK xref
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the relationship between Source and Link tables.\r\n");

                try
                {
                    var preparestgLnkXrefStatement = new StringBuilder();
                    /*LBM 2019/01/10: Changing to use @ String*/
                    preparestgLnkXrefStatement.AppendLine(@"
                                                            SELECT
                                                              lnk_tbl.LINK_ID,
                                                              lnk_tbl.LINK_NAME,
                                                              stg_tbl.SOURCE_ID,
                                                              stg_tbl.SOURCE_NAME,
                                                              lnk.FILTER_CRITERIA,
                                                              lnk.BUSINESS_KEY_ATTRIBUTE
                                                            FROM [dbo].[TMP_MD_TABLE_MAPPING] lnk
                                                            JOIN [dbo].[MD_LINK] lnk_tbl ON lnk.TARGET_TABLE = lnk_tbl.[SCHEMA_NAME]+'.'+lnk_tbl.LINK_NAME
                                                            JOIN [dbo].[MD_SOURCE] stg_tbl ON lnk.SOURCE_TABLE = stg_tbl.[SCHEMA_NAME]+'.'+stg_tbl.SOURCE_NAME
                                                            WHERE lnk.TARGET_TABLE_TYPE = 'Link'
                                                            AND[PROCESS_INDICATOR] = 'Y'");

                    var listHlXref = GetDataTable(ref connOmd, preparestgLnkXrefStatement.ToString());

                    foreach (DataRow tableName in listHlXref.Rows)
                    {
                        using (var connection = new SqlConnection(metaDataConnection))
                        {
                            _alert.SetTextLogging("-->  Processing the " + tableName["SOURCE_NAME"] + " / " + tableName["LINK_NAME"] + " relationship\r\n");

                            var insertStgLinkStatement = new StringBuilder();

                            var filterCriterion = tableName["FILTER_CRITERIA"].ToString().Trim();
                            filterCriterion = filterCriterion.Replace("'", "''");

                            var businessKeyDefinition = tableName["BUSINESS_KEY_ATTRIBUTE"].ToString().Trim();
                            businessKeyDefinition = businessKeyDefinition.Replace("'", "''");

                            var loadVector = ClassMetadataHandling.GetLoadVector(tableName["SOURCE_NAME"].ToString(), tableName["LINK_NAME"].ToString());

                            insertStgLinkStatement.AppendLine("INSERT INTO [MD_SOURCE_LINK_XREF]");
                            insertStgLinkStatement.AppendLine("([SOURCE_ID], [LINK_ID], [FILTER_CRITERIA], [BUSINESS_KEY_DEFINITION], [LOAD_VECTOR])");
                            insertStgLinkStatement.AppendLine("VALUES ('" + tableName["SOURCE_ID"] +
                                                              "','" + tableName["LINK_ID"] + 
                                                              "','" + filterCriterion + 
                                                              "','" + businessKeyDefinition +
                                                              "','" + loadVector +
                                                              "')");

                            var command = new SqlCommand(insertStgLinkStatement.ToString(), connection);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                errorCounter++;
                                _alert.SetTextLogging("An issue has occured during preparation of the Hub / Link XREF metadata. Please check the Error Log for more details.\r\n");
                                errorLog.AppendLine("\r\nAn issue has occured during preparation of the Hub / Link XREF metadata: \r\n\r\n" +ex);
                            }
                        }
                    }

                    worker.ReportProgress(80);
                    _alert.SetTextLogging("Preparation of the relationship between Source and the Links completed.\r\n");
                }
                catch (Exception ex)
                {
                    {
                        errorCounter++;
                        _alert.SetTextLogging("An issue has occured during preparation of the Staging / Link XREF metadata. Please check the Error Log for more details.\r\n");
                        errorLog.AppendLine("\r\nAn issue has occured during preparation of the Staging / Link XREF metadata: \r\n\r\n" + ex);
                    }
                }
                #endregion

                #region Manually mapped attributes for SAT and LSAT 90%
                //12. Prepare Manual Attribute mapping for Satellites and Link Satellites
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Satellite and Link-Satellite column-to-column mapping metadata based on the manual mappings.\r\n");

                var attributeMappings = new DataTable(); // Defined here to enable population from multiple steps, and inserted in one go.

                try
                {
                    var prepareMappingStatement = new StringBuilder();
   
                    prepareMappingStatement.AppendLine("SELECT  stg.SOURCE_ID");
                    prepareMappingStatement.AppendLine("	   ,stg.SOURCE_NAME");
                    prepareMappingStatement.AppendLine("       ,sat.SATELLITE_ID");
                    prepareMappingStatement.AppendLine("	   ,sat.SATELLITE_NAME");
                    prepareMappingStatement.AppendLine("	   ,stg_attr.ATTRIBUTE_ID AS ATTRIBUTE_FROM_ID");
                    prepareMappingStatement.AppendLine("	   ,stg_attr.ATTRIBUTE_NAME AS ATTRIBUTE_FROM_NAME");
                    prepareMappingStatement.AppendLine("       ,target_attr.ATTRIBUTE_ID AS ATTRIBUTE_TO_ID   ");
                    prepareMappingStatement.AppendLine("	   ,target_attr.ATTRIBUTE_NAME AS ATTRIBUTE_TO_NAME");
                    prepareMappingStatement.AppendLine("	   ,'N' as MULTI_ACTIVE_KEY_INDICATOR");
                    prepareMappingStatement.AppendLine("	   ,'manually_mapped' as VERIFICATION");
                    prepareMappingStatement.AppendLine("FROM dbo.TMP_MD_ATTRIBUTE_MAPPING mapping");
                    prepareMappingStatement.AppendLine("       LEFT OUTER JOIN dbo.MD_SATELLITE sat on sat.[SCHEMA_NAME]+'.'+sat.SATELLITE_NAME=mapping.TARGET_TABLE");
                    prepareMappingStatement.AppendLine("	   LEFT OUTER JOIN dbo.MD_ATTRIBUTE target_attr on UPPER(mapping.TARGET_COLUMN) = UPPER(target_attr.ATTRIBUTE_NAME)");
                    prepareMappingStatement.AppendLine("	   LEFT OUTER JOIN dbo.MD_SOURCE stg on stg.[SCHEMA_NAME]+'.'+stg.SOURCE_NAME = mapping.SOURCE_TABLE");
                    prepareMappingStatement.AppendLine("	   LEFT OUTER JOIN dbo.MD_ATTRIBUTE stg_attr on UPPER(mapping.SOURCE_COLUMN) = UPPER(stg_attr.ATTRIBUTE_NAME)");
                    prepareMappingStatement.AppendLine("	   LEFT OUTER JOIN dbo.TMP_MD_TABLE_MAPPING table_mapping");
                    prepareMappingStatement.AppendLine("	     on mapping.TARGET_TABLE = table_mapping.TARGET_TABLE");
                    prepareMappingStatement.AppendLine("	    and mapping.SOURCE_TABLE = table_mapping.SOURCE_TABLE");
                    prepareMappingStatement.AppendLine("WHERE mapping.TARGET_TABLE_TYPE IN ('Satellite', 'Link-Satellite')");
                    prepareMappingStatement.AppendLine("      AND table_mapping.PROCESS_INDICATOR = 'Y'");

                    attributeMappings = GetDataTable(ref connOmd, prepareMappingStatement.ToString());

                    if (attributeMappings.Rows.Count == 0)
                    {
                        _alert.SetTextLogging("-->  No manual column-to-column mappings were detected.\r\n");
                    }

                    worker.ReportProgress(90);
                    _alert.SetTextLogging("-->  Processing " + attributeMappings.Rows.Count + " manual attribute mappings\r\n");
                    _alert.SetTextLogging("Preparation of the manual column-to-column mapping for Satellites and Link-Satellites completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the manual Satellite attribute mapping metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the manual Satellite attribute mapping metadata: \r\n\r\n" + ex);
                }
                #endregion

                #region Automatically mapped attributes for SAT and LSAT 93%
                //12. Prepare automatic attribute mapping
                _alert.SetTextLogging("\r\n");

                try
                {
                    int automaticMappingCounter = 0;
                    var prepareMappingStatement = new StringBuilder();

                    if (checkBoxIgnoreVersion.Checked)
                    {
                        _alert.SetTextLogging("Commencing preparing the (automatic) column-to-column mapping metadata for Satellites and Link-Satellites, based on what's available in the database.\r\n");
                    }
                    else
                    {
                        _alert.SetTextLogging("Commencing preparing the (automatic) column-to-column mapping metadata for Satellites and Link-Satellites, based on what's available in the physical model metadata.\r\n");
                    }

                    // Run the statement, the virtual vs. physical lookups are embedded in allDatabaseAttributes
                    prepareMappingStatement.AppendLine("WITH ALL_DATABASE_COLUMNS AS");
                    prepareMappingStatement.AppendLine("(");
                    prepareMappingStatement.Append(allDatabaseAttributes); // The master list of all columns as defined earlier
                    prepareMappingStatement.AppendLine("),");
                    prepareMappingStatement.AppendLine("XREF AS");
                    prepareMappingStatement.AppendLine("(");
                    prepareMappingStatement.AppendLine("  SELECT");
                    prepareMappingStatement.AppendLine("    xref.*,");
                    prepareMappingStatement.AppendLine("    src.[SCHEMA_NAME] AS SOURCE_SCHEMA_NAME,");
                    prepareMappingStatement.AppendLine("    src.SOURCE_NAME AS SOURCE_NAME,");
                    prepareMappingStatement.AppendLine("    sat.[SCHEMA_NAME] AS TARGET_SCHEMA_NAME,");
                    prepareMappingStatement.AppendLine("    sat.SATELLITE_NAME AS TARGET_NAME");
                    prepareMappingStatement.AppendLine("  FROM MD_SOURCE_SATELLITE_XREF xref");
                    prepareMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_SOURCE src ON xref.SOURCE_ID = src.SOURCE_ID");
                    prepareMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_SATELLITE sat ON xref.SATELLITE_ID = sat.SATELLITE_ID");
                    prepareMappingStatement.AppendLine(") ");
                    prepareMappingStatement.AppendLine("SELECT");
                    prepareMappingStatement.AppendLine("  XREF.SOURCE_ID,");
                    prepareMappingStatement.AppendLine("  XREF.SOURCE_NAME, ");
                    prepareMappingStatement.AppendLine("  XREF.SATELLITE_ID,");
                    prepareMappingStatement.AppendLine("  XREF.TARGET_NAME AS SATELLITE_NAME,");
                    prepareMappingStatement.AppendLine("  stg_attr.ATTRIBUTE_ID AS ATTRIBUTE_FROM_ID,");
                    prepareMappingStatement.AppendLine("  ADC_SOURCE.COLUMN_NAME AS ATTRIBUTE_FROM_NAME,");
                    prepareMappingStatement.AppendLine("  tgt_attr.ATTRIBUTE_ID AS ATTRIBUTE_TO_ID,");
                    prepareMappingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_TO_NAME,");
                    prepareMappingStatement.AppendLine("  'N' AS MULTI_ACTIVE_KEY_INDICATOR,");
                    prepareMappingStatement.AppendLine("  'automatically mapped' as VERIFICATION");
                    prepareMappingStatement.AppendLine("FROM XREF");
                    prepareMappingStatement.AppendLine("JOIN ALL_DATABASE_COLUMNS ADC_SOURCE ON XREF.SOURCE_SCHEMA_NAME = ADC_SOURCE.[SCHEMA_NAME] AND XREF.SOURCE_NAME = ADC_SOURCE.TABLE_NAME");
                    prepareMappingStatement.AppendLine("JOIN ALL_DATABASE_COLUMNS ADC_TARGET ON XREF.TARGET_SCHEMA_NAME = ADC_TARGET.[SCHEMA_NAME] AND XREF.TARGET_NAME = ADC_TARGET.TABLE_NAME");
                    prepareMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_ATTRIBUTE stg_attr ON UPPER(ADC_SOURCE.COLUMN_NAME) = UPPER(stg_attr.ATTRIBUTE_NAME) COLLATE DATABASE_DEFAULT");
                    prepareMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_ATTRIBUTE tgt_attr ON UPPER(ADC_TARGET.COLUMN_NAME) = UPPER(tgt_attr.ATTRIBUTE_NAME) COLLATE DATABASE_DEFAULT");
                    prepareMappingStatement.AppendLine("WHERE stg_attr.ATTRIBUTE_ID = tgt_attr.ATTRIBUTE_ID");                    
                    
                    var automaticAttributeMappings = GetDataTable(ref connOmd, prepareMappingStatement.ToString());

                    if (automaticAttributeMappings.Rows.Count == 0)
                    {
                        _alert.SetTextLogging("-->  No automatic column-to-column mappings were detected.\r\n");
                    }
                    else
                    {
                        // Prevent duplicates to be inserted into the data table, by only inserting new ones
                        // Entries found in the automatic check which are not already in the manual datat able will be added
                        foreach (DataRow automaticMapping in automaticAttributeMappings.Rows)
                        {
                            DataRow[] foundRow = attributeMappings.Select("SOURCE_ID = '" + automaticMapping["SOURCE_ID"] + "' AND SATELLITE_ID = '" + automaticMapping["SATELLITE_ID"] + "' AND ATTRIBUTE_FROM_ID = '" + automaticMapping["ATTRIBUTE_FROM_ID"] + "'AND ATTRIBUTE_TO_ID = '" + automaticMapping["ATTRIBUTE_TO_ID"] + "'");
                            if (foundRow.Length == 0)
                            {
                                // If nothing is found, add to the overall data table that is inserted into SOURCE_SATELLITE_ATTRIBUTE_XREF
                                attributeMappings.Rows.Add(
                                    automaticMapping["SOURCE_ID"],
                                    automaticMapping["SOURCE_NAME"],
                                    automaticMapping["SATELLITE_ID"],
                                    automaticMapping["SATELLITE_NAME"],
                                    automaticMapping["ATTRIBUTE_FROM_ID"],
                                    automaticMapping["ATTRIBUTE_FROM_NAME"],
                                    automaticMapping["ATTRIBUTE_TO_ID"],
                                    automaticMapping["ATTRIBUTE_TO_NAME"],
                                    automaticMapping["MULTI_ACTIVE_KEY_INDICATOR"],
                                    automaticMapping["VERIFICATION"]);

                                    automaticMappingCounter++;
                            }
                        }
                    }

                    // Now the full data table can be processed
                    if (attributeMappings.Rows.Count > 0)
                    {
                        foreach (DataRow tableName in attributeMappings.Rows)
                        {
                            using (var connection = new SqlConnection(metaDataConnection))
                            {

                                var insertMappingStatement = new StringBuilder();

                                insertMappingStatement.AppendLine("INSERT INTO [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]");
                                insertMappingStatement.AppendLine("( [SOURCE_ID],[SATELLITE_ID],[ATTRIBUTE_ID_FROM],[ATTRIBUTE_ID_TO],[MULTI_ACTIVE_KEY_INDICATOR])");
                                insertMappingStatement.AppendLine("VALUES ('" +
                                                               tableName["SOURCE_ID"] + "'," +
                                                               tableName["SATELLITE_ID"] + "," +
                                                               tableName["ATTRIBUTE_FROM_ID"] + "," +
                                                               tableName["ATTRIBUTE_TO_ID"] + ",'" +
                                                               tableName["MULTI_ACTIVE_KEY_INDICATOR"] + "')");

                                var command = new SqlCommand(insertMappingStatement.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                    
                                }
                                catch (Exception)
                                {
                                    _alert.SetTextLogging("-----> An issue has occurred mapping columns from table " + tableName["SOURCE_NAME"] + " to " + tableName["SATELLITE_NAME"] + ". \r\n");
                                    errorCounter++;
                                    if (tableName["ATTRIBUTE_FROM_ID"].ToString() == "")
                                    {
                                        _alert.SetTextLogging("Both attributes are NULL.");
                                    }
                                }
                            }
                        }
                    }

                    worker.ReportProgress(90);
                    _alert.SetTextLogging("-->  Processing " + automaticMappingCounter + " automatically added attribute mappings\r\n");
                    _alert.SetTextLogging("Preparation of the automatically mapped column-to-column metadata completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the automatically mapped Satellite Attribute  metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the automatically mapped Satellite Attribute metadata: \r\n\r\n" + ex);
                }

                #endregion

                #region Manually mapped degenerate attributes for Links 95%
                //12. Prepare Manual Attribute mapping for Link degenerate fields
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the column-to-column mapping metadata based on the manual mappings for degenerate attributes.\r\n");

                var degenerateMappings = new DataTable();

                try
                {
                    var prepareMappingStatement = new StringBuilder();

                    prepareMappingStatement.AppendLine("SELECT  stg.SOURCE_ID");
                    prepareMappingStatement.AppendLine("	   ,stg.SOURCE_NAME");
                    prepareMappingStatement.AppendLine("       ,lnk.LINK_ID");
                    prepareMappingStatement.AppendLine("	   ,lnk.LINK_NAME");
                    prepareMappingStatement.AppendLine("	   ,stg_attr.ATTRIBUTE_ID AS ATTRIBUTE_FROM_ID");
                    prepareMappingStatement.AppendLine("	   ,stg_attr.ATTRIBUTE_NAME AS ATTRIBUTE_FROM_NAME");
                    prepareMappingStatement.AppendLine("       ,target_attr.ATTRIBUTE_ID AS ATTRIBUTE_TO_ID   ");
                    prepareMappingStatement.AppendLine("	   ,target_attr.ATTRIBUTE_NAME AS ATTRIBUTE_TO_NAME");
                    prepareMappingStatement.AppendLine("	   ,'manually_mapped' as VERIFICATION");
                    prepareMappingStatement.AppendLine("FROM dbo.TMP_MD_ATTRIBUTE_MAPPING mapping");
                    prepareMappingStatement.AppendLine("       LEFT OUTER JOIN dbo.MD_LINK lnk on lnk.[SCHEMA_NAME]+'.'+lnk.LINK_NAME=mapping.TARGET_TABLE");
                    prepareMappingStatement.AppendLine("	   LEFT OUTER JOIN dbo.MD_ATTRIBUTE target_attr on mapping.TARGET_COLUMN = target_attr.ATTRIBUTE_NAME");
                    prepareMappingStatement.AppendLine("	   LEFT OUTER JOIN dbo.MD_SOURCE stg on stg.[SCHEMA_NAME]+'.'+stg.SOURCE_NAME = mapping.SOURCE_TABLE");
                    prepareMappingStatement.AppendLine("	   LEFT OUTER JOIN dbo.MD_ATTRIBUTE stg_attr on mapping.SOURCE_COLUMN = stg_attr.ATTRIBUTE_NAME");
                    prepareMappingStatement.AppendLine("	   LEFT OUTER JOIN dbo.TMP_MD_TABLE_MAPPING table_mapping");
                    prepareMappingStatement.AppendLine("	     on mapping.TARGET_TABLE = table_mapping.TARGET_TABLE");
                    prepareMappingStatement.AppendLine("	    and mapping.SOURCE_TABLE = table_mapping.SOURCE_TABLE");
                    prepareMappingStatement.AppendLine("WHERE mapping.TARGET_TABLE_TYPE IN ('Link')");
                    prepareMappingStatement.AppendLine("      AND table_mapping.PROCESS_INDICATOR = 'Y'");

                    degenerateMappings = GetDataTable(ref connOmd, prepareMappingStatement.ToString());

                    if (degenerateMappings.Rows.Count == 0)
                    {
                        _alert.SetTextLogging("-->  No manually mapped degenerate columns were detected.\r\n");
                    }

                    worker.ReportProgress(95);

                    _alert.SetTextLogging("-->  Processing " + degenerateMappings.Rows.Count + " manual degenerate attribute mappings\r\n");
                    _alert.SetTextLogging("Preparation of the degenerate column metadata completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the degenerate attribute metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the degenerate attribute metadata: \r\n\r\n" + ex);
                }
                #endregion

                #region Automatically mapped degenerate attributes for Links 95%
                //13. Prepare the automatic degenerate attribute mapping
                _alert.SetTextLogging("\r\n");

                try
                {
                    int automaticDegenerateMappingCounter = 0;
                    var prepareDegenerateMappingStatement = new StringBuilder();

                    prepareDegenerateMappingStatement.AppendLine("WITH ALL_DATABASE_COLUMNS AS");
                    prepareDegenerateMappingStatement.AppendLine("(");
                    prepareDegenerateMappingStatement.Append(allDatabaseAttributes); // The master list of all columns as defined earlier
                    prepareDegenerateMappingStatement.AppendLine("),");
                    prepareDegenerateMappingStatement.AppendLine("XREF AS");
                    prepareDegenerateMappingStatement.AppendLine("(");
                    prepareDegenerateMappingStatement.AppendLine("  SELECT");
                    prepareDegenerateMappingStatement.AppendLine("    xref.*,");
                    prepareDegenerateMappingStatement.AppendLine("    src.[SCHEMA_NAME] AS SOURCE_SCHEMA_NAME,");
                    prepareDegenerateMappingStatement.AppendLine("    src.SOURCE_NAME AS SOURCE_NAME,");
                    prepareDegenerateMappingStatement.AppendLine("    lnk.[SCHEMA_NAME] AS TARGET_SCHEMA_NAME,");
                    prepareDegenerateMappingStatement.AppendLine("    lnk.LINK_NAME AS TARGET_NAME");
                    prepareDegenerateMappingStatement.AppendLine("  FROM MD_SOURCE_LINK_XREF xref");
                    prepareDegenerateMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_SOURCE src ON xref.SOURCE_ID = src.SOURCE_ID");
                    prepareDegenerateMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_LINK lnk ON xref.LINK_ID = lnk.LINK_ID");
                    prepareDegenerateMappingStatement.AppendLine(") ");
                    prepareDegenerateMappingStatement.AppendLine("SELECT");
                    prepareDegenerateMappingStatement.AppendLine("  XREF.SOURCE_ID,");
                    prepareDegenerateMappingStatement.AppendLine("  XREF.SOURCE_NAME, ");
                    prepareDegenerateMappingStatement.AppendLine("  XREF.LINK_ID,");
                    prepareDegenerateMappingStatement.AppendLine("  XREF.TARGET_NAME AS LINK_NAME,");
                    prepareDegenerateMappingStatement.AppendLine("  stg_attr.ATTRIBUTE_ID AS ATTRIBUTE_FROM_ID,");
                    prepareDegenerateMappingStatement.AppendLine("  ADC_SOURCE.COLUMN_NAME AS ATTRIBUTE_FROM_NAME,");
                    prepareDegenerateMappingStatement.AppendLine("  tgt_attr.ATTRIBUTE_ID AS ATTRIBUTE_TO_ID,");
                    prepareDegenerateMappingStatement.AppendLine("  ADC_TARGET.COLUMN_NAME AS ATTRIBUTE_TO_NAME,");
                    prepareDegenerateMappingStatement.AppendLine("  'N' AS MULTI_ACTIVE_INDICATOR,");
                    prepareDegenerateMappingStatement.AppendLine("  'automatically mapped' as VERIFICATION");
                    prepareDegenerateMappingStatement.AppendLine("FROM XREF");
                    prepareDegenerateMappingStatement.AppendLine("JOIN ALL_DATABASE_COLUMNS ADC_SOURCE ON XREF.SOURCE_SCHEMA_NAME = ADC_SOURCE.[SCHEMA_NAME] AND XREF.SOURCE_NAME = ADC_SOURCE.TABLE_NAME");
                    prepareDegenerateMappingStatement.AppendLine("JOIN ALL_DATABASE_COLUMNS ADC_TARGET ON XREF.TARGET_SCHEMA_NAME = ADC_TARGET.[SCHEMA_NAME] AND XREF.TARGET_NAME = ADC_TARGET.TABLE_NAME");
                    prepareDegenerateMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_ATTRIBUTE stg_attr ON UPPER(ADC_SOURCE.COLUMN_NAME) = UPPER(stg_attr.ATTRIBUTE_NAME) COLLATE DATABASE_DEFAULT");
                    prepareDegenerateMappingStatement.AppendLine("LEFT OUTER JOIN dbo.MD_ATTRIBUTE tgt_attr ON UPPER(ADC_TARGET.COLUMN_NAME) = UPPER(tgt_attr.ATTRIBUTE_NAME) COLLATE DATABASE_DEFAULT");
                    prepareDegenerateMappingStatement.AppendLine("WHERE stg_attr.ATTRIBUTE_ID = tgt_attr.ATTRIBUTE_ID");


                    if (checkBoxIgnoreVersion.Checked)
                    {
                        _alert.SetTextLogging("Commencing preparing the (automatic) column-to-column mapping metadata for degenerate attributes, based on what's available in the database.\r\n");
                    }
                    else
                    {
                        _alert.SetTextLogging("Commencing preparing the degenerate column metadata using the physical model metadata.\r\n");
                    }

                    var automaticDegenerateMappings = GetDataTable(ref connOmd, prepareDegenerateMappingStatement.ToString());

                    if (automaticDegenerateMappings.Rows.Count == 0)
                    {
                        _alert.SetTextLogging("-->  No automatic degenerate columns were detected.\r\n");
                    }
                    else
                    {
                        // Prevent duplicates to be inserted into the datatable, by only inserting new ones
                        // Entries found in the automatic check which are not already in the manual datatable will be added
                        foreach (DataRow automaticMapping in automaticDegenerateMappings.Rows)
                        {
                            DataRow[] foundRow = degenerateMappings.Select("SOURCE_ID = '" + automaticMapping["SOURCE_ID"] + "' AND LINK_ID = '" + automaticMapping["LINK_ID"] + "' AND ATTRIBUTE_FROM_ID = '" + automaticMapping["ATTRIBUTE_FROM_ID"] + "'AND ATTRIBUTE_TO_ID = '" + automaticMapping["ATTRIBUTE_TO_ID"] + "'");
                            if (foundRow.Length == 0)
                            {
                                // If nothing is found, add to the overall data table that is inserted into SOURCE_SATELLITE_ATTRIBUTE_XREF
                                degenerateMappings.Rows.Add(
                                    automaticMapping["SOURCE_ID"],
                                    automaticMapping["SOURCE_NAME"],
                                    automaticMapping["LINK_ID"],
                                    automaticMapping["LINK_NAME"],
                                    automaticMapping["ATTRIBUTE_FROM_ID"],
                                    automaticMapping["ATTRIBUTE_FROM_NAME"],
                                    automaticMapping["ATTRIBUTE_TO_ID"],
                                    automaticMapping["ATTRIBUTE_TO_NAME"],
                                    automaticMapping["VERIFICATION"]);

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

                                var insertMappingStatement = new StringBuilder();

                                insertMappingStatement.AppendLine("INSERT INTO [MD_SOURCE_LINK_ATTRIBUTE_XREF]");
                                insertMappingStatement.AppendLine("( [SOURCE_ID],[LINK_ID],[ATTRIBUTE_ID_FROM],[ATTRIBUTE_ID_TO])");
                                insertMappingStatement.AppendLine("VALUES (" +
                                                               tableName["SOURCE_ID"] + "," +
                                                               tableName["LINK_ID"] + "," +
                                                               tableName["ATTRIBUTE_FROM_ID"] + "," +
                                                               tableName["ATTRIBUTE_TO_ID"] + ")");

                                var command = new SqlCommand(insertMappingStatement.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();

                                }
                                catch (Exception)
                                {
                                    _alert.SetTextLogging("-----> An issue has occurred mapping degenerate columns from table " + tableName["SOURCE_NAME"] + " to " + tableName["LINK_NAME"] + ". \r\n");
                                    if (tableName["ATTRIBUTE_FROM_ID"].ToString() == "")
                                    {
                                        _alert.SetTextLogging("Both attributes are NULL.");
                                    }
                                }
                            }
                        }
                    }

                    worker.ReportProgress(95);

                    _alert.SetTextLogging("-->  Processing " + automaticDegenerateMappingCounter + " automatically added degenerate attribute mappings\r\n");
                    _alert.SetTextLogging("Preparation of the degenerate column metadata completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the degenerate attribute metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the degenerate attribute metadata: \r\n\r\n" + ex);
                }

                #endregion

                #region 14. Multi-Active Key - 97%

                //14. Handle the Multi-Active Key
                _alert.SetTextLogging("\r\n");


                try
                {
                    var prepareMultiKeyStatement = new StringBuilder();

                    if (checkBoxIgnoreVersion.Checked)
                    {
                        _alert.SetTextLogging("Commencing Multi-Active Key handling using database.\r\n");

                        prepareMultiKeyStatement.AppendLine("SELECT ");
                        prepareMultiKeyStatement.AppendLine("   u.SOURCE_ID,");
                        prepareMultiKeyStatement.AppendLine("	u.SATELLITE_ID,");
                        prepareMultiKeyStatement.AppendLine("	sat.SATELLITE_NAME,");
                        prepareMultiKeyStatement.AppendLine("	u.ATTRIBUTE_ID_FROM,");
                        prepareMultiKeyStatement.AppendLine("	u.ATTRIBUTE_ID_TO,");
                        prepareMultiKeyStatement.AppendLine("	att.ATTRIBUTE_NAME");
                        prepareMultiKeyStatement.AppendLine("FROM MD_SOURCE_SATELLITE_ATTRIBUTE_XREF u");
                        prepareMultiKeyStatement.AppendLine("INNER JOIN MD_SATELLITE sat ON sat.SATELLITE_ID=u.SATELLITE_ID");
                        prepareMultiKeyStatement.AppendLine("INNER JOIN MD_ATTRIBUTE att ON att.ATTRIBUTE_ID = u.ATTRIBUTE_ID_TO");
                        prepareMultiKeyStatement.AppendLine("INNER JOIN ");
                        prepareMultiKeyStatement.AppendLine("(");
                        prepareMultiKeyStatement.AppendLine("  SELECT ");
                        prepareMultiKeyStatement.AppendLine("  	sc.name AS SATELLITE_NAME,");
                        prepareMultiKeyStatement.AppendLine("  	C.name AS ATTRIBUTE_NAME");
                        prepareMultiKeyStatement.AppendLine("  FROM " + linkedServer + integrationDatabase + ".sys.index_columns A");
                        prepareMultiKeyStatement.AppendLine("  JOIN " + linkedServer + integrationDatabase + ".sys.indexes B");
                        prepareMultiKeyStatement.AppendLine("    ON A.OBJECT_ID=B.OBJECT_ID AND A.index_id=B.index_id");
                        prepareMultiKeyStatement.AppendLine("  JOIN " + linkedServer + integrationDatabase + ".sys.columns C");
                        prepareMultiKeyStatement.AppendLine("    ON A.column_id=C.column_id AND A.OBJECT_ID=C.OBJECT_ID");
                        prepareMultiKeyStatement.AppendLine("  JOIN " + linkedServer + integrationDatabase + ".sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
                        prepareMultiKeyStatement.AppendLine("    WHERE is_primary_key=1");
                        prepareMultiKeyStatement.AppendLine("  AND C.name!='" + effectiveDateTimeAttribute + "' AND C.name!='" + currentRecordAttribute + "' AND C.name!='" + eventDateTimeAtttribute + "'");
                        prepareMultiKeyStatement.AppendLine("  AND C.name NOT LIKE '"+dwhKeyIdentifier+"'");
                        prepareMultiKeyStatement.AppendLine(") ddsub");
                        prepareMultiKeyStatement.AppendLine("ON sat.SATELLITE_NAME=ddsub.SATELLITE_NAME COLLATE DATABASE_DEFAULT");
                        prepareMultiKeyStatement.AppendLine("AND att.ATTRIBUTE_NAME=ddsub.ATTRIBUTE_NAME COLLATE DATABASE_DEFAULT");
                        prepareMultiKeyStatement.AppendLine("  WHERE ddsub.SATELLITE_NAME LIKE '"+satTablePrefix+"' OR ddsub.SATELLITE_NAME LIKE '"+lsatTablePrefix+"'");
                    }
                    else
                    {
                        _alert.SetTextLogging("Commencing Multi-Active Key handling using model metadata.\r\n");

                        prepareMultiKeyStatement.AppendLine("SELECT ");
                        prepareMultiKeyStatement.AppendLine("   u.SOURCE_ID,");
                        prepareMultiKeyStatement.AppendLine("	u.SATELLITE_ID,");
                        prepareMultiKeyStatement.AppendLine("	sat.SATELLITE_NAME,");
                        prepareMultiKeyStatement.AppendLine("	u.ATTRIBUTE_ID_FROM,");
                        prepareMultiKeyStatement.AppendLine("	u.ATTRIBUTE_ID_TO,");
                        prepareMultiKeyStatement.AppendLine("	att.ATTRIBUTE_NAME");
                        prepareMultiKeyStatement.AppendLine("FROM MD_SOURCE_SATELLITE_ATTRIBUTE_XREF u");
                        prepareMultiKeyStatement.AppendLine("INNER JOIN MD_SATELLITE sat ON sat.SATELLITE_ID=u.SATELLITE_ID");
                        prepareMultiKeyStatement.AppendLine("INNER JOIN MD_ATTRIBUTE att ON att.ATTRIBUTE_ID = u.ATTRIBUTE_ID_TO");
                        prepareMultiKeyStatement.AppendLine("INNER JOIN ");
                        prepareMultiKeyStatement.AppendLine("(");
                        prepareMultiKeyStatement.AppendLine("	SELECT");
                        prepareMultiKeyStatement.AppendLine("		TABLE_NAME AS SATELLITE_NAME,");
                        prepareMultiKeyStatement.AppendLine("		COLUMN_NAME AS ATTRIBUTE_NAME");
                        prepareMultiKeyStatement.AppendLine("	FROM TMP_MD_VERSION_ATTRIBUTE");
                        prepareMultiKeyStatement.AppendLine("	WHERE MULTI_ACTIVE_INDICATOR='Y'");
                        prepareMultiKeyStatement.AppendLine(") sub");
                        prepareMultiKeyStatement.AppendLine("ON sat.SATELLITE_NAME=sub.SATELLITE_NAME");
                        prepareMultiKeyStatement.AppendLine("AND att.ATTRIBUTE_NAME=sub.ATTRIBUTE_NAME");
                    }

                    var listMultiKeys = GetDataTable(ref connOmd, prepareMultiKeyStatement.ToString());

                    if (listMultiKeys.Rows.Count == 0)
                    {
                        _alert.SetTextLogging("-->  No Multi-Active Keys were detected.\r\n");
                    }
                    else
                    {
                        foreach (DataRow tableName in listMultiKeys.Rows)
                        {
                            using (var connection = new SqlConnection(metaDataConnection))
                            {
                                _alert.SetTextLogging("-->  Processing the Multi-Active Key attribute " +
                                                      tableName["ATTRIBUTE_NAME"] + " for " +
                                                      tableName["SATELLITE_NAME"] + "\r\n");

                                var updateMultiActiveKeyStatement = new StringBuilder();

                                updateMultiActiveKeyStatement.AppendLine("UPDATE [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]");
                                updateMultiActiveKeyStatement.AppendLine("SET MULTI_ACTIVE_KEY_INDICATOR='Y'");
                                updateMultiActiveKeyStatement.AppendLine("WHERE SOURCE_ID = " + tableName["SOURCE_ID"]);
                                updateMultiActiveKeyStatement.AppendLine("AND SATELLITE_ID = " + tableName["SATELLITE_ID"]);
                                updateMultiActiveKeyStatement.AppendLine("AND ATTRIBUTE_ID_FROM = " + tableName["ATTRIBUTE_ID_FROM"]);
                                updateMultiActiveKeyStatement.AppendLine("AND ATTRIBUTE_ID_TO = " + tableName["ATTRIBUTE_ID_TO"]);


                                var command = new SqlCommand(updateMultiActiveKeyStatement.ToString(), connection);

                                try
                                {
                                    connection.Open();
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    errorCounter++;
                                    _alert.SetTextLogging("An issue has occured during preparation of the Multi-Active key metadata. Please check the Error Log for more details.\r\n");
                                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the Multi-Active key metadata: \r\n\r\n" + ex);
                                }
                            }
                        }
                    }
                    worker.ReportProgress(80);
                    _alert.SetTextLogging("Preparation of the Multi-Active Keys completed.\r\n");
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the Multi-Active key metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the Multi-Active key metadata: \r\n\r\n" + ex);
                }

                #endregion

                #region Driving Key preparation
                //13. Prepare driving keys
                _alert.SetTextLogging("\r\n");
                _alert.SetTextLogging("Commencing preparing the Driving Key metadata.\r\n");

                try
                {
                    var prepareDrivingKeyStatement = new StringBuilder();

                        prepareDrivingKeyStatement.AppendLine("SELECT DISTINCT");
                        prepareDrivingKeyStatement.AppendLine("    -- base.[TABLE_MAPPING_HASH]");
                        prepareDrivingKeyStatement.AppendLine("    --,base.[VERSION_ID]");
                        prepareDrivingKeyStatement.AppendLine("    --,base.[SOURCE_TABLE]");
                        prepareDrivingKeyStatement.AppendLine("    --,base.[BUSINESS_KEY_ATTRIBUTE]");
                        prepareDrivingKeyStatement.AppendLine("       sat.SATELLITE_ID");
                        prepareDrivingKeyStatement.AppendLine("    --,base.[TARGET_TABLE] AS LINK_SATELLITE_NAME");
                        prepareDrivingKeyStatement.AppendLine("    --,base.[FILTER_CRITERIA]");
                        prepareDrivingKeyStatement.AppendLine("    --,base.[DRIVING_KEY_ATTRIBUTE]");
                        prepareDrivingKeyStatement.AppendLine("      ,COALESCE(hubkey.HUB_ID, (SELECT HUB_ID FROM MD_HUB WHERE HUB_NAME = 'Not applicable')) AS HUB_ID");
                        prepareDrivingKeyStatement.AppendLine("    --,hub.[TARGET_TABLE] AS [HUB_TABLE]");
                        prepareDrivingKeyStatement.AppendLine("FROM");
                        prepareDrivingKeyStatement.AppendLine("(");
                        prepareDrivingKeyStatement.AppendLine("       SELECT");
                        prepareDrivingKeyStatement.AppendLine("              SOURCE_TABLE,");
                        prepareDrivingKeyStatement.AppendLine("              TARGET_TABLE,");
                        prepareDrivingKeyStatement.AppendLine("              VERSION_ID,");
                        prepareDrivingKeyStatement.AppendLine("              CASE");
                        prepareDrivingKeyStatement.AppendLine("                     WHEN CHARINDEX('(', RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)')))) > 0");
                        prepareDrivingKeyStatement.AppendLine("                     THEN RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)')))");
                        prepareDrivingKeyStatement.AppendLine("                     ELSE REPLACE(RTRIM(LTRIM(Split.a.value('.', 'VARCHAR(MAX)'))), ')', '')");
                        prepareDrivingKeyStatement.AppendLine("              END AS BUSINESS_KEY_ATTRIBUTE--For Driving Key");
                        prepareDrivingKeyStatement.AppendLine("       FROM");
                        prepareDrivingKeyStatement.AppendLine("       (");
                        prepareDrivingKeyStatement.AppendLine("              SELECT SOURCE_TABLE, TARGET_TABLE, DRIVING_KEY_ATTRIBUTE, VERSION_ID, CONVERT(XML, '<M>' + REPLACE(DRIVING_KEY_ATTRIBUTE, ',', '</M><M>') + '</M>') AS DRIVING_KEY_ATTRIBUTE_XML");
                        prepareDrivingKeyStatement.AppendLine("              FROM");
                        prepareDrivingKeyStatement.AppendLine("              (");
                        prepareDrivingKeyStatement.AppendLine("                     SELECT DISTINCT SOURCE_TABLE, TARGET_TABLE, VERSION_ID, LTRIM(RTRIM(DRIVING_KEY_ATTRIBUTE)) AS DRIVING_KEY_ATTRIBUTE");
                        prepareDrivingKeyStatement.AppendLine("                     FROM TMP_MD_TABLE_MAPPING");
                        prepareDrivingKeyStatement.AppendLine("                     WHERE TARGET_TABLE_TYPE IN ('Link-Satellite') AND DRIVING_KEY_ATTRIBUTE IS NOT NULL AND DRIVING_KEY_ATTRIBUTE != ''");
                        prepareDrivingKeyStatement.AppendLine("                     AND [PROCESS_INDICATOR] = 'Y'");
                        prepareDrivingKeyStatement.AppendLine("              ) TableName");
                        prepareDrivingKeyStatement.AppendLine("       ) AS A CROSS APPLY DRIVING_KEY_ATTRIBUTE_XML.nodes('/M') AS Split(a)");
                        prepareDrivingKeyStatement.AppendLine(")  base");
                        prepareDrivingKeyStatement.AppendLine("LEFT JOIN [dbo].[TMP_MD_TABLE_MAPPING]");
                        prepareDrivingKeyStatement.AppendLine("        hub");
                        prepareDrivingKeyStatement.AppendLine(" ON  base.SOURCE_TABLE=hub.SOURCE_TABLE");
                        prepareDrivingKeyStatement.AppendLine(" AND hub.TARGET_TABLE_TYPE IN ('Hub')");
                        prepareDrivingKeyStatement.AppendLine("  AND base.BUSINESS_KEY_ATTRIBUTE=hub.BUSINESS_KEY_ATTRIBUTE");
                        prepareDrivingKeyStatement.AppendLine("LEFT JOIN MD_SATELLITE sat");
                        prepareDrivingKeyStatement.AppendLine("  ON base.TARGET_TABLE = sat.[SCHEMA_NAME]+'.'+sat.SATELLITE_NAME");
                        prepareDrivingKeyStatement.AppendLine("LEFT JOIN MD_HUB hubkey");
                        prepareDrivingKeyStatement.AppendLine("  ON hub.TARGET_TABLE = hubkey.[SCHEMA_NAME]+'.'+hubkey.HUB_NAME");
                        prepareDrivingKeyStatement.AppendLine("WHERE 1=1");
                        prepareDrivingKeyStatement.AppendLine("AND base.BUSINESS_KEY_ATTRIBUTE IS NOT NULL");
                        prepareDrivingKeyStatement.AppendLine("AND base.BUSINESS_KEY_ATTRIBUTE!=''");
                        prepareDrivingKeyStatement.AppendLine("AND [PROCESS_INDICATOR] = 'Y'");


                    var listDrivingKeys = GetDataTable(ref connOmd, prepareDrivingKeyStatement.ToString());

                        if (listDrivingKeys.Rows.Count == 0)
                        {
                            _alert.SetTextLogging("-->  No Driving Key based Link-Satellites were detected.\r\n");
                        }
                        else
                        {
                            foreach (DataRow tableName in listDrivingKeys.Rows)
                            {
                                using (var connection = new SqlConnection(metaDataConnection))
                                {
                                    var insertDrivingKeyStatement = new StringBuilder();

                                    insertDrivingKeyStatement.AppendLine("INSERT INTO [MD_DRIVING_KEY_XREF]");
                                    insertDrivingKeyStatement.AppendLine("( [SATELLITE_ID] ,[HUB_ID] )");
                                    insertDrivingKeyStatement.AppendLine("VALUES ");
                                    insertDrivingKeyStatement.AppendLine("(");
                                    insertDrivingKeyStatement.AppendLine("  " + tableName["SATELLITE_ID"] + ",");
                                    insertDrivingKeyStatement.AppendLine("  " + tableName["HUB_ID"]);
                                    insertDrivingKeyStatement.AppendLine(")");

                                    var command = new SqlCommand(insertDrivingKeyStatement.ToString(), connection);

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
                                    }
                                }
                            }
                        }

                        worker.ReportProgress(95);
                        _alert.SetTextLogging("Preparation of the Driving Key column metadata completed.\r\n");

                }
                catch (Exception ex)
                {
                    errorCounter++;
                    _alert.SetTextLogging("An issue has occured during preparation of the Driving Key metadata. Please check the Error Log for more details.\r\n");
                    errorLog.AppendLine("\r\nAn issue has occured during preparation of the Driving Key metadata: \r\n\r\n" + ex);
                }

                #endregion

                //Activation completed!


                // Error handling
                if (errorCounter > 0)
                {
                    _alert.SetTextLogging("\r\nWarning! There were "+errorCounter+" error(s) found while processing the metadata.\r\n");
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

                // Saving the interfaces to Json
                if (checkBoxSaveInterfaceToJson.Checked)
                {
                    _alert.SetTextLogging("\r\nSaving interface output to disk.\r\n");

                    // Business Key Component
                    try
                    {
                        ClassJsonHandling.SaveJsonInterfaceBusinessKeyComponent();
                        _alert.SetTextLogging("\r\n-->  Saving the Business Key Component interface file.");
                    }
                    catch (Exception ex)
                    {
                        _alert.SetTextLogging("\r\n-->  An error has occured saving the Business Key Component interface file. The reported error is: "+ex);
                    }

                    // Business Key Component Part
                    try
                    {
                        ClassJsonHandling.SaveJsonInterfaceBusinessKeyComponentPart();
                        _alert.SetTextLogging("\r\n-->  Saving the Business Key Component Part interface file.");
                    }
                    catch (Exception ex)
                    {
                        _alert.SetTextLogging("\r\n-->  An error has occured saving the Business Key Component Part interface file. The reported error is: " + ex);
                    }

                    // Driving Key
                    try
                    {
                        ClassJsonHandling.SaveJsonInterfaceDrivingKey();
                        _alert.SetTextLogging("\r\n-->  Saving the Driving Key interface file.");
                    }
                    catch (Exception ex)
                    {
                        _alert.SetTextLogging("\r\n-->  An error has occured saving the Driving Key interface file. The reported error is: " + ex);
                    }

                    // Hub Link Xref
                    try
                    {
                        ClassJsonHandling.SaveJsonInterfaceHubLinkXref();
                        _alert.SetTextLogging("\r\n-->  Saving the Hub to Link Xref interface file.");
                    }
                    catch (Exception ex)
                    {
                        _alert.SetTextLogging("\r\n-->  An error has occured saving the Hub to Link Xref interface file. The reported error is: " + ex);
                    }

                    // Physical Model
                    try
                    {
                        ClassJsonHandling.SaveJsonInterfacePhysicalModel();
                        _alert.SetTextLogging("\r\n-->  Saving the Physical Model interface file.");
                    }
                    catch (Exception ex)
                    {
                        _alert.SetTextLogging("\r\n-->  An error has occured saving the Physical Model interface file. The reported error is: " + ex);
                    }

                    // Source Hub Xref
                    try
                    {
                        ClassJsonHandling.SaveJsonInterfaceSourceHubXref();
                        _alert.SetTextLogging("\r\n-->  Saving the Source to Hub Xref interface file.");
                    }
                    catch (Exception ex)
                    {
                        _alert.SetTextLogging("\r\n-->  An error has occured saving the Source to Hub interface file. The reported error is: " + ex);
                    }

                    // Source Link Attribute Xref
                    try
                    {
                        ClassJsonHandling.SaveJsonInterfaceSourceLinkAttributeXref();
                        _alert.SetTextLogging("\r\n-->  Saving the Source to Link Attribute Xref interface file.");
                    }
                    catch (Exception ex)
                    {
                        _alert.SetTextLogging("\r\n-->  An error has occured saving the Source to Link Attribute Xref interface file. The reported error is: " + ex);
                    }

                    // Source Link Xref
                    try
                    {
                        ClassJsonHandling.SaveJsonInterfaceSourceLinkXref();
                        _alert.SetTextLogging("\r\n-->  Saving the Source to Link Xref interface file.");
                    }
                    catch (Exception ex)
                    {
                        _alert.SetTextLogging("\r\n-->  An error has occured saving the Source to Link Xref interface file. The reported error is: " + ex);
                    }

                    // Source Satellite Attribute Xref
                    try
                    {
                        ClassJsonHandling.SaveJsonInterfaceSourceSatelliteAttributeXref();
                        _alert.SetTextLogging("\r\n-->  Saving the Source to Satellite Attribute Xref interface file.");
                    }
                    catch (Exception ex)
                    {
                        _alert.SetTextLogging("\r\n-->  An error has occured saving the Source to Satellite Attribute Xref interface file. The reported error is: " + ex);
                    }

                    // Source Link Xref
                    try
                    {
                        ClassJsonHandling.SaveJsonInterfaceSourceSatelliteXref();
                        _alert.SetTextLogging("\r\n-->  Saving the Source to Satellite Xref interface file.");
                    }
                    catch (Exception ex)
                    {
                        _alert.SetTextLogging("\r\n-->  An error has occured saving the Source to Satellite Xref interface file. The reported error is: " + ex);
                    }

                }

                // Report completion
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
                MessageBox.Show("There is an issue with the data formate for this cell!");
            }
        }

        private void FormManageMetadata_SizeChanged(object sender, EventArgs e)
        {
            GridAutoLayout();
        }

        private void dataGridViewTableMetadata_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Validate the data entry on the Table Mapping datagrid

            var stagingPrefix = ConfigurationSettings.StgTablePrefixValue;
            var cellValue = e.FormattedValue.ToString();
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

                if (valueLength > 0)
                {
                    //if (!cellValue.StartsWith(stagingPrefix))
                    //{
                    //    //dataGridViewTableMetadata.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = System.Drawing.Color.Red;

                    //    e.Cancel = true;
                    //    dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "The Source (Source) name is not conform with the Source prefix ('" + stagingPrefix + "').";
                    //}

                    //if (!e.FormattedValue.ToString().Contains(stagingPrefix))
                    //{
                    //    e.Cancel = true;
                    //    dataGridViewTableMetadata.Rows[e.RowIndex].ErrorText = "The Source (Source) is not conform to the Source prefix ('" + stagingPrefix + "').";
                    //}
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

        private void saveAsDirectionalGraphMarkupLanguageDGMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
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
                    try
                    {
                        var chosenFile = theDialog.FileName;

                        if (dataGridViewTableMetadata != null) // There needs to be metadata available
                        {
                            var connOmd = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };

                            //For later, get the source/target model relationships for Hubs and Sats
                            var sqlStatementForHubCategories = new StringBuilder();
                            sqlStatementForHubCategories.AppendLine("SELECT ");
                            sqlStatementForHubCategories.AppendLine(" [SOURCE_ID]");
                            sqlStatementForHubCategories.AppendLine(",[SOURCE_NAME]");
                            //sqlStatementForHubCategories.AppendLine(",[SOURCE_SCHEMA_NAME]");
                            sqlStatementForHubCategories.AppendLine(",[FILTER_CRITERIA]");
                            sqlStatementForHubCategories.AppendLine(",[SATELLITE_ID]");
                            sqlStatementForHubCategories.AppendLine(",[SATELLITE_NAME]");
                            sqlStatementForHubCategories.AppendLine(",[SATELLITE_TYPE]");
                            sqlStatementForHubCategories.AppendLine(",[HUB_ID]");
                            sqlStatementForHubCategories.AppendLine(",[HUB_NAME]");
                            sqlStatementForHubCategories.AppendLine(",[SOURCE_BUSINESS_KEY_DEFINITION]");
                            sqlStatementForHubCategories.AppendLine(",[LINK_ID]");
                            sqlStatementForHubCategories.AppendLine(",[LINK_NAME]");
                            sqlStatementForHubCategories.AppendLine("FROM [interface].[INTERFACE_SOURCE_SATELLITE_XREF]");
                            sqlStatementForHubCategories.AppendLine("WHERE SATELLITE_TYPE = 'Normal'");
 
                            var modelRelationshipsHubDataTable = GetDataTable(ref connOmd, sqlStatementForHubCategories.ToString());

                            //For later, get the source/target model relationships for Links and Link Satellites
                            var sqlStatementForLinkCategories = new StringBuilder();
                            sqlStatementForLinkCategories.AppendLine("SELECT ");
                            sqlStatementForLinkCategories.AppendLine(" [SOURCE_ID]");
                            sqlStatementForLinkCategories.AppendLine(",[SOURCE_NAME]");
                            //sqlStatementForLinkCategories.AppendLine(",[SOURCE_SCHEMA_NAME]");
                            sqlStatementForLinkCategories.AppendLine(",[FILTER_CRITERIA]");
                            sqlStatementForLinkCategories.AppendLine(",[SATELLITE_ID]");
                            sqlStatementForLinkCategories.AppendLine(",[SATELLITE_NAME]");
                            sqlStatementForLinkCategories.AppendLine(",[SATELLITE_TYPE]");
                            sqlStatementForLinkCategories.AppendLine(",[HUB_ID]");
                            sqlStatementForLinkCategories.AppendLine(",[HUB_NAME]");
                            sqlStatementForLinkCategories.AppendLine(",[SOURCE_BUSINESS_KEY_DEFINITION]");
                            sqlStatementForLinkCategories.AppendLine(",[LINK_ID]");
                            sqlStatementForLinkCategories.AppendLine(",[LINK_NAME]");
                            sqlStatementForLinkCategories.AppendLine("FROM [interface].[INTERFACE_SOURCE_SATELLITE_XREF]");
                            sqlStatementForLinkCategories.AppendLine("WHERE SATELLITE_TYPE = 'Link Satellite'");

                            var modelRelationshipsLinksDataTable = GetDataTable(ref connOmd, sqlStatementForLinkCategories.ToString());


                            //Create the relationships between business concepts (Hubs, Links)
                            var sqlStatementForRelationships = new StringBuilder();
                            sqlStatementForRelationships.AppendLine("SELECT ");
                            sqlStatementForRelationships.AppendLine(" [LINK_ID]");
                            sqlStatementForRelationships.AppendLine(",[LINK_NAME]");
                            sqlStatementForRelationships.AppendLine(",[SOURCE_ID]");
                            sqlStatementForRelationships.AppendLine(",[SOURCE_NAME]");
                            sqlStatementForRelationships.AppendLine(",[SOURCE_SCHEMA_NAME]");
                            sqlStatementForRelationships.AppendLine(",[HUB_ID]");
                            sqlStatementForRelationships.AppendLine(",[HUB_NAME]");
                            sqlStatementForRelationships.AppendLine(",[BUSINESS_KEY_DEFINITION]");
                            sqlStatementForRelationships.AppendLine("FROM [interface].[INTERFACE_HUB_LINK_XREF]");

                            var businessConceptsRelationships = GetDataTable(ref connOmd, sqlStatementForRelationships.ToString());


                            //Make sure the source-to-target mappings are created for the attributes (STG->SAT)
                            var sqlStatementForSatelliteAttributes = new StringBuilder();
                            sqlStatementForSatelliteAttributes.AppendLine("SELECT ");
                            sqlStatementForSatelliteAttributes.AppendLine(" [SOURCE_ID]");
                            sqlStatementForSatelliteAttributes.AppendLine(",[SOURCE_NAME]");
                            sqlStatementForSatelliteAttributes.AppendLine(",[SOURCE_SCHEMA_NAME]");
                            sqlStatementForSatelliteAttributes.AppendLine(",[SATELLITE_ID]");
                            sqlStatementForSatelliteAttributes.AppendLine(",[SATELLITE_NAME]");
                            sqlStatementForSatelliteAttributes.AppendLine(",[SOURCE_ATTRIBUTE_ID]");
                            sqlStatementForSatelliteAttributes.AppendLine(",[SOURCE_ATTRIBUTE_NAME]");
                            sqlStatementForSatelliteAttributes.AppendLine(",[SATELLITE_ATTRIBUTE_ID]");
                            sqlStatementForSatelliteAttributes.AppendLine(",[SATELLITE_ATTRIBUTE_NAME]");
                            sqlStatementForSatelliteAttributes.AppendLine(",[MULTI_ACTIVE_KEY_INDICATOR]");
                            sqlStatementForSatelliteAttributes.AppendLine("FROM [interface].[INTERFACE_SOURCE_SATELLITE_ATTRIBUTE_XREF]");

                            var satelliteAttributes = GetDataTable(ref connOmd, sqlStatementForSatelliteAttributes.ToString());

                            
                            //Create a list of segments to create, based on nodes (Hubs and Sats)
                            List<string> segmentNodeList = new List<string>();

                            foreach (DataRow row in modelRelationshipsHubDataTable.Rows)
                            {
                                var modelRelationshipsHub = (string)row["HUB_NAME"];

                                if (!segmentNodeList.Contains(modelRelationshipsHub))
                                {
                                    segmentNodeList.Add(modelRelationshipsHub);
                                }
                            }
                            
                            // ... and the Links / LSATs
                            foreach (DataRow row in modelRelationshipsLinksDataTable.Rows)
                            {
                                var modelRelationshipsLink = (string)row["LINK_NAME"];

                                if (!segmentNodeList.Contains(modelRelationshipsLink))
                                {
                                    segmentNodeList.Add(modelRelationshipsLink);
                                }
                            }

                            // ... and for any orphan Hubs or Links (without Satellites)
                            foreach (DataRow row in businessConceptsRelationships.Rows)
                            {
                                var modelRelationshipsLink = (string)row["LINK_NAME"];
                                var modelRelationshipsHub = (string)row["HUB_NAME"];

                                if (!segmentNodeList.Contains(modelRelationshipsLink))
                                {
                                    segmentNodeList.Add(modelRelationshipsLink);
                                }

                                if (!segmentNodeList.Contains(modelRelationshipsHub))
                                {
                                    segmentNodeList.Add(modelRelationshipsHub);
                                }
                            }

                            //Build up the list of nodes
                            List<string> nodeList = new List<string>();
                            List<string> systemList = new List<string>();

                            for (int i = 0; i < dataGridViewTableMetadata.Rows.Count - 1; i++)
                            {
                                DataGridViewRow row = dataGridViewTableMetadata.Rows[i];
                                string sourceNode = row.Cells[2].Value.ToString();
                                var systemName = sourceNode.Split('_')[1];
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

                                // Create a system list
                                if (!systemList.Contains(systemName))
                                {
                                    systemList.Add(systemName);
                                }
                            }

                            //Write the nodes to DGML
                            var dgmlExtract = new StringBuilder();
                            dgmlExtract.AppendLine("<?xml version=\"1.0\" encoding=\"utf - 8\"?>");
                            dgmlExtract.AppendLine("<DirectedGraph ZoomLevel=\" - 1\" xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">");
                            dgmlExtract.AppendLine("  <Nodes>");

                            foreach (string node in nodeList)
                            {
                                if (node.Contains("STG_"))
                                {
                                    dgmlExtract.AppendLine("    <Node Id=\"" + node + "\"  Category=\"Source System\" Group=\"Collapsed\" Label=\"" + node +"\" />");
                                }
                                else if (node.Contains("HUB_"))
                                {
                                    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Hub\"  Label=\"" + node + "\" />");
                                }
                                else if (node.Contains("LNK_"))
                                {
                                    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Link\" Label=\"" + node + "\" />");
                                }
                                else if (node.Contains("SAT_") || node.Contains("LSAT_"))
                                {
                                    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Satellite\" Group=\"Collapsed\" Label=\"" + node + "\" />");
                                }
                                else // The others
                                {
                                    dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Category=\"Unknown\" Label=\"" + node + "\" />");
                                }
                            }

                            // Separate routine for attribute nodes, with some additional logic to allow for 'duplicate' nodes e.g. source and target attribute names
                            foreach (DataRow row in satelliteAttributes.Rows)
                            {
                                var sourceNodeLabel = (string)row["SOURCE_ATTRIBUTE_NAME"];
                                var sourceNode = "staging_" + sourceNodeLabel;
                                var targetNodeLabel = (string)row["SATELLITE_ATTRIBUTE_NAME"];
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

                                dgmlExtract.AppendLine("     <Node Id=\"" + sourceNode + "\"  Category=\"Unknown\" Label=\"" + sourceNodeLabel + "\" />");
                                dgmlExtract.AppendLine("     <Node Id=\"" + targetNode + "\"  Category=\"Unknown\" Label=\"" + targetNodeLabel + "\" />");
                            }




                            //Adding the category nodes
                            dgmlExtract.AppendLine("    <Node Id=\"Source\" Group=\"Expanded\" Label=\"Source\"/>");
                            dgmlExtract.AppendLine("    <Node Id=\"Data Vault\" Group=\"Expanded\" Label=\"Data Vault\"/>");

                            //Adding the source system containers as nodes
                            foreach (var node in systemList)
                            {
                                dgmlExtract.AppendLine("     <Node Id=\"" + node + "\"  Group=\"Expanded\" Category=\"Source System\" Label=\"" + node + "\" />");
                            }

                            //Adding the CBC nodes (Hubs and Links)
                            foreach (string node in segmentNodeList)
                            {
                                string segmentName = node.Remove(0, 4).ToLower();
                                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                                segmentName = textInfo.ToTitleCase(segmentName);

                                dgmlExtract.AppendLine("    <Node Id=\"" + segmentName + "\" Group=\"Expanded\" Label=\""+ segmentName + "\" IsHubContainer=\"True\" />");
                            }
                            
                            dgmlExtract.AppendLine("  </Nodes>");
                            //End of Nodes

                            //Edges and containers
                            dgmlExtract.AppendLine("  <Links>");

                            for (var i = 0; i < dataGridViewTableMetadata.Rows.Count - 1; i++)
                            {
                                var row = dataGridViewTableMetadata.Rows[i];
                                var sourceNode = row.Cells[2].Value.ToString();
                                var targetNode = row.Cells[3].Value.ToString();
                                var businessKey = row.Cells[4].Value.ToString();

                                dgmlExtract.AppendLine("    <Link Source=\"" + sourceNode + "\" Target=\""+targetNode+"\" BusinessKeyDefintion=\""+ businessKey +"\"/>");
                            }

                            //Add container groupings (node-based) - adding source system containers to 'Source'
                            foreach (var node in systemList)
                            {
                                dgmlExtract.AppendLine("     <Link Source=\"Source\" Target=\"" + node + "\" Category=\"Contains\" />");
                            }

                            // Adding the Source table to the source system container
                            for (var i = 0; i < dataGridViewTableMetadata.Rows.Count - 1; i++)
                            {
                                var row = dataGridViewTableMetadata.Rows[i];
                                var node = row.Cells[2].Value.ToString();
                                var systemName = node.Split('_')[1];

                                if (node.Contains("STG_"))
                                {
                                    dgmlExtract.AppendLine("    <Link Source=\""+systemName+"\" Target=\"" + node + "\" Category=\"Contains\" />");
                                }
                            }

                            // Separate routine to create STG/ATT and SAT/ATT relationships
                            foreach (DataRow row in satelliteAttributes.Rows)
                            {
                                var sourceNodeSat = (string)row["SATELLITE_NAME"];
                                var targetNodeSat = "dwh_"+(string)row["SATELLITE_ATTRIBUTE_NAME"];
                                var sourceNodeStg = (string)row["SOURCE_NAME"];
                                var targetNodeStg = "staging_"+(string)row["SOURCE_ATTRIBUTE_NAME"];

                                // This is adding the attributes to the tables
                                dgmlExtract.AppendLine("    <Link Source=\"" + sourceNodeSat + "\" Target=\"" + targetNodeSat + "\" Category=\"Contains\" />");
                                dgmlExtract.AppendLine("    <Link Source=\"" + sourceNodeStg + "\" Target=\"" + targetNodeStg + "\" Category=\"Contains\" />");

                                // This is adding the edge between the attributes
                                dgmlExtract.AppendLine("    <Link Source=\"" + targetNodeStg + "\" Target=\"" + targetNodeSat + "\" />");
                            }

                            //Add Data Vault objects to Segment
                            foreach (var node in segmentNodeList)
                            {
                                var segmentName = node.Remove(0, 4).ToLower();
                                var textInfo = new CultureInfo("en-US", false).TextInfo;
                                segmentName = textInfo.ToTitleCase(segmentName);
                                   // <Link Source="Renewal_Membership" Target="LNK_RENEWAL_MEMBERSHIP" Category="Contains" />
                                dgmlExtract.AppendLine("    <Link Source=\"" + segmentName + "\" Target=\"" + node + "\" Category=\"Contains\" />");
                                dgmlExtract.AppendLine("    <Link Source=\"Data Vault\" Target=\"" + segmentName + "\" Category=\"Contains\" />");
                            }

                            //Add groupings to a Hub (CBC), if there is a Satellite
                            foreach (DataRow row in modelRelationshipsHubDataTable.Rows)
                            {
                                if (row["SATELLITE_NAME"] == DBNull.Value || row["HUB_NAME"] == DBNull.Value)
                                    continue;
                                var modelRelationshipsHub = (string) row["HUB_NAME"];
                                var modelRelationshipsSat = (string) row["SATELLITE_NAME"];

                                var segmentName = modelRelationshipsHub.Remove(0, 4).ToLower();
                                var textInfo = new CultureInfo("en-US", false).TextInfo;
                                segmentName = textInfo.ToTitleCase(segmentName);

                                //Map the Satellite to the Hub and CBC
                                dgmlExtract.AppendLine("    <Link Source=\"" + segmentName + "\" Target=\"" +
                                                       modelRelationshipsSat + "\" Category=\"Contains\" />");
                                dgmlExtract.AppendLine("    <Link Source=\"" + modelRelationshipsHub +
                                                       "\" Target=\"" + modelRelationshipsSat + "\" />");
                            }

                            //Add groupings per Link (CBC), if there is a Satellite
                            foreach (DataRow row in modelRelationshipsLinksDataTable.Rows)
                            {
                                if (row["SATELLITE_NAME"] == DBNull.Value || row["LINK_NAME"] == DBNull.Value)
                                    continue;
                                var modelRelationshipsLink = (string)row["LINK_NAME"];
                                var modelRelationshipsSat = (string)row["SATELLITE_NAME"];

                                var segmentName = modelRelationshipsLink.Remove(0, 4).ToLower();
                                var textInfo = new CultureInfo("en-US", false).TextInfo;
                                segmentName = textInfo.ToTitleCase(segmentName);

                                //Map the Satellite to the Link and CBC
                                dgmlExtract.AppendLine("    <Link Source=\"" + segmentName + "\" Target=\"" + modelRelationshipsSat + "\" Category=\"Contains\" />");
                                dgmlExtract.AppendLine("    <Link Source=\"" + modelRelationshipsLink + "\" Target=\"" + modelRelationshipsSat + "\" />");
                            }



                            //Add the relationships between groupings (core business concepts) - from Hub to Link
                            foreach (DataRow row in businessConceptsRelationships.Rows)
                            {
                                if (row["HUB_NAME"] == DBNull.Value || row["LINK_NAME"] == DBNull.Value)
                                    continue;
                                var modelRelationshipsHub = (string)row["HUB_NAME"];
                                var modelRelationshipsLink = (string)row["LINK_NAME"];

                                var segmentNameFrom = modelRelationshipsHub.Remove(0, 4).ToLower();
                                var textInfoFrom = new CultureInfo("en-US", false).TextInfo;
                                segmentNameFrom = textInfoFrom.ToTitleCase(segmentNameFrom);

                                var segmentNameTo = modelRelationshipsLink.Remove(0, 4).ToLower();
                                var textInfoTo = new CultureInfo("en-US", false).TextInfo;
                                segmentNameTo = textInfoTo.ToTitleCase(segmentNameTo);

                                dgmlExtract.AppendLine("    <Link Source=\"" + segmentNameFrom + "\" Target=\"" + segmentNameTo + "\" />");
                            }

                            dgmlExtract.AppendLine("  </Links>");

                            //Add containers
                            dgmlExtract.AppendLine("  <Categories>");
                            dgmlExtract.AppendLine("    <Category Id = \"Source System\" Label = \"Source System\" Background = \"#FFE51400\" IsTag = \"True\" /> ");
                            dgmlExtract.AppendLine("    <Category Id = \"Hub\" Label = \"Hub\" IsTag = \"True\" /> ");
                            dgmlExtract.AppendLine("    <Category Id = \"Link\" Label = \"Link\" IsTag = \"True\" /> ");
                            dgmlExtract.AppendLine("    <Category Id = \"Satellite\" Label = \"Satellite\" IsTag = \"True\" /> ");
                            dgmlExtract.AppendLine("  </Categories>");

                            //Add styles 
                            dgmlExtract.AppendLine("  <Styles >");

                            dgmlExtract.AppendLine("    <Style TargetType = \"Node\" GroupLabel = \"Source System\" ValueLabel = \"Has category\" >");
                            dgmlExtract.AppendLine("      <Condition Expression = \"HasCategory('Source System')\" />");
                            dgmlExtract.AppendLine("      <Setter Property=\"Foreground\" Value=\"#FF000000\" />");
                            dgmlExtract.AppendLine("      <Setter Property = \"Background\" Value = \"#FF6E6A69\" />");
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

                            dgmlExtract.AppendLine("  </Styles >");



                            dgmlExtract.AppendLine("</DirectedGraph>");

                            using (StreamWriter outfile = new StreamWriter(chosenFile))
                            {
                                outfile.Write(dgmlExtract.ToString());
                                outfile.Close();
                            }

                            richTextBoxInformation.Text = "The DGML metadata file file://" + chosenFile + " has been saved successfully.";
                        } 
                        else
                        {
                            richTextBoxInformation.Text = "There was no metadata to save, is the grid view empty?";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A problem occure when attempting to save the file to disk. The detail error message is: " + ex.Message);
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

            if (checkBoxIgnoreVersion.Checked == false && _bindingSourcePhysicalModelMetadata.Count == 0)
            {
                richTextBoxInformation.Text += "There is no physical model metadata available, so the metadata can only be validated with the 'Ignore Version' enabled.\r\n ";
            }
            else
            {
                if (backgroundWorkerValidationOnly.IsBusy) return;
                // create a new instance of the alert form
                _alertValidation = new FormAlert();
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

            // Truncate existing metadata - if selected
            if (checkBoxClearMetadata.Checked)
            {
                TruncateMetadata();
            }

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
            var completeDataTable = stagingReverseEngineerResults.Copy();
            completeDataTable.Merge(integrationReverseEngineerResults);
            completeDataTable.Merge(psaReverseEngineerResults);
            completeDataTable.Merge(presentationReverseEngineerResults);

            completeDataTable.DefaultView.Sort = "[DATABASE_NAME] ASC, [SCHEMA_NAME] ASC, [TABLE_NAME] ASC, [ORDINAL_POSITION] ASC";

            // Display the results on the datagrid
            _bindingSourcePhysicalModelMetadata.DataSource = completeDataTable;

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
            sqlStatementForAttributeVersion.AppendLine("  CONVERT(CHAR(32),HASHBYTES('MD5',CONVERT(NVARCHAR(100), " + GlobalParameters.currentVersionId + ") + '|' + OBJECT_NAME(main.OBJECT_ID) + '|' + main.[name]),2) AS ROW_CHECKSUM,");
            sqlStatementForAttributeVersion.AppendLine("  " + GlobalParameters.currentVersionId + " AS [VERSION_ID],");
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

            sqlStatementForAttributeVersion.AppendLine("WHERE OBJECT_NAME(main.OBJECT_ID) LIKE '" + prefix + "_%'");

            // Retrieve (and apply) the list of tables to filter from the Table Mapping datagrid
            sqlStatementForAttributeVersion.AppendLine("  AND OBJECT_NAME(main.OBJECT_ID) IN (");
            var filterList = TableMetadataFilter((DataTable)_bindingSourceTableMetadata.DataSource);
            foreach (var filter in filterList)
            {
                // Always add the 'regular' mapping.
                sqlStatementForAttributeVersion.AppendLine("  '" + filter + "',");

                // Workaround to allow PSA tables to be reverse-engineered automatically by replacing the STG prefix/suffix
                if (filter.StartsWith(ConfigurationSettings.StgTablePrefixValue+"_") || filter.EndsWith("_"+ConfigurationSettings.StgTablePrefixValue))
                {
                    var tempFilter = filter.Replace(ConfigurationSettings.StgTablePrefixValue,ConfigurationSettings.PsaTablePrefixValue);
                    sqlStatementForAttributeVersion.AppendLine("  '" + tempFilter + "',");
                }
            }
            sqlStatementForAttributeVersion.Remove(sqlStatementForAttributeVersion.Length - 3, 3);
            sqlStatementForAttributeVersion.AppendLine();
            sqlStatementForAttributeVersion.AppendLine("  )");
            sqlStatementForAttributeVersion.AppendLine("ORDER BY main.column_id");

            var reverseEngineerResults = GetDataTable(ref conn, sqlStatementForAttributeVersion.ToString());
            conn.Close();
            return reverseEngineerResults;
        }
        
        public void exportThisRowAsSourcetoTargetInterfaceJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Check if any cells were clicked / selected
            Int32 selectedCellCount = dataGridViewTableMetadata.SelectedCells.Count;

            if (selectedCellCount > 0)
            {
                //For every cell, get the row and the rest of the row values
                for (int i = 0; i < selectedCellCount; i++)
                {
                    var fullRow = dataGridViewTableMetadata.SelectedCells[i].OwningRow;

                    var sourceTableName = fullRow.Cells[2].Value.ToString();
                    var targetTableName = fullRow.Cells[3].Value.ToString();
                    var businessKeyDefinition = fullRow.Cells[4].Value.ToString();

                    CreateSourceToTargetMapping(sourceTableName, targetTableName, businessKeyDefinition);
                }
            }
        }

        public void CreateSourceToTargetMapping(string sourceTableName, string targetTableName,string businessKeyDefinition)
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

        /// <summary>
        ///   Run the validation checks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerValidation_DoWork(object sender, DoWorkEventArgs e)
        {
            //LBM 2019-01-24 - We don't need to have the checked box marked when pressing Validation Only, removing the IF
            //LBM 2109-05-25 - Need to create a validation to check if all Mapped Attibutes exists in the Target
            //if (checkBoxValidation.Checked)
            //{
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
                        ValidateSourceObject();
                    }
                    if (worker != null) worker.ReportProgress(15);

                    if (ValidationSettings.TargetObjectExistence == "True")
                    {
                        ValidateTargetObject();
                    }
                    if (worker != null) worker.ReportProgress(30);

                    if (ValidationSettings.SourceBusinessKeyExistence == "True")
                    {
                        ValidateBusinessKeyObject();
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
                    _alertValidation.SetTextLogging("\r\nIn total "+ MetadataParameters.ValidationIssues + " validation issues have been found.");
                }
            //}
            //else
            //{
            //    // Raise exception
            //    MessageBox.Show("Validation has been requested but is disabled in the application. Please re-enable the validation checkbox.", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        internal static class MetadataParameters
        {
            // TEAM core path parameters
            public static int ValidationIssues { get; set; }
            public static bool ValidationRunning {get; set;}
        }

        private void ValidateSourceObject()
        {
            string evaluationMode = checkBoxIgnoreVersion.Checked ? "physical" : "virtual";

            #region Validation for Source Object Existence
            // Informing the user.
            _alertValidation.SetTextLogging("--> Commencing the validation to determine if the sources as captured in metadata exists in the physical model.\r\n\r\n");

            // Creating a list of unique table names from the data grid / data table
            var objectListSTG = new List<string>();
            var objectListPSA = new List<string>();
            var stagingPrefix = ConfigurationSettings.StgTablePrefixValue;
            var psaPrefix = ConfigurationSettings.PsaTablePrefixValue;
            var resultList = new Dictionary<string, string>();

            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (!row.IsNewRow)
                {
                    if (row.Cells[2].Value.ToString().Substring(0, stagingPrefix.Length) == stagingPrefix)
                    {
                        if (!objectListSTG.Contains(row.Cells[2].Value.ToString()))
                        {
                            objectListSTG.Add(row.Cells[2].Value.ToString());
                        }
                    }
                    else if (row.Cells[2].Value.ToString().Substring(0, psaPrefix.Length) == psaPrefix)
                    {
                        if (!objectListPSA.Contains(row.Cells[2].Value.ToString()))
                        {
                            objectListPSA.Add(row.Cells[2].Value.ToString());
                        }
                    }
                    else
                    {
                        if (!resultList.ContainsKey(row.Cells[2].Value.ToString()))
                            resultList.Add(row.Cells[2].Value.ToString(), "The provided prefix doesn't match either Staging or Persistent Staging.\r\n");
                    }
                }
            }


            // Execute the validation check using the list of unique objects

            //Validate STG Entries 
            foreach (string sourceObject in objectListSTG)
            {
                string sourceObjectValidated = "False";
                if (evaluationMode == "physical")
                {
                    sourceObjectValidated = ClassMetadataValidation.ValidateObjectExistencePhysical(sourceObject,ConfigurationSettings.ConnectionStringStg);
                }
                else if (evaluationMode == "virtual")
                {
                    sourceObjectValidated = ClassMetadataValidation.ValidateObjectExistenceVirtual(sourceObject,(DataTable) _bindingSourcePhysicalModelMetadata.DataSource);
                }
                else
                {
                    sourceObjectValidated = "The validation approach (physical/virtual) could not be asserted.";
                }

                // Add negative results to dictionary
                if (sourceObjectValidated == "False")
                {
                    resultList.Add(sourceObject, sourceObjectValidated); // Add objects that did not pass the test
                }
            }


            //Validate PSA Entries
            foreach (string sourceObject in objectListPSA)
            {
                string sourceObjectValidated = "False";
                if (evaluationMode == "physical")
                {
                    sourceObjectValidated =ClassMetadataValidation.ValidateObjectExistencePhysical(sourceObject,ConfigurationSettings.ConnectionStringHstg);
                }
                else if (evaluationMode == "virtual")
                {
                    sourceObjectValidated = ClassMetadataValidation.ValidateObjectExistenceVirtual(sourceObject, (DataTable)_bindingSourcePhysicalModelMetadata.DataSource);
                }
                else
                {
                    sourceObjectValidated = "The validation approach (physical/virtual) could not be asserted.";
                }

                if (sourceObjectValidated == "False")
                {
                    resultList.Add(sourceObject, sourceObjectValidated); // Add objects that did not pass the test
                }
            }


            // Return the results back to the user
            if (resultList.Count > 0)
            {
                foreach (var sourceObjectResult in resultList)
                {
                    _alertValidation.SetTextLogging(sourceObjectResult.Key + " is tested with this outcome: " + sourceObjectResult.Value + "\r\n");
                }

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count();
            }
            else
            {
                _alertValidation.SetTextLogging("There were no validation issues related to the existence of the source table / object (Source table).\r\n");
            }

            #endregion
        }

        private void ValidateTargetObject()
        {
            string evaluationMode = checkBoxIgnoreVersion.Checked ? "physical" : "virtual";

            #region Validation for Source Object Existence
            // Informing the user.
            _alertValidation.SetTextLogging("\r\n--> Commencing the validation to determine if the Integration Layer metadata exists in the physical model.\r\n\r\n");

            // Creating a list of unique table names from the data grid / data table
            var objectList = new List<string>();
            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (!row.IsNewRow)
                {
                    if (!objectList.Contains(row.Cells[3].Value.ToString()))
                    {
                        objectList.Add(row.Cells[3].Value.ToString());
                    }
                }
            }


            // Execute the validation check using the list of unique objects
            var resultList = new Dictionary<string, string>();
            foreach (string sourceObject in objectList)
            {
                string sourceObjectValidated = "False";
                if (evaluationMode == "physical")
                {
                    sourceObjectValidated = ClassMetadataValidation.ValidateObjectExistencePhysical(sourceObject, ConfigurationSettings.ConnectionStringInt);
                }
                else if (evaluationMode == "virtual")
                {
                    sourceObjectValidated = ClassMetadataValidation.ValidateObjectExistenceVirtual(sourceObject, (DataTable)_bindingSourcePhysicalModelMetadata.DataSource);
                }
                else
                {
                    sourceObjectValidated = "The validation approach (physical/virtual) could not be asserted.";
                }



                if (sourceObjectValidated == "False")
                {
                    resultList.Add(sourceObject, sourceObjectValidated); // Add objects that did not pass the test
                }
            }


            // Return the results back to the user
            if (resultList.Count > 0)
            {
                foreach (var sourceObjectResult in resultList)
                {
                    _alertValidation.SetTextLogging(sourceObjectResult.Key + " is tested with this outcome: " + sourceObjectResult.Value + "\r\n");
                }

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count();
            }
            else
            {
                _alertValidation.SetTextLogging("There were no validation issues related to the existence of the target table / object (Integration Layer table).\r\n");
            }

            #endregion
        }

        /// <summary>
        /// This method will check if the order of the keys in the Link is consistent with the physical table structures
        /// </summary>
        internal void ValidateLinkKeyOrder()
        {
            string evaluationMode = checkBoxIgnoreVersion.Checked ? "physical" : "virtual";

            #region Retrieving the Links
            // Informing the user.
            _alertValidation.SetTextLogging("\r\n--> Commencing the validation to ensure the order of Business Keys in the Link metadata corresponds with the physical model.\r\n\r\n");


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
                var sourceObjectValidated = ClassMetadataValidation.ValidateLinkKeyOrder(sourceObject, ConfigurationSettings.ConnectionStringOmd, GlobalParameters.currentVersionId, (DataTable)_bindingSourceTableMetadata.DataSource, (DataTable)_bindingSourcePhysicalModelMetadata.DataSource,evaluationMode);

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
                    _alertValidation.SetTextLogging(sourceObjectResult.Key + " is tested with this outcome: " + sourceObjectResult.Value + "\r\n");
                }

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count();
            }
            else
            {
                _alertValidation.SetTextLogging("There were no validation issues related to order of business keys in the Link tables.\r\n");
            }



        }

        internal void ValidateLogicalGroup()
        {
            string evaluationMode = checkBoxIgnoreVersion.Checked ? "physical" : "virtual";

            #region Retrieving the Integration Layer tables
            // Informing the user.
            _alertValidation.SetTextLogging("\r\n--> Commencing the validation to check if the functional dependencies (logical group / unit of work) are present.\r\n\r\n");

            // Creating a list of tables which are dependent on other tables being present
            var objectList = new List<Tuple<string, string, string>>();
            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (!row.IsNewRow && (row.Cells[3].Value.ToString().StartsWith(ConfigurationSettings.LinkTablePrefixValue) || row.Cells[3].Value.ToString().StartsWith(ConfigurationSettings.SatTablePrefixValue) || row.Cells[3].Value.ToString().StartsWith(ConfigurationSettings.LsatPrefixValue))  )
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
                var sourceObjectValidated = ClassMetadataValidation.ValidateLogicalGroup(sourceObject, ConfigurationSettings.ConnectionStringOmd, GlobalParameters.currentVersionId, (DataTable)_bindingSourceTableMetadata.DataSource);

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
                    _alertValidation.SetTextLogging(sourceObjectResult.Key + " is tested with this outcome: " + sourceObjectResult.Value + "\r\n");
                }

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count();
            }
            else
            {
                _alertValidation.SetTextLogging("There were no validation issues related to order of business keys in the Link tables.\r\n");
            }


        }

        /// <summary>
        ///   A validation check to make sure the Business Key is available in the source model.
        /// </summary>
        private void ValidateBusinessKeyObject()
        {
            string evaluationMode = checkBoxIgnoreVersion.Checked ? "physical" : "virtual";

            #region Validation for source Business Key attribute existence
            // Informing the user.
            _alertValidation.SetTextLogging("\r\n--> Commencing the validation to determine if the Business Key metadata attributes exist in the physical model.\r\n\r\n");

            // Creating a list of (Source) table names and business key (combinations) from the data grid / data table
            var objectListSTG = new List<Tuple<string, string>>();
            var objectListPSA = new List<Tuple<string, string>>();
            var stagingPrefix = ConfigurationSettings.StgTablePrefixValue;
            var psaPrefix = ConfigurationSettings.PsaTablePrefixValue;
            var resultList = new Dictionary<Tuple<string, string>, bool>();

            foreach (DataGridViewRow row in dataGridViewTableMetadata.Rows)
            {
                if (!row.IsNewRow)
                {
                    if (row.Cells[2].Value.ToString().Substring(0, stagingPrefix.Length) == stagingPrefix)
                    {
                        if (!objectListSTG.Contains(new Tuple<string, string>(row.Cells[2].Value.ToString(), row.Cells[4].Value.ToString())))
                        {
                            objectListSTG.Add(new Tuple<string, string>(row.Cells[2].Value.ToString(), row.Cells[4].Value.ToString()));
                        }
                    }
                    else if (row.Cells[2].Value.ToString().Substring(0, psaPrefix.Length) == psaPrefix)
                    {
                        if (!objectListPSA.Contains(new Tuple<string, string>(row.Cells[2].Value.ToString(), row.Cells[4].Value.ToString())))
                        {
                            objectListPSA.Add(new Tuple<string, string>(row.Cells[2].Value.ToString(), row.Cells[4].Value.ToString()));
                        }
                    }
                }
            }




            // Execute the validation check using the list of unique objects
            foreach (var sourceObject in objectListSTG)
            {
                // The validation check returns a Dictionary
                var sourceObjectValidated = new Dictionary<Tuple<string, string>, bool>();
                if (evaluationMode == "physical")
                {
                    sourceObjectValidated = ClassMetadataValidation.ValidateSourceBusinessKeyExistencePhysical(sourceObject, ConfigurationSettings.ConnectionStringStg, GlobalParameters.currentVersionId);
                }
                else if (evaluationMode == "virtual")
                {
                    sourceObjectValidated = ClassMetadataValidation.ValidateSourceBusinessKeyExistenceVirtual(sourceObject, (DataTable)_bindingSourcePhysicalModelMetadata.DataSource);
                }
                else
                {
                    //sourceObjectValidated = "The validation approach (physical/virtual) could not be asserted.";
                }

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

            foreach (var sourceObject in objectListPSA)
            {
                // The validation check returns a Dictionary
                var sourceObjectValidated = ClassMetadataValidation.ValidateSourceBusinessKeyExistencePhysical(sourceObject, ConfigurationSettings.ConnectionStringHstg, GlobalParameters.currentVersionId);

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
                    _alertValidation.SetTextLogging("Table " + sourceObjectResult.Key.Item1 + " does not contain Business Key attribute " + sourceObjectResult.Key.Item2 + "\r\n");
                }

                MetadataParameters.ValidationIssues = MetadataParameters.ValidationIssues + resultList.Count();
            }
            else
            {
                _alertValidation.SetTextLogging("There were no validation issues related to the existence of the business keys in the Source tables.\r\n");
            }
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
                richTextBoxInformation.Text += "\r\nThe metadata was validated succesfully!\r\n";
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
                                var backupFile = new ClassJsonHandling();
                                var targetFileName = backupFile.BackupJsonFile(GlobalParameters.JsonModelMetadataFileName + @"_v" + GlobalParameters.currentVersionId + ".json");
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
                        DataTable dt = ConvertToDataTable(jsonArray);

                        // Setup the datatable with proper headings
                        dt.Columns[0].ColumnName = "VERSION_ATTRIBUTE_HASH";
                        dt.Columns[1].ColumnName = "VERSION_ID";
                        dt.Columns[2].ColumnName = "DATABASE_NAME";
                        dt.Columns[3].ColumnName = "SCHEMA_NAME";
                        dt.Columns[4].ColumnName = "TABLE_NAME";
                        dt.Columns[5].ColumnName = "COLUMN_NAME";
                        dt.Columns[6].ColumnName = "DATA_TYPE";
                        dt.Columns[7].ColumnName = "CHARACTER_MAXIMUM_LENGTH";
                        dt.Columns[8].ColumnName = "NUMERIC_PRECISION";
                        dt.Columns[9].ColumnName = "ORDINAL_POSITION";
                        dt.Columns[10].ColumnName = "PRIMARY_KEY_INDICATOR";
                        dt.Columns[11].ColumnName = "MULTI_ACTIVE_INDICATOR";

                        // Sort the columns
                        dt.DefaultView.Sort = "[DATABASE_NAME] ASC, [SCHEMA_NAME] ASC, [TABLE_NAME] ASC, [ORDINAL_POSITION] ASC";

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
    }
}