using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Drawing;

namespace TEAM
{
    public partial class FormManageConfiguration : FormBase
    {
        public FormManageConfiguration()
        {
            InitializeComponent();
        }

        public FormManageConfiguration(FormMain parent) : base(parent)
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine("Error were detected:");
            errorMessage.AppendLine();

            var errorDetails = new StringBuilder();
            errorDetails.AppendLine();

            var errorCounter = 0;



            InitializeComponent();

            //Make sure the root directories exist, based on hard-coded (tool) parameters
            //Also create the initial file with the configuration if it doesn't exist already
            InitialisePath();

            //Set the root path, to be able to locate the configuration file and load it
            InitialiseRootPath();

            var configurationSettings = new ConfigurationSettings();

            try
            {
                LocalInitialiseConnections(configurationSettings.ConfigurationPath + GlobalParameters.ConfigfileName);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText("Errors occured trying to load the configuration file, the message is " + ex + ". No default values were loaded. \r\n\r\n");
            }

            var connOmd = new SqlConnection { ConnectionString = textBoxMetadataConnection.Text };
            var connStg = new SqlConnection { ConnectionString = textBoxStagingConnection.Text };
            var connPsa = new SqlConnection { ConnectionString = textBoxPSAConnection.Text };

            try
            {
                connOmd.Open();
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


        private static void InitialisePath()
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
                    initialConfigurationFile.AppendLine("/* Roelant Vos - 2018 */");
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

        private void LocalInitialiseConnections(string chosenFile)
        {
            var configList = new Dictionary<string, string>();
            var fs = new FileStream(chosenFile, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs);

            try
            {
                string textline;
                while ((textline = sr.ReadLine()) != null)
                {
                    if (textline.IndexOf(@"/*", StringComparison.Ordinal) == -1)
                    {
                        var line = textline.Split('|');
                        configList.Add(line[0], line[1]);
                    }
                }

                sr.Close();
                fs.Close();

                var connectionStringOmd = configList["connectionStringMetadata"];
                connectionStringOmd = connectionStringOmd.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringSource = configList["connectionStringSource"];
                connectionStringSource = connectionStringSource.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringStg = configList["connectionStringStaging"];
                connectionStringStg = connectionStringStg.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringHstg = configList["connectionStringPersistentStaging"];
                connectionStringHstg = connectionStringHstg.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringInt = configList["connectionStringIntegration"];
                connectionStringInt = connectionStringInt.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringPres = configList["connectionStringPresentation"];
                connectionStringPres = connectionStringPres.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                //Paths
                //textBoxOutputPath.Text = GlobalParameters.OutputPath;
                textBoxOutputPath.Text = configList["OutputPath"];
                textBoxConfigurationPath.Text = configList["ConfigurationPath"];

                //Connections
                textBoxIntegrationConnection.Text = connectionStringInt;
                textBoxPSAConnection.Text = connectionStringHstg;
                textBoxSourceConnection.Text = connectionStringSource;
                textBoxStagingConnection.Text = connectionStringStg;
                textBoxMetadataConnection.Text = connectionStringOmd;
                textBoxPresentationConnection.Text = connectionStringPres;

                //DWH settings
                textBoxHubTablePrefix.Text = configList["HubTablePrefix"];
                textBoxSatPrefix.Text = configList["SatTablePrefix"];
                textBoxLinkTablePrefix.Text = configList["LinkTablePrefix"];
                textBoxLinkSatPrefix.Text = configList["LinkSatTablePrefix"];
                textBoxDWHKeyIdentifier.Text = configList["KeyIdentifier"];
                textBoxSchemaName.Text = configList["SchemaName"];
                textBoxEventDateTime.Text = configList["EventDateTimeStamp"];
                textBoxLDST.Text = configList["LoadDateTimeStamp"];
                textBoxExpiryDateTimeName.Text = configList["ExpiryDateTimeStamp"];
                textBoxChangeDataCaptureIndicator.Text = configList["ChangeDataIndicator"];
                textBoxRecordSource.Text = configList["RecordSourceAttribute"];
                textBoxETLProcessID.Text = configList["ETLProcessID"];
                textBoxETLUpdateProcessID.Text = configList["ETLUpdateProcessID"];
                textBoxSourcePrefix.Text = configList["SourceSystemPrefix"];
                textBoxStagingAreaPrefix.Text = configList["StagingAreaPrefix"];
                textBoxPSAPrefix.Text = configList["PersistentStagingAreaPrefix"];
                textBoxSourceRowId.Text = configList["RowID"];
                textBoxSourceDatabase.Text = configList["SourceDatabase"];
                textBoxStagingDatabase.Text = configList["StagingDatabase"];
                textBoxPSADatabase.Text = configList["PersistentStagingDatabase"];
                textBoxIntegrationDatabase.Text = configList["IntegrationDatabase"];
                textBoxPresentationDatabase.Text = configList["PresentationDatabase"];
                textBoxRecordChecksum.Text = configList["RecordChecksum"];
                textBoxCurrentRecordAttributeName.Text = configList["CurrentRecordAttribute"];
                textBoxAlternativeRecordSource.Text = configList["AlternativeRecordSource"];
                textBoxHubAlternativeLDTSAttribute.Text = configList["AlternativeHubLDTS"];
                textBoxSatelliteAlternativeLDTSAttribute.Text = configList["AlternativeSatelliteLDTS"];
                textBoxLogicalDeleteAttributeName.Text = configList["LogicalDeleteAttribute"];
                textBoxLinkedServer.Text = configList["LinkedServerName"];

                //Checkbox setting based on loaded configuration
                CheckBox myConfigurationCheckBox;

                if (configList["AlternativeRecordSourceFunction"] == "False")
                {
                    myConfigurationCheckBox = checkBoxAlternativeRecordSource;
                    myConfigurationCheckBox.Checked = false;
                    textBoxAlternativeRecordSource.Enabled = false;
                }
                else
                {
                    myConfigurationCheckBox = checkBoxAlternativeRecordSource;
                    myConfigurationCheckBox.Checked = true; 
                }

                if (configList["AlternativeHubLDTSFunction"] == "False")
                {
                    myConfigurationCheckBox = checkBoxAlternativeHubLDTS;
                    myConfigurationCheckBox.Checked = false;
                    textBoxHubAlternativeLDTSAttribute.Enabled = false;
                }
                else
                {
                    myConfigurationCheckBox = checkBoxAlternativeHubLDTS;
                    myConfigurationCheckBox.Checked = true;
                }

                if (configList["AlternativeSatelliteLDTSFunction"] == "False")
                {
                    myConfigurationCheckBox = checkBoxAlternativeSatLDTS;
                    myConfigurationCheckBox.Checked = false;
                    textBoxSatelliteAlternativeLDTSAttribute.Enabled = false;
                }
                else
                {
                    myConfigurationCheckBox = checkBoxAlternativeSatLDTS;
                    myConfigurationCheckBox.Checked = true;
                }
                

                //Radiobutton setting for prefix / suffix 
                RadioButton myTableRadioButton;

                if (configList["TableNamingLocation"] == "Prefix")
                {
                    myTableRadioButton = tablePrefixRadiobutton;
                    myTableRadioButton.Checked = true;
                }
                else
                {
                    myTableRadioButton = tableSuffixRadiobutton;
                    myTableRadioButton.Checked = true;
                }

                //Radiobutton settings for on key location
                RadioButton myKeyRadioButton;

                if (configList["KeyNamingLocation"] == "Prefix")
                {
                    myKeyRadioButton = keyPrefixRadiobutton;
                    myKeyRadioButton.Checked = true;
                }
                else
                {
                    myKeyRadioButton = keySuffixRadiobutton;
                    myKeyRadioButton.Checked = true;
                }

                //Radiobutton settings for PSA Natural Key determination
                RadioButton myPsaBusinessKeyLocation;

                if (configList["PSAKeyLocation"] == "PrimaryKey")
                {
                    myPsaBusinessKeyLocation = radioButtonPSABusinessKeyPK;
                    myPsaBusinessKeyLocation.Checked = true;
                }
                else
                {
                    myPsaBusinessKeyLocation = radioButtonPSABusinessKeyIndex;
                    myPsaBusinessKeyLocation.Checked = true;
                }

                //Radiobutton settings for repository type
                RadioButton myMetadatarepositoryType;

                if (configList["metadataRepositoryType"] == "JSON")
                {
                    myMetadatarepositoryType = radioButtonJSON;
                    myMetadatarepositoryType.Checked = true;
                }
                else
                {
                    myMetadatarepositoryType = radioButtonSQLServer;
                    myMetadatarepositoryType.Checked = true;
                }

                richTextBoxInformation.AppendText("The default values were loaded. \r\n\r\n");
                richTextBoxInformation.AppendText(@"The file " + chosenFile + " was uploaded successfully. \r\n\r\n");
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText("\r\n\r\nAn error occured while interpreting the configuration file. The original error is: '" + ex.Message + "'");
            }
        }

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
            try
            {
                Process.Start(textBoxOutputPath.Text);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occured while attempting to open the output directory. The error message is: "+ex;
            }
        }

