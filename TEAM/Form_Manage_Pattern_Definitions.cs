using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            if ((ConfigurationSettings.patternDefinitionList != null) && (!ConfigurationSettings.patternDefinitionList.Any()))
            {
                richTextBoxInformationMain.Text= "There are no pattern definitions / types found in the designated load pattern directory. Please verify if the file "+ GlobalParameters.RootPath + @"..\..\..\LoadPatterns\" + GlobalParameters.LoadPatternDefinitionFile + "exits.";
            }

            if (ConfigurationSettings.patternDefinitionList != null) 
            {
                populateLoadPatternDefinitionDataGrid();
                textBoxLoadPatternPath.Text = ConfigurationSettings.LoadPatternListPath;
            }
            else
            {
                richTextBoxInformationMain.Text = "The pattern definition file could not be loaded. Please verify if the file " + GlobalParameters.RootPath + @"..\..\..\LoadPatterns\" + GlobalParameters.LoadPatternDefinitionFile + "exits.";

            }

            dataGridViewLoadPatternDefinition.Focus();
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

        internal class LoadPatternDefinitionFileHandling
        {
            internal List<LoadPatternDefinition> DeserializeLoadPatternDefinition()
            {
                List<LoadPatternDefinition> loadPatternDefinitionList = new List<LoadPatternDefinition>();

                // Retrieve the file contents and store in a string
                if (File.Exists(GlobalParameters.RootPath + @"..\..\..\LoadPatterns\" + GlobalParameters.LoadPatternDefinitionFile))
                {
                    var jsonInput = File.ReadAllText(GlobalParameters.RootPath + @"..\..\..\LoadPatterns\" + GlobalParameters.LoadPatternDefinitionFile);

                    //Move the (json) string into a List object (a list of the type LoadPattern)
                    loadPatternDefinitionList = JsonConvert.DeserializeObject<List<LoadPatternDefinition>>(jsonInput);

                    ConfigurationSettings.patternDefinitionList = loadPatternDefinitionList;
                    ConfigurationSettings.LoadPatternListPath = Path.GetFullPath(GlobalParameters.RootPath + @"..\..\..\LoadPatterns\");
                }
                else
                {
                    //richTextBoxInformationMain.Text = "The file " + ConfigurationSettings.LoadPatternListPath +
                    //                                  GlobalParameters.LoadPatternDefinitionFile +
                    //                                  " could not be found!";
                    loadPatternDefinitionList = null;
                }

                // Return the list to the instance
                return loadPatternDefinitionList;
            }
        }

        public void populateLoadPatternDefinitionDataGrid()
        {
            // Create a datatable 
            DataTable dt = ConfigurationSettings.patternDefinitionList.ToDataTable();

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
            try
            {
                richTextBoxInformationMain.Clear();

                var chosenFile = ConfigurationSettings.LoadPatternListPath + GlobalParameters.LoadPatternDefinitionFile;


                DataTable gridDataTable = (DataTable)_bindingSourceLoadPatternDefinition.DataSource;

                // Make sure the output is sorted
                gridDataTable.DefaultView.Sort = "[KEY] ASC";

                gridDataTable.TableName = "LoadPatternDefinition";

                JArray outputFileArray = new JArray();
                foreach (DataRow singleRow in gridDataTable.DefaultView.ToTable().Rows)
                {
                    JObject individualRow = JObject.FromObject(new
                    {
                        loadPatternKey = singleRow[0].ToString(),
                        loadPatternType = singleRow[1].ToString(),
                        LoadPatternSelectionQuery = singleRow[2].ToString(),
                        loadPatternBaseQuery = singleRow[3].ToString(),
                        loadPatternAttributeQuery = singleRow[4].ToString(),
                        loadPatternAdditionalBusinessKeyQuery = singleRow[5].ToString(),
                        loadPatternNotes = singleRow[6].ToString(),
                        LoadPatternConnectionKey = singleRow[7].ToString()
                    });
                    outputFileArray.Add(individualRow);
                }

                string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);

                // Create a backup file, if enabled
                if (checkBoxBackupFiles.Checked)
                {
                    try
                    {
                        var backupFile = new ClassJsonHandling();
                        var targetFileName = backupFile.BackupJsonFile(GlobalParameters.LoadPatternDefinitionFile, ConfigurationSettings.LoadPatternListPath);
                        richTextBoxInformationMain.Text="A backup of the in-use JSON file was created as " + targetFileName + ".\r\n\r\n";
                    }
                    catch (Exception exception)
                    {
                        richTextBoxInformationMain.Text = "An issue occured when trying to make a backup of the in-use JSON file. The error message was " +
                                                          exception + ".";
                    }
                }

                File.WriteAllText(chosenFile, json);

                richTextBoxInformationMain.Text = "The file " + chosenFile + " was updated.\r\n";

                try
                {
                    // Quick fix, in the file again to commit changes to memory.
                    ConfigurationSettings.patternDefinitionList =
                        JsonConvert.DeserializeObject<List<LoadPatternDefinition>>(File.ReadAllText(chosenFile));
        
                }
                catch (Exception ex)
                {
                    richTextBoxInformationMain.AppendText(
                        "An issue was encountered when regenerating the UI (Tab Pages). The reported error is " + ex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void dataGridViewLoadPatternDefinition_SizeChanged(object sender, EventArgs e)
        {
            GridAutoLayoutLoadPatternDefinition();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            var fileBrowserDialog = new FolderBrowserDialog();
            fileBrowserDialog.SelectedPath = textBoxLoadPatternPath.Text;

            DialogResult result = fileBrowserDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fileBrowserDialog.SelectedPath))
            {
                string[] files = Directory.GetFiles(fileBrowserDialog.SelectedPath);

                int fileCounter = 0;
                foreach (string file in files)
                {
                    if (file.Contains("loadPatternCollection"))
                    {
                        fileCounter++;
                    }
                }

                string finalPath = "";
                if (fileBrowserDialog.SelectedPath.EndsWith(@"\"))
                {
                    finalPath = fileBrowserDialog.SelectedPath;
                }
                else
                {
                    finalPath = fileBrowserDialog.SelectedPath + @"\";
                }


                textBoxLoadPatternPath.Text = finalPath;

                if (fileCounter == 0)
                {
                    richTextBoxInformationMain.Text =
                        "The selected directory does not seem to contain a loadPatternCollection.json file. Did you select a correct Load Pattern directory?";
                }
                else
                {
                    richTextBoxInformationMain.Text =
                        "The path now points to a directory that contains the loadPatternCollection.json Load Pattern Collection file.";
                }

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

        private void openConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Load Pattern Definition File",
                Filter = @"Load Pattern Definition|*.json",
                InitialDirectory = ConfigurationSettings.LoadPatternListPath
            };

            var ret = STAShowDialog(theDialog);

            if (ret == DialogResult.OK)
            {
                try
                {
                    var chosenFile = theDialog.FileName;
                  

                    //string filePath = Path.GetFullPath(theDialog.FileName);

                    // Save the list to memory
                    ConfigurationSettings.patternDefinitionList = JsonConvert.DeserializeObject<List<LoadPatternDefinition>>(File.ReadAllText(chosenFile));

                    // ... and populate the data grid
                    populateLoadPatternDefinitionDataGrid();

                    richTextBoxInformationMain.Text="The file " + chosenFile + " was loaded.\r\n";

                    GridAutoLayoutLoadPatternDefinition();
                }
                catch (Exception ex)
                {
                    richTextBoxInformationMain.AppendText("An error has been encountered! The reported error is: " + ex);
                }

            }
        }

        private void FormManagePattern_SizeChanged(object sender, EventArgs e)
        {
            GridAutoLayoutLoadPatternDefinition();
        }

        private void FormManagePattern_Load(object sender, EventArgs e)
        {
            GridAutoLayoutLoadPatternDefinition();
        }

        private void richTextBoxInformationMain_TextChanged(object sender, EventArgs e)
        {
            // Set the current caret position to the end
            richTextBoxInformationMain.SelectionStart = richTextBoxInformationMain.Text.Length;
            // Scroll automatically
            richTextBoxInformationMain.ScrollToCaret();
        }
    }
}
