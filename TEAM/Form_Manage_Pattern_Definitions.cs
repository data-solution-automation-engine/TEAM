using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TEAM_Library;

namespace TEAM
{
    public partial class FormManagePattern: FormBase
    {
        //private FormMain parentFormMain;

        private BindingSource _bindingSourceLoadPatternDefinition = new BindingSource();

        // Default constructor
        public FormManagePattern()
        {
            InitializeComponent();
        }

        // TEAM form constructor
        public FormManagePattern(FormMain parent) : base(parent)
        {
            //parentFormMain = parent;
            InitializeComponent();

            //var patternDefinition = new LoadPatternDefinition.LoadPatternDefinitionFileHandling();

            var filePath = GlobalParameters.LoadPatternPath + GlobalParameters.LoadPatternDefinitionFile;

            UpdatePatternDefinitions(filePath);

            dataGridViewLoadPatternDefinition.Focus();
        }

        private void UpdatePatternDefinitions(string filePath)
        {
            filePath = Path.GetFullPath(filePath);

            GlobalParameters.PatternDefinitionList = LoadPatternDefinition.DeserializeLoadPatternDefinition(filePath);

            // Load Pattern definition in memory
            if ((GlobalParameters.PatternDefinitionList != null) && (!GlobalParameters.PatternDefinitionList.Any()))
            {
                richTextBoxInformationMain.Text = "There are no pattern definitions / types found in the designated load pattern directory. Please verify if the file " + filePath + " exists.";
            }

            if (GlobalParameters.PatternDefinitionList != null)
            {
                PopulateLoadPatternDefinitionDataGrid();
                textBoxLoadPatternPath.Text = filePath;
            }
            else
            {
                richTextBoxInformationMain.Text = "The pattern definition file could not be loaded. Please verify if the file " + filePath + " exists.";
            }
        }

        internal static List<LoadPatternDefinition> patternDefinitionList { get; set; }


        public void PopulateLoadPatternDefinitionDataGrid()
        {
            // Create a datatable 
            DataTable dt = GlobalParameters.PatternDefinitionList.ToDataTable();

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

        private void toolStripSave_Click(object sender, EventArgs e)
        {
            try
            {
                richTextBoxInformationMain.Clear();

                var chosenFile = textBoxLoadPatternPath.Text;

                // Prepare the data grid and make sure the output is sorted
                DataTable gridDataTable = (DataTable)_bindingSourceLoadPatternDefinition.DataSource;

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

                // Create a backup file, if enabled, and inform the user.
                if (checkBoxBackupFiles.Checked)
                {
                    richTextBoxInformationMain.AppendText(LoadPatternDefinition.BackupLoadPatternDefinition(chosenFile)+"\r\n");
                }

                // Save the file, and inform the user.
                richTextBoxInformationMain.AppendText(LoadPatternDefinition.SaveLoadPatternDefinition(chosenFile, json));

                try
                {
                   GlobalParameters.PatternDefinitionList = JsonConvert.DeserializeObject<List<LoadPatternDefinition>>(File.ReadAllText(chosenFile));
        
                }
                catch (Exception ex)
                {
                    richTextBoxInformationMain.AppendText("An issue was encountered committing the information to memory. The reported error is " + ex);
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

        private void OpenFileClick(object sender, EventArgs e)
        {
            var fileBrowserDialog = new OpenFileDialog();
            fileBrowserDialog.Title = "Open Pattern Definition File";
            fileBrowserDialog.InitialDirectory = Path.GetDirectoryName(textBoxLoadPatternPath.Text);

            DialogResult result = fileBrowserDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fileBrowserDialog.InitialDirectory))
            {
                string[] files = Directory.GetFiles(fileBrowserDialog.InitialDirectory);

                int fileCounter = 0;
                foreach (string file in files)
                {
                    if (file.Contains("loadPatternDefinition"))
                    {
                        fileCounter++;
                    }
                }

                string finalPath;
                if (fileBrowserDialog.InitialDirectory.EndsWith(@"\"))
                {
                    finalPath = fileBrowserDialog.FileName.Replace(@"\", "");
                }
                else
                {
                    finalPath = fileBrowserDialog.FileName;
                }

                textBoxLoadPatternPath.Text = finalPath;

                if (fileCounter == 0)
                {
                    richTextBoxInformationMain.Text = "The selected directory does not seem to contain a loadPatternCollection.json file. Did you select a correct Load Pattern directory?";
                }
                else
                {
                    UpdatePatternDefinitions(finalPath);
                    richTextBoxInformationMain.Text = "New pattern definitions have been loaded in memory.";
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
                InitialDirectory = GlobalParameters.LoadPatternPath
            };

            var ret = STAShowDialog(theDialog);

            if (ret == DialogResult.OK)
            {
                try
                {
                    var chosenFile = theDialog.FileName;

                    //string filePath = Path.GetFullPath(theDialog.FileName);

                    // Save the list to memory
                    GlobalParameters.PatternDefinitionList = JsonConvert.DeserializeObject<List<LoadPatternDefinition>>(File.ReadAllText(chosenFile));

                    // ... and populate the data grid
                    PopulateLoadPatternDefinitionDataGrid();

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
