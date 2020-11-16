using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TEAM
{

    public class LoadPatternDefinition
    {
        public int LoadPatternKey { get; set; }
        public string LoadPatternType { get; set; }
        public string LoadPatternSelectionQuery { get; set; }
        public string LoadPatternBaseQuery { get; set; }
        public string LoadPatternAttributeQuery { get; set; }
        public string LoadPatternAdditionalBusinessKeyQuery { get; set; }
        public string LoadPatternNotes { get; set; }
        public string LoadPatternConnectionKey { get; set; }

        /// <summary>
        /// Create a file backup for the configuration file at the provided location and return notice of success or failure as a string.
        /// /// </summary>
        internal static string BackupLoadPatternDefinition(string loadPatternDefinitionFilePath)
        {
            string returnMessage = "";

            try
            {
                if (File.Exists(loadPatternDefinitionFilePath))
                {
                    
                    var targetFilePathName = string.Concat(Path.GetDirectoryName(loadPatternDefinitionFilePath), @"\Backup_" + DateTime.Now.ToString("yyyyMMddHHmmss_"), Path.GetFileName(loadPatternDefinitionFilePath));

                    File.Copy(loadPatternDefinitionFilePath, targetFilePathName);
                    returnMessage = "A backup was created at: " + targetFilePathName;
                }
                else
                {
                    returnMessage = "TEAM could not create a backup of the file.";
                }
            }
            catch (Exception ex)
            {
                returnMessage = ("An error has occured while creating a file backup. The error message is " + ex);
            }

            return returnMessage;
        }

        internal static List<LoadPatternDefinition> DeserializeLoadPatternDefinition(string filePath)
        {
            List<LoadPatternDefinition> loadPatternDefinitionList = new List<LoadPatternDefinition>();

            // Retrieve the file contents and store in a string
            if (File.Exists(filePath))
            {
                var jsonInput = File.ReadAllText(filePath);

                //Move the (json) string into a List object (a list of the type LoadPattern)
                loadPatternDefinitionList = JsonConvert.DeserializeObject<List<LoadPatternDefinition>>(jsonInput);

                FormBase.GlobalParameters.PatternDefinitionList = loadPatternDefinitionList;
                //FormBase.GlobalParameters.LoadPatternListPath =
                //    Path.GetFullPath(FormBase.GlobalParameters.RootPath + @"..\..\..\LoadPatterns\");
            }
            else
            {
                //richTextBoxInformationMain.Text = "The file " + ConfigurationSettings.LoadPatternListPath +
                //                                  GlobalParameters.LoadPatternDefinitionFile +
                //                                  " could not be found!";
                loadPatternDefinitionList = null;
            }

            // Return the list to the instance
            return loadPatternDefinitionList;
        }

        /// <summary>
        /// The method that backs-up and saves a specific pattern (based on its path) with whatever is passed as contents.
        /// </summary>
        /// <param name="loadPatternDefinitionFilePath"></param>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        internal static string SaveLoadPatternDefinition(string loadPatternDefinitionFilePath, string fileContent)
        {
            string returnMessage = "";

            try
            {
                using (var outfile = new StreamWriter(loadPatternDefinitionFilePath))
                {
                    outfile.Write(fileContent);
                    outfile.Close();
                }

                returnMessage = "The file has been updated.";
            }
            catch (Exception ex)
            {
                returnMessage = ("An error has occured while creating saving the file. The error message is " + ex);
            }

            return returnMessage;
        }
    }



}
    