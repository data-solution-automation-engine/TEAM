using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Threading;
using System.Drawing;
using System.Data;
using System.Globalization;

namespace TEAM
{
    public partial class FormMain : FormBase
    {
        public FormMain()
        {
            var errorMessage = new StringBuilder();

            errorMessage.AppendLine("Error were detected:");
            errorMessage.AppendLine();
           
            var errorDetails = new StringBuilder();
            errorDetails.AppendLine();

            var errorCounter = 0;

            InitializeComponent();
            InitializePath();

            InitialiseConnections(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigfileName);

            //Startup information
            richTextBoxInformation.Text = "Application initialised - the Taxonomy of ETL Automation Metadata (TEAM). \r\n";
            richTextBoxInformation.AppendText("Version 1.5 \r\n\r\n");

            richTextBoxInformation.AppendText("Source code on Github: https://github.com/RoelantVos/TEAM \r\n\r\n");

            richTextBoxInformation.AppendText("Changes for this version: \r\n");
            richTextBoxInformation.AppendText("- Enabled JSON storage format for core mapping data sets (MD_TABLE_MAPPING and MD_ATTRIBUTE_MAPPING)\r\n");
            richTextBoxInformation.AppendText("- This means that, at this stage, the new JSON repository type only applies to the above tables and still needs a SQL Server repository for the other tabels!\r\n");
            richTextBoxInformation.AppendText("- Created new configuration screen\r\n");
            richTextBoxInformation.AppendText("- Minor bug fixes (see Github)\r\n\r\n");

            //Retrieving the configuration strings from the global variables (configuration settings)
            var configurationSettings = new ConfigurationSettings();

            var connOmd = new SqlConnection { ConnectionString = configurationSettings.ConnectionStringOmd };
            var connStg = new SqlConnection { ConnectionString = configurationSettings.ConnectionStringStg };
            var connPsa = new SqlConnection { ConnectionString = configurationSettings.ConnectionStringHstg };

            try
            {
                connOmd.Open();

                DisplayMaxVersion(connOmd);
                DisplayCurrentVersion(connOmd);
                DisplayRepositoryVersion(connOmd);
            }
            catch
            {
                richTextBoxInformation.AppendText("There was an issue establishing a database connection to the Metadata Repository Database. Can you verify the connection information in the 'settings' tab? \r\n");
            }

            try
            {
                connStg.Open();
            }
            catch
            {
                richTextBoxInformation.AppendText("There was an issue establishing a database connection to the Staging Area Database. Can you verify the connection information in the 'settings' tab? \r\n");
            }

            try
            {
                connPsa.Open();
            }
            catch
            {
                richTextBoxInformation.AppendText("There was an issue establishing a database connection to the Persistent Staging Area (PSA) Database. Can you verify the connection information in the 'settings' tab? \r\n");
            }

            if (errorCounter > 0)
            {
                richTextBoxInformation.AppendText(errorMessage.ToString());
            }



        }

        internal void DisplayMaxVersion(SqlConnection connOmd)
        {
            var selectedVersion = GetMaxVersionId(connOmd);

            var versionMajorMinor = GetVersion(selectedVersion, connOmd);
            var majorVersion = versionMajorMinor.Key;
            var minorVersion = versionMajorMinor.Value;

            labelVersion.Text = majorVersion + "." + minorVersion;
        }

        internal void DisplayCurrentVersion(SqlConnection connOmd)
        {
            var sqlStatementForCurrentVersion = new StringBuilder();
            sqlStatementForCurrentVersion.AppendLine("SELECT [VERSION_NAME] FROM [MD_MODEL_METADATA]");

            var versionList = GetDataTable(ref connOmd, sqlStatementForCurrentVersion.ToString());

            foreach (DataRow versionNameRow in versionList.Rows)
            {
                var versionName = (string) versionNameRow["VERSION_NAME"];
                labelActiveVersion.Text = versionName;
            }

            var configurationSettings = new ConfigurationSettings();
            labelMetadataRepository.Text = "Repository type in configuration is set to " + configurationSettings.metadataRepositoryType;
        }


