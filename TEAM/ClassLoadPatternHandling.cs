using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TEAM
{

    internal class LoadPatternDefinition
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
                    var targetFilePathName = loadPatternDefinitionFilePath + string.Concat("Backup_" + DateTime.Now.ToString("yyyyMMddHHmmss"));

                    File.Copy(loadPatternDefinitionFilePath, targetFilePathName);
                    returnMessage = "A backup was created at: " + targetFilePathName;
                }
                else
                {
                    returnMessage =
                        "VEDW couldn't locate a configuration file! Can you check the paths and existence of directories?";
                }
            }
            catch (Exception ex)
            {
                returnMessage = ("An error has occured while creating a file backup. The error message is " + ex);
            }

            return returnMessage;
        }

        internal Dictionary<String, String> MatchConnectionKey()
        {
            Dictionary<string, string> returnValue = new Dictionary<string, string>();

            if (LoadPatternConnectionKey == "SourceDatabase")
            {
                returnValue.Add(LoadPatternConnectionKey, FormBase.ConfigurationSettings.ConnectionStringSource);
            }
            else if (LoadPatternConnectionKey == "StagingDatabase")
            {
                returnValue.Add(LoadPatternConnectionKey, FormBase.ConfigurationSettings.ConnectionStringStg);
            }
            else if (LoadPatternConnectionKey == "PersistentStagingDatabase")
            {
                returnValue.Add(LoadPatternConnectionKey, FormBase.ConfigurationSettings.ConnectionStringHstg);
            }
            else if (LoadPatternConnectionKey == "IntegrationDatabase")
            {
                returnValue.Add(LoadPatternConnectionKey, FormBase.ConfigurationSettings.ConnectionStringInt);
            }
            else if (LoadPatternConnectionKey == "PresentationDatabase")
            {
                returnValue.Add(LoadPatternConnectionKey, FormBase.ConfigurationSettings.ConnectionStringPres);
            }

            return returnValue;
        }


        internal static List<LoadPatternDefinition> DeserializeLoadPatternDefinition()
        {
            List<LoadPatternDefinition> loadPatternDefinitionList = new List<LoadPatternDefinition>();

            // Retrieve the file contents and store in a string
            if (File.Exists(FormBase.GlobalParameters.RootPath + @"..\..\..\LoadPatterns\" +
                            FormBase.GlobalParameters.LoadPatternDefinitionFile))
            {
                var jsonInput = File.ReadAllText(FormBase.GlobalParameters.RootPath + @"..\..\..\LoadPatterns\" +
                                                 FormBase.GlobalParameters.LoadPatternDefinitionFile);

                //Move the (json) string into a List object (a list of the type LoadPattern)
                loadPatternDefinitionList = JsonConvert.DeserializeObject<List<LoadPatternDefinition>>(jsonInput);

                FormBase.ConfigurationSettings.patternDefinitionList = loadPatternDefinitionList;
                FormBase.ConfigurationSettings.LoadPatternListPath =
                    Path.GetFullPath(FormBase.GlobalParameters.RootPath + @"..\..\..\LoadPatterns\");
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
    