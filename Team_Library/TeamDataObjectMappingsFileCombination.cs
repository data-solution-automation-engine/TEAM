using DataWarehouseAutomation;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows.Forms;

namespace TEAM_Library
{
    /// <summary>
    /// Convenience class to capture the data object mappings for each file (since a file may contain one or more mappings in a mapping list).
    /// </summary>
    public class DataObjectMappingsFileCombination
    {
        public string FileName { get; set; }
        public DataObjectMappings DataObjectMappings { get; set; }
    }

    /// <summary>
    /// Deserialize the DataWarehouseAutomation JSON files into a list of objects per file.
    /// </summary>
    public class TeamDataObjectMappingsFileCombinations
    {
        public EventLog EventLog { get; set; }

        public string MetadataPath { get; set; }

        /// <summary>
        /// A list of the filename / data object mapping list combinations.
        /// </summary>
        public SortableBindingList<DataObjectMappingsFileCombination> DataObjectMappingsFileCombinations { get; set; }

        /// <summary>
        /// Default constructor, requires a path to draw the metadata files from.
        /// </summary>
        /// <param name="inputPath"></param>
        public TeamDataObjectMappingsFileCombinations(string inputPath)
        {
            MetadataPath = inputPath;
            EventLog = new EventLog();
            DataObjectMappingsFileCombinations = new SortableBindingList<DataObjectMappingsFileCombination>();
        }

        /// <summary>
        /// Load compatible JSON files into memory.
        /// </summary>
        public void GetMetadata()
        {
            if (Directory.Exists(MetadataPath))
            {
                string[] jsonFiles = Directory.GetFiles(MetadataPath, "*.json");

                // Hard-coded exclusions.
                string[] excludedFiles =
                {
                    "Development_TEAM_Table_Mapping_v0.json", 
                    "Development_TEAM_Attribute_Mapping_v0.json",
                    "Development_TEAM_Table_Mapping.json",
                    "Development_TEAM_Attribute_Mapping.json",
                    "Development_TEAM_Physical_Model_v0.json",
                    "Development_TEAM_Physical_Model.json"
                };

                if (jsonFiles.Length > 0)
                {
                    foreach (string fileName in jsonFiles)
                    {
                        if (!Array.Exists(excludedFiles, x => x == Path.GetFileName(fileName)))
                        {
                            try
                            {
                                // Validate the file contents against the schema definition.
                                if (File.Exists(Application.StartupPath + @"\Schema\" + GlobalParameters.JsonSchemaForDataWarehouseAutomationFileName))
                                {
                                    var result = JsonHandling.ValidateJsonFileAgainstSchema(Application.StartupPath + @"\Schema\" + GlobalParameters.JsonSchemaForDataWarehouseAutomationFileName, fileName);

                                    foreach (var error in result.Errors)
                                    {
                                        EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error was encountered validating the contents {fileName}.{error.Message}. This occurs at line {error.LineNumber}."));
                                    }
                                }
                                else
                                {
                                    EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error occurred while validating the file against the Data Warehouse Automation schema. Does the schema file exist?"));
                                }

                                // Add the deserialised file to the list of mappings.

                                var jsonInput = File.ReadAllText(fileName);
                                var dataObjectMappings = JsonConvert.DeserializeObject<DataObjectMappings>(jsonInput);

                                var localDataObjectMappingsFileCombination = new DataObjectMappingsFileCombination
                                {
                                    FileName = fileName,
                                    DataObjectMappings = dataObjectMappings
                                };

                                DataObjectMappingsFileCombinations.Add(localDataObjectMappingsFileCombination);
                                EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The file {fileName} was successfully loaded."));
                            }
                            catch
                            {
                                EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The file {fileName} could not be loaded."));
                            }
                        }
                    }
                }
                else
                {
                    EventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"No files were found in directory {MetadataPath}."));

                }

            }
            else
            {
                EventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"There were issues accessing the directory {MetadataPath}. It does not seem to exist."));
            }
        }
    }
}
