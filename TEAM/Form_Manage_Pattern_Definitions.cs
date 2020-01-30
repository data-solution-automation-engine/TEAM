using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TEAM
{
    public partial class FormManagePattern: FormBase
    {
        private bool _formLoading = true;
        private FormMain parentFormMain;

        private BindingSource _bindingSourceLoadPatternDefinition = new BindingSource();

        // Default constructor
        public FormManagePattern()
        {
            InitializeComponent();
        }

        // TEAM form constructor
        public FormManagePattern(FormMain parent) : base(parent)
        {
            this.parentFormMain = parent;
            InitializeComponent();

            var patternDefinition = new LoadPatternDefinitionFileHandling();
            ConfigurationSettings.patternDefinitionList = patternDefinition.DeserializeLoadPatternDefinition();

            // Load Pattern definition in memory
            if ((ConfigurationSettings.patternDefinitionList != null) &&
                (!ConfigurationSettings.patternDefinitionList.Any()))
            {
                richTextBoxInformationMain.Text= "There are no pattern definitions / types found in the designated load pattern directory. Please verify if there is a " +
                                                 GlobalParameters.LoadPatternDefinitionFile + " in the " +
                                                 ConfigurationSettings.LoadPatternListPath +
                                                 " directory, and if the file contains pattern types.";
            }


            populateLoadPatternDefinitionDataGrid();

            _formLoading = false;
        }

        internal static List<LoadPatternDefinition> patternDefinitionList { get; set; }

        internal class LoadPatternDefinition
        {
            public int LoadPatternKey { get; set; }
            public string LoadPatternType { get; set; }
            public string LoadPatternSelectionQuery { get; set; }
            public string LoadPatternBaseQuery { get; set; }
            public string LoadPatternAttributeQuery { get; set; }
            public string LoadPatternAdditionalBusinessKeyQuery { get; set; }
            public string LoadPatternNotes { get; set; }
            public string LoadPatternConnectionKey { get; set; }

            /// <summary>
            /// Create a file backup for the configuration file at the provided location and return notice of success or failure as a string.
            /// /// </summary>
            internal static string BackupLoadPatternDefinition(string loadPatternDefinitionFilePath)
            {
                string returnMessage = "";

                try
                {
                    if (File.Exists(loadPatternDefinitionFilePath))
                    {
                        var targetFilePathName = loadPatternDefinitionFilePath +
                                                 string.Concat("Backup_" + DateTime.Now.ToString("yyyyMMddHHmmss"));

                        File.Copy(loadPatternDefinitionFilePath, targetFilePathName);
                        returnMessage = "A backup was created at: " + targetFilePathName;
                    }
                    else
                    {
                        returnMessage = "VEDW couldn't locate a configuration file! Can you check the paths and existence of directories?";
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ("An error has occured while creating a file backup. The error message is " + ex);
                }

                return returnMessage;
            }

            internal Dictionary<String, String> MatchConnectionKey()
            {
                Dictionary<string, string> returnValue = new Dictionary<string, string>();

                if (LoadPatternConnectionKey == "SourceDatabase")
                {
                    returnValue.Add(LoadPatternConnectionKey, ConfigurationSettings.ConnectionStringSource);
                }
                else if (LoadPatternConnectionKey == "StagingDatabase")
                {
                    returnValue.Add(LoadPatternConnectionKey, ConfigurationSettings.ConnectionStringStg);
                }
                else if (LoadPatternConnectionKey == "PersistentStagingDatabase")
                {
                    returnValue.Add(LoadPatternConnectionKey, ConfigurationSettings.ConnectionStringHstg);
                }
                else if (LoadPatternConnectionKey == "IntegrationDatabase")
                {
                    returnValue.Add(LoadPatternConnectionKey, ConfigurationSettings.ConnectionStringInt);
                }
                else if (LoadPatternConnectionKey == "PresentationDatabase")
                {
                    returnValue.Add(LoadPatternConnectionKey, ConfigurationSettings.ConnectionStringPres);
                }

                return returnValue;
            }



            /// <summary>
            /// The method that backs-up and saves a specific pattern (based on its path) with whatever is passed as contents.
            /// </summary>
            /// <param name="loadPatternFilePath"></param>
            /// <param name="fileContent"></param>
            /// <returns></returns>
            internal static string SaveLoadPattern(string loadPatternDefinitionFilePath, string fileContent)
            {
                string returnMessage = "";

                try
                {
                    using (var outfile = new StreamWriter(loadPatternDefinitionFilePath))
                    {
                        outfile.Write(fileContent);
                        outfile.Close();
                    }

                    returnMessage = "The file has been updated.";
                }
                catch (Exception ex)
                {
                    returnMessage = ("An error has occured while creating saving the file. The error message is " + ex);
                }


                return returnMessage;
            }
        }

        class LoadPatternDefinitionFileHandling
        {
            internal List<LoadPatternDefinition> DeserializeLoadPatternDefinition()
            {
                List<LoadPatternDefinition> loadPatternDefinitionList = new List<LoadPatternDefinition>();
                // Retrieve the file contents and store in a string
                if (File.Exists(ConfigurationSettings.LoadPatternListPath + GlobalParameters.LoadPatternDefinitionFile))
                {
                    var jsonInput = File.ReadAllText(ConfigurationSettings.LoadPatternListPath +
                                                     GlobalParameters.LoadPatternDefinitionFile);

                    //Move the (json) string into a List object (a list of the type LoadPattern)
                    loadPatternDefinitionList = JsonConvert.DeserializeObject<List<LoadPatternDefinition>>(jsonInput);
                }

                // Return the list to the instance
                return loadPatternDefinitionList;
            }
        }

        public void populateLoadPatternDefinitionDataGrid()
        {
            // Create a datatable 
            DataTable dt = patternDefinitionList.ToDataTable();

            dt.AcceptChanges(); //Make sure the changes are seen as committed, so that changes can be detected later on
            dt.Columns[0].ColumnName = "Key";
            dt.Columns[1].ColumnName = "Type";
            dt.Columns[2].ColumnName = "SelectionQuery";
            dt.Columns[3].ColumnName = "BaseQuery";
            dt.Columns[4].ColumnName = "AttributeQuery";
            dt.Columns[5].ColumnName = "AdditionalBusinessKeyQuery";
            dt.Columns[6].ColumnName = "Notes";
            dt.Columns[7].ColumnName = "ConnectionKey";

            _bindingSourceLoadPatternDefinition.DataSource = dt;


                // Set the column header names.
                dataGridViewLoadPatternDefinition.DataSource = _bindingSourceLoadPatternDefinition;
                dataGridViewLoadPatternDefinition.ColumnHeadersVisible = true;
                dataGridViewLoadPatternDefinition.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                dataGridViewLoadPatternDefinition.Columns[0].HeaderText = "Key";
                dataGridViewLoadPatternDefinition.Columns[0].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.TopLeft;

                dataGridViewLoadPatternDefinition.Columns[1].HeaderText = "Type";
                dataGridViewLoadPatternDefinition.Columns[1].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.TopLeft;

                dataGridViewLoadPatternDefinition.Columns[2].HeaderText = "Selection Query";
                dataGridViewLoadPatternDefinition.Columns[2].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dataGridViewLoadPatternDefinition.Columns[2].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.TopLeft;

                dataGridViewLoadPatternDefinition.Columns[3].HeaderText = "Base Query";
                dataGridViewLoadPatternDefinition.Columns[3].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dataGridViewLoadPatternDefinition.Columns[3].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.TopLeft;

                dataGridViewLoadPatternDefinition.Columns[4].HeaderText = "Attribute Query";
                dataGridViewLoadPatternDefinition.Columns[4].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dataGridViewLoadPatternDefinition.Columns[4].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.TopLeft;

                dataGridViewLoadPatternDefinition.Columns[5].HeaderText = "Add. Business Key Query";
                dataGridViewLoadPatternDefinition.Columns[5].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dataGridViewLoadPatternDefinition.Columns[5].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.TopLeft;

                dataGridViewLoadPatternDefinition.Columns[6].HeaderText = "Notes";
                dataGridViewLoadPatternDefinition.Columns[6].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dataGridViewLoadPatternDefinition.Columns[6].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.TopLeft;

                dataGridViewLoadPatternDefinition.Columns[7].HeaderText = "ConnectionKey";
                dataGridViewLoadPatternDefinition.Columns[7].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dataGridViewLoadPatternDefinition.Columns[7].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.TopLeft;
            

            GridAutoLayoutLoadPatternDefinition();
        }

        private void GridAutoLayoutLoadPatternDefinition()
        {
            //Table Mapping metadata grid - set the auto size based on all cells for each column
            for (var i = 0; i < dataGridViewLoadPatternDefinition.Columns.Count - 1; i++)
            {
                dataGridViewLoadPatternDefinition.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            if (dataGridViewLoadPatternDefinition.Columns.Count > 0)
            {
                dataGridViewLoadPatternDefinition.Columns[dataGridViewLoadPatternDefinition.Columns.Count - 1]
                    .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            // Table Mapping metadata grid - disable the auto size again (to enable manual resizing)
            for (var i = 0; i < dataGridViewLoadPatternDefinition.Columns.Count - 1; i++)
            {
                int columnWidth = dataGridViewLoadPatternDefinition.Columns[i].Width;
                dataGridViewLoadPatternDefinition.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridViewLoadPatternDefinition.Columns[i].Width = columnWidth;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBoxLoadPatternPath_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void buttonSaveAsLoadPatternDefinition_Click(object sender, EventArgs e)
        {

        }
    }
}
