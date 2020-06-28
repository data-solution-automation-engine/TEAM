using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TEAM
{
    internal class JsonHandling
    {
        /// <summary>
        ///    Method to create a new dummy Json file in the designated working directory.
        /// </summary>
        /// <param name="fileType"></param>
        internal static void CreateDummyJsonFile(string fileType)
        {
            var jsonVersionExtension = @"_v" + FormBase.GlobalParameters.CurrentVersionId + ".json";

            JArray outputFileArray = new JArray();
            JObject dummyJsonFile = new JObject();

            string outputFileName = "";
            if (fileType == FormBase.GlobalParameters.JsonTableMappingFileName) // Table Mapping
            {
                dummyJsonFile = new JObject(
                    new JProperty("enabledIndicator", true),
                    new JProperty("tableMappingHash", "NewHash"),
                    new JProperty("versionId", 0),
                    new JProperty("sourceTable", "STG_EXAMPLESYSTEM_EXAMPLETABLE"),
                    new JProperty("sourceConnection", null),
                    new JProperty("targetTable", "HUB_EXAMPLE"),
                    new JProperty("targetConnectionKey", null),
                    new JProperty("businessKeyDefinition", "EXAMPLE"),
                    new JProperty("drivingKeyDefinition", ""),
                    new JProperty("filterCriteria", "")
                ); 
                
                outputFileName = FileConfiguration.tableMappingJsonFileName();

            } else if (fileType == FormBase.GlobalParameters.JsonModelMetadataFileName) // Physical Model
            {
                dummyJsonFile = new JObject(
                    new JProperty("versionAttributeHash", "NewHash"),
                    new JProperty("versionId", 0),
                    new JProperty("databaseName", "Sample database"),
                    new JProperty("schemaName", "[dbo]"),
                    new JProperty("tableName", "Sample Table"),
                    new JProperty("columnName", "Sample Column"),
                    new JProperty("dataType", "nvarchar"),
                    new JProperty("characterMaximumLength", "100"),
                    new JProperty("numericPrecision", "0"),
                    new JProperty("ordinalPosition", "1"),
                    new JProperty("primaryKeyIndicator", "N"),
                    new JProperty("multiActiveIndicator", "N")
                );

                outputFileName = FileConfiguration.physicalModelJsonFileName();

            } else if (fileType == FormBase.GlobalParameters.JsonAttributeMappingFileName) // Attribute Mapping
            {
                dummyJsonFile = new JObject(
                    new JProperty("attributeMappingHash", "NewHash"),
                    new JProperty("versionId", 0),
                    new JProperty("sourceTable", "SOURCE_TABLE"),
                    new JProperty("sourceAttribute", "EXAMPLE_FROM_ATTRIBUTE"),
                    new JProperty("targetTable", "TARGET_TABLE"),
                    new JProperty("targetAttribute", "EXAMPLE_TO_ATTRIBUTE"),
                    new JProperty("notes", "")
                );

                outputFileName = FileConfiguration.attributeMappingJsonFileName();
            }
            else
            {
                // No action - issue in code
            }

            // Spool to disk
            outputFileArray.Add(dummyJsonFile);
            string json = JsonConvert.SerializeObject(outputFileArray, Formatting.Indented);
            
            File.WriteAllText(outputFileName, json);
        }


        /// <summary>
        /// Local class to manage file names and changes thereof.
        /// </summary>
        internal static class FileConfiguration
        {
            public static string tableMappingJsonFileName()
            {
                string localJsonFileName = FormBase.GlobalParameters.ConfigurationPath +
                                           FormBase.GlobalParameters.WorkingEnvironment + "_" +
                                           FormBase.GlobalParameters.JsonTableMappingFileName +
                                           jsonVersionExtension;

                return localJsonFileName;
            }

            public static string attributeMappingJsonFileName()
            {
                string localJsonFileName = FormBase.GlobalParameters.ConfigurationPath +
                                           FormBase.GlobalParameters.WorkingEnvironment + "_" +
                                           FormBase.GlobalParameters.JsonAttributeMappingFileName +
                                           jsonVersionExtension;

                return localJsonFileName;
            }

            public static string physicalModelJsonFileName()
            {
                string localJsonFileName = FormBase.GlobalParameters.ConfigurationPath +
                                                       FormBase.GlobalParameters.WorkingEnvironment + "_" +
                                                       FormBase.GlobalParameters.JsonModelMetadataFileName +
                                                       jsonVersionExtension;

                return localJsonFileName;
            }

            internal static string newFileTableMapping { get; set; }
            internal static string newFileAttributeMapping { get; set; }
            internal static string newFilePhysicalModel { get; set; }
            internal static string jsonVersionExtension { get; set; }
        }

        /// <summary>
        ///   Create a completely empty file, for the purposes of clearing a version / existing data.
        /// </summary>
        /// <param name="fileType"></param>
        internal static void CreatePlaceholderJsonFile(string fileType)
        {
            var jsonVersionExtension = @"_v" + FormBase.GlobalParameters.CurrentVersionId + ".json";

            File.WriteAllText(FormBase.GlobalParameters.ConfigurationPath + fileType + jsonVersionExtension, "");
        }


        /// <summary>
        ///    Create a backup of a given JSON file.
        /// </summary>
        /// <param name="inputFileName"></param>
        internal string BackupJsonFile(string inputFileName, string inputFilePath)
        {
            string result;
            var shortDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");

            //var targetFilePathName = FormBase.GlobalParameters.ConfigurationPath + string.Concat("Backup_" + shortDatetime + "_", inputFileName);
            var targetFilePathName = inputFilePath + string.Concat("Backup_" + shortDatetime + "_", inputFileName);

            //File.Copy(FormBase.GlobalParameters.ConfigurationPath + inputFileName, targetFilePathName);
            File.Copy( inputFilePath+inputFileName, targetFilePathName);
            result = targetFilePathName;

            return result;
        }

        /// <summary>
        ///   Clear out (remove) an existing Json file, to facilitate overwriting.
        /// </summary>
        internal static void RemoveExistingJsonFile(string inputFileName)
        {
            File.Delete(FormBase.GlobalParameters.ConfigurationPath + inputFileName);
        }
    }
}