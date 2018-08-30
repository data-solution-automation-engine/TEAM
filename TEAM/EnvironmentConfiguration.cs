using System;
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
        internal void CreateNewEnvironmentConfiguration(string filename)
        {
            var initialConfigurationFile = new StringBuilder();

            initialConfigurationFile.AppendLine("/* TEAM Configuration Settings */");
            initialConfigurationFile.AppendLine("/* Roelant Vos - 2018 */");
            initialConfigurationFile.AppendLine("SourceDatabase|Source_Database");
            initialConfigurationFile.AppendLine("StagingDatabase|Staging_Area_Database");
            initialConfigurationFile.AppendLine("PersistentStagingDatabase|Persistent_Staging_Area_Database");
            initialConfigurationFile.AppendLine("IntegrationDatabase|Data_Vault_Database");
            initialConfigurationFile.AppendLine("PresentationDatabase|Presentation_Database");
            initialConfigurationFile.AppendLine("OutputPath|" + FormBase.GlobalParameters.OutputPath);
            initialConfigurationFile.AppendLine("ConfigurationPath|" + FormBase.GlobalParameters.ConfigurationPath);
            initialConfigurationFile.AppendLine(
                @"connectionStringSource|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Source_Database>;user id=sa; password=<>");
            initialConfigurationFile.AppendLine(
                @"connectionStringStaging|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Staging_Area>;user id=sa; password=<>");
            initialConfigurationFile.AppendLine(
                @"connectionStringPersistentStaging|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Persistent_Staging_Area>;user id=sa; password=<>");
            initialConfigurationFile.AppendLine(
                @"connectionStringMetadata|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Metadata>;user id=sa; password=<>");
            initialConfigurationFile.AppendLine(
                @"connectionStringIntegration|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Data_Vault>;user id=sa; password=<>");
            initialConfigurationFile.AppendLine(
                @"connectionStringPresentation|Provider=SQLNCLI11;Server=<>;Initial Catalog=<Presentation>;user id=sa; password=<>");
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
            initialConfigurationFile.AppendLine("LinkedServerName|");
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
        }

        /// <summary>
        ///    Create a file backup for the configuration file at the provided location
        /// </summary>
        internal static void CreateEnvironmentConfigurationBackupFile()
        {
            // Check if the paths are available, just to be sure
            InitialisePath();

            // Retrieve the application parameters from memory
            var configurationSettings = new FormBase.ConfigurationSettings();

            try
            {
                if (File.Exists(configurationSettings.ConfigurationPath + FormBase.GlobalParameters.ConfigfileName))
                {
                    var targetFilePathName = configurationSettings.ConfigurationPath +
                                             string.Concat("Backup_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_",
                                                 FormBase.GlobalParameters.ConfigfileName);

                    File.Copy(configurationSettings.ConfigurationPath + FormBase.GlobalParameters.ConfigfileName,
                        targetFilePathName);

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
        internal static void InitialisePath()
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
                MessageBox.Show("Error creation default directory at " + configurationPath + " the message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Error creation default directory at " + outputPath + " the message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                // Create a default configuration file if the file does not exist as expected
                if (File.Exists(FormBase.GlobalParameters.ConfigurationPath +
                                FormBase.GlobalParameters.ConfigfileName)) return;
                var newEnvironmentConfiguration = new EnvironmentConfiguration();
                newEnvironmentConfiguration.CreateNewEnvironmentConfiguration(
                    FormBase.GlobalParameters.ConfigurationPath + FormBase.GlobalParameters.ConfigfileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while creation the default Configuration File. The error message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        ///    Retrieve the configuration information from memory and save this to disk
        /// </summary>
        internal static void SaveEnvironmentConfiguration()
        {
            try
            {
                var configurationSettings = new FormBase.ConfigurationSettings();

                var configurationFile = new StringBuilder();
                configurationFile.AppendLine("/* TEAM Configuration Settings */");
                configurationFile.AppendLine("/* Saved at " + DateTime.Now + " */");
                configurationFile.AppendLine("SourceDatabase|" + configurationSettings.SourceDatabaseName + "");
                configurationFile.AppendLine("StagingDatabase|" + configurationSettings.StagingDatabaseName + "");
                configurationFile.AppendLine("PersistentStagingDatabase|" + configurationSettings.PsaDatabaseName + "");
                configurationFile.AppendLine(
                    "IntegrationDatabase|" + configurationSettings.IntegrationDatabaseName + "");
                configurationFile.AppendLine("PresentationDatabase|" + configurationSettings.PresentationDatabaseName +
                                             "");
                configurationFile.AppendLine("OutputPath|" + configurationSettings.OutputPath + "");
                configurationFile.AppendLine("ConfigurationPath|" + configurationSettings.ConfigurationPath + "");
                configurationFile.AppendLine(@"connectionStringSource|" + configurationSettings.ConnectionStringSource +
                                             "");
                configurationFile.AppendLine(@"connectionStringStaging|" + configurationSettings.ConnectionStringStg +
                                             "");
                configurationFile.AppendLine(@"connectionStringPersistentStaging|" +
                                             configurationSettings.ConnectionStringHstg + "");
                configurationFile.AppendLine(@"connectionStringMetadata|" + configurationSettings.ConnectionStringOmd +
                                             "");
                configurationFile.AppendLine(@"connectionStringIntegration|" +
                                             configurationSettings.ConnectionStringInt + "");
                configurationFile.AppendLine(@"connectionStringPresentation|" +
                                             configurationSettings.ConnectionStringPres + "");
                configurationFile.AppendLine("SourceSystemPrefix|" + configurationSettings.SourceSystemPrefix + "");
                configurationFile.AppendLine("StagingAreaPrefix|" + configurationSettings.StgTablePrefixValue + "");
                configurationFile.AppendLine("PersistentStagingAreaPrefix|" +
                                             configurationSettings.PsaTablePrefixValue + "");
                configurationFile.AppendLine("HubTablePrefix|" + configurationSettings.HubTablePrefixValue + "");
                configurationFile.AppendLine("SatTablePrefix|" + configurationSettings.SatTablePrefixValue + "");
                configurationFile.AppendLine("LinkTablePrefix|" + configurationSettings.LinkTablePrefixValue + "");
                configurationFile.AppendLine("LinkSatTablePrefix|" + configurationSettings.LsatPrefixValue + "");
                configurationFile.AppendLine("KeyIdentifier|" + configurationSettings.DwhKeyIdentifier + "");
                configurationFile.AppendLine("SchemaName|" + configurationSettings.SchemaName + "");
                configurationFile.AppendLine("RowID|" + configurationSettings.RowIdAttribute + "");
                configurationFile.AppendLine("EventDateTimeStamp|" + configurationSettings.EventDateTimeAttribute + "");
                configurationFile.AppendLine("LoadDateTimeStamp|" + configurationSettings.LoadDateTimeAttribute + "");
                configurationFile.AppendLine(
                    "ExpiryDateTimeStamp|" + configurationSettings.ExpiryDateTimeAttribute + "");
                configurationFile.AppendLine("ChangeDataIndicator|" + configurationSettings.ChangeDataCaptureAttribute +
                                             "");
                configurationFile.AppendLine(
                    "RecordSourceAttribute|" + configurationSettings.RecordSourceAttribute + "");
                configurationFile.AppendLine("ETLProcessID|" + configurationSettings.EtlProcessAttribute + "");
                configurationFile.AppendLine("ETLUpdateProcessID|" + configurationSettings.EtlProcessUpdateAttribute +
                                             "");
                configurationFile.AppendLine("LogicalDeleteAttribute|" + configurationSettings.LogicalDeleteAttribute +
                                             "");
                configurationFile.AppendLine("LinkedServerName|" + configurationSettings.LinkedServer + "");
                configurationFile.AppendLine("TableNamingLocation|" + configurationSettings.TableNamingLocation + "");
                configurationFile.AppendLine("KeyNamingLocation|" + configurationSettings.KeyNamingLocation + "");


                configurationFile.AppendLine("RecordChecksum|" + configurationSettings.RecordChecksumAttribute + "");
                configurationFile.AppendLine("CurrentRecordAttribute|" + configurationSettings.CurrentRowAttribute +
                                             "");

                configurationFile.AppendLine("AlternativeRecordSource|" +
                                             configurationSettings.AlternativeRecordSourceAttribute + "");
                configurationFile.AppendLine("AlternativeHubLDTS|" +
                                             configurationSettings.AlternativeLoadDateTimeAttribute + "");
                configurationFile.AppendLine("AlternativeSatelliteLDTS|" +
                                             configurationSettings.AlternativeSatelliteLoadDateTimeAttribute + "");
                configurationFile.AppendLine("AlternativeRecordSourceFunction|" +
                                             configurationSettings.EnableAlternativeRecordSourceAttribute + "");
                configurationFile.AppendLine("AlternativeHubLDTSFunction|" +
                                             configurationSettings.EnableAlternativeLoadDateTimeAttribute + "");
                configurationFile.AppendLine("AlternativeSatelliteLDTSFunction|" +
                                             configurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute +
                                             "");

                configurationFile.AppendLine("PSAKeyLocation|" + configurationSettings.PsaKeyLocation + "");
                configurationFile.AppendLine("metadataRepositoryType|" + configurationSettings.metadataRepositoryType +
                                             "");

                // Closing off
                configurationFile.AppendLine("/* End of file */");

                using (var outfile =
                    new StreamWriter(configurationSettings.ConfigurationPath + FormBase.GlobalParameters.ConfigfileName))
                {
                    outfile.Write(configurationFile.ToString());
                    outfile.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured saving the Configuration File. The error message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}