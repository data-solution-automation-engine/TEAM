using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TEAM
{
    /// <summary>
    /// Configuration settings related to the export of Json files.
    /// </summary>
    public class JsonExportSetting
    {
        // Data Item
        public string GenerateSourceDataItemTypes { get; set; }
        public string GenerateTargetDataItemTypes { get; set; }

        // Data Object Connection
        public string GenerateSourceDataObjectConnection { get; set; }
        public string GenerateTargetDataObjectConnection { get; set; }
        public string GenerateDatabaseAsExtension { get; set; }
        public string GenerateSchemaAsExtension { get; set; }

        // Related Data Objects
        public string AddMetadataAsRelatedDataObject { get; set; }
        public string AddUpstreamDataObjectsAsRelatedDataObject { get; set; }

        /// <summary>
        /// Retrieve the validation information from disk and save this to memory.
        /// </summary>
        /// <param name="filename"></param>
        internal void LoadJsonConfigurationFile(string filename)
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

                GenerateSourceDataItemTypes = configList["GenerateSourceDataItemTypes"];
                GenerateTargetDataItemTypes = configList["GenerateTargetDataItemTypes"];

                GenerateSourceDataObjectConnection = configList["GenerateSourceDataObjectConnection"];
                GenerateTargetDataObjectConnection = configList["GenerateTargetDataObjectConnection"];
                GenerateDatabaseAsExtension = configList["GenerateDatabaseAsExtension"];
                GenerateSchemaAsExtension = configList["GenerateSchemaAsExtension"];

                AddMetadataAsRelatedDataObject = configList["AddMetadataAsRelatedDataObject"];
                AddUpstreamDataObjectsAsRelatedDataObject = configList["AddUpstreamDataObjectsAsRelatedDataObject"];
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        internal void SaveJsonConfigurationFile()
        {
            try
            {
                // Creating the file
                var validationFile = new StringBuilder();
                validationFile.AppendLine("/* TEAM Json Export Configuration Settings */");
                validationFile.AppendLine("/* Saved at " + DateTime.Now + " */");
                validationFile.AppendLine("GenerateSourceDataItemTypes|" + GenerateSourceDataItemTypes + "");
                validationFile.AppendLine("GenerateTargetDataItemTypes|" + GenerateTargetDataItemTypes + "");

                validationFile.AppendLine("GenerateSourceDataObjectConnection|" + GenerateSourceDataObjectConnection + "");
                validationFile.AppendLine("GenerateTargetDataObjectConnection|" + GenerateTargetDataObjectConnection + "");
                validationFile.AppendLine("GenerateDatabaseAsExtension|" + GenerateDatabaseAsExtension + "");
                validationFile.AppendLine("GenerateSchemaAsExtension|" + GenerateSchemaAsExtension + "");

                validationFile.AppendLine("AddMetadataAsRelatedDataObject|" +AddMetadataAsRelatedDataObject + "");
                validationFile.AppendLine("AddUpstreamDataObjectsAsRelatedDataObject|" + AddUpstreamDataObjectsAsRelatedDataObject + "");

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
                MessageBox.Show("An error occurred saving the Json configuration file. The error message is " + ex, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
