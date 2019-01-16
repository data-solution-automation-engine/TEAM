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
    internal class ClassEnvironmentConfiguration
    {
        /// <summary>
        ///    Method to create a new configuration file with default values at the default location
        /// </summary>
        internal void CreateDummyEnvironmentConfiguration(string filename)
        {
            if (FormBase.GlobalParameters.WorkingEnvironment == "Development")
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
                initialConfigurationFile.AppendLine("MetadataPassword|k3kobus2");

                initialConfigurationFile.AppendLine("PhysicalModelSSPI|False");
                initialConfigurationFile.AppendLine("PhysicalModelNamed|True");
                initialConfigurationFile.AppendLine("PhysicalModelUserName|sa");
                initialConfigurationFile.AppendLine("PhysicalModelPassword|k3kobus2");

                // Connection strings
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
            } else if (FormBase.GlobalParameters.WorkingEnvironment == "Production")
            {
                // Just copy the dev file to a prod version to retain settings
                // Check if the paths are available, just to be sure
                InitialiseRootPath();

                try
                {
                    var sourceFilePathName = FormBase.GlobalParameters.ConfigurationPath +
                                             FormBase.GlobalParameters.ConfigfileName + '_' + "Development" +
                                             FormBase.GlobalParameters.FileExtension;

                    if (File.Exists(sourceFilePathName))
                    {
                        var targetFilePathName = FormBase.GlobalParameters.ConfigurationPath + FormBase.GlobalParameters.ConfigfileName + '_' + "Production" + FormBase.GlobalParameters.FileExtension;

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
            validationFile.AppendLine("/* Roelant Vos - 2018 */");

            // Object existence validation
            validationFile.AppendLine("SourceObjectExistence|True");
            validationFile.AppendLine("TargetObjectExistence|True");
            validationFile.AppendLine("BusinessKeyExistence|True");

            // Consistency validation
            validationFile.AppendLine("LogicalGroup|True");
            validationFile.AppendLine("LinkKeyOrder|True");

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
                if (File.Exists(FormBase.GlobalParameters.ConfigurationPath +
                                FormBase.GlobalParameters.ConfigfileName + '_' +
                                FormBase.GlobalParameters.WorkingEnvironment +
                                FormBase.GlobalParameters.FileExtension))
                {
                    var targetFilePathName = FormBase.GlobalParameters.ConfigurationPath +
                                             string.Concat("Backup_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_",
                                                 FormBase.GlobalParameters.ConfigfileName + '_' +
                                                 FormBase.GlobalParameters.WorkingEnvironment +
                                                 FormBase.GlobalParameters.FileExtension);

                    File.Copy(
                        FormBase.GlobalParameters.ConfigurationPath + FormBase.GlobalParameters.ConfigfileName +
                        '_' + FormBase.GlobalParameters.WorkingEnvironment +
                        FormBase.GlobalParameters.FileExtension, targetFilePathName);

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
                if (!Directory.Exists(FormBase.GlobalParameters.ConfigurationPath))
                {
                    Directory.CreateDirectory(FormBase.GlobalParameters.ConfigurationPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error creation default directory at " + FormBase.GlobalParameters.ConfigurationPath +
                    " the message is " + ex, "An issue has been encountered", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            // Create the output directory if it does not exist yet
            try
            {
                if (!Directory.Exists(FormBase.GlobalParameters.OutputPath))
                {
                    Directory.CreateDirectory(FormBase.GlobalParameters.OutputPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error creation default directory at " + FormBase.GlobalParameters.OutputPath + " the message is " +
                    ex, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Create root path file, with dummy values if it doesn't exist already
            try
            {
                if (!File.Exists(FormBase.GlobalParameters.ConfigurationPath + FormBase.GlobalParameters.PathfileName +
                                 FormBase.GlobalParameters.FileExtension))
                {
                    var initialConfigurationFile = new StringBuilder();

                    initialConfigurationFile.AppendLine("/* TEAM File Path Settings */");
                    initialConfigurationFile.AppendLine("ConfigurationPath|" +FormBase.GlobalParameters.ConfigurationPath);
                    initialConfigurationFile.AppendLine("OutputPath|" + FormBase.GlobalParameters.OutputPath);
                    initialConfigurationFile.AppendLine("WorkingEnvironment|Development");
                    initialConfigurationFile.AppendLine("/* End of file */");

                    using (var outfile = new StreamWriter(FormBase.GlobalParameters.ConfigurationPath +
                                                          FormBase.GlobalParameters.PathfileName +
                                                          FormBase.GlobalParameters.FileExtension))
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
                if (!Directory.Exists(FormBase.GlobalParameters.ConfigurationPath))
                {
                    Directory.CreateDirectory(FormBase.GlobalParameters.ConfigurationPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error creation default directory at " + FormBase.GlobalParameters.ConfigurationPath +
                    " the message is " + ex, "An issue has been encountered", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            // Create the output directory if it does not exist yet
            try
            {
                if (!Directory.Exists(FormBase.GlobalParameters.OutputPath))
                {
                    Directory.CreateDirectory(FormBase.GlobalParameters.OutputPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error creation default directory at " + FormBase.GlobalParameters.OutputPath +
                    " the message is " + ex, "An issue has been encountered", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            // Create a new dummy configuration file
            try
            {
                // Create a default configuration file if the file does not exist as expected
                if (File.Exists(FormBase.GlobalParameters.ConfigurationPath +
                                FormBase.GlobalParameters.ConfigfileName + '_' +
                                FormBase.GlobalParameters.WorkingEnvironment +
                                FormBase.GlobalParameters.FileExtension)) return;
                var newEnvironmentConfiguration = new ClassEnvironmentConfiguration();
                newEnvironmentConfiguration.CreateDummyEnvironmentConfiguration(
                    FormBase.GlobalParameters.ConfigurationPath + FormBase.GlobalParameters.ConfigfileName + '_' +
                    FormBase.GlobalParameters.WorkingEnvironment + FormBase.GlobalParameters.FileExtension);
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
                if (File.Exists(FormBase.GlobalParameters.ConfigurationPath +
                                FormBase.GlobalParameters.ValidationFileName + '_' +
                                FormBase.GlobalParameters.WorkingEnvironment +
                                FormBase.GlobalParameters.FileExtension)) return;
                var newEnvironmentConfiguration = new ClassEnvironmentConfiguration();
                newEnvironmentConfiguration.CreateDummyEnvironmentConfiguration(
                    FormBase.GlobalParameters.ConfigurationPath + FormBase.GlobalParameters.ValidationFileName +
                    '_' + FormBase.GlobalParameters.WorkingEnvironment + FormBase.GlobalParameters.FileExtension);
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
                FormBase.GlobalParameters.ConfigurationPath + FormBase.GlobalParameters.PathfileName +
                FormBase.GlobalParameters.FileExtension, FileMode.Open, FileAccess.Read);
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

                // These variables are used as global variables throughout the application
                FormBase.GlobalParameters.ConfigurationPath = configList["ConfigurationPath"];
                FormBase.GlobalParameters.OutputPath = configList["OutputPath"];

                FormBase.GlobalParameters.WorkingEnvironment = configList["WorkingEnvironment"];

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

                configurationFile.AppendLine("SourceDatabase|" + FormBase.ConfigurationSettings.SourceDatabaseName +"");
                configurationFile.AppendLine("StagingDatabase|" + FormBase.ConfigurationSettings.StagingDatabaseName +"");
                configurationFile.AppendLine("PersistentStagingDatabase|" +FormBase.ConfigurationSettings.PsaDatabaseName + "");
                configurationFile.AppendLine("IntegrationDatabase|" +FormBase.ConfigurationSettings.IntegrationDatabaseName + "");
                configurationFile.AppendLine("PresentationDatabase|" +FormBase.ConfigurationSettings.PresentationDatabaseName +"");
                configurationFile.AppendLine("MetadataDatabase|" + FormBase.ConfigurationSettings.MetadataDatabaseName + "");
                configurationFile.AppendLine("PhysicalModelServerName|" + FormBase.ConfigurationSettings.PhysicalModelServerName + "");
                configurationFile.AppendLine("MetadataServerName|" + FormBase.ConfigurationSettings.MetadataServerName + "");


                configurationFile.AppendLine("MetadataSSPI|" + FormBase.ConfigurationSettings.MetadataSSPI);
                configurationFile.AppendLine("MetadataNamed|" + FormBase.ConfigurationSettings.MetadataNamed);
                configurationFile.AppendLine("MetadataUserName|" + FormBase.ConfigurationSettings.MetadataUserName);
                configurationFile.AppendLine("MetadataPassword|" + FormBase.ConfigurationSettings.MetadataPassword);

                configurationFile.AppendLine("PhysicalModelSSPI|" + FormBase.ConfigurationSettings.PhysicalModelSSPI);
                configurationFile.AppendLine("PhysicalModelNamed|" + FormBase.ConfigurationSettings.PhysicalModelNamed);
                configurationFile.AppendLine("PhysicalModelUserName|" + FormBase.ConfigurationSettings.PhysicalModelUserName);
                configurationFile.AppendLine("PhysicalModelPassword|" + FormBase.ConfigurationSettings.PhysicalModelPassword);


                configurationFile.AppendLine(@"connectionStringSource|" +FormBase.ConfigurationSettings.ConnectionStringSource +"");
                configurationFile.AppendLine(@"connectionStringStaging|" +FormBase.ConfigurationSettings.ConnectionStringStg +"");
                configurationFile.AppendLine(@"connectionStringPersistentStaging|" +FormBase.ConfigurationSettings.ConnectionStringHstg + "");
                configurationFile.AppendLine(@"connectionStringMetadata|" +FormBase.ConfigurationSettings.ConnectionStringOmd +"");
                configurationFile.AppendLine(@"connectionStringIntegration|" +FormBase.ConfigurationSettings.ConnectionStringInt + "");
                configurationFile.AppendLine(@"connectionStringPresentation|" +FormBase.ConfigurationSettings.ConnectionStringPres + "");

                configurationFile.AppendLine("SourceSystemPrefix|" + FormBase.ConfigurationSettings.SourceSystemPrefix +"");
                configurationFile.AppendLine("StagingAreaPrefix|" + FormBase.ConfigurationSettings.StgTablePrefixValue +"");
                configurationFile.AppendLine("PersistentStagingAreaPrefix|" +FormBase.ConfigurationSettings.PsaTablePrefixValue + "");
                configurationFile.AppendLine("HubTablePrefix|" + FormBase.ConfigurationSettings.HubTablePrefixValue + "");
                configurationFile.AppendLine("SatTablePrefix|" + FormBase.ConfigurationSettings.SatTablePrefixValue + "");
                configurationFile.AppendLine("LinkTablePrefix|" + FormBase.ConfigurationSettings.LinkTablePrefixValue +"");
                configurationFile.AppendLine("LinkSatTablePrefix|" + FormBase.ConfigurationSettings.LsatPrefixValue + "");
                configurationFile.AppendLine("KeyIdentifier|" + FormBase.ConfigurationSettings.DwhKeyIdentifier + "");
                configurationFile.AppendLine("SchemaName|" + FormBase.ConfigurationSettings.SchemaName + "");
                configurationFile.AppendLine("RowID|" + FormBase.ConfigurationSettings.RowIdAttribute + "");
                configurationFile.AppendLine("EventDateTimeStamp|" + FormBase.ConfigurationSettings.EventDateTimeAttribute + "");
                configurationFile.AppendLine("LoadDateTimeStamp|" + FormBase.ConfigurationSettings.LoadDateTimeAttribute + "");
                configurationFile.AppendLine("ExpiryDateTimeStamp|" + FormBase.ConfigurationSettings.ExpiryDateTimeAttribute + "");
                configurationFile.AppendLine("ChangeDataIndicator|" + FormBase.ConfigurationSettings.ChangeDataCaptureAttribute +"");
                configurationFile.AppendLine("RecordSourceAttribute|" + FormBase.ConfigurationSettings.RecordSourceAttribute + "");
                configurationFile.AppendLine("ETLProcessID|" + FormBase.ConfigurationSettings.EtlProcessAttribute + "");
                configurationFile.AppendLine("ETLUpdateProcessID|" +FormBase.ConfigurationSettings.EtlProcessUpdateAttribute +"");
                configurationFile.AppendLine("LogicalDeleteAttribute|" +FormBase.ConfigurationSettings.LogicalDeleteAttribute +"");
                configurationFile.AppendLine("TableNamingLocation|" + FormBase.ConfigurationSettings.TableNamingLocation + "");
                configurationFile.AppendLine("KeyNamingLocation|" + FormBase.ConfigurationSettings.KeyNamingLocation +"");
                configurationFile.AppendLine("RecordChecksum|" +FormBase.ConfigurationSettings.RecordChecksumAttribute + "");
                configurationFile.AppendLine("CurrentRecordAttribute|" +FormBase.ConfigurationSettings.CurrentRowAttribute +"");
                configurationFile.AppendLine("AlternativeRecordSource|" +FormBase.ConfigurationSettings.AlternativeRecordSourceAttribute + "");
                configurationFile.AppendLine("AlternativeHubLDTS|" +FormBase.ConfigurationSettings.AlternativeLoadDateTimeAttribute + "");
                configurationFile.AppendLine("AlternativeSatelliteLDTS|" +FormBase.ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute +"");
                configurationFile.AppendLine("AlternativeRecordSourceFunction|" +FormBase.ConfigurationSettings.EnableAlternativeRecordSourceAttribute +"");
                configurationFile.AppendLine("AlternativeHubLDTSFunction|" +FormBase.ConfigurationSettings.EnableAlternativeLoadDateTimeAttribute +"");
                configurationFile.AppendLine("AlternativeSatelliteLDTSFunction|" +FormBase.ConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute +"");
                configurationFile.AppendLine("PSAKeyLocation|" + FormBase.ConfigurationSettings.PsaKeyLocation + "");
                configurationFile.AppendLine("metadataRepositoryType|" +FormBase.ConfigurationSettings.MetadataRepositoryType +"");

                // Closing off
                configurationFile.AppendLine("/* End of file */");

                using (var outfile =
                    new StreamWriter(FormBase.GlobalParameters.ConfigurationPath +
                                     FormBase.GlobalParameters.ConfigfileName + '_' +
                                     FormBase.GlobalParameters.WorkingEnvironment +
                                     FormBase.GlobalParameters.FileExtension))
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

                // These variables are used as global variables throughout the application
                // They will be set once after startup
                FormBase.ConfigurationSettings.ConnectionStringSource = connectionStringSource;
                FormBase.ConfigurationSettings.ConnectionStringStg = connectionStringStg;
                FormBase.ConfigurationSettings.ConnectionStringHstg = connectionStringHstg;
                FormBase.ConfigurationSettings.ConnectionStringInt = connectionStringInt;
                FormBase.ConfigurationSettings.ConnectionStringOmd = connectionStringOmd;
                FormBase.ConfigurationSettings.ConnectionStringPres = connectionStringPres;

                FormBase.ConfigurationSettings.MetadataRepositoryType = configList["metadataRepositoryType"];

                FormBase.ConfigurationSettings.StgTablePrefixValue = configList["StagingAreaPrefix"];
                FormBase.ConfigurationSettings.PsaTablePrefixValue = configList["PersistentStagingAreaPrefix"];
                FormBase.ConfigurationSettings.HubTablePrefixValue = configList["HubTablePrefix"];
                FormBase.ConfigurationSettings.SatTablePrefixValue = configList["SatTablePrefix"];
                FormBase.ConfigurationSettings.LinkTablePrefixValue = configList["LinkTablePrefix"];
                FormBase.ConfigurationSettings.LsatPrefixValue = configList["LinkSatTablePrefix"];
                FormBase.ConfigurationSettings.DwhKeyIdentifier = configList["KeyIdentifier"];
                FormBase.ConfigurationSettings.PsaKeyLocation = configList["PSAKeyLocation"];
                FormBase.ConfigurationSettings.TableNamingLocation = configList["TableNamingLocation"];
                FormBase.ConfigurationSettings.KeyNamingLocation = configList["KeyNamingLocation"];
                FormBase.ConfigurationSettings.SchemaName = configList["SchemaName"];
                FormBase.ConfigurationSettings.SourceSystemPrefix = configList["SourceSystemPrefix"];
                FormBase.ConfigurationSettings.EventDateTimeAttribute = configList["EventDateTimeStamp"];
                FormBase.ConfigurationSettings.LoadDateTimeAttribute = configList["LoadDateTimeStamp"];
                FormBase.ConfigurationSettings.ExpiryDateTimeAttribute = configList["ExpiryDateTimeStamp"];
                FormBase.ConfigurationSettings.ChangeDataCaptureAttribute = configList["ChangeDataIndicator"];
                FormBase.ConfigurationSettings.RecordSourceAttribute = configList["RecordSourceAttribute"];
                FormBase.ConfigurationSettings.EtlProcessAttribute = configList["ETLProcessID"];
                FormBase.ConfigurationSettings.EtlProcessUpdateAttribute = configList["ETLUpdateProcessID"];
                FormBase.ConfigurationSettings.RowIdAttribute = configList["RowID"];
                FormBase.ConfigurationSettings.RecordChecksumAttribute = configList["RecordChecksum"];
                FormBase.ConfigurationSettings.CurrentRowAttribute = configList["CurrentRecordAttribute"];
                FormBase.ConfigurationSettings.LogicalDeleteAttribute = configList["LogicalDeleteAttribute"];
                FormBase.ConfigurationSettings.EnableAlternativeRecordSourceAttribute =configList["AlternativeRecordSourceFunction"];
                FormBase.ConfigurationSettings.AlternativeRecordSourceAttribute = configList["AlternativeRecordSource"];
                FormBase.ConfigurationSettings.EnableAlternativeLoadDateTimeAttribute =configList["AlternativeHubLDTSFunction"];
                FormBase.ConfigurationSettings.AlternativeLoadDateTimeAttribute = configList["AlternativeHubLDTS"];
                FormBase.ConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute =configList["AlternativeSatelliteLDTSFunction"];
                FormBase.ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute =configList["AlternativeSatelliteLDTS"];

                // Databases
                FormBase.ConfigurationSettings.SourceDatabaseName = configList["SourceDatabase"];
                FormBase.ConfigurationSettings.StagingDatabaseName = configList["StagingDatabase"];
                FormBase.ConfigurationSettings.PsaDatabaseName = configList["PersistentStagingDatabase"];
                FormBase.ConfigurationSettings.IntegrationDatabaseName = configList["IntegrationDatabase"];
                FormBase.ConfigurationSettings.PresentationDatabaseName = configList["PresentationDatabase"];
                FormBase.ConfigurationSettings.MetadataDatabaseName = configList["MetadataDatabase"];

                // Servers (instances)
                FormBase.ConfigurationSettings.PhysicalModelServerName = configList["PhysicalModelServerName"];
                FormBase.ConfigurationSettings.MetadataServerName = configList["MetadataServerName"];

                // Authentication & connectivity
                FormBase.ConfigurationSettings.MetadataSSPI = configList["MetadataSSPI"];
                FormBase.ConfigurationSettings.MetadataNamed = configList["MetadataNamed"];
                FormBase.ConfigurationSettings.MetadataUserName = configList["MetadataUserName"];
                FormBase.ConfigurationSettings.MetadataPassword = configList["MetadataPassword"];

                FormBase.ConfigurationSettings.PhysicalModelSSPI = configList["PhysicalModelSSPI"];
                FormBase.ConfigurationSettings.PhysicalModelNamed = configList["PhysicalModelNamed"];
                FormBase.ConfigurationSettings.PhysicalModelUserName = configList["PhysicalModelUserName"];
                FormBase.ConfigurationSettings.PhysicalModelPassword = configList["PhysicalModelPassword"];

                // Paths
                FormBase.GlobalParameters.OutputPath = configList["OutputPath"];
                FormBase.GlobalParameters.ConfigurationPath = configList["ConfigurationPath"];

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
                    if (textline.IndexOf(@"/*", StringComparison.Ordinal) == -1)
                    {
                        var line = textline.Split('|');
                        configList.Add(line[0], line[1]);
                    }
                }

                sr.Close();
                fs.Close();

                FormBase.ValidationSettings.SourceObjectExistence = configList["SourceObjectExistence"];
                FormBase.ValidationSettings.TargetObjectExistence = configList["TargetObjectExistence"];
                FormBase.ValidationSettings.SourceBusinessKeyExistence = configList["BusinessKeyExistence"];

                FormBase.ValidationSettings.LogicalGroup = configList["LogicalGroup"];
                FormBase.ValidationSettings.LinkKeyOrder = configList["LinkKeyOrder"];

                FormBase.ValidationSettings.LinkKeyOrder = configList["BusinessKeySyntax"];
            }
            catch (Exception)
            {

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
                validationFile.AppendLine("SourceObjectExistence|" + FormBase.ValidationSettings.SourceObjectExistence +"");
                validationFile.AppendLine("TargetObjectExistence|" + FormBase.ValidationSettings.TargetObjectExistence +"");
                validationFile.AppendLine("BusinessKeyExistence|" +FormBase.ValidationSettings.SourceBusinessKeyExistence + "");
                validationFile.AppendLine("LogicalGroup|" +FormBase.ValidationSettings.LogicalGroup + "");
                validationFile.AppendLine("LinkKeyOrder|" + FormBase.ValidationSettings.LinkKeyOrder + "");
                validationFile.AppendLine("BusinessKeySyntax|" + FormBase.ValidationSettings.BusinessKeySyntax + "");


                // Closing off
                validationFile.AppendLine("/* End of file */");

                using (var outfile =
                    new StreamWriter(FormBase.GlobalParameters.ConfigurationPath +
                                     FormBase.GlobalParameters.ValidationFileName + '_' +
                                     FormBase.GlobalParameters.WorkingEnvironment +
                                     FormBase.GlobalParameters.FileExtension))
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