        private void openConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var configurationSettings = new ConfigurationSettings();
            var configurationPath = configurationSettings.ConfigurationPath; //Application.StartupPath + @"\Configuration\";
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Configuration File",
                Filter = @"Text files|*.txt",
                InitialDirectory = @""+configurationPath+""
            };

            if (theDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                var myStream = theDialog.OpenFile();

                using (myStream)
                {
                    richTextBoxInformation.Clear();
                    var chosenFile = theDialog.FileName;
                    LocalInitialiseConnections(chosenFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message, "An issues has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Update the root path file
            var rootPathConfigurationFile = new StringBuilder();

            // Setup an object to retrieve the configuration
            var configurationSettings = new ConfigurationSettings();

            rootPathConfigurationFile.AppendLine("/* TEAM File Path Settings */");
            rootPathConfigurationFile.AppendLine("/* Saved at " + DateTime.Now + " */");
            rootPathConfigurationFile.AppendLine("ConfigurationPath|" + textBoxConfigurationPath.Text + "");
            rootPathConfigurationFile.AppendLine("OutputPath|" + textBoxOutputPath.Text + "");
            rootPathConfigurationFile.AppendLine("/* End of file */");

            using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + GlobalParameters.PathfileName))
            {
                outfile.Write(rootPathConfigurationFile.ToString());
                outfile.Close();
            }


            // Create a file backup for the config file
            try
            {
                if (File.Exists(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigfileName))
                {
                    var shortDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var targetFilePathName = GlobalParameters.ConfigurationPath +
                                             string.Concat("Backup_" + shortDatetime + "_", GlobalParameters.ConfigfileName);

                    File.Copy(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigfileName, targetFilePathName);
                    richTextBoxInformation.Text = "A backup of the current configuration was made at " + targetFilePathName;
                }
                else
                {
                    InitialisePath();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured while creating a file backup. The error message is " + ex, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Update the configuration file
            try
            {
                var configurationFile = new StringBuilder();


                configurationFile.AppendLine("/* TEAM Configuration Settings */");
                configurationFile.AppendLine("/* Saved at "+DateTime.Now+" */");
                configurationFile.AppendLine("SourceDatabase|"+textBoxSourceDatabase.Text+"");
                configurationFile.AppendLine("StagingDatabase|"+textBoxStagingDatabase.Text+"");
                configurationFile.AppendLine("PersistentStagingDatabase|"+textBoxPSADatabase.Text+"");
                configurationFile.AppendLine("IntegrationDatabase|"+textBoxIntegrationDatabase.Text+"");
                configurationFile.AppendLine("PresentationDatabase|"+textBoxPresentationDatabase.Text+"");
                configurationFile.AppendLine("OutputPath|" + textBoxOutputPath.Text + "");
                configurationFile.AppendLine("ConfigurationPath|" + textBoxConfigurationPath.Text + "");
                configurationFile.AppendLine(@"connectionStringSource|"+textBoxSourceConnection.Text+"");
                configurationFile.AppendLine(@"connectionStringStaging|" + textBoxStagingConnection.Text + "");
                configurationFile.AppendLine(@"connectionStringPersistentStaging|" + textBoxPSAConnection.Text + "");
                configurationFile.AppendLine(@"connectionStringMetadata|"+textBoxMetadataConnection.Text+"");
                configurationFile.AppendLine(@"connectionStringIntegration|"+textBoxIntegrationConnection.Text+"");
                configurationFile.AppendLine(@"connectionStringPresentation|"+textBoxPresentationConnection.Text+"");
                configurationFile.AppendLine("SourceSystemPrefix|"+textBoxSourcePrefix.Text+"");
                configurationFile.AppendLine("StagingAreaPrefix|" + textBoxStagingAreaPrefix.Text + "");
                configurationFile.AppendLine("PersistentStagingAreaPrefix|" + textBoxPSAPrefix.Text + "");
                configurationFile.AppendLine("HubTablePrefix|"+textBoxHubTablePrefix.Text+"");
                configurationFile.AppendLine("SatTablePrefix|"+textBoxSatPrefix.Text+"");
                configurationFile.AppendLine("LinkTablePrefix|"+textBoxLinkTablePrefix.Text+"");
                configurationFile.AppendLine("LinkSatTablePrefix|"+textBoxLinkSatPrefix.Text+"");
                configurationFile.AppendLine("KeyIdentifier|"+textBoxDWHKeyIdentifier.Text+"");
                configurationFile.AppendLine("SchemaName|"+textBoxSchemaName.Text+"");
                configurationFile.AppendLine("RowID|"+textBoxSourceRowId.Text+"");
                configurationFile.AppendLine("EventDateTimeStamp|"+textBoxEventDateTime.Text+"");
                configurationFile.AppendLine("LoadDateTimeStamp|"+textBoxLDST.Text+"");
                configurationFile.AppendLine("ExpiryDateTimeStamp|" + textBoxExpiryDateTimeName.Text + "");
                configurationFile.AppendLine("ChangeDataIndicator|"+textBoxChangeDataCaptureIndicator.Text+"");
                configurationFile.AppendLine("RecordSourceAttribute|"+textBoxRecordSource.Text+"");
                configurationFile.AppendLine("ETLProcessID|"+textBoxETLProcessID.Text+"");
                configurationFile.AppendLine("ETLUpdateProcessID|"+textBoxETLUpdateProcessID.Text+"");
                configurationFile.AppendLine("LogicalDeleteAttribute|" + textBoxLogicalDeleteAttributeName.Text + "");
                configurationFile.AppendLine("LinkedServerName|" + textBoxLinkedServer.Text + "");


                // Evaluate the prefix position for the table
                if (tablePrefixRadiobutton.Checked)
                {
                    configurationFile.AppendLine("TableNamingLocation|Prefix");
                }
                if (tableSuffixRadiobutton.Checked)
                {
                    configurationFile.AppendLine("TableNamingLocation|Suffix");
                }

                // Evaluate the prefix position of the primary key in the Data Vault (i.e. SK_xyz or xyz_SK)
                if (keyPrefixRadiobutton.Checked)
                {
                    configurationFile.AppendLine("KeyNamingLocation|Prefix");
                }
                if (keySuffixRadiobutton.Checked)
                {
                    configurationFile.AppendLine("KeyNamingLocation|Suffix");
                }

                configurationFile.AppendLine("RecordChecksum|"+textBoxRecordChecksum.Text+"");
                configurationFile.AppendLine("CurrentRecordAttribute|" + textBoxCurrentRecordAttributeName.Text+"");

                configurationFile.AppendLine("AlternativeRecordSource|" + textBoxAlternativeRecordSource.Text + "");
                configurationFile.AppendLine("AlternativeHubLDTS|" + textBoxHubAlternativeLDTSAttribute.Text + "");
                configurationFile.AppendLine("AlternativeSatelliteLDTS|" + textBoxSatelliteAlternativeLDTSAttribute.Text + "");
                configurationFile.AppendLine("AlternativeRecordSourceFunction|" + checkBoxAlternativeRecordSource.Checked + "");
                configurationFile.AppendLine("AlternativeHubLDTSFunction|" + checkBoxAlternativeHubLDTS.Checked + "");
                configurationFile.AppendLine("AlternativeSatelliteLDTSFunction|" + checkBoxAlternativeSatLDTS.Checked + "");

                // Defining the way the natural key is retrieved from the PSA
                if (radioButtonPSABusinessKeyIndex.Checked)
                {
                    configurationFile.AppendLine("PSAKeyLocation|UniqueIndex");
                }
                if (radioButtonPSABusinessKeyPK.Checked)
                {
                    configurationFile.AppendLine("PSAKeyLocation|PrimaryKey");
                }

                // Capturing the type of metadata repository (i.e. file or database and types)
                if (radioButtonJSON.Checked)
                {
                    configurationFile.AppendLine("metadataRepositoryType|JSON");
                }
                if (radioButtonSQLServer.Checked)
                {
                    configurationFile.AppendLine("metadataRepositoryType|SQLServer");
                }

                // Closing off
                configurationFile.AppendLine("/* End of file */");

                using (var outfile = new StreamWriter(configurationSettings.ConfigurationPath + GlobalParameters.ConfigfileName))
                {
                    outfile.Write(configurationFile.ToString());
                    outfile.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured saving the Configuration File. The error message is " + ex, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }




            //Make sure the variables in memory are updated as well
            configurationSettings.SourceDatabaseName = textBoxSourceDatabase.Text;
            configurationSettings.StagingDatabaseName = textBoxStagingDatabase.Text;
            configurationSettings.PsaDatabaseName = textBoxPSADatabase.Text;
            configurationSettings.IntegrationDatabaseName = textBoxIntegrationDatabase.Text;
            configurationSettings.PresentationDatabaseName = textBoxPresentationDatabase.Text;

            configurationSettings.ConnectionStringSource = textBoxSourceConnection.Text;
            configurationSettings.ConnectionStringStg = textBoxStagingConnection.Text;
            configurationSettings.ConnectionStringHstg = textBoxPSAConnection.Text;
            configurationSettings.ConnectionStringInt = textBoxIntegrationConnection.Text;
            configurationSettings.ConnectionStringOmd = textBoxMetadataConnection.Text;
            configurationSettings.ConnectionStringPres = textBoxPresentationConnection.Text;

            if (radioButtonJSON.Checked)
            {
                configurationSettings.metadataRepositoryType = "JSON";
            }
            else if (radioButtonSQLServer.Checked)
            {
                configurationSettings.metadataRepositoryType = "SQLServer";
            }
            else
            {
                richTextBoxInformation.AppendText("Issues storing the metadata repository type. Is one of the radio buttons checked?");
            }


            configurationSettings.OutputPath = textBoxOutputPath.Text;
            configurationSettings.ConfigurationPath = textBoxConfigurationPath.Text;

            configurationSettings.SourceSystemPrefix = textBoxSourcePrefix.Text;
            configurationSettings.StgTablePrefixValue = textBoxStagingAreaPrefix.Text;
            configurationSettings.PsaTablePrefixValue = textBoxPSAPrefix.Text;
            configurationSettings.HubTablePrefixValue = textBoxHubTablePrefix.Text;
            configurationSettings.SatTablePrefixValue = textBoxSatPrefix.Text;
            configurationSettings.LinkTablePrefixValue = textBoxLinkTablePrefix.Text;
            configurationSettings.LsatPrefixValue = textBoxLinkSatPrefix.Text;

            if (keyPrefixRadiobutton.Checked)
            {
                configurationSettings.DwhKeyIdentifier = "Prefix";
            }
            else if (keySuffixRadiobutton.Checked)
            {
                configurationSettings.DwhKeyIdentifier = "Suffix";
            }
            else
            {
                richTextBoxInformation.AppendText("Issues storing the key location (prefix/suffix). Is one of the radio buttons checked?");
            }

            configurationSettings.SchemaName = textBoxSchemaName.Text;
            configurationSettings.RowIdAttribute = textBoxSourceRowId.Text;
            configurationSettings.EventDateTimeAttribute = textBoxEventDateTime.Text;
            configurationSettings.LoadDateTimeAttribute = textBoxLDST.Text;
            configurationSettings.ExpiryDateTimeAttribute = textBoxExpiryDateTimeName.Text;
            configurationSettings.ChangeDataCaptureAttribute = textBoxChangeDataCaptureIndicator.Text;
            configurationSettings.RecordSourceAttribute = textBoxRecordSource.Text;
            configurationSettings.EtlProcessAttribute = textBoxETLProcessID.Text;
            configurationSettings.EtlProcessUpdateAttribute = textBoxETLUpdateProcessID.Text;
            configurationSettings.LogicalDeleteAttribute = textBoxLogicalDeleteAttributeName.Text;
            configurationSettings.LinkedServer = textBoxLinkedServer.Text;

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
    }
}
