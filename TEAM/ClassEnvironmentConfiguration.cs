using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TEAM
{
    /// <summary>
    ///   The configuration information used to drive variables and make the various configuration settings available in the application
    /// </summary>
    internal class EnvironmentConfiguration
    {
        /// <summary>
        ///    Method to create a new configuration file with default values at the default location
        /// </summary>
        internal void CreateDummyEnvironmentConfiguration(string filename)
        {
            if (Form_Base.GlobalParameters.WorkingEnvironment == "Development")
            {
                // Create a completely new file
                var initialConfigurationFile = new StringBuilder();

                initialConfigurationFile.AppendLine("/* TEAM Configuration Settings */");

                // Databases
                initialConfigurationFile.AppendLine("SourceDatabase|Source_Database");
                initialConfigurationFile.AppendLine("StagingDatabase|Staging_Area_Database");
                initialConfigurationFile.AppendLine("PersistentStagingDatabase|Persistent_Staging_Area_Database");
                initialConfigurationFile.AppendLine("IntegrationDatabase|Data_Vault_Database");
                initialConfigurationFile.AppendLine("PresentationDatabase|Presentation_Database");
                initialConfigurationFile.AppendLine("MetadataDatabase|Metadata_Database");

                // Instances
                initialConfigurationFile.AppendLine("PhysicalModelServerName|");
                initialConfigurationFile.AppendLine("MetadataServerName|");

                // Connectivity
                initialConfigurationFile.AppendLine("MetadataSSPI|False");
                initialConfigurationFile.AppendLine("MetadataNamed|True");
                initialConfigurationFile.AppendLine("MetadataUserName|sa");
                initialConfigurationFile.AppendLine("MetadataPassword|");

                initialConfigurationFile.AppendLine("PhysicalModelSSPI|False");
                initialConfigurationFile.AppendLine("PhysicalModelNamed|True");
                initialConfigurationFile.AppendLine("PhysicalModelUserName|sa");
                initialConfigurationFile.AppendLine("PhysicalModelPassword|");

                // Connection strings
                initialConfigurationFile.AppendLine(@"connectionStringSource|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Source_Database>;user id=sa; password=<>");
                initialConfigurationFile.AppendLine(@"connectionStringStaging|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Staging_Area>;user id=sa; password=<>");
                initialConfigurationFile.AppendLine(@"connectionStringPersistentStaging|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Persistent_Staging_Area>;user id=sa; password=<>");
                initialConfigurationFile.AppendLine(@"connectionStringMetadata|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Metadata>;user id=sa; password=<>");
                initialConfigurationFile.AppendLine(@"connectionStringIntegration|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Data_Vault>;user id=sa; password=<>");
                initialConfigurationFile.AppendLine(@"connectionStringPresentation|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Presentation>;user id=sa; password=<>");

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
                initialConfigurationFile.AppendLine("LogicalDeleteAttribute|DELETED_RECORD_INDICATOR");
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
                initialConfigurationFile.AppendLine("PSAKeyLocation|PrimaryKey"); //Can be PrimaryKey or UniqueIndex
                initialConfigurationFile.AppendLine("metadataRepositoryType|JSON");

                initialConfigurationFile.AppendLine("/* End of file */");

                using (var outfile = new StreamWriter(filename))
                {
                    outfile.Write(initialConfigurationFile.ToString());
                    outfile.Close();
                }
            } else if (Form_Base.GlobalParameters.WorkingEnvironment == "Production")
            {
                // Just copy the dev file to a prod version to retain settings
                // Check if the paths are available, just to be sure
                InitialiseRootPath();

                try
                {
                    var sourceFilePathName = Form_Base.GlobalParameters.ConfigurationPath +
                                             Form_Base.GlobalParameters.ConfigFileName + '_' + "Development" +
                                             Form_Base.GlobalParameters.FileExtension;

                    if (File.Exists(sourceFilePathName))
                    {
                        var targetFilePathName = Form_Base.GlobalParameters.ConfigurationPath + Form_Base.GlobalParameters.ConfigFileName + '_' + "Production" + Form_Base.GlobalParameters.FileExtension;

                        File.Copy(sourceFilePathName, targetFilePathName);

                    }
                    else
                    {
                        MessageBox.Show(
                            "TEAM couldn't locate a development configuration file! Can you check the paths and existence of directories?",
                            "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error has occured during the creating of the production settings file. The error message is " + ex,
                        "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(
                    "Either a Development or Production environment was expected! Can you check the radiobox settings for the environment?",
                    "An issue has been enountered.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


        }


        /// <summary>
        ///    Method to create a new validation file with default values at the default location
        /// </summary>
        internal void CreateDummyValidationConfiguration(string filename)
        {
            var validationFile = new StringBuilder();

            validationFile.AppendLine("/* TEAM Validation Settings */");

            // Object existence validation
            validationFile.AppendLine("SourceObjectExistence|True");
            validationFile.AppendLine("TargetObjectExistence|True");
            validationFile.AppendLine("BusinessKeyExistence|True");

            validationFile.AppendLine("SourceAttributeExistence|True");
            validationFile.AppendLine("TargetAttributeExistence|True");

            // Consistency validation
            validationFile.AppendLine("LogicalGroup|True");
            validationFile.AppendLine("LinkKeyOrder|True");
            validationFile.AppendLine("BusinessKeySyntax|True"); 


            validationFile.AppendLine("/* End of file */");

            using (var outfile = new StreamWriter(filename))
            {
                outfile.Write(validationFile.ToString());
                outfile.Close();
            }
        }


        /// <summary>
        ///    Create a file backup for the configuration file at the provided location
        /// </summary>
        internal static void CreateEnvironmentConfigurationBackupFile()
        {
            // Check if the paths are available, just to be sure
            InitialiseRootPath();

            try
            {
                if (File.Exists(Form_Base.GlobalParameters.ConfigurationPath +
                                Form_Base.GlobalParameters.ConfigFileName + '_' +
                                Form_Base.GlobalParameters.WorkingEnvironment +
                                Form_Base.GlobalParameters.FileExtension))
                {
                    var targetFilePathName = Form_Base.GlobalParameters.ConfigurationPath +
                                             string.Concat("Backup_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_",
                                                 Form_Base.GlobalParameters.ConfigFileName + '_' +
                                                 Form_Base.GlobalParameters.WorkingEnvironment +
                                                 Form_Base.GlobalParameters.FileExtension);

                    File.Copy(
                        Form_Base.GlobalParameters.ConfigurationPath + Form_Base.GlobalParameters.ConfigFileName +
                        '_' + Form_Base.GlobalParameters.WorkingEnvironment +
                        Form_Base.GlobalParameters.FileExtension, targetFilePathName);

                }
                else
                {
                    MessageBox.Show(
                        "TEAM couldn't locate a configuration file! Can you check the paths and existence of directories?",
                        "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured while creating a file backup. The error message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        ///    Check if the paths exists and create them if necessary
        /// </summary>
        internal static void InitialiseRootPath()
        {
            // Create the configuration directory if it does not exist yet
            try
            {
                if (!Directory.Exists(Form_Base.GlobalParameters.ConfigurationPath))
                {
                    Directory.CreateDirectory(Form_Base.GlobalParameters.ConfigurationPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error creation default directory at " + Form_Base.GlobalParameters.ConfigurationPath +
                    " the message is " + ex, "An issue has been encountered", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            // Create the output directory if it does not exist yet
            try
            {
                if (!Directory.Exists(Form_Base.GlobalParameters.OutputPath))
                {
                    Directory.CreateDirectory(Form_Base.GlobalParameters.OutputPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error creation default directory at " + Form_Base.GlobalParameters.OutputPath + " the message is " +
                    ex, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Create root path file, with dummy values if it doesn't exist already
            try
            {
                if (!File.Exists(Form_Base.GlobalParameters.ConfigurationPath + Form_Base.GlobalParameters.PathFileName +
                                 Form_Base.GlobalParameters.FileExtension))
                {
                    var initialConfigurationFile = new StringBuilder();

                    initialConfigurationFile.AppendLine("/* TEAM File Path Settings */");
                    initialConfigurationFile.AppendLine("ConfigurationPath|" +Form_Base.GlobalParameters.ConfigurationPath);
                    initialConfigurationFile.AppendLine("OutputPath|" + Form_Base.GlobalParameters.OutputPath);
                    initialConfigurationFile.AppendLine("WorkingEnvironment|Development");
                    initialConfigurationFile.AppendLine("/* End of file */");

                    using (var outfile = new StreamWriter(Form_Base.GlobalParameters.ConfigurationPath +
                                                          Form_Base.GlobalParameters.PathFileName +
                                                          Form_Base.GlobalParameters.FileExtension))
                    {
                        outfile.Write(initialConfigurationFile.ToString());
                        outfile.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while creation the default path file. The error message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        ///    Check if the paths exists and create them if necessary
        /// </summary>
        internal static void InitialiseConfigurationPath()
        {
            // Create the configuration directory if it does not exist yet
            try
            {
                if (!Directory.Exists(Form_Base.GlobalParameters.ConfigurationPath))
                {
                    Directory.CreateDirectory(Form_Base.GlobalParameters.ConfigurationPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error creation default directory at " + Form_Base.GlobalParameters.ConfigurationPath +
                    " the message is " + ex, "An issue has been encountered", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            // Create the output directory if it does not exist yet
            try
            {
                if (!Directory.Exists(Form_Base.GlobalParameters.OutputPath))
                {
                    Directory.CreateDirectory(Form_Base.GlobalParameters.OutputPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error creation default directory at " + Form_Base.GlobalParameters.OutputPath +
                    " the message is " + ex, "An issue has been encountered", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            // Create a new dummy configuration file
            try
            {
                // Create a default configuration file if the file does not exist as expected
                if (File.Exists(Form_Base.GlobalParameters.ConfigurationPath +
                                Form_Base.GlobalParameters.ConfigFileName + '_' +
                                Form_Base.GlobalParameters.WorkingEnvironment +
                                Form_Base.GlobalParameters.FileExtension)) return;
                var newEnvironmentConfiguration = new EnvironmentConfiguration();
                newEnvironmentConfiguration.CreateDummyEnvironmentConfiguration(
                    Form_Base.GlobalParameters.ConfigurationPath + Form_Base.GlobalParameters.ConfigFileName + '_' +
                    Form_Base.GlobalParameters.WorkingEnvironment + Form_Base.GlobalParameters.FileExtension);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while creation the default Configuration File. The error message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Create a new dummy validation file
            try
            {
                // Create a default configuration file if the file does not exist as expected
                if (File.Exists(Form_Base.GlobalParameters.ConfigurationPath +
                                Form_Base.GlobalParameters.ValidationFileName + '_' +
                                Form_Base.GlobalParameters.WorkingEnvironment +
                                Form_Base.GlobalParameters.FileExtension)) return;
                var newEnvironmentConfiguration = new EnvironmentConfiguration();
                newEnvironmentConfiguration.CreateDummyEnvironmentConfiguration(
                    Form_Base.GlobalParameters.ConfigurationPath + Form_Base.GlobalParameters.ValidationFileName +
                    '_' + Form_Base.GlobalParameters.WorkingEnvironment + Form_Base.GlobalParameters.FileExtension);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while creation the default Configuration File. The error message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Retrieve the values of the application root path (where the paths to the configuration file is maintained)
        /// </summary>
        public static void LoadRootPathFile()
        {
            // This is the hardcoded base path that always needs to be accessible, it has the main file which can locate the rest of the configuration
            var configList = new Dictionary<string, string>();
            var fs = new FileStream(
                Form_Base.GlobalParameters.ConfigurationPath + Form_Base.GlobalParameters.PathFileName +
                Form_Base.GlobalParameters.FileExtension, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs);

            try
            {
                string textline;
                while ((textline = sr.ReadLine()) != null)
                {
                    if (textline.IndexOf(@"/*", StringComparison.Ordinal) == -1 && textline.Trim() != "")
                    {
                        var line = textline.Split('|');
                        configList.Add(line[0], line[1]);
                    }
                }

                sr.Close();
                fs.Close();

                // These variables are used as global variables throughout the application
                Form_Base.GlobalParameters.ConfigurationPath = configList["ConfigurationPath"];
                Form_Base.GlobalParameters.OutputPath = configList["OutputPath"];
                Form_Base.GlobalParameters.WorkingEnvironment = configList["WorkingEnvironment"];

            }
            catch (Exception)
            {
                // richTextBoxInformation.AppendText("\r\n\r\nAn error occured while interpreting the configuration file. The original error is: '" + ex.Message + "'");
            }
        }


        /// <summary>
        ///    Retrieve the configuration information from memory and save this to disk
        /// </summary>
        internal static void SaveConfigurationFile()
        {
            try
            {
                var configurationFile = new StringBuilder();
                configurationFile.AppendLine("/* TEAM Configuration Settings */");
                configurationFile.AppendLine("/* Saved at " + DateTime.Now + " */");

                configurationFile.AppendLine("SourceDatabase|" + Form_Base.ConfigurationSettings.SourceDatabaseName +"");
                configurationFile.AppendLine("StagingDatabase|" + Form_Base.ConfigurationSettings.StagingDatabaseName +"");
                configurationFile.AppendLine("PersistentStagingDatabase|" +Form_Base.ConfigurationSettings.PsaDatabaseName + "");
                configurationFile.AppendLine("IntegrationDatabase|" +Form_Base.ConfigurationSettings.IntegrationDatabaseName + "");
                configurationFile.AppendLine("PresentationDatabase|" +Form_Base.ConfigurationSettings.PresentationDatabaseName +"");
                configurationFile.AppendLine("MetadataDatabase|" + Form_Base.ConfigurationSettings.MetadataDatabaseName + "");
                configurationFile.AppendLine("PhysicalModelServerName|" + Form_Base.ConfigurationSettings.PhysicalModelServerName + "");
                configurationFile.AppendLine("MetadataServerName|" + Form_Base.ConfigurationSettings.MetadataServerName + "");


                configurationFile.AppendLine("MetadataSSPI|" + Form_Base.ConfigurationSettings.MetadataSSPI);
                configurationFile.AppendLine("MetadataNamed|" + Form_Base.ConfigurationSettings.MetadataNamed);
                configurationFile.AppendLine("MetadataUserName|" + Form_Base.ConfigurationSettings.MetadataUserName);
                configurationFile.AppendLine("MetadataPassword|" + Form_Base.ConfigurationSettings.MetadataPassword);

                configurationFile.AppendLine("PhysicalModelSSPI|" + Form_Base.ConfigurationSettings.PhysicalModelSSPI);
                configurationFile.AppendLine("PhysicalModelNamed|" + Form_Base.ConfigurationSettings.PhysicalModelNamed);
                configurationFile.AppendLine("PhysicalModelUserName|" + Form_Base.ConfigurationSettings.PhysicalModelUserName);
                configurationFile.AppendLine("PhysicalModelPassword|" + Form_Base.ConfigurationSettings.PhysicalModelPassword);


                configurationFile.AppendLine(@"connectionStringSource|" +Form_Base.ConfigurationSettings.ConnectionStringSource +"");
                configurationFile.AppendLine(@"connectionStringStaging|" +Form_Base.ConfigurationSettings.ConnectionStringStg +"");
                configurationFile.AppendLine(@"connectionStringPersistentStaging|" +Form_Base.ConfigurationSettings.ConnectionStringHstg + "");
                configurationFile.AppendLine(@"connectionStringMetadata|" +Form_Base.ConfigurationSettings.ConnectionStringOmd +"");
                configurationFile.AppendLine(@"connectionStringIntegration|" +Form_Base.ConfigurationSettings.ConnectionStringInt + "");
                configurationFile.AppendLine(@"connectionStringPresentation|" +Form_Base.ConfigurationSettings.ConnectionStringPres + "");

                configurationFile.AppendLine("StagingAreaPrefix|" + Form_Base.ConfigurationSettings.StgTablePrefixValue +"");
                configurationFile.AppendLine("PersistentStagingAreaPrefix|" +Form_Base.ConfigurationSettings.PsaTablePrefixValue + "");
                configurationFile.AppendLine("HubTablePrefix|" + Form_Base.ConfigurationSettings.HubTablePrefixValue + "");
                configurationFile.AppendLine("SatTablePrefix|" + Form_Base.ConfigurationSettings.SatTablePrefixValue + "");
                configurationFile.AppendLine("LinkTablePrefix|" + Form_Base.ConfigurationSettings.LinkTablePrefixValue +"");
                configurationFile.AppendLine("LinkSatTablePrefix|" + Form_Base.ConfigurationSettings.LsatTablePrefixValue + "");
                configurationFile.AppendLine("KeyIdentifier|" + Form_Base.ConfigurationSettings.DwhKeyIdentifier + "");
                configurationFile.AppendLine("SchemaName|" + Form_Base.ConfigurationSettings.SchemaName + "");
                configurationFile.AppendLine("RowID|" + Form_Base.ConfigurationSettings.RowIdAttribute + "");
                configurationFile.AppendLine("EventDateTimeStamp|" + Form_Base.ConfigurationSettings.EventDateTimeAttribute + "");
                configurationFile.AppendLine("LoadDateTimeStamp|" + Form_Base.ConfigurationSettings.LoadDateTimeAttribute + "");
                configurationFile.AppendLine("ExpiryDateTimeStamp|" + Form_Base.ConfigurationSettings.ExpiryDateTimeAttribute + "");
                configurationFile.AppendLine("ChangeDataIndicator|" + Form_Base.ConfigurationSettings.ChangeDataCaptureAttribute +"");
                configurationFile.AppendLine("RecordSourceAttribute|" + Form_Base.ConfigurationSettings.RecordSourceAttribute + "");
                configurationFile.AppendLine("ETLProcessID|" + Form_Base.ConfigurationSettings.EtlProcessAttribute + "");
                configurationFile.AppendLine("ETLUpdateProcessID|" +Form_Base.ConfigurationSettings.EtlProcessUpdateAttribute +"");
                configurationFile.AppendLine("LogicalDeleteAttribute|" +Form_Base.ConfigurationSettings.LogicalDeleteAttribute +"");
                configurationFile.AppendLine("TableNamingLocation|" + Form_Base.ConfigurationSettings.TableNamingLocation + "");
                configurationFile.AppendLine("KeyNamingLocation|" + Form_Base.ConfigurationSettings.KeyNamingLocation +"");
                configurationFile.AppendLine("RecordChecksum|" +Form_Base.ConfigurationSettings.RecordChecksumAttribute + "");
                configurationFile.AppendLine("CurrentRecordAttribute|" +Form_Base.ConfigurationSettings.CurrentRowAttribute +"");
                configurationFile.AppendLine("AlternativeRecordSource|" +Form_Base.ConfigurationSettings.AlternativeRecordSourceAttribute + "");
                configurationFile.AppendLine("AlternativeHubLDTS|" +Form_Base.ConfigurationSettings.AlternativeLoadDateTimeAttribute + "");
                configurationFile.AppendLine("AlternativeSatelliteLDTS|" +Form_Base.ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute +"");
                configurationFile.AppendLine("AlternativeRecordSourceFunction|" +Form_Base.ConfigurationSettings.EnableAlternativeRecordSourceAttribute +"");
                configurationFile.AppendLine("AlternativeHubLDTSFunction|" +Form_Base.ConfigurationSettings.EnableAlternativeLoadDateTimeAttribute +"");
                configurationFile.AppendLine("AlternativeSatelliteLDTSFunction|" +Form_Base.ConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute +"");
                configurationFile.AppendLine("PSAKeyLocation|" + Form_Base.ConfigurationSettings.PsaKeyLocation + "");
                configurationFile.AppendLine("metadataRepositoryType|" +Form_Base.ConfigurationSettings.MetadataRepositoryType +"");

                // Closing off
                configurationFile.AppendLine("/* End of file */");

                using (var outfile =
                    new StreamWriter(Form_Base.GlobalParameters.ConfigurationPath +
                                     Form_Base.GlobalParameters.ConfigFileName + '_' +
                                     Form_Base.GlobalParameters.WorkingEnvironment +
                                     Form_Base.GlobalParameters.FileExtension))
                {
                    outfile.Write(configurationFile.ToString());
                    outfile.Flush();
                    outfile.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured saving the Configuration File. The error message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        ///    Retrieve the configuration information from disk and save this to memory
        /// </summary>
        internal static void LoadConfigurationFile(string filename)
        {
            try
            {
                var configList = new Dictionary<string, string>();
                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var sr = new StreamReader(fs);

                string textline;
                while ((textline = sr.ReadLine()) != null)
                {
                    if (textline.IndexOf(@"/*", StringComparison.Ordinal) == -1 && textline.Trim() != "")
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

                // These variables are used as global variables throughout the application
                // They will be set once after startup
                Form_Base.ConfigurationSettings.ConnectionStringSource = connectionStringSource;
                Form_Base.ConfigurationSettings.ConnectionStringStg = connectionStringStg;
                Form_Base.ConfigurationSettings.ConnectionStringHstg = connectionStringHstg;
                Form_Base.ConfigurationSettings.ConnectionStringInt = connectionStringInt;
                Form_Base.ConfigurationSettings.ConnectionStringOmd = connectionStringOmd;
                Form_Base.ConfigurationSettings.ConnectionStringPres = connectionStringPres;

                Form_Base.ConfigurationSettings.MetadataRepositoryType = configList["metadataRepositoryType"];

                Form_Base.ConfigurationSettings.StgTablePrefixValue = configList["StagingAreaPrefix"];
                Form_Base.ConfigurationSettings.PsaTablePrefixValue = configList["PersistentStagingAreaPrefix"];
                Form_Base.ConfigurationSettings.HubTablePrefixValue = configList["HubTablePrefix"];
                Form_Base.ConfigurationSettings.SatTablePrefixValue = configList["SatTablePrefix"];
                Form_Base.ConfigurationSettings.LinkTablePrefixValue = configList["LinkTablePrefix"];
                Form_Base.ConfigurationSettings.LsatTablePrefixValue = configList["LinkSatTablePrefix"];
                Form_Base.ConfigurationSettings.DwhKeyIdentifier = configList["KeyIdentifier"];
                Form_Base.ConfigurationSettings.PsaKeyLocation = configList["PSAKeyLocation"];
                Form_Base.ConfigurationSettings.TableNamingLocation = configList["TableNamingLocation"];
                Form_Base.ConfigurationSettings.KeyNamingLocation = configList["KeyNamingLocation"];
                Form_Base.ConfigurationSettings.SchemaName = configList["SchemaName"];
                Form_Base.ConfigurationSettings.EventDateTimeAttribute = configList["EventDateTimeStamp"];
                Form_Base.ConfigurationSettings.LoadDateTimeAttribute = configList["LoadDateTimeStamp"];
                Form_Base.ConfigurationSettings.ExpiryDateTimeAttribute = configList["ExpiryDateTimeStamp"];
                Form_Base.ConfigurationSettings.ChangeDataCaptureAttribute = configList["ChangeDataIndicator"];
                Form_Base.ConfigurationSettings.RecordSourceAttribute = configList["RecordSourceAttribute"];
                Form_Base.ConfigurationSettings.EtlProcessAttribute = configList["ETLProcessID"];
                Form_Base.ConfigurationSettings.EtlProcessUpdateAttribute = configList["ETLUpdateProcessID"];
                Form_Base.ConfigurationSettings.RowIdAttribute = configList["RowID"];
                Form_Base.ConfigurationSettings.RecordChecksumAttribute = configList["RecordChecksum"];
                Form_Base.ConfigurationSettings.CurrentRowAttribute = configList["CurrentRecordAttribute"];
                Form_Base.ConfigurationSettings.LogicalDeleteAttribute = configList["LogicalDeleteAttribute"];
                Form_Base.ConfigurationSettings.EnableAlternativeRecordSourceAttribute =configList["AlternativeRecordSourceFunction"];
                Form_Base.ConfigurationSettings.AlternativeRecordSourceAttribute = configList["AlternativeRecordSource"];
                Form_Base.ConfigurationSettings.EnableAlternativeLoadDateTimeAttribute =configList["AlternativeHubLDTSFunction"];
                Form_Base.ConfigurationSettings.AlternativeLoadDateTimeAttribute = configList["AlternativeHubLDTS"];
                Form_Base.ConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute = configList["AlternativeSatelliteLDTSFunction"];
                Form_Base.ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute = configList["AlternativeSatelliteLDTS"];

                // Databases
                Form_Base.ConfigurationSettings.SourceDatabaseName = configList["SourceDatabase"];
                Form_Base.ConfigurationSettings.StagingDatabaseName = configList["StagingDatabase"];
                Form_Base.ConfigurationSettings.PsaDatabaseName = configList["PersistentStagingDatabase"];
                Form_Base.ConfigurationSettings.IntegrationDatabaseName = configList["IntegrationDatabase"];
                Form_Base.ConfigurationSettings.PresentationDatabaseName = configList["PresentationDatabase"];
                Form_Base.ConfigurationSettings.MetadataDatabaseName = configList["MetadataDatabase"];

                // Servers (instances)
                Form_Base.ConfigurationSettings.PhysicalModelServerName = configList["PhysicalModelServerName"];
                Form_Base.ConfigurationSettings.MetadataServerName = configList["MetadataServerName"];

                // Authentication & connectivity
                Form_Base.ConfigurationSettings.MetadataSSPI = configList["MetadataSSPI"];
                Form_Base.ConfigurationSettings.MetadataNamed = configList["MetadataNamed"];
                Form_Base.ConfigurationSettings.MetadataUserName = configList["MetadataUserName"];
                Form_Base.ConfigurationSettings.MetadataPassword = configList["MetadataPassword"];

                Form_Base.ConfigurationSettings.PhysicalModelSSPI = configList["PhysicalModelSSPI"];
                Form_Base.ConfigurationSettings.PhysicalModelNamed = configList["PhysicalModelNamed"];
                Form_Base.ConfigurationSettings.PhysicalModelUserName = configList["PhysicalModelUserName"];
                Form_Base.ConfigurationSettings.PhysicalModelPassword = configList["PhysicalModelPassword"];

                // Paths
                Form_Base.GlobalParameters.OutputPath = configList["OutputPath"];
                Form_Base.GlobalParameters.ConfigurationPath = configList["ConfigurationPath"];

            }
            catch (Exception)
            {
                // richTextBoxInformation.AppendText("\r\n\r\nAn error occured while interpreting the configuration file. The original error is: '" + ex.Message + "'");
            }
        }


        /// <summary>
        ///    Retrieve the validation information from disk and save this to memory
        /// </summary>
        internal static void LoadValidationFile(string filename)
        {
            try
            {
                var configList = new Dictionary<string, string>();
                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var sr = new StreamReader(fs);

                string textline;
                while ((textline = sr.ReadLine()) != null)
                {
                    if (textline.IndexOf(@"/*", StringComparison.Ordinal) == -1 && textline.Trim() != "")
                    {
                        var line = textline.Split('|');
                        configList.Add(line[0], line[1]);
                    }
                }

                sr.Close();
                fs.Close();

                Form_Base.ValidationSettings.SourceObjectExistence = configList["SourceObjectExistence"];
                Form_Base.ValidationSettings.TargetObjectExistence = configList["TargetObjectExistence"];
                Form_Base.ValidationSettings.SourceBusinessKeyExistence = configList["BusinessKeyExistence"];
                Form_Base.ValidationSettings.SourceAttributeExistence = configList["SourceAttributeExistence"];
                Form_Base.ValidationSettings.TargetAttributeExistence = configList["TargetAttributeExistence"];

                Form_Base.ValidationSettings.LogicalGroup = configList["LogicalGroup"];
                Form_Base.ValidationSettings.LinkKeyOrder = configList["LinkKeyOrder"];

                Form_Base.ValidationSettings.BusinessKeySyntax = configList["BusinessKeySyntax"];
            }
            catch (Exception)
            {
                // Do nothing
            }
        }


        /// <summary>
        ///    Retrieve the validation information from memory and save this to disk
        /// </summary>
        internal static void SaveValidationFile()
        {
            try
            {
                // Creating the file
                var validationFile = new StringBuilder();
                validationFile.AppendLine("/* TEAM Validation Settings */");
                validationFile.AppendLine("/* Saved at " + DateTime.Now + " */");
                validationFile.AppendLine("SourceObjectExistence|" + Form_Base.ValidationSettings.SourceObjectExistence +"");
                validationFile.AppendLine("TargetObjectExistence|" + Form_Base.ValidationSettings.TargetObjectExistence +"");
                validationFile.AppendLine("BusinessKeyExistence|" +Form_Base.ValidationSettings.SourceBusinessKeyExistence + "");
                validationFile.AppendLine("SourceAttributeExistence|" + Form_Base.ValidationSettings.SourceAttributeExistence + "");
                validationFile.AppendLine("TargetAttributeExistence|" + Form_Base.ValidationSettings.TargetAttributeExistence + "");
                validationFile.AppendLine("LogicalGroup|" +Form_Base.ValidationSettings.LogicalGroup + "");
                validationFile.AppendLine("LinkKeyOrder|" + Form_Base.ValidationSettings.LinkKeyOrder + "");
                validationFile.AppendLine("BusinessKeySyntax|" + Form_Base.ValidationSettings.BusinessKeySyntax + "");

                // Closing off
                validationFile.AppendLine("/* End of file */");

                using (var outfile =
                    new StreamWriter(Form_Base.GlobalParameters.ConfigurationPath +
                                     Form_Base.GlobalParameters.ValidationFileName + '_' +
                                     Form_Base.GlobalParameters.WorkingEnvironment +
                                     Form_Base.GlobalParameters.FileExtension))
                {
                    outfile.Write(validationFile.ToString());
                    outfile.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured saving the Validation File. The error message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}