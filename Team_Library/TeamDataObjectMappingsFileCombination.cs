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
        /// <summary>
        /// The file name is based on the target data object, a file contains one or more data object mappings.
        /// </summary>
        public string FileName { get; set; }
        public DataObjectMappings DataObjectMappings { get; set; }

        /// <summary>
        /// Get only the file name from the full path.
        /// </summary>
        /// <returns></returns>
        public string GetFileName()
        {
            return Path.GetFileName(FileName);
        }

        /// <summary>
        /// Get only the path for the file.
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            return Path.GetDirectoryName(FileName);
        }

        /// <summary>
        /// Return the file name without the path or extension, which is the name of the target data object.
        /// </summary>
        /// <returns></returns>
        public string GetTargetDataObjectName()
        {
            return Path.GetFileNameWithoutExtension(FileName);
        }
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
        public void GetMetadata(GlobalParameters globalParameters)
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
                    "Development_TEAM_Physical_Model.json",
                    "Development_TEAM_Model_Metadata_v0.json",
                    "Development_TEAM_Model_Metadata.json",
                };

                if (jsonFiles.Length > 0)
                {
                    foreach (string fileName in jsonFiles)
                    {
                        if (!Array.Exists(excludedFiles, x => x == Path.GetFileName(fileName)) &&
                            !fileName.EndsWith("TEAM_Table_Mapping.json") &&
                            !fileName.EndsWith("TEAM_Attribute_Mapping.json") &&
                            !fileName.EndsWith("TEAM_Model_Metadata.json")
                            )
                        {
                            try
                            {
                                #region Schema validation

                                // Validate the file contents against the schema definition.
                                var schemaDefinitionFileName = Application.StartupPath + @"\Schema\" + globalParameters.JsonSchemaForDataWarehouseAutomationFileName;

                                if (File.Exists(schemaDefinitionFileName))
                                {
                                    var validationResult = JsonHandling.ValidateJsonFileAgainstSchema(schemaDefinitionFileName, fileName);

                                    foreach (var error in validationResult.Errors)
                                    {
                                        EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error was encountered validating the contents '{fileName}' against the Data Warehouse Automation schema. The error is {error.Message}"));
                                    }
                                }
                                else
                                {
                                    EventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The Data Warehouse Automation schema definition can not be found. Does the schema file exist?. The expected file is '{schemaDefinitionFileName}'."));
                                }

                                #endregion

                                #region Deserialise JSON file

                                // Add the deserialised file to the list of mappings.
                                var jsonInput = File.ReadAllText(fileName);

                                try
                                {
                                    var dataObjectMappings = JsonConvert.DeserializeObject<DataObjectMappings>(jsonInput);

                                    if (dataObjectMappings == null || dataObjectMappings.dataObjectMappings.Count == 0)
                                    {
                                        EventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The file {fileName} could not be loaded."));
                                    }
                                    else
                                    {
                                        var localDataObjectMappingsFileCombination = new DataObjectMappingsFileCombination
                                        {
                                            FileName = fileName,
                                            DataObjectMappings = dataObjectMappings
                                        };

                                        DataObjectMappingsFileCombinations.Add(localDataObjectMappingsFileCombination);
                                        EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The file {fileName} was successfully loaded."));
                                    }
                                }
                                catch (Exception exception)
                                {
                                    EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The file {fileName} could not be loaded. The reported error is {exception.Message}"));
                                }

                                #endregion
                            }
                            catch (Exception exception)
                            {
                                EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The file {fileName} could not be loaded. The reported error is {exception.Message}"));
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
