using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TEAM
{
    /// <summary>
    ///   The configuration information used to drive variables and make the various configuration settings available in the application
    /// </summary>
    internal class EnvironmentConfiguration
    {
        /// <summary>
        /// Load in to memory (deserialise) a TEAM connection file.
        /// </summary>
        public static void LoadConnectionFile()
        {
            var connectionFileName = 
                                     FormBase.GlobalParameters.ConfigurationPath +
                                     FormBase.GlobalParameters.JsonConnectionFileName + '_' +
                                     FormBase.GlobalParameters.WorkingEnvironment +
                                     FormBase.GlobalParameters.JsonExtension;

            // Load the connections
            try
            {
                // Create a new empty file if it doesn't exist.
                if (!File.Exists(connectionFileName))
                {
                    File.Create(connectionFileName).Close();

                    // Generate the sample connection dictionary and commit to memory.
                    CreateDummyConnectionDictionary();

                    // There was no key in the file for this connection, so it's new.
                    var list = new List<TeamConnectionProfile>();

                    foreach (var connection in FormBase.ConfigurationSettings.connectionDictionary)
                    {
                        list.Add(connection.Value);
                    }

                    string output = JsonConvert.SerializeObject(list.ToArray(), Formatting.Indented);
                    File.WriteAllText(connectionFileName, output);
                }
                else
                {
                    // Clear the in-memory dictionary and load the file.
                    FormBase.ConfigurationSettings.connectionDictionary.Clear();
                    TeamConnectionProfile[] connectionJson = JsonConvert.DeserializeObject<TeamConnectionProfile[]>(File.ReadAllText(connectionFileName));

                    foreach (var connection in connectionJson)
                    {
                        FormBase.ConfigurationSettings.connectionDictionary.Add(connection.connectionInternalId, connection);
                    }
                }

                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The connections file {connectionFileName} was loaded successfully."));
            }
            catch
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered loading the connections file {connectionFileName}."));
            }
        }

        /// <summary>
        /// Create the paths in the TEAM application (configuration, output and backup).
        /// </summary>
        internal static void InitialiseEnvironmentPaths()
        {
            CreateConfigurationPath();
            CreateOutputPath();
            CreateBackupPath();
        }

        internal static void CreateConfigurationPath()
        {
            try
            {
                InitialiseRootPath(FormBase.GlobalParameters.ConfigurationPath);
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information,
                    $"The TEAM directory {FormBase.GlobalParameters.ConfigurationPath} is available."));
            }
            catch
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    "The directories required to operate TEAM are not available and can not be created. Do you have administrative privileges in the installation directory to create these additional directories?"));
            }
        }

        internal static void CreateOutputPath()
        {
            try
            {
                InitialiseRootPath(FormBase.GlobalParameters.OutputPath);
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information,
                    $"The TEAM directory {FormBase.GlobalParameters.OutputPath} is available."));
            }
            catch
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    "The directories required to operate TEAM are not available and can not be created. Do you have administrative privileges in the installation directory to create these additional directories?"));
            }
        }

        internal static void CreateBackupPath()
        {
            try
            {
                InitialiseRootPath(FormBase.GlobalParameters.BackupPath);
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The TEAM directory {FormBase.GlobalParameters.BackupPath} is available."));
            }
            catch
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    "The directories required to operate TEAM are not available and can not be created. Do you have administrative privileges in the installation directory to create these additional directories?"));
            }
        }

        /// <summary>
        /// Create the sample / start dictionary of connections and commit to memory (global parameter connection dictionary).
        /// </summary>
        internal static void CreateDummyConnectionDictionary()
        {
            var localDictionary = new Dictionary<string, TeamConnectionProfile>();

            // Metadata
            var newTeamConnectionProfileMetadata = new TeamConnectionProfile
            {
                connectionInternalId = "Metadata",
                databaseConnectionKey = "Metadata",
                databaseConnectionName = "Metadata Repository",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionMetadata = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "900_Metadata",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfileMetadata.databaseServer = newTeamDatabaseConnectionMetadata;

            // Source
            var newTeamConnectionProfileSource = new TeamConnectionProfile
            {
                connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Source" }, "%$@"),
                databaseConnectionKey = "Source",
                databaseConnectionName = "Source System",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionSource = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "000_Source",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfileSource.databaseServer = newTeamDatabaseConnectionSource;

            // Staging
            var newTeamConnectionProfileStaging = new TeamConnectionProfile
            {
                connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Staging" }, "%$@"),
                databaseConnectionKey = "Staging",
                databaseConnectionName = "Staging / Landing Area",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionStaging = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "100_Staging_Area",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfileStaging.databaseServer = newTeamDatabaseConnectionStaging;

            // PSA
            var newTeamConnectionProfilePsa = new TeamConnectionProfile
            {
                connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "PersistentStagingArea" }, "%$@"),
                databaseConnectionKey = "PSA",
                databaseConnectionName = "Persistent Staging Area",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionPsa = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "150_Persistent_Staging_Area",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfilePsa.databaseServer = newTeamDatabaseConnectionPsa;

            // Integration
            var newTeamConnectionProfileIntegration = new TeamConnectionProfile
            {
                connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Integration" }, "%$@"),
                databaseConnectionKey = "Integration",
                databaseConnectionName = "Integration Layer",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionIntegration= new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "200_Integration_Layer",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfileIntegration.databaseServer = newTeamDatabaseConnectionIntegration;

            // Presentation
            var newTeamConnectionProfilePresentation = new TeamConnectionProfile
            {
                connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Presentation" }, "%$@"),
                databaseConnectionKey = "Presentation",
                databaseConnectionName = "Presentation Layer",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionPresentation = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "300_Presentation_Layer",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfilePresentation.databaseServer = newTeamDatabaseConnectionPresentation;

            // Compile the dictionary
            localDictionary.Add(newTeamConnectionProfileMetadata.connectionInternalId, newTeamConnectionProfileMetadata);
            localDictionary.Add(newTeamConnectionProfileSource.connectionInternalId, newTeamConnectionProfileSource);
            localDictionary.Add(newTeamConnectionProfileStaging.connectionInternalId, newTeamConnectionProfileStaging);
            localDictionary.Add(newTeamConnectionProfilePsa.connectionInternalId, newTeamConnectionProfilePsa);
            localDictionary.Add(newTeamConnectionProfileIntegration.databaseConnectionNotes, newTeamConnectionProfileIntegration);
            localDictionary.Add(newTeamConnectionProfilePresentation.connectionInternalId, newTeamConnectionProfilePresentation);

            // Commit to memory.
            FormBase.ConfigurationSettings.connectionDictionary = localDictionary;
        }

        /// <summary>
        /// Method to create a new configuration file with default values at the default location.
        /// Checks if the file already exists. If it does, nothing will happen.
        /// </summary>
        internal static void CreateDummyEnvironmentConfigurationFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                // Create a completely new file
                var initialConfigurationFile = new StringBuilder();

                initialConfigurationFile.AppendLine("/* TEAM Configuration Settings */");

                initialConfigurationFile.AppendLine("MetadataConnectionId|Metadata");

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
                initialConfigurationFile.AppendLine("metadataRepositoryType|Json");

                initialConfigurationFile.AppendLine("/* End of file */");

                using (var outfile = new StreamWriter(fileName))
                {
                    outfile.Write(initialConfigurationFile.ToString());
                    outfile.Close();
                }
            }
        }

        internal void CopyExistingFile()
        {
            try
            {
                var sourceFilePathName = FormBase.GlobalParameters.ConfigurationPath +
                                         FormBase.GlobalParameters.ConfigFileName + '_' + "Development" +
                                         FormBase.GlobalParameters.FileExtension;

                if (File.Exists(sourceFilePathName))
                {
                    var targetFilePathName = FormBase.GlobalParameters.ConfigurationPath +
                                             FormBase.GlobalParameters.ConfigFileName + '_' + "Production" +
                                             FormBase.GlobalParameters.FileExtension;

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
                MessageBox.Show(
                    "An error has occured during the creating of the production settings file. The error message is " +
                    ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Method to create a new validation file with default values at the default location
        /// Checks if the file already exists. If it does, nothing will happen.
        /// </summary>
        internal static void CreateDummyValidationFile(string fileName)
        {
            if (!File.Exists(fileName))
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

                using (var outfile = new StreamWriter(fileName))
                {
                    outfile.Write(validationFile.ToString());
                    outfile.Close();
                }

                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information,
                    $"A new configuration file was created for {fileName}."));

            }
            else
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information,
                    $"The existing configuration file {fileName} was detected."));
            }
        }


        /// <summary>
        ///    Create a file backup for the configuration file at the provided location
        /// </summary>
        internal static void CreateFileBackup(string fileName, string filePath = "")
        {
            var localFileName = Path.GetFileName(fileName);

            // Manage that the backup path can be defaulted or derived.
            if (filePath == "")
            {
                filePath = FormBase.GlobalParameters.BackupPath;
            }
            else
            {
                filePath = Path.GetDirectoryName(fileName);
            }

            try
            {
                if (File.Exists(fileName))
                {
                    var targetFilePathName = filePath + string.Concat("Backup_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_", localFileName);

                    if (fileName != null)
                    {
                        File.Copy(fileName, targetFilePathName);
                    }
                    else
                    {
                        FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The file cannot be backed up because it cannot be identified."));
                    }
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
        /// Check if the path exists and create it if necessary.
        /// Is often used for both the Configuration Path and Output Path - both being essential TEAM paths.
        /// </summary>
        internal static void InitialiseRootPath(string inputPath)
        {
            // Create the configuration directory if it does not exist yet
            try
            {
                if (!Directory.Exists(inputPath))
                {
                    Directory.CreateDirectory(inputPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error creation default directory at " + inputPath +
                    " the message is " + ex, "An issue has been encountered", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Retrieve the values of the application root path (where the paths to the configuration file is maintained).
        /// This is the hardcoded base path that always needs to be accessible, it has the main file which can locate the rest of the configuration.
        /// </summary>
        public static void LoadRootPathFile(string fileName, string configurationPath, string outputPath)
        {
            // Create root path file, with dummy values if it doesn't exist already
            try
            {
                if (!File.Exists(fileName))
                {
                    var initialConfigurationFile = new StringBuilder();

                    initialConfigurationFile.AppendLine("/* TEAM File Path Settings */");
                    initialConfigurationFile.AppendLine("ConfigurationPath|" + configurationPath);
                    initialConfigurationFile.AppendLine("OutputPath|" + outputPath);
                    initialConfigurationFile.AppendLine("WorkingEnvironment|Development");
                    initialConfigurationFile.AppendLine("/* End of file */");

                    using (var outfile = new StreamWriter(fileName))
                    {
                        outfile.Write(initialConfigurationFile.ToString());
                        outfile.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while creation the default path file. The error message is " + ex, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            
            var configList = new Dictionary<string, string>();
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
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
        /// Load a TEAM environment file into memory.
        /// </summary>
        /// <param name="fileName"></param>
        public static void LoadEnvironmentFile(string fileName)
        {
            // Create a new file if it doesn't exist.
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();

                // There was no key in the file for this environment, so it's new.
                // Create two initial environments, development and production.
                var list = new List<TeamWorkingEnvironment>();

                var developmentEnvironment = new TeamWorkingEnvironment
                {
                    environmentInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Development" }, "%$@"),
                    environmentKey = "Development",
                    environmentName = "Development environment",
                    environmentNotes = "Environment created as initial / starter environment."
                };

                list.Add(developmentEnvironment);

                var productionEnvironment = new TeamWorkingEnvironment
                {
                    environmentInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Production" }, "%$@"),
                    environmentKey = "Production",
                    environmentName = "Production environment",
                    environmentNotes = "Environment created as initial / starter environment."
                };

                list.Add(productionEnvironment);

                string output = JsonConvert.SerializeObject(list.ToArray(), Formatting.Indented);
                File.WriteAllText(fileName, output);

                // Commit to memory also.
                var localDictionary = new Dictionary<string, TeamWorkingEnvironment>();

                localDictionary.Add(developmentEnvironment.environmentInternalId, developmentEnvironment);
                localDictionary.Add(productionEnvironment.environmentInternalId, productionEnvironment);

                FormBase.ConfigurationSettings.environmentDictionary = localDictionary;
            }
            else
            {
                FormBase.ConfigurationSettings.environmentDictionary.Clear();
                TeamWorkingEnvironment[] environmentJson = JsonConvert.DeserializeObject<TeamWorkingEnvironment[]>(File.ReadAllText(fileName));

                foreach (var environment in environmentJson)
                {
                    FormBase.ConfigurationSettings.environmentDictionary.Add(environment.environmentInternalId, environment);
                }
            }
        }

        /// <summary>
        /// Retrieve the configuration information from memory and save this to disk
        /// </summary>
        internal static void SaveConfigurationFile()
        {
            try
            {
                var configurationFile = new StringBuilder();
                configurationFile.AppendLine("/* TEAM Configuration Settings */");
                configurationFile.AppendLine("/* Saved at " + DateTime.Now + " */");

                configurationFile.AppendLine("MetadataConnectionId|" + FormBase.ConfigurationSettings.MetadataConnection.connectionInternalId + "");

                configurationFile.AppendLine("StagingAreaPrefix|" + FormBase.ConfigurationSettings.StgTablePrefixValue +"");
                configurationFile.AppendLine("PersistentStagingAreaPrefix|" +FormBase.ConfigurationSettings.PsaTablePrefixValue + "");
                configurationFile.AppendLine("HubTablePrefix|" + FormBase.ConfigurationSettings.HubTablePrefixValue + "");
                configurationFile.AppendLine("SatTablePrefix|" + FormBase.ConfigurationSettings.SatTablePrefixValue + "");
                configurationFile.AppendLine("LinkTablePrefix|" + FormBase.ConfigurationSettings.LinkTablePrefixValue +"");
                configurationFile.AppendLine("LinkSatTablePrefix|" + FormBase.ConfigurationSettings.LsatTablePrefixValue + "");
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
                                     FormBase.GlobalParameters.ConfigFileName + '_' +
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
        /// Retrieve the configuration information from disk and save this to memory.
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

                FormBase.ConfigurationSettings.MetadataRepositoryType = configList["metadataRepositoryType"] == "SqlServer" ? FormBase.MetadataRepositoryStorageType.SqlServer : FormBase.MetadataRepositoryStorageType.Json;

                FormBase.ConfigurationSettings.StgTablePrefixValue = configList["StagingAreaPrefix"];
                FormBase.ConfigurationSettings.PsaTablePrefixValue = configList["PersistentStagingAreaPrefix"];
                FormBase.ConfigurationSettings.HubTablePrefixValue = configList["HubTablePrefix"];
                FormBase.ConfigurationSettings.SatTablePrefixValue = configList["SatTablePrefix"];
                FormBase.ConfigurationSettings.LinkTablePrefixValue = configList["LinkTablePrefix"];
                FormBase.ConfigurationSettings.LsatTablePrefixValue = configList["LinkSatTablePrefix"];
                FormBase.ConfigurationSettings.DwhKeyIdentifier = configList["KeyIdentifier"];
                FormBase.ConfigurationSettings.PsaKeyLocation = configList["PSAKeyLocation"];
                FormBase.ConfigurationSettings.TableNamingLocation = configList["TableNamingLocation"];
                FormBase.ConfigurationSettings.KeyNamingLocation = configList["KeyNamingLocation"];
                FormBase.ConfigurationSettings.SchemaName = configList["SchemaName"];
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
                FormBase.ConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute = configList["AlternativeSatelliteLDTSFunction"];
                FormBase.ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute = configList["AlternativeSatelliteLDTS"];

                // Databases
                if (configList["MetadataConnectionId"] != null)
                {
                    FormBase.ConfigurationSettings.MetadataConnection = FormBase.ConfigurationSettings.connectionDictionary[configList["MetadataConnectionId"]];
                }
                else
                {
                    FormBase.ConfigurationSettings.MetadataConnection = null;
                }

            }
            catch (Exception ex)
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered while interpreting the connections file {filename}. The issue is {ex.Message}"));
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

                FormBase.ValidationSettings.SourceObjectExistence = configList["SourceObjectExistence"];
                FormBase.ValidationSettings.TargetObjectExistence = configList["TargetObjectExistence"];
                FormBase.ValidationSettings.SourceBusinessKeyExistence = configList["BusinessKeyExistence"];
                FormBase.ValidationSettings.SourceAttributeExistence = configList["SourceAttributeExistence"];
                FormBase.ValidationSettings.TargetAttributeExistence = configList["TargetAttributeExistence"];

                FormBase.ValidationSettings.LogicalGroup = configList["LogicalGroup"];
                FormBase.ValidationSettings.LinkKeyOrder = configList["LinkKeyOrder"];

                FormBase.ValidationSettings.BusinessKeySyntax = configList["BusinessKeySyntax"];
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
                validationFile.AppendLine("SourceObjectExistence|" + FormBase.ValidationSettings.SourceObjectExistence +"");
                validationFile.AppendLine("TargetObjectExistence|" + FormBase.ValidationSettings.TargetObjectExistence +"");
                validationFile.AppendLine("BusinessKeyExistence|" +FormBase.ValidationSettings.SourceBusinessKeyExistence + "");
                validationFile.AppendLine("SourceAttributeExistence|" + FormBase.ValidationSettings.SourceAttributeExistence + "");
                validationFile.AppendLine("TargetAttributeExistence|" + FormBase.ValidationSettings.TargetAttributeExistence + "");
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


    // Delegate to pass through a string (for example to update text boxes in a delegate function).
    public class MyStringEventArgs : EventArgs
    {
        public string Value { get; set; }

        public MyStringEventArgs(string value)
        {
            Value = value;
        }
    }

    // Delegate to pass through a TEAM working environment.
    public class MyWorkingEnvironmentEventArgs : EventArgs
    {
        public TeamWorkingEnvironment Value { get; set; }

        public MyWorkingEnvironmentEventArgs(TeamWorkingEnvironment value)
        {
            Value = value;
        }
    }

    // Delegate to pass through a TEAM connection.
    public class MyConnectionProfileEventArgs : EventArgs
    {
        public TeamConnectionProfile Value { get; set; }

        public MyConnectionProfileEventArgs(TeamConnectionProfile value)
        {
            Value = value;
        }
    }
}