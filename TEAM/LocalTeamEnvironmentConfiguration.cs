using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TEAM_Library;

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
            FileHandling.InitialisePath(FormBase.globalParameters.BackupPath, TeamPathTypes.BackupPath, FormBase.TeamEventLog);
            FileHandling.InitialisePath(FormBase.globalParameters.CorePath, TeamPathTypes.CorePath, FormBase.TeamEventLog);
            FileHandling.InitialisePath(FormBase.globalParameters.ScriptPath, TeamPathTypes.ScriptPath, FormBase.TeamEventLog);
            FileHandling.InitialisePath(FormBase.globalParameters.FilesPath, TeamPathTypes.FilesPath, FormBase.TeamEventLog);
        }

        /// <summary>
        /// Retrieve the values of the application root path (where the paths to the configuration file is maintained).
        /// This is the hardcoded base path that always needs to be accessible, it has the main file which can locate the rest of the configuration.
        /// </summary 
        public static void LoadRootPathFile(string fileName, string corePath)
        {
            // Create root path file, with dummy values if it doesn't exist already
            try
            {
                if (!File.Exists(fileName))
                {
                    var initialConfigurationFile = new StringBuilder();

                    initialConfigurationFile.AppendLine("/* TEAM Core Settings */");
                    initialConfigurationFile.AppendLine("CorePath|" + corePath);
                    initialConfigurationFile.AppendLine($"WorkingEnvironment|{Utility.CreateMd5(new[] { "Development" }, "%$@")}");
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
                MessageBox.Show($@"An error occurred while creation the default path file. The error message is {ex.Message}", @"An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                FormBase.globalParameters.ActiveEnvironmentInternalId = configList["WorkingEnvironment"];
            }
            catch (Exception)
            {
                // richTextBoxInformation.AppendText("\r\n\r\nAn error occurred while interpreting the configuration file. The original error is: '" + ex.Message + "'");
            }
        }

        /// <summary>
        /// Retrieve the configuration information from memory and save this to disk.
        /// </summary>
        internal static void SaveTeamConfigurationFile()
        {
            try
            {
                var configurationFile = new StringBuilder();
                configurationFile.AppendLine("/* TEAM Configuration Settings */");

                configurationFile.AppendLine("MetadataConnectionId|" + FormBase.TeamConfiguration.MetadataConnection.ConnectionInternalId + "");

                configurationFile.AppendLine("StagingAreaPrefix|" + FormBase.TeamConfiguration.StgTablePrefixValue +"");
                configurationFile.AppendLine("PersistentStagingAreaPrefix|" +FormBase.TeamConfiguration.PsaTablePrefixValue + "");
                configurationFile.AppendLine("PresentationLayerLabels|" + FormBase.TeamConfiguration.PresentationLayerLabels + "");
                configurationFile.AppendLine("TransformationLabels|" + FormBase.TeamConfiguration.TransformationLabels + "");
                configurationFile.AppendLine("HubTablePrefix|" + FormBase.TeamConfiguration.HubTablePrefixValue + "");
                configurationFile.AppendLine("SatTablePrefix|" + FormBase.TeamConfiguration.SatTablePrefixValue + "");
                configurationFile.AppendLine("LinkTablePrefix|" + FormBase.TeamConfiguration.LinkTablePrefixValue +"");
                configurationFile.AppendLine("LinkSatTablePrefix|" + FormBase.TeamConfiguration.LsatTablePrefixValue + "");
                configurationFile.AppendLine("KeyIdentifier|" + FormBase.TeamConfiguration.KeyIdentifier + "");
                configurationFile.AppendLine("KeyPattern|" + FormBase.TeamConfiguration.KeyPattern + "");
                configurationFile.AppendLine("SchemaName|" + FormBase.TeamConfiguration.SchemaName + "");
                configurationFile.AppendLine("RowID|" + FormBase.TeamConfiguration.RowIdAttribute + "");
                configurationFile.AppendLine("EventDateTimeStamp|" + FormBase.TeamConfiguration.EventDateTimeAttribute + "");
                configurationFile.AppendLine("LoadDateTimeStamp|" + FormBase.TeamConfiguration.LoadDateTimeAttribute + "");
                configurationFile.AppendLine("ExpiryDateTimeStamp|" + FormBase.TeamConfiguration.ExpiryDateTimeAttribute + "");
                configurationFile.AppendLine("ChangeDataIndicator|" + FormBase.TeamConfiguration.ChangeDataCaptureAttribute +"");
                configurationFile.AppendLine("RecordSourceAttribute|" + FormBase.TeamConfiguration.RecordSourceAttribute + "");
                configurationFile.AppendLine("ETLProcessID|" + FormBase.TeamConfiguration.EtlProcessAttribute + "");
                configurationFile.AppendLine("ETLUpdateProcessID|" +FormBase.TeamConfiguration.EtlProcessUpdateAttribute +"");
                configurationFile.AppendLine("LogicalDeleteAttribute|" +FormBase.TeamConfiguration.LogicalDeleteAttribute +"");
                configurationFile.AppendLine("OtherExceptionColumns|" + FormBase.TeamConfiguration.OtherExceptionColumns + "");
                configurationFile.AppendLine("TableNamingLocation|" + FormBase.TeamConfiguration.TableNamingLocation + "");
                configurationFile.AppendLine("RecordChecksum|" +FormBase.TeamConfiguration.RecordChecksumAttribute + "");
                configurationFile.AppendLine("CurrentRecordAttribute|" +FormBase.TeamConfiguration.CurrentRowAttribute +"");
                configurationFile.AppendLine("AlternativeRecordSource|" +FormBase.TeamConfiguration.AlternativeRecordSourceAttribute + "");
                configurationFile.AppendLine("AlternativeHubLDTS|" +FormBase.TeamConfiguration.AlternativeLoadDateTimeAttribute + "");
                configurationFile.AppendLine("AlternativeSatelliteLDTS|" +FormBase.TeamConfiguration.AlternativeSatelliteLoadDateTimeAttribute +"");
                configurationFile.AppendLine("AlternativeRecordSourceFunction|" +FormBase.TeamConfiguration.EnableAlternativeRecordSourceAttribute +"");
                configurationFile.AppendLine("AlternativeHubLDTSFunction|" +FormBase.TeamConfiguration.EnableAlternativeLoadDateTimeAttribute +"");
                configurationFile.AppendLine("AlternativeSatelliteLDTSFunction|" +FormBase.TeamConfiguration.EnableAlternativeSatelliteLoadDateTimeAttribute +"");
                configurationFile.AppendLine("PSAKeyLocation|" + FormBase.TeamConfiguration.PsaKeyLocation + "");

                // Closing off
                configurationFile.AppendLine("/* End of file */");

                using (var outfile = new StreamWriter(FormBase.globalParameters.ConfigurationPath + FormBase.globalParameters.ConfigFileName + '_' + FormBase.globalParameters.ActiveEnvironmentKey + FormBase.globalParameters.FileExtension)) 
                {
                    outfile.Write(configurationFile.ToString());
                    outfile.Flush();
                    outfile.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An error occurred saving the Configuration File. The error message is {ex.Message}", @"An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        public TeamEnvironment Value { get; set; }

        public MyWorkingEnvironmentEventArgs(TeamEnvironment value)
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