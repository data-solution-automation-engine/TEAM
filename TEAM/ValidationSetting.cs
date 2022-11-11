using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TEAM_Library;

namespace TEAM
{
    /// <summary>
    /// Gets or sets the values for the validation of the metadata.
    /// </summary>
    public class ValidationSetting
    {
        // Configuration settings related to validation checks (in physical model or virtual representation of it).
        public string DataObjectExistence { get; set; }
        public string SourceBusinessKeyExistence { get; set; }
        public string DataItemExistence { get; set; }

        // Consistency of the unit of work.
        public string LogicalGroup { get; set; }
        public string LinkKeyOrder { get; set; }

        // Syntax validation.
        public string BusinessKeySyntax { get; set; }

        public string LinkCompletion { get; set; }
        
        // Data Vault Modelling.
        public string BasicDataVaultValidation { get; set; }

        // Generic
        public string DuplicateDataObjectMappings { get; set; }

        /// <summary>
        /// Retrieve the validation information from disk and save this to memory.
        /// </summary>
        /// <param name="filename"></param>
        internal void LoadValidationFile(string filename)
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

                DataObjectExistence = configList["DataObjectExistence"];
                SourceBusinessKeyExistence = configList["BusinessKeyExistence"];
                DataItemExistence = configList["DataItemExistence"];

                LogicalGroup = configList["LogicalGroup"];
                LinkKeyOrder = configList["LinkKeyOrder"];
                BusinessKeySyntax = configList["BusinessKeySyntax"];
                LinkCompletion = configList["LinkCompletion"];

                BasicDataVaultValidation = configList["BasicDataVaultValidation"];

                DuplicateDataObjectMappings = configList["DuplicateDataObjectMappings"];
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        /// <summary>
        /// Retrieve the validation information from memory and save this to disk.
        /// </summary>
        internal void SaveValidationFile()
        {
            try
            {
                // Creating the file.
                var validationFile = new StringBuilder();
                validationFile.AppendLine("/* TEAM Validation Settings */");
                validationFile.AppendLine("DataObjectExistence|" + DataObjectExistence + "");
                validationFile.AppendLine("BusinessKeyExistence|" + SourceBusinessKeyExistence + "");
                validationFile.AppendLine("DataItemExistence|" + DataItemExistence + "");
                validationFile.AppendLine("LogicalGroup|" + LogicalGroup + "");
                validationFile.AppendLine("LinkKeyOrder|" + LinkKeyOrder + "");
                validationFile.AppendLine("BusinessKeySyntax|" + BusinessKeySyntax + "");
                validationFile.AppendLine("LinkCompletion|" + BusinessKeySyntax + "");
                validationFile.AppendLine("BasicDataVaultValidation|" + BasicDataVaultValidation + "");
                validationFile.AppendLine("DuplicateDataObjectMappings|" + DuplicateDataObjectMappings + "");

                // Closing off.
                validationFile.AppendLine("/* End of file */");

                using (var outfile =
                    new StreamWriter(FormBase.globalParameters.ConfigurationPath + FormBase.globalParameters.ValidationFileName + '_' + FormBase.globalParameters.ActiveEnvironmentKey + FormBase.globalParameters.FileExtension))
                {
                    outfile.Write(validationFile.ToString());
                    outfile.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An error occurred saving the Validation File. The error message is {ex}.", @"An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Method to create a new validation file with default values at the default location
        /// Checks if the file already exists. If it does, nothing will happen.
        /// </summary>
        internal void CreateDummyValidationFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                var validationFile = new StringBuilder();

                validationFile.AppendLine("/* TEAM Validation Settings */");

                // Object existence validation
                validationFile.AppendLine("DataObjectExistence|True");
                validationFile.AppendLine("BusinessKeyExistence|True");

                validationFile.AppendLine("DataItemExistence|True");

                // Consistency validation
                validationFile.AppendLine("LogicalGroup|True");
                validationFile.AppendLine("LinkKeyOrder|True");
                validationFile.AppendLine("BusinessKeySyntax|True");
                validationFile.AppendLine("LinkCompletion|True");

                validationFile.AppendLine("BasicDataVaultValidation|True");

                validationFile.AppendLine("DuplicateDataObjectMappings|True");

                validationFile.AppendLine("/* End of file */");

                using (var outfile = new StreamWriter(fileName))
                {
                    outfile.Write(validationFile.ToString());
                    outfile.Close();
                }

                FormBase.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"A new configuration file was created for {fileName}."));
            }
            else
            {
                FormBase.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The existing configuration file {fileName} was detected."));
            }
        }
    }
}