        internal void DisplayRepositoryVersion(SqlConnection connOmd)
        {
            var sqlStatementForCurrentVersion = new StringBuilder();
            sqlStatementForCurrentVersion.AppendLine("SELECT [REPOSITORY_VERSION],[REPOSITORY_UPDATE_DATETIME] FROM [MD_REPOSITORY_VERSION]");

            var versionList = GetDataTable(ref connOmd, sqlStatementForCurrentVersion.ToString());

            foreach (DataRow versionNameRow in versionList.Rows)
            {
                var versionName = (string)versionNameRow["REPOSITORY_VERSION"];
                var versionDate = (DateTime)versionNameRow["REPOSITORY_UPDATE_DATETIME"];
                labelRepositoryVersion.Text = versionName;
                labelRepositoryDate.Text = versionDate.ToString(CultureInfo.InvariantCulture);
            }





        }

        private static void InitializePath()
        {
            var configurationPath = Application.StartupPath + @"\Configuration\";
            var outputPath = Application.StartupPath + @"\Output\";

            try
            {
                if (!Directory.Exists(configurationPath))
                {
                    Directory.CreateDirectory(configurationPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creation default directory at " + configurationPath +" the message is "+ex, "An issue has been encountered", MessageBoxButtons.OK,MessageBoxIcon.Error);
            }

            try
            {
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creation default directory at " + outputPath + " the message is " + ex, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                if (!File.Exists(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigfileName))
                {
                    var initialConfigurationFile = new StringBuilder();

                    initialConfigurationFile.AppendLine("/* Virtual EDW Configuration Settings */");
                    initialConfigurationFile.AppendLine("/* Roelant Vos - 2016 */");
                    initialConfigurationFile.AppendLine("SourceDatabase|Source_Database");
                    initialConfigurationFile.AppendLine("StagingDatabase|Staging_Area_Database");
                    initialConfigurationFile.AppendLine("PersistentStagingDatabase|Persistent_Staging_Area_Database");
                    initialConfigurationFile.AppendLine("IntegrationDatabase|Data_Vault_Database");
                    initialConfigurationFile.AppendLine("PresentationDatabase|Presentation_Database");
                    initialConfigurationFile.AppendLine(@"connectionStringSource|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Source_Database>;user id=sa; password=<>");
                    initialConfigurationFile.AppendLine(@"connectionStringStaging|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Staging_Area>;user id=sa; password=<>");
                    initialConfigurationFile.AppendLine(@"connectionStringPersistentStaging|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Persistent_Staging_Area>;user id=sa; password=<>");
                    initialConfigurationFile.AppendLine(@"connectionStringMetadata|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Metadata>;user id=sa; password=<>");
                    initialConfigurationFile.AppendLine(@"connectionStringIntegration|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Data_Vault>;user id=sa; password=<>");
                    initialConfigurationFile.AppendLine(@"connectionStringPresentation|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Presentation>;user id=sa; password=<>");
                    initialConfigurationFile.AppendLine("SourceSystemPrefix|PROFILER");
                    initialConfigurationFile.AppendLine("StagingAreaPrefix|STG");
                    initialConfigurationFile.AppendLine("PersistentStagingAreaPrefix|PSA");
                    initialConfigurationFile.AppendLine("HubTablePrefix|HUB");
                    initialConfigurationFile.AppendLine("SatTablePrefix|SAT");
                    initialConfigurationFile.AppendLine("LinkTablePrefix|LNK");
                    initialConfigurationFile.AppendLine("LinkSatTablePrefix|LSAT");
                    initialConfigurationFile.AppendLine("KeyIdentifier|HSH");
                    initialConfigurationFile.AppendLine("SchemaName|dbo");
                    initialConfigurationFile.AppendLine("RowID|SOURCE_ROW_ID");
                    initialConfigurationFile.AppendLine("EventDateTimeStamp|EVENT_DATETIME");
                    initialConfigurationFile.AppendLine("LoadDateTimeStamp|LOAD_DATETIME");
                    initialConfigurationFile.AppendLine("ExpiryDateTimeStamp|LOAD_END_DATETIME");
                    initialConfigurationFile.AppendLine("ChangeDataIndicator|CDC_OPERATION");
                    initialConfigurationFile.AppendLine("RecordSourceAttribute|RECORD_SOURCE");
                    initialConfigurationFile.AppendLine("ETLProcessID|ETL_INSERT_RUN_ID");
                    initialConfigurationFile.AppendLine("ETLUpdateProcessID|ETL_UPDATE_RUN_ID");
                    initialConfigurationFile.AppendLine("TableNamingLocation|Prefix");
                    initialConfigurationFile.AppendLine("KeyNamingLocation|Suffix");
                    initialConfigurationFile.AppendLine("RecordChecksum|HASH_FULL_RECORD");
                    initialConfigurationFile.AppendLine("CurrentRecordAttribute|CURRENT_RECORD_INDICATOR");
                    initialConfigurationFile.AppendLine("AlternativeRecordSource|N/A");
                    initialConfigurationFile.AppendLine("AlternativeHubLDTS|N/A");
                    initialConfigurationFile.AppendLine("AlternativeSatelliteLDTS|N/A");
                    initialConfigurationFile.AppendLine("AlternativeRecordSourceFunction|False");
                    initialConfigurationFile.AppendLine("AlternativeHubLDTSFunction|False");
                    initialConfigurationFile.AppendLine("AlternativeSatelliteLDTSFunction|False");
                    initialConfigurationFile.AppendLine("LogicalDeleteAttribute|DELETED_RECORD_INDICATOR");
                    initialConfigurationFile.AppendLine("PSAKeyLocation|PrimaryKey"); //Can be PrimaryKey or UniqueIndex
                    initialConfigurationFile.AppendLine("LinkedServerName|"); //Can be PrimaryKey or UniqueIndex
                    
                    initialConfigurationFile.AppendLine("/* End of file */");

                    using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigfileName))
                    {
                        outfile.Write(initialConfigurationFile.ToString());
                        outfile.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while creation the default Configuration File. The error message is " + ex, "An issue has been encountered", MessageBoxButtons.OK,MessageBoxIcon.Error);
            }

        }

        //private void InitialiseConnections(string chosenFile)
        //{
        //    var configList = new Dictionary<string, string>();
        //    var fs = new FileStream(chosenFile, FileMode.Open, FileAccess.Read);
        //    var sr = new StreamReader(fs);

        //    try
        //    {
        //        string textline;
        //        while ((textline = sr.ReadLine()) != null)
        //        {
        //            if (textline.IndexOf(@"/*", StringComparison.Ordinal) == -1)
        //            {
        //                var line = textline.Split('|');
        //                configList.Add(line[0], line[1]);
        //            }
        //        }

        //        sr.Close();
        //        fs.Close();

        //        var connectionStringOmd = configList["connectionStringMetadata"];
        //        connectionStringOmd = connectionStringOmd.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

        //        var connectionStringSource = configList["connectionStringSource"];
        //        connectionStringSource = connectionStringSource.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

        //        var connectionStringStg = configList["connectionStringStaging"];
        //        connectionStringStg = connectionStringStg.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

        //        var connectionStringHstg = configList["connectionStringPersistentStaging"];
        //        connectionStringHstg = connectionStringHstg.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

        //        var connectionStringInt = configList["connectionStringIntegration"];
        //        connectionStringInt = connectionStringInt.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

        //        var connectionStringPres = configList["connectionStringPresentation"];
        //        connectionStringPres = connectionStringPres.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

        //        textBoxOutputPath.Text = GlobalVariables.OutputPath;
        //        textBoxIntegrationConnection.Text = connectionStringInt;
        //        textBoxPSAConnection.Text = connectionStringHstg;
        //        textBoxSourceConnection.Text = connectionStringSource;
        //        textBoxStagingConnection.Text = connectionStringStg;
        //       // textBoxMetadataConnection.Text = connectionStringOmd;
        //        textBoxPresentationConnection.Text = connectionStringPres;

        //        textBoxHubTablePrefix.Text = configList["HubTablePrefix"];
        //        textBoxSatPrefix.Text = configList["SatTablePrefix"];
        //        textBoxLinkTablePrefix.Text = configList["LinkTablePrefix"];
        //        textBoxLinkSatPrefix.Text = configList["LinkSatTablePrefix"];
        //        textBoxDWHKeyIdentifier.Text = configList["KeyIdentifier"];
        //        textBoxSchemaName.Text = configList["SchemaName"];
        //        textBoxEventDateTime.Text = configList["EventDateTimeStamp"];
        //        textBoxLDST.Text = configList["LoadDateTimeStamp"];
        //        textBoxExpiryDateTimeName.Text = configList["ExpiryDateTimeStamp"];
        //        textBoxChangeDataCaptureIndicator.Text = configList["ChangeDataIndicator"];
        //        textBoxRecordSource.Text = configList["RecordSourceAttribute"];
        //        textBoxETLProcessID.Text = configList["ETLProcessID"];
        //        textBoxETLUpdateProcessID.Text = configList["ETLUpdateProcessID"];
        //        textBoxSourcePrefix.Text = configList["SourceSystemPrefix"];
        //        textBoxStagingAreaPrefix.Text = configList["StagingAreaPrefix"];
        //        textBoxPSAPrefix.Text = configList["PersistentStagingAreaPrefix"];
        //        textBoxSourceRowId.Text = configList["RowID"];
        //        textBoxSourceDatabase.Text = configList["SourceDatabase"];
        //        textBoxStagingDatabase.Text = configList["StagingDatabase"];
        //        textBoxPSADatabase.Text = configList["PersistentStagingDatabase"];
        //        textBoxIntegrationDatabase.Text = configList["IntegrationDatabase"];
        //        textBoxPresentationDatabase.Text = configList["PresentationDatabase"];
        //        textBoxRecordChecksum.Text = configList["RecordChecksum"];
        //        textBoxCurrentRecordAttributeName.Text = configList["CurrentRecordAttribute"];
        //        textBoxAlternativeRecordSource.Text = configList["AlternativeRecordSource"];
        //        textBoxHubAlternativeLDTSAttribute.Text = configList["AlternativeHubLDTS"];
        //        textBoxSatelliteAlternativeLDTSAttribute.Text = configList["AlternativeSatelliteLDTS"];
        //        textBoxLogicalDeleteAttributeName.Text = configList["LogicalDeleteAttribute"];
        //        textBoxLinkedServer.Text = configList["LinkedServerName"];

        //        //Checkbox setting based on loaded configuration
        //        CheckBox myConfigurationCheckBox;

        //        if (configList["AlternativeRecordSourceFunction"] == "False")
        //        {
        //            myConfigurationCheckBox = checkBoxAlternativeRecordSource;
        //            myConfigurationCheckBox.Checked = false;
        //            textBoxAlternativeRecordSource.Enabled = false;
        //        }
        //        else
        //        {
        //            myConfigurationCheckBox = checkBoxAlternativeRecordSource;
        //            myConfigurationCheckBox.Checked = true; 
        //        }

        //        if (configList["AlternativeHubLDTSFunction"] == "False")
        //        {
        //            myConfigurationCheckBox = checkBoxAlternativeHubLDTS;
        //            myConfigurationCheckBox.Checked = false;
        //            textBoxHubAlternativeLDTSAttribute.Enabled = false;
        //        }
        //        else
        //        {
        //            myConfigurationCheckBox = checkBoxAlternativeHubLDTS;
        //            myConfigurationCheckBox.Checked = true;
        //        }

        //        if (configList["AlternativeSatelliteLDTSFunction"] == "False")
        //        {
        //            myConfigurationCheckBox = checkBoxAlternativeSatLDTS;
        //            myConfigurationCheckBox.Checked = false;
        //            textBoxSatelliteAlternativeLDTSAttribute.Enabled = false;
        //        }
        //        else
        //        {
        //            myConfigurationCheckBox = checkBoxAlternativeSatLDTS;
        //            myConfigurationCheckBox.Checked = true;
        //        }
                

        //        //Radiobutton setting for prefix / suffix 
        //        RadioButton myTableRadioButton;

        //        if (configList["TableNamingLocation"] == "Prefix")
        //        {
        //            myTableRadioButton = tablePrefixRadiobutton;
        //            myTableRadioButton.Checked = true;
        //        }
        //        else
        //        {
        //            myTableRadioButton = tableSuffixRadiobutton;
        //            myTableRadioButton.Checked = true;
        //        }

        //        //Radiobutton settings for on key location
        //        RadioButton myKeyRadioButton;

        //        if (configList["KeyNamingLocation"] == "Prefix")
        //        {
        //            myKeyRadioButton = keyPrefixRadiobutton;
        //            myKeyRadioButton.Checked = true;
        //        }
        //        else
        //        {
        //            myKeyRadioButton = keySuffixRadiobutton;
        //            myKeyRadioButton.Checked = true;
        //        }

        //        //Radiobutton settings for PSA Natural Key determination
        //        RadioButton myPsaBusinessKeyLocation;

        //        if (configList["PSAKeyLocation"] == "PrimaryKey")
        //        {
        //            myPsaBusinessKeyLocation = radioButtonPSABusinessKeyPK;
        //            myPsaBusinessKeyLocation.Checked = true;
        //        }
        //        else
        //        {
        //            myPsaBusinessKeyLocation = radioButtonPSABusinessKeyIndex;
        //            myPsaBusinessKeyLocation.Checked = true;
        //        } 

        //        richTextBoxInformation.AppendText("The default values were loaded. \r\n\r\n");
        //        richTextBoxInformation.AppendText(@"The file " + chosenFile + " was uploaded successfully. \r\n\r\n");
        //    }
        //    catch (Exception ex)
        //    {
        //        richTextBoxInformation.AppendText("\r\n\r\nAn error occured while interpreting the configuration file. The original error is: '" + ex.Message + "'");
        //    }

        //}

        private void CheckKeyword(string word, Color color, int startIndex)
        {
            if (richTextBoxInformation.Text.Contains(word))
            {
                int index = -1;
                int selectStart = richTextBoxInformation.SelectionStart;

                while ((index = richTextBoxInformation.Text.IndexOf(word, (index + 1), StringComparison.Ordinal)) != -1)
                {
                    richTextBoxInformation.Select((index + startIndex), word.Length);
                    richTextBoxInformation.SelectionColor = color;
                    richTextBoxInformation.Select(selectStart, 0);
                    richTextBoxInformation.SelectionColor = Color.Black;
                }
            }
        }

        //  Executing a SQL object against the databasa (SQL Server SMO API)
        public void GenerateInDatabase(SqlConnection sqlConnection, string viewStatement)
        {
            using (var connection = sqlConnection)
            {
                var server = new Server(new ServerConnection(connection));
                try
                {
                    server.ConnectionContext.ExecuteNonQuery(viewStatement);
                    SetTextDebug("The statement was executed succesfully.\r\n");
                }
                catch (Exception exception)
                {
                    SetTextDebug("Issues occurred executing the SQL statement.\r\n");
                    SetTextDebug(@"SQL error: " + exception.Message + "\r\n\r\n");
                 // SetTextDebug(@"The executed query was: " + viewStatement);
                }
            }               
        }


        private void openOutputDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var configurationSettings = new ConfigurationSettings();

            try
            {
                Process.Start(configurationSettings.OutputPath);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occured while attempting to open the output directory. The error message is: "+ex;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CloseTestDataForm(object sender, FormClosedEventArgs e)
        {
            _myTestDataForm = null;
        } 
        private void CloseMetadataForm(object sender, FormClosedEventArgs e)
        {
            _myModelMetadataForm = null;
        }

        private void CloseRepositoryForm(object sender, FormClosedEventArgs e)
        {
            _myRepositoryForm = null;
        }

        private void CloseAboutForm(object sender, FormClosedEventArgs e)
        {
            _myAboutForm = null;
        }

        private void CloseGraphForm(object sender, FormClosedEventArgs e)
        {
            _myGraphForm = null;
        }

        private void CloseConfigurationForm(object sender, FormClosedEventArgs e)
        {
            _myConfigurationForm = null;
        }

        private void openMetadataFormToolStripMenuItem_Click(object sender, EventArgs e)
        {         
            var t = new Thread(ThreadProcMetadata);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }


        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcAbout);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This feature is yet to be implemented.", "Upcoming!", MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }

        private void linksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This feature is yet to be implemented.", "Upcoming!", MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }


        private FormTestData _myTestDataForm;
        public void ThreadProcTestData()
        {
            if (_myTestDataForm == null)
            {
                _myTestDataForm = new FormTestData(this);
                _myTestDataForm.Show();

                Application.Run();
            }

            else
            {
                if (_myTestDataForm.InvokeRequired)
                {
                    // Thread Error
                    _myTestDataForm.Invoke((MethodInvoker)delegate { _myTestDataForm.Close(); });
                    _myTestDataForm.FormClosed += CloseTestDataForm;

                    _myTestDataForm = new FormTestData(this);
                    _myTestDataForm.Show();
                    Application.Run();
                }
                else
                {
                    // No invoke required - same thread
                    _myTestDataForm.FormClosed += CloseTestDataForm;

                    _myTestDataForm = new FormTestData(this);
                    _myTestDataForm.Show();
                    Application.Run();
                }

            }
        }

        private FormModelMetadata _myModelMetadataForm;
        public void ThreadProcModelMetadata()
        {
            if (_myModelMetadataForm == null)
            {
                _myModelMetadataForm = new FormModelMetadata(this);
                _myModelMetadataForm.Show();

                Application.Run();
            }

            else
            {
                if (_myModelMetadataForm.InvokeRequired)
                {
                    // Thread Error
                    _myModelMetadataForm.Invoke((MethodInvoker)delegate { _myModelMetadataForm.Close(); });
                    _myModelMetadataForm.FormClosed += CloseMetadataForm;

                    _myModelMetadataForm = new FormModelMetadata(this);
                    _myModelMetadataForm.Show();
                    Application.Run();
                }
                else
                {
                    // No invoke required - same thread
                    _myModelMetadataForm.FormClosed += CloseMetadataForm;

                    _myModelMetadataForm = new FormModelMetadata(this);
                    _myModelMetadataForm.Show();
                    Application.Run();
                }

            }
        }


        private FormManageGraph _myGraphForm;
        [STAThread]
        public void ThreadProcGraph()
        {
            if (_myGraphForm == null)
            {
                _myGraphForm = new FormManageGraph(this);
                Application.Run(_myGraphForm);
            }
            else
            {
                if (_myGraphForm.InvokeRequired)
                {
                    // Thread Error
                    _myGraphForm.Invoke((MethodInvoker)delegate { _myGraphForm.Close(); });
                    _myGraphForm.FormClosed += CloseGraphForm;

                    _myGraphForm = new FormManageGraph(this);
                    Application.Run(_myGraphForm);
                }
                else
                {
                    // No invoke required - same thread
                    _myGraphForm.FormClosed += CloseGraphForm;
                    _myGraphForm = new FormManageGraph(this);

                    Application.Run(_myGraphForm);
                }
            }
        }


        private FormManageConfiguration _myConfigurationForm;
        /// <summary>
        /// 
        /// </summary>
        [STAThread]
        public void ThreadProcConfiguration()
        {
            if (_myConfigurationForm == null)
            {
                _myConfigurationForm = new FormManageConfiguration(this);
                Application.Run(_myConfigurationForm);
            }
            else
            {
                if (_myConfigurationForm.InvokeRequired)
                {
                    // Thread Error
                    _myConfigurationForm.Invoke((MethodInvoker)delegate { _myConfigurationForm.Close(); });
                    _myConfigurationForm.FormClosed += CloseConfigurationForm;

                    _myConfigurationForm = new FormManageConfiguration(this);
                    Application.Run(_myConfigurationForm);
                }
                else
                {
                    // No invoke required - same thread
                    _myConfigurationForm.FormClosed += CloseConfigurationForm;
                    _myConfigurationForm = new FormManageConfiguration(this);

                    Application.Run(_myConfigurationForm);
                }
            }
        }


        private FormManageMetadata _myMetadataForm;
        [STAThread]
        public void ThreadProcMetadata()
        {
            if (_myMetadataForm == null)
            {
                _myMetadataForm = new FormManageMetadata(this);
                Application.Run(_myMetadataForm);
            }
            else
            {
                if (_myMetadataForm.InvokeRequired)
                {
                    // Thread Error
                    _myMetadataForm.Invoke((MethodInvoker)delegate { _myMetadataForm.Close(); });
                    _myMetadataForm.FormClosed += CloseMetadataForm;

                    _myMetadataForm = new FormManageMetadata(this);
                    Application.Run(_myMetadataForm);
                }
                else
                {
                    // No invoke required - same thread
                    _myMetadataForm.FormClosed += CloseMetadataForm;
                    _myMetadataForm = new FormManageMetadata(this);

                    Application.Run(_myMetadataForm);
                }
            }
        }

        private FormManageRepository _myRepositoryForm;
        [STAThread]
        public void ThreadProcRepository()
        {
            if (_myRepositoryForm == null)
            {
                _myRepositoryForm = new FormManageRepository(this);
                _myRepositoryForm.Show();

                Application.Run();
            }
            else
            {
                if (_myRepositoryForm.InvokeRequired)
                {
                    // Thread Error
                    _myRepositoryForm.Invoke((MethodInvoker)delegate { _myRepositoryForm.Close(); });
                    _myRepositoryForm.FormClosed += CloseMetadataForm;

                    _myRepositoryForm = new FormManageRepository(this);
                    _myRepositoryForm.Show();
                    Application.Run();
                }
                else
                {
                    // No invoke required - same thread
                    _myRepositoryForm.FormClosed += CloseRepositoryForm;

                    _myRepositoryForm = new FormManageRepository(this);
                    _myRepositoryForm.Show();
                    Application.Run();
                }
            }
        }

        private FormAbout _myAboutForm;
        public void ThreadProcAbout()
        {
            if (_myAboutForm == null)
            {
                _myAboutForm = new FormAbout(this);
                _myAboutForm.Show();

                Application.Run();
            }

            else
            {
                if (_myAboutForm.InvokeRequired)
                {
                    // Thread Error
                    _myAboutForm.Invoke((MethodInvoker)delegate { _myAboutForm.Close(); });
                    _myAboutForm.FormClosed += CloseAboutForm;

                    _myAboutForm = new FormAbout(this);
                    _myAboutForm.Show();
                    Application.Run();
                }
                else
                {
                    // No invoke required - same thread
                    _myAboutForm.FormClosed += CloseAboutForm;

                    _myAboutForm = new FormAbout(this);
                    _myAboutForm.Show();
                    Application.Run();
                }
            }
        }

        private void generateTestDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcTestData);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void manageModelMetadataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcModelMetadata);
            t.SetApartmentState(ApartmentState.STA);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        // Multithreading for updating the user (debugging form)
        delegate void SetTextCallBackDebug(string text);
        private void SetTextDebug(string text)
        {
            if (richTextBoxInformation.InvokeRequired)
            {
                var d = new SetTextCallBackDebug(SetTextDebug);
                Invoke(d, text);
            }
            else
            {
                richTextBoxInformation.AppendText(text);
            }
        }


        private void richTextBoxInformation_TextChanged(object sender, EventArgs e)
        {
            CheckKeyword("Issues occurred", Color.Red, 0);
            CheckKeyword("The statement was executed succesfully.", Color.GreenYellow, 0);
            // this.CheckKeyword("if", Color.Green, 0);
        }



        private void createRebuildRepositoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcRepository);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void sourceSystemRegistryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This feature is yet to be implemented.", "Upcoming!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void maintainMetadataGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcGraph);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void generalSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = new Thread(ThreadProcConfiguration);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void richTextBoxInformation_Enter(object sender, EventArgs e)
        {
            ActiveControl = null;
        }
    }
}
