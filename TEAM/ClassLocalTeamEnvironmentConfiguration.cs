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
    internal class LocalTeamEnvironmentConfiguration
    {
        /// <summary>
        /// Create the paths in the TEAM application (configuration, output and backup).
        /// </summary>
        internal static void InitialiseEnvironmentPaths()
        {
            CreateConfigurationPath();
            CreateOutputPath();
            CreateBackupPath();
            CreateCorePath();
        }

        internal static void CreateConfigurationPath()
        {
            try
            {
                FileHandling.InitialisePath(FormBase.GlobalParameters.ConfigurationPath);

                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The TEAM directory {FormBase.GlobalParameters.ConfigurationPath} is available."));
            }
            catch
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, "The directories required to operate TEAM are not available and can not be created. Do you have administrative privileges in the installation directory to create these additional directories?"));
            }
        }

        internal static void CreateOutputPath()
        {
            try
            {
                FileHandling.InitialisePath(FormBase.GlobalParameters.OutputPath);
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information,
                    $"The TEAM directory {FormBase.GlobalParameters.OutputPath} is available."));
            }
            catch
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    "The directories required to operate TEAM are not available and can not be created. Do you have administrative privileges in the installation directory to create these additional directories?"));
            }
        }

        internal static void CreateCorePath()
        {
            try
            {
                FileHandling.InitialisePath(FormBase.GlobalParameters.CorePath);
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information,
                    $"The TEAM directory {FormBase.GlobalParameters.CorePath} is available."));
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
                FileHandling.InitialisePath(FormBase.GlobalParameters.BackupPath);
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The TEAM directory {FormBase.GlobalParameters.BackupPath} is available."));
            }
            catch
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                    "The directories required to operate TEAM are not available and can not be created. Do you have administrative privileges in the installation directory to create these additional directories?"));
            }
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

                initialConfigurationFile.AppendLine("MetadataConnectionId|MetadataConnectionInternalId");

                initialConfigurationFile.AppendLine("StagingAreaPrefix|STG");
                initialConfigurationFile.AppendLine("PersistentStagingAreaPrefix|PSA");
                initialConfigurationFile.AppendLine("PresentationLayerLabels|DIM, FACT");
                initialConfigurationFile.AppendLine("TransformationLabels|BDV");
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
        /// Method to create a new validation file with default values at the default location
        /// Checks if the file already exists. If it does, nothing will happen.
        /// </summary>
        internal static void CreateDummyJsonExtractConfigurationFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                var validationFile = new StringBuilder();

                validationFile.AppendLine("/* TEAM Json Export File Settings */");

                validationFile.AppendLine("GenerateSourceDataItemTypes|True");
                validationFile.AppendLine("GenerateTargetDataItemTypes|True");
                validationFile.AppendLine("GenerateSourceDataObjectConnection|True");
                validationFile.AppendLine("GenerateTargetDataObjectConnection|True");

                validationFile.AppendLine("/* End of file */");

                using (var outfile = new StreamWriter(fileName))
                {
                    outfile.Write(validationFile.ToString());
                    outfile.Close();
                }

                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information,
                    $"A new Json extract configuration file was created for {fileName}."));

            }
            else
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information,
                    $"The existing Json extract configuration file {fileName} was detected."));
            }
        }

        /// <summary>
        /// Retrieve the values of the application root path (where the paths to the configuration file is maintained).
        /// This is the hardcoded base path that always needs to be accessible, it has the main file which can locate the rest of the configuration.
        /// </summary
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
        /// Retrieve the configuration information from memory and save this to disk.
        /// </summary>
        internal static void SaveConfigurationFile()
        {
            try
            {
                var configurationFile = new StringBuilder();
                configurationFile.AppendLine("/* TEAM Configuration Settings */");
                configurationFile.AppendLine("/* Saved at " + DateTime.Now + " */");

                configurationFile.AppendLine("MetadataConnectionId|" + FormBase.TeamConfigurationSettings.MetadataConnection.ConnectionInternalId + "");

                configurationFile.AppendLine("StagingAreaPrefix|" + FormBase.TeamConfigurationSettings.StgTablePrefixValue +"");
                configurationFile.AppendLine("PersistentStagingAreaPrefix|" +FormBase.TeamConfigurationSettings.PsaTablePrefixValue + "");
                configurationFile.AppendLine("PresentationLayerLabels|" + FormBase.TeamConfigurationSettings.PresentationLayerLabels + "");
                configurationFile.AppendLine("TransformationLabels|" + FormBase.TeamConfigurationSettings.TransformationLabels + "");
                configurationFile.AppendLine("HubTablePrefix|" + FormBase.TeamConfigurationSettings.HubTablePrefixValue + "");
                configurationFile.AppendLine("SatTablePrefix|" + FormBase.TeamConfigurationSettings.SatTablePrefixValue + "");
                configurationFile.AppendLine("LinkTablePrefix|" + FormBase.TeamConfigurationSettings.LinkTablePrefixValue +"");
                configurationFile.AppendLine("LinkSatTablePrefix|" + FormBase.TeamConfigurationSettings.LsatTablePrefixValue + "");
                configurationFile.AppendLine("KeyIdentifier|" + FormBase.TeamConfigurationSettings.DwhKeyIdentifier + "");
                configurationFile.AppendLine("SchemaName|" + FormBase.TeamConfigurationSettings.SchemaName + "");
                configurationFile.AppendLine("RowID|" + FormBase.TeamConfigurationSettings.RowIdAttribute + "");
                configurationFile.AppendLine("EventDateTimeStamp|" + FormBase.TeamConfigurationSettings.EventDateTimeAttribute + "");
                configurationFile.AppendLine("LoadDateTimeStamp|" + FormBase.TeamConfigurationSettings.LoadDateTimeAttribute + "");
                configurationFile.AppendLine("ExpiryDateTimeStamp|" + FormBase.TeamConfigurationSettings.ExpiryDateTimeAttribute + "");
                configurationFile.AppendLine("ChangeDataIndicator|" + FormBase.TeamConfigurationSettings.ChangeDataCaptureAttribute +"");
                configurationFile.AppendLine("RecordSourceAttribute|" + FormBase.TeamConfigurationSettings.RecordSourceAttribute + "");
                configurationFile.AppendLine("ETLProcessID|" + FormBase.TeamConfigurationSettings.EtlProcessAttribute + "");
                configurationFile.AppendLine("ETLUpdateProcessID|" +FormBase.TeamConfigurationSettings.EtlProcessUpdateAttribute +"");
                configurationFile.AppendLine("LogicalDeleteAttribute|" +FormBase.TeamConfigurationSettings.LogicalDeleteAttribute +"");
                configurationFile.AppendLine("TableNamingLocation|" + FormBase.TeamConfigurationSettings.TableNamingLocation + "");
                configurationFile.AppendLine("KeyNamingLocation|" + FormBase.TeamConfigurationSettings.KeyNamingLocation +"");
                configurationFile.AppendLine("RecordChecksum|" +FormBase.TeamConfigurationSettings.RecordChecksumAttribute + "");
                configurationFile.AppendLine("CurrentRecordAttribute|" +FormBase.TeamConfigurationSettings.CurrentRowAttribute +"");
                configurationFile.AppendLine("AlternativeRecordSource|" +FormBase.TeamConfigurationSettings.AlternativeRecordSourceAttribute + "");
                configurationFile.AppendLine("AlternativeHubLDTS|" +FormBase.TeamConfigurationSettings.AlternativeLoadDateTimeAttribute + "");
                configurationFile.AppendLine("AlternativeSatelliteLDTS|" +FormBase.TeamConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute +"");
                configurationFile.AppendLine("AlternativeRecordSourceFunction|" +FormBase.TeamConfigurationSettings.EnableAlternativeRecordSourceAttribute +"");
                configurationFile.AppendLine("AlternativeHubLDTSFunction|" +FormBase.TeamConfigurationSettings.EnableAlternativeLoadDateTimeAttribute +"");
                configurationFile.AppendLine("AlternativeSatelliteLDTSFunction|" +FormBase.TeamConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute +"");
                configurationFile.AppendLine("PSAKeyLocation|" + FormBase.TeamConfigurationSettings.PsaKeyLocation + "");
                configurationFile.AppendLine("metadataRepositoryType|" +FormBase.TeamConfigurationSettings.MetadataRepositoryType +"");

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
        /// Retrieve the validation information from disk and save this to memory.
        /// </summary>
        /// <param name="filename"></param>
        internal static void LoadJsonConfigurationFile(string filename)
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

                FormBase.JsonExportSettings.GenerateSourceDataItemTypes = configList["GenerateSourceDataItemTypes"];
                FormBase.JsonExportSettings.GenerateTargetDataItemTypes = configList["GenerateTargetDataItemTypes"];
                FormBase.JsonExportSettings.GenerateSourceDataObjectConnection = configList["GenerateSourceDataObjectConnection"];
                FormBase.JsonExportSettings.GenerateTargetDataObjectConnection = configList["GenerateTargetDataObjectConnection"];

            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        internal static void SaveJsonConfigurationFile()
        {
            try
            {
                // Creating the file
                var validationFile = new StringBuilder();
                validationFile.AppendLine("/* TEAM Json Export Configuration Settings */");
                validationFile.AppendLine("/* Saved at " + DateTime.Now + " */");
                validationFile.AppendLine("GenerateSourceDataItemTypes|" + FormBase.JsonExportSettings.GenerateSourceDataItemTypes + "");
                validationFile.AppendLine("GenerateTargetDataItemTypes|" + FormBase.JsonExportSettings.GenerateTargetDataItemTypes + "");
                validationFile.AppendLine("GenerateSourceDataObjectConnection|" + FormBase.JsonExportSettings.GenerateSourceDataObjectConnection + "");
                validationFile.AppendLine("GenerateTargetDataObjectConnection|" + FormBase.JsonExportSettings.GenerateTargetDataObjectConnection + "");

                // Closing off
                validationFile.AppendLine("/* End of file */");

                using (var outfile =
                    new StreamWriter(FormBase.GlobalParameters.ConfigurationPath +
                                     FormBase.GlobalParameters.JsonExportConfigurationFileName + '_' +
                                     FormBase.GlobalParameters.WorkingEnvironment +
                                     FormBase.GlobalParameters.FileExtension))
                {
                    outfile.Write(validationFile.ToString());
                    outfile.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured saving the Json configuration file. The error message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Retrieve the validation information from disk and save this to memory.
        /// </summary>
        /// <param name="filename"></param>
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
        /// Retrieve the validation information from memory and save this to disk.
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
        public TeamConnection Value { get; set; }

        public MyConnectionProfileEventArgs(TeamConnection value)
        {
            Value = value;
        }
    }
